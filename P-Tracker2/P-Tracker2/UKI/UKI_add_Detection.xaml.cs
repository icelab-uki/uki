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
    public partial class UKI_add_Detection : Window
    {
        Editor form_editor = null;
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_add_Detection(Editor form_editor, TreeViewItem self, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.addIndex = addIndex;
        }

        m_Detection d_origin = null;
        public void editDetection(m_Detection d_origin)
        {
            addNew = false;
            this.Title = "Edit Detection";
            butAdd.Content = "Edit";
            this.d_origin = d_origin;
            textBox1.Text = d_origin.name;
            checkLoop.IsChecked = d_origin.loop;
            checkPrior.IsChecked = d_origin.priority;
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
                        m_Detection d = new m_Detection();
                        loadData(d);
                        form_editor.addDetection(d, addIndex);
                    }
                    else
                    {
                        int[] loc = form_editor.getTVI_Location(self);
                        loadData(d_origin);
                        form_editor.reloadTree_map_map(loc);
                    }
                    this.Close();
                }
            }
            catch { }
        }

        void loadData(m_Detection d)
        {
            d.name = textBox1.Text;
            d.loop = checkLoop.IsChecked.Value;
            d.priority = checkPrior.IsChecked.Value;
            d.expand = true;
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
