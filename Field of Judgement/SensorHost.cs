/*  
    Field Of Judgement, Copyright (C) 2014 Tomasz Hachaj, Ph.D
    The source code is under MIT license. 
    All dlls are under licenses provided by theirs copyright holders.

    The MIT License (MIT)
    Copyright (c) 2014 Tomasz Hachaj, Ph.D
  
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.IO;
using OpenTK;
using System.Diagnostics;
using org.GDL;
//using System.Windows.Media.Imaging;
//using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
//using Hexpoint.Blox;
//using Hexpoint.Blox.GameObjects.Units;
using System.Collections;
using System.Threading;
using FieldOfJudgement;
//using System.Windows.Media;

namespace FieldOfJudgement
{
    public class SensorHost
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        public KinectSensor Sensor = null;
        private Stopwatch stopwatch;
        private GDLInterpreter gDLInterpreterL = null;
        private GDLInterpreter gDLInterpreterR = null;
        Stopwatch recordingSKLStopwatch = null;

        //Game game = null;
        //public SensorHost(Game g)
        FieldOfJudgementGame program;

        public SensorHost(FieldOfJudgementGame program)
        {
            this.Sensor = null;
            this.program = program;
            //this.game = g;
            int sensors = 0;
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    //this.SelectKinnect.Items.Add("Kinect no. " + sensor);
                    //break;
                }
                sensors++;
            }
            if (sensors > 0)
            {
                Sensor = KinectSensor.KinectSensors[0];
            }
            StartSensor();
        }

        //////////////////////////////////////////////////////////
        long count = 0;
        public ArrayList recording = new ArrayList();
        long timeSkeletonCapture = -1;
        //Stopwatch st = null;
        StreamWriter sw = null;
        public void BeginCaptureToSKLFile(String fileNameSkeleton)
        {
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
            if (recordingSKLStopwatch != null)
            {
                recordingSKLStopwatch.Stop();
                recordingSKLStopwatch = null;
            }
            recordingSKLStopwatch = new Stopwatch();
            recordingSKLStopwatch.Start();
            recording.Clear();
            timeSkeletonCapture = -1;
            if (fileNameSkeleton == null)
            {
                String dirName = System.AppDomain.CurrentDomain.BaseDirectory;
                fileNameSkeleton = "SkeletonRecord";
                int imageIndex = 0;
                while (File.Exists(dirName + "/" + fileNameSkeleton + imageIndex + ".skl"))
                {
                    imageIndex++;
                }
                sw = new StreamWriter(dirName + "/" + fileNameSkeleton + imageIndex + ".skl");
            }
            else
                sw = new StreamWriter(fileNameSkeleton);


        }

        public void EndCaptureToSKLFile()
        {
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
            if (recordingSKLStopwatch != null)
            {
                recordingSKLStopwatch.Stop();
                recordingSKLStopwatch = null;
            }
        }
        /*
        ///////////////////////////////////
        //NIE DZIAŁA JAK TRZEBA
        public Thread sklPlayerThread = null;
        private ArrayList recordingToPlay = null;
        private bool playSKLRecording = false;
        public void PlayRecording(ArrayList recording)
        {
            if (playSKLRecording == true)
                return;
            this.recordingToPlay = recording;
            Thread ReaderThread = new Thread(PlayerThread);
            ReaderThread.Start();
        }
        
        private void PlayerThread()
        {
            int line = 0;
            TSkeleton[] tm = null;
            TSkeleton firstTracked = null;
            playSKLRecording = true;
            Skeleton []fT = new Skeleton[1];
            //cler the heap
            if (gDLInterpreter != null)
                gDLInterpreter.Heap.Clear();
            while (line < recordingToPlay.Count)
            {
                firstTracked = null;
                tm = (TSkeleton[])recordingToPlay[line];

                for (int a = 0; a < tm.Length; a++)
                {
                    if (firstTracked == null)
                        firstTracked = tm[a];
                     line++;
                }

                if (gDLInterpreter != null && firstTracked != null)
                {

                    double seconds = (double)firstTracked.TimePeriod / 1000.0;
                    
                    fT[0] = TSkeletonHelper.TSkeletonToSkeleton(firstTracked);
                    ProcessSkeletons(fT);
                }
                Thread.Sleep((int)tm[0].TimePeriod);
            }
            playSKLRecording = false;
        }
        ///////////////////////////////////
        */
        public void Dispose()
        {
            EndCaptureToSKLFile();
            if (null != this.Sensor)
            {
                this.Sensor.Stop();
            }
        }

        public void StartSensor()
        {
            //ParserToken[] features = null;
            //ParserToken[] rules = null;
            ParserToken[] rules = GDLParser.ParseFile("movements.gdl");
            gDLInterpreterR = new GDLInterpreter(rules);
            gDLInterpreterL = new GDLInterpreter(rules);
            // Start the sensor!
            try
            {
                if (Sensor != null)
                    Sensor.Start();
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            catch (IOException)
            {
                Sensor = null;
            }
            if (Sensor != null)
            {
                ////////////////////////////////
                //SKELETON
                // Turn on the skeleton stream to receive skeleton frames
                Sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                Sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                /////////////////////////////////
                //RGB
                // Turn on the color stream to receive color frames
                Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                Sensor.ColorFrameReady += this.SensorColorFrameReady;
                //this.PreviewImage.Source = this.skeletonBitmap;
                ////////////////////////////////
                //Depth
                // Turn on the depth stream to receive depth frames
                Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                // Add an event handler to be called whenever there is new depth frame data
                Sensor.DepthFrameReady += this.SensorDepthFrameReady;
            }

        }
        int frames = 0;
        public int getFrames()
        {
            return frames;
        }
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            int a = 0;
            a++;
            frames++;
        }

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private byte[] colorPixelsDepth;

        int a = 0;
        byte[] depthFrame32 = null;
        short[] depthPixelData = null;
        //public bool ToNear = false;
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    //if (this.depthPixelData != null)
                    {
                        this.depthPixelData = new short[depthFrame.PixelDataLength];
                    }
                    depthFrame.CopyPixelDataTo(this.depthPixelData);
                    this.ConvertDepthFrame(this.depthPixelData, program.players[0].ToNear, program.depthViewL, program.players[0].SkeletonArrayId);
                    this.ConvertDepthFrame(this.depthPixelData, program.players[1].ToNear, program.depthViewR, program.players[1].SkeletonArrayId);
                    ////////////////////////////
                    program.depthViewR.dirty = true;
                    program.depthViewL.dirty = true;
                }
            }
        }

        private void ConvertDepthFrame(short[] depthFrame, bool ToNear, TextureLoader tl, int skeletonArrayId)
        {
            int player = 0;
            //Run through the depth frame making the correlation between the two arrays
            if (ToNear)
            {
                for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < tl.pixels.Length; i16++, i32 += 4)
                {
                    int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    byte Distance = 0;
                    int MinimumDistance = 800;
                    int MaximumDistance = 4096;
                    if (realDepth > MinimumDistance)
                    {
                        Distance = (byte)(255 - ((realDepth - MinimumDistance) * 255 / (MaximumDistance - MinimumDistance)));
                        player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                        if (player != skeletonArrayId)
                            player = 0;
                        tl.pixels[i32 + AlphaIndex] = (byte)255;
                        if (player > 0)
                        {
                            tl.pixels[i32 + RedIndex] = (byte)(Distance);
                            tl.pixels[i32 + GreenIndex] = (byte)(Distance);
                            tl.pixels[i32 + BlueIndex] = (byte)(255);
                        }
                        else
                        {
                            tl.pixels[i32 + RedIndex] = (byte)(255);
                            tl.pixels[i32 + GreenIndex] = (byte)(Distance);
                            tl.pixels[i32 + BlueIndex] = (byte)(Distance);
                        }
                    }
                    else
                    {
                        tl.pixels[i32 + AlphaIndex] = (byte)255;
                        tl.pixels[i32 + RedIndex] = 0;
                        tl.pixels[i32 + GreenIndex] = 0;
                        tl.pixels[i32 + BlueIndex] = 0;
                    }
                }
            }
            else
            {
                for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < tl.pixels.Length; i16++, i32 += 4)
                {
                    int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    byte Distance = 0;
                    int MinimumDistance = 800;
                    int MaximumDistance = 4096;
                    if (realDepth > MinimumDistance)
                    {
                        Distance = (byte)(255 - ((realDepth - MinimumDistance) * 255 / (MaximumDistance - MinimumDistance)));
                        player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                        if (player != skeletonArrayId)
                            player = 0;
                        tl.pixels[i32 + AlphaIndex] = (byte)255;
                        if (player > 0)
                        {
                            tl.pixels[i32 + RedIndex] = (byte)(255);
                            tl.pixels[i32 + GreenIndex] = (byte)(Distance);
                            tl.pixels[i32 + BlueIndex] = (byte)(Distance);
                        }
                        else
                        {
                            tl.pixels[i32 + RedIndex] = (byte)(Distance);
                            tl.pixels[i32 + GreenIndex] = (byte)(Distance);
                            tl.pixels[i32 + BlueIndex] = (byte)(255);
                        }
                    }
                    else
                    {

                        tl.pixels[i32 + AlphaIndex] = (byte)255;
                        tl.pixels[i32 + RedIndex] = 0;
                        tl.pixels[i32 + GreenIndex] = 0;
                        tl.pixels[i32 + BlueIndex] = 0;
                    }
                }
            }
        }



        int RedIndex = 2;
        int GreenIndex = 1;
        int BlueIndex = 0;
        int AlphaIndex = 3;

        private void TrackClosestSkeleton(Skeleton[] skeletonData)
        {
            if (this.Sensor != null && this.Sensor.SkeletonStream != null)
            {
                if (!this.Sensor.SkeletonStream.AppChoosesSkeletons)
                {
                    this.Sensor.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
                }

                float closestDistance = 10000f; // Start with a far enough distance
                int closestID = 0;

                foreach (Skeleton skeleton in skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                {
                    if (skeleton.Position.Z < closestDistance)
                    {
                        closestID = skeleton.TrackingId;
                        closestDistance = skeleton.Position.Z;
                    }
                }

                if (closestID > 0)
                {
                    this.Sensor.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton
                }
            }
        }

        public static Point3D[] GenerateBodyPartArray(Skeleton skeleton, int user)
        {
            Point3D[] returnArray = new Point3D[skeleton.Joints.Count];
            if (skeleton == null) return null;
            for (int a = 0; a < returnArray.Length; a++)
            {
                returnArray[a] = new Point3D(0, 0, 0);
                try
                {
                    returnArray[a].X = skeleton.Joints[(JointType)a].Position.X * 1000.0f;
                    returnArray[a].Y = skeleton.Joints[(JointType)a].Position.Y * 1000.0f;
                    returnArray[a].Z = skeleton.Joints[(JointType)a].Position.Z * 1000.0f;
                }
                catch
                {

                }
            }

            return returnArray;
        }

        private float RightHandCursorPositionX = -1;
        private float RightHandCursorPositionY = -1;
        private float RightHandCursorPositionZ = -1;

        internal static double DegreesToRadians(double radians)
        {
            return radians / 57.2957795130823;
        }

        Vector3 ProjectionOnThePlane(Vector3 vectorToCalculate, Vector3 axis1, Vector3 axis2)
        {
            axis1.Normalize();
            axis2.Normalize();
            Vector3 normal = Vector3.Cross(axis1, axis2);
            normal.Normalize();
            Vector3 proj1 = Vector3.Dot(vectorToCalculate, normal) * normal / Vector3.Dot(normal, normal);
            Vector3 proj = vectorToCalculate - proj1;
            proj.Normalize();
            return proj;
        }
        public static Quaternion GetShortestRotationBetweenVectors(Vector3 vector1, Vector3 vector2)
        {
            vector1.Normalize();
            vector2.Normalize();
            float angle = (float)Math.Acos(Vector3.Dot(vector1, vector2));
            Vector3 axis = Vector3.Cross(vector2, vector1);

            // Check to see if the angle is very small, in which case, the cross product becomes unstable,
            // so set the axis to a default.  It doesn't matter much what this axis is, as the rotation angle 
            // will be near zero anyway.
            if (angle < 0.001f)
            {
                axis = new Vector3(0.0f, 0.0f, 1.0f);
            }

            if (axis.Length < .001f)
            {
                return Quaternion.Identity;
            }

            axis.Normalize();
            Quaternion rot = Quaternion.FromAxisAngle(
                new Vector3((float)axis.X, (float)axis.Y, (float)axis.Z),
                angle);

            return rot;
        }

        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float num1 = roll * 0.5f;
            float num2 = (float)Math.Sin((double)num1);
            float num3 = (float)Math.Cos((double)num1);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin((double)num4);
            float num6 = (float)Math.Cos((double)num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin((double)num7);
            float num9 = (float)Math.Cos((double)num7);
            Quaternion quaternion = Quaternion.Identity;
            quaternion.X = (float)((double)num9 * (double)num5 * (double)num3 + (double)num8 * (double)num6 * (double)num2);
            quaternion.Y = (float)((double)num8 * (double)num6 * (double)num3 - (double)num9 * (double)num5 * (double)num2);
            quaternion.Z = (float)((double)num9 * (double)num6 * (double)num2 - (double)num8 * (double)num5 * (double)num3);
            quaternion.W = (float)((double)num9 * (double)num6 * (double)num3 + (double)num8 * (double)num5 * (double)num2);
            return quaternion;
        }

        public Quaternion ComputeRotation(Vector3 vJoint, Vector3 axis)
        {
            Quaternion resultQ = Quaternion.Identity;
            vJoint.Normalize();
            axis.Normalize();
            float angle = (float)Math.Acos(Vector3.Dot(vJoint, axis));
            Vector3 nAxis = Vector3.Cross(vJoint, axis);
            Vector3 axis2 = new Vector3(nAxis.Z, nAxis.X, nAxis.Y);
            if (angle < 0.001f)
            {
                nAxis = new Vector3(0.0f, 0.0f, 1.0f);
            }

            if (axis.Length < .001f)
            {
                resultQ = Quaternion.Identity;
            }
            else
            {
                axis2.Normalize();
                resultQ = Quaternion.FromAxisAngle(axis2, angle);
            }
            return resultQ;
        }

        private void CalculatePlayerRotation(Player player, Skeleton skeleton)
        {
            Vector3 down = new Vector3(0, -1, 0);
            Vector3 chestJoint = down;
            Vector3 hipsJoint = down;

            Vector3 shoulderRJoint = new Vector3(
                skeleton.Joints[JointType.ElbowRight].Position.X - skeleton.Joints[JointType.ShoulderRight].Position.X,
                skeleton.Joints[JointType.ElbowRight].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y,
                skeleton.Joints[JointType.ElbowRight].Position.Z - skeleton.Joints[JointType.ShoulderRight].Position.Z);
            player.model.rightShoulder.rotationQ = ComputeRotation(shoulderRJoint, chestJoint);
            Vector3 shoulderLJoint = new Vector3(
                skeleton.Joints[JointType.ElbowLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X,
                skeleton.Joints[JointType.ElbowLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y,
                skeleton.Joints[JointType.ElbowLeft].Position.Z - skeleton.Joints[JointType.ShoulderLeft].Position.Z);
            player.model.leftShoulder.rotationQ = ComputeRotation(shoulderLJoint, chestJoint);

            player.model.leftElbow.rotationQ = ComputeRotation(new Vector3(
                skeleton.Joints[JointType.WristLeft].Position.X - skeleton.Joints[JointType.ElbowLeft].Position.X,
                skeleton.Joints[JointType.WristLeft].Position.Y - skeleton.Joints[JointType.ElbowLeft].Position.Y,
                skeleton.Joints[JointType.WristLeft].Position.Z - skeleton.Joints[JointType.ElbowLeft].Position.Z), shoulderLJoint);

            player.model.rightElbow.rotationQ = ComputeRotation(new Vector3(
                skeleton.Joints[JointType.WristRight].Position.X - skeleton.Joints[JointType.ElbowRight].Position.X,
                skeleton.Joints[JointType.WristRight].Position.Y - skeleton.Joints[JointType.ElbowRight].Position.Y,
                skeleton.Joints[JointType.WristRight].Position.Z - skeleton.Joints[JointType.ElbowRight].Position.Z), shoulderRJoint);

            Vector3 hipRJoint = new Vector3(
                skeleton.Joints[JointType.KneeRight].Position.X - skeleton.Joints[JointType.HipRight].Position.X,
                skeleton.Joints[JointType.KneeRight].Position.Y - skeleton.Joints[JointType.HipRight].Position.Y,
                skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.HipRight].Position.Z);
            player.model.rightHip.rotationQ = ComputeRotation(hipRJoint, hipsJoint);

            Vector3 kneeRJoint = new Vector3(
                skeleton.Joints[JointType.AnkleRight].Position.X - skeleton.Joints[JointType.KneeRight].Position.X,
                skeleton.Joints[JointType.AnkleRight].Position.Y - skeleton.Joints[JointType.KneeRight].Position.Y,
                skeleton.Joints[JointType.AnkleRight].Position.Z - skeleton.Joints[JointType.KneeRight].Position.Z);
            player.model.rightKnee.rotationQ = ComputeRotation(kneeRJoint, hipRJoint);

            Vector3 hipLJoint = new Vector3(
                skeleton.Joints[JointType.KneeLeft].Position.X - skeleton.Joints[JointType.HipLeft].Position.X,
                skeleton.Joints[JointType.KneeLeft].Position.Y - skeleton.Joints[JointType.HipLeft].Position.Y,
                skeleton.Joints[JointType.KneeLeft].Position.Z - skeleton.Joints[JointType.HipLeft].Position.Z);
            player.model.leftHip.rotationQ = ComputeRotation(hipLJoint, hipsJoint);

            Vector3 kneeLJoint = new Vector3(
                skeleton.Joints[JointType.AnkleLeft].Position.X - skeleton.Joints[JointType.KneeLeft].Position.X,
                skeleton.Joints[JointType.AnkleLeft].Position.Y - skeleton.Joints[JointType.KneeLeft].Position.Y,
                skeleton.Joints[JointType.AnkleLeft].Position.Z - skeleton.Joints[JointType.KneeLeft].Position.Z);
            player.model.leftKnee.rotationQ = ComputeRotation(kneeLJoint, hipLJoint);
        }

        private void ResetPlayerRotation(Player player)
        {
            player.model.chest.rotationQ = Quaternion.Identity;
            player.model.hips.rotationQ = Quaternion.Identity;
            player.model.rightShoulder.rotationQ = Quaternion.Identity;
            player.model.leftShoulder.rotationQ = Quaternion.Identity;
            player.model.rightElbow.rotationQ = Quaternion.Identity;
            player.model.rightHip.rotationQ = Quaternion.Identity;
            player.model.rightKnee.rotationQ = Quaternion.Identity;
            player.model.leftHip.rotationQ = Quaternion.Identity;
            player.model.leftKnee.rotationQ = Quaternion.Identity;
        }

        int firstTrackedId = -1;
        int secondTrackedId = -1;
        Skeleton[]skeletonsTracked = new Skeleton[2];
        Skeleton leftTracked = null;
        Skeleton rightTracked = null;
        

        private void ProcessMovements(Player player, double TimeHelp, GDLInterpreter gDLInterpreter, Skeleton skeleton)
        {
            Point3D[] bodyParts = GenerateBodyPartArray(skeleton, 0);
            String[] con = gDLInterpreter.ReturnConclusions(bodyParts, TimeHelp);

            player.backrward = 0;
            player.forward = 0;
            player.right = 0;
            player.left = 0;
            player.jump = 0;
            if (con.Contains("tonear!"))
            {
                player.ToNear = true;
            }
            else
            {
                //return;
                player.ToNear = false;
                if (!player.IsInsideForceField)
                {
                    if (con.Contains("walking!"))
                        player.forward = 3;

                    if (con.Contains("walkingf!"))
                        player.forward = 12;

                    if (con.Contains("walkingback!"))
                        player.backrward = 3;

                    if (con.Contains("walkingfback!"))
                        player.backrward = 12;

                    if (con.Contains("rights1!"))
                    {
                        player.right = 2;
                    }
                    if (con.Contains("rights2!"))
                    {
                        player.right = 5;
                    }
                    if (con.Contains("rights3!"))
                    {
                        player.right = 10;
                    }


                    if (con.Contains("lefts1!"))
                    {
                        player.left = 2;
                    }
                    if (con.Contains("lefts2!"))
                    {
                        player.left = 5;
                    }
                    if (con.Contains("lefts3!"))
                    {
                        player.left = 10;
                    }

                    if (con.Contains("jump!"))
                    {
                        player.forward = 8;
                        player.jump = 1;
                    }

                    if (con.Contains("castspell0!"))
                    {
                        player.CastSpell(0);
                    }
                    if (con.Contains("castspell1!"))
                    {
                        player.CastSpell(1);
                    }
                    if (con.Contains("castspell2!"))
                    {
                        player.CastSpell(2);
                    }


                    SkeletonPoint head = skeleton.Joints[JointType.Head].Position;
                    SkeletonPoint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position;
                    Vector3d v1 = new Vector3d(head.X, head.Y, head.Z);
                    Vector3d v2 = new Vector3d(shoulderCenter.X, shoulderCenter.Y, shoulderCenter.Z);
                    Vector3d v3 = v1 - v2;

                    Vector3d v4 = new Vector3d(0, 0, 1);

                    double angle = (Math.PI / 2.0) - Vector3d.CalculateAngle(v3, v4);

                    player.lookingUpDownFactor = (float)(angle * 57.2957795f);
                }
            }
        }

        private void ProcessSkeletons(Skeleton[] skeletons)
        {
            int trackingCount = 0;
            //if (!playSKLRecording)
            //TrackClosestSkeleton(skeletons);
            skeletonsTracked[0] = null;
            skeletonsTracked[1] = null;
            int []skeletonArrayPosition = new int[2];
            skeletonArrayPosition[0] = -1;
            skeletonArrayPosition[1] = -1;
            int leftSkeletonsTracked = -1;
            int rightSkeletonsTracked = -1;
            if (skeletons.Length != 0)
            {
                int a = 0;
                foreach (Skeleton skel in skeletons)
                {
                    a++;
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeletonsTracked[trackingCount] = skel;
                        skeletonArrayPosition[trackingCount] = a;
                        
                        trackingCount++;
                    }
                }
            }
            if (trackingCount < 1)
                return;
            if (trackingCount == 1)
            {
                bool found = false;
                if (leftTracked == skeletonsTracked[0])
                {
                    rightTracked = null;
                    leftTracked = skeletonsTracked[0];
                    leftSkeletonsTracked = skeletonArrayPosition[0];
                    found = true;
                }
                if (rightTracked == skeletonsTracked[0])
                {
                    leftTracked = null;
                    rightTracked = skeletonsTracked[0];
                    rightSkeletonsTracked = skeletonArrayPosition[0];
                    found = true;
                }
                if (!found)
                {
                    leftTracked = skeletonsTracked[0];
                    leftSkeletonsTracked = skeletonArrayPosition[0];
                }
            }
            if (trackingCount > 1)
            {
                if (skeletonsTracked[0].Joints[JointType.Spine].Position.X < skeletonsTracked[1].Joints[JointType.Spine].Position.X)
                {
                    leftTracked = skeletonsTracked[0];
                    rightTracked = skeletonsTracked[1];

                    leftSkeletonsTracked = skeletonArrayPosition[0];
                    rightSkeletonsTracked = skeletonArrayPosition[1];
                }
                else
                {
                    rightTracked = skeletonsTracked[0];
                    leftTracked = skeletonsTracked[1];

                    rightSkeletonsTracked = skeletonArrayPosition[0];
                    leftSkeletonsTracked = skeletonArrayPosition[1];
                }
            }
            if (leftTracked != null || rightTracked != null)
            {
                stopwatch.Stop();
                double TimeHelp = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Reset();
                stopwatch.Start();
                if (leftTracked != null)
                {
                    CalculatePlayerRotation(program.players[0], leftTracked);
                    ProcessMovements(program.players[0], TimeHelp, gDLInterpreterL, leftTracked);
                    program.players[0].SkeletonArrayId = leftSkeletonsTracked;

                }
                else
                {
                    ResetPlayerRotation(program.players[0]);
                    program.players[0].SkeletonArrayId = -1;
                    program.players[0].ToNear = false;
                }
                if (rightTracked != null)
                {
                    CalculatePlayerRotation(program.players[1], rightTracked);
                    ProcessMovements(program.players[1], TimeHelp, gDLInterpreterR, rightTracked);
                    program.players[1].SkeletonArrayId = rightSkeletonsTracked;
                }
                else
                {
                    ResetPlayerRotation(program.players[1]);
                    program.players[1].SkeletonArrayId = -1;
                    program.players[1].ToNear = false;
                }
            }
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            frames++;
            Skeleton[] skeletons = null;
            System.Tuple<float, float, float, float> FloorClipPlane = new Tuple<float, float, float, float>(0, 0, 0, 0);
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    FloorClipPlane = skeletonFrame.FloorClipPlane;
                }
            }
            ProcessSkeletons(skeletons);
        }
    }
}
