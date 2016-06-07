using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{

    class logDetection
    {
        public int detectAt = 99999;//frame that detection complete
        public int num_pose = 0;//number of posture
        public string info = "";//P1:41, P2:65, P3:98 //Posture 1 is detected at 41
    }

    class TheRuleTester
    {
        public static logDetection temp_log = new logDetection();//temp data for show result

        //Check if a specific Motion is exist in Data
        //All Data - 1 Motion
        public static Boolean testDetectMotion(List<UKI_Data_AnalysisForm> data, List<m_If> list_rule_motion)
        {
            temp_log = new logDetection();
            Boolean pass_Motion = false;
            //-- Prepare Posture & Rule ------------------
            List<List<m_If>> list_posture = new List<List<m_If>>();
            List<m_If> posture = new List<m_If>();
            foreach (m_If rule in list_rule_motion) {
                if (rule.type == TheMapData.if_type_2Joint || rule.type == TheMapData.if_type_BasicPose
                    || rule.type == TheMapData.if_type_Change || rule.type == TheMapData.if_type_TimeAfterPose
                    || rule.type == TheMapData.if_type_SphereAngle || rule.type == TheMapData.if_type_FlexionAngle
                    )
                {
                    posture.Add(rule); //waiting will be the end of rules
                }
                if (rule.type == TheMapData.if_type_TimeAfterPose || rule == list_rule_motion.Last())
                { 
                    list_posture.Add(posture); posture = new List<m_If>(); 
                }
            }
            //-- Test Data ------------------
            List<m_If>[] arr_posture = list_posture.ToArray();
            int current_posture_id = 0;
            List<m_If> previous_posture = new List<m_If>();
            int time_wait = 0;// = sec * fps
            if (arr_posture.Count() >= 1)
            {
                List<m_If> current_posture = arr_posture[0];
                Boolean first_record = true;
                foreach (UKI_Data_AnalysisForm row in data)
                {
                    Boolean pass_1Posture = testDetect1Posture(row, current_posture, ref time_wait);
                    //
                    if (pass_1Posture)
                    {
                        if (!first_record) { temp_log.info += ", "; }
                        first_record = false;
                        temp_log.info += "P" + (current_posture_id + 1) + ":" + row.id;
                        previous_posture = current_posture;
                        current_posture_id++;
                        if (current_posture_id == arr_posture.Count())
                        {
                            //Already Pass All Posture
                            pass_Motion = true;
                            temp_log.detectAt = row.id;
                            temp_log.num_pose = current_posture_id - 1;
                            break;
                        }
                        else
                        {
                            //Go the the next posture
                            current_posture = arr_posture[current_posture_id];
                        }
                    }
                    else if (current_posture_id > 0)
                    {
                        //new posture is not detected, check if previous posture is ended
                        Boolean pass_previousPoseture = testDetect1Posture(row, previous_posture, ref time_wait);
                        if (!pass_previousPoseture)
                        {
                            time_wait--;//if end then start counting down
                            //reset
                            if (time_wait <= 0)
                            {
                                current_posture_id = 0;
                            }
                        }
                    }
                }
            }
            return pass_Motion;
        }


        public static int fps = 30;

        //Check IF pass all rules of 1 Posture
        public static Boolean testDetect1Posture(UKI_Data_AnalysisForm row, List<m_If> current_posture, ref int time_wait)
        {
            Boolean pass_1Posture = false;
            foreach (m_If subRule in current_posture)
            {
                if (subRule.type == TheMapData.if_type_TimeAfterPose)
                {
                    time_wait = (int)subRule.value_d * fps;
                }
                else
                {
                    pass_1Posture = testRule(row, subRule);
                    if (!pass_1Posture) { break; }
                }
            }
            return pass_1Posture;
        }

        //Retest
        //No Header
        public static List<String> buildMatrixA_file(List<Instance> list_inst, List<m_Motion> list_motion)
        {
            List<String> matrix = new List<String>();
            foreach (Instance inst in list_inst)
            {
                string matrix_data = inst.name + "," + inst.subject_id + "," + inst.motion_id;
                foreach (m_Motion motion in list_motion)
                {
                    Boolean detected = false;
                    detected = TheRuleTester.testDetectMotion(inst.getDataRaw(true), motion.inputs);
                    matrix_data += "," + TheTool.convertBoolean_01(detected);
                }
                matrix.Add(matrix_data);
            }
            return matrix;//TheTool.convert_List_toDataTable(matrix)
        }


        public static Boolean testDetectMotion(List<UKI_DataRaw> data, List<m_If> list_rule_motion)
        {
            List<UKI_Data_AnalysisForm> data_forAnalysis = TheUKI.getData_AnalysisForm(data);
            return testDetectMotion(data_forAnalysis, list_rule_motion);
        }

        // 1 data - 1 rule
        public static Boolean testRule(UKI_Data_AnalysisForm data, m_If rule)
        {
            Boolean pass = false;
            if (rule.type == TheMapData.if_type_2Joint) {
                double diff = 0;
                if (rule.axis == "X" || rule.axis == "Y" || rule.axis == "Z")
                {
                    double v1 = getJointAxis(data.raw, rule.v, rule.axis);
                    double v2 = getJointAxis(data.raw, rule.v2, rule.axis);
                    diff = v1 - v2;
                }
                else
                {
                    diff = TheTool.calEuclidian(getJoint(data.raw, rule.v), getJoint(data.raw, rule.v2));
                }
                pass = TheTool.checkOpt(diff, rule.opt, rule.value_d);
            }
            else if (rule.type == TheMapData.if_type_BasicPose)
            {
                pass = UKI_DataBasicPose_checkBasicPosture(rule, data.bp);
            }
            else if (rule.type == TheMapData.if_type_Change)
            {
                double v1 = getJointAxis(data.raw, rule.v, rule.axis);
                double v2 = getJointAxis(TheInitialPosture.data_raw, rule.v2, rule.axis);
                double test_value = v1 - v2;
                pass = TheTool.checkOpt(test_value, rule.opt, rule.value_d);
                //TheSys.showError(v1 + " - " + v2 + " = " + test_value + rule.opt + rule.value_d);
            }
            else if (rule.type == TheMapData.if_type_SphereAngle)
            {
                Boolean azimuth = false;
                if (rule.value == TheMapData.then_SphereAngle_Azimuth) { azimuth = true; }
                double testValue = TheTool.calSpherical(getJoint(data.raw, rule.v), getJoint(data.raw, rule.v2), azimuth);
                pass = TheTool.checkOpt(testValue, rule.opt, rule.value_d);
            }
            else if (rule.type == TheMapData.if_type_FlexionAngle)
            {
                double testValue = 0;
                if (rule.v == "ElbowL") { testValue = ThePostureCal.calAngle_3Points(data.raw.ShoulderLeft, data.raw.ElbowLeft, data.raw.WristLeft); }
                else if (rule.v == "ElbowR") { testValue = ThePostureCal.calAngle_3Points(data.raw.ShoulderRight, data.raw.ElbowRight, data.raw.WristRight); }
                else if (rule.v == "KneeL") { testValue = ThePostureCal.calAngle_3Points(data.raw.HipLeft, data.raw.KneeLeft, data.raw.AnkleLeft); }
                else if (rule.v == "KneeR") { testValue = ThePostureCal.calAngle_3Points(data.raw.HipRight, data.raw.KneeRight, data.raw.AnkleRight); }
                pass = TheTool.checkOpt(testValue, rule.opt, rule.value_d);
            }
            return pass;
        }


        public static Boolean UKI_DataBasicPose_checkBasicPosture(m_If i, UKI_Data_BasicPose bp)
        {
            Boolean pass = false;
            if (i.v == "jump" && i.value == bp.basic_pose[0]) { pass = true; }
            else if (i.v == "lean" && i.value == bp.basic_pose[1]) { pass = true; }
            else if (i.v == "spin" && i.value == bp.basic_pose[2]) { pass = true; }
            else if (i.v == "handL_X" && i.value == bp.basic_pose[3]) { pass = true; }
            else if (i.v == "handL_Y" && i.value == bp.basic_pose[4]) { pass = true; }
            else if (i.v == "handL_Z" && i.value == bp.basic_pose[5]) { pass = true; }
            else if (i.v == "handR_X" && i.value == bp.basic_pose[6]) { pass = true; }
            else if (i.v == "handR_Y" && i.value == bp.basic_pose[7]) { pass = true; }
            else if (i.v == "handR_Z" && i.value == bp.basic_pose[8]) { pass = true; }
            else if (i.v == "handLR_close" && i.value == bp.basic_pose[9]) { pass = true; }
            else if (i.v == "legL" && i.value == bp.basic_pose[10]) { pass = true; }
            else if (i.v == "legR" && i.value == bp.basic_pose[11]) { pass = true; }
            if (i.opt == "!=") { pass = !pass; }
            return pass;
        }

        //public static double getJointAxis_ChangeFromInitial(UKI_DataRaw data, String jointName, String axis)
        //{
        //    double v1 = getJointAxis(data, jointName, axis);
        //    double v2 = getJointAxis(TheInitialPosture.data_raw, jointName, axis);
        //    return v1 - v2;
        //}

        public static double getJointAxis(UKI_DataRaw data, String jointName, String axis)
        {
            double output = 0;
            int axis_id = 0;
            if (axis == "Y") { axis_id = 1; }
            else if (axis == "Z") { axis_id = 2; }
            //
            if (jointName == "AnkleL") { output = data.AnkleLeft[axis_id]; }
            else if (jointName == "AnkleR") { output = data.AnkleRight[axis_id]; }
            else if (jointName == "ElbowL") { output = data.ElbowLeft[axis_id]; }
            else if (jointName == "ElbowR") { output = data.ElbowRight[axis_id]; }
            else if (jointName == "FootL") { output = data.FootLeft[axis_id]; }
            else if (jointName == "FootR") { output = data.FootRight[axis_id]; }
            else if (jointName == "HandL") { output = data.HandLeft[axis_id]; }
            else if (jointName == "HandR") { output = data.HandRight[axis_id]; }
            else if (jointName == "Head") { output = data.Head[axis_id]; }
            else if (jointName == "HipC") { output = data.HipCenter[axis_id]; }
            else if (jointName == "HipL") { output = data.HipLeft[axis_id]; }
            else if (jointName == "HipR") { output = data.HipRight[axis_id]; }
            else if (jointName == "KneeL") { output = data.KneeLeft[axis_id]; }
            else if (jointName == "KneeR") { output = data.KneeRight[axis_id]; }
            else if (jointName == "ShoulderC") { output = data.ShoulderCenter[axis_id]; }
            else if (jointName == "ShoulderL") { output = data.ShoulderLeft[axis_id]; }
            else if (jointName == "ShoulderR") { output = data.ShoulderRight[axis_id]; }
            else if (jointName == "Spine") { output = data.Spine[axis_id]; }
            else if (jointName == "WristL") { output = data.WristLeft[axis_id]; }
            else if (jointName == "WristR") { output = data.WristRight[axis_id]; }
            return output;
        }

        public static double[] getJoint(UKI_DataRaw data, String jointName)
        {
            double[] output = new double[3];
            if (jointName == "AnkleL") { output = data.AnkleLeft; }
            else if (jointName == "AnkleR") { output = data.AnkleRight; }
            else if (jointName == "ElbowL") { output = data.ElbowLeft; }
            else if (jointName == "ElbowR") { output = data.ElbowRight; }
            else if (jointName == "FootL") { output = data.FootLeft; }
            else if (jointName == "FootR") { output = data.FootRight; }
            else if (jointName == "HandL") { output = data.HandLeft; }
            else if (jointName == "HandR") { output = data.HandRight; }
            else if (jointName == "Head") { output = data.Head; }
            else if (jointName == "HipC") { output = data.HipCenter; }
            else if (jointName == "HipL") { output = data.HipLeft; }
            else if (jointName == "HipR") { output = data.HipRight; }
            else if (jointName == "KneeL") { output = data.KneeLeft; }
            else if (jointName == "KneeR") { output = data.KneeRight; }
            else if (jointName == "ShoulderC") { output = data.ShoulderCenter; }
            else if (jointName == "ShoulderL") { output = data.ShoulderLeft; }
            else if (jointName == "ShoulderR") { output = data.ShoulderRight; }
            else if (jointName == "Spine") { output = data.Spine; }
            else if (jointName == "WristL") { output = data.WristLeft; }
            else if (jointName == "WristR") { output = data.WristRight; }
            return output;
        }

    }

}
