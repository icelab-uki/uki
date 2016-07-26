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

            TheBVH.createRotationMatrix(10,20,30,"zyx");
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


        private void button6_Click(object sender, RoutedEventArgs e)
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

        private void butSkel_Click(object sender, RoutedEventArgs e)
        {
            new SkelViewer().Show();
        }


    }
}
