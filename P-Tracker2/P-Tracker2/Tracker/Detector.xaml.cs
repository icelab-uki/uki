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
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Diagnostics;

namespace P_Tracker2
{
    public partial class Detector : Window
    {
        UserTracker mainForm;

        public Detector(UserTracker mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            setupComboClassifier();comboClassifier.SelectedIndex = 0;
            setupComboNormaliz();comboNormaliz.SelectedIndex = 0;
            reStart();
            //alwayOnTop();
            //----------------
            microStart();
            //
            psd_applyThreshold();
            psd_listSetup();
            pd_restart();
            cal_TotalRisk_threshold();
            progressBar_applyMax();
            //
            //classifyChange();//Initial Classifier
        }

        void microStart()
        {
            TheTool_micro.reset(false);
            TheTool_micro.sendCmd(MicroCmd.cmd_ready);
        }


        //================================================================
        //========= Main Operation ===============================

        Skeleton tracked_Person = null;
        public void detect(Skeleton trackedPerson)
        {
            checkStart();
            this.tracked_Person = trackedPerson;
            //
            try { sst_operation(); }
            catch { }
            try { cal_Head_Shoulder(); }
            catch { }

            if (getBase == true)
            {
                if (checkPSD.IsChecked == true)
                {
                    try { pd_prolongedSitDetect(); }
                    catch { }
                }
                //------------------
                calStage_realTime();
            }
            else
            {
                txtState.Content = "Base Posture Recognizing ...";
                txtPSDcheck.Content = "";
            }
        }

        //----------PP : Consensus Algorithm-------------
        //Do not Judge immediately >> Make Assumption and wait for Consensus
        int pitch_agree = 0;
        int twist_agree = 0;
        //======================
        //--- Check Current Position -----
        string state_String = "";
        int pitch_flag = 0;
        int twist_flag = 0;//1 = R , -1 = L
        int stand_flag = 0;
        int break_flag = 0;
        //======================
        public int range_threshold = 100;// >x: "Break"
        public int pitch_threshold = -10;// <x: "Pitch"
        public int turn_threshold = 20;//   >x: Turn-L , <-x: Turn

        Boolean is_waitForComeBack = false;

        //show Text & Alert , check Threshold but not Calculate
        void calStage_realTime()
        {
            string state = "";
            //-----------------
            if (range_cur_diff < range_threshold && is_toBreak == false)
            {
                //First Frame
                if (break_flag == 1) {
                    playSpeech(TheURL.sp_welcomeBack);
                    HRL_breakEffect_whenComeBack();//calculate effect of Break when comeback
                    break_flag = 0; HRL_set1(false);
                    ardui_switch_n_alert(ref ardui_isBreak, false, MicroCmd.cmd_comeBack);
                    if (butBackToWork.IsEnabled) { butBreak.IsEnabled = true; butBackToWork.IsEnabled = false; }                   
                } 
                else { break_flag = 0; }
                //---------------------
                if (is_Sit)
                {
                    if (stand_flag == 1) { stand_flag = 0; HRL_set1(false); } //First Frame >> Draw
                    else { stand_flag = 0; }
                    state += "Sit"; 
                    ardui_isStand = false;
                    is_waitForComeBack = false;
                    //------------- PITCH -----------
                    if (pitch_angle <= pitch_threshold)
                    {
                        pitch_agree++;
                        if (pitch_agree > total_agree_consensus) { 
                            state += ", Pitch"; pitch_flag = 1;
                            ardui_switch_n_alert(ref ardui_isPitch, true, MicroCmd.cmd_pitch);
                        }
                    }
                    else {
                        pitch_agree = 0; pitch_flag = 0;
                        ardui_switch_n_alert(ref ardui_isPitch, false, MicroCmd.cmd_clearLCD);
                    }
                    //------------- TURN --------------
                    if (facing_deg_cur <= -turn_threshold)
                    {
                        twist_agree++;
                        if (twist_agree > total_agree_consensus) { 
                            state += ", Twist-L"; twist_flag = -1;
                            ardui_switch_n_alert(ref ardui_isTwist, true, MicroCmd.cmd_turn_l);
                        }
                    }
                    else if (facing_deg_cur >= turn_threshold)
                    {
                        twist_agree--;
                        if (twist_agree < -total_agree_consensus) {
                            state += ", Twist-R"; twist_flag = 1;
                            ardui_switch_n_alert(ref ardui_isTwist, true, MicroCmd.cmd_turn_r);
                        }
                    }
                    else {
                        ardui_switch_n_alert(ref ardui_isTwist, false, MicroCmd.cmd_clearLCD);
                        twist_agree = 0;
                        twist_flag = 0;
                        //(Bend)
                    }
                }
                else
                {
                    if (is_waitForComeBack == false)
                    {
                        if (stand_flag == 0)
                        {  //First Frame
                            stand_flag = 1; HRL_set1(false);
                            playSpeech(TheURL.sp_niceBreak);
                        }
                        else { stand_flag = 1; }
                        state += "Stand";
                        ardui_switch_n_alert(ref ardui_isStand, true, MicroCmd.cmd_offAll_butLCD + MicroCmd.cmd_stand);
                    }
                }
            }
            else
            {
                if (break_flag == 0) {
                    //First Frame
                    break_flag = 1; HRL_set1(false); 
                    time_walkout = DateTime.Now;
                    is_waitForComeBack = true; ardui_switch_n_alert(ref ardui_isBreak, true, MicroCmd.cmd_sleep);                    
                    if (butBreak.IsEnabled) { butBreak.IsEnabled = false; butBackToWork.IsEnabled = true; }     
                } 
                else { break_flag = 1; }
                state += "Break";
            }
            state_String = state;
            if (is_Freeze == false){txtState.Content = state;}
        }


        //***********************************************************************************
        //=========== POS Monitor ==========

        // For Alert
        public Boolean ardui_isPitch = false;
        public Boolean ardui_isTwist = false;
        public Boolean ardui_isBend = false;
        public Boolean ardui_isBreak = false;
        public Boolean ardui_isStand = false;

        //POS-Monitor can alert only once subject at a time
        void ardui_switch_n_alert(ref Boolean target,Boolean turn,string microCmd)
        {
            if (checkPOSmonitor.IsChecked == true)
            {
                if (target != turn)
                {
                    TheTool_micro.sendCmd(microCmd);
                    if (turn == false) { ardui_toAlertSomeThingElse(); }
                }
                target = turn;
            }
        }

        void ardui_toAlertSomeThingElse()
        {
            ardui_isPitch = false;
            ardui_isTwist = false;
            ardui_isBreak = false;
            ardui_isStand = false;
        }

        void ardui_Txt_BasePose(){
            if (getBase == false) { 
                TheTool_micro.sendCmd(MicroCmd.cmd_baseRecog);
                playSpeech(TheURL.sp_baseReg);
            }
            else { 
                TheTool_micro.sendCmd(MicroCmd.cmd_clearLCD);
            }
        }


        //---------- Blink ------------------
        Boolean isBlink = false;
        int ardui_prolong_lv_last = 0;
        int ardui_pitch_lv_last = 0;
        int ardui_twist_lv_last = 0;
        void ardui_toBlink()
        {
            if (hrl_prolong_lv > ardui_prolong_lv_last
                || hrl_pitch_lv > ardui_pitch_lv_last
                || hrl_twist_lv > ardui_twist_lv_last)
            {
                if (isBlink == false) { TheTool_micro.sendCmd(MicroCmd.cmd_blinkLED_on); }
                isBlink = true;
            }
            else {
                if (isBlink == true) { TheTool_micro.sendCmd(MicroCmd.cmd_blinkLED_off); }
                isBlink = false;
            }
            ardui_prolong_lv_last = hrl_prolong_lv;
            ardui_pitch_lv_last = hrl_pitch_lv;
            ardui_twist_lv_last = hrl_twist_lv;
        }

