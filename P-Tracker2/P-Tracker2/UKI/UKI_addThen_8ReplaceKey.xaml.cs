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
    public partial class UKI_addThen_ReplaceKey : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addThen_ReplaceKey(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.addIndex = addIndex;
            foreach (String s in TheKeySender.key_list_basicOnly) {
                comboV1.Items.Add(s); comboV2.Items.Add(s);
            }
            foreach (String s in TheKeySender.key_list_specialOnly)
            {
                comboV1.Items.Add(s); comboV2.Items.Add(s);
            }
            comboV1.SelectedIndex = 0; comboV2.SelectedIndex = 0;
        }

        m_Then t_origin = null;
        public void edit_ReplaceKey(m_Then t_origin)
        {
            try
            {
                addNew = false;
                this.t_origin = t_origin;
                this.Title = "Edit Key Replacement Event";
                butAdd.Content = "Edit";
                comboV1.Text = t_origin.v;
                if (t_origin.v2 != "") { comboV2.Text = t_origin.v2; }
                else { comboV2.Text = t_origin.v; }
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
            t.type = TheMapData.then_type_ReplaceKey;
            t.v = comboV1.Text;
            if (comboV1.Text != comboV2.Text) { t.v2 = comboV2.Text; }
            else { t.v2 = ""; }
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
