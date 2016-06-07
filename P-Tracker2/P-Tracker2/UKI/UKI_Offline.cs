using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    public class UKI_Offline
    {
        public UKI_Data data = new UKI_Data(true);

        //Offline version of UKI.cs
        public int current_time = 0;//to keep track time change (assum 1 sec = 25)
        public UKI_DataRaw current_data = null;
        public int run_type = 0;

        //if not Full Run, generate only Base posture
        //run_type 0 = default, 1 full_run, -1 movement only
        public void UKI_OfflineProcessing(List<UKI_DataRaw> data_raw, int run_type){
            data.data_raw = data_raw;
            this.run_type = run_type;
            recog_InitialPosture(data_raw.First());
            simulateRunning();
        }

        //use first row
        public void recog_InitialPosture(UKI_DataRaw bp)
        {
            TheInitialPosture.data_raw = bp;
            data.initial_y = bp.Spine[1];
            data.initial_knee_y = (bp.KneeLeft[1] + bp.KneeRight[1]) / 2;
            data.initial_bend_ang = ThePostureCal.calAngle_3D(bp.Head, bp.Spine, 2);
        }

        //compare to UKI.doTrack()
        public void simulateRunning()
        {
            foreach (UKI_DataRaw d in data.data_raw)
            { 
                current_data = d;
                data.process01_calVariable_ver1_mandatory(current_data);
                if (run_type != -1)
                {
                    data.process01_calVariable_ver1_optional(current_data);
                    data.process02_BasicPostureDetection();
                }
                if (run_type == 1) { data.process04_specialPose(); }
                collectData();
                current_time++;
            }
        }

        void collectData()
        {
            data.current_id= current_data.id;
            data.current_time = current_data.time;
            data.tool_posExtract.offline_process(this);
            if (run_type == 1)
            {
                data.collectData_BasicPose();
                data.collectData_Log(current_data.ShoulderCenter[2], current_data.Spine[1]);
                data.collectData_Addition_01();
                data.collectData_Movement();
            }
            else if (run_type == -1)
            {
                data.collectData_Movement();
            }
            else
            {
                data.collectData_BasicPose();
            }
        }

    }
}
