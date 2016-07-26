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
        public List<int> list_sid = new List<int>();
        public List<int> list_mid = new List<int>();
        public void sortBySID() { list_inst.OrderBy(x => x.subject_id); }
        public void sortByMID() { list_inst.OrderBy(x => x.motion_id); }
    }

        

    class Instance
    {
        public string path = "";//original file path
        public string path_key = "";
        public string path_keyJ = "";
        public string name = "";//name from path, no extension
        public int subject_id = 0;
        public int motion_id = 0;
        public int skip = 0;//skip data (row)
        // Below are loaded on request --------------
        //Head,ShoulderCenter,ShoulderLeft,ShoulderRight,ElbowLeft,ElbowRight,WristLeft,WristRight,HandLeft,HandRight,Spine,HipCenter,HipLeft,HipRight,KneeLeft,KneeRight,AnkleLeft,AnkleRight,FootLeft,FootRight
        public List<UKI_DataRaw> data_raw = new List<UKI_DataRaw>();//Sequantial RAW data for the whole motion file
        public List<UKI_DataMovement> data_movement_adj = new List<UKI_DataMovement>();//already adjusted
        public List<int[]> keyPose = new List<int[]>();//e.g. 1,12
        public List<keyPoseGT> keyPoseGT = new List<keyPoseGT>();//ground truth (correct range), e.g. 1-10,11-20
        public List<int_double> keyPoseJump = new List<int_double>();//KeyPose that Jump is detected (for debug PosExtract)
        public List<UKI_DataDouble> data_norm = new List<UKI_DataDouble>();//List of Norm (X,Y,Z to norm)
        //-------------
        public int motion_id_predicted = 0;//No Usage yet
        public Boolean extraFeature_isAvailable = false;

        //load on request
        public List<UKI_DataRaw> getDataRaw(Boolean extraFeature)
        {
            if (this.data_raw.Count == 0)
            {
                this.data_raw = TheUKI.csv_loadFileTo_DataRaw(path, skip); 
            }
            if(extraFeature_isAvailable == false){
                TheUKI.DataRaw_AddExtraFeature(this.data_raw);
                extraFeature_isAvailable = true;
            }
            return this.data_raw;
        }

        public List<UKI_DataDouble> getDataNorm()
        {
            this.data_norm = new List<UKI_DataDouble>();
            foreach (UKI_DataRaw raw in getDataRaw(false))
            {
                UKI_DataDouble d = new UKI_DataDouble();
                d.id = raw.id;
                d.data.Add(TheTool.calNorm(raw.Head));
                d.data.Add(TheTool.calNorm(raw.ShoulderCenter));
                d.data.Add(TheTool.calNorm(raw.ShoulderLeft));
                d.data.Add(TheTool.calNorm(raw.ShoulderRight));
                d.data.Add(TheTool.calNorm(raw.ElbowLeft));
                d.data.Add(TheTool.calNorm(raw.ElbowRight));
                d.data.Add(TheTool.calNorm(raw.WristLeft));
                d.data.Add(TheTool.calNorm(raw.WristRight));
                d.data.Add(TheTool.calNorm(raw.HandLeft));
                d.data.Add(TheTool.calNorm(raw.HandRight));
                d.data.Add(TheTool.calNorm(raw.Spine));
                d.data.Add(TheTool.calNorm(raw.HipCenter));
                d.data.Add(TheTool.calNorm(raw.HipLeft));
                d.data.Add(TheTool.calNorm(raw.HipRight));
                d.data.Add(TheTool.calNorm(raw.KneeLeft));
                d.data.Add(TheTool.calNorm(raw.KneeRight));
                d.data.Add(TheTool.calNorm(raw.AnkleLeft));
                d.data.Add(TheTool.calNorm(raw.AnkleRight));
                d.data.Add(TheTool.calNorm(raw.FootLeft));
                d.data.Add(TheTool.calNorm(raw.FootRight));
                this.data_norm.Add(d);
            }
            return this.data_norm;
        }

        public List<UKI_DataMovement> getDataMove()
        {
            if (this.data_movement_adj.Count == 0)
            {
                UKI_Offline uki = new UKI_Offline();
                uki.UKI_OfflineProcessing(getDataRaw(true), -1);//previously use -1
                this.data_movement_adj = TheUKI.adjustMovementData(uki.data.data_movement);
            }
            return this.data_movement_adj;
        }

        public List<int[]> getKeyPose()
        {
            return getKeyPose(false,false);
        }

        //recompute = compute even data is already exist
        public List<int[]> getKeyPose(Boolean recompute, Boolean saveData)
        {
            if (Path.GetExtension(path_key) != ThePosExtract.extension_key)
            {
                path_key = ""; this.keyPose.Clear();
            }
            if (recompute) { this.keyPose.Clear(); }
            if (this.keyPose.Count == 0)
            {
                if (path_key == "") {
                    path_key = TheTool.getFilePathExcludeExtension_byPath(path) + ThePosExtract.extension_key; 
                    path_keyJ = TheTool.getFilePathExcludeExtension_byPath(path) + ".j";
                }
                if (recompute == false)
                {
                    this.keyPose = TheUKI.loadKeyPose(path_key);// .key is exist
                    this.keyPoseJump = TheUKI.loadKeyJump(path_keyJ);
                }
                if (recompute || (this.keyPose.Count == 0 && !TheTool.checkPathExist(path_key)))
                {
                    if (recompute == false && !TheTool.checkPathExist(path_key)) { TheSys.showError("Not Found: " + path_key); }
                    this.keyPose = ThePosExtract.extractKeyPose(this);
                    this.keyPoseJump = new List<int_double>();
                    this.keyPoseJump.AddRange(ThePosExtract.list_jump_selected);
                    if (saveData) { 
                        TheUKI.exportKey(path_key, keyPose);
                        if (ThePosExtract.capJumping) { TheUKI.exportKeyJ(path_keyJ, keyPoseJump); }
                    }
                }
            }
            return keyPose;
        }

        public List<keyPoseGT> getKeyPoseGT(Boolean reload)
        {
            if (keyPoseGT.Count() == 0 || reload)
            {
                string path_gt = TheTool.getDirectory_byPath(path) + @"\" + TheTool.getFileName_byPath(path) + ".gt";
                keyPoseGT = TheUKI.loadKeyPoseGT(path_gt);
            }
            if (TheUKI.captureJump && motion_id == 1)
            {
                foreach (keyPoseGT gt in keyPoseGT) {
                    int end_avg = (gt.end[0] + gt.end[1]) / 2;
                    gt.end[0] = end_avg;
                    gt.end[1] = end_avg;
                }
            }
            return this.keyPoseGT;
        }
        
    }



    class TheInstanceContainer
    {

        //************************************************************************
        //******* Database Usage ***********************************************
        //Full Load

        public static string path_database = TheURL.url_saveFolder + TheURL.url_9_ukiInst;

        //show only indentity, load data only when perform analysis
        public static InstanceContainer loadInstanceList_fromDB(InstanceContainer inst_container_old, Boolean loadData, List<int> sid_list, List<int> mid_list)
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
            container_reuse(inst_container, inst_container_old);
            return inst_container;
        }

        public static void container_reuse(InstanceContainer container_new, InstanceContainer container_old)
        {
            try
            {
                if (container_old != null)
                {
                    for (int i = 0; i < container_new.list_inst.Count; i++) 
                    {
                        Instance p = container_old.list_inst.Find(s => s.path == container_new.list_inst[i].path);
                        if (p != null) { container_new.list_inst[i] = p; }
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
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


        //show only indentity, load data only when perform analysis
        public static InstanceContainer loadInstanceList_fromDBoth_NoSubFolder(InstanceContainer inst_container_old, Boolean loadData, string path_db, string txt_filter, int skip)
        {
            InstanceContainer inst_container = new InstanceContainer();
            try
            {
                if (!Directory.Exists(path_db))
                {
                    TheSys.showError("Target folder is not exist: " + path_db); return inst_container;
                }
                string[] inst_file = Directory.GetFiles(path_db, "*.csv");
                Array.Sort(inst_file);
                for (int i_i = 0; i_i < inst_file.Count(); i_i++)
                {
                    if (inst_file[i_i] == "" || inst_file[i_i].Contains(txt_filter))
                    {
                        Instance inst = new Instance();
                        inst.subject_id = 1;
                        inst.motion_id = 1;
                        inst.path = inst_file[i_i];
                        inst.name = TheTool.getFileName_byPath(inst.path);
                        inst.skip = skip;
                        if (loadData) { inst.getDataRaw(true); }
                        inst_container.list_inst.Add(inst);
                        inst_container.list_sid.Add(1);
                        inst_container.list_mid.Add(1);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            container_reuse(inst_container, inst_container_old);
            return inst_container;
        }

    }
}
