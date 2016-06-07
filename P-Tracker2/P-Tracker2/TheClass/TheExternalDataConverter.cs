using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheExternalDataConverter
    {

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
                foreach (string s in data_origin.Skip(1)) {
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
                            else if (joint_id == 20) { 
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
            catch (Exception ex) { 
                TheSys.showError(TheTool.getFileName_byPath(path_origin) + " : " + ex); 
            }
        }

    }
}
