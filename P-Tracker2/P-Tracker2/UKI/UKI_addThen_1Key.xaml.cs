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
    public partial class UKI_addThen_Key : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;//add at
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addThen_Key(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.addIndex = addIndex;
            foreach (String s in TheKeySender.key_list_basicOnly) { comboKey.Items.Add(s); }
            foreach (String s in TheKeySender.key_list_specialOnly) { comboKey2.Items.Add(s); }
            comboKey.SelectedIndex = 0;
            comboKey2.SelectedIndex = 0;
            comboType.SelectedIndex = 0;
            txtDef.Content = "";
        }

        m_Then t_origin = null;
        public void editThen_Key(m_Then t_origin)
        {
            try
            {
                addNew = false;
                this.t_origin = t_origin;
                this.Title = "Edit Key Event";
                butAdd.Content = "Edit";
                //
                setKeytype(t_origin.press);
                if (TheTool.stringExist_inList(t_origin.key0, TheKeySender.key_list_basicOnly))
                {
                    radio1.IsChecked = true; comboKey.Text = t_origin.key0;
                }
                else { radio2.IsChecked = true; comboKey2.Text = t_origin.key0; }
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
                    m_Then t = new m_Then();
                    loadData(t);
                    form_editor.addThen(t, self, map_type, addIndex);
                }
                else
                {
                    int[] loc = form_editor.getTVI_Location(self);
                    loadData(t_origin);
                    form_editor.reloadTree(loc, map_type);
                }
                this.Close();
            }
            catch { }
        }

        void loadData(m_Then t)
        {
            t.type = TheMapData.then_type_Key;
            if (radio2.IsChecked.Value) { t.key0 = comboKey2.SelectedValue.ToString(); }
            else { t.key0 = comboKey.SelectedValue.ToString(); }
            t.key = TheKeySender.getKey(t.key0);
            t.press = getKeytype();
        }
        
        int getKeytype()
        {
            if (comboType.SelectedIndex == 0) { return TheMapData.then_key_press; }
            else if (comboType.SelectedIndex == 1) { return TheMapData.then_key_holdEoM; }
            else if (comboType.SelectedIndex == 3) { return TheMapData.then_key_holdEoM; }
            else { return TheMapData.then_key_up; }
        }

        void setKeytype(int i)
        {
            if (i == TheMapData.then_key_press) { comboType.SelectedIndex = 0; }
            else if (i == TheMapData.then_key_holdEoM) { comboType.SelectedIndex = 1; }
            else { comboType.SelectedIndex = 2; }
        }

        private void comboKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtDef.Content = TheKeySender.getKeyDef(comboKey.SelectedValue.ToString());
        }

        private void comboKey2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtDef.Content = TheKeySender.getKeyDef(comboKey2.SelectedValue.ToString());
        }

        private void radio1_Checked(object sender, RoutedEventArgs e)
        {
            comboKey2.IsEnabled = !radio1.IsChecked.Value;
        }

        private void radio2_Checked(object sender, RoutedEventArgs e)
        {
            comboKey.IsEnabled = !radio2.IsChecked.Value;
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
