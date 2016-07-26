using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace P_Tracker2
{
    public class PosExtract
    {
        //Boolean ready = false;
        public List<Skeleton> skel_list = new List<Skeleton>();
        public Skeleton skel_last = null;
        public  Skeleton skel_current = null;
        //for offline process, use UKI_DataRaw instead of Skeleton
        public List<UKI_DataRaw> offline_skel_list = new List<UKI_DataRaw>();
        public UKI_DataRaw offline_skel_last = null;
        public UKI_DataRaw offline_skel_current = null;

        //----------------------------------
        //export variable
        public double m_Head = 0;
        public double m_SC = 0;
        public double m_SL = 0;
        public double m_SR = 0;
        public double m_EL= 0;
        public double m_ER = 0;
        public double m_WL = 0;
        public double m_WR = 0;
        public double m_HL = 0;
        public double m_HR = 0;
        public double m_Sp = 0;
        public double m_HpC = 0;
        public double m_HpL = 0;
        public double m_HpR = 0;
        public double m_KL = 0;
        public double m_KR = 0;
        public double m_AL = 0;
        public double m_AR = 0;
        public double m_FL = 0;
        public double m_FR = 0;
        public double m_elbowL_last = 0;
        public double m_elbowR_current = 0;
        public double m_kneeL_last = 0;
        public double m_kneeR_current = 0;
        //----------------------------------
        //variable from SUM
        public double ms_all = 0;
        public double ms_hand = 0;
        public double ms_leg = 0;
        public double ms_core = 0;
        //----------------------------------
        //moving AVG
        public double ms_all_avg = 0;
        public double ms_hand_avg = 0;
        public double ms_leg_avg = 0;
        public double ms_core_avg = 0;
        List<double> ms_all_list = new List<double>();
        List<double> ms_hand_list = new List<double>();
        List<double> ms_leg_list = new List<double>();
        List<double> ms_core_list = new List<double>();


        public static int move_size = 10;//dist between 1 frame
        public static int mva_size_minus1 = 11;//Moving AVG size 11: 5 previous + self + 5 after

        //Main Process
        public void process(UKI f)
        {
            skel_current = f.posture_current;
            updateSkel();
            //if (ready)
            //{
                calMove();
                calSum();
                calSumAvg();
            //}
        }

        public void offline_process(UKI_Offline mr)
        {
            offline_skel_current = mr.current_data;
            offline_updateSkel(offline_skel_current);
            //if (ready)
            //{
                offline_calMove();
                calSum();
                calSumAvg();
            //}
        }

        public void updateSkel()
        {
            skel_list.Add(skel_current);
            if (skel_list.Count > move_size - 1)
            {
                //ready = true;
                skel_last = skel_list.First(); skel_list.RemoveAt(0);
            }
            else { skel_last = skel_list.First(); }
            //else { ready = false; }
        }

        public void offline_updateSkel(UKI_DataRaw s)
        {
            offline_skel_list.Add(offline_skel_current);
            if (offline_skel_list.Count > move_size - 1)
            {
                //ready = true;
                offline_skel_last = offline_skel_list.First(); offline_skel_list.RemoveAt(0); 
            }
            else { offline_skel_last = offline_skel_list.First(); }
            //else { ready = false; }
        }

        public void calMove()
        {
            m_Head = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.Head], skel_last.Joints[JointType.Head]);
            m_SC = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.ShoulderCenter], skel_last.Joints[JointType.ShoulderCenter]);
            m_SL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.ShoulderLeft], skel_last.Joints[JointType.ShoulderLeft]);
            m_SR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.ShoulderRight], skel_last.Joints[JointType.ShoulderRight]);
            m_EL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.ElbowLeft], skel_last.Joints[JointType.ElbowLeft]);
            m_ER = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.ElbowRight], skel_last.Joints[JointType.ElbowRight]);
            m_WL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.WristLeft], skel_last.Joints[JointType.WristLeft]);
            m_WR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.WristRight], skel_last.Joints[JointType.WristRight]);
            m_HL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.HandLeft], skel_last.Joints[JointType.HandLeft]);
            m_HR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.HandRight], skel_last.Joints[JointType.HandRight]);
            m_Sp = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.Spine], skel_last.Joints[JointType.Spine]);
            m_HpC = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.HipCenter], skel_last.Joints[JointType.HipCenter]);
            m_HpL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.HipLeft], skel_last.Joints[JointType.HipLeft]);
            m_HpR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.HipRight], skel_last.Joints[JointType.HipRight]);
            m_KL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.KneeLeft], skel_last.Joints[JointType.KneeLeft]);
            m_KR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.KneeRight], skel_last.Joints[JointType.KneeRight]);
            m_AL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.AnkleLeft], skel_last.Joints[JointType.AnkleLeft]);
            m_AR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.AnkleRight], skel_last.Joints[JointType.AnkleRight]);
            m_FL = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.FootLeft], skel_last.Joints[JointType.FootLeft]);
            m_FR = TheTool.calEuclidian_2Joint(skel_current.Joints[JointType.FootRight], skel_last.Joints[JointType.FootRight]);
        }

        public void offline_calMove()
        {
            m_Head = TheTool.calEuclidian(offline_skel_current.Head, offline_skel_last.Head);
            m_SC = TheTool.calEuclidian(offline_skel_current.ShoulderCenter, offline_skel_last.ShoulderCenter);
            m_SL = TheTool.calEuclidian(offline_skel_current.ShoulderLeft, offline_skel_last.ShoulderLeft);
            m_SR = TheTool.calEuclidian(offline_skel_current.ShoulderRight, offline_skel_last.ShoulderRight);
            m_EL = TheTool.calEuclidian(offline_skel_current.ElbowLeft, offline_skel_last.ElbowLeft);
            m_ER = TheTool.calEuclidian(offline_skel_current.ElbowRight, offline_skel_last.ElbowRight);
            m_WL = TheTool.calEuclidian(offline_skel_current.WristLeft, offline_skel_last.WristLeft);
            m_WR = TheTool.calEuclidian(offline_skel_current.WristRight, offline_skel_last.WristRight);
            m_HL = TheTool.calEuclidian(offline_skel_current.HandLeft, offline_skel_last.HandLeft);
            m_HR = TheTool.calEuclidian(offline_skel_current.HandRight, offline_skel_last.HandRight);
            m_Sp = TheTool.calEuclidian(offline_skel_current.Spine, offline_skel_last.Spine);
            m_HpC = TheTool.calEuclidian(offline_skel_current.HipCenter, offline_skel_last.HipCenter);
            m_HpL = TheTool.calEuclidian(offline_skel_current.HipLeft, offline_skel_last.HipLeft);
            m_HpR = TheTool.calEuclidian(offline_skel_current.HipRight, offline_skel_last.HipRight);
            m_KL = TheTool.calEuclidian(offline_skel_current.KneeLeft, offline_skel_last.KneeLeft);
            m_KR = TheTool.calEuclidian(offline_skel_current.KneeRight, offline_skel_last.KneeRight);
            m_AL = TheTool.calEuclidian(offline_skel_current.AnkleLeft, offline_skel_last.AnkleLeft);
            m_AR = TheTool.calEuclidian(offline_skel_current.AnkleRight, offline_skel_last.AnkleRight);
            m_FL = TheTool.calEuclidian(offline_skel_current.FootLeft, offline_skel_last.FootLeft);
            m_FR = TheTool.calEuclidian(offline_skel_current.FootRight, offline_skel_last.FootRight);
        }

        public void calSum()
        {
            ms_all = 0;
            ms_hand = 0;
            ms_leg = 0;
            ms_core = 0;
            ms_all += m_Head;
            ms_all += m_SC;
            ms_all += m_SL;
            ms_all += m_SR;
            ms_all += m_EL;
            ms_all += m_ER;
            ms_all += m_WL;
            ms_all += m_WR;
            ms_all += m_HL;
            ms_all += m_HR;
            ms_all += m_Sp;
            ms_all += m_HpC;
            ms_all += m_HpL;
            ms_all += m_HpR;
            ms_all += m_KL;
            ms_all += m_KR;
            ms_all += m_AL;
            ms_all += m_AR;
            ms_all += m_FL;
            ms_all += m_FR;
            ms_hand += m_EL;
            ms_hand += m_ER;
            ms_hand += m_WL;
            ms_hand += m_WR;
            ms_hand += m_HL;
            ms_hand += m_HR;
            ms_leg += m_KL;
            ms_leg += m_KR;
            ms_leg += m_AL;
            ms_leg += m_AR;
            ms_leg += m_FL;
            ms_leg += m_FR;
            ms_core += m_Head;
            ms_core += m_SC;
            ms_core += m_Sp;
            ms_core += m_HpC;
        }

        public void calSumAvg()
        {
            ms_all_list.Add(ms_all);
            ms_hand_list.Add(ms_hand);
            ms_leg_list.Add(ms_leg);
            ms_core_list.Add(ms_core);
            ms_all_avg = TheTool.calAVG(ms_all_list);
            ms_hand_avg = TheTool.calAVG(ms_hand_list);
            ms_leg_avg = TheTool.calAVG(ms_leg_list);
            ms_core_avg = TheTool.calAVG(ms_core_list); 
            if (ms_all_list.Count >= mva_size_minus1) { ms_all_list.RemoveAt(0); }
            if (ms_hand_list.Count >= mva_size_minus1) { ms_hand_list.RemoveAt(0); }
            if (ms_leg_list.Count >= mva_size_minus1) { ms_leg_list.RemoveAt(0); }
            if (ms_core_list.Count >= mva_size_minus1) { ms_core_list.RemoveAt(0); }
        }

    }

}
