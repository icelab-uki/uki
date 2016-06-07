using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class CalSparse_XYZ
    {
        public enum JointValues
        {
            ShoulderRight = 1,
            ShoulderLeft,
            ShoulderCenter,
            Spine,
            HipRight,
            HipLeft,
            HipCenter,
            ElbowRight,
            ElbowLeft,
            WristRight,
            WristLeft,
            HandRight,
            HandLeft,
            KneeRight,
            KneeLeft,
            AnkleRight,
            AnkleLeft,
            FootRight,
            FootLeft,
            Head
        }


        /* This function summarize all Joint Vector from a given Joint Data Set.
         * jointData is the joints data set.
         */

        private static double[] SummarizeJointsVector(UKI_DataRaw jointData)
        {
            Type joint_data_type = jointData.GetType();
            double[] result = new double[3] { 0f, 0f, 0f };

            // To loop through the members
            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                if (info.FieldType == result.GetType())
                {
                    double[] vector_infos = new double[] { 0, 0, 0 };

                    vector_infos = info.GetValue(jointData) as double[];
                    result[0] += vector_infos[0];
                    result[1] += vector_infos[1];
                    result[2] += vector_infos[2];
                }
            }

            return (result);
        }


        /* This function summarize only head and spines Joint Vector from a given Joint Data Set.
         * jointData is the joints data set.
         */

        private static double[] SummarizeJointsVectorFromHeadAndTorso(UKI_DataRaw joint_data)
        {
            Type joint_data_type = joint_data.GetType();
            double[] result = new double[3] { 0f, 0f, 0f };
            string[] field_names = new string[] { "ShoulderCenter", "Spine", "HipCenter", "Head" };

            // To loop through the members
            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                // Console.WriteLine(info.Name);
                for (int i = 0; i < field_names.Length; i++)
                {
                    if (info.Name == field_names[i])
                    {

                        double[] vector_infos = new double[] { 0, 0, 0 };

                        vector_infos = info.GetValue(joint_data) as double[];
                        result[0] += vector_infos[0];
                        result[1] += vector_infos[1];
                        result[2] += vector_infos[2];
                    }
                }
            }

            return (result);
        }


        /* This function add result of final centroid caculation in the CSV file */

        private static string DisplayJointsVectorHeadAndTorsoToCentroid(UKI_DataRaw joint_data, double[] joint_sum_canonical, double[] joint_sum_current, double duration_frame_inverse)
        {
            string infos = "";
            Type joint_data_type = joint_data.GetType();
            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                if (info.FieldType == joint_sum_canonical.GetType())
                {
                    double[] xyz_joint_vector = info.GetValue(joint_data) as double[];
                    double normalized_x = SpineCentroidCalculation(xyz_joint_vector[0], joint_sum_canonical[0], joint_sum_current[0], duration_frame_inverse);
                    double normalized_y = SpineCentroidCalculation(xyz_joint_vector[1], joint_sum_canonical[1], joint_sum_current[1], duration_frame_inverse);
                    double normalized_z = SpineCentroidCalculation(xyz_joint_vector[2], joint_sum_canonical[2], joint_sum_current[2], duration_frame_inverse);

                    infos += normalized_x + "," + normalized_y + "," + normalized_z + ",";
                }
            }
            return (infos);
        }

        /* This function apply formula (14) to the given axis */

        private static double SpineCentroidCalculation(double joint_axis, double joint_sum_canonical_ht_axis, double joint_sum_current_ht_axis, double duration_frame_inverse)
        {
            return (joint_axis + duration_frame_inverse * (joint_sum_canonical_ht_axis - joint_sum_current_ht_axis));
        }


        /* This function add column infos on the CSV file */

        private static string DisplayColumnInfo(UKI_DataRaw jointData, Type type_of_members)
        {
            string infos = "    ,";
            Type joint_data_type = jointData.GetType();

            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                if (info.FieldType == type_of_members)
                    infos += info.Name + "_x," + info.Name + "_y," + info.Name + "_z,";
            }
            return (infos);
        }

        /* This function apply formula (12) to the given axis */

        private static double ShiftJointVectorToCentroid(double joint_axis, double joint_sum_axis, double duration_frame_inverse)
        {
            return (joint_axis - duration_frame_inverse * joint_sum_axis);
        }

        /* This function shifts joint such that the centroid is at the origin*/

        private static string DisplayJointsVectorToCentroid(UKI_DataRaw jointData, double[] joint_sum, double duration_frame_inverse,
                                                            UKI_DataRaw canonicalJointData)
        {
            string infos = "";
            Type joint_data_type = jointData.GetType();
            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                if (info.FieldType == joint_sum.GetType())
                {
                    double[] xyz_joint_vector = info.GetValue(jointData) as double[];
                    double normalized_x = ShiftJointVectorToCentroid(xyz_joint_vector[0], joint_sum[0], duration_frame_inverse);
                    double normalized_y = ShiftJointVectorToCentroid(xyz_joint_vector[1], joint_sum[1], duration_frame_inverse); ;
                    double normalized_z = ShiftJointVectorToCentroid(xyz_joint_vector[2], joint_sum[2], duration_frame_inverse); ;

                    infos += normalized_x + "," + normalized_y + "," + normalized_z + ",";
                }
            }
            return (infos);
        }

        /* This function create a RawData object from a string format as a CSV File using coma delimiter */

        public static UKI_DataRaw CreateSkeletonRawDataFromString(string data)
        {

            var normalized_skeleton_data = new UKI_DataRaw();
            double[] tmp_condition_type = new double[] { 0, 0, 0 };
            int i = 1;
            string[] xyz_vector_data = data.Split(',');

            normalized_skeleton_data.time = "";
            normalized_skeleton_data.id = 0;

            Type joint_data_type = normalized_skeleton_data.GetType();

            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                if (info.FieldType == tmp_condition_type.GetType())
                {
                    double[] xyz_joint_vector = new double[] { 0, 0, 0 };
                    xyz_joint_vector[0] = double.Parse(xyz_vector_data[i]);
                    xyz_joint_vector[1] = double.Parse(xyz_vector_data[i + 1]);
                    xyz_joint_vector[2] = double.Parse(xyz_vector_data[i + 2]);
                    i += 3;
                    info.SetValue(normalized_skeleton_data, xyz_joint_vector);
                }
            }

            return (normalized_skeleton_data);
        }

        /* This function give ids to Joints */

        private static Dictionary<int, double[]> FillSkeletonJointDictionary(UKI_DataRaw JointData)
        {
            var JointNumberDictionary = new Dictionary<int, double[]>();

            /* Center Body Parts from up to down*/
            JointNumberDictionary.Add(20, JointData.Head);
            JointNumberDictionary.Add(3, JointData.ShoulderCenter);
            JointNumberDictionary.Add(4, JointData.Spine);
            JointNumberDictionary.Add(7, JointData.HipCenter);

            /* Left Body Parts from up to down */
            JointNumberDictionary.Add(2, JointData.ShoulderLeft);
            JointNumberDictionary.Add(9, JointData.ElbowLeft);
            JointNumberDictionary.Add(11, JointData.WristLeft);
            JointNumberDictionary.Add(13, JointData.HandLeft);

            JointNumberDictionary.Add(6, JointData.HipLeft);
            JointNumberDictionary.Add(15, JointData.KneeLeft);
            JointNumberDictionary.Add(17, JointData.AnkleLeft);
            JointNumberDictionary.Add(19, JointData.FootLeft);

            /* Right Body Parts from up to down */
            JointNumberDictionary.Add(1, JointData.ShoulderRight);
            JointNumberDictionary.Add(8, JointData.ElbowRight);
            JointNumberDictionary.Add(10, JointData.WristRight);
            JointNumberDictionary.Add(12, JointData.HandRight);

            JointNumberDictionary.Add(5, JointData.HipRight);
            JointNumberDictionary.Add(14, JointData.KneeRight);
            JointNumberDictionary.Add(16, JointData.AnkleRight);
            JointNumberDictionary.Add(18, JointData.FootRight);

            return (JointNumberDictionary);
        }

        /* This function return the JointToJoint geodesic distance of the given axis */

        private static double JointToJointPath(UKI_DataRaw joint_data, int axis)
        {
            Dictionary<int, double[]> joint_ids = FillSkeletonJointDictionary(joint_data);
            double path_value;

            path_value = (joint_ids[(int)JointValues.Head][axis] - joint_ids[(int)JointValues.ShoulderCenter][axis])
                         + (joint_ids[(int)JointValues.ShoulderCenter][axis] - joint_ids[(int)JointValues.ShoulderLeft][axis])
                         + (joint_ids[(int)JointValues.ShoulderLeft][axis] - joint_ids[(int)JointValues.ElbowLeft][axis])
                         + (joint_ids[(int)JointValues.ElbowLeft][axis] - joint_ids[(int)JointValues.WristLeft][axis])
                         + (joint_ids[(int)JointValues.WristLeft][axis] - joint_ids[(int)JointValues.HandLeft][axis])
                         + (joint_ids[(int)JointValues.ShoulderCenter][axis] - joint_ids[(int)JointValues.ShoulderRight][axis])
                         + (joint_ids[(int)JointValues.ShoulderRight][axis] - joint_ids[(int)JointValues.ElbowRight][axis])
                         + (joint_ids[(int)JointValues.ElbowRight][axis] - joint_ids[(int)JointValues.WristRight][axis])
                         + (joint_ids[(int)JointValues.WristRight][axis] - joint_ids[(int)JointValues.HandRight][axis])
                         + (joint_ids[(int)JointValues.ShoulderCenter][axis] - joint_ids[(int)JointValues.Spine][axis])
                         + (joint_ids[(int)JointValues.Spine][axis] - joint_ids[(int)JointValues.HipCenter][axis])
                         + (joint_ids[(int)JointValues.HipCenter][axis] - joint_ids[(int)JointValues.HipLeft][axis])
                         + (joint_ids[(int)JointValues.HipLeft][axis] - joint_ids[(int)JointValues.KneeLeft][axis])
                         + (joint_ids[(int)JointValues.KneeLeft][axis] - joint_ids[(int)JointValues.AnkleLeft][axis])
                         + (joint_ids[(int)JointValues.AnkleLeft][axis] - joint_ids[(int)JointValues.FootLeft][axis])
                         + (joint_ids[(int)JointValues.HipCenter][axis] - joint_ids[(int)JointValues.HipRight][axis])
                         + (joint_ids[(int)JointValues.HipRight][axis] - joint_ids[(int)JointValues.KneeRight][axis])
                         + (joint_ids[(int)JointValues.KneeRight][axis] - joint_ids[(int)JointValues.AnkleRight][axis])
                         + (joint_ids[(int)JointValues.AnkleRight][axis] - joint_ids[(int)JointValues.FootRight][axis]);


            return (path_value);
        }

        /* This function return the XYZ_Vector of the total size of the given skeleton */

        private static double[] JointToJointSize(UKI_DataRaw joint_data)
        {


            double size_x;
            double size_y;
            double size_z;
            double[] xyz_size_vector = new double[] { 0, 0, 0 };

            size_x = JointToJointPath(joint_data, 0);
            size_y = JointToJointPath(joint_data, 1);
            size_z = JointToJointPath(joint_data, 2);

            xyz_size_vector[0] = size_x;
            xyz_size_vector[1] = size_y;
            xyz_size_vector[2] = size_z;

            return (xyz_size_vector);
        }

        /* This function apply formula (13) to the given axis */

        private static double RescaleJointsVector(double joint_axis, double canonical_size_axis, double current_size_axis)
        {
            return (joint_axis * (canonical_size_axis / current_size_axis));
        }

        /* This function add result of rescalling in the CSV file */

        private static string DisplayJointsVectorRescalling(UKI_DataRaw joint_data, double[] canonical_pose_vector, double[] current_pose_vector)
        {
            string infos = "";

            Type joint_data_type = joint_data.GetType();

            foreach (System.Reflection.FieldInfo info in joint_data_type.GetFields())
            {
                if (info.FieldType == canonical_pose_vector.GetType())
                {
                    double[] xyz_joint_vector = info.GetValue(joint_data) as double[];
                    double normalized_x = RescaleJointsVector(xyz_joint_vector[0], canonical_pose_vector[0], current_pose_vector[0]);
                    double normalized_y = RescaleJointsVector(xyz_joint_vector[1], canonical_pose_vector[1], current_pose_vector[1]);
                    double normalized_z = RescaleJointsVector(xyz_joint_vector[2], canonical_pose_vector[2], current_pose_vector[2]);

                    infos += normalized_x + "," + normalized_y + "," + normalized_z + ",";
                }
            }
            return (infos);
        }

        // "saveFolder" end with \
        public static void calSparse(List<UKI_DataRaw> list_extractPose, string saveFolder, string filename)
        {
            try
            {
                List<String> list_data_n1 = new List<String> { };
                List<String> list_data_n2 = new List<String> { };
                List<String> list_data_n3 = new List<String> { };

                UKI_DataRaw pose_canonical = null;
                UKI_DataRaw pose_current = null;

                list_data_n1.Add(DisplayColumnInfo(list_extractPose[0], list_extractPose[0].AnkleLeft.GetType()));
                list_data_n2.Add(DisplayColumnInfo(list_extractPose[0], list_extractPose[0].AnkleLeft.GetType()));
                list_data_n3.Add(DisplayColumnInfo(list_extractPose[0], list_extractPose[0].AnkleLeft.GetType()));

                int index_canonical = 0; int index_current = 0; String row_name;
                foreach (UKI_DataRaw r in list_extractPose)
                {
                    pose_current = r; index_current = r.id;
                    //----------------------------------------------------
                    String data = "";
                    if (pose_canonical != null)
                    {
                        row_name = index_canonical + " to " + index_current + ",";
                        double duration_frame_inverse = 1.0f / (pose_current.id - pose_canonical.id);
                        double[] joint_sum = SummarizeJointsVector(pose_current);
                        data = row_name + DisplayJointsVectorToCentroid(pose_current, joint_sum, duration_frame_inverse, pose_canonical);
                        list_data_n1.Add(data);

                        UKI_DataRaw joint_vector_s_1 = CreateSkeletonRawDataFromString(data);

                        double[] xyz_size_vector_current_pose = JointToJointSize(joint_vector_s_1);
                        double[] xyz_size_vector_canonical_pose = JointToJointSize(pose_canonical);

                        data = row_name + DisplayJointsVectorRescalling(joint_vector_s_1, xyz_size_vector_canonical_pose, xyz_size_vector_current_pose);
                        list_data_n2.Add(data);

                        UKI_DataRaw joint_vector_s_2 = CreateSkeletonRawDataFromString(data);

                        double[] joint_sum_canonical_ht = SummarizeJointsVectorFromHeadAndTorso(pose_canonical);
                        double[] joint_sum_current_ht = SummarizeJointsVectorFromHeadAndTorso(joint_vector_s_2);

                        data = row_name + DisplayJointsVectorHeadAndTorsoToCentroid(joint_vector_s_2, joint_sum_canonical_ht, joint_sum_current_ht, duration_frame_inverse);
                        list_data_n3.Add(data);
                    }
                    //----------------------------------------------------
                    pose_canonical = r; index_canonical = r.id;
                }
                TheTool.exportCSV_orTXT(saveFolder + "N1_" + filename + ".csv", list_data_n1, false);
                TheTool.exportCSV_orTXT(saveFolder + "N2_" + filename + ".csv", list_data_n2, false);
                TheTool.exportCSV_orTXT(saveFolder + "N3_" + filename + ".csv", list_data_n3, false);
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

    }
}
