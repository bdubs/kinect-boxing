   M S B A M L   `   `   `  ���� ^  ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers   SkeletalTracking!   SkeletalTracking.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     mmy^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers     -     � Title$  
MainWindow��$	��600��$	��800�� � WindowState$ 	Maximized=�.���� -   $��
MainCanvas��.+�;   5Microsoft.Samples.Kinect.WpfViewers.KinectColorViewer  -   $��kinectColorViewer1�� �� Left$ 0��
 �� Top$ 0��$	��640��$	��480��5   /Microsoft.Samples.Kinect.WpfViewers.ImageViewer   Kinect ��  �� ElementName$ kinectSensorChooser1�� �� Path$ Kinect�A� -   $��leftEllipse��$ 0��$ 0��$��30��$��30�����	#FF4D298D J� Opacity$
 0.15Q�
�������A� -   $��rightEllipse��$	 100��$ 0�����	#FF2CACE3$��30��$��30��$
 0.15Q�
������� � DataContext �� =   7Microsoft.Samples.Kinect.WpfViewers.KinectSensorChooser  -   $��kinectSensorChooser1��$	 250��$	 380��$	��328���� -   $��label1��$	 561��$ 0��$��Score��$��38��$��71��$��22(�$
��Bold"��� -   $��label2��$	 561��$ 38��$��Label��$��36��$��71��$��20(�$��Normal"��� -	   $��warningLabel��$ 69��$	 180��$*��$Please center yourself on the screen��$��63��$��25(�$	��494��$ 1Q�$��	ExtraBold"�$��Normal&�$y�Hidden=�A� -
   $��chestEllipse��$	 170��$ 0��$��1��
���   �$��1��$ 0Q��� -   $��headWarning��$	 239��$	 157��$��51��
���   �$��55��
���  ��$	 0.5Q�$y�Hidden=��� -   $��bodyWarning��$	 222��$	 254��$��97��
���   �$��96��
��� ���$	 0.5Q�$y�Hidden=��� -   $��ellipse1��$	 187��$ 24��$��60��$
��Fill=�$��68��$,��&/SkeletalTracking;component/target.png��