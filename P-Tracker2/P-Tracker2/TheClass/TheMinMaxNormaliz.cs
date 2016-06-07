using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheMinMaxNormaliz
    {
        static string col_suffix_d = "_D";
        //=======================================
        static public Boolean minmax_Euclidian_ready = false;

        //------------------------------------
        static public List<string> listJoint;//e.g. Head
        static public List<string> listJoint_D;//e.g. Head_D
        static public double[] min_euclidian = new double[10];
        static public double[] max_euclidian = new double[10];
        //-----------------

        static public DataTable dt_fullTable;
        static public DataTable dt_MinMax_Euclidian;

        static public void getDataTable(string filePath)
        {
            dt_fullTable = CSVReader.ReadCSVFile(filePath, true);
            getMinMax_Euclidian();
        }

        
        
        static public void getMinMax_Euclidian()
        {
            try
            {
                min_euclidian = new double[10];
                max_euclidian = new double[10];
                //----------
                listJoint = TheTool.getListJoint("");
                listJoint_D = TheTool.getListJoint(col_suffix_d);
                List<double> d;
                int i = 0;
                foreach (string col in listJoint_D)
                {
                    d = new List<double>();
                    for (int row = 0; row < dt_fullTable.Rows.Count; row++)
                    {
                        d.Add(double.Parse(dt_fullTable.Rows[row][col].ToString()));
                    }
                    max_euclidian[i] = d.Max(); 
                    min_euclidian[i] = d.Min();
                    i++;
                }
                minmax_Euclidian_ready = true;
            }
            catch { 
                TheSys.showError("File must contain column e.g. 'Head_D'", true);
                TheSys.showError(">> P-Analysis & Concat files first.", true); 
            }
        }

        static public void getMinMax(string jointName, ref double min, ref double max)
        {
            string[] list = listJoint.ToArray();
            for (int i = 0; i < list.Count(); i++)
            {
                if (jointName == list[i])
                {
                    min = min_euclidian[i];
                    max = max_euclidian[i];
                }
            }

        }

        //static public double getMinMax(int i,Boolean getMax)
        //{
        //    if (getMax == true) { return max_euclidian[i]; }
        //    else { return max_euclidian[i];}
        //}
        
        //static public void showMinMax_Euclidian()
        //{
        //    try
        //    {
        //        string txt = "";
        //        string[] list = listJoint_D.ToArray();
        //        for(int i = 0;i<min_euclidian.Count();i++){
        //            txt += list[i] + ":min:" + min_euclidian[i] + Environment.NewLine;
        //            txt += list[i] + ":max:" + max_euclidian[i] + Environment.NewLine;
        //        }
        //        TheSys.showError(txt);
        //    }
        //    catch { }
        //}


        static public void showMinMax_Euclidian_Table()
        {
            buildMinMax_Euclidian_Table();
            TheTool.showTable(dt_MinMax_Euclidian, "Min-Max", "MinMax");
        }

        static public void buildMinMax_Euclidian_Table()
        {
            try
            {
                //---- Create
                dt_MinMax_Euclidian = new DataTable();
                foreach (string j in listJoint_D)
                {
                    dt_MinMax_Euclidian.Columns.Add(j);
                }
                //--------------
                //---- Add Min Max
                dt_MinMax_Euclidian.Rows.Add();
                dt_MinMax_Euclidian.Rows.Add();
                string[] joint = listJoint_D.ToArray();
                for (int i = 0; i < min_euclidian.Count(); i++)
                {
                    dt_MinMax_Euclidian.Rows[0][joint[i]] = min_euclidian[i];
                    dt_MinMax_Euclidian.Rows[1][joint[i]] = max_euclidian[i];
                }
            }
            catch { TheSys.showError("Error [buildMinMax_Euclidian_Table]", true); }
        }
    }
}
