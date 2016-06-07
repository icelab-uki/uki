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


namespace P_Tracker2
{

    public partial class UKIAnalyser : Window
    {
        public UKIAnalyser()
        {
            InitializeComponent();
            resetTable();
        }

        //===============================================================================================
        //=== Basic Template ===================================================================================

        string col_id = "id";
        string col_path = "file_path";

        DataTable dataTable = null;//data table that become datagrid

        void resetTable()
        {
            if (dataTable != null) { dataTable.Clear(); }
            dataTable = new DataTable();
            dataTable.Columns.Add(col_id);
            dataTable.Columns.Add(col_path);
            setDataGrid(dataTable);
        }

        void setDataGrid(DataTable dt)
        {
            dataGrid.AutoGenerateColumns = false;
            dataGrid.ItemsSource = dt.DefaultView;
            dataGrid.AutoGenerateColumns = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.Show();
        }

        //Browser File
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            TheTool.openFileDialog_01(true, ".csv", "");

            int id = 1;
            string[] result = TheTool.dialog.FileNames;
            if (result.Count() > 0) { resetTable(); }
            foreach (string y in result)
            {
                dataTable.Rows.Add(id, y);
                id++;
            }
            rowCount();
        }

        int row_count = 0;
        void rowCount()
        {
            try
            {
                row_count = dataTable.Rows.Count;
                txtRow.Content = "Row: " + row_count;
            }
            catch { }
        }

        private void butUKI_rawSample_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(TheURL.url_tv_sample_raw);
        }

        private void butOpenFolderClick(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder); }
            catch { }
        }

        //string default_path = TheURL.url_saveFolder + TheURL.url_2_ukiAnalysis;
        string default_path = "";

        private void butBrowseRaw_Click(object sender, RoutedEventArgs e)
        {
            TheTool.openFileDialog_01(true, ".csv", default_path);
            int id = 1;
            string[] result = TheTool.dialog.FileNames;
            if (result.Count() > 0) { resetTable(); }
            foreach (string y in result)
            {
                dataTable.Rows.Add(id, y);
                id++;
            }
            rowCount();
        }
        
        //===============================================================================================
        //===============================================================================================

        public List<m_Motion> list_motions = new List<m_Motion>();
        
        private void but_dbSample_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_uki_motionDB); }
            catch { }
        }

        string file_path_motionDB = "";
        private void but_dbBrowse_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".xml", default_path);
            if (openDialog == true)
            {
                try
                {
                    file_path_motionDB = TheTool.dialog.FileName;
                    list_motions = TheMapData.loadXML_motion(file_path_motionDB);
                    txtDB.Content = "Motion: " + list_motions.Count();
                    if (list_motions.Count() > 0) { butAnalyze.IsEnabled = true; }
                    else { butAnalyze.IsEnabled = false; }
                }
                catch (Exception ex) { TheSys.showError(ex); }
            }
        }

        //------------------

        private void butAnalyze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderPath = TheURL.url_saveFolder + "Analysis_" + DateTime.Now.ToString("ddHHmmssff");
                string note_path = folderPath + @"\note.txt";
                string matrix_path = folderPath + @"\Matrix.csv";
                TheTool.Folder_CreateIfMissing(folderPath);
                List<String> matrix_data = new List<String>();
                String matrix_Head = "";
                foreach (m_Motion motion in list_motions){ matrix_Head += "," + motion.name;}
                matrix_data.Add(matrix_Head);
                //-------------------
                String path_RawData;
                foreach (DataRow r in dataTable.Rows)
                {
                    try
                    {
                        path_RawData = r[col_path].ToString();
                        matrix_data.Add(motionAnalysis(path_RawData, folderPath, this.list_motions));
                    }
                    catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
                }
                //--------------------
                TheTool.exportCSV_orTXT(matrix_path, matrix_data, false);
                TheTool.exportFile(TheSys.getText_List(), note_path, false);
                System.Windows.MessageBox.Show(@"Save to '" + folderPath + "'", "Export Data");
            }
            catch (Exception ex) { TheSys.showError(ex);}
        }

        //Motion Analysis on 1 File
        //All Data (Whole Sequence) - All Motion
        //Output: Matrix
        static public string motionAnalysis(String path_load, String path_save, List<m_Motion> list_motions)
        {
            string currentFileName = TheTool.getFileName_byPath(path_load);
            string matrix_data = currentFileName;
            List<UKI_DataRaw> list_raw = TheUKI.csv_loadFileTo_DataRaw(path_load);
            List<logDetection> log_list = new List<logDetection>();//keep output summary
            //--- Preprocess to obtain BasePosture Data
            UKI_Offline mr = new UKI_Offline();
            mr.UKI_OfflineProcessing(list_raw, 0);
            List<UKI_Data_AnalysisForm> data = TheUKI.getData_AnalysisForm(mr.data.data_raw, mr.data.data_bp);
            //--- Analysis Motion by Motion
            foreach(m_Motion motion in list_motions){
                Boolean detected = TheRuleTester.testDetectMotion(data, motion.inputs);
                logDetection log = TheRuleTester.temp_log;
                //
                log.info = motion.name + " ( " + log.info + " )";
                if (detected) { log.info = "[" + log.detectAt + "] " + log.info; }
                else { log.info = "[X] " + log.info; }
                log_list.Add(log);
                matrix_data += "," + TheTool.convertBoolean_01(detected);
            }
            //--------------------------------
            TheSys.showError("File: " + currentFileName);
            foreach (logDetection s in log_list.OrderBy(o => o.detectAt).ThenBy(o => o.num_pose))
            {
                TheSys.showError(s.info);
            }
            TheSys.showError("---------------------");
            return matrix_data;
        }

        //Motion Recognition
        private void butMR_test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String path_RawData;
                foreach (DataRow r in dataTable.Rows)
                {
                    try
                    {
                        path_RawData = r[col_path].ToString();
                        string folderName = TheTool.getFileName_byPath(path_RawData);
                        UKI_Offline mr = new UKI_Offline();
                        mr.UKI_OfflineProcessing(TheUKI.csv_loadFileTo_DataRaw(path_RawData), 1);
                        mr.data.exportFile(folderName, "UKI_", true, true, false);
                    }
                    catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
                }
                System.Windows.MessageBox.Show(@"Save to file", "Export Data");
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Input1: RAW Data");
            TheSys.showError("Input2: Rules for Motions");
            TheSys.showError("Output: Results what are motions detected in each file");
        }

        private void butHelp2_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Input: RAW Data");
            TheSys.showError("Output: Motion Recognition (Offline ver. of Realtime UKI)");
        }

    }
}
