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
    public partial class UKI_addIf_Atomic : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addIf_Atomic(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.addIndex = addIndex;
            refreshCombo1();
            combo1.SelectedIndex = 0;
            refreshCombo2(combo1.Text);
            combo2.SelectedIndex = 0;
            comboOpt.SelectedIndex = 0;
        }

        private void refreshCombo1()
        {
            combo1.Items.Clear();
            String last = "";
            foreach (TheMapData.v_BasePosture bp in TheMapData.list_basepose)
            {
                String txt1 = bp.part1;
                if (txt1 != last) { combo1.Items.Add(txt1); }
                last = txt1;
            }
            //Sort
            TheTool.sortComboBox(combo1);
        }

        private void refreshCombo2(String txt1)
        {
            combo2.Items.Clear();
            String last = "";
            foreach (TheMapData.v_BasePosture bp in TheMapData.list_basepose)
            {
                String txt2 = bp.part2;
                if (txt1 == bp.part1 && txt2 != last) { combo2.Items.Add(txt2); }
                last = txt2;
            }
        }


        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            addNew = false;
            this.Title = "Edit Atomic Posture Condition";
            butAdd.Content = "Edit";
            this.i_origin = i_origin;
            foreach (TheMapData.v_BasePosture bp in TheMapData.list_basepose)
            {
                if (i_origin.v == bp.v && i_origin.value == bp.value)
                {
                    combo1.Text = bp.part1;
                    refreshCombo2(combo1.Text);
                    combo2.Text = bp.part2;
                    loadOpt(i_origin.opt);
                }
            }
        }

        void loadOpt(String opt)
        {
            if (opt == "ne" || opt == "!=") { comboOpt.SelectedIndex = 1; }
            else { comboOpt.SelectedIndex = 0; }
        }

        String getOpt()
        {
            if (comboOpt.SelectedIndex == 1) { return "!="; }
            else { return ""; }
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
                    m_If i = new m_If();
                    loadData(i);
                    form_editor.addIf(i, self, map_type, addIndex);
                }
                else
                {
                    int[] loc = form_editor.getTVI_Location(self);
                    loadData(i_origin);
                    form_editor.reloadTree(loc, map_type);
                }
                this.Close();
            }
            catch { }
        }

        void loadData(m_If i)
        {
            i.type = TheMapData.if_type_BasicPose;
            String p1 = combo1.Text;
            String p2 = combo2.Text;
            foreach (TheMapData.v_BasePosture bp in TheMapData.list_basepose)
            {
                if (p1 == bp.part1 && p2 == bp.part2)
                {
                    i.v = bp.v;
                    i.value = bp.value;
                    i.opt = getOpt();
                }
            }
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

        private void combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshCombo2((sender as ComboBox).SelectedItem.ToString());
            combo2.SelectedIndex = 0;
        }


        
    }
}
