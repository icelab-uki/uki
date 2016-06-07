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
    public partial class UKI_add_Database : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        Boolean isIF = true;
        int addIndex = 999;

        public UKI_add_Database(Editor form_editor, TreeViewItem self, int map_type, Boolean isIF, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.isIF = isIF;
            this.addIndex = addIndex;
            if (isIF)
            {
                this.Title = "Add Motion from Database Condition";
                txt01.Content = "Motion";
                foreach (m_Motion m in form_editor.list_motions) { comboDB.Items.Add(m.name); }
            }
            else 
            {
                this.Title = "Add Event from Database Event";
                txt01.Content = "Event";
                foreach (m_Event e in form_editor.list_events) { comboDB.Items.Add(e.name); }
            }
            if (comboDB.Items.Count > 0)
            {
                TheTool.sortComboBox(comboDB);
                comboDB.SelectedIndex = 0;
            }
        }

        m_Then t_origin = null;
        public void edit_Database(m_Then t_origin)
        {
            try
            {
                addNew = false;
                this.t_origin = t_origin;
                this.Title = "Edit Motion from Database Event";
                butAdd.Content = "Edit";
                comboDB.Text = t_origin.v;
            }
            catch { }
        }

        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            try
            {
                addNew = false;
                this.i_origin = i_origin;
                this.Title = "Edit Motion from Database Condition";
                comboDB.Text = i_origin.v;
            }
            catch { }
        }

        private void butAdd_Click(object sender, RoutedEventArgs e)
        {
            finish();
        }

        void finish()
        {
            try
            {
                if (addNew)
                {
                    if (isIF)
                    {
                        m_If i = new m_If();
                        loadData(i);
                        form_editor.addIf(i, self, map_type, addIndex);
                    }
                    else
                    {
                        m_Then t = new m_Then();
                        loadData(t);
                        form_editor.addThen(t, self, map_type, addIndex);
                    }
                }
                else
                {
                    int[] loc = form_editor.getTVI_Location(self);
                    if (isIF) { loadData(i_origin); }
                    else { loadData(t_origin); }
                    form_editor.reloadTree(loc, map_type);
                }
                this.Close();
            }
            catch { }
        }

        void loadData(m_Then t)
        {
            t.type = TheMapData.then_type_EventDatabase;
            t.v = comboDB.Text;
        }
        
        void loadData(m_If i)
        {
            i.type = TheMapData.if_type_MotionDatabase;
            i.v = comboDB.Text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
