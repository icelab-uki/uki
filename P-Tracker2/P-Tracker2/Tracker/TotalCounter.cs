using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    public class TotalCounter
    {
        public int detector_count = 0;

        public int sit_total = 0;
        public int pitch_total = 0;
        public int bend_total_r = 0;
        public int bend_total_l = 0;
        public int turn_total_r = 0;
        public int turn_total_l = 0;

        public TotalCounter() { }

        public void reset()
        {
            detector_count = 0;
            sit_total = 0;
            pitch_total = 0;
            bend_total_r = 0;
            bend_total_l = 0;
            turn_total_r = 0;
            turn_total_l = 0;
        }

        public void addData(PersonD pd)
        {
            detector_count++;
            if(pd.sit_flag == 1){sit_total++;}
            if(pd.pitch_flag == 1){pitch_total++;}
            if(pd.bend_flag == 1){bend_total_r++;}
            else if(pd.bend_flag == -1){bend_total_l++;}
            if(pd.turn_flag == 1){turn_total_r++;}
            else if(pd.turn_flag == -1){turn_total_l++;}
        }
    }
}
