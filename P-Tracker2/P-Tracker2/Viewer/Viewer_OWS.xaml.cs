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

    public partial class Viewer_OWS : Window
    {
        public Viewer_OWS()
        {
            InitializeComponent();
        }


        public string fileName_noExtension = "unnamed";
        DataTable dataTable = null;//data table that become datagrid
        //-------------------------------------

        //Browser File
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".csv", TheURL.url_saveFolder);
            // Get the selected file name and display in a TextBox
            if (openDialog == true)
            {
                // Open document
                string filePath = TheTool.dialog.FileName;
                FileNameTextBox.Text = filePath;
                readData(filePath);
                fileName_noExtension = TheTool.getFileName_byPath(filePath);
            }
        }


        void readData(string fileName)
        {
            try
            {
                dataTable = CSVReader.ReadCSVFile(fileName, true);
                dataGrid.ItemsSource = dataTable.DefaultView;
                dataGrid.AutoGenerateColumns = true;
                dataGrid.IsReadOnly = true;
                txtRow.Content = "Row: " + dataTable.Rows.Count;
                calSplit();
            }
            catch { }
        }

        //----------------------------------
        int beginCentiSec = 0;
        int endCentiSec = 0;

        void calSplit()
        {
            try{
                int splitUnit = TheTool.getInt(txtSplit) * 100;
                if (dataTable != null 
                    && dataTable.Rows.Count > 0 
                    && splitUnit > 0
                    && txtSplit.Text != "")
                {
                    calBeginEndSec();
                    int i = 0;
                    int totalCentiSec = endCentiSec - beginCentiSec;
                    while(totalCentiSec > splitUnit){
                        i++;
                        totalCentiSec -= splitUnit;
                    }
                    txtSplitCount.Content = "Split files: " + i;
                    double remain = totalCentiSec;
                    remain /= 100;
                    txtSplitRemain.Content = "remain (sec): " + remain;
                }
            }
            catch (Exception e) { System.Windows.MessageBox.Show(e.Message); }
        }

       void calBeginEndSec(){
           string timeBegin = dataTable.Rows[0][0].ToString();
           string timeEnd = dataTable.Rows[dataTable.Rows.Count - 1][0].ToString();
           beginCentiSec = TheTool.calCentisecond(timeBegin);
           endCentiSec = TheTool.calCentisecond(timeEnd);
       }

        //------------------------------------------------------

        private void butOpen_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileNameTextBox.Text;
            readData(filePath);
            fileName_noExtension = TheTool.getFileName_byPath(filePath);
        }

        private void txtSpli_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

        private void butExport_Click(object sender, RoutedEventArgs e)
        {
            export_Split();
        }

        void export_Split()
        {
            string tag = txtTag.Text;
            int split = TheTool.getInt(txtSplit);
            TheTool.v_exportCSV_Split(fileName_noExtension, tag, dataTable, split);
        }

        private void butOpenFolderClick(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder); }
            catch { }
        }


        // Execute shutdown tasks
        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.Show();
        }

        private void txtSplit_TextChanged(object sender, TextChangedEventArgs e)
        {
            calSplit();
        }


        //====================================================================
        //======= FTG Data ===================================================
        List<String> ftg_cap_data = new List<String> { };

        private void butFTG_cap_Click(object sender, RoutedEventArgs e)
        {
            ftg_capture();
        }

        void ftg_capture()
        {
            try
            {
                int range = TheTool.getInt(txtFTG_range) * 1000;
                //
                if (dataTable != null
                    && dataTable.Rows.Count > 0
                    && range > 0)
                {
                    int tempi; int i;
                    int r_start = -1; int t_start; 
                    //--- Get Start ------------
                    if (comboFTG_start.SelectedIndex == 1)
                    {
                        //-- By Time
                        t_start = TheTool.getInt(txtFTG_start);
                        i = 0;
                        foreach (DataRow r in dataTable.Rows)
                        {
                            tempi = int.Parse(r[0].ToString());
                            if (tempi >= t_start) { r_start = i; break; }
                            i++;
                        }
                    }
                    else
                    {
                        //-- By Row
                        r_start = TheTool.getInt(txtFTG_start);
                        t_start = TheTool.getInt(dataTable.Rows[r_start][0].ToString());
                    }
                    //--------------------------
                    //--- Get End  ------------
                    DateTime dt = DateTime.ParseExact(t_start.ToString(), "ddHHmmssff", CultureInfo.InvariantCulture);
                    dt = dt.AddMilliseconds(range);
                    int t_end = int.Parse(dt.ToString("ddHHmmssff"));
                    int r_end = 0;
                    //find end
                    i = 0;
                    foreach (DataRow r in dataTable.Rows)
                    {
                        tempi = int.Parse(r[0].ToString());
                        if (tempi <= t_end) { r_end = i; }
                        i++;
                    }
                    //get real time ending
                    t_end = TheTool.getInt(dataTable.Rows[r_end][0].ToString());
                    //-------------------------------------------------------------
                    if (r_start >= 0 && r_end >= 0)
                    {
                        int colcount = dataTable.Columns.Count;
                        string txtData = r_start + "," + r_end + "," + dataTable.Rows[r_start][0];
                        List<double> arr_db;
                        for (int ic = 1; ic < colcount; ic++)
                        {
                            arr_db = new List<double> { };
                            for (int ir = r_start; ir <= r_end; ir++)
                            {
                                arr_db.Add(Double.Parse(dataTable.Rows[ir][ic].ToString()));
                            }
                            txtData += "," + arr_db.Average();
                        }
                        ftg_cap_data.Add(txtData);
                    }
                    //-------------------------------------------------------------
                    txtFTG_info.Content = "Row:" + r_start + "-" + r_end + Environment.NewLine
                        + "Time:" + t_start + "-" + dataTable.Rows[r_end][0].ToString();
                }
                ftg_setCounterText();
            }
            catch (Exception ex) { TheSys.showError(ex.Message); }
        }

        void ftg_setCounterText()
        {
            txtFTG_count.Content = "data: " + ftg_cap_data.Count;
        }

        private void butFTG_clear_Click(object sender, RoutedEventArgs e)
        {
            ftg_cap_data.Clear();
            ftg_setCounterText();
        }

        private void butFTG_export_Click(object sender, RoutedEventArgs e)
        {
            string fname = "cap" + DateTime.Now.ToString("MMdd_HHmmss");
            fname = TheURL.url_saveFolder + fname + ".csv";
            if (ftg_cap_data.Count > 0)
            {
                if (checkPreview.IsChecked.Value) {                    
                    TheTool.exportCSV_orTXT(fname, ftg_cap_data, false);
                    Process.Start(fname);
                }
                else {
                    TheTool.exportCSV_orTXT(fname, ftg_cap_data, true); 
                }
            }
        }

        private void ftg_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ftg_capture();
                txtFTG_start.Text = "";
            }
        }

        private void butFileSample_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(TheURL.url_tv_sample_raw_ows);
        }

        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Process: Split data with the same time range and export");
            TheSys.showError("Input: RAW data tracked by normal User Tracker (time column is the MUST)");
            TheSys.showError("Output: Splitted data");
        }

        private void butHelp2_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Process: Start from specified point, use range data of ... second, compute average value on each column");
            TheSys.showError("Input: RAW data tracked by normal User Tracker (time column is the MUST)");
            TheSys.showError("Output: Data with 1 Row, which is average");
        }

    }
}
