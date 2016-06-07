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
    public partial class UKI_addIf_Change : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addIf_Change(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.map_type = map_type;
            this.self = self;
            this.addIndex = addIndex;
            foreach (map_Joint j in TheMapData.joint_list) { comboJ1.Items.Add(j.def); }
            foreach (map_Opt opt in TheMapData.opt_list) { comboOpt.Items.Add(opt.def); }
            foreach (map_MoveDirection md in TheMapData.moveMoveDirection_list) { comboDirect.Items.Add(md.def); }
            comboJ1.SelectedIndex = 0;
            comboDirect.SelectedIndex = 0;
            comboOpt.SelectedIndex = 1;
        }

        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            try
            {
                addNew = false;
                this.Title = "Edit Change-from-Initial Condition";
                butAdd.Content = "Edit"; 
                //
                comboJ1.Text = TheMapData.getJointDef(i_origin.v);
                this.i_origin = i_origin;
                int value = (int)(i_origin.value_d * 100);
                comboDirect.Text = TheMapData.moveMoveDirection_getDef_byAxisDirection(i_origin.axis, 1);
                if (value >= 0)
                {
                    comboOpt.Text = TheMapData.convertOpt_getDef_byMath(i_origin.opt, false);
                    txtValue.Text = value.ToString();
                }
                else
                {
                    comboOpt.Text = TheMapData.convertOpt_getDef_byMath(i_origin.opt, true);
                    txtValue.Text = (-value).ToString();
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
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
            int move_direction = comboDirect.SelectedIndex;
            int reverse = TheMapData.moveMoveDirection_list[move_direction].direction;
            double v_modified = TheTool.getDouble(txtValue.Text) / 100;
            v_modified *= reverse;
            i.type = TheMapData.if_type_Change;
            i.v = TheMapData.getJointName_byDef(comboJ1.Text);
            i.axis = TheMapData.moveMoveDirection_list[move_direction].axis;
            i.value_d = v_modified;
            i.opt = TheMapData.convertOpt_getMath_byDef(comboOpt.Text, i.axis, i.value_d);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtValue.Focus();
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
