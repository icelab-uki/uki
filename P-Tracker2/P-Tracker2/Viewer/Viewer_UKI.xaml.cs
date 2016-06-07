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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;


namespace P_Tracker2
{

    public partial class Viewer_UKI : Window
    {
        public Viewer_UKI()
        {
            InitializeComponent();
        }

        public string fileName = "unnamed";
        DataTable dataTable = null;

        //Browser File
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".csv", TheURL.url_saveFolder);
            if (openDialog == true)
            {
                string filePath = TheTool.dialog.FileName;
                readData(filePath);
                fileName = TheTool.getFileName_byPath(filePath);
                Title = fileName;
            }
        }

        int row_count = 0;
        void readData(string fileName)
        {
            try
            {
                dataTable = CSVReader.ReadCSVFile(fileName, true);
                if (checkHide.IsChecked.Value == false)
                {
                    dataGrid.ItemsSource = dataTable.DefaultView;
                    dataGrid.AutoGenerateColumns = true;
                    dataGrid.IsReadOnly = true;
                }
                row_count = dataTable.Rows.Count;
                txtRow.Content = "Row: " + row_count;
                if (row_count > 0) { butSplit.IsEnabled = true; }
                else { butSplit.IsEnabled = false; }
            }
            catch { }
        }


        private void checkHide_Checked(object sender, RoutedEventArgs e)
        {
            if (checkHide.IsChecked.Value) {
                dataGrid.ItemsSource = null;
            }
            else
            {
                dataGrid.ItemsSource = dataTable.DefaultView;
                dataGrid.AutoGenerateColumns = true;
                dataGrid.IsReadOnly = true;
            }
        }

        private void butOpenFolderClick(object sender, RoutedEventArgs e)
        {
            TheTool.openFolder(TheURL.url_saveFolder);
        }

        // Execute shutdown tasks
        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.Show();
        }

        private void butFileSample_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(TheURL.url_tv_sample_raw);
        }

        //=======================================================


        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Process: Split data with specified range");
            TheSys.showError("Input: dynamic (id column is the MUST)");
            TheSys.showError("Output: Splitted data (files)");
        }

        private void butSplit_Click(object sender, RoutedEventArgs e)
        {
            prepareSplitter();
            export_Split();
        }

        List<int[]> splitter = new List<int[]>();
        void prepareSplitter()
        {
            splitter.Clear();
            string txt0 = txtUKI_keyID.Text;
            string[] txt1 = TheTool.splitText(txt0,",");
            for (int i = 0; i < txt1.Count(); i++)
            {
                string[] txt2 = TheTool.splitText(txt1[i], "-");
                if (txt2.Count() > 0)
                {
                    int start = TheTool.getInt(txt2[0]); 
                    int end = row_count - 1;
                    if (txt2.Count() >= 2) { end = TheTool.getInt(txt2[1]); }
                    splitter.Add(new int[] { start, end });
                }
            }
        }

        void export_Split()
        {
            try
            {
                int aa_start = TheTool.getInt(txtAAstart);
                int aa_range = TheTool.getInt(txtAARange);
                if (aa_range == 0) { checkAutoArrange.IsChecked = false; }
                int aa_i = 0;
                int aa_i_max = TheTool.getPartition(splitter.Count,aa_range);
                //--------
                foreach (int[] split in splitter)
                {
                    DataTable dt_split = TheTool.dataTable_selectRow_byIndex(dataTable, split[0], split[1]);
                    string save_path = TheURL.url_saveFolder;
                    if (checkAutoArrange.IsChecked.Value) {
                        save_path += @"\" + (aa_start + aa_i);
                        TheTool.Folder_CreateIfMissing(save_path);
                    }
                    save_path += @"\" + fileName + " (" + split[0] + "-" + split[1] + ").csv";
                    TheTool.export_dataTable_to_CSV(save_path, dt_split);
                    aa_i++;
                    if (aa_i >= aa_i_max) { aa_i = 0; }
                }
                System.Windows.MessageBox.Show(@"Save to '" + TheURL.url_saveFolder + "'", "Export CSV");
                txtAAstart.Text = (aa_start + aa_i_max).ToString();
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        private void txtUKI_keyID_TextChanged(object sender, TextChangedEventArgs e)
        {
            TheTool.removeSpace(txtUKI_keyID);
        }

    }
}
