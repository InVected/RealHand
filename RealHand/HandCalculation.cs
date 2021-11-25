using Intel.RealSense;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace RealHand
{
    public class RealLandmark
    {
        public int ID { get; set; }
        public Vector3 Coordinat { get; set; }
        public bool Visibility { get; set; }
        public RealLandmark(int id, Vector3 coordinat)
        {
            ID = id;
            Coordinat = coordinat;
            Visibility = false;

        }
        public RealLandmark(int id, Vector3 coordinat, bool visibility)
        {
            ID = id;
            Coordinat = coordinat;
            Visibility = visibility;

        }
        public RealLandmark(int id, float x, float y, float z)
        {
            ID = id;
            Coordinat = new Vector3(x, y, z);
            Visibility = false;

        }
        public RealLandmark(int id, float x, float y, float z, bool visibility)
        {
            ID = id;
            Coordinat = new Vector3(x, y, z);
            Visibility = visibility;

        }
        public RealLandmark Clone()
        {
            return new RealLandmark(ID, Coordinat, Visibility);
        }
    }

    public class HandLandmark
    {
        //public string Name { get; set; }
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public float Z { get; set; }
        public bool Visibility { get; set; }
        public HandLandmark(int id, int x, int y, float z)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Visibility = true;

        }
        public HandLandmark(int id, int x, int y, float z, bool visibility)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Visibility = visibility;

        }
        public HandLandmark Clone()
        {
            return new HandLandmark(ID, X, Y, Z, Visibility);
        }

    }

    public class MediaPipeHand
    {
        //public string Name { get; set; }
        public HandLandmark[] Data { get; set; }
        public int[,] Structure { get; }

        private int numberOfJoints = 21;

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
        public void setJsonData(string json)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(json);

            Data = new HandLandmark[numberOfJoints];
            int i = 0;
            foreach (var landmark in jsonData) // creates landmark Objects and stores them in a List
            {

                int landmarkID = landmark.GetValue("id").ToObject<int>();
                int landmarkX = landmark.GetValue("x").ToObject<int>();
                int landmarkY = landmark.GetValue("y").ToObject<int>();
                float landMarkZ = landmark.GetValue("relZ").ToObject<float>();

                Data[i] = new HandLandmark(landmarkID, landmarkX, landmarkY, landMarkZ);
                i++;
            }
        }
        public void setData(HandLandmark[] data)
        {
            Data = data;
        }
        public int getJointCount()
        {
            return Structure.Length;
        }

    }
    public class RealHandmark
    {
        public int[,] Structure { get; set; }
        public List<int>[] ReStructure;
        public HandLandmark[] Data { get; }
        public RealHandmark(int[,] structure, int landmarkCount)
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
        public List<HandLandmark> getOnlyVisibleList(HandLandmark[] landmarkList, VideoFrame colorFrame, DebugWindow debugWindow)
        {
            HandLandmark[] VisibleLandmarkList = getVisibleList(landmarkList, colorFrame, debugWindow);
            List<HandLandmark> tempList = new List<HandLandmark> { };
            foreach (HandLandmark landmark in VisibleLandmarkList)
            {
                if (landmark.Visibility)
                {
                    tempList.Add(landmark);
                }
            }
            tempList = tempList.OrderBy(landmark => landmark.ID).ToList();
            return tempList;

        }
        public unsafe HandLandmark[] getVisibleList(HandLandmark[] landmarkList, VideoFrame colorFrame, DebugWindow debugWindow)
        {
            if (landmarkList != null && landmarkList.Length != 0)
            {
                BorderControle(landmarkList, colorFrame);
                OcclusionControle(landmarkList, debugWindow);
            }
            return landmarkList;
        }
        public unsafe HandLandmark[] getVisibleList(HandLandmark[] landmarkList, VideoFrame colorFrame)
        {
            if (landmarkList != null && landmarkList.Length != 0)
            {
                BorderControle(landmarkList, colorFrame);
                //OcclusionControle(landmarkList, debugWindow);
            }
            return landmarkList;
        }

        public unsafe HandLandmark[] calculateWithDepth(HandLandmark[] VisibleLandmarkList, DepthFrame depthFrame, VideoFrame colorFrame, DebugWindow debugWindow, Intrinsics intrinsics)
        {
            RealLandmark[] RealLandmarkArray = new RealLandmark[VisibleLandmarkList.GetLength(0)];
            int counter=0;
            for (int i =0; i< VisibleLandmarkList.GetLength(0); i++)
            {
                if (VisibleLandmarkList[i].Visibility)
                {
                    RealLandmarkArray[i] = RealWorldCoordinates(VisibleLandmarkList[i], colorFrame, depthFrame, intrinsics);
                    RealLandmarkArray[i].Visibility = true;
                    counter++;
                }
                else
                {
                    RealLandmarkArray[i] = new RealLandmark(VisibleLandmarkList[i].ID, VisibleLandmarkList[i].X, VisibleLandmarkList[i].Y, VisibleLandmarkList[i].Z);
                }
                
            }
            double factor = DepthFactor(RealLandmarkArray, VisibleLandmarkList, counter);





            return VisibleLandmarkList;
        }
        public bool hasData()
        {
            if (Data != null) return false;
            else return true;
        }
        private void BorderControle(HandLandmark[] LandmarkList, VideoFrame colorFrame)
        {
            int width = colorFrame.Width;
            int height = colorFrame.Height;

            foreach (HandLandmark handLandmark in LandmarkList)
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

        public bool isOccluded(Vector2 pointJoint, Vector2 pointFront, Vector2 pointBack)
        {
            float factor = 20;
            double distanceQuad = Math.Pow(pointBack.X - pointFront.X, 2) + Math.Pow(pointBack.Y - pointFront.Y, 2);
            if (distanceQuad <= factor * factor)
            {
                return true;
            }

            Vector2 jointLine = Vector2.Subtract(pointJoint, pointFront);
            Vector2 ortogonal = new Vector2(jointLine.Y, -jointLine.X);
            Vector2 einheitsOrtogonal = Vector2.Normalize(ortogonal);
            Vector2 u = Vector2.Subtract(pointFront, pointBack);
            var reziprokeDeterminante = 1.0 / (jointLine.X * einheitsOrtogonal.Y - jointLine.Y * einheitsOrtogonal.X);
            var lambda = reziprokeDeterminante * (u.X * einheitsOrtogonal.Y - u.Y * einheitsOrtogonal.X); // relativ position of intersection on jointLine
            var delta = reziprokeDeterminante * (u.Y * jointLine.X - u.X * jointLine.Y); //distance point to jointLine
            if (lambda <= 1.0 && lambda >= 0.0 && Math.Abs(delta) <= factor)
            {
                return true;
            }

            return false;
        }

        private unsafe void OcclusionControle(HandLandmark[] LandmarkList, DebugWindow debugWindow)
        {

            HandLandmark[] SortedLandmarkList = new HandLandmark[LandmarkList.Length];
            for (int i = 0; i < LandmarkList.Length; i++)
            {
                SortedLandmarkList[i] = LandmarkList[i].Clone();
            }
            Array.Sort(SortedLandmarkList, delegate (HandLandmark x, HandLandmark y) { return x.Z.CompareTo(y.Z); });
            //sort joints by List
            float factor = 30.0F;
            Trace.WriteLine(LandmarkList[8].Z);
         

            float minZ = SortedLandmarkList[0].Z;
            float maxZ = SortedLandmarkList[SortedLandmarkList.Length - 1].Z;

            DebugWindow.RGB* pointer = debugWindow.GetPixelPointer(); /// !!!!!!!!!!!!!!!!!!!!! causes Thread issues

            int width = 0;
            int height = 0;

            var bmp = debugWindow.imgColor.Source as WriteableBitmap;
            unsafe
            {
                using (var context = bmp.GetBitmapContext())
                {
                    width = context.Width;
                    height = context.Height;
                }
            }
                

                    //=============================================================================================================================
            for (int joint = 0; joint < Structure.GetLength(0); joint++)
            {
                int jointAId = Structure[joint, 0];
                int jointBId = Structure[joint, 1];
                HandLandmark jointA = LandmarkList[jointAId];
                HandLandmark jointB = LandmarkList[jointBId];
                Vector2 pointJointA = new Vector2(jointA.X, jointA.Y);
                Vector2 pointJointB = new Vector2(jointB.X, jointB.Y);

                
                Vector2 jointLine = Vector2.Subtract(pointJointB, pointJointA);
                Vector2 ortogonal = new Vector2(jointLine.Y, -jointLine.X);
                Vector2 einheitsOrtogonal = Vector2.Normalize(ortogonal);
                
                var reziprokeDeterminante = 1.0 / (jointLine.X * einheitsOrtogonal.Y - jointLine.Y * einheitsOrtogonal.X);


                {
                    foreach (HandLandmark backPoint in SortedLandmarkList)
                    {
                        if (LandmarkList[backPoint.ID].Visibility == true && (backPoint.Z > jointA.Z || backPoint.Z > jointB.Z) && backPoint.ID != jointA.ID && backPoint.ID != jointB.ID) // if not point part of joint
                        {

                            Vector2 pointBack = new Vector2(backPoint.X, backPoint.Y);
                            Vector2 u = Vector2.Subtract(pointJointB, pointBack);
                            var lambda = reziprokeDeterminante * (u.X * einheitsOrtogonal.Y - u.Y * einheitsOrtogonal.X); // relativ position of intersection on jointLine
                            var delta = reziprokeDeterminante * (u.Y * jointLine.X - u.X * jointLine.Y); //distance point to jointLine
                            if (lambda <= 1.0 && lambda >= 0.0 && Math.Abs(delta) <= factor)
                            {
                                var localLambdaZ = jointB.Z + lambda * (jointA.Z - jointB.Z);
                                if (localLambdaZ <= backPoint.Z)
                                {
                                    LandmarkList[backPoint.ID].Visibility = false;

                                    Trace.WriteLine("Joint: " + jointA.ID + " " + jointB.ID + " Landmark: " + backPoint.ID);
                                }


                            }
                        }


                    }
                }

                //=============================================================================

            }
        }
     private RealLandmark RealWorldCoordinates(HandLandmark handLandmark, VideoFrame colorFrame, DepthFrame depthFrame, Intrinsics intrinsics)
        {
           
            var xM = handLandmark.X;
            var yM = handLandmark.Y;
            Vector2 point2D = new Vector2(xM, yM);
            float depth = depthFrame.GetDistance(xM, yM);
            var point3D = Map2DTo3D(intrinsics, point2D, depth);
            RealLandmark landmark = new RealLandmark(handLandmark.ID, point3D);
            
            return landmark;

        }
        public Vector3 Map2DTo3D(Intrinsics intrinsics, Vector2 pixel, float depth)
        {
            Vector3 point = new Vector3();

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
        private double DepthFactor(RealLandmark[] realLandmarkArray, HandLandmark[] handLandmarkArray, int counter)
        {
           
            double[,] aArray = new double[counter,2];
            double[,] bArray = new double[counter,1];

            for (int r = 0; r < handLandmarkArray.GetLength(0); r++)
            {
                if (handLandmarkArray[r].Visibility)
                {
                    aArray[(counter) - 1, 0] = handLandmarkArray[r].Z;
                    aArray[(counter) - 1, 1] = 1;
                    bArray[(counter) - 1,0] = realLandmarkArray[r].Coordinat.Z;
                    counter--;
                }
            }
            Matrix<double> A = DenseMatrix.OfArray(aArray);
            Matrix<double> b = DenseMatrix.OfArray(bArray);
            var x = A.Transpose().Multiply(A).Inverse().Multiply(A.Transpose().Multiply(b));
            Trace.WriteLine(x);
            var counterValue = 0;
            var differenzValue = 0.0;
            for (int r = 0; r < realLandmarkArray.GetLength(0); r++)
            {
                if (realLandmarkArray[r].Visibility)
                {
                    var zR = realLandmarkArray[r].Coordinat.Z;
                    var zM = handLandmarkArray[r].Z;
                    var zValue = zM * x[0,0] + x[1,0];
                    var differenz = Math.Abs( zR - zValue);
                    differenzValue = differenzValue + differenz;
                    counterValue++;
                }

            }
            Trace.WriteLine(differenzValue / counterValue);
            

            return 0;
        }
    }
}