        //------------ LED ------------------
        void ardui_toLED()
        {
            double a = hrl_prolong_score;
            double b = hrl_prolong_score_max;
            if (b > 0)
            {
                a /= b;
                a *= 8;
                TheTool_micro.sendCmd(MicroCmd.cmd_LED_off);
                if (a >= 1)
                {
                    TheTool_micro.sendCmd(MicroCmd.cmd_LED_time(a));
                }
            }
        }

        //***********************************************************************************
        int startTime = 0;//Time to Get Base Value
        int startTime_cooldown = 10;
        public Boolean getBase = false;
        public int range_base = 0;
        int range_cur_diff = 0;
        public double[] hip_virtual_base = new double[]{0,0,0};

        void checkStart()
        {
            if (getBase == false)
            {
                if (initialize == true)
                {
                    int thisTime = mainForm.sec_since_start;
                    int timeDiff = startTime - thisTime;
                    if (timeDiff <= 0)
                    {
                        setBase();pd_restart();txtCool.Content = "";
                    }
                    else
                    {
                        txtCool.Content = timeDiff;
                    }
                }
                else {
                    checkInitialize();
                }
            }
        }


        Boolean initialize = false;//Initialize only if Human is detected
        void checkInitialize()
        {
            if (mainForm.person_detected == true) {
                playSpeech(TheURL.sp_searchSkel);
                initialize = true;
                this.startTime = mainForm.sec_since_start + startTime_cooldown;
                ardui_Txt_BasePose();
            }
        }

        void setBase()
        {
            getBase = true;
            facing_deg_base = facing_deg_cur;
            range_base = range_cur_diff;
            sst_Head_Y_base = sst_Head_Y_cur;
            sst_Should_Y_base = sst_Should_Y_cur;
            pgBar_HeadBase.Value = sst_Head_Y_base;
            pgBar_ShouldBase.Value = sst_Should_Y_base;
            bodyBend_angleBase = bodyBend_angle;
            shouldRL_balanceBase = shouldRL_balance;
            roll_angleBase = roll_angle;
            pitch_angleBase = pitch_angle;
            ardui_Txt_BasePose();
        }

        public void calVirtualHip(){
            double avg_Y = tracked_Person.Joints[JointType.ElbowRight].Position.Y;
            avg_Y += tracked_Person.Joints[JointType.ElbowLeft].Position.Y;
            avg_Y /= 2;
            hip_virtual_base[0] = tracked_Person.Joints[joint_ShoulderCenter].Position.X;
            hip_virtual_base[1] = avg_Y;
            hip_virtual_base[2] = tracked_Person.Joints[joint_ShoulderCenter].Position.Z;
        }

        void reStart() {
            getBase = false;
            initialize = false;
            sst_Head_Y_base = 0;
            sst_Should_Y_base = 0;
            range_base = 0;
            facing_deg_base = 0;
            bodyBend_angleBase = 0;
            shouldRL_balanceBase = 0;
            roll_angleBase = 0;
            pitch_angleBase = 0;
            hip_virtual_base = new double[] { 0, 0, 0 };
            //--------------------------------------------------
            startTime_cooldown = TheTool.getInt(txtWait); 
            txtState.Content = "";
            //------
            total_agree_consensus = TheTool.getInt(txtFrameAgree);
            sst_agree = 0;
            is_toBreak = false;
        }

        //----------------------------------------------------------------------

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                mainForm.form_Detector = null;
                TheTool_micro.sendCmd(MicroCmd.cmd_sleep);
                if (graphVisualizer != null) { graphVisualizer.detector = null; }
            }
            catch { }
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

        //=======================================================================
        //============ Sit Stand Tracking  : SST ==================
        //PP DAC Theory : Detection >> Assumption >> Confirmation
        /* Detection can be caused from some noise, 
         * we solve by "Perfect Consensus (all frames agree)"
        */
        public int total_agree_consensus = 15;//Number of frame agree

        JointType joint_Head = JointType.Head;
        JointType joint_ShoulderCenter = JointType.ShoulderCenter;
        JointType joint_ShoulderLeft = JointType.ShoulderLeft;
        JointType joint_ShoulderRight = JointType.ShoulderRight;
        //------------------
        Boolean is_Sit = true;//realtime posture
        Boolean base_is_Sit = true;//base posture
        int sst_agree = 0;

        double sst_Head_Y_base = 0;
        double sst_Should_Y_base = 0;
        double sst_Head_Y_cur = 0;
        double sst_Should_Y_cur = 0;

        public void sst_operation()
        {
            sst_Head_Y_cur = tracked_Person.Joints[joint_Head].Position.Y;
            sst_Should_Y_cur = tracked_Person.Joints[joint_ShoulderCenter].Position.Y;
            //------ Show Data
            if (is_Freeze == false)
            {
                pgBar_HeadCur.Value = sst_Head_Y_cur;
                pgBar_ShouldCur.Value = sst_Should_Y_cur;
            }
            //------ Detect SitStand
            if (getBase == true) { sst_detectSitStand(); }//Available After Base value Exist
        }

        void sst_detectSitStand()
        {
            //Sit >> Stand
            if (is_Sit == true && isStandUp())
            {
                sst_agree++;
                if (sst_agree > total_agree_consensus){
                    is_Sit = false;
                }
            }
            //Stand >> Sit
            else if (is_Sit == false && !isStandUp())
            {
                //Don't Judge immediately, wait for Consensus
                sst_agree++;
                if (sst_agree > total_agree_consensus){ 
                    is_Sit = true;
                }
            }
            //reject any Assumption >> Some Data not agree in Consensus
            else{sst_agree = 0;}
        }

        Boolean isStandUp()
        {
            Boolean standUp = false;
            if (base_is_Sit == true && sst_Should_Y_cur > sst_Head_Y_base)
            {
                standUp = true;
            }
            else if (base_is_Sit == false && sst_Head_Y_cur < sst_Should_Y_base)
            {
                standUp = true;
            }
            return standUp;
        }

        private void butReset_Click(object sender, RoutedEventArgs e)
        {
            if (checkStand.IsChecked.Value) { is_Sit = false; base_is_Sit = false; }
            else { is_Sit = true; base_is_Sit = true; }
            reStart();
        }

        //=======================================================================
        //============ Facing Angle ====================================
        public int facing_deg_base = 0;
        int facing_deg_cur = 0;

        string getText_withSign_fromInt(int a)
        {
            if (a > 0) { return "+" + a; }
            else { return "" + a; }
        }

        //============ Pitch : Neck Angle + Rool + Range + etc ====================================
        int pitch_angle = 0;
        int pitch_angleBase = 0;
        int roll_angle = 0;
        int roll_angleBase = 0;

        public void cal_Head_Shoulder()
        {
            SkeletonPoint head =  tracked_Person.Joints[joint_Head].Position;
            SkeletonPoint shouldC =  tracked_Person.Joints[joint_ShoulderCenter].Position;
            SkeletonPoint shouldL =  tracked_Person.Joints[joint_ShoulderLeft].Position;
            SkeletonPoint shouldR =  tracked_Person.Joints[joint_ShoulderRight].Position;
            //SkeletonPoint bodyCenter = tracked_Person.Position;
            if (getBase != true) { calVirtualHip(); }
            //------------------------------------
            //Do not Rearrange
            cal_ShoulderRL_func(shouldL, shouldR);
            cal_Head_Shoulder_func(head,shouldC);
            cal_Bend(shouldC);
            //------------------------------------------------------------------------------------
            if (is_Freeze == false)
            {
                try { 
                    pgBalance.Value = shouldRL_balance;
                    txtRange.Content = "range: " + range_base + "  " + getText_withSign_fromInt(range_cur_diff);
                    txtFace.Content = "facing: " + facing_deg_base + "°  " + getText_withSign_fromInt(facing_deg_cur) + "°";
                    txtPitch.Content = pitch_angle + "°";
                    txtPitchBase.Content = pitch_angleBase + "°";
                    txtRoll.Content = roll_angle + "°";
                    txtRollBase.Content = roll_angleBase + "°";
                    txtBal.Content = shouldRL_balanceBase + "°  " + getText_withSign_fromInt(shouldRL_balance) + "°";
                    txtBend.Content = bodyBend_angleBase + "°  " + getText_withSign_fromInt(bodyBend_angle) + "°";
                }
                catch { }
            }
        }

