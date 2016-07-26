using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Kinect;

namespace P_Tracker2
{
    //The Whole Data Container
    public class UKI_Data
    {
        Boolean offline_process = true;

        public int current_id = 0;
        public string current_time = "";
        public DateTime time2;

        //Realtime
        public Skeleton posture_current = null;

        public UKI_Data(Boolean offline_process) { this.offline_process = offline_process; }

        public void collectData_realTime(UKI uki)
        {
            this.current_id = uki.data_id;
            this.current_time = uki.time2.ToString("ddHHmmssff");
            collectData_Raw();
            collectData_BasicPose();
            collectData_Log(getZ(JointType.ShoulderCenter), getY(JointType.Spine));
            collectData_Addition_01();
            collectData_Movement();
        }

        public List<UKI_DataRaw> data_raw = new List<UKI_DataRaw>();//Raw Data X,Y,Z - No Header
        public List<UKI_Data_BasicPose> data_bp = new List<UKI_Data_BasicPose>();//Basic Posture
        public List<UKI_DataMovement> data_movement = new List<UKI_DataMovement>();//for Posture Extraction
        public List<String> data_log = new List<String>();//Additional Data (Non-Analysis)
        public List<String> data_add_01 = new List<String>();//Additional Data (For Analysis)
        //Collect 01 during running , Collect 02 after finish by using RAW

        public void clearData()
        {
            data_raw.Clear();
            data_bp.Clear();
            data_movement.Clear();
            data_add_01.Clear();
            data_log.Clear();
        }

        public PosExtract tool_posExtract = new PosExtract();
        public void resetPosExtract() { tool_posExtract = new PosExtract(); }

        public double initial_y = 0;
        public double initial_knee_y = 0;
        public double initial_bend_ang = 0;
        public double cur_bendAngle;
        public double facing_Angle;
        public double diff_bend_Angle;
        public double diff_sp_Y;
        public double diff_kneeL_Y;
        public double diff_kneeR_Y;
        //Centered Joint to SC
        public double diff_hL_sC_X;
        public double diff_hL_sC_Y;
        public double diff_hL_sC_Z;
        public double diff_hR_sC_X;
        public double diff_hR_sC_Y;
        public double diff_hR_sC_Z;
        public double diff_fL_kL_Z;
        public double diff_fR_kR_Z;
        public double diff_handLR_eu;
        public double diff_footLR_Z;

        //-- Basic Posture----
        public int p_jump = 0;              // -1 crouch , 1 jump
        public int p_lean = 0;              // -1 backward , 1 forward
        public int p_spin = 0;              // -1 reverse , 1 clock/right
        public int p_handL_X = 0;
        public int p_handL_Y = 0;
        public int p_handL_Z = 0;
        public int p_handR_X = 0;           // 1 Extend , -1 Cross
        public int p_handR_Y = 0;           // 1 Raise Hand
        public int p_handR_Z = 0;           // 1 Punch
        public int p_legL = 0;
        public int p_handLR_close = 0;      // Two hand closer than 30 cm
        public int p_legR = 0;              // 1 Knee , 2 Kick
        //exluded
        public int px_foot_step = 0;        // -1 step back , 1 forward  
        public int px_footLR_close = 0;      // 2 foot closer than 40 cm

        public void defaultOutput()
        {
            p_jump = 0; p_lean = 0; p_spin = 0;
            p_handR_X = 0; p_handR_Y = 0; p_handR_Z = 0;
            p_handL_X = 0; p_handL_Y = 0; p_handL_Z = 0;
            p_handLR_close = 0;
            p_legR = 0; p_legL = 0;
            px_foot_step = 0;
            px_footLR_close = 0;
        }

        public void process01_calVariable_ver1_mandatory(UKI_DataRaw current_data)
        {
            diff_sp_Y = current_data.Spine[1] - initial_y;
        }

