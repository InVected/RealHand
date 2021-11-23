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

namespace RealHand
{
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
        public unsafe HandLandmark[] calculateWithDepth(HandLandmark[] landmarkList, DepthFrame depthFrame, VideoFrame colorFrame, DebugWindow debugWindow)
        {
            HandLandmark[] VisibleLandmarkList = getVisibleList(landmarkList, colorFrame, debugWindow);

            /*foreach (HandLandmark handLandmark in VisibleLandmarkList)
            {
                if (handLandmark.Visibility)
                {
                    //get Real coordinates
                }
            }
            // calculate formula to interpolate Occludet landmarks
            foreach (HandLandmark handLandmark in VisibleLandmarkList)
            {
                if (!handLandmark.Visibility)
                {
                    //calculate Real coordinates with formular
                }
            }*/
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

        /*private void OcclusionControle(HandLandmark[] LandmarkList)
        {

            HandLandmark[] SortedLandmarkList = new HandLandmark[LandmarkList.GetLength(0)];
            for (int i = 0; i < LandmarkList.GetLength(0); i++)
            {
                SortedLandmarkList[i] = LandmarkList[i].Clone();
            }
            //sort list
            float factor = 20.0F;
            for (int k = 0; k < SortedLandmarkList.GetLength(0); k++)
            {
                var continuer = false;
                HandLandmark backPoint = SortedLandmarkList[k];
                for (int m = k + 1; m < SortedLandmarkList.GetLength(0); m++)
                {
                    HandLandmark frontPoint = SortedLandmarkList[m];

                    foreach (int joint in ReStructure[m])
                    {
                        
                        HandLandmark jointPoint = SortedLandmarkList[joint];
                        if (frontPoint.Z < jointPoint.Z) { 
                        Vector2 pointJoint = new Vector2(jointPoint.X, jointPoint.Y);
                        Vector2 pointFront = new Vector2(frontPoint.X, frontPoint.Y);
                        Vector2 pointBack = new Vector2(backPoint.X, backPoint.Y);
                        bool result = isOccluded(pointJoint, pointFront, pointBack);
                        if (result)
                        {
                            LandmarkList[SortedLandmarkList[k].ID].Visibility = false;
                            continuer = true;
                            break;
                        }
                    }
                    }
                    if (continuer) continue;
                }
            }
        }*/
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



                if (true)
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
                else
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            Vector2 pointBack = new Vector2(x, y);
                            Vector2 u = Vector2.Subtract(pointJointB, pointBack);
                            var lambda = reziprokeDeterminante * (u.X * einheitsOrtogonal.Y - u.Y * einheitsOrtogonal.X); // relativ position of intersection on jointLine
                            var delta = reziprokeDeterminante * (u.Y * jointLine.X - u.X * jointLine.Y); //distance point to jointLine
                            if (lambda <= 1.0 && lambda >= 0.0 && Math.Abs(delta) <= factor)
                            {
                                var localLambdaZ = jointB.Z + lambda * (jointA.Z - jointB.Z);
                                int index = x + y * width;
                                pointer[index].R = (byte)(255.0*(localLambdaZ - minZ)/ (maxZ - minZ));
                                pointer[index].G = (byte)(255.0*(localLambdaZ - minZ)/ (maxZ - minZ));
                                pointer[index].B = (byte)(255.0*(localLambdaZ - minZ)/ (maxZ - minZ));

                            }
                        }
                    }
                }


                
                //=============================================================================

            }/*
            for ( int backpointPos = 0; backpointPos < SortedLandmarkList.Length; backpointPos++)
            {
                HandLandmark pointBack = SortedLandmarkList[backpointPos];
                if (LandmarkList[pointBack.ID].Visibility == true)
                {
                    
                    for (int frontPointPos = backpointPos + 1; frontPointPos < SortedLandmarkList.Length; frontPointPos++ )
                    {
                        HandLandmark pointFront = SortedLandmarkList[frontPointPos];
                        double distanceQuad = Math.Pow(pointBack.X - pointFront.X, 2) + Math.Pow(pointBack.Y - pointFront.Y, 2);
                        if (distanceQuad <= factor * factor)
                        {
                            LandmarkList[pointBack.ID].Visibility = false;
                            break;
                        }
                    }
                }

            }*/
        }
        /*
        private void OcclusionControle(HandLandmark[] LandmarkList)
        {

            HandLandmark[] SortedLandmarkList = new HandLandmark[LandmarkList.Length];
            for (int i = 0; i < LandmarkList.Length; i++)
            {
                SortedLandmarkList[i] = LandmarkList[i].Clone();
            }
            //sort List
            float factor = 20.0F;
            for (int k = 0; k < SortedLandmarkList.Length; k++)
            {
                var continuer = false;
                HandLandmark backPoint = SortedLandmarkList[k];
                for (int m = k + 1; m < SortedLandmarkList.Length; m++)
                {
                    HandLandmark frontPoint = SortedLandmarkList[m];
                    double distanceQuad = Math.Pow(backPoint.X - frontPoint.X, 2) + Math.Pow(backPoint.Y - frontPoint.Y, 2);
                    if (distanceQuad <= factor * factor)
                    {
                        LandmarkList[SortedLandmarkList[k].ID].Visibility = false;
                        continue;
                    }
                    foreach (int joint in ReStructure[m])
                    {
                        HandLandmark jointPoint = SortedLandmarkList[joint];
                        if (frontPoint.Z < jointPoint.Z)
                        {
                            Vector2 pointJoint = new Vector2(jointPoint.X, jointPoint.Y);
                            Vector2 pointFront = new Vector2(frontPoint.X, frontPoint.Y);
                            Vector2 pointBack = new Vector2(backPoint.X, backPoint.Y);
                            Vector2 jointLine = Vector2.Subtract(pointJoint, pointFront);
                            Vector2 ortogonal = new Vector2(jointLine.Y, -jointLine.X);
                            Vector2 einheitsOrtogonal = Vector2.Normalize(ortogonal);
                            Vector2 u = Vector2.Subtract(pointFront, pointBack);
                            var reziprokeDeterminante = 1.0 / (jointLine.X * einheitsOrtogonal.Y - jointLine.Y * einheitsOrtogonal.X);
                            var lambda = reziprokeDeterminante * (u.X * einheitsOrtogonal.Y - u.Y * einheitsOrtogonal.X); // relativ position of intersection on jointLine
                            var delta = reziprokeDeterminante * (u.Y * jointLine.X - u.X * jointLine.Y); //distance point to jointLine
                            if (lambda <= 1.0 && lambda >= 0.0 && Math.Abs(delta) <= factor)
                            {
                                LandmarkList[SortedLandmarkList[k].ID].Visibility = false;
                                continuer = true;
                                break;
                            }
                        }

                    }
                    if (continuer) continue;
                }
            }
        }*/

    }
}
