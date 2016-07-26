using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Net.Sockets;
using System.Net;

using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Threading;

namespace OKNS
{

    public partial class Window1 : Window
    {
        
        private TabPage tabPage1, tabPage2, tabPage3;
        private System.Windows.Forms.Button button1, button2, button3,btnConfig,btnCalB,btnCalC;
        private System.Windows.Forms.TextBox txtBframe, txtCframe;
        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.Label lbl_AVGHip, lbl_CG, lbl_AVGHip_x, lbl_AVGHip_y, lbl_AVGHip_z, lbl_CG_x, lbl_CG_y, lbl_CG_z, lbl_angle, lbl_angle_FB_var, lbl_angle_LR_var;
        TrackBar trackBar1;
        DataGridView dataGridView1;
        Microsoft.Win32.OpenFileDialog openFileDialog1;
        System.Windows.Forms.Label label1, label2;
        System.Windows.Forms.Timer timer1;
        System.IO.StreamWriter mcFile;
        System.Windows.Forms.CheckBox checkBox1, checkBox2, checkBox3, checkBox4, checkBox5;

        int range_1 = 5;
        int range_2 = 10;
        int range_3 = 15;
        int frame_count = 0;
        

        String[] frame;
        static byte NUM_JOINT = 15;
        bool isPlay, isLoaded;
        double CONVERT_TO_DEGREE = 57.2957795;

        double[] dA = new double[15];
        double[] dB = new double[15];
        double[] dC = new double[15];

        double[] XXXA = new double[15];
        double[] YYYA = new double[15];
        double[] ZZZA = new double[15];

        double[] XXXB = new double[15];
        double[] YYYB = new double[15];
        double[] ZZZB = new double[15];

        double[] XXXC = new double[15];
        double[] YYYC = new double[15];
        double[] ZZZC = new double[15];

        double[] XXXM = new double[15];
        double[] YYYM = new double[15];
        double[] ZZZM = new double[15];

//double xa1 = 0.291706349;
//double xa2 = 0.283829365;
//double xa3 = 0.279821429;
//double xa4 = 0.630595238;
//double xa5 = 0.96281746;
//double xa6 = 1.289742063;
//double xa7 = -0.063253968;
//double xa8 = -0.689761905;
//double xa9 = -0.817222222;
//double xa10 = 0.511577381;
//double xa11 = 0.500257937;
//double xa12 = 0.46140873;
//double xa13 = 0.041240079;
//double xa14 = -0.183164683;
//double xa15 = -0.464563492;


        string[] label = new string[] {
            "Cameras","Tx","Ty","Tz","Ax","Ay","Az",
            "Head","Neck","Torso","ShoulderLeft","ElbowLeft","HandLeft","ShoulderRight","ElbowRight","HandRight","HipLeft","KneeLeft","FootLeft","HipRight","KneeRight","FootRight","Distance","Time"
        };
        string[,] oldvalue = new string[3, 6];
        private double[][,] tranmat = new double[3][,];

        Point3D[] subPointB = new Point3D[15];
        Point3D[] subPointC = new Point3D[15];
        Point3D[] mainPointB = new Point3D[15];
        Point3D[] mainPointC = new Point3D[15];
        Decimal[] sub_joint_noB = new Decimal[15];
        Decimal[] sub_joint_noC = new Decimal[15];
        Decimal[] main_joint_noB = new Decimal[15];
        Decimal[] main_joint_noC = new Decimal[15];
        Double[] sub_confidenceB = new Double[15];
        Double[] sub_confidenceC = new Double[15];
        Double[] main_confidenceB = new Double[15];
        Double[] main_confidenceC = new Double[15];
        bool isCollect = false;
        bool isSettingB = false;
        bool isSettingC = false;

        public Window1()
        {
            InitializeComponent();

            tabPage1 = new TabPage();
            tabPage1.Text = "Run Time";

            tabPage2 = new TabPage();
            tabPage2.Text = "Play Back";

            tabPage3 = new TabPage();
            tabPage3.Text = "Walking Balance";

            tabControl1.TabPages.Add(tabPage1);
            tabControl1.TabPages.Add(tabPage2);
            tabControl1.TabPages.Add(tabPage3);
            tabControl1.SelectedIndex = 0;

            pic1 = new System.Windows.Forms.PictureBox();
            pic1.Parent = tabPage3;
            pic1.Location = new System.Drawing.Point(400, 20);
            pic1.Size = new System.Drawing.Size(300, 300);
            pic1.SizeMode = PictureBoxSizeMode.StretchImage;
            pic1.BorderStyle = BorderStyle.Fixed3D;
            string path = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\images\\0000.jpg";
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
            pic1.Image = image;

            //////////////////////////////

            lbl_AVGHip = new System.Windows.Forms.Label();
            lbl_AVGHip.Parent = tabPage3;
            lbl_AVGHip.Location = new System.Drawing.Point(540, 3);
            lbl_AVGHip.Text = "Front";
            Font myFont1 = new Font(lbl_AVGHip.Font, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline);
            lbl_AVGHip.Font = myFont1;

            lbl_AVGHip = new System.Windows.Forms.Label();
            lbl_AVGHip.Parent = tabPage3;
            lbl_AVGHip.Location = new System.Drawing.Point(540, 320);
            lbl_AVGHip.Text = "Back";
            lbl_AVGHip.Font = myFont1;

            lbl_AVGHip = new System.Windows.Forms.Label();
            lbl_AVGHip.Parent = tabPage3;
            lbl_AVGHip.Location = new System.Drawing.Point(370, 160);
            lbl_AVGHip.Text = "Left";
            lbl_AVGHip.Font = myFont1;

            lbl_AVGHip = new System.Windows.Forms.Label();
            lbl_AVGHip.Parent = tabPage3;
            lbl_AVGHip.Location = new System.Drawing.Point(707, 160);
            lbl_AVGHip.Text = "Right";
            lbl_AVGHip.Font = myFont1;

            //////////////////////////////

            lbl_AVGHip = new System.Windows.Forms.Label();
            lbl_AVGHip.Parent = tabPage3;
            lbl_AVGHip.Location = new System.Drawing.Point(5, 15);
            lbl_AVGHip.Text = "Hip AVG";
            lbl_AVGHip.Font = myFont1;

            lbl_AVGHip_x = new System.Windows.Forms.Label();
            lbl_AVGHip_x.Parent = tabPage3;
            lbl_AVGHip_x.Location = new System.Drawing.Point(5, 35);
            lbl_AVGHip_x.Text = "X : ";

            lbl_AVGHip_y = new System.Windows.Forms.Label();
            lbl_AVGHip_y.Parent = tabPage3;
            lbl_AVGHip_y.Location = new System.Drawing.Point(5, 55);
            lbl_AVGHip_y.Text = "Y : ";

            lbl_AVGHip_z = new System.Windows.Forms.Label();
            lbl_AVGHip_z.Parent = tabPage3;
            lbl_AVGHip_z.Location = new System.Drawing.Point(5, 75);
            lbl_AVGHip_z.Text = "Z : ";

            lbl_CG = new System.Windows.Forms.Label();
            lbl_CG.Parent = tabPage3;
            lbl_CG.Location = new System.Drawing.Point(155, 15);
            lbl_CG.Text = "Center of Gravity";
            lbl_CG.Size = new System.Drawing.Size(150, 22);
            lbl_CG.Font = myFont1;

            lbl_CG_x = new System.Windows.Forms.Label();
            lbl_CG_x.Parent = tabPage3;
            lbl_CG_x.Location = new System.Drawing.Point(155, 35);
            lbl_CG_x.Text = "X : ";

            lbl_CG_y = new System.Windows.Forms.Label();
            lbl_CG_y.Parent = tabPage3;
            lbl_CG_y.Location = new System.Drawing.Point(155, 55);
            lbl_CG_y.Text = "Y : ";

            lbl_CG_z = new System.Windows.Forms.Label();
            lbl_CG_z.Parent = tabPage3;
            lbl_CG_z.Location = new System.Drawing.Point(155, 75);
            lbl_CG_z.Text = "Z : ";


            button1 = new System.Windows.Forms.Button();
            button1.Parent = tabPage2;
            button1.Location = new System.Drawing.Point(5, 5);
            button1.Size = new System.Drawing.Size(60, 22);
            button1.Text = "Open";
            button1.Click += new System.EventHandler(this.button1_Click);

            label1 = new System.Windows.Forms.Label();
            label1.Parent = tabPage2;
            label1.Location = new System.Drawing.Point(70, 5);
            label1.Size = new System.Drawing.Size(100, 22);
            label1.Text = "file";
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.TextAlign = ContentAlignment.MiddleLeft;

            button2 = new System.Windows.Forms.Button();
            button2.Parent = tabPage2;
            button2.Location = new System.Drawing.Point(175, 5);
            button2.Size = new System.Drawing.Size(60, 22);
            button2.Text = "Play";
            button2.Click += new System.EventHandler(this.button2_Click);

            label2 = new System.Windows.Forms.Label();
            label2.Parent = tabPage2;
            label2.Location = new System.Drawing.Point(240, 5);
            label2.Size = new System.Drawing.Size(100, 22);
            label2.Text = "frame";
            label2.BorderStyle = BorderStyle.FixedSingle;
            label2.TextAlign = ContentAlignment.MiddleCenter;

            lbl_angle = new System.Windows.Forms.Label();
            lbl_angle.Parent = tabPage3;
            lbl_angle.Location = new System.Drawing.Point(5, 110);
            lbl_angle.Text = "Balance Angle";
            lbl_angle.Size = new System.Drawing.Size(150, 22);
            Font myFont3 = new Font(lbl_angle.Font, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
            lbl_angle.Font = myFont3;

            lbl_angle_FB_var = new System.Windows.Forms.Label();
            lbl_angle_FB_var.Parent = tabPage3;
            lbl_angle_FB_var.Location = new System.Drawing.Point(5, 130);
            lbl_angle_FB_var.Text = "Front/Back : 0.0";
            lbl_angle_FB_var.Size = new System.Drawing.Size(250, 22);
            Font myFont4 = new Font(lbl_angle_FB_var.Font, System.Drawing.FontStyle.Bold);
            lbl_angle_FB_var.Font = new Font("Tahoma", 18.0F);
            lbl_angle_FB_var.Font = myFont4;

            lbl_angle_LR_var = new System.Windows.Forms.Label();
            lbl_angle_LR_var.Parent = tabPage3;
            lbl_angle_LR_var.Location = new System.Drawing.Point(5, 150);
            lbl_angle_LR_var.Text = "Left/Right : 0.0";
            lbl_angle_LR_var.Size = new System.Drawing.Size(250, 22);
            lbl_angle_LR_var.Font = new Font("Tahoma", 18.0F);
            lbl_angle_LR_var.Font = myFont4;
            
            button3 = new System.Windows.Forms.Button();
            button3.Parent = tabPage1;
            button3.Location = new System.Drawing.Point(5, 5);
            button3.Size = new System.Drawing.Size(60, 22);
            button3.Text = "Start";
            button3.Click += new EventHandler(button3_Click);

            trackBar1 = new TrackBar();
            trackBar1.Location = new System.Drawing.Point(0, 60);
            trackBar1.Size = new System.Drawing.Size(200, 20);
            trackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            trackBar1.Maximum = 0;
            trackBar1.Parent = tabPage2;
            trackBar1.ValueChanged += new EventHandler(trackBar1_ValueChanged);

            dataGridView1 = new DataGridView();
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellValueChanged);


            dataGridView1.Parent = tabPage2;
            dataGridView1.Location = new System.Drawing.Point(0, 110);
            dataGridView1.Size = new System.Drawing.Size(200, 130);
            dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));




