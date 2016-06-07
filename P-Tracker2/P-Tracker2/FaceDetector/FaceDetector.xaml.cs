using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using libsvm;
using System.IO;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Diagnostics;

namespace P_Tracker2
{

    public partial class FaceDetector : Window
    {
        public UserTracker mainForm;

        //The model of each features
        private svm_model[] AuModel = new svm_model[7];
        private svm_model[] FppModel = new svm_model[7];

        //The test data of each features
        private svm_node[] AuTestData;
        private svm_node[] FppTestData;

        //The range data in scaling
        private StreamReader[] Range = new StreamReader[2];

        public DateTime startTime = DateTime.Now;
        ///<summery>
        ///The dictionary of index and result of each emotion
        ///</summery>
        private Dictionary<int, List<int>> AuDictionary = new Dictionary<int, List<int>>() 
        { 
          {0 , new List<int>()}, 
          {1 , new List<int>()}, 
          {2 , new List<int>()}, 
          {3 , new List<int>()},
          {4 , new List<int>()},
          {5 , new List<int>()},
          {6 , new List<int>()}
        };

        ///<summary>
        ///The dictionary of index and result of each emotion
        /// </summary>
        private Dictionary<int, List<int>> FppDictionary = new Dictionary<int, List<int>>()
        { 
          {0 , new List<int>()}, 
          {1 , new List<int>()}, 
          {2 , new List<int>()}, 
          {3 , new List<int>()},
          {4 , new List<int>()},
          {5 , new List<int>()},
          {6 , new List<int>()}
        };

        ///<summery>
        ///The dictionary of fpp features using SVM
        ///</summery>
        private static Dictionary<String, int> FeaturePoint = new Dictionary<string, int>()
        {
            {"TopSkull", 0},
            {"TopRightForehead", 1},
            {"MiddleTopDipUpperLip", 7},
            {"AboveChin", 9},
            {"BottomOfChin", 10},
            {"RightOfRightEyebrow", 15},
            {"MiddleTopOfRightEyebrow", 16},
            {"LeftOfRightEyebrow", 17},
            {"MiddleBottomOfRightEyebrow", 18},       
            {"OuterCornerOfRightEye", 20},
            {"MiddleTopRightEyelid", 21},
            {"MiddleBottomRightEyelid", 22},
            {"InnerCornerRightEye", 23},     
            {"RightSideOfChin", 30},
            {"OutsideRightCornerMouth", 31},
            {"RightOfChin", 32},
            {"RightTopDipUpperLip", 33},
            {"TopLeftForehead", 34},
            {"MiddleTopLowerLip", 40},
            {"MiddleBottomLowerLip", 41},
            {"LeftOfLeftEyebrow", 48},
            {"MiddleTopOfLeftEyebrow", 49},
            {"RightOfLeftEyebrow", 50},
            {"MiddleBottomOfLeftEyebrow", 51},       
            {"OuterCornerOfLeftEye", 53},
            {"MiddleTopLeftEyelid", 54},
            {"MiddleBottomLeftEyelid", 55},
            {"InnerCornerLeftEye", 56},      
            {"LeftSideOfCheek", 63},
            {"OutsideLeftCornerMouth", 64},
            {"LeftOfChin", 65},
            {"LeftTopDipUpperLip", 66},
            {"RightTopUpperLip", 79},
            {"LeftTopUpperLip", 80},
            {"RightBottomUpperLip", 81},
            {"LeftBottomUpperLip", 82},
            {"RightTopLowerLip", 83},
            {"LeftTopLowerLip", 84},
            {"RightBottomLowerLip", 85},
            {"LeftBottomLowerLip", 86},
            {"MiddleBottomUpperLip", 87},
            {"LeftCornerMouth", 88},
            {"RightCornerMouth", 89},
            {"BottomOfRightCheek", 90},
            {"BottomOfLeftCheek", 91}
        };

        ///<summary>
        ///The index of each emotion
        ///</summary>
        private Dictionary<int, string> EmotionIndex = new Dictionary<int, string>()
        {
            {0, "Anger"},
            {1, "Disgust"},
            {2, "Fear"},
            {3, "Happiness"},
            {4, "Neutral"},
            {5, "Sadness"},
            {6, "Surprise"},
        };

        ///<summary>
        ///The parameters of each emotion AU model
        ///</summary>
        private Dictionary<int,List<double>> AuModelParameters = new Dictionary<int, List<double>>()
        {
            {0, new List<double>(){2048,8}},
            {1, new List<double>(){2048,8}},
            {2, new List<double>(){128,8}},
            {3, new List<double>(){512,8}},
            {4, new List<double>(){2048,8}},
            {5, new List<double>(){128,8}},
            {6, new List<double>(){8192,2}},
        };

