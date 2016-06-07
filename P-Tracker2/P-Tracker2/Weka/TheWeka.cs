using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using weka.classifiers.evaluation;
using weka.classifiers.misc;


//Newer Version is in "Prophet" //


namespace P_Tracker2
{
    class TheWeka
    {
        public static string txt_report_short = "";//Store Result
        public static string txt_report_full = "";//Store Result
        public static string txt_report_part_sampling = "";
        public static string txt_report_part_result = "";
        public static string txt_report_time = "";//Keep time - Train + Test

        public static void txtResult_Reset()
        {
            txt_report_short = "";
            txt_report_full = "";
            txt_report_part_sampling = "";
            txt_report_part_result = "";
        }
        //----------------------------------
        public static int colClass = 0;//Column for classify
        public static int count_feature = 0;
        public static weka.core.Instances insts;
        public static int predict_numCorrect = 0;
        public static int classify_model = 0;// 0 >> NN , 1 >> Tree , 2 >> Naive Bayes
        public static string getModelName()
        {
            if (classify_model == 1) { return "Trees J48"; }
            else if (classify_model == 2) { return "Naive Bayes"; }
            else { return "Multilayer Perceptron Neural Networks"; }
        }
        //--------------
        public static int test_mode = 0;
        //Split
        public static int splt_percentTrain = 66;//Percent of unit train
        public static int split_trainSize = 0;//Exact# of unit train
        public static int splt_testSize = 0;//Exact# of unit test
        public static int crossV_fold = 10;
        //----------------------------------
        public static weka.classifiers.Classifier classifier;

        //-------------------
        public static Evaluation eval = null;
        public static Boolean random_sort = true;


        public static void do_Classification_full(Boolean loadData)
        {
            try
            {
                if (loadData == true) { step_loadInstance(); }
                insts.setClassIndex(colClass);//Which Column is classified
                if (random_sort == true) { step_randomInstanceOrder(); }
                //------ Train & Test ----------
                classifier = step_buildClassifier(classify_model);
                txtResult_Reset();
                step_train_test();
            }
            catch (Exception ex)
            {
                TheSys.showError("Err: " + ex.ToString(), true);
            }
        }

        //--------------------------------------------------------------------

        static public void step_train_test()
        {
            if (test_mode == 1)
            {
                //======== Cross Validation =============
                eval = new Evaluation(insts);
                eval.crossValidateModel(classifier, insts, 3, new java.util.Random(1));

                //--------- Write Report --------
                txt_report_short += crossV_fold + "-fold cross-validation" + Environment.NewLine;
                txt_report_part_result = eval.pctCorrect().ToString();
                txt_report_short += "Correctly Predicted: " + txt_report_part_result + "%";
                txt_report_short += Environment.NewLine + Environment.NewLine;
                //------------------
                txt_report_full += "File: " + TheTool.getFileName_byPath(TheURL.dm_path_file) + Environment.NewLine;
                txt_report_full += "Scheme: " + getModelName() + Environment.NewLine;
                txt_report_full += "Features: " + count_feature + Environment.NewLine + Environment.NewLine;
                txt_report_full += "=== Summary ===" + Environment.NewLine;
                txt_report_full += eval.toSummaryString() + Environment.NewLine;
                txt_report_full += eval.toClassDetailsString() + Environment.NewLine;
                txt_report_full += eval.toMatrixString() + Environment.NewLine;
                txt_report_full += "Correct:" + eval.pctCorrect() + Environment.NewLine;
                txt_report_full += "PositiveRate:" + eval.truePositiveRate(0) + Environment.NewLine;
                txt_report_full += "NegativeRate:" + eval.trueNegativeRate(0) + Environment.NewLine;
            }
            else
            {
                //======== Split =============
                calSize_TrainTest();
                step_train();
                step_test();
                //--------- Write Report --------
                write_preprocess();
                write_predictResult();
                txt_report_short += Environment.NewLine;
            }
        }


