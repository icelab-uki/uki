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
    public partial class UKI_addThen_Mouse : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;//add at
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addThen_Mouse(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.addIndex = addIndex;
        }

        m_Then t_origin = null;
        public void editThen_Key(m_Then t_origin)
        {
            try
            {
                addNew = false;
                this.t_origin = t_origin;
                this.Title = "Edit Mouse Move Event";
                butAdd.Content = "Edit";
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
            t.type = TheMapData.then_type_Mouse;
            t.value = comboKey.SelectedIndex + 1;
            t.x = TheTool.getInt(txtX);
            t.y = TheTool.getInt(txtY);
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