        public void process01_calVariable_ver1_optional(UKI_DataRaw current_data)
        {
            cur_bendAngle = ThePostureCal.calAngle_3D(current_data.Head, current_data.Spine, 2);
            facing_Angle = ThePostureCal.calAngle_3D(current_data.ShoulderLeft, current_data.ShoulderRight, 2);
            diff_bend_Angle = cur_bendAngle - initial_bend_ang;
            diff_kneeL_Y = current_data.KneeLeft[1] - initial_knee_y;
            diff_kneeR_Y = current_data.KneeRight[1] - initial_knee_y;
            //
            diff_hL_sC_X = current_data.HandLeft[0] - current_data.ShoulderCenter[0];
            diff_hL_sC_Y = current_data.HandLeft[1] - current_data.ShoulderCenter[1];
            diff_hL_sC_Z = current_data.HandLeft[2] - current_data.ShoulderCenter[2];
            diff_hR_sC_X = current_data.HandRight[0] - current_data.ShoulderCenter[0];
            diff_hR_sC_Y = current_data.HandRight[1] - current_data.ShoulderCenter[1];
            diff_hR_sC_Z = current_data.HandRight[2] - current_data.ShoulderCenter[2];
            //Additional
            diff_fL_kL_Z = current_data.FootLeft[2] - current_data.KneeLeft[2];
            diff_fR_kR_Z = current_data.FootRight[2] - current_data.KneeRight[2];
            //
            diff_handLR_eu = TheUKI.getDist(current_data.HandLeft, current_data.HandRight);
            diff_footLR_Z = current_data.FootLeft[2] - current_data.FootRight[2];
        }


        public void process01_calVariable_ver2()
        {
            cur_bendAngle = ThePostureCal.calAngle_3D(
                posture_current.Joints[JointType.Head].Position,
                posture_current.Joints[JointType.Spine].Position, 2);
            facing_Angle = ThePostureCal.calAngle_3D(
                posture_current.Joints[JointType.ShoulderLeft].Position,
                posture_current.Joints[JointType.ShoulderRight].Position, 2);
            diff_bend_Angle = cur_bendAngle - initial_bend_ang;
            diff_sp_Y = getY(JointType.Spine) - initial_y;
            diff_kneeL_Y = getY(JointType.KneeLeft) - initial_knee_y;
            diff_kneeR_Y = getY(JointType.KneeRight) - initial_knee_y;
            //
            diff_hL_sC_X = getX_diff(JointType.HandLeft, JointType.ShoulderCenter);
            diff_hL_sC_Y = getY_diff(JointType.HandLeft, JointType.ShoulderCenter);
            diff_hL_sC_Z = getZ_diff(JointType.HandLeft, JointType.ShoulderCenter);
            diff_hR_sC_X = getX_diff(JointType.HandRight, JointType.ShoulderCenter);
            diff_hR_sC_Y = getY_diff(JointType.HandRight, JointType.ShoulderCenter);
            diff_hR_sC_Z = getZ_diff(JointType.HandRight, JointType.ShoulderCenter);
            //Additional
            diff_fL_kL_Z = getZ_diff(JointType.FootLeft, JointType.KneeLeft);
            diff_fR_kR_Z = getZ_diff(JointType.FootRight, JointType.KneeRight);
            //
            diff_handLR_eu = getDist(JointType.HandLeft, JointType.HandRight);
            diff_footLR_Z = getZ_diff(JointType.FootLeft, JointType.FootRight);
        }

        double getX_diff(JointType j1, JointType j2) { return posture_current.Joints[j1].Position.X - posture_current.Joints[j2].Position.X; }
        double getY_diff(JointType j1, JointType j2) { return posture_current.Joints[j1].Position.Y - posture_current.Joints[j2].Position.Y; }
        double getZ_diff(JointType j1, JointType j2) { return posture_current.Joints[j1].Position.Z - posture_current.Joints[j2].Position.Z; }
        double getX(JointType j) { return posture_current.Joints[j].Position.X; }
        double getY(JointType j) { return posture_current.Joints[j].Position.Y; }
        double getZ(JointType j) { return posture_current.Joints[j].Position.Z; }
        double getDist(JointType j1, JointType j2)
        {
            return TheTool.calEuclidian_2Joint(posture_current.Joints[j1],
                posture_current.Joints[j2]);
        }

