using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using P_Tracker2;
using Microsoft.Kinect;
using System.Windows.Media;
using System.IO;

namespace P_Tracker2
{
    class TheMapData
    {

        public const int v_type_Boolean = 0;
        public const int if_type_BasicPose = 0;
        public const int if_type_MotionDatabase = 1;
        public const int if_type_2Joint = 2;
        public const int if_type_Change = 3;//Feature from Change-from_BasePoseture
        public const int if_type_SphereAngle = 4;
        public const int if_type_FlexionAngle = 5;
        public const int if_type_Variable = 9;
        public const int if_type_Icon = 10;
        public const int if_type_TimeAfterPose = -1;
        public const int if_type_Comment = 99;//just text
        public const int then_type_Key = 0;
        public const int then_type_EventDatabase = 1;
        public const int then_type_Mouse = 2;
        public const int then_type_ReplaceKey = 8;
        public const int then_type_Variable = 9;
        public const int then_type_Icon = 10;
        public const int then_type_TimeWait = -1;
        public const int then_type_Comment = 99;//just text
        //---------
        public const int then_SphereAngle_Polar = 1;
        public const int then_SphereAngle_Azimuth = 2;
        public const int then_key_press = -1;
        public const int then_key_up = 0;
        public const int then_key_holdEoM = 1;
        public const int then_key_hold = 2;
        public const int then_mouse_moveBy = 1;
        public const int then_mouse_moveTo = 2;

