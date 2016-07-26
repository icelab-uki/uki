using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheConverter
    {
        public static void MSR_convertFile(DataTable dt, string col_path, string path_saveFolder)
        {
            foreach (DataRow r in dt.Rows)
            {
                try
                {
                    string path_origin = r[col_path].ToString();
                    TheTool.Folder_CreateIfMissing(path_saveFolder);
                    string path_save = path_saveFolder + TheTool.getFileName_byPath(path_origin) + ".csv";
                    MSR_convertFile(path_origin, path_save);
                }
                catch (Exception ex) { TheSys.showError(r[col_path].ToString() + " : " + ex.ToString()); }
            }
            System.Windows.MessageBox.Show(@"Save to file\[Convert]\");
        }

        public static void MSR_showSample()
        {
            TheSys.showError("Link: http://research.microsoft.tcom/en-us/um/people/zliu/ActionRecoRsrc/default.htm");
            TheSys.showError("Below is an example of data");
            TheSys.showError("");
            foreach (String s in TheTool.read_File_getListString(TheURL.url_tv_sample_msr))
            {
                TheSys.showError(s);
            }
        }

        //covert MSR to UKI format
        //MSR data http://research.microsoft.com/en-us/um/people/zliu/ActionRecoRsrc/default.htm
        public static void MSR_convertFile(String path_origin, String path_save)
        {
            try
            {
                List<string> data_origin = TheTool.read_File_getListString(path_origin);
                List<string> data_final = new List<String>();
                data_final.Add(TheUKI.data_raw_Header);
                //-------------------------------------------------
                int joint_id = 1; Boolean skip = false;
                UKI_DataRaw_String data_raw = new UKI_DataRaw_String();
                DateTime time_similated = DateTime.Now;//similated time
                //
                foreach (string s in data_origin.Skip(1))
                {
                    if (s == "40" || s == "80")
                    {
                        data_raw = new UKI_DataRaw_String();
                        joint_id = 1; skip = false;
                        time_similated = time_similated.AddMilliseconds(40);
                    }
                    else if (joint_id <= 20)
                    {
                        if (skip) { skip = false; }
                        else
                        {
                            skip = true;
                            string[] r = TheTool.splitText(s, " ");
                            if (joint_id == 1) { data_raw.HipCenter = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 2) { data_raw.Spine = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 3) { data_raw.ShoulderCenter = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 4) { data_raw.Head = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 5) { data_raw.ShoulderLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 6) { data_raw.ElbowLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 7) { data_raw.WristLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 8) { data_raw.HandLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 9) { data_raw.ShoulderRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 10) { data_raw.ElbowRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 11) { data_raw.WristRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 12) { data_raw.HandRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 13) { data_raw.HipLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 14) { data_raw.KneeLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 15) { data_raw.AnkleLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 16) { data_raw.FootLeft = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 17) { data_raw.HipRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 18) { data_raw.KneeRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 19) { data_raw.AnkleRight = "," + r[0] + "," + r[1] + "," + r[2]; }
                            else if (joint_id == 20)
                            {
                                data_raw.FootRight = "," + r[0] + "," + r[1] + "," + r[2];
                                //
                                data_raw.id = (data_final.Count() - 1).ToString();
                                data_raw.time = "," + time_similated.ToString("ddHHmmssff");
                                data_final.Add(TheUKI.get_UKI_DataRaw_String(data_raw));
                            }
                            joint_id++;
                        }
                    }
                }
                //-------------------------------------------------
                TheTool.exportCSV_orTXT(path_save, data_final, false);
            }
            catch (Exception ex)
            {
                TheSys.showError(TheTool.getFileName_byPath(path_origin) + " : " + ex);
            }
        }

        //===============================================================================
        
        public static void Verlab_convertFile(String path_origin, String path_save)
        {
            try
            {
                List<string> data_origin = TheTool.read_File_getListString(path_origin);
                //-------------------------------------------------
                DataTable dt = TheTool.convert_List_toDataTable(data_origin);
                int i_c = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    int i_r = 0;
                    UKI_DataRaw_String data_raw = new UKI_DataRaw_String();
                    foreach (DataRow dr in dt.Rows)
                    {
                        //if (i_r == 17) { data_raw.}
                        

                        //1. Left Shoulder X Coordinate

                        //2. Left Shoulder Y Coordinate

                        //3. Left Shoulder Z Coordinate

                        //4. Right Shoulder X Coordinate

                        //5. Right Shoulder Y Coordinate

                        //6. Right Shoulder Z Coordinate

                        //7. Head X Coordinate

                        //8. Head Y Coordinate

                        //9. Head Z Coordinate

                        //10.Torso X Coordinate

                        //11.Torso Y Coordinate

                        //12.Torso Z Coordinate

                        //13.Left Hip X Coordinate

                        //14.Left Hip Y Coordinate

                        //15.Left Hip Z Coordinate

                        //16.Right Hip X Coordinate

                        //17.Right Hip Y Coordinate

                        //18.Right Hip Z Coordinate

                        //19.Left Elbow X Coordinate

                        //20.Left Elbow Y Coordinate

                        //21.Left Elbow Z Coordinate

                        //22.Right Elbow X Coordinate

                        //23.Right Elbow Y Coordinate

                        //24.Right Elbow Z Coordinate

                        //25.Left Hand X Coordinate

                        //26.Left Hand Y Coordinate

                        //27.Left Hand Z Coordinate

                        //28.Right Hand X Coordinate

                        //29.Right Hand Y Coordinate

                        //30.Right Hand Z Coordinate

                        //31.Left Knee X Coordinate

                        //32.Left Knee Y Coordinate

                        //33.Left Knee Z Coordinate

                        //34.Right Knee X Coordinate

                        //35.Right Knee Y Coordinate

                        //36.Right Knee Z Coordinate

                        //37.Left Foot X Coordinate

                        //38.Left Foot Y Coordinate

                        //39.Left Foot Z Coordinate

                        //40.Right Foot X Coordinate

                        //41.Right Foot Y Coordinate

                        //42.Right Foot Z Coordinate

                        //43.Neck X Coordinate

                        //44.Neck Y Coordinate

                        //45.Neck Z Coordinate

                        i_r++;
                    }
                    i_c++;
                }



                TheTool.export_dataTable_to_CSV(TheURL.url_saveFolder + "test.csv",dt);
                //UKI_DataRaw_String data_raw = new UKI_DataRaw_String();
                //DateTime time_similated = DateTime.Now;//similated time
                ////
                //foreach (string s in data_origin.Skip(1))
                //{
                //    TheTool.splitText(s,",");
                //    //string s_new = 

                //    ////,

                //}
                //-------------------------------------------------
                //TheTool.exportCSV_orTXT(path_save, data_final, false);
            }
            catch (Exception ex)
            {
                TheSys.showError(TheTool.getFileName_byPath(path_origin) + " : " + ex);
            }
        }


        //==============================================

        public static void BVH_convertFile(String path_origin, String path_save)
        {
            TheUKI.saveData_Raw(path_save, BVH_convert(path_origin));
        }

        public static List<UKI_DataRaw> BVH_convert(String path_origin)
        {
            List<UKI_DataRaw> list_raw = new List<UKI_DataRaw>();
            List<MocapNode[]> list_nodeSet = new List<MocapNode[]>();
            try
            {
                List<string> data_origin = TheTool.read_File_getListString(path_origin);
                Boolean start = false;
                int row_i = 0;
                foreach (string row in data_origin)
                {
                    if (start)
                    {
                        BVH_readRow(ref list_raw, ref list_nodeSet, row_i, row);
                        row_i++;
                    }
                    if (!start && row.Contains("Frame Time")) { start = true; }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            //Scale = http://mocap.cs.cmu.edu/faqs.php : /.45 * 0.0254
            TheUKI.UKI_DataRaw_scaling(ref list_raw, 0.05644444444444444444444444444444, 0);
            return list_raw;
        }

        //row_bvh : 9.3722 17.8693 -17.3198 0 0 0 ...
        public static void BVH_readRow(ref List<UKI_DataRaw> list_raw, ref List<MocapNode[]> list_nodeSet, int row_i, string row_bvh) 
        {
            if(row_bvh != ""){
                UKI_DataRaw raw = new UKI_DataRaw();
                MocapNode[] nodeSet = TheBVH.getInitialNode();
                string[] column = TheTool.splitText(row_bvh," ");
                raw.id = row_i;
                int i = 0;
                for(int countnode = 0 ; countnode < nodeSet.Count() ; countnode++)
                {
                    if(countnode == 0) //root
                    {
                        nodeSet[countnode].setTranslate(column[i], column[i + 1], column[i + 2]);
                        nodeSet[countnode].setRotation(column[i + 5], column[i + 4], column[i + 3]);
                        nodeSet[countnode].createRotationMatrix();
                        i += 6;
                    }
                    else if(nodeSet[countnode].isEndsite)
                    {
                        nodeSet[countnode].setRotation(0,0,0);
                        nodeSet[countnode].createRotationMatrix();
                    }
                    else 
                    {
                        nodeSet[countnode].setRotation(column[i + 2], column[i + 1], column[i]);
                        nodeSet[countnode].createRotationMatrix();
                        i +=3;
                    }
                    nodeSet[countnode].tranformRotate();
                }
                TheBVH.MocapNode_loadXYZ(ref raw,nodeSet);
                list_nodeSet.Add(nodeSet);
                list_raw.Add(raw);
            }
        }

        public static List<UKI_DataRaw> MSRAction_convert(String path_origin)
        {
            List<UKI_DataRaw> list_raw = new List<UKI_DataRaw>();
            try
            {
                int id = 0;
                UKI_DataRaw data_raw = new UKI_DataRaw(); 
                data_raw.id = id;
                List<string> data_origin = TheTool.read_File_getListString(path_origin);
                int row_i = 1;
                foreach (string row in data_origin)
                {
                    int j_id = row_i % 20;
                    if (j_id == 1) { MSRAction_convert(ref data_raw.ShoulderRight, row); }
                    else if (j_id == 2) { MSRAction_convert(ref data_raw.ShoulderLeft, row); }
                    else if (j_id == 3) { MSRAction_convert(ref data_raw.ShoulderCenter, row); }
                    else if (j_id == 4) { MSRAction_convert(ref data_raw.Spine, row); }
                    else if (j_id == 5) { MSRAction_convert(ref data_raw.HipRight, row); }
                    else if (j_id == 6) { MSRAction_convert(ref data_raw.HipLeft, row); }
                    else if (j_id == 7) { MSRAction_convert(ref data_raw.HipCenter, row); }
                    else if (j_id == 8) { MSRAction_convert(ref data_raw.ElbowRight, row); }
                    else if (j_id == 9) { MSRAction_convert(ref data_raw.ElbowLeft, row); }
                    else if (j_id == 10) { MSRAction_convert(ref data_raw.WristRight, row); }
                    else if (j_id == 11) { MSRAction_convert(ref data_raw.WristLeft, row); }
                    else if (j_id == 12) { MSRAction_convert(ref data_raw.HandRight, row); }
                    else if (j_id == 13) { MSRAction_convert(ref data_raw.HandLeft, row); }
                    else if (j_id == 14) { MSRAction_convert(ref data_raw.KneeRight, row); }
                    else if (j_id == 15) { MSRAction_convert(ref data_raw.KneeLeft, row); }
                    else if (j_id == 16) { MSRAction_convert(ref data_raw.AnkleRight, row); }
                    else if (j_id == 17) { MSRAction_convert(ref data_raw.AnkleLeft, row); }
                    else if (j_id == 18) { MSRAction_convert(ref data_raw.FootRight, row); }
                    else if (j_id == 19) { MSRAction_convert(ref data_raw.FootLeft, row); }
                    else if (j_id == 0) { 
                        MSRAction_convert(ref data_raw.Head, row);
                        list_raw.Add(data_raw);
                        data_raw = new UKI_DataRaw();
                        id++;
                    }
                    row_i++;
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return list_raw;
        }

        public static void MSRAction_convert(ref double[] j, string data){
            string[] d = TheTool.splitText(data, " "); 
            if (d.Count() > 0) { j[0] = TheTool.getDouble(d[0]); } 
            if (d.Count() > 1) { j[1] = TheTool.getDouble(d[1]); }
            if (d.Count() > 2) { j[2] = TheTool.getDouble(d[2]); }
        }

    }
}
