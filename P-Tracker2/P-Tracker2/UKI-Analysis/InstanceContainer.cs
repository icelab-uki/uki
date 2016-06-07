using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace P_Tracker2
{
    class InstanceContainer
    {
        public List<Instance> list_inst = new List<Instance>();//Main Data
        //
        public List<int> list_sid = new List<int>();
        public List<int> list_mid = new List<int>();
        public void sortBySID() { list_inst.OrderBy(x => x.subject_id); }
        public void sortByMID() { list_inst.OrderBy(x => x.motion_id); }
    }

        

    class Instance
    {
        public string path = "";//original file path
        public string path_key = "";
        public string name = "";//name from path, no extension
        public int subject_id = 0;
        public int motion_id = 0;
        // Below are loaded on request --------------
        public List<UKI_DataRaw> raw = new List<UKI_DataRaw>();//Sequantial RAW data for the whole motion file
        public List<int> keyPose = new List<int>();
        public List<string> keyPoseJump = new List<string>();//KeyPose that Jump is detected (for debug PosExtract)
        public int pose_count = 0;
        //-------------
        public int motion_id_predicted = 0;//No Usage yet
        public Boolean extraFeature_isAvailable = false;

        //load on request
        public List<UKI_DataRaw> getDataRaw(Boolean extraFeature)
        {
            if (this.raw.Count == 0)
            {
                this.raw = TheUKI.csv_loadFileTo_DataRaw(this.path); 
            }
            if(extraFeature_isAvailable == false){
                TheUKI.DataRaw_AddExtraFeature(this.raw);
                extraFeature_isAvailable = true;
            }
            return this.raw;
        }

        public List<int> getKeyPose()
        {
            return getKeyPose(false,false);
        }

        //recompute even data is already exist
        public List<int> getKeyPose(Boolean recompute, Boolean saveData)
        {
            if (recompute) { keyPose.Clear(); }
            if (keyPose.Count == 0){
                if (path_key == "") { path_key = TheTool.getDirectory_byPath(path) + @"\" + TheTool.getFileName_byPath(path) + ".key"; }
                if (recompute == false)
                {
                    this.keyPose = TheUKI.loadKeyPoseture(path_key);// .key is exist
                }
                if (keyPose.Count == 0)
                {
                    this.keyPose = TheUKI.getKeyPosture(getDataRaw(true), true);
                    this.keyPoseJump = TheUKI.list_keyPose_Jump;
                    if (saveData) { TheTool.exportCSV_orTXT(path_key, keyPose, false); }
                }
                this.pose_count = keyPose.Count();
            }
            return keyPose;
        }
    }

    class TheInstanceContainer
    {

        //************************************************************************
        //******* Database Usage ***********************************************
        //Full Load

        public static string path_database = TheURL.url_saveFolder + TheURL.url_9_ukiInst;

        //show only indentity, load data only when perform analysis
        public static InstanceContainer loadInstanceList_fromDatabase(Boolean loadData, List<int> sid_list, List<int> mid_list)
        {
            InstanceContainer inst_container = new InstanceContainer();
            try
            {
                if (!Directory.Exists(path_database))
                {
                    TheSys.showError("Target folder is not exist: " + path_database); return inst_container;
                }
                string[] subject_folder = Directory.GetDirectories(path_database);
                TheTool.sortList_StringWithNumeric(subject_folder);
                //
                int s_start = 0;
                int s_end = subject_folder.Count();
                for (int s_i = s_start; s_i < s_end; s_i++)
                {
                    int s_current = TheTool.getInt(TheTool.getFileName_byPath(subject_folder[s_i]));
                    if (sid_list == null || sid_list.Contains(s_current))
                    {
                        string[] motion_folder = Directory.GetDirectories(subject_folder[s_i]);
                        TheTool.sortList_StringWithNumeric(motion_folder);
                        int m_start = 0;
                        int m_end = motion_folder.Count();
                        for (int m_i = m_start; m_i < m_end; m_i++)
                        {
                            int m_current = TheTool.getInt(TheTool.getFileName_byPath(motion_folder[m_i]));
                            if (mid_list == null || mid_list.Contains(m_current))
                            {
                                string[] inst_file = Directory.GetFiles(motion_folder[m_i], "*.csv");
                                Array.Sort(inst_file);
                                for (int i_i = 0; i_i < inst_file.Count(); i_i++)
                                {
                                    Instance inst = new Instance();
                                    inst.subject_id = s_current;
                                    inst.motion_id = m_current;
                                    inst.path = inst_file[i_i];
                                    inst.name = TheTool.getFileName_byPath(inst.path);
                                    if (loadData) { inst.getDataRaw(true); }
                                    inst_container.list_inst.Add(inst);
                                    if (inst_container.list_sid.Contains(s_current) == false)
                                    {
                                        inst_container.list_sid.Add(s_current);
                                    }
                                    if (inst_container.list_mid.Contains(m_current) == false)
                                    {
                                        inst_container.list_mid.Add(m_current);
                                    }
                                }
                            }
                        }
                    }
                } 
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return inst_container;
        }

        public static void instanceDB_fileRenaming()
        {
            try
            {
                string[] subject_folder = Directory.GetDirectories(path_database);
                for (int s_i = 0; s_i < subject_folder.Count(); s_i++)
                {
                    string tag_subject = "{S" 
                        + TheTool.getTxt_Numeric_FillBy0(TheTool.getFileName_byPath(subject_folder[s_i]),2) 
                        + "}";
                    string[] motion_folder = Directory.GetDirectories(subject_folder[s_i]);
                    for (int m_i = 0; m_i < motion_folder.Count(); m_i++)
                    {
                        string tag_motion = "{M" 
                            + TheTool.getTxt_Numeric_FillBy0(TheTool.getFileName_byPath(motion_folder[m_i]),2) 
                            + "} ";
                        string[] inst_file = Directory.GetFiles(motion_folder[m_i], "*.csv");
                        for (int i_i = 0; i_i < inst_file.Count(); i_i++)
                        {
                            string path_oldFullPath = inst_file[i_i];
                            string path_directory = Path.GetDirectoryName(path_oldFullPath);
                            string file_name = TheTool.getFileName_byPath(path_oldFullPath);
                            string[] file_name_splitted = TheTool.splitText(file_name,"} ");
                            string file_name_clean = file_name_splitted.Last();
                            //-----------
                            string path_newFullPath = path_directory + @"\"
                                + tag_subject + tag_motion + file_name_clean + ".csv";
                            System.IO.File.Move(path_oldFullPath, path_newFullPath);
                        }
                    }
                } 
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        //************************************************************************
        //******* Free Style Usage ***********************************************

        public static Instance load1Instance_fromPath(string path)
        {
            Instance inst = new Instance();
            inst.path = path;
            inst.name = TheTool.getFileName_byPath(inst.path);
            inst.getDataRaw(true);
            return inst;
        }

        public static List<Instance> getInst_bySubject(List<Instance> list_inst, int sid, Boolean equal)
        {
            return list_inst.Where(i => i.subject_id.Equals(sid) == equal).ToList();
        }

        public static List<Instance> getInst_byMotion(List<Instance> list_inst, int mid, Boolean equal)
        {
            return list_inst.Where(i => i.motion_id.Equals(mid) == equal).ToList();
        }


    }
}
