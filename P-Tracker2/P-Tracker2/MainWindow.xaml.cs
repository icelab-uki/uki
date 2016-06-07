using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Collections.Generic;
//
using System.Data;

namespace P_Tracker2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //InitializeComponent();
            initialize();
            TheStore.mainWindow = this;
            TheTool_micro.reset(false);
            //test();
            //TheInstanceContainer.loadInstanceList();
        }

        //void test()
        //{
        //    DateTime end;
        //    DateTime start;
        //    int looptime = 100000;
        //    double dividend = 10.1233466753;
        //    double divisor = 3.123452313136;
        //    TheSys.showError("Testing " + dividend + "/" + divisor + " on " + looptime + " loop");
        //    string a = "";
        //    //-----------------------------------
        //    start = DateTime.Now;
        //    for (int i = 0; i < looptime; i++)
        //    {
        //        a = Math.Round( (dividend / divisor)) + "";
        //    }
        //    end = DateTime.Now;
        //    TheSys.showError("Basic = " + a);
        //    TheSys.showError((end - start).TotalMilliseconds);
        //    //-----------------------------------
        //    start = DateTime.Now;
        //    for (int i = 0; i < looptime; i++)
        //    {
        //        a = double.Parse((dividend / divisor).ToString("0")) + "";
        //    }
        //    end = DateTime.Now;
        //    TheSys.showError("Format = " + a);
        //    TheSys.showError((end - start).TotalMilliseconds);
        //    //-----------------------------------
        //    start = DateTime.Now;
        //    for (int i = 0; i < looptime; i++)
        //    {
        //        //a = (int) dividend / (int)divisor + "";
        //        a = manualDivision(dividend, divisor, 2) + "";
        //    }
        //    end = DateTime.Now;
        //    TheSys.showError("Manual Division = " + a);
        //    TheSys.showError((end - start).TotalMilliseconds);
        //}

        int manualDivision(double dividend, double divisor, int digit)
        {
            int i = 0;
            //------------
            //int d = (int)dividend; int s = (int)divisor;
            //while (d > s) { i++; d -= s; }
            //------------
            while (dividend > divisor) { i++; dividend -= divisor; }
            return i;
        }

        void initialize()
        {
            try
            {
                TheURL.url_0_root = AppDomain.CurrentDomain.BaseDirectory;
                TheURL.url_config = TheURL.url_0_root + TheURL.url_config;
                MySetting.readSetting();
                TheURL.initializeURL();
                TheURL.initializeURL_permanent();
                TheProlongSitDetector.initializePath();
            }
            catch { TheSys.showError("Initializ. Error"); }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void buttonUKI1_Click(object sender, RoutedEventArgs e)
        {
            UserTracker utrack1 = new UserTracker();
            utrack1.Show();
            this.Hide();
            utrack1.startUKI();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            UserTracker utrack1 = new UserTracker();
            utrack1.Show();
            this.Hide();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            new UKIAnalyser().Show();
            //this.Hide();
        }

        public OtherMenu form_othMenu = null;
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (form_othMenu == null)
            {
                form_othMenu = new OtherMenu();
                new OtherMenu().Show();
                this.Hide();
            }
        }

        private void butEntropy_Click(object sender, RoutedEventArgs e)
        {
            new EntropyAnalyser().Show();
            //this.Hide();
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            new UKIFullEx().Show();
            //this.Hide();
        }


    }
}