        public void process02_BasicPostureDetection()
        {
            defaultOutput();
            //------ Basic Posture ----------------------------------
            if (diff_bend_Angle > UKI.p_lean_1_threshold) { p_lean = 1; }
            else if (diff_bend_Angle < UKI.p_lean_m1_threshold) { p_lean = -1; }
            if (diff_sp_Y > UKI.p_jump_1_threshold) { p_jump = 1; }
            else if (p_lean != -1 && diff_sp_Y < UKI.p_jump_m1_threshold) { p_jump = -1; }//Must check after Lean
            if (p_spin_ready)
            {
                if (p_spin_clockwise) { p_spin = 1; }
                else { p_spin = -1; }
            }
            if (diff_hL_sC_X < UKI.p_hand_X_1_threshold) { p_handL_X = 1; }
            else if (diff_hL_sC_X > UKI.p_hand_X_m1_threshold) { p_handL_X = -1; }
            if (diff_hL_sC_Y > UKI.p_hand_Y_1_threshold) { p_handL_Y = 1; }
            else if (diff_hL_sC_Y < UKI.p_hand_Y_m1_threshold) { p_handL_Y = -1; }
            if (diff_hL_sC_Z < UKI.p_hand_Z_1_threshold) { p_handL_Z = 1; }
            else if (diff_hL_sC_Z > UKI.p_hand_Z_m1_threshold) { p_handL_Z = -1; }
            //
            if (diff_hR_sC_X > -UKI.p_hand_X_1_threshold) { p_handR_X = 1; }
            else if (diff_hR_sC_X < -UKI.p_hand_X_m1_threshold) { p_handR_X = -1; }
            if (diff_hR_sC_Y > UKI.p_hand_Y_1_threshold) { p_handR_Y = 1; }
            else if (diff_hR_sC_Y < UKI.p_hand_Y_m1_threshold) { p_handR_Y = -1; }
            if (diff_hR_sC_Z < UKI.p_hand_Z_1_threshold) { p_handR_Z = 1; }
            else if (diff_hR_sC_Z > UKI.p_hand_Z_m1_threshold) { p_handR_Z = -1; }
            //
            if (diff_handLR_eu < UKI.p_handLR_close_1_threshold) { p_handLR_close = 1; }
            //
            if (diff_kneeL_Y > UKI.p_leg_raise_threshold)
            {
                if (diff_fL_kL_Z > UKI.p_leg_knee_threshold) { p_legL = 1; }
                else { p_legL = 2; }
            }
            if (diff_kneeR_Y > UKI.p_leg_raise_threshold)
            {
                if (diff_fR_kR_Z > UKI.p_leg_knee_threshold) { p_legR = 1; }
                else { p_legR = 2; }
            }
            //
            if (diff_footLR_Z > UKI.px_foot_step_1_threshold) { px_foot_step = 1; }
            else if (diff_footLR_Z < UKI.px_foot_step_m1_threshold) { px_foot_step = -1; }
        }

        public void process04_specialPose()
        {
            checkSwitch_Spin();
        }

