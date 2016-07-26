using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class ThePosExtract2
    {
        public static string path_saveFolder = "";

        public static double samAlgo_threshold = 0.01;
        public static List<int[]> extractKeyPose_SamAlgo(Instance inst)
        {
            List<int[]> list_keyPose_preCombine = new List<int[]>();
            List<int[]> list_keyPose_afterCombine = new List<int[]>();
            List<string> output_print = new List<string>();
            List<string> output_print2 = new List<string>();
            //----------
            List<UKI_DataDouble> list_data = TheUKI.UKI_DataDouble_ChangeBwFrame(inst.getDataNorm());
            List<UKI_DataDouble> list_data2 = TheUKI.UKI_DataDouble_MVA(list_data,25);
            list_keyPose_preCombine.AddRange(TheUKI.UKI_DataDouble_getKeyPose_CrossThreshold(list_data2,samAlgo_threshold));
            list_keyPose_preCombine = TheTool.listArray_sort(list_keyPose_preCombine, 0);
            list_keyPose_afterCombine = TheUKI.keyPose_Combine_longestPath(list_keyPose_preCombine);
            if (path_saveFolder != "")
            {
                output_print.Add("id,Head,ShoulderCenter,ShoulderLeft,ShoulderRight,ElbowLeft,ElbowRight,WristLeft,WristRight,HandLeft,HandRight,Spine,HipCenter,HipLeft,HipRight,KneeLeft,KneeRight,AnkleLeft,AnkleRight,FootLeft,FootRight");
                output_print.AddRange(TheUKI.UKI_DataDouble_convertToListString(list_data));
                TheTool.exportCSV_orTXT(path_saveFolder + @"/" + inst.name + " 1 (Change).csv", output_print, false);
                output_print2.Add("id,Head,ShoulderCenter,ShoulderLeft,ShoulderRight,ElbowLeft,ElbowRight,WristLeft,WristRight,HandLeft,HandRight,Spine,HipCenter,HipLeft,HipRight,KneeLeft,KneeRight,AnkleLeft,AnkleRight,FootLeft,FootRight");
                output_print2.AddRange(TheUKI.UKI_DataDouble_convertToListString(list_data2));
                TheTool.exportCSV_orTXT(path_saveFolder + @"/" + inst.name + " 2 (MVA).csv", output_print2, false);
                TheUKI.exportKey(path_saveFolder + @"/" + inst.name + " 1 (uncombined).key", list_keyPose_preCombine);
                TheUKI.exportKey(path_saveFolder + @"/" + inst.name + " 2 (combined).key", list_keyPose_afterCombine);
            }
            return list_keyPose_afterCombine;
        }

        //Modeling transition patterns between events for temporal human action segmentation and classification
        public static List<int[]> extractKeyPose_Angular(Instance inst)
        {
            List<int[]> list_keyPose = new List<int[]>();
            List<UKI_DataDouble> list_dataAngles = new List<UKI_DataDouble>();
            List<string> output_print = new List<string>();
            foreach (UKI_DataRaw d in inst.getDataRaw(false))
            {
                UKI_DataDouble dataAngles = new UKI_DataDouble();
                dataAngles.id = d.id;
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.Spine, d.ShoulderLeft, d.ElbowLeft));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.ShoulderLeft, d.ElbowLeft, d.WristLeft));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.Spine, d.ShoulderRight, d.ElbowRight));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.ShoulderRight, d.ElbowRight, d.WristRight));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.Spine, d.HipLeft, d.KneeLeft));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.HipLeft, d.KneeLeft, d.AnkleLeft));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.Spine, d.HipRight, d.KneeRight));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.HipLeft, d.KneeLeft, d.AnkleLeft));
                dataAngles.data.Add(ThePostureCal.calAngle_3Points(d.Head, d.ShoulderCenter, d.Spine));
            }
            if (path_saveFolder != "")
            {
                output_print.Add("id,ShouldL,ElbowL,ShouldR,ElbowR,HipL,KneeL,HipR,KneeR,Neck");
                output_print.AddRange(TheUKI.UKI_DataDouble_convertToListString(list_dataAngles));
                TheTool.exportCSV_orTXT(path_saveFolder + @"/" + inst.name + ".csv", output_print, false);
            }
            return list_keyPose;
        }


    }


}

