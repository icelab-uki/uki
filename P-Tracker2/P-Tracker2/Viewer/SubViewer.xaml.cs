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
using System.Data;

namespace P_Tracker2
{

    public partial class SubViewer : Window
    {
        public string path_saveRoot = "";

        public SubViewer()
        {
            InitializeComponent();
        }

        //------------------------------------------------------
        DataTable dataTable = null;

        public void setDataGrid(DataTable dt)
        {
            dataTable = dt;
            dataGrid.AutoGenerateColumns = false;
            dataGrid.ItemsSource = dt.DefaultView;
            dataGrid.AutoGenerateColumns = true;
            txtRow.Content = dt.Rows.Count;
        }

        public void setTitle(string txt)
        {
            this.Title = txt;
        }
        //------------------------------------------------------
        List<Weka_EachResult> listWeka = null;

        public void setDataGrid(List<Weka_EachResult> list)
        {
            dataGrid.ItemsSource = list;
            listWeka = list;
            txtRow.Content = list.Count;
        }

        //============================================================================
        

        public int mode = 0;
        public string fileName = "";

        private void butExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mode != 1) // 0 = P-Analysis , 1 = Weka mode
                {
                    string file_name0;
                    if (fileName != "") { file_name0 = fileName; }
                    else { file_name0 = DateTime.Now.ToString("MMdd HHmmss"); }
                    string path = TheURL.url_saveFolder + file_name0 + ".csv";
                    //-------------
                    TheTool.Folder_CreateIfMissing(TheURL.url_saveFolder);
                    TheTool.export_dataTable_to_CSV(path, dataTable);
                    System.Windows.MessageBox.Show(@"Save to '" + path);
                }
                confirmOnClose = false;
            }
            catch { }
        }

        public Boolean confirmOnClose = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (confirmOnClose == true)
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Data will be lost, you may save files first."
                                                , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
                if (result == System.Windows.MessageBoxResult.OK)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        
    }
}
