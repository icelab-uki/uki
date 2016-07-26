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
using System.Windows.Media.Media3D;
using System.Threading;
using System.Diagnostics;

namespace P_Tracker2
{
    public partial class SkelViewer : Window
    {
        public SkelViewer()
        {
            InitializeComponent();
        }

        string fileFullPath = "";//Full
        string fileName = "";
        string fileFolder = "";

        private void butBrowse_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".*","");
            if (openDialog == true && TheTool.dialog.FileNames.Count() > 0)
            {
                this.fileFullPath = TheTool.dialog.FileNames[0];
                this.fileFolder = System.IO.Path.GetDirectoryName(fileFullPath);
                this.fileName = TheTool.getFileName_byPath(fileFullPath);
                Title = TheTool.getFileName_byPath(fileName);
                List<UKI_DataRaw> data = new List<UKI_DataRaw>();
                if (TheTool.getExtension_byPath(this.fileFullPath) == ".csv")
                {
                    data.AddRange(TheUKI.csv_loadFileTo_DataRaw(fileFullPath, 0));
                    if (checkFPSauto.IsChecked.Value) { txtFPS.Text = "20"; }
                }
                else if (TheTool.getExtension_byPath(this.fileFullPath) == ".bvh")
                {
                    data.AddRange(TheConverter.BVH_convert(fileFullPath));
                    TheUKI.saveData_Raw(fileFolder + @"/" + fileName + "(copy).csv", data);
                    if (checkFPSauto.IsChecked.Value) { txtFPS.Text = "120"; }
                }
                if (checkCamera.IsChecked.Value) { 
                    data = TheUKI.UKI_DataRaw_centerize(data,3); 
                }
                loadData(data);
                txtMark.Text = "";
                mark_count = 0;
            }
        }

        List<UKI_DataRaw> data_skel = new List<UKI_DataRaw>();
        UKI_DataRaw skel = new UKI_DataRaw();//selected data
        UKI_DataRaw skel_centered = new UKI_DataRaw();//selected data (center at 0,0,0), Used for draw 2 screens on the left
        UKI_DataRaw skel_visual = new UKI_DataRaw();//the one that is visualized on the main screen (rotate + tilt)

        public void loadData(List<UKI_DataRaw> skel)
        {
            if (skel.Count() > 0)
            {
                data_skel = skel;
                slider.Maximum = skel.Count();
                txtStart.Content = skel.First().id;
                txtEnd.Content = skel.Last().id;
                slider.Value = slider.Maximum;
                slider.Value = 0;//default
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                skel = data_skel[(int)slider.Value];
                skel_centered = TheUKI.UKI_DataRaw_centerize(skel,3);
                txtCurrent.Content = skel.id;
                update();
            }
            catch { }
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update();
        }

        private void sliderX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update();
        }

        private void sliderY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update();
        }

        private void sliderRotate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update();
        }

        private void sliderTilt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update();
        }


        private void butReset_Click(object sender, RoutedEventArgs e)
        {
            sliderZoom.Value = 200; sliderX.Value = 0; sliderY.Value = 0;
            sliderRotate.Value = 0; sliderTilt.Value = 0; 
        }

        void update() {
            if (data_skel.Count > 0)
            {
                int angle = (int)sliderRotate.Value;
                int tilt = (int)sliderTilt.Value;
                UKI_DataRaw skel_rotate = TheTool.rotateSkel(skel, angle, false);
                this.skel_visual = TheTool.rotateSkel(skel_rotate, tilt, true);
                zoomRange = -150;
                noSlide = true;
                drawSkel(image2, TheTool.rotateSkel(skel_centered, 90, false));
                drawSkel(image3, TheTool.rotateSkel(skel_centered, -90, false));
                noSlide = false;
                zoomRange = sliderZoom.Value;
                drawSkel(image1, this.skel_visual);
                txtRange.Content = "Range: " + (Math.Round(skel.Spine[2],2) * 100) + " cm";
            }
        }

        //=================================================================


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp += new System.Windows.Input.KeyEventHandler(hotKey);
        }

        void hotKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                quickMark();
                return;
            }
            else if (e.Key == Key.F2)
            {
                slider.Value = slider.Maximum;
                txtCurrent.Content = data_skel.Last().id;
                skel = data_skel.Last();
                update();
                //
                quickMark();
                export();
                return;
            }
            else if (e.Key == Key.Space)
            {
                playPause();
                return;
            }
        }

        void drawSkel(Image img, UKI_DataRaw skel)
        {
            DrawingImage imageSource;
            DrawingGroup drawingGroup;
            drawingGroup = new DrawingGroup();
            imageSource = new DrawingImage(drawingGroup);
            using (DrawingContext dc = drawingGroup.Open())
            {
                checkRange(skel);
                dc.DrawRectangle(Brushes.Black, null, new Rect(-ScreenWidthHalf, 0, ScreenWidth, ScreenHeight));
                // Render Bones
                drawBone(dc, skel.Head, skel.ShoulderCenter);
                drawBone(dc, skel.ShoulderCenter, skel.Spine);
                drawBone(dc, skel.ShoulderCenter, skel.ShoulderLeft);
                drawBone(dc, skel.ShoulderCenter, skel.ShoulderRight);
                drawBone(dc, skel.ShoulderLeft, skel.ElbowLeft);
                drawBone(dc, skel.ElbowLeft, skel.WristLeft);
                drawBone(dc, skel.WristLeft, skel.HandLeft);
                drawBone(dc, skel.ShoulderRight, skel.ElbowRight);
                drawBone(dc, skel.ElbowRight, skel.WristRight);
                drawBone(dc, skel.WristRight, skel.HandRight);
                drawBone(dc, skel.Spine, skel.HipCenter);
                drawBone(dc, skel.HipCenter, skel.HipLeft);
                drawBone(dc, skel.HipCenter, skel.HipRight);
                drawBone(dc, skel.HipLeft, skel.KneeLeft);
                drawBone(dc, skel.KneeLeft, skel.AnkleLeft);
                drawBone(dc, skel.AnkleLeft, skel.FootLeft);
                drawBone(dc, skel.HipRight, skel.KneeRight);
                drawBone(dc, skel.KneeRight, skel.AnkleRight);
                drawBone(dc, skel.AnkleRight, skel.FootRight);
                //-------------------
                // Render Joints
                drawJoint(dc, skel.Head);
                drawJoint(dc, skel.ShoulderCenter); 
                drawJoint(dc, skel.ShoulderLeft);
                drawJoint(dc, skel.ElbowLeft);
                drawJoint(dc, skel.WristLeft);
                drawJoint(dc, skel.HandLeft);
                drawJoint(dc, skel.ShoulderRight);
                drawJoint(dc, skel.ElbowRight);
                drawJoint(dc, skel.WristRight);
                drawJoint(dc, skel.HandRight);
                drawJoint(dc, skel.Spine);
                drawJoint(dc, skel.HipCenter);
                drawJoint(dc, skel.HipLeft);
                drawJoint(dc, skel.KneeLeft);
                drawJoint(dc, skel.AnkleLeft);
                drawJoint(dc, skel.FootLeft);
                drawJoint(dc, skel.HipRight);
                drawJoint(dc, skel.KneeRight);
                drawJoint(dc, skel.AnkleRight);
                drawJoint(dc, skel.FootRight);
            }
            img.Source = imageSource;
        }

        void checkRange(UKI_DataRaw skel)
        {
            range_jointNearest = skel.Head[2];
            range_jointNearest = Math.Min(range_jointNearest, skel.ShoulderCenter[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.ShoulderLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.ElbowLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.WristLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.HandLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.ShoulderRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.ElbowRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.WristRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.HandRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.Spine[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.HipCenter[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.HipLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.KneeLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.AnkleLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.FootLeft[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.HipRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.KneeRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.AnkleRight[2]);
            range_jointNearest = Math.Min(range_jointNearest, skel.FootRight[2]);
            range_jointFarthest = skel.Head[2];
            range_jointFarthest = Math.Max(range_jointNearest, skel.ShoulderCenter[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.ShoulderLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.ElbowLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.WristLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.HandLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.ShoulderRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.ElbowRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.WristRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.HandRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.Spine[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.HipCenter[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.HipLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.KneeLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.AnkleLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.FootLeft[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.HipRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.KneeRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.AnkleRight[2]);
            range_jointFarthest = Math.Max(range_jointNearest, skel.FootRight[2]);
            if (range_jointFarthest == range_jointNearest) { range_jointFarthest += 1; }
        }

        Boolean noSlide = false;
        double range_jointNearest = 0; double range_jointFarthest = 0; 
        double ScreenWidth = 640; double ScreenHeight = 480;
        double ScreenWidthHalf = 320; double ScreenHeightHalf = 240;
        double horizonMin = -320; double horizonMax = 320;
        double verticalMin = 0; double verticalMax = 480;
        double maxRange = 5;

        double zoomRange = 0;
        private Point map3D_to2D(double[] data_3D)
        {
            maxRange = data_3D[2] + (zoomRange / 100);
            if (maxRange < 1) { maxRange = 1; }
            double ScreenX = data_3D[0] * ScreenWidth / maxRange;
            double ScreenY = data_3D[1] * ScreenHeight / maxRange - ScreenHeightHalf;
            if (!noSlide) 
            {
                ScreenX += (ScreenWidthHalf * sliderX.Value / 100);
                ScreenY += (ScreenHeightHalf * sliderY.Value / 100);
            }
            return new System.Windows.Point(ScreenX, -ScreenY);
        }

        void drawJoint(DrawingContext dc, double[] joint_3d)
        {
            double blend_ratio = TheTool.getRatio(joint_3d[2], range_jointFarthest, range_jointNearest);
            int blend_color = (int)(155 + (100 * blend_ratio));
            //
            Point p = map3D_to2D(joint_3d);
            if (p.X > horizonMin && p.X < horizonMax && p.Y > verticalMin && p.Y < verticalMax)
            {
                Brush brush = new SolidColorBrush(Color.FromArgb(255, 68, (byte)blend_color, 68));
                double range = joint_3d[2] / .1 + sliderZoom.Value / 10;//unit of 10 cm
                if (range < 0) { range = 0; }
                double JointThickness = 1 + 5 * Math.Pow(.98, range);//Size by Range
                dc.DrawEllipse(brush, null, p, JointThickness, JointThickness);
            }
        }

        void drawBone(DrawingContext dc, double[] joint_3d1, double[] joint_3d2)
        {
            double blend_ratio = TheTool.getRatio(Math.Min(joint_3d1[2], joint_3d2[2]), range_jointFarthest, range_jointNearest);
            int blend_color = (int)(55 + (200 * blend_ratio));
            Brush brush = new SolidColorBrush(Color.FromArgb(255, 68, 68, (byte)blend_color));
            //
            Point p1 = map3D_to2D(joint_3d1);
            Point p2 = map3D_to2D(joint_3d2);
            if (Math.Min(p1.X, p2.X) > horizonMin && Math.Max(p1.X, p2.X) < horizonMax
                && Math.Min(p1.Y, p2.Y) > verticalMin && Math.Max(p1.Y, p2.Y) < verticalMax)
            {
                double range = Math.Min(joint_3d1[2], joint_3d2[2]) / .1 + sliderZoom.Value / 10;//unit of 10 cm
                if (range < 0) { range = 0; }
                double JointThickness = 1 + 5 * Math.Pow(.98, range);//Size by Range
                Pen drawPen = new Pen(brush, JointThickness);
                dc.DrawLine(drawPen, p1, p2);
            }
        }

        //==== PLAY =========================================================

        public Thread thread = null;
        Boolean isRunning = false;
        public void butPlay_Click(object sender, EventArgs e)
        {
            if (slider.Value == slider.Maximum) { slider.Value = slider.Minimum; }
            playPause();
        }

        void playPause()
        {
            int fps = TheTool.getInt(txtFPS);
            if (fps < 1) { fps = 1; }
            this.frame_time = TimeSpan.FromMilliseconds((double) 1000 / fps);
            if (isRunning) { thread.Abort(); isRunning = false; butPlay.Content = "Play"; }
            else { thread = new Thread(running); thread.Start(); isRunning = true; butPlay.Content = "Pause"; }
        }

        TimeSpan frame_time = TimeSpan.FromMilliseconds(40);
        void running()
        {
            while (true)
            {
                Thread.Sleep(frame_time);
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (slider.Value < slider.Maximum) { slider.Value++; }
                    else { thread.Abort(); isRunning = false; butPlay.Content = "Play"; }
                }));
            }
        }

        //==============================================
        int mark_count = 0;
        string mark_lastItem = "";//e.g. 30
        string mark_lastKey = "";//e.g. 30-60

        private void butMark_Click(object sender, RoutedEventArgs e)
        {
            mark();
        }

        void mark()
        {
            if (txtMark.Text == "")
            {
                txtMark.Text += txtCurrent.Content;
                mark_lastKey += txtCurrent.Content;
                mark_count = 0; mark_lastItem = "";
            }
            else
            {
                if (mark_count % 4 == 0)
                {
                    txtMark.Text += "_" + txtCurrent.Content;
                    mark_lastKey = "";
                }
                else if (mark_count % 2 == 1)
                {
                    if (txtCurrent.Content.ToString() == mark_lastItem) { }
                    else { 
                        txtMark.Text += "-" + txtCurrent.Content;
                        mark_lastKey += "-" + txtCurrent.Content;
                    }
                }
                else
                {
                    txtMark.Text += "," + txtCurrent.Content;
                    mark_lastKey = txtCurrent.Content + "";
                }
            }
            mark_lastItem = txtCurrent.Content.ToString();
            mark_count++;
        }

        private void butCopy_Click(object sender, RoutedEventArgs e)
        {
            copyEndtoStart();
        }

        //End of last pose to start of new pose
        void copyEndtoStart()
        {
            txtMark.Text += "_" + mark_lastKey;
            mark_count += 2;
            mark_lastItem = "";
        }

        void quickMark()
        {
            if (mark_count % 4 == 0 && mark_count > 0) { copyEndtoStart(); }
            mark();
        }

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(this.fileFolder); }
            catch { }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            export();
        }

        void export()
        {
            string[] s = TheTool.splitText(txtMark.Text, "_");
            TheTool.exportFile(s.ToList(), fileFolder + @"\" + fileName + ".gt", true, false);
        }

        //void autoCamera()
        //{
        //    try
        //    {
        //        int z = -(int)(skel_visual.Spine[2] * 100 - 300);
        //        if (z >= sliderZoom.Minimum && z <= sliderZoom.Maximum) { sliderZoom.Value = z; }
        //        //TheSys.showError(z);
        //        update();
        //        Point p = map3D_to2D(this.skel_visual.Spine);
        //        TheSys.showError("x + " + p.X + " y" + p.Y);
        //        //int x = -(int)(skel_visualized.Spine[0] * 100);
        //        //int y = -(int)(skel_visualized.Spine[1] * 100);
        //        //int x = (int) (-p.X);
        //        //int y = (int) (ScreenHeightHalf - p.Y);
        //        //ScreenWidthHalf = 320; double ScreenHeightHalf = 240;
        //        //if (x >= sliderX.Minimum && x <= sliderX.Maximum) { sliderX.Value = x; }
        //        //if (y >= sliderY.Minimum && y <= sliderY.Maximum) { sliderY.Value = y; }
        //    }
        //    catch (Exception ex) { TheSys.showError(ex); }
        //}

        private void butViewDefault_Click(object sender, RoutedEventArgs e)
        {
            sliderX.Value = 0; sliderY.Value = 0; sliderZoom.Value = 0;
        }

    }
}