        static public void write_preprocess()
        {
            txt_report_part_sampling += "Split " + splt_percentTrain + "% : ";
            txt_report_part_sampling += "train " + split_trainSize + " , test " + splt_testSize;
            txt_report_part_sampling += Environment.NewLine;
            txt_report_short = txt_report_part_sampling;
        }

        static public void write_predictResult()
        {
            double correct = ((double)predict_numCorrect / (double)splt_testSize * 100.0);
            txt_report_part_result = correct.ToString();
            txt_report_short += "Correctly Predicted: " + predict_numCorrect + " / " + splt_testSize + " (" + correct + "%)";
            txt_report_short += Environment.NewLine;
        }


        static public weka.classifiers.functions.MultilayerPerceptron classifier_NN = new weka.classifiers.functions.MultilayerPerceptron();
        static public weka.classifiers.trees.J48 classifier_tree = new weka.classifiers.trees.J48();
        static public weka.classifiers.bayes.NaiveBayes classifier_NBay = new weka.classifiers.bayes.NaiveBayes();

        //0 = NN, 2 = Bay, 1 = Tree
        static public weka.classifiers.Classifier step_buildClassifier(int algo)
        {
            weka.classifiers.Classifier classifier = null;
            if (algo == 0)
            {
                classifier_NN.setLearningRate(0.3);
                classifier_NN.setMomentum(0.2);
                classifier_NN.setHiddenLayers("a");
                classifier = classifier_NN;
            }
            else if (algo == 2)
            {
                classifier = classifier_NBay;
            }
            else
            {
                classifier = classifier_tree;
            }
            return classifier;
        }

        //static public void classifyLastColumn(){
        //    try { colClass = insts.numAttributes() - 1; }
        //    catch { colClass = 0;  }
        //}

        static public void step_loadInstance()
        {
            insts = new weka.core.Instances(new java.io.FileReader(TheURL.dm_path_file));
            count_feature = insts.numAttributes() - 1;
        }

        //static public void step_loadInstance(string path)
        //{
        //    java.io.FileReader reader = new java.io.FileReader(path);
        //    insts = new weka.core.Instances(reader);
        //    reader.close();
        //    count_feature = insts.numAttributes() - 1;
        //}

        static public void step_randomInstanceOrder()
        {
            weka.filters.Filter myRandom = new weka.filters.unsupervised.instance.Randomize();
            myRandom.setInputFormat(insts);
            insts = weka.filters.Filter.useFilter(insts, myRandom);
        }

        static public void step_train()
        {
            weka.core.Instances train = new weka.core.Instances(insts, 0, split_trainSize);
            classifier.buildClassifier(train);
        }

        static public List<Weka_EachResult> listResult = new List<Weka_EachResult>();
        static public int step_test()
        {
            predict_numCorrect = 0;
            double predictedClass;
            double actualClass;
            listResult = new List<Weka_EachResult>();
            weka.core.Instance currentInst;
            for (int i = split_trainSize; i < insts.numInstances(); i++)
            {
                currentInst = insts.instance(i);
                predictedClass = classifier.classifyInstance(currentInst);
                actualClass = insts.instance(i).classValue();
                if (predictedClass == actualClass)
                    predict_numCorrect++;
                //==================================
                listResult.Add(new Weka_EachResult()
                {
                    ID = i.ToString(),
                    Actual = actualClass.ToString(),
                    Predict = predictedClass.ToString(),
                    Diff = (predictedClass - actualClass).ToString()
                });
            }
            //currentInst = insts.lastInstance();
            //TheSys.showError(currentInst.ToString(), true);
            return predict_numCorrect;
        }

        static public void calSize_TrainTest()
        {
            split_trainSize = insts.numInstances() * splt_percentTrain / 100;
            splt_testSize = insts.numInstances() - split_trainSize;
        }


        //void report_write(){
        //    Evaluation eval = new Evaluation(trainCases);
        //    eval.crossValidateModel(tree, trainCases, 3, new Random(1));
        //    System.out.println(eval.toSummaryString("\nResults\n======\n", false));
        //    System.out.println(eval.toClassDetailsString());
        //    System.out.println(eval.toMatrixString());
        //    System.out.println("accuracy:"+eval.pctCorrect());
        //    System.out.println("Specificity:"+eval.truePositiveRate(0));
        //    System.out.println("sensitivity:"+eval.trueNegativeRate(0));
        //}





