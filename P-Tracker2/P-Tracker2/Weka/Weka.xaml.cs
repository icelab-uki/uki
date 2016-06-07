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
using System.Diagnostics;
using System.Data;
using System.Collections.ObjectModel;

namespace P_Tracker2
{

    public partial class Weka : Window
    {
        public Weka()
        {
            InitializeComponent();
            setComboAlgo();
            dataGrid.ItemsSource = listWekaLog;
        }

        void setComboAlgo()
        {
            comboAlgo.Items.Add("Neural Network");
            comboAlgo.Items.Add("Tree J48");
            comboAlgo.Items.Add("Naive Bayes");
            comboAlgo.SelectedIndex = 1;
        }

        string file_name = "";
        string model = "";

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                model = comboAlgo.SelectedValue.ToString();
                txt1.Text += model + Environment.NewLine;
                TheWeka.colClass = TheTool.getInt(txtColClass);
                TheWeka.classify_model = comboAlgo.SelectedIndex;
                TheWeka.random_sort = checkRSort.IsChecked.Value;
                //
                setTestMode();
                //-------------------
                TheWeka.do_Classification_full(false);
                txt1.Text += TheWeka.txt_report_short;
                txt1.CaretIndex = txt1.Text.Length;//move Cursor
                //-------------------------------
                //------- Show Each Report -----------
                lastResult = TheWeka.listResult;
                listResult.Add(lastResult);
                showReport();
                //-------------------------------
                //------- Summary Table -----
                writeSummaryTable();
                testID++;
            }
            catch { }
        }

        void setTestMode()
        {
            if(radio2.IsChecked == true){
                TheWeka.test_mode = 1;
                TheWeka.crossV_fold = TheTool.getInt(txtFold);
            }
            else{
                TheWeka.test_mode = 0;
                setRatio_TrainTest();
            }
        }

        void setRatio_TrainTest()
        {
            int train = TheTool.getInt(txtTrain);
            int test = TheTool.getInt(txtTest);
            int total = train + test;
            int percent = 0;
            if (total > 0)
            {
                percent = train * 100 / total;
            }
            else { percent = 80; txtTrain.Text = "8"; txtTest.Text = "2"; }
            TheWeka.splt_percentTrain = percent;
        }

        //----------------------------------

        ObservableCollection<WekaLog> listWekaLog = new ObservableCollection<WekaLog>(); //Value as String
        List<List<Weka_EachResult>> listResult = new List<List<Weka_EachResult>>();
        List<Weka_EachResult> lastResult = new List<Weka_EachResult>();

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            listWekaLog = new ObservableCollection<WekaLog>(); //Value as String
            listResult = new List<List<Weka_EachResult>>();
            lastResult = new List<Weka_EachResult>();
        }

        //-------------------------------------

        int testID = 0;
        void writeSummaryTable()
        {
            string txt_sampling;
            if (radio2.IsChecked == true) { txt_sampling = txtFold.Text + "-fold"; }
            else { txt_sampling = TheWeka.txt_report_part_sampling; }
            WekaLog log = new WekaLog() {
                ID = testID.ToString(),
                File = file_name,
                Model = model,
                Sampling = txt_sampling,
                Result = TheWeka.txt_report_part_result
            };
            listWekaLog.Add(log);
        }

        void showReport()
        {
            if (checkRTable.IsChecked == true && radio1.IsChecked == true) {
                SubViewer sub2 = new SubViewer();
                sub2.mode = 1;
                sub2.Show();
                sub2.Width = 200;
                sub2.setDataGrid(lastResult);
            }
            if (checkReport.IsChecked == true && radio2.IsChecked == true)
            {
                Report r = new Report();
                r.Show();
                r.setText(TheWeka.txt_report_full);
            }
        }

        private void butLoad_Click(object sender, RoutedEventArgs e)
        {
            TheURL.dm_path_file = txtPath.Text;
            TheWeka.step_loadInstance();
            if(instanceReady == false){
                button1.IsEnabled = true;
                butLoad.Foreground = Brushes.Black;
                instanceReady = true;
            }
            file_name = TheTool.getFileName_byPath(TheURL.dm_path_file);
            txt1.Text += "File: " + file_name + Environment.NewLine + Environment.NewLine;
        }

        Boolean instanceReady = false;

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder + TheURL.url_9_Train); }
            catch { }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".arff", TheURL.url_saveFolder);
            // Get the selected file name and display in a TextBox
            if (openDialog == true)
            {
                // Open document
                string filePath = TheTool.dialog.FileName;
                txtPath.Text = filePath;
            }
        }

        
        //============================================================
        //============= DataGrid ====================
        public class WekaLog
        {
            public string ID { get; set; }
            public string File { get; set; }
            public string Model { get; set; }
            public string Sampling { get; set; }
            public string Result { get; set; }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.Show();
        }

        private void butHelp_Click(object sender, RoutedEventArgs e)
        {
            TheSys.showError("Process: Classification by Weka");
            TheSys.showError("Input: Instances with 1 class column");
            TheSys.showError("Output: Output from Weka & Table show Predicted Class of each instance");
        }

        
        


    }

}
