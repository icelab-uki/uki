using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    public class DetectorReportData
    {
        public string time = "";
        public int flag_move = 0;
        public int flag_pitch = 0;
        public int flag_twist = 0;//1 = R , -1 = L
        public int flag_stand = 0;
        //
        public int flag_break = 0;
        public int prolong_lv = 0;
        public int pitch_lv = 0;
        public int twist_lv = 0;
        public double total_lv = 0;
        //
        public int prolong_score = 0;
        public int pitch_score = 0;
        public int twist_score = 0;
        public int total_score = 0;
        public string state = "";

        public DetectorReportData(){}
    }
}
