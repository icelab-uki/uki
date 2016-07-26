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
    public partial class OtherMenu : Window
    {
        public OtherMenu()
        {
            InitializeComponent();
        }

        Boolean showMainOnClose = true;
        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.form_othMenu = null;
            if (showMainOnClose) { TheStore.mainWindow.Show(); }
        }

        private void butDataView_Click(object sender, RoutedEventArgs e)
        {
            Viewer_OWS v = new Viewer_OWS();
            v.Show();
            showMainOnClose = false;
            this.Close();
        }

        private void butDataView2_Click(object sender, RoutedEventArgs e)
        {
            Viewer_UKI v = new Viewer_UKI();
            v.Show();
            showMainOnClose = false;
            this.Close();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            TotalViewer v = new TotalViewer();
            v.Show();
            showMainOnClose = false;
            this.Close();
        }

        private void butPredictor_Click(object sender, RoutedEventArgs e)
        {
            new Weka().Show();
            showMainOnClose = false;
            this.Close();
        }

        private void butMapEditor_Click(object sender, RoutedEventArgs e)
        {
            new Editor("").Show();
        }

        private void butConverter_Click(object sender, RoutedEventArgs e)
        {
            new Converter().Show();
        }


    }
}
