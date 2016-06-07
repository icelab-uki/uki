using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace P_Tracker2
{
    class TheURL
    {
        //Hirachy : only this level end with "/"
        public static string url_0_root = ""; //e.g. ".../"
        public static string url_1_sound = @"sound\";
        public static string url_1_speech = @"speech\";
        public static string url_1_greenScrn = @"greenScrn\";
        public static string url_1_sys = @"sys\";
        //
        public static string url_2_ows = @"[OWS]\";
        public static string url_2_uki = @"[UKI]\";
        public static string url_2_sampleData = @"[SampleData]\";
        public static string url_2_ukiAnalysis = @"[UKI-Analysis]\";
        public static string url_2_ukiEntropy = @"[Entropy-Analysis]\";

        //
        public static string url_saveFolder = "";// e.g. "file\"
        public static string url_saveFolder0 = "";// e.g. "file"

        //Standalone : under 'url_saveFolder'
        public static string url_9_ukiInst = "[UKI-Inst]";
        public static string url_9_DelCol = "[DelCol]";
        public static string url_9_Canon = "[Canon]";
        public static string url_9_NameCode = "[NameCode]";
        public static string url_9_PAnalysis = "[P-Analysis]";
        public static string url_9_PSD = "[PSD]";
        public static string url_9_Train = "[Train]";
        public static string url_9_UKIMap = "[UKI-Map]";
        public static string url_9_Convert = "[Convert]";
        //----------
        public static string url_ows_GraphHB = "GraphHB.jpg";
        public static string url_ows_Handbook = "Handbook.docx";
        public static string url_ows_Handbook_tech = "Handbook technical.docx";
        public static string url_ows_Thesis = "Thesis.pdf";
        public static string url_ows_ECTI = "ECTI.pdf";
        public static string url_ows_APCC = "APCC.pdf";
        //
        public static string url_uki_Handbook = "Handbook.docx";
        public static string url_uki_CIG2015 = "CIG2015.pdf";
        public static string url_uki_GCCE2015_1 = "GCCE2015_1.pdf";
        public static string url_uki_GCCE2015_2 = "GCCE2015_2.pdf";

        public static string url_uki_motionDB = "[Motions].xml";
        public static string url_uki_eventDB = "[Events].xml";
        
        //-----------------
        public static string url_config = "config.txt";
        public static string dm_path_file = "test.arff";
        public static string totalView_path_saveRoot = @"file\[P-Analysis]\";
        public static string url_config_FTG = "configFTG.txt";
        //
        public static string url_tv_sample_msr = "MSR.txt";
        public static string url_tv_sample_raw = "RAW.csv";
        public static string url_tv_sample_raw_ows = "RAW_OWS";
        public static string url_libsvm_model = @"facemodel\";

        public static string url_FTGautoSide = @"C:\side.txt";

        public static void initializeURL()
        {
            string path = MySetting.savepath;
            path = path.Replace(@"file\", @"file");//prevent common Error
            if (Directory.Exists(MySetting.savepath))
            {
                url_saveFolder0 = path;
                url_saveFolder = path + @"\";
                dm_path_file = path;
            }
            else
            {
                url_saveFolder0 = url_0_root + "file";
                url_saveFolder = url_saveFolder0 + @"\";
                //TheSys.showError("User-defined save path does not exist (" + path + ")";
                //TheSys.showError("Set " + url_saveFolder0 + " as save folder");
            }
            dm_path_file = url_saveFolder + url_9_Train + @"\" + dm_path_file;
            totalView_path_saveRoot = url_saveFolder + url_9_PAnalysis + @"\";
        }


        //----------------------

        public static string sound_alert = @"alert.wav";
        public static string s_timeBreak = @"t_break.wav";
        public static string s_lv0 = @"lv0.wav";
        public static string s_lv1 = @"lv1.wav";
        public static string s_lv2 = @"lv2.wav";
        public static string s_p_correct = @"p_correct.wav";
        public static string s_p_wrong = @"p_wrong.wav";
        //---------
        public static string sp_welcome = @"Welcome.wav";
        public static string sp_searchSkel = @"searchSkel.wav";
        public static string sp_baseReg = @"BaseReg.wav";
        public static string sp_break1 = @"Break1.wav";
        public static string sp_break2 = @"Break2.wav";
        public static string sp_lv1 = @"lv1.wav";
        public static string sp_lv2 = @"lv2.wav";
        public static string sp_pitch = @"pitch.wav";
        public static string sp_twistL = @"twistL.wav";
        public static string sp_twistR = @"twistR.wav";
        public static string sp_niceBreak = @"NiceBreak.wav";
        public static string sp_welcomeBack = @"WelcomeBack.wav";

        public static void initializeURL_permanent()
        {
            sound_alert = url_0_root + url_1_sound + @"\" + sound_alert;
            s_timeBreak = url_0_root + url_1_sound + @"\" + s_timeBreak;
            s_lv0 = url_0_root + url_1_sound + @"\" + s_lv0;
            s_lv1 = url_0_root + url_1_sound + @"\" + s_lv1;
            s_lv2 = url_0_root + url_1_sound + @"\" + s_lv2;
            s_p_correct = url_0_root + url_1_sound + @"\" + s_p_correct;
            s_p_wrong = url_0_root + url_1_sound + @"\" + s_p_wrong;
            //---------
            sp_welcome = url_0_root + url_1_speech + @"\" + sp_welcome;
            sp_searchSkel = url_0_root + url_1_speech + @"\" + sp_searchSkel;
            sp_baseReg = url_0_root + url_1_speech + @"\" + sp_baseReg;
            sp_break1 = url_0_root + url_1_speech + @"\" + sp_break1;
            sp_break2 = url_0_root + url_1_speech + @"\" + sp_break2;
            sp_lv1 = url_0_root + url_1_speech + @"\" + sp_lv1;
            sp_lv2 = url_0_root + url_1_speech + @"\" + sp_lv2;
            sp_pitch = url_0_root + url_1_speech + @"\" + sp_pitch;
            sp_twistL = url_0_root + url_1_speech + @"\" + sp_twistL;
            sp_twistR = url_0_root + url_1_speech + @"\" + sp_twistR;
            sp_niceBreak = url_0_root + url_1_speech + @"\" + sp_niceBreak;
            sp_welcomeBack = url_0_root + url_1_speech + @"\" + sp_welcomeBack;
            //-------
            url_ows_GraphHB = url_0_root + url_1_sys + url_2_ows + url_ows_GraphHB;
            url_ows_Handbook = url_0_root + url_1_sys + url_2_ows + url_ows_Handbook;
            url_ows_Handbook_tech = url_0_root + url_1_sys + url_2_ows + url_ows_Handbook_tech;
            url_ows_Thesis = url_0_root + url_1_sys + url_2_ows + url_ows_Thesis;
            url_ows_ECTI = url_0_root + url_1_sys + url_2_ows + url_ows_ECTI;
            url_ows_APCC = url_0_root + url_1_sys + url_2_ows + url_ows_APCC;
            //-------
            url_uki_Handbook = url_0_root + url_1_sys + url_2_uki + url_uki_Handbook;
            url_uki_CIG2015 = url_0_root + url_1_sys + url_2_uki + url_uki_CIG2015;
            url_uki_GCCE2015_1 = url_0_root + url_1_sys + url_2_uki + url_uki_GCCE2015_1;
            url_uki_GCCE2015_2 = url_0_root + url_1_sys + url_2_uki + url_uki_GCCE2015_2;
            //-------
            url_config_FTG = url_0_root + url_config_FTG;
            //------
            url_tv_sample_msr = url_1_sys + url_2_sampleData + url_tv_sample_msr;
            url_tv_sample_raw = url_1_sys + url_2_sampleData + url_tv_sample_raw;
            url_tv_sample_raw_ows = url_1_sys + url_2_sampleData + url_tv_sample_raw_ows;
            //------
            url_uki_motionDB = url_saveFolder + url_9_UKIMap + @"\" + url_uki_motionDB;
            url_uki_eventDB = url_saveFolder + url_9_UKIMap + @"\" + url_uki_eventDB;

        }

        


    }
}
