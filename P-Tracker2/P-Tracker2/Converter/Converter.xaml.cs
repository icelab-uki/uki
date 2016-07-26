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
using System.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace P_Tracker2
{

    public partial class Converter : Window
    {
        public Converter()
        {
            InitializeComponent();
            resetTable();
        }

        //--- Column ----------
        string col_id = "id";
        string col_path = "file_path";
        //-----------------------------

        void resetTable()
        {
            if (dataTable != null) { dataTable.Clear(); }
            dataTable = new DataTable();
            dataTable.Columns.Add(col_id);
            dataTable.Columns.Add(col_path);
            setDataGrid(dataTable);
        }

        void setDataGrid(DataTable dt)
        {
            dataGrid.AutoGenerateColumns = false;
            dataGrid.ItemsSource = dt.DefaultView;
            dataGrid.AutoGenerateColumns = true;
        }

        //Browser File
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            TheTool.openFileDialog_01(true, ".*", "");
            int id = 1;
            string[] result = TheTool.dialog.FileNames;
            if (result.Count() > 0) { resetTable(); }
            foreach (string y in result)
            {
                dataTable.Rows.Add(id,y);
                id++;
            }
            rowCount();
        }

        DataTable dataTable = null;//data table that become datagrid
        //-------------------------------------

        int row_count = 0;
        void rowCount()
        {
            try { 
                row_count = dataTable.Rows.Count;
                txtRow.Content = "Row: " + row_count; 
            }
            catch { }
        }


        private void butOpenFolderClick(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder); }
            catch { }
        }

        // Execute shutdown tasks
        private void Window_Closed(object sender, EventArgs e)
        {
            TheStore.mainWindow.Show();
        }

        //=== From below are specific ==========================================================

        String path_folder_convert = TheURL.url_saveFolder + TheURL.url_9_Convert + @"\";

        private void butMsrConv_Click(object sender, RoutedEventArgs e)
        {
            TheConverter.MSR_convertFile(dataTable, col_path, path_folder_convert);
        }

        private void msr_sample_Click(object sender, RoutedEventArgs e)
        {
            TheConverter.MSR_showSample();
            TheSys.showError("Link: http://research.microsoft.tcom/en-us/um/people/zliu/ActionRecoRsrc/default.htm");
            TheSys.showError("Below is an example of data"); 
            TheSys.showError("");
            foreach (String s in TheTool.read_File_getListString(TheURL.url_tv_sample_msr))
            {
                TheSys.showError(s);
            }
        }

        private void butConvertVerLab_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRow r in dataTable.Rows)
            {
                try
                {
                    string path_origin = r[col_path].ToString();
                    TheTool.Folder_CreateIfMissing(path_folder_convert);
                    string path_save = path_folder_convert + TheTool.getFileName_byPath(path_origin) + ".csv";
                    TheConverter.Verlab_convertFile(path_origin, path_save);
                }
                catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
            }
            System.Windows.MessageBox.Show(@"Save to file\[Convert]\");
        }

        //----------------------------------

        private void butBVH_Convert_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRow r in dataTable.Rows)
            {
                try
                {
                    string path_origin = r[col_path].ToString();
                    TheTool.Folder_CreateIfMissing(path_folder_convert);
                    //-----------
                    string sub_folder = path_folder_convert + "CMU " + TheTool.splitText(TheTool.getFileName_byPath(path_origin), "_")[0];
                    TheTool.Folder_CreateIfMissing(sub_folder);
                    //-----------
                    string path_save = sub_folder + @"\" + TheTool.getFileName_byPath(path_origin) + ".csv";
                    TheConverter.BVH_convertFile(path_origin, path_save);
                }
                catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
            }
            System.Windows.MessageBox.Show(@"Save to file\[Convert]\");
        }

        private void butConvertMSRAction_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRow r in dataTable.Rows)
            {
                try
                {
                    string path_origin = r[col_path].ToString();
                    TheTool.Folder_CreateIfMissing(path_folder_convert);
                    //-----------
                    string sub_folder = path_folder_convert + "MSRAction " + TheTool.splitText(TheTool.getFileName_byPath(path_origin), "_")[0];
                    TheTool.Folder_CreateIfMissing(sub_folder);
                    //-----------
                    string path_save = sub_folder + @"\" + TheTool.getFileName_byPath(path_origin) + ".csv";
                    TheUKI.saveData_Raw(path_save, TheConverter.MSRAction_convert(path_origin));
                }
                catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
            }
            System.Windows.MessageBox.Show(@"Save to file\[Convert]\");
        }


    }
}