        int kinectAngle = 0;
        public void cal_Head_Shoulder_func(SkeletonPoint head, SkeletonPoint should)
        {
            double PitchAngle = 0;
            if (checkAlgo2.IsChecked == true)
            {
                // acos(Sqrt(deltaX^2 + deltaZ^2)/r)
                PitchAngle = ThePostureCal.calAngle_3D(head, should, 0, 2);
            }
            else
            {
                // acos(deltaZ/r)
                PitchAngle = ThePostureCal.calAngle_3D(head, should, 2);
            }
            PitchAngle = 90 - PitchAngle;
            //
            double RollAngle = ThePostureCal.calAngle_3D_fromTemp(0);
            RollAngle = 90 - RollAngle;
            //---------------------------------------------------------
            kinectAngle = mainForm.kinectAngle;
            pitch_angle = (int)(PitchAngle - pitch_angleBase);//Adjust by Base
            roll_angle = (int)(RollAngle - roll_angleBase - shouldRL_balance);//Adjust by Base & Balance
            range_cur_diff = (int)(should.Z * 100);//centimeter from Shoulder
            range_cur_diff = range_cur_diff - range_base;//Get Diff
        }



        int shouldRL_balance = 0;
        int shouldRL_balanceBase = 0;
        public void cal_ShoulderRL_func(SkeletonPoint shouldL, SkeletonPoint shouldR)
        {
            //Target
            double facing = 0;
            if (checkAlgo2.IsChecked == true)
            {
                // acos(Sqrt(deltaX^2 + deltaZ^2)/r)
                facing = ThePostureCal.calAngle_3D(shouldL, shouldR, 0, 2);
            }
            else
            {
                // acos(deltaZ/r)
                facing = ThePostureCal.calAngle_3D(shouldL, shouldR, 2);
            }
            facing_deg_cur = (int)facing;
            facing_deg_cur = facing_deg_cur - 90;
            facing_deg_cur = facing_deg_cur - facing_deg_base;//Adjust to get Absolute
            //
            double shouldRL_bal = ThePostureCal.calAngle_3D_fromTemp(1);
            shouldRL_bal = 90 - shouldRL_bal;
            //---------------------------------------------------------
            shouldRL_balance = (int)(shouldRL_bal - shouldRL_balanceBase);
        }

        int bodyBend_angle = 0;
        int bodyBend_angleBase = 0;
        public void cal_Bend(SkeletonPoint shouldC)
        {
            double[] j1 = { shouldC.X, shouldC.Y, shouldC.Z };
            //-------------------
            //Target
            //Target
            double bend = 0;
            if (checkAlgo2.IsChecked == true)
            {
                // acos(Sqrt(deltaX^2 + deltaZ^2)/r)
                bend = ThePostureCal.calAngle_3D_byDouble(j1, hip_virtual_base, 0, 2);
            }
            else
            {
                // acos(deltaX/r)
                bend = ThePostureCal.calAngle_3D(j1, hip_virtual_base, 0);
            }
            bend = 90 - bend;
            //---------------------------------------------------------
            this.bodyBend_angle = (int)(bend - bodyBend_angleBase);//
        }

        //=======================================================================

        public PersonD getPersonDetect()
        {
            PersonD personD = new PersonD();
            personD.state = state_String;
            if (is_Sit == false) { personD.sit_flag = 0; }
            personD.pitch_flag = this.pitch_flag;
            personD.turn_flag = this.twist_flag;
            return personD;
        }

