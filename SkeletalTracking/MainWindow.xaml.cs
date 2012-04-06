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
        bool gameStart = false;
        int calibrateTimer = 0;
        int warningCounter = 0;
        int strikeZone = 0;
        bool warn = false;
        int attackCounter = 0;
        double wingspan;
        bool closing = false;
        const int skeletonCount = 6; 
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        DispatcherTimer timer;
        bool hit;
        Random rdm = new Random();

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
            ScalePosition(headWarning, first.Joints[JointType.Head]);
            ScalePosition(bodyWarning, first.Joints[JointType.ShoulderCenter]);
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);
            ScalePosition(chestEllipse, first.Joints[JointType.ShoulderCenter]);
            
            GetCameraPoint(first, e);
            prepAttack(first);
            checkForHit(first);
            
        }
        void checkForHit(Skeleton skel)
        {
            int currentAction = CheckActions(skel);
            if (currentAction >= 3 && !hit)
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
                double rightCenterX = Canvas.GetLeft(rightEllipse) + rightEllipse.Width / 2;
                double rightCenterY = Canvas.GetTop(rightEllipse) + rightEllipse.Height / 2;

                double upperBoundRight = ellipse1.Width / 2 + rightEllipse.Width / 2;
                double actualDistanceRight = Math.Sqrt(Math.Pow(ellipseCenterX - rightCenterX, 2) + Math.Pow(ellipseCenterY - rightCenterY, 2));

                if (actualDistanceLeft < upperBoundLeft && (currentAction == 3 || currentAction == 4))
                {
                    //increment score label
                    score = score + (currentAction % 2) + 1;
                    label2.Content = score;
                    //set hit to true
                    hit = true;
                }

                if (actualDistanceRight < upperBoundRight && (currentAction == 5 || currentAction == 6))
                {
                    //increment score label
                    score = score + (currentAction % 2) + 1;
                    label2.Content = score;
                    //set hit to true
                    hit = true;
                }
                
            }

            if (hit){
                DrawTarget(skel);
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
                //chest
                DepthImagePoint chestDepthPoint = 
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderCenter].Position);
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
                //chest
                ColorImagePoint chestColorPoint =
                    depth.MapToColorImagePoint(chestDepthPoint.X, chestDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                CameraPosition(leftEllipse, leftColorPoint);
                CameraPosition(rightEllipse, rightColorPoint);
                CameraPosition(chestEllipse, chestColorPoint);
                CameraPosition(bodyWarning, chestColorPoint);
                CameraPosition(headWarning, headColorPoint);



               
                
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

        public void calibrate(Skeleton skel){
            
            

            if ((skel.Joints[JointType.HandLeft].Position.Y - skel.Joints[JointType.HandRight].Position.Y) < .25 && (skel.Joints[JointType.HandLeft].Position.Y - skel.Joints[JointType.HandRight].Position.Y) > -.25)
            {
                if ((skel.Joints[JointType.HandLeft].Position.Z - skel.Joints[JointType.ShoulderCenter].Position.Z) > -.075 
                    && (skel.Joints[JointType.HandRight].Position.Z - skel.Joints[JointType.ShoulderCenter].Position.Z) > -.075) 
                {
                    if (skel.Joints[JointType.ElbowRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y < 0.075
                        && skel.Joints[JointType.ShoulderRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y < 0.075
                        && skel.Joints[JointType.ElbowLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y < 0.075
                        && skel.Joints[JointType.ShoulderLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y < 0.075
                        && skel.Joints[JointType.ElbowRight].Position.X < skel.Joints[JointType.HandRight].Position.X
                        && skel.Joints[JointType.ElbowRight].Position.X > skel.Joints[JointType.ShoulderRight].Position.X
                        && skel.Joints[JointType.HandLeft].Position.X < skel.Joints[JointType.ElbowLeft].Position.X
                        && skel.Joints[JointType.ElbowLeft].Position.X < skel.Joints[JointType.ShoulderLeft].Position.X)
                    {
                        attackCounter = 0;
                        calibrateTimer++;
                        
                        if (calibrateTimer >= 8)
                        {
                            warningLabel.Visibility = Visibility.Visible;
                            warningLabel.Content = "Calibrating...";
                        }
                        //wingspan = (skel.Joints[JointType.HandLeft].Position.X - skel.Joints[JointType.HandRight].Position.X / 2);
                        if (calibrateTimer == 30)
                        {
                            wingspan = (Canvas.GetLeft(rightEllipse) - Canvas.GetLeft(chestEllipse));
                            gameStart = true;
                            ellipse1.Visibility = Visibility.Visible;
                            DrawTarget(skel);
                            //label2.Content = wingspan;
                            calibrateTimer = 0;
                            warningLabel.Visibility = Visibility.Hidden;

                        }
                       
                    }
                    else
                    {
                        ellipse1.Visibility = Visibility.Visible;
                        calibrateTimer = 0;
                        
                    }
                    
                }
                else
                {
                    calibrateTimer = 0;
                    warningLabel.Visibility = Visibility.Hidden;
                    
                }
            }
            else
            {
                calibrateTimer = 0;
                warningLabel.Visibility = Visibility.Hidden;
                
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true; 
            StopKinect(kinectSensorChooser1.Kinect); 
        }

        private void DrawTarget(Skeleton drawtargetskel)
        {
            double canvasWidthLimit = kinectColorViewer1.Width - ellipse1.Width;
            double canvasHeightLimit = kinectColorViewer1.Height - ellipse1.Height;

            //double xspan = ((wingspan * 2) * rdm.NextDouble()) - wingspan + drawtargetskel.Joints[JointType.ShoulderCenter].Position.X;
            //double yspan = ((wingspan * 2) * rdm.NextDouble()) - wingspan + drawtargetskel.Joints[JointType.ShoulderCenter].Position.Y;

            double xspan = Canvas.GetLeft(chestEllipse) + ((rdm.NextDouble() * (2 * wingspan) - wingspan));
            double yspan = Canvas.GetTop(chestEllipse) + ((rdm.NextDouble() * (2 * wingspan) - wingspan));

            //label6.Content = drawtargetskel.Joints[JointType.ShoulderCenter].Position.X;
            
            //label6.Content = skel.Joints[JointType.ElbowRight].Position.X < skel.Joints[JointType.HandRight].Position.X;
            

            if (xspan > canvasWidthLimit || xspan < 0)
            {
                warningLabel.Content = "Please Center Yourself On Screen";
                warningLabel.Visibility = Visibility.Visible;

            }

            else if (yspan > canvasHeightLimit || yspan < 0)
            {
                warningLabel.Content = "Please Center Yourself On Screen";
                warningLabel.Visibility = Visibility.Visible;

            }
            else
            {
                warningLabel.Visibility = Visibility.Hidden;
            }

            Canvas.SetLeft(ellipse1, xspan);
            Canvas.SetTop(ellipse1, yspan);

            hit = false;
        }
        private void prepAttack(Skeleton first)
        {
            if (gameStart == false) {
                return;
            }
            int playerAction = CheckActions(first);
            if (warn == true)
            {
                warningCounter++;
                if (warningCounter % 2 == 0)
                {
                    if (strikeZone != 1)
                    {
                        bodyWarning.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        headWarning.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    bodyWarning.Visibility = Visibility.Hidden;
                    headWarning.Visibility = Visibility.Hidden;
                }
            }
            if(warningCounter == 25){
                opponentAttack(first,strikeZone);
                attackCounter = 0;
                bodyWarning.Visibility = Visibility.Hidden;
                headWarning.Visibility = Visibility.Hidden;
                warn = false;
                warningCounter = 0;
            }
            if (playerAction != 1 && playerAction != 2)
            {
                attackCounter++;
            }
            if (attackCounter == 45)//DECREASE TO GAME MAKE HARDER
            {
                warn = true;
                strikeZone = rdm.Next(3);
            }
        }
        private void opponentAttack(Skeleton first, int attack)
        {
            int playerAction = CheckActions(first);
            if (attack == 1)
            {
                //BOOM! HEADSHOT!
                headWarning.Visibility = Visibility.Visible;
                if (playerAction != 1)
                {
                    score -= 2;
                    label2.Content = score;
                }
                else
                {
                    score += 2;
                    label2.Content = score;
                }
            }
            else
            {
                //UGGH! BODYSHOT!
                if (playerAction != 2)
                {
                    bodyWarning.Visibility = Visibility.Visible;
                    score--;
                    label2.Content = score;
                }
                else
                {
                    score += 1;
                    label2.Content = score;
                }
            }
        }
        //nothing = 0
        //headblocking = 1
        //chestblocking = 2        
        //leftUppercut = 3
        //leftPunch = 4
        //rightUppercut = 5
        //rightPunch = 6
        private int CheckActions(Skeleton first)
        {
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
            float leftPunch = chestDepth - leftHandDepth;
            float rightPunch = chestDepth - rightHandDepth;

            if (isBlocking(leftHand, head, 0.3) && isBlocking(rightHand, head, 0.3))
            {
                leftEllipse.Fill = Brushes.Black;
                rightEllipse.Fill = Brushes.Black;
                return 1;
            }
            else if (isBlocking(leftHand, chest, 0.3) && isBlocking(rightHand, chest, 0.3))
            {
                leftEllipse.Fill = Brushes.Green;
                rightEllipse.Fill = Brushes.Green;
                return 2;
            }
            else if (leftPunch > 0.35)
            {
                rightEllipse.Fill = Brushes.White;
                leftEllipse.Fill = Brushes.Red;
                if (leftElbowDepth - leftHandDepth < .2 && leftElbowDepth - leftHandDepth >= -.1)
                {
                    leftEllipse.Fill = Brushes.Orange;
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
            else if (rightPunch > 0.35)
            {
                //rightEllipse.Fill = Brushes
                leftEllipse.Fill = Brushes.White;
                rightEllipse.Fill = Brushes.Red;
                if (rightElbowDepth - rightHandDepth < .2 && rightElbowDepth - rightHandDepth >= -.1)
                {
                    rightEllipse.Fill = Brushes.Orange;
                    return 5;
                }
                else
                {
                    return 6;
                }
            }
            /*Detecting when hands go behind the head - this was to ensure that you couldn't draw
            / a hand back and get that counted as a punch*/
            else if (leftHandDepth > headDepth && rightHandDepth > headDepth)
            {
                leftEllipse.Fill = Brushes.Aqua;
                rightEllipse.Fill = Brushes.Aqua;
                return 0;
            }

            else
            {
                leftEllipse.Fill = Brushes.White;
                rightEllipse.Fill = Brushes.White;
                return 0;
            }
        }



    }
}