            checkBox1 = new System.Windows.Forms.CheckBox();
            checkBox1.Parent = tabPage2;
            checkBox1.Location = new System.Drawing.Point(350, 0);
            checkBox1.Size = new System.Drawing.Size(50, 30);
            checkBox1.Text = "A";
            checkBox1.CheckedChanged += new EventHandler(checkBox1_CheckedChanged);

            checkBox2 = new System.Windows.Forms.CheckBox();
            checkBox2.Parent = tabPage2;
            checkBox2.Location = new System.Drawing.Point(400, 0);
            checkBox2.Size = new System.Drawing.Size(50, 30);
            checkBox2.Text = "B";
            checkBox2.CheckedChanged += new EventHandler(checkBox2_CheckedChanged);

            checkBox3 = new System.Windows.Forms.CheckBox();
            checkBox3.Parent = tabPage2;
            checkBox3.Location = new System.Drawing.Point(450, 0);
            checkBox3.Size = new System.Drawing.Size(50, 30);
            checkBox3.Text = "C";
            checkBox3.CheckedChanged += new EventHandler(checkBox3_CheckedChanged);

            checkBox4 = new System.Windows.Forms.CheckBox();
            checkBox4.Parent = tabPage2;
            checkBox4.Location = new System.Drawing.Point(500, 0);
            checkBox4.Size = new System.Drawing.Size(50, 30);
            checkBox4.Text = "D";
            checkBox4.CheckedChanged += new EventHandler(checkBox4_CheckedChanged);

            checkBox5 = new System.Windows.Forms.CheckBox();
            checkBox5.Parent = tabPage2;
            checkBox5.Location = new System.Drawing.Point(550, 0);
            checkBox5.Size = new System.Drawing.Size(50, 30);
            checkBox5.Text = "E";
            checkBox5.CheckedChanged += new EventHandler(checkBox5_CheckedChanged);

            btnConfig = new System.Windows.Forms.Button();
            btnConfig.Parent = tabPage2;
            btnConfig.Location = new System.Drawing.Point(615, 5);
            btnConfig.Size = new System.Drawing.Size(80, 22);
            btnConfig.Text = "Start Setting";
            btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            btnConfig.Visible = false;//

            btnCalB = new System.Windows.Forms.Button();
            btnCalB.Parent = tabPage2;
            btnCalB.Location = new System.Drawing.Point(700, 5);
            btnCalB.Size = new System.Drawing.Size(80, 22);
            btnCalB.Text = "Setting B";
            btnCalB.Click += new EventHandler(btnCalB_Click);
            btnCalB.Enabled = false;

            btnCalC = new System.Windows.Forms.Button();
            btnCalC.Parent = tabPage2;
            btnCalC.Location = new System.Drawing.Point(785, 5);
            btnCalC.Size = new System.Drawing.Size(80, 22);
            btnCalC.Text = "Setting C";
            btnCalC.Click += new EventHandler(btnCalC_Click);
            btnCalC.Enabled = false;

            openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            openFileDialog1.Filter = "Text Document (*.txt)|*.txt";

            txtBframe = new System.Windows.Forms.TextBox();
            txtBframe.Parent = tabPage2;
            txtBframe.Location = new System.Drawing.Point(700, 30);
            txtBframe.Size = new System.Drawing.Size(80, 22);
            txtBframe.Enabled = false;

            txtCframe = new System.Windows.Forms.TextBox();
            txtCframe.Parent = tabPage2;
            txtCframe.Location = new System.Drawing.Point(785, 30);
            txtCframe.Size = new System.Drawing.Size(80, 22);
            txtCframe.Enabled = false;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 30;
            timer1.Tick += new EventHandler(timer1_Tick);


