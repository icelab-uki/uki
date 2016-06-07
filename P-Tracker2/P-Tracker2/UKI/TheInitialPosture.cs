using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace P_Tracker2
{
    //Centralized Storage for Base Posture / Initial Posture
    class TheInitialPosture
    {
        //REALTIME
        static public Skeleton skeleton = null;
        //OFFLINE
        static public UKI_DataRaw data_raw = new UKI_DataRaw();
        //public static UKI_Data_AnalysisForm data_initial = new UKI_Data_AnalysisForm();

    }
}