        public DateTime p_spin_timeCombo = DateTime.Now;
        public int p_spin_timeCombo_offline = 0;//combo in 30 frame
        public Boolean p_spin_ready = false;
        public Boolean p_spin_clockwise = false;
        public int p_spin_step = 0;
        public void checkSwitch_Spin()
        {
            //70-120
            if (p_spin_step == 0 && p_handR_Z < 1 && p_handL_Z < 1)
            {
                if (facing_Angle < 70) { 
                    p_spin_step = 1;  p_spin_clockwise = true;
                    if (offline_process) { p_spin_timeCombo_offline = current_id; }
                    else { p_spin_timeCombo = DateTime.Now; }
                }
                else if (facing_Angle > 110) { 
                    p_spin_step = 1;  p_spin_clockwise = false;
                    if (offline_process) { p_spin_timeCombo_offline = current_id; }
                    else { p_spin_timeCombo = DateTime.Now; }
                }
            }
            else if (p_spin_step == 1)
            {
                if (p_spin_clockwise == true && facing_Angle > 140) { 
                    p_spin_step = 2;
                    if (offline_process) { p_spin_timeCombo_offline = current_id; }
                    else { p_spin_timeCombo = DateTime.Now; }

                }
                else if (p_spin_clockwise == false && facing_Angle < 60) { 
                    p_spin_step = 2; 
                    if (offline_process) { p_spin_timeCombo_offline = current_id; }
                    else { p_spin_timeCombo = DateTime.Now; }
                }
            }
            else if (p_spin_step == 2)
            {
                if (p_spin_clockwise == true && facing_Angle < 90)
                {
                    p_spin_step = 3; p_spin_ready = true;
                    if (offline_process) { p_spin_timeCombo_offline = current_id; }
                    else { p_spin_timeCombo = DateTime.Now; }
                }
                else if (p_spin_clockwise == false && facing_Angle > 90)
                {
                    p_spin_step = 3; p_spin_ready = true;
                    if (offline_process) { p_spin_timeCombo_offline = current_id; }
                    else { p_spin_timeCombo = DateTime.Now; }
                }
            }
            //----------
            Boolean timeout = false;
            if (p_spin_step == 1 || p_spin_step == 2 || p_spin_step == 3)
            {
                if (offline_process && current_id > p_spin_timeCombo_offline + 25) { timeout = true; }
                else if (TheTool.checkTimePass(1000, p_spin_timeCombo, time2)) { timeout = true; }
            }
            //
            if (timeout) { p_spin_step = 0; p_spin_ready = false; }
        }

        //----------------------------------------------------------------------------------------------

        public void rawData_getJointData(ref double[] v, JointType jt)
        {
            v = new double[]{ posture_current.Joints[jt].Position.X, 
                posture_current.Joints[jt].Position.Y, 
                posture_current.Joints[jt].Position.Z };
        }

        // i start at 0
        public void collectData_Raw()
        {
            UKI_DataRaw new_data = new UKI_DataRaw();
            new_data.id = current_id;
            new_data.time = current_time;
            rawData_getJointData(ref new_data.Head, JointType.Head);
            rawData_getJointData(ref new_data.ShoulderCenter, JointType.ShoulderCenter);
            rawData_getJointData(ref new_data.ShoulderLeft, JointType.ShoulderLeft);
            rawData_getJointData(ref new_data.ShoulderRight, JointType.ShoulderRight);
            rawData_getJointData(ref new_data.ElbowLeft, JointType.ElbowLeft);
            rawData_getJointData(ref new_data.ElbowRight, JointType.ElbowRight);
            rawData_getJointData(ref new_data.WristLeft, JointType.WristLeft);
            rawData_getJointData(ref new_data.WristRight, JointType.WristRight);
            rawData_getJointData(ref new_data.HandLeft, JointType.HandLeft);
            rawData_getJointData(ref new_data.HandRight, JointType.HandRight);
            //-------------------
            rawData_getJointData(ref new_data.Spine, JointType.Spine);
            rawData_getJointData(ref new_data.HipCenter, JointType.HipCenter);
            rawData_getJointData(ref new_data.HipLeft, JointType.HipLeft);
            rawData_getJointData(ref new_data.HipRight, JointType.HipRight);
            rawData_getJointData(ref new_data.KneeLeft, JointType.KneeLeft);
            rawData_getJointData(ref new_data.KneeRight, JointType.KneeRight);
            rawData_getJointData(ref new_data.AnkleLeft, JointType.AnkleLeft);
            rawData_getJointData(ref new_data.AnkleRight, JointType.AnkleRight);
            rawData_getJointData(ref new_data.FootLeft, JointType.FootLeft);
            rawData_getJointData(ref new_data.FootRight, JointType.FootRight);
            //===============
            data_raw.Add(new_data);
        }

