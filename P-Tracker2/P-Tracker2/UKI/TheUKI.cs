using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace P_Tracker2
{
    class TheUKI
    {

        public static List<String> joint_list = new List<String>{ 
            "Head","ShoulderCenter",
            "ShoulderLeft","ShoulderRight"
            ,"ElbowLeft","ElbowRight"
            ,"WristLeft","WristRight"
            ,"HandLeft","HandRight",
            "Spine","HipCenter",
            "HipLeft","HipRight",
            "KneeLeft","KneeRight",
            "AnkleLeft","AnkleRight",
            "FootLeft","FootRight"
            };

        public static List<List<String>> list_bodyGroup = new List<List<String>>
        {
            new List<String>{"ElbowLeft","WristLeft","HandLeft"},
            new List<String>{"ElbowRight","WristRight","HandRight"},
            new List<String>{"KneeLeft","AnkleLeft","FootLeft"},
            new List<String>{"KneeRight","AnkleRight","FootRight"}
            };

        public static List<List<String>> list_bodyGroup_full = new List<List<String>>
        {
            new List<String>{"ElbowLeft_SC_x","WristLeft_SC_x","HandLeft_SC_x"},
            new List<String>{"ElbowLeft_SC_y","WristLeft_SC_y","HandLeft_SC_y"},
            new List<String>{"ElbowLeft_SC_z","WristLeft_SC_z","HandLeft_SC_z"},
            new List<String>{"ElbowRight_SC_x","WristRight_SC_x","HandRight_SC_x"},
            new List<String>{"ElbowRight_SC_y","WristRight_SC_y","HandRight_SC_y"},
            new List<String>{"ElbowRight_SC_z","WristRight_SC_z","HandRight_SC_z"},
            new List<String>{"KneeLeft_HC_x","AnkleLeft_HC_x","FootLeft_HC_x"},
            new List<String>{"KneeLeft_HC_y","AnkleLeft_HC_y","FootLeft_HC_y"},
            new List<String>{"KneeLeft_HC_z","AnkleLeft_HC_z","FootLeft_HC_z"},
            new List<String>{"KneeRight_HC_x","AnkleRight_HC_x","FootRight_HC_x"},
            new List<String>{"KneeRight_HC_y","AnkleRight_HC_y","FootRight_HC_y"},
            new List<String>{"KneeRight_HC_z","AnkleRight_HC_z","FootRight_HC_z"}
            };

        public static string data_raw_Header = "id,time," + TheUKI.getHeader_20Joint(new List<String> { "_x", "_y", "_z" });
        public static string data_ang_Header = "id,time," + TheUKI.getHeader_20Joint(new List<String> { "_Ap", "_Aa" });

        public static string data_raw_centered_Header(int method_id)
        {
            if (method_id == TheUKI.angTechq_SC_HC)
            {
                String s = "id,time";
                int i = 0;
                foreach (String j in joint_list)
                {
                    if (i < 10 && j != "ShoulderCenter") { s += "," + j + "_SC_x," + j + "_SC_y," + j + "_SC_z"; }//upper body
                    else { s += "," + j + "_HC_x," + j + "_HC_y," + j + "_HC_z"; }//upper body
                    i++;
                }
                return s;
            }
            else
            {
                //1 Center
                return "id,time," + TheUKI.getHeader_20Joint(new List<String> { 
                    "_" + TheUKI.getCenterTechqName(method_id) +"_x", "_y", "_z" 
                });
            }
        }

        public static string getHeader_20Joint(String suffix)
        {
            String output = "";
            int i = 0;
            foreach (String j in joint_list)
            {
                if (i > 0) { output += ","; }
                output += j + suffix;
                i++;
            }
            return output;
        }

        //Input: suffix = {_X,_Y,_Z}
        //Output: Head_X,Head_Y,Head_Z
        public static string getHeader_20Joint(List<String> suffix)
        {
            String output = "";
            int i = 0;
            foreach (String j in joint_list)
            {
                foreach (String s in suffix)
                {
                    if (i > 0) { output += ","; }
                    output += j + s;
                    i++;
                }
            }
            return output;
        }

        //*******************************************************************************************************
        //*******************************************************************************************************

        //=====================================================

        //method 0 = center at Spine
        //method 1 = center at HipCenter
        //method 2 = Shoulder Center & Hip Center
        public static List<UKI_DataAngular> calAngle_fromRaw(List<UKI_DataRaw> list_raw, int method)
        {
            List<UKI_DataAngular> list_ang = new List<UKI_DataAngular>();
            foreach (UKI_DataRaw r in list_raw) { list_ang.Add(calAngle_fromRaw(r, method)); }
            return list_ang;
        }

        public static UKI_DataAngular calAngle_fromRaw(UKI_DataRaw data_raw, int method)
        {
            UKI_DataAngular data_ang = new UKI_DataAngular();
            double[] center = data_raw.HipCenter;
            if (method == 1) { center = data_raw.Spine; }
            else if (method == 2) { center = data_raw.ShoulderCenter; }
            data_ang.id = data_raw.id;
            data_ang.time = data_raw.time;
            data_ang.Head = calSpherical_toCenter(data_raw.Head, center);
            data_ang.Head = calSpherical_toCenter(data_raw.Head, center);
            data_ang.ShoulderCenter = calSpherical_toCenter(data_raw.ShoulderCenter, center);
            data_ang.ShoulderLeft = calSpherical_toCenter(data_raw.ShoulderLeft, center);
            data_ang.ShoulderRight = calSpherical_toCenter(data_raw.ShoulderRight, center);
            data_ang.ElbowLeft = calSpherical_toCenter(data_raw.ElbowLeft, center);
            data_ang.ElbowRight = calSpherical_toCenter(data_raw.ElbowRight, center);
            data_ang.WristLeft = calSpherical_toCenter(data_raw.WristLeft, center);
            data_ang.WristRight = calSpherical_toCenter(data_raw.WristRight, center);
            data_ang.HandLeft = calSpherical_toCenter(data_raw.HandLeft, center);
            data_ang.HandRight = calSpherical_toCenter(data_raw.HandRight, center);
            data_ang.Spine = calSpherical_toCenter(data_raw.Spine, center);
            if (method == 2) { center = data_raw.HipCenter; }
            data_ang.HipCenter = calSpherical_toCenter(data_raw.HipCenter, center);
            data_ang.HipLeft = calSpherical_toCenter(data_raw.HipLeft, center);
            data_ang.HipRight = calSpherical_toCenter(data_raw.HipRight, center);
            data_ang.KneeLeft = calSpherical_toCenter(data_raw.KneeLeft, center);
            data_ang.KneeRight = calSpherical_toCenter(data_raw.KneeRight, center);
            data_ang.AnkleLeft = calSpherical_toCenter(data_raw.AnkleLeft, center);
            data_ang.AnkleRight = calSpherical_toCenter(data_raw.AnkleRight, center);
            data_ang.FootLeft = calSpherical_toCenter(data_raw.FootLeft, center);
            data_ang.FootRight = calSpherical_toCenter(data_raw.FootRight, center);
            return data_ang;
        }

        //j2 is center
        public static double[] calSpherical_toCenter(double[] target, double[] origin)
        {
            double[] output = new double[2];
            try
            {
                if (target != origin)
                {
                    double a_polar = 0;
                    double a_azimuth = 0;
                    //--------------------------
                    double x = target[0] - origin[0];
                    double y = target[1] - origin[1];
                    double z = target[2] - origin[2];
                    double r = Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2);
                    r = Math.Sqrt(r);
                    a_polar = Math.Acos(y / r);
                    a_polar = TheTool.RadianToDegree(a_polar);
                    a_azimuth = Math.Atan(x / z);
                    a_azimuth = TheTool.RadianToDegree(a_azimuth);
                    output[0] = a_azimuth;
                    output[1] = a_polar;
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return output;
        }

        //=====================================================

        public static void saveData_Raw(string filepath, List<UKI_DataRaw> data)
        {
            saveData_Raw_specifyHeader(filepath, data, TheUKI.data_raw_Header);
        }

        public static void saveData_Raw_centered(string filepath, List<UKI_DataRaw> data, int center_techq)
        {
            saveData_Raw_specifyHeader(filepath, data, TheUKI.data_raw_centered_Header(center_techq));
        }

        static void saveData_Raw_specifyHeader(string filepath, List<UKI_DataRaw> data, string header)
        {
            List<string> listData = new List<string> { };
            if (data.Count > 0)
            {
                listData.Add(header);
                foreach (UKI_DataRaw r in data) { listData.Add(rawdata_getRow(r)); }
            }
            TheTool.exportCSV_orTXT(filepath, listData, false);
        }


        public static void DataRaw_AddExtraFeature(List<UKI_DataRaw> list_raw)
        {
            if (list_raw.Count > 0)
            {
                double Spine_Y = list_raw.First().Spine[1];
                double LKnee_Y = list_raw.First().KneeLeft[1];
                double RKnee_Y = list_raw.First().KneeRight[1];
                //first_raw.
                foreach (UKI_DataRaw raw in list_raw)
                {
                    raw.Spine_Y = raw.Spine[1] - Spine_Y;
                    raw.LKnee_Y = raw.KneeLeft[1] - LKnee_Y;
                    raw.RKnee_Y = raw.KneeRight[1] - RKnee_Y;
                }
            }
        }

        //TheUKI.centerTechq_SC_HC
        public static DataTable convert_UKI_DataRaw_toDataTable(List<UKI_DataRaw> list_raw, int center_techq, Boolean extraColumn){
            DataTable dt = new DataTable();
            //-----
            string col_str = TheUKI.data_raw_centered_Header(center_techq);
            string[] col_set = TheTool.splitText(col_str, ",");
            for (int i = 0; i < col_set.Count(); i++) { dt.Columns.Add(col_set[i]); }
            if (extraColumn) { 
                dt.Columns.Add("r_sp_Y");
                dt.Columns.Add("r_kL_Y");
                dt.Columns.Add("r_kR_Y");
            }
            try
            {
                if (dt.Columns.Count >= 62)
                {
                    int i = 0;
                    foreach (UKI_DataRaw raw in list_raw)
                    {
                        dt.Rows.Add();
                        dt.Rows[i][0] = raw.id;
                        dt.Rows[i][1] = raw.time;
                        dt.Rows[i][2] = raw.Head[0];
                        dt.Rows[i][3] = raw.Head[1];
                        dt.Rows[i][4] = raw.Head[2];
                        dt.Rows[i][5] = raw.ShoulderCenter[0];
                        dt.Rows[i][6] = raw.ShoulderCenter[1];
                        dt.Rows[i][7] = raw.ShoulderCenter[2];
                        dt.Rows[i][8] = raw.ShoulderLeft[0];
                        dt.Rows[i][9] = raw.ShoulderLeft[1];
                        dt.Rows[i][10] = raw.ShoulderLeft[2];
                        dt.Rows[i][11] = raw.ShoulderRight[0];
                        dt.Rows[i][12] = raw.ShoulderRight[1];
                        dt.Rows[i][13] = raw.ShoulderRight[2];
                        dt.Rows[i][14] = raw.ElbowLeft[0];
                        dt.Rows[i][15] = raw.ElbowLeft[1];
                        dt.Rows[i][16] = raw.ElbowLeft[2];
                        dt.Rows[i][17] = raw.ElbowRight[0];
                        dt.Rows[i][18] = raw.ElbowRight[1];
                        dt.Rows[i][19] = raw.ElbowRight[2];
                        dt.Rows[i][20] = raw.WristLeft[0];
                        dt.Rows[i][21] = raw.WristLeft[1];
                        dt.Rows[i][22] = raw.WristLeft[2];
                        dt.Rows[i][23] = raw.WristRight[0];
                        dt.Rows[i][24] = raw.WristRight[1];
                        dt.Rows[i][25] = raw.WristRight[2];
                        dt.Rows[i][26] = raw.HandLeft[0];
                        dt.Rows[i][27] = raw.HandLeft[1];
                        dt.Rows[i][28] = raw.HandLeft[2];
                        dt.Rows[i][29] = raw.HandRight[0];
                        dt.Rows[i][30] = raw.HandRight[1];
                        dt.Rows[i][31] = raw.HandRight[2];
                        dt.Rows[i][32] = raw.Spine[0];
                        dt.Rows[i][33] = raw.Spine[1];
                        dt.Rows[i][34] = raw.Spine[2];
                        dt.Rows[i][35] = raw.HipCenter[0];
                        dt.Rows[i][36] = raw.HipCenter[1];
                        dt.Rows[i][37] = raw.HipCenter[2];
                        dt.Rows[i][38] = raw.HipLeft[0];
                        dt.Rows[i][39] = raw.HipLeft[1];
                        dt.Rows[i][40] = raw.HipLeft[2];
                        dt.Rows[i][41] = raw.HipRight[0];
                        dt.Rows[i][42] = raw.HipRight[1];
                        dt.Rows[i][43] = raw.HipRight[2];
                        dt.Rows[i][44] = raw.KneeLeft[0];
                        dt.Rows[i][45] = raw.KneeLeft[1];
                        dt.Rows[i][46] = raw.KneeLeft[2];
                        dt.Rows[i][47] = raw.KneeRight[0];
                        dt.Rows[i][48] = raw.KneeRight[1];
                        dt.Rows[i][49] = raw.KneeRight[2];
                        dt.Rows[i][50] = raw.AnkleLeft[0];
                        dt.Rows[i][51] = raw.AnkleLeft[1];
                        dt.Rows[i][52] = raw.AnkleLeft[2];
                        dt.Rows[i][53] = raw.AnkleRight[0];
                        dt.Rows[i][54] = raw.AnkleRight[1];
                        dt.Rows[i][55] = raw.AnkleRight[2];
                        dt.Rows[i][56] = raw.FootLeft[0];
                        dt.Rows[i][57] = raw.FootLeft[1];
                        dt.Rows[i][58] = raw.FootLeft[2];
                        dt.Rows[i][59] = raw.FootRight[0];
                        dt.Rows[i][60] = raw.FootRight[1];
                        dt.Rows[i][61] = raw.FootRight[2];
                        if (extraColumn)
                        {
                            dt.Rows[i][62] = raw.Spine_Y;
                            dt.Rows[i][63] = raw.LKnee_Y;
                            dt.Rows[i][64] = raw.RKnee_Y;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex);}
            return dt;
        }

        public static void saveData_Ang(string filepath, List<UKI_DataAngular> data)
        {
            List<string> listData = new List<string> { };
            if (data.Count > 0)
            {
                listData.Add(TheUKI.data_ang_Header);
                foreach (UKI_DataAngular a in data) { listData.Add(angledata_getRow(a)); }
            }
            TheTool.exportCSV_orTXT(filepath, listData, false);
        }

        public static string rawdata_getRow(UKI_DataRaw r)
        {
            string data = r.id + "," + r.time + ",";
            data += r.Head[0] + "," + r.Head[1] + "," + r.Head[2] + ",";
            data += r.ShoulderCenter[0] + "," + r.ShoulderCenter[1] + "," + r.ShoulderCenter[2] + ",";
            data += r.ShoulderLeft[0] + "," + r.ShoulderLeft[1] + "," + r.ShoulderLeft[2] + ",";
            data += r.ShoulderRight[0] + "," + r.ShoulderRight[1] + "," + r.ShoulderRight[2] + ",";
            data += r.ElbowLeft[0] + "," + r.ElbowLeft[1] + "," + r.ElbowLeft[2] + ",";
            data += r.ElbowRight[0] + "," + r.ElbowRight[1] + "," + r.ElbowRight[2] + ",";
            data += r.WristLeft[0] + "," + r.WristLeft[1] + "," + r.WristLeft[2] + ",";
            data += r.WristRight[0] + "," + r.WristRight[1] + "," + r.WristRight[2] + ",";
            data += r.HandLeft[0] + "," + r.HandLeft[1] + "," + r.HandLeft[2] + ",";
            data += r.HandRight[0] + "," + r.HandRight[1] + "," + r.HandRight[2] + ",";
            data += r.Spine[0] + "," + r.Spine[1] + "," + r.Spine[2] + ",";
            data += r.HipCenter[0] + "," + r.HipCenter[1] + "," + r.HipCenter[2] + ",";
            data += r.HipLeft[0] + "," + r.HipLeft[1] + "," + r.HipLeft[2] + ",";
            data += r.HipRight[0] + "," + r.HipRight[1] + "," + r.HipRight[2] + ",";
            data += r.KneeLeft[0] + "," + r.KneeLeft[1] + "," + r.KneeLeft[2] + ",";
            data += r.KneeRight[0] + "," + r.KneeRight[1] + "," + r.KneeRight[2] + ",";
            data += r.AnkleLeft[0] + "," + r.AnkleLeft[1] + "," + r.AnkleLeft[2] + ",";
            data += r.AnkleRight[0] + "," + r.AnkleRight[1] + "," + r.AnkleRight[2] + ",";
            data += r.FootLeft[0] + "," + r.FootLeft[1] + "," + r.FootLeft[2] + ",";
            data += r.FootRight[0] + "," + r.FootRight[1] + "," + r.FootRight[2];
            return data;
        }

        public static string angledata_getRow(UKI_DataAngular a)
        {
            string data = a.id + "," + a.time + ",";
            data += a.Head[0] + "," + a.Head[1] + ",";
            data += a.ShoulderCenter[0] + "," + a.ShoulderCenter[1] + ",";
            data += a.ShoulderLeft[0] + "," + a.ShoulderLeft[1] + ",";
            data += a.ShoulderRight[0] + "," + a.ShoulderRight[1] + ",";
            data += a.ElbowLeft[0] + "," + a.ElbowLeft[1] + ",";
            data += a.ElbowRight[0] + "," + a.ElbowRight[1] + ",";
            data += a.WristLeft[0] + "," + a.WristLeft[1] + ",";
            data += a.WristRight[0] + "," + a.WristRight[1] + ",";
            data += a.HandLeft[0] + "," + a.HandLeft[1] + ",";
            data += a.HandRight[0] + "," + a.HandRight[1] + ",";
            data += a.Spine[0] + "," + a.Spine[1] + ",";
            data += a.HipCenter[0] + "," + a.HipCenter[1] + ",";
            data += a.HipLeft[0] + "," + a.HipLeft[1] + ",";
            data += a.HipRight[0] + "," + a.HipRight[1] + ",";
            data += a.KneeLeft[0] + "," + a.KneeLeft[1] + ",";
            data += a.KneeRight[0] + "," + a.KneeRight[1] + ",";
            data += a.AnkleLeft[0] + "," + a.AnkleLeft[1] + ",";
            data += a.AnkleRight[0] + "," + a.AnkleRight[1] + ",";
            data += a.FootLeft[0] + "," + a.FootLeft[1] + ",";
            data += a.FootRight[0] + "," + a.FootRight[1];
            return data;
        }

        public static string get_UKI_DataRaw_String(UKI_DataRaw_String d)
        {
            return d.id + d.time + d.Head
                + d.ShoulderCenter + d.ShoulderLeft + d.ShoulderRight
                + d.ElbowLeft + d.ElbowRight + d.WristLeft + d.WristRight
                + d.HandLeft + d.HandRight
                + d.Spine + d.HipCenter
                + d.HipLeft + d.HipRight + d.KneeLeft + d.KneeRight
                + d.AnkleLeft + d.AnkleRight + d.FootLeft + d.FootRight;
        }

        public static List<string> get_UKI_DataRaw_String_List(List<UKI_DataRaw> list)
        {
            List<string> output = new List<string>();
            foreach (UKI_DataRaw d in list)
            {
                output.Add(get_UKI_DataRaw_String(d));
            }
            return output;
        }

        public static string get_UKI_DataRaw_String(UKI_DataRaw d)
        {
            return d.id + "," + d.time + "," +
                d.Head[0] + "," + d.Head[1] + "," + d.Head[2] + "," +
                d.ShoulderCenter[0] + "," + d.ShoulderCenter[1] + "," + d.ShoulderCenter[2] + "," +
                d.ShoulderLeft[0] + "," + d.ShoulderLeft[1] + "," + d.ShoulderLeft[2] + "," +
                d.ShoulderRight[0] + "," + d.ShoulderRight[1] + "," + d.ShoulderRight[2] + "," +
                d.ElbowLeft[0] + "," + d.ElbowLeft[1] + "," + d.ElbowLeft[2] + "," +
                d.ElbowRight[0] + "," + d.ElbowRight[1] + "," + d.ElbowRight[2] + "," +
                d.WristLeft[0] + "," + d.WristLeft[1] + "," + d.WristLeft[2] + "," +
                d.WristRight[0] + "," + d.WristRight[1] + "," + d.WristRight[2] + "," +
                d.HandLeft[0] + "," + d.HandLeft[1] + "," + d.HandLeft[2] + "," +
                d.HandRight[0] + "," + d.HandRight[1] + "," + d.HandRight[2] + "," +
                d.Spine[0] + "," + d.Spine[1] + "," + d.Spine[2] + "," +
                d.HipCenter[0] + "," + d.HipCenter[1] + "," + d.HipCenter[2] + "," +
                d.HipLeft[0] + "," + d.HipLeft[1] + "," + d.HipLeft[2] + "," +
                d.HipRight[0] + "," + d.HipRight[1] + "," + d.HipRight[2] + "," +
                d.KneeLeft[0] + "," + d.KneeLeft[1] + "," + d.KneeLeft[2] + "," +
                d.KneeRight[0] + "," + d.KneeRight[1] + "," + d.KneeRight[2] + "," +
                d.AnkleLeft[0] + "," + d.AnkleLeft[1] + "," + d.AnkleLeft[2] + "," +
                d.AnkleRight[0] + "," + d.AnkleRight[1] + "," + d.AnkleRight[2] + "," +
                d.FootLeft[0] + "," + d.FootLeft[1] + "," + d.FootLeft[2] + "," +
                d.FootRight[0] + "," + d.FootRight[1] + "," + d.FootRight[2];
        }

        public static List<UKI_DataAngular> csv_loadFileTo_DataAngular(String path)
        {
            List<UKI_DataAngular> data_list = new List<UKI_DataAngular>();
            try
            {
                DataTable dt = CSVReader.ReadCSVFile(path, true);
                if (dt.Columns.Count > 41)
                {
                    UKI_DataAngular data;
                    foreach (DataRow dr in dt.Rows)
                    {
                        data = new UKI_DataAngular();
                        data.id = TheTool.getInt(dr[0].ToString());
                        data.time = dr[1].ToString();
                        data.Head = new double[2] { TheTool.getDouble(dr[2].ToString()), TheTool.getDouble(dr[3].ToString()) };
                        data.ShoulderCenter = new double[2] { TheTool.getDouble(dr[4].ToString()), TheTool.getDouble(dr[5].ToString()) };
                        data.ShoulderLeft = new double[2] { TheTool.getDouble(dr[6].ToString()), TheTool.getDouble(dr[7].ToString()) };
                        data.ShoulderRight = new double[2] { TheTool.getDouble(dr[8].ToString()), TheTool.getDouble(dr[9].ToString()) };
                        data.ElbowLeft = new double[2] { TheTool.getDouble(dr[10].ToString()), TheTool.getDouble(dr[11].ToString()) };
                        data.ElbowRight = new double[2] { TheTool.getDouble(dr[12].ToString()), TheTool.getDouble(dr[13].ToString()) };
                        data.WristLeft = new double[2] { TheTool.getDouble(dr[14].ToString()), TheTool.getDouble(dr[15].ToString()) };
                        data.WristRight = new double[2] { TheTool.getDouble(dr[16].ToString()), TheTool.getDouble(dr[17].ToString()) };
                        data.HandLeft = new double[2] { TheTool.getDouble(dr[18].ToString()), TheTool.getDouble(dr[19].ToString()) };
                        data.HandRight = new double[2] { TheTool.getDouble(dr[20].ToString()), TheTool.getDouble(dr[21].ToString()) };
                        data.Spine = new double[2] { TheTool.getDouble(dr[22].ToString()), TheTool.getDouble(dr[23].ToString()) };
                        data.HipCenter = new double[2] { TheTool.getDouble(dr[24].ToString()), TheTool.getDouble(dr[25].ToString()) };
                        data.HipLeft = new double[2] { TheTool.getDouble(dr[26].ToString()), TheTool.getDouble(dr[27].ToString()) };
                        data.HipRight = new double[2] { TheTool.getDouble(dr[28].ToString()), TheTool.getDouble(dr[29].ToString()) };
                        data.KneeLeft = new double[2] { TheTool.getDouble(dr[30].ToString()), TheTool.getDouble(dr[31].ToString()) };
                        data.KneeRight = new double[2] { TheTool.getDouble(dr[32].ToString()), TheTool.getDouble(dr[33].ToString()) };
                        data.AnkleLeft = new double[2] { TheTool.getDouble(dr[34].ToString()), TheTool.getDouble(dr[35].ToString()) };
                        data.AnkleRight = new double[2] { TheTool.getDouble(dr[36].ToString()), TheTool.getDouble(dr[37].ToString()) };
                        data.FootLeft = new double[2] { TheTool.getDouble(dr[38].ToString()), TheTool.getDouble(dr[39].ToString()) };
                        data.FootRight = new double[2] { TheTool.getDouble(dr[40].ToString()), TheTool.getDouble(dr[41].ToString()) };
                        data_list.Add(data);
                    }
                }
                else { TheSys.showError(path + ": col < 42"); }
            }
            catch (Exception ex) { TheSys.showError(path + ": " + ex); }
            return data_list;
        }

        //Faster ver
        public static List<UKI_DataRaw> csv_loadFileTo_DataRaw(String path, int skip)
        {
            //TheSys.showTime();
            List<UKI_DataRaw> data_list = new List<UKI_DataRaw>();
            try
            {
                List<string> data_origin = TheTool.read_File_getListString(path);
                int i_row = 0;
                foreach (string row in data_origin)
                {
                    if (i_row >= skip + 1)
                    {
                        string[] dr = TheTool.splitText(row, ",");
                        UKI_DataRaw data = new UKI_DataRaw();
                        data.id = TheTool.getInt(dr[0].ToString());
                        data.time = dr[1].ToString();
                        data.Head = new double[3] { TheTool.getDouble(dr[2].ToString()), TheTool.getDouble(dr[3].ToString()), TheTool.getDouble(dr[4].ToString()) };
                        data.ShoulderCenter = new double[3] { TheTool.getDouble(dr[5].ToString()), TheTool.getDouble(dr[6].ToString()), TheTool.getDouble(dr[7].ToString()) };
                        data.ShoulderLeft = new double[3] { TheTool.getDouble(dr[8].ToString()), TheTool.getDouble(dr[9].ToString()), TheTool.getDouble(dr[10].ToString()) };
                        data.ShoulderRight = new double[3] { TheTool.getDouble(dr[11].ToString()), TheTool.getDouble(dr[12].ToString()), TheTool.getDouble(dr[13].ToString()) };
                        data.ElbowLeft = new double[3] { TheTool.getDouble(dr[14].ToString()), TheTool.getDouble(dr[15].ToString()), TheTool.getDouble(dr[16].ToString()) };
                        data.ElbowRight = new double[3] { TheTool.getDouble(dr[17].ToString()), TheTool.getDouble(dr[18].ToString()), TheTool.getDouble(dr[19].ToString()) };
                        data.WristLeft = new double[3] { TheTool.getDouble(dr[20].ToString()), TheTool.getDouble(dr[21].ToString()), TheTool.getDouble(dr[22].ToString()) };
                        data.WristRight = new double[3] { TheTool.getDouble(dr[23].ToString()), TheTool.getDouble(dr[24].ToString()), TheTool.getDouble(dr[25].ToString()) };
                        data.HandLeft = new double[3] { TheTool.getDouble(dr[26].ToString()), TheTool.getDouble(dr[27].ToString()), TheTool.getDouble(dr[28].ToString()) };
                        data.HandRight = new double[3] { TheTool.getDouble(dr[29].ToString()), TheTool.getDouble(dr[30].ToString()), TheTool.getDouble(dr[31].ToString()) };
                        data.Spine = new double[3] { TheTool.getDouble(dr[32].ToString()), TheTool.getDouble(dr[33].ToString()), TheTool.getDouble(dr[34].ToString()) };
                        data.HipCenter = new double[3] { TheTool.getDouble(dr[35].ToString()), TheTool.getDouble(dr[36].ToString()), TheTool.getDouble(dr[37].ToString()) };
                        data.HipLeft = new double[3] { TheTool.getDouble(dr[38].ToString()), TheTool.getDouble(dr[39].ToString()), TheTool.getDouble(dr[40].ToString()) };
                        data.HipRight = new double[3] { TheTool.getDouble(dr[41].ToString()), TheTool.getDouble(dr[42].ToString()), TheTool.getDouble(dr[43].ToString()) };
                        data.KneeLeft = new double[3] { TheTool.getDouble(dr[44].ToString()), TheTool.getDouble(dr[45].ToString()), TheTool.getDouble(dr[46].ToString()) };
                        data.KneeRight = new double[3] { TheTool.getDouble(dr[47].ToString()), TheTool.getDouble(dr[48].ToString()), TheTool.getDouble(dr[49].ToString()) };
                        data.AnkleLeft = new double[3] { TheTool.getDouble(dr[50].ToString()), TheTool.getDouble(dr[51].ToString()), TheTool.getDouble(dr[52].ToString()) };
                        data.AnkleRight = new double[3] { TheTool.getDouble(dr[53].ToString()), TheTool.getDouble(dr[54].ToString()), TheTool.getDouble(dr[55].ToString()) };
                        data.FootLeft = new double[3] { TheTool.getDouble(dr[56].ToString()), TheTool.getDouble(dr[57].ToString()), TheTool.getDouble(dr[58].ToString()) };
                        data.FootRight = new double[3] { TheTool.getDouble(dr[59].ToString()), TheTool.getDouble(dr[60].ToString()), TheTool.getDouble(dr[61].ToString()) };
                        data_list.Add(data);
                    }
                    i_row++;
                }
            }
            catch (Exception ex) { TheSys.showError(path + ": " + ex); }
            //TheSys.showTime();
            return data_list;
        }

        //Slower Ver
        //public static List<UKI_DataRaw> csv_loadFileTo_DataRaw(String path, int skip)
        //{
        //    List<UKI_DataRaw> data_list = new List<UKI_DataRaw>();
        //    try
        //    {
        //        DataTable dt = CSVReader.ReadCSVFile(path, true);
        //        if (dt.Columns.Count > 61)
        //        {
        //            UKI_DataRaw data;
        //            int i_r = 0;
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                if (i_r >= skip)
        //                {
        //                    data = new UKI_DataRaw();
        //                    data.id = TheTool.getInt(dr[0].ToString());
        //                    data.time = dr[1].ToString();
        //                    data.Head = new double[3] { TheTool.getDouble(dr[2].ToString()), TheTool.getDouble(dr[3].ToString()), TheTool.getDouble(dr[4].ToString()) };
        //                    data.ShoulderCenter = new double[3] { TheTool.getDouble(dr[5].ToString()), TheTool.getDouble(dr[6].ToString()), TheTool.getDouble(dr[7].ToString()) };
        //                    data.ShoulderLeft = new double[3] { TheTool.getDouble(dr[8].ToString()), TheTool.getDouble(dr[9].ToString()), TheTool.getDouble(dr[10].ToString()) };
        //                    data.ShoulderRight = new double[3] { TheTool.getDouble(dr[11].ToString()), TheTool.getDouble(dr[12].ToString()), TheTool.getDouble(dr[13].ToString()) };
        //                    data.ElbowLeft = new double[3] { TheTool.getDouble(dr[14].ToString()), TheTool.getDouble(dr[15].ToString()), TheTool.getDouble(dr[16].ToString()) };
        //                    data.ElbowRight = new double[3] { TheTool.getDouble(dr[17].ToString()), TheTool.getDouble(dr[18].ToString()), TheTool.getDouble(dr[19].ToString()) };
        //                    data.WristLeft = new double[3] { TheTool.getDouble(dr[20].ToString()), TheTool.getDouble(dr[21].ToString()), TheTool.getDouble(dr[22].ToString()) };
        //                    data.WristRight = new double[3] { TheTool.getDouble(dr[23].ToString()), TheTool.getDouble(dr[24].ToString()), TheTool.getDouble(dr[25].ToString()) };
        //                    data.HandLeft = new double[3] { TheTool.getDouble(dr[26].ToString()), TheTool.getDouble(dr[27].ToString()), TheTool.getDouble(dr[28].ToString()) };
        //                    data.HandRight = new double[3] { TheTool.getDouble(dr[29].ToString()), TheTool.getDouble(dr[30].ToString()), TheTool.getDouble(dr[31].ToString()) };
        //                    data.Spine = new double[3] { TheTool.getDouble(dr[32].ToString()), TheTool.getDouble(dr[33].ToString()), TheTool.getDouble(dr[34].ToString()) };
        //                    data.HipCenter = new double[3] { TheTool.getDouble(dr[35].ToString()), TheTool.getDouble(dr[36].ToString()), TheTool.getDouble(dr[37].ToString()) };
        //                    data.HipLeft = new double[3] { TheTool.getDouble(dr[38].ToString()), TheTool.getDouble(dr[39].ToString()), TheTool.getDouble(dr[40].ToString()) };
        //                    data.HipRight = new double[3] { TheTool.getDouble(dr[41].ToString()), TheTool.getDouble(dr[42].ToString()), TheTool.getDouble(dr[43].ToString()) };
        //                    data.KneeLeft = new double[3] { TheTool.getDouble(dr[44].ToString()), TheTool.getDouble(dr[45].ToString()), TheTool.getDouble(dr[46].ToString()) };
        //                    data.KneeRight = new double[3] { TheTool.getDouble(dr[47].ToString()), TheTool.getDouble(dr[48].ToString()), TheTool.getDouble(dr[49].ToString()) };
        //                    data.AnkleLeft = new double[3] { TheTool.getDouble(dr[50].ToString()), TheTool.getDouble(dr[51].ToString()), TheTool.getDouble(dr[52].ToString()) };
        //                    data.AnkleRight = new double[3] { TheTool.getDouble(dr[53].ToString()), TheTool.getDouble(dr[54].ToString()), TheTool.getDouble(dr[55].ToString()) };
        //                    data.FootLeft = new double[3] { TheTool.getDouble(dr[56].ToString()), TheTool.getDouble(dr[57].ToString()), TheTool.getDouble(dr[58].ToString()) };
        //                    data.FootRight = new double[3] { TheTool.getDouble(dr[59].ToString()), TheTool.getDouble(dr[60].ToString()), TheTool.getDouble(dr[61].ToString()) };
        //                    data_list.Add(data);
        //                }
        //                i_r++;
        //            }
        //        }
        //        else { TheSys.showError(path + " (Wrong Format) : "); }
        //    }
        //    catch (Exception ex) { TheSys.showError(path + ": " + ex); }
        //    return data_list;
        //}

        public static int angTechq_Spine = 0;
        public static int angTechq_HC = 1;
        public static int angTechq_SC_HC = 2;
        public static string getAngTechqName(int method_id)
        {
            if (method_id == angTechq_Spine) { return "Sp"; }
            else if (method_id == angTechq_SC_HC) { return "SC-HC"; }
            else { return "HC"; }
        }

        public static int centerTechq_NoN = 0;
        public static int centerTechq_HC = 1;
        public static int centerTechq_SC_HC = 2;
        public static string getCenterTechqName(int method_id)
        {
            if (method_id == centerTechq_HC) { return "HC"; }
            else if (method_id == centerTechq_SC_HC) { return "SC-HC"; }
            else { return ""; }
        }

        public static List<UKI_DataRaw> raw_centerBodyJoint(List<UKI_DataRaw> raw, int techId)
        {
            List<UKI_DataRaw> final = new List<UKI_DataRaw>();
            foreach (UKI_DataRaw item in raw)
            {
                UKI_DataRaw new_item = new UKI_DataRaw();
                //Upper Part of Body
                double[] center_joint = item.HipCenter;
                if (techId == 2) { center_joint = item.ShoulderCenter; }
                new_item.id = item.id;
                new_item.time = item.time;
                new_item.Head = centerPosition(item.Head, center_joint);
                new_item.ShoulderLeft = centerPosition(item.ShoulderLeft, center_joint);
                new_item.ShoulderRight = centerPosition(item.ShoulderRight, center_joint);
                new_item.ElbowLeft = centerPosition(item.ElbowLeft, center_joint);
                new_item.ElbowRight = centerPosition(item.ElbowRight, center_joint);
                new_item.WristLeft = centerPosition(item.WristLeft, center_joint);
                new_item.WristRight = centerPosition(item.WristRight, center_joint);
                new_item.HandLeft = centerPosition(item.HandLeft, center_joint);
                new_item.HandRight = centerPosition(item.HandRight, center_joint);
                //Lower Part of Body
                center_joint = item.HipCenter;
                new_item.ShoulderCenter = centerPosition(item.ShoulderCenter, center_joint);
                new_item.Spine = centerPosition(item.Spine, center_joint);
                new_item.HipCenter = centerPosition(center_joint, center_joint);
                new_item.HipLeft = centerPosition(item.HipLeft, center_joint);
                new_item.HipRight = centerPosition(item.HipRight, center_joint);
                new_item.KneeLeft = centerPosition(item.KneeLeft, center_joint);
                new_item.KneeRight = centerPosition(item.KneeRight, center_joint);
                new_item.AnkleLeft = centerPosition(item.AnkleLeft, center_joint);
                new_item.AnkleRight = centerPosition(item.AnkleRight, center_joint);
                new_item.FootLeft = centerPosition(item.FootLeft, center_joint);
                new_item.FootRight = centerPosition(item.FootRight, center_joint);
                //
                new_item.Spine_Y = item.Spine_Y;
                new_item.LKnee_Y = item.LKnee_Y;
                new_item.RKnee_Y = item.RKnee_Y;
                final.Add(new_item);
            }
            return final;
        }

        public static double[] centerPosition(double[] position, double[] origin)
        {
            if (position == origin) { return new double[] { 0, 0, 0 }; }
            else { return new double[] { position[0] - origin[0], position[1] - origin[1], position[2] - origin[2] }; }
        }

        public static double[] centerPosition(double[] position, double[] origin, double range)
        {
            if (position == origin) { return new double[] { 0, 0, 0 + range }; }
            else { return new double[] { position[0] - origin[0], position[1] - origin[1], position[2] - origin[2] + range }; }
        }

        //******************************************************************************************************

        public static List<UKI_Data_AnalysisForm> getData_AnalysisForm(List<UKI_DataRaw> list_raw, List<UKI_Data_BasicPose> list_bp)
        {
            List<UKI_Data_AnalysisForm> new_data = new List<UKI_Data_AnalysisForm>();
            TheUKI.DataRaw_AddExtraFeature(list_raw);//Make sure every thing is included
            UKI_DataRaw[] arr_raw = list_raw.ToArray();
            UKI_Data_BasicPose[] arr_bp = list_bp.ToArray();
            if (arr_raw.Count() == arr_bp.Count())
            {
                for (int i = 0; i < arr_raw.Count(); i++)
                {
                    UKI_Data_AnalysisForm da = new UKI_Data_AnalysisForm();
                    da.id = arr_raw[i].id;
                    da.raw = arr_raw[i];
                    da.bp = arr_bp[i];
                    new_data.Add(da);
                }
            }
            return new_data;
        }

        //base posture is not available >> simulate default value
        public static List<UKI_Data_AnalysisForm> getData_AnalysisForm(List<UKI_DataRaw> list_raw)
        {
            List<UKI_Data_BasicPose> list_bp = new List<UKI_Data_BasicPose>();
            for (int i = 0; i < list_raw.Count; i++) { list_bp.Add(new UKI_Data_BasicPose()); }
            return getData_AnalysisForm(list_raw, list_bp);
        }

        public static String[] data_bp_name = new String[]{
            "jump","lean","spin",
            "handL_X","handL_Y","handL_Z",
            "handR_X","handR_Y","handR_Z",
            "handLR_close","legL","legR"
        };

        public static string data_bp_header()
        {
            String s = "id,time";
            for (int i = 0; i < data_bp_name.Count(); i++) { s += "," + data_bp_name[i]; }
            return s;
        }

        public static List<string> getBasicPostureData(List<UKI_Data_BasicPose> list_bp)
        {
            List<string> output = new List<string>();
            foreach (UKI_Data_BasicPose bp in list_bp)
            {
                string s = "";
                s += bp.id + "," + bp.time;
                for (int i = 0; i < bp.basic_pose.Count(); i++)
                {
                    s += "," + bp.basic_pose[i];
                }
                output.Add(s);
            }
            return output;
        }

        public static string data_movement_header = "id,time," +
            "e_m_all_mva,e_m_arm_mva,e_m_leg_mva,e_m_core_mva," +
            "e_m_all,e_m_arm,e_m_leg,e_m_core";

        public static List<string> getMovementData(List<UKI_DataMovement> list_movement, Boolean showMinMax)
        {
            List<string> output = new List<string>();
            foreach (UKI_DataMovement m in list_movement)
            {
                string s = m.id + "," + m.time + "," +
                    m.ms_all_avg + "," + m.ms_hand_avg + "," +
                    m.ms_leg_avg + "," + m.ms_core_avg + "," +
                    m.ms_all + "," + m.ms_hand + "," +
                    m.ms_leg + "," + m.ms_core;
                if (showMinMax) { s = m.type_algo + "," + m.type_gt + "," + s; }
                output.Add(s);
            }
            return output;
        }

        public static string data_addition_02_Header = "," +
            "r1_hLR_X,r1_hLR_Y,r1_hLR_Z,r1_hLR_eu," +
            "r1_fLR_X,r1_fLR_Y,r1_fLR_Z,r1_fLR_eu," +
            //
            "r2_hL_eL_X,r2_hL_eL_Y,r2_hL_eL_Z," +
            "r2_hR_eR_X,r2_hR_eR_Y,r2_hR_eR_Z," +
            //
            "r2_fL_kL_X,r2_fL_kL_Y,r2_fL_kL_Z," +
            "r2_fR_kR_X,r2_fR_kR_Y,r2_fR_kR_Z"
            ;

        public static List<string> collectData_Addition_02(List<UKI_DataRaw> data_raw)
        {
            List<string> data_add_02 = new List<string>();
            foreach (UKI_DataRaw d in data_raw)
            {
                data_add_02.Add("," +
                    getX_diff(d.HandLeft, d.HandRight) + "," +
                    getY_diff(d.HandLeft, d.HandRight) + "," +
                    getZ_diff(d.HandLeft, d.HandRight) + "," +
                    getDist(d.HandLeft, d.HandRight) + "," +
                    //
                    getX_diff(d.FootLeft, d.FootRight) + "," +
                    getY_diff(d.FootLeft, d.FootRight) + "," +
                    getZ_diff(d.FootLeft, d.FootRight) + "," +
                    getDist(d.FootLeft, d.FootRight) + "," +
                    //
                    getX_diff(d.HandLeft, d.ElbowLeft) + "," +
                    getY_diff(d.HandLeft, d.ElbowLeft) + "," +
                    getZ_diff(d.HandLeft, d.ElbowLeft) + "," +
                    //
                    getX_diff(d.HandRight, d.ElbowRight) + "," +
                    getY_diff(d.HandRight, d.ElbowRight) + "," +
                    getZ_diff(d.HandRight, d.ElbowRight) + "," +
                    //
                    getX_diff(d.FootLeft, d.KneeLeft) + "," +
                    getY_diff(d.FootLeft, d.KneeLeft) + "," +
                    getZ_diff(d.FootLeft, d.KneeLeft) + "," +
                    //
                    getX_diff(d.FootRight, d.KneeRight) + "," +
                    getY_diff(d.FootRight, d.KneeRight) + "," +
                    getZ_diff(d.FootRight, d.KneeRight)
                    );
            }
            return data_add_02;
        }

        public static double getX_diff(double[] j1, double[] j2) { return j1[0] - j2[2]; }
        public static double getY_diff(double[] j1, double[] j2) { return j1[1] - j2[1]; }
        public static double getZ_diff(double[] j1, double[] j2) { return j1[2] - j2[2]; }
        public static double getDist(double[] j1, double[] j2) { return TheTool.calEuclidian(j1, j2); }
        public static double getAng(double[] j1, double[] j2, double[] j3)
        {
            return ThePostureCal.calAngle_3Points(j1, j2, j3);
        }

        //-------------------------------------------------------

        public static List<UKI_DataMovement> adjustMovementData(List<UKI_DataMovement> list_origin)
        {
            //Input t+10 then AVG(t,t+10)
            //Output t-5,t+5 then AVG(t-5,t+5)
            int move = (PosExtract.move_size / 2) + ((PosExtract.mva_size_minus1 - 1) / 2);
            return adjustMovementDataAVG(list_origin, move);
        }

        //move = 5 >> move up 5
        public static List<UKI_DataMovement> adjustMovementDataAVG(List<UKI_DataMovement> list_origin, int move)
        {
            List<UKI_DataMovement> list_new = new List<UKI_DataMovement>();
            UKI_DataMovement[] arr_origin = list_origin.ToArray();
            for (int i = 0; i < arr_origin.Count(); i++)
            {
                UKI_DataMovement new_data = new UKI_DataMovement();
                new_data.id = arr_origin[i].id;
                new_data.time = arr_origin[i].time;
                new_data.spine_Y = arr_origin[i].spine_Y;
                new_data.ms_all = arr_origin[i].ms_all;
                new_data.ms_hand = arr_origin[i].ms_hand;
                new_data.ms_leg = arr_origin[i].ms_leg;
                new_data.ms_core = arr_origin[i].ms_core;
                if (i < arr_origin.Count() - move)
                {
                    new_data.ms_all_avg = arr_origin[i + move].ms_all_avg;
                    new_data.ms_hand_avg = arr_origin[i + move].ms_hand_avg;
                    new_data.ms_leg_avg = arr_origin[i + move].ms_leg_avg;
                    new_data.ms_core_avg = arr_origin[i + move].ms_core_avg;
                }
                list_new.Add(new_data);
            }
            return list_new;
        }


        public static string data_log_header = "id,time,ini_sp_Y,ini_knee_Y,ini_bend_A,cur_range,cur_sp_Y,cur_bend_A";


        //adjust MVA column before put to this
        public static void exportData_Movement(List<UKI_DataMovement> data_movement_adjusted, String path_movement, Boolean showMinMax)
        {
            List<string> data_movement_final = new List<string>();
            if (showMinMax) { data_movement_final.Add("min_max," + TheUKI.data_movement_header); }
            else { data_movement_final.Add(TheUKI.data_movement_header); }
            data_movement_final.AddRange(TheUKI.getMovementData(data_movement_adjusted, showMinMax));
            TheTool.exportCSV_orTXT(path_movement, data_movement_final, false);
        }


        public static string data_addition_01_header = ",facing_A,bending_A,r_sp_Y,r_kL_Y,r_kR_Y";
        public static string data_addition_full_header = "";
        public static List<string> createData_Additional(
            List<UKI_DataRaw> data_raw, List<UKI_DataRaw> data_raw_centered, List<String> data_add_01,
            int center_techq)
        {
            List<string> data_addition_full = new List<string>();
            try
            {
                string[] arr_raw = TheUKI.get_UKI_DataRaw_String_List(data_raw_centered).ToArray();
                string[] arr_add_01 = data_add_01.ToArray();
                string[] arr_add_02 = TheUKI.collectData_Addition_02(data_raw).ToArray();
                //
                data_addition_full_header = TheUKI.data_raw_centered_Header(center_techq) + data_addition_01_header + TheUKI.data_addition_02_Header;
                for (int i = 0; i < arr_raw.Count(); i++)
                {
                    data_addition_full.Add(arr_raw[i] + arr_add_01[i] + arr_add_02[i]);
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return data_addition_full;
        }

        public static List<int[]> loadKeyPose(string key_path)
        {
            List<int[]> list_keyPose = new List<int[]>();
            if (File.Exists(key_path))
            {
                foreach (string s in TheTool.read_File_getListString(key_path))
                {
                    if (s != "") {
                        string[] a = TheTool.splitText(s,",");
                        int start = TheTool.getInt(a[0]);
                        int end = start;
                        if (a.Count() > 1) { end = TheTool.getInt(a[1]); }
                        list_keyPose.Add(new int[] { start, end });
                    }
                }
            }
            return list_keyPose;
        }

        public static List<int_double> loadKeyJump(string key_path)
        {
            List<int_double> list_keyPose = new List<int_double>();
            if (File.Exists(key_path))
            {
                foreach (string s in TheTool.read_File_getListString(key_path))
                {
                    if (s != "")
                    {
                        string[] a = TheTool.splitText(s, ":");
                        if (a.Count() == 2) {
                            list_keyPose.Add(new int_double{ i = TheTool.getInt(a[0]), v = TheTool.getDouble(a[1]) });
                        }
                    }
                }
            }
            return list_keyPose;
        }

        public static Boolean captureJump = false;//capture at highest (Specially for MID = 1)

        public static List<keyPoseGT> loadKeyPoseGT(string key_path)
        {
            List<keyPoseGT> list_keyPose = new List<keyPoseGT>();
            if (File.Exists(key_path))
            {
                foreach (string s in TheTool.read_File_getListString(key_path))
                {
                    int v = 0;
                    keyPoseGT keyGT = new keyPoseGT();
                    if (s != "")
                    {
                        string[] a = TheTool.splitText(s, ",");
                        if (a.Count() > 0)
                        {
                            string[] b = TheTool.splitText(a[0], "-");
                            if (b.Count() > 0) { 
                                v = TheTool.getInt(b[0]); 
                                keyGT.start[0] = v; keyGT.start[1] = v;
                                keyGT.end[0] = v; keyGT.end[1] = v;
                            }
                            if (b.Count() > 1)
                            {
                                v = TheTool.getInt(b[1]); 
                                keyGT.start[1] = v;
                                keyGT.end[0] = v; keyGT.end[1] = v;
                            }
                        }
                        if (a.Count() > 1)
                        {
                            string[] b = TheTool.splitText(a[1], "-");
                            if (b.Count() > 0)
                            {
                                v = TheTool.getInt(b[0]); 
                                keyGT.end[0] = v; keyGT.end[1] = v;
                            }
                            if (b.Count() > 1)
                            {
                                v = TheTool.getInt(b[1]); 
                                keyGT.end[1] = v;
                            }
                        }
                        list_keyPose.Add(keyGT);
                    }
                }
            }
            return list_keyPose;
        }

        public static List<UKI_DataRaw> UKI_DataRaw_selectRow(List<UKI_DataRaw> full_list, int start_id, int end_id)
        {
            List<UKI_DataRaw> selected = new List<UKI_DataRaw>();
            foreach (UKI_DataRaw d in full_list)
            {
                if (d.id > end_id) { break; }
                else if (d.id >= start_id) { selected.Add(d); }
            }
            return selected;
        }

        public static void exportKey(string path_key, List<int[]> keyList)
        {
            List<string> list_data = new List<string>();
            foreach (int[] k in keyList)
            {
                string d = "";
                for (int i = 0; i < k.Count(); i++)
                {
                    if (i > 0) { d += ","; }
                    d += k[i];
                }
                list_data.Add(d);
            }
            TheTool.exportCSV_orTXT(path_key, list_data, false);
        }

        //exportKeyJ
        public static void exportKeyJ(string path_key, List<int_double> keyList)
        {
            List<string> list_data = new List<string>();
            foreach (int_double k in keyList)
            {
                string d = k.i + ":" + k.v;
                list_data.Add(d);
            }
            TheTool.exportCSV_orTXT(path_key, list_data, false);
        }

        public static List<string> UKI_DataDouble_convertToListString(List<UKI_DataDouble> list_data)
        {
            List<string> output = new List<string>();
            foreach (UKI_DataDouble d in list_data) { output.Add(d.id + "," + TheTool.printTxt(d.data, ",")); }
            return output;
        }

        ////absolute change over x frame
        //public static List<UKI_DataDouble> UKI_DataDouble_AbsChangeXFrame(List<UKI_DataDouble> list_data, int x)
        //{
        //    List<UKI_DataDouble> output = new List<UKI_DataDouble>();
            //if (list_data.Count > 0)
            //{
            //    int i_last = 0;
            //    for (int i = 1; i < list_data.Count(); i++)
            //    {
            //        if (i_last >= x) { i_last++; }
            //        UKI_DataDouble o = new UKI_DataDouble();
            //        o.id = list_data[i].id;
            //        for (int j = 0; j < list_data[i].data.Count(); j++)
            //        {
            //            o.data.Add(Math.Abs(list_data[i].data[j] - list_data[i_last].data[j]));
            //        }
            //        output.Add(o);
            //    }
            //}
        //    return output;
        //}

        //moving average
        public static List<UKI_DataDouble> UKI_DataDouble_MVA(List<UKI_DataDouble> list_data, int range)
        {
            List<UKI_DataDouble> output = new List<UKI_DataDouble>();
            if(list_data.Count > 0){
                List<double> avg = new List<double>(); 
                TheTool.list_initialize(avg, list_data.First().data.Count());
                int range0 = 0;
                for (int i = 0; i < list_data.Count(); i++)
                {
                    if(range0 < range){range0++;}
                    UKI_DataDouble o = new UKI_DataDouble();
                    o.id = list_data[i].id;
                    for (int j = 0; j < list_data[i].data.Count(); j++)
                    {
                        if (i > 0 && i - 1 < range0) { avg[j] = avg[j] * i / range0; }
                        avg[j] += list_data[i].data[j] / range0;
                        if (range0 == range) { avg[j] -= list_data[i-(range-1)].data[j] / range0; }
                        o.data.Add(avg[j]);
                    }
                    output.Add(o);
                }
            }
            return output;
        }

        //change between consecutive frames
        public static List<UKI_DataDouble> UKI_DataDouble_ChangeBwFrame(List<UKI_DataDouble> list_data)
        {
            List<UKI_DataDouble> output = new List<UKI_DataDouble>();
            if (list_data.Count > 0) 
            {
                UKI_DataDouble o = new UKI_DataDouble();
                o.id = list_data.First().id;
                TheTool.list_initialize(o.data, list_data.First().data.Count());
                output.Add(o);
                for (int i = 1; i < list_data.Count(); i++)
                {
                    o = new UKI_DataDouble();
                    o.id = list_data[i].id;
                    for (int j = 0; j < list_data[i].data.Count(); j++)
                    {
                        o.data.Add(Math.Abs(list_data[i].data[j] - list_data[i-1].data[j]));
                    }
                    output.Add(o);
                }
            }
            return output;
        }

        //Sum Keyframe from all feature
        public static List<int[]> UKI_DataDouble_getKeyPose_CrossThreshold(List<UKI_DataDouble> list_data, double threshold)
        {
            List<int[]> list_keyPose_sum = new List<int[]>();
            if (list_data.Count > 0)
            {
                for (int j = 0; j < list_data.First().data.Count(); j++)
                {
                    int[] keyPose = new int[]{-1,-1};
                    for (int i = 1; i < list_data.Count(); i++)
                    {
                        if(keyPose[0] <0 
                            && list_data[i - 1].data[j] <= threshold && list_data[i].data[j] > threshold ){
                            keyPose[0] = list_data[i].id;
                        }
                        else if (keyPose[0] >= 0 && keyPose[1] < 0 
                            && list_data[i - 1].data[j] >= threshold && list_data[i].data[j] < threshold)
                        {
                            keyPose[1] = list_data[i].id;
                            list_keyPose_sum.Add(keyPose);
                            keyPose = new int[] { -1, -1 };
                        }
                    }
                }
            }
            return list_keyPose_sum;
        }

        //int[2]
        public static List<int[]> keyPose_Combine_longestPath(List<int[]> list_key)
        {
            List<int[]> list_key_combine = new List<int[]>();
            if(list_key.Count > 0){
                int[] c = new int[]{ -1,-1};
                foreach (int[] k in list_key)
                {
                    if (c[0] > 0 && c[1] < k[0]) { list_key_combine.Add(c); c = new int[] { -1, -1 }; }
                    if (c[0] < 0) { c[0] = k[0];}
                    if (c[1] < k[1]) { c[1] = k[1]; }
                }
                list_key_combine.Add(c);
            }
            return list_key_combine;
        }

        public static void UKI_DataRaw_scaling(ref List<UKI_DataRaw> list_raw, double scale_ratio, double moveBack){
            foreach (UKI_DataRaw raw in list_raw)
            {
                UKI_DataRaw_scaling_sub(ref raw.Head, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.ShoulderCenter, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.ShoulderLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.ShoulderRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.ElbowLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.ElbowRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.WristLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.WristRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.HandLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.HandRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.Spine, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.HipCenter, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.HipLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.HipRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.KneeLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.KneeRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.AnkleLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.AnkleRight, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.FootLeft, scale_ratio, moveBack);
                UKI_DataRaw_scaling_sub(ref raw.FootRight, scale_ratio, moveBack);
            }
        }

        public static void UKI_DataRaw_scaling_sub(ref double[] j, double scale_ratio, double moveBack)
        {
            j[0] = j[0] * scale_ratio;
            j[1] = j[1] * scale_ratio;
            j[2] = j[2] * scale_ratio + moveBack;
        }

        public static List<UKI_DataRaw> UKI_DataRaw_centerize(List<UKI_DataRaw> list_raw, double range)
        {
            List<UKI_DataRaw> centered_list = new List<UKI_DataRaw>();
            if(list_raw.Count > 0)
            {
                double[] c = list_raw.First().Spine;
                foreach (UKI_DataRaw raw in list_raw)
                {
                    UKI_DataRaw centered = new UKI_DataRaw();
                    centered.id = raw.id;
                    centered.time = raw.time;
                    centered.Head = centerPosition(raw.Head, c, range);
                    centered.ShoulderCenter = centerPosition(raw.ShoulderCenter, c, range);
                    centered.ShoulderLeft = centerPosition(raw.ShoulderLeft, c, range);
                    centered.ShoulderRight = centerPosition(raw.ShoulderRight, c, range);
                    centered.ElbowLeft = centerPosition(raw.ElbowLeft, c, range);
                    centered.ElbowRight = centerPosition(raw.ElbowRight, c, range);
                    centered.WristLeft = centerPosition(raw.WristLeft, c, range);
                    centered.WristRight = centerPosition(raw.WristRight, c, range);
                    centered.HandLeft = centerPosition(raw.HandLeft, c, range);
                    centered.HandRight = centerPosition(raw.HandRight, c, range);
                    centered.Spine = centerPosition(raw.Spine, c, range);
                    centered.HipCenter = centerPosition(raw.HipCenter, c, range);
                    centered.HipLeft = centerPosition(raw.HipLeft, c, range);
                    centered.HipRight = centerPosition(raw.HipRight, c, range);
                    centered.KneeLeft = centerPosition(raw.KneeLeft, c, range);
                    centered.KneeRight = centerPosition(raw.KneeRight, c, range);
                    centered.AnkleLeft = centerPosition(raw.AnkleLeft, c, range);
                    centered.AnkleRight = centerPosition(raw.AnkleRight, c, range);
                    centered.FootLeft = centerPosition(raw.FootLeft, c, range);
                    centered.FootRight = centerPosition(raw.FootRight, c, range);
                    centered_list.Add(centered);
                }
            }
            return centered_list;
        }

        public static UKI_DataRaw UKI_DataRaw_centerize(UKI_DataRaw raw, double range)
        {
            UKI_DataRaw centered = new UKI_DataRaw();
            centered.id = raw.id;
            centered.time = raw.time;
            centered.Head = centerPosition(raw.Head, raw.Spine, range);
            centered.ShoulderCenter = centerPosition(raw.ShoulderCenter, raw.Spine, range);
            centered.ShoulderLeft = centerPosition(raw.ShoulderLeft, raw.Spine, range);
            centered.ShoulderRight = centerPosition(raw.ShoulderRight, raw.Spine, range);
            centered.ElbowLeft = centerPosition(raw.ElbowLeft, raw.Spine, range);
            centered.ElbowRight = centerPosition(raw.ElbowRight, raw.Spine, range);
            centered.WristLeft = centerPosition(raw.WristLeft, raw.Spine, range);
            centered.WristRight = centerPosition(raw.WristRight, raw.Spine, range);
            centered.HandLeft = centerPosition(raw.HandLeft, raw.Spine, range);
            centered.HandRight = centerPosition(raw.HandRight, raw.Spine, range);
            centered.Spine = centerPosition(raw.Spine, raw.Spine, range);
            centered.HipCenter = centerPosition(raw.HipCenter, raw.Spine, range);
            centered.HipLeft = centerPosition(raw.HipLeft, raw.Spine, range);
            centered.HipRight = centerPosition(raw.HipRight, raw.Spine, range);
            centered.KneeLeft = centerPosition(raw.KneeLeft, raw.Spine, range);
            centered.KneeRight = centerPosition(raw.KneeRight, raw.Spine, range);
            centered.AnkleLeft = centerPosition(raw.AnkleLeft, raw.Spine, range);
            centered.AnkleRight = centerPosition(raw.AnkleRight, raw.Spine, range);
            centered.FootLeft = centerPosition(raw.FootLeft, raw.Spine, range);
            centered.FootRight = centerPosition(raw.FootRight, raw.Spine, range);
            return centered;
        }

    }


    class UKI_DataDouble
    {
        public int id = 0;
        public List<double> data = new List<double>();
    }


}
