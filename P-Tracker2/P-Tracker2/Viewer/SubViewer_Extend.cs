using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class SubViewer_Extend
    {
        public SubViewer subViewer = null;
        DataTable mainTable = null;

        public int decimalNum = 10;//Base Decimal

        //------------------
        //for Super Normalization
        public List<string> list_fileSave = new List<string>();//List of exported file
        //=============================================
        public int normaliz_method = 0;

        public void getMainTable_byPathTable(DataTable dt)
        {
            try
            {
                resetMainTable();
                int fieldID = 0;
                //----------
                foreach (DataRow r in dt.Rows)
                {
                    mainTable_getEachRecord(fieldID, r[col_path].ToString());
                    fieldID++;
                    if (saveSubTable == true) { save_SubTable(); }
                }
                //----------
                //calSuperNormal();
                //check error
                if (showSample == true) { TheTool.showTable(this.sub_table,"Sample - each file","sample"); }
            }
            catch (Exception e) { TheSys.showError(e.ToString(), true); }
        }

        public Boolean showSample = true;
        public Boolean saveSubTable = false;
        public string filename_current = "unnamed";
        public string folder_forSave = "unnamed";

        public string path_saveRoot = TheURL.url_saveFolder + TheURL.url_9_PAnalysis + @"\";
        void save_SubTable()
        {
            string folderPath = path_saveRoot + folder_forSave;
            TheTool.Folder_CreateIfMissing(folderPath);
            folderPath = folderPath + @"\" + getSubSubFolder();
            TheTool.Folder_CreateIfMissing(folderPath);
            //
            string filePath = folderPath + @"\" + filename_current + ".csv";//Save in Folder
            TheTool.export_dataTable_to_CSV(filePath,sub_table);
            list_fileSave.Add(filePath);
        }

        public string getSubSubFolder()
        {
            string[] words = filename_current.Split('_');
            string word = words[0];
            if (word == "") { word = "unnamed"; }
            return word;
        }
        //=========================================================
        //-----------------------------------------------------------------------


        string col_path = "file_path";
        //
        string col_m_id = "id";
        string col_m_file = "file";
        string col_m_user = "user";
        string col_m_class = "class";
        string col_m_class2 = "class2";
        //
        //string col_m_cAvg = "_C_Avg";
        static string col_suffix_dV = "_Dist_V";
        static string col_suffix_dAvg = "_Dist_Avg";
        static string col_suffix_dMax = "_Dist_Max";
        static string col_suffix_dP75 = "_Dist_P75";
        static string col_suffix_dP90 = "_Dist_P90";

        static string[] suffix = {
                                  col_suffix_dV
                                  ,col_suffix_dMax
                                  ,col_suffix_dAvg
                                  ,col_suffix_dP75
                                  ,col_suffix_dP90
                              };
        //
        string col_m_SumD = "Sum";
        string col_m_DHand = "Hand_LR";
        string col_m_Turn = "Turn";
        //
        //
        string col_s_SumD = "Sum_D";
        string col_s_DHand = "Hand_LR_D";
        string col_s_Turn = "Turn_D";
        //
        static string col_s_suffix_d = "_D";

        void resetMainTable()
        {
            if (mainTable != null) { mainTable.Clear(); }
            mainTable = new DataTable();
            //------------------------
            mainTable.Columns.Add(col_m_id);
            mainTable.Columns.Add(col_m_file);
            mainTable.Columns.Add(col_m_user);
            mainTable.Columns.Add(col_m_class);
            mainTable.Columns.Add(col_m_class2);
            //---- Features ----------
            columnAdd_forJoints(this.mainTable, suffix);
            addCol(this.mainTable, col_m_SumD, suffix);
            //------------------------
            subViewer.setDataGrid(mainTable);
        }


        void columnAdd_forJoints(DataTable dt, string suffix)
        {
            foreach (string a in TheTool.joint_list_upperOnly)
            {
                dt.Columns.Add(a + suffix);
            }
        }

        void columnAdd_forJoints(DataTable dt, string[] suffix)
        {
            foreach (string a in TheTool.joint_list_upperOnly)
            {
                addCol(dt, a, suffix);
            }
        }

        void addCol(DataTable dt, string a, string[] suffix)
        {
            foreach (string b in suffix)
            {
                dt.Columns.Add(a + b);
            }
        }

        //=============================================================
        //=============================================================
        DataTable sub_table = null;

        void mainTable_getEachRecord(int fileID, string path)
        {
            try
            {
                //=========== Main Table =====================
                TheTool_Stat.processing_file = path;//tell which file is in process, for Error report
                mainTable.Rows.Add();
                mainTable.Rows[fileID][col_m_id] = fileID.ToString();
                filename_current = TheTool.getFileName_byPath(path);
                mainTable.Rows[fileID][col_m_file] = filename_current;
                dataTable_calColumn_Class_oth(fileID, filename_current);
                //=========== cal from Subtable =============
                //---- create Sub Table for each file -------
                this.sub_table = CSVReader.ReadCSVFile(path, true);
                //---- Normalize ----------
                subTable_normalize();
                //-------------------------
                subTable_calJointEuclidian(fileID, sub_table);
            }
            catch (Exception e) { TheSys.showError("getEachRec: " + e.ToString(),true); }
        }

        void dataTable_calColumn_Class_oth(int fileID, string filename)
        {
            mainTable.Rows[fileID][col_m_user] = TheTool.spec_getUser_fromFileName(filename);
            //============================================
            string class1 = ""; string class2 = "";
            if (filename.Contains("MM")) { class1 = "MM"; class2 = "M"; }
            else if (filename.Contains("M")) { class1 = "M"; class2 = "M"; }
            else if (filename.Contains("SS")) { class1 = "S"; class2 = "S"; }
            else if (filename.Contains("S")) { class1 = "SS"; class2 = "S"; }
            mainTable.Rows[fileID][col_m_class] = class1;
            mainTable.Rows[fileID][col_m_class2] = class2;
        }

        void subTable_normalize()
        {
            if (normaliz_method == 1)
            {
                List<string> col_list = TheTool.getListJointXYZ();
                TheTool_Stat.normalize_table_MinMax(this.sub_table, col_list, decimalNum);
            }
        }


        void subTable_calJointEuclidian(int fileID, DataTable dt)
        {
            try
            {
                //=================================================
                //============= Write Sub Table ==================
                //build Column First
                columnAdd_forJoints(dt, col_s_suffix_d);
                //======================================================
                //------ Variable For cal Euclidian Distance
                double[] lastXYZ = { 0, 0, 0 }; double[] currentXYZ = { 0, 0, 0 };
                List<double> list_D;
                double dist;
                //======================================================
                foreach (string joint in TheTool.joint_list_upperOnly)//Column first method
                {
                    list_D = new List<double> { };
                    if (normaliz_method == 4) { TheMinMaxNormaliz.getMinMax(joint, ref temp_Dist_min, ref temp_Dist_max); }
                    //------ Cal AvgC -----------------------------
                    //double avg_C = TheTool_Stat.calAvg_byCol(dt, joint + "_c", decimalNum);
                    //dataTable.Rows[fileID][joint + col_m_cAvg] = avg_C;
                    //------ Cal Joint D -----------------------------
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        //------ Cal Euclidian --------------------------------
                        currentXYZ = TheTool.getDouble_fromJoint(dt, row, joint);
                        if (row == 0) { dt.Rows[row][joint + col_s_suffix_d] = "0"; }
                        else
                        {
                            dist = TheTool_Stat.calEuclidean(currentXYZ, lastXYZ, decimalNum);
                            if (normaliz_method == 4) { dist = TheTool_Stat.calMinMaxNormalize(dist, temp_Dist_min, temp_Dist_max); }
                            dt.Rows[row][joint + col_s_suffix_d] = dist;
                            list_D.Add(dist);
                        }
                        lastXYZ = currentXYZ;
                    }
                    //=================================================
                    //============= Write Main Table ==================
                    calMainTable(list_D, fileID, joint);
                }
            }
            catch { }
        }


        //------ Temp Variable for Z-Score
        double v_mean; double v_sd;
        double v_variance;
        double v_p75; double v_p90;

        //baseName = "Head"
        void calMainTable(List<double> list_D, int fileID, string baseName)
        {
            //------ For cal Distance Z-Score
            v_mean = TheTool_Stat.calMean(list_D);
            v_mean = list_D.Average();
            mainTable.Rows[fileID][baseName + col_suffix_dAvg] = v_mean.ToString();
            //
            v_variance = TheTool_Stat.calVariance(list_D, v_mean);
            mainTable.Rows[fileID][baseName + col_suffix_dV] = v_variance;
            //
            mainTable.Rows[fileID][baseName + col_suffix_dMax] = list_D.Max().ToString();
            //
            v_sd = Math.Sqrt(v_variance);
            v_p75 = (0.674 * v_sd) + v_mean;
            mainTable.Rows[fileID][baseName + col_suffix_dP75] = v_p75.ToString();
            v_p90 = (1.282 * v_sd) + v_mean;
            mainTable.Rows[fileID][baseName + col_suffix_dP90] = v_p90.ToString();
        }

        double calSumDist(DataTable dt, int row)
        {
            double sum = 0;
            try
            {
                foreach (string a in TheTool.joint_list_upperOnly)
                {
                    sum += double.Parse(dt.Rows[row][a + col_s_suffix_d].ToString());
                }
            }
            catch (Exception e) { TheSys.showError("Err: [SumDist r" + row + "]" + e.ToString(), true); }
            return sum;
        }

        //***********************************************************************
        //******************** Super ************************************

        double temp_Dist_min = 0;
        double temp_Dist_max = 0;



    }
}