        public void collectData_BasicPose()
        {
            UKI_Data_BasicPose new_data = new UKI_Data_BasicPose();
            new_data.id = current_id;
            new_data.time = current_time;
            new_data.basic_pose[0] = p_jump;
            new_data.basic_pose[1] = p_lean;
            new_data.basic_pose[2] = p_spin;
            new_data.basic_pose[3] = p_handL_X;
            new_data.basic_pose[4] = p_handL_Y;
            new_data.basic_pose[5] = p_handL_Z;
            new_data.basic_pose[6] = p_handR_X;
            new_data.basic_pose[7] = p_handR_Y;
            new_data.basic_pose[8] = p_handR_Z;
            new_data.basic_pose[9] = p_handLR_close;
            new_data.basic_pose[10] = p_legL;
            new_data.basic_pose[11] = p_legR;
            data_bp.Add(new_data);
        }

        public void collectData_Movement()
        {
            UKI_DataMovement new_data = new UKI_DataMovement();
            new_data.id = current_id;
            new_data.time = current_time;
            new_data.spine_Y = diff_sp_Y;
            new_data.ms_all_avg = tool_posExtract.ms_all_avg;
            new_data.ms_hand_avg = tool_posExtract.ms_hand_avg;
            new_data.ms_leg_avg = tool_posExtract.ms_leg_avg;
            new_data.ms_core_avg = tool_posExtract.ms_core_avg;
            new_data.ms_all = tool_posExtract.ms_all;
            new_data.ms_hand = tool_posExtract.ms_hand;
            new_data.ms_leg = tool_posExtract.ms_leg;
            new_data.ms_core = tool_posExtract.ms_core;
            data_movement.Add(new_data);
        }

        public void collectData_Log(double range, double height)
        {
            data_log.Add(
                current_id + "," +
                current_time + "," +
                initial_y + "," +
                initial_knee_y + "," +
                initial_bend_ang + "," +
                range  + "," +
                height  + "," +
                cur_bendAngle);
        }

        //collect column ""
        public void collectData_Addition_01()
        {
            data_add_01.Add("," +
               facing_Angle + "," +
               diff_bend_Angle + "," +
               diff_sp_Y + "," +
               diff_kneeL_Y + "," +
               diff_kneeR_Y
               );
        }

        //===================================================================

        public void cutData(int pe_cutAt)
        {
            if (pe_cutAt > 0)
            {
                data_raw = TheTool.list_CutAt(data_raw, pe_cutAt);
                data_bp = TheTool.list_CutAt(data_bp, pe_cutAt);
                data_movement = TheTool.list_CutAt(data_movement, pe_cutAt);
                data_add_01 = TheTool.list_CutAt(data_add_01, pe_cutAt);
                data_log = TheTool.list_CutAt(data_log, pe_cutAt);
            }
        }

