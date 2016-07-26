using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class UKI_ThePreprocessor
    {
        public static int centerTechnique = TheUKI.centerTechq_SC_HC;

        //INPUT : raw or raw concat
        public static DataTable getDatatable_centered(List<UKI_DataRaw> list_raw, Boolean extraColumn)
        {
            List<UKI_DataRaw> list_raw_centered = TheUKI.raw_centerBodyJoint(list_raw, centerTechnique);
            DataTable dt_raw_center = TheUKI.convert_UKI_DataRaw_toDataTable(list_raw_centered, centerTechnique, extraColumn);//raw
            return dt_raw_center;
        }

        public static List<string> temp_summary = new List<string>();
        public static int[] temp_mode = new int[2];//most occurance value

        //List<DataTable> dt_sequence : 1 DataTable = Concated Sequences of 1 Posture (For Entropy Analysis)
        //List<DataTable> dt_threshold : 1 DataTable = 1 Pose (1 Row for 1 Inst) >> List<DataTable> = Motion
        //Hadoulen #dt_sequence = 2 , #dt_threshold = 3
        //Assumption: all given Instance has same class
        public static List<DataTable> preprocess_CombinedSegmented(List<Instance> list_inst, Boolean extraColumn,
            ref List<DataTable> dt_sequence, ref List<DataTable> dt_threshold)
        {
            List<DataTable> output = new List<DataTable>();
            temp_summary.Clear();
            //-- Check Pose consist in each Instance ---------
            List<int> count_keyPose = new List<int>();
            foreach (Instance inst in list_inst)
            {
                int p_count = inst.getKeyPose().Count();
                count_keyPose.Add(p_count);
                temp_summary.Add("- " + inst.name + " : " + (p_count - 1) + " Postures");
            }
            List<int[]> occurance_counter = TheTool.listInt_countOccurance(count_keyPose);
            temp_mode = TheTool.findMode(occurance_counter);// int [ Number of Keypose , Occurance ] (Hadouken = 3)
            int key_count = temp_mode[0]; //(Hadouken = 3)
            int pose_count = key_count - 1; //(Hadouken = 2)
            temp_summary.Add("Most Common: " + pose_count + " Postures");
            //-- Build Concat DataRaw for each Posture
            if (temp_mode[1] < 1) {
                foreach (Instance inst in list_inst) { TheSys.showError(inst.name + " : " + inst.getKeyPose().Count()); }
                TheSys.showError("Cannot find Most Common Number-of-Postures"); 
            }
            else
            {
                //-- Prepare Concat Table
                List<UKI_DataRaw>[] list_raw_seq = new List<UKI_DataRaw>[pose_count];
                List<UKI_DataRaw>[] list_raw_threshold = new List<UKI_DataRaw>[key_count];
                for (int i = 0; i < list_raw_seq.Count(); i++) { list_raw_seq[i] = new List<UKI_DataRaw>(); }
                for (int i = 0; i < list_raw_threshold.Count(); i++) { list_raw_threshold[i] = new List<UKI_DataRaw>(); }
                foreach (Instance inst in list_inst)
                {
                    if (inst.keyPose.Count() == temp_mode[0])
                    {
                        int key_number = 0;
                        foreach (int[] keyPose in inst.getKeyPose())
                        {
                            if (key_number > 0)
                            {
                                List<UKI_DataRaw> selectedRange = TheUKI.UKI_DataRaw_selectRow(inst.getDataRaw(extraColumn), keyPose[0], keyPose[1]);
                                list_raw_seq[key_number - 1].AddRange(selectedRange);
                                if (key_number == 1)
                                {
                                    list_raw_threshold[0].Add(selectedRange.First());
                                }
                                list_raw_threshold[key_number].Add(selectedRange.Last());
                            }
                            key_number++;
                        }
                    }
                }
                foreach (List<UKI_DataRaw> sequences in list_raw_seq)
                {
                    dt_sequence.Add(getDatatable_centered(sequences, extraColumn));
                }
                foreach (List<UKI_DataRaw> thresholds in list_raw_threshold)
                {
                    dt_threshold.Add(getDatatable_centered(thresholds, extraColumn));
                }
            }
            return output;
        }

    }
}
