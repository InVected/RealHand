using Intel.RealSense;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;


namespace RealHand
{
    public class AbsoluteHandLandmark
    {
        public int ID { get; set; }
        public Vector3 Coordinat { get; set; }
        public bool Visibility { get; set; }
        public AbsoluteHandLandmark(int id, Vector3 coordinat)
        {
            ID = id;
            Coordinat = coordinat;
            Visibility = false;

        }
        public AbsoluteHandLandmark(int id, Vector3 coordinat, bool visibility)
        {
            ID = id;
            Coordinat = coordinat;
            Visibility = visibility;

        }
        public AbsoluteHandLandmark(int id, float x, float y, float z)
        {
            ID = id;
            Coordinat = new Vector3(x, y, z);
            Visibility = false;

        }
        public AbsoluteHandLandmark(int id, float x, float y, float z, bool visibility)
        {
            ID = id;
            Coordinat = new Vector3(x, y, z);
            Visibility = visibility;

        }
        public AbsoluteHandLandmark Clone()
        {
            return new AbsoluteHandLandmark(ID, Coordinat, Visibility);
        }
    }
    public class RelativHandLandmark
    {
        //public string Name { get; set; }
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public float Z { get; set; }
        public bool Visibility { get; set; }
        public RelativHandLandmark(int id, int x, int y, float z)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Visibility = true;

        }
        public RelativHandLandmark(int id, int x, int y, float z, bool visibility)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Visibility = visibility;

        }
        public RelativHandLandmark Clone()
        {
            return new RelativHandLandmark(ID, X, Y, Z, Visibility);
        }
        public string[] getStrArray()
        {
            string[] strArray = { ID.ToString(), X.ToString(), Y.ToString(), Z.ToString(), Visibility.ToString()};
            return strArray;
        }
    }

    public class TransformToReal
    {
        public int[,] Structure { get; set; }
        public List<int>[] ReStructure;
        public RelativHandLandmark[] Data { get; }
        public TransformToReal(int[,] structure, int landmarkCount)
        {
            Structure = structure;
            List<int>[] tempList = new List<int>[landmarkCount];
            for (int count = 0; count < landmarkCount; count++)
            {
                tempList[count] = new List<int>();
            }
            for (int i = 0; i < structure.GetLength(0); i++)
            {

                tempList[structure[i, 0]].Add(structure[i, 1]);
                tempList[structure[i, 1]].Add(structure[i, 0]);
            }
            ReStructure = tempList;


        }
        public unsafe RelativHandLandmark[] GetVisibleList(RelativHandLandmark[] landmarkList, VideoFrame colorFrame)
        {
            if (landmarkList != null && landmarkList.Length != 0)
            {
                BorderControle(landmarkList, colorFrame);
                OcclusionControle(landmarkList);
            }
            return landmarkList;
        }
        public unsafe RelativHandLandmark[] CalculateWithDepth(RelativHandLandmark[] VisibleLandmarkList, DepthFrame depthFrame, VideoFrame colorFrame, Intrinsics intrinsics)
        {

            AbsoluteHandLandmark[] AbsoluteHandLandmarkArray = new AbsoluteHandLandmark[VisibleLandmarkList.GetLength(0)];
            int counter = 0;
            for (int i = 0; i < VisibleLandmarkList.GetLength(0); i++)
            {
                if (VisibleLandmarkList[i].Visibility)
                {
                    AbsoluteHandLandmarkArray[i] = RealWorldCoordinates(VisibleLandmarkList[i], colorFrame, depthFrame, intrinsics);
                    AbsoluteHandLandmarkArray[i].Visibility = true;
                    counter++;
                }
                else
                {
                    AbsoluteHandLandmarkArray[i] = new AbsoluteHandLandmark(VisibleLandmarkList[i].ID, VisibleLandmarkList[i].X, VisibleLandmarkList[i].Y, VisibleLandmarkList[i].Z);
                }

            }
            Matrix factor = DepthFactor(AbsoluteHandLandmarkArray, VisibleLandmarkList, counter);
            for (int i = 0; i < VisibleLandmarkList.GetLength(0); i++)
            {
                if (!VisibleLandmarkList[i].Visibility)
                {
                    AbsoluteHandLandmarkArray[i] = RealWorldCoordinates(VisibleLandmarkList[i], colorFrame, depthFrame, intrinsics, factor);
                }

            }

            return VisibleLandmarkList;
        }
        public bool HasData()
        {
            if (Data != null) return false;
            else return true;
        }
        private static void BorderControle(RelativHandLandmark[] LandmarkList, VideoFrame colorFrame)
        {
            int width = colorFrame.Width;
            int height = colorFrame.Height;

            foreach (RelativHandLandmark handLandmark in LandmarkList)
            {
                if (handLandmark.Visibility)
                {
                    if (handLandmark.X < 0 || handLandmark.X >= width || handLandmark.Y < 0 || handLandmark.Y >= height)
                    {
                        handLandmark.Visibility = false;
                    }

                }

            }
        }
        private unsafe void OcclusionControle(RelativHandLandmark[] LandmarkList)
        {

            RelativHandLandmark[] SortedLandmarkList = new RelativHandLandmark[LandmarkList.Length];
            for (int i = 0; i < LandmarkList.Length; i++)
            {
                SortedLandmarkList[i] = LandmarkList[i].Clone();
            }
            Array.Sort(SortedLandmarkList, delegate (RelativHandLandmark x, RelativHandLandmark y) { return x.Z.CompareTo(y.Z); });
            //sort joints by List
            float factor = 30.0F;


            float minZ = SortedLandmarkList[0].Z;
            float maxZ = SortedLandmarkList[^1].Z; //same as SortedLandmarkList[SortedLandmarkList.Length - 1].Z



            //=============================================================================================================================
            for (int joint = 0; joint < Structure.GetLength(0); joint++)
            {
                int jointAId = Structure[joint, 0];
                int jointBId = Structure[joint, 1];
                RelativHandLandmark jointA = LandmarkList[jointAId];
                RelativHandLandmark jointB = LandmarkList[jointBId];
                Vector2 pointJointA = new(jointA.X, jointA.Y);
                Vector2 pointJointB = new(jointB.X, jointB.Y);


                Vector2 jointLine = Vector2.Subtract(pointJointB, pointJointA);
                Vector2 ortogonal = new(jointLine.Y, -jointLine.X);
                Vector2 einheitsOrtogonal = Vector2.Normalize(ortogonal);

                var reziprokeDeterminante = 1.0 / (jointLine.X * einheitsOrtogonal.Y - jointLine.Y * einheitsOrtogonal.X);


                {
                    foreach (RelativHandLandmark backPoint in SortedLandmarkList)
                    {
                        if (LandmarkList[backPoint.ID].Visibility == true && (backPoint.Z > jointA.Z || backPoint.Z > jointB.Z) && backPoint.ID != jointA.ID && backPoint.ID != jointB.ID) // if not point part of joint
                        {

                            Vector2 pointBack = new(backPoint.X, backPoint.Y);
                            Vector2 u = Vector2.Subtract(pointJointB, pointBack);
                            var lambda = reziprokeDeterminante * (u.X * einheitsOrtogonal.Y - u.Y * einheitsOrtogonal.X); // relativ position of intersection on jointLine
                            var delta = reziprokeDeterminante * (u.Y * jointLine.X - u.X * jointLine.Y); //distance point to jointLine
                            if (lambda <= 1.0 && lambda >= 0.0 && Math.Abs(delta) <= factor)
                            {
                                var localLambdaZ = jointB.Z + lambda * (jointA.Z - jointB.Z);
                                if (localLambdaZ <= backPoint.Z)
                                {
                                    LandmarkList[backPoint.ID].Visibility = false;
                                }


                            }
                        }


                    }
                }

                //=============================================================================

            }
        }
        private AbsoluteHandLandmark RealWorldCoordinates(RelativHandLandmark handLandmark, VideoFrame colorFrame, DepthFrame depthFrame, Intrinsics intrinsics)
        {

            var xM = handLandmark.X / 2;  //????depthframe only half the size as ColorFrame
            var yM = handLandmark.Y / 2;  //????depthframe only half the size as ColorFrame
            Vector2 point2D = new(xM, yM);
            Vector2 depthFrameSize = new(depthFrame.Width, depthFrame.Height);
            Vector2 colorFrameSize = new(colorFrame.Width, colorFrame.Height);
            float depth = depthFrame.GetDistance(xM, yM);
            var point3D = Map2DTo3D(intrinsics, point2D, depth);
            AbsoluteHandLandmark landmark = new(handLandmark.ID, point3D);

            return landmark;

        }
        private AbsoluteHandLandmark RealWorldCoordinates(RelativHandLandmark handLandmark, VideoFrame colorFrame, DepthFrame depthFrame, Intrinsics intrinsics, Matrix factorMatrix)
        {
            var depth = (float)(handLandmark.Z * factorMatrix[0, 0] + factorMatrix[1, 0]);
            var xM = handLandmark.X / 2;  //????depthframe only half the size as ColorFrame
            var yM = handLandmark.Y / 2;  //????depthframe only half the size as ColorFrame
            Vector2 point2D = new(xM, yM);
            Vector2 depthFrameSize = new(depthFrame.Width, depthFrame.Height);
            Vector2 colorFrameSize = new(colorFrame.Width, colorFrame.Height);
            var point3D = Map2DTo3D(intrinsics, point2D, depth);
            AbsoluteHandLandmark landmark = new(handLandmark.ID, point3D);

            return landmark;

        }
        public Vector3 Map2DTo3D(Intrinsics intrinsics, Vector2 pixel, float depth)
        {
            Vector3 point = new();

            float x = (pixel.X - intrinsics.ppx) / intrinsics.fx;
            float y = (pixel.Y - intrinsics.ppy) / intrinsics.fy;

            if (intrinsics.model == Distortion.InverseBrownConrady)
            {
                float r2 = x * x + y * y;
                float f = 1 + intrinsics.coeffs[0] * r2 + intrinsics.coeffs[1] * r2 * r2 + intrinsics.coeffs[4] * r2 * r2 * r2;
                float ux = x * f + 2 * intrinsics.coeffs[2] * x * y + intrinsics.coeffs[3] * (r2 + 2 * x * x);
                float uy = y * f + 2 * intrinsics.coeffs[3] * x * y + intrinsics.coeffs[2] * (r2 + 2 * y * y);

                x = ux;
                y = uy;
            }

            point.X = depth * x;
            point.Y = depth * y;
            point.Z = depth;

            return point;
        }
        private static Matrix DepthFactor(AbsoluteHandLandmark[] realLandmarkArray, RelativHandLandmark[] handLandmarkArray, int counter)
        {

            double[,] aArray = new double[counter, 2];
            double[,] bArray = new double[counter, 1];

            for (int r = 0; r < handLandmarkArray.GetLength(0); r++)
            {
                if (handLandmarkArray[r].Visibility)
                {
                    aArray[(counter) - 1, 0] = handLandmarkArray[r].Z;
                    aArray[(counter) - 1, 1] = 1;
                    bArray[(counter) - 1, 0] = realLandmarkArray[r].Coordinat.Z;
                    counter--;
                }
            }
            Matrix<double> A = DenseMatrix.OfArray(aArray);
            Matrix<double> b = DenseMatrix.OfArray(bArray);
            Matrix x = (Matrix)A.Transpose().Multiply(A).Inverse().Multiply(A.Transpose().Multiply(b)); // casting to Matrix...why?
            //Trace.WriteLine(x);
            var counterValue = 0;
            var differenzValue = 0.0;
            for (int r = 0; r < realLandmarkArray.GetLength(0); r++)
            {
                if (realLandmarkArray[r].Visibility)
                {
                    var zR = realLandmarkArray[r].Coordinat.Z;
                    var zM = handLandmarkArray[r].Z;
                    var zValue = zM * x[0, 0] + x[1, 0];
                    var differenz = Math.Abs(zR - zValue);
                    differenzValue += differenz;
                    counterValue++;
                }

            }
            var d = differenzValue / counterValue;

            return x;
        }
    }

    //========== Specified Handmodels === v
    public class MediaPipeHand
    {
        //public string Name { get; set; }
        public RelativHandLandmark[] Data { get; set; }
        public int[,] Structure { get; }

        private readonly int numberOfJoints = 21;

        public MediaPipeHand()
        {

            Structure = new int[,]
            {
                    {0, 1},
                    {1, 2},
                    {2, 3},
                    {3, 4},
                    {0, 5},
                    {5, 6},
                    {6, 7},
                    {7, 8},
                    {5, 9},
                    {9, 10},
                    {10, 11},
                    {11, 12},
                    {9, 13},
                    {13, 14},
                    {14, 15},
                    {15, 16},
                    {13, 17},
                    {0, 17},
                    {17, 18},
                    {18, 19},
                    {19, 20},

            };


        }
        public void SetJsonData(string json)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(json);

            Data = new RelativHandLandmark[numberOfJoints];
            int i = 0;
            foreach (var landmark in jsonData) // creates landmark Objects and stores them in a List
            {

                int landmarkID = landmark.GetValue("id").ToObject<int>();
                int landmarkX = landmark.GetValue("x").ToObject<int>();
                int landmarkY = landmark.GetValue("y").ToObject<int>();
                float landMarkZ = landmark.GetValue("relZ").ToObject<float>();

                Data[i] = new RelativHandLandmark(landmarkID, landmarkX, landmarkY, landMarkZ);
                i++;
            }
        }
        public void SetData(RelativHandLandmark[] data)
        {
            Data = data;
        }
        public int GetJointCount()
        {
            return Structure.Length;
        }

    }
}
