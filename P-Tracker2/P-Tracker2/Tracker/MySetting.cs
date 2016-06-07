using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace P_Tracker2
{
    class MySetting
    {
        public static Boolean loadSetting = false;//if load successfully
        public static UserTracker userTracker = null;

        public static void viewSetting()
        {
            try
            {
                Process.Start(TheURL.url_config);
            }
            catch { }
        }


        public static void readSetting()
        {
            try
            {
                List<string> stringList = TheTool.read_File_getListString(TheURL.url_config);
                foreach (string s in stringList){storeSetting(s);}
                loadSetting = true;
            }
            catch { TheSys.showError("Error Read Setting : delete config.txt to solve this problem"); }
        }

        //========================================================
        public static string savepath = "file";
        public static string mute = "0";
        //Kinect Mode
        public static string kAngle = "0";//Camera Angle
        public static string kCheck_seat = "0";
        public static string kCheck_close = "0";
        public static string kCheck_face = "0";
        public static string kCheck_flip = "0";
        //Kinect Smooth
        public static string kSmooth = "0";
        public static string kSmooth_S = "0";
        public static string kSmooth_P = "0";
        public static string kSmooth_C = "0";
        public static string kSmooth_J = "0";
        public static string kSmooth_M = "0";
        //        
        public static string hideTableData = "1";
        public static string hideDebug = "0";
        //
        public static string v1 = "0";
        public static string v2 = "0";
        public static string v3 = "0";
        public static string v4 = "0";
        //
        public static string view1_j = "0";
        public static string view1_b = "0";
        public static string view1_e = "0";
        public static string view2_j = "0";
        public static string view2_b = "0";
        public static string view2_e = "0";
        public static string view3_j = "0";
        public static string view3_b = "0";
        public static string view3_e = "0";
        public static string view4_j = "0";
        public static string view4_b = "0";
        public static string view4_e = "0";
        //--------
        public static string stream = "0";
        public static string follow = "0";
        public static string backup = "300";
        public static string digit = "0";

        //--------
        public static string d_onTop = "0";
        public static string d_POS = "0";
        public static string speech_off = "0";
        public static string alarm_off = "0";
        public static string d_concensus = "15";
        public static string d_BPcooldown = "10";

        public static void saveSetting()
        {
            try
            {
                List<string> stringList = new List<string>() { };
                stringList.Add("savepath:" + TheURL.url_saveFolder);
                stringList.Add("mute:" + TheTool.convertBoolean_01(userTracker.checkMute.IsChecked.Value));
                //--------
                stringList.Add("kAngle:" + Math.Round(userTracker.slideAngle.Value));
                stringList.Add("kSeat:" + TheTool.convertBoolean_01(userTracker.checkSeat.IsChecked.Value));
                stringList.Add("kClose:" + TheTool.convertBoolean_01(userTracker.checkClose.IsChecked.Value));
                stringList.Add("kFace:" + TheTool.convertBoolean_01(userTracker.checkFace.IsChecked.Value));
                stringList.Add("kFlip:" + TheTool.convertBoolean_01(userTracker.checkFlip.IsChecked.Value));
                //--------
                stringList.Add("kSmooth:" + TheTool.convertBoolean_01(userTracker.checkSmooth.IsChecked.Value));
                stringList.Add("kSmooth_S:" + Math.Round(userTracker.slide_Smooth.Value, 1));
                stringList.Add("kSmooth_P:" + Math.Round(userTracker.slide_Predict.Value, 1));
                stringList.Add("kSmooth_C:" + Math.Round(userTracker.slide_Correct.Value, 1));
                stringList.Add("kSmooth_J:" + Math.Round(userTracker.slide_Jitter.Value, 2));
                stringList.Add("kSmooth_M:" + Math.Round(userTracker.slide_Davia.Value, 2));
                //--------
                stringList.Add("hideData:" + TheTool.convertBoolean_01(userTracker.checkHide.IsChecked.Value));
                stringList.Add("hideDebug:" + TheTool.convertBoolean_01(userTracker.checkHideDebug.IsChecked.Value));
                //--------
                stringList.Add("view1:" + userTracker.comboV1.SelectedIndex);
                stringList.Add("view2:" + userTracker.comboV2.SelectedIndex);
                stringList.Add("view3:" + userTracker.comboV2.SelectedIndex);
                stringList.Add("view4:" + userTracker.comboV2.SelectedIndex);
                stringList.Add("view1_j:" + TheTool.convertBoolean_01(userTracker.checkVJoint1.IsChecked.Value));
                stringList.Add("view1_b:" + TheTool.convertBoolean_01(userTracker.checkVBone1.IsChecked.Value));
                stringList.Add("view1_e:" + TheTool.convertBoolean_01(userTracker.checkVEdge1.IsChecked.Value));
                stringList.Add("view2_j:" + TheTool.convertBoolean_01(userTracker.checkVJoint2.IsChecked.Value));
                stringList.Add("view2_b:" + TheTool.convertBoolean_01(userTracker.checkVBone2.IsChecked.Value));
                stringList.Add("view2_e:" + TheTool.convertBoolean_01(userTracker.checkVEdge2.IsChecked.Value));
                stringList.Add("view3_j:" + TheTool.convertBoolean_01(userTracker.checkVJoint3.IsChecked.Value));
                stringList.Add("view3_b:" + TheTool.convertBoolean_01(userTracker.checkVBone3.IsChecked.Value));
                stringList.Add("view3_e:" + TheTool.convertBoolean_01(userTracker.checkVEdge3.IsChecked.Value));
                stringList.Add("view4_j:" + TheTool.convertBoolean_01(userTracker.checkVJoint4.IsChecked.Value));
                stringList.Add("view4_b:" + TheTool.convertBoolean_01(userTracker.checkVBone4.IsChecked.Value));
                stringList.Add("view4_e:" + TheTool.convertBoolean_01(userTracker.checkVEdge4.IsChecked.Value));
                //--------
                stringList.Add("stream:" + userTracker.comboStream.SelectedIndex);
                stringList.Add("follow:" + userTracker.comboFollow.SelectedIndex);
                stringList.Add("backup:" + userTracker.txtBackup.Text);
                stringList.Add("digit:" + userTracker.comboDecimal.SelectedIndex);
                //
                if (userTracker.form_Detector != null)
                {
                    stringList.Add("d_onTop:" + TheTool.convertBoolean_01(userTracker.form_Detector.checkOnTop.IsChecked.Value));
                    stringList.Add("POS_monitor:" + TheTool.convertBoolean_01(userTracker.form_Detector.checkPOSmonitor.IsChecked.Value));
                    stringList.Add("speech_off:" + TheTool.convertBoolean_01(userTracker.form_Detector.checkSpeech.IsChecked.Value));
                    stringList.Add("alarm_off:" + TheTool.convertBoolean_01(userTracker.form_Detector.checkSound.IsChecked.Value));
                    stringList.Add("consensus:" + userTracker.form_Detector.txtFrameAgree.Text);
                    stringList.Add("BPcooldown:" + userTracker.form_Detector.txtWait.Text);
                }
                TheTool.writeFile(stringList, TheURL.url_config, true);
            }
            catch { TheSys.showError("Error Save Setting"); }
        }
        

        public static void storeSetting(string fullString)
        {
            storeSetting(ref savepath, fullString, "savepath:");
            storeSetting(ref mute, fullString, "mute:");
            //--------
            storeSetting(ref kAngle, fullString, "kAngle:");
            storeSetting(ref kCheck_seat, fullString, "kSeat:");
            storeSetting(ref kCheck_close, fullString, "kClose:");
            storeSetting(ref kCheck_face, fullString, "kFace:");
            storeSetting(ref kCheck_flip, fullString, "kFlip:");
            //Kinect Smooth
            storeSetting(ref kSmooth, fullString, "kSmooth:");
            storeSetting(ref kSmooth_S, fullString, "kSmooth_S:");
            storeSetting(ref kSmooth_P, fullString, "kSmooth_P:");
            storeSetting(ref kSmooth_C, fullString, "kSmooth_C:");
            storeSetting(ref kSmooth_J, fullString, "kSmooth_J:");
            storeSetting(ref kSmooth_M, fullString, "kSmooth_M:");
            //
            storeSetting(ref hideTableData, fullString, "hideData:");
            storeSetting(ref hideDebug, fullString, "hideDebug:");
            //
            storeSetting(ref v1, fullString, "view1:");
            storeSetting(ref v2, fullString, "view2:");
            storeSetting(ref v3, fullString, "view3:");
            storeSetting(ref v4, fullString, "view4:");
            //--------
            storeSetting(ref view1_j, fullString, "view1_j:");
            storeSetting(ref view1_b, fullString, "view1_b:");
            storeSetting(ref view1_e, fullString, "view1_e:");
            storeSetting(ref view2_j, fullString, "view2_j:");
            storeSetting(ref view2_b, fullString, "view2_b:");
            storeSetting(ref view2_e, fullString, "view2_e:");
            storeSetting(ref view3_j, fullString, "view3_j:");
            storeSetting(ref view3_b, fullString, "view3_b:");
            storeSetting(ref view3_e, fullString, "view3_e:");
            storeSetting(ref view4_j, fullString, "view4_j:");
            storeSetting(ref view4_b, fullString, "view4_b:");
            storeSetting(ref view4_e, fullString, "view4_e:");
            //--------
            storeSetting(ref stream, fullString, "stream:");
            storeSetting(ref follow, fullString, "follow:");
            storeSetting(ref backup, fullString, "backup:");
            storeSetting(ref digit, fullString, "digit:");
            //--------
            storeSetting(ref d_onTop, fullString, "d_onTop:");
            storeSetting(ref d_POS, fullString, "POS_monitor:");
            storeSetting(ref speech_off, fullString, "speech_off:");
            storeSetting(ref alarm_off, fullString, "alarm_off:");
            storeSetting(ref d_concensus, fullString, "consensus:");
            storeSetting(ref d_BPcooldown, fullString, "BPcooldown:");

        }

        public static void storeSetting(ref string store, string fullString, string prefix)
        {
            string s;
            s = getText_afterPrefix(fullString, prefix);
            if (s != "") { store = s; }
        }

        public static string getText_afterPrefix(string fullString,string prefix)
        {
            string s = "";
            string[] split = fullString.Split(new string[] { prefix }, StringSplitOptions.None);
            if (split.Length > 1)
            {
                s = split[1];
            }
            return s;
        }

        public static void applySetting()
        {
        }

    }
}
