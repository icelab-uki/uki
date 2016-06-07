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
    public partial class UKI_addIf_Relative : Window
    {
        Editor form_editor = null;
        int map_type = 0;//1 MAP , 2 Motion , 3 Event
        TreeViewItem self = null;
        public Boolean addNew = true;//true = Add , false = Edit
        int addIndex = 999;

        public UKI_addIf_Relative(Editor form_editor, TreeViewItem self, int map_type, int addIndex)
        {
            InitializeComponent();
            this.form_editor = form_editor;
            this.self = self;
            this.map_type = map_type;
            this.addIndex = addIndex;
            //--------
            List<String> j_list = new List<String>();
            foreach (map_Joint j in TheMapData.joint_list)
            {
                j_list.Add(j.def);
            }
            j_list.Sort();
            foreach (String s in j_list)
            {
                comboJ1.Items.Add(s);
                comboJ2.Items.Add(s);
            }
            foreach (map_Relation r in TheMapData.relation_list)
            {
                comboRelative.Items.Add(r.def);
            }
            foreach (String opt in TheMapData.opt_list_forIF)
            {
                comboOpt.Items.Add(opt);
            }
            comboJ1.SelectedIndex = 0;
            comboJ2.SelectedIndex = 0;
            comboRelative.SelectedIndex = 0;
            comboOpt.SelectedIndex = 0;
        }


        m_If i_origin = null;
        public void editIf(m_If i_origin)
        {
            addNew = false;
            this.Title = "Edit Relative Joints Condition";
            butAdd.Content = "Edit";
            this.i_origin = i_origin;
            //
            comboJ1.Text = TheMapData.getJointDef(i_origin.v);
            comboJ2.Text = TheMapData.getJointDef(i_origin.v2);
            comboRelative.Text = TheMapData.relation_getDef_byAxisValue(i_origin.axis, i_origin.value_d);
            comboOpt.Text = TheMapData.convertOpt_getDef_byMath(i_origin.opt, i_origin.axis, i_origin.value_d);
            txtValue.Text = (Math.Abs(i_origin.value_d) * 100).ToString();
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
            i.type = TheMapData.if_type_2Joint;
            i.v = TheMapData.getJointName_byDef(comboJ1.Text);
            i.v2 = TheMapData.getJointName_byDef(comboJ2.Text);
            i.axis = TheMapData.relation_getAxis_byDef(comboRelative.Text);
            i.opt = TheMapData.convertOpt_getMath_byDef(comboOpt.Text, i.axis, i.value_d);
            double v = TheTool.getDouble(txtValue.Text);
            v *= TheMapData.relation_getDirection_byDef(i.axis, comboRelative.Text);
            v /= 100;
            i.value_d = v;
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

        private void txtValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

    }
}
