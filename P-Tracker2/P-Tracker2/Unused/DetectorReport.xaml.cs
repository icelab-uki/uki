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

namespace P_Tracker2
{

    public partial class DetectorReport : Window
    {

        Detector main_detector;

        public DetectorReport(Detector detector)
        {
            main_detector = detector;
            InitializeComponent();
            resetDataTable();
            setDataGrid();
        }

        DataTable dt;
        
        List<string> col_list = new List<string> { "Time","Move","Pitch","Twist","Prolong_lv",
                                    "Pitch_lv","Twist_lv","Total_lv","Prolong_score",
                                    "Pitch_score","Twist_score","Total_score","note"};

        public void resetDataTable()
        {
            dt = new DataTable();
            foreach (string s in col_list) { dt.Columns.Add(s); }
        }

        public void setDataGrid()
        {
            dataGrid.AutoGenerateColumns = false;
            dataGrid.ItemsSource = dt.DefaultView;
            dataGrid.AutoGenerateColumns = true;
            dataGrid.Background = Brushes.SlateGray;
            dataGrid.RowBackground = Brushes.SlateGray;
            //dataGrid.BorderBrush = Brushes.Gray;
        }


        DataGridRow datagrid_row; 
        DataGridCell targetCell;
        int dt_row = 0;

        public void addRow(string time,
            int still,int pitch,int turn,
            int still_lv, int pitch_lv, int turn_lv,
            int still_score, int pitch_score, int turn_score,
            int isStand,int isBreak
            )
        {
            dt.Rows.Add(); 
            dt_row = dt.Rows.Count - 1;
            datagrid_row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dt_row + 1);
            //------------
            dt.Rows[dt_row][0] = time.ToString();
            if (isBreak == 1) { dt.Rows[dt_row][4] = "stand"; }
            else if (isStand == 1) { dt.Rows[dt_row][4] = "break"; }
            else
            {
                dt.Rows[dt_row][1] = still.ToString();
                dt.Rows[dt_row][2] = pitch.ToString();
                dt.Rows[dt_row][3] = turn.ToString();
                dt.Rows[dt_row][5] = still_lv.ToString();
                dt.Rows[dt_row][6] = pitch_lv.ToString();
                dt.Rows[dt_row][7] = turn_lv.ToString();
                dt.Rows[dt_row][8] = still_score.ToString();
                dt.Rows[dt_row][9] = pitch_score.ToString();
                dt.Rows[dt_row][10] = turn_score.ToString();

                //-------------------------------
                targetCell = (DataGridCell)dataGrid.Columns[1].GetCellContent(datagrid_row).Parent;
                targetCell.Background = getBrush(still_lv);
                //
                targetCell = (DataGridCell)dataGrid.Columns[2].GetCellContent(datagrid_row).Parent;
                targetCell.Background = getBrush(pitch_lv);
                //
                targetCell = (DataGridCell)dataGrid.Columns[3].GetCellContent(datagrid_row).Parent;
                targetCell.Background = getBrush(turn_lv);
            }
            //targetRow.Foreground = Brushes.White;//Fill BG first
            //----------------------------------------
            dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
            //dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            //dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            //dataGrid.UpdateLayout();
        }


        public SolidColorBrush getBrush(int value)
        {
            SolidColorBrush b = Brushes.Green;
            if (value > 1) { b = Brushes.Red; }
            else if (value > 0) { b = Brushes.Yellow; }
            return b;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                TheTool_micro.sendCmd(MicroCmd.cmd_sleep);
            }
            catch { }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            //dataGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            dataGrid.UpdateLayout();
        }

        public Boolean confirmOnClose = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (confirmOnClose == true)
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Data will be lost, you may save files first."
                                                , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
                if (result == System.Windows.MessageBoxResult.OK)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }


        private void butExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string file_name0;
                file_name0 = "Report_" + DateTime.Now.ToString("MMdd_HHmmss");
                string path = TheURL.url_saveFolder + file_name0 + ".csv";
                //-------------
                //TheTool.Folder_CreateIfMissing(path_saveRoot);
                TheTool.export_dataTable_to_CSV(path, dt.Copy());
            }
            catch { }
        }



        private void butExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string file_name0;
                file_name0 = "Report_" + DateTime.Now.ToString("MMdd_HHmmss");
                string path = file_name0;
                //-------------
                TheTool.exportExcel_DReport(path, dt.Copy());
            }
            catch { }
        }


    }
}