        ///<summary>
        ///The parameters of each emotion FPP model
        ///</summary>
        private Dictionary<int, List<double>> FppModelParameters = new Dictionary<int, List<double>>()
        {
            {0, new List<double>(){2048,0.5}},
            {1, new List<double>(){32,8}},
            {2, new List<double>(){32,8}},
            {3, new List<double>(){8,8}},
            {4, new List<double>(){128,2}},
            {5, new List<double>(){128,2}},
            {6, new List<double>(){32,8}},
        };

        ///<summary>
        ///The label of each emotion in AU data  
        ///</summary>
        private int[] AuClassLabel;

        ///<summary>
        ///The label of each emotion in FPP data  
        ///</summary>
        private int[] FppClassLabel;

        ///<summary>
        ///The array of labels which result of classfication
        /// </summary>
        private int[] AuC;
        private int[] FppC;

        //Final estimation of emotion
        private int FinalEstimation;

        private const int MaxRecordFrame = 30;
        private const int MaxAuFeatures = 6;
        private int MaxFppFeatures = FeaturePoint.Count * 3;

        private double[,] AuScaleMin;
        private double[,] AuScaleMax;
        private double[,] FppScaleMin;
        private double[,] FppScaleMax;




        public FaceDetector(UserTracker mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;

            AuScaleMin = new double[7, MaxAuFeatures];
            AuScaleMax = new double[7, MaxAuFeatures];
            FppScaleMin = new double[7, MaxFppFeatures];
            FppScaleMax = new double[7, MaxFppFeatures];

            for (int i = 0; i < 7; i++)
            {
                //load model data of each emotion
                LoadModelData(i);
                //set c and g parameters
                SetParameters(i);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainForm.form_FaceDetector = null;
        }


        public void process(FaceData newData)
        {
            try
            {
                int time_change = (int)mainForm.thisTime.Subtract(fps_lastCheck).TotalSeconds;
                if (time_change >= 1)
                {
                    txtFps.Content = "fps " + fps_counter;
                    fps_lastCheck = mainForm.thisTime;
                    fps_counter = 0;
                }
                fps_counter++;
                //=====================================================
                if (checkRun.IsChecked.Value)
                {

                    //------ Test (Show Data) 以下に自分のコード記述--------------
                    //initialize class labels of each data
                    InitializeSVM();

                    //do classfication using SVM
                    TestSVM(newData);

                    for (int i = 0; i < 7; i++)
                    {
                        //Remove first element if dictionary size is more than equal 30.
                        if (AuDictionary[i].Count >= MaxRecordFrame)
                        {
                            AuDictionary[i].RemoveAt(0);
                        }
                        if (FppDictionary[i].Count >= MaxRecordFrame)
                        {
                            FppDictionary[i].RemoveAt(0);
                        }

                        //Add the result of classfication
                        AuDictionary[i].Add(AuClassLabel[i]);
                        FppDictionary[i].Add(FppClassLabel[i]);
                    }

                    //Sum labels
                    for (int i = 0; i < 7; i++)
                    {
                        AuC[i] = AuDictionary[i].Sum();
                        FppC[i] = FppDictionary[i].Sum();

                    }

                    //Get index which sum of labels is the highest
                    int[] AuMaxEmotionIndex = AuC.Select((item, index) => new { Index = index, Value = item })
                     .Where(item => item.Value == AuC.Max())
                     .Select(item => item.Index).ToArray();

                    int[] FppMaxEmotionIndex = FppC.Select((item, index) => new { Index = index, Value = item })
                    .Where(item => item.Value == FppC.Max())
                    .Select(item => item.Index).ToArray();

                    Random rnd = new Random();
                    AuMaxEmotionIndex[0] = AuMaxEmotionIndex[rnd.Next(AuMaxEmotionIndex.Count())];
                    FppMaxEmotionIndex[0] = FppMaxEmotionIndex[rnd.Next(FppMaxEmotionIndex.Count())];
         
                    FinalEstimation = (AuC.Max() > FppC.Max()) ? AuMaxEmotionIndex[0] : FppMaxEmotionIndex[0];

                    //output on Window
                    currentEmotion = FinalEstimation;
                    txtOutput.Content = "" + EmotionIndex[FinalEstimation];

                    showFace3D(newData);
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void showFace3D(FaceData newData){
            txtPitch.Content = Math.Round(newData.head3D[0],0) + "°";
            txtYaw.Content = Math.Round(newData.head3D[1],0) + "°";
            txtRoll.Content = Math.Round(newData.head3D[2], 0) + "°";
        }

        public int currentEmotion = -1;

        DateTime fps_lastCheck = DateTime.Now;
        int fps_counter = 0;

        private void InitializeSVM()
        {
            AuClassLabel = new int[7];
            FppClassLabel = new int[7];
            AuC = new int[7];
            FppC = new int[7];
        }

        private void TestSVM(FaceData newData)
        {
            for (int i = 0; i < 7; i++)
            {
                //Create test data
                CreateTestData(newData, i);

                //Get the result of estimation
                AuClassLabel[i] = (int)svm.svm_predict(AuModel[i], AuTestData);
                FppClassLabel[i] = (int)svm.svm_predict(FppModel[i], FppTestData);
            }
        }

        private void CreateTestData(FaceData faceData, int idx)
        {
            AuTestData = new svm_node[MaxAuFeatures];
            FppTestData = new svm_node[MaxFppFeatures];

            for (int i = 0; i < faceData.au_data.Count; i++)
            {
                AuTestData[i] = new svm_node();

                AuTestData[i].index = i;
                AuTestData[i].value = faceData.au_data[i];
            }

            int index = 0;
            int j = 0;
            while (index < faceData.point_3D.Count)
            {
                if (FeaturePoint.ContainsValue(index / 3))
                {
                    FppTestData[j] = new svm_node();

                    FppTestData[j].index = j;
                    FppTestData[j].value = faceData.point_3D[index];

                    j++;
                }
                index++;
            }

            ScalingData(AuTestData, AuScaleMin, AuScaleMax, MaxAuFeatures, idx);
            ScalingData(FppTestData, FppScaleMin, FppScaleMax, MaxFppFeatures, idx);
        }

        private void ScalingData(svm_node[] testData, double[,] scaleMin, double[,] scaleMax, int dataSize, int index)
        {
            for (int i = 0; i < dataSize; i++)
            {
                testData[i].value = 2 * (testData[i].value - scaleMin[index, i]) / (scaleMax[index, i] - scaleMin[index, i]) - 1;

                if (testData[i].value > 1)
                {
                    testData[i].value = 1;
                }
                else if (testData[i].value < -1)
                {
                    testData[i].value = -1;
                }
            }

        }

        private void LoadModelData(int i)
        {
            AuModel[i] = svm.svm_load_model(@TheURL.url_0_root + @TheURL.url_libsvm_model + "AU_" + EmotionIndex[i] + ".scale.model");
            FppModel[i] = svm.svm_load_model(@TheURL.url_0_root + @TheURL.url_libsvm_model + "FPP_" + EmotionIndex[i] + ".scale.model");
            Range[0] = new StreamReader(@TheURL.url_0_root + @TheURL.url_libsvm_model + "AU_" + EmotionIndex[i] + ".range");
            Range[1] = new StreamReader(@TheURL.url_0_root + @TheURL.url_libsvm_model + "FPP_" + EmotionIndex[i] + ".range");

            //save scaling data about training 
            for (int j = 0; j < 2; j++)
            {
                String line;
                int index = 0;
                String[] scaleData = new String[4];

                while ((line = Range[j].ReadLine()) != null)
                {
                    scaleData = line.Trim().Split(' ');
                    if (scaleData[0] != "x" && scaleData[0] != "-1" && scaleData[1] != "1")
                    {
                        if (j == 0)
                        {
                            AuScaleMin[i, index] = double.Parse(scaleData[1]);
                            AuScaleMax[i, index] = double.Parse(scaleData[2]);
                        }
                        else
                        {
                            FppScaleMin[i, index] = double.Parse(scaleData[1]);
                            FppScaleMax[i, index] = double.Parse(scaleData[2]);
                        }
                        index++;
                    }
                }
            }
            Range[0].Close();
            Range[1].Close();
        }

        private void SetParameters(int i)
        {
            AuModel[i].param.C = AuModelParameters[i].ElementAt(0);
            AuModel[i].param.gamma = AuModelParameters[i].ElementAt(1);
            FppModel[i].param.C = FppModelParameters[i].ElementAt(0);
            FppModel[i].param.gamma = FppModelParameters[i].ElementAt(1);

        }

        //--------------------------

        public void updateInfo()
        {
            int i = mainForm.listFaceData.Count();
            if (i == 1) { startTime = mainForm.thisTime; }
            txtCount.Content = "# " + i;
        }

        public Boolean collectData = true;
        private void checkCollectData_Checked(object sender, RoutedEventArgs e)
        {
            collectData = checkCollectData.IsChecked.Value;
        }

        private void butClear_Click(object sender, RoutedEventArgs e)
        {
            mainForm.listFaceData.Clear(); updateInfo();
        }

        private void butExport_Click(object sender, RoutedEventArgs e)
        {
            mainForm.export_faceData();
        }

        private void checkShowEmo_Checked(object sender, RoutedEventArgs e)
        {
            if (checkShowEmo.IsChecked.Value) { txtOutput.Visibility = Visibility.Visible; }
            else { txtOutput.Visibility = Visibility.Hidden; }
        }
    }
}
