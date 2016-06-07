using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Data;

namespace P_Tracker2
{
    public partial class UKI : Window
    {
        
        UserTracker mainForm;
        public UKI_FightingICE uki_fightingICE = null;
        int buffer = 0;

        public UKI(UserTracker mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            txtPath.Text = TheTool.read_File_get1String(TheURL.url_config_FTG);
            uki_fightingICE = new UKI_FightingICE(this);//must be after txtPath.Text;
            uki_fightingICE.path_fightingICE = txtPath.Text;
            refreashMotionList();
            refreashEventList();
            loadXMLtoCombo();
            if (comboUKIXML.Items.Count > 0) { comboUKIXML.SelectedIndex = 1; }
        }

        //Main Process
        void doTrack()
        {
            try
            {
                time2 = mainForm.thisTime;
                if (buffer == 0 || TheTool.checkTimePass(buffer, time1, time2))
                {
                    time1 = time2;
                    uki_data.posture_current = posture_current;
                    uki_data.time2 = time2;
                    //
                    uki_data.process01_calVariable_ver2();
                    uki_data.process02_BasicPostureDetection();
                    process03_ftgOutput();
                    uki_data.process04_specialPose();
                    process04_2_specialPose();
                    process05_sendInput();
                    collectData();
                    mainForm.showTimeMeter(time2.ToString("HH:mm:ss.ff") + " (" + data_id +")");
                }
            }
            catch (Exception ex) { TheSys.showError("FTG_track:" + ex.Message, true); }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp += new System.Windows.Input.KeyEventHandler(hotKey);
        }

        void hotKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (checkHotKey.IsChecked.Value)
            {
                if (e.Key == Key.F1)
                {
                    checkLock.IsChecked = true;
                    checkRun.IsChecked = false;
                    return;
                }
                if (e.Key == Key.F11)
                {
                    completeStop();
                    return;
                }
            }
        }

        //------ FightingICE input ---------------------------------------------------------
        public int key_Atk = 0;
        public int key_X = 0;
        public int key_Y = 0;
        public int key_X_double = 0;
        public int key_Special = 0;
        void defaultFTG() { key_Atk = 0; key_X = 0; key_Y = 0; key_X_double = 0; key_Special = 0; }


        //======= Data for Processing ======================================================
        public DateTime time1 = DateTime.Now;
        public DateTime time2;
        public DateTime time_start;
        Boolean is_waitForBasePosture = true;

        

        //================= Setting =========================================
        private void comboBuff_DropDownClosed(object sender, EventArgs e)
        {
            buffer = (int)(TheTool.getDb(comboBuff) * 1000);
        }

        //Apply FightingICE AI folder
        public void butApply_Click(object sender, RoutedEventArgs e)
        {
            uki_fightingICE.setPath(txtPath.Text);
        }

