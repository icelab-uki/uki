using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace P_Tracker2
{
    class ThePostureCal
    {

        static double math_pi_180 = 180 / Math.PI;//Convert radian to Degree
        
        static double temp_deltaX = 0;
        static double temp_deltaY = 0;
        static double temp_deltaZ = 0;
        static double temp_deltaR = 0;

        //Degree = Arcos(delta? / deltaR)
        static public double calAngle_3D(SkeletonPoint joint1, SkeletonPoint joint2,int baseSide)
        {
            double targetDegree = 0;
            try
            {
                double[] j1 = { joint1.X, joint1.Y, joint1.Z };
                double[] j2 = { joint2.X, joint2.Y, joint2.Z };
                targetDegree = calAngle_3D(j1, j2, baseSide);
            }
            catch (Exception e) { TheSys.showError("Err cal3D: " + e.ToString(), true); }
            return targetDegree;
        }

        //Joint to double[3] >> 0 = x , 1 = y , 2 = z
        //baseside >> 0 = x , 1 = y, 2 = z
        static public double calAngle_3D(double[] joint1, double[] joint2, int baseSide)
        {
            double targetDegree = 0;
            try
            {
                temp_deltaX = joint1[0] - joint2[0];//Calculates distance from Points 
                temp_deltaY = joint1[1] - joint2[1];
                temp_deltaZ = joint1[2] - joint2[2];
                double DifferenceSquared = Math.Pow(temp_deltaX, 2) + Math.Pow(temp_deltaY, 2) + Math.Pow(temp_deltaZ, 2);
                temp_deltaR = Math.Sqrt(DifferenceSquared);
                //
                targetDegree = calAngle_3D_fromTemp(baseSide);
            }
            catch (Exception e) { TheSys.showError("Err cal3D_d: " + e.ToString(), true); }
            return targetDegree;
        }

        //Cascade Calculation
        static public double calAngle_3D_fromTemp(int baseSide)
        {
            double targetDegree = 0;
            try { 
                targetDegree = Math.Acos(getBaseSide(baseSide) / temp_deltaR); 
                targetDegree = targetDegree * math_pi_180; 
            }
            catch (Exception e) { TheSys.showError("Err cal3D_Temp: " + e.ToString(), true); }
            return targetDegree;
        }

        static public double getBaseSide(int bid)
        {
            if (bid == 1) { return temp_deltaY; }
            else if (bid == 2) { return temp_deltaZ; }
            else { return temp_deltaX; }
        }
        
        //*******************************************************************************************
        //***************** Angle Translation *******************************************************

        static public double calAngle_3D(SkeletonPoint joint1, SkeletonPoint joint2, int baseSide1, int baseSide2)
        {
            double targetDegree = 0;
            try
            {
                double[] j1 = { joint1.X, joint1.Y, joint1.Z };
                double[] j2 = { joint2.X, joint2.Y, joint2.Z };
                targetDegree = calAngle_3D_byDouble(j1, j2, baseSide1, baseSide2);
            }
            catch (Exception e) { TheSys.showError("Err cal3D: " + e.ToString(), true); }
            return targetDegree;
        }

        //Joint to double[3] >> 0 = x , 1 = y , 2 = z
        //baseside >> 0 = x , 1 = y, 2 = z
        static public double calAngle_3D_byDouble(double[] joint1, double[] joint2, int baseSide1, int baseSide2)
        {
            double targetDegree = 0;
            try
            {
                temp_deltaX = joint1[0] - joint2[0];
                temp_deltaY = joint1[1] - joint2[1];
                temp_deltaZ = joint1[2] - joint2[2];
                double DifferenceSquared = Math.Pow(temp_deltaX, 2) + Math.Pow(temp_deltaY, 2) + Math.Pow(temp_deltaZ, 2);
                temp_deltaR = Math.Sqrt(DifferenceSquared);
                //
                targetDegree = calAngle_3D_fromTemp(baseSide1, baseSide2);
            }
            catch (Exception e) { TheSys.showError("Err cal3D_d: " + e.ToString(), true); }
            return targetDegree;
        }

        //Cascade Calculation
        static public double calAngle_3D_fromTemp(int baseSide1, int baseSide2)
        {
            double targetDegree = 0;
            try
            {
                double delta1 = getBaseSide(baseSide1); double delta2 = getBaseSide(baseSide2);
                double div = Math.Pow(delta1, 2) + Math.Pow(delta2, 2); div = Math.Sqrt(div);//Algo#2
                targetDegree = Math.Acos(div / temp_deltaR);
                targetDegree = targetDegree * math_pi_180;
            }
            catch (Exception e) { TheSys.showError("Err cal3D_Temp: " + e.ToString(), true); }
            return targetDegree;
        }


        //==========================================
        //======= 3D ===============================
        /// Return the angle between 3 Joints
        /// http://stackoverflow.com/questions/12499602/body-joints-angle-using-kinect

        public static double calAngle_3Joints(Joint j1, Joint j2, Joint j3)
        {
            try
            {
                double Angulo = 0;
                double shrhX = j1.Position.X - j2.Position.X;
                double shrhY = j1.Position.Y - j2.Position.Y;
                double shrhZ = j1.Position.Z - j2.Position.Z;
                double hsl = vectorNorm(shrhX, shrhY, shrhZ);
                double unrhX = j3.Position.X - j2.Position.X;
                double unrhY = j3.Position.Y - j2.Position.Y;
                double unrhZ = j3.Position.Z - j2.Position.Z;
                double hul = vectorNorm(unrhX, unrhY, unrhZ);
                double mhshu = shrhX * unrhX + shrhY * unrhY + shrhZ * unrhZ;
                double x = mhshu / (hul * hsl);
                if (x != Double.NaN)
                {
                    if (-1 <= x && x <= 1)
                    {
                        double angleRad = Math.Acos(x);
                        Angulo = angleRad * (180.0 / Math.PI);
                    }
                    else
                        Angulo = 0;
                }
                else
                    Angulo = 0;
                return Angulo;
            }
            catch (Exception e) { TheSys.showError("Err cal3D_Temp: " + e.ToString(), true); }
            return 0;
        }

        public static double calAngle_3Points(double[] j1, double[] j2, double[] j3)
        {
            try
            {
                double Angulo = 0;
                double shrhX = j1[0] - j2[0];
                double shrhY = j1[1] - j2[1];
                double shrhZ = j1[2] - j2[2];
                double hsl = vectorNorm(shrhX, shrhY, shrhZ);
                double unrhX = j3[0] - j2[0];
                double unrhY = j3[1] - j2[1];
                double unrhZ = j3[2] - j2[2];
                double hul = vectorNorm(unrhX, unrhY, unrhZ);
                double mhshu = shrhX * unrhX + shrhY * unrhY + shrhZ * unrhZ;
                double x = mhshu / (hul * hsl);
                if (x != Double.NaN)
                {
                    if (-1 <= x && x <= 1)
                    {
                        double angleRad = Math.Acos(x);
                        Angulo = angleRad * (180.0 / Math.PI);
                    }
                    else
                        Angulo = 0;
                }
                else
                    Angulo = 0;
                return Angulo;
            }
            catch (Exception e) { TheSys.showError("Err cal3D_Temp: " + e.ToString(), true); }
            return 0;
        }

        public static double calAngle_3Joints(Skeleton s, JointType jt1, JointType jt2, JointType jt3)
        {
            try
            {
                double Angulo = 0;
                double shrhX = s.Joints[jt1].Position.X - s.Joints[jt2].Position.X;
                double shrhY = s.Joints[jt1].Position.Y - s.Joints[jt2].Position.Y;
                double shrhZ = s.Joints[jt1].Position.Z - s.Joints[jt2].Position.Z;
                double hsl = vectorNorm(shrhX, shrhY, shrhZ);
                double unrhX = s.Joints[jt3].Position.X - s.Joints[jt2].Position.X;
                double unrhY = s.Joints[jt3].Position.Y - s.Joints[jt2].Position.Y;
                double unrhZ = s.Joints[jt3].Position.Z - s.Joints[jt2].Position.Z;
                double hul = vectorNorm(unrhX, unrhY, unrhZ);
                double mhshu = shrhX * unrhX + shrhY * unrhY + shrhZ * unrhZ;
                double x = mhshu / (hul * hsl);
                if (x != Double.NaN)
                {
                    if (-1 <= x && x <= 1)
                    {
                        double angleRad = Math.Acos(x);
                        Angulo = angleRad * (180.0 / Math.PI);
                    }
                    else
                        Angulo = 0;
                }
                else
                    Angulo = 0;
                return Angulo;
            }
            catch (Exception e) { TheSys.showError("Err cal3D_Temp: " + e.ToString(), true); }
            return 0;
        }

        private static double vectorNorm(double x, double y, double z)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }

        
    }
}
