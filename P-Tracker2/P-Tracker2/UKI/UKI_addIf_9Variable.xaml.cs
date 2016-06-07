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
    public partial class UKI_addIf_Variable : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addIf_Variable(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.map_type = map_type;
            this.self = self;
            this.addIndex = addIndex;
            foreach (m_Variable v in form_editor.detectMap.variables) { comboV.Items.Add(v.name); }
            foreach (map_Opt opt in TheMapData.opt_list)
            {
                comboOpt.Items.Add(opt.opt_math);
            }
            comboV.SelectedIndex = 0;
            comboOpt.Text = "=";
        }

        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            try
            {
                addNew = false;
                this.Title = "Edit Variable Condition";
                butAdd.Content = "Edit"; 
                //
                this.i_origin = i_origin;
                comboV.Text = i_origin.v;
                comboOpt.Text = i_origin.opt;
                textValue.Text = i_origin.value.ToString();
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
            i.type = TheMapData.if_type_Variable;
            i.v = comboV.Text;
            i.opt = comboOpt.Text;
            i.value = TheTool.getInt(textValue.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textValue.Focus();
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

        private void textBox2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

    }
}
