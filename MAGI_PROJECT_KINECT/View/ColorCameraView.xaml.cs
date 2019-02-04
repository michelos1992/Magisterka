using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MAGI_PROJECT_KINECT
{
    public partial class ColorCameraView : Window
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;

        public ColorCameraView(KinectSensor kinect)
        {
            _sensor = kinect;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();
            if (_sensor != null)
            {
                _sensor.Open();
                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bitmapSource = frame.ToBitmap1();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }
        }


    }
}
