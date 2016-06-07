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
    public partial class UKI_add_Variable : Window
    {
        Editor form_editor = null;
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_add_Variable(Editor form_editor, TreeViewItem self, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.addIndex = addIndex;
        }

        m_Variable v_origin = null;
        public void editVariable(m_Variable v_origin)
        {
            addNew = false;
            this.Title = "Edit Variable";
            butAdd.Content = "Edit";
            this.v_origin = v_origin;
            textBox1.Text = v_origin.name;
            textBox2.Text = v_origin.value.ToString();
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
                        m_Variable v = new m_Variable();
                        loadData(v);
                        form_editor.addVariable(v, addIndex);
                    }
                    else
                    {
                        int[] loc = form_editor.getTVI_Location(self);
                        loadData(v_origin);
                        form_editor.reloadTree_map_variable(loc);
                    }
                    this.Close();
                }
            }
            catch { }
        }

        void loadData(m_Variable v)
        {
            v.name = textBox1.Text;
            v.value = TheTool.getInt(textBox2.Text);
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

        private void textBox2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

    }
}
