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
using System.Windows.Shapes;

namespace P_Tracker2
{

    public partial class Debugger : Window
    {
        public Debugger()
        {
            InitializeComponent();
        }

        public void showTxt(string txt, Boolean endLine)
        {
            try
            {
                txt1.Text += txt;
                if (endLine == true) { txt1.Text += Environment.NewLine; }
            }
            catch { }
        }

        public string[] getTxt()
        {
            try
            {
                string[] sep = new string[] { "\r\n" };
                string[] lines = txt1.Text.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                return lines;
            }
            catch { }
            return new string[1];
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            TheSys.createDebugger();
        }

        public void goLastLine()
        {
            txt1.SelectionStart = txt1.Text.Length;
            txt1.ScrollToEnd();
        }
    }
}
