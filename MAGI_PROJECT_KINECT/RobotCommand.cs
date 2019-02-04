using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAGI_PROJECT_KINECT
{
    class RobotCommand
    {
        SerialPort mySerialPort { get; set; }
        bool IsOn = false;
        int angleTurn { get; set; }
        public string Command { get; set; }
        public int Battery { get; set; }
        public List<Check> newCheck { get; set; }

        public RobotCommand(SerialPort _mySerialPort, bool _isOn, int _angleTurn, string _Command, int _Battery)
        {
            mySerialPort = _mySerialPort;
            IsOn = _isOn;
            angleTurn = _angleTurn;
            Command = _Command;
            Battery = _Battery;
        }
        public RobotCommand(SerialPort _mySerialPort)
        {
            mySerialPort = _mySerialPort;
        }
         
        public string Connect()
        {
            string Status;
            mySerialPort = new SerialPort("COM4", 115200);
            
            if (mySerialPort.IsOpen != true)
            {
                mySerialPort.Open();
            }
            IsOn = mySerialPort.IsOpen;

            if (IsOn == true)
            {
                Status = "Connect";
            }
            else
            {
                Status = "Disconnect";
            }

            return Status;
        }

        public string Disconnect()
        {
            string Status;

            mySerialPort.Close();
            IsOn = mySerialPort.IsOpen;

            if (IsOn != false)
            {
                Status = "Connect";
            }
            else
            {
                Status = "Disconnect";
            }

            return Status;
        }

        public void SendCommand(object _command)
        {
            mySerialPort.WriteLine(_command.ToString());
        }
        public string CheckingAnswer(object _command)
        {
            mySerialPort.WriteLine(_command.ToString());
            return mySerialPort.ReadLine();
        }

        public string Answer()
        {
            return mySerialPort.ReadLine();
        }

        public List<Check> Checkk(int [,] Cheeckk)
        {
            int temp1 = 0, temp2 = 0, temp3 = 0;
            newCheck = new List<Check>();

            #region SPRAWDZANIE DROGI KTORA KROTSZA
            for (int i = 0; i < Cheeckk.GetLength(0); i++)
            {
                temp1 = 0;
                for (int j = 1; j < Cheeckk.GetLength(1); j++)
                {
                    if (temp1 <= 150)
                    {
                        if (Cheeckk[i, j] == 1)
                        {
                            if (Cheeckk[i, j - 1] == 1)
                            {
                                temp1++;
                            }
                            else
                            {
                                temp1 = 0;
                            }
                        }
                        else
                        {
                            temp1 = 0;
                        }
                    }
                    else
                    {
                        if (j >= 150)
                        {
                            temp2 = j - 150;
                        }
                        else
                        {
                            temp2 = temp2 - temp2;
                        }
                        temp3 = i;

                        for (int k = 0; k < 1; k++)
                        {
                            for (int l = temp2; l < j; l++)
                            {
                                newCheck.Add(new Check()
                                {
                                    Checking = 1,
                                    Width = temp2,
                                    Height = temp3
                                });
                                temp2++;
                            }
                        }
                        temp1 = 0;
                    }
                }
            }
            #endregion

            return newCheck;
        }

        public int WhichSide(List<Check> newCCeck)
        {
            int cosik = 0, cosik2 = 0, wynik = 0;

            foreach (var item in newCCeck)
            {
                if (item.Height > 100 && item.Height < 200)
                {
                    if (item.Width < 240)
                    {
                        cosik++;
                    }
                    else if (item.Width > 240)
                    {
                        cosik2++;
                    }
                }
            }

            if (cosik > cosik2)
            {
                wynik = 0;
                return wynik;
            }else if(cosik < cosik2 )
            {
                wynik = 1;
                return wynik;
            }else
            {
                wynik = -1;
                return wynik;
            }
        }
    }
}
