using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class TheTool_DetectorReportData
    {

        public static void import(string file_path
            , ref List<DetectorReportData> report_data
            , ref  List<string> report_dataString)
        {
            try
            {
                List<string> readedData = TheTool.read_File_getListString(file_path);
                //TheSys.showError(readedData.Count + " : " + file_path);
                report_data = new List<DetectorReportData> { };
                report_dataString = new List<string> { };
                int i = 0;
                DetectorReportData d;
                foreach (string a in readedData)
                {
                    if (i > 0)
                    {
                        report_dataString.Add(a);
                        //-----------------
                        d = new DetectorReportData();
                        string[] b = a.Split(',');
                        d.time = b[0];
                        d.flag_move = int.Parse(b[1]);
                        d.flag_pitch = int.Parse(b[2]);
                        d.flag_twist = int.Parse(b[3]);
                        d.flag_stand = int.Parse(b[4]);
                        //
                        d.flag_break = int.Parse(b[5]);
                        d.prolong_lv = int.Parse(b[6]);
                        d.pitch_lv = int.Parse(b[7]);
                        d.twist_lv = int.Parse(b[8]);
                        d.total_lv = double.Parse(b[9]);
                        //
                        d.prolong_score = int.Parse(b[10]);
                        d.pitch_score = int.Parse(b[11]);
                        d.twist_score = int.Parse(b[12]);
                        d.total_score = int.Parse(b[13]);
                        d.state = b[14];
                        //
                        report_data.Add(d);
                    }
                    i++;//Skip Header
                }
            }
            catch (Exception e)
            {
                TheSys.showError("import:" + e.Message, true);
            }
        }

    }
}
