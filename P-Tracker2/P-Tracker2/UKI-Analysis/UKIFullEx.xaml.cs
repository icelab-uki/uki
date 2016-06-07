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

    public partial class UKIFullEx : Window
    {
        public UKIFullEx()
        {
            InitializeComponent();
            excluded_list = GetExcludedMotions_List();
            foreach (ExcludedList ex in excluded_list)
            {
                Console.Write(ex.mid + " :");
                foreach (int i in ex.excluded)
                    Console.Write(" " + i);
                Console.WriteLine();
            }
            resetTable();
        }

        //=== Basic Template ===================================================================================

        string col_id = "id";
        string col_file = "file";
        string col_sid = "subject";
        string col_mid = "motion";
        //string col_mid_predict = "predicted";
        string col_pose_ex = "poses-expect";
        string col_pose = "poses";
        string col_key = "key";
        string col_jump = "jump";

        DataTable dataTable = null;//data table that become datagrid

        void resetTable()
        {
            if (dataTable != null) { dataTable.Clear(); }
            else
            {
                dataTable = new DataTable();
                dataTable.Columns.Add(col_id);
                dataTable.Columns.Add(col_file);
                dataTable.Columns.Add(col_sid);
                dataTable.Columns.Add(col_mid);
                //dataTable.Columns.Add(col_mid_predict);
                dataTable.Columns.Add(col_pose_ex);
                dataTable.Columns.Add(col_pose);
                dataTable.Columns.Add(col_key);
                dataTable.Columns.Add(col_jump);
                setDataGrid(dataTable);
            }
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

        //===============================================================================================
        
        InstanceContainer container = null;//contain data 
        string path_database = TheURL.url_saveFolder + TheURL.url_9_ukiInst;

        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Input: " + path_database);
            TheSys.showError("Output: Evaluation");
        }

        private void butBrowseRaw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                resetTable();
                loadInstance();
                refreshTable();
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        void loadInstance()
        {
            List<int> sid_list = null;
            List<int> mid_list = null;
            if (checkPartial.IsChecked.Value)
            {
                sid_list = TheTool.getSelectRange(textSID.Text);
                mid_list = TheTool.getSelectRange(textMID.Text);
            }
            container = TheInstanceContainer.loadInstanceList_fromDatabase(false, sid_list, mid_list);
        }

        void refreshTable()
        {
            dataTable.Clear();
            if (container.list_inst.Count() > 0)
            {
                int id = 1;
                foreach (Instance inst in container.list_inst)
                {
                    int count = inst.keyPose.Count - 1;
                    if (count < 0) { count = 0; }
                    //--------------
                    dataTable.Rows.Add(id, inst.name,
                        inst.subject_id, 
                        inst.motion_id, 
                        count,
                        TheTool.listArr_getValueById(list_poseCount_expect, inst.motion_id)[1].ToString(),
                        TheTool.getString_fromList(inst.keyPose, "_"),
                        TheTool.getString_fromList(inst.keyPoseJump, "_"));
                    id++;
                }
                butAnalyze.IsEnabled = true;
                butCountPose.IsEnabled = true;
                butCountInst.IsEnabled = true;
            }
            rowCount();
        }

        List<int[]> list_poseCount_expect = new List<int[]>()
        {
            new int[]{1,1}, new int[]{2,1}, new int[]{3,1}, new int[]{4,1}, new int[]{5,1}, 
            new int[]{6,1}, new int[]{7,1}, new int[]{8,1}, new int[]{9,1}, new int[]{10,1}, 
            new int[]{11,1}, new int[]{12,1}, new int[]{13,1}, new int[]{14,1}, new int[]{15,1}, 
            new int[]{16,1}, new int[]{17,1}, new int[]{18,1}, new int[]{19,1}, new int[]{20,1}, 
            new int[]{21,1}, new int[]{22,1}, new int[]{23,2}, new int[]{24,2}, new int[]{25,2},
            new int[]{26,3}, new int[]{27,1}, new int[]{28,3}, new int[]{29,2}, new int[]{30,2}, 
        };

        private void butAnalyze_Click(object sender, RoutedEventArgs e)
        {
            doAnalysis(true);
        }


        private void butLoop_Click(object sender, RoutedEventArgs e)
        {
            //doAnalysis(false);
            //checkLoopMaxTrue.IsChecked = true;
            //doAnalysis(false);
            //checkMinF.IsChecked = true;
            //doAnalysis(false);
            //for (int i = 1; i <= 20; i += 1)
            //{
            //    doAnalysis(false);
            //}

            checkLoopMaxTrue.IsChecked = true;
            checkMinF.IsChecked = true;
            for (int i = 0; i <= 100; i += 2)
            {
                txtTradeOff.Text = i.ToString();
                doAnalysis(false);
            }

            //- Test Partition -----------------------------------
            //for (double d = .01; d <= .20; d += .01)
            //{
            //    txtPartitionRange.Text = d.ToString();
            //    doAnalysis(false);
            //}
            //txtPartitionRange.Text = ".25";
            //doAnalysis(false);
            //- Test Segment -----------------------------------
            //checkReCal.IsChecked = true;
            //checkPoseCapJump.IsChecked = true;
            //for (double d = .01; d <= .5; d += .01)
            //{
            //    txtThreJump.Text = TheTool.getTxt_NumericDigit_FillBy0(d.ToString(),2);
            //    countPose();
            //    exportPoseTable(false);
            //}
            //txtThreStill.Text = "0.05";
            //countPose();
            //exportPoseTable(false);
        }

        void doAnalysis(Boolean showDialog)
        {
            try
            {
                ThePosExtract.capJumping = checkPoseCapJump.IsChecked.Value;
                countPose(false);
                string folderPath = "";
                evaluation_export_part1(out folderPath);
                tobesaved_score_list = new List<FileToBeSaved>();
                matrix_reset();
                analysis_allMotion();
                evaluation_export_part2(folderPath, showDialog);//evaluation & export
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }
        //=====================================================================

        private void butAutoRename_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Rename File by adding Prefix Tag"
                                           , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                TheInstanceContainer.instanceDB_fileRenaming();
            }
        }

        private void butCountPose_Click(object sender, RoutedEventArgs e)
        {
            countPose();
            exportPoseTable(true);
        }

        void countPose()
        {
            ThePosExtract.setThreshold(TheTool.getDouble(txtThreStill), TheTool.getDouble(txtThreJump));
            ThePosExtract.capJumping = checkPoseCapJump.IsChecked.Value;
            countPose(checkReCal.IsChecked.Value);
            ThePosExtract.setThreshold_default();
            if (checkPoseSegExport.IsChecked.Value) { export_PoseSegmentAnalysis(); }
        }

        void export_PoseSegmentAnalysis()
        {
            int count_correct = 0;
            int count_total = 0;
            int jump_T = 0;
            int jump_T_total = 0;
            int jump_F = 0;
            int jump_F_total = 0;
            foreach (DataRow r in dataTable.Rows)
            {
                if (r[col_pose_ex].ToString() == r[col_pose].ToString()) { count_correct++; }
                count_total++;
                if (r[col_mid].ToString() == "1") { 
                    if(r[col_jump].ToString() != ""){jump_T++;}
                    jump_T_total++;
                }
                else{
                    if(r[col_jump].ToString() != ""){ jump_F++;}
                    jump_F_total++;
                }
            }
            //===========================
            List<string> data = new List<string>();
            data.Add("count_correct,count_correct_p,jump_T,jump_T_p,jump_F,jump_F_p");
            data.Add(
                count_correct + "," +
                Math.Round((double)count_correct / count_total, 2) + "," +
                jump_T + "," +
                Math.Round((double)jump_T / jump_T_total, 2) + "," +
                jump_F + "," +
                Math.Round((double)jump_F / jump_F_total, 2)
                );
            //----------------
            string filename = TheURL.url_saveFolder + "PostureSegment" + getPoseCount_fileSuffix();
            TheTool.exportCSV_orTXT(filename, data, false);
        }

        string getPoseCount_fileSuffix()
        {
            string suffix = " s=" + txtThreStill.Text;
            if (checkPoseCapJump.IsChecked.Value)
            {
                suffix += " j=" + txtThreJump.Text;
            }
            suffix += ".csv";
            return suffix;
        }

        void countPose(Boolean Recompute)
        {
            try
            {
                foreach (Instance inst in container.list_inst)
                {
                    inst.getKeyPose(Recompute, checkSaveKey.IsChecked.Value);
                }
                refreshTable();
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        private void butCountInst_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (int sid in container.list_sid)
                {
                    List<Instance> selected_Subject = TheInstanceContainer.getInst_bySubject(container.list_inst, sid, true);
                    foreach (int mid in container.list_mid)
                    {
                        List<Instance> inst_true = TheInstanceContainer.getInst_byMotion(selected_Subject, mid, true);
                        int count = inst_true.Count;
                        if (count != 3) { TheSys.showError("S" + sid + "-" + "M" + mid + " : " + count); }
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        private void butExport_Click(object sender, RoutedEventArgs e)
        {
            exportPoseTable(true);
        }

        void exportPoseTable(Boolean showDialog)
        {
            string filename = TheURL.url_saveFolder + "PostureCount" + getPoseCount_fileSuffix();
            TheTool.export_dataTable_to_CSV(filename, dataTable);
            if (showDialog) { System.Windows.MessageBox.Show(@"Save to '" + TheURL.url_saveFolder + "PostureCount.csv'", "Export Files"); }
        }

        private void textSID_TextChanged(object sender, TextChangedEventArgs e)
        {
            TheTool.removeSpace(textSID);
        }

        //====================================================================
        List<SID_model> sid_model_list = new List<SID_model>();//list contain all model, seperate by subject

        //1 Motion
        m_Motion temp_motion_Greedy = new m_Motion() { };
        m_Motion temp_motion_MIFS = new m_Motion() { };
        m_Motion temp_motion_CMIM = new m_Motion() { };
        m_Motion temp_motion_JMIM = new m_Motion() { };
        m_Motion temp_motion_DCFS = new m_Motion() { };

        void model_AddMotion(SID_model sid_model)
        {
            sid_model.motion_set[0].Add(temp_motion_Greedy);
            sid_model.motion_set[1].Add(temp_motion_MIFS);
            sid_model.motion_set[2].Add(temp_motion_CMIM);
            sid_model.motion_set[3].Add(temp_motion_JMIM);
            sid_model.motion_set[4].Add(temp_motion_DCFS);
        }

        int sid_current = 0;
        int mid_current = 0;

        void analysis_allMotion()
        {
            sid_model_list.Clear();
            foreach (int sid in container.list_sid)
            {
                this.sid_current = sid;
                SID_model sid_model = new SID_model() { sid = sid };
                List<Instance> selected_Subject = TheInstanceContainer.getInst_bySubject(container.list_inst, sid, true);
                foreach (int mid in container.list_mid)
                {
                    this.mid_current = mid;
                    List<Instance> inst_true = TheInstanceContainer.getInst_byMotion(selected_Subject, mid, true);
                    List<Instance> inst_false = TheInstanceContainer.getInst_byMotion(selected_Subject, mid, false);
                    analysis_1Motion(inst_true, inst_false);
                    model_AddMotion(sid_model);
                }
                //----- Model for all motion is already built -------
                //Test Model on Training
                prebuild_MatrixA(sid_model, selected_Subject, ref matrix_A_list_onTraining);
                //Test Model on Unseen
                List<Instance> other_Subject = TheInstanceContainer.getInst_bySubject(container.list_inst, sid, false);
                if (checkTestUnseen.IsChecked.Value)
                {
                    prebuild_MatrixA(sid_model, other_Subject, ref matrix_A_list_onUnseen);
                }
                sid_model_list.Add(sid_model);
            }
        }

        void analysis_1Motion(List<Instance> inst_true, List<Instance> inst_false)
        {
            string motion_name = mid_current.ToString();
            //Keep complete Motion Rule (for whole motion)
            temp_motion_Greedy = new m_Motion() { name = motion_name };
            temp_motion_MIFS = new m_Motion() { name = motion_name };
            temp_motion_CMIM = new m_Motion() { name = motion_name };
            temp_motion_JMIM = new m_Motion() { name = motion_name };
            temp_motion_DCFS = new m_Motion() { name = motion_name };
            //-----
            ModelGenerator model_generator = new ModelGenerator();
            model_generator.process00_setting(inst_true, inst_false, true,
                checkLoopMaxTrue.IsChecked.Value, checkMinF.IsChecked.Value,
                TheTool.getInt(txtTradeOff), checkExtraFeature.IsChecked.Value, 
                checkOptThre.IsChecked.Value,
                checkAvoidGabage.IsChecked.Value, 
                TheTool.getDouble(txtPartitionRange)
                );
            List<DataTable> dt_centered_concat_list = new List<DataTable>();
            List<DataTable> dt_threshold = new List<DataTable>();
            UKI_ThePreprocessor.preprocess_CombinedSegmented(inst_true, checkExtraFeature.IsChecked.Value , ref dt_centered_concat_list, ref dt_threshold);
            int[] mode = UKI_ThePreprocessor.temp_mode;
            int pose_num = 1;//poseture number
            foreach (DataTable dt_1posture in dt_centered_concat_list)
            {
                TheTool.export_dataTable_to_CSV(TheURL.url_saveFolder + "test.csv", dt_1posture);
                //txtPartitionRange
                model_generator.process01_selectFirstFeature(checkAlgo_Entropy.IsChecked.Value, checkAlgo_DCFS.IsChecked.Value
                    , dt_1posture, dt_threshold[0], dt_threshold[pose_num]);
                //----
                List<m_If> temp_posture_current;
                List<string> score_regression_summary = new List<string>();
                //
                if (checkAlgo_Entropy.IsChecked.Value)
                {
                    temp_posture_current = analysis_1Posture(model_generator, TheEntropy.methodSelection_Greedy);
                    temp_model_Greedy_count_rule += temp_posture_current.Count();
                    temp_model_Greedy_count_pose++;
                    TheModelGenerator.motionModel_add1Posture(temp_motion_Greedy, temp_posture_current);
                    score_regression_summary.Add("f_num,new_feature,threshold,Greedy_Total,Greedy_T,Greedy_F");
                    score_regression_summary.AddRange(model_generator.score_regression_summary);
                    //
                    temp_posture_current = analysis_1Posture(model_generator, TheEntropy.methodSelection_MIFS);
                    temp_model_MIFS_count_rule += temp_posture_current.Count();
                    temp_model_MIFS_count_pose++; 
                    TheModelGenerator.motionModel_add1Posture(temp_motion_MIFS, temp_posture_current);
                    score_regression_summary.Add("");
                    score_regression_summary.Add("f_num,new_feature,threshold,MIFS_Total,MIFS_T,MIFS_F");
                    score_regression_summary.AddRange(model_generator.score_regression_summary);
                    //
                    temp_posture_current = analysis_1Posture(model_generator, TheEntropy.methodSelection_CMIM);
                    temp_model_CMIM_count_rule += temp_posture_current.Count();
                    temp_model_CMIM_count_pose++; 
                    TheModelGenerator.motionModel_add1Posture(temp_motion_CMIM, temp_posture_current);
                    score_regression_summary.Add("");
                    score_regression_summary.Add("f_num,new_feature,threshold,CMIM_Total,CMIM_T,CMIM_F");
                    score_regression_summary.AddRange(model_generator.score_regression_summary);
                    //
                    temp_posture_current = analysis_1Posture(model_generator, TheEntropy.methodSelection_JMIM);
                    temp_model_JMIM_count_rule += temp_posture_current.Count();
                    temp_model_JMIM_count_pose++; 
                    TheModelGenerator.motionModel_add1Posture(temp_motion_JMIM, temp_posture_current);
                    score_regression_summary.Add("");
                    score_regression_summary.Add("f_num,new_feature,threshold,JMIM_Total,JMIM_T,JMIM_F");
                    score_regression_summary.AddRange(model_generator.score_regression_summary);
                }
                if (checkAlgo_DCFS.IsChecked.Value)
                {
                    temp_posture_current = analysis_1Posture(model_generator, TheEntropy.methodSelection_DCFS);
                    temp_model_DCFS_count_rule += temp_posture_current.Count();
                    temp_model_DCFS_count_pose++;
                    TheModelGenerator.motionModel_add1Posture(temp_motion_DCFS, temp_posture_current);
                    score_regression_summary.Add("");
                    score_regression_summary.Add("f_num,new_feature,threshold,DCFS_Total,DCFS_T,DCFS_F");
                    score_regression_summary.AddRange(model_generator.score_regression_summary);
                }
                //
                FileToBeSaved newFile_score = new FileToBeSaved();
                newFile_score.path = "{S" + sid_current + "}{M" + mid_current + "_" + pose_num + "} score.csv";
                newFile_score.data = new List<string>();
                newFile_score.data.AddRange(score_regression_summary);
                tobesaved_score_list.Add(newFile_score);
                //
                pose_num++;
            }
            FileToBeSaved newFile_threshold = new FileToBeSaved();
            newFile_threshold.path = "{S" + sid_current + "}{M" + mid_current + "} threshold.csv";
            newFile_threshold.data = TheTool.dataTable_CombineShuffle_getListString(dt_threshold);
            tobesaved_threshold_list.Add(newFile_threshold);
        }

        List<FileToBeSaved> tobesaved_score_list = new List<FileToBeSaved>();
        List<FileToBeSaved> tobesaved_threshold_list = new List<FileToBeSaved>();
        
        List<m_If> analysis_1Posture(ModelGenerator model_generator, int algo)
        {
            List<Feature> list_S0 = model_generator.process02_selectCandidate("", algo, 0);
            return TheMapData.convertfeaturelist_to_ListIf(list_S0);
        }

        string path_suffix = "";

        string getFolderName()
        {
            string folderPath = TheURL.url_saveFolder + "DB Analysis";
            if (checkPartial.IsChecked.Value)
            {
                folderPath += " (M" + textMID.Text + ") (S" + textSID.Text + ")";
            }
            else { folderPath += " (All)"; }
            //------
            path_suffix = "";
            int tag = 0;
            if (checkLoopMaxTrue.IsChecked.Value)
            {
                path_suffix += "MaxTRUE"; tag++;
            }
            if (checkMinF.IsChecked.Value)
            {
                if (tag > 0) { path_suffix += ", "; }
                path_suffix += "MinNum"; tag++;
                int t = TheTool.getInt(txtTradeOff);
                if (t > 0) { path_suffix += "(t=" + t + ")"; }
            }
            if (checkExtraFeature.IsChecked.Value)
            {
                if (tag > 0) { path_suffix += ", "; }
                path_suffix += "ExtraF"; tag++;
            }
            if (checkOptThre.IsChecked.Value)
            {
                if (tag > 0) { path_suffix += ", "; }
                path_suffix += "OptThre"; tag++;
                if (checkAvoidGabage.IsChecked.Value)
                {
                    path_suffix += "-G";
                }
            }
            //=================================================
            path_suffix += " (p=" + TheTool.getTxt_NumericDigit_FillBy0(TheTool.getDouble(txtPartitionRange).ToString(), 2) + ")";
            //-----------------
            if (path_suffix != "") { folderPath += " " + path_suffix; }
            return folderPath;
        }

        void evaluation_export_part1(out string folderPath)
        {
            folderPath = getFolderName();
            TheTool.Folder_CreateIfMissing(folderPath);
            TheTool.SaveScreen_activeWindow(folderPath + @"\" + DateTime.Now.ToString("MMdd_HHmmss") + ".png", false);
        }

        void evaluation_export_part2(string folderPath, Boolean showDialog)
        {
            export_MotionData(folderPath);
            export_MatrixData(folderPath + @"\Acc Training", matrix_A_list_onTraining, false);
            if (checkTestUnseen.IsChecked.Value && container.list_mid.Count > 1)
            {
                export_MatrixData(folderPath + @"\Acc Unseen", matrix_A_list_onUnseen, true);
                //
                List<DataTable> dataTable = new List<DataTable>();
                for(int i = 0 ; i < matrix_file_quality.Count(); i++)
                {
                    if (matrix_file_quality[i] != null && matrix_file_quality[i].Rows.Count > 0) { dataTable.Add(matrix_file_quality[i]); }
                }
                DataTable dt_quality = TheTool.dataTable_combineValue(dataTable, new List<int> { 0, 1, 2 });
                dt_quality = TheTool.dataTable_sort(dt_quality,"MID",false);
                DataTable dt_quality_TF = getDataTable_FileQualityTF(ref dt_quality);
                TheTool.export_dataTable_to_CSV(folderPath + @"\file quality.csv", dt_quality);
                TheTool.export_dataTable_to_CSV(folderPath + @"\file quality TF.csv", dt_quality_TF);
            }
            export_countRule(folderPath);
            if (showDialog) { System.Windows.MessageBox.Show(@"Save to '" + folderPath + "'", "Export Files"); }
        }

        void export_countRule(string folderPath)
        {
            string header = "";
            string data = "";
            if (checkAlgo_Entropy.IsChecked.Value) {
                header += "Greedy_rules,Greedy_poses,MIFS_rules,MIFS_poses,CMIM_rules,CMIM_poses,JMIM_rules,JMIM_poses";
                data += temp_model_Greedy_count_rule + "," +
                    temp_model_Greedy_count_pose + "," +
                    temp_model_MIFS_count_rule + "," +
                    temp_model_MIFS_count_pose + "," +
                    temp_model_CMIM_count_rule + "," +
                    temp_model_CMIM_count_pose + "," +
                    temp_model_JMIM_count_rule + "," +
                    temp_model_JMIM_count_pose;
            }
            if (checkAlgo_DCFS.IsChecked.Value) {
                if (header != "") { header += ","; data += ","; }
                header += "DCFS_rules,DCFS_poses";
                data += temp_model_DCFS_count_rule + "," + temp_model_DCFS_count_pose;
            }
            List<string> export_data = new List<string>();
            export_data.Add(header); export_data.Add(data);
            TheTool.exportCSV_orTXT(folderPath + @"\counter " + path_suffix + ".csv", export_data, false);
        }

        void export_MotionData(string folderPath)
        {
            string folderPath_sub_1 = folderPath + @"\Map";
            TheTool.Folder_CreateIfMissing(folderPath_sub_1);
            int algo_start = 0; int algo_end = algo.Count();
            if (checkAlgo_Entropy.IsChecked.Value == false) { algo_start = 3;}
            if (checkAlgo_DCFS.IsChecked.Value == false) { algo_end = 3; }
            for (int i = algo_start; i < algo_end; i++)
            {
                foreach (SID_model s in sid_model_list)
                {
                    string filename = folderPath_sub_1 + @"\" + algo[i] + " model (S" + TheTool.getTxt_Numeric_FillBy0(s.sid.ToString(), 2) + ").xml";
                    TheMapData.saveXML_Motion(filename, s.motion_set[i]);
                }
            }
            export_toBeSave(folderPath_sub_1 + @"\log score", tobesaved_score_list);
            export_toBeSave(folderPath_sub_1 + @"\log threshold", tobesaved_threshold_list);
        }

        void export_toBeSave(string folderPath, List<FileToBeSaved> tobesaved_file_list)
        {
            TheTool.Folder_CreateIfMissing(folderPath);
            foreach (FileToBeSaved file in tobesaved_file_list)
            {
                TheTool.exportCSV_orTXT(folderPath + @"\" + file.path, file.data, false);
            }
        }

        //Testing on Unseen data only
        void export_MatrixData(string folderPath, List<string>[] matrix_A_list, Boolean unseenTesting)
        {
            prepare_Matrix_D();
            int algo_start = 0; int algo_end = algo.Count();
            if (checkAlgo_Entropy.IsChecked.Value == false) { algo_start = 3; }
            if (checkAlgo_DCFS.IsChecked.Value == false) { algo_end = 3; }
            for (int i = algo_start; i < algo_end; i++)
            {
                string folderPath_sub = folderPath + @"\" + algo[i];
                TheTool.Folder_CreateIfMissing(folderPath_sub);
                export_matrix_A(folderPath_sub, i, matrix_A_list, unseenTesting);// matrix A is built as list during Rule Generation
                export_matrix_B(folderPath_sub, i);// Fully built from A
                export_matrix_C(folderPath_sub, i, false, unseenTesting);// Fully built from B, 1 row of matrix D is built in C
                export_matrix_C(folderPath_sub, i, true, unseenTesting);
            }
            export_matrix_D(folderPath, unseenTesting);
        }

        void prepare_Matrix_D()
        {
            matrix_D.Clear(); 
            matrix_Dp.Clear();
            matrix_D_withExclusion.Clear();
            matrix_Dp_withExclusion.Clear(); 
            matrix_D.Add("Algorithm,True,False,True_total,False_total");
            matrix_Dp.Add("Algorithm,True,False"); 
            matrix_D_withExclusion.Add("Algorithm,True,False,True_total,False_total");
            matrix_Dp_withExclusion.Add("Algorithm,True,False");
        }

        void export_matrix_D(string folderPath, Boolean unseenTesting)
        {
            string suffix = " " + path_suffix;
            string prefix = @"\";
            if (unseenTesting) { prefix += "(u)"; }
            TheTool.exportCSV_orTXT(folderPath + prefix + "(ex) overall" + suffix + ".csv", matrix_D_withExclusion, false);
            TheTool.exportCSV_orTXT(folderPath + prefix + "(ex) overall (p)" + suffix + ".csv", matrix_Dp_withExclusion, false);
            if (unseenTesting) { prefix += " "; }
            TheTool.exportCSV_orTXT(folderPath + prefix + "overall" + suffix + ".csv", matrix_D, false);
            TheTool.exportCSV_orTXT(folderPath + prefix + "overall (p)" + suffix + ".csv", matrix_Dp, false);            
        }

        //All Motion, 1 Algorithm
        void prebuild_MatrixA(SID_model sid_model, List<Instance> selected_Subject, ref List<string>[] matrix_A)
        {
            for (int i = 0; i < sid_model.motion_set.Count(); i++)
            {
                List<string> data = TheRuleTester.buildMatrixA_file(selected_Subject, sid_model.motion_set[i]);
                foreach (string s in data)
                {
                    matrix_A[i].Add(sid_model.sid + "," + s);
                }
            }
        }

        void export_matrix_A(string folderPath, int algo_i, List<string>[] matrix_A_list, Boolean unseenTesting)
        {
            string header = "Model by SID,file,SID,MID";
            string header_sub = "";
            foreach (int m in container.list_mid) { header_sub += "," + m; }
            header += header_sub;
            matrix_A_list[algo_i].Insert(0, header);
            DataTable matrixA_dt = TheTool.convert_List_toDataTable(matrix_A_list[algo_i]);
            matrix_A_dt[algo_i] = matrixA_dt;
            TheTool.export_dataTable_to_CSV(folderPath + @"\" + algo[algo_i] + " matrix A.csv", matrix_A_dt[algo_i]);
            //---- Remove Training
            if (unseenTesting)
            {
                //----- File Quality --------
                DataTable matrixA_dt_copy = matrixA_dt.Copy();
                matrixA_dt_copy.Columns.Remove("Model by SID");
                List<string> file_quality = new List<string>();
                List<string> file_quality_P = new List<string>();
                file_quality.Add("file,SID,MID" + header_sub);
                file_quality_P.Add("file,SID,MID" + header_sub);
                TheTool.dataTable_SumUp_GroupByFirst(matrixA_dt_copy, true, new List<int> { 1, 2 }, ref file_quality, ref file_quality_P);
                TheTool.exportCSV_orTXT(folderPath + @"\" + algo[algo_i] + " file quality.csv", file_quality, false);
                matrix_file_quality[algo_i] = TheTool.convert_List_toDataTable(file_quality);
                //------ New Matrix A -----------
                for (int r = 0; r < matrixA_dt.Rows.Count; r++)
                {
                    DataRow dr = matrixA_dt.Rows[r];
                    if (dr[0].ToString() == dr[2].ToString())
                    {
                        //Model from SubjectX tested with SubjectX
                        for (int c = 4; c < matrixA_dt.Columns.Count; c++)
                        {
                            if (dr[3].ToString() == matrixA_dt.Columns[c].ColumnName)
                            {
                                //Model of MotionX tested with MotionX (training data)
                                dr[c] = "";
                            }
                        }
                    }
                }
            }
        }

        void export_matrix_B(string folderPath, int algo_i)
        {
            //Add Data - Prepare Table
            DataTable dt = matrix_A_dt[algo_i].Copy();
            dt.Columns.Remove("Model by SID");
            dt.Columns.Remove("file");
            dt.Columns.Remove("SID");
            dt = TheTool.dataTable_sortNumeric(dt, "MID");
            //------------
            List<String> list_matrix_B = new List<String>();
            List<String> list_matrix_Bp = new List<String>();
            //Add Header
            String header = "MID";
            foreach (int m in container.list_mid) { header += "," + m; }
            list_matrix_B.Add(header);
            list_matrix_Bp.Add(header);
            //
            TheTool.dataTable_SumUp_GroupByFirst(dt, false, null, ref list_matrix_B, ref list_matrix_Bp);
            matrix_B_dt[algo_i] = TheTool.convert_List_toDataTable(list_matrix_B);
            matrix_Bp_dt[algo_i] = TheTool.convert_List_toDataTable(list_matrix_Bp);
            TheTool.exportCSV_orTXT(folderPath + @"\" + algo[algo_i] + " matrix B.csv", list_matrix_B, false);
            TheTool.exportCSV_orTXT(folderPath + @"\" + algo[algo_i] + " matrix BP.csv", list_matrix_Bp, false);
            //
            matrix_B_dt_withExclusion[algo_i] = build_matrixB_exclusion(matrix_B_dt[algo_i], false);
            matrix_Bp_dt_withExclusion[algo_i] = build_matrixB_exclusion(matrix_Bp_dt[algo_i], false);
            TheTool.export_dataTable_to_CSV(folderPath + @"\(ex) " + algo[algo_i] + " matrix B.csv", matrix_B_dt_withExclusion[algo_i]);
            TheTool.export_dataTable_to_CSV(folderPath + @"\(ex) " + algo[algo_i] + " matrix BP.csv", matrix_Bp_dt_withExclusion[algo_i]);
        }


        //Assumption : 3 instance / motions
        void export_matrix_C(string folderPath, int algo_i, Boolean withExclusion, Boolean unseenTesting)
        {
            int inst_perMotion = 3;
            List<string> list_matrix_C = new List<string>();
            List<string> list_matrix_CP = new List<string>();
            list_matrix_C.Add("MID,True,False,True_total,False_total");
            list_matrix_CP.Add("MID,True,False");
            DataTable dt = matrix_B_dt[algo_i];
            if (withExclusion) { dt = matrix_B_dt_withExclusion[algo_i]; }
            //-- Matrix C ---
            int sum_true_total = 0;
            int sum_false_total = 0;
            int sum_true_count = 0;
            int sum_false_count = 0;
            foreach (DataRow r in dt.Rows)
            {
                string mid_target = r[0].ToString();
                int count_subject = container.list_sid.Count();
                int count_motion = container.list_mid.Count();
                int count_motionExcluded = 0;
                if (withExclusion)
                {
                    foreach (int mid_ex in getExcluded_forSingleMotion(TheTool.getInt(mid_target)))
                    {
                        if (container.list_mid.Contains(mid_ex)) { count_motionExcluded++; }
                    }
                }
                //--------------
                int true_total = 0;
                int false_total = 0;
                if (unseenTesting)
                {
                    true_total = inst_perMotion * count_subject * (count_subject - 1);//test on 3 training inst of other subject
                    false_total = inst_perMotion * count_subject * count_subject * (count_motion - count_motionExcluded - 1);
                }
                else{
                    true_total = inst_perMotion * count_subject;//test on 3 training inst of this subject
                    false_total = inst_perMotion * count_subject * (count_motion - count_motionExcluded - 1);// 3 inst x 29 motion
                }
                //--------------
                int true_count = 0;
                int false_count = 0;
                int c_i = 0;
                foreach (DataColumn c in dt.Columns)
                {
                    if (c_i > 0)
                    {
                        string mid_this = c.ColumnName;
                        int v = TheTool.getInt(r[c].ToString());
                        //Check TRUE FALSE
                        if (mid_target == mid_this) { true_count += v; }
                        else { false_count += v; }
                    }
                    c_i++;
                }
                int count_col_false = count_motion - 1;
                double true_chance = 0;
                if (true_total > 0) { true_chance = (double)true_count / true_total; }
                double false_chance = 0;
                if (false_total > 0) { false_chance = (double)false_count / false_total; }
                list_matrix_C.Add(mid_target + "," + true_count + "," + false_count + "," + true_total + "," + false_total);
                list_matrix_CP.Add(mid_target + "," + true_chance + "," + false_chance);
                sum_true_count += true_count;
                sum_false_count += false_count; 
                sum_true_total += true_total;
                sum_false_total += false_total;
            }
            //-- Matrix D-----------------
            double sum_true_chance = 0;
            if (sum_true_total > 0) { sum_true_chance = (double)sum_true_count / sum_true_total; }
            double sum_false_chance = 0;
            if (sum_false_total > 0) { sum_false_chance = (double)sum_false_count / sum_false_total; }
            if (withExclusion)
            {
                TheTool.exportCSV_orTXT(folderPath + @"\(ex) " + algo[algo_i] + " matrix C.csv", list_matrix_C, false);
                TheTool.exportCSV_orTXT(folderPath + @"\(ex) " + algo[algo_i] + " matrix CP.csv", list_matrix_CP, false);
                matrix_D_withExclusion.Add(algo[algo_i] + "," + sum_true_count + "," + sum_false_count + "," + sum_true_total + "," + sum_false_total);
                matrix_Dp_withExclusion.Add(algo[algo_i] + "," + sum_true_chance + "," + sum_false_chance);
            }
            else
            {
                TheTool.exportCSV_orTXT(folderPath + @"\" + algo[algo_i] + " matrix C.csv", list_matrix_C, false);
                TheTool.exportCSV_orTXT(folderPath + @"\" + algo[algo_i] + " matrix CP.csv", list_matrix_CP, false);
                matrix_D.Add(algo[algo_i] + "," + sum_true_count + "," + sum_false_count + "," + sum_true_total + "," + sum_false_total);
                matrix_Dp.Add(algo[algo_i] + "," + sum_true_chance + "," + sum_false_chance);
            }
        }

        public string[] algo = new string[] { "Greedy", "MIFS", "CMIM", "JMIM", "DCFS" };

        public List<String>[] matrix_A_list_onTraining = new List<String>[]{ 
            new List<String>() { }, new List<String>() { },
            new List<String>() { }, new List<String>() { },
            new List<String>() { } };//Greedy,MIFS,CMIM,JMIM

        public List<String>[] matrix_A_list_onUnseen = new List<String>[]{ 
            new List<String>() { }, new List<String>() { },
            new List<String>() { }, new List<String>() { },
            new List<String>() { } };//Greedy,MIFS,CMIM,JMIM

        DataTable[] matrix_A_dt;//file
        DataTable[] matrix_B_dt;//motion
        DataTable[] matrix_Bp_dt;//motion, percent
        DataTable[] matrix_B_dt_withExclusion;//motion
        DataTable[] matrix_Bp_dt_withExclusion;//motion
        //DataTable[] matrix_C;//true & false
        //DataTable[] matrix_Cp;//true & false, percent
        List<String> matrix_D = new List<String>();//Overall
        List<String> matrix_Dp = new List<String>(); //Overall Percent
        List<String> matrix_D_withExclusion = new List<String>();//Overall
        List<String> matrix_Dp_withExclusion = new List<String>(); //Overall Percent

        DataTable[] matrix_file_quality;//file
        //--------

        void matrix_reset()
        {
            matrix_A_list_onTraining = new List<String>[]{ 
                new List<String>() { }, new List<String>() { },
                new List<String>() { }, new List<String>() { },
                new List<String>() { }, };
            matrix_A_list_onUnseen = new List<String>[]{ 
                new List<String>() { }, new List<String>() { },
                new List<String>() { }, new List<String>() { },
                new List<String>() { }, };
            matrix_A_dt= new DataTable[5];
            matrix_B_dt= new DataTable[5];
            matrix_Bp_dt = new DataTable[5];
            matrix_B_dt_withExclusion = new DataTable[5];
            matrix_Bp_dt_withExclusion = new DataTable[5];
            //dt_matrix_C = new DataTable[4];
            //dt_matrix_Cp = new DataTable[4];
            matrix_D = new List<String>();
            matrix_Dp = new List<String>();
            matrix_D_withExclusion = new List<String>();
            matrix_Dp_withExclusion = new List<String>();
            matrix_file_quality = new DataTable[5];
            //-----------
            temp_model_Greedy_count_rule = 0;
            temp_model_MIFS_count_rule = 0;
            temp_model_CMIM_count_rule = 0;
            temp_model_JMIM_count_rule = 0;
            temp_model_DCFS_count_rule = 0;
            temp_model_Greedy_count_pose = 0;
            temp_model_MIFS_count_pose = 0;
            temp_model_CMIM_count_pose = 0;
            temp_model_JMIM_count_pose = 0;
            temp_model_DCFS_count_pose = 0;
        }

        //count rule from whole analysis
        int temp_model_Greedy_count_rule = 0;
        int temp_model_MIFS_count_rule = 0;
        int temp_model_CMIM_count_rule = 0;
        int temp_model_JMIM_count_rule = 0;
        int temp_model_DCFS_count_rule = 0;
        //count rule from whole analysis
        int temp_model_Greedy_count_pose = 0;
        int temp_model_MIFS_count_pose = 0;
        int temp_model_CMIM_count_pose = 0;
        int temp_model_JMIM_count_pose = 0;
        int temp_model_DCFS_count_pose = 0;

        //------------------------------------------------------------------
        public List<ExcludedList> excluded_list = new List<ExcludedList>();//list of motion to be excluded in Score

        public List<ExcludedList> GetExcludedMotions_List()
        {
            List<ExcludedList> list = new List<ExcludedList>();
            ExcludedList Jump = new ExcludedList(1, new List<int>() { 2,3,4,5,6,19,20,21,22 });
            list.Add(Jump);
            ExcludedList Crouch = new ExcludedList(2, new List<int>() { 5, 6 });
            list.Add(Crouch); 
            ExcludedList RightKick = new ExcludedList(20, new List<int>() { 5 });
            list.Add(RightKick);
            ExcludedList LeftKick = new ExcludedList(22, new List<int>() { 6 });
            list.Add(LeftKick); 
            ExcludedList KnifehandStrike = new ExcludedList(23, new List<int>() { 11, 15 });
            list.Add(KnifehandStrike);
            ExcludedList Hadouken = new ExcludedList(24, new List<int>() { 2, 3, 5, 6, 10, 15, 17, 27 });
            list.Add(Hadouken);
            ExcludedList HadoukenAir = new ExcludedList(25, new List<int>() { 2, 3, 5, 6, 10, 15, 17, 11, 13, 24, 27, 30 });
            list.Add(HadoukenAir);
            ExcludedList Shakunetsu = new ExcludedList(26, new List<int>() { 2, 3, 5, 6, 10, 15, 17, 24, 27 });
            list.Add(Shakunetsu);
            ExcludedList TwoHandedPunch = new ExcludedList(27, new List<int>() { 15, 17 });
            list.Add(TwoHandedPunch);
            ExcludedList TriplePunchCombo = new ExcludedList(28, new List<int>() { 15, 17 });
            list.Add(TriplePunchCombo);
            ExcludedList RightSwing = new ExcludedList(29, new List<int>() { 7, 15 });
            list.Add(RightSwing);
            ExcludedList RightUppercut = new ExcludedList(30, new List<int>() { 1,2,12, 11, 15 });
            list.Add(RightUppercut);
            return list;
        }

        //Output: Excluded List
        public List<int> getExcluded_forSingleMotion(int mid_target)
        {
            foreach (ExcludedList exception in excluded_list)
            {
                if (exception.mid == mid_target)
                {
                    return exception.excluded;
                }
            }
            return new List<int>();
        }

        //export_matrix_B_exc(folderPath_sub, i);
        public DataTable build_matrixB_exclusion(DataTable MatrixB, Boolean setZero)
        
        {
            DataTable MatrixB_exc = MatrixB.Copy();
            List<int> tmp;
            try
            {
                int r_i = 0;
                foreach (DataRow row in MatrixB.Rows)
                {

                    int number = -1;
                    int.TryParse(row[0].ToString(), out number);
                    if (number == 0)
                        continue;
                    if ((tmp = getExcluded_forSingleMotion(number)) != null)
                    {
                        int c_i = 0;
                        foreach (DataColumn column in MatrixB_exc.Columns)
                        {
                            int nb = -1;
                            int.TryParse(column.ColumnName, out nb);
                            if (nb == 0)
                                continue;
                            foreach (int value in tmp)
                            {
                                if (value == nb)
                                {
                                    string new_v = "";
                                    if (setZero) { new_v = "0"; }
                                    MatrixB_exc.Rows[r_i][c_i + 1] = new_v;
                                }
                            }
                            c_i++;
                        }
                    }
                    r_i++;
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return MatrixB_exc;
        }

        private void butEditor_Click(object sender, RoutedEventArgs e)
        {
            new Editor("").Show();
        }

        DataTable getDataTable_FileQualityTF(ref DataTable dt_quality)
        {
            DataTable dt_quality_TF = new DataTable();
            dt_quality_TF.Columns.Add("file");
            dt_quality_TF.Columns.Add("SID");
            dt_quality_TF.Columns.Add("MID");
            dt_quality_TF.Columns.Add("True");
            dt_quality_TF.Columns.Add("False");
            int i_r = 0;
            foreach (DataRow dr in dt_quality.Rows)
            {
                int i_c = 0;
                int countT = 0;
                int countF = 0;
                int mid_actual = TheTool.getInt(dr["MID"].ToString());
                List<int> list_exclude = getExcluded_forSingleMotion(mid_actual);
                foreach (DataColumn dc in dt_quality.Columns)
                {
                    if (i_c > 2)
                    {
                        int mid_predict = TheTool.getInt(dc.ColumnName);
                        if (list_exclude.Contains(mid_predict))
                        {
                            //TheSys.showError(dr[dc] + " >> 0");
                            dr[dc] = "";
                        }
                        else
                        {
                            int v = TheTool.getInt(dr[dc].ToString());
                            if (mid_actual == mid_predict)
                            {
                                countT += v;
                            }
                            else { countF += v; }
                        }
                    }
                    i_c++;
                }
                //
                dt_quality_TF.Rows.Add();
                dt_quality_TF.Rows[i_r][0] = dt_quality.Rows[i_r][0];
                dt_quality_TF.Rows[i_r][1] = dt_quality.Rows[i_r][1];
                dt_quality_TF.Rows[i_r][2] = dt_quality.Rows[i_r][2];
                dt_quality_TF.Rows[i_r][3] = countT;
                dt_quality_TF.Rows[i_r][4] = countF;
                i_r++;
            }
            return dt_quality_TF;
        }

        private void checkReCal_Checked(object sender, RoutedEventArgs e)
        {
            checkPoseSegExport.IsChecked = true;
        }

    }

    //====================================================

    class SID_model
    {
        public int sid = 0;

        //All Motions (MAP) :  for Greedy,MIFS,CMIM,JMIM
        public List<m_Motion>[] motion_set = new List<m_Motion>[]{ 
            new List<m_Motion>() { }, new List<m_Motion>() { },
            new List<m_Motion>() { }, new List<m_Motion>() { },
            new List<m_Motion>() { }, };
    }

    public class ExcludedList
    {
        public int mid = 0;//target motion
        public List<int> excluded = new List<int>();//motion to be exluded in score computation

        public ExcludedList(int _mid = 0, List<int> _excluded = null)
        {
            mid = _mid;
            excluded = _excluded;
        }
    }

    


}
