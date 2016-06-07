using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace P_Tracker2
{
    class ModelGenerator
    {
        //Output : Model for 1 MOTION
        //Process01-02 is run for each POSTURE


        //-- VARIABLE : for each Posture
        public DataTable dt_raw = new DataTable();//centered & concat
        public DataTable dt_raw_exclude = new DataTable();//centered & concat with exclusion
        public DataTable dt_ready = new DataTable();//partitized "dt_raw"
        public List<str_double> list_entropy = new List<str_double>();
        public List<str2_double> list_MI = new List<str2_double>();
        public DataTable dt_threshold_pose1 = new DataTable(); //For threshold : 1 Row for 1 Inst >> 1 Table for 1 Pose >> List<Table> for Motion
        public DataTable dt_threshold_pose2 = new DataTable(); //For threshold : 1 Row for 1 Inst >> 1 Table for 1 Pose >> List<Table> for Motion
        
        public List<string> EnAlgo_first_list_F = new List<string>();
        public Feature EnAlgo_first_S = new Feature();
        public List<string> DCFS_first_list_F = new List<string>();
        public Feature DCFS_first_S = new Feature();
        public List<str2_double> DCFS_listCorrelation = new List<str2_double>();

        //-------
        public List<FeatureDelta> DCFS_listDelta = new List<FeatureDelta>();//initial, will not be editted
        public List<FeatureDelta> DCFS_listDeltaCorrelation = new List<FeatureDelta>();//reset at process01

        public void DCFS_listDelta_update(string name)
        {
            //Penalize Redundant Feature
            foreach (FeatureDelta f in DCFS_listDeltaCorrelation)
            {
                if (f.name != name) { 
                    //Penalize Duplicate Feature
                    double duplicate_lv = TheTool.str2_double_getValue(DCFS_listCorrelation, f.name, name);
                    double multiplier = 1 - duplicate_lv;
                    f.delta *= multiplier;
                }
            }
            //Delete
            foreach (FeatureDelta f in DCFS_listDeltaCorrelation)
            {
                if (f.name == name) { DCFS_listDeltaCorrelation.Remove(f); break; }
            }
        }

        //-- OUTPUT after all Posture

        public void process00_setting(
            List<Instance> motion_true, List<Instance> motion_false,
            Boolean selection_autoNumber, Boolean selection_maxTrue,
            Boolean selection_minNumber, int selection_MinTradeOff, 
            Boolean selection_extraFeature,
            Boolean selection_optimizeThreshold, Boolean selection_avoidGabage,
            double partition_range
            )
        {
            this.selection_autoNumber = selection_autoNumber;
            this.selection_maxTrue = selection_maxTrue;
            this.selection_minNumber = selection_minNumber;
            this.selection_tradeoff_threshold = selection_MinTradeOff;
            this.selection_extraFeature = selection_extraFeature;
            this.selection_optimizeThreshold = selection_optimizeThreshold;
            this.selection_avoidGabage = selection_avoidGabage;
            //-----------------
            if (motion_false == null) { motion_false = new List<Instance>(); }
            this.inst_true = motion_true;
            this.inst_false = motion_false;
            this.partition_range = partition_range;
        }

        //Used if "Auto Loop"
        public Boolean selection_autoNumber = false;//auto select (non-specific number of feature)
        public Boolean selection_maxTrue = false;//nerver let TRUE drop
        public Boolean selection_minNumber = false;//minimize number of feature
        public int selection_tradeoff_threshold = 0;
        public Boolean selection_extraFeature = false;// Y from base, etc.
        public Boolean selection_optimizeThreshold = false;
        public Boolean selection_avoidGabage = false;
        public static double partition_range_default = 0.05;
        double partition_range = 0.05;


        public List<Instance> inst_true = new List<Instance>();//motion X (TRUE+)
        public List<Instance> inst_false = new List<Instance>();//other motion (FALSE+)

        List<String> list_exclude = new List<String>{
                        "id", "time",  
                        "ShoulderLeft_SC_x", "ShoulderLeft_SC_y", "ShoulderLeft_SC_z", 
                        "ShoulderRight_SC_x", "ShoulderRight_SC_y", "ShoulderRight_SC_z", 
                        "Spine_HC_x", "Spine_HC_y", "Spine_HC_z", 
                        "HipCenter_HC_x", "HipCenter_HC_y", "HipCenter_HC_z", 
                        "HipLeft_HC_x", "HipLeft_HC_y", "HipLeft_HC_z", 
                        "HipRight_HC_x", "HipRight_HC_y", "HipRight_HC_z"
                    };

        //set start & end = null if analyze whole file
        public void process01_selectFirstFeature(Boolean AlgoEntropy, Boolean AlgoDCFS, DataTable dt_raw, DataTable dt_threshold_pose1, DataTable dt_threshold_pose2)
        {
            this.dt_raw = dt_raw;
            this.dt_threshold_pose1 = dt_threshold_pose1;
            this.dt_threshold_pose2 = dt_threshold_pose2;
            this.dt_raw_exclude = TheTool.dataTable_removeCol(dt_raw, list_exclude);
            if (AlgoEntropy) { process01EntropyAlgo(); }
            if (AlgoDCFS) { process01DCFS(); }
        }

        void process01EntropyAlgo()
        {
            this.dt_ready = TheTool.dataTable_partitize(dt_raw_exclude, partition_range);//01 partitize
            //------------------------------------------------------------------------------
            this.EnAlgo_first_list_F = TheTool.dataTable_getColList(dt_raw_exclude);//unselected feature (pointed by id)
            this.list_entropy = TheEntropy.calEntropy(dt_ready);
            this.list_MI = TheEntropy.calMIList(dt_ready, list_entropy);//for reuse MI
            //Feature Selection Start : get first --------------------------------------
            String fi1 = TheEntropy.temp_colWithHigestEntropy;
            this.EnAlgo_first_list_F.Remove(fi1);
            this.EnAlgo_first_S = TheData.getFeature(fi1, dt_threshold_pose1, dt_threshold_pose2);//Initial Threshold is assigned here (Not optimized)
        }

        void process01DCFS()
        {
            //prepare
            this.DCFS_listDelta = getListDelta();
            this.DCFS_listDeltaCorrelation = TheTool.list_Copy(DCFS_listDelta);
            this.DCFS_first_list_F = TheTool.dataTable_getColList(dt_raw_exclude);
            this.DCFS_listCorrelation = getlistCorrelation();
            //pick first feature
            string fi1 = DCFS_listDelta_getMaxValue();
            this.DCFS_first_list_F.Remove(fi1);
            this.DCFS_first_S = TheData.getFeature(fi1, dt_threshold_pose1, dt_threshold_pose2);
            DCFS_listDelta_update(fi1);
        }

        List<FeatureDelta> getListDelta()
        {
            List<FeatureDelta> output = new List<FeatureDelta>();
            foreach (DataColumn dc in dt_threshold_pose1.Columns)
            {
                string c = dc.ColumnName;
                if (list_exclude.Contains(c) == false)
                {
                    FeatureDelta f = new FeatureDelta();
                    f.name = c;
                    double start = TheTool.dataTable_getAverage(dt_threshold_pose1, c);
                    double end = TheTool.dataTable_getAverage(dt_threshold_pose2, c);
                    f.delta = Math.Abs(end - start);
                    output.Add(f);
                }
            }
            return output;
        }

        List<str2_double> getlistCorrelation()
        {
            List<str2_double> output = new List<str2_double>();
            try
            {
                foreach (List<String> group in TheUKI.list_bodyGroup_full)//List of Redundantable
                {
                    string[] feature = group.ToArray();
                    for (int i = 0; i < feature.Count(); i++)
                    {
                        string s1 = ""; string s2 = "";
                        if (i == feature.Count() - 1) { s1 = feature[i]; s2 = feature[0]; }
                        else { s1 = feature[i]; s2 = feature[i + 1]; }
                        str2_double new_data = new str2_double();
                        new_data.str1 = s1;
                        new_data.str2 = s2;
                        new_data.v = TheTool.LinearRegression_Cal_Correlation(this.dt_raw_exclude, s1, s2);
                        output.Add(new_data);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return output;
        }

        public List<String> score_regression_summary = new List<String>();

        //path_prefix = "" if no export needed
        //looptime is negative in case auto
        public List<Feature> process02_selectCandidate(string path_prefix, int methodSelection, int loopTime)
        {
            // F: threshold not needed, so just string
            // S: threshold is needed, so use Feature
            this.score_regression_summary = new List<String>();//keep summary
            String selected;
            List<string> local_list_F = new List<string>();
            List<Feature> local_list_S0 = new List<Feature>();
            List<string> local_list_S = new List<string>();
            List<m_If> local_rules_forTest = new List<m_If>();
            double best_scoreTRUE = 0; double best_scoreFALSE = 0; double best_scoreTotal = 0;
            //-- add first feature----------
            Feature local_first_S = new Feature();
            if (methodSelection == TheEntropy.methodSelection_DCFS)
            {
                local_list_F.AddRange(DCFS_first_list_F);
                local_first_S = this.DCFS_first_S.Copy();
            }
            else {
                local_list_F.AddRange(EnAlgo_first_list_F);
                local_first_S = this.EnAlgo_first_S.Copy(); 
            }
            selected = local_first_S.name;
            process02sub_optimizeFeature(local_rules_forTest, ref local_first_S, ref best_scoreTRUE, ref best_scoreFALSE, ref best_scoreTotal);
            process02sub_checkGoOn(best_scoreTRUE, best_scoreFALSE, best_scoreTotal);//get initial score
            local_list_S0.Add(local_first_S);//to add threshold
            local_list_S.Add(selected);
            local_rules_forTest.Add(TheMapData.convertFeature_to_If(local_first_S));
            //-------------------
            if (selection_autoNumber) { loopTime = local_list_F.Count(); }
            for (int i = 2; i < loopTime + 2; i++)
            {
                if (local_list_F.Count == 0) { break; }//out of data
                if (methodSelection == TheEntropy.methodSelection_DCFS)
                {
                    selected = DCFS_listDelta_getMaxValue();
                    if (path_prefix != "")
                    {
                        TheTool.exportCSV_orTXT(path_prefix + i + ".csv", DCFS_listDelta_getListString(true), false);
                    } 
                }
                else {
                    selected = TheEntropy.select1Candidate_entropy(methodSelection, dt_ready, local_list_F, local_list_S, list_entropy, list_MI);
                    if (path_prefix != "")
                    {
                        TheTool.exportCSV_orTXT(path_prefix + i + ".csv", TheTool.str_double_getListString(TheEntropy.temp_listEntropy), false);
                    } 
                }
                Feature selected0 = TheData.getFeature(selected, dt_threshold_pose1, dt_threshold_pose2);
                process02sub_optimizeFeature(local_rules_forTest, ref selected0, ref best_scoreTRUE, ref best_scoreFALSE, ref best_scoreTotal);
                Boolean goOn = process02sub_checkGoOn(best_scoreTRUE, best_scoreFALSE, best_scoreTotal);
                if (methodSelection == TheEntropy.methodSelection_DCFS)
                {
                    DCFS_listDelta_update(selected);
                }
                if (selection_autoNumber == false || goOn)
                {
                    local_list_F.Remove(selected);
                    local_list_S0.Add(selected0);
                    local_list_S.Add(selected);
                    local_rules_forTest.Add(TheMapData.convertFeature_to_If(selected0));
                }
                else
                {
                    break;
                }
            }
            return local_list_S0;
        }

        //as a result new_f.v is optimized
        //current_rules remain the same as Feature is removed after testing
        //MAIN Output : new_f with optimized v
        void process02sub_optimizeFeature(List<m_If> current_rules, ref Feature new_f
            , ref double best_scoreTRUE, ref double best_scoreFALSE, ref double best_scoreTotal)
        {
            double best_threshold = new_f.v;
            current_rules.Add(TheMapData.convertFeature_to_If(new_f));
            process02sub_computeScore(current_rules, ref best_scoreTRUE, ref best_scoreFALSE, ref best_scoreTotal);
            current_rules.RemoveAt(current_rules.Count - 1);
            int feature_num = current_rules.Count + 1;
            score_regression_summary.Add(feature_num + "," + new_f.name + "," + new_f.v 
                + "," + best_scoreTotal + "," + best_scoreTRUE + "," + best_scoreFALSE);
            //-----------
            if (selection_optimizeThreshold)
            {
                if (feature_num > 1 && selection_avoidGabage && best_scoreTotal < previous_scoreTotal)
                { 
                    //first threshold show that this feature could drop the performance
                    //do not enhance this feature, just eliminate let's further process eliminate it
                }
                else
                {
                    while (best_scoreTotal < 100)
                    {
                        Feature f_a = new Feature();
                        f_a.name = new_f.name;
                        f_a.opt = new_f.opt;
                        f_a.v = Math.Round(new_f.v + new_f.momentum, 2);
                        double f_a_scoreTRUE = 0; double f_a_scoreFALSE = 0; double f_a_scoreTotal = 0;
                        current_rules.Add(TheMapData.convertFeature_to_If(f_a));
                        process02sub_computeScore(current_rules, ref f_a_scoreTRUE, ref f_a_scoreFALSE, ref f_a_scoreTotal);
                        current_rules.RemoveAt(current_rules.Count - 1);
                        this.score_regression_summary.Add(",," + f_a.v + "," + f_a_scoreTotal + "," + f_a_scoreTRUE + "," + f_a_scoreFALSE);
                        //
                        Feature f_b = new Feature();
                        f_b.name = new_f.name;
                        f_b.opt = new_f.opt;
                        f_b.v = Math.Round(new_f.v - new_f.momentum, 2);
                        double f_b_scoreTRUE = 0; double f_b_scoreFALSE = 0; double f_b_scoreTotal = 0;
                        current_rules.Add(TheMapData.convertFeature_to_If(f_b));
                        process02sub_computeScore(current_rules, ref f_b_scoreTRUE, ref f_b_scoreFALSE, ref f_b_scoreTotal);
                        current_rules.RemoveAt(current_rules.Count - 1);
                        this.score_regression_summary.Add(",," + f_b.v + "," + f_b_scoreTotal + "," + f_b_scoreTRUE + "," + f_b_scoreFALSE);
                        //
                        if (f_a_scoreTotal <= best_scoreTotal && f_b_scoreTotal <= best_scoreTotal)
                        {
                            break;
                        }
                        else
                        {
                            //At least one of them better than current threshold
                            if (f_a_scoreTotal > f_b_scoreTotal)
                            {
                                best_scoreTotal = f_a_scoreTotal;
                                best_scoreTRUE = f_a_scoreTRUE;
                                best_scoreFALSE = f_a_scoreFALSE;
                                new_f.v = f_a.v;
                            }
                            else
                            {
                                best_scoreTotal = f_b_scoreTotal;
                                best_scoreTRUE = f_b_scoreTRUE;
                                best_scoreFALSE = f_b_scoreFALSE;
                                new_f.v = f_b.v;
                            }
                            new_f.momentum /= 2;
                            if (new_f.momentum <= .01) { new_f.momentum = .01; }
                            new_f.momentum = Math.Round(new_f.momentum, 2);
                        }
                    }
                }
            }
        }

        void process02sub_computeScore(List<m_If> rules, ref double this_scoreTRUE, ref double this_scoreFALSE, ref double this_scoreTotal)
        {
            this_scoreTRUE = computeDetectionRate(rules, inst_true);
            this_scoreFALSE = computeDetectionRate(rules, inst_false);
            this_scoreTotal = this_scoreTRUE - this_scoreFALSE;
        }

        double previous_scoreTotal = 0;
        double previous_scoreTRUE = 0;
        double previous_scoreFALSE = 0;

        //check whether select more candidate

        Boolean process02sub_checkGoOn(double this_scoreTRUE, double this_scoreFALSE, double this_scoreTotal)
        {
            Boolean goOn = true;
            if (this_scoreTRUE < previous_scoreTRUE && selection_maxTrue) {  goOn = false; }
            else if (this_scoreTotal <= previous_scoreTotal + selection_tradeoff_threshold && selection_minNumber) { goOn = false; }
            else if (this_scoreTotal < previous_scoreTotal) { goOn = false; }
            //------------------------------
            previous_scoreTotal = this_scoreTotal;
            previous_scoreTRUE = this_scoreTRUE;
            previous_scoreFALSE = this_scoreFALSE;
            return goOn;
        }


        public int computeDetectionRate(List<m_If> rules, List<Instance> data_list)
        {
            int score = 0;
            if (data_list.Count > 0)
            {
                int inst_count = 0;
                foreach (Instance d in data_list)
                {
                    if (TheRuleTester.testDetectMotion(d.getDataRaw(true), rules))
                    {
                        score++;
                    }
                    inst_count++;
                }
                score *= 100;
                score /= inst_count;
            }
            return score;
        }


        //get max value, feature must be in F list
        string DCFS_listDelta_getMaxValue()
        {
            FeatureDelta f = new FeatureDelta();
            foreach (FeatureDelta f0 in DCFS_listDeltaCorrelation)
            {
                if (f.name == "" || f0.delta > f.delta) { f = f0; }
            }
            if (f.name == "") { TheSys.showError("DCFS_listDelta_getMaxValue() : " + DCFS_listDeltaCorrelation.Count + " >> " + f.name); }
            return f.name;
        }

        //afterCorrelation = false >> get initial
        public List<string> DCFS_listDelta_getListString(Boolean afterCorrelation)
        {
            List<string> output = new List<string>();
            string header = "";
            string data = "";
            List<FeatureDelta> list_f = DCFS_listDeltaCorrelation;
            if (!afterCorrelation) { list_f = DCFS_listDelta; }
            foreach (FeatureDelta f in list_f)
            {
                header += f.name;
                data += f.delta;
                if (f != DCFS_listDeltaCorrelation.Last())
                {
                    header += ",";
                    data += ",";
                }
            }
            output.Add(header);
            output.Add(data);
            return output;
        }

        
    }

    //*********************************************************************

    class TheModelGenerator
    {
        public static void motionModel_add1Posture(m_Motion target, List<m_If> rule)
        {
            if (target.inputs.Count > 0)
            {
                target.inputs.Add(TheMapData.get_m_If_TimeWithin(1)); //Add Time Within
            }
            target.inputs.AddRange(rule);
        }

        public static string[] algo = new string[] { "Greedy", "MIFS", "CMIM", "JMIM" };

    }

    
}
