   M S B A M L   `   `   `  ����^  ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers  5   	    SkeletalTracking!   SkeletalTracking.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   mmy^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers     5   	   -   5          � Title$  
MainWindow��5   	   $	��600��6   $	��800��6)    � WindowState$ 	Maximized=�5   "   .��5      �� -   5        $��
MainCanvas��5      .+�5   
   ;   5Microsoft.Samples.Kinect.WpfViewers.KinectColorViewer  -   5        $��kinectColorViewer1��5   W    �� Left$ 0��6   
 �� Top$ 0��6/   $	��640��6>   $	��480��6J   5   /Microsoft.Samples.Kinect.WpfViewers.ImageViewer   Kinect 5	      ��  �� ElementName$ kinectSensorChooser1�� �� Path$ Kinect�A� 5
   
   -   5        $��leftEllipse��5
   =   $ 0��6   $ 0��6"   $��30��61   $��30��6Q   ���	#FF4D298D J� Opacity$
 0.15Q�6m   
�������6|   A� 5   
   -   5        $��rightEllipse��5   P   $	 100��6   $ 0��6$   ���	#FF2CACE3$��30��6D   $��30��6d   $
 0.15Q�6o   
������� � DataContext 6�   �� =   7Microsoft.Samples.Kinect.WpfViewers.KinectSensorChooser  5   
   -   5        $��kinectSensorChooser1��5   D   $	 250��6!   $	 380��63   $	��328��6`   �� 5   
   -   5        $��label1��5   M   $	 561��6   $ 0��6"   $��Score��61   $��38��6A   $��71��6[   $��22(�6f   $
��Bold"�6t   �� 5   
   -   5        $��label2��5   J   $	 561��6   $ 38��6"   $��0��62   $��36��6>   $��71��6X   $��20(�6c   $��Normal"�6q   �� 5   
   -	   5        $��warningLabel��5   m   $ 69��6   $	 180��6!   $*��$Please center yourself on the screen��62   $��63��6a   $��25(�6�   $	��494��6�   $ 1Q�6�   $��	ExtraBold"�6�   $��Normal&�6�   $y�Hidden=�6�   A� 5   
   -
   5        $��chestEllipse��5   >   $	 170��6   $ 0��6$   $��1��63   
���   �$��1��6a   $ 0Q�6k   �� 5   
   -   5        $��headWarning��5   C   $	 239��6   $	 157��6&   $��51��67   
���   �$��55��6e   
���  ��$	 0.5Q�6{   $y�Hidden=�6�   �� 5   
   -   5        $��bodyWarning��5   C   $	 222��6   $	 254��6&   $��97��67   
���   �$��96��6e   
��� ���$	 0.5Q�6~   $y�Hidden=�6�   �� 5   
   -   5        $��ellipse1��5   >   $	 187��6   $ 24��6"   $��60��62   $
��Fill=�6N   $��68��6]   $,��&/SkeletalTracking;component/target.png��6h   $y�Hidden=�6�   5      5      