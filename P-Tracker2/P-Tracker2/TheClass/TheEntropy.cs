using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheEntropy
    {
        public static string temp_colWithHigestEntropy = "";
        public static List<str_double> temp_listEntropy = new List<str_double>();

        //Threshold End - Start
        //public static Feature getFeature(string selected_feature, DataTable dt)
        //{
        //    Feature f = new Feature();
        //    f.name = selected_feature;
        //    try
        //    {
        //        double start = TheTool.getDouble(dt.Rows[0][selected_feature].ToString());
        //        double end = TheTool.getDouble(dt.Rows[dt.Rows.Count - 1][selected_feature].ToString());
        //        if (end > start) { f.opt = ">="; }
        //        else { f.opt = "<="; }
        //        //-----
        //        start = Math.Round(start, 2);
        //        end = Math.Round(end, 2);
        //        double v = (start + end) / 2;
        //        f.v = Math.Round(v, 2);
        //    }
        //    catch (Exception ex) { TheSys.showError(ex); }
        //    return f;
        //}

        ////indices is row number
        //public static Feature getFeature(string selected_feature, DataTable dt_concat
        //    , List<int> start_indices, List<int> end_indices)
        //{
        //    Feature f = new Feature();
        //    f.name = selected_feature;
        //    try
        //    {
        //        double start = TheTool.dataTable_getAverage(dt_concat, start_indices, selected_feature);
        //        double end = TheTool.dataTable_getAverage(dt_concat, end_indices, selected_feature);
        //        if (end > start) { f.opt = ">="; }
        //        else { f.opt = "<="; }
        //        //-----
        //        start = Math.Round(start, 2);
        //        end = Math.Round(end, 2);
        //        double v = (start + end) / 2;
        //        f.v = Math.Round(v, 2);
        //    }
        //    catch (Exception ex) { TheSys.showError(ex); }
        //    return f;
        //}

        
        ////Feature Name with Threshold by DataTable (First - End)
        //public static Feature getFeature(string s)
        //{
        //    Feature f = new Feature();
        //    f.name = s;
        //    return f;
        //}

        //public static List<string> Feature_getListName(List<Feature> list_f)
        //{
        //    List<string> list = new List<string>();
        //    foreach (Feature f in list_f) { list.Add(f.name); }
        //    return list;
        //}

        //public static List<string> Feature_getListFullString(List<Feature> list_f)
        //{
        //    List<string> list = new List<string>();
        //    foreach (Feature f in list_f) { list.Add(f.name + " " + f.opt + " " + f.v); }
        //    return list;
        //}

        //H(X)
        //table contain only useful Column, value are discretized
        //analyze all row
        public static List<str_double> calEntropy(DataTable dt)
        {
            List<str_double> list_entropy = new List<str_double>();
            List<string> final_output = new List<string>();//Data
            try
            {
                double highestEntropy = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    str_double new_data = calEntropyOfColumn(dt,dc);
                    list_entropy.Add(new_data);
                    if (new_data.v > highestEntropy)
                    {
                        temp_colWithHigestEntropy = new_data.str;
                        highestEntropy = new_data.v;
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); temp_colWithHigestEntropy = ""; }
            return list_entropy;
        }

        public static str_double calEntropyOfColumn(DataTable dt, DataColumn dc)
        {
            str_double entropy = new str_double();
            try
            {
                //--- build unique list ------------------------------------------------
                List<str_int> list_unique = new List<str_int>();
                foreach (DataRow dr in dt.Rows)
                {
                    Boolean neverExist = true;
                    string current_v = dr[dc].ToString(); 
                    foreach (str_int uni in list_unique)
                    {
                        if (uni.str == current_v)
                        {
                            uni.i++; neverExist = false; break;
                        }
                    }
                    if (neverExist)
                    {
                        list_unique.Add(new str_int() { str = current_v, i = 1 });
                    }
                }
                //--- Compute Entropy ------------------------------------------------
                double prob_log_cummulative = 0;
                int total_row = dt.Rows.Count;
                foreach (str_int uni in list_unique)
                {
                    prob_log_cummulative += calPossibleLog(uni.i, total_row);
                }
                entropy.str = dc.ColumnName;
                entropy.v = -prob_log_cummulative;
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return entropy;
        }

        public static double calPossibleLog(int dividend, int divisor)
        {
            if (divisor > 0)
            {
                double possible = (double) dividend / divisor;
                return possible * Math.Log(possible,2);
            }
            else { return 0; }
        }

        // H(X,Y)
        // H(X,..,N)Joint Entropy of N features
        //Input : Given set of feature
        public static double calJointEntropy(DataTable dt, List<string> feature_list)
        {
            double entropy = 0;
            try
            {
                int f_count = feature_list.Count();
                List<strList_int> unique_list = new List<strList_int>();
                //----- Prepare Unique Count List ------------
                int dr_i = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    Boolean neverExist = true;
                    //Prepare Current Data
                    strList_int current_data = new strList_int(); current_data.v = 1;
                    foreach (string f in feature_list) { current_data.str_list.Add(dt.Rows[dr_i][f].ToString()); }
                    //Compare to Unique List
                    if (dr_i > 0)
                    {
                        foreach(strList_int uni in unique_list){
                            for (int i = 0; i < f_count; i++)
                            {
                                if (current_data.str_list[i] != uni.str_list[i]) { 
                                    // Not in this list (at least 1 diff)
                                    break;
                                }
                                else if (i == f_count - 1){
                                    // Find Exist (equal until the last)
                                    neverExist = false; 
                                    uni.v++;
                                }
                            }
                            if (!neverExist) { break; }
                        }
                    }
                    //Add of never Exist
                    if (neverExist) { unique_list.Add(current_data); }
                    dr_i++;
                }
                //------------------------------------------------
                foreach (strList_int uni in unique_list)
                {
                    entropy += calPossibleLog(uni.v, dt.Rows.Count);
                }
                entropy = -entropy;
            }
            catch (Exception ex) { TheSys.showError(ex); }
            //TheSys.showError(feature_list, true, false); TheSys.showError(" = " + entropy);
            return entropy;
        }

        //H(X|Y) = H(X,Y) - H(Y)
        public static double calConditionalEntropy(DataTable dt, List<str_double> list_entropy, string x, string y)
        {
            double entropy = 0;
            entropy += calJointEntropy(dt, new List<string> {x,y});
            entropy -= TheTool.str_double_getDouble_byStr(list_entropy, y);
            return entropy;
        }

        //MI for pair of redundantable feature
        public static List<str2_double> calMIList(DataTable dt, List<str_double> list_entropy)
        {
            List<str2_double> listMI = new List<str2_double>();
            try{
                foreach (List<String> group in TheUKI.list_bodyGroup_full)//List of Redundantable
                {
                    string[] feature = group.ToArray();
                    for (int i = 0; i < feature.Count(); i++)
                    {
                        string s1 = ""; string s2 = "";
                        if (i == feature.Count() - 1) { s1 = feature[i]; s2 = feature[0]; }
                        else { s1 = feature[i]; s2 = feature[i + 1]; }
                        List<string> pair = new List<string> { s1, s2 };
                        str2_double new_data = new str2_double();
                        new_data.str1 = s1;
                        new_data.str2 = s2;
                        new_data.v = TheTool.str_double_getDouble_byStr(list_entropy, s1);
                        new_data.v += TheTool.str_double_getDouble_byStr(list_entropy, s2);
                        new_data.v -= calJointEntropy(dt, pair);
                        //TheSys.showError(
                        //    Math.Round(TheTool.str_double_getDouble_byStr(list_entropy, s1),2) + " + " +
                        //    Math.Round(TheTool.str_double_getDouble_byStr(list_entropy, s2),2) + " - " +
                        //    calJointEntropy(dt, pair) + " = " + new_data.v);
                        listMI.Add(new_data);
                    }
                }
            }
            catch(Exception ex) { TheSys.showError(ex); }
            return listMI;
        }

        //****************************************************************************************

        public static List<string> calEntropy_MotionData(String path_loadFrom, List<int> keyPostureId
            , int degit, int col_combine_range)
        {
            List<string> final_output = new List<string>();//Data
            try
            {
                DataTable dt = CSVReader.ReadCSVFile(path_loadFrom, true);//prepare Data
                dt = TheTool.dataTable_cropCol(dt, 2, 0);//del first 2 Col
                TheTool.dataTable_roundValue(dt, degit);// eliminate decimal
                int start = -1; int end = 0;
                foreach (int i in keyPostureId)
                {
                    end = i;
                    if (start > -1)
                    {
                        if (end == start) { end = dt.Rows.Count - 1; }
                        if (end > start)
                        {
                            DataTable dt_sub = TheTool.dataTable_selectRow_byIndex(dt, start, end);
                            final_output.Add(start + " to " + end + "," + calEntrophy_1Action(dt_sub, col_combine_range));
                        }
                    }
                    start = i;
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return final_output;
        }

        //dt is already preprocessed(Crop & Select Row & contain data for 1 action)
        //col_combine_range = 2;// Combine 2 columns
        public static String calEntrophy_1Action(DataTable dt, int col_combine_range)
        {
            string data_line = "";//Data
            int col_combine_id = 0;// ( 0 , col_combine_range )
            Boolean firstData = true;
            double sumEntropy = 0;
            //
            foreach (DataColumn dc in dt.Columns)
            {
                sumEntropy += calEntropyOfColumn(dt, dc).v;
                col_combine_id++;
                if (col_combine_id >= col_combine_range)
                {
                    if (firstData) { firstData = false; }
                    else { data_line += ","; }
                    data_line += (-sumEntropy).ToString();
                    col_combine_id = 0;
                    sumEntropy = 0;
                }
            }
            return data_line;
        }

        // Possibity x Logarithm of Possibity
        // list = { num, count_of_num }
        public static double calListPossibleLog(List<str_int> list, int total_record)
        {
            double output = 0;
            if (total_record > 0)
            {
                foreach (str_int v in list)
                {
                    double possible = (double)v.i / total_record;
                    output += possible * Math.Log(possible, 2);
                }
            }
            return output;
        }

        //==================================================================================
        public static int methodSelection_Greedy = 1;//H(fi,fs)
        public static int methodSelection_MIFS = 2;//H( fi ) + SUM ( I ( fi , fs ) )
        public static int methodSelection_JMIM = 3;//MAXIMIN( H( fi , fs ) )
        public static int methodSelection_CMIM = 4;//MAXIMIN( H( fi | fs ) )
        //
        public static int methodSelection_DCFS = -1;
        

        //INPUT : get list of H(fi)
        //PROCESS : select 1 candidate, return col name
        public static string select1Candidate_entropy(int method, DataTable dt,
            List<string> list_F, List<string> list_S
            , List<str_double> list_entropy,  List<str2_double> list_MI)
        {
            List<string> list_predict = new List<string>();
            list_predict.AddRange(list_S);
            try
            {
                temp_listEntropy.Clear();
                temp_colWithHigestEntropy = "";
                double highestQuality = -9999;
                foreach (string f in list_F)
                {
                    //----- test 1 candidate ---------
                    list_predict.Add(f);
                    str_double new_data = new str_double();
                    new_data.str = f;
                    if (method == methodSelection_Greedy) { new_data.v = calJointEntropy(dt, list_predict); }//target
                    else if (method == methodSelection_MIFS) { new_data.v = candidate_calMIFS(dt, f, list_S, list_entropy, list_MI); }
                    else if (method == methodSelection_JMIM) { new_data.v = candidate_calMAXIMIN(miximinMethod_JMIM, dt, f, list_S, list_entropy, list_MI); }
                    else if (method == methodSelection_CMIM) { new_data.v = candidate_calMAXIMIN(miximinMethod_CMIM, dt, f, list_S, list_entropy, list_MI); } 
                    temp_listEntropy.Add(new_data);
                    if (new_data.v > highestQuality)
                    {
                        temp_colWithHigestEntropy = new_data.str;
                        highestQuality = new_data.v;
                    }
                    list_predict.RemoveAt(list_predict.Count - 1);
                }
            }
            catch (Exception ex) { TheSys.showError(ex); temp_colWithHigestEntropy = ""; }
            return temp_colWithHigestEntropy;
        }

        public static double candidate_calMIFS(DataTable dt,
            string candidate_f, List<string> list_S
            , List<str_double> list_entropy,  List<str2_double> list_MI)
        {
            double mifs = TheTool.str_double_getDouble_byStr(list_entropy, candidate_f);
            double sumMI = 0;
            foreach (str2_double mi in list_MI)
            {
                if ((candidate_f == mi.str1 && list_S.Contains(mi.str2))
                    || (candidate_f == mi.str2 && list_S.Contains(mi.str1)))
                {
                    sumMI += mi.v;
                }
            }
            mifs -= sumMI;
            return mifs;
        }

        public static int miximinMethod_JMIM = 1;
        public static int miximinMethod_CMIM = 2;

        //candidate fi
        public static double candidate_calMAXIMIN(
            int miximinMethod, DataTable dt,
            string fi, List<string> list_S
            , List<str_double> list_entropy,  List<str2_double> list_MI)
        {
            double min = 99;
            List<string> testSet = new List<string>();
            testSet.Add(fi);
            foreach (string si in list_S)
            {
                testSet.Add(si);
                double v = 0;
                if (miximinMethod == miximinMethod_JMIM) { v = calJointEntropy(dt, testSet); }
                else if (miximinMethod == miximinMethod_CMIM) { v = calConditionalEntropy(dt, list_entropy, fi, si); }
                if (v < min) { min = v; }
                testSet.RemoveAt(1);
            }
            return min;
        }
        
    }
}
