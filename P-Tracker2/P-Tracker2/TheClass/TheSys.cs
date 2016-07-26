using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace P_Tracker2
{
    class TheSys
    {
        public static Debugger debugger = new  Debugger();
        public static Boolean debugger_hide = false;

        public static void createDebugger() { debugger = new Debugger(); }

        public static void showError(string txt)
        {
            if (debugger_hide == false)
            {
                debugger.Show();
                debugger.showTxt(txt, true);
            }
        }

        public static void showError(List<string> txt_list)
        {
            if (debugger_hide == false)
            {
                debugger.Show();
                foreach (string txt in txt_list)
                {
                    debugger.showTxt(txt, true);
                }
            }
        }

        //endLine only for case oneLine
        public static void showError(List<string> txt_list, Boolean oneLine, Boolean endLine)
        {
            if (debugger_hide == false)
            {
                debugger.Show();
                int i = 0;
                foreach (string txt in txt_list)
                {
                    if (oneLine && i > 1) { debugger.showTxt(",", false); }
                    debugger.showTxt(txt, !oneLine);
                    i++;
                }
                if (oneLine && endLine) { debugger.showTxt("", true); }
            }
        }

        public static void showError(string txt, Boolean endLine)
        {
            if (debugger_hide == false)
            {
                debugger.Show();
                debugger.showTxt(txt, endLine);
            }
        }
        public static void showError(int i) { showError(i.ToString()); }
        public static void showError(double d) { showError(d.ToString()); }
        public static void showError(Exception ex) { showError(ex.ToString()); }

        public static void showError(string txt, Boolean endLine, Boolean goLastLine)
        {
            if (debugger_hide == false)
            {
                debugger.Show();
                debugger.showTxt(txt, endLine);
            }
            if (goLastLine == true)
            {
                debugger.goLastLine();
            }
        }

        public static void showError(List<int> i_list) {
            int a = 0;
            foreach (int i in i_list) {
                if (a > 0) { TheSys.showError(",", false); } 
                showError(i + "", false); 
                a++; 
            }
            TheSys.showError("");
        }

        public static void showError(List<double> i_list)
        {
            double a = 0;
            foreach (double i in i_list)
            {
                if (a > 0) { TheSys.showError(",", false); }
                showError(i + "", false);
                a++;
            }
            TheSys.showError("");
        }


        public static void showError(List<double> i_list, string txt_join)
        {
            double a = 0;
            foreach (double i in i_list)
            {
                if (a > 0) { TheSys.showError(txt_join, false); }
                showError(i + "", false);
                a++;
            }
            TheSys.showError("");
        }

        public static void showError(int[] i_arr)
        {
            for(int a = 0; a < i_arr.Count(); a++){
                if (a > 0) { TheSys.showError(",", false); }
                showError(i_arr[a] + "", false);
            }
            TheSys.showError("");
        }

        public static void showError(List<int[]> i_list)
        {
            foreach (int[] i in i_list) {
                for (int a = 0; a < i.Count(); a++) {
                    if (a > 0) { TheSys.showError(",", false); } 
                    TheSys.showError(i[a] + "", false); 
                }
                TheSys.showError("");
            }
        }


        public static string[] getText_Array()
        {
            if (debugger != null) { return debugger.getTxt(); }
            else { return new string[1]; }
        }

        public static List<string> getText_List()
        {
            return getText_Array().ToList();
        }

        public static void wait(int millisec)
        {
            Thread.Sleep(millisec);
        }

        //testing double[]
        public static void testData(double[,] arr)
        {
            int bound0 = arr.GetUpperBound(0);//last existng #
            int bound1 = arr.GetUpperBound(1);
            for (int i = 0; i <= bound0; i++)
            {
                for (int x = 0; x <= bound1; x++)
                {
                    TheSys.showError(arr[i, x].ToString());
                }
            }
        }

        public static void testData(int[] arr)
        {
            for (int i = 0; i < arr.Count(); i++) { TheSys.showError("_" + arr[i].ToString(), false); }
        }

        public static void showTime()
        {
            TheSys.showError(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture));
        }
    }
}