        //======================================================================
        //--------- Seperate Part -------

        public static weka.core.Instances getInst(string path_file, int colIndex)
        {
            try
            {
                weka.core.Instances ins = new weka.core.Instances(new java.io.FileReader(path_file));
                ins.setClassIndex(colIndex);
                return ins;
            }
            catch (Exception ex)
            {
                TheSys.showError("Err: " + ex.ToString(), true);
                return null;
            }
        }

        static public double do_test_single(weka.classifiers.Classifier classifier, weka.core.Instances insts_test)
        {
            weka.core.Instance currentInst = insts_test.lastInstance();
            return classifier.classifyInstance(currentInst);
        }


        static public void setClassifyCol(int i)
        {
            TheWeka.colClass = i;
            TheWeka.insts.setClassIndex(i);//Which Column is classified
        }

        //static public void setClassifyCol_lastAtt()
        //{
        //    int i = insts.numAttributes() - 1;
        //    TheWeka.colClass = i;
        //    TheWeka.insts.setClassIndex(i);//Which Column is classified
        //}

        //=====================================================================================================
        //============= Classify by SerializedClassifier (saved model) ==============================
        static public SerializedClassifier serialClassifier = new SerializedClassifier();

        static public List<string> do_Classification_bySerialClassfier()
        {
            List<string> listPredictClass = new List<string>();
            //----------
            predict_numCorrect = 0;
            double predictedClass;
            double actualClass;
            weka.core.Instance currentInst;
            listResult = new List<Weka_EachResult>();
            for (int i = split_trainSize; i < insts.numInstances(); i++)
            {
                currentInst = insts.instance(i);
                //TheSys.showError(currentInst.ToString(), true);
                predictedClass = serialClassifier.classifyInstance(currentInst);
                actualClass = insts.instance(i).classValue();
                if (predictedClass == actualClass)
                    predict_numCorrect++;
                //==================================
                listResult.Add(new Weka_EachResult()
                {
                    ID = i.ToString(),
                    Actual = actualClass.ToString(),
                    Predict = predictedClass.ToString(),
                    Diff = (predictedClass - actualClass).ToString()
                });
                //Get Class Name
                listPredictClass.Add(insts.classAttribute().value((int)predictedClass));
            }
            return listPredictClass;
        }


        //======================================================================================
        //===== Stand Alone ==============================================

        static public weka.core.Instances createInstance(string path)
        {
            return new weka.core.Instances(new java.io.FileReader(path));
        }

        static public List<string> do_Classification_bySerialClassfier_standAlone
            (SerializedClassifier serialClassifier, weka.core.Instances instances, int colClass)
        {
            instances.setClassIndex(colClass);
            List<string> listPredictClass = new List<string>();
            double predictedClass; double actualClass; predict_numCorrect = 0;
            weka.core.Instance each;
            for (int i = 0; i < instances.numInstances(); i++)
            {
                each = instances.instance(i);
                predictedClass = serialClassifier.classifyInstance(each);
                actualClass = instances.instance(i).classValue();
                if (predictedClass == actualClass) { predict_numCorrect++; }
                //Get Class Name
                listPredictClass.Add(instances.classAttribute().value((int)predictedClass));
            }
            return listPredictClass;
        }

                //Only 1 output: last instance
        static public string do_Classification_bySerialClassfier_1out_standAlone
            (SerializedClassifier serialClassifier, weka.core.Instances instances, int colClass)
        {
            instances.setClassIndex(colClass);
            weka.core.Instance each = instances.instance(instances.numInstances() - 1);
            double predictedClass = serialClassifier.classifyInstance(each);
            return instances.classAttribute().value((int)predictedClass);
        }

    }

    //***********************************************************

    public class Weka_EachResult
    {
        public string ID { get; set; }
        public string Actual { get; set; }
        public string Predict { get; set; }
        public string Diff { get; set; }
    }

}
