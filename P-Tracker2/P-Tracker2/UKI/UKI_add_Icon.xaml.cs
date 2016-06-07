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
    public partial class UKI_add_Icon : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        Boolean isIF = true;
        int addIndex = 999;

        public UKI_add_Icon(Editor form_editor, TreeViewItem self, int map_type, Boolean isIF, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.isIF = isIF;
            this.addIndex = addIndex;
            comboColor.Items.Add(noColorChange);
            foreach (map_Brush b in TheMapData.brush_list) { comboColor.Items.Add(b.name); }
            comboColor.SelectedIndex = 0;
            if (isIF) { this.Title = "Add Change Icon Condition"; }
            else { this.Title = "Add Change Icon Event"; }
        }
        public String noColorChange = "No Change";

        m_Then t_origin = null;
        public void edit_Icon(m_Then t_origin)
        {
            addNew = false;
            this.Title = "Edit Change Icon Condition";
            butAdd.Content = "Edit";
            this.t_origin = t_origin;
            if (t_origin.v != "" && t_origin.v != " ") { txt_txt.Text = t_origin.v; }
            txt_time.Text = t_origin.value_d.ToString();
            if (t_origin.brush0 != "")
            {
                comboColor.Text = t_origin.brush0;
                refreshColor(comboColor.Text);
            }
        }

        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            addNew = false;
            this.Title = "Edit Change Icon Event";
            butAdd.Content = "Edit";
            this.i_origin = i_origin;
            if (i_origin.v != "" && i_origin.v != " ") { txt_txt.Text = i_origin.v; }
            txt_time.Text = i_origin.value_d.ToString();
            if (i_origin.brush0 != noColorChange && i_origin.brush0 != "")
            {
                comboColor.Text = i_origin.brush0;
                refreshColor(comboColor.Text);
            }
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
            t.type = TheMapData.then_type_Icon;
            t.value_d = TheTool.getDouble(txt_time.Text);
            if (txt_txt.Text == "") { t.v = " "; }
            else { t.v = txt_txt.Text; }
            if (comboColor.Text != noColorChange) { t.brush0 = comboColor.Text; }
            else { t.brush0 = ""; }
        }

        void loadData(m_If i)
        {
            i.type = TheMapData.if_type_Icon;
            i.value_d = TheTool.getDouble(txt_time.Text);
            if (txt_txt.Text == "") { i.v = " "; }
            else { i.v = txt_txt.Text; }
            if (comboColor.Text != noColorChange) { i.brush0 = comboColor.Text; }
            else { i.brush0 = ""; }
        }

        void refreshColor(String color)
        {
            if (color != "" && color != noColorChange)
            {
                foreach (map_Brush b in TheMapData.brush_list)
                {
                    if (b.name == color) { txtIcon.Foreground = b.brush; break; }
                }
            }
            else { txtIcon.Foreground = Brushes.Cyan; }
        }

        private void txt_txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt_txt.Text == "" || txt_txt.Text == " ") { txtIcon.Content = "T"; }
            else { txtIcon.Content = txt_txt.Text; }
        }

        private void txt_time_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.isTextDouble(e.Text);
        }

        private void comboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { refreshColor((sender as ComboBox).SelectedItem.ToString()); }//to make sure it not use old value
            catch (Exception ex) { TheSys.showError(ex); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_txt.Focus();
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
