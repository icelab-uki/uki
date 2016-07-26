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
        public static string extension_key = @".key";

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


        public static List<int_double> list_jump_all = new List<int_double>();//int[2]{key,spine_Y}, list of keypose that jump is detected (highest height)
        public static List<int_double> list_jump_selected = new List<int_double>();//Only selected
        public static List<int[]> list_MinMaxMin = new List<int[]>();//int[3] = Min-Max-Min
        public static Boolean capJumping = false;
        public static Boolean useFirstRecord = false;// TRUE: 0,20_20,40 FALSE: 7,20_20,40 
        public static Boolean useLastRecord = false;
        public static Boolean boundPass = true;
        public static Boolean isContinous = true;// TRUE: 0,20_20,40 FALSE: 1,15_20,40
        public static Boolean s_locate = true;//NOT USE YET

        public static int extractKeyPose_algo_selected = 0;//0 = proposed algo
        public static int extractKeyPose_algo_myAlgo = 0;
        public static int extractKeyPose_algo_sam = 1;
        public static int extractKeyPose_algo_angular = 2;

        public static List<int[]> extractKeyPose(Instance inst)
        {
            List<int[]> list_keyPose = new List<int[]>();
            if (extractKeyPose_algo_selected == extractKeyPose_algo_sam)
            {
                list_keyPose.AddRange(ThePosExtract2.extractKeyPose_SamAlgo(inst));
            }
            else if (extractKeyPose_algo_selected == extractKeyPose_algo_angular)
            {
                list_keyPose.AddRange(ThePosExtract2.extractKeyPose_Angular(inst));
            }
            else
            {
                //My Algo
                list_keyPose.AddRange(ThePosExtract.extractKeyPose_MyAlgo(inst.getDataMove()));
                UKI_DataMovement_markGT(inst);
            }
            return list_keyPose;
        }

        public static void UKI_DataMovement_markGT(Instance inst)
        {
            foreach(keyPoseGT kGT in inst.getKeyPoseGT(false)){
                UKI_DataMovement m;
                m = inst.getDataMove().Find(s => s.id == kGT.start[1]);
                if (m != null) { m.type_gt = -1; }
                m = inst.getDataMove().Find(s => s.id == kGT.end[0]);
                if (m != null) { m.type_gt = -1; }
            }
        }

        public static void UKI_DataMovement_markAlgo(Instance inst)
        {
            foreach (int[] k in inst.getKeyPose())
            {
                UKI_DataMovement m;
                m = inst.getDataMove().Find(s => s.id == k[0]);
                if (m != null) { m.type_algo = -1; }
                m = inst.getDataMove().Find(s => s.id == k[1]);
                if (m != null) { m.type_algo = -1; }
            }
        }

        public static Boolean UKI_DataMovement_CheckIfMarkExist(Instance inst)
        {
            Boolean exist = false;
            foreach(UKI_DataMovement m in inst.getDataMove()){
                if(m.type_algo != 0){exist = true;break;}
            }
            return exist;
        }

        //can Min Max, return row id at Extracted, also adjust UKI_DataMovement
        //List<int[2]>
        public static List<int[]> extractKeyPose_MyAlgo(List<UKI_DataMovement> list_movement)
        {
            List<int[]> list_PostureKey_Range = new List<int[]>();
            int[] current_PostureKey_Range = new int[] { -1, -1 };
            if(list_movement.Count > 0){
                list_jump_selected = new List<int_double>();
                list_MinMaxMin = new List<int[]>();
                //--- Prepare --------------------------------
                list_MinMaxMin.AddRange(calMinMaxMin(list_movement));
                //-------------------------------------------
                try {
                    //-- Build List --------------------------
                    if (useFirstRecord) { current_PostureKey_Range[0] = list_movement.First().id; }
                    int lastEnd = -1;
                    foreach (int[] mmm in list_MinMaxMin)
                    {
                        if (isContinous && lastEnd >= 0) {
                            int middle = (lastEnd + mmm[0]) / 2;
                            current_PostureKey_Range[0] = middle;
                            if (list_PostureKey_Range.Count > 0)
                            {
                                list_PostureKey_Range.Last()[1] = middle;
                            }
                        }
                        else if (current_PostureKey_Range[0] < 0) { current_PostureKey_Range[0] = mmm[0]; }
                        if (current_PostureKey_Range[1] < 0) { current_PostureKey_Range[1] = mmm[2]; lastEnd = mmm[2]; }
                        //--------------
                        list_PostureKey_Range.Add(current_PostureKey_Range);
                        current_PostureKey_Range = new int[] { -1, -1 };
                    }
                    //-- Adjust List --------------------------
                    if (capJumping)
                    {
                        list_jump_all = new List<int_double>();
                        list_jump_all.AddRange(calJump(list_movement));
                        foreach (int[] mmm in list_PostureKey_Range)
                        {
                            foreach (int_double j in list_jump_all)
                            {
                                if (j.i >= mmm[0] && j.i <= mmm[1])
                                {
                                    mmm[1] = j.i;
                                    list_jump_selected.Add(new int_double() { i = j.i, v = j.v });
                                }
                            }
                        }
                    }
                    if (useLastRecord && list_PostureKey_Range.Count > 0)
                    {
                         list_PostureKey_Range.Last()[1] = list_movement.Last().id;
                    }
                }
                catch (Exception ex) { TheSys.showError("calMinMax: " + ex.ToString()); }
            }
            return list_PostureKey_Range;
        }

        public static List<int[]> calMinMaxMin(List<UKI_DataMovement> list_movement)
        {
            List<int[]> list_MMM = new List<int[]>();
            int[] MMM = new int[]{-1,-1,-1};
            if (list_movement.Count > 0)
            {
                try
                {
                    //-- check Min Max value ----------------------------
                    double d = 0;//current
                    double d_min = 0;//must be > 0 after process
                    double d_max = 0;
                    foreach (UKI_DataMovement u in list_movement)
                    {
                        d = u.ms_all_avg;
                        if (d > 0)
                        {
                            if (d_min == 0) { d_min = d; }
                            else if (d < d_min) { d_min = d; }
                            if (d > d_max) { d_max = d; }
                        }
                    }
                    double d_range = d_max - d_min;
                    double d_minmove = Math.Abs(d_range * threshold_minmove);
                    double d_lowerBound = d_min + d_minmove;//at Percentile 20 from Global Min
                    double predictMin = d_lowerBound;//highest point that next minimum can be accepted
                    double predictMax = d_lowerBound;//lowest point that next maximum can be accepted
                    //-- fin Min Max location -------------- -------------
                    Boolean goUp_current = true;//current direction
                    Boolean goUp_last = true;//last scaned direction
                    double value_last = 0;//last scaned unit
                    double value_current = 0;//current scaned unit
                    double value_change = 0;
                    //-----------------
                    UKI_DataMovement recentAccepted = list_movement.First();//last accepted Minima / Maxima
                    UKI_DataMovement lastItem = list_movement.First();//last accepted Minima / Maxima
                    recentAccepted.type_algo = -1;
                    MMM[0] = recentAccepted.id;
                    //Start higher than threshold, use 0 as Min1
                    foreach (UKI_DataMovement u in list_movement.Skip(1))
                    {
                        u.type_algo = 0;
                        value_current = u.ms_all_avg;
                        value_change = value_current - value_last;
                        //---------------------------------
                        if (value_change >= 0) { goUp_current = true; }
                        else { goUp_current = false; }
                        //
                        if (predictMin == 0 || value_current < predictMin)
                        {
                            predictMax = Math.Max(Math.Min(value_current + d_minmove, predictMax),d_lowerBound);
                            predictMin = Math.Max(value_current, d_lowerBound);
                        }
                        if (value_current > predictMax) { 
                            predictMax = value_current; 
                            predictMin = Math.Max(value_current - d_minmove, d_lowerBound); 
                        }
                        //---------------------------------
                        //Min0 by Bound-Pass or LastAccept-Pass
                        if ((boundPass && value_last < d_lowerBound && value_current >= d_lowerBound)
                            ||
                            (!boundPass && value_last < recentAccepted.ms_all_avg && value_current >= recentAccepted.ms_all_avg))
                        {
                            if (list_MMM.Count == 0 || (list_MMM.Last()[2] != MMM[0]))
                            { 
                                recentAccepted.type_algo = 0;
                            }
                            recentAccepted = u;
                            recentAccepted.type_algo = -1;
                            MMM[0] = recentAccepted.id;
                        }
                        //Min2 by Bound
                        else if (value_last >= d_lowerBound && value_current < d_lowerBound)
                        {
                            if (MMM[1] >= 0 && MMM[2] < 0)
                            {
                                //found Min2 & Complete
                                recentAccepted = u;
                                recentAccepted.type_algo = -1;
                                MMM[2] = recentAccepted.id; list_MMM.Add(MMM);
                                //
                                MMM = new int[] { -1, -1, -1 };
                                MMM[0] = recentAccepted.id;//First of new set
                            }
                            else if (MMM[1] < 0 && list_MMM.Count > 0 && list_MMM.Last()[2] > d_lowerBound)
                            {
                                //adjust last End
                                recentAccepted.type_algo = 0;
                                recentAccepted = u;
                                recentAccepted.type_algo = -1;
                            }
                        }
                        else if (goUp_current != goUp_last) //found change in Direction
                        {
                            //During finding Min2
                            if (MMM[1] >= 0 && MMM[2] < 0)
                            {
                                if (s_locate && goUp_current && value_last <= predictMin)
                                {
                                    //found Min2 & Complete
                                    if (MMM[2] >= 0) { recentAccepted.type_algo = 0; }
                                    recentAccepted = lastItem;
                                    lastItem.type_algo = -1;
                                    MMM[2] = recentAccepted.id; list_MMM.Add(MMM);
                                    //
                                    MMM = new int[] { -1, -1, -1 };
                                    MMM[0] = recentAccepted.id;//First of new set
                                }
                                else if (!goUp_current && value_last > recentAccepted.ms_all_avg)
                                {
                                    //found higher Max1
                                    if (MMM[1] >= 0) { recentAccepted.type_algo = 0; }
                                    recentAccepted = lastItem;
                                    recentAccepted.type_algo = 1;
                                    MMM[1] = recentAccepted.id;// -1 because we collect last data before change
                                    //
                                    predictMin = Math.Max(value_last - d_minmove, d_lowerBound);
                                }
                            }
                            //During finding Max1
                            else if (MMM[0] >= 0 && MMM[1] < 0)
                            {
                                if (!goUp_current && value_last >= predictMax)
                                {
                                    //found Max
                                    if (MMM[1] >= 0) { recentAccepted.type_algo = 0; }
                                    recentAccepted = u;
                                    MMM[1] = recentAccepted.id;
                                    recentAccepted.type_algo = 1;
                                    //
                                    predictMin = Math.Max(value_current - d_minmove, d_lowerBound);
                                }
                                else if (value_last < recentAccepted.ms_all_avg && value_last > d_lowerBound)
                                {
                                    //found lower Min0
                                    recentAccepted.type_algo = 0;
                                    recentAccepted = lastItem;
                                    MMM[0] = recentAccepted.id;
                                    recentAccepted.type_algo = -1;
                                    //
                                    if (list_MMM.Count > 0) { list_MMM.Last()[2] = recentAccepted.id; }
                                }
                            }
                        }
                        //-----------------------------------------
                        u.myalgo_bound_lowest = d_lowerBound;
                        u.myalgo_bound_predictedMin = predictMin;
                        u.myalgo_bound_predictedMax = predictMax;
                        //----
                        goUp_last = goUp_current;
                        value_last = value_current;
                        lastItem = u;
                    }//for each
                    //if (MMM[2] >= 0) { list_MMM.Add(MMM); }//add last item
                    //else if (MMM[0] >= 0 && MMM[1] < 0) { recentAccepted.type_algo = 0; }//delete last item
                }
                catch (Exception ex) { TheSys.showError(ex); }
            }
            //------------------------------------
            return list_MMM;
        }

        //return key when Jump (highest point)
        public static List<int_double> calJump(List<UKI_DataMovement> list_movement)
        {
            List<int_double> list_key = new List<int_double>();
            Boolean duringJump = false;
            UKI_DataMovement highest_key = new UKI_DataMovement();
            foreach (UKI_DataMovement u in list_movement)
            {
                if (u.spine_Y > jump_threshold)
                {
                    if (u.spine_Y > highest_key.spine_Y) { highest_key = u; }
                    duringJump = true;
                    if (highest_key.type_algo != 0) { highest_key.type_algo = 0.33; }
                }
                else if (duringJump){
                    highest_key.type_algo = 0.66; 
                    int[] key = new int[]{};
                    list_key.Add(new int_double() { i = highest_key.id, v = highest_key.spine_Y });
                    duringJump = false;
                    highest_key = new UKI_DataMovement();
                }
            }
            return list_key;
        }

        //----------------------------------------------------------------------------

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
                    if(row_i > 1) {result.Add("");}
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

        public static void UKI_CalEntropy_1By1(String path_saveTo, String path_loadFrom, List<int[]> keyPosture_Range)
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
                final_output.AddRange(TheEntropy.calEntropy_MotionData(path_loadFrom, keyPosture_Range, 2, 1)); 
                TheTool.exportCSV_orTXT(path_saveTo, final_output, false);
            }catch(Exception ex){TheSys.showError(ex);}
        }

        public static void UKI_CalEntropy_Angle(String path_saveTo, String path_loadFrom, List<int[]> keyPostureRange)
        {
            List<string> final_output = new List<string>();//Data
            final_output.Add("id," + TheUKI.getHeader_20Joint("_H"));//Header
            final_output.AddRange(TheEntropy.calEntropy_MotionData(path_loadFrom, keyPostureRange, 0, 2));
            TheTool.exportCSV_orTXT(path_saveTo, final_output, false);
        }

        public static void UKI_CalEntropy_Eu(String path_saveTo, String path_loadFrom, List<int[]> keyPostureRange)
        {
            List<string> final_output = new List<string>();//Data
            final_output.Add("id," + TheUKI.getHeader_20Joint("_dist_H"));//Header
            final_output.AddRange(TheEntropy.calEntropy_MotionData(path_loadFrom, keyPostureRange, 2, 3));
            TheTool.exportCSV_orTXT(path_saveTo, final_output, false);
        }

        //INPUT 30-100, OUTPUT 100
        public static List<int> getKeyPose_ID_StartEnd(List<int[]> keyPose_Range)
        {
            List<int> keyPose_ID = new List<int>();
            foreach (int[] k in keyPose_Range) { keyPose_ID.Add(k[0]); keyPose_ID.Add(k[1]); }
            return keyPose_ID;
        }

        //INPUT: 10,11_14,16
        public static List<int[]> getKetPose_Range_from1String(string s)
        {
            List<int[]> keyPose_list = new List<int[]>();
            string[] a = TheTool.splitText(s,"_");
            for (int i = 0; i < a.Count(); i++)
            {
                if(a[i] != ""){//this line is not ""
                    int[] d = new int[2];
                    string[] b = TheTool.splitText(a[i], ",");
                    if (b.Count() > 0) { d[0] = TheTool.getInt(b[0]); d[1] = TheTool.getInt(b[0]); }
                    if (b.Count() > 1) {d[1] = TheTool.getInt(b[1]); }
                    keyPose_list.Add(d);
                }
            }
            return keyPose_list;
        }

        //joinTxt = ","
        public static string printKeyPose(List<int[]> keyPose,string joinTxt)
        {
            string s = ""; int i = 0;
            foreach (int[] k in keyPose)
            {
                if (i > 0) { s += "_"; }
                for (int a = 0; a < k.Count(); a++)
                {
                    if (a > 0) { s += joinTxt; }
                    s += k[a];
                }
                i++;
            }
            return s;
        }

        //joinTxt = ","
        public static string printKeyPoseGT(List<keyPoseGT> keyPoseGT, string joinTxt)
        {
            string s = ""; int i = 0;
            foreach (keyPoseGT k in keyPoseGT)
            {
                if (i > 0) { s += "_"; }
                s += k.start[0] + "-" + k.start[1] + joinTxt + k.end[0] + "-" + k.end[1];
                i++;
            }
            return s;
        }

        //joinTxt = ","
        public static string printKeyJump(List<int_double> keyJump, string joinTxt)
        {
            string s = ""; int i = 0;
            foreach (int_double k in keyJump)
            {
                if (i > 0) { s += "_"; }
                s += k.i + ":Y" + Math.Round(k.v,2);
                i++;
            }
            return s;
        }

        //Ref: (2014) Sequential max-margin event detectors
        //Ref: ChairLearn
        //Ref: Graph-based representation learning for automatic human motion segmentation
        static public List<Performance> export_SegmentAnalysis(InstanceContainer container, string pathSave)
        {
            List<string> list_output = new List<string>();
            List<Performance> list_performanceByMID = new List<Performance>();
            try
            {
                list_output.Add("name,sid,mid," + performance_header + ",key,keyGT,keyJ");
                if (container.list_inst.Count() > 0)
                {
                    foreach (Instance inst in container.list_inst)
                    {
                        Performance p = performance_measure(inst);
                        performance_measure2(p);
                        string output = inst.name + "," + inst.subject_id + "," + inst.motion_id
                            + "," + performance_Print(p)
                            + "," + printKeyPose(inst.keyPose, " ")
                            + "," + printKeyPoseGT(inst.keyPoseGT, " ")
                            + "," + printKeyJump(inst.keyPoseJump, " ");
                        performance_AddData(list_performanceByMID, p);
                        list_output.Add(output);
                    }
                }
                //path_segmentByMotion
                TheTool.exportCSV_orTXT(pathSave, list_output, false);
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return list_performanceByMID;
        }

        public static Performance performance_measure(Instance inst){
            Performance p = new Performance{};
            //-------------------------------------------------------
            //(Sum) r_sumATSR,r_sumconcordance,Found,Total,Deleted,Inserted,Correct
            p.mid = inst.motion_id;
            p.Total = inst.keyPoseGT.Count;
            p.Found = inst.keyPose.Count;
            List<int[]> list_key = new List<int[]>();
            List<int[]> list_keyGT = new List<int[]>();
            list_key.AddRange(inst.keyPose);
            foreach (keyPoseGT kGT in inst.keyPoseGT)
            {
                list_keyGT.Add(new int[]{ kGT.start[1], kGT.end[0]});
            }
            for (int i_g = 0; i_g < list_keyGT.Count; i_g++)
            {
                int[] keyGT = list_keyGT[i_g];
                Boolean deleted = true;
                for (int i = 0; i < list_key.Count; i++)
                {
                    int[] key = list_key[i];
                    if (key[0] > keyGT[1]){ break; }
                    int[] overlap = new int[]{ Math.Max(keyGT[0],key[0]),  Math.Min(keyGT[1],key[1])};
                    if (overlap[0] < overlap[1])
                    {
                        double overlapLengthGT = (double) (overlap[1] - overlap[0]) / (keyGT[1] - keyGT[0]) ;
                        if (overlapLengthGT >= 0.5)
                        {
                            p.Correct++;
                            p.list_ATSR.Add(calATSR(keyGT, key) );
                            p.list_concordance.Add(calConcordance(keyGT, key));
                            deleted = false;
                            list_key.Remove(key);
                            i--;
                            continue;
                        }
                    }
                    p.Inserted++;
                    list_key.Remove(key);
                    i--;
                }
                if (deleted) { p.Deleted++; }
            }
            //------------------------------------------------------
            //(ratio) p_ATSR,p_concordance, p_Accuracy,p_Error,p_Recall,p_Precision,p_F-Score,Perf1,Perf2
            performance_measure2(p);
            return p;
        }

        public static void performance_measure2(Performance p)
        {
            if (p.Total > 0)
            {
                p.p_accuracy = (double)(p.Correct - p.Inserted) / p.Total;
                p.p_error = (double)(p.Deleted + p.Inserted) / (p.Total + p.Inserted);
            }
            if (p.Correct > 0)
            {
                p.p_recall = (double)p.Correct / (p.Correct + p.Deleted);
                p.p_precision = (double)p.Correct / (p.Correct + p.Inserted);
                p.p_F = 2 * (p.p_recall * p.p_precision) / (p.p_recall + p.p_precision);
                if (p.list_ATSR.Count > 0) { p.p_ATSR = p.list_ATSR.Average(); }
                if (p.list_concordance.Count > 0) { p.p_concordance = p.list_concordance.Average(); } 
            }
            double dividend = 0; double divisor = 0;
            dividend = 5 * p.p_ATSR * p.p_F; divisor = (4 * p.p_ATSR) + p.p_F;
            if (divisor > 0) { p.perf1 = dividend / divisor; }
            dividend = 5 * p.p_concordance * p.p_F; divisor = (4 * p.p_concordance) + p.p_F;
            if (divisor > 0) { p.perf2 = dividend / divisor; }
        }

        public static double calATSR(int[] keyGT, int[] key)
        {
            double range = keyGT[1] - keyGT[0];
            if (range == 0) { return 0; }
            else {
                double atse = (Math.Abs(keyGT[0] - key[0]) + Math.Abs(keyGT[1] - key[1])) / range;
                if (atse <= .1) { atse = 0; }
                else if (atse > 1) { atse = 1; }
                return 1 - atse; 
            }
        }

        public static double calConcordance(int[] keyGT, int[] key)
        {
            double rc = 0;
            double divisor = keyGT[1] - keyGT[0] + key[1] - key[0];
            if (divisor > 0)
            {
                double dividend = Math.Min(keyGT[1], key[1]) - Math.Max(keyGT[0], key[0]);
                dividend *= 2;
                rc = dividend / divisor;
                if (rc >= .9) { rc = 1; }
            }
            return rc;
        }

        public static void performance_AddData(List<Performance> list_performance, Performance p2)
        {
            Performance p = null;
            if (list_performance.Count > 0) { p = list_performance.Find(s => s.mid == p2.mid); }
            if (p != null)
            {
                p.Found += p2.Found;
                p.Total += p2.Total;
                p.Deleted += p2.Deleted;
                p.Inserted += p2.Inserted;
                p.Correct += p2.Correct;
                if (p2.p_ATSR > 0){ p.list_ATSR.Add(p2.p_ATSR); }
                if (p2.p_concordance > 0) { p.list_concordance.Add(p2.p_concordance); }
            }
            else {
                p = new Performance();
                p.mid = p2.mid;
                p.Found = p2.Found;
                p.Total = p2.Total;
                p.Deleted = p2.Deleted;
                p.Inserted = p2.Inserted;
                p.Correct = p2.Correct;
                if (p2.p_ATSR > 0) { p.list_ATSR.Add(p2.p_ATSR); }
                if (p2.p_concordance > 0) { p.list_concordance.Add(p2.p_concordance); }
                list_performance.Add(p);
            }
        }

        ////INPUT: list of performance 1 by 1
        ////OUTPUT: list of performance by MID
        public static void export_SegmentMIDAnalysis(List<Performance> list_performance, string pathSave)
        {
            List<string> list_output = new List<string>();
            Performance sum = new Performance();
            foreach (Performance p in list_performance)
            {
                performance_measure2(p);
                list_output.Add(p.mid + "," + performance_Print(p));
                //-----
                sum.Found += p.Found;
                sum.Total += p.Total;
                sum.Deleted += p.Deleted;
                sum.Inserted += p.Inserted;
                sum.Correct += p.Correct;
                if (p.p_ATSR > 0) { sum.list_ATSR.Add(p.p_ATSR); }
                if (p.p_concordance > 0) { sum.list_concordance.Add(p.p_concordance); }
            }
            performance_measure2(sum);
            string overall_performance = "All," + performance_Print(sum);
            temp_performance.Add(overall_performance);
            list_output.Insert(0, overall_performance);
            list_output.Insert(0, "MID," + performance_header);
            TheTool.exportCSV_orTXT(pathSave, list_output, false);
            //------
            temp_overallPerformance =
                " P2:" + Math.Round(sum.perf2, 2) + " P1:" + Math.Round(sum.perf1, 2)
                + " F:" + Math.Round(sum.p_F, 2)
                + " ATSR:" + Math.Round(sum.p_ATSR, 2) + " Rc:" + Math.Round(sum.p_concordance, 2)
                + " Rec:" + Math.Round(sum.p_recall, 2) + " Pre:" + Math.Round(sum.p_precision, 2)
                + " Acc:" + Math.Round(sum.p_accuracy, 2) + " Err:" + Math.Round(sum.p_error, 2);
        }

        public static List<string> temp_performance = new List<string>();
        
        public static string temp_overallPerformance = "";

        public static string performance_header = "Perf1,Perf2,p_ATSR,p_concordance,p_F-Score,p_Recall,p_Precision,p_Accuracy,p_Error,Found,Total,Deleted,Inserted,Corret";
        public static string performance_Print(Performance p)
        {
            return p.perf1 + "," + p.perf2 + "," + 
                p.p_ATSR + "," + p.p_concordance + "," + p.p_F + "," +
                p.p_recall + "," + p.p_precision + "," + p.p_accuracy + "," + p.p_error + "," +
                p.Found + "," + p.Total + "," + p.Deleted + "," + p.Inserted + "," + p.Correct;
        }

    }

    class Performance
    {
        public int mid = 0;
        public int Found = 0;
        public int Total = 0;
        public int Deleted = 0;
        public int Inserted = 0;
        public int Correct = 0;
        public List<double> list_ATSR = new List<double>();
        public List<double> list_concordance = new List<double>();
        //
        public double p_accuracy = 0;
        public double p_error = 0;
        public double p_recall = 0;
        public double p_precision = 0;
        public double p_F = 0;
        //
        public double p_ATSR = 0;
        public double p_concordance = 0;//concordance rate
        public double perf1 = 0;
        public double perf2 = 0;
    }

}