            for (int i = 0, j = label.Length; i < j; i++)
            {
                dataGridView1.Columns.Add("" + i, label[i]);
                dataGridView1.Columns["" + i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (i == 0)
                {
                    dataGridView1.Columns["" + i].ReadOnly = true;
                    dataGridView1.Columns["" + i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.Columns["" + i].Width = 70;
                }
                else if (i > 6) dataGridView1.Columns["" + i].ReadOnly = true;
                else
                {
                    dataGridView1.Columns["" + i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridView1.Columns["" + i].Width = 50;
                }
            }

            for (byte i = 0; i < 3; i++)
            {
                dataGridView1.Rows.Add();
                for (byte j = 1; j < 7; j++) dataGridView1.Rows[i].Cells[j].Value = "0";
            }
            dataGridView1.Rows[0].Cells[0].Value = "A";
            dataGridView1.Rows[1].Cells[0].Value = "B";
            dataGridView1.Rows[2].Cells[0].Value = "C";

            dataGridView1.Rows.Add();
            dataGridView1.Rows[3].Cells[0].Value = "Composed";
            dataGridView1.AllowUserToAddRows = false;


            for (byte i = 0; i < 3; i++)
            {
                tranmat[i] = new double[4, 4];
                tranmat[i][0, 0] = tranmat[i][1, 1] = tranmat[i][2, 2] = tranmat[i][3, 3] = 1;
                oldvalue[i, 0] = oldvalue[i, 1] = oldvalue[i, 2] = oldvalue[i, 3] = oldvalue[i, 4] = oldvalue[i, 5] = "0";
            }


            Skeleton sRA = new Skeleton();
            sRA.line = System.Windows.Media.Colors.DarkRed;
            sRA.lineThickness = 3;
            sRA.pointThickness = 5;
            sRA.pointSize = 0.03;

            Skeleton sRB = new Skeleton();
            sRB.line = System.Windows.Media.Colors.DarkGreen;
            sRB.lineThickness = 3;
            sRB.pointThickness = 5;
            sRB.pointSize = 0.03;
            //sRB.ShoulderLeft = sRB.ElbowLeft = sRB.HandLeft = sRB.HipLeft = sRB.KneeLeft = sRB.FootLeft = System.Windows.Media.Colors.Red;
            sRB.update();

            Skeleton sRC = new Skeleton();
            sRC.line = System.Windows.Media.Colors.DarkBlue;
            sRC.lineThickness = 3;
            sRC.pointThickness = 5;
            sRC.pointSize = 0.03;


            Skeleton sTA = new Skeleton();
            sTA.line = System.Windows.Media.Colors.DarkRed;
            sTA.lineThickness = 3;
            sTA.pointThickness = 5;
            sTA.pointSize = 0.03;
            //sTA.ShoulderLeft = sTA.ElbowLeft = sTA.HandLeft = sTA.HipLeft = sTA.KneeLeft = sTA.FootLeft = System.Windows.Media.Colors.Red;
            sTA.update();
            Skeleton sTB = new Skeleton();
            sTB.line = System.Windows.Media.Colors.DarkGreen;
            sTB.lineThickness = 3;
            sTB.pointThickness = 5;
            sTB.pointSize = 0.03;
            //sTB.ShoulderLeft = sTB.ElbowLeft = sTB.HandLeft = sTB.HipLeft = sTB.KneeLeft = sTB.FootLeft = System.Windows.Media.Colors.Red;
            sTB.update();
            Skeleton sTC = new Skeleton();
            sTC.line = System.Windows.Media.Colors.DarkBlue;
            sTC.lineThickness = 3;
            sTC.pointThickness = 5;
            sTC.pointSize = 0.03;
            //sTC.ShoulderLeft = sTC.ElbowLeft = sTC.HandLeft = sTC.HipLeft = sTC.KneeLeft = sTC.FootLeft = System.Windows.Media.Colors.Red;
            sTC.update();

            Skeleton sMA = new Skeleton();
            sMA.line = System.Windows.Media.Colors.DarkRed;
            sMA.lineThickness = 3;
            sMA.pointThickness = 5;
            sMA.pointSize = 0.015;
            Skeleton sMB = new Skeleton();
            sMB.line = System.Windows.Media.Colors.DarkGreen;
            sMB.lineThickness = 3;
            sMB.pointThickness = 5;
            sMB.pointSize = 0.015;
            Skeleton sMC = new Skeleton();
            sMC.line = System.Windows.Media.Colors.DarkBlue;
            sMC.lineThickness = 3;
            sMC.pointThickness = 5;
            sMC.pointSize = 0.015;
            Skeleton sMM = new Skeleton();
            sMM.line = System.Windows.Media.Colors.Black;
            sMM.lineThickness = 3;
            sMM.pointThickness = 5;
            sMM.pointSize = 0.015;

            CamView cRA = new CamView(viewA, 1);
            cRA.skeletons[0] = sRA;
            cRA.Update();

            CamView cRB = new CamView(viewB, 1);
            cRB.skeletons[0] = sRB;
            cRB.Update();

            CamView cRC = new CamView(viewC, 1);
            cRC.skeletons[0] = sRC;
            cRC.Update();

            /*
            CamView cTA = new CamView(viewTA, 1);
            cTA.skeletons[0] = sTA;
            cTA.Update();

            CamView cTB = new CamView(viewTB, 1);
            cTB.skeletons[0] = sTB;
            cTB.Update();

            CamView cTC = new CamView(viewTC, 1);
            cTC.skeletons[0] = sTC;
            cTC.Update();
            */
            CamView cMM = new CamView(viewMain, 4);
            cMM.skeletons[0] = sMA;
            cMM.skeletons[1] = sMB;
            cMM.skeletons[2] = sMC;
            cMM.skeletons[3] = sMM;
            cMM.drawGrid(5, 1);
            cMM.Update();

            skeletons = new Skeleton[10];
            skeletons[0] = sRA;
            skeletons[1] = sRB;
            skeletons[2] = sRC;
            skeletons[3] = sTA;
            skeletons[4] = sTB;
            skeletons[5] = sTC;
            skeletons[6] = sMA;
            skeletons[7] = sMB;
            skeletons[8] = sMC;
            skeletons[9] = sMM;

            views = new CamView[7];
            views[0] = cRA;
            views[1] = cRB;
            views[2] = cRC;
//            views[3] = cTA;
  //          views[4] = cTB;
    //        views[5] = cTC;
            views[6] = cMM;

            isLoaded = true;
        }

        void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }
        void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }
        void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }
        void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }
        void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            draw();
        }
        void btnConfig_Click(object sender, EventArgs e)
        {
            isCollect = !isCollect;
            if (isCollect)
                btnConfig.Text = "Stop Setting";
            else
                btnConfig.Text = "Start Setting";
        }
        void btnCalB_Click(object sender, EventArgs e)
        {
            //isSettingB = true;
            btnCalB.Enabled = false;
            Thread progressThread = new Thread(delegate()
            {

                ProgressForm PF = new ProgressForm();
                PF.ShowDialog();
            });
            progressThread.SetApartmentState(ApartmentState.STA);
            progressThread.Start();
            
            preSettingKinectCamera(mainPointB, subPointB, "B");

            progressThread.Abort();

            //this.Cursor = System.Windows.Input.Cursors.Wait;
            //preSettingKinectCamera(mainPointB, subPointB,"B");
            //this.Cursor = System.Windows.Input.Cursors.Arrow;
            
        }
        void btnCalC_Click(object sender, EventArgs e)
        {
            isSettingC = true;
            btnCalC.Enabled = false;
            Thread progressThread = new Thread(delegate()
            {

                ProgressForm PF = new ProgressForm();
                PF.ShowDialog();
            });
            progressThread.SetApartmentState(ApartmentState.STA);
            progressThread.Start();
            preSettingKinectCamera(mainPointC, subPointC, "C");
            progressThread.Abort();
            //this.Cursor = System.Windows.Input.Cursors.Wait;
            //preSettingKinectCamera(mainPointC, subPointC,"C");
            //this.Cursor = System.Windows.Input.Cursors.Arrow;
        }
        Skeleton[] skeletons;
        CamView[] views;



        bool isStart;
        System.Threading.Thread listen;

        void button3_Click(object sender, EventArgs e)
        {


            if (isStart)
            {
                isStart = false;
                button3.Text = "Start";
                mcFile.Close();
            }
            else
            {
                mcFile = new System.IO.StreamWriter("" + System.DateTime.Now.Day + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + ".txt");
                isStart = true;
                listen = new System.Threading.Thread(listenData);
                listen.Start();
                button3.Text = "Stop";
            }
        }
        private void listenData()
        {
            TcpListener server = null;
            string data = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, 8888);
                server.Start();
                byte[] bytes = new byte[2000];
                
                while (isStart)
                {
                    if (!server.Pending()) continue;
                    data = null;
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    stream.Read(bytes, 0, bytes.Length);
                    data = System.Text.Encoding.ASCII.GetString(bytes);
                    data = data.Substring(0, data.IndexOf('$'));
                    mcFile.WriteLine(data);

                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (MethodInvoker)delegate
                    {
                        setData(data);
                    });
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                Console.WriteLine(data);
            }
            finally
            {
                server.Stop();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((bool)openFileDialog1.ShowDialog())
            {
                string path = openFileDialog1.FileName;
                string line;
                bool isValidDataFile = true;
                int i = 0;
                System.IO.StreamReader file = new System.IO.StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Trim().Length > 0)
                    {
                        try
                        {
                            testData(line.Trim());
                            i++;
                        }
                        catch (Exception er)
                        {
                            Console.WriteLine(er + "\n" + line);
                            isValidDataFile = false;
                            break;
                        }
                    }
                }
                file.Close();
                if (!isValidDataFile || i < 1) System.Windows.MessageBox.Show("Invalid data file!", "Error");
                else
                {
                    frame = new string[i];
                    i = 0;
                    file = new System.IO.StreamReader(path);
                    while ((line = file.ReadLine()) != null) if (line.Trim().Length > 0) frame[i++] = line.Trim();
                    file.Close();
                    prePlay(path);
                }
            }
        }
        private void testData(string data)
        {
            string[] t = data.Split(' ');
            if (!t[0].Equals("A") && !t[0].Equals("B") && !t[0].Equals("C") && !t[0].Equals("D")) throw new Exception();
            double[][][] body = new double[2][][];
            body[0] = new double[NUM_JOINT][];
            byte j = 1;
            for (byte i = 0; i < NUM_JOINT; i++) body[0][int.Parse(t[j++])] = new double[] { double.Parse(t[j++]), double.Parse(t[j++]), double.Parse(t[j++]), double.Parse(t[j++]) };
            /*
            if (t.Length > j)
            {
                body[1] = new double[NUM_JOINT][];
                for (byte i = 0; i < NUM_JOINT; i++) body[1][int.Parse(t[j++])] = new double[] { int.Parse(t[j++]), double.Parse(t[j++]), double.Parse(t[j++]), double.Parse(t[j++]) };
            }
            */
        }
        private void prePlay(string path)
        {
            label1.Text = path.Substring(path.LastIndexOf('\\') + 1);
            label2.Text = "0 / " + (frame.Length - 1);
            trackBar1.Value = 0;
            trackBar1.Maximum = frame.Length - 1;
            PausePlay();
        }
        private void PausePlay()
        {
            isPlay = false;
            timer1.Stop();
            button2.Text = "Play";
            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (frame == null) return;
            if (isPlay) PausePlay();
            else
            {
                isPlay = true;
                timer1.Start();
                button2.Text = "Pause";
            }
            
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (trackBar1.Value < trackBar1.Maximum) trackBar1.Value++;
            else
            {
                PausePlay();
                trackBar1.Value = 0;
            }
        }
        void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            int n = trackBar1.Value;

            setData(frame[n]);
            label2.Text = n + " / " + frame.Length;
        }
        void setData(string data)
        {
            string[] t = data.Split(' ');
            byte cam = 0;
            if (t[0].Equals("A"))
            {
                cam = 0;
                views[0].Active();
                views[1].Active();
                views[2].Active();
            }
            else if (t[0].Equals("B"))
            {
                cam = 1;
                views[0].Deactive();
                views[1].Active();
                views[2].Deactive();
            }
            else if (t[0].Equals("C"))
            {
                cam = 2;
                views[0].Deactive();
                views[1].Deactive();
                views[2].Active();
            }
            //if (cam == 0)
            //{
                byte j = 1, jn;
                views[cam].Clear();
                bool h = false;
                for (byte i = 0; i < NUM_JOINT; i++)
                {
                    jn = byte.Parse(t[j++]);
                    skeletons[cam].joints[jn].F = double.Parse(t[j++]);
                    skeletons[cam].joints[jn].X = double.Parse(t[j++]);
                    skeletons[cam].joints[jn].Y = double.Parse(t[j++]);
                    skeletons[cam].joints[jn].Z = double.Parse(t[j++]);
                    if (skeletons[cam].joints[jn].F == 1) h = true;
                }
                skeletons[cam].hasSkeleton = h;
                views[cam].Draw();
                draw();
            //}
            
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (isLoaded && e.ColumnIndex < 7)
            {
                try
                {
                    if (e.RowIndex > 2) return;
                    double d = double.Parse("" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    if (e.ColumnIndex < 4 && (d > 10 || d < -10)) throw new Exception();
                    else if (e.ColumnIndex > 3 && (d > 361 || d < -361)) throw new Exception();
                    oldvalue[e.RowIndex, e.ColumnIndex - 1] = "" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                    double X = double.Parse(oldvalue[e.RowIndex, 0]);
                    double Y = double.Parse(oldvalue[e.RowIndex, 1]);
                    double Z = double.Parse(oldvalue[e.RowIndex, 2]);
                    double Ax = double.Parse(oldvalue[e.RowIndex, 3]);
                    double Ay = double.Parse(oldvalue[e.RowIndex, 4]);
                    double Az = double.Parse(oldvalue[e.RowIndex, 5]);

                    double SAx = Ax == -360 || Ax == -180 || Ax == 0 || Ax == 180 || Ax == 360 ? 0 : Ax == -270 || Ax == 90 ? 1 : Ax == -90 || Ax == 270 ? -1 : Math.Sin(Deg2Rad(Ax));
                    double CAx = Ax == -270 || Ax == -90 || Ax == 90 || Ax == 270 ? 0 : Ax == -360 || Ax == 0 || Ax == 360 ? 1 : Ax == -180 || Ax == 180 ? -1 : Math.Cos(Deg2Rad(Ax));
                    double SAy = Ay == -360 || Ay == -180 || Ay == 0 || Ay == 180 || Ay == 360 ? 0 : Ay == -270 || Ay == 90 ? 1 : Ay == -90 || Ay == 270 ? -1 : Math.Sin(Deg2Rad(Ay));
                    double CAy = Ay == -270 || Ay == -90 || Ay == 90 || Ay == 270 ? 0 : Ay == -360 || Ay == 0 || Ay == 360 ? 1 : Ay == -180 || Ay == 180 ? -1 : Math.Cos(Deg2Rad(Ay));
                    double SAz = Az == -360 || Az == -180 || Az == 0 || Az == 180 || Az == 360 ? 0 : Az == -270 || Az == 90 ? 1 : Az == -90 || Az == 270 ? -1 : Math.Sin(Deg2Rad(Az));
                    double CAz = Az == -270 || Az == -90 || Az == 90 || Az == 270 ? 0 : Az == -360 || Az == 0 || Az == 360 ? 1 : Az == -180 || Az == 180 ? -1 : Math.Cos(Deg2Rad(Az));

                    double[,] TM = new double[4, 4];
                    TM[0, 0] = CAz * CAy;
                    TM[0, 1] = (-1 * SAz * CAx) + (CAz * SAy * SAx);
                    TM[0, 2] = (SAz * SAx) + (CAz * SAy * CAx);
                    TM[0, 3] = X;

                    TM[1, 0] = SAz * CAy;
                    TM[1, 1] = (CAz * CAx) + (SAz * SAy * SAx);
                    TM[1, 2] = (-1 * CAz * SAx) + (SAz * SAy * CAx);
                    TM[1, 3] = Y;

                    TM[2, 0] = -1 * SAy;
                    TM[2, 1] = CAy * SAx;
                    TM[2, 2] = CAy * CAx;
                    TM[2, 3] = Z;

                    TM[3, 0] = TM[3, 1] = TM[3, 2] = 0;
                    TM[3, 3] = 1;

                    Console.WriteLine(TM[0, 0] + " " + TM[0, 1] + " " + TM[0, 2] + " " + TM[0, 3]);
                    Console.WriteLine(TM[1, 0] + " " + TM[1, 1] + " " + TM[1, 2] + " " + TM[1, 3]);
                    Console.WriteLine(TM[2, 0] + " " + TM[2, 1] + " " + TM[2, 2] + " " + TM[2, 3]);
                    Console.WriteLine(TM[3, 0] + " " + TM[3, 1] + " " + TM[3, 2] + " " + TM[3, 3]);
                    Console.WriteLine("------------------");

                    tranmat[e.RowIndex] = TM;
                    draw();
                }
                catch (Exception er)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldvalue[e.RowIndex, e.ColumnIndex - 1];
                    //Console.WriteLine(e.ColumnIndex + " " + e.RowIndex + "\n" + er);
                }
            }
        }
        private double Deg2Rad(double angle) { return Math.PI * angle / 180.0; }
        private double Rad2Deg(double angle) { return angle * (180.0 / Math.PI); }
        void swap(Joint j1, Joint j2)
        {
            double x = j1.X, y = j1.Y, z = j1.Z, f = j1.F;
            j1.X = j2.X;
            j1.Y = j2.Y;
            j1.Z = j2.Z;
            j1.F = j2.F;
            j2.X = x;
            j2.Y = y;
            j2.Z = z;
            j2.F = f;
        }
        void draw()
        {
            byte n = 0;
            views[6].Clear();
            double F_A = 0.0;
            double F_B = 0.0;
            double F_C = 0.0;
            for (byte i = 0, c = 3, m = 6; i < 3; i++, c++, m++)
            {
                if (views[c]!=null) views[c].Clear();
                if (skeletons[i].hasSkeleton == false)
                {
                    skeletons[c].hasSkeleton = skeletons[m].hasSkeleton = false;
                    for (byte j = 0; j < NUM_JOINT; j++) dataGridView1.Rows[i].Cells[j + 7].Value = "";
                    continue;
                }
                else
                {
                    //---------------------------------collect data for setting------------------------
                    //if (isCollect)//isCollect change value by btnConfig Click
                    {
                        for (byte j = 0; j < NUM_JOINT; j++)
                        {
                            F_A += skeletons[0].joints[j].F;
                            F_B += skeletons[1].joints[j].F;
                            F_C += skeletons[2].joints[j].F;
                        }
                        if (F_A == 15 && F_B == 15)
                        {
                            txtBframe.Text = trackBar1.Value.ToString();
                            for (byte j = 0; j < NUM_JOINT; j++)
                            {
                                subPointB[j] = new Point3D(skeletons[1].joints[j].X, skeletons[1].joints[j].Y, skeletons[1].joints[j].Z);
                                mainPointB[j] = new Point3D(skeletons[0].joints[j].X, skeletons[0].joints[j].Y, skeletons[0].joints[j].Z);

                                sub_confidenceB[j] = skeletons[1].joints[j].F;
                                main_confidenceB[j] = skeletons[0].joints[j].F;
                            }
                            btnCalB.Enabled = true;
                            //if (isSettingB)
                            //{
                                
                            //    isSettingB = false;
                            //    Thread progressThread = new Thread(delegate()
                            //    {

                            //        ProgressForm PF = new ProgressForm();
                            //        PF.ShowDialog();
                            //    });
                            //    progressThread.SetApartmentState(ApartmentState.STA);
                            //    progressThread.Start();

                            //    preSettingKinectCamera(mainPointB, subPointB, "B");
                            //    progressThread.Abort();
                            //}
                        }
                        if (F_A == 15 && F_C == 15)
                        {
                            txtCframe.Text = trackBar1.Value.ToString();
                            for (byte j = 0; j < NUM_JOINT; j++)
                            {
                                subPointC[j] = new Point3D(skeletons[2].joints[j].X, skeletons[2].joints[j].Y, skeletons[2].joints[j].Z);
                                mainPointC[j] = new Point3D(skeletons[0].joints[j].X, skeletons[0].joints[j].Y, skeletons[0].joints[j].Z);

                                sub_confidenceC[j] = skeletons[2].joints[j].F;
                                main_confidenceC[j] = skeletons[0].joints[j].F;
                            }
                            btnCalC.Enabled = true;
                            //if (isSettingC)
                            //{
                                
                            //    isSettingC = false;
                            //    Thread progressThread = new Thread(delegate()
                            //    {

                            //        ProgressForm PF = new ProgressForm();
                            //        PF.ShowDialog();
                            //    });
                            //    progressThread.SetApartmentState(ApartmentState.STA);
                            //    progressThread.Start();
                            //    preSettingKinectCamera(mainPointC, subPointC, "C");
                            //    progressThread.Abort();
                            //}
                        }
                        F_A = F_B = F_C = 0.0;
                    }
                    //----------------------------------------------------------------------------------
                    n++;
                    skeletons[c].hasSkeleton = skeletons[m].hasSkeleton = true;
                    for (byte j = 0; j < NUM_JOINT; j++)
                    {
                        double x = skeletons[i].joints[j].X;
                        double y = skeletons[i].joints[j].Y;
                        double z = skeletons[i].joints[j].Z;
                        skeletons[c].joints[j].F = skeletons[m].joints[j].F = skeletons[i].joints[j].F;
                        skeletons[c].joints[j].X = skeletons[m].joints[j].X = Math.Round((tranmat[i][0, 0] * x) + (tranmat[i][0, 1] * y) + (tranmat[i][0, 2] * z) + tranmat[i][0, 3], 2);
                        skeletons[c].joints[j].Y = skeletons[m].joints[j].Y = Math.Round((tranmat[i][1, 0] * x) + (tranmat[i][1, 1] * y) + (tranmat[i][1, 2] * z) + tranmat[i][1, 3], 2);
                        skeletons[c].joints[j].Z = skeletons[m].joints[j].Z = Math.Round((tranmat[i][2, 0] * x) + (tranmat[i][2, 1] * y) + (tranmat[i][2, 2] * z) + tranmat[i][2, 3], 2);
                        dataGridView1.Rows[i].Cells[j + 7].Value = skeletons[c].joints[j].F + " " + skeletons[c].joints[j].X + " " + skeletons[c].joints[j].Y + " " + skeletons[c].joints[j].Z;
                    }
                    if (views[c]!=null) views[c].Draw();
                }
            }


            if (checkBox1.Checked == true && skeletons[3].hasSkeleton)
            {
                swap(skeletons[3].joints[3], skeletons[3].joints[6]);
                swap(skeletons[3].joints[4], skeletons[3].joints[7]);
                swap(skeletons[3].joints[5], skeletons[3].joints[8]);
                swap(skeletons[3].joints[9], skeletons[3].joints[12]);
                swap(skeletons[3].joints[10], skeletons[3].joints[13]);
                swap(skeletons[3].joints[11], skeletons[3].joints[14]);
            }
            if (checkBox2.Checked == true && skeletons[4].hasSkeleton)
            {
                swap(skeletons[4].joints[3], skeletons[4].joints[6]);
                swap(skeletons[4].joints[4], skeletons[4].joints[7]);
                swap(skeletons[4].joints[5], skeletons[4].joints[8]);
                swap(skeletons[4].joints[9], skeletons[4].joints[12]);
                swap(skeletons[4].joints[10], skeletons[4].joints[13]);
                swap(skeletons[4].joints[11], skeletons[4].joints[14]);
            }
            if (checkBox3.Checked == true && skeletons[5].hasSkeleton)
            {
                swap(skeletons[5].joints[3], skeletons[5].joints[6]);
                swap(skeletons[5].joints[4], skeletons[5].joints[7]);
                swap(skeletons[5].joints[5], skeletons[5].joints[8]);
                swap(skeletons[5].joints[9], skeletons[5].joints[12]);
                swap(skeletons[5].joints[10], skeletons[5].joints[13]);
                swap(skeletons[5].joints[11], skeletons[5].joints[14]);
            }

            if (checkBox4.Checked == true)
            {
                skeletons[6].hide = skeletons[7].hide = skeletons[8].hide = true;
            }
            else
            {
                skeletons[6].hide = skeletons[7].hide = skeletons[8].hide = false;
            }
            if (checkBox5.Checked == true)
            {
                skeletons[9].hide = true;
            }
            else
            {
                skeletons[9].hide = false;
            }



            if (n > 0)
            {
                views[6].Clear();
                skeletons[9].hasSkeleton = true;
                double x, y, z;
                double balance_angle_FB = 0.0;
                double balance_angle_LR = 0.0;
                double xHip, yHip, zHip;
                double xShoulder, yShoulder, zShoulder;
                double xCG, yCG, zCG;
                xHip = yHip = zHip = 0;
                xShoulder = yShoulder = zShoulder = 0;
                xCG = yCG = zCG = 0;


                
                for (byte j = 0, d = 0; j < NUM_JOINT; j++)
                {
                    x = y = z = d = 0;
                    for (byte i = 3; i < 6; i++)
                    {
                        if (skeletons[i].joints[j].F == 1)
                        {
                            x += skeletons[i].joints[j].X;
                            y += skeletons[i].joints[j].Y;
                            z += skeletons[i].joints[j].Z;
                            d++;
                        }
                    }
                    if (d > 0)
                    {
                        skeletons[9].joints[j].F = 1;
                        skeletons[9].joints[j].X = Math.Round(x / d, 2);
                        skeletons[9].joints[j].Y = Math.Round(y / d, 2);
                        skeletons[9].joints[j].Z = Math.Round(z / d, 2);
                        frame_count++;
                        dA[j] += 100 * getDistance(skeletons[9].joints[j].X, skeletons[9].joints[j].Y, skeletons[9].joints[j].Z,
                            skeletons[3].joints[j].X, skeletons[3].joints[j].Y, skeletons[3].joints[j].Z);
                        dB[j] += 100 * getDistance(skeletons[9].joints[j].X, skeletons[9].joints[j].Y, skeletons[9].joints[j].Z,
                            skeletons[4].joints[j].X, skeletons[4].joints[j].Y, skeletons[4].joints[j].Z);
                        dC[j] += 100 * getDistance(skeletons[9].joints[j].X, skeletons[9].joints[j].Y, skeletons[9].joints[j].Z,
                            skeletons[5].joints[j].X, skeletons[5].joints[j].Y, skeletons[5].joints[j].Z);

                        XXXA[j] += skeletons[3].joints[j].X;
                        YYYA[j] += skeletons[3].joints[j].Y;
                        ZZZA[j] += skeletons[3].joints[j].Z;

                        XXXB[j] += skeletons[4].joints[j].X;
                        YYYB[j] += skeletons[4].joints[j].Y;
                        ZZZB[j] += skeletons[4].joints[j].Z;

                        XXXC[j] += skeletons[5].joints[j].X;
                        YYYC[j] += skeletons[5].joints[j].Y;
                        ZZZC[j] += skeletons[5].joints[j].Z;

                        XXXM[j] += skeletons[9].joints[j].X;
                        YYYM[j] += skeletons[9].joints[j].Y;
                        ZZZM[j] += skeletons[9].joints[j].Z;

                        double xxx = 0.0;
                        xxx = Math.Pow(XXXA[j] - XXXM[j], 2);

                        //using (StreamWriter w = File.AppendText("data.txt"))
                        //{
                        //    w.WriteLine("  :{0}", skeletons[3].joints[j].X);
                        //}

                        //new point
                        if ((j == 9) || (j == 12))
                        {
                            xHip += skeletons[9].joints[j].X / 2;
                            yHip += skeletons[9].joints[j].Y / 2;
                            zHip += skeletons[9].joints[j].Z / 2;
                        }
                        else if ((j == 3) || (j == 6))
                        {
                            xShoulder += skeletons[9].joints[j].X / 2;
                            yShoulder += skeletons[9].joints[j].Y / 2;
                            zShoulder += skeletons[9].joints[j].Z / 2;
                        }
                        else if (j == 14)
                        {
                            //calculate CG
                            xCG = xHip + (xShoulder - xHip) * 0.54;
                            yCG = yHip + (yShoulder - yHip) * 0.54;
                            zCG = zHip + (zShoulder - zHip) * 0.54;

                            //calulate balance angle front-back
                            //double d_a = getDistance(xCG, yCG, zCG, xHip, yCG, zHip);
                            //double d_b = getDistance(xHip, yHip, zHip, xCG, yCG, zCG);
                            //balance_angle = (Math.Sin(d_a / d_b) * CONVERT_TO_DEGREE);
                            double d_a_FB = getDistance(xCG, yCG, zCG, xCG, yCG, zHip);
                            double d_b_FB = getDistance(xCG, yCG, zCG, xCG, yHip, zHip);
                            balance_angle_FB = (Math.Sin(d_a_FB / d_b_FB) * CONVERT_TO_DEGREE);

                            double d_a_LR = getDistance(xCG, yCG, zCG, xHip, yCG, zCG);
                            double d_b_LR = getDistance(xCG, yCG, zCG, xHip, yHip, zCG);
                            balance_angle_LR = (Math.Sin(d_a_LR / d_b_LR) * CONVERT_TO_DEGREE);

                        }
                    }
                    else skeletons[9].joints[j].F = 0;
                }
                //using (StreamWriter w = File.AppendText("c://AAA.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
                //        dB[0],
                //        dB[1],
                //        dB[2],
                //        dB[3],
                //        dB[4],
                //        dB[5],
                //        dB[6],
                //        dB[7],
                //        dB[8],
                //        dB[9],
                //        dB[10],
                //        dB[11],
                //        dB[12],
                //        dB[13],
                //        dB[14]
                //        );
                //}
                //using (StreamWriter w = File.AppendText("c://BBB.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
                //        dB[0],
                //        dB[1],
                //        dB[2],
                //        dB[3],
                //        dB[4],
                //        dB[5],
                //        dB[6],
                //        dB[7],
                //        dB[8],
                //        dB[9],
                //        dB[10],
                //        dB[11],
                //        dB[12],
                //        dB[13],
                //        dB[14]
                //        );
                //}
                //using (StreamWriter w = File.AppendText("c://CCC.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
                //        dC[0],
                //        dC[1],
                //        dC[2],
                //        dC[3],
                //        dC[4],
                //        dC[5],
                //        dC[6],
                //        dC[7],
                //        dC[8],
                //        dC[9],
                //        dC[10],
                //        dC[11],
                //        dC[12],
                //        dC[13],
                //        dC[14]
                //        );
                //}


                lbl_AVGHip_x.Text = "X : " + xHip.ToString();
                lbl_AVGHip_y.Text = "Y : " + yHip.ToString();
                lbl_AVGHip_z.Text = "Z : " + zHip.ToString();

                lbl_CG_x.Text = "X : " + xCG.ToString();
                lbl_CG_y.Text = "Y : " + yCG.ToString();
                lbl_CG_z.Text = "Z : " + zCG.ToString();

                lbl_angle_FB_var.Text = "Front/Back Angle (degree) : " + Math.Round(balance_angle_FB,2).ToString();
                lbl_angle_LR_var.Text = "Left/Right Angle (degree) : " + Math.Round(balance_angle_LR,2).ToString();
                changeImg(xHip, yHip, zHip, xCG, yCG, zCG, balance_angle_FB, balance_angle_LR);

                views[6].Draw();


                

                /////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAaaaa
                //using (StreamWriter w = File.AppendText("c://ACordinate.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} {23} {24} {25} {26} {27} {28} {29} {30} {31} {32} {33} {34} {35} {36} {37} {38} {39} {40} {41} {42} {43} {44}",
                //        skeletons[3].joints[0].X,
                //        skeletons[3].joints[0].Y,
                //        skeletons[3].joints[0].Z,
                //        skeletons[3].joints[1].X,
                //        skeletons[3].joints[1].Y,
                //        skeletons[3].joints[1].Z,
                //        skeletons[3].joints[2].X,
                //        skeletons[3].joints[2].Y,
                //        skeletons[3].joints[2].Z,
                //        skeletons[3].joints[3].X,
                //        skeletons[3].joints[3].Y,
                //        skeletons[3].joints[3].Z,
                //        skeletons[3].joints[4].X,
                //        skeletons[3].joints[4].Y,
                //        skeletons[3].joints[4].Z,
                //        skeletons[3].joints[5].X,
                //        skeletons[3].joints[5].Y,
                //        skeletons[3].joints[5].Z,
                //        skeletons[3].joints[6].X,
                //        skeletons[3].joints[6].Y,
                //        skeletons[3].joints[6].Z,
                //        skeletons[3].joints[7].X,
                //        skeletons[3].joints[7].Y,
                //        skeletons[3].joints[7].Z,
                //        skeletons[3].joints[8].X,
                //        skeletons[3].joints[8].Y,
                //        skeletons[3].joints[8].Z,
                //        skeletons[3].joints[9].X,
                //        skeletons[3].joints[9].Y,
                //        skeletons[3].joints[9].Z,
                //        skeletons[3].joints[10].X,
                //        skeletons[3].joints[10].Y,
                //        skeletons[3].joints[10].Z,
                //        skeletons[3].joints[11].X,
                //        skeletons[3].joints[11].Y,
                //        skeletons[3].joints[11].Z,
                //        skeletons[3].joints[12].X,
                //        skeletons[3].joints[12].Y,
                //        skeletons[3].joints[12].Z,
                //        skeletons[3].joints[13].X,
                //        skeletons[3].joints[13].Y,
                //        skeletons[3].joints[13].Z,
                //        skeletons[3].joints[14].X,
                //        skeletons[3].joints[14].Y,
                //        skeletons[3].joints[14].Z
                //        );
                //}
                //using (StreamWriter w = File.AppendText("c://BCordinate.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} {23} {24} {25} {26} {27} {28} {29} {30} {31} {32} {33} {34} {35} {36} {37} {38} {39} {40} {41} {42} {43} {44}",
                //        skeletons[4].joints[0].X,
                //        skeletons[4].joints[0].Y,
                //        skeletons[4].joints[0].Z,
                //        skeletons[4].joints[1].X,
                //        skeletons[4].joints[1].Y,
                //        skeletons[4].joints[1].Z,
                //        skeletons[4].joints[2].X,
                //        skeletons[4].joints[2].Y,
                //        skeletons[4].joints[2].Z,
                //        skeletons[4].joints[3].X,
                //        skeletons[4].joints[3].Y,
                //        skeletons[4].joints[3].Z,
                //        skeletons[4].joints[4].X,
                //        skeletons[4].joints[4].Y,
                //        skeletons[4].joints[4].Z,
                //        skeletons[4].joints[5].X,
                //        skeletons[4].joints[5].Y,
                //        skeletons[4].joints[5].Z,
                //        skeletons[4].joints[6].X,
                //        skeletons[4].joints[6].Y,
                //        skeletons[4].joints[6].Z,
                //        skeletons[4].joints[7].X,
                //        skeletons[4].joints[7].Y,
                //        skeletons[4].joints[7].Z,
                //        skeletons[4].joints[8].X,
                //        skeletons[4].joints[8].Y,
                //        skeletons[4].joints[8].Z,
                //        skeletons[4].joints[9].X,
                //        skeletons[4].joints[9].Y,
                //        skeletons[4].joints[9].Z,
                //        skeletons[4].joints[10].X,
                //        skeletons[4].joints[10].Y,
                //        skeletons[4].joints[10].Z,
                //        skeletons[4].joints[11].X,
                //        skeletons[4].joints[11].Y,
                //        skeletons[4].joints[11].Z,
                //        skeletons[4].joints[12].X,
                //        skeletons[4].joints[12].Y,
                //        skeletons[4].joints[12].Z,
                //        skeletons[4].joints[13].X,
                //        skeletons[4].joints[13].Y,
                //        skeletons[4].joints[13].Z,
                //        skeletons[4].joints[14].X,
                //        skeletons[4].joints[14].Y,
                //        skeletons[4].joints[14].Z
                //        );
                //}
                //using (StreamWriter w = File.AppendText("c://CCordinate.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} {23} {24} {25} {26} {27} {28} {29} {30} {31} {32} {33} {34} {35} {36} {37} {38} {39} {40} {41} {42} {43} {44}",
                //        skeletons[5].joints[0].X,
                //        skeletons[5].joints[0].Y,
                //        skeletons[5].joints[0].Z,
                //        skeletons[5].joints[1].X,
                //        skeletons[5].joints[1].Y,
                //        skeletons[5].joints[1].Z,
                //        skeletons[5].joints[2].X,
                //        skeletons[5].joints[2].Y,
                //        skeletons[5].joints[2].Z,
                //        skeletons[5].joints[3].X,
                //        skeletons[5].joints[3].Y,
                //        skeletons[5].joints[3].Z,
                //        skeletons[5].joints[4].X,
                //        skeletons[5].joints[4].Y,
                //        skeletons[5].joints[4].Z,
                //        skeletons[5].joints[5].X,
                //        skeletons[5].joints[5].Y,
                //        skeletons[5].joints[5].Z,
                //        skeletons[5].joints[6].X,
                //        skeletons[5].joints[6].Y,
                //        skeletons[5].joints[6].Z,
                //        skeletons[5].joints[7].X,
                //        skeletons[5].joints[7].Y,
                //        skeletons[5].joints[7].Z,
                //        skeletons[5].joints[8].X,
                //        skeletons[5].joints[8].Y,
                //        skeletons[5].joints[8].Z,
                //        skeletons[5].joints[9].X,
                //        skeletons[5].joints[9].Y,
                //        skeletons[5].joints[9].Z,
                //        skeletons[5].joints[10].X,
                //        skeletons[5].joints[10].Y,
                //        skeletons[5].joints[10].Z,
                //        skeletons[5].joints[11].X,
                //        skeletons[5].joints[11].Y,
                //        skeletons[5].joints[11].Z,
                //        skeletons[5].joints[12].X,
                //        skeletons[5].joints[12].Y,
                //        skeletons[5].joints[12].Z,
                //        skeletons[5].joints[13].X,
                //        skeletons[5].joints[13].Y,
                //        skeletons[5].joints[13].Z,
                //        skeletons[5].joints[14].X,
                //        skeletons[5].joints[14].Y,
                //        skeletons[5].joints[14].Z
                //        );
                //}
                //using (StreamWriter w = File.AppendText("c://AVGCordinate.txt"))
                //{
                //    w.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} {23} {24} {25} {26} {27} {28} {29} {30} {31} {32} {33} {34} {35} {36} {37} {38} {39} {40} {41} {42} {43} {44}",
                //        skeletons[9].joints[0].X,
                //        skeletons[9].joints[0].Y,
                //        skeletons[9].joints[0].Z,
                //        skeletons[9].joints[1].X,
                //        skeletons[9].joints[1].Y,
                //        skeletons[9].joints[1].Z,
                //        skeletons[9].joints[2].X,
                //        skeletons[9].joints[2].Y,
                //        skeletons[9].joints[2].Z,
                //        skeletons[9].joints[3].X,
                //        skeletons[9].joints[3].Y,
                //        skeletons[9].joints[3].Z,
                //        skeletons[9].joints[4].X,
                //        skeletons[9].joints[4].Y,
                //        skeletons[9].joints[4].Z,
                //        skeletons[9].joints[5].X,
                //        skeletons[9].joints[5].Y,
                //        skeletons[9].joints[5].Z,
                //        skeletons[9].joints[6].X,
                //        skeletons[9].joints[6].Y,
                //        skeletons[9].joints[6].Z,
                //        skeletons[9].joints[7].X,
                //        skeletons[9].joints[7].Y,
                //        skeletons[9].joints[7].Z,
                //        skeletons[9].joints[8].X,
                //        skeletons[9].joints[8].Y,
                //        skeletons[9].joints[8].Z,
                //        skeletons[9].joints[9].X,
                //        skeletons[9].joints[9].Y,
                //        skeletons[9].joints[9].Z,
                //        skeletons[9].joints[10].X,
                //        skeletons[9].joints[10].Y,
                //        skeletons[9].joints[10].Z,
                //        skeletons[9].joints[11].X,
                //        skeletons[9].joints[11].Y,
                //        skeletons[9].joints[11].Z,
                //        skeletons[9].joints[12].X,
                //        skeletons[9].joints[12].Y,
                //        skeletons[9].joints[12].Z,
                //        skeletons[9].joints[13].X,
                //        skeletons[9].joints[13].Y,
                //        skeletons[9].joints[13].Z,
                //        skeletons[9].joints[14].X,
                //        skeletons[9].joints[14].Y,
                //        skeletons[9].joints[14].Z
                //        );
                //}
            }
        }
        private void changeImg(double xAVG_HIP, double yAVG_HIP, double zAVG_HIP, double xCG, double yCG, double zCG, double angle_FB, double angle_LR)
        {
            System.Drawing.Image image = null;
            string path = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\images\\";
            string pic_code = "0000";

            //left-right
            if (xAVG_HIP < xCG)
            {
                if (angle_LR < range_1)//left
                    pic_code = "11";
                else if (angle_LR < range_2)
                    pic_code = "12";
                else if (angle_LR < range_3)
                    pic_code = "13";
                else
                    pic_code = "14";
            }
            else if (xAVG_HIP > xCG)
            {
                if (angle_LR < range_1)//right
                    pic_code = "21";
                else if (angle_LR < range_2)
                    pic_code = "22";
                else if (angle_LR < range_3)
                    pic_code = "23";
                else
                    pic_code = "24";
            }
            else
                pic_code = "11";
            
            //front_back
            if (zAVG_HIP < zCG)
            {
                if (angle_FB < range_1)//up
                    pic_code += "11";
                else if (angle_FB < range_2)
                    pic_code += "12";
                else if (angle_FB < range_3)
                    pic_code += "13";
                else
                    pic_code += "14";
            }
            else if (zAVG_HIP > zCG)
            {
                if (angle_FB < range_1)//down
                    pic_code += "21";
                else if (angle_FB < range_2)
                    pic_code += "22";
                else if (angle_FB < range_3)
                    pic_code += "23";
                else
                    pic_code += "24";
            }
            else
                pic_code += "11";
            if ((double.IsNaN(angle_FB)) || (double.IsNaN(angle_LR))) pic_code = "0000";
            image = System.Drawing.Image.FromFile(path + pic_code + ".jpg");
            pic1.Image = image;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isStart = false;
            System.IO.StreamWriter mcf = new System.IO.StreamWriter("config.txt");
            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 6; j++)
                {
                    mcf.Write(" " + oldvalue[i, j]);
                }
                mcf.WriteLine();
            }
            mcf.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string line;
                System.IO.StreamReader mcf = new System.IO.StreamReader("./config.txt");
                byte i = 0;
                while ((line = mcf.ReadLine()) != null)
                {
                    if (line.Trim().Length > 0)
                    {
                        string[] t = line.Trim().Split(' ');
                        oldvalue[i, 0] = t[0];
                        oldvalue[i, 1] = t[1];
                        oldvalue[i, 2] = t[2];
                        oldvalue[i, 3] = t[3];
                        oldvalue[i, 4] = t[4];
                        oldvalue[i, 5] = t[5];
                        i++;
                    }
                }
                mcf.Close();

                for (i = 0; i < 3; i++)
                {
                    dataGridView1.Rows[i].Cells[1].Value = oldvalue[i, 0];
                    dataGridView1.Rows[i].Cells[2].Value = oldvalue[i, 1];
                    dataGridView1.Rows[i].Cells[3].Value = oldvalue[i, 2];
                    dataGridView1.Rows[i].Cells[4].Value = oldvalue[i, 3];
                    dataGridView1.Rows[i].Cells[5].Value = oldvalue[i, 4];
                    dataGridView1.Rows[i].Cells[6].Value = oldvalue[i, 5];
                }
            }
            catch (Exception er) { Console.WriteLine(er); }
        }
        public double getDistance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2) + Math.Pow(z1 - z2, 2));
        }
        public Point3D getNewPoint(Point3D AP, Point3D BP, Double curTx, Double curTy, Double curTz, Double cosX, Double sinX, Double cosY, Double sinY, Double cosZ, Double sinZ, ref Double distance)
        {
            Point3D NP = new Point3D(0, 0, 0);
            NP.X = (float)(((cosZ * cosY) * BP.X) + (((-1 * sinZ * cosX) + (cosZ * sinY * sinX)) * BP.Y) + (((sinZ * sinX) + cosZ * sinY * cosX) * BP.Z) + curTx);
            NP.Y = (float)(((sinZ * cosY) * BP.X) + (((cosZ * cosX) + (sinZ * sinY * sinX)) * BP.Y) + (((-1 * cosZ * sinX) + sinZ * sinY * cosX) * BP.Z) + curTy);
            NP.Z = (float)((-1 * sinY * BP.X) + ((cosY * sinX) * BP.Y) + ((cosY * cosX) * BP.Z) + curTz);
            distance = getDistance(AP.X, AP.Y, AP.Z, NP.X, NP.Y, NP.Z);
            return NP;
        }
        public void preSettingKinectCamera(Point3D[] mainPoint, Point3D[] subPoint,String type_camera)
        {
            int Ay_var = 0;
            Double Tx_var, Ty_var, Tz_var,distance_var;
            Tx_var = Ty_var = Tz_var = distance_var = 0.0;
            DateTime time_before = DateTime.Now;
            settingKinectCamera(-360, 360, -4, 4, -4, 4, -4, 4, 1, 0.5, mainPoint, subPoint, ref Ay_var, ref Tx_var, ref Ty_var, ref Tz_var,ref distance_var);
            settingKinectCamera(Ay_var - 5, Ay_var + 5, Tx_var - 0.5, Tx_var + 0.5, Ty_var - 0.5, Ty_var + 0.5, Tz_var - 0.5, Tz_var + 0.5, 1, 0.1, mainPoint, subPoint, ref Ay_var, ref Tx_var, ref Ty_var, ref Tz_var,ref distance_var);
            settingKinectCamera(Ay_var - 5, Ay_var + 5, Tx_var - 0.1, Tx_var + 0.1, Ty_var - 0.1, Ty_var + 0.1, Tz_var - 0.1, Tz_var + 0.1, 1, 0.01, mainPoint, subPoint, ref Ay_var, ref Tx_var, ref Ty_var, ref Tz_var,ref distance_var);
            DateTime time_after = DateTime.Now;
            if (type_camera == "B")
            {
                dataGridView1.Rows[1].Cells[1].Value = Tx_var;
                dataGridView1.Rows[1].Cells[2].Value = Ty_var;
                dataGridView1.Rows[1].Cells[3].Value = Tz_var;
                dataGridView1.Rows[1].Cells[5].Value = Ay_var;
                dataGridView1.Rows[1].Cells[22].Value = distance_var;
                dataGridView1.Rows[1].Cells[23].Value =  (time_after - time_before).TotalSeconds;
                btnCalB.Enabled = true;
            }
            else if (type_camera == "C")
            {
                dataGridView1.Rows[2].Cells[1].Value = Tx_var;
                dataGridView1.Rows[2].Cells[2].Value = Ty_var;
                dataGridView1.Rows[2].Cells[3].Value = Tz_var;
                dataGridView1.Rows[2].Cells[5].Value = Ay_var;
                dataGridView1.Rows[2].Cells[22].Value = distance_var;
                dataGridView1.Rows[2].Cells[23].Value = (time_after - time_before).TotalSeconds;
                btnCalC.Enabled = true;
            }
        }
        public void settingKinectCamera(int minAy, int maxAy, Double minDx, Double maxDx, Double minDy, Double maxDy, Double minDz, Double maxDz, int incrementA, Double incrementD, Point3D[] mainPoint, Point3D[] subPoint, ref int Ay_var, ref Double Tx_var, ref Double Ty_var, ref Double Tz_var, ref Double distance_var)
        {

            const Double CONVERT_DEGREE = 0.0174532925;

            Point3D NP = new Point3D(0, 0, 0);

            int curAy = minAy;
            Double curTx = minDx;
            Double curTy = minDy;
            Double curTz = minDz;
            Double temDistance = 99999;

            while (true)
            {
                Double cosX = (Double)Math.Cos(CONVERT_DEGREE * 0);
                Double sinX = (Double)Math.Sin(CONVERT_DEGREE * 0);
                Double cosY = (Double)Math.Cos(CONVERT_DEGREE * curAy);
                Double sinY = (Double)Math.Sin(CONVERT_DEGREE * curAy);
                Double cosZ = (Double)Math.Cos(CONVERT_DEGREE * 0);
                Double sinZ = (Double)Math.Sin(CONVERT_DEGREE * 0);

                curTx = minDx;
                while (true)
                {
                    curTy = minDy;
                    while (true)
                    {
                        curTz = minDz;
                        while (true)
                        {
                            curTx = Math.Round(curTx, 2);
                            curTy = Math.Round(curTy, 2);
                            curTz = Math.Round(curTz, 2);
                            Double distance = 0.0;

                            Double sumDistance = 0.0;
                            for (int i = 0; i < 15; i++)
                            {
                                NP = getNewPoint(mainPoint[i], subPoint[i], curTx, curTy, curTz, cosX, sinX, cosY, sinY, cosZ, sinZ, ref distance);
                                sumDistance += distance;
                            }
                            if (sumDistance < temDistance)
                            {
                                Tx_var = curTx;
                                Ty_var = curTy;
                                Tz_var = curTz;
                                Ay_var = curAy;
                                temDistance = sumDistance;
                                distance_var = sumDistance;
                            }
                            if (curTz >= maxDz) break;
                            curTz += incrementD;
                        }
                        if (curTy >= maxDy) break;
                        curTy += incrementD;
                    }
                    if (curTx >= maxDx) break;
                    curTx += incrementD;
                }
                if (curAy >= maxAy) break;
                curAy += incrementA;
            }
        }
    }
    public partial class ProgressForm : Window
    {
        bool _contentLoaded;
        public ProgressForm()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/O3KNS;component/progressform.xaml", System.UriKind.Relative);

            #line 1 "..\..\..\ProgressForm.xaml"
                        System.Windows.Application.LoadComponent(this, resourceLocater);

            #line default
            #line hidden

        }



    }
    class CamView
    {
        public byte updf;
        private Viewport3D view;
        public Skeleton[] skeletons;
        private byte num_skeletons;
        ScreenSpaceLines3D[][] jointPoint, jointLine;
        byte num_joints = 15;
        public CamView(Viewport3D view, byte num_skeletons)
        {
            this.view = view;
            this.num_skeletons = num_skeletons;
            skeletons = new Skeleton[num_skeletons];
            updf = 10;
        }
        public void Active()
        {
            updf = 0;
        }
        public void Deactive()
        {
            updf++;
            if (updf > 10)
            {
                Clear();
                updf--;
                for (byte i = 0; i < num_skeletons; i++) skeletons[i].hasSkeleton = false;
            }
        }
        public void Update()
        {
            jointPoint = new ScreenSpaceLines3D[num_skeletons][];
            jointLine = new ScreenSpaceLines3D[num_skeletons][];
            for (byte i = 0; i < num_skeletons; i++)
            {
                jointPoint[i] = new ScreenSpaceLines3D[num_joints];
                jointLine[i] = new ScreenSpaceLines3D[num_joints];
                for (byte j = 0; j < num_joints; j++)
                {
                    jointPoint[i][j] = new ScreenSpaceLines3D();
                    jointPoint[i][j].Color = skeletons[i].colors[j];
                    jointPoint[i][j].Thickness = skeletons[i].pointThickness;
                    view.Children.Add(jointPoint[i][j]);

                    jointLine[i][j] = new ScreenSpaceLines3D();
                    jointLine[i][j].Color = skeletons[i].line;
                    jointLine[i][j].Thickness = skeletons[i].lineThickness;
                    view.Children.Add(jointLine[i][j]);
                }
            }
        }
        public void Clear()
        {
            for (byte i = 0; i < num_skeletons; i++)
            {
                for (byte j = 0; j < num_joints; j++)
                {
                    jointPoint[i][j].Points.Clear();
                    jointLine[i][j].Points.Clear();
                }
            }
        }
        public void Draw()
        {
            for (byte i = 0; i < num_skeletons; i++)
            {
                if (skeletons[i].hasSkeleton && skeletons[i].hide == false) DrawSkeleton(i);
            }
        }
        public void DrawSkeleton(byte ske)
        {
            for (byte j = 0; j < num_joints; j++)
            {
                if (skeletons[ske].joints[j].F != 1) continue;
                skeletons[ske].joints[j].point.X = skeletons[ske].joints[j].X;
                skeletons[ske].joints[j].point.Y = skeletons[ske].joints[j].Y;
                skeletons[ske].joints[j].point.Z = skeletons[ske].joints[j].Z;

                skeletons[ske].joints[j].point.X -= skeletons[ske].pointSize;
                jointPoint[ske][j].Points.Add(skeletons[ske].joints[j].point);
                skeletons[ske].joints[j].point.X += skeletons[ske].pointSize;
                skeletons[ske].joints[j].point.X += skeletons[ske].pointSize;
                jointPoint[ske][j].Points.Add(skeletons[ske].joints[j].point);
                skeletons[ske].joints[j].point.X -= skeletons[ske].pointSize;
            }

            drawLine(ske, 0, 0, 1);
            drawLine(ske, 1, 1, 2);

            drawLine(ske, 2, 1, 3);
            drawLine(ske, 3, 1, 6);
            drawLine(ske, 4, 2, 9);
            drawLine(ske, 5, 2, 12);

            drawLine(ske, 6, 3, 4);
            drawLine(ske, 7, 4, 5);

            drawLine(ske, 8, 6, 7);
            drawLine(ske, 9, 7, 8);

            drawLine(ske, 10, 9, 10);
            drawLine(ske, 11, 10, 11);

            drawLine(ske, 12, 12, 13);
            drawLine(ske, 13, 13, 14);
        }
        void drawLine(byte ske, byte line, byte j1, byte j2)
        {
            if (skeletons[ske].joints[j1].F == 1 && skeletons[ske].joints[j2].F == 1)
            {
                jointLine[ske][line].Points.Add(skeletons[ske].joints[j1].point);
                jointLine[ske][line].Points.Add(skeletons[ske].joints[j2].point);
            }
        }
        public void drawGrid(double size, double step)
        {
            // x-axis
            ScreenSpaceLines3D l = new ScreenSpaceLines3D();
            l.Points.Add(new Point3D(-size, 0, 0));
            l.Points.Add(new Point3D(size, 0, 0));
            l.Color = Colors.Blue;
            l.Thickness = 1;
            view.Children.Add(l);
            // z-axis
            l = new ScreenSpaceLines3D();
            l.Points.Add(new Point3D(0, 0, -size));
            l.Points.Add(new Point3D(0, 0, size));
            l.Color = Colors.Red;
            l.Thickness = 1;
            view.Children.Add(l);

            for (double i = step; i <= size; i += step)
            {
                l = new ScreenSpaceLines3D();
                l.Points.Add(new Point3D(-size, 0, i));
                l.Points.Add(new Point3D(size, 0, i));
                l.Color = Colors.Gray;
                l.Thickness = 1;
                view.Children.Add(l);
                l = new ScreenSpaceLines3D();
                l.Points.Add(new Point3D(-size, 0, -i));
                l.Points.Add(new Point3D(size, 0, -i));
                l.Color = Colors.Gray;
                l.Thickness = 1;
                view.Children.Add(l);

                l = new ScreenSpaceLines3D();
                l.Points.Add(new Point3D(i, 0, -size));
                l.Points.Add(new Point3D(i, 0, size));
                l.Color = Colors.Gray;
                l.Thickness = 1;
                view.Children.Add(l);
                l = new ScreenSpaceLines3D();
                l.Points.Add(new Point3D(-i, 0, -size));
                l.Points.Add(new Point3D(-i, 0, size));
                l.Color = Colors.Gray;
                l.Thickness = 1;
                view.Children.Add(l);
            }
        }
    }

    class Skeleton
    {
        public Joint[] joints;
        public System.Windows.Media.Color Head, Neck, Torso, ShoulderLeft, ElbowLeft, HandLeft, ShoulderRight, ElbowRight, HandRight, HipLeft, KneeLeft, FootLeft, HipRight, KneeRight, FootRight, line;
        public System.Windows.Media.Color[] colors;
        public double pointThickness, lineThickness, pointSize;
        public bool hasSkeleton, hide;

        public Skeleton()
        {
            joints = new Joint[15];
            for (byte j = 0; j < 15; j++) joints[j] = new Joint();
            colors = new System.Windows.Media.Color[] { Head, Neck, Torso, ShoulderLeft, ElbowLeft, HandLeft, ShoulderRight, ElbowRight, HandRight, HipLeft, KneeLeft, FootLeft, HipRight, KneeRight, FootRight };
        }
        public void update()
        {
            colors = new System.Windows.Media.Color[] { Head, Neck, Torso, ShoulderLeft, ElbowLeft, HandLeft, ShoulderRight, ElbowRight, HandRight, HipLeft, KneeLeft, FootLeft, HipRight, KneeRight, FootRight };
        }
    }
    class Joint
    {
        public double F, X, Y, Z;
        public Point3D point;
        public Joint()
        {
            point = new Point3D();
            point.X = X;
            point.Y = Y;
            point.Z = Z;
        }
    }
}