        //--- Compare UKI.exportFile()
        int center_techq = TheUKI.centerTechq_SC_HC;//center to SC-HC
        int ang_techq = TheUKI.angTechq_SC_HC;//center to SC-HC
        public void exportFile(string fileName, string folderPrefix, Boolean posture_extraction, Boolean useGlobalMinMax, Boolean showDialog)
        {
            try
            {
                if (data_raw.Count() > 0)
                {
                    List<string> temp;
                    string folderPath = TheURL.url_saveFolder + folderPrefix + fileName;
                    string folderPath_oth = folderPath + @"\oth";
                    string path_raw = folderPath + @"\" + fileName + ".csv";
                    string path_center_suffix = " (center" + TheUKI.getCenterTechqName(center_techq) + ")";
                    string path_raw_centered = folderPath + @"\" + fileName + path_center_suffix + ".csv";
                    string path_bp = folderPath + @"\" + fileName + " Atomic.csv";
                    string path_log = folderPath_oth + @"\" + fileName + " Log.csv";
                    string path_plus = folderPath_oth + @"\" + fileName + " Plus.csv";
                    string path_movement = folderPath_oth + @"\" + fileName + " Movement.csv";
                    string path_GlobalMM = TheURL.url_saveFolder + "[MinMax].csv";//in case Global
                    if (useGlobalMinMax == false) { path_GlobalMM = ""; }
                    TheTool.Folder_CreateIfMissing(folderPath);
                    TheTool.Folder_CreateIfMissing(folderPath_oth);
                    //--- RAW -------------------------------------------
                    TheUKI.saveData_Raw(path_raw, data_raw);
                    //--- RAW center to HipC ----------------------------
                    List<UKI_DataRaw> data_raw_centered = TheUKI.raw_centerBodyJoint(data_raw, center_techq);
                    TheUKI.saveData_Raw_centered(path_raw_centered, data_raw_centered, center_techq);
                    //--- Basic Posture ---------------------------------
                    List<string> data_bp_final = new List<string>();
                    data_bp_final.Add(TheUKI.data_bp_header());
                    data_bp_final.AddRange(TheUKI.getBasicPostureData(data_bp));
                    TheTool.exportCSV_orTXT(path_bp, data_bp_final, false);
                    //--- Log : Additional Data (Non-Analysis) ----------
                    List<string> data_log_final = new List<string>();
                    data_log_final.Add(TheUKI.data_log_header);
                    data_log_final.AddRange(data_log);
                    TheTool.exportCSV_orTXT(path_log, data_log_final, false);
                    //----- Movement Data ---------------------------
                    List<UKI_DataMovement> data_movement_adjusted = TheUKI.adjustMovementData(data_movement);
                    if (!posture_extraction) { TheUKI.exportData_Movement(data_movement_adjusted, path_movement, false); }
                    //--- Additional Data For Analysis -----------------------
                    List<string> data_addition_full = TheUKI.createData_Additional(data_raw, data_raw_centered, data_add_01, center_techq);
                    temp = new List<string>();//data_addition_full with header
                    temp.Add(TheUKI.data_addition_full_header);
                    temp.AddRange(data_addition_full);
                    TheTool.exportCSV_orTXT(path_plus, temp, false);
                    
                    //===================================================================
                    if (posture_extraction)
                    {
                        string folderPath_PE = folderPath + @"\PosExtract";
                        string folderPath_Sparse = folderPath + @"\Sparse";
                        string folderPath_Entropy = folderPath + @"\Entropy";
                        //
                        string path_note = folderPath + @"\note.txt";
                        string path_plus_normal = folderPath_oth + @"\" + fileName + " Plus Normal.csv";
                        string path_plus_discrete = folderPath_oth + @"\" + fileName + " Plus Descrete.csv";
                        string path_ang = folderPath_oth + @"\" + fileName + " ANG(" + TheUKI.getAngTechqName(ang_techq) + ").csv";
                        //
                        string path_PE_raw_centered_extract = folderPath_PE + @"\" + fileName + " Extracted-01" + path_center_suffix + ".csv";
                        string path_PE_addition_extract = folderPath_PE + @"\" + fileName + " Extracted-02 Plus.csv";
                        //
                        string path_raw_en = folderPath_Entropy + @"\" + fileName + " Entropy-01 RAW.csv";
                        string path_raw_centered_en = folderPath_Entropy + @"\" + fileName + " Entropy-02 RAW" + path_center_suffix + ".csv";
                        string path_plus_normal_en = folderPath_Entropy + @"\" + fileName + " Entropy-03 Plus Normal.csv";
                        string path_plus_discrete_en = folderPath_Entropy + @"\" + fileName + " Entropy-04 Plus Discrete.csv";
                        string path_ang_en = folderPath_Entropy + @"\" + fileName + " Entropy-11 ANG.csv";
                        string path_dist_en = folderPath_Entropy + @"\" + fileName + " Entropy-12 Dist.csv";
                        //
                        TheTool.Folder_CreateIfMissing(folderPath_PE);
                        TheTool.Folder_CreateIfMissing(folderPath_Sparse);
                        TheTool.Folder_CreateIfMissing(folderPath_Entropy);
                        //--- Normalize & Discretize -------------------------
                        DataTable data_addition_normalized = ThePosExtract.getNormalizedTable(path_plus, path_GlobalMM, true);
                        TheTool.export_dataTable_to_CSV(path_plus_normal, data_addition_normalized);
                        DataTable data_discritized = TheTool.dataTable_discritize10Partition(data_addition_normalized);
                        TheTool.export_dataTable_to_CSV(path_plus_discrete, data_discritized);
                        //----- Angle Data ----------------------------------------
                        List<UKI_DataAngular> data_ang = TheUKI.calAngle_fromRaw(data_raw, ang_techq);
                        TheUKI.saveData_Ang(path_ang, data_ang);//save original
                        //----- (Prepare Key List) ----------------------
                        List<int[]> list_keyPose_Range = new List<int[]>();
                        list_keyPose_Range.AddRange(ThePosExtract.extractKeyPose_MyAlgo(data_movement_adjusted));
                        List<int> list_keyPose_ID = new List<int>();
                        list_keyPose_ID.AddRange(ThePosExtract.getKeyPose_ID_StartEnd(list_keyPose_Range));
                        //----- Movement Data ---------------------------
                        TheUKI.exportData_Movement(data_movement_adjusted, path_movement, true);//must be after "calMinimaMaxima"
                        //----- Basic Posture Analysis ------------------
                        List<UKI_Data_BasicPose> data_bp_selected = TheTool.list_SelectRow(data_bp, list_keyPose_ID);
                        ThePosExtract.BasicPostureAnalysis(data_bp_selected, true);
                        TheTool.exportFile(ThePosExtract.log_BasicPostureAnalysis, path_note, false);
                        TheSys.showError(ThePosExtract.log_BasicPostureAnalysis);
                        //----- Raw Extracted Data ----------------------------
                        List<UKI_DataRaw> data_raw_selected = TheTool.list_SelectRow(data_raw_centered, list_keyPose_ID);
                        TheUKI.saveData_Raw_centered(path_PE_raw_centered_extract, data_raw_selected, center_techq);
                        //----- Addition Extracted Data ----------------------------
                        List<String> data_addition_selected = TheTool.list_SelectRow(data_addition_full, list_keyPose_ID);
                        temp = new List<string>();//data_addition_full with header
                        temp.Add(TheUKI.data_addition_full_header);
                        temp.AddRange(data_addition_selected);
                        TheTool.exportCSV_orTXT(path_PE_addition_extract, temp, false);
                        //======= Feature Selecting Using Delta Analysis ==========================
                        ThePosExtract.ChangeAnalysis(path_PE_addition_extract, path_GlobalMM, folderPath_PE, fileName);
                        //======= Feature Selecting Using MI ======================================

                        //======= Entropy =========================================================
                        ThePosExtract.UKI_CalEntropy_Angle(path_ang_en, path_ang, list_keyPose_Range);
                        ThePosExtract.UKI_CalEntropy_Eu(path_dist_en, path_raw, list_keyPose_Range);
                        ThePosExtract.UKI_CalEntropy_1By1(path_raw_en, path_raw, list_keyPose_Range);
                        ThePosExtract.UKI_CalEntropy_1By1(path_raw_centered_en, path_raw_centered, list_keyPose_Range);
                        ThePosExtract.UKI_CalEntropy_1By1(path_plus_normal_en, path_plus_normal, list_keyPose_Range);
                        ThePosExtract.UKI_CalEntropy_1By1(path_plus_discrete_en, path_plus_discrete, list_keyPose_Range);
                        //======= Oth =========================================================
                        //---- Paper : Sparse ---------------------
                        CalSparse_XYZ.calSparse(data_raw_selected, folderPath_Sparse + @"\", fileName);//cal & export
                    }
                    if(showDialog){System.Windows.MessageBox.Show(@"Save to '" + folderPath + "'", "Export Data");}
                }
            }
            catch (Exception ex) { TheSys.showError("Export: " + ex); }
        }

        
    }
}
