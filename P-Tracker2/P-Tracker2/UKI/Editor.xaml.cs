using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Win32;


namespace P_Tracker2
{
    public partial class Editor : Window
    {
        String file_path_map = "";
        String file_path_motion = TheURL.url_uki_motionDB;
        String file_path_event = TheURL.url_uki_eventDB;
        String file_path_temp = TheURL.url_0_root + TheURL.url_1_sys + @"temp_map.xml";

        public int tab_focus = 0;//see type below
        public static int tab_cont_map = 1;
        public static int tab_cont_motion = 2;
        public static int tab_cont_event = 3;
        public static int tab_cont_map_groupIF = 9;//special type

        //--- TVI data ---------------------------------
        TreeViewItem selected = null;
        int selected_lv = 0;
        int[] selected_loc = new int[0];
        TreeViewItem selected_parent = null;
        string selected_parentname = "";
        int selected_loc_fromParent = 0;
        //------------------------------------

        public Editor(String file_path_map0)
        {
            this.file_path_map = file_path_map0;
            InitializeComponent();
            list_motions = TheMapData.loadXML_motion(file_path_motion);
            buildTree_motion();
            list_events = TheMapData.loadXML_event(file_path_event);
            buildTree_event();
            reloadMap();
        }

        //reload XML and built
        void reloadMap()
        {
            if (file_path_map != "")
            {
                String filename = TheTool.getFileName_byPath(file_path_map);
                tabMap.Header= "MAP: " + filename;
                detectMap = TheMapData.loadXML_map(file_path_map);
                buildTree_map();
            }
        }

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_saveFolder + TheURL.url_9_UKIMap); }
            catch { }
        }

        //*********************************************************************************************************
        //******** Build Tree **************************************************************************************
        public MapData detectMap = new MapData();
        TreeViewItem tv_variable_root = new TreeViewItem() { };
        TreeViewItem tv_map_root = new TreeViewItem() { };
        //---------------------------------------------
        public List<m_Motion> list_motions = new List<m_Motion>();
        TreeViewItem tv_motion_root = new TreeViewItem() { };
        //---------------------------------------------
        public List<m_Event> list_events = new List<m_Event>();
        TreeViewItem tv_event_root = new TreeViewItem() { };
        //---------------------------------------------
        public String treeNode_Variables = @"<Variables>";
        public String treeNode_Maps = @"<Maps>";
        public String treeNode_Conditions = @"<Conditions>";
        public String treeNode_Motions = @"<Motions>";
        public String treeNode_Events = @"<Events>";

        //*********************************************************************************************************
        //***** Build Tree ****************************************************************************************

        public void buildTree_map()
        {
            try
            {
                treeViewer.Items.Clear();
                tv_variable_root = new TreeViewItem() { Header = treeNode_Variables };
                tv_map_root = new TreeViewItem() { Header = treeNode_Maps };
                //-----------
                foreach (m_Variable v in detectMap.variables)
                {
                    tv_variable_root.Items.Add(getTVI_variable_v(v));
                }
                //-----------
                foreach (m_Group g in detectMap.groups)
                {
                    TreeViewItem tv_group = new TreeViewItem() { Header = g.name };
                    //-------------------------------
                    TreeViewItem tv_group_i_head = new TreeViewItem() { Header = treeNode_Conditions };
                    foreach (m_If i in g.inputs)
                    {
                        tv_group_i_head.Items.Add(getTVI_input(i));
                    }
                    tv_group.Items.Add(tv_group_i_head);
                    tv_group_i_head.IsExpanded = g.expand_if;
                    //-------------------------------
                    foreach (m_Detection d in g.detections)
                    {
                        TreeViewItem tv_detection = getTVI_detection(d);
                        TreeViewItem tv_if_head = new TreeViewItem() { Header = treeNode_Conditions };
                        foreach (m_If i in d.inputs)
                        {
                            tv_if_head.Items.Add(getTVI_input(i));
                        }
                        tv_detection.Items.Add(tv_if_head);
                        tv_if_head.IsExpanded = d.expand_if;
                        //
                        TreeViewItem tv_then_head = new TreeViewItem() { Header = treeNode_Events };
                        foreach (m_Then o in d.outputs)
                        {
                            tv_then_head.Items.Add(getTVI_Output(o));
                        }
                        tv_detection.Items.Add(tv_then_head);
                        tv_then_head.IsExpanded = d.expand_then;
                        //
                        tv_group.Items.Add(tv_detection);
                        tv_detection.IsExpanded = d.expand;
                    }
                    tv_map_root.Items.Add(tv_group);
                    tv_group.IsExpanded = g.expand;
                }
                if (detectMap.expand_v) { tv_variable_root.IsExpanded = true; }
                if (detectMap.expand_map) { tv_map_root.IsExpanded = true; }
                treeViewer.Items.Add(tv_variable_root);
                treeViewer.Items.Add(tv_map_root);
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
        }
        
        public void buildTree_motion()
        {
            try
            {
                m_treeViewer.Items.Clear();
                tv_motion_root = new TreeViewItem() { Header = treeNode_Motions };
                foreach (m_Motion m in list_motions)
                {
                    TreeViewItem tv_motion = new TreeViewItem() { Header = m.name };
                    foreach (m_If i in m.inputs)
                    {
                        tv_motion.Items.Add(getTVI_input(i));
                    }
                    tv_motion_root.Items.Add(tv_motion);
                    tv_motion.IsExpanded = m.expand;
                }
                m_treeViewer.Items.Add(tv_motion_root);
                tv_motion_root.IsExpanded = true;
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
        }

         void buildTree_event()
        {
            try
            {
                e_treeViewer.Items.Clear();
                tv_event_root = new TreeViewItem() { Header = treeNode_Events };
                foreach (m_Event e in list_events)
                {
                    TreeViewItem tv_event = new TreeViewItem() { Header = e.name };
                    foreach (m_Then o in e.outputs)
                    {
                        tv_event.Items.Add(getTVI_Output(o));
                    }
                    tv_event_root.Items.Add(tv_event);
                    tv_event.IsExpanded = e.expand;
                }
                e_treeViewer.Items.Add(tv_event_root);
                tv_event_root.IsExpanded = true;
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
        }

        //reload tree and select at Location
        public void reloadTree_event(int[] loc)
        {
            buildTree_event();
            TreeViewItem tvi = getTVI_byLocation(loc, tv_event_root);
            tvi_focus(tvi);
        }

        public void reloadTree_motion(int[] loc)
        {
            buildTree_motion();
            TreeViewItem tvi = getTVI_byLocation(loc, tv_motion_root);
            tvi_focus(tvi);
        }

        public void reloadTree_map_variable(int[] loc)
        {
            buildTree_map();
            TreeViewItem tvi = getTVI_byLocation(loc, tv_variable_root);
            tvi_focus(tvi);
        }

        public void reloadTree_map_map(int[] loc)
        {
            buildTree_map();
            TreeViewItem tvi = getTVI_byLocation(loc, tv_map_root);
            tvi_focus(tvi);
        }

        //--- Tree View Item =-------------------
        
        TreeViewItem getTVI_variable_v(m_Variable v){
            string s = v.name + " = " + v.value;
            return new TreeViewItem(){Header = s};
        }

        TreeViewItem getTVI_detection(m_Detection d)
        {
            new TreeViewItem() { Header = d.name };
            string s = "";
            s = d.name;
            if (d.loop) { s += " (loop)"; }
            else { s += " (trigger)"; }
            if (d.priority) { s += " (priority)"; }
            return new TreeViewItem() { Header = s };
        }

        TreeViewItem getTVI_input(m_If i)
        {
            string s = "";
            if (i.type == TheMapData.if_type_BasicPose)
            {
                s = "[Atomic] ";
                if (i.opt == "!=") { s += "(NOT) "; }
                s += TheMapData.convert_getBasePoseDef(i.v, i.value);
            }
            else if (i.type == TheMapData.if_type_MotionDatabase)
            {
                s = "[DB] " + i.v;
            }
            else if (i.type == TheMapData.if_type_2Joint)
            {
                s = "[Joints] " + TheMapData.getRuleOptDef(i);
            }
            else if (i.type == TheMapData.if_type_Change)
            {
                s = "[Change] " + TheMapData.getChangeFromInitialdef(i);
            }
            else if (i.type == TheMapData.if_type_SphereAngle)
            {
                s = "[S-Angle] " + TheMapData.getSphereAngledef(i);
            }
            else if (i.type == TheMapData.if_type_FlexionAngle)
            {
                s = "[F-Angle] " + TheMapData.getFlexAngledef(i);
            }
            else if (i.type == TheMapData.if_type_Variable)
            {
                s = "[Variable] " + i.v + " " + i.opt + " " + i.value;
            }
            else if (i.type == TheMapData.if_type_Icon)
            {
                s = "(change icon";
                if (i.value_d > 0) { s += " for " + i.value_d + " sec"; }
                s += ")";
            }
            else if (i.type == TheMapData.if_type_Comment)
            {
                s += i.v;
            }
            else if (i.type == TheMapData.if_type_TimeAfterPose)
            {
                s += "(within " + i.value_d + " sec)";
            }
            return new TreeViewItem() { Header = s };
        }
        
        TreeViewItem getTVI_Output(m_Then o)
        {
            string s = "";
            if (o.type == TheMapData.then_type_Key)
            { 
                s = "[Key] " + o.key0;
                if (o.press == TheMapData.then_key_holdEoM) { s += " (hold until end-of-motion)"; }
                else if (o.press == TheMapData.then_key_up) { s += " (release)"; }
                else if (o.press == TheMapData.then_key_hold) { s += " (hold)"; }
                else { s += " (press)"; }
            }
            else if (o.type == TheMapData.then_type_Mouse)
            {
                s = "[Mouse] Move ";
                if (o.value == TheMapData.then_mouse_moveTo) { s += "to "; }
                else { s += "by "; }
                s += "X:" + o.x + " Y:" + o.y;
            }
            else if (o.type == TheMapData.then_type_EventDatabase)
            {
                s = "[DB] " + o.v;
            }
            else if (o.type == TheMapData.then_type_ReplaceKey)
            {
                s = "[ReplaceKey] ";
                if (o.v2 == "") { s += o.v + " -> " + o.v; }
                else { s += o.v + " -> " + o.v2; }
            }
            else if (o.type == TheMapData.then_type_Variable)
            {
                if(o.opt == "+"){ s = "[Variable] " + o.v + " += " + o.value;}
                else if(o.opt == "-"){ s = "[Variable] " + o.v + " -= " + o.value;}
                else { s = "[Variable] " + o.v + " " + o.opt + " " + o.value;}
            }
            else if (o.type == TheMapData.then_type_Icon)
            {
                s = "(change icon";
                if (o.value_d > 0) { s += " for " + o.value_d + " sec"; }
                s += ")";
            }
            else if (o.type == TheMapData.then_type_TimeWait)
            {
                s += "(wait " + o.value_d + " sec)";
            }
            else if (o.type == TheMapData.if_type_Comment)
            {
                s += o.v;
            }
            return new TreeViewItem() { Header = s };
        }

        public void tvi_focus(TreeViewItem selected)
        {
            if (selected != null) {
                tvi_expandPath(selected);
                selected.Focus(); selected.IsSelected = true;
                selected.BringIntoView();
            }
        }

        public void tvi_expandPath(TreeViewItem selected)
        {
            TreeViewItem parent = selected.Parent as TreeViewItem;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent as TreeViewItem;
            }
        }

        //*********************************************************************************************************
        //******** Access TVI *********************************************************************************************

        //get child of parent by index
        public TreeViewItem getTVI_byIndex(TreeViewItem parent, int target)
        {
            TreeViewItem output = null;
            int i = 0;
            foreach (TreeViewItem tvi in parent.Items)
            {
                output = tvi;
                if (target == i) { break; }
                i++;
            }
            return output;
        }

        public String TVI_getParentName(TreeViewItem child)
        {
            String s = "";
            try
            {
                TreeViewItem parent = child.Parent as TreeViewItem;
                if (parent != null) { s = parent.Header.ToString(); }
            }
            catch { }
            return s;
        }

        public String TVI_getGrandParentName(TreeViewItem child)
        {
            String s = "";
            try
            {
                TreeViewItem parent = child.Parent as TreeViewItem;
                if (parent != null) {
                    TreeViewItem grand = parent.Parent as TreeViewItem;
                    if (grand != null) { s = grand.Header.ToString(); }
                }
            }
            catch { }
            return s;
        }
     
        //level 0 = root
        private int getTVI_Level(TreeViewItem child)
        {
            int level = 0;
            try
            {
                while (child.Parent as TreeViewItem != null)
                {
                    level++;
                    child = child.Parent as TreeViewItem;
                }
            }
            catch { }
            return level;
        }

        //Return location from Root-Child01-Child0x, start at 0
        public int[] getTVI_Location(TreeViewItem child)
        {
            List<int> location = new List<int>();
            try
            {
                TreeViewItem parent = child.Parent as TreeViewItem;
                while (parent != null)
                {
                    int i = 0;
                    foreach (TreeViewItem tvi in parent.Items)
                    {
                        if (tvi == child) { location.Add(i); break; }
                        i++;
                    }
                    child = parent;
                    parent = child.Parent as TreeViewItem;
                }
                location.Reverse();
            }
            catch{}
            return location.ToArray();
        }

        //only from parent level
        public int getTVI_Location_fromParent(TreeViewItem child)
        {
            int loc = 0;
            TreeViewItem parent = child.Parent as TreeViewItem;
            if (parent != null)
            {
                foreach (TreeViewItem tvi in parent.Items)
                {
                    if (tvi == child) { break; }
                    loc++;
                }
            }
            return loc;
        }

        public TreeViewItem TVI_selectLastChild(int[] parent_loc, TreeViewItem root)
        {
            try
            {
                TreeViewItem new_selected = getTVI_byLocation(parent_loc, root);
                new_selected = new_selected.Items[new_selected.Items.Count - 1] as TreeViewItem;
                tvi_focus(new_selected);
                return new_selected;
            }
            catch (Exception ex) { TheSys.showError(ex); return null; }
        }

        public TreeViewItem TVI_selectChild(int[] parent_loc, TreeViewItem root, int child_i)
        {
            TreeViewItem new_selected = getTVI_byLocation(parent_loc, root);
            new_selected = new_selected.Items[child_i] as TreeViewItem;
            tvi_focus(new_selected);
            return new_selected;
        }

        //adj = 1 or -1
        public TreeViewItem TVI_getNextSibling(TreeViewItem self, int adj)
        {
            TreeViewItem output = null;
            TreeViewItem parent = self.Parent as TreeViewItem;
            int position = getTVI_Location_fromParent(self) + adj;
            if (position >= 0 && position < parent.Items.Count)
            {
                output = parent.Items[position] as TreeViewItem;
            }
            return output;
        }

        //*********************************************************************************************************
        //******** Add Data *********************************************************************************************

        public void addEvent(m_Event ev, int addIndex)
        {
            try
            {
                if (addIndex == 999)
                {
                    addIndex = list_events.Count();
                }
                list_events.Insert(addIndex, ev);
                buildTree_event();
                tvi_focus(getTVI_byIndex(tv_event_root, addIndex));
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void addMotion(m_Motion m, int addIndex)
        {
            try
            {
                if (addIndex == 999)
                {
                    addIndex = list_motions.Count();
                }
                list_motions.Insert(addIndex, m);
                buildTree_motion();
                tvi_focus(getTVI_byIndex(tv_motion_root, addIndex));
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void addVariable(m_Variable v, int addIndex)
        {
            try
            {
                if (addIndex == 999)
                {
                    addIndex = detectMap.variables.Count();
                }
                detectMap.expand_v = true;
                detectMap.variables.Insert(addIndex, v);
                buildTree_map();
                TreeViewItem lv1 = getTVI_byIndex(tv_variable_root, addIndex);
                tvi_focus(lv1);
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void addGroup(m_Group g, int addIndex)
        {
            try
            {
                if (addIndex == 999)
                {
                    addIndex = detectMap.groups.Count();
                }
                detectMap.expand_map = true;
                detectMap.groups.Insert(addIndex, g);
                buildTree_map();
                TreeViewItem lv1 = getTVI_byIndex(tv_map_root, addIndex);
                tvi_focus(lv1);
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void addDetection(m_Detection d, int addIndex)
        {
            try
            {
                int i_g = selected_loc[0];
                if (addIndex == 999)
                {
                    addIndex = detectMap.groups[i_g].detections.Count();
                }
                detectMap.groups[i_g].expand = true;
                detectMap.groups[i_g].detections.Insert(addIndex, d);
                buildTree_map();
                TreeViewItem lv1 = getTVI_byIndex(tv_map_root, i_g);
                TreeViewItem lv2 = getTVI_byIndex(lv1, addIndex + 1);// +<Condition>
                tvi_focus(lv2);
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void addThen(m_Then t, TreeViewItem self, int map_type, int addIndex)
        {
            try
            {
                if (map_type == tab_cont_event)
                {
                    int[] loc = getTVI_Location(self);//TheSys.testData(loc);
                    int target = getTVI_Location_fromParent(self);
                    m_Event e = list_events[target];
                    e.expand = true;
                    if (addIndex == 999)
                    {
                        //add at last child
                        e.outputs.Add(t);
                        buildTree_event();
                        TVI_selectLastChild(loc, tv_event_root);
                    }
                    else
                    {
                        e.outputs.Insert(addIndex,t);
                        loc = TheTool.array_addIndex(loc, addIndex);
                        reloadTree_event(loc);
                    }
                }
                else if (map_type == tab_cont_map)
                {
                    //Group-Detection-Condition-item
                    int[] loc = getTVI_Location(self);//TheSys.testData(loc);
                    detectMap.groups[loc[0]].detections[loc[1] - 1].expand_then = true;
                    if (addIndex == 999)
                    {
                        //add at last child
                        detectMap.groups[loc[0]].detections[loc[1] - 1].outputs.Add(t);
                        //-- Rebuilt & Reselect --
                        buildTree_map();
                        TVI_selectLastChild(loc, tv_map_root);
                    }
                    else
                    {
                        detectMap.groups[loc[0]].detections[loc[1] - 1].outputs.Insert(addIndex, t);
                        loc = TheTool.array_addIndex(loc, addIndex);
                        reloadTree_map_map(loc);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void addIf(m_If i, TreeViewItem self, int map_type, int addIndex)
        {
            try
            {
                if (map_type == tab_cont_motion)
                {
                    int[] loc = getTVI_Location(self);//TheSys.testData(loc);
                    int target = getTVI_Location_fromParent(self);
                    m_Motion m = list_motions[target];
                    m.expand = true;
                    if (addIndex == 999)
                    {
                        m.inputs.Add(i);
                        buildTree_motion();
                        TVI_selectLastChild(loc, tv_motion_root);

                    }
                    else
                    {
                        m.inputs.Insert(addIndex, i);
                        loc = TheTool.array_addIndex(loc, addIndex);
                        reloadTree_motion(loc);
                    }
                }
                else if (map_type == tab_cont_map)
                {
                    //Group-Detection-Condition-item
                    int[] loc = getTVI_Location(self);//TheSys.testData(loc);
                    detectMap.groups[loc[0]].detections[loc[1] - 1].expand_if = true;
                    if (addIndex == 999)
                    {
                        //add at last child
                        detectMap.groups[loc[0]].detections[loc[1] - 1].inputs.Add(i);
                        buildTree_map();
                        TVI_selectLastChild(loc, tv_map_root);
                    }
                    else
                    {
                        detectMap.groups[loc[0]].detections[loc[1] - 1].inputs.Insert(addIndex, i);
                        loc = TheTool.array_addIndex(loc, addIndex);
                        reloadTree_map_map(loc);
                    }
                }
                else if (map_type == tab_cont_map_groupIF)//Group-Condition-IF
                {
                    //Group-Condition-item
                    int[] loc = getTVI_Location(self);//TheSys.testData(loc);
                    detectMap.groups[loc[0]].expand_if = true;
                    if (addIndex == 999)
                    {
                        detectMap.groups[loc[0]].inputs.Add(i);
                        buildTree_map();
                        TVI_selectLastChild(loc, tv_map_root);
                    }
                    else
                    {
                        //TheSys.testData(loc); TheSys.showError("=" + addIndex);
                        detectMap.groups[loc[0]].inputs.Insert(addIndex, i);
                        loc = TheTool.array_addIndex(loc, addIndex);
                        reloadTree_map_map(loc);
                        //TheSys.testData(loc);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        public void reloadTree(int[] loc, int map_type)
        {
            if (map_type == tab_cont_motion)
            {
                reloadTree_motion(loc);
            }
            else if (map_type == tab_cont_event)
            {
                reloadTree_event(loc);
            }
            else if (map_type == tab_cont_map)
            {
                reloadTree_map_map(loc);
            }
            else if (map_type == tab_cont_map_groupIF)
            {
                reloadTree_map_map(loc);
            }
        }

        //*********************************************************************************************************
        //******** All by Group *********************************************************************************************

        //true if some item is selected
        Boolean calSelected_Event()
        {
            this.selected = e_treeViewer.SelectedItem as TreeViewItem;
            return calSelected_step2();
        }

        //true if some item is selected
        Boolean calSelected_Motion()
        {
            this.selected = m_treeViewer.SelectedItem as TreeViewItem;
            return calSelected_step2();
        }

        //true if some item is selected
        Boolean calSelected_Map()
        {
            this.selected = treeViewer.SelectedItem as TreeViewItem;
            return calSelected_step2();
        }

        Boolean calSelected_step2()
        {
            if (this.selected != null)
            {
                this.selected_lv = getTVI_Level(selected);
                this.selected_loc = getTVI_Location(selected);
                this.selected_parent = selected.Parent as TreeViewItem;
                this.selected_parentname = TVI_getParentName(selected);
                this.selected_loc_fromParent = getTVI_Location_fromParent(selected); 
                return true;
            }
            else {
                this.selected_lv = 0;
                this.selected_loc = new int[0];
                this.selected_parent = null;
                this.selected_parentname = "";
                this.selected_loc_fromParent = 0;
                return false; 
            }
        }

        void setPasteButton(Button button, object obj)
        {
            if (obj != null) { button.IsEnabled = true; }
            else { button.IsEnabled = false; }
        }

        //== EVENT  ================================================================

        public void event_deleteItem(int[] loc)
        {
            try
            {
                if (loc.Count() > 0 && loc[0] <= list_events.Count - 1)
                {
                    m_Event e = list_events[loc[0]];
                    if (loc.Count() > 1 && loc[1] <= e.outputs.Count - 1)
                    {
                        m_Then t = e.outputs[loc[1]];
                        if (loc.Count() == 2) { e.outputs.Remove(t); }
                    }
                    else { list_events.Remove(e); }
                }
            }
            catch (Exception ex) { TheSys.showError("Delete Error: " + ex.ToString()); }
        }

        public TreeViewItem getTVI_byLocation(int[] loc, TreeViewItem root)
        {
            TreeViewItem output = null;
            try
            {
                output = root;
                for (int i = 0; i < loc.Count(); i++)
                {
                    output = getTVI_byIndex(output, loc[i]);
                }
            }
            catch (Exception ex) { TheSys.showError("Target Item: " + ex.ToString()); }
            return output;
        }

        void saveXML_Event(String path)
        {
            String xml_data = @"<events>";
            foreach (m_Event ev in list_events)
            {
                xml_data += getXML_Event(ev);
                foreach (m_Then o in ev.outputs) { xml_data += getXML_Then(o); }
                xml_data += "</event>";
            }
            xml_data += "</events>";
            TheTool.saveXML(xml_data, path);
        }

        String getXML_Event(m_Event ev)
        {
            return @"<event name=""" + ev.name + @""" enabled=""" 
                + TheTool.convertBoolean_01(ev.enabled) + @""" expand=""" + TheTool.convertBoolean_01(ev.expand) + @""">";
        }

        String getXML_Then(m_Then o)
        {
            String txt = @"<then type=""" + o.type + @""" ";
            if (o.type == TheMapData.then_type_TimeWait) { txt += @"value_d=""" + o.value_d + @"""/>"; }
            else if (o.type == TheMapData.then_type_EventDatabase) { txt += @"v=""" + o.v + @"""/>"; }
            else if (o.type == TheMapData.then_type_Mouse) { 
                txt += @"value=""" + o.value + @""" x=""" + o.x + @""" y=""" + o.y + @"""/>";
            }
            else if (o.type == TheMapData.then_type_ReplaceKey) { txt += @"v=""" + o.v + @""" v2=""" + o.v2 + @"""/>"; }
            else if (o.type == TheMapData.then_type_Variable)
            {
                txt += @"v=""" + o.v + @""" opt=""" + o.opt + @""" value=""" + o.value + @"""/>";
            }
            else if (o.type == TheMapData.then_type_Icon) { txt += @"v=""" + o.v + @""" brush=""" + o.brush0 + @"""/>"; }
            else { txt += @"press=""" + o.press + @""" key=""" + o.key0 + @"""/>"; }
            return txt;
        }

        private void e_TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (TreeViewItem node in tv_event_root.Items) { 
                list_events[i].expand = node.IsExpanded;
                i++;
            } 
        }

        private void e_SelectionChanged(object sender, RoutedEventArgs e)
        {
            e_selection();
        }

        void e_selection()
        {
            if (calSelected_Event())
            {
                tab_focus = tab_cont_event;
                if (selected_lv > 0)
                {
                    e_butEdit.IsEnabled = true; e_butDel.IsEnabled = true;
                    e_butUp.IsEnabled = true; e_butDown.IsEnabled = true;
                    e_butCopy.IsEnabled = true;
                    if (selected_lv == 1) { setPasteButton(e_butPaste, copy_event); }
                    else if (selected_lv == 2) { setPasteButton(e_butPaste, copy_then); }
                }
                else
                {
                    e_butEdit.IsEnabled = false; e_butDel.IsEnabled = false;
                    e_butUp.IsEnabled = false; e_butDown.IsEnabled = false;
                    e_butCopy.IsEnabled = false; e_butPaste.IsEnabled = false;
                }
            }
        }

        private void e_butSave_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Save progress?"
                                        , "Save Events to Database", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                saveXML_Event(file_path_event);
            }
        }

        private void e_butReload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Any unsaved progress will be lost"
                                        , "Reload Events from Database", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                list_events = TheMapData.loadXML_event(file_path_event);
                buildTree_event();
            }
        }

        private void e_butDel_Click(object sender, RoutedEventArgs e)
        {
            calSelected_Event();
            event_deleteItem(selected_loc);
            reloadTree_event(selected_loc);
        }

        private void e_butEdit_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Event())
            {
                if (selected_lv == 1)
                {
                    m_Event ev = list_events[selected_loc_fromParent];
                    //
                    UKI_add_Head form = new UKI_add_Head(this, selected, UKI_add_Head.type_e_event, addIndex);
                    form.editEvent(ev);
                    form.Show();
                }
                else if (selected_lv == 2)
                {
                    int target_parent = getTVI_Location_fromParent(selected_parent);
                    m_Event ev = list_events[target_parent];
                    m_Then t = ev.outputs[selected_loc_fromParent];
                    //
                    editTHEN(t, tab_focus); 
                }
            }
        }

        void editTHEN(m_Then t, int maptype)
        {
            if (t.type == TheMapData.then_type_Key)
            {
                UKI_addThen_Key form = new UKI_addThen_Key(this, selected, maptype, addIndex);
                form.editThen_Key(t);
                form.Show();
            }
            else if (t.type == TheMapData.then_type_EventDatabase)
            {
                UKI_add_Database form = new UKI_add_Database(this, selected, maptype, false, addIndex);
                form.edit_Database(t);
                form.Show();
            }
            else if (t.type == TheMapData.then_type_ReplaceKey)
            {
                UKI_addThen_ReplaceKey form = new UKI_addThen_ReplaceKey(this, selected, maptype, addIndex);
                form.edit_ReplaceKey(t);
                form.Show();
            }
            else if (t.type == TheMapData.then_type_Icon)
            {
                UKI_add_Icon form = new UKI_add_Icon(this, selected, maptype, false, addIndex);
                form.edit_Icon(t);
                form.Show();
            }
            else if (t.type == TheMapData.then_type_TimeWait)
            {
                UKI_addThen_Time form = new UKI_addThen_Time(this, selected, maptype, addIndex);
                form.edit_Time(t);
                form.Show();
            }
            else if (t.type == TheMapData.then_type_Variable)
            {
                UKI_addThen_Variable form = new UKI_addThen_Variable(this, selected, maptype, addIndex);
                form.editThen_Variable(t);
                form.Show();
            }
        }

        private void e_butAdd_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Event())
            {
                check_addAt();
                if (selected_lv == 0)
                {
                    new UKI_add_Head(this, selected, UKI_add_Head.type_e_event, addIndex).Show();
                }
                else if (selected_lv > 0)
                {
                    (sender as Button).ContextMenu.IsEnabled = true;
                    (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
                    (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    (sender as Button).ContextMenu.IsOpen = true;
                }
            }
        }

        TreeViewItem addAt = null;
        int addIndex = 999;
        void check_addAt()
        {
            this.addAt = selected;
            this.addIndex = 999;
            if (
                (tab_focus == tab_cont_event && selected_lv == 2)
                || (tab_focus == tab_cont_motion && selected_lv == 2)
                || (tab_focus == tab_cont_map && selected_parentname == treeNode_Variables)
                || (tab_focus == tab_cont_map_groupIF && selected_parentname == treeNode_Conditions)
                || (tab_focus == tab_cont_map && selected_parentname == treeNode_Conditions)
                || (tab_focus == tab_cont_map && selected_parentname == treeNode_Events)
                )
            {
                //Last Level
                this.addAt = selected_parent;
                this.addIndex = selected_loc_fromParent + 1;
            }
            else if (tab_focus == tab_cont_map && TVI_getGrandParentName(selected) == treeNode_Maps)
            {
                //add Detection at Detection - exclude <Condition>
                this.addAt = selected_parent;
                this.addIndex = selected_loc_fromParent;
            }
            //TheSys.showError(addAt.Header + "_");
        }

        private void addEvent_Keyboard(object sender, RoutedEventArgs e)
        {
            new UKI_addThen_Key(this, addAt, tab_focus, addIndex).Show();
        }


        private void addEvent_MouseMove(object sender, RoutedEventArgs e)
        {
            new UKI_addThen_Mouse(this, addAt, tab_focus, addIndex).Show();
        }

        private void addEvent_Replacement(object sender, RoutedEventArgs e)
        {
            new UKI_addThen_ReplaceKey(this, addAt, tab_focus, addIndex).Show();
        }

        private void addEvent_Database(object sender, RoutedEventArgs e)
        {
            new UKI_add_Database(this, addAt, tab_focus, false, addIndex).Show();
        }

        private void addEvent_Icon(object sender, RoutedEventArgs e)
        {
            new UKI_add_Icon(this, addAt, tab_focus, false, addIndex).Show();
        }

        private void addEvent_Time(object sender, RoutedEventArgs e)
        {
            new UKI_addThen_Time(this, addAt, tab_focus, addIndex).Show();
        }

        private void addEvent_Variable(object sender, RoutedEventArgs e)
        {
            new UKI_addThen_Variable(this, addAt, tab_focus, addIndex).Show();
        }

        private void e_butOther_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void e_butSort_Click(object sender, RoutedEventArgs e)
        {
            list_events = list_events.OrderBy(o => o.name).ToList();
            buildTree_event();
        }

        private void e_butExAll_Click(object sender, RoutedEventArgs e)
        {
            TheTool.TVI_ExpandAll(tv_event_root, true, 0);
        }

        private void e_butColAll_Click(object sender, RoutedEventArgs e)
        {
            TheTool.TVI_ExpandAll(tv_event_root, false, 0);
        }
         
        //------------------------

        private void butDown_Click(object sender, RoutedEventArgs e)
        {
            map_Move(1);
        }

        private void butUp_Click(object sender, RoutedEventArgs e)
        {
            map_Move(-1);
        }

        void map_Move(int adj)
        {
            if (calSelected_Map())
            {
                if (selected_parentname == treeNode_Variables)
                {
                    map_move_Variable(adj);
                }
                else
                {
                    if (selected_parentname == treeNode_Maps)
                    {
                        map_move_Group(adj);
                    }
                    else if (selected_lv == 2)
                    {
                        map_move_Detection(adj);
                    }
                    else if (selected_lv == 3)
                    {
                        map_move_groupIf(adj);
                    }
                    else if (selected_lv == 4 && selected_parentname == treeNode_Conditions)
                    {
                        map_move_detectionIT(adj, true);
                    }
                    else if (selected_lv == 4 && selected_parentname == treeNode_Events)
                    {
                        map_move_detectionIT(adj, false);
                    }
                }
                completeReloadTree_map();//complete clone
            }
        }

        void map_move_Variable(int adj)
        {
            int target = selected_loc_fromParent;
            int new_target = selected_loc_fromParent + adj;
            if (new_target >= 0 && new_target < detectMap.variables.Count)
            {
                m_Variable v_temp = detectMap.variables[target];
                detectMap.variables.Remove(v_temp);
                detectMap.variables.Insert(new_target, v_temp);
                //Reload & Select
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target);
                reloadTree_map_variable(getTVI_Location(newTVI));
            }
        }

        void map_move_Group(int adj)
        {
            int target = selected_loc_fromParent;
            int new_target = selected_loc_fromParent + adj;
            if (new_target >= 0 && new_target < detectMap.groups.Count)
            {
                m_Group g_temp = detectMap.groups[target];
                detectMap.groups.Remove(g_temp);
                detectMap.groups.Insert(new_target, g_temp);
                //Reload & Select
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target);
                reloadTree_map_map(getTVI_Location(newTVI));
            }
        }

        void map_move_Detection(int adj)
        {
            int target = selected_loc_fromParent - 1;//exclude <Condition>
            int new_target = target + adj;
            m_Group g = detectMap.groups[selected_loc[0]];
            m_Detection d_temp = g.detections[target];
            if (new_target >= 0 && new_target < g.detections.Count)
            {
                g.detections.Remove(d_temp);
                g.detections.Insert(new_target, d_temp);
                //Reload & Select
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target + 1);
                reloadTree_map_map(getTVI_Location(newTVI));
            }
            else if (adj > 0)//next parent
            {
                int new_g_i = detectMap.groups.IndexOf(g) + 1;
                if (new_g_i < detectMap.groups.Count)
                {
                    g.detections.Remove(d_temp);
                    detectMap.groups[new_g_i].detections.Insert(0, d_temp);
                    //
                    TreeViewItem uncle = TVI_getNextSibling(selected_parent, 1);
                    buildTree_map();
                    TVI_selectChild(getTVI_Location(uncle), tv_map_root, 1);
                }
            }
            else if (adj < 0)//previous parent
            {
                int new_g_i = detectMap.groups.IndexOf(g) - 1;
                if (new_g_i >= 0)
                {
                    g.detections.Remove(d_temp);
                    detectMap.groups[new_g_i].detections.Add(d_temp);
                    //
                    TreeViewItem uncle = TVI_getNextSibling(selected_parent, -1);
                    buildTree_map();
                    TVI_selectLastChild(getTVI_Location(uncle), tv_map_root);
                }
            }
        }

        void map_move_groupIf(int adj)
        {
            TreeViewItem tvi_con = selected_parent;
            TreeViewItem tvi_group = tvi_con.Parent as TreeViewItem;
            int i_group = getTVI_Location_fromParent(tvi_group);
            int i_if = selected_loc_fromParent;
            int i_if_new = i_if + adj;
            m_Group g = detectMap.groups[i_group];
            m_If i = g.inputs[i_if];
            //
            if (i_if_new >= 0 && i_if_new < g.inputs.Count)
            {
                g.inputs.Remove(i);
                g.inputs.Insert(i_if_new, i);
                //
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, i_if_new);
                reloadTree_map_map(getTVI_Location(newTVI));
            }
            else if (adj > 0) //Pass to Next
            {
                int i_group_new = detectMap.groups.IndexOf(g) + 1;
                if (i_group_new < detectMap.groups.Count)
                {
                    g.inputs.Remove(i);
                    detectMap.groups[i_group_new].inputs.Insert(0, i);
                    //
                    TreeViewItem new_group = TVI_getNextSibling(tvi_group, 1);
                    TreeViewItem new_con = getTVI_byIndex(new_group, 0);
                    buildTree_map();
                    TVI_selectChild(getTVI_Location(new_con), tv_map_root, 0);
                }
            }
            else if (adj < 0) //previous Exist
            {
                int i_group_new = detectMap.groups.IndexOf(g) - 1;
                if (i_group_new >= 0)
                {
                    g.inputs.Remove(i);
                    detectMap.groups[i_group_new].inputs.Add(i);
                    //
                    TreeViewItem new_group = TVI_getNextSibling(tvi_group, -1);
                    TreeViewItem new_con = getTVI_byIndex(new_group, 0);
                    buildTree_map();
                    TVI_selectLastChild(getTVI_Location(new_con), tv_map_root);
                }
            }
        }

        void map_move_detectionIT(int adj, Boolean isIF)
        {
            TreeViewItem tvi_con = selected_parent;
            TreeViewItem tvi_detect = tvi_con.Parent as TreeViewItem;
            TreeViewItem tvi_group = tvi_detect.Parent as TreeViewItem;
            int i_group = getTVI_Location_fromParent(tvi_group);
            int i_detect = getTVI_Location_fromParent(tvi_detect) - 1;//exclude <Condition>
            int i_this = selected_loc_fromParent;
            int i_this_new = i_this + adj;
            m_Group g = detectMap.groups[i_group];
            m_Detection d = g.detections[i_detect];
            //---------------------------
            m_If i = null; m_Then t = null;
            if (isIF) { i = d.inputs[i_this]; } else { t = d.outputs[i_this]; }
            int d_count = 0;
            if (isIF) { d_count = d.inputs.Count; } else { d_count = d.outputs.Count; }
            //
            if (i_this_new >= 0 && i_this_new < d_count)
            {
                if (isIF) { d.inputs.Remove(i); d.inputs.Insert(i_this_new, i); }
                else { d.outputs.Remove(t); d.outputs.Insert(i_this_new, t); }
                //
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, i_this_new);
                reloadTree_map_map(getTVI_Location(newTVI));
            }
            else if (adj > 0) //Pass to Next
            {
                int i_detect_new = i_detect + 1;
                if (i_detect_new < g.detections.Count)
                {
                    m_Detection d_new = g.detections[i_detect_new];
                    if (isIF) { d.inputs.Remove(i); d_new.inputs.Insert(0, i); }
                    else { d.outputs.Remove(t); d_new.outputs.Insert(0, t); }
                    //
                    TreeViewItem new_detect = TVI_getNextSibling(tvi_detect, 1);
                    int k = 0; if (!isIF) { k = 1; } 
                    TreeViewItem new_parent = getTVI_byIndex(new_detect, k);
                    buildTree_map();
                    TVI_selectChild(getTVI_Location(new_parent), tv_map_root, 0);
                }
                else
                {
                    int i_group_new = i_group + 1;
                    if (i_group_new < detectMap.groups.Count)
                    {
                        m_Group g_new = detectMap.groups[i_group_new];
                        if (g_new.detections.Count > 0)
                        {
                            if (isIF) { d.inputs.Remove(i); g_new.detections[0].inputs.Insert(0, i); }
                            else { d.outputs.Remove(t); g_new.detections[0].outputs.Insert(0, t); }
                            //
                            TreeViewItem new_group = TVI_getNextSibling(tvi_group, 1);
                            TreeViewItem new_detect = getTVI_byIndex(new_group, 1);//skip <Condition>
                            int k = 0; if (!isIF) { k = 1; } 
                            TreeViewItem new_parent = getTVI_byIndex(new_detect, k);
                            buildTree_map();
                            TVI_selectChild(getTVI_Location(new_parent), tv_map_root, 0);
                        }
                    }
                }
            }
            else if (adj < 0) //Back to previous
            {
                int i_detect_new = i_detect - 1;
                if (i_detect_new >= 0)
                {
                    m_Detection d_new = g.detections[i_detect_new];
                    if (isIF) { d.inputs.Remove(i); d_new.inputs.Add(i); }
                    else { d.outputs.Remove(t); d_new.outputs.Add(t); }
                    //
                    //
                    TreeViewItem new_detect = TVI_getNextSibling(tvi_detect, -1);
                    int k = 0; if (!isIF) { k = 1; }
                    TreeViewItem new_parent = getTVI_byIndex(new_detect, k);
                    buildTree_map();
                    TVI_selectLastChild(getTVI_Location(new_parent), tv_map_root);
                }
                else
                {
                    int i_group_new = i_group - 1;
                    if (i_group_new >= 0)
                    {
                        m_Group g_new = detectMap.groups[i_group_new];
                        int g_new_detectionCount = g_new.detections.Count;
                        if (g_new_detectionCount > 0)
                        {
                            if (isIF)
                            {
                                d.inputs.Remove(i);
                                g_new.detections[g_new_detectionCount - 1].inputs.Add(i);
                            }
                            else { 
                                d.outputs.Remove(t);
                                g_new.detections[g_new_detectionCount - 1].outputs.Add(t);
                            }
                            //
                            TreeViewItem new_group = TVI_getNextSibling(tvi_group, -1);
                            TreeViewItem new_detect = getTVI_byIndex(new_group, g_new_detectionCount + 1);//skip <Condition>
                            int k = 0; if (!isIF) { k = 1; }
                            TreeViewItem new_con = getTVI_byIndex(new_detect, k);
                            buildTree_map();
                            TVI_selectLastChild(getTVI_Location(new_con), tv_map_root);
                        }
                    }
                }
            }
        }

        //== MOTION  ================================================================

        public void motion_deleteItem(int[] loc)
        {
            try
            {
                if (loc.Count() > 0 && loc[0] <= list_motions.Count - 1)
                {
                    m_Motion m = list_motions[loc[0]];
                    if (loc.Count() > 1 && loc[1] <= m.inputs.Count - 1)
                    {
                        m_If i = m.inputs[loc[1]];
                        if (loc.Count() == 2) { m.inputs.Remove(i); }
                    }
                    else { list_motions.Remove(m); }
                }
            }
            catch (Exception ex) { TheSys.showError("Delete Error: " + ex.ToString()); }
        }

        

        private void m_TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (TreeViewItem node in tv_motion_root.Items)
            {
                list_motions[i].expand = node.IsExpanded;
                i++;
            }
        }

        private void m_SelectionChanged(object sender, RoutedEventArgs e)
        {
            m_selection();
        }

        void m_selection()
        {
            if (calSelected_Motion())
            {
                tab_focus = tab_cont_motion;
                if (selected_lv > 0)
                {
                    m_butEdit.IsEnabled = true; m_butDel.IsEnabled = true;
                    m_butUp.IsEnabled = true; m_butDown.IsEnabled = true;
                    m_butCopy.IsEnabled = true;
                    if (selected_lv == 1) { setPasteButton(m_butPaste, copy_motion); }
                    else if (selected_lv == 2) { setPasteButton(m_butPaste, copy_if); }
                }
                else
                {
                    m_butEdit.IsEnabled = false; m_butDel.IsEnabled = false;
                    m_butUp.IsEnabled = false; m_butDown.IsEnabled = false;
                    m_butCopy.IsEnabled = false; m_butPaste.IsEnabled = false;
                }
            }
        }

        private void m_butReload_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Any unsaved progress will be lost"
                                                    , "Reload Motion from Database", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                list_motions = TheMapData.loadXML_motion(file_path_motion);
                buildTree_motion();
            }
        }

        private void m_butSave_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Save progress?"
                                        , "Save Motion to Database", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                TheMapData.saveXML_Motion(file_path_motion, list_motions);
            }
        }

        private void m_butDel_Click(object sender, RoutedEventArgs e)
        {
            calSelected_Motion();
            motion_deleteItem(selected_loc);
            reloadTree_motion(selected_loc);
        }

        private void m_butEdit_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Motion())
            {
                if (selected_lv == 1)
                {
                    UKI_add_Head form = new UKI_add_Head(this, selected, UKI_add_Head.type_m_motion, addIndex);
                    m_Motion m = list_motions[selected_loc_fromParent];
                    form.editMotion(m);
                    form.Show();
                }
                else if (selected_lv == 2)
                {
                    TreeViewItem selected_parent = selected.Parent as TreeViewItem;
                    int target_parent = getTVI_Location_fromParent(selected_parent);
                    m_Motion m = list_motions[target_parent];
                    m_If i = m.inputs[selected_loc_fromParent];
                    editIF(i, tab_focus);
                }
            }
        }

        private void editIF(m_If i, int maptype)
        {
            if (i.type == TheMapData.if_type_BasicPose)
            {
                UKI_addIf_Atomic form = new UKI_addIf_Atomic(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_MotionDatabase)
            {
                UKI_add_Database form = new UKI_add_Database(this, selected, maptype, true, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_2Joint)
            {
                UKI_addIf_Relative form = new UKI_addIf_Relative(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_Change)
            {
                UKI_addIf_Change form = new UKI_addIf_Change(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_Icon)
            {
                UKI_add_Icon form = new UKI_add_Icon(this, selected, maptype, true, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_TimeAfterPose)
            {
                UKI_addIf_Time form = new UKI_addIf_Time(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_Variable)
            {
                UKI_addIf_Variable form = new UKI_addIf_Variable(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_FlexionAngle)
            {
                UKI_addIf_FlexionAngle form = new UKI_addIf_FlexionAngle(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
            else if (i.type == TheMapData.if_type_SphereAngle)
            {
                UKI_addIf_SphereAngle form = new UKI_addIf_SphereAngle(this, selected, maptype, addIndex);
                form.editIf(i);
                form.Show();
            }
        }

        private void m_butAdd_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Motion())
            {
                check_addAt();
                if (selected_lv == 0)
                {
                    new UKI_add_Head(this, selected, UKI_add_Head.type_m_motion, addIndex).Show();
                }
                else if (selected_lv > 0)
                {
                    (sender as Button).ContextMenu.IsEnabled = true;
                    (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
                    (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    (sender as Button).ContextMenu.IsOpen = true;
                }
            }
        }
        
        private void addMotion_Basic(object sender, RoutedEventArgs e) {
            new UKI_addIf_Atomic(this, addAt, tab_focus, addIndex).Show();
        }

        private void addMotion_Position(object sender, RoutedEventArgs e) {
            new UKI_addIf_Relative(this, addAt, tab_focus, addIndex).Show();
        }

        private void addMotion_AngleSpherical(object sender, RoutedEventArgs e)
        {
            new UKI_addIf_SphereAngle(this, addAt, tab_focus, addIndex).Show();
        }

        private void addMotion_AngleFlexion(object sender, RoutedEventArgs e)
        {
            new UKI_addIf_FlexionAngle(this, addAt, tab_focus, addIndex).Show();
        }

        private void addMotion_ChangeBasePose(object sender, RoutedEventArgs e)
        {
            new UKI_addIf_Change(this, addAt, tab_focus, addIndex).Show();
        }

        private void addMotion_Database(object sender, RoutedEventArgs e)
        {
            new UKI_add_Database(this, addAt, tab_focus, true, addIndex).Show();
        }

        private void addMotion_Icon(object sender, RoutedEventArgs e)
        {
            new UKI_add_Icon(this, addAt, tab_focus, true, addIndex).Show();
        }

        private void addMotion_Time(object sender, RoutedEventArgs e) 
        {
            new UKI_addIf_Time(this, addAt, tab_focus, addIndex).Show();
        }

        private void addMotion_Variable(object sender, RoutedEventArgs e)
        {
            new UKI_addIf_Variable(this, addAt, tab_focus, addIndex).Show();
        }
        
        private void m_butOther_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void m_butSort_Click(object sender, RoutedEventArgs e)
        {
            list_motions = list_motions.OrderBy(o => o.name).ToList();
            buildTree_motion();
        }

        private void m_butExAll_Click(object sender, RoutedEventArgs e)
        {
            TheTool.TVI_ExpandAll(tv_motion_root, true, 0);
        }

        private void m_butColAll_Click(object sender, RoutedEventArgs e)
        {
            TheTool.TVI_ExpandAll(tv_motion_root, false, 0);
        }

        private void m_butDown_Click(object sender, RoutedEventArgs e)
        {
            m_Move(1);
        }

        private void m_butUp_Click(object sender, RoutedEventArgs e)
        {
            m_Move(-1);
        }

        void m_Move(int adj)
        {
            if (calSelected_Motion())
            {
                if (selected_lv == 1)
                {
                    m_move_Motion(adj);
                }
                else if (selected_lv == 2)
                {
                    m_move_If(adj);
                }
            }
        }

        void m_move_Motion(int adj)
        {
            int target = selected_loc_fromParent;
            int new_target = selected_loc_fromParent + adj;
            if (new_target >= 0 && new_target < list_motions.Count)
            {
                m_Motion m_temp = list_motions[target];
                list_motions.Remove(m_temp);
                list_motions.Insert(new_target, m_temp);
                //Reload & Select
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target);
                reloadTree_motion(getTVI_Location(newTVI));
            }
        }

        void m_move_If(int adj)
        {
            int target = selected_loc_fromParent;
            int new_target = selected_loc_fromParent + adj;
            int target_parent = getTVI_Location_fromParent(selected_parent);
            m_Motion m = list_motions[target_parent];
            m_If i = m.inputs[target];
            //
            if (new_target >= 0 && new_target < m.inputs.Count)
            {
                m.inputs.Remove(i);
                m.inputs.Insert(new_target, i);
                //
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target);
                reloadTree_motion(getTVI_Location(newTVI));
            }
            else if (adj > 0) //Pass to next Motion
            {
                int new_m_i = list_motions.IndexOf(m) + 1;
                if (new_m_i < list_motions.Count)
                {
                    m.inputs.Remove(i);
                    list_motions[new_m_i].inputs.Insert(0, i);
                    //
                    TreeViewItem uncle = TVI_getNextSibling(selected_parent, 1);
                    buildTree_motion();
                    TVI_selectChild(getTVI_Location(uncle), tv_motion_root, 0);
                }
            }
            else if (adj < 0) //previous Exist
            {
                int new_m_i = list_motions.IndexOf(m) - 1;
                if (new_m_i >= 0)
                {
                    m.inputs.Remove(i);
                    list_motions[new_m_i].inputs.Add(i);
                    //
                    TreeViewItem uncle = TVI_getNextSibling(selected_parent, -1);
                    buildTree_motion();
                    TVI_selectLastChild(getTVI_Location(uncle), tv_motion_root);
                }
            }
        }

        //== MAP  ================================================================

        void saveXML_Map(String path)
        {
            String xml_data = @"<mapping>";
            xml_data += @"<variable expand=""" + TheTool.convertBoolean_01(detectMap.expand_v) + @""">";
            foreach (m_Variable v in detectMap.variables)
            {
                xml_data += getXML_Variable(v);
            }
            xml_data += @"</variable>";
            xml_data += @"<map expand=""" + TheTool.convertBoolean_01(detectMap.expand_map) + @""">";
            foreach (m_Group g in detectMap.groups)
            {
                xml_data += getXML_Group(g);
                foreach (m_If gi in g.inputs)
                {
                    xml_data += TheMapData.getXML_If(gi);
                }
                foreach (m_Detection d in g.detections)
                {
                    xml_data += getXML_Detection(d);
                    foreach (m_If i in d.inputs)
                    {
                        xml_data += TheMapData.getXML_If(i);
                    }
                    foreach (m_Then o in d.outputs)
                    {
                        xml_data += getXML_Then(o);
                    }
                    xml_data += "</detection>";
                }
                xml_data += "</group>";
            }
            xml_data += "</map>";
            xml_data += "</mapping>";
            //----------
            TheTool.saveXML(xml_data, path);
        }

        String getXML_Detection(m_Detection d)
        {
            return @"<detection name=""" + d.name 
                + @""" loop=""" + TheTool.convertBoolean_01(d.loop)
                + @""" priority=""" + TheTool.convertBoolean_01(d.priority)
                + @""" expand=""" + TheTool.convertBoolean_01(d.expand)
                + @""" expand_if=""" + TheTool.convertBoolean_01(d.expand_if)
                + @""" expand_then=""" + TheTool.convertBoolean_01(d.expand_then) 
                + @""">";
        }


        String getXML_Group(m_Group g)
        {
            return @"<group name=""" + g.name
                + @""" enabled=""" + TheTool.convertBoolean_01(g.enabled)
                + @""" expand=""" + TheTool.convertBoolean_01(g.expand)
                + @""" expand_if=""" + TheTool.convertBoolean_01(g.expand_if) 
                + @""">";
        }

        String getXML_Variable(m_Variable v)
        {
            return @"<v name=""" + v.name + @""" value=""" + v.value + @"""/>";
        }
        
        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MAP file (*.xml)|*.xml";
            saveFileDialog.FileName = TheTool.getFileName_byPath(file_path_map);
            if (saveFileDialog.ShowDialog() == true)
            {
                 saveXML_Map(saveFileDialog.FileName);
                 tabMap.Header = "MAP: " + TheTool.getFileName_byPath(saveFileDialog.FileName);
                 file_path_map = saveFileDialog.FileName;
            }

        }

        private void butReload_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".xml", 
                TheURL.url_saveFolder + TheURL.url_9_UKIMap);
            if (openDialog == true)
            {
                try
                {
                    file_path_map = TheTool.dialog.FileName;
                    reloadMap();
                    TheMapData.checkMissingDatabase(detectMap,list_motions,list_events);
                }
                catch (Exception ex) { TheSys.showError(ex); }
            }
        }

        private void butOther_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void butExAll_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Map()) { TheTool.TVI_ExpandAll(selected, true, 0); }
        }

        private void butColAll_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Map()) { TheTool.TVI_ExpandAll(selected, false, 0); }
        }

        private void butLocalize_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Replace ALL [DB] with data from the database?"
                                        , "Localize Map to be Stand-Alone ", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                detectMap = TheMapData.getFullXML(detectMap, list_motions, list_events);
                buildTree_map();
            }
        }

        private void butDecodeBP_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Decode ALL [P] to [Joint]"
                                        , "Localize Map to be Stand-Alone ", System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                detectMap = TheMapData.getDecodeBP(detectMap);
                buildTree_map();
            }
        }

        //-------

        private void map_TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                detectMap.expand_v = tv_variable_root.IsExpanded;
                detectMap.expand_map = tv_map_root.IsExpanded;
                for (int i_g = 0; i_g < tv_map_root.Items.Count; i_g++) {
                    TreeViewItem node_g = tv_map_root.Items[i_g] as TreeViewItem;
                    m_Group g = detectMap.groups[i_g];
                    g.expand = node_g.IsExpanded;
                    //
                    TreeViewItem node_g_if = node_g.Items[0] as TreeViewItem;
                    g.expand_if = node_g_if.IsExpanded;
                    //
                    for (int i_d = 1; i_d < node_g.Items.Count; i_d++)
                    {
                        TreeViewItem node_d = node_g.Items[i_d] as TreeViewItem;
                        m_Detection d = g.detections[i_d - 1];
                        d.expand = node_d.IsExpanded;
                        //
                        TreeViewItem node_d_if = node_d.Items[0] as TreeViewItem;
                        d.expand_if = node_d_if.IsExpanded;
                        //
                        TreeViewItem node_d_then = node_d.Items[1] as TreeViewItem;
                        d.expand_then = node_d_then.IsExpanded;
                    }
                }
            }
            catch { }
        }

        private void map_SelectionChanged(object sender, RoutedEventArgs e)
        {
            map_selection();
        }

        void map_selection() {
            try
            {
                if (calSelected_Map())
                {
                    tab_focus = tab_cont_map;//may be changed later
                    if ((selected.Header == treeNode_Conditions && selected_lv == 2)
                        || (selected_parentname == treeNode_Conditions && selected_lv == 3))
                    {
                        tab_focus = tab_cont_map_groupIF;
                    }
                    //--------------
                    if (selected.Header == treeNode_Variables
                        || selected.Header == treeNode_Maps
                        || selected.Header == treeNode_Conditions
                        || selected.Header == treeNode_Events
                        || selected.Header == treeNode_Maps
                        )
                    {
                        butEdit.IsEnabled = false; butDel.IsEnabled = false;
                        butUp.IsEnabled = false; butDown.IsEnabled = false;
                        butCopy.IsEnabled = false;
                    }
                    else
                    {
                        butEdit.IsEnabled = true; butDel.IsEnabled = true;
                        butUp.IsEnabled = true; butDown.IsEnabled = true;
                        butCopy.IsEnabled = true;
                    }
                    //------------------------
                    if (selected.Header == treeNode_Conditions
                        || (selected_parentname == treeNode_Conditions))
                    {
                        butAdd.Visibility = Visibility.Hidden;
                        butAdd_if.Visibility = Visibility.Visible;
                        butAdd_then.Visibility = Visibility.Hidden;
                        setPasteButton(butPaste, copy_if);
                    }
                    else if (selected.Header == treeNode_Events
                        || (selected_parentname == treeNode_Events))
                    {
                        butAdd.Visibility = Visibility.Hidden;
                        butAdd_if.Visibility = Visibility.Hidden;
                        butAdd_then.Visibility = Visibility.Visible;
                        setPasteButton(butPaste, copy_then);
                    }
                    else
                    {
                        butAdd.Visibility = Visibility.Visible;
                        butAdd_if.Visibility = Visibility.Hidden;
                        butAdd_then.Visibility = Visibility.Hidden;
                        //
                        if (selected.Header == treeNode_Variables
                            || (selected_parentname == treeNode_Variables))
                        {
                            butCopy.IsEnabled = false; butPaste.IsEnabled = false;
                        }
                        //------------------------
                        if (detectMap.variables.Count < 1)
                        {
                            but_if_v.IsEnabled = false; but_then_v.IsEnabled = false;
                        }
                        else { but_if_v.IsEnabled = true; but_then_v.IsEnabled = true; }
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }
        //------------------------------

        private void butDel_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (calSelected_Map())
                {
                    if (selected_parentname == treeNode_Variables)
                    {
                        detectMap.variables.RemoveAt(selected_loc_fromParent);
                        reloadTree_map_variable(selected_loc);
                    }
                    else
                    {
                        if (selected_parentname == treeNode_Maps)
                        {
                            detectMap.groups.RemoveAt(selected_loc_fromParent);
                        }
                        else if (selected_lv == 2)
                        {
                            //Group-Detection
                            m_Detection target = detectMap
                                .groups[selected_loc[0]].detections[selected_loc[1] - 1];
                            detectMap.groups[selected_loc[0]].detections.Remove(target);
                        }
                        else if (selected_lv == 3)
                        {
                            //Group-Condition-item
                            m_If target = detectMap
                                .groups[selected_loc[0]].inputs[selected_loc[2]];
                            detectMap.groups[selected_loc[0]].inputs.Remove(target);
                        }
                        else if (selected_lv == 4 && selected_parentname == treeNode_Conditions)
                        {
                            //Group-Detection-Condition-item
                            m_If target = detectMap
                                .groups[selected_loc[0]].detections[selected_loc[1] - 1]
                                .inputs[selected_loc[3]];
                            detectMap.groups[selected_loc[0]].detections[selected_loc[1] - 1].inputs.Remove(target);
                        }
                        else if (selected_lv == 4 && selected_parentname == treeNode_Events)
                        {
                            //Group-Detection-Condition-item
                            m_Then target = detectMap
                                .groups[selected_loc[0]].detections[selected_loc[1] - 1]
                                .outputs[selected_loc[3]];
                            detectMap.groups[selected_loc[0]].detections[selected_loc[1] - 1].outputs.Remove(target);
                        }
                        reloadTree_map_map(selected_loc);
                    }
                }
            }
            catch { }
        }

        //-----------------

        private void butAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (calSelected_Map())
                {
                    check_addAt();
                    if (selected.Header == treeNode_Variables)
                    {
                        new UKI_add_Variable(this, selected, addIndex).Show();
                    }
                    else if (selected_parentname == treeNode_Variables)
                    {
                        new UKI_add_Variable(this, selected.Parent as TreeViewItem, addIndex).Show();
                    }
                    else if (selected.Header == treeNode_Maps)
                    {
                        new UKI_add_Head(this, selected, UKI_add_Head.type_group, addIndex).Show();
                    }
                    else if (selected_parentname == treeNode_Maps)
                    {
                        new UKI_add_Detection(this, selected, addIndex).Show();
                    }
                    else if (selected_lv == 2)
                    {
                        new UKI_add_Detection(this, selected.Parent as TreeViewItem, addIndex).Show();
                    }
                }
            }
            catch { }
        }

        private void butAdd_if_Click(object sender, RoutedEventArgs e)
        {
            check_addAt(); 
            selected = treeViewer.SelectedItem as TreeViewItem;
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void butAdd_then_Click(object sender, RoutedEventArgs e)
        {
            check_addAt();
            selected = treeViewer.SelectedItem as TreeViewItem;
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        //------------------------------------------------

        private void butEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (calSelected_Map())
                {
                    if (selected_parentname == treeNode_Variables)
                    {
                        UKI_add_Variable form = new UKI_add_Variable(this, selected, addIndex);
                        m_Variable v = detectMap.variables[selected_loc_fromParent];
                        form.editVariable(v);
                        form.Show();
                    }
                    else
                    {
                        if (selected_parentname == treeNode_Maps)
                        {
                            UKI_add_Head form = new UKI_add_Head(this, selected, UKI_add_Head.type_group, addIndex);
                            m_Group g = detectMap.groups[selected_loc_fromParent];
                            form.editGroup(g);
                            form.Show();
                        }
                        else if (selected_lv == 2)
                        {
                            UKI_add_Detection form = new UKI_add_Detection(this, selected, addIndex);
                            m_Detection d = detectMap
                                .groups[selected_loc[0]].detections[selected_loc[1] - 1];
                            form.editDetection(d);
                            form.Show();
                        }
                        else if (selected_lv == 3)
                        {
                            //Group-Condition-item
                            m_If i = detectMap.groups[selected_loc[0]].inputs[selected_loc[2]];
                            editIF(i, tab_focus);
                        }
                        else if (selected_lv == 4 && selected_parentname == treeNode_Conditions)
                        {
                            //Group-Detection-Condition-item
                            m_If i = detectMap
                                .groups[selected_loc[0]].detections[selected_loc[1] - 1]
                                .inputs[selected_loc[3]];
                            editIF(i, tab_focus);
                        }
                        else if (selected_lv == 4 && selected_parentname == treeNode_Events)
                        {
                            //Group-Detection-Event-item
                            m_Then t = detectMap
                                .groups[selected_loc[0]].detections[selected_loc[1] - 1]
                                .outputs[selected_loc[3]];
                            editTHEN(t, tab_focus);
                        }
                    }
                }
            }
            catch { }
        }

        private void e_butDown_Click(object sender, RoutedEventArgs e)
        {
            e_Move(1);
        }

        private void e_butUp_Click(object sender, RoutedEventArgs e)
        {
            e_Move(-1);
        }

        void e_Move(int adj)
        {
            if (calSelected_Event())
            {
                if (selected_lv == 1)
                {
                    e_move_Event(adj);
                }
                else if (selected_lv == 2)
                {
                    e_move_Then(adj);
                }
            }
        }

        void e_move_Event(int adj)
        {
            int target = selected_loc_fromParent;
            int new_target = selected_loc_fromParent + adj;
            if (new_target >= 0 && new_target < list_events.Count)
            {
                m_Event e_temp = list_events[target];
                list_events.Remove(e_temp);
                list_events.Insert(new_target, e_temp);
                //Reload & Select
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target);
                reloadTree_event(getTVI_Location(newTVI));
            }
        }

        void e_move_Then(int adj)
        {
            int target = selected_loc_fromParent;
            int new_target = selected_loc_fromParent + adj;
            int target_parent = getTVI_Location_fromParent(selected_parent);
            m_Event ev = list_events[target_parent];
            m_Then t = ev.outputs[target];
            //
            if (new_target >= 0 && new_target < ev.outputs.Count)
            {
                ev.outputs.Remove(t);
                ev.outputs.Insert(new_target, t);
                //
                TreeViewItem newTVI = getTVI_byIndex(selected_parent, new_target);
                reloadTree_event(getTVI_Location(newTVI));
            }
            else if (adj > 0)//Pass to next Event
            {
                int new_ev_i = list_events.IndexOf(ev) + 1;
                if (new_ev_i < list_events.Count)
                {
                    ev.outputs.Remove(t);
                    list_events[new_ev_i].outputs.Insert(0, t);
                    //
                    TreeViewItem uncle = TVI_getNextSibling(selected_parent, 1);
                    buildTree_event();
                    TVI_selectChild(getTVI_Location(uncle), tv_event_root, 0);
                };
            }
            else if (adj < 0)//previous Event exist
            {
                int new_ev_i = list_events.IndexOf(ev) - 1;
                if (new_ev_i >= 0)
                {
                    ev.outputs.Remove(t);
                    list_events[new_ev_i].outputs.Add(t);
                    //
                    TreeViewItem uncle = TVI_getNextSibling(selected_parent, -1);
                    buildTree_event();
                    TVI_selectLastChild(getTVI_Location(uncle), tv_event_root);
                }
            }
        }
        //========================================================================
        //====== Copy & Paste ====================================================

        //--------------------------------
        public m_Event copy_event = null;
        public m_Then copy_then = null;
        public m_Motion copy_motion = null;
        public m_If copy_if = null;
        public m_Variable copy_v = null;
        public m_Group copy_group = null;
        public m_Detection copy_detection = null;

        //-- Regenerate Tree : ensure independently ----
        void completeReloadTree_event()
        {
            saveXML_Event(file_path_temp);
            list_events = TheMapData.loadXML_event(file_path_temp);
            reloadTree_event(selected_loc);
            TheTool.delFile(file_path_temp);
        }

        void completeReloadTree_motion()
        {
            TheMapData.saveXML_Motion(file_path_temp, list_motions);
            list_motions = TheMapData.loadXML_motion(file_path_temp);
            reloadTree_motion(selected_loc);
            TheTool.delFile(file_path_temp);
        }

        void completeReloadTree_map()
        {
            try
            {
                saveXML_Map(file_path_temp);
                detectMap = TheMapData.loadXML_map(file_path_temp);
                if (selected.Header == treeNode_Variables || selected_parentname == treeNode_Variables)
                {
                    reloadTree_map_variable(selected_loc);
                }
                else { reloadTree_map_map(selected_loc); }
                TheTool.delFile(file_path_temp);
            }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        //-----------------------------------------

        private void e_butCopy_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Event())
            {
                if (selected_lv == 1)
                {
                    copy_event = list_events[selected_loc_fromParent];
                }
                else if (selected_lv == 2)
                {
                    int target_parent = getTVI_Location_fromParent(selected_parent);
                    m_Event ev = list_events[target_parent];
                    copy_then = ev.outputs[selected_loc_fromParent]; 
                }
                e_butPaste.IsEnabled = true;
            }
        }

        private void e_butPaste_Click(object sender, RoutedEventArgs e)
        {
            check_addAt();
            if (calSelected_Event())
            {
                if (selected_lv == 1)
                {
                    addEvent(copy_event, addIndex);
                }
                else if (selected_lv == 2)
                {
                    addThen(copy_then, selected_parent, tab_focus, addIndex);
                }
                completeReloadTree_event();//complete clone
            }
        }

        private void m_butCopy_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Motion())
            {
                if (selected_lv == 1)
                {
                    copy_motion = list_motions[selected_loc_fromParent];
                }
                else if (selected_lv == 2)
                {
                    int target_parent = getTVI_Location_fromParent(selected_parent);
                    copy_if = list_motions[target_parent].inputs[selected_loc_fromParent];
                }
                m_butPaste.IsEnabled = true;
            }
        }

        private void m_butPaste_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Motion())
            {
                check_addAt();
                if (selected_lv == 1)
                {
                    addMotion(copy_motion, addIndex);
                }
                else if (selected_lv == 2)
                {
                    addIf(copy_if, selected_parent, tab_focus, addIndex);
                }
                completeReloadTree_motion();//complete clone
            }
        }

        private void butCopy_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Map())
            {
                if (selected_parentname == treeNode_Variables)
                {
                    copy_v = detectMap.variables[selected_loc_fromParent];
                }
                else
                {
                    if (selected_parentname == treeNode_Maps)
                    {
                        copy_group = detectMap.groups[selected_loc_fromParent];
                    }
                    else if (selected_lv == 2)
                    {
                        copy_detection = detectMap.groups[selected_loc[0]].detections[selected_loc[1] - 1];
                    }
                    else if (selected_lv == 3)
                    {
                        copy_if = detectMap.groups[selected_loc[0]].inputs[selected_loc[2]];
                   } 
                    else if (selected_lv == 4 && selected_parentname == treeNode_Conditions)
                    {
                        copy_if = detectMap.groups[selected_loc[0]].detections[selected_loc[1] - 1].inputs[selected_loc[3]];
                    }
                    else if (selected_lv == 4 && selected_parentname == treeNode_Events)
                    {
                        copy_then = detectMap.groups[selected_loc[0]].detections[selected_loc[1] - 1].outputs[selected_loc[3]];
                   } 
                }
                butPaste.IsEnabled = true;
            }
        }

        private void butPaste_Click(object sender, RoutedEventArgs e)
        {
            if (calSelected_Map())
            {
                check_addAt();
                if (selected_parentname == treeNode_Variables)
                {
                    addVariable(copy_v, addIndex);
                }
                else
                {
                    if (selected_parentname == treeNode_Maps)
                    {
                        addGroup(copy_group, addIndex);
                    }
                    else if (selected_lv == 2 && selected.Header == treeNode_Conditions)
                    {
                        addIf(copy_if, selected, tab_focus, addIndex);;
                    }
                    else if (selected_lv == 2)
                    {
                        addDetection(copy_detection, addIndex);
                    }
                    else if (selected_lv == 3 && selected_parentname == treeNode_Conditions)
                    {
                        addIf(copy_if, selected_parent, tab_focus, addIndex);
                    }
                    else if (selected_lv == 3 && selected.Header == treeNode_Conditions)
                    {
                        addIf(copy_if, selected, tab_focus, addIndex);
                    }
                    else if (selected_lv == 3 && selected.Header == treeNode_Events)
                    {
                        addThen(copy_then, selected, tab_focus, addIndex);
                    }
                    else if (selected_lv == 4 && selected_parentname == treeNode_Conditions)
                    {
                        addIf(copy_if, selected_parent, tab_focus, addIndex);
                    }
                    else if (selected_lv == 4 && selected_parentname == treeNode_Events)
                    {
                        addThen(copy_then, selected_parent, tab_focus, addIndex);
                    }
                }
                completeReloadTree_map();//complete clone
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Name as string;
            if (tabItem == tabEvent.Name) { e_selection(); }
            else if (tabItem == tabMotion.Name) { m_selection(); }
            else { map_selection(); }
        }

        private void butViewMotionFile_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".xml",
               TheURL.url_saveFolder);
            if (openDialog == true)
            {
                try
                {
                    list_motions = TheMapData.loadXML_motion(TheTool.dialog.FileName);
                    buildTree_motion();
                }
                catch (Exception ex) { TheSys.showError(ex); }
            }
        }

        //===================================================================

        private void butShowText_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem_printText_each(treeViewer);
        }


        private void m_butShowText_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem_printText_each(m_treeViewer);
        }

        private void e_butShowText_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem_printText_each(e_treeViewer);
        }

        void TreeViewItem_printText_each(TreeView tv)
        {
            foreach (TreeViewItem child in tv.Items)
            {
                printText_lv = 0;
                TreeViewItem_printText_each(child);
            }
        }

        int printText_lv = 0;
        void TreeViewItem_printText_each(TreeViewItem tvi)
        {
            string prefix = "";
            for (int i = 0; i < printText_lv; i++) { prefix += "-";}
            if (printText_lv > 0) { prefix += " "; }
            TheSys.showError(prefix + tvi.Header.ToString());
            if (tvi.HasItems)
            {
                printText_lv += 1;
                foreach (TreeViewItem child in tvi.Items)
                {
                    TreeViewItem_printText_each(child);
                }
                printText_lv -= 1;
            }
        }

        //========================================================================
        
    }
}
