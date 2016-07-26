using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class TheBVH
    {
        public static double[,] createRotationMatrix(double a_x_deg, double a_y_deg, double a_z_deg, string rotationOrder)
        {
            double[,] matrix_r = new double[4, 4];
            double a_x = a_x_deg * 0.01745329252;//to radian
            double a_y = a_y_deg * 0.01745329252;
            double a_z = a_z_deg * 0.01745329252;
            if (rotationOrder == "zyx")
            {
                return new double[,]{
                    {Math.Cos(a_y)*Math.Cos(a_z),
                        Math.Cos(a_z)*Math.Sin(a_x)*Math.Sin(a_y) - Math.Sin(a_z)*Math.Cos(a_x),
                        Math.Cos(a_z)*Math.Cos(a_x)*Math.Sin(a_y) + Math.Sin(a_z)*Math.Sin(a_x),
                       0 },
                     {Math.Cos(a_y)*Math.Sin(a_z),
                        Math.Sin(a_x)*Math.Sin(a_y)*Math.Sin(a_z) + Math.Cos(a_x)*Math.Cos(a_z),
                        Math.Cos(a_x)*Math.Sin(a_y)*Math.Sin(a_z) - Math.Sin(a_x)*Math.Cos(a_z),
                        0 },
                    {-Math.Sin(a_y),
                        Math.Cos(a_y)*Math.Sin(a_x),
                        Math.Cos(a_x)*Math.Cos(a_y),
                       0 },
                    {0,0,0,1}
                };
            }
            else
            {
                //---------------------------------------------------------------
                double[,] matrix_X = new double[,]{
                    {1,0,0,0},
                    {0, Math.Cos(a_x), -Math.Sin(a_x),0},
                    {0, Math.Sin(a_x), Math.Cos(a_x),0},
                    {0,0,0,1}
                };
                    double[,] matrix_Y = new double[,]{
                    {Math.Cos(a_y),0,Math.Sin(a_y),0},
                    {0, 1, 0,0},
                    {-Math.Sin(a_y), 0, Math.Cos(a_y),0},
                    {0,0,0,1}
                };
                    double[,] matrix_Z = new double[,]{
                    { Math.Cos(a_z), -Math.Sin(a_z),0,0},
                    { Math.Sin(a_z), Math.Cos(a_z), 0,0},
                    { 0,0,1,0},
                    {0,0,0,1}
                };
                if (rotationOrder == "zyx") { matrix_r = TheTool.Matrix_Multiply(matrix_Z, TheTool.Matrix_Multiply(matrix_Y, matrix_X)); }
                else if (rotationOrder == "xyz") { matrix_r = TheTool.Matrix_Multiply(matrix_X, TheTool.Matrix_Multiply(matrix_Y, matrix_Z)); }
                else if (rotationOrder == "xzy") { matrix_r = TheTool.Matrix_Multiply(matrix_X, TheTool.Matrix_Multiply(matrix_Z, matrix_Y)); }
                else if (rotationOrder == "yxz") { matrix_r = TheTool.Matrix_Multiply(matrix_Y, TheTool.Matrix_Multiply(matrix_X, matrix_Z)); }
                else if (rotationOrder == "yzx") { matrix_r = TheTool.Matrix_Multiply(matrix_Y, TheTool.Matrix_Multiply(matrix_Z, matrix_X)); }
                else { matrix_r = TheTool.Matrix_Multiply(matrix_Z, TheTool.Matrix_Multiply(matrix_X, matrix_Y)); }
                //TheTool.Matrix_print(matrix_r, 3);
            }
            return matrix_r;
        }

        public static MocapNode[] getInitialNode()
        {
            List<MocapNode> node_list = new List<MocapNode>();
            for (int i = 0; i < 38; i++) { node_list.Add(new MocapNode()); }
            MocapNode[] node_set = node_list.ToArray();
            node_set[0].setting(false,"HipCenter",null,0.00000,0.00000,0.00000);
            //
            node_set[1].setting(false,"LHipJoint",node_set[0],0,0,0);
            node_set[2].setting(false,"LeftUpLeg",node_set[1],1.36306,-1.79463,0.83929);
            node_set[3].setting(false,"LeftLeg",node_set[2],2.44811,-6.72613,0.00000);
            node_set[4].setting(false,"LeftFoot",node_set[3],2.56220,-7.03959,0.00000);
            node_set[5].setting(false,"LeftToeBase",node_set[4],0.15764,-0.43311,2.32255);
            node_set[6].setting(true,"End",node_set[5],0.00000,-0.00000,1.18966);
            //
            node_set[7].setting(false,"RHipJoint",node_set[0],0,0,0);
            node_set[8].setting(false,"RightUpLeg",node_set[7],-1.30552,-1.79463,0.83929);
            node_set[9].setting(false,"RightLeg",node_set[8],-2.54253,-6.98555,0.00000);
            node_set[10].setting(false,"RightFoot",node_set[9],-2.56826,-7.05623,0.00000);
            node_set[11].setting(false,"RightToeBase",node_set[10],-0.16473,-0.45259,2.36315);
            node_set[12].setting(true, "End", node_set[11], -0.00000, -0.00000, 1.21082);
            //
            node_set[13].setting(false,"LowerBack",node_set[0],0,0,0);
            node_set[14].setting(false,"Spine",node_set[13],0.02827,2.03559,-0.19338);
            node_set[15].setting(false,"Spine1",node_set[14],0.05672,2.04885,-0.04275);
            node_set[16].setting(false,"Neck",node_set[15],0,0,0);
            node_set[17].setting(false,"Neck1",node_set[16],-0.05417,1.74624,0.17202);
            node_set[18].setting(false,"Head",node_set[17],0.10407,1.76136,-0.12397);
            node_set[19].setting(true, "End", node_set[18], 0.03720, 1.77044, -0.06241);
            //
            node_set[20].setting(false,"LeftShoulder",node_set[15],0,0,0);
            node_set[21].setting(false,"LeftArm",node_set[20],3.36241,1.20089,-0.31121);
            node_set[22].setting(false,"LeftForeArm",node_set[21],4.98300,-0.00000,-0.00000);
            node_set[23].setting(false,"LeftHand",node_set[22],3.48356,-0.00000,-0.00000);
            node_set[24].setting(false,"LeftFingerBase",node_set[23],0,0,0);
            node_set[25].setting(false,"LeftHandIndex1",node_set[24],0.71526,-0.00000,-0.00000);
            node_set[26].setting(true, "End", node_set[25], 0.57666, -0.00000, -0.00000);
            node_set[27].setting(false, "LThumb", node_set[23], 0, 0, 0);
            node_set[28].setting(true, "End", node_set[27], 0.58547, -0.00000, 0.58547);
            //
            node_set[29].setting(false,"RightShoulder",node_set[15],0,0,0);
            node_set[30].setting(false,"RightArm",node_set[29],-3.13660,1.37405,-0.40465);
            node_set[31].setting(false,"RightForeArm",node_set[30],-5.24190,-0.00000,-0.00000);
            node_set[32].setting(false,"RightHand",node_set[31],-3.44417,-0.00000,-0.00000);
            node_set[33].setting(false,"RightFingerBase",node_set[32],-0.62253,-0.00000,-0.00000);
            node_set[34].setting(false,"RightHandIndex1",node_set[33],0,0,0);
            node_set[35].setting(true, "End", node_set[34], -0.50190, -0.00000, -0.00000);
            node_set[36].setting(false, "RThumb", node_set[32], 0, 0, 0);
            node_set[37].setting(true, "End", node_set[36], -0.50956, -0.00000, 0.50956);
            return node_set;
        }

        public static void MocapNode_loadXYZ(ref UKI_DataRaw raw, MocapNode[] node_set)
        {
            MocapNode_loadXYZ_sub(ref raw.HipCenter, ref node_set[0]);
            MocapNode_loadXYZ_sub(ref raw.HipLeft, ref node_set[2]);
            MocapNode_loadXYZ_sub(ref raw.KneeLeft, ref node_set[3]);
            MocapNode_loadXYZ_sub(ref raw.AnkleLeft, ref node_set[4]);
            MocapNode_loadXYZ_sub(ref raw.FootLeft, ref node_set[5]);
            MocapNode_loadXYZ_sub(ref raw.HipRight, ref node_set[8]);
            MocapNode_loadXYZ_sub(ref raw.KneeRight, ref node_set[9]);
            MocapNode_loadXYZ_sub(ref raw.AnkleRight, ref node_set[10]);
            MocapNode_loadXYZ_sub(ref raw.FootRight, ref node_set[11]);
            //
            MocapNode_loadXYZ_sub(ref raw.Spine, ref node_set[14]);
            MocapNode_loadXYZ_sub(ref raw.ShoulderCenter, ref node_set[17]);
            MocapNode_loadXYZ_sub(ref raw.Head, ref node_set[19]);
            //
            MocapNode_loadXYZ_sub(ref raw.ShoulderLeft, ref node_set[21]);
            MocapNode_loadXYZ_sub(ref raw.ElbowLeft, ref node_set[22]);
            MocapNode_loadXYZ_sub(ref raw.WristLeft, ref node_set[23]);
            MocapNode_loadXYZ_sub(ref raw.HandLeft, ref node_set[26]);
            MocapNode_loadXYZ_sub(ref raw.ShoulderRight, ref node_set[30]);
            MocapNode_loadXYZ_sub(ref raw.ElbowRight, ref node_set[31]);
            MocapNode_loadXYZ_sub(ref raw.WristRight, ref node_set[32]);
            MocapNode_loadXYZ_sub(ref raw.HandRight, ref node_set[35]);
        }

        public static void MocapNode_loadXYZ_sub(ref double[] j, ref MocapNode n)
        {
            j[0] = n.x;
            j[1] = n.y; 
            j[2] = n.z;
        }

    }

}