        public static MapData loadXML_map(String path)
        {
            MapData detectMap = new MapData();
            try
            {
                XmlDocument xdcDocument = new XmlDocument();
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //prevent error: process by another
                xdcDocument.Load(fs);//instead of "xdcDocument.Load(path);"
                XmlElement xelRoot = xdcDocument.DocumentElement;
                //-------------------------------
                XmlNodeList xnl_vroot = xelRoot.SelectNodes("/mapping/variable");
                XmlNode vroot = xnl_vroot[0];
                if (vroot.Attributes["expand"] != null)
                {
                    detectMap.expand_v = TheTool.convert01_Boolean(vroot.Attributes["expand"].Value.ToString());
                }
                foreach (XmlNode v_each in vroot.SelectNodes("child::v"))
                {
                    m_Variable v = new m_Variable();
                    v.name = v_each.Attributes["name"].Value.ToString();
                    v.value = TheTool.getInt(v_each.Attributes["value"].Value.ToString());
                    detectMap.variables.Add(v);
                }
                //-------------------------------
                XmlNodeList xnl_map = xelRoot.SelectNodes("/mapping/map");
                XmlNode mroot = xnl_map[0];
                if (mroot.Attributes["expand"] != null)
                {
                    detectMap.expand_map = TheTool.convert01_Boolean(mroot.Attributes["expand"].Value.ToString());
                }
                foreach (XmlNode g_each in mroot.SelectNodes("child::group"))
                {
                    m_Group g = new m_Group();
                    g.name = g_each.Attributes["name"].Value.ToString();
                    if (g_each.Attributes["enabled"] != null)
                    {
                        g.enabled = TheTool.convert01_Boolean(g_each.Attributes["enabled"].Value.ToString());
                    }
                    if (g_each.Attributes["expand"] != null)
                    {
                        g.expand = TheTool.convert01_Boolean(g_each.Attributes["expand"].Value.ToString());
                    }
                    if (g_each.Attributes["expand_if"] != null)
                    {
                        g.expand_if = TheTool.convert01_Boolean(g_each.Attributes["expand_if"].Value.ToString());
                    }
                    foreach (XmlNode i_each in g_each.SelectNodes("child::if"))
                    {
                        g.inputs.Add(get_If(i_each));
                    }
                    foreach (XmlNode d_each in g_each.SelectNodes("child::detection"))
                    {
                        m_Detection d = new m_Detection();
                        d.name = d_each.Attributes["name"].Value.ToString();
                        if (d_each.Attributes["loop"] != null)
                        {
                            d.loop = TheTool.convert01_Boolean(d_each.Attributes["loop"].Value.ToString());
                        }
                        if (d_each.Attributes["priority"] != null)
                        {
                            d.priority = TheTool.convert01_Boolean(d_each.Attributes["priority"].Value.ToString());
                        }
                        if (d_each.Attributes["expand"] != null)
                        {
                            d.expand = TheTool.convert01_Boolean(d_each.Attributes["expand"].Value.ToString());
                        }
                        if (d_each.Attributes["expand_if"] != null)
                        {
                            d.expand_if = TheTool.convert01_Boolean(d_each.Attributes["expand_if"].Value.ToString());
                        }
                        if (d_each.Attributes["expand_then"] != null)
                        {
                            d.expand_then = TheTool.convert01_Boolean(d_each.Attributes["expand_then"].Value.ToString());
                        }
                        foreach (XmlNode i_each in d_each.SelectNodes("child::if"))
                        {
                            d.inputs.Add(get_If(i_each));
                        }
                        foreach (XmlNode o_each in d_each.SelectNodes("child::then"))
                        {
                            d.outputs.Add(get_Then(o_each));
                        }
                        g.detections.Add(d);
                    }
                    detectMap.groups.Add(g);
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return detectMap;
        }

        public static void debugMap(MapData detectMap)
        {
            foreach (m_Group g in detectMap.groups)
            {
                foreach (m_Detection d in g.detections)
                {
                    TheSys.showError("[D]" + d.name);
                    foreach (m_If i in d.inputs) { TheSys.showError("[I]"+i.v + "_" + i.value + "_" + i.value_d); }
                    foreach (m_Then o in d.outputs) { TheSys.showError("[O]" + o.v + "_" + o.key); }
                }
            }
        }

        public static Boolean checkDataExist(XmlNode each, String att)
        {
            if (each.Attributes[att] != null && each.Attributes[att].Value.ToString() != "")
            {
                return true;
            }
            else { return false; }
        }


        public static m_If get_If(XmlNode i_each)
        {
            m_If i = new m_If();
            if (checkDataExist(i_each, "type"))
            {
                i.type = TheTool.getInt(i_each.Attributes["type"].Value.ToString());
            }
            if (checkDataExist(i_each, "v"))
            {
                i.v = i_each.Attributes["v"].Value.ToString();
                if (i.type == if_type_2Joint) { i.j1 = getJoint(i.v); }
            }
            if (checkDataExist(i_each, "v2"))
            {
                i.v2 = i_each.Attributes["v2"].Value.ToString();
                if (i.type == if_type_2Joint) { i.j2 = getJoint(i.v2); }
            }
            if (checkDataExist(i_each, "axis"))
            {
                i.axis = i_each.Attributes["axis"].Value.ToString();
            }
            if (checkDataExist(i_each, "opt"))
            {
                i.opt = convertOpt_getMath_byLetter(i_each.Attributes["opt"].Value.ToString());
            }
            if (checkDataExist(i_each, "value"))
            {
                i.value = TheTool.getInt(i_each.Attributes["value"].Value.ToString());
            }
            if (checkDataExist(i_each, "value_d"))
            {
                i.value_d = TheTool.getDouble(i_each.Attributes["value_d"].Value.ToString());
                i.value_d_1000 = (int)(i.value_d * 1000);
            }
            if (checkDataExist(i_each, "brush"))
            {
                i.brush0 = i_each.Attributes["brush"].Value.ToString();
                i.brush = getBrush(i.brush0);
            }
            return i;
        }

        public static m_Then get_Then(XmlNode o_each)
        {
            m_Then o = new m_Then();
            if (checkDataExist(o_each, "type"))
            {
                o.type = TheTool.getInt(o_each.Attributes["type"].Value.ToString());
            }
            if (checkDataExist(o_each, "press"))
            {
                o.press = TheTool.getInt(o_each.Attributes["press"].Value.ToString());
            }
            if (checkDataExist(o_each, "key"))
            {
                o.key0 = o_each.Attributes["key"].Value.ToString();
                o.key = TheKeySender.getKey(o.key0);
            }
            if (checkDataExist(o_each, "v"))
            {
                o.v = o_each.Attributes["v"].Value.ToString();
            }
            if (checkDataExist(o_each, "v2"))
            {
                o.v2 = o_each.Attributes["v2"].Value.ToString();
            }
            if (checkDataExist(o_each, "value"))
            {
                o.value = TheTool.getInt(o_each.Attributes["value"].Value.ToString());
            }
            if (checkDataExist(o_each, "value_d"))
            {
                o.value_d = TheTool.getDouble(o_each.Attributes["value_d"].Value.ToString());
                o.value_d_1000 = (int)(o.value_d * 1000);
            }
            if (checkDataExist(o_each, "brush"))
            {
                o.brush0 = o_each.Attributes["brush"].Value.ToString();
                o.brush = getBrush(o.brush0);
            }
            if (checkDataExist(o_each, "x"))
            {
                o.x = TheTool.getInt(o_each.Attributes["x"].Value.ToString());
            }
            if (checkDataExist(o_each, "y"))
            {
                o.y = TheTool.getInt(o_each.Attributes["y"].Value.ToString());
            }
            if (checkDataExist(o_each, "opt"))
            {
                o.opt = o_each.Attributes["opt"].Value.ToString();
            }
            return o;
        }
        
        public static List<m_Motion> loadXML_motion(String path)
        {
            List<m_Motion> motions = new List<m_Motion>();
            try
            {
                XmlDocument xdcDocument = new XmlDocument();
                xdcDocument.Load(path);
                XmlElement xelRoot = xdcDocument.DocumentElement;
                XmlNodeList xnl_group;
                xnl_group = xelRoot.SelectNodes("/motions/motion");
                foreach (XmlNode m_each in xnl_group)
                {
                    m_Motion m = new m_Motion();
                    m.name = m_each.Attributes["name"].Value.ToString();
                    if (m_each.Attributes["enabled"] != null)
                    {
                        m.enabled = TheTool.convert01_Boolean(m_each.Attributes["enabled"].Value.ToString());
                    }
                    if (m_each.Attributes["expand"] != null)
                    {
                        m.expand = TheTool.convert01_Boolean(m_each.Attributes["expand"].Value.ToString());
                    }
                    foreach (XmlNode i_each in m_each.SelectNodes("child::if"))
                    {
                        m.inputs.Add(get_If(i_each));
                    }
                    motions.Add(m);
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return motions;
        }

        public static List<m_Event> loadXML_event(String path)
        {
            List<m_Event> events = new List<m_Event>();
            try
            {
                XmlDocument xdcDocument = new XmlDocument();
                xdcDocument.Load(path);
                XmlElement xelRoot = xdcDocument.DocumentElement;
                XmlNodeList xnl_group;
                xnl_group = xelRoot.SelectNodes("/events/event");
                foreach (XmlNode e_each in xnl_group)
                {
                    m_Event e = new m_Event();
                    e.name = e_each.Attributes["name"].Value.ToString();
                    if (e_each.Attributes["enabled"] != null)
                    {
                        e.enabled = TheTool.convert01_Boolean(e_each.Attributes["enabled"].Value.ToString());
                    }
                    if (e_each.Attributes["expand"] != null)
                    {
                        e.expand = TheTool.convert01_Boolean(e_each.Attributes["expand"].Value.ToString());
                    }
                    foreach (XmlNode o_each in e_each.SelectNodes("child::then"))
                    {
                        e.outputs.Add(get_Then(o_each));
                    }
                    events.Add(e);
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return events;
        }


        //load all database and store in Map
        public static MapData getFullXML(MapData detectMap, List<m_Motion> list_motions, List<m_Event> list_events)
        {
            try
            {
                foreach (m_Group g in detectMap.groups)
                {
                    List<m_If> inputs_new0 = new List<m_If>();
                    getFullInput(ref inputs_new0, g.inputs, list_motions);
                    g.inputs = inputs_new0;
                    foreach (m_Detection d in g.detections)
                    {
                        List<m_If> inputs_new = new List<m_If>();
                        getFullInput(ref inputs_new, d.inputs, list_motions);
                        d.inputs = inputs_new;
                        d.posture_count = 1;
                        foreach (m_If d_i in d.inputs)
                        {
                            if (d_i.type == TheMapData.if_type_TimeAfterPose) { d.posture_count++; }
                        }
                        //TheSys.showError(d.name + " " + d.posture_count);
                        List<m_Then> output_new = new List<m_Then>();
                        getFullOutput(ref output_new, d.outputs, list_events);
                        d.outputs = output_new;
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return detectMap;
        }

        //new list to add item , inputs , motion database
        static public void getFullInput(ref List<m_If> new_input, List<m_If> old_inputs, List<m_Motion> list_motions)
        {
            try
            {
                foreach (m_If i in old_inputs)
                {
                    if (i.type == TheMapData.if_type_MotionDatabase)
                    {
                        Boolean notFound = true;
                        foreach (m_Motion m in list_motions)
                        {
                            if (i.v == m.name && m.enabled) 
                            {
                                getFullInput(ref new_input, m.inputs, list_motions);//ReLoop
                                notFound = false; break;
                            }
                        }
                        if (notFound) { TheSys.showError(i.v + " is not found"); } 
                    }
                    else { new_input.Add(i); }
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
        }

        static public void getFullOutput(ref List<m_Then> new_output, List<m_Then> old_outputs, List<m_Event> list_events)
        {
            try
            {
                foreach (m_Then o in old_outputs)
                {
                    if (o.type == TheMapData.then_type_EventDatabase)
                    {
                        Boolean notFound = true;
                        foreach (m_Event e in list_events)
                        {
                            if (o.v == e.name && e.enabled) { 
                                getFullOutput(ref new_output, e.outputs, list_events);
                                notFound = false; break; 
                            }
                        }
                        if (notFound) { TheSys.showError(o.v + " is not found"); }
                    }
                    else { new_output.Add(o); }
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
        }

        //-------------------------------------

        //replace [P]
        public static MapData getDecodeBP(MapData detectMap)
        {
            try
            {
                foreach (m_Group g in detectMap.groups)
                {
                    List<m_If> inputs_new0 = new List<m_If>();
                    decodeInput(ref inputs_new0, g.inputs);
                    g.inputs = inputs_new0;
                    foreach (m_Detection d in g.detections)
                    {
                        List<m_If> inputs_new = new List<m_If>();
                        decodeInput(ref inputs_new, d.inputs);
                        d.inputs = inputs_new;
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return detectMap;
        }

        //new list to add item , inputs , motion database
        static public void decodeInput(ref List<m_If> new_input, List<m_If> old_inputs)
        {
            try
            {
                foreach (m_If i in old_inputs)
                {
                    if (i.type == TheMapData.if_type_BasicPose)
                    {
                        new_input.AddRange(UKI.convertBasicPosture_toRelativePosture(i));
                    }
                    else { new_input.Add(i); }
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------

        static public string convert_getBasePoseDef(string v, int value){
            string s = "";
            foreach(v_BasePosture b in list_basepose){
                if (b.v == v && b.value == value) { s = b.name; }
            }
            return s;
        }

        //check consistant with UKI.cs
        static public List<v_BasePosture> list_basepose = new List<v_BasePosture>{
            new v_BasePosture { name = "Jump" 
                , part1 = "Body" , part2 = "Jump" 
                , v = "jump" , value = 1 },
            new v_BasePosture { name = "Crouch" 
                , part1 = "Body" , part2 = "Crouch" 
                , v = "jump" , value = -1 },
            new v_BasePosture { name = "NOT Jump or Crouch" 
                , part1 = "Body" , part2 = "Not Jump/Crouch" 
                , v = "jump" , value = 0 },
            new v_BasePosture { name = "Lean Forward" 
                , part1 = "Lean" , part2 = "Forward" 
                , v = "lean" , value = 1 },
            new v_BasePosture { name = "Lean Backward" 
                , part1 = "Lean" , part2 = "Backward" 
                , v = "lean" , value = -1 },
            new v_BasePosture { name = "Not Lean" 
                , part1 = "Lean" , part2 = "Not Lean" 
                , v = "lean" , value = 0 },
            new v_BasePosture { name = "Left Spin" 
                , part1 = "Spin" , part2 = "Left" 
                , v = "spin" , value = -1 },
            new v_BasePosture { name = "Right Spin" 
                , part1 = "Spin" , part2 = "Right" 
                , v = "spin" , value = 1 },
            new v_BasePosture { name = "NOT Spin" 
                , part1 = "Spin" , part2 = "Not Spin" 
                , v = "spin" , value = 0 },
            new v_BasePosture { name = "Left Hand: to the Left" 
                , part1 = "Left Hand [X]" , part2 = "Extend to the Left" 
                , v = "handL_X" , value = 1 },
            new v_BasePosture { name = "Left Hand: to the Right" 
                , part1 = "Left Hand [X]" , part2 = "Extend to the Right" 
                , v = "handL_X" , value = -1 },
            new v_BasePosture { name = "Left Hand: NOT to the Left or the Right" 
                , part1 = "Left Hand [X]" , part2 = "Not Extend" 
                , v = "handL_X" , value = 0 },
            new v_BasePosture { name = "Left Hand: Raise Up"
                , part1 = "Left Hand [Y]" , part2 = "Raise Up" 
                , v = "handL_Y"  , value = 1 },
            new v_BasePosture { name = "Left Hand: Down" 
                , part1 = "Left Hand [Y]" , part2 = "Down"  
                , v = "handL_Y" , value = -1 },
            new v_BasePosture { name = "Left Hand: Not Raise Up or Down" 
                , part1 = "Left Hand [Y]" , part2 = "Not Raise Up or Down"  
                , v = "handL_Y" , value = 0 },
            new v_BasePosture { name = "Left Hand: in Front of body" 
                , part1 = "Left Hand [Z]" , part2 = "In Front of the Body"  
                , v = "handL_Z" , value = 1 },
            new v_BasePosture { name = "Left Hand: Behind the body" 
                , part1 = "Left Hand [Z]" , part2 = "Behind the Body"  
                , v = "handL_Z" , value = -1 },
            new v_BasePosture { name = "Left Hand: NOT in Front of or Behind the body" 
                , part1 = "Left Hand [Z]" , part2 = "Not In Front of or Behind the Body"  
                , v = "handL_Z" , value = 0 },
            new v_BasePosture { name = "Right Hand: to the Right" 
                , part1 = "Right Hand [X]" , part2 = "Extend to the Right" 
                , v = "handR_X" , value = 1 },
            new v_BasePosture { name = "Right Hand: to the Left" 
                , part1 = "Right Hand [X]" , part2 = "Extend to the Left" 
                , v = "handR_X" , value = -1 },
            new v_BasePosture { name = "Right Hand: NOT to the Right or the Left" 
                , part1 = "Right Hand [X]" , part2 = "Not Extend" 
                , v = "handR_X" , value = 0 },
            new v_BasePosture { name = "Right Hand: Raise Up"
                , part1 = "Right Hand [Y]" , part2 = "Raise Up" 
                , v = "handR_Y"  , value = 1 },
            new v_BasePosture { name = "Right Hand: Down" 
                , part1 = "Right Hand [Y]" , part2 = "Down"  
                , v = "handR_Y" , value = -1 },
            new v_BasePosture { name = "Right Hand: Not Raise Up or Down" 
                , part1 = "Right Hand [Y]" , part2 = "Not Raise Up or Down"  
                , v = "handR_Y" , value = 0 },
            new v_BasePosture { name = "Right Hand: in Front of body" 
                , part1 = "Right Hand [Z]" , part2 = "In Front of the Body"  
                , v = "handR_Z" , value = 1 },
            new v_BasePosture { name = "Right Hand: Behind the body" 
                , part1 = "Right Hand [Z]" , part2 = "Behind the Body"  
                , v = "handR_Z" , value = -1 },
            new v_BasePosture { name = "Right Hand: NOT in Front of or Behind the body" 
                , part1 = "Right Hand [Z]" , part2 = "Not In Front of or Behind the Body"  
                , v = "handR_Z" , value = 0 },
            new v_BasePosture { name = "Two Hands are closed to each other" 
                , part1 = "2 Hands" , part2 = "Closed"  
                , v = "handLR_close" , value = 1 },
            new v_BasePosture { name = "Two Hands are away from each other" 
                , part1 = "2 Hands" , part2 = "Away from each other"  
                , v = "handLR_close" , value = 0 },
            new v_BasePosture { name = "Left Leg: Stand" 
                , part1 = "Left Leg" , part2 = "Stand"  
                , v = "legL" , value = 0 },
            new v_BasePosture { name = "Left Leg: Knee Strike" 
                , part1 = "Left Leg" , part2 = "Knee Strike"  
                , v = "legL" , value = 1 },
            new v_BasePosture { name = "Left Leg: Kick" 
                , part1 = "Left Leg" , part2 = "Kick"  
                , v = "legL" , value = 2 },
            new v_BasePosture { name = "Right Leg: Stand" 
                , part1 = "Right Leg" , part2 = "Stand"  
                , v = "legR" , value = 0 },
            new v_BasePosture { name = "Right Leg: Knee Strike" 
                , part1 = "Right Leg" , part2 = "Knee Strike"  
                , v = "legR" , value = 1 },
            new v_BasePosture { name = "Right Leg: Kick" 
                , part1 = "Right Leg" , part2 = "Kick"  
                , v = "legR" , value = 2 },
            //---------------
            new v_BasePosture { name = "Two Feet are closed from each other" 
                , part1 = "2 Feet" , part2 = "Closed"  
                , v = "footLR_close" , value = 1 },
            new v_BasePosture { name = "Two Feet are away from each other" 
                , part1 = "2 Feet" , part2 = "Away from each other"  
                , v = "footLR_close" , value = 0 },
            new v_BasePosture { name = "Step Up" 
                , part1 = "Step" , part2 = "Up"  
                , v = "step" , value = 1 },
            new v_BasePosture { name = "Step Down" 
                , part1 = "Step" , part2 = "Down"  
                , v = "step" , value = -1 }
        };

        public class v_BasePosture
        {
            public string name { get; set; }
            public string v { get; set; }
            public int value { get; set; }
            public string part1 { get; set; }
            public string part2 { get; set; }
        }

        //-----------------------------------------------------------------------------------------

        public static List<map_Joint> joint_list = new List<map_Joint>
        {
            new map_Joint{ name = "AnkleL" , def = "Left Ankle" , joint = JointType.AnkleLeft },
            new map_Joint{ name = "AnkleR" , def = "Right Ankle" , joint = JointType.AnkleRight },
            new map_Joint{ name = "ElbowL" , def = "Left Elbow" , joint = JointType.ElbowLeft },
            new map_Joint{ name = "ElbowR" , def = "Right Elbow" , joint = JointType.ElbowRight },
            new map_Joint{ name = "FootL" , def = "Left Foot" , joint = JointType.FootLeft },
            new map_Joint{ name = "FootR" , def = "Right Foot" , joint = JointType.FootRight },
            new map_Joint{ name = "HandL" , def = "Left Hand" , joint = JointType.HandLeft },
            new map_Joint{ name = "HandR" , def = "Right Hand" , joint = JointType.HandRight },
            new map_Joint{ name = "Head" , def = "Head" , joint = JointType.Head },
            new map_Joint{ name = "HipC" , def = "Center Hip" , joint = JointType.HipCenter },
            new map_Joint{ name = "HipL" , def = "Left Hip" , joint = JointType.HipLeft },
            new map_Joint{ name = "HipR" , def = "Right Hip" , joint = JointType.HipRight },
            new map_Joint{ name = "KneeL" , def = "Left Knee" , joint = JointType.KneeLeft },
            new map_Joint{ name = "KneeR" , def = "Right Knee" , joint = JointType.KneeRight },
            new map_Joint{ name = "ShoulderC" , def = "Center Shoulder" , joint = JointType.ShoulderCenter },
            new map_Joint{ name = "ShoulderL" , def = "Left Shoulder" , joint = JointType.ShoulderLeft },
            new map_Joint{ name = "ShoulderR" , def = "Right Shoulder" , joint = JointType.ShoulderRight },
            new map_Joint{ name = "Spine" , def = "Spine" , joint = JointType.Spine },
            new map_Joint{ name = "WristL" , def = "Left Wrist" , joint = JointType.WristLeft },
            new map_Joint{ name = "WristR" , def = "Right Wrist" , joint = JointType.WristRight }
        };

        public static List<map_Joint> joint_list_flexion = new List<map_Joint>
        {
            new map_Joint{ name = "ElbowL" , def = "Left Elbow" , joint = JointType.ElbowLeft },
            new map_Joint{ name = "ElbowR" , def = "Right Elbow" , joint = JointType.ElbowRight },
            new map_Joint{ name = "WristL" , def = "Left Wrist" , joint = JointType.WristLeft },
            new map_Joint{ name = "WristR" , def = "Right Wrist" , joint = JointType.WristRight }
        };

        public static string getJointName(JointType j)
        {
            String s = "";
            foreach (map_Joint mj in joint_list)
            {
                if (j == mj.joint) { s = mj.name; }
            }
            return s;
        }

        public static JointType getJoint(String txt)
        {
            JointType joint = JointType.Head;
            foreach (map_Joint j in joint_list)
            {
                if (j.name == txt) { joint = j.joint; break; }
            }
            return joint;
        }

        public static String getJointDef(string txt)
        {
            String def = "Head";
            foreach (map_Joint j in joint_list)
            {
                if (j.name == txt) { def = j.def; break; }
            }
            return def;
        }

        public static String getJointName_byDef(string txt)
        {
            String name = "Head";
            foreach (map_Joint j in joint_list)
            {
                if (j.def == txt) { name = j.name; break; }
            }
            return name;
        }

        public static List<map_Brush> brush_list = new List<map_Brush>
        {
            new map_Brush{ name = "AliceBlue" , brush = Brushes.AliceBlue },
            new map_Brush{ name = "AntiqueWhite" , brush = Brushes.AntiqueWhite },
            new map_Brush{ name = "Aqua" , brush = Brushes.Aqua },
            new map_Brush{ name = "Aquamarine" , brush = Brushes.Aquamarine },
            new map_Brush{ name = "Azure" , brush = Brushes.Azure },
            new map_Brush{ name = "Beige" , brush = Brushes.Beige },
            new map_Brush{ name = "Bisque" , brush = Brushes.Bisque },
            new map_Brush{ name = "Black" , brush = Brushes.Black },
            new map_Brush{ name = "BlanchedAlmond" , brush = Brushes.BlanchedAlmond },
            new map_Brush{ name = "Blue" , brush = Brushes.Blue },
            new map_Brush{ name = "BlueViolet" , brush = Brushes.BlueViolet },
            new map_Brush{ name = "Brown" , brush = Brushes.Brown },
            new map_Brush{ name = "BurlyWood" , brush = Brushes.BurlyWood },
            new map_Brush{ name = "CadetBlue" , brush = Brushes.CadetBlue },
            new map_Brush{ name = "Chartreuse" , brush = Brushes.Chartreuse },
            new map_Brush{ name = "Chocolate" , brush = Brushes.Chocolate },
            new map_Brush{ name = "Coral" , brush = Brushes.Coral },
            new map_Brush{ name = "CornflowerBlue" , brush = Brushes.CornflowerBlue },
            new map_Brush{ name = "Cornsilk" , brush = Brushes.Cornsilk },
            new map_Brush{ name = "Crimson" , brush = Brushes.Crimson },
            new map_Brush{ name = "Cyan" , brush = Brushes.Cyan },
            new map_Brush{ name = "DarkBlue" , brush = Brushes.DarkBlue },
            new map_Brush{ name = "DarkCyan" , brush = Brushes.DarkCyan },
            new map_Brush{ name = "DarkGoldenrod" , brush = Brushes.DarkGoldenrod },
            new map_Brush{ name = "DarkGray" , brush = Brushes.DarkGray },
            new map_Brush{ name = "DarkGreen" , brush = Brushes.DarkGreen },
            new map_Brush{ name = "DarkKhaki" , brush = Brushes.DarkKhaki },
            new map_Brush{ name = "DarkMagenta" , brush = Brushes.DarkMagenta },
            new map_Brush{ name = "DarkOliveGreen" , brush = Brushes.DarkOliveGreen },
            new map_Brush{ name = "DarkOrange" , brush = Brushes.DarkOrange },
            new map_Brush{ name = "DarkOrchid" , brush = Brushes.DarkOrchid },
            new map_Brush{ name = "DarkRed" , brush = Brushes.DarkRed },
            new map_Brush{ name = "DarkSalmon" , brush = Brushes.DarkSalmon },
            new map_Brush{ name = "DarkSeaGreen" , brush = Brushes.DarkSeaGreen },
            new map_Brush{ name = "DarkSlateBlue" , brush = Brushes.DarkSlateBlue },
            new map_Brush{ name = "DarkSlateGray" , brush = Brushes.DarkSlateGray },
            new map_Brush{ name = "DarkTurquoise" , brush = Brushes.DarkTurquoise },
            new map_Brush{ name = "DarkViolet" , brush = Brushes.DarkViolet },
            new map_Brush{ name = "DeepPink" , brush = Brushes.DeepPink },
            new map_Brush{ name = "DeepSkyBlue" , brush = Brushes.DeepSkyBlue },
            new map_Brush{ name = "DimGray" , brush = Brushes.DimGray },
            new map_Brush{ name = "DodgerBlue" , brush = Brushes.DodgerBlue },
            new map_Brush{ name = "Firebrick" , brush = Brushes.Firebrick },
            new map_Brush{ name = "FloralWhite" , brush = Brushes.FloralWhite },
            new map_Brush{ name = "ForestGreen" , brush = Brushes.ForestGreen },
            new map_Brush{ name = "Fuchsia" , brush = Brushes.Fuchsia },
            new map_Brush{ name = "Gainsboro" , brush = Brushes.Gainsboro },
            new map_Brush{ name = "GhostWhite" , brush = Brushes.GhostWhite },
            new map_Brush{ name = "Gold" , brush = Brushes.Gold },
            new map_Brush{ name = "Goldenrod" , brush = Brushes.Goldenrod },
            new map_Brush{ name = "Gray" , brush = Brushes.Gray },
            new map_Brush{ name = "Green" , brush = Brushes.Green },
            new map_Brush{ name = "GreenYellow" , brush = Brushes.GreenYellow },
            new map_Brush{ name = "Honeydew" , brush = Brushes.Honeydew },
            new map_Brush{ name = "HotPink" , brush = Brushes.HotPink },
            new map_Brush{ name = "IndianRed" , brush = Brushes.IndianRed },
            new map_Brush{ name = "Indigo" , brush = Brushes.Indigo },
            new map_Brush{ name = "Ivory" , brush = Brushes.Ivory },
            new map_Brush{ name = "Khaki" , brush = Brushes.Khaki },
            new map_Brush{ name = "Lavender" , brush = Brushes.Lavender },
            new map_Brush{ name = "LavenderBlush" , brush = Brushes.LavenderBlush },
            new map_Brush{ name = "LawnGreen" , brush = Brushes.LawnGreen },
            new map_Brush{ name = "LemonChiffon" , brush = Brushes.LemonChiffon },
            new map_Brush{ name = "LightBlue" , brush = Brushes.LightBlue },
            new map_Brush{ name = "LightCoral" , brush = Brushes.LightCoral },
            new map_Brush{ name = "LightCyan" , brush = Brushes.LightCyan },
            new map_Brush{ name = "LightGoldenrodYellow" , brush = Brushes.LightGoldenrodYellow },
            new map_Brush{ name = "LightGray" , brush = Brushes.LightGray },
            new map_Brush{ name = "LightGreen" , brush = Brushes.LightGreen },
            new map_Brush{ name = "LightPink" , brush = Brushes.LightPink },
            new map_Brush{ name = "LightSalmon" , brush = Brushes.LightSalmon },
            new map_Brush{ name = "LightSeaGreen" , brush = Brushes.LightSeaGreen },
            new map_Brush{ name = "LightSkyBlue" , brush = Brushes.LightSkyBlue },
            new map_Brush{ name = "LightSlateGray" , brush = Brushes.LightSlateGray },
            new map_Brush{ name = "LightSteelBlue" , brush = Brushes.LightSteelBlue },
            new map_Brush{ name = "LightYellow" , brush = Brushes.LightYellow },
            new map_Brush{ name = "Lime" , brush = Brushes.Lime },
            new map_Brush{ name = "LimeGreen" , brush = Brushes.LimeGreen },
            new map_Brush{ name = "Linen" , brush = Brushes.Linen },
            new map_Brush{ name = "Magenta" , brush = Brushes.Magenta },
            new map_Brush{ name = "Maroon" , brush = Brushes.Maroon },
            new map_Brush{ name = "MediumAquamarine" , brush = Brushes.MediumAquamarine },
            new map_Brush{ name = "MediumBlue" , brush = Brushes.MediumBlue },
            new map_Brush{ name = "MediumOrchid" , brush = Brushes.MediumOrchid },
            new map_Brush{ name = "MediumPurple" , brush = Brushes.MediumPurple },
            new map_Brush{ name = "MediumSeaGreen" , brush = Brushes.MediumSeaGreen },
            new map_Brush{ name = "MediumSlateBlue" , brush = Brushes.MediumSlateBlue },
            new map_Brush{ name = "MediumSpringGreen" , brush = Brushes.MediumSpringGreen },
            new map_Brush{ name = "MediumTurquoise" , brush = Brushes.MediumTurquoise },
            new map_Brush{ name = "MediumVioletRed" , brush = Brushes.MediumVioletRed },
            new map_Brush{ name = "MidnightBlue" , brush = Brushes.MidnightBlue },
            new map_Brush{ name = "MintCream" , brush = Brushes.MintCream },
            new map_Brush{ name = "MistyRose" , brush = Brushes.MistyRose },
            new map_Brush{ name = "Moccasin" , brush = Brushes.Moccasin },
            new map_Brush{ name = "NavajoWhite" , brush = Brushes.NavajoWhite },
            new map_Brush{ name = "Navy" , brush = Brushes.Navy },
            new map_Brush{ name = "OldLace" , brush = Brushes.OldLace },
            new map_Brush{ name = "Olive" , brush = Brushes.Olive },
            new map_Brush{ name = "OliveDrab" , brush = Brushes.OliveDrab },
            new map_Brush{ name = "Orange" , brush = Brushes.Orange },
            new map_Brush{ name = "OrangeRed" , brush = Brushes.OrangeRed },
            new map_Brush{ name = "Orchid" , brush = Brushes.Orchid },
            new map_Brush{ name = "PaleGoldenrod" , brush = Brushes.PaleGoldenrod },
            new map_Brush{ name = "PaleGreen" , brush = Brushes.PaleGreen },
            new map_Brush{ name = "PaleTurquoise" , brush = Brushes.PaleTurquoise },
            new map_Brush{ name = "PaleVioletRed" , brush = Brushes.PaleVioletRed },
            new map_Brush{ name = "PapayaWhip" , brush = Brushes.PapayaWhip },
            new map_Brush{ name = "PeachPuff" , brush = Brushes.PeachPuff },
            new map_Brush{ name = "Peru" , brush = Brushes.Peru },
            new map_Brush{ name = "Pink" , brush = Brushes.Pink },
            new map_Brush{ name = "Plum" , brush = Brushes.Plum },
            new map_Brush{ name = "PowderBlue" , brush = Brushes.PowderBlue },
            new map_Brush{ name = "Purple" , brush = Brushes.Purple },
            new map_Brush{ name = "Red" , brush = Brushes.Red },
            new map_Brush{ name = "RosyBrown" , brush = Brushes.RosyBrown },
            new map_Brush{ name = "RoyalBlue" , brush = Brushes.RoyalBlue },
            new map_Brush{ name = "SaddleBrown" , brush = Brushes.SaddleBrown },
            new map_Brush{ name = "Salmon" , brush = Brushes.Salmon },
            new map_Brush{ name = "SandyBrown" , brush = Brushes.SandyBrown },
            new map_Brush{ name = "SeaGreen" , brush = Brushes.SeaGreen },
            new map_Brush{ name = "SeaShell" , brush = Brushes.SeaShell },
            new map_Brush{ name = "Sienna" , brush = Brushes.Sienna },
            new map_Brush{ name = "Silver" , brush = Brushes.Silver },
            new map_Brush{ name = "SkyBlue" , brush = Brushes.SkyBlue },
            new map_Brush{ name = "SlateBlue" , brush = Brushes.SlateBlue },
            new map_Brush{ name = "SlateGray" , brush = Brushes.SlateGray },
            new map_Brush{ name = "Snow" , brush = Brushes.Snow },
            new map_Brush{ name = "SpringGreen" , brush = Brushes.SpringGreen },
            new map_Brush{ name = "SteelBlue" , brush = Brushes.SteelBlue },
            new map_Brush{ name = "Tan" , brush = Brushes.Tan },
            new map_Brush{ name = "Teal" , brush = Brushes.Teal },
            new map_Brush{ name = "Thistle" , brush = Brushes.Thistle },
            new map_Brush{ name = "Tomato" , brush = Brushes.Tomato },
            new map_Brush{ name = "Transparent" , brush = Brushes.Transparent },
            new map_Brush{ name = "Turquoise" , brush = Brushes.Turquoise },
            new map_Brush{ name = "Violet" , brush = Brushes.Violet },
            new map_Brush{ name = "Wheat" , brush = Brushes.Wheat },
            new map_Brush{ name = "White" , brush = Brushes.White },
            new map_Brush{ name = "WhiteSmoke" , brush = Brushes.WhiteSmoke },
            new map_Brush{ name = "Yellow" , brush = Brushes.Yellow },
            new map_Brush{ name = "YellowGreen" , brush = Brushes.YellowGreen }
        };

        public static Brush getBrush(string txt)
        {
            Brush brush = Brushes.Cyan;
            foreach (map_Brush b in brush_list)
            {
                if (String.Equals(b.name, txt)) { brush = b.brush; break; }
            }
            return brush;
        }

        //==============================================================

        static public string getRuleOptDef(m_If i)
        {
            return getJointDef(i.v)
                    + relation_getDef_byAxisValue(i.axis, i.value_d)
                    + getJointDef(i.v2)
                    + convertOpt_getDef_byMath(i.opt, i.axis, i.value_d)
                    + (int) Math.Abs(i.value_d * 100)
                    + " cm";
        }

        public static List<map_Relation> relation_list = new List<map_Relation>
        {
            new map_Relation{ axis = "X" , direction = 1 , reverse = false , def = " is to the right of " },
            new map_Relation{ axis = "X" , direction = -1 , reverse = true , def = " is to the left of " },
            new map_Relation{ axis = "Y" , direction = 1 , reverse = false , def = " is above " },
            new map_Relation{ axis = "Y" , direction = -1 , reverse = true , def = " is below " },
            new map_Relation{ axis = "Z" , direction = 1 , reverse = false , def = " is behind " },
            new map_Relation{ axis = "Z", direction = -1, reverse = true , def = " is infront of " },
            new map_Relation{ axis = "D" , direction = 1 , reverse = false , def = " is away from " }
        };

        public static int relation_getDirection_byDef(String axis, String def)
        {
            foreach (map_Relation r in relation_list)
            {
                if (r.axis == axis && r.def == def) { return r.direction; }
            }
            return 1;
        }

        static public Boolean relation_checkReverse(string axis, double d)
        {
            if (d < 0) { return true; }
            else { return false; }
        }

        public static String relation_getAxis_byDef(String def)
        {
            foreach (map_Relation r in relation_list)
            {
                if (r.def == def) { return r.axis; }
            }
            return "X";
        }

        static public string relation_getDef_byAxisValue(string axis, double d)
        {
            Boolean reverse = relation_checkReverse(axis, d);
            foreach (map_Relation r in relation_list)
            {
                if (axis == r.axis && reverse == r.reverse) { return r.def; }
            }
            return "";
        }

        public static List<map_MoveDirection> moveMoveDirection_list = new List<map_MoveDirection>
        {
            new map_MoveDirection{ axis = "Y" , direction = 1 , def = " moves up" },
            new map_MoveDirection{ axis = "Y" , direction = -1 , def = " moves down" },
            new map_MoveDirection{ axis = "Z" , direction = 1 , def = " moves forward" },
            new map_MoveDirection{ axis = "Z" , direction = -1 , def = " moves backward" },
            new map_MoveDirection{ axis = "X" , direction = 1 , def = " moves to the right" },
            new map_MoveDirection{ axis = "X", direction = -1, def = " moves to the left" }
        };

        public static string moveMoveDirection_getDef_byAxisDirection(String axis, int direction)
        {
            foreach (map_MoveDirection r in moveMoveDirection_list)
            {
                if (r.axis == axis && r.direction == direction) { return r.def; }
            }
            return moveMoveDirection_list.First().def;
        }

        public static int moveMoveDirection_getMoveDirection_byDef(String axis, String def)
        {
            foreach (map_Relation r in relation_list)
            {
                if (r.axis == axis && r.def == def) { return r.direction; }
            }
            return 1;
        }

        // "r" is when reverse
        public static List<map_Opt> opt_list = new List<map_Opt>
        {
            new map_Opt{ opt_math = "<" , opt_letter = "l" , opt_r_math = ">" , opt_r_letter = "m" , def = " less than "},
            new map_Opt{ opt_math = ">" , opt_letter = "m", opt_r_math = "<" , opt_r_letter = "l" , def = " more than "},
            new map_Opt{ opt_math = "<=" , opt_letter = "le" , opt_r_math = ">=" , opt_r_letter = "me" , def = " at most "},
            new map_Opt{ opt_math = ">=" , opt_letter = "me", opt_r_math = "<=" , opt_r_letter = "le" , def = " at least "},
            new map_Opt{ opt_math = "!=" , opt_letter = "ne", opt_r_math = "!=" , opt_r_letter = "ne" , def = " not equal to "},
            new map_Opt{ opt_math = "=" , opt_letter = "e" , opt_r_math = "=" , opt_r_letter = "e" , def = " equal to "},
        };

        public static List<String> opt_list_forIF = new List<String> {" less than "," more than "," at most "," at least " };

        public static String opt_reverse(String math)
        {
            if (math == "" || math == "==") { math = "="; }
            else if (math == "<>") { math = "!="; }
            return math;
        }

        public static String opt_refineMath(String math)
        {
            if (math == "" || math == "==") { math = "="; }
            else if (math == "<>") { math = "!="; }
            return math;
        }

        public static String convertOpt_getMath_byLetter(String opt_letter)
        {
            String output = "=";
            opt_letter = opt_refineMath(opt_letter);
            foreach (map_Opt opt in opt_list)
            {
                if (opt.opt_letter == opt_letter) { output = opt.opt_math; }
            }
            return output;
        }


        public static String convertOpt_getLetter_byMath(String opt_math)
        {
            opt_math = opt_refineMath(opt_math);
            String output = "e";
            foreach (map_Opt a in opt_list)
            {
              if (a.opt_math == opt_math) { output = a.opt_letter; }
            }
            return output;
        }

        public static String convertOpt_getDef_byMath(String opt_math, Boolean reverse)
        {
            opt_math = opt_refineMath(opt_math);
            foreach (map_Opt a in opt_list)
            {
                if (!reverse && a.opt_math == opt_math) { return a.def; }
                else if (reverse && a.opt_r_math == opt_math) { return a.def; }
            }
            return opt_math;
        }

        public static String convertOpt_getDef_byMath(String opt_math, String axis, double d)
        {
            opt_math = opt_refineMath(opt_math);
            Boolean reverse = relation_checkReverse(axis, d);
            foreach (map_Opt a in opt_list)
            {
                if (!reverse && a.opt_math == opt_math) { return a.def; }
                else if (reverse && a.opt_r_math == opt_math) { return a.def; }
            }
            return opt_math;
        }

        static public String convertOpt_getMath_byDef(String opt_def, String axis, double d)
        {
            Boolean reverse = relation_checkReverse(axis, d);
            foreach (map_Opt a in opt_list)
            {
                if (a.def == opt_def)
                {
                    if (reverse) { return a.opt_r_math; }
                    else { return a.opt_math; }
                }
            }
            return "=";
        }

        static public String convertOpt_getMath_byDef(String opt_def)
        {
            foreach (map_Opt a in opt_list)
            {
                if (a.def == opt_def) { return a.opt_math; }
            }
            return "=";
        }

        //=======================================================

        public static void saveXML_Motion(String path, List<m_Motion> list_motions)
        {
            String xml_data = @"<motions>";
            foreach (m_Motion m in list_motions)
            {
                xml_data += getXML_Motion(m);
                foreach (m_If i in m.inputs) { xml_data += getXML_If(i); }
                xml_data += "</motion>";
            }
            xml_data += "</motions>";
            //----------
            TheTool.saveXML(xml_data, path);
        }

        public static String getXML_Motion(m_Motion m)
        {
            return @"<motion name=""" + m.name + @""" enabled=""" + TheTool.convertBoolean_01(m.enabled)
                + @""" expand=""" + TheTool.convertBoolean_01(m.expand) + @""">";
        }

        public static String getXML_If(m_If i)
        {
            String txt = @"<if type=""" + i.type + @""" ";
            if (i.type == TheMapData.if_type_TimeAfterPose) { txt += @"value_d=""" + i.value_d + @"""/>"; }
            else if (i.type == TheMapData.if_type_MotionDatabase) { txt += @"v=""" + i.v + @"""/>"; }
            else if (i.type == TheMapData.if_type_2Joint)
            {
                txt += @"axis=""" + i.axis + @""" v=""" + i.v + @""" v2=""" + i.v2
                    + @""" opt=""" + TheMapData.convertOpt_getLetter_byMath(i.opt) + @""" value_d=""" + i.value_d + @"""/>";
            }
            else if (i.type == TheMapData.if_type_Change)
            {
                txt += @"v=""" + i.v + @""" axis=""" + i.axis + @""" opt=""" + TheMapData.convertOpt_getLetter_byMath(i.opt) + @""" value_d=""" + i.value_d + @"""/>";
            }
            else if (i.type == TheMapData.if_type_SphereAngle)
            {
                txt += @"v=""" + i.v + @""" v2=""" + i.v2
                    + @""" opt=""" + TheMapData.convertOpt_getLetter_byMath(i.opt)
                    + @""" value=""" + i.value + @""" value_d=""" + i.value_d + @"""/>";
            }
            else if (i.type == TheMapData.if_type_FlexionAngle)
            {
                txt += @"v=""" + i.v + @""" opt=""" + TheMapData.convertOpt_getLetter_byMath(i.opt) + @""" value_d=""" + i.value_d + @"""/>";
            }
            else if (i.type == TheMapData.if_type_Variable)
            {
                txt += @"v=""" + i.v + @""" opt=""" + TheMapData.convertOpt_getLetter_byMath(i.opt) + @""" value=""" + i.value + @"""/>";
            }
            else if (i.type == TheMapData.if_type_Icon)
            {
                txt += @"v=""" + i.v + @""" brush=""" + i.brush0 + @""" value_d=""" + i.value_d + @"""/>";
            }
            else { txt += @"v=""" + i.v + @""" opt=""" + TheMapData.convertOpt_getLetter_byMath(i.opt) + @""" value=""" + i.value + @"""/>"; }
            return txt;
        }

        public static List<m_If> convertfeaturelist_to_ListIf(List<Feature> feature_list)
        {
            List<m_If> output = new List<m_If>();
            foreach (Feature f in feature_list)
            {
                output.Add(convertFeature_to_If(f));
            }
            return output;
        }

        public static m_If convertFeature_to_If(Feature feature)
        {
            m_If output = new m_If();
            output.opt = feature.opt;
            output.value_d = feature.v;
            string[] data = TheTool.splitText(feature.name, "_");
            if (data.Count() == 3)
            {
                if (data[0] == "r")
                {
                    output.type = if_type_Change;
                    if (feature.name == "r_sp_Y") { output.v = "0"; }
                    else if (feature.name == "r_kL_Y") { output.v = "1"; }
                    else if (feature.name == "r_kR_Y") { output.v = "2"; }
                }
                else
                {
                    //Relative Position
                    output.type = if_type_2Joint;
                    if (data[0] == "Head") { output.v = getJointName(JointType.Head); }
                    else if (data[0] == "Head") { output.v = getJointName(JointType.Head); }
                    else if (data[0] == "Head") { output.v = getJointName(JointType.Head); }
                    else if (data[0] == "ShoulderCenter") { output.v = getJointName(JointType.ShoulderCenter); }
                    else if (data[0] == "ShoulderCenter") { output.v = getJointName(JointType.ShoulderCenter); }
                    else if (data[0] == "ShoulderCenter") { output.v = getJointName(JointType.ShoulderCenter); }
                    else if (data[0] == "ShoulderLeft") { output.v = getJointName(JointType.ShoulderLeft); }
                    else if (data[0] == "ShoulderLeft") { output.v = getJointName(JointType.ShoulderLeft); }
                    else if (data[0] == "ShoulderLeft") { output.v = getJointName(JointType.ShoulderLeft); }
                    else if (data[0] == "ShoulderRight") { output.v = getJointName(JointType.ShoulderRight); }
                    else if (data[0] == "ShoulderRight") { output.v = getJointName(JointType.ShoulderRight); }
                    else if (data[0] == "ShoulderRight") { output.v = getJointName(JointType.ShoulderRight); }
                    else if (data[0] == "ElbowLeft") { output.v = getJointName(JointType.ElbowLeft); }
                    else if (data[0] == "ElbowLeft") { output.v = getJointName(JointType.ElbowLeft); }
                    else if (data[0] == "ElbowLeft") { output.v = getJointName(JointType.ElbowLeft); }
                    else if (data[0] == "ElbowRight") { output.v = getJointName(JointType.ElbowRight); }
                    else if (data[0] == "ElbowRight") { output.v = getJointName(JointType.ElbowRight); }
                    else if (data[0] == "ElbowRight") { output.v = getJointName(JointType.ElbowRight); }
                    else if (data[0] == "WristLeft") { output.v = getJointName(JointType.WristLeft); }
                    else if (data[0] == "WristLeft") { output.v = getJointName(JointType.WristLeft); }
                    else if (data[0] == "WristLeft") { output.v = getJointName(JointType.WristLeft); }
                    else if (data[0] == "WristRight") { output.v = getJointName(JointType.WristRight); }
                    else if (data[0] == "WristRight") { output.v = getJointName(JointType.WristRight); }
                    else if (data[0] == "WristRight") { output.v = getJointName(JointType.WristRight); }
                    else if (data[0] == "HandLeft") { output.v = getJointName(JointType.HandLeft); }
                    else if (data[0] == "HandLeft") { output.v = getJointName(JointType.HandLeft); }
                    else if (data[0] == "HandLeft") { output.v = getJointName(JointType.HandLeft); }
                    else if (data[0] == "HandRight") { output.v = getJointName(JointType.HandRight); }
                    else if (data[0] == "HandRight") { output.v = getJointName(JointType.HandRight); }
                    else if (data[0] == "HandRight") { output.v = getJointName(JointType.HandRight); }
                    else if (data[0] == "Spine") { output.v = getJointName(JointType.Spine); }
                    else if (data[0] == "Spine") { output.v = getJointName(JointType.Spine); }
                    else if (data[0] == "Spine") { output.v = getJointName(JointType.Spine); }
                    else if (data[0] == "HipCenter") { output.v = getJointName(JointType.HipCenter); }
                    else if (data[0] == "HipCenter") { output.v = getJointName(JointType.HipCenter); }
                    else if (data[0] == "HipCenter") { output.v = getJointName(JointType.HipCenter); }
                    else if (data[0] == "HipLeft") { output.v = getJointName(JointType.HipLeft); }
                    else if (data[0] == "HipLeft") { output.v = getJointName(JointType.HipLeft); }
                    else if (data[0] == "HipLeft") { output.v = getJointName(JointType.HipLeft); }
                    else if (data[0] == "HipRight") { output.v = getJointName(JointType.HipRight); }
                    else if (data[0] == "HipRight") { output.v = getJointName(JointType.HipRight); }
                    else if (data[0] == "HipRight") { output.v = getJointName(JointType.HipRight); }
                    else if (data[0] == "KneeLeft") { output.v = getJointName(JointType.KneeLeft); }
                    else if (data[0] == "KneeLeft") { output.v = getJointName(JointType.KneeLeft); }
                    else if (data[0] == "KneeLeft") { output.v = getJointName(JointType.KneeLeft); }
                    else if (data[0] == "KneeRight") { output.v = getJointName(JointType.KneeRight); }
                    else if (data[0] == "KneeRight") { output.v = getJointName(JointType.KneeRight); }
                    else if (data[0] == "KneeRight") { output.v = getJointName(JointType.KneeRight); }
                    else if (data[0] == "AnkleLeft") { output.v = getJointName(JointType.AnkleLeft); }
                    else if (data[0] == "AnkleLeft") { output.v = getJointName(JointType.AnkleLeft); }
                    else if (data[0] == "AnkleLeft") { output.v = getJointName(JointType.AnkleLeft); }
                    else if (data[0] == "AnkleRight") { output.v = getJointName(JointType.AnkleRight); }
                    else if (data[0] == "AnkleRight") { output.v = getJointName(JointType.AnkleRight); }
                    else if (data[0] == "AnkleRight") { output.v = getJointName(JointType.AnkleRight); }
                    else if (data[0] == "FootLeft") { output.v = getJointName(JointType.FootLeft); }
                    else if (data[0] == "FootLeft") { output.v = getJointName(JointType.FootLeft); }
                    else if (data[0] == "FootLeft") { output.v = getJointName(JointType.FootLeft); }
                    else if (data[0] == "FootRight") { output.v = getJointName(JointType.FootRight); }
                    else if (data[0] == "FootRight") { output.v = getJointName(JointType.FootRight); }
                    else if (data[0] == "FootRight") { output.v = getJointName(JointType.FootRight); }
                    //
                    if (data[1] == "SC") { output.v2 = getJointName(JointType.ShoulderCenter); }
                    else if (data[1] == "HC") { output.v2 = getJointName(JointType.HipCenter); }
                    //
                    if (data[2] == "x") { output.axis = "X"; }
                    else if (data[2] == "y") { output.axis = "Y"; }
                    else { output.axis = "Z"; }
                }
            }
            return output;
        }

        static public m_If get_m_If_TimeWithin(double time_s)
        {
            return new m_If() { type = if_type_TimeAfterPose, value_d = time_s };
        }

        //
        //public static List<String> feature_byBP_list = new List<String>
        //{
        //    "Spine's Height" , "Left Knee's Height" , "Right Knee's Height"
        //};

        public static string getChangeFromInitialdef(m_If i)
        {
            int direction = 1;
            string opt_txt = "";
            string direct_txt = "";
            if (i.value_d < 0) { direction = -1; }
            int v_adj = (int)(Math.Abs(i.value_d * 100));
            direct_txt = moveMoveDirection_getDef_byAxisDirection(i.axis, direction);
            if (direction == -1)
            {
                opt_txt = convertOpt_getDef_byMath(i.opt, true);
            }
            else
            {
                opt_txt = convertOpt_getDef_byMath(i.opt, false);
            }
            return
                getJointDef(i.v) + direct_txt + opt_txt + v_adj + " cm";
        }

        public static string getSphereAngledef(m_If i)
        {
            string txt_angle = "";
            if (i.value == then_SphereAngle_Azimuth) { txt_angle = "The azimuthal angle"; }
            else { txt_angle = "The polar angle"; }
            string opt_txt = convertOpt_getDef_byMath(i.opt, false);
            return
                txt_angle + " from " + getJointDef(i.v) + " to " + getJointDef(i.v2) 
                + " is" + opt_txt + i.value_d + " deg";
        }

        public static string getFlexAngledef(m_If i)
        {
            string opt_txt = convertOpt_getDef_byMath(i.opt, false);
            return getJointDef(i.v) + " is flexed" + opt_txt + i.value_d + " deg.";
        }

        public static void checkMissingDatabase(MapData map, List<m_Motion> motion_db, List<m_Event> event_db)
        {
            List<string> reporter = new List<string>();
            List<string> motion_list = new List<string>();
            List<string> event_list = new List<string>();
            foreach (m_Motion m in motion_db) { motion_list.Add(m.name); }
            foreach (m_Event e in event_db) { event_list.Add(e.name); }
            int i_g = 0; 
            foreach(m_Group g in map.groups){
                i_g++;
                int i_g_if = 0; 
                foreach(m_If g_if in g.inputs){
                    i_g_if++;
                    if(g_if.type == if_type_MotionDatabase && motion_list.Contains(g_if.v) == false){
                        reporter.Add(g_if.v + " at " + i_g + "-" + i_g_if);
                    }
                }
                int i_g_d = 0; 
                foreach(m_Detection g_d in g.detections){
                    i_g_d++;
                    int i_g_d_if = 0;
                    foreach (m_If g_d_if in g_d.inputs)
                    {
                        i_g_d_if++;
                        if (g_d_if.type == if_type_MotionDatabase && motion_list.Contains(g_d_if.v) == false)
                        {
                            reporter.Add(g_d_if.v + " at " + i_g + "-" + i_g_d + "-" + i_g_d_if);
                        }
                    }
                    int i_g_d_then = 0;
                    foreach (m_Then g_d_then in g_d.outputs)
                    {
                        i_g_d_then++;
                        if (g_d_then.type == then_type_EventDatabase && event_list.Contains(g_d_then.v) == false)
                        {
                            reporter.Add(g_d_then.v + " at " + i_g + "-" + i_g_d + "-" + i_g_d_then);
                        }
                    }
                }
            }
            if (reporter.Count > 0) {
                TheSys.showError("Missing Database Items");
                TheSys.showError(reporter); 
            }
        }

    }

    //============================================================================

    public class map_Joint
    {
        public String name { get; set; }
        public String def { get; set; }
        public JointType joint { get; set; }
    }

    public class map_Brush
    {
        public String name { get; set; }
        public Brush brush { get; set; }
    }

    public class map_Relation
    {
        public String def { get; set; }
        public String axis { get; set; }
        public int direction { get; set; }
        public Boolean reverse { get; set; }
    }

    public class map_MoveDirection
    {
        public String def { get; set; }
        public String axis { get; set; }
        public int direction { get; set; }
    }

    public class map_Opt
    {
        public String def { get; set; }
        public String opt_math { get; set; }
        public String opt_letter { get; set; }
        public String opt_r_math { get; set; }//reverse
        public String opt_r_letter { get; set; }
    }

}
