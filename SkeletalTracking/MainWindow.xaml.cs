// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Windows.Threading;
using Microsoft.Xna.Framework;

namespace SkeletalTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        float wingspan;
        bool closing = false;
        const int skeletonCount = 6; 
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        DispatcherTimer timer;
        bool hit;
        Random rdm;

        int score = 0;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;

            StopKinect(old);

            KinectSensor sensor = (KinectSensor)e.NewValue;

            if (sensor == null)
            {
                return;
            }

            


            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); 
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            try
            {
                sensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton first =  GetFirstSkeleton(e);

            if (first == null)
            {
                return; 
            }


            calibrate(first);
            //set scaled position
            //ScalePosition(headImage, first.Joints[JointType.Head]);
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);
            
            GetCameraPoint(first, e);

            checkForHit(first);
        }
        void checkForHit(Skeleton skel)
        {
            if (!hit)
            {
                //
                double ellipseCenterX = Canvas.GetLeft(ellipse1) + ellipse1.Width / 2;
                double ellipseCenterY = Canvas.GetTop(ellipse1) + ellipse1.Height / 2;


                //lefthand left / top
                double leftCenterX = Canvas.GetLeft(leftEllipse) + leftEllipse.Width / 2;
                double leftCenterY = Canvas.GetTop(leftEllipse) + leftEllipse.Height / 2;

                double upperBoundLeft = ellipse1.Width / 2 + leftEllipse.Width / 2;
                double actualDistanceLeft = Math.Sqrt(Math.Pow(ellipseCenterX - leftCenterX, 2) + Math.Pow(ellipseCenterY - leftCenterY, 2));

                //righthand left / top
                double rightCenterX = Canvas.GetLeft(rightEllipse) - rightEllipse.Width / 2;
                double rightCenterY = Canvas.GetTop(rightEllipse) - rightEllipse.Height / 2;

                double upperBoundRight = ellipse1.Width / 2 + rightEllipse.Width / 2;
                double actualDistanceRight = Math.Sqrt(Math.Pow(ellipseCenterX - rightCenterX, 2) + Math.Pow(ellipseCenterY - rightCenterY, 2));

                if (actualDistanceLeft < upperBoundLeft || actualDistanceRight < upperBoundRight)
                {
                    //increment score label
                    score++;
                    label2.Content = score;
                    //set hit to true
                    //hit = true;

                    double canvasWidthLimit = kinectColorViewer1.Width - ellipse1.Width;
                    double canvasHeightLimit = kinectColorViewer1.Height - ellipse1.Height;

                    double xspan = ((wingspan * 2) * rdm.NextDouble()) - wingspan + skel.Joints[JointType.ShoulderCenter].Position.X;
                    double yspan = ((wingspan * 2) * rdm.NextDouble()) - wingspan + skel.Joints[JointType.ShoulderCenter].Position.Y;

                    if (xspan < canvasWidthLimit  && yspan < canvasHeightLimit) {
                        //PICK BACK UP FROM HERE
                    }

                    

                    Canvas.SetLeft(ellipse1,rdm.Next(Convert.ToInt32(canvasWidthLimit)));
                    Canvas.SetTop(ellipse1, rdm.Next(Convert.ToInt32(canvasHeightLimit)));
                }

            }
        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }
                

                //Map a joint location to a point on the depth map
                //head
                DepthImagePoint headDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left hand
                DepthImagePoint leftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //right hand
                DepthImagePoint rightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);


                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftColorPoint =
                    depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightColorPoint =
                    depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

               //Subtraction things
                //Left Hand
                Joint leftHand = first.Joints[JointType.HandLeft];
                float leftHandDepth = first.Joints[JointType.HandLeft].Position.Z;
                //Right Hand
                Joint rightHand = first.Joints[JointType.HandRight];
                float rightHandDepth = first.Joints[JointType.HandRight].Position.Z;
                //Elbows
                float leftElbowDepth = first.Joints[JointType.ElbowLeft].Position.Z;
                float rightElbowDepth = first.Joints[JointType.ElbowRight].Position.Z;
                //Head & Chest
                Joint head = first.Joints[JointType.Head];
                float headDepth = first.Joints[JointType.Head].Position.Z;
                Joint chest = first.Joints[JointType.ShoulderCenter];
                float chestDepth = first.Joints[JointType.ShoulderCenter].Position.Z;
                //Punch values
                float leftPunch =  chestDepth - leftHandDepth;
                float rightPunch = chestDepth - rightHandDepth;

                CameraPosition(leftEllipse, leftColorPoint);
                CameraPosition(rightEllipse, rightColorPoint);

                

                if (isBlocking(leftHand, head, 0.3) && isBlocking(rightHand,head,0.3))
                {
                    leftEllipse.Fill = Brushes.Black;
                    rightEllipse.Fill = Brushes.Black;
                }
                else if(isBlocking(leftHand, chest, 0.3) && isBlocking(rightHand,chest,0.3))
                {
                    leftEllipse.Fill = Brushes.Green;
                    rightEllipse.Fill = Brushes.Green;
                }
                else if (leftPunch > 0.35)
                {
                    rightEllipse.Fill = Brushes.White;
                    leftEllipse.Fill = Brushes.Red;
                    if (leftElbowDepth - leftHandDepth < .2 && leftElbowDepth - leftHandDepth >=-.1)
                    {
                        leftEllipse.Fill = Brushes.Orange;
                    }
                }
                else if (rightPunch > 0.35)
                {
                    //rightEllipse.Fill = Brushes
                    leftEllipse.Fill = Brushes.White;
                    rightEllipse.Fill = Brushes.Red;
                    if (rightElbowDepth - rightHandDepth < .2 && rightElbowDepth - rightHandDepth >=-.1)
                    {
                        rightEllipse.Fill = Brushes.Orange;
                    }
                }
                /*Detecting when hands go behind the head - this was to ensure that you couldn't draw
                / a hand back and get that counted as a punch*/
                else if (leftHandDepth > headDepth && rightHandDepth > headDepth)
                {
                    leftEllipse.Fill = Brushes.Aqua;
                    rightEllipse.Fill = Brushes.Aqua;
                }

                else
                {
                    leftEllipse.Fill = Brushes.White;
                    rightEllipse.Fill = Brushes.White;

                }
                
            }        
        }


        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null; 
                }

                
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                         where s.TrackingState == SkeletonTrackingState.Tracked
                                         select s).FirstOrDefault();
 
                return first;

            }
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);//center image on the joint
            Canvas.SetTop(element, point.Y - element.Height / 2);//center image on the joint

        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 
            
            //convert & scale (.3 = means 1/3 of joint distance)
            Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y); 
            
        }

        private bool isBlocking(Joint hand, Joint target, double Radius)
        {
          float handX = hand.Position.X;
          float handY = hand.Position.Y;

          float targetX = target.Position.X;
          float targetY = target.Position.Y;

          float resultX = handX - targetX;
          float resultY = handY - targetY;
          float hypot = (float)Math.Sqrt(Math.Pow(resultX, 2) + Math.Pow(resultY, 2));
          //label2.Content = hand.Position.Z - target.Position.Z;
          
          if (hypot <= Radius && hand.Position.Z - target.Position.Z>= -.3)
          {
              return true;
          }
          
            return false;
        }

        public Boolean calibrate(Skeleton skel){

            if ((skel.Joints[JointType.HandLeft].Position.Y - skel.Joints[JointType.HandRight].Position.Y) < .25 && (skel.Joints[JointType.HandLeft].Position.Y - skel.Joints[JointType.HandRight].Position.Y) > -.25)
            {
                if ((skel.Joints[JointType.HandLeft].Position.Z - skel.Joints[JointType.ShoulderCenter].Position.Z) > -.075 
                    && (skel.Joints[JointType.HandRight].Position.Z - skel.Joints[JointType.ShoulderCenter].Position.Z) > -.075) 
                {
                    label3.Content = skel.Joints[JointType.ShoulderRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y;
                    label4.Content = skel.Joints[JointType.ElbowLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y;
                    label5.Content = skel.Joints[JointType.ShoulderLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y;
                    label6.Content = skel.Joints[JointType.ElbowRight].Position.X < skel.Joints[JointType.HandRight].Position.X;
                    label7.Content = skel.Joints[JointType.ElbowRight].Position.X > skel.Joints[JointType.ShoulderRight].Position.X;
                    label8.Content = skel.Joints[JointType.HandLeft].Position.X < skel.Joints[JointType.ElbowLeft].Position.X;
                    label9.Content = skel.Joints[JointType.ElbowLeft].Position.X < skel.Joints[JointType.ShoulderLeft].Position.X;

                    if (skel.Joints[JointType.ElbowRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y < 0.075
                        && skel.Joints[JointType.ShoulderRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y < 0.075
                        && skel.Joints[JointType.ElbowLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y < 0.075
                        && skel.Joints[JointType.ShoulderLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y < 0.075
                        && skel.Joints[JointType.ElbowRight].Position.X < skel.Joints[JointType.HandRight].Position.X
                        && skel.Joints[JointType.ElbowRight].Position.X > skel.Joints[JointType.ShoulderRight].Position.X
                        && skel.Joints[JointType.HandLeft].Position.X < skel.Joints[JointType.ElbowLeft].Position.X
                        && skel.Joints[JointType.ElbowLeft].Position.X < skel.Joints[JointType.ShoulderLeft].Position.X)
                    {
                       



                        wingspan = (skel.Joints[JointType.HandLeft].Position.X - skel.Joints[JointType.HandRight].Position.X / 2);
                        label2.Content = wingspan;
                        return true;
                    }
                }
            }







            return false;
           

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true; 
            StopKinect(kinectSensorChooser1.Kinect); 
        }



    }
}