        private void butFTGOpen_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(txtPath.Text); }
            catch { }
        }

        //================= Tracking ========================================
        public Skeleton posture_current = null;
        public void process(Skeleton trackedPerson)
        {
            this.posture_current = trackedPerson;
            if (is_waitForBasePosture)
            {
                check_initializeBasePosture();//No Base Yet
                if (pe_readying) { pe_preStart_toTime = mainForm.sec_since_start + pe_preStart_cooldown_const; }
            }
            else if (checkRun.IsChecked.Value)
            {          
                if (checkRun.IsChecked.Value) { doTrack(); }
                checkAutoStop();
            }
            else
            {
                checkAutoStart();
            }
            checkMarking();
            setIcon();
        }

        void checkMarking()
        {
            if (marking){
                double timeLeft_ms = marker_endTime.Subtract(mainForm.thisTime).TotalMilliseconds;
                marker_timeLeft = (int) Math.Ceiling(timeLeft_ms / 1000);
                
                if (marker_timeLeft < 1) { checkMark.IsChecked = false; }
            }
        }

        public void check_initializeBasePosture()
        {
            if (checkPose_JiangShi())
            {
                TheInitialPosture.skeleton = posture_current;
                is_waitForBasePosture = false;
                //------------
                uki_data.initial_y = getY(JointType.Spine);
                uki_data.initial_knee_y = (getY(JointType.KneeRight) + getY(JointType.KneeLeft)) / 2;
                uki_data.initial_bend_ang = ThePostureCal.calAngle_3D(
                    posture_current.Joints[JointType.Head].Position,
                    posture_current.Joints[JointType.Spine].Position, 2);
                //------------
                recog_baseStand();
            }            
        }

        public void checkAutoStart()
        {
            if (checkLock.IsChecked == false)
            {
                if (pe_readying) { pe_02_prestart(); }//Start by Posture Recognition
                else
                {
                    if (checkStart_Pose.IsChecked.Value)
                    {
                        checkStart_byGreetingPos();
                    }
                    if (checkStart_Range.IsChecked.Value)
                    {
                        if (getZ(JointType.ShoulderCenter) > 2.5)
                        {
                            checkRun.IsChecked = true;
                        }
                    }
                }
            }
        }

        public void checkStart_byGreetingPos()
        {
            if (checkPose_Greeting())
            {
                if (switch_pos_greeting) { checkRun.IsChecked = true; switch_pos_greeting = false; }
            }
            else if (getDist(JointType.HandLeft,JointType.HandRight) > .40)
            {
                switch_pos_greeting = true;
            }
        }

        public void checkAutoStop()
        {
            if (pe_processing) { pe_03_prestop(); }//during Posture Extraction
            else
            {
                //Stop due to range
                if (checkStopByRange.IsChecked.Value && posture_current.Joints[JointType.ShoulderCenter].Position.Z < 1)
                {                    
                    checkRun.IsChecked = false;
                    if (radio11.IsChecked.Value) { uki_fightingICE.fightingICE_zeroCmd(); }
                }
                //Stop by Pose
                else if (checkStop_Pose.IsChecked.Value)
                {
                    if (checkPose_GreetingEnd())
                    {
                        if (switch_pos_greeting) { checkRun.IsChecked = false; switch_pos_greeting = false; }
                    }
                    else if (getDist(JointType.HandLeft,JointType.HandRight) > .40)
                    {
                        switch_pos_greeting = true;
                    }
                }
            }
        }
        
        //====================================================================================
        //===== Data Collection ==============================================================

        void collectData()
        {
            if (checkFTGData.IsChecked.Value)
            {
                uki_data.tool_posExtract.process(this);
                uki_data.collectData_realTime(this);
                data_id = uki_data.data_raw.Count(); 
                txtDataCount.Content = "# " + data_id;
            }
        }

        //----------------------------------------------------------------

        public static double p_lean_1_threshold = 12; //lean forward
        public static double p_lean_m1_threshold = -3; // lean backward
        public static double p_jump_1_threshold = 0.08; //jump
        public static double p_jump_m1_threshold = -0.15; //crouch
        //
        public static double p_hand_X_1_threshold = -0.4; //Hand extend
        public static double p_hand_X_m1_threshold = 0.15; //Hand cross
        public static double p_hand_Y_1_threshold = 0.30; //Hand raise
        public static double p_hand_Y_m1_threshold = -0.40; //Hand down
        public static double p_hand_Z_1_threshold = -.35; //Hand punch
        public static double p_hand_Z_m1_threshold = .30; //Hand behind
        //
        public static double p_handLR_close_1_threshold = .4; //2H close
        public static double p_leg_raise_threshold = .12; //Leg raise
        public static double p_leg_knee_threshold = -0.1; //Knee Strike - Kick
        public static double px_foot_step_1_threshold = .35; //Step up
        public static double px_foot_step_m1_threshold = -.4; //Step back

        public static List<m_If> convertBasicPosture_toRelativePosture(m_If rule_basic)
        {
            List<m_If> rule_RelativePosition = new List<m_If>();
            //p_lean
            //p_jump
            //p_spin
            if (rule_basic.v == "handL_X" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft); 
                rule1.v2 =TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "X"; rule1.opt = getOpt("<", rule_basic); 
                rule1.value_d = p_hand_X_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handL_X" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft); 
                rule1.v2 =TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "X"; rule1.opt = getOpt(">", rule_basic); 
                rule1.value_d = p_hand_X_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handL_X" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "X"; rule1.opt = getOpt(">=", rule_basic);
                rule1.value_d = p_hand_X_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.HandLeft);
                rule2.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule2.axis = "X"; rule2.opt = getOpt("<=", rule_basic);
                rule2.value_d = p_hand_X_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            else if (rule_basic.v == "handL_Y" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft); rule1.v2 =TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Y"; rule1.opt = getOpt(">", rule_basic); 
                rule1.value_d = p_hand_Y_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handL_Y" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft); 
                rule1.v2 =TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Y"; rule1.opt = getOpt("<", rule_basic); 
                rule1.value_d = p_hand_Y_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handL_Y" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft); rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Y"; rule1.opt = getOpt("<=", rule_basic);
                rule1.value_d = p_hand_Y_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.HandLeft);
                rule2.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule2.axis = "Y"; rule2.opt = getOpt(">=", rule_basic);
                rule2.value_d = p_hand_Y_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            else if (rule_basic.v == "handL_Z" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Z"; rule1.opt = getOpt("<", rule_basic);
                rule1.value_d = p_hand_Z_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handL_Z" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Z"; rule1.opt = getOpt(">", rule_basic); 
                rule1.value_d = p_hand_Z_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handL_Z" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandLeft);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Z"; rule1.opt = getOpt(">=", rule_basic);
                rule1.value_d = p_hand_Z_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.HandLeft);
                rule2.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule2.axis = "Z"; rule2.opt = getOpt("<=", rule_basic); 
                rule2.value_d = p_hand_Z_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            //
            else if (rule_basic.v == "handR_X" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "X"; rule1.opt = getOpt(">", rule_basic);
                rule1.value_d = -p_hand_X_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handR_X" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "X"; rule1.opt = getOpt("<", rule_basic);
                rule1.value_d = -p_hand_X_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handR_X" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "X"; rule1.opt = getOpt("<=", rule_basic);
                rule1.value_d = -p_hand_X_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.HandRight);
                rule2.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule2.axis = "X"; rule2.opt = getOpt(">=", rule_basic);
                rule2.value_d = -p_hand_X_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            else if (rule_basic.v == "handR_Y" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Y"; rule1.opt = getOpt(">", rule_basic);
                rule1.value_d = p_hand_Y_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handR_Y" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Y"; rule1.opt = getOpt("<", rule_basic);
                rule1.value_d = p_hand_Y_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handR_Y" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Y"; rule1.opt = getOpt("<=", rule_basic);
                rule1.value_d = p_hand_Y_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.HandRight);
                rule2.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule2.axis = "Y"; rule2.opt = getOpt(">=", rule_basic);
                rule2.value_d = p_hand_Y_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            else if (rule_basic.v == "handR_Z" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Z"; rule1.opt = getOpt("<", rule_basic);
                rule1.value_d = p_hand_Z_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handR_Z" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Z"; rule1.opt = getOpt(">", rule_basic);
                rule1.value_d = p_hand_Z_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handR_Z" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule1.axis = "Z"; rule1.opt = getOpt(">=", rule_basic);
                rule1.value_d = p_hand_Z_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.HandRight);
                rule2.v2 = TheMapData.getJointName(JointType.ShoulderCenter);
                rule2.axis = "Z"; rule2.opt = getOpt("<=", rule_basic);
                rule2.value_d = p_hand_Z_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            //p_legL
            //p_legR
            else if (rule_basic.v == "handLR_close" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.HandLeft);
                rule1.axis = "D"; rule1.opt = getOpt("<", rule_basic);
                rule1.value_d = p_handLR_close_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "handLR_close" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.HandRight);
                rule1.v2 = TheMapData.getJointName(JointType.HandLeft);
                rule1.axis = "D"; rule1.opt = getOpt(">=", rule_basic);
                rule1.value_d = p_handLR_close_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "step" && rule_basic.value == 1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.FootLeft);
                rule1.v2 = TheMapData.getJointName(JointType.FootRight);
                rule1.axis = "Z"; rule1.opt = getOpt(">", rule_basic);
                rule1.value_d = px_foot_step_1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "step" && rule_basic.value == -1)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.FootLeft);
                rule1.v2 = TheMapData.getJointName(JointType.FootRight);
                rule1.axis = "Z"; rule1.opt = getOpt("<", rule_basic);
                rule1.value_d = px_foot_step_m1_threshold;
                rule_RelativePosition.Add(rule1);
            }
            else if (rule_basic.v == "step" && rule_basic.value == 0)
            {
                m_If rule1 = new m_If(); rule1.type = TheMapData.if_type_2Joint;
                rule1.v = TheMapData.getJointName(JointType.FootLeft);
                rule1.v2 = TheMapData.getJointName(JointType.FootRight);
                rule1.axis = "Z"; rule1.opt = getOpt("<=", rule_basic);
                rule1.value_d = px_foot_step_1_threshold;
                rule_RelativePosition.Add(rule1);
                m_If rule2 = new m_If(); rule2.type = TheMapData.if_type_2Joint;
                rule2.v = TheMapData.getJointName(JointType.FootLeft);
                rule2.v2 = TheMapData.getJointName(JointType.FootRight);
                rule2.axis = "Z"; rule2.opt = getOpt(">=", rule_basic);
                rule2.value_d = px_foot_step_m1_threshold;
                rule_RelativePosition.Add(rule2);
            }
            else
            {
                rule_RelativePosition.Add(rule_basic);//unable to convert
            }
            return rule_RelativePosition;
        }

        //get operator, auto reverse
        public static string getOpt(string opt, m_If origin)
        {
            if (origin.opt == "!=") { return TheTool.math_getOptReversion(opt, true); }
            else { return opt; }
        }

        void process03_ftgOutput()
        {
            //FTG
            key_Atk = 0;
            key_X = 0;
            key_Y = 0;
            key_X_double = 0;
            key_Special = 0;
            //=========== FTG Detection ===============================================
            if (radioFTG122.IsChecked.Value)
            {
                //------ ver 1.22 -----
                //detect Xdb ---------
                if (uki_data.p_handL_Z == 1) { key_Atk = 2; }
                else if (uki_data.p_handR_Z == 1) { key_Atk = 1; }
                //detect X ---------                    
                if (uki_data.p_lean == 1) { key_X = 1; key_X_double = 1; }
                else if (uki_data.p_lean == -1)
                {
                    if (uki_data.diff_footLR_Z < -.4) { key_X = -1; key_X_double = 1; }
                    else { key_X = -1; key_Y = 1; }
                }
                else
                {
                    if (uki_data.px_foot_step == 1) { key_X = 1; }
                    else if (uki_data.px_foot_step == -1 && key_Atk == 0) { key_X = -1; }
                }
            }
            else if (radioFTGx.IsChecked.Value)
            {
                //------ ver X -----
                //detect Xdb ---------
                if (uki_data.p_handR_Z == 1 && uki_data.p_handL_Z == 1) { key_X = -1; }
                else if (uki_data.p_handR_Z == 1) { key_Atk = 1; }
                else if (uki_data.p_handL_Z == 1) { key_Atk = 2; }
                if (key_X == 0)
                {
                    //detect X ---------                    
                    if (uki_data.p_lean == 1) { key_X = 1; key_X_double = 1; }
                    else if (uki_data.p_lean == -1)
                    {
                        if (uki_data.diff_footLR_Z < -.4) { key_X = -1; key_X_double = 1; }
                        else { key_X = -1; key_Y = 1; }
                    }
                    else
                    {
                        if (uki_data.px_foot_step == 1) { key_X = 1; }
                    }
                }
            }
            if (key_Y == 0 && key_X_double == 0) { key_Y = uki_data.p_jump; }
        }

        void process04_2_specialPose(){
            checkPose_Knifthand();
            checkPose_Hadouken();
            if (radio13.IsChecked.Value == false)
            {
                checkSwitch_Side();//in case not XML
            }
        }

        public Boolean p_ultimateReady = false;
        public Boolean p_ultimateRelease = false;
        public DateTime p_ultimate_timeCombo = DateTime.Now;
        void checkPose_Knifthand()
        {
            if (uki_data.p_lean == 0 && uki_data.p_handR_Y == 1
                && p_hadoukenReady == false && p_hadoukenRelease == 0)
            {
                p_ultimateReady = true;//Ready
                p_ultimate_timeCombo = time2;
            }
            else if (p_ultimateReady == true && uki_data.p_handR_Y < 1)
            {
                if (uki_data.p_handR_Z == 1) //Release
                {
                    key_Special = 3; p_ultimateRelease = true;
                }
                else //Cancle by lower hand at the back
                {
                    p_ultimateReady = false; p_ultimateRelease = false;
                }
            }
            //Cancle by Jump or Time
            if (uki_data.p_jump == 1 || TheTool.checkTimePass(1500, p_ultimate_timeCombo, time2))
            {
                p_ultimateReady = false; p_ultimateRelease = false;
            }
        }

        public Boolean p_hadoukenReady = false;
        public int p_hadoukenRelease = 0;
        public DateTime p_hadouken_timeCombo = DateTime.Now;
        void checkPose_Hadouken()
        {
            if (uki_data.p_lean == 0 && uki_data.p_handL_Z == 0 && uki_data.p_handR_Z == 0
                && uki_data.p_handL_X == -1 && uki_data.p_handLR_close == 1)
            {
                p_hadoukenReady = true;
                p_hadouken_timeCombo = time2;
            }
            if (p_hadoukenReady)
            {
                if (uki_data.p_handL_Z == 1 && uki_data.p_handR_Z == 1)
                {
                    if (uki_data.diff_hL_sC_Y > 0.15 && uki_data.diff_hR_sC_Y > 0.15)
                    {
                        key_Special = 2; p_hadoukenRelease = 2;
                    }
                    else { key_Special = 1; p_hadoukenRelease = 1; }
                }
                else if (uki_data.diff_handLR_eu > .40)
                {
                    p_hadoukenReady = false; p_hadoukenRelease = 0;
                }
            }
            //Cancle by Time
            if (TheTool.checkTimePass(1500, p_hadouken_timeCombo, time2))
            {
                p_hadoukenReady = false; p_hadoukenRelease = 0;
            }
        }

        public Boolean p_switchSide_on = false;
        public Boolean p_switchSide_ready = true;
        void checkSwitch_Side()
        {
            if (check_SideSwitch.IsChecked.Value)
            {
                if (uki_data.p_jump == 0
                    && p_ultimateReady == false && p_hadoukenReady == false
                    && p_ultimateRelease == false && p_hadoukenRelease == 0
                    && p_switchSide_ready)
                {
                    if (uki_data.p_handL_Y == 1)
                    {
                        switch_switchSide(-1);
                        p_switchSide_ready = false;
                    }
                }
                else
                {
                    if (uki_data.p_handL_Y == -1)
                    {
                        p_switchSide_ready = true;
                    }
                }
            }
        }

        void process05_sendInput()
        {
            if (radio11.IsChecked.Value) { 
                checkPose_FTG_special();//muse be after Knifthand & Hadouken
                uki_fightingICE.fightingICE_sendInput(); 
            }
            else
            {
                cal3Dmove();
                //Using Detection Result
                if (radio13.IsChecked.Value)
                {
                    processMAP_XMLbasedSendInput();
                }
            }
        }

        //========================================================================
        //====== Check Spacial Posture ===========================================
        Boolean switch_pos_greeting = true;
        //stand alone (do not use variable)
        public Boolean checkPose_Greeting()
        {
            Boolean output = false;
            if (getZ(JointType.ShoulderCenter) > 1.5
                && getDist(JointType.HandLeft, JointType.HandRight) < 0.2
                && getY_diff(JointType.HandLeft, JointType.ShoulderCenter) < 0 
                && getY_diff(JointType.HandRight, JointType.ShoulderCenter) < 0
                && getZ_diff(JointType.HandLeft, JointType.ShoulderCenter) > -0.35
                && getZ_diff(JointType.HandRight, JointType.ShoulderCenter) > -0.35
                && Math.Abs(getY(JointType.Spine) - uki_data.initial_y) < .1
                )
            {
                output = true;
            }
            return output;
        }

        //use Basic Variable
        public Boolean checkPose_GreetingEnd()
        {
            Boolean output = false;
            if (uki_data.diff_handLR_eu < 0.2
                && uki_data.diff_hL_sC_Y > 0
                && uki_data.diff_hR_sC_Y > 0
                && uki_data.p_handL_Z == 0
                && uki_data.p_handR_Z == 0
                && uki_data.p_jump == 0
                )
            {
                output = true;
            }
            return output;
        }

        //stand alone (do not use variable)
        public Boolean checkPose_JiangShi()
        {
            Boolean output = false;
            //---- JiangShi -----
            double eAngleL = getAng(JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft);
            double eAngleR = getAng(JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight);
            if (getY_diff(JointType.HandRight, JointType.ShoulderCenter) > -.3 
                && getY_diff(JointType.HandLeft, JointType.ShoulderCenter) > -.3
                && getZ_diff(JointType.HandRight, JointType.ShoulderCenter) < -.35
                && getZ_diff(JointType.HandLeft, JointType.ShoulderCenter) < -.35
                ) { output = true; }
            return output;
        }

        
        void checkPose_FTG_special()
        {
            //Special attack on FightingICE
            if (radio11.IsChecked.Value && key_Special == 0)
            {
                if (key_Y == 0)
                {
                    if (uki_data.p_legR == 1) { key_Special = 6; }
                    else if (uki_data.p_legL == 1) { key_Special = 7; }
                    else if (key_X == 1 && key_X_double == 1 && key_Atk == 1) { key_Special = 4; }
                    else if (key_X == 1 && key_X_double == 1 && key_Atk == 2) { key_Special = 5; }
                }
            }
        }

        void cal3Dmove()
        {
            double temp_d;
            temp_d = posture_current.Joints[JointType.HipCenter].Position.X;
            double moveBase_Xd = temp_d - base_stand_X;
            temp_d = posture_current.Joints[JointType.HipCenter].Position.Z;
            double moveBase_Zd = temp_d - base_stand_Z;
            if (moveBase_Zd < -.3) { px_moveBase_Z = 1; }
            else if (moveBase_Zd > .3) { px_moveBase_Z = -1; }
            if (moveBase_Xd < -.3) { px_moveBase_X = -1; }
            else if (moveBase_Xd > .3) { px_moveBase_X = 1; }
        }

        //============================================================================================
        //========= Get X Y Z , etc ==================================================================

        double getX_diff(JointType j1, JointType j2) { return posture_current.Joints[j1].Position.X - posture_current.Joints[j2].Position.X; }
        double getY_diff(JointType j1, JointType j2) { return posture_current.Joints[j1].Position.Y - posture_current.Joints[j2].Position.Y; }
        double getZ_diff(JointType j1, JointType j2) { return posture_current.Joints[j1].Position.Z - posture_current.Joints[j2].Position.Z; }
        double getX(JointType j){return posture_current.Joints[j].Position.X;}
        double getY(JointType j) { return posture_current.Joints[j].Position.Y; }
        double getZ(JointType j) { return posture_current.Joints[j].Position.Z; }
        double getDist(JointType j1, JointType j2)
        {
            return TheTool.calEuclidian_2Joint(posture_current.Joints[j1],
                posture_current.Joints[j2]);
        }

        double[] getJoint(JointType j1)
        {
            return new double[] { posture_current.Joints[j1].Position.X, posture_current.Joints[j1].Position.Y, posture_current.Joints[j1].Position.Z };
        }

        double getAng(JointType j1, JointType j2, JointType j3)
        {
            return ThePostureCal.calAngle_3Joints(
                posture_current.Joints[j1], posture_current.Joints[j2],posture_current.Joints[j3]);
        }

        // Head.Z - ShoulderCenter.Z >= 0
        // "Z", Head, ShoulderCenter, ">=", 0
        Boolean checkInput_2JRelative(String axis, JointType j1, JointType j2, String opt, double value){
            Boolean pass = false;
            try{
                double diff = 0;
                if(axis == "X"){ diff = getX_diff(j1,j2); }
                else if(axis == "Y"){ diff = getY_diff(j1,j2); }
                else if(axis == "Z"){ diff = getZ_diff(j1,j2); }
                else { diff = getDist(j1,j2); }
                pass = TheTool.checkOpt(diff, opt, value);
            } catch{}
            return pass;
        }

        //==========================================================================================

        public void switch_switchSide(int cmd)
        {
            if (cmd == 1) { p_switchSide_on = true; }
            else if (cmd == 0) { p_switchSide_on = false; }
            else { p_switchSide_on = !p_switchSide_on; }
        }

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(TheURL.url_saveFolder);
            }
            catch {  }
        }

        private void checkRun_Checked(object sender, RoutedEventArgs e)
        {
            if (checkRun.IsChecked.Value)
            {
                time_start = DateTime.Now; uki_data.tool_posExtract = new PosExtract();
                if (pe_readying == true) { pe_processing = true; pe_icon(); }
                checkMark.IsEnabled = true;
            }
            else {
                map_PriorityQue = null;
                map_PriorityQue_start = false;
                icon_temp_timeout = DateTime.Now;
                icon_static_content = "T";
                icon_static_color = Brushes.Cyan;
                icon_temp_content = "";
                icon_temp_color = Brushes.Cyan;
                pe_04_off();
                detectMap.variables = detectMap_variables;//refresh variable
                checkMark.IsEnabled = false;
                listReplace_default();
            }
            setIcon();
        }

        //listReplace should not be .Clear()
        public void listReplace_default()
        {
            foreach (m_ReplaceKey k in list_keyReplace)
            {
                k.keyNew = "";
            }
        }


        private void butReset_Click(object sender, RoutedEventArgs e)
        {
            is_waitForBasePosture = true;
            setIcon();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try { mainForm.form_UKI = null; }
            catch { }
        }

        private void butAIhelp_Click(object sender, RoutedEventArgs e)
        {
            txtPath.Text = @"[Path]\FightingICE\data\ai\AI_Kinect\FTG_input.txt";
        }

        //===========================================================================
        //==== Change Icon by XML base ===============================================
        public DateTime icon_temp_timeout = DateTime.Now;
        public string icon_static_content = "T";
        public Brush icon_static_color = Brushes.Cyan;
        public string icon_temp_content = "";
        public Brush icon_temp_color = Brushes.Cyan;

        void setIcon()
        {
            String new_content = "T";
            Brush new_color = Brushes.Cyan;
            // T = ice , Y = Star of David , [ = YinYang
            if (is_waitForBasePosture) { new_color = Brushes.DimGray; }
            else if (checkRun.IsChecked.Value)
            {
                if (marking) {
                    txtPE.Content = marker_timeLeft.ToString();
                    txtPE.Foreground = Brushes.Yellow;
                }
                if (radio13.IsChecked.Value)
                { 
                    //XML base
                    if (icon_temp_content != "" && DateTime.Compare(icon_temp_timeout, DateTime.Now) > 0)
                    {
                        new_color = icon_temp_color;
                        new_content = icon_temp_content;
                    }
                    else if (icon_static_content != "")
                    {
                        new_color = icon_static_color;
                        new_content = icon_static_content;
                    }
                }
                else if (checkHideIcon.IsChecked.Value == false)
                {
                    if (uki_data.p_spin_ready) { new_content = "["; }
                    else if (p_switchSide_on) { new_content = "Y"; }
                    //------
                    if (p_hadoukenRelease > 0) { new_color = Brushes.White; new_content = "R"; }
                    else if (p_ultimateRelease) { new_color = Brushes.White; new_content = "]"; }
                    else if (p_hadoukenReady) { new_color = Brushes.Yellow; new_content = "R"; }
                    else if (p_ultimateReady) { new_color = Brushes.Yellow; new_content = "]"; }
                    else { new_color = Brushes.Cyan; }
                }
            }
            else { new_color = Brushes.Red; }
            //-----------
            if (map_PriorityQue == null)
            {
                txtOn.Content = new_content;
                txtOn.Foreground = new_color;
            }
        }

        //===== Data Collector ================
        public UKI_Data uki_data = new UKI_Data(false);

        private void butFTGreset_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Clear Data"
                                           , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                clearData();
            }
        }

        public int data_id = 0;
        void clearData()
        {
            data_id = 0;
            marker_data_id_last = 0;
            checkMark.IsChecked = false;
            data_mark.Clear();
            uki_data.clearData();
            txtDataCount.Content = "# 0";
        }

        public List<int[]> data_mark = new List<int[]>();
        public int marker_data_id_last = 0;
        DateTime marker_endTime = DateTime.Now;
        Boolean marking = false;
        int marker_timeLeft = 0;

        private void checkMark_Checked(object sender, RoutedEventArgs e)
        {
            marking = checkMark.IsChecked.Value;
            if (data_id > marker_data_id_last)
            {
                data_mark.Add(new int[] { data_id, TheTool.convertBoolean_01int(checkMark.IsChecked.Value) });
                marker_data_id_last = data_id;
            }
            if (marking)
            {
                marker_endTime = mainForm.thisTime.AddSeconds(TheTool.getDouble(txtMarkerSecond));
                txtPE.Visibility = Visibility.Visible; txtOn.Visibility = Visibility.Hidden;
            }
            else { txtOn.Visibility = Visibility.Visible; txtPE.Visibility = Visibility.Hidden; }
        }

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++=
        //====== Universal Kinect Interface ==========================

        //--Variable for 3D Movement ----
        public int px_moveBase_X = 0;       // -1 left , 1 right
        public int px_moveBase_Z = 0;       // -1 backward , 1 forward

        void defaultOutput()
        {
            px_moveBase_X = 0;
            px_moveBase_Z = 0;
        }

        //base posture for calculate 3D Movement
        double base_stand_X = 0;
        double base_stand_Z = 0;
        void recog_baseStand()
        {
            base_stand_X = posture_current.Joints[JointType.HipCenter].Position.X;
            base_stand_Z = posture_current.Joints[JointType.HipCenter].Position.Z;
        }

        //=========== XML-base detection===========================================================================

        //------ XML setup ----------------------------------------------

        String XML_map_file_path = "";
        public MapData detectMap = new MapData();
        public List<m_Variable> detectMap_variables = new List<m_Variable>();//for refresh variable

        String XML_motion_file_path = TheURL.url_saveFolder + TheURL.url_9_UKIMap + @"\[Motions].xml";
        public List<m_Motion> list_motions = new List<m_Motion>();

        String XML_event_file_path = TheURL.url_saveFolder + TheURL.url_9_UKIMap + @"\[Events].xml";
        public List<m_Event> list_events = new List<m_Event>();

        void refreashMotionList()
        {
            try { list_motions = TheMapData.loadXML_motion(XML_motion_file_path); }
            catch (Exception ex) { TheSys.showError("Failed to load [Motions].xml : " + ex.ToString()); }
        }

        void refreashEventList()
        {
            try { list_events = TheMapData.loadXML_event(XML_event_file_path); }
            catch (Exception ex) { TheSys.showError("Failed to load [Events].xml : " + ex.ToString()); }
        }

        void loadXMLtoCombo()
        {
            String folder_path = TheURL.url_saveFolder + TheURL.url_9_UKIMap;
            String[] temp_arr = TheTool.getFilePath_inFolder(folder_path, true);
            comboUKIXML.Items.Clear();
            for (int i = 0; i < temp_arr.Count(); i++)
            {
                if (temp_arr[i] != "[Motions]" && temp_arr[i] != "[Events]") { comboUKIXML.Items.Add(temp_arr[i]); }
            }
        }

        private void radio13_Checked(object sender, RoutedEventArgs e)
        {
            XML_load();
        }

        void XML_load()
        {
            XML_map_file_path = TheURL.url_saveFolder + TheURL.url_9_UKIMap + @"\" + comboUKIXML.SelectedValue + ".xml";
            if (File.Exists(XML_map_file_path))
            {
                detectMap = TheMapData.loadXML_map(XML_map_file_path);
                detectMap = TheMapData.getFullXML(detectMap, list_motions, list_events);
                detectMap_variables = detectMap.variables;
                listReplace_generate();
            }
            else { TheSys.showError("Failed to load File: " + XML_map_file_path); }
        }

        private void comboUKIXML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (radio13.IsChecked.Value) { XML_load(); }
        }

        //------ XML operation ----------------------------------------------

        //----- Replacing Key  ------
        public List<m_ReplaceKey> list_keyReplace = new List<m_ReplaceKey>();
        public class m_ReplaceKey
        {
            public String keyOrigin = "";
            public String keyNew = "";
            public Keys key = Keys.Space;
        }

        //get Key, checking if replacement is exist
        public Keys getKey_checkReplacment(m_Then o)
        {
            Keys key = o.key;
            foreach (m_ReplaceKey k in list_keyReplace)
            {
                if (String.Equals(k.keyOrigin, o.key0))
                {
                    if (k.keyNew != "") { key = k.key; }
                    break;
                }
            }
            return key;
        }

        public void listReplace_generate()
        {
            List<m_ReplaceKey> list = new List<m_ReplaceKey>();
            foreach (m_Group g in detectMap.groups)
            {
                foreach (m_Detection d in g.detections)
                {
                    foreach (m_Then o in d.outputs)
                    {
                        if (o.type == TheMapData.then_type_ReplaceKey)
                        {
                            list.Add(new m_ReplaceKey() { keyOrigin = o.v, keyNew = "" });
                        }
                    }
                }
            }
            var sortList = list.OrderByDescending(x => x.keyOrigin).ToArray();
            //-- Filter Dubplicate out ------------------------
            list_keyReplace = new List<m_ReplaceKey>();
            String txt = "";
            foreach (m_ReplaceKey k in sortList)
            {
                if (txt != k.keyOrigin) { list_keyReplace.Add(k); }
                txt = k.keyOrigin;
            }
            if (checkFTGAutoSide.IsChecked.Value) { listReplaceFTG_addSpecial(); }
        }

        public void listReplace_update(m_Then o)
        {
            foreach (m_ReplaceKey k in list_keyReplace)
            {
                if (k.keyOrigin == o.v) { k.keyNew = o.v2; k.key = TheKeySender.getKey(o.v2); break; }
            }
        }

        public void listReplace_update(string origin, string output)
        {
            foreach (m_ReplaceKey k in list_keyReplace)
            {
                if (k.keyOrigin == origin) { k.keyNew = output; k.key = TheKeySender.getKey(output); break; }
            }
        }

        public void listReplaceFTG_addSpecial()
        {
            try
            {
                string string_Right = "Right";
                string string_Left = "Left";
                bool exist_Right = false;
                bool exist_Left = false;
                foreach (m_ReplaceKey k in list_keyReplace)
                {
                    if (k.keyOrigin == string_Right) { exist_Right = true; }
                    else if (k.keyOrigin == string_Left) { exist_Left = true; }
                }
                if (!exist_Right) { list_keyReplace.Add(new m_ReplaceKey() { keyOrigin = string_Right }); }
                if (!exist_Left) { list_keyReplace.Add(new m_ReplaceKey() { keyOrigin = string_Left }); }

                TheSys.showError(list_keyReplace.Count());
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        //-------------------------

        //processDetectionMap
        void processMAP_XMLbasedSendInput()
        {
            try
            {
                foreach (m_Group g in detectMap.groups)
                {
                    if (map_PriorityQue != null) { break; }
                    if (g.enabled && map_group_checkInput(g.inputs))//pass input condition
                    {
                        Boolean g_foundMain = false;//found main detection in group
                        foreach (m_Detection d in g.detections)
                        {
                            if (map_PriorityQue != null) { break; }
                            if (!g_foundMain)
                            {
                                if (map_detection_checkInput(d.inputs, d))//pass input condition
                                {
                                    g_foundMain = true;
                                    map_processOutput_Main(d);//Found main
                                }
                                else
                                {
                                    map_processOutput_nonMain(d); //Non-Main
                                }
                            }
                            else { map_processOutput_nonMain(d); }//Non-Main for the rest
                            d.input_wait_checkTimeOut();
                        }
                    }
                }
                map_processOutput_PriorityQue();
                if (checkFTGAutoSide.IsChecked.Value) { FTG_autoSide(); }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString(), true); }
        }

        DateTime FTG_checkSide_time = DateTime.Now;
        void FTG_autoSide()
        {
            if (mainForm.thisTime > FTG_checkSide_time) {
                FTG_checkSide_time = mainForm.thisTime.AddSeconds(1);
                string path = TheURL.url_FTGautoSide;
                if (TheTool.checkPathExist(path))
                {
                    string code = TheTool.read_File_get1String(TheURL.url_FTGautoSide);

                    if (code == "1")
                    {
                        listReplace_update("Right", "Left");
                        listReplace_update("Left", "Right");
                    }
                    else
                    {
                        listReplace_update("Right","");
                        listReplace_update("Left", "");
                    }
                }
            }
        }

        Boolean map_group_checkInput(List<m_If> i_list)
        {
            Boolean pass = true;
            foreach (m_If i in i_list)
            {
                pass = map_processInput(i, null);
                if (!pass) { break; }// 1 condition failed, skip the res
            }
            return pass;
        }

        int temp_input_step_main = 0;
        Boolean map_detection_checkInput(List<m_If> i_list, m_Detection d)
        {
            Boolean pass = true;
            temp_input_step_main = 0;
            foreach (m_If i in i_list)
            {
                if (temp_input_step_main >= d.input_step)
                {
                    pass = map_processInput(i, d);
                    if (!pass) { break; }// 1 condition failed, skip the rest
                }
                temp_input_step_main++;
            }
            return pass;
        }

        //************************************************************************************
        //************************************************************************************


        Boolean map_processInput(m_If rule, m_Detection d)
        {
            Boolean pass = false;
            if (rule.type == TheMapData.if_type_BasicPose)
            {
                if (rule.v == "jump" && rule.value == uki_data.p_jump) { pass = true; }
                else if (rule.v == "lean" && rule.value == uki_data.p_lean) { pass = true; }
                else if (rule.v == "spin" && rule.value == uki_data.p_spin) { pass = true; }
                else if (rule.v == "handL_X" && rule.value == uki_data.p_handL_X) { pass = true; }
                else if (rule.v == "handL_Y" && rule.value == uki_data.p_handL_Y) { pass = true; }
                else if (rule.v == "handL_Z" && rule.value == uki_data.p_handL_Z) { pass = true; }
                else if (rule.v == "handR_X" && rule.value == uki_data.p_handR_X) { pass = true; }
                else if (rule.v == "handR_Y" && rule.value == uki_data.p_handR_Y) { pass = true; }
                else if (rule.v == "handR_Z" && rule.value == uki_data.p_handR_Z) { pass = true; }
                else if (rule.v == "handLR_close" && rule.value == uki_data.p_handLR_close) { pass = true; }
                else if (rule.v == "legL" && rule.value == uki_data.p_legL) { pass = true; }
                else if (rule.v == "legR" && rule.value == uki_data.p_legR) { pass = true; }
                //---------------------
                else if (rule.v == "step" && rule.value == uki_data.px_foot_step) { pass = true; }
                else if (rule.v == "footLR_close" && rule.value == uki_data.px_footLR_close) { pass = true; }
                //---------------------
                if (rule.opt == "!=") { pass = !pass; }
            }
            else if (rule.type == TheMapData.if_type_2Joint)
            {
                pass = checkInput_2JRelative(rule.axis, rule.j1, rule.j2, rule.opt, rule.value_d);
            }
            else if (rule.type == TheMapData.if_type_Change)
            {
                double v1 = 0;
                double v2 = 0;
                if (rule.axis == "X")
                {
                    v1 = posture_current.Joints[rule.j1].Position.X;
                    v2 = TheInitialPosture.skeleton.Joints[rule.j1].Position.X;
                }
                else if(rule.axis == "Y"){
                    v1 = posture_current.Joints[rule.j1].Position.Y;
                    v2 = TheInitialPosture.skeleton.Joints[rule.j1].Position.Y;
                }
                else 
                {
                    v1 = posture_current.Joints[rule.j1].Position.Z;
                    v2 = TheInitialPosture.skeleton.Joints[rule.j1].Position.Z;
                }
                double test_value = v1 - v2;
                pass = TheTool.checkOpt(test_value, rule.opt, rule.value_d);
            }
            else if (rule.type == TheMapData.if_type_Variable)
            {
                pass = map_processSubInput_Variable(rule);
            }
            else if (rule.type == TheMapData.if_type_Icon)
            {
                prepareIcon(rule.v, rule.brush, rule.value_d_1000);
                pass = true;
            }
            else if (rule.type == TheMapData.if_type_TimeAfterPose)
            {
                //point at next step
                d.input_step = temp_input_step_main + 1;
                d.input_time_waitUntil = DateTime.Now.AddMilliseconds(rule.value_d_1000);
                pass = true;
            }
            else if (rule.type == TheMapData.if_type_SphereAngle)
            {
                Boolean azimuth = false;
                if (rule.value == TheMapData.then_SphereAngle_Azimuth) { azimuth = true; }
                double testValue = TheTool.calSpherical(getJoint(rule.j1), getJoint(rule.j2), azimuth);
                pass = TheTool.checkOpt(testValue, rule.opt, rule.value_d);
            }
            else if (rule.type == TheMapData.if_type_FlexionAngle)
            {
                double testValue = 0;
                if (rule.v == "ElbowL") { testValue = getAng(JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft); }
                else if (rule.v == "ElbowR") { testValue = getAng(JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight); }
                else if (rule.v == "KneeL") { testValue = getAng(JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft); }
                else if (rule.v == "KneeR") { testValue = getAng(JointType.HipRight, JointType.KneeRight, JointType.AnkleRight); }
                pass = TheTool.checkOpt(testValue, rule.opt, rule.value_d);
            }
            return pass;
        }

        public void prepareIcon(String v, Brush brush, int value_d_1000)
        {
            if (value_d_1000 > 0)
            {
                if (v == " ") { }
                else { icon_temp_content = v; }
                icon_temp_color = brush;
                icon_temp_timeout = DateTime.Now.AddMilliseconds(value_d_1000);
            }
            else
            {
                if (v == " ") { }
                else { icon_static_content = v; }
                icon_static_color = brush;
            }

        }

        //===========================================================

        void map_processOutput_Main(m_Detection d)
        {
            if (d.output_is_activate == false || d.loop == true)
            {
                d.output_checkRunThread(this);//open loop
            }
            d.output_is_activate = true;
            if (checkMAP_renewal.IsChecked.Value) { do_DetectionRenewal(d); }
        }

        //Prevemt situation that Component Pose of motion Y detected in a part of completed motion X
        void do_DetectionRenewal(m_Detection d0)
        {
            if (d0.posture_count > 1)
            {
                foreach (m_Group g in detectMap.groups)
                {
                    if (g.detections.Contains(d0))
                    {
                        foreach (m_Detection d in g.detections)
                        {
                            if (d != d0) { d.input_step = 0; }
                        }
                    }
                }
            }
        }


        //Non-Detected Motion >> release being-pressed key (Motion no longer detect)
        void map_processOutput_nonMain(m_Detection d)
        {
            if (d.output_is_activate == true)
            {
                foreach (m_Then o in d.outputs)
                {
                    if (o.type == TheMapData.then_type_Key)
                    {
                        if (o.press != TheMapData.then_key_holdEoM || o.press != TheMapData.then_key_press)
                        {
                            // release "hold until end-of-action" or "press"
                            if (o.press == 1) { InputManager.Keyboard.KeyUp(getKey_checkReplacment(o)); }
                        }
                    }
                }
            }
            d.output_is_activate = false;
        }


        public Boolean map_processSubInput_Variable(m_If i)
        {
            Boolean pass = false;
            foreach (m_Variable v in detectMap.variables)
            {
                if (v.name == i.v)
                {
                    pass = TheTool.checkOpt(i.value, i.opt, v.value);
                    break;
                }
            }
            return pass;
        }

        public void map_processSubOutput_Variable(m_Then o)
        {
            foreach (m_Variable v in detectMap.variables)
            {
                if (v.name == o.v)
                {
                    if (o.opt == "+") { v.value += o.value; }
                    else if (o.opt == "-") { v.value -= o.value; }
                    else { v.value = o.value; }
                }
            }
        }
        //------------------------------------------------------------------------------------------------
        Boolean map_PriorityQue_start = false;
        public m_Detection map_PriorityQue = null;

        public void map_processOutput_PriorityQue()
        {
            if (map_PriorityQue != null)
            {
                //------ Stop other Thread, release all Key, then Run ----------
                if (map_PriorityQue_start == false)
                {
                    map_PriorityQue_start = true;
                    foreach (m_Group g in detectMap.groups)
                    {
                        foreach (m_Detection d in g.detections)
                        {
                            d.killThread();
                        }
                    }
                    map_PriorityQue.output_runThread();
                }
                //----- Run Finish ----------
                else if (map_PriorityQue.output_thread_running == false)
                {
                    map_PriorityQue = null;
                    map_PriorityQue_start = false;
                }
            }
        }

        //****************************************************************************************
        //********* GUI ***********************************************************************

        private void radio11_Checked(object sender, RoutedEventArgs e)
        {
            uki_fightingICE.fightingICE_zeroCmd();
        }

        private void butEditor_Click(object sender, RoutedEventArgs e)
        {
            new Editor(TheURL.url_saveFolder + TheURL.url_9_UKIMap + @"\" + comboUKIXML.SelectedValue + ".xml").Show();
        }

        private void butReloadMotion_Click(object sender, RoutedEventArgs e)
        {
            loadXMLtoCombo();
            refreashMotionList();
            refreashEventList();
            XML_load();
        }

        //****************************************************************************************
        //********* Posture Extraction ***********************************************************

        Boolean pe_readying = false;//PE is turn on
        Boolean pe_processing = false;//PE is processing
        Boolean pe_stoping = false;//PE is processing

        int pe_preStart_cooldown_left = 0;//cooldown to start
        int pe_preStart_cooldown_const = 7;
        int pe_preStart_toTime = 0;//time to start

        int pe_preStop_cooldown_left = 0;//cooldown to stop
        int pe_preStop_cooldown_const = 7;
        int pe_preStop_toTime = 0;//time to stop

        void pe_reset()
        {
            pe_readying = false; pe_processing = false; pe_stoping = false;
        }

        private void butPR_start_Click(object sender, RoutedEventArgs e)
        {
            if (!pe_readying) { pe_01_prepare(); }
            else { checkRun.IsChecked = false; }
        }

        //prepare to start
        void pe_01_prepare()
        {
            clearData();
            checkFTGData.IsChecked = true;//to collect Data
            pe_preStart_toTime = mainForm.sec_since_start + pe_preStart_cooldown_const;//time to start : might be adjusted if no base yet
            pe_readying = true;
            butPR_start.Content = "Stop"; butPR_start.Foreground = Brushes.Red;
            butPR_cancle.IsEnabled = true;
            pe_cutAt = 0;
        }

        //count down to start
        void pe_02_prestart()
        {
            pe_preStart_cooldown_left = pe_preStart_toTime - mainForm.sec_since_start;
            if (pe_preStart_cooldown_left > 0) { pe_icon(); }
            else { checkRun.IsChecked = true; }
        }

        int pe_cutAt = 0;//last data before stopping period
        //count down at still, prepare to cut data
        void pe_03_prestop()
        {
            if (uki_data.tool_posExtract.ms_all < 1)
            {
                if (pe_stoping == false)
                { //begin to stop
                    pe_stoping = true;
                    pe_preStop_toTime = mainForm.sec_since_start + pe_preStop_cooldown_const + 2;
                }
                else
                {
                    pe_preStop_cooldown_left = pe_preStop_toTime - mainForm.sec_since_start;
                    if (pe_preStop_cooldown_left < 0)
                    {
                        //completed stop
                        switch_pos_greeting = false;
                        checkRun.IsChecked = false;
                        uki_data.cutData(pe_cutAt);
                    }
                    if (pe_preStop_cooldown_left > 5) { pe_cutAt = uki_data.data_raw.Count(); }
                }
                pe_icon();
            }
            else if (pe_stoping) { pe_stoping = false; pe_icon(); }//cancle
        }

        //fully close process : should be called by checkRun
        void pe_04_off()
        {
            if (pe_processing) { uki_data.exportFile(DateTime.Now.ToString("MMdd_HHmmss"), "UKI_" , true, checkGlobalMM.IsChecked.Value, true); }
            butPR_start.Content = "Start"; butPR_start.Foreground = Brushes.Green;
            pe_readying = false; pe_processing = false; pe_stoping = false;
            pe_icon();
            butPR_cancle.IsEnabled = false;
        }

        private void butPR_cancle_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Stop Motion Recognition"
                                           , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                pe_reset();
                checkRun.IsChecked = false;
            }
        }

        void pe_icon()
        {
            if (pe_readying == false) { txtOn.Visibility = Visibility.Visible; txtPE.Visibility = Visibility.Hidden; }
            else if (pe_stoping && pe_preStop_cooldown_left <= pe_preStop_cooldown_const - 2) //skip first 2 sec
            {
                //Stopping
                if (pe_preStop_cooldown_left > 0) { txtPE.Content = pe_preStop_cooldown_left; }
                else { txtPE.Content = ""; }
                txtPE.Foreground = Brushes.Red;
            }
            else
            {
                //Readying
                txtPE.Visibility = Visibility.Visible; txtOn.Visibility = Visibility.Hidden;
                if (pe_processing == false && pe_preStart_cooldown_left > 0) { txtPE.Content = pe_preStart_cooldown_left; }
                else { txtPE.Content = "R"; }
                if (is_waitForBasePosture) { txtPE.Foreground = Brushes.Gray; }
                else { txtPE.Foreground = Brushes.Lime; }
            }
        }

        //********* Export ***********************************************************

        private void butFTGexport_Click(object sender, RoutedEventArgs e)
        {
            export("");
        }

        void export(string prefix)
        {
            string fileName = DateTime.Now.ToString("MMdd_HHmmss");
            string folderPrefix = prefix + "UKI_";
            uki_data.exportFile(fileName, folderPrefix, false, checkGlobalMM.IsChecked.Value, true);
            exportMarked(fileName, folderPrefix);
        }

        public void exportMarked(String fileName, string folderPrefix)
        {
            try
            {
                if (data_mark.Count > 0)
                {
                    string folderPath = TheURL.url_saveFolder + folderPrefix + fileName;
                    List<string> list_data = new List<string>();
                    string s = "";
                    foreach(int[] d in data_mark){
                        if (d[1] == 1) { s = d[0] + "-"; }
                        else { 
                            list_data.Add(s + d[0]); 
                            s = ""; 
                        }
                    }
                    TheTool.exportCSV_orTXT(folderPath + @"\marker.txt", list_data, false);
                }
            }
            catch (Exception ex) { TheSys.showError("Export Marker: " + ex); }
        }

        private void butCollectMode_Click(object sender, RoutedEventArgs e)
        {
            mainForm.do_fullScreen();
            checkFTGData.IsChecked = true;
            checkStart_Pose.IsChecked = false;
            checkStop_Pose.IsChecked = false;
            check_SideSwitch.IsChecked = false;
            checkHideIcon.IsChecked = true;
            checkAutoPrefix.IsChecked = true;
            checkStopByRange.IsChecked = false;
        }

        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

        private void butComplete_Click(object sender, RoutedEventArgs e)
        {
            completeStop();
        }

        void completeStop()
        {
            checkRun.IsChecked = false;
            string prefix = "";
            if (checkAutoPrefix.IsChecked.Value)
            {
                int i = TheTool.getInt(textAutoPrefix);
                prefix += i + " ";
                if (i < 10) { prefix = "0" + prefix; }
                i++;
                textAutoPrefix.Text = i.ToString();
            }
            export(prefix);
            clearData();
        }

        private void checkFTGAutoSide_Checked(object sender, RoutedEventArgs e)
        {
            listReplaceFTG_addSpecial();
        }

        private void butFTGset_Click(object sender, RoutedEventArgs e)
        {
            checkFTGAutoSide.IsChecked = true;
            checkStart_Range.IsChecked = false;
            checkStopByRange.IsChecked = false;
            checkStop_Pose.IsChecked = false;
        }

    }

}
