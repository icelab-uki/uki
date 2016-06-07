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
using System.Globalization;
using System.Diagnostics;

namespace P_Tracker2
{
    public partial class GraphVisualizer : Window
    {
        //UserTracker main = null;

        //public GraphVisualizer(UserTracker main)
        //{
        //    InitializeComponent();
        //    this.main = main;
        //    initializeDrawComponent();
        //}

        public GraphVisualizer()
        {
            InitializeComponent();
            initializeDrawComponent();
        }



        private Point origin;
        private Point start;
        TransformGroup transformGroup;
        ScaleTransform transform;

        double image_Scale = 1;

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
        }

        double viewX = 0;
        double viewY = 0;
        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(border);
            viewX = origin.X - v.X;
            viewY = origin.Y - v.Y;

            //---- Max-Min ---
            if (image_Scale > 1)
            {
                if (viewX < moveBorder_xRight) { viewX = moveBorder_xRight; }
                if (viewX > moveBorder_xLeft) { viewX = moveBorder_xLeft; }
                if (viewY < moveBorder_yButtom) { viewY = moveBorder_yButtom; }
                if (viewY > moveBorder_yTop) { viewY = moveBorder_yTop; }
                //-----
                //if (grid_xSize > image_Size) { viewX = 0; }
            }
            else { viewX = 0; viewY = 0; }
            tt.X = viewX;
            tt.Y = viewY;
            //------
            //TheSys.showError("Grid " + grid_xSize + ":" + grid_ySize + "_" + image_Size, true, true);
            //TheSys.showError("Move " + tt.X + ":" + tt.Y, true, true);
        }


        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);

            //TheSys.showError(
            //    "view" + viewX +":"+ viewY + Environment.NewLine +
            //    "grid" + gridSize_x +":"+ gridSize_y + Environment.NewLine +
            //    "scale" + image_Scale + Environment.NewLine
            //    ,true,true);
        }


        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            transformGroup = (TransformGroup)image.RenderTransform;
            transform = (ScaleTransform)transformGroup.Children[0];

            double zoomChange = e.Delta > 0 ? .1 : -.1;
            double newImage_Scale = transform.ScaleX + zoomChange;
            if (newImage_Scale >= 1)
            {
                //if (newImage_Scale > 5) { newImage_Scale = 5; }
                image_Scale = newImage_Scale;
                transform.ScaleX = image_Scale;
                transform.ScaleY = image_Scale;
            }
            //TheSys.showError("Scale " + transform.ScaleX + ":" + transform.ScaleY, true, true);
            calBorderMoveThreshold();
            //------------------
            //---- Auto Adjust ------
            if (image_Scale > 1)
            {
                if (viewX < moveBorder_xRight) { viewX = moveBorder_xRight; }
                if (viewX > moveBorder_xLeft) { viewX = moveBorder_xLeft; }
                if (viewY < moveBorder_yButtom) { viewY = moveBorder_yButtom; }
                if (viewY > moveBorder_yTop) { viewY = moveBorder_yTop; }
            }
            else { viewX = 0; viewY = 0; }
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);
            tt.X = viewX;
            tt.Y = viewY;
        }

        void zoom(double zoomChange)
        {
            transformGroup = (TransformGroup)image.RenderTransform;
            transform = (ScaleTransform)transformGroup.Children[0];
            double newImage_Scale = transform.ScaleX + zoomChange;
            if (newImage_Scale >= 1)
            {
                //if (newImage_Scale > 5) { newImage_Scale = 5; }
                image_Scale = newImage_Scale;
                transform.ScaleX = image_Scale;
                transform.ScaleY = image_Scale;
            }
            calBorderMoveThreshold();
        }

        private void butZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoom(.1);
        }

        private void butZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoom(-.1);
        }

        int moveBorder_xLeft = 0;
        int moveBorder_xRight = 0;
        int moveBorder_yTop = 0;
        int moveBorder_yButtom = 0;
        int gridSize_x = 0;//GUI "grid"
        int gridSize_y = 0;
        int gridSize_diff = 0;
        //int imageActualSize = 0;//GUI "image"
        int scale_x = 0;
        int scale_y = 0;

        //int imageOriginalSize_x = 1000; int imageOriginalSize_y = 450;

        void calBorderMoveThreshold()
        {
            scale_x = (int)(gridSize_x * image_Scale);
            scale_y = (int)(gridSize_y * image_Scale);
            moveBorder_xLeft = 20;
            moveBorder_yTop = 20;
            moveBorder_yButtom = gridSize_y - scale_y;
            moveBorder_yButtom -= 20;
            moveBorder_xRight = gridSize_x - scale_x;
            moveBorder_xRight -= 20;
        }

        private void image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            calGridSize();
            calBorderMoveThreshold();
        }

        void calGridSize()
        {
            gridSize_x = (int)grid.ActualWidth;
            gridSize_y = (int)grid.ActualHeight;
            gridSize_diff = Math.Abs(gridSize_x - gridSize_y);
        }

        //----------------------------------------------------------------------
        //------------ Load Data -------------------------------------------------

        List<DetectorReportData> report_data = new List<DetectorReportData> { };
        List<DetectorReportData> report_data_35List = new List<DetectorReportData> { };//only shown data
        DetectorReportData[] report_data_35List_array = null;//only shown data
        int data_count = 35;

        List<string> report_dataString = new List<string> { };

        private void butBrowse_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> openDialog = TheTool.openFileDialog_01(false, ".csv", TheURL.url_saveFolder);
            // Get the selected file name and display in a TextBox
            if (openDialog == true)
            {
                TheTool_DetectorReportData.import(TheTool.dialog.FileName, ref report_data, ref report_dataString);
                txtFile.Content = "File: " + TheTool.getFileName_byPath(TheTool.dialog.FileName);
                //-------------
                adjustGUI();
                prepareData();
                draw();

            }
        }

        int show_startAt = 0;
        int show_Max = 35;

        private void slideTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            show_startAt = (int) slideTime.Value;
            prepareData();
            draw();
        }

        void adjustGUI()
        {
            double total = report_data.Count();
            double max = total-((show_Max-1)*(frame_skip))-(show_Max-1);
            if (max < 0) { max = 0; }
            slideTime.Maximum = (double) max;
        }

        int frame_skip = 0;
        int frame_m = 0;//Frame Size : min
        int frame_s = 10;//Frame Size : sec

        void prepareData()
        {
            try
            {
                report_data_35List.Clear();
                int i = 0; int collected = 0;
                int a = frame_skip;
                foreach (DetectorReportData data in report_data)
                {
                    if(collected >= show_Max){ break; }
                    if (i >= show_startAt)
                    {
                        if (a == 0)
                        {
                            report_data_35List.Add(data); 
                            collected++;
                            a = frame_skip;
                        }
                        else { a--; }
                    }
                    i++;
                }
                report_data_35List_array = report_data_35List.ToArray();
                data_count = report_data_35List_array.Count();
            }
            catch (Exception ex) { TheSys.showError("PrepareData: "+ex.Message); }
        }

        private void butTF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame_skip = 0;
                int a = TheTool.getInt(txtTF.Text);
                if (a < 10) { frame_skip = 0; txtTF.Text = "10"; }
                else { while (a >= 20) { frame_skip++; a -= 10; } }
                //--------
                int temp = frame_skip;
                frame_m = 0; frame_s = 0;
                while (temp >= 5) { temp -= 6; frame_m += 1; }
                while (temp >= 0) { temp -= 1; frame_s += 10; }
                //-------
                adjustGUI();
                prepareData();
                draw();
            }
            catch (Exception ex) { frame_skip = 0; TheSys.showError("TimeFrame Error: " + ex.Message); }
        }


        //==================================================================
        //==================================================================
        
        DrawingGroup drawingGroup;
        DrawingImage drawingImage;
        public Brush brush_team1 = Brushes.Red;
        public Brush brush_team2 = Brushes.Blue;

        void initializeDrawComponent()
        {
            drawingGroup = new DrawingGroup();
            drawingImage = new DrawingImage(drawingGroup);
            image.Source = drawingImage;

            //---- Zoom ---------------
            TransformGroup group = new TransformGroup();
            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);

            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            image.RenderTransform = group;
            image.MouseWheel += image_MouseWheel;
            image.MouseLeftButtonDown += image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;
        }


        public void draw()
        {
            try
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    if (data_count > 0)
                    {
                        dc.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(0.0, 0.0, 1000, 450));
                        drawTextLabel_FixSet(dc);
                        drawTextFrameRate(dc);
                        Boolean afterBreak = true;
                        //--------------------------------------
                        int size_x1 = 22;
                        int size_x1_half = size_x1 / 2;
                        int size_x1_e = size_x1 + 3;//wioth border/edge
                        int size_x1_e_half = size_x1_e / 2;
                        int size_y0 = 200;
                        int size_y1 = 27;

                        //--------
                        int x_temp; int y_temp;
                        int x1;
                        int y0;
                        int y1; int y2; int y3; int y4;
                        int y0_f; int y1_f; int y2_f; int y3_f;
                        int y_diff_0_1;
                        x1 = 150;
                        y0 = 60; y1 = 265; y2 = 295; y3 = 325; y4 = 375;
                        y0_f = y0 + size_y0; y1_f = y1 + size_y1; y2_f = y2 + size_y1; y3_f = y3 + size_y1;
                        y_diff_0_1 = y1 - y0;

                        int y_map;
                        Point line_p2 = new Point(x1, y0_f); Point line_p1 = line_p2;
                        Point line1_p2 = new Point(x1, y1_f); Point line1_p1 = line1_p2;
                        Point line2_p2 = new Point(x1, y2_f); Point line2_p1 = line2_p2;
                        Point line3_p2 = new Point(x1, y3_f); Point line3_p1 = line3_p2;

                        Brush brush_1 = Brushes.DarkMagenta; 
                        Pen drawingPen_1 = new Pen(brush_1, 2);
                        Brush brush_2 = Brushes.Orange; 
                        Pen drawingPen_2 = new Pen(brush_2, 2);
                        
                        Pen drawingPen_headText = new Pen(Brushes.White, 3);
                        SolidColorBrush sb0 = new SolidColorBrush(Color.FromArgb(255, 125, 125, 125));
                        SolidColorBrush sb1; SolidColorBrush sb2; SolidColorBrush sb3; SolidColorBrush sb4;


                        for (int i = 0; i < data_count; i++ )
                        {
                            DetectorReportData data = report_data_35List_array[i];
                            //-- Head Text --------------
                            if (i % 6 == 0)
                            {
                                dc.DrawLine(drawingPen_headText, new Point(x1, y0 - 10), new Point(x1, y0 + 10));
                                drawTextLabel(dc, data.time, x1, y0 - 30);
                            }
                            //-- BG Color ------------------------
                            if (data.flag_break != 1)
                            {
                                sb1 = getBrush_byHRL_xlv(data.total_lv);
                                sb2 = getBrush_byHRL_3lv(data.twist_lv);
                                sb3 = getBrush_byHRL_3lv(data.pitch_lv);
                                sb4 = getBrush_byHRL_3lv(data.prolong_lv);
                            }
                            else { sb1 = sb0; sb2 = sb0; sb3 = sb0; sb4 = sb0; }
                            dc.DrawRectangle(sb1, null, new System.Windows.Rect(x1, y0, size_x1, size_y0));
                            dc.DrawRectangle(sb2, null, new System.Windows.Rect(x1, y1, size_x1, size_y1));
                            dc.DrawRectangle(sb3, null, new System.Windows.Rect(x1, y2, size_x1, size_y1));
                            dc.DrawRectangle(sb4, null, new System.Windows.Rect(x1, y3, size_x1, size_y1));
                            //-------------------------------
                            x_temp = x1 + size_x1_e_half;
                            y_temp = y4;
                            //---------------------------------
                            if (data.flag_break == 0)
                            {
                                //--- Line  -----------------------         
                                y_map = mapPosition(data.total_score, 100, y0, y0_f);
                                drawLine(dc, drawingPen_1, brush_1, ref line_p1, ref line_p2, x_temp, y_map, afterBreak);
                                if (checkChangeInData(i,0)  || afterBreak) { dc.DrawEllipse(brush_1, null, line_p2, 4, 4); }
                                //
                                if (checkLine.IsChecked.Value)
                                {
                                    y_map = mapPosition(data.twist_score, 60, y1, y1_f);
                                    drawLine(dc, drawingPen_2, brush_2, ref line1_p1, ref line1_p2, x_temp, y_map, afterBreak);
                                    if (checkChangeInData(i, 1) || afterBreak) { dc.DrawEllipse(brush_2, null, line1_p2, 4, 4); }                                    //----
                                    //
                                    y_map = mapPosition(data.pitch_score, 60, y2, y2_f);
                                    drawLine(dc, drawingPen_2, brush_2, ref line2_p1, ref line2_p2, x_temp, y_map, afterBreak);
                                    if (checkChangeInData(i, 2) || afterBreak) { dc.DrawEllipse(brush_2, null, line2_p2, 4, 4); }                                    //----
                                    //
                                    y_map = mapPosition(data.prolong_score, 240, y3, y3_f);
                                    drawLine(dc, drawingPen_2, brush_2, ref line3_p1, ref line3_p2, x_temp, y_map, afterBreak);
                                    if (checkChangeInData(i,3) || afterBreak) { dc.DrawEllipse(brush_2, null, line3_p2, 4, 4); }
                                }
                                //--- First Icon -------------------
                                if (data.flag_stand == 1)
                                {
                                    drawIcon(dc, 9, x_temp, y_temp); y_temp += 25;
                                    if (data.flag_move == 1) { drawIcon(dc, 12, x_temp, y_temp); y_temp += 25; }
                                }
                                else
                                {
                                    if (data.flag_move == 1) { drawIcon(dc, 12, x_temp, y_temp); y_temp += 25; }
                                    else
                                    {
                                        if (data.flag_pitch == 1) { drawIcon(dc, 11, x_temp, y_temp); y_temp += 25; }
                                        if (data.flag_twist == 1) { drawIcon(dc, 21, x_temp - 5, y_temp); y_temp += 25; }
                                        if (data.flag_twist == -1) { drawIcon(dc, 22, x_temp + 5, y_temp); y_temp += 25; }
                                    }
                                }
                                afterBreak = false;
                            }
                            else
                            {
                                if (afterBreak == false)
                                {
                                    drawLine_endBreak(dc, drawingPen_1, line_p2, size_x1_e_half);
                                    if (checkLine.IsChecked.Value)
                                    {
                                        drawLine_endBreak(dc, drawingPen_2, line1_p2, 10);
                                        drawLine_endBreak(dc, drawingPen_2, line2_p2, 10);
                                        drawLine_endBreak(dc, drawingPen_2, line3_p2, 10);
                                    }
                                }
                                afterBreak = true;
                            }
                            //-------------------
                            x1 += size_x1 + 2;
                        }
                        //---- Draw Line End for the last record -----
                        drawLine_endBreak(dc, drawingPen_1, line_p2, size_x1_e_half);
                        if (checkLine.IsChecked.Value)
                        {
                            drawLine_endBreak(dc, drawingPen_2, line1_p2, 10);
                            drawLine_endBreak(dc, drawingPen_2, line2_p2, 10);
                            drawLine_endBreak(dc, drawingPen_2, line3_p2, 10);
                        }
                    }
                }
            }
            catch //(Exception ex) 
            { 
                //TheSys.showError("Draw:" + ex.Message); 
            }
        }


        Boolean checkChangeInData(int i, int v_id)
        {
            if (!checkNoDup.IsChecked.Value) { return true; }
            DetectorReportData[] data = report_data_35List_array;
            if (i == 0) { return true; }
            else if (i == data.Count() - 1) { return true; }
            else if (data[i - 1].flag_break == 1 || data[i + 1].flag_break == 1) { return true; }
            else
            {
                if (v_id == 0)
                {
                    if (data[i].total_score != data[i - 1].total_score) { return true; }
                    else if (data[i].total_score != data[i + 1].total_score) { return true; }
                    else { return false; }
                }
                else if (v_id == 1)
                {
                    if (data[i].twist_score != data[i - 1].twist_score) { return true; }
                    else if (data[i].twist_score != data[i + 1].twist_score) { return true; }
                    else { return false; }
                }
                else if (v_id == 2)
                {
                    if (data[i].pitch_score != data[i - 1].pitch_score) { return true; }
                    else if (data[i].pitch_score != data[i + 1].pitch_score) { return true; }
                    else { return false; }
                }
                else if (v_id == 3)
                {
                    if (data[i].prolong_score != data[i - 1].prolong_score) { return true; }
                    else if (data[i].prolong_score != data[i + 1].prolong_score) { return true; }
                    else { return false; }
                }
                else { return true; }
            }
           
        }

        void drawLine(DrawingContext dc, Pen drawingPen, Brush brush, 
            ref Point line_p1, ref Point line_p2, 
            double x_temp, double y_map,
            Boolean afterBreak)
        {
            if (afterBreak) { line_p1 = new Point(x_temp-12, y_map); }
            else { line_p1 = line_p2; }
            line_p2 = new Point(x_temp, y_map);
            dc.DrawLine(drawingPen, line_p1, line_p2); 
        }

        void drawLine_endBreak(DrawingContext dc, Pen drawingPen, Point line_p2, int range)
        {
            Point line_p3 = new Point(line_p2.X + range, line_p2.Y);
            dc.DrawLine(drawingPen, line_p2, line_p3); 
        }


        //11 = Red Circle
        //12 = Green Circle
        //21 = Right Tri
        //22 = Left Tri
        void drawIcon(DrawingContext dc, int icon_id, int x_begin, int y_begin)
        {
            if (icon_id == 11) {
                Point p = new Point(x_begin, y_begin);
                dc.DrawEllipse(Brushes.Red, null, p, 8, 8);
            }
            else if (icon_id == 12)
            {
                Point p = new Point(x_begin, y_begin);
                dc.DrawEllipse(Brushes.Green, null, p, 8, 8);
            }
            else if (icon_id == 21)
            {
                Point p1 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, 0, 10);
                Point p2 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, 90, 10);
                Point p3 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, -90, 10);
                var segments = new[] { new LineSegment(p1, true), new LineSegment(p2, true), new LineSegment(p3, true) };
                var figure = new PathFigure(p1, segments, true);
                var geo = new PathGeometry(new[] { figure });
                dc.DrawGeometry(Brushes.Red, null, geo);
            }
            else if (icon_id == 22)
            {
                Point p1 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, 180, 10);
                Point p2 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, 90, 10);
                Point p3 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, -90, 10);
                var segments = new[] { new LineSegment(p1, true), new LineSegment(p2, true), new LineSegment(p3, true) };
                var figure = new PathFigure(p1, segments, true);
                var geo = new PathGeometry(new[] { figure });
                dc.DrawGeometry(Brushes.Red, null, geo);
            }
            else if (icon_id == 9)
            {
                Point p1 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, -90, 10);
                Point p2 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, 0, 10);
                Point p3 = TheTool_Draw.pointToNewPoint(x_begin, y_begin, 180, 10);
                var segments = new[] { new LineSegment(p1, true), new LineSegment(p2, true), new LineSegment(p3, true) };
                var figure = new PathFigure(p1, segments, true);
                var geo = new PathGeometry(new[] { figure });
                dc.DrawGeometry(Brushes.Green, null, geo);
            }
        }

        //Map Value to draw
        int mapPosition(int value, int value_max, int draw_ceiling, int draw_floor)
        {
            int gap = draw_floor - draw_ceiling;
            double ratio = (double)value / (double)value_max;
            ratio = gap * ratio;
            return (int) (draw_floor - ratio);
        }

        void drawTextLabel_FixSet(DrawingContext dc)
        {            
            dc.DrawText(
               new FormattedText("Total",
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  45, System.Windows.Media.Brushes.White),
                  new System.Windows.Point(14, 125));
            dc.DrawText(
               new FormattedText("Risk",
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  45, System.Windows.Media.Brushes.White),
                  new System.Windows.Point(14, 175));
            dc.DrawText(
               new FormattedText("Twisted",
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  24, System.Windows.Media.Brushes.White),
                  new System.Windows.Point(14, 265));
            dc.DrawText(
               new FormattedText("Pitch",
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  24, System.Windows.Media.Brushes.White),
                  new System.Windows.Point(14, 295));
            dc.DrawText(
               new FormattedText("Prolong.",
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  24, System.Windows.Media.Brushes.White),
                  new System.Windows.Point(14, 325));
            int x1 = 115;
            BitmapImage ows_a0 = new BitmapImage(new Uri(TheURL.url_0_root + @"img/ows_a0.png", UriKind.Relative));
            dc.DrawImage(ows_a0, new Rect(x1, 265, 30, 30));
            BitmapImage ows_fp00 = new BitmapImage(new Uri(TheURL.url_0_root + @"img/ows_fp00.png", UriKind.Relative));
            dc.DrawImage(ows_fp00, new Rect(x1, 295, 30, 30));
            BitmapImage ows_f2 = new BitmapImage(new Uri(TheURL.url_0_root + @"img/ows_f2.png", UriKind.Relative));
            dc.DrawImage(ows_f2, new Rect(x1, 325, 30, 30));            
        }

        void drawTextLabel(DrawingContext dc, string txt, double x, double y)
        {
            FormattedText ftxt = new FormattedText(txt,
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  16, System.Windows.Media.Brushes.White);
            ftxt.TextAlignment = TextAlignment.Center;
            dc.DrawText(ftxt,new System.Windows.Point(x, y));
        }

        void drawTextFrameRate(DrawingContext dc)
        {
            try
            {
                string framesize = "Frame Size: ";
                if (frame_m > 0) { framesize += frame_m + "m "; }
                if (frame_s > 0) { framesize += frame_s + "s"; }
                dc.DrawText(
                   new FormattedText(framesize,
                      CultureInfo.GetCultureInfo("en-us"),
                      FlowDirection.LeftToRight,
                      new Typeface("Verdana"),
                      16, System.Windows.Media.Brushes.White),
                      new System.Windows.Point(14, 400));
            }
            catch (Exception ex){ TheSys.showError("TxtFR: " + ex.Message); }
        }

        //Green 128
        SolidColorBrush getBrush_byHRL_3lv(int i)
        {
            if (i == 2) { return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); }
            else if (i == 1) { return new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)); }
            else { return new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); } 
        }

        //Green 128
        SolidColorBrush getBrush_byHRL_xlv(double d)
        {
            byte red; byte green;
            d--;
            if (d <= 0) { green = 255; } else { green = (byte)(255 * (1 - d)); }
            if (d >= 0) { red = 255; } else { red = (byte)(255 * d); }
            //TheSys.showError(255 * (1 - d) + "");
            return new SolidColorBrush(Color.FromArgb(255, red, green, 0));
        }

        
        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }

        private void checkLine_Checked(object sender, RoutedEventArgs e)
        {
            draw();
        }

        private void checkNoDup_Checked(object sender, RoutedEventArgs e)
        {
            draw();
        }

        public Detector detector = null;

        public void runData(List<DetectorReportData> d)
        {
            this.report_data = d;
            adjustGUI();
            prepareData();
            draw();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (detector != null) { detector.graphVisualizer = null; }
            }
            catch { }
        }

        private void but0_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_ows_GraphHB); }
            catch { }
        }

        


    }


}
