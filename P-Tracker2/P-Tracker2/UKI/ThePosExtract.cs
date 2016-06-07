using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace P_Tracker2
{
    class ThePosExtract
    {
        public static Boolean capJumping = false;
        public static double threshold_minmove = 0.20;//20% of range
        public static double jump_threshold = 0.10;

        public static void setThreshold_default(){
            threshold_minmove = 0.20;
            jump_threshold = 0.10;
        }

        public static void setThreshold(double threshold_minmove0, double jump_threshold0)
        {
            threshold_minmove = threshold_minmove0;
            jump_threshold = jump_threshold0;
        }

        public static List<string> temp_list_jump = new List<string>();//list of keypose that jump is detected e.g. 1(spine_Y)

        //can Min Max, return row id at Extracted
        //also adjust UKI_DataMovement
        public static List<int> calMinimaMaxima(List<UKI_DataMovement> list_movement)
        {
            temp_list_jump = new List<string>();
            List<int> list_PostureKey_ID = new List<int>();
            try
            {
                //-- check Min Max value ----------------------------
                double d = 0;//current
                double d_min = 0;//must be > 0 after process
                double d_max = 0;
                //
                foreach (UKI_DataMovement u in list_movement)
                {
                    d = u.ms_all_avg;
                    if (d_min == 0) { d_min = d; }
                    else if (d < d_min) { d_min = d; }
                    if (d > d_max) { d_max = d; }
                }
                double d_range = d_max - d_min;
                double d_minmove = d_range * threshold_minmove;
                double d_lowerBound = d_min + d_minmove;//at Percentile 20 from Global Min
                //-- fin Min Max location ---------------------------
                List<int> list_minmax_id_list = new List<int>();//index of all detected (even = Maxima, odd = Minima) 
                Boolean findMax = true;// true = find Max , false = find Min
                Boolean goUp_current = true;//current direction
                Boolean goUp_last = true;//last scaned direction
                double value_current = 0;//curren scaned unit
                double value_last = 0;//last scaned unit
                double value_change = 0;
                double lastAcp_MM = 0;//last accepted Minima / Maxima
                double diff_fromLastMM = 0;//change from last accepted Minima / Maxima
                //
                foreach (UKI_DataMovement u in list_movement)
                {
                    value_current = u.ms_all_avg;
                    value_change = value_current - value_last;
                    //find Direction
                    if (value_change >= 0) { goUp_current = true; }
                    else { goUp_current = false; }
                    //-----------
                    if (goUp_current != goUp_last) //found change in Direction
                    {
                        diff_fromLastMM = value_last - lastAcp_MM;
                        if (findMax && goUp_current == false && diff_fromLastMM > d_minmove && value_current > d_lowerBound)
                        {
                            //found Max
                            list_minmax_id_list.Add(u.id - 1);// -1 because we collect last data before change
                            lastAcp_MM = value_last;
                            findMax = false;
                        }
                        else if (findMax == false && goUp_current && diff_fromLastMM < -d_minmove)
                        {
                            //found Min
                            list_minmax_id_list.Add(u.id - 1);
                            lastAcp_MM = value_last;
                            findMax = true;
                        }
                        else if (list_minmax_id_list.Count > 0)
                        {
                            //finding Min but found higher Maxima
                            if (!findMax && value_last > lastAcp_MM)
                            {
                                list_minmax_id_list.RemoveAt(list_minmax_id_list.Count - 1);
                                list_minmax_id_list.Add(u.id - 1);
                                lastAcp_MM = value_last;
                            }
                            else if (findMax && value_last < lastAcp_MM
                                && diff_fromLastMM < -d_minmove)// (prevent slow down)
                            {
                                //finding Max but found lower Minima
                                list_minmax_id_list.RemoveAt(list_minmax_id_list.Count - 1);
                                list_minmax_id_list.Add(u.id - 1);
                                lastAcp_MM = value_last;
                            }
                        }
                    }
                    else if (findMax == false && value_current < d_lowerBound)
                    {
                        //special case : when finding Minima and go below Percentile 20
                        list_minmax_id_list.Add(u.id);
                        lastAcp_MM = value_current;
                        findMax = true;
                    }
                    goUp_last = goUp_current;
                    value_last = value_current;
                }
                //-- fin Mid_M ---------------------------------------------------------------
                List<int> mid = new List<int>();
                double m_max = 0; double m_min = 0;
                int m_i = 0; int m_v;
                foreach (double m in list_minmax_id_list)
                {
                    if (m_i % 2 == 0) { m_max = m; }
                    else
                    {
                        m_min = m;
                        m_v = (int)(m_max + m_min) / 2;
                        mid.Add(m_v);
                    }
                    m_i++;
                }
                list_minmax_id_list.AddRange(mid);
                list_minmax_id_list.Sort();
                //===========================================================================
                //-- Adjust Movement Data & built Minama List -------------------------------
                int[] list_minmax_id_arr = list_minmax_id_list.ToArray();
                int mm_pointed = 0;
                int mm_max = list_minmax_id_arr.Count();
                Boolean skipMin = false;
                foreach (UKI_DataMovement u in list_movement)
                {
                    if (mm_max > 0 && u.id == list_minmax_id_arr[mm_pointed])
                    {
                        if (mm_pointed % 3 == 0) { 
                            if (u.spine_Y > jump_threshold)
                            {
                                u.type = 0;
                                if (capJumping)
                                {
                                    //TheSys.showError("Jump " + u.spine_Y);
                                    skipMin = true;
                                    list_PostureKey_ID.Add(u.id);
                                    temp_list_jump.Add(u.id + ":Y" + Math.Round(u.spine_Y,2));
                                }
                            }
                            else
                            {
                                u.type = 1;
                            }
                        }
                        else if (mm_pointed % 3 == 1) { u.type = 0.5; }
                        else 
                        { 
                            u.type = -1;
                            if (skipMin) { skipMin = false; }
                            else {
                                list_PostureKey_ID.Add(u.id); 
                            }
                        }
                        //--------
                        if (mm_pointed < mm_max - 1) { mm_pointed++; };
                    }
                }
            }
            catch (Exception ex) { TheSys.showError("calMinMax: " + ex.ToString()); }
            return list_PostureKey_ID;
        }

        public static List<string> log_BasicPostureAnalysis = new List<string>();//for show summary
        //canonical = useBasePose , if not use previous
        public static void BasicPostureAnalysis(List<UKI_Data_BasicPose> data_bp_selected, Boolean useBasePose)
        {
            try
            {
                log_BasicPostureAnalysis = new List<string>();
                //--- Collect Header "Key Postures: 0,42,82,115,173,439" ---------------
                string s = "Key Postures: "; int a = 0;
                foreach (UKI_Data_BasicPose bp in data_bp_selected)
                {
                    if (a > 0) { s += ","; }
                    s += bp.id; 
                    a++;
                }
                log_BasicPostureAnalysis.Add(s);
                if (useBasePose) { log_BasicPostureAnalysis.Add("Canonical: Ready Stance"); }
                else { log_BasicPostureAnalysis.Add("Canonical: (Previous Posture)"); }
                log_BasicPostureAnalysis.Add("");
                //--- Colelct Data -------------------------------------------------------
                if (data_bp_selected.Count > 1)
                {
                    UKI_Data_BasicPose[] arr_bp = data_bp_selected.ToArray();
                    UKI_Data_BasicPose canonical = arr_bp[0];
                    for (int p_num = 1; p_num < arr_bp.Count(); p_num++)
                    {
                        if (!useBasePose) { canonical = arr_bp[p_num - 1]; }//previos as canonical
                        log_BasicPostureAnalysis.Add("Pose " + p_num);
                        List<String> summary_human = new List<String>();//summary in human language
                        List<String> summary_code = new List<String>();//summary in code
                        for (int k = 0; k < arr_bp[p_num].basic_pose.Count(); k++)
                        {
                            int v_canonical = canonical.basic_pose[k];
                            int v_current = arr_bp[p_num].basic_pose[k];
                            if (v_current != v_canonical)
                            {
                                string v_name = TheUKI.data_bp_name[k];
                                summary_human.Add(TheTool.string_Tab(1) + TheMapData.convert_getBasePoseDef(v_name, v_current));
                                summary_code.Add(TheTool.string_Tab(2) + v_name + ": " + v_canonical + " -> " + v_current);
                            }
                        }
                        if (summary_human.Count == 0)
                        {
                            summary_human.Add(TheTool.string_Tab(1) + "Ready Stance");
                        }
                        foreach (string txt in summary_human) { log_BasicPostureAnalysis.Add(txt); }
                        foreach (string txt in summary_code) { log_BasicPostureAnalysis.Add(txt); }
                    }
                }
            }
            catch (Exception ex) { TheSys.showError("BasicPostureAnalysis: " + ex.ToString()); }
        }

        //============================================================================

        //For Temporary Reference
        public static string path_PE_localMM = "";//in case Local
        public static string path_PE_vRank = "";
        public static string path_PE_normal_extract = "";

        //path_data : row are selected, 2 columns to be cropped ("time,id")
        //path_minmax : minmax, no crop is needed, leave "" for local MM
        //path_folder & filename : path to save
        //Auto Crop Column Name 
        public static void ChangeAnalysis(string path_data_selected, string path_GlobalMM, string path_folder, string filename)
        {
            try
            {
                path_PE_localMM = path_folder + @"\[MinMax(local)].csv";//in case Local
                path_PE_vRank = path_folder + @"\" + filename + " F-Rank.csv";
                path_PE_normal_extract = path_folder + @"\" + filename + " Extracted-03 Normalized.csv";
                DataTable dt_temp = CSVReader.ReadCSVFile(path_data_selected, true);//have 2 unused col
                DataTable dt_data = TheTool.dataTable_cropCol(dt_temp, 2, 0);//only analyzed column
                DataTable dt_mm = null;//Datatable of MinMax
                //--- Prepare MinMax Table
                Boolean useGlobalMM = true;
                if (path_GlobalMM == "" || File.Exists(path_GlobalMM) == false) { useGlobalMM = false; }
                else
                {
                    try { dt_mm = CSVReader.ReadCSVFile(path_GlobalMM, true); }
                    catch (Exception ex) { TheSys.showError(ex); useGlobalMM = false; }
                }
                if (useGlobalMM == false)
                {
                    //build MM table by local data
                    dt_mm = TheTool.dataTable_getMaxMinTable(dt_data);//generate MM table
                    TheTool.export_dataTable_to_CSV(path_PE_localMM, dt_mm);
                }
                try
                {
                    DataTable dt_normal = TheTool.dataTable_MinMaxNormalization(dt_data, dt_mm);
                    //--- Cal Change -------------------------------------
                    List<String> data_raw_change = ThePosExtract.process_calChange(dt_data, false, false);
                    List<String> data_normal_change = ThePosExtract.process_calChange(dt_normal, true, true);
                    List<String> data_ChangeAnalysis = new List<String>();
                    data_ChangeAnalysis.Add("RAW");
                    data_ChangeAnalysis.AddRange(data_raw_change);
                    data_ChangeAnalysis.Add("");
                    data_ChangeAnalysis.Add("");
                    data_ChangeAnalysis.Add("NORMALIZED F-RANKING");
                    data_ChangeAnalysis.AddRange(data_normal_change);
                    TheTool.exportCSV_orTXT(path_PE_vRank, data_ChangeAnalysis, false);
                    //--- Normalize Table : re-added column before save
                    dt_normal.Columns.Add("time", typeof(string)).SetOrdinal(0);
                    dt_normal.Columns.Add("id", typeof(string)).SetOrdinal(0);
                    int r = 0;
                    foreach (DataRow row in dt_normal.Rows)
                    {
                        row[0] = dt_temp.Rows[r][0].ToString();
                        row[1] = dt_temp.Rows[r][1].ToString();
                        r++;
                    }
                    TheTool.export_dataTable_to_CSV(path_PE_normal_extract, dt_normal);
                }
                catch (Exception ex) { TheSys.showError("Normalize: " + ex.ToString()); }
            }
            catch (Exception ex) { TheSys.showError("Change Analysis: " + ex.ToString()); }
        }

        //Data contain "time,id"
        public static DataTable getNormalizedTable(string path_data_selected, string path_GlobalMM, Boolean Crop2Col)
        {
            try
            {
                DataTable dt_temp = CSVReader.ReadCSVFile(path_data_selected, true);//have 2 unused col
                DataTable dt_data = dt_temp;
                if (Crop2Col)
                {
                    dt_data = TheTool.dataTable_cropCol(dt_temp, 2, 0);//only analyzed column
                }
                DataTable dt_mm = null;//Datatable of MinMax
                //--- Prepare MinMax Table
                Boolean useGlobalMM = true;
                if (path_GlobalMM == "" || File.Exists(path_GlobalMM) == false) { useGlobalMM = false; }
                else
                {
                    try { dt_mm = CSVReader.ReadCSVFile(path_GlobalMM, true); }
                    catch (Exception ex) { TheSys.showError(ex); useGlobalMM = false; }
                }
                if (useGlobalMM == false)
                {
                    //build MM table by local data
                    dt_mm = TheTool.dataTable_getMaxMinTable(dt_data);//generate MM table
                }
                DataTable dt_normal = TheTool.dataTable_MinMaxNormalization(dt_data, dt_mm);
                //--- Normalize Table : re-added column before save
                if (Crop2Col)
                {
                    dt_normal.Columns.Add("time", typeof(string)).SetOrdinal(0);
                    dt_normal.Columns.Add("id", typeof(string)).SetOrdinal(0);
                    int r = 0;
                    foreach (DataRow row in dt_normal.Rows)
                    {
                        row[0] = dt_temp.Rows[r][0].ToString();
                        row[1] = dt_temp.Rows[r][1].ToString();
                        r++;
                    }
                }
                return dt_normal;
            }
            catch (Exception ex) { TheSys.showError(ex); return new DataTable(); }
        }

        //input : data with header (abs = absolute)
        //output has header
        public static List<String> process_calChange(DataTable dt, Boolean abs, Boolean sort)
        {
            List<String> result = new List<String>();
            try
            {
                int row_i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row_i > 1) {result.Add("");}
                    if(row_i > 0){
                        List<PE_Feature> list_feature = new List<PE_Feature>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            PE_Feature feature = new PE_Feature();
                            feature.head = col.ColumnName;
                            feature.bef = TheTool.getDouble(dt.Rows[row_i - 1][col].ToString());
                            feature.after = TheTool.getDouble(dt.Rows[row_i][col].ToString());
                            if(abs){ feature.change = Math.Abs(feature.after - feature.bef); }
                            else{ feature.change = feature.after - feature.bef; }
                            list_feature.Add(feature);
                        }
                        if (sort)
                        {
                            var sortList = list_feature.OrderByDescending(pd => pd.change).ToArray();
                            list_feature = sortList.ToList();
                        }
                        String r1 = "";
                        String r2 = "Base";
                        String r3 = "Pose " + row_i;
                        String r4 = "Change";
                        foreach(PE_Feature f in list_feature){
                            r1 += "," + f.head;
                            r2 += "," + f.bef;
                            r3 += "," + f.after;
                            r4 += "," + f.change;
                        }
                        result.Add(r1);
                        result.Add(r2);
                        result.Add(r3);
                        result.Add(r4);
                    }
                    row_i++;
                }
            }
            catch (Exception ex) { TheSys.showError("Normal: " + ex.ToString()); }
            return result;//New Table
        }

        public class PE_Feature
        {
            public string head { get; set; }
            public double bef { get; set; }
            public double after { get; set; }
            public double change { get; set; }
        }

        //============================================================

        public static void UKI_CalEntropy_1By1(String path_saveTo, String path_loadFrom, List<int> keyPostureId)
        {
            try{
                List<string> final_output = new List<string>();//Data
                //Header
                string origin_header = TheTool.read_File_getFirstLine(path_loadFrom);
                string new_header = "id";
                string[] h = TheTool.splitText(origin_header, ",");
                for(int i = 2; i < h.Count(); i++){
                    new_header += "," + h[i] + "_H";
                }
                final_output.Add(new_header);
                //
                final_output.AddRange(TheEntropy.calEntropy_MotionData(path_loadFrom, keyPostureId, 2, 1)); 
                TheTool.exportCSV_orTXT(path_saveTo, final_output, false);
            }catch(Exception ex){TheSys.showError(ex);}
        }

        public static void UKI_CalEntropy_Angle(String path_saveTo, String path_loadFrom, List<int> keyPostureId)
        {
            List<string> final_output = new List<string>();//Data
            final_output.Add("id," + TheUKI.getHeader_20Joint("_H"));//Header
            final_output.AddRange(TheEntropy.calEntropy_MotionData(path_loadFrom, keyPostureId, 0, 2));
            TheTool.exportCSV_orTXT(path_saveTo, final_output, false);
        }

        public static void UKI_CalEntropy_Eu(String path_saveTo, String path_loadFrom, List<int> keyPostureId)
        {
            List<string> final_output = new List<string>();//Data
            final_output.Add("id," + TheUKI.getHeader_20Joint("_dist_H"));//Header
            final_output.AddRange(TheEntropy.calEntropy_MotionData(path_loadFrom, keyPostureId, 2, 3));
            TheTool.exportCSV_orTXT(path_saveTo, final_output, false);
        }
    }

}
