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
    public partial class UKI_addIf_Time : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addIf_Time(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.addIndex = addIndex;
        }

        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            addNew = false;
            this.Title = "Edit Time Within Condition";
            butAdd.Content = "Edit";
            this.i_origin = i_origin;
            textBox1.Text = i_origin.value_d.ToString();
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
            i.type = TheMapData.if_type_TimeAfterPose;
            i.value_d = TheTool.getDouble(textBox1.Text);
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.isTextDouble(e.Text);
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
