using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.Ports;
using System.Threading;
using MAGI_PROJECT_KINECT;

namespace MAGI_PROJECT_KINECT
{
    public partial class MainWindow : Window
    {
        public List<Calibration> Calibrattion{ get; set; }

        DateTime start, stop;

        public int[,] Glebokosci1;
        public int[,] Glebokosci2;
        public int[,] Cheeckk;
        public int[,] Callibr;

        RobotCommand Commands;
        SerialPort mySerialPort;

        Thread thrGogo;
        private int i = 0;
        int decision;
        private ushort maxDepth = 3000;
        private KinectSensor _kinect = null;
        private readonly int _bytePerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private DepthFrameReader _depthReader = null;
        private ushort[] _depthData = null;
        private byte[] _depthPixels = null;
        private WriteableBitmap _depthBitmap = null;
        private Point _startPoint = new System.Windows.Point();
        private Point _endPoint = new System.Windows.Point();
        DepthSpacePoint[] _depthSpace;

        public MainWindow()
        {
            InitializeComponent();
            InitializeKinect();
            Closing += OnClosing;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_kinect != null) _kinect.Close();
        }

        private void InitializeKinect()
        {
            _kinect = KinectSensor.GetDefault();
            if (_kinect == null) return;
            _kinect.Open();
            InitializeDepth();
        }
        
        private void InitializeDepth()
        {
            if (_kinect == null) return;
            FrameDescription desc = _kinect.DepthFrameSource.FrameDescription;
            _depthReader = _kinect.DepthFrameSource.OpenReader();
            _depthData = new ushort[desc.Width * desc.Height];
            _depthPixels = new byte[desc.Width * desc.Height * _bytePerPixel];
            _depthBitmap = new WriteableBitmap(desc.Width, desc.Height, 96, 96, PixelFormats.Bgr32, null);
            _depthSpace = new DepthSpacePoint[_kinect.DepthFrameSource.FrameDescription.LengthInPixels];

            MaskedColor.Source = _depthBitmap;
            _depthReader.FrameArrived += OnDepthFrameArrived;
        }

        private void OnDepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            int af = 0, depth22 = 0;
            double distance = 0;
            DepthFrameReference refer = e.FrameReference;
            if (refer == null) return;
            DepthFrame frame = refer.AcquireFrame();
            if (frame == null) return;

            using (frame)
            {
                FrameDescription frameDesc = frame.FrameDescription;
                if(((frameDesc.Width*frameDesc.Height)== _depthData.Length)&&(frameDesc.Width==_depthBitmap.PixelWidth) && (frameDesc.Height == _depthBitmap.PixelHeight))
                {
                    uint size = frame.FrameDescription.LengthInPixels;

                    frame.CopyFrameDataToArray(_depthData);

                    ushort minDepth = frame.DepthMinReliableDistance;
                    
                    int colorPixelIndex = 0;
                    for (int i = 0; i < _depthData.Length; i++)
                    {
                        ushort depth = _depthData[i];
                        if(depth < minDepth)
                        {
                            _depthPixels[colorPixelIndex++] = 0;
                            _depthPixels[colorPixelIndex++] = 0;
                            _depthPixels[colorPixelIndex++] = 0;
                        }
                        else if (depth > maxDepth)
                        {
                            _depthPixels[colorPixelIndex++] = 255;
                            _depthPixels[colorPixelIndex++] = 255;
                            _depthPixels[colorPixelIndex++] = 255;
                        }
                        else
                        {
                            double gray = (Math.Floor((double)depth / 250) * 12.75);
                            _depthPixels[colorPixelIndex++] = (byte)gray;
                            _depthPixels[colorPixelIndex++] = (byte)gray;
                            _depthPixels[colorPixelIndex++] = (byte)gray;
                        }
                        ++colorPixelIndex;
                    }
                    _depthBitmap.WritePixels(new Int32Rect(0, 0, frameDesc.Width, frameDesc.Height), _depthPixels, frameDesc.Width * _bytePerPixel, 0);

                    distance = 260 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                    af = (int)distance;
                    depth22 = _depthData[af];
                    distanceTextBox.Text = depth22.ToString();
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(MaskedColor);
        }
        
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _endPoint = e.GetPosition(MaskedColor);
            this.textBox1.Text = string.Format("Coordinates: ({0},{1})\n", (int)_startPoint.X, (int)_startPoint.Y);
            this.textBox2.Text = string.Format("Coordinates: ({0},{1})\n", (int)_endPoint.X, (int)_endPoint.Y);
            double distance = _startPoint.X + (_startPoint.Y * _kinect.DepthFrameSource.FrameDescription.Width);
            int af = (int)distance;
            int depth = _depthData[af];
            this.textBox3.Text = "Depth:" + depth + "startpoint: " + this.textBox1.Text + "endpoint: " + this.textBox2.Text;
        }

