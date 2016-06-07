using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using weka.classifiers.misc;
using java.io;
using weka.core;

namespace P_Tracker2
{
    class TheProlongSitDetector
    {
        public static int colClass = 0;//column index where Class is

        static public SerializedClassifier classifier = new SerializedClassifier();
        public static string path_prefix = "";
        public static string path_model = "";
        public static string path_file_test = "";

        public static void initializePath()
        {
            path_prefix = TheURL.url_saveFolder + TheURL.url_9_PSD + @"\";
            path_model = path_prefix + "BPNNNoN.model";
            path_file_test = path_prefix + "Test.arff";
        }

        //Classify : 0 BPNN , 1 Bay , 2 J48 , 3 K5
        //Normaliz : 0 NoN , 1 GMM , 2 PMM
        public static void setupModel(int method_classify, int method_normalize){
            try
            {
                path_model = path_prefix;
                if (method_classify == 2) { path_model += "Bay"; }
                else if (method_classify == 3) { path_model += "J48"; }
                else if (method_classify == 4) { path_model += "K5"; }
                else { path_model += "BPNN"; }
                if (method_classify == 2) { path_model += "GMM"; }
                else if (method_classify == 3) { path_model += "PMM"; }
                else { path_model += "NoN"; }
                path_model += ".model";
                classifier.setModelFile(new File(path_model));
            }
            catch {  }
        }
        //** ERROR **

        static public string classify()
        {
            try
            {
                Instances inst = TheWeka.createInstance(path_file_test);
                return TheWeka.do_Classification_bySerialClassfier_1out_standAlone(classifier, inst, colClass);
            }
            catch (Exception ex) { TheSys.showError(ex.ToString(), true); return ""; }
        }

        static public string test(string data)
        {
            exportForTest(data);
            return classify();
        }

        //Single Data
        public static List<string> textList = new List<string>();
        public static void exportForTest(string data)
        {
            try
            {
                textList.Clear();
                textList.Add(@"@relation 'test'");
                textList.Add(@"@attribute class2 {M,S}");
                textList.Add(@"@attribute Head_Dist_Avg numeric");
                textList.Add(@"@attribute ElbowLeft_Dist_Avg numeric");
                textList.Add(@"@attribute ElbowRight_Dist_Avg numeric");
                textList.Add(@"@data");
                textList.Add(data);
                TheTool.exportFile(textList, path_file_test, false);
            }
            catch (Exception ex) { TheSys.showError(ex.ToString(), true); }
        }


        
    }
}
