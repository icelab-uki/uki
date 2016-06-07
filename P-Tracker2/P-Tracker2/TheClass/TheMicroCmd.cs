using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class MicroCmd
    {
        /* Microcontroller: POS-monitor
         * using P-command
         */

        static public string cmd_sleep = "@QX;";
        static public string cmd_offAll_butLCD = "@X#7;";
        static public string cmd_clearLCD = "@Q;";
        static public string cmd_blinkLED_on = "@!+;";
        static public string cmd_blinkLED_off = "@!-;";
        //---------------------------------------
        static public string cmd_hello = "@#7QM;Welcome to;@N;OWS Monitoring;";
        static public string cmd_ready = "@#7QM;Skeleton;@N;Searching..;";
        static public string cmd_stand = "@#7QM;Have a good;@N;break;";
        static public string cmd_comeBack = "@X#7QM;Welcome back!!;";
        static public string cmd_pitch = "@#7QM;Pitch Detected;@N;Move your neck;";
        static public string cmd_turn_l = "@#7QM;Turn-L Detected;@N;";
        static public string cmd_turn_r = "@#7QM;Turn-R Detected;@N;";
        static public string cmd_baseRecog = "@#7QM;Base Posture;@N;Recognizing ...;";
        //
        //Good!! Proper Posture
        //Break Time, Let's Walk & Relax


        static public string cmd_LED_off = "@$89ABCD;";
        static public string cmd_LED_time(double a)
        {
            string cmd = "@#";
            if (a >= 1) { cmd += "8";}
            if (a >= 2) { cmd += "9"; }
            if (a >= 3) { cmd += "A"; }
            if (a >= 4) { cmd += "B"; }
            if (a >= 5) { cmd += "C"; }
            if (a >= 6) { cmd += "D"; }
            return cmd + ";";
        }
    }
}
