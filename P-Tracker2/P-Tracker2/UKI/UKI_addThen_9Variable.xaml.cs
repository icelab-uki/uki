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
    public partial class UKI_addThen_Variable : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addThen_Variable(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.map_type = map_type;
            this.self = self;
            foreach (m_Variable v in form_editor.detectMap.variables) { comboV.Items.Add(v.name); }
            comboV.SelectedIndex = 0;
            comboOpt.SelectedIndex = 0;
        }

        m_Then t_origin = null;
        public void editThen_Variable(m_Then t_origin)
        {
            try
            {
                addNew = false;
                this.Title = "Edit Variable Event";
                butAdd.Content = "Edit"; 
                //
                this.t_origin = t_origin;
                comboV.Text = t_origin.v;
                comboOpt.Text = convert1(t_origin.opt);
                textValue.Text = t_origin.value.ToString();
            }
            catch { }
        }
        String convert1(String opt)
        {
            if (opt == "+") { return "+="; }
            else if (opt == "-") { return "-="; }
            else { return "="; }
        }

        String convert2(String opt)
        {
            if (opt == "+=") { return "+"; }
            else if (opt == "-=") { return "-"; }
            else { return "="; }
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
            t.type = TheMapData.then_type_Variable;
            t.v = comboV.Text;
            t.opt = convert2(comboOpt.Text);
            t.value = TheTool.getInt(textValue.Text);
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
