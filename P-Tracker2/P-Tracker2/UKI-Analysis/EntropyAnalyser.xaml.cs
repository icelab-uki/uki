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

    public partial class EntropyAnalyser : Window
    {
        //Data Container of this table is...
        InstanceContainer container = new InstanceContainer();

        public EntropyAnalyser()
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
            try { Process.Start(TheURL.url_saveFolder + TheURL.url_2_ukiEntropy); }
            catch { }
        }

        private void butBrowseRaw_Click(object sender, RoutedEventArgs e)
        {
            TheTool.openFileDialog_01(true, ".csv", TheURL.url_saveFolder);
            int id = 1;
            string[] result = TheTool.dialog.FileNames;
            if (result.Count() > 0) { resetTable(); }
            foreach (string y in result)
            {
                dataTable.Rows.Add(id, y);
                id++;
            }
            rowCount();
            butAnalyze.IsEnabled = true;
            loadData();
        }
        
        //===============================================================================================
        //===============================================================================================

        ////Normalized >> Discrete 0.1 >> Entropy
        //void test1()
        //{
        //    string path_GlobalMM = TheURL.url_saveFolder + "[MinMax].csv";//in case Global
        //    foreach (DataRow r in dataTable.Rows)
        //    {
        //        try
        //        {
        //            string path_input = r[col_path].ToString();//Raw Data, Non-Normalized
        //            string path_normal = TheURL.url_saveFolder + "test01 Normal.csv";
        //            string path_discritized = TheURL.url_saveFolder + "test02 Discrete.csv";
        //            string path_discritized_en = TheURL.url_saveFolder + "test03 Discrete Entropy.csv";
        //            DataTable data_normalized = ThePosExtract.getNormalizedTable(path_input, path_GlobalMM, false);
        //            int row = data_normalized.Rows.Count;
        //            DataTable data_discritized = TheTool.dataTable_discritize10Partition(data_normalized);
        //            TheTool.export_dataTable_to_CSV(path_normal, data_normalized);
        //            TheTool.export_dataTable_to_CSV(path_discritized, data_discritized);
        //            List<int> list_key = new List<int> { 0, row };
        //            ThePosExtract.UKI_CalEntropy_1By1(path_discritized_en, path_discritized, list_key);
        //            System.Windows.MessageBox.Show(@"Save to 'file'", "Export Data");
        //        }
        //        catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
        //    }
        //}

        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Input: RAW X,Y,Z");
            TheSys.showError("Output: Recommended Feature by several Feature Selection Algorithm");
        }

        List<string> summary_total = new List<string>();//for write .txt

        //------------------------------------

        List<m_If> temp_posture_current = new List<m_If>();//currently analyzed posture
        List<String> temp_score_regession = new List<String>();
        void motionModel_exportAll(string path_saveFolder, string suffix)
        {
            if (checkEntropy.IsChecked.Value)
            {
                motionModel_exportEach(path_saveFolder + @"\Greedy model" + suffix + ".xml", temp_motion_Greedy);
                motionModel_exportEach(path_saveFolder + @"\MIFS model" + suffix + ".xml", temp_motion_MIFS);
                motionModel_exportEach(path_saveFolder + @"\CMIM model" + suffix + ".xml", temp_motion_CMIM);
                motionModel_exportEach(path_saveFolder + @"\JMIM model" + suffix + ".xml", temp_motion_JMIM);
            }
            if (checkDCFS.IsChecked.Value)
            {
                motionModel_exportEach(path_saveFolder + @"\DCFS model" + suffix + ".xml", temp_motion_DCFS);
            }
        }

        void motionModel_exportEach(string path_save, m_Motion motion)
        {
            List<m_Motion> motion_list = new List<m_Motion>();
            motion_list.Add(motion);
            TheMapData.saveXML_Motion(path_save, motion_list);
        }

        void motionModel_evaluationAll() {
            if (checkEntropy.IsChecked.Value)
            {
                motionModel_evaluationEach("Performance Greedy", temp_motion_Greedy);
                motionModel_evaluationEach("Performance MIFS", temp_motion_MIFS);
                motionModel_evaluationEach("Performance JMIM", temp_motion_JMIM);
                motionModel_evaluationEach("Performance CMIM", temp_motion_CMIM);
            }
            if (checkDCFS.IsChecked.Value)
            {
                motionModel_evaluationEach("Performance DCFS", temp_motion_DCFS);
            }
        }

        void motionModel_evaluationEach(string header, m_Motion motion)
        {
            summary_total.Add("");
            List<string> motionFailed = new List<string>();

            int detectected_count = 0;
            int all_inst = container.list_inst.Count();
            List<string> all_log = new List<string>();
            foreach (Instance inst in container.list_inst)
            {
                Boolean detected = TheRuleTester.testDetectMotion(inst.getDataRaw(true), motion.inputs);
                logDetection log = TheRuleTester.temp_log;
                string s = "";
                if (detected) { 
                    s += "[O] ";
                    detectected_count++;
                }
                else
                { 
                    s += "[X] ";
                    motionFailed.Add(inst.name + " is unrecognized with " + header);
                }
                s += inst.name + " [" + log.info + "]";
                all_log.Add(s);
            }
            double acc = (double) detectected_count * 100 / all_inst;
            acc = Math.Round(acc, 2);
            //---------
            summary_total.Add(header += " : " + acc + "% (" + detectected_count + "/" + all_inst + ")");
            foreach (string s in all_log) { summary_total.Add(s); }
            if (acc < 100)
                TheTool.exportFile(motionFailed, TheTool.filePath + "all_incomplete_motion.txt", false, true);
        }

        string path_root = TheURL.url_saveFolder + TheURL.url_2_ukiEntropy;

        //Keep complete Motion Rule (for whole motion)
        m_Motion temp_motion_Greedy = new m_Motion() { name = "New Motion" };
        m_Motion temp_motion_MIFS = new m_Motion() { name = "New Motion" };
        m_Motion temp_motion_JMIM = new m_Motion() { name = "New Motion" };
        m_Motion temp_motion_CMIM = new m_Motion() { name = "New Motion" };
        m_Motion temp_motion_DCFS = new m_Motion() { name = "New Motion" };
        
        void motionModel_reset()
        {
            temp_motion_Greedy.inputs.Clear();
            temp_motion_MIFS.inputs.Clear();
            temp_motion_JMIM.inputs.Clear();
            temp_motion_CMIM.inputs.Clear();
            temp_motion_DCFS.inputs.Clear();
        }

        public static int centerTechnique = TheUKI.centerTechq_SC_HC;

        private void butAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (checkCombine.IsChecked.Value) { analyze_combined(); }
            else { analyze_1By1(); }
            System.Windows.MessageBox.Show(@"Save to '" + TheURL.url_saveFolder + TheURL.url_2_ukiEntropy + "'", "Export Files");
        }

        //INPUT = All , Output = All
        void analyze_1By1() 
        {
            foreach (Instance inst in container.list_inst)
            {
                //Per File (1 Whole Motion)
                try
                {
                    TheSys.showError("=================================");
                    string fileName = TheTool.getFileName_byPath(inst.path);
                    string path_saveFolder = path_root + fileName + (" (FS)");
                    TheTool.Folder_CreateIfMissing(path_saveFolder); path_saveFolder = path_saveFolder + @"\";
                    string path_raw_centered = path_saveFolder + fileName + " (center" + TheUKI.getCenterTechqName(centerTechnique) + ").csv";
                    //-----------------
                    motionModel_reset();
                    //----- Load Data >> Build Centered Data ---------------------------------------
                    DataTable dt_raw_center = UKI_ThePreprocessor.getDatatable_centered(inst.getDataRaw(checkExtraFeature.IsChecked.Value), checkExtraFeature.IsChecked.Value);//raw
                    //----- Process  ---------------------------------------
                    TheTool.export_dataTable_to_CSV(path_raw_centered, dt_raw_center);
                    summary_total.Clear();
                    summary_total.Add(fileName); 
                    if (checkSegment.IsChecked == false)
                    {
                        DataTable dt_threshold_pose1 = dt_raw_center.Clone();
                        dt_threshold_pose1.Rows.Add(dt_raw_center.Rows[0].ItemArray);
                        DataTable dt_threshold_pose2 = dt_raw_center.Clone();
                        dt_threshold_pose2.Rows.Add(dt_raw_center.Rows[dt_raw_center.Rows.Count - 1].ItemArray);
                        analyze_Table(dt_raw_center, path_saveFolder, dt_threshold_pose1, dt_threshold_pose2);
                        motionModel_exportAll(path_saveFolder, "");
                        motionModel_evaluationAll();
                        summary_total.Add("");
                        TheSys.showError(summary_total);
                        TheTool.exportFile(summary_total, path_saveFolder + "result.txt", false);
                    }
                    else
                    {
                        int pose_number = 0;
                        List<int[]> key_pose = inst.getKeyPose();
                        summary_total.Add((key_pose.Count - 1) + " Key Postures");
                        List<DataTable> dt_1pose_list = TheTool.dataTable_split(dt_raw_center, key_pose);
                        DataTable dt_threshold_pose1 = dt_raw_center.Clone();
                        dt_threshold_pose1.Rows.Add(dt_1pose_list.First().Rows[0].ItemArray);
                        foreach (DataTable dt_1pose in dt_1pose_list)
                        {
                            DataTable dt_threshold_pose2 = dt_raw_center.Clone();
                            dt_threshold_pose2.Rows.Add(dt_1pose.Rows[dt_1pose.Rows.Count - 1].ItemArray);
                            int p_id = pose_number + 1;
                            int start = inst.keyPose[pose_number][0];
                            int end = inst.keyPose[pose_number][1];
                            string txt1 = "Pose" + p_id + ": " + start + "-" + end;
                            summary_total.Add("");
                            summary_total.Add(txt1);
                            //
                            string subFolder = "Pose" + p_id + " (" + start + "-" + end + ")";
                            string path_saveFolder_sub = path_saveFolder + subFolder;
                            TheTool.Folder_CreateIfMissing(path_saveFolder_sub);
                            path_saveFolder_sub = path_saveFolder_sub + @"\";
                            //TheTool.export_dataTable_to_CSV(path_saveFolder_sub + "data.csv", dt);
                            analyze_Table(dt_1pose, path_saveFolder_sub, dt_threshold_pose1, dt_threshold_pose2);
                            pose_number++;
                        }
                        motionModel_exportAll(path_saveFolder, " (segmented)");
                        motionModel_evaluationAll();
                        summary_total.Add("");
                        TheSys.showError(summary_total);
                        TheTool.exportFile(summary_total, path_saveFolder + "result (segmented).txt", false);
                    }
                }
                catch (Exception ex) { TheSys.showError(inst.path + " : " + ex.ToString()); }
            }
        }

        void analyze_combined()
        {
            try
            {
                TheSys.showError("=================================");
                string fileName = DateTime.Now.ToString("MMdd_HHmmss");
                string path_saveFolder = path_root + "Combine " + fileName + (" (FS)");
                //
                double partition_range = TheTool.getDouble(txtPartitionRange);
                path_saveFolder += "(p=" + partition_range + ")";
                //
                TheTool.Folder_CreateIfMissing(path_saveFolder); path_saveFolder = path_saveFolder + @"\";
                //-----------------
                motionModel_reset();
                summary_total.Clear();
                summary_total.Add(fileName + " Combination");
                if (checkSegment.IsChecked == false)
                {
                    string path_raw_centered = path_saveFolder + "Combine " + fileName + " (center" + TheUKI.getCenterTechqName(centerTechnique) + ").csv";
                    List<UKI_DataRaw> list_raw_concat = new List<UKI_DataRaw>();
                    foreach (Instance inst in container.list_inst)
                    {
                        list_raw_concat.AddRange(inst.getDataRaw(true));
                        summary_total.Add("- " + inst.name);
                    }
                    //----- Load Data >> Build Centered Data ---------------------------------------
                    DataTable dt_raw_center = UKI_ThePreprocessor.getDatatable_centered(list_raw_concat, checkExtraFeature.IsChecked.Value);//raw
                    TheTool.export_dataTable_to_CSV(path_raw_centered, dt_raw_center);
                    //----- Process  ---------------------------------------
                    //DataTable dt_raw = CSVReader.ReadCSVFile(path_raw_centered, true);//raw
                    //
                    DataTable dt_threshold_pose1 = dt_raw_center.Clone();
                    dt_threshold_pose1.Rows.Add(dt_raw_center.Rows[0].ItemArray);
                    DataTable dt_threshold_pose2 = dt_raw_center.Clone();
                    dt_threshold_pose2.Rows.Add(dt_raw_center.Rows[dt_raw_center.Rows.Count - 1].ItemArray);

                    analyze_Table(dt_raw_center, path_saveFolder, dt_threshold_pose1, dt_threshold_pose2);
                    motionModel_exportAll(path_saveFolder, "");
                    motionModel_evaluationAll();
                    TheSys.showError(summary_total);
                    TheTool.exportFile(summary_total, path_saveFolder + "result (combined).txt", false);
                }
                else
                {
                    //Assumption: all given Instance has same class
                    List<DataTable> dt_centered_concat_list = new List<DataTable>();
                    List<DataTable> dt_threshold = new List<DataTable>();
                    UKI_ThePreprocessor.preprocess_CombinedSegmented(container.list_inst, checkExtraFeature.IsChecked.Value, ref dt_centered_concat_list, ref dt_threshold);
                    //-----
                    summary_total.AddRange(UKI_ThePreprocessor.temp_summary);
                    int[] mode = UKI_ThePreprocessor.temp_mode;
                    int p_key = 0;
                    foreach (DataTable dt_1pose in dt_centered_concat_list)
                    {
                        //1 Posture of Motion
                        int p_id = p_key + 1;
                        string subFolder = "Pose " + p_id;
                        string path_saveFolder_sub = path_saveFolder + @"\" + subFolder;
                        TheTool.Folder_CreateIfMissing(path_saveFolder_sub);
                        path_saveFolder_sub += @"\";
                        string path_raw_centered = path_saveFolder_sub + " Combine " + fileName + " (center" + TheUKI.getCenterTechqName(centerTechnique) + ").csv";
                        TheTool.export_dataTable_to_CSV(path_raw_centered, dt_1pose);
                        //----------
                        summary_total.Add("");
                        summary_total.Add("Pose" + p_id + ":");
                        foreach (Instance inst in container.list_inst)
                        {
                            if (inst.keyPose.Count() == mode[0])
                            {
                                summary_total.Add("- " + inst.keyPose[p_key][0] 
                                    + "-" + inst.keyPose[p_key][1]
                                    + " of " + inst.name);
                            }
                        }
                        analyze_Table(dt_1pose, path_saveFolder_sub, dt_threshold[0], dt_threshold[p_id]);
                        p_key++;
                    }
                    motionModel_exportAll(path_saveFolder, " (segmented combined)");
                    motionModel_evaluationAll();
                    summary_total.Add("");
                    //
                    List<string> threshold_data = TheTool.dataTable_CombineShuffle_getListString(dt_threshold);
                    TheTool.exportFile(threshold_data, path_saveFolder + "threshold.csv", false);
                    //
                    TheTool.exportFile(summary_total, path_saveFolder + "result (segmented combined).txt", false);
                    TheSys.showError(summary_total);
                }
            }
            catch (Exception ex)
            {
                TheSys.showError("Combined Analysis : " + ex.ToString());
            }
        }

        //----------------------------------------------------------------------

        ModelGenerator model_generator;

        //Data Table = 1 Concat File : 1 Posture (segmented)
        void analyze_Table(DataTable dt_raw_center_concat, string path_saveFolder, DataTable dt_threshold_pose1, DataTable dt_threshold_pose2)
        {
            model_generator = new ModelGenerator();
            model_generator.process00_setting(container.list_inst, null, 
                radioLoopAuto.IsChecked.Value, checkLoopMaxTrue.IsChecked.Value, 
                checkMinF.IsChecked.Value, 0, checkExtraFeature.IsChecked.Value, 
                false, false,
                TheTool.getDouble(txtPartitionRange));
            model_generator.process01_selectFirstFeature(checkEntropy.IsChecked.Value, checkDCFS.IsChecked.Value, dt_raw_center_concat, dt_threshold_pose1, dt_threshold_pose2);
            //--- Export -------------------------------
            if (checkEntropy.IsChecked.Value)
            {
                List<string> list_MIstr = new List<string>();
                list_MIstr.Add("fi1,fi2,MI");
                list_MIstr.AddRange(TheTool.str2_double_getListString(model_generator.list_MI));
                TheTool.export_dataTable_to_CSV(path_saveFolder + "01 partitize.csv", model_generator.dt_ready);
                TheTool.exportCSV_orTXT(path_saveFolder + "02 Entropy.csv", TheTool.str_double_getListString(model_generator.list_entropy), false);
                TheTool.exportCSV_orTXT(path_saveFolder + "03 MI.csv", list_MIstr, false);
            }
            if (checkDCFS.IsChecked.Value)
            {
                List<string> list_Correlation = new List<string>();
                list_Correlation.Add("fi1,fi2,R");
                list_Correlation.AddRange(TheTool.str2_double_getListString(model_generator.DCFS_listCorrelation));
                TheTool.exportCSV_orTXT(path_saveFolder + "05 Delta.csv", model_generator.DCFS_listDelta_getListString(false), false);
                TheTool.exportCSV_orTXT(path_saveFolder + "06 Correlation.csv", list_Correlation, false);
            }
            //loop for candidate =============================================================
            int loopTime = TheTool.getInt(textLoopTime.Text) - 1;
            List<string> score_regression_summary = new List<string>();
            //
            if (checkEntropy.IsChecked.Value)
            {
                selectCandidate_withText(path_saveFolder + "Greedy", TheEntropy.methodSelection_Greedy, loopTime);
                TheModelGenerator.motionModel_add1Posture(temp_motion_Greedy, temp_posture_current);
                score_regression_summary.Add("f_num,new_feature,threshold,Greedy_Total,Greedy_T,Greedy_F");
                score_regression_summary.AddRange(model_generator.score_regression_summary);
                //
                selectCandidate_withText(path_saveFolder + "MIFS", TheEntropy.methodSelection_MIFS, loopTime);
                TheModelGenerator.motionModel_add1Posture(temp_motion_MIFS, temp_posture_current);
                score_regression_summary.Add("");
                score_regression_summary.Add("f_num,new_feature,threshold,MIFS_Total,MIFS_T,MIFS_F");
                score_regression_summary.AddRange(model_generator.score_regression_summary);
                //
                selectCandidate_withText(path_saveFolder + "CMIM", TheEntropy.methodSelection_CMIM, loopTime);
                TheModelGenerator.motionModel_add1Posture(temp_motion_CMIM, temp_posture_current);
                score_regression_summary.Add("");
                score_regression_summary.Add("f_num,new_feature,threshold,CMIM_Total,CMIM_T,CMIM_F");
                score_regression_summary.AddRange(model_generator.score_regression_summary);
                //
                selectCandidate_withText(path_saveFolder + "JMIM", TheEntropy.methodSelection_JMIM, loopTime);
                TheModelGenerator.motionModel_add1Posture(temp_motion_JMIM, temp_posture_current);
                score_regression_summary.Add("");
                score_regression_summary.Add("f_num,new_feature,threshold,JMIM_Total,JMIM_T,JMIM_F");
                score_regression_summary.AddRange(model_generator.score_regression_summary);
            }
            if (checkDCFS.IsChecked.Value)
            {
                selectCandidate_withText(path_saveFolder + "DCFS", TheEntropy.methodSelection_DCFS, loopTime);
                TheModelGenerator.motionModel_add1Posture(temp_motion_DCFS, temp_posture_current);
                if (checkEntropy.IsChecked.Value) { score_regression_summary.Add(""); }
                score_regression_summary.Add("f_num,new_feature,threshold,DCFS_Total,DCFS_T,DCFS_F");
                score_regression_summary.AddRange(model_generator.score_regression_summary);
            }
            //-----------
            TheTool.exportCSV_orTXT(path_saveFolder + "04 Score.csv", score_regression_summary, false);
        }

        List<string> temp_summary = new List<string>();
        
        //Output is f2 and so on
        void selectCandidate_withText(
            string path_saveFolder,
            int methodSelection, int loopTime)
        {
            temp_summary.Clear();
            DateTime time_start = DateTime.Now;
            String name = TheTool.getFileName_byPath(path_saveFolder);
            TheTool.Folder_CreateIfMissing(path_saveFolder);
            path_saveFolder = path_saveFolder + @"\";
            //-----------------
            string path_prefix = path_saveFolder + name + " Quality fi";
            List<Feature> list_S0 = model_generator.process02_selectCandidate(path_prefix, methodSelection, loopTime);
            temp_posture_current = TheMapData.convertfeaturelist_to_ListIf(list_S0);//add model
            temp_score_regession = model_generator.score_regression_summary;
            //-----------------
            int i = 1;
            foreach (Feature f in list_S0)
            {
                temp_summary.Add(getSummarizedFeature(i, f));
                i++;
            }
            //============================================
            int timespan = (int)DateTime.Now.Subtract(time_start).TotalMilliseconds;
            //----------------
            summary_total.Add("----- " + name + " (" + timespan + " ms) -----");
            summary_total.AddRange(temp_summary);
        }

        string getSummarizedFeature(int id, Feature f)
        {
            return id + " " + f.name + " " + f.opt + " " + f.v;
        }


        //=================================================================

        void loadData()
        {
            container.list_inst.Clear();
            foreach (DataRow r in dataTable.Rows)
            {
                try
                {
                    string path_raw = r[col_path].ToString();
                    container.list_inst.Add(TheInstanceContainer.load1Instance_fromPath(path_raw));
                }
                catch (Exception ex) { TheSys.showError(ex); }
            } 
        }

    }
}
