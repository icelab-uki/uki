using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InputManager;
using System.Security.Permissions;
using System.Windows.Media;
using Microsoft.Kinect;

namespace P_Tracker2
{
    public class MapData
    {
        public List<m_Variable> variables = new List<m_Variable>();
        public List<m_Group> groups = new List<m_Group>();
        public Boolean expand_v = false;
        public Boolean expand_map = true;
    }

    public class m_Motion
    {
        public String name = "";
        public Boolean enabled = true;
        public Boolean expand = false;
        public List<m_If> inputs = new List<m_If>();
    }

    public class m_Event
    {
        public String name = "";
        public Boolean enabled = true;
        public Boolean expand = false;
        public List<m_Then> outputs = new List<m_Then>();
    }

    public class m_If
    {
        public int type = 0;
        public String v = "";//variable
        public String v2 = "";//variable
        public String axis = "";//operator
        public String opt = "=";//operator in Math form
        public int value = 0;//value
        public double value_d = 0;//value
        public String brush0 = "";
        //-----------------------------
        public Brush brush = Brushes.Cyan;
        public int value_d_1000 = 0;//value_d * 1000
        public JointType j1;//from v
        public JointType j2;//from 2
    }

    public class m_Then
    {
        public int type = 0;
        public int press = 0;
        public String key0 = "";
        public String v = "";//variable
        public String v2 = "";//variable 2
        public int value = 0;
        public double value_d = 0;
        public List<String> data = new List<String>();
        public String brush0 = "";
        public String opt = "=";//operator in Math form
        public int x = 0;
        public int y = 0;
        //-----------------------------
        public Brush brush = Brushes.Cyan;
        public System.Windows.Forms.Keys key;//key in executable ver
        public int value_d_1000 = 0;//value_d * 1000
    }
    
    public class m_Variable
    {
        public String name = "";
        public int value = 0;
    }

    public class m_Group
    {
        public String name = "";
        public Boolean enabled = true;
        public List<m_If> inputs = new List<m_If>();
        public List<m_Detection> detections = new List<m_Detection>();
        public Boolean expand = false;
        public Boolean expand_if = false;
    }

    public class m_Detection
    {
        public String name = "";
        public Boolean loop = true;
        public Boolean priority = false;//send output by pausing all other thread
        public List<m_If> inputs = new List<m_If>();
        public List<m_Then> outputs = new List<m_Then>();
        public Boolean expand = false;
        public Boolean expand_if = false;
        public Boolean expand_then = false;
        //----------------------------------------------------------
        public int input_step = 0;// 0 = start from the beginning 
        public DateTime input_time_waitUntil = DateTime.Now;
        //No Usage yet
        public int posture_count = 1;//number of segmented postures, can be counted only on FullXML (localized)

        public void input_wait_checkTimeOut()
        {
            if (input_step > 0 && output_is_activate == false)
            {
                if (DateTime.Compare(input_time_waitUntil, DateTime.Now) < 0)
                {
                    input_step = 0;
                }
            }
        }

        //-----------------
        public Thread output_thread;
        public Boolean output_is_activate = false;//being selected as group output : control "posture finish"
        public Boolean output_thread_running = false;//during processing output : control "1 Thread at a time"
        UKI uki = null;

        public void output_checkRunThread(UKI uki)
        {
            if (output_thread_running == false)
            {
                this.uki = uki;
                if (!priority) { output_runThread(); }//start immediate
                else { uki.map_PriorityQue = this; }//wait to process
            }
        }

        public void output_runThread()
        {
            output_thread_running = true;
            output_thread = new Thread(output_processOutput);
            output_thread.Start();
        }

        void output_processOutput()
        {
            try
            {
                foreach (m_Then o in outputs)
                {
                    if (o.type == TheMapData.then_type_Key)
                    {
                        if (o.press == TheMapData.then_key_press) { TheKeySender.KeyPress(uki.getKey_checkReplacment(o)); }
                        else if (o.press == TheMapData.then_key_up) { InputManager.Keyboard.KeyUp(uki.getKey_checkReplacment(o)); }
                        else if (o.press == TheMapData.then_key_holdEoM || o.press == TheMapData.then_key_hold) { InputManager.Keyboard.KeyDown(uki.getKey_checkReplacment(o)); }
                    }
                    else if (o.type == TheMapData.then_type_Mouse)
                    {
                        TheSys.showError("Mouse Click event is temporary unavailable");
                    }
                    else if (o.type == TheMapData.then_type_ReplaceKey)
                    {
                        uki.listReplace_update(o);
                    }
                    else if (o.type == TheMapData.then_type_Variable)
                    {
                        uki.map_processSubOutput_Variable(o);
                    }
                    else if (o.type == TheMapData.then_type_Icon)
                    {
                        uki.prepareIcon(o.v, o.brush, o.value_d_1000);
                    }
                    else if (o.type == TheMapData.then_type_TimeWait)
                    {
                        Thread.Sleep(o.value_d_1000);
                    }
                }
            }
            catch {  }
            output_thread_running = false;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void killThread()
        {
            if (output_thread_running) { 
                output_thread.Abort(); 
                // release Key
                foreach (m_Then o in outputs)
                {
                    if (o.type == TheMapData.then_type_Key)
                    {
                        InputManager.Keyboard.KeyUp(uki.getKey_checkReplacment(o));
                    }
                }
                output_thread_running = false;
                output_is_activate = false;
            }
        }

    }

}
