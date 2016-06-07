using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    //------ Manage by TheUKI -----------------------

    //Raw X, Y, Z
    public class UKI_DataRaw
    {
        public int id = 0;
        public String time = "";
        public double[] Head = new double[] { 0, 0, 0 };//X,Y,Z
        public double[] ShoulderCenter = new double[] { 0, 0, 0 };
        public double[] ShoulderLeft = new double[] { 0, 0, 0 };
        public double[] ShoulderRight = new double[] { 0, 0, 0 };
        public double[] ElbowLeft = new double[] { 0, 0, 0 };
        public double[] ElbowRight = new double[] { 0, 0, 0 };
        public double[] WristLeft = new double[] { 0, 0, 0 };
        public double[] WristRight = new double[] { 0, 0, 0 };
        public double[] HandLeft = new double[] { 0, 0, 0 };
        public double[] HandRight = new double[] { 0, 0, 0 };
        //-------------------
        public double[] Spine = new double[] { 0, 0, 0 };
        public double[] HipCenter = new double[] { 0, 0, 0 };
        public double[] HipLeft = new double[] { 0, 0, 0 };
        public double[] HipRight = new double[] { 0, 0, 0 };
        public double[] KneeLeft = new double[] { 0, 0, 0 };
        public double[] KneeRight = new double[] { 0, 0, 0 };
        public double[] AnkleLeft = new double[] { 0, 0, 0 };
        public double[] AnkleRight = new double[] { 0, 0, 0 };
        public double[] FootLeft = new double[] { 0, 0, 0 };
        public double[] FootRight = new double[] { 0, 0, 0 };

        //-- Used only in UKI Analysis----------------
        public double Spine_Y = 0;//r_sp_Y
        public double LKnee_Y = 0;//r_kL_Y
        public double RKnee_Y = 0;//r_kR_Y

        //BE CAREFUL with raw_centerBodyJoint()
    }

    public class UKI_DataRaw_String
    {
        public String id = "";
        public String time = "";// ,time
        public String Head = "";// ,X,Y,Z
        public String ShoulderCenter = "";
        public String ShoulderLeft = "";
        public String ShoulderRight = "";
        public String ElbowLeft = "";
        public String ElbowRight = "";
        public String WristLeft = "";
        public String WristRight = "";
        public String HandLeft = "";
        public String HandRight = "";
        //-------------------
        public String Spine = "";
        public String HipCenter = "";
        public String HipLeft = "";
        public String HipRight = "";
        public String KneeLeft = "";
        public String KneeRight = "";
        public String AnkleLeft = "";
        public String AnkleRight = "";
        public String FootLeft = "";
        public String FootRight = "";
    }

    //Spherical
    public class UKI_DataAngular
    {
        public String time = "";
        public int id = 0;
        public double[] Head = new double[] { 0, 0 };//azimuthal , polar
        public double[] ShoulderCenter = new double[] { 0, 0 };
        public double[] ShoulderLeft = new double[] { 0, 0 };
        public double[] ShoulderRight = new double[] { 0, 0 };
        public double[] ElbowLeft = new double[] { 0, 0 };
        public double[] ElbowRight = new double[] { 0, 0 };
        public double[] WristLeft = new double[] { 0, 0 };
        public double[] WristRight = new double[] { 0, 0 };
        public double[] HandLeft = new double[] { 0, 0 };
        public double[] HandRight = new double[] { 0, 0 };
        //-------------------
        public double[] Spine = new double[] { 0, 0 };
        public double[] HipCenter = new double[] { 0, 0 };
        public double[] HipLeft = new double[] { 0, 0 };
        public double[] HipRight = new double[] { 0, 0 };
        public double[] KneeLeft = new double[] { 0, 0 };
        public double[] KneeRight = new double[] { 0, 0 };
        public double[] AnkleLeft = new double[] { 0, 0 };
        public double[] AnkleRight = new double[] { 0, 0 };
        public double[] FootLeft = new double[] { 0, 0 };
        public double[] FootRight = new double[] { 0, 0 };
    }

    public class UKI_Data_AnalysisForm
    {
        public int id = 0;
        public UKI_DataRaw raw;
        public UKI_Data_BasicPose bp;
    }

    public class UKI_Data_BasicPose
    {
        public int id = 0;
        public String time = "";
        public int[] basic_pose = new int[12];
        /* p_jump, p_lean, p_spin, p_handL_X, p_handL_Y, 
         * p_handL_Z, p_handR_X, p_handR_Y, p_handR_Z, p_handLR_close, 
         * p_legL, p_legR, 
         */
    }

    public class UKI_DataMovement
    {
        public double type = 0;// -1 Min , 1 Max , 0.5 Mid
        public int id = 0;
        public String time = "";
        //
        public double spine_Y = 0;//height from ground
        //moving AVG
        public double ms_all_avg = 0;
        public double ms_hand_avg = 0;
        public double ms_leg_avg = 0;
        public double ms_core_avg = 0;
        //variable from SUM
        public double ms_all = 0;
        public double ms_hand = 0;
        public double ms_leg = 0;
        public double ms_core = 0;
    }

    //e.g. Keep Column & Entropy
    class str_double
    {
        public string str = "";
        public double v = 0;
    }

    class str2_double
    {
        public string str1 = "";
        public string str2 = ""; 
        public double v = 0;
    }

    class str_int
    {
        public string str = "";
        public int i = 0;
    }

    class strList_int
    {
        public List<string> str_list = new List<string>();
        public int v = 0;
    }

    //for generate rule
    public class Feature
    {
        public string name = "";
        public string opt = "";
        public double v = 0;//selected threshold

        //--- for optimization ---
        //public Boolean isOptimized = false;
        public double u = 0;
        public double l = 0;
        public double ceiling = 0;
        public double floor = 0;
        public double momentum = 0;

        public Feature Copy()
        {
            Feature f = new Feature();
            f.name = this.name;
            f.opt = this.opt;
            f.v = this.v;
            //
            f.u = this.u;
            f.l = this.l;
            f.ceiling = this.ceiling;
            f.floor = this.floor;
            f.momentum = this.momentum;
            return f;
        }
    }

    //for Feature Selection DCFS
    public class FeatureDelta
    {
        public string name = "";
        public double delta = 0;
    }
}