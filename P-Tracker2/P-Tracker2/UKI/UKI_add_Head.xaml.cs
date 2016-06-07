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

namespace P_Tracker2
{
    public partial class UKI_add_Head : Window
    {
        Editor form_editor = null;
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int map_type = 0;//see below
        public static int type_e_event = 3;
        public static int type_m_motion = 2;
        public static int type_group = 1;
        int addIndex = 999;

        public UKI_add_Head(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            if (map_type == type_e_event) { this.Title = "Add Event Group"; }
            else if (map_type == type_m_motion) { this.Title = "Add Motion Group"; }
            else if (map_type == type_group) { this.Title = "Add Detection Group"; }
            this.addIndex = addIndex;
        }

        m_Event ev_origin = null;
        public void editEvent(m_Event ev_origin)
        {
            addNew = false;
            this.Title = "Edit Event Group";
            butAdd.Content = "Edit";
            this.ev_origin = ev_origin;
            textBox1.Text = ev_origin.name;
        }

        m_Motion m_origin = null;
        public void editMotion(m_Motion m_origin)
        {
            addNew = false;
            this.Title = "Edit Motion Group";
            butAdd.Content = "Edit";
            this.m_origin = m_origin;
            textBox1.Text = m_origin.name;
        }

        m_Group g_origin = null;
        public void editGroup(m_Group g_origin)
        {
            addNew = false;
            this.Title = "Edit Detection Group";
            butAdd.Content = "Edit";
            this.g_origin = g_origin;
            textBox1.Text = g_origin.name;
        }

        private void butAdd_Click(object sender, RoutedEventArgs e)
        {
            finish();
        }

        void finish()
        {
            try
            {
                String name = textBox1.Text;
                if (name != "")
                {
                    if (addNew)
                    {
                        if (map_type == type_e_event)
                        {
                            m_Event ev = new m_Event();
                            ev.name = name;
                            form_editor.addEvent(ev, addIndex);

                        }
                        else if (map_type == type_m_motion)
                        {
                            m_Motion m = new m_Motion();
                            m.name = name;
                            form_editor.addMotion(m, addIndex);
                        }
                        else if (map_type == type_group)
                        {
                            m_Group g = new m_Group();
                            g.name = name;
                            form_editor.addGroup(g, addIndex);
                        }
                    }
                    else
                    {
                        int[] loc = form_editor.getTVI_Location(self);
                        if (map_type == type_e_event)
                        {
                            ev_origin.name = name;
                            form_editor.reloadTree_event(loc);
                        }
                        else if (map_type == type_m_motion)
                        {
                            m_origin.name = name;
                            form_editor.reloadTree_motion(loc);
                        }
                        else if (map_type == type_group)
                        {
                            g_origin.name = name;
                            form_editor.reloadTree_map_map(loc);
                        }
                    }
                    this.Close();
                }
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Focus();
            this.KeyUp += new KeyEventHandler(hotKey);
        }

        void hotKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                finish();
                return;
            }
        }

    }
}