        public Boolean confirmOnClose = true;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (confirmOnClose == true)
                {
                    if (mainForm.is_Tracking == true)
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Some data will be lost, you should stop tracking and save files first."
                                                        , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
                        if (result == System.Windows.MessageBoxResult.OK){e.Cancel = false;}
                        else{ e.Cancel = true;}
                    }
                }
            }
            catch { }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            alwayOnTop();
        }

        void alwayOnTop() { this.Topmost = checkOnTop.IsChecked.Value; }


        //======================================================================================================
        //=========== Posture Detection (PD) =========================================
        //1.[GD] General Detection
        //2.[PSD] Prolonged Sitting Detection

        //Main Function
        void pd_prolongedSitDetect()
        {
            pd_timing();
            psd_list_add();
            pd_checkReady();
        }
        
        //---- List 5 for General Detection
        //---- List 30 for Prolonged Sitting Detection (PSD)
        public int list_5_time_every = 10;
        public int list_5_timeCooldownAt;//time to reset
        public List<double>[] list_30;// 0 Head , 1 ElbowLeft, 2 ElbowRight
        public int list_30_time_every = 30;
        public int list_30_timeCooldownAt;//time to reset

        public void psd_listSetup()
        {
            list_30 = new List<double>[3];
            list_30[0] = new List<double>(); 
            list_30[1] = new List<double>();
            list_30[2] = new List<double>();
        }

        public int thisTime = 0;
        public int timeDiff_list5 = 0;
        public int timeDiff_list30 = 0;
        void pd_timing()
        {
            thisTime = mainForm.thisTime0;
            timeDiff_list5 = list_5_timeCooldownAt - thisTime;
            timeDiff_list30 = list_30_timeCooldownAt - thisTime;
            txtPSDcheck.Content = timeDiff_list5 + " | " + timeDiff_list30;
        }

        //-----------------------------------------------------
        int count_psd = 0;//count number of record
        // 0 Head , 1 ElbowLeft, 2 ElbowRight
        public Joint last_joint0; public Joint current_joint0; 
        public double delta_joint0 = 0;
        public Joint last_joint1; public Joint current_joint1;
        public double delta_joint1 = 0;
        public Joint last_joint2; public Joint current_joint2; 
        public double delta_joint2 = 0;
        public void psd_list_add()
        {
            try
            {
                current_joint0 = tracked_Person.Joints[JointType.Head];
                current_joint1 = tracked_Person.Joints[JointType.ElbowLeft];
                current_joint2 = tracked_Person.Joints[JointType.ElbowRight];
                if(count_psd > 0){
                    delta_joint0 = TheTool.calEuclidian_2Joint(last_joint0, current_joint0);
                    delta_joint1 = TheTool.calEuclidian_2Joint(last_joint1, current_joint1);
                    delta_joint2 = TheTool.calEuclidian_2Joint(last_joint2, current_joint2);
                    list_30[0].Add(delta_joint0); 
                    list_30[1].Add(delta_joint1);
                    list_30[2].Add(delta_joint2);
                }
                last_joint0 = current_joint0;
                last_joint1 = current_joint1;
                last_joint2 = current_joint2;
                count_psd++;
            }
            catch (Exception e) { TheSys.showError("Err: [psd_list_add()] " + e.ToString(), true); }
        }

        public void pd_restart()
        {
            flag_classMove0 = 0;
            count_psd = 0;
            int thisTime = mainForm.thisTime0;
            list_5_timeCooldownAt = thisTime + list_5_time_every;
            list_30_timeCooldownAt = thisTime + list_30_time_every;
            list_30[0].Clear();list_30[1].Clear();list_30[2].Clear();
        }

        public void pd_checkReady()
        {
            try
            {
                //List 30   >> check PSD, not add data
                psd_calAVG();
                if (timeDiff_list30 <= 0)
                {
                    prolongScoring = true;
                    psd_checkMoveFlag();
                    list_30_timeCooldownAt = thisTime + list_30_time_every;
                    list_30[0].Clear(); list_30[1].Clear(); list_30[2].Clear();
                    ardui_toLED();
                }
                //List 5    >> add data, not check PSD
                if (timeDiff_list5 <= 0)
                {
                    psd_addData();
                    list_5_timeCooldownAt = thisTime + list_5_time_every;
                    ardui_toBlink();
                }
            }
            catch (Exception e) { TheSys.showError("Err: [psd_checkReady()] " + e.ToString(), true); }
        }

        //-------------------------------------------------------------------

        void setupComboClassifier()
        {
            comboClassifier.Items.Add("Neural Network");
            comboClassifier.Items.Add("Naive Bayes");
            comboClassifier.Items.Add("Tree J48");
            comboClassifier.Items.Add("K-Near 5");
        }

        void setupComboNormaliz()
        {
            comboNormaliz.Items.Add("Non");
            comboNormaliz.Items.Add("Gross MinMax");
            comboNormaliz.Items.Add("Per Capita MinMax");
        }

        private void butClassifyChange_Click(object sender, RoutedEventArgs e)
        {
            classifyChange();
        }

        void classifyChange()
        {
            try
            {
                int normal_method = comboNormaliz.SelectedIndex;
                TheProlongSitDetector.setupModel(comboClassifier.SelectedIndex, comboNormaliz.SelectedIndex);
                if (checkAutoThreshold.IsChecked == true) { psd_prepareThreshold(normal_method); }
                if (checkAutoMM.IsChecked == true) { psd_prepareNormaliz(normal_method); }
            }
            catch { 
                //TheSys.showError("classifyChange: " + ex.Message); 
            }
        }

        void psd_prepareNormaliz(int normal_method)
        {
            if (normal_method > 0)
            {
                checkNormaliz.IsChecked = true;
                txtNormalMin0.Text = "0";
                txtNormalMax0.Text = "1.506979";
                txtNormalMin1.Text = "0";
                txtNormalMax1.Text = "1.296434";
                txtNormalMin2.Text = "0";
                txtNormalMax2.Text = "1.426036"; 
                applyMinMax();
            }
            else
            {
                checkNormaliz.IsChecked = false;
            }
        }

        void psd_prepareThreshold(int normal_method)
        {
            if (normal_method == 1)
            {
                txtThreshold0.Text = "0.00915";
                txtThreshold1.Text = "0.010792";
                txtThreshold2.Text = "0.001958";
                psd_applyThreshold();
            }
            else if (normal_method == 2)
            {
                txtThreshold0.Text = "0.009394";
                txtThreshold1.Text = "0.001702";
                txtThreshold2.Text = "0.007196";
                psd_applyThreshold();
            }
            else
            {
                //txtThreshold0.Text = ".005610";
                //txtThreshold1.Text = ".008319";
                //txtThreshold2.Text = ".001373";
                txtThreshold0.Text = "0.015";
                txtThreshold1.Text = "0.025";
                txtThreshold2.Text = "0.020";
                psd_applyThreshold();
            }
        }

        //====================================================================================
        double psd_avg_0 = 0;//head
        double psd_avg_1 = 0;//left
        double psd_avg_2 = 0;//right
        public void psd_calAVG()
        {
            if (list_30[0].Count > 0)
            {
                psd_avg_0 = list_30[0].Average();//head
                psd_avg_1 = list_30[1].Average();//left
                psd_avg_2 = list_30[2].Average();//right
                if (checkNormaliz.IsChecked == true)
                {
                    psd_avg_0 -= normal_min0; psd_avg_0 /= normal_diff0;
                    psd_avg_1 -= normal_min1; psd_avg_1 /= normal_diff1;
                    psd_avg_2 -= normal_min1; psd_avg_2 /= normal_diff2;
                }
                printAvgD(psd_avg_0, psd_avg_1, psd_avg_2);
            }
        }

        public void psd_checkMoveFlag()
        {
            if (list_30[0].Count > 0)
            {
                //classify by Threshold
                if (psdByThreshold.IsChecked == true)
                {
                    
                    flag_classMove = psd_classify_byThreshold(psd_avg_0, psd_avg_1, psd_avg_2);
                }
                //classify by Classifier Model
                else
                {
                    string arff_data = "S," + psd_avg_0 + "," + psd_avg_1 + "," + psd_avg_2;
                    flag_classMove = TheProlongSitDetector.test(arff_data);
                    if (flag_classMove == "M") { flag_classMove = "Move"; }
                    else { flag_classMove = "Still"; }
                }
                //------------------------------------
                //int class
                if (flag_classMove == "Move") { flag_classMove0 = 1; }
                else { flag_classMove0 = 0; }
            }
        }

        public void psd_addData()
        {
            //Stand == Move
            if (is_Sit == true && getBase == true) { txtMove.Content = flag_classMove; }
            else { txtMove.Content = ""; }
            //--------------------------------------
            try
            {
                HRL_calculation(); HRL_set1(false); playSound();
            }
            catch { TheSys.showError("HRL_calculation()", true); }
            try
            {
                reporter();
            }
            catch (Exception ex) { TheSys.showError("dtr_AddData()", true); TheSys.showError("psdAdd:" + ex.ToString(), true); }
        }

        void HRL_set1(Boolean ignoreFreeze)
        {
            try
            {
                HRL_totalrisk_scoreCal();
                HRL_totalrisk_lvCal();
                if (is_Freeze == false || ignoreFreeze == true)
                {
                    HRL_progressbar();
                    HRL_totalrisk_scoreDraw(hrl_totalrisk_score);
                    HRL_totalrisk_barColor(hrl_totalrisk_level);
                    if (checkVMan.IsChecked == true)
                    {
                        HRL_img(break_flag, stand_flag
                            , flag_classMove0, pitch_flag, twist_flag
                            , hrl_prolong_lv, hrl_pitch_lv, hrl_twist_lv);
                    }
                }
            }
            catch (Exception ex) {
                TheSys.showError("HRL1:" + ex.Message, true); 
            }
        }


        public void printAvgD(double avg0, double avg1,double avg2)
        {
            if (is_Freeze == false)
            {
                txtHead.Content = "H: " + Math.Round(avg0, 3);
                txtElbowL.Content = "L: " + Math.Round(avg1, 3);
                txtElbowR.Content = "R: " + Math.Round(avg2, 3);
            }
        }

        double psd_threshold0 = 0;//Head
        double psd_threshold1 = 0;//Left Elbow
        double psd_threshold2 = 0;//Right Elbow


        public string psd_classify_byThreshold(double avg_0, double avg_1, double avg_2)
        {
            string class0 = "Still";//still
            if (avg_0 > psd_threshold0) { class0 = "Move"; }
            else if (avg_1 > psd_threshold1) { class0 = "Move"; }
            else if (avg_2 > psd_threshold2) { class0 = "Move"; }
            //else
            //{
            //    double a = avg_0 + avg_1 + avg_2;
            //    double b = psd_threshold1 + psd_threshold2;
            //    if (a > b) { class0 = "Move"; }
            //}

            txtHead0.Content = Math.Round(avg_0, 3) + "";
            txtElbowL0.Content = Math.Round(avg_1, 3) + "";
            txtElbowR0.Content = Math.Round(avg_2, 3) + "";
            //TheSys.showError(
            //    Math.Round(avg_0,3) + "//" + psd_threshold0 + ";"
            //    + Math.Round(avg_1, 3) + "//" + psd_threshold1 + ";"
            //    + Math.Round(avg_2, 3) + "//" + psd_threshold2 + ";"
            //    + class0
            //    , true);
            return class0;
        }


        //-----------------------------------------------------------
        // For Normaliz 

        double normal_min0 = 0; double normal_max0 = 0; double normal_diff0 = 0;//Hand
        double normal_min1 = 0; double normal_max1 = 0; double normal_diff1 = 0;//ElbowL
        double normal_min2 = 0; double normal_max2 = 0; double normal_diff2 = 0;//ElbowR

        private void butNormaliz_Click(object sender, RoutedEventArgs e)
        {
            applyMinMax();
        }

        void applyMinMax()
        {
            normal_min0 = TheTool.getDouble(txtNormalMin0);
            normal_max0 = TheTool.getDouble(txtNormalMax0);
            normal_min1 = TheTool.getDouble(txtNormalMin1);
            normal_max1 = TheTool.getDouble(txtNormalMax1);
            normal_min2 = TheTool.getDouble(txtNormalMin2);
            normal_max2 = TheTool.getDouble(txtNormalMax2);
            normal_diff0 = normal_max0 - normal_min0;
            normal_diff1 = normal_max1 - normal_min1;
            normal_diff2 = normal_max1 - normal_min2;
        }

        private void butClassifySet_Click(object sender, RoutedEventArgs e)
        {
            psd_applyThreshold();
        }

        void psd_applyThreshold()
        {
            psd_threshold0 = TheTool.getDouble(txtThreshold0);
            psd_threshold1 = TheTool.getDouble(txtThreshold1);
            psd_threshold2 = TheTool.getDouble(txtThreshold2);
        }

        //===============================================================================
        //=================== Heal Risk Level System =========================
        //at this moment
        int hrl_isNow_prolong = 0;//1 is move
        int hrl_isNow_pitch = 0;//1 is pitch
        int hrl_isNow_turn = 0;//1 is turn

        //HRL : 0 green, 1 yellow, 2 red ------------------------
        int hrl_prolong_lv = 0; int hrl_prolong_lv_last = 0;
        int hrl_pitch_lv = 0;int hrl_pitch_lv_last = 0;
        int hrl_twist_lv = 0;int hrl_twist_lv_last = 0;

        //Risk score -------------------------
        int hrl_prolong_score = 0; 
        int hrl_prolong_score_max = 240;
        int hrl_prolong_score_add = 1; int hrl_prolong_score_subtract = 12;
        int hrl_prolong_score_lv1 = 40; int hrl_prolong_score_lv2 = 60;
        //
        int hrl_pitch_score = 0; 
        int hrl_pitch_score_max = 60;
        int hrl_pitch_score_add = 1; int hrl_pitch_score_subtract = 5;
        int hrl_pitch_score_lv1 = 6; int hrl_pitch_score_lv2 = 18;
        //
        int hrl_twist_score = 0; 
        int hrl_twist_score_max = 60;
        int hrl_twist_score_add = 1; int hrl_twist_score_subtract = 5;
        int hrl_twist_score_lv1 = 6; int hrl_twist_score_lv2 = 18;

        //Total Risk
        int hrl_totalrisk_score = 0;
        double hrl_totalrisk_level = 0;
        Brush hrl_totalrisk_brush = Brushes.Green;
        int hrl_totalrisk_score_lv1 = 0; 
        int hrl_totalrisk_score_lv2 = 0;

        private void butHRL_apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                butHRL_apply(); progressBar_applyMax();
                butHRL_applyLv.Foreground = Brushes.Black;
                cal_TotalRisk_threshold();
            }
            catch { }
        }

        private void butHRL_default_Click(object sender, RoutedEventArgs e)
        {
            txtHRL_still_lv1.Text = "40"; txtHRL_still_lv2.Text = "60"; txtHRL_still_max.Text = "240";
            txtHRL_pitch_lv1.Text = "6"; txtHRL_pitch_lv2.Text = "18"; txtHRL_pitch_max.Text = "60";
            txtHRL_face_lv1.Text = "6"; txtHRL_face_lv2.Text = "18"; txtHRL_face_max.Text = "60";
            butHRL_applyLv.Foreground = Brushes.Blue;
        }

        private void butHRL_test_Click(object sender, RoutedEventArgs e)
        {
            txtHRL_still_lv1.Text = "1"; txtHRL_still_lv2.Text = "2"; txtHRL_still_max.Text = "4";
            txtHRL_pitch_lv1.Text = "2"; txtHRL_pitch_lv2.Text = "4"; txtHRL_pitch_max.Text = "24";
            txtHRL_face_lv1.Text = "2"; txtHRL_face_lv2.Text = "4"; txtHRL_face_max.Text = "24";
            butHRL_applyLv.Foreground = Brushes.Blue;
        }

        void butHRL_apply()
        {
            hrl_prolong_score_lv1 = TheTool.getInt(txtHRL_still_lv1);
            hrl_prolong_score_lv2 = TheTool.getInt(txtHRL_still_lv2);
            hrl_prolong_score_max = TheTool.getInt(txtHRL_still_max);
            hrl_pitch_score_lv1 = TheTool.getInt(txtHRL_pitch_lv1);
            hrl_pitch_score_lv2 = TheTool.getInt(txtHRL_pitch_lv2);
            hrl_pitch_score_max = TheTool.getInt(txtHRL_pitch_max);
            hrl_twist_score_lv1 = TheTool.getInt(txtHRL_face_lv1);
            hrl_twist_score_lv2 = TheTool.getInt(txtHRL_face_lv2);
            hrl_twist_score_max = TheTool.getInt(txtHRL_face_max);
        }

        void progressBar_applyMax()
        {
            progressPS.Maximum = hrl_prolong_score_max;
            progressPitch.Maximum = hrl_pitch_score_max;
            progressTwist.Maximum = hrl_twist_score_max;
        }

        void cal_TotalRisk_threshold()
        {
            try{
                double temp1 = 0;
                temp1 += (double)(70 * hrl_prolong_score_lv1 / hrl_prolong_score_max);
                temp1 += (double)(15 * hrl_pitch_score_lv1 / hrl_pitch_score_max);
                temp1 += (double)(15 * hrl_twist_score_lv1 / hrl_twist_score_max);
                hrl_totalrisk_score_lv1 = (int)temp1;
                //----------------------
                double temp2 = 0;
                temp2 += (double)(70 * hrl_prolong_score_lv2 / hrl_prolong_score_max);
                temp2 += (double)(15 * hrl_pitch_score_lv2 / hrl_pitch_score_max);
                temp2 += (double)(15 * hrl_twist_score_lv2 / hrl_twist_score_max);
                hrl_totalrisk_score_lv2 = (int)temp2;
            }
            catch (Exception ex) { TheSys.showError("totalRisk:" + ex.Message, true); }
        }


        private void butHRL_reset_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Health Data will be reset."
                                        , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                HRL_reset();
            }
        }

        void HRL_reset(){
             hrl_prolong_lv = 0; 
             hrl_pitch_lv = 0;
             hrl_twist_lv = 0;
             hrl_prolong_score = 0; 
             hrl_pitch_score = 0; 
             hrl_twist_score = 0; 
        }

        //Mai function
        void HRL_calculation(){
            if (break_flag == 0)
            {
                if (stand_flag == 0)
                {
                    //------ Prolong --------------
                    if (flag_classMove0 == 0)
                    {
                        hrl_isNow_prolong = 1;
                        //--------- Pitch & Twist ------------
                        if (pitch_flag == 1) { hrl_isNow_pitch = 1; }
                        else { hrl_isNow_pitch = 0; }
                        if (twist_flag == 1 || twist_flag == -1) { hrl_isNow_turn = 1; }
                        else { hrl_isNow_turn = 0; }
                    }
                    else { hrl_isNow_prolong = 0; hrl_isNow_pitch = 0; hrl_isNow_turn = 0; }
                }
                else { hrl_isNow_prolong = 0; hrl_isNow_pitch = 0; hrl_isNow_turn = 0; }
                //-------------
                HRL_sub_Scoring(); HRL_levelCal();
            }
            else {
                //Taking Break
                //HRL_reset(); 
            }
        }

        Boolean prolongScoring = true;
        void HRL_sub_Scoring()
        {
            //PROLONGED
            if (prolongScoring == true)
            {
                if (hrl_isNow_prolong == 1)
                {
                    hrl_prolong_score = TheTool.math_add(hrl_prolong_score, hrl_prolong_score_add, hrl_prolong_score_max);
                }
                else
                {
                    hrl_prolong_score = TheTool.math_subtract(hrl_prolong_score, hrl_prolong_score_subtract, 0);
                }
                prolongScoring = false;
            }
            //PITCH
            if (hrl_isNow_pitch == 1)
            {
                hrl_pitch_score = TheTool.math_add(hrl_pitch_score, hrl_pitch_score_add, hrl_pitch_score_max);
            }
            else
            {
                hrl_pitch_score = TheTool.math_subtract(hrl_pitch_score, hrl_pitch_score_subtract, 0);
            }            
            //----------------------------
            //TWIST
            if (hrl_isNow_turn == 1)
            {
                hrl_twist_score = TheTool.math_add(hrl_twist_score, hrl_twist_score_add, hrl_twist_score_max);
            }
            else
            {
                hrl_twist_score = TheTool.math_subtract(hrl_twist_score, hrl_twist_score_subtract, 0);
            }
        }

            
        void HRL_levelCal()
        {
            if (hrl_prolong_score > hrl_prolong_score_lv2) { hrl_prolong_lv = 2; }
            else if (hrl_prolong_score > hrl_prolong_score_lv1) { hrl_prolong_lv = 1; }
            else { hrl_prolong_lv = 0; }
            if (hrl_pitch_score > hrl_pitch_score_lv2) { hrl_pitch_lv = 2; }
            else if (hrl_pitch_score > hrl_pitch_score_lv1) { hrl_pitch_lv = 1; }
            else { hrl_pitch_lv = 0; }
            if (hrl_twist_score > hrl_twist_score_lv2) { hrl_twist_lv = 2; }
            else if (hrl_twist_score > hrl_twist_score_lv1) { hrl_twist_lv = 1; }
            else { hrl_twist_lv = 0; }
        }

        void HRL_totalrisk_scoreCal()
        {
            double totalRisk = 0;
            totalRisk += (double)(15 * hrl_twist_score / hrl_twist_score_max);
            totalRisk += (double)(15 * hrl_pitch_score / hrl_pitch_score_max);
            totalRisk += (double)(70 * hrl_prolong_score / hrl_prolong_score_max);
            if (totalRisk > 100) { totalRisk = 100; }
            hrl_totalrisk_score = (int)totalRisk;
        }

        void HRL_totalrisk_lvCal()
        {
            double temp = (double)hrl_totalrisk_score / (double)hrl_totalrisk_score_lv2;
            if (temp > 1) { temp = 1; }
            hrl_totalrisk_level = Math.Round(temp * 2, 2);
        }

        //score max = 100
        void HRL_totalrisk_barColor(double lv)
        {
            try
            {
                lv = lv / 2 * 255;
                byte red = 0; byte green = 0;
                red = (byte)lv;
                green = (byte)(255 - lv);
                Brush lvColor = new SolidColorBrush(Color.FromRgb(red, green, 0));
                hrl_totalrisk_brush = lvColor;
                rectTRisk.Fill = lvColor;
                pgTRisk.Foreground = hrl_totalrisk_brush;
            }
            catch (Exception ex)
            {
                TheSys.showError("barColor:" + ex.Message, true);
            }
        }

        void HRL_totalrisk_scoreDraw(int score)
        {
            txtScoreTRisk.Content = "Score: " + score;
            pgTRisk.Value = score;
        }

        private void checkVHip_Checked(object sender, RoutedEventArgs e)
        {
            try{mainForm.show_VHip = checkVHip.IsChecked.Value;}catch{}
        }

        private void checkPSD_Checked(object sender, RoutedEventArgs e)
        {
            classifyChange();
        }

        //--------------------------------------

        void HRL_progressbar()
        {
            try
            {
                progressPS.Value = hrl_prolong_score;
                setBarColor_checkLv(ref progressPS, ref hrl_prolong_lv, ref hrl_prolong_lv_last);
                //
                progressPitch.Value = hrl_pitch_score;
                setBarColor_checkLv(ref progressPitch, ref hrl_pitch_lv, ref hrl_pitch_lv_last);
                //
                progressTwist.Value = hrl_twist_score;
                setBarColor_checkLv(ref progressTwist, ref hrl_twist_lv, ref hrl_twist_lv_last);
                //-------------------
                setBarScore(hrl_prolong_score, hrl_pitch_score, hrl_twist_score);
            }
            catch (Exception e) { TheSys.showError("pgbar:" + e.Message, true); }
        }

        void setBarScore(int intPS, int intPitch, int intTwist)
        {
            txtScorePS.Content = "Score: " + intPS.ToString();
            txtScorePitch.Content = "Score: " + intPitch.ToString();
            txtScoreTwist.Content = "Score: " + intTwist.ToString();
        }

        //Check to reduce workload, if there is no change
        void setBarColor_checkLv(ref ProgressBar pg, ref int lv_current, ref int lv_last)
        {
            if (lv_current != lv_last)
            {
                setBarColor(ref pg, lv_current);
            }
            lv_last = lv_current;
        }

        void setBarColor(ref ProgressBar pg, int lv)
        {
            if (lv == 1) { pg.Foreground = Brushes.Yellow; }
            else if (lv == 2) { pg.Foreground = Brushes.Red; }
            else { pg.Foreground = Brushes.Green; }
        }

        System.Windows.Media.SolidColorBrush hrl_getColor(int lv)
        {
            if (lv == 1) { return Brushes.Yellow; }
            else if (lv == 2) { return Brushes.Red; }
            else { return Brushes.Green; }
        }

        //---------------------------------------------------------------------------------------------------------------------------

        public static BitmapImage ows_main1 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_main1.jpg", UriKind.Relative));
        public static BitmapImage ows_main2 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_main2.jpg", UriKind.Relative));
        public static BitmapImage ows_main3 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_main3.jpg", UriKind.Relative));
        public static BitmapImage ows_main4 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_main4.jpg", UriKind.Relative));
        public static BitmapImage ows_a0 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_a0.png", UriKind.Relative));
        public static BitmapImage ows_a1 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_a1.png", UriKind.Relative));
        public static BitmapImage ows_a2 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_a2.png", UriKind.Relative));
        public static BitmapImage ows_f0 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_f0.png", UriKind.Relative));
        public static BitmapImage ows_f1 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_f1.png", UriKind.Relative));
        public static BitmapImage ows_f2 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_f2.png", UriKind.Relative));
        public static BitmapImage ows_fp00 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp00.png", UriKind.Relative));
        public static BitmapImage ows_fp01 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp01.png", UriKind.Relative));
        public static BitmapImage ows_fp02 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp02.png", UriKind.Relative));
        public static BitmapImage ows_fp10 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp10.png", UriKind.Relative));
        public static BitmapImage ows_fp11 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp11.png", UriKind.Relative));
        public static BitmapImage ows_fp12 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp12.png", UriKind.Relative));
        public static BitmapImage ows_fp20 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp20.png", UriKind.Relative));
        public static BitmapImage ows_fp21 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp21.png", UriKind.Relative));
        public static BitmapImage ows_fp22 = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_fp22.png", UriKind.Relative));
        public static BitmapImage ows_move = new BitmapImage(new Uri(@"/P_Tracker2;component/img/ows_m.png", UriKind.Relative));
        public static BitmapImage img_blank = new BitmapImage(new Uri(@"/P_Tracker2;component/img/blank.png", UriKind.Relative));

        void HRL_img(int f_break, int  f_stand
            , int f_move, int f_pitch, int f_twist
            , int lv_prolong, int lv_pitch, int lv_twist)
        {
            try
            {
                
                    if (f_break == 0)
                    {
                        if (f_stand == 0)
                        {
                            if (f_pitch == 1)
                            {
                                if (lv_prolong == 0 && lv_pitch == 0) { imgFace.Source = ows_fp00; }
                                else if (lv_prolong == 1 && lv_pitch == 0) { imgFace.Source = ows_fp10; }
                                else if (lv_prolong == 2 && lv_pitch == 0) { imgFace.Source = ows_fp20; }
                                else if (lv_prolong == 0 && lv_pitch == 1) { imgFace.Source = ows_fp01; }
                                else if (lv_prolong == 1 && lv_pitch == 1) { imgFace.Source = ows_fp11; }
                                else if (lv_prolong == 2 && lv_pitch == 1) { imgFace.Source = ows_fp21; }
                                else if (lv_prolong == 0 && lv_pitch == 2) { imgFace.Source = ows_fp02; }
                                else if (lv_prolong == 1 && lv_pitch == 2) { imgFace.Source = ows_fp12; }
                                else if (lv_prolong == 2 && lv_pitch == 2) { imgFace.Source = ows_fp22; }
                            }
                            else
                            {
                                if (lv_prolong == 0) { imgFace.Source = ows_f0; }
                                else if (lv_prolong == 1) { imgFace.Source = ows_f1; }
                                else if (lv_prolong == 2) { imgFace.Source = ows_f2; }
                            }
                            //
                            if (lv_twist == 1)
                            {
                                if (f_twist == 1) { imgTR.Source = ows_a1; imgTL.Source = img_blank; }
                                else if (f_twist == -1) { imgTL.Source = ows_a1; imgTR.Source = img_blank; }

                            }
                            else if (lv_twist == 2)
                            {
                                if (f_twist == 1) { imgTR.Source = ows_a2; imgTL.Source = img_blank; }
                                else if (f_twist == -1) { imgTL.Source = ows_a2; imgTR.Source = img_blank; }
                            }
                            else {
                                if (f_twist == 1) { imgTR.Source = ows_a0; imgTL.Source = img_blank; }
                                else if (f_twist == -1) { imgTL.Source = ows_a0; imgTR.Source = img_blank; }
                                else
                                {
                                    imgTR.Source = img_blank; imgTL.Source = img_blank;
                                }
                            }
                            //
                            if (f_move == 1) { imgMove.Source = ows_move; }
                            else { imgMove.Source = img_blank; }
                            imgMain.Source = ows_main1;
                        }
                        else {
                            if (f_move == 1) { HRL_img_set(ows_main4, img_blank, img_blank, img_blank, img_blank); } 
                            else { HRL_img_set(ows_main2, img_blank, img_blank, img_blank, img_blank); }
                        }
                    }
                    else { HRL_img_set(ows_main3, img_blank, img_blank, img_blank, img_blank); }
            }
            catch (Exception e) { TheSys.showError("HRLimg:" + e.Message, true); }
        }

        void HRL_img_set(BitmapImage i_main,BitmapImage i_face,BitmapImage i_tr,BitmapImage i_tl,BitmapImage i_move)
        {
            imgMain.Source = i_main;
            imgFace.Source = i_face;
            imgTR.Source = i_tr;
            imgTL.Source = i_tl;
            imgMove.Source = i_move;
        }

        //------------------------------
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            hrl_prolong_score = TheTool.math_add(hrl_prolong_score, 30, hrl_prolong_score_max);
            HRL_levelCal(); HRL_set1(true);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            hrl_prolong_score = TheTool.math_subtract(hrl_prolong_score, 30, 0);
            HRL_levelCal(); HRL_set1(true);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            hrl_pitch_score = TheTool.math_add(hrl_pitch_score, 5, hrl_pitch_score_max);
            HRL_levelCal(); HRL_set1(true);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            hrl_pitch_score = TheTool.math_subtract(hrl_pitch_score, 5, 0); 
            HRL_levelCal(); HRL_set1(true);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            hrl_twist_score = TheTool.math_add(hrl_twist_score, 5, hrl_twist_score_max);
            HRL_levelCal(); HRL_set1(true);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            hrl_twist_score = TheTool.math_subtract(hrl_twist_score, 5, 0); 
            hrl_twist_score -= 5; HRL_levelCal(); HRL_set1(true);
        }

        private void checkPOSmonitor_Checked(object sender, RoutedEventArgs e)
        {
            if (checkPOSmonitor.IsChecked == true)
            {
                microStart();
            }
        }

        //-------------------
        Boolean is_Freeze = false;

        private void checkFreeze_Checked(object sender, RoutedEventArgs e)
        {
            is_Freeze = checkFreeze.IsChecked.Value;
        }

        int flag_classMove0 = 0;// 0 is still
        string flag_classMove = "";// Still or Move

        //===============================================================================
        //========== Report =============================================================
        
        // full data = time + data
        List<DetectorReportData> report_data = new List<DetectorReportData> { };
        List<string> report_dataString = new List<string> { };

        void reporter()
        {
            try
            {
                temp_report_lastTime = DateTime.Now;
                if (is_waitForComeBack == false){
                    //prevent Overlap
                    report_addData(temp_report_lastTime);
                }
                cal_sliderTime();
                if (is_Freeze == false)
                {
                    skip_slideTime_ValueChanged = true;
                    slideTime.Value = slider_max;
                    txtSlideTime.Content = temp_report_lastTime.ToString("HH:mm:ss");
                }
                txtSlideNum.Content = "(" + slideTime.Value + "/" + slider_max + ")";
            }
            catch (Exception e) { TheSys.showError("report:" + e.Message, true); }
        }


        //string temp_report_lastTime_txt = "";
        DateTime temp_report_lastTime = DateTime.Now;

        void report_addData(DateTime time0)
        {
            string time = time0.ToString("HH:mm:ss");
            DetectorReportData drd = new DetectorReportData();
            //-------------------------------------
            string data =  time;
            data += "," + flag_classMove0;
            data += "," + pitch_flag;
            data += "," + twist_flag;
            data += "," + stand_flag;
            //
            data += "," + break_flag;
            data += "," + hrl_prolong_lv;
            data += "," + hrl_pitch_lv;
            data += "," + hrl_twist_lv;
            data += "," + hrl_totalrisk_level;//double
            //
            data += "," + hrl_prolong_score;
            data += "," + hrl_pitch_score;
            data += "," + hrl_twist_score;
            data += "," + hrl_totalrisk_score;
            data += "," + TheTool.string_replaceChar(state_String,", "," ");
            //-------------------
            drd.time = time;
            drd.flag_move = flag_classMove0;
            drd.flag_pitch = pitch_flag;
            drd.flag_twist = twist_flag;
            drd.flag_stand = stand_flag;
            //
            drd.flag_break = break_flag;
            drd.prolong_lv = hrl_prolong_lv;
            drd.pitch_lv = hrl_pitch_lv;
            drd.twist_lv = hrl_twist_lv;
            drd.total_lv = hrl_totalrisk_level;
            //
            drd.prolong_score = hrl_prolong_score;
            drd.pitch_score = hrl_pitch_score;
            drd.twist_score = hrl_twist_score;
            drd.total_score = hrl_totalrisk_score;
            drd.state = state_String;
            //-------------------------------------
            report_data.Add(drd);
            report_dataString.Add(data);
        }

        int slider_max = 0;
        void cal_sliderTime()
        {
            slider_max = report_dataString.Count - 1;
            if (slideTime.Value > slider_max) { slideTime.Value = slider_max; }
            slideTime.Maximum = slider_max;
        }

        Boolean skip_slideTime_ValueChanged = false;
        private void slideTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (skip_slideTime_ValueChanged == false)
            {
                try
                {
                    DetectorReportData drd = report_data[(int)slideTime.Value];
                    //txtSlideTime.Content = getTimeFormat(drd.time);
                    txtSlideTime.Content = drd.time;
                    txtSlideNum.Content = "(" + slideTime.Value + "/" + slider_max + ")";
                    //
                    HRL_img(drd.flag_break, drd.flag_stand
                            , drd.flag_move, drd.flag_pitch, drd.flag_twist
                            , drd.prolong_lv, drd.pitch_lv, drd.twist_lv);
                    //---------------------
                    progressPS.Value = drd.prolong_score;
                    progressPitch.Value = drd.pitch_score;
                    progressTwist.Value = drd.twist_score;
                    setBarScore(drd.prolong_score, drd.pitch_score, drd.twist_score);
                    setBarColor(ref progressPS, drd.prolong_lv);
                    setBarColor(ref progressPitch, drd.pitch_lv);
                    setBarColor(ref progressTwist, drd.twist_lv);
                    //
                    HRL_totalrisk_scoreDraw(drd.total_score);
                    HRL_totalrisk_barColor(drd.total_lv);
                    //
                    string txt = TheTool.string_replaceChar(drd.state, " ", ", ");
                    txt =  TheTool.string_replaceChar(txt, ",,", ",");
                    txtState.Content = txt;
                    //
                    if (drd.flag_move == 1) { txtMove.Content = "Move"; }
                    else { txtMove.Content = "Still"; }
                    //
                    
                }
                catch { }
            }
            skip_slideTime_ValueChanged = false;
        }


        //***************************************************************
        //******** History Data ******************

        //Browse
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".csv", TheURL.url_saveFolder);
            // Get the selected file name and display in a TextBox
            if (openDialog == true)
            {
                TheTool_DetectorReportData.import(TheTool.dialog.FileName
                    ,ref report_data,ref report_dataString);
                cal_sliderTime();
            }
        }

        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            string file_name = "R" + DateTime.Now.ToString("MMdd_HHmmss");
            saveReportData(file_name, true);
        }


        public void saveReportData(string file_name, Boolean popup)
        {
            if(report_dataString.Count > 0){
                List<string> listData = new List<string> { };
                listData.Add("Time,Move,Pitch,Twist,Stand,Break,ProlongLv,PitchLv,TwistLv,TotalLv,ProlongScore,PitchScore,TwistScore,TotalScore,State");
                foreach (string s0 in report_dataString)
                {
                    listData.Add(s0);
                }
                TheTool.exportCSV_orTXT(TheURL.url_saveFolder + file_name + ".csv", listData, popup);
            }
        }


        //***************************************************************
        //******** Sound ******************

        int lastFlag_wrongPost = 0;
        int lastFlag_maxRiskLv = 0;
        DateTime lastBreak_alert = DateTime.Now;

        void playSound()
        {
            string thisSoundPath = "";
            //Do not play sound when Stand / Move
            if (stand_flag == 0 && flag_classMove0 == 0)
            {
                int flag_wrongPost = 0;
                int flag_maxRiskLv = 0;
                int flag_break = 0;
                //==============================
                //--- Get Max Lv -----------------------------------
                if (hrl_prolong_lv == 2 || hrl_pitch_lv == 2 || hrl_twist_lv == 2)
                {
                    flag_maxRiskLv = 2;
                }
                else if (hrl_prolong_lv == 1 || hrl_pitch_lv == 1 || hrl_twist_lv == 1)
                {
                    flag_maxRiskLv = 1;
                }
                //--- Check Wrong Post --------------------------------------------------------
                if((pitch_flag == 1) && (twist_flag == 1 || twist_flag == -1 ))
                {
                    flag_wrongPost = 12;
                }
                else if (twist_flag == 1 || twist_flag == -1)
                {
                    flag_wrongPost = 2;
                }
                else if (pitch_flag == 1)
                {
                    flag_wrongPost = 1;
                }
                //--- Check Break Time -----------------------------------
                if (hrl_prolong_lv == 2)
                {
                    flag_break = 2;
                }
                else if (hrl_prolong_lv == 1)
                {
                    flag_break = 1;
                }
                //====================================================
                //========= sound to report =============================

                //--- 1 : check Break  --------
                if (flag_break > 0)
                {
                    DateTime thisTime = DateTime.Now;
                    TimeSpan span = thisTime.Subtract(lastBreak_alert);
                    if ((int)span.TotalSeconds > 180)//speak every 3 min
                    {
                        if (checkSpeech.IsChecked == true && flag_break == 2){thisSoundPath = TheURL.sp_break2;}
                        else if (checkSpeech.IsChecked == true && flag_break == 1){thisSoundPath = TheURL.sp_break1;}
                        else if (checkSound.IsChecked == true) { thisSoundPath = TheURL.s_timeBreak; }
                        lastBreak_alert = thisTime;
                    }
                }
                if (thisSoundPath == "")
                {
                    //--- 2 : check Lv change  --------
                    if (flag_maxRiskLv != lastFlag_maxRiskLv)
                    {
                        if (flag_maxRiskLv == 2)
                        {
                            if (checkSpeech.IsChecked == true) { thisSoundPath = TheURL.sp_lv2; }
                            else if (checkSound.IsChecked == true) { thisSoundPath = TheURL.s_lv2; }
                        }
                        else if (flag_maxRiskLv == 1)
                        {
                            if (checkSpeech.IsChecked == true) { thisSoundPath = TheURL.sp_lv1; }
                            else if (checkSound.IsChecked == true) { thisSoundPath = TheURL.s_lv1; }
                        }
                        else
                        {
                            if (checkSound.IsChecked == true) { thisSoundPath = TheURL.s_lv0; }
                        }
                        lastFlag_maxRiskLv = flag_maxRiskLv;
                    }
                    else
                    {
                        //--- 3 : check Post change  --------
                        if (flag_wrongPost != lastFlag_wrongPost)
                        {
                            if (flag_wrongPost > 0)
                            {
                                if (pitch_flag == 1 && checkSpeech.IsChecked == true) { thisSoundPath = TheURL.sp_pitch; }
                                else if (twist_flag == 1 && checkSpeech.IsChecked == true) { thisSoundPath = TheURL.sp_twistR; }
                                else if (twist_flag == -1 && checkSpeech.IsChecked == true) { thisSoundPath = TheURL.sp_twistL; }
                                else if (checkSound.IsChecked == true) { thisSoundPath = TheURL.s_p_wrong; }
                            }
                            else
                            {
                                if (checkSound.IsChecked == true) { thisSoundPath = TheURL.s_p_correct; }
                            }
                            lastFlag_wrongPost = flag_wrongPost;
                        }
                        
                    }
                }
                if (thisSoundPath != "") { TheSound.soundPlay(thisSoundPath); }
            }
        }


        //====================================================
        //=== Break Effect ======
        void break_parameterSetup()
        {
            state_String = "Break";
            flag_classMove0 = 1;
            pitch_flag = 0;
            twist_flag = 0;
            stand_flag = 0;
            break_flag = 1;
        }

        DateTime time_walkout = DateTime.Now;
        void HRL_breakEffect_whenComeBack()
        {
            try
            {
                //Initial Effect ---------------
                report_addData(time_walkout);
                hrl_pitch_score = 0;//Reset when walk out
                hrl_twist_score = 0;//Reset when walk out
                break_parameterSetup();
                HRL_levelCal();HRL_totalrisk_scoreCal();HRL_totalrisk_lvCal();
                //Calculate Time ---------------
                DateTime time_comeback = DateTime.Now;
                TimeSpan span = time_comeback.Subtract(time_walkout);
                int break_sec = (int)span.TotalSeconds;
                //Effect per time --------------
                while(break_sec > 30){
                    //effect at 10s, 20s
                    time_walkout = time_walkout.AddSeconds(10);
                    report_addData(time_walkout);
                    time_walkout = time_walkout.AddSeconds(10);
                    report_addData(time_walkout);
                    //effect at 30s
                    hrl_prolong_score -= hrl_prolong_score_subtract;
                    if (hrl_prolong_score <= 0) { hrl_prolong_score = 0; }
                    HRL_levelCal();HRL_totalrisk_scoreCal();HRL_totalrisk_lvCal();
                    time_walkout = time_walkout.AddSeconds(10); 
                    report_addData(time_walkout);
                    //---------------
                    break_sec -= 30;
                }
                while (break_sec > 10)
                {
                    time_walkout = time_walkout.AddSeconds(10); 
                    report_addData(time_walkout);
                    break_sec -= 10;
                }
            }
            catch (Exception e) { TheSys.showError("breakEff:" + e.Message, true); }
        }

        //***************************************************************
        //******** Speech ******************

        void playSpeech(string path)
        {
            if (checkSpeech.IsChecked == true)
            {
                TheSound.speechPlay(path);
            }
        }

        //-----------------------------------------------------------
        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(TheURL.url_saveFolder);
            }
            catch { }
        }

        public Boolean is_toBreak = false;
        private void butBreak_Click(object sender, RoutedEventArgs e)
        {
            if (is_toBreak == false) { is_toBreak = true; playSpeech(TheURL.sp_niceBreak); }
        }

        private void butBackToWork_Click(object sender, RoutedEventArgs e)
        {
            if (is_toBreak == true) { is_toBreak = false; }
        }

        public GraphVisualizer graphVisualizer = null;
        private void buGraph_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (graphVisualizer == null) { graphVisualizer = new GraphVisualizer(); graphVisualizer.Show(); }
                graphVisualizer.detector = this;
                graphVisualizer.runData(report_data);
            }
            catch {}
        }

    }
}
