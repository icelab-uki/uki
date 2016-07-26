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

    public partial class TotalViewer : Window
    {
        public TotalViewer()
        {
            InitializeComponent();
            resetTable();
            setupCombo();
        }

        //--- Column ----------
        string col_id = "id";
        string col_path = "file_path";
        string col_random = "RandomNum";
        string col_colCount = "ColCount";
        //-----------------------------

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

        //Browser File
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            TheTool.openFileDialog_01(true,".csv","");
            
            int id = 1;
            string[] result = TheTool.dialog.FileNames;
            if (result.Count() > 0) { resetTable(); }
            foreach (string y in result)
            {
                dataTable.Rows.Add(id,y);
                id++;
            }
            rowCount();
        }

        void runID()
        {
            try
            {
                int id = 1;
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    dataTable.Rows[i][col_id] = id.ToString(); id++;
                }
            }
            catch { }
        }

        DataTable dataTable = null;//data table that become datagrid
        //-------------------------------------

        int row_count = 0;
        void rowCount()
        {
            try { 
                row_count = dataTable.Rows.Count;
                txtRow.Content = "Row: " + row_count; 
            }
            catch { }
        }


        private void butOpenFolderClick(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder); }
            catch { }
        }

        private void butOpenFolderClick2(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder + TheURL.url_9_PAnalysis); }
            catch { }
        }

        // Execute shutdown tasks
        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.Show();
        }

        private void butDelCol_Click(object sender, RoutedEventArgs e)
        {
            string delCol = txtDelCol.Text;
            string path;
            string errorList = "";
            foreach (DataRow r in dataTable.Rows)
            {
                path = r[col_path].ToString();
                errorList += TheTool.export_CSV_delColumn(path, delCol);
            }
            System.Windows.MessageBox.Show(@"Save to file\[DelCol]\");
            //--------------------------------
            if (errorList != "") { 
                System.Windows.MessageBox.Show("ERROR files" + System.Environment.NewLine  + errorList); 
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = dataTable;
                int colCountExpect = TheTool.getInt(txtColCount);
                if (dt.Columns.Contains(col_colCount) == false)
                {
                    dt.Columns.Add(col_colCount);
                }
                foreach (DataRow r in dt.Rows)
                {
                    int c = TheTool.CSV_countCol(r[col_path].ToString());
                    if (c != colCountExpect 
                        && c != 0) {
                            r[col_colCount] = c; 
                        } 
                    else { r[col_colCount] = ""; }
                }
                setDataGrid(this.dataTable);
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
        }

        private void butRandomSort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = dataTable;
                if (dt.Columns.Contains(col_random) == false)
                {
                    dt.Columns.Add(col_random, typeof(Int32));
                }
                Random random = new Random();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][col_random] = random.Next(10000);
                }
                //------------------------------
                DataView dv = new DataView(dt);
                dv.Sort = col_random;
                //------------------------------
                this.dataTable = dv.ToTable();
                setDataGrid(this.dataTable);
                runID();
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
        }

        private void buNameCode_Click(object sender, RoutedEventArgs e)
        {
            TheTool.export_nameCoder(dataTable, col_path, col_id);
            exportNameCodeList();
            System.Windows.MessageBox.Show(@"Save to file\[NameCode]\");
        }


        //file that tell what are original name
        void exportNameCodeList()
        {
            try
            {
                DataTable dt = dataTable.Copy();
                string colK2 = "K2"; string colK4 = "K4";
                dt.Columns.Add(colK2);
                dt.Columns.Add(colK4);
                //---------------------------
                string v;
                foreach (DataRow r in dt.Rows)
                {
                    v = r[col_path].ToString();
                    v = TheTool.getFileName_byPath(v);
                    r[col_path] = v;
                    if (v.Contains("MM")) { r[colK2] = "M"; r[colK4] = "MM"; }
                    else if (v.Contains("M")) { r[colK2] = "M"; r[colK4] = "M"; }
                    else if (v.Contains("S")) { r[colK2] = "S"; r[colK4] = "SS"; }
                    else if (v.Contains("SS")) { r[colK2] = "S"; r[colK4] = "S"; }
                }
                //---------------------------
                dt.Columns[col_id].ColumnName = "filename";
                dt.Columns[col_path].ColumnName = "original";
                dt.Columns.Remove(col_random);
                TheTool.export_dataTable_to_CSV(TheURL.url_saveFolder + TheURL.url_9_NameCode + @"\[List].csv", dt);
            }
            catch { }
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

        private void butCleanColCount_Click(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Remove(col_colCount);
            setDataGrid(this.dataTable);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            doTransform(0);
            sub.confirmOnClose = true;
        }

        //----------------------------------
        //Temp Variable
        SubViewer sub = new SubViewer();
        SubViewer_Extend sub_t = new SubViewer_Extend();
        string save_setName = "unnamed";
        //---------------------------
        
        void doTransform(int type)
        {
            sub = new SubViewer();
            save_setName = getExportFileName();
            sub.fileName = save_setName;
            sub.setTitle("P-Analysis");
            sub.Show();
            sub.path_saveRoot = TheURL.totalView_path_saveRoot;
            //------------------------------------
            sub_t = new SubViewer_Extend();
            sub_t.path_saveRoot = TheURL.totalView_path_saveRoot;
            sub_t.folder_forSave = save_setName;
            sub_t.saveSubTable = checkSaveSub.IsChecked.Value;
            sub_t.showSample = checkSample.IsChecked.Value;
            sub_t.subViewer = sub;
            sub_t.decimalNum = this.decimalNum;
            //
            sub_t.normaliz_method = getNormalizeCode(type);
            sub_t.getMainTable_byPathTable(dataTable.Copy());
            //------------------------------------
        }

        string getExportFileName(){
            try
            {
                string txt = DateTime.Now.ToString("MMdd HHmmss") + " " + row_count;
                return txt;
            }
            catch { return "unnamed"; }
        }

        int getNormalizeCode(int type)
        {
            int normaliz_method = 0;
            if (type == 1)
            {
                if (normal4.IsChecked == true) { normaliz_method = 4; }
            }
            else
            {
                if (normal1.IsChecked == true) { normaliz_method = 1; }
            }
            return normaliz_method;
        }

        //==========================================
        int decimalNum = 10;//Base Decimal

        void setupCombo()
        {
            for (int i = 0; i <= 10; i++) { comboDecimal.Items.Add(i.ToString()); }
            comboDecimal.SelectedIndex = decimalNum;
        }

        //===========================================
  
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            List<string> filePath_list = TheTool.dataTable_getList_fromColumn(dataTable, col_path);
            int skip = TheTool.getInt(txtSkipConcat);
            List<string> concat_txt = new List<string>();
            if (checkOWSconcat.IsChecked.Value) { concat_txt = TheTool.concatFile_OWS(filePath_list, true, skip); }
            else { concat_txt = TheTool.concatFile(filePath_list, true, skip); }
            TheTool.exportFile(concat_txt, TheURL.url_saveFolder + @"concat.csv", true);
        }



        //***************************************************************
        //*********************** Super Normalize****************************

        
        //Learn where is Min - Max
        private void butLearn_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".csv", "");
            // Get the selected file name and display in a TextBox
            if (openDialog == true)
            {
                TheMinMaxNormaliz.getDataTable(TheTool.dialog.FileName);
                if (TheMinMaxNormaliz.minmax_Euclidian_ready)
                {
                    TheMinMaxNormaliz.showMinMax_Euclidian_Table();
                    butA2.IsEnabled = true;
                };
            }
        }

        //-------------------
        
        private void normal4_Click(object sender, RoutedEventArgs e)
        {
            if (TheMinMaxNormaliz.minmax_Euclidian_ready == true)
            {
                doTransform(1);
            }
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //P-Analysis ---------------------------------
                checkSaveSub.IsChecked = true;
                checkSample.IsChecked = false;
                doTransform(0);
                sub.Close();
                //Concat Sub files & export ---------------------------------
                List<string> concat_txt = TheTool.concatFile_OWS(sub_t.list_fileSave, true, 2);
                string path_saveFolder = TheURL.totalView_path_saveRoot + @"[NEu]\" + sub_t.getSubSubFolder();
                TheTool.Folder_CreateIfMissing(path_saveFolder);
                string path_concatFile = path_saveFolder + @"\concat.csv";
                TheTool.exportFile(concat_txt, path_concatFile, false);
                //Learn MinMax & export ---------------------------------
                TheMinMaxNormaliz.getDataTable(path_concatFile);
                TheMinMaxNormaliz.buildMinMax_Euclidian_Table();
                TheTool.export_dataTable_to_CSV(path_saveFolder + @"\minmax.csv", TheMinMaxNormaliz.dt_MinMax_Euclidian);
                //--------------------
                doTransform(1);
                sub.confirmOnClose = true;
            }
            catch { }
        }


        //========================================================================
        //========== Back to Nature =============================================

        public string[] natureColumn = { "file"
                                    ,"user","Time"
                                    ,"Head_x","Head_y","Head_z"
                                    ,"ShoulderCenter_x","ShoulderCenter_y","ShoulderCenter_z"
                                    ,"ShoulderLeft_x","ShoulderLeft_y","ShoulderLeft_z"
                                    ,"ShoulderRight_x","ShoulderRight_y","ShoulderRight_z"
                                    ,"ElbowLeft_x","ElbowLeft_y","ElbowLeft_z"
                                    ,"ElbowRight_x","ElbowRight_y","ElbowRight_z"
                                    ,"WristLeft_x","WristLeft_y","WristLeft_z"
                                    ,"WristRight_x","WristRight_y","WristRight_z"
                                    ,"HandLeft_x","HandLeft_y","HandLeft_z"
                                    ,"HandRight_x","HandRight_y","HandRight_z"
                                    ,"Head_D","ShoulderCenter_D","ShoulderLeft_D","ShoulderRight_D"
                                    ,"ElbowLeft_D","ElbowRight_D","WristLeft_D","WristRight_D"
                                    ,"HandLeft_D","HandRight_D"
                                };
        
        private void butBackNature_Click(object sender, RoutedEventArgs e)
        {
            string path;
            string errorList = "";
            foreach (DataRow r in dataTable.Rows)
            {
                path = r[col_path].ToString();
                errorList += TheTool.export_CSV_delColumn_byAllow(path, natureColumn);
            }
            System.Windows.MessageBox.Show(@"Save to file\[DelCol]\");
            //--------------------------------
            if (errorList != "")
            {
                System.Windows.MessageBox.Show("ERROR files" + System.Environment.NewLine + errorList);
            }
        }

        private void butViewColList_Click(object sender, RoutedEventArgs e)
        {
            string txt = "";
            foreach (string t in natureColumn)
            {
                txt += t + System.Environment.NewLine;
            }
            TheSys.showError(txt, true);
        }

        //===========================================================================

        String path_folder_convert = TheURL.url_saveFolder + TheURL.url_9_Convert + @"\";

        private void msr_butBrowse_Click(object sender, RoutedEventArgs e)
        {
            TheTool.openFileDialog_01(true, ".txt", "");
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

        private void msr_butFolder_Click(object sender, RoutedEventArgs e)
        {
            try {
                TheTool.Folder_CreateIfMissing(path_folder_convert);
                Process.Start(path_folder_convert);
            }
            catch { }
        }

        private void butMsrConv_Click(object sender, RoutedEventArgs e)
        {
            TheConverter.MSR_convertFile(dataTable, col_path, path_folder_convert);
        }

        private void msr_sample_Click(object sender, RoutedEventArgs e)
        {
            TheConverter.MSR_showSample();
        }

        //===========================================================================

        private void butUKI_rawSample_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(TheURL.url_tv_sample_raw);
        }

        private void butUKIconvert_Click(object sender, RoutedEventArgs e)
        {
            String fileName;
            String path_raw;
            String path_raw_centered;
            String path_ang;
            String path_ang_en;
            String path_raw_en;
            String path_dist_en;
            int ang_techq = comboAngTech.SelectedIndex;//center of angle
            int center_techq = comboRawCenter.SelectedIndex;//center of body position
            foreach (DataRow r in dataTable.Rows)
            {
                path_raw = r[col_path].ToString();
                fileName = TheTool.getFileName_byPath(path_raw);
                TheTool.Folder_CreateIfMissing(path_folder_convert + fileName);
                String path_sub_folder = path_folder_convert + fileName + @"\";
                path_raw_centered = path_sub_folder + fileName + " (center" + TheUKI.getCenterTechqName(center_techq) + ").csv";
                path_ang = path_sub_folder + fileName + " ANG(" + TheUKI.getAngTechqName(ang_techq) + ").csv";
                path_ang_en = path_sub_folder + fileName + " ANG_Entropy.csv";
                path_raw_en = path_sub_folder + fileName + " RAW_Entropy.csv";
                path_dist_en = path_sub_folder + fileName + " Dist_Entropy.csv";
                if (center_techq > 0) {
                    path_raw_en = path_sub_folder + fileName +
                        " (center" + TheUKI.getCenterTechqName(center_techq) + ") RAW_Entropy.csv"; 
                }
                //----- Raw -----
                List<UKI_DataRaw> list_raw = TheUKI.csv_loadFileTo_DataRaw(path_raw, 0);
                //----- Angle ---------
                List<UKI_DataAngular> list_ang = TheUKI.calAngle_fromRaw(list_raw, ang_techq);
                if (checkUKI_Angle.IsChecked.Value || checkUKI_E_Angle.IsChecked.Value) { TheUKI.saveData_Ang(path_ang, list_ang); }
                //---------------------
                //List<int> keyPostureId = UKI_getKeyPostureId();
                List<int[]> keyPostureRange = ThePosExtract.getKetPose_Range_from1String(txtUKI_keyID.Text);
                //----- Angle Entropy ---------
                if (checkUKI_E_Angle.IsChecked.Value)
                {
                    ThePosExtract.UKI_CalEntropy_Angle(path_ang_en, path_ang, keyPostureRange);
                }
                //----- Dist ---------
                if (checkUKI_E_Dist.IsChecked.Value)
                {
                    ThePosExtract.UKI_CalEntropy_Eu(path_dist_en, path_raw, keyPostureRange);
                }
                //----- XYZ Entropy ---------
                if (checkUKI_E_XYZ.IsChecked.Value) { 
                    if(center_techq > 0){
                        List<UKI_DataRaw> list_raw_centered = TheUKI.raw_centerBodyJoint(list_raw, center_techq);
                        TheUKI.saveData_Raw_centered(path_raw_centered, list_raw_centered, center_techq);
                        ThePosExtract.UKI_CalEntropy_1By1(path_raw_en, path_raw_centered, keyPostureRange);
                    }
                    else { ThePosExtract.UKI_CalEntropy_1By1(path_raw_en, path_raw, keyPostureRange); }
                }
            }
            System.Windows.MessageBox.Show(@"Save to file\[Convert]");
        }

        //List<int> UKI_getKeyPostureId()
        //{
        //    String[] keyPostureStr = TheTool.splitText(txtUKI_keyID.Text, ",");
        //    List<int> keyPostureId = new List<int> { };
        //    for (int i = 0; i < keyPostureStr.Count(); i++)
        //    {
        //        keyPostureId.Add(TheTool.getInt(keyPostureStr[i]));
        //    }
        //    return keyPostureId;
        //}

        private void butSortIndy_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRow r in dataTable.Rows)
            {
                string path = r[col_path].ToString();
                List<string> list_output = new List<string>();//keep output
                list_output.Add(TheTool.read_File_getFirstLine(path));//add header
                //------------------
                List<List<double>> row_col = TheTool.read_File_getListListDouble(path, true);
                List<List<double>> col_row = TheTool.ListList_SwitchColumnToRow(row_col);
                TheTool.SortData(col_row);
                row_col = TheTool.ListList_SwitchColumnToRow(col_row);
                list_output.AddRange(TheTool.getListString(row_col));
                TheTool.exportCSV_orTXT(TheURL.url_saveFolder + TheTool.getFileName_byPath(path) + " (Indy Sort).csv", list_output, false);
            }
            System.Windows.MessageBox.Show("Export to " + TheURL.url_saveFolder); 
        }

        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Input: (Dynamic Format)");
            TheSys.showError("Process [CheckError]:  For all files, check if the total number of column is equal to expected");
            TheSys.showError("Process [Col Del]: For all files, delete specific column");
            TheSys.showError("Process [File name Coder]: Random sort and rename to climinate bias before testing");
            TheSys.showError("Process [Col Del]: For all files, delete all columns those are not match the List");
        }

        private void butHelp2_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Process [Indy Sort]: Sort data from first to last column (Sample Usage - Entropy Analysis)");
        }

        private void butHelp3_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Process: Create Files with different Data Representation for UKI data");
        }


    }
}
