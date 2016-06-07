using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.DirectInput;
using System.IO.Ports;
using System.Windows;

namespace P_Tracker2
{
    //Microcontroller
    static public class TheTool_micro
    {
        static Boolean connected = false;//POS monitor is connected

        static public SerialPort serialPort = new SerialPort();
        static public string selectPort = "";
        static public string bRate = "9600";


        static public void reset(Boolean showError)
        {
            loadComPort(showError);
            connect(showError);
        }

        static public string[] list_port = new string[] { };
        static public void loadComPort(Boolean showError)
        {
            try
            {
                list_port = SerialPort.GetPortNames();
                selectPort = list_port.Last();
            }
            catch (Exception e) { if (showError == true) { TheSys.showError("micro:" + e.Message, true); connected = false; } }
        }


        static public void connect(Boolean showError)
        {
            try
            {
                serialPort.PortName = selectPort;
                serialPort.BaudRate = int.Parse(bRate);
                connected = true;
            }
            catch (Exception e) { if (showError == true) { TheSys.showError("connect:" + e.Message, true); connected = false; } }
        }

        static public void sendCmd(string cmd)
        {
            try
            {
                if (connected == true)
                {
                    serialPort.Open();
                    serialPort.Write(cmd);
                    serialPort.Close();
                }
            }
            catch { reset(false); }
        }
    }
}
