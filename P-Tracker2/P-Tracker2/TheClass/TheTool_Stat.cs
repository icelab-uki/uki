using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheTool_Stat
    {

        static public string processing_file = "";//to report which file error

        static public double calNorm3D(double a, double b, double c, int digit)
        {
            double r = Math.Pow(a, 2) + Math.Pow(b, 2) + Math.Pow(c, 2);
            r = Math.Sqrt(r);
            return Math.Round(r, digit);
        }

        public static double calMean(List<double> list)
        {
            try{return list.Average();}
            catch { return 0; }
        }

        public static double calVariance(List<double> list, double mean)
        {
            double v = 0;
            try
            {
                if (list.Count > 1)
                {
                    foreach (double d in list)
                    {
                        v += Math.Pow((d - mean), 2);
                    }
                    int n = list.Count;
                    return v / (n-1);
                }
            }
            catch { }
            return v;
        }

        public static double calZScore(double value, double mean, double sd, int digit)
        {
            double z = 0;
            try
            {
                z = value - mean;
                z = z / sd;
                z = Math.Round(z, digit);
            }
            catch { }
            return z;
        }

        public static double calAvg_byCol(DataTable dt, string col, int digit)
        {
            double avg = 0;
            try{
                List<double> list = new List<double>();
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    list.Add(double.Parse(dt.Rows[row][col].ToString()));
                }
                avg = list.Average();
                avg = Math.Round(avg, digit);
            }catch{}
            return avg;
        }


        //Euclidean Distance =SQRT((x2-x1)^2+(y2-y1)^2+(z2-z1)^2)
        //double[3] >> x,y,z 
        static public double calEuclidean(double[] p1, double[] p2, int digit)
        {
            double eucli = 0;
            try
            {
                for (int i = 0; i < p1.Count(); i++ )
                {
                    eucli += Math.Pow(p1[i] - p2[i], 2);
                }
                eucli = Math.Sqrt(eucli);
            }
            catch (Exception e) { TheSys.showError("Err: [" + processing_file + "] " + e.ToString(), true); }
            return Math.Round(eucli, digit);
        }


        //Automatic change to original table
        static public void normalize_table_MinMax(
            DataTable dt, List<string> col_list , int digit)
        {
            try{
                double v;
                double min = 0;
                double max = 0;
                double range = 0;
                List<double> d;
                //
                foreach (string col in col_list)
                {
                    d = new List<double>();
                    for (int row = 0; row < dt.Rows.Count;row++){
                        d.Add(double.Parse(dt.Rows[row][col].ToString()));
                    }
                    max = d.Max();min = d.Min(); range = max - min;
                    //
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        v =double.Parse(dt.Rows[row][col].ToString());
                        v = (v - min)/ range;
                        dt.Rows[row][col] = Math.Round(v,digit);
                    }
                }
            }
            catch (Exception e) { TheSys.showError("Err: [" + processing_file + "] " + e.ToString(), true); }
        }


        //static public void normalize_table_MinMax_singleCol(
        //    DataTable dt, string col_in, string col_out
        //    , double min, double max, int digit)
        //{
        //    try
        //    {
        //        double v;
        //        double range = max - min;
        //        for (int row = 0; row < dt.Rows.Count; row++)
        //        {
        //            v = double.Parse(dt.Rows[row][col_in].ToString());
        //            v = (v - min) / range;
        //            dt.Rows[row][col_out] = Math.Round(v, digit);
        //        }
        //    }
        //    catch (Exception e) { TheSys.showError("Err: [MinMax]["
        //        + processing_file + "] " + e.ToString()); }
        //}

        static public double calMinMaxNormalize(double d,double min,double max)
        {
            double v = 0;
            try{v = (d - min) / (max - min);}
            catch{}
            return v;
        }

        //Automatic change to original table
        static public void normalize_table_byValue(
            DataTable dt, List<string> col_list, double value,int digit)
        {
            try
            {
                double v;
                foreach (string col in col_list)
                {
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        v = double.Parse(dt.Rows[row][col].ToString());
                        v = v * value;
                        dt.Rows[row][col] = Math.Round(v, digit);
                    }
                }
            }
            catch (Exception e) { TheSys.showError("Err: [" + processing_file + "] " + e.ToString(), true); }
        }

        //Automatic change to original table
        static public void transform_table_canonDigit(
            DataTable dt, List<string> col_list, int canon_lv, int digit)
        {
            try
            {
                double v;
                double canon = 0;
                foreach (string col in col_list)
                {
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        v = double.Parse(dt.Rows[row][col].ToString());
                        canon = getCanon(canon, canon_lv);
                        v = v + canon;
                        dt.Rows[row][col] = Math.Round(v, digit);
                    }
                }
            }
            catch (Exception e) { TheSys.showError("Err: [" + processing_file + "] " + e.ToString(), true); }
        }

        static public double getCanon(double initial,int canon_lv)
        {
            try
            {
                initial = initial * Math.Pow(10, 10);
                Random rnd = new Random();
                int max = 0;
                //
                if (canon_lv == 1) { max = 199999; }
                else if (canon_lv == 2) { max = 299999; }
                else if (canon_lv == 3) { max = 499999; }
                int canon = rnd.Next(-max, max);
                double canon0 = initial + canon;
                if (canon0 > 499999) { canon0 -= 499999; }
                if (canon0 < -499999) { canon0 += 499999; }
                //
                canon0 = canon0 / Math.Pow(10, 10);
                return Math.Round(canon0, 10);
            }
            catch (Exception e) {
                TheSys.showError("Err: [" + processing_file + "] " + e.ToString(), true);
                return 0; 
            }
        }

        static public double getAbsolute(double a,double b){
            if(b > a){return b-a;}
            else{return a-b;}
        }
    }
}
