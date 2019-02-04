using System.IO.Ports;
using System.Windows;

namespace MAGI_PROJECT_KINECT
{
    public partial class Robots : Window
    {
        RobotCommand temp;
        bool isOn = false;
        public int AngleTurn { get; set; }
        int batteryLive = 0;
        string command = "";
        SerialPort mySerialPort { get; set; }

        public Robots()
        {
            InitializeComponent();
            temp = new RobotCommand(mySerialPort, isOn, AngleTurn, command, batteryLive);
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(temp.Connect());
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(temp.Disconnect());
        }

        private void BatteryLive_Click(object sender, RoutedEventArgs e)
        {
            temp.CheckingAnswer("B");
        }

        private void BackToMain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            wnd.Show();
        }
    }
}