        private void SpaceScanEmpty_Click(object sender, RoutedEventArgs e)
        {
            int af = 0, depth = 0;
            double distance = 0;
            Glebokosci1 = new int[424, 512];

            for (int i = 0; i < 424; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    distance = j + (i * _kinect.DepthFrameSource.FrameDescription.Width);
                    af = (int)distance;
                    depth = _depthData[af];
                    Glebokosci1[i, j] = depth;
                }
            }
        }

        private void SpaceScanFully_Click(object sender, RoutedEventArgs e)
        {
            int af = 0, depth = 0;
            double distance = 0;
            Glebokosci2 = new int[424, 512];

            for (int i = 0; i < 424; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    distance = j + (i * _kinect.DepthFrameSource.FrameDescription.Width);
                    af = (int)distance;
                    depth = _depthData[af];
                    Glebokosci2[i, j] = depth;
                }
            }
        }
      
        private void Calibration_Click(object sender, RoutedEventArgs e)
        {
            Callibr = new int[Glebokosci1.GetLength(0), Glebokosci1.GetLength(1)];
            for (int i = 0; i < Glebokosci1.GetLength(0); i++)
            {
                for (int j = 0; j < Glebokosci1.GetLength(1); j++)
                {
                    Callibr[i, j] = 0;
                }
            }
        }

        private void CheckedButton_Click(object sender, RoutedEventArgs e)
        {
            int roznica = 0;
            Cheeckk = new int[Glebokosci1.GetLength(0), Glebokosci1.GetLength(1)];

            for (int i = 0; i < Glebokosci1.GetLength(0); i++)
            {
                for (int j = 0; j < Glebokosci1.GetLength(1); j++)
                {
                    if (Glebokosci1[i,j] > Glebokosci2[i,j] && Glebokosci1[i, j] > 0 && Glebokosci2[i, j] > 0)
                    {
                        roznica = Glebokosci1[i, j] - Glebokosci2[i, j];
                    }
                    if (Glebokosci1[i, j] < Glebokosci2[i, j] && Glebokosci1[i, j] > 0 && Glebokosci2[i, j] > 0)
                    {
                        roznica = Glebokosci2[i, j] - Glebokosci1[i, j];
                    }
                    if (Glebokosci1[i, j] > 0 && Glebokosci2[i, j] > 0 && Glebokosci1[i, j] == Glebokosci2[i, j] || Glebokosci1[i, j] > 0 && Glebokosci2[i, j] > 0 && roznica <= 600)
                    {
                        Cheeckk[i, j] = 0;
                    }
                    if (roznica > 500)
                    {
                        Cheeckk[i, j] = 1;
                    }
                }
            }
        }
        
        private void ConnectToRobot_Click(object sender, RoutedEventArgs e)
        {
            Robots wnd = new Robots();
            wnd.Show();
        }
        
        private void ConnectRobot_Click(object sender, RoutedEventArgs e)
        {
            Commands = new RobotCommand(mySerialPort);
            Commands.Connect();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Commands.Disconnect();
        }
        
        private void Gogo_Click(object sender, RoutedEventArgs e)
        {
            thrGogo = new Thread(new ThreadStart(Gogogo));
            thrGogo.Start();
        }
                
        private void Gogogo()
        {

            int distance = 3500;
            Commands.SendCommand("D+" + distance);

            decision = Commands.WhichSide(Commands.Checkk(Cheeckk));
            int checkPath=0;
            if (decision == 0 || decision == 1)
            {
                GoToObstacle(decision);
                start = DateTime.Now;
                Thread.Sleep(2000);
                PassingObstacle(decision);
                stop = DateTime.Now;
                TimeSpan roznica = stop - start;
                int roznn = Convert.ToInt32(roznica.TotalMilliseconds);
                CheckEdges(decision);
                Thread.Sleep(1000);
                checkPath = ComeBackToPath(decision);
                if(checkPath == 1)
                {
                    ReturnToPath(decision, roznn);
                    GoStraight();
                    Commands.SendCommand("H");
                }else
                {
                    Commands.SendCommand("H");
                    Commands.SendCommand("!");
                    MessageBox.Show("Koniec jazdy");
                    return;
                }
                MessageBox.Show("Koniec jazdy");
            }
            else
            {
                GoStraight();
                Commands.SendCommand("H");
                MessageBox.Show("Koniec jazdy");
            }
        }

        private void GoToObstacle(int _decision)
        {
            double distance = 0, distance2 = 0, distance3 = 0;
            int depth22 = 0, depth222 = 0, depth223 = 0, af = 0, af2 = 0, af3 = 0;
            bool check1 = true;

            while (check1 != false)
            {
                distance = 255 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                distance2 = 245 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                distance3 = 265 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                af = (int)distance;
                af2 = (int)distance2;
                af3 = (int)distance3;
                depth22 = _depthData[af];
                depth222 = _depthData[af2];
                depth223 = _depthData[af3];
                if (depth22 < 720 && depth222 < 720 && depth223 < 720)
                {
                    if (_decision == 0)
                    {
                        Commands.SendCommand("S60");
                        check1 = false;
                    }
                    else
                    {
                        Commands.SendCommand("S120");
                        check1 = false;
                    }
                }
            }
        }

        private void PassingObstacle(int _decision)
        {
            double distance = 0;
            int depth1 = 0, af = 0;
            bool check2 = true;

            if (_decision == 0) {
                while (check2 != false)
                {
                    distance = 5 + (190 * _kinect.DepthFrameSource.FrameDescription.Width);
                    af = (int)distance;
                    depth1 = _depthData[af];

                    if (depth1 > 720)
                    {
                        check2 = false;
                    }
                }
                Thread.Sleep(1000);
            } else
            {
                while (check2 != false)
                {
                    distance = 500 + (190 * _kinect.DepthFrameSource.FrameDescription.Width);
                    af = (int)distance;
                    depth1 = _depthData[af];

                    if (depth1 > 720)
                    {
                        check2 = false;
                    }
                }
                Thread.Sleep(1500);
            }
        }

        public void CheckEdges(int _decision)
        {
            string answer = "";
            if (_decision == 0)
            {
                Commands.SendCommand("S85");
                Thread.Sleep(1000);
                answer = Commands.Answer();
                Commands.SendCommand("S120");
                Thread.Sleep(100);
                answer = Commands.Answer();
                Thread.Sleep(2900);
                Commands.SendCommand("S85");
                Thread.Sleep(100);
                answer = Commands.Answer();
                Commands.SendCommand("H");
                Thread.Sleep(2000);
            }else
            {
                Commands.SendCommand("S85");
                Thread.Sleep(1400);
                answer = Commands.Answer();
                Commands.SendCommand("S60");
                Thread.Sleep(100);
                answer = Commands.Answer();
                Thread.Sleep(3000);
                Commands.SendCommand("S85");
                Thread.Sleep(100);
                answer = Commands.Answer();
                Commands.SendCommand("H");
                Thread.Sleep(2000);
            }
        }

        private int ComeBackToPath(int _decision)
        {
            double distance = 0;
            int af1 = 0, af2 = 0, af3 = 0, depth1 = 0;
            int[,] depths = new int[200, 230];
            Calibrattion = new List<Calibration>();

            if (_decision == 0)
            {
                for (int i = 100; i < 330; i++)
                {
                    for (int j = 0; j < 200; j++)
                    {
                        distance = j + (i * _kinect.DepthFrameSource.FrameDescription.Width);
                        af1 = (int)distance;
                        depth1 = _depthData[af1];
                        Calibrattion.Add(new Calibration()
                        {
                            Checking = depth1,
                            Width = j,
                            Height = i
                        });
                    }
                }
            }else
            {
                for (int i = 100; i < 330; i++)
                {
                    for (int j = 310; j < 510; j++)
                    {
                        distance = j + (i * _kinect.DepthFrameSource.FrameDescription.Width);
                        af1 = (int)distance;
                        depth1 = _depthData[af1];
                        Calibrattion.Add(new Calibration()
                        {
                            Checking = depth1,
                            Width = j,
                            Height = i
                        });
                    }
                }
            }
            foreach (var item in Calibrattion)
            {
                if (item.Checking > 550)
                {
                    af2++;
                }
                else
                {
                    af3++;
                }
            }
            if (af2 > 30000)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        
        private void ReturnToPath(int _decision, int roznn)
        {
            string answer = "";

            if (_decision == 0)
            {
                Commands.SendCommand("D+620");
                Thread.Sleep(4000);
                answer = Commands.Answer();
                Commands.SendCommand("D+3000");
                Thread.Sleep(2000);
                Commands.SendCommand("S120");
                Thread.Sleep(roznn);
                Commands.SendCommand("S85");
                Thread.Sleep(500);
                Commands.SendCommand("S60");
                Thread.Sleep(2900);
                Commands.SendCommand("S85");
            }else
            {
                Commands.SendCommand("D+620");
                Thread.Sleep(4000);
                answer = Commands.Answer();
                Commands.SendCommand("D+3000");
                Thread.Sleep(2000);
                Commands.SendCommand("S60");
                Thread.Sleep(roznn);
                Commands.SendCommand("S85");
                Thread.Sleep(1000);
                Commands.SendCommand("S120");
                Thread.Sleep(2900);
                Commands.SendCommand("S85");
            }
        }

        private void GoStraight()
        {
            double distance = 0, distance2 = 0, distance3 = 0;
            int depth22 = 0, depth222 = 0, depth223 = 0, af = 0, af2 = 0, af3 = 0;
            bool check = true;

            while (check != false)
            {
                distance = 255 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                distance2 = 245 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                distance3 = 265 + (200 * _kinect.DepthFrameSource.FrameDescription.Width);
                af = (int)distance;
                af2 = (int)distance2;
                af3 = (int)distance3;
                depth22 = _depthData[af];
                depth222 = _depthData[af2];
                depth223 = _depthData[af3];
                if (depth22 < 550 && depth222 < 550 && depth223 < 500)
                {
                    check = false;
                }
            }
        }

        private void ColorCamera_Click(object sender, RoutedEventArgs e)
        {
            ColorCameraView colorWin = new ColorCameraView(_kinect);
            colorWin.Show();
        }
        
        private void STOP_Click(object sender, RoutedEventArgs e)
        {
            Commands.SendCommand("H");
            MessageBox.Show("STOP");
        }
    }
}