using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    ///<summary>
    /// Person >> 1 Skeleton Data
    /// PersonString >> string version of Person

    public class Person
    {
        public PersonD personD = null;
        public string TimeTrack = "";
        public string TimeSinceStart = "";
        public string Class = "";
        //-------------------
        public double[] Head = new double[] { 0, 0, 0, 0 };
        public double[] ShoulderCenter = new double[] { 0, 0, 0, 0 };    //(Neck in O3KNS)
        public double[] ShoulderLeft = new double[] { 0, 0, 0, 0 };
        public double[] ShoulderRight = new double[] { 0, 0, 0, 0 };
        public double[] ElbowLeft = new double[] { 0, 0, 0, 0 };
        public double[] ElbowRight = new double[] { 0, 0, 0, 0 };
        public double[] WristLeft = new double[] { 0, 0, 0, 0 };         //(HandLeft in O3KNS)
        public double[] WristRight = new double[] { 0, 0, 0, 0 };        //(HandRight in O3KNS)
        //---------
        public double[] HandLeft = new double[] { 0, 0, 0, 0 };          //(exclude in O3KNS)
        public double[] HandRight = new double[] { 0, 0, 0, 0 };         //(exclude in O3KNS)
        //-------------------
        public double[] Spine = new double[] { 0, 0, 0, 0 };             //(Torso in O3KNS)
        public double[] HipCenter = new double[] { 0, 0, 0, 0 };         //(exclude in O3KNS)
        public double[] HipLeft = new double[] { 0, 0, 0, 0 };
        public double[] HipRight = new double[] { 0, 0, 0, 0 };
        public double[] KneeLeft = new double[] { 0, 0, 0, 0 };
        public double[] KneeRight = new double[] { 0, 0, 0, 0 };
        public double[] AnkleLeft = new double[] { 0, 0, 0, 0 };         //(FootLeft in O3KNS)
        public double[] AnkleRight = new double[] { 0, 0, 0, 0 };        //(FootRight in O3KNS)
        public double[] FootLeft = new double[] { 0, 0, 0, 0 };          //(exclude in O3KNS)
        public double[] FootRight = new double[] { 0, 0, 0, 0 };          //(exclude in O3KNS)
    }

    //============================================================

    public class PersonString
    {
        public string TimeTrack { get; set; }
        //-------------------
        public string Head { get; set; }
        public string ShoulderCenter { get; set; }    //(Neck in O3KNS)
        public string ShoulderLeft { get; set; }
        public string ShoulderRight { get; set; }
        public string ElbowLeft { get; set; }
        public string ElbowRight { get; set; }
        public string WristLeft { get; set; }         //(HandLeft in O3KNS)
        public string WristRight { get; set; }        //(HandRight in O3KNS)
        //---------
        public string HandLeft { get; set; }          //(exclude in O3KNS)
        public string HandRight { get; set; }         //(exclude in O3KNS)
        //-------------------
        public string Spine { get; set; }             //(Torso in O3KNS)
        public string HipCenter { get; set; }         //(exclude in O3KNS)
        public string HipLeft { get; set; }
        public string HipRight { get; set; }
        public string KneeLeft { get; set; }
        public string KneeRight { get; set; }
        public string AnkleLeft { get; set; }         //(FootLeft in O3KNS)
        public string AnkleRight { get; set; }        //(FootRight in O3KNS)
        public string FootLeft { get; set; }          //(exclude in O3KNS)
        public string FootRight { get; set; }          //(exclude in O3KNS)
    }

}
