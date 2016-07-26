//To keep Tool for whole Project

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Media;
using System.Data;
using System.Diagnostics;
using Microsoft.Kinect;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Media.Media3D;


namespace P_Tracker2
{
    class TheTool
    {
        //@"pack://application:,,,/P_Tracker2;component/img/Snap2.png"
        public static void change_ImgSource(System.Windows.Controls.Image img, string path)
        {
            try
            {
                img.Source = new BitmapImage(new Uri(path));
            }
            catch { }
        }


        //@"greenScrn/Background.png"
        public static void change_ImgSource_inDebug(System.Windows.Controls.Image img, string path)
        {
            try
            {
                BitmapImage b = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/"
                     + path, UriKind.Absolute));
                img.Source = b;
            }
            catch { }
        }

        //only Numeric Textbox
        public static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        public static bool isTextDouble(String s)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return regex.IsMatch(s);
        }

        //If blank get 0
        public static int getInt(System.Windows.Controls.TextBox txt)
        {
            int i = 0;
            try { i = int.Parse(txt.Text); }
            catch { }
            return i;
        }


        public static void removeSpace(System.Windows.Controls.TextBox tb)
        {
            tb.Text = tb.Text.Replace(" ", string.Empty);
            tb.Select(tb.Text.Length, 0);//move cursor
        }

        //If blank get 0
        public static int getInt(System.Windows.Controls.ComboBox cb)
        {
            int i = 0;
            try
            {
                i = int.Parse(cb.SelectedItem.ToString());
            }
            catch { }
            return i;
        }

        //If blank get 0
        public static double getDb(System.Windows.Controls.ComboBox cb)
        {
            double d = 0;
            try
            {
                d = double.Parse(cb.Text);
            }
            catch { }
            return d;
        }

        //If blank get 0
        public static int getInt(string txt)
        {
            int i = 0;
            try { i = int.Parse(txt); }
            catch { }
            return i;
        }

        //If blank get 0
        public static double getDouble(System.Windows.Controls.TextBox txt)
        {
            double d = 0;
            try { d = double.Parse(txt.Text); }
            catch { }
            return d;
        }

        //If blank get 0
        public static double getDouble(string txt)
        {
            double d = 0;
            try { d = double.Parse(txt); }
            catch { }
            return d;
        }

        public static double adjustRange(double d, double min, double max)
        {
            if (d < min) { d = min; }
            else if (d > max) { d = max; }
            return d;
        }

        static public void openFileLocation(string fullPath)
        {
            try
            {
                string path = Path.GetDirectoryName(fullPath);
                Process.Start(path);
            }
            catch { }
        }

        static public void delFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }

        static public Boolean checkPathExist(string s)
        {
            return File.Exists(s);
        }

        //=============================================================================================
        //===================== prepare ===================================================

        static public string getFileName_byPath(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        
        static public string getExtension_byPath(string path)
        {
            return Path.GetExtension(path);
        }

        static public string getDirectory_byPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        //______.exe >> ______
        static public string getFilePathExcludeExtension_byPath(string path)
        {
            return TheTool.getDirectory_byPath(path) + @"\" + TheTool.getFileName_byPath(path);
        }

        //0 : 1 >> 01
        static public string getTxt_Numeric_FillBy0(string txt, int letter)
        {
            for (int i = txt.Length; i < letter; i++)
            {
                txt = "0" + txt;
            }
            return txt;
        }

        //2 : 0.1 >> 0.10
        static public string getTxt_NumericDigit_FillBy0(string txt, int digit)
        {
            int length = txt.Length;
            int length_expect = 2 + digit;
            for (int i = length; i < length_expect; i++)
            {
                txt += "0";
            }
            return txt;
        }

        static public void Folder_CreateIfMissing(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        //=============================================================================================
        //===================== Export CSV ===================================================

        static public List<double[]> list_attribute_data_last = null;//temporary keep Previous Data

        //abnormalPath = not save in to "file" folder
        static public void exportCSV_listPerson(string full_path, List<Person> listPerson
            , Boolean detector
            , Boolean euclidean, Boolean mode_seat, Boolean mode_lossy
            , Boolean popUp_ifFinish)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(full_path))
                {
                    //Write Header-----------------------
                    file.WriteLine(getCSV_Header(detector, euclidean, mode_seat, mode_lossy));
                    //Write Data-----------------------
                    list_attribute_data_last = null;//reset temp
                    foreach (Person p_current in listPerson)
                    {
                        file.WriteLine(getCSV_StringLine(p_current, detector, euclidean, mode_seat, mode_lossy));
                    }
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + full_path + "'", "Export CSV"); }
            }
            catch
            {
                System.Windows.MessageBox.Show("CSV: unsuccessful export", "Unknown Error");
            };
        }

        static public string getCSV_Header(Boolean detector, Boolean euclidean, Boolean mode_seat, Boolean mode_lossy)
        {
            String header = "Time";
            List<String> list_attribute = TheTool_Person.get_personAttribute_name(mode_seat, mode_lossy);
            if (detector == true) { header += header_Detector(); }
            if (euclidean == true) { foreach (string s in list_attribute) { header += header_e(s); } }
            foreach (string s in list_attribute) { header += header_cxyz(s); }
            return header;
        }

        //Get: Head
        //Return: ,Head_c,Head_x,Head_y,Head_z
        static public string header_cxyz(string head)
        {
            return "," + head + "_c," + head + "_x," + head + "_y," + head + "_z";
        }


        //Line, For write each .csv line
        static public string getCSV_StringLine(Person p_current
            , Boolean detector, Boolean euclidean
            , Boolean mode_seat, Boolean mode_lossy)
        {
            //------ Get Attribure to be Write ---------
            List<double[]> list_attribute_data = TheTool_Person.get_personAttribute_data(p_current, mode_seat, mode_lossy);
            //========== Begin Write Data ========
            string text = p_current.TimeTrack;
            //-------- Euclidean Data ------------
            if (detector == true) { text += getCSV_addDetector(p_current); }
            //-------- Euclidean Data ------------
            if (euclidean == true) { text += getCSV_addEuclidian(list_attribute_data); }
            //-------- c,x,y,z Data --------------
            foreach (double[] d in list_attribute_data)
            {
                text += "," + string.Join(",", d);
            }
            //---------------------------------------
            list_attribute_data_last = list_attribute_data;
            return text;
        }

        //============================ Detector ==============================
        static public string header_Detector()
        {
            return ",state,sit,pitch,bend,turn";
        }
        static String getCSV_addDetector(Person person)
        {
            string txt_add = "";
            PersonD pd = person.personD;
            string stateTxt;
            if (pd != null)
            {
                stateTxt = "\"" + pd.state.Replace(", ", " ") + "\"";// "(state)"
                txt_add += "," + stateTxt;
                txt_add += "," + pd.sit_flag;
                txt_add += "," + pd.pitch_flag;
                txt_add += "," + pd.bend_flag;
                txt_add += "," + pd.turn_flag;
            }
            else { txt_add += ",,,,,"; }
            return txt_add;
        }

        //============================ Euclidian ==============================
        //Return: ,Head_e
        static public string header_e(string head)
        {
            return "," + head + "_e";
        }
        static String getCSV_addEuclidian(List<double[]> list_attribute_data)
        {
            string txt_add = "";
            if (list_attribute_data_last != null)
            {
                for (int i = 0; i < list_attribute_data.Count(); i++)
                {
                    txt_add += "," + string.Join(",", getEuclidean(list_attribute_data[i], list_attribute_data_last[i]));
                }
            }
            else// No last Row >> add 0 data
            {
                for (int i = 0; i < list_attribute_data.Count(); i++)
                {
                    txt_add += ",0";
                }
            }
            return txt_add;
        }

        //Euclidean Distance =SQRT((x2-x1)^2+(y2-y1)^2+(z2-z1)^2)
        //double[4] >> c,x,y,z 
        static String getEuclidean(double[] p1, double[] p2)
        {
            try
            {
                double eucli = Math.Pow(p1[1] - p2[1], 2) + Math.Pow(p1[2] - p2[2], 2) + Math.Pow(p1[3] - p2[3], 2);
                eucli = Math.Sqrt(eucli);
                return eucli.ToString();
            }
            catch { return "Null"; }
        }



        static public void exportFile(
            List<string> textList, string path
            , Boolean popUp_ifFinish, bool append = false)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, append))
                {
                    foreach (string s in textList)
                    {
                        file.WriteLine(s);
                    }
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + path + "'", "Export File"); }
            }
            catch (Exception ex) { TheSys.showError("Error Export:" + path + " : " + ex.ToString(), true); };
        }

        //static public void exportFile(
        //    string text, string path
        //    , Boolean popUp_ifFinish)
        //{
        //    try
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
        //        {
        //            file.WriteLine(text);
        //        }
        //        if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + path + "'", "Export File"); }
        //    }
        //    catch { TheSys.showError("Error Export:" + path, true); };
        //}

        //=============================================================================================
        //===================== txt O3KNS ===================================================

        static public void exportTxt_O3KNS(string file_name, List<Person> listPerson
            , Boolean popUp_ifFinish)
        {
            try
            {
                string path = TheURL.url_saveFolder + file_name + ".txt";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                {
                    foreach (Person p in listPerson)
                    {
                        file.WriteLine(getStringLine(p));
                    }
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + path + "'", "Export .txt for O3KNS"); }
            }
            catch { System.Windows.MessageBox.Show("Txt O3KNS: unsuccessful export", "Unknown Error"); };
        }


        public static string trackPersonID = "A";
        //Line, For write each .txt line
        //O3KNS format
        static public string getStringLine(Person p)
        {
            string text = trackPersonID;
            text += " 0 " + string.Join(" ", p.Head);
            text += " 1 " + string.Join(" ", p.ShoulderCenter);
            text += " 2 " + string.Join(" ", p.Spine);
            text += " 3 " + string.Join(" ", p.ShoulderLeft);
            text += " 4 " + string.Join(" ", p.ElbowLeft);
            text += " 5 " + string.Join(" ", p.WristLeft);
            text += " 6 " + string.Join(" ", p.ShoulderRight);
            text += " 7 " + string.Join(" ", p.ElbowRight);
            text += " 8 " + string.Join(" ", p.WristRight);
            text += " 9 " + string.Join(" ", p.HipLeft);
            text += " 10 " + string.Join(" ", p.KneeLeft);
            text += " 11 " + string.Join(" ", p.AnkleLeft);
            text += " 12 " + string.Join(" ", p.HipRight);
            text += " 13 " + string.Join(" ", p.KneeRight);
            text += " 14 " + string.Join(" ", p.AnkleRight);
            return text;
        }


        //=============================================================================================
        //===================== txt Note ===================================================

        static public void exportTxtNote(string file_name, UserTracker tracker
            , Boolean popUp_ifFinish)
        {
            try
            {
                string path = TheURL.url_saveFolder + file_name + ".txt";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                {
                    int row_tot = tracker.rowTotal;
                    Detector detector = tracker.form_Detector;
                    TotalCounter t = tracker.totalCounter;
                    //=====================
                    file.WriteLine("UserTracker by Pujana P");
                    file.WriteLine("Date: " + tracker.thisTime.ToString("yyyy/MM/dd"));
                    file.WriteLine("Time: " + tracker.startTime.ToString("HH:mm:ss")
                        + " - " + tracker.thisTime.ToString("HH:mm:ss"));
                    file.WriteLine("------------------------------");
                    file.WriteLine(tracker.txtRow.Content);
                    file.WriteLine(tracker.txtTime.Content + "  " + calTimeTotal(tracker.thisTime0));
                    file.WriteLine(tracker.txtAvgFps.Content);
                    file.WriteLine("------------------------------");
                    file.WriteLine("Setup");
                    if (tracker.checkSeat.IsChecked == true) { file.WriteLine("- Seated mode"); }
                    if (tracker.checkClose.IsChecked == true) { file.WriteLine("- Closed mode"); }
                    if (tracker.checkLossy.IsChecked == true) { file.WriteLine("- Lossy"); }
                    if (tracker.checkSmooth.IsChecked == true)
                    {
                        file.WriteLine("- Smoothing"
                            + "  (s" + tracker.smooth_Smooth
                            + "  c" + tracker.smooth_Correct
                            + "  p" + tracker.smooth_Predict
                            + "  j" + tracker.smooth_Jitter
                            + "  d" + tracker.smooth_Davia
                            + ")"
                        );
                    }
                    if (detector != null)
                    {
                        file.WriteLine("- Facing deg: " + detector.facing_deg_base);
                        file.WriteLine("- Based Range: " + detector.range_base);
                    }
                    file.WriteLine("------------------------------");
                    if (detector != null && row_tot > 0)
                    {
                        file.WriteLine("Detector Setup");
                        file.WriteLine("consensus: " + detector.total_agree_consensus);
                        file.WriteLine("threshold");
                        file.WriteLine("- pitch: " + detector.pitch_threshold);
                        //file.WriteLine("- bend: " + detector.bend_threshold);
                        file.WriteLine("- twist: " + detector.turn_threshold);
                        file.WriteLine("- range: " + detector.range_threshold);
                        file.WriteLine("------------------------------");
                        file.WriteLine("Detector Summary");
                        if (row_tot > 0)
                        {
                            file.WriteLine("detect: " + writeDetectPercent(t.detector_count, row_tot));
                        }
                        file.WriteLine("- sit: " + writeDetectSummary(t.sit_total, row_tot));
                        file.WriteLine("- pitch: " + writeDetectSummary(t.pitch_total, row_tot));
                        file.WriteLine("- bend R: " + writeDetectSummary(t.bend_total_r, row_tot));
                        file.WriteLine("- bend L: " + writeDetectSummary(t.bend_total_l, row_tot));
                        file.WriteLine("- turn R: " + writeDetectSummary(t.turn_total_r, row_tot));
                        file.WriteLine("- turn L: " + writeDetectSummary(t.bend_total_l, row_tot));
                    }
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + path + "'", "Export .txt Note"); }
            }
            catch { System.Windows.MessageBox.Show("Txt Note: unsuccessful export", "Unknown Error"); };
        }

        // d1/d2
        public static string writeDetectSummary(int total_d1, int total_d2)
        {
            double temp = total_d1 * 100;
            temp = temp / total_d2;
            return total_d1 + " _ " + Math.Round(temp, 2) + "%";
        }

        public static string writeDetectPercent(int total_d1, int total_d2)
        {
            double temp = total_d1 * 100;
            temp = temp / total_d2;
            return total_d1 + " / " + total_d2 + " _ " + Math.Round(temp, 2) + "%";
        }

        public static string calTimeTotal(int time)
        {
            string timeStr = "";
            if (time > 60)
            {
                timeStr += "/";
                int h = 0; int m = 0; int s = 0;
                //
                while (time >= 3600) { h++; time -= 3600; }
                while (time >= 60) { m++; time -= 60; }
                s = time;
                //

                if (h > 0) { timeStr += " " + h + "h"; }
                if (m > 0) { timeStr += " " + m + "m"; }
                if (s > 0) { timeStr += " " + s + "s"; }
            }
            return timeStr;
        }
        //=============================================================================================
        //===================== Excel ===================================================
        static int excel_row = 1;
        static int excel_col = 1;
        static Microsoft.Office.Interop.Excel._Worksheet worksheet = null;

        static public void exportExcel_UserTracker(string file_name, List<Person> listJoint
            , Boolean mode_seat, Boolean mode_lossy, String txtTime
            , String txtTotalTime, String txtTotalRow)
        {
            try
            {
                //================================  Prepare  ================================
                // creating Excel Application
                Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
                // creating new WorkBook within Excel application
                Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                // creating new Excelsheet in workbook
                worksheet = null;
                // see the excel sheet behind the program
                app.Visible = true;
                // get the reference of first sheet. By default its name is Sheet1.
                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.Sheets["Sheet1"];
                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.ActiveSheet;
                // changing the name of active sheet
                worksheet.Name = file_name;
                //================================ Writing ================================
                // storing header part in Excel
                excel_row = 1;//initial at Row....
                excel_col = 1;

                //---------------- File Info -------------------------
                worksheet.Cells[excel_row, 1] = "P-Tracker by Pujana P.";
                excel_row += 1;//go to next Row
                worksheet.Cells[excel_row, 1] = ""
                            + "Date: " + DateTime.Now.ToString("dd/MM/yyyy")
                            + ", " + txtTime;
                excel_row += 1;//go to next Row
                worksheet.Cells[excel_row, 1] = "Row: " + txtTotalRow + " (" + txtTotalTime + ")";
                excel_row += 1;//go to next Row
                excel_row += 1;//go to next Row
                //---------------- Body -------------------------
                //---------- Column Header
                worksheet.Cells[excel_row, excel_col] = "Time"; excel_col++;
                List<String> header = new List<String>();
                foreach (string s in TheTool_Person.get_personAttribute_name(mode_seat, mode_lossy)) { header.Add(s); }
                addHeader(header);
                excel_row++;
                //---------- Data
                foreach (Person p in listJoint)
                {
                    excel_col = 1;
                    worksheet.Cells[excel_row, excel_col] = p.TimeTrack;
                    //-------------
                    List<double[]> list_attribute_data = TheTool_Person.get_personAttribute_data(p, mode_seat, mode_lossy);
                    addData(list_attribute_data);
                    excel_row++;
                }
                //================================ Modify format ================================
                worksheet.Columns.AutoFit();
                //worksheet.Columns.HorizontalAlignment = HorizontalAlignment.Left;
                worksheet.Rows.RowHeight = 15;
                //worksheet.Rows.AutoFit();

                try
                {
                    worksheet.Cells.Font.Color = Color.FromArgb(255, 255, 255);
                    worksheet.Cells.Interior.Color = Color.FromArgb(0, 0, 0);

                    //--------- Head Info --------
                    Microsoft.Office.Interop.Excel.Range chartRange;
                    chartRange = worksheet.get_Range("a1", "x1");
                    chartRange.Font.Bold = true;
                    chartRange.Font.Size = 20;
                    chartRange.Rows.RowHeight = 25;
                    chartRange.Font.Color = Color.FromArgb(255, 255, 0);
                    //
                    chartRange = worksheet.get_Range("a2", "x3");
                    chartRange.Font.Color = Color.FromArgb(255, 255, 0);
                }
                catch { };


                //================================ Save ================================
                // save the application
                //workbook.SaveAs("c:\\output.xlsx",Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive , Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                // Exit from the application
                //app.Quit();
            }
            catch { System.Windows.MessageBox.Show("Excel: unsuccessful export", "Unknown Error"); };
        }

        static void addHeader(List<String> header)
        {
            foreach (string str in header)
            {
                //confident + X + Y + Z
                worksheet.Cells[excel_row, excel_col] = str + "_c"; excel_col++;
                worksheet.Cells[excel_row, excel_col] = str + "_x"; excel_col++;
                worksheet.Cells[excel_row, excel_col] = str + "_y"; excel_col++;
                worksheet.Cells[excel_row, excel_col] = str + "_z"; excel_col++;
            }
        }


        static double[] temp_db = new double[] { 0, 0, 0, 0 };//Anti-Null
        static void addData(List<double[]> dbList)
        {
            foreach (double[] db in dbList)
            {
                if (db.Count() >= 4)
                {
                    temp_db = db;
                }
                else { temp_db = new double[] { 0, 0, 0, 0 }; }
                for (int i = 0; i < 4; i++)
                {
                    excel_col++; worksheet.Cells[excel_row, excel_col] = db[i].ToString();
                }
            }
        }


        //=============================================================================================
        //===================== Prt Scrn : Print Screen ===================================================

        static public void SaveScreen(string file_path, Boolean popUp_ifFinish)
        {
            try
            {
                string path = file_path;
                Bitmap scrn = takeAllScreen();
                scrn.Save(path, ImageFormat.Png);
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + path + "'", "PrntScr PNG"); }
            }
            catch { System.Windows.MessageBox.Show("PNG: unsuccessful PrntScr", "Unknown Error"); };
        }

        static public void SaveScreen_activeWindow(string file_path, Boolean popUp_ifFinish)
        {
            try
            {
                Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                    }
                    bitmap.Save(file_path, ImageFormat.Jpeg);
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + file_path + "'", "PrntScr PNG"); }
            }
            catch { System.Windows.MessageBox.Show("PNG: unsuccessful PrntScr", "Unknown Error"); };
        }


        //static public Bitmap takePrimaryScreen()
        //{
        //    Bitmap myImage = new Bitmap(Screen.PrimaryScreen.WorkingArea.Width,
        //    Screen.PrimaryScreen.WorkingArea.Height);
        //    Graphics gr1 = Graphics.FromImage(myImage);
        //    IntPtr dc1 = gr1.GetHdc();
        //    IntPtr dc2 = NativeMethods.GetWindowDC(NativeMethods.GetForegroundWindow());
        //    NativeMethods.BitBlt(dc1, Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, Screen.PrimaryScreen.Bounds.Width,
        //         Screen.PrimaryScreen.Bounds.Height, dc2, 0, 0, 13369376);
        //    gr1.ReleaseHdc(dc1);
        //    return myImage;
        //}

        static public Bitmap takeAllScreen()
        {
            Graphics G;
            System.Drawing.Point Pt = new System.Drawing.Point(0, 0);
            Screen[] Scrns = Screen.AllScreens;
            Bitmap[] ScrnBmp = new Bitmap[Scrns.Length];
            for (int i = 0; i < Scrns.Length; i++)
            {
                ScrnBmp[i] = new Bitmap(Scrns[i].Bounds.Width, Scrns[i].Bounds.Height);
                G = Graphics.FromImage(ScrnBmp[i]);
                G.CopyFromScreen(Pt, Pt, new System.Drawing.Size(ScrnBmp[i].Width, ScrnBmp[i].Height));
                G.Dispose();
            }
            return Concat_Bitmaps(ScrnBmp);
        }


        static public Bitmap Concat_Bitmaps(Bitmap[] bitmaps)
        {
            Bitmap Merged_Img = null;
            int Merged_Height = 0;
            int Merged_Width = 0;
            int drawed_height = 0;
            for (int i = 0; i < bitmaps.Length; i++)
            {
                if (Merged_Width < bitmaps[i].Width)
                    Merged_Width = bitmaps[i].Width;
                Merged_Height += bitmaps[i].Height;
            }
            Merged_Img = new Bitmap(Merged_Width, Merged_Height);
            Graphics gr = Graphics.FromImage(Merged_Img);
            for (int j = 0; j < bitmaps.Length; j++)
            {
                gr.DrawImage(bitmaps[j], 0, drawed_height, bitmaps[j].Width, bitmaps[j].Height);
                drawed_height += bitmaps[j].Height;
            }
            return Merged_Img;
        }

        //=============================================================================================
        //================== Play Sound ==============



        public static SoundPlayer soundPlayer = new SoundPlayer();

        public static void soundPlay(string sound)
        {
            try
            {
                soundPlayer = new SoundPlayer(sound);
                soundPlayer.Play();
            }
            catch { }
        }

        public static void soundStop()
        {
            try
            {
                soundPlayer.Stop();
            }
            catch { }
        }

        //==========================================================================
        //====== Viewer =============
        static public void v_exportCSV_Split(string folderName, string tag
            , DataTable dataTable, int split)
        {
            try
            {
                if (dataTable.Rows.Count > 0)
                {
                    split = split * 100;//change sec to centisecond
                    Folder_CreateIfMissing(TheURL.url_saveFolder + folderName);
                    string pathFolder = TheURL.url_saveFolder + folderName + @"\";
                    int record = dataTable.Rows.Count - 1;
                    //------------------------------------------
                    //--- Prepare header ---
                    string header = "";
                    int colCount = dataTable.Columns.Count;
                    for (int i = 0; i < colCount; i++)
                    {
                        header += dataTable.Columns[i];
                        if (i < colCount - 1)
                        {
                            header += ",";
                        }
                    }
                    //-----------------------------------------------------------
                    //-- First File Initialize --
                    string thisTime_s = "";
                    int thisCentiSec = 0;
                    //-
                    string timeStart_s = dataTable.Rows[0][0].ToString();
                    string fileName = v_getFileName_Split(tag, timeStart_s);
                    StreamWriter sw = new StreamWriter(pathFolder + fileName + ".csv", false);// Create the CSV
                    sw.Write(header); sw.Write(sw.NewLine);
                    int centiSecStart = calCentisecond(timeStart_s);
                    int centiSecSplit = centiSecStart + split;
                    //--------------------------------------------------
                    // Now write all the rows.
                    foreach (DataRow r in dataTable.Rows)
                    {
                        //---- check Split file-----------------------------------
                        thisTime_s = r[0].ToString();
                        thisCentiSec = calCentisecond(thisTime_s);
                        if (thisCentiSec >= centiSecSplit)
                        {
                            sw.Close();
                            //-------------------------
                            timeStart_s = thisTime_s;
                            fileName = v_getFileName_Split(tag, timeStart_s);
                            sw = new StreamWriter(pathFolder + fileName + ".csv", false);
                            sw.Write(header); sw.Write(sw.NewLine);
                            centiSecStart = thisCentiSec;
                            centiSecSplit = centiSecStart + split;
                        }
                        //----- Write Data ----------------------------------------
                        for (int i = 0; i < colCount; i++)
                        {
                            if (!Convert.IsDBNull(r[i]))
                            {
                                sw.Write(r[i].ToString());
                            }
                            if (i < colCount - 1)
                            {
                                sw.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                    sw.Close();
                    System.Windows.MessageBox.Show("Save to " + pathFolder);
                }
            }
            catch (Exception e) { System.Windows.MessageBox.Show(e.Message); }
        }

        /* get A1_S_0811_ and 1115454476
         * return A1_S_0811_153707
         */
        //In Case 315454476 change to 0315454476 first
        static public string v_getFileName_Split(string tag, string TimeStart)
        {
            TimeStart = solveZeroPrefix(TimeStart, 10);
            return tag + TimeStart.Substring(2, 6);
        }

        //Incase 001 become 1
        //change 1 to 001
        static public string solveZeroPrefix(string str, int digitTarget)
        {
            int i = digitTarget - str.Length;
            while (i > 0) { str = "0" + str; i--; }
            return str;
        }

        //----- Ignore Problem from 0.00 AM : Curse of New Day ------------
        //1115454476
        //D11 H15 M45 S44 MS76
        static public int calCentisecond(string time)
        {
            time = solveZeroPrefix(time, 10);
            //tranform to total centisec
            int hour = int.Parse(time.Substring(2, 2)) * 360000;
            int min = int.Parse(time.Substring(4, 2)) * 6000;
            int sec = int.Parse(time.Substring(6, 4));
            return hour + min + sec;
        }



        //=========================================================================
        //=========================================================================
        static public Boolean export_dataTable_to_CSV(string pathSave, DataTable dataTable)
        {
            try
            {
                StreamWriter sw = new StreamWriter(pathSave, false);// Create the CSV
                //--- Write header ---
                string header = "";
                int colCount = dataTable.Columns.Count;
                for (int i = 0; i < colCount; i++)
                {
                    header += dataTable.Columns[i];
                    if (i < colCount - 1)
                    {
                        header += ",";
                    }
                }
                sw.Write(header); sw.Write(sw.NewLine);
                //--------------------------------------------------
                // Now write all the rows.
                foreach (DataRow r in dataTable.Rows)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        if (!Convert.IsDBNull(r[i]))
                        {
                            sw.Write(TheTool.string_replaceChar(r[i].ToString(),","," "));
                        }
                        if (i < colCount - 1)
                        {
                            sw.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                //
                return true;
            }
            catch { return false; }
        }

        static public int CSV_countCol(string path)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                string[] cols = lines[0].Split(',');
                return cols.Count();
            }
            catch { return 0; }
        }

        //return list;
        static public void read_File(
            ref List<string> list, string path, Boolean readHeader)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (i == 0 && readHeader == false) { }
                    else { list.Add(lines[i]); }
                }
            }
            catch { }
        }

        //===========================================
        static public List<string> concatFile_OWS(List<string> filePath_list, Boolean withHeader, int skip)
        {
            List<string> concat_txt = new List<string>();
            string filename;
            int i = 0;
            foreach (string file_path in filePath_list)
            {
                filename = TheTool.getFileName_byPath(file_path);
                if (i == 0)
                {
                    TheTool.spec_read_File_toConcat(ref concat_txt, file_path, skip, withHeader, filename);
                }
                else
                {
                    TheTool.spec_read_File_toConcat(ref concat_txt, file_path, skip, false, filename);
                }
                i++;
            }
            return concat_txt;
        }

        static public List<string> concatFile(List<string> filePath_list, Boolean withHeader, int skip)
        {
            List<string> concat_txt = new List<string>();
            string filename;
            int i = 0;
            foreach (string file_path in filePath_list)
            {
                filename = TheTool.getFileName_byPath(file_path);
                if (i == 0)
                {
                    read_File_toConcat(ref concat_txt, file_path, skip, true, filename);
                }
                else
                {
                    read_File_toConcat(ref concat_txt, file_path, skip, false, filename);
                }
                i++;
            }
            return concat_txt;
        }

        //return list;
        static public void read_File_toConcat(
            ref List<string> list, string path
            , int skipRow, Boolean firstFile, string fileName)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (i == 0 && firstFile == true)
                    {
                        list.Add("file," + lines[i]);
                    }
                    else if (i < skipRow) { }
                    else { list.Add(fileName + "," + lines[i]); }
                }
            }
            catch { }
        }


        //specific fileName
        static public void spec_read_File_toConcat(
            ref List<string> list, string path
            , int skipRow, Boolean firstFile, string fileName)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (i == 0 && firstFile == true)
                    {
                        list.Add("file,user," + lines[i]);
                    }
                    else if (i < skipRow) { }
                    else
                    {
                        list.Add(
                            fileName + ","
                            + spec_getUser_fromFileName(fileName) + ","
                            + lines[i]);
                    }
                }
            }
            catch { }
        }

        //File A01_xxxxxx  >> get A01
        static public string spec_getUser_fromFileName(string fileName)
        {
            return fileName.Split('_')[0];
        }


        //return path if ERROR
        static public string export_CSV_delColumn(string path, string delCol)
        {
            try
            {
                DataTable dataTable = CSVReader.ReadCSVFile(path, true);
                dataTable.Columns.Remove(delCol);
                string fileName_noExtension = TheTool.getFileName_byPath(path);
                Folder_CreateIfMissing(TheURL.url_saveFolder + TheURL.url_9_DelCol);
                string savePath = TheURL.url_saveFolder + TheURL.url_9_DelCol + @"\" + fileName_noExtension + ".csv";
                export_dataTable_to_CSV(savePath, dataTable);
                return "";
            }
            //catch (Exception e) { System.Windows.MessageBox.Show(e.Message); }
            catch { return path + System.Environment.NewLine; }
        }


        //return path if ERROR
        static public string export_CSV_delColumn_byAllow(string path, string[] colAllow)
        {
            try
            {
                DataTable dataTable = CSVReader.ReadCSVFile(path, true);
                List<string> allCol = new List<string> { }; Boolean allow;
                //--------------------------------------------
                //dataTable.Columns["class"].ColumnName = "c";
                //dataTable.Columns["class2"].ColumnName = "class";
                foreach (DataColumn c in dataTable.Columns)
                {
                    allCol.Add(c.ColumnName);
                }
                foreach (string eachCol in allCol)
                {
                    //Check Col whether Allow
                    allow = false;
                    foreach (string colA in colAllow)
                    {
                        if (eachCol == colA) { allow = true; break; }
                    }
                    //Delete unAllow Col
                    if (allow == false) { dataTable.Columns.Remove(eachCol); }
                }
                //--------------------------------------------
                string fileName_noExtension = TheTool.getFileName_byPath(path);
                Folder_CreateIfMissing(TheURL.url_saveFolder + TheURL.url_9_DelCol);
                string savePath = TheURL.url_saveFolder + TheURL.url_9_DelCol + @"\" + fileName_noExtension + ".csv";
                export_dataTable_to_CSV(savePath, dataTable);
                return "";
            }
            catch (Exception e)
            {
                TheSys.showError("CSV:" + e.Message, true);
                return path + System.Environment.NewLine;
            }
        }


        static public string filePath = "";
        static public Microsoft.Win32.OpenFileDialog dialog;

        static public Nullable<bool> openFileDialog_01(Boolean multiFile, string type, string initialPath)
        {
            dialog = new Microsoft.Win32.OpenFileDialog();
            if (initialPath != "") { dialog.InitialDirectory = initialPath; }
            if (multiFile) { dialog.Multiselect = true; }
            //
            dialog.DefaultExt = type;
            if (type == ".arff")
            {
                dialog.Filter = "Arff data files (*.arff) |*.arff";
            }
            else if (type == ".xml")
            {
                dialog.Filter = "XML map files (*.xml) |*.xml";
            }
            else if (type == ".txt")
            {
                dialog.Filter = "Text files (*.txt) |*.txt";
            }
            else if (type == ".*")
            {
                dialog.Filter = "All files (*.*) |*.*";
            }
            else
            {
                dialog.Filter = "CSV basic (*.csv) |*.csv";
            }
            //
            return dialog.ShowDialog();
        }


        static public Nullable<bool> openFileDialog(Boolean multiFile, string type, string initialPath, string ext)
        {
            dialog = new Microsoft.Win32.OpenFileDialog();
            if (initialPath != "") { dialog.InitialDirectory = initialPath; }
            if (multiFile) { dialog.Multiselect = true; }
            //
            dialog.DefaultExt = type;
            dialog.Filter = ext;
            //
            return dialog.ShowDialog();
        }

        static public void copy_File(string path_origin, string path_save)
        {
            try
            {
                FileInfo file = new FileInfo(path_origin);
                file.CopyTo(path_save);
            }
            catch { }
        }

        static public void export_nameCoder(DataTable dt, string col_path_origin, string col_path_save)
        {
            string path_origin;
            string path_save;
            TheTool.Folder_CreateIfMissing(TheURL.url_saveFolder + TheURL.url_9_NameCode);
            foreach (DataRow r in dt.Rows)
            {
                path_origin = r[col_path_origin].ToString();
                path_save = TheURL.url_saveFolder + TheURL.url_9_NameCode + @"\" + r[col_path_save].ToString() + Path.GetExtension(path_origin);
                TheTool.copy_File(path_origin, path_save);
            }
        }



        //get double[3] = {x,y,z}
        static public double[] getDouble_fromJoint(DataTable dt, int row, string jointName)
        {
            try
            {
                return new double[]{
                    double.Parse(dt.Rows[row][jointName + "_x"].ToString())
                    ,double.Parse(dt.Rows[row][jointName + "_y"].ToString())
                    ,double.Parse(dt.Rows[row][jointName + "_z"].ToString())
                };
            }
            catch { return new double[] { }; }
        }

        //==============================================================
        //==============================================================

        public static string[] joint_list_upperOnly =
            new string[]{"Head","ShoulderCenter",
            "ShoulderLeft","ShoulderRight"
            ,"ElbowLeft","ElbowRight"
            ,"WristLeft","WristRight"
            ,"HandLeft","HandRight"};

        //get Head_x , Head_y, Head_z, Shoulder.....
        public static List<string> getListJoint(string suffix)
        {
            List<string> list = new List<string>();
            foreach (string s in joint_list_upperOnly)
            {
                list.Add(s + suffix);
            }
            return list;
        }

        //get Head_x , Head_y, Head_z, Shoulder.....
        public static List<string> getListJointXYZ()
        {
            List<string> list = new List<string>();
            foreach (string s in joint_list_upperOnly)
            {
                list.Add(s + "_x");
                list.Add(s + "_y");
                list.Add(s + "_z");
            }
            return list;
        }


        public static void showTable(DataTable dt, string title, string fileName)
        {
            SubViewer sub2 = new SubViewer();
            if (title != "") { sub2.setTitle(title); }
            if (fileName != "") { sub2.fileName = fileName; }
            sub2.Show();
            sub2.setDataGrid(dt);
        }



        public static List<string> dataTable_getList_fromColumn(DataTable dt, string col_name)
        {
            List<string> list = new List<string>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(r[col_name].ToString());
            }
            return list;
        }

        //Never used
        //List<string[2]> rule >> column name & value
        public static void dataTable_deleteRow_byRule(DataTable dt, List<string[]> rules)
        {
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = dt.Rows[i];
                Boolean passAllRule = false;
                foreach (string[] r in rules)
                {
                    if (dr[r[0]].ToString() == r[1]) { passAllRule = true; }
                    else { passAllRule = false; break; }
                }
                if (passAllRule) { dr.Delete(); }
            }
        }

        //list string with header
        // DT 1 Row 1 >> DT 2 Row 1 >> DT 3 Row 1 >> DT 1 Row 2 >> 
        public static List<string> dataTable_CombineShuffle_getListString(List<DataTable> dt_list)
        {
            List<string> output = new List<string>();
            try{
                DataTable[] dt_arr = dt_list.ToArray();
                //header --------------
                string data = "class";
                int i = 0;
                foreach (DataColumn dc in dt_arr[0].Columns)
                {
                    data += "," + dc.ColumnName;
                    i++;
                }
                output.Add(data);
                //---------------------
                int i_r = 0; int i_r_last = dt_list.First().Rows.Count;
                int i_c = 0; int i_c_last = dt_list.First().Columns.Count;
                int i_dt = 0; int i_dt_last = dt_list.Count;
                for (i_r = 0; i_r < i_r_last; i_r++)
                {
                    for(i_dt = 0; i_dt < i_dt_last; i_dt++)
                    {
                        data = "" + i_dt;
                        for(i_c = 0; i_c < i_c_last; i_c++)
                        {
                             data += "," + dt_arr[i_dt].Rows[i_r][i_c].ToString();
                        }
                        output.Add(data);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError("Shuffle: " + ex); }
            return output;
        }

        static public DataTable dataTable_combineValue(List<DataTable> dataTable, List<int> skip_col)
        {
            DataTable output_dt = dataTable.First().Copy();
            int i_r = 0;
            foreach (DataRow dr in dataTable.First().Rows)
            {
                int i_c = 0;
                foreach (DataColumn dc in dataTable[0].Columns)
                {
                    if (skip_col.Contains(i_c) == false) 
                    {
                        int v = 0;
                        foreach (DataTable dt in dataTable)
                        {
                            v += TheTool.getInt(dt.Rows[i_r][i_c].ToString());
                        }
                        output_dt.Rows[i_r][i_c] = v;
                    }
                    i_c++;
                }
                i_r++;
            }
            return output_dt;
        }

        //NEVER USED
        //"Size >= 230 AND Sex = 'm'"
        public static DataRow[] dataTable_selectRow(DataTable dt, string rule)
        {
            DataRow[] selected = dt.Select(rule);
            return selected;
        }

        public static DataTable dataTable_sortNumeric(DataTable dt, string col)
        {
            return dt.AsEnumerable()
                      .OrderBy(r => int.Parse(r.Field<String>(col)))
                      .CopyToDataTable();
        }

        //Group by first row
        //value is boolean "0" or "1" -- if "" then excluded
        public static void dataTable_SumUp_GroupByFirst(DataTable dt, Boolean resort, List<int> skip_colIDs, ref List<String> list_sum, ref List<String> list_percent)
        {
            if (skip_colIDs == null) { skip_colIDs = new List<int>(); }
            DataTable dt0;
            if (resort) { dt0 = dataTable_sort(dt, dt.Columns[0].ColumnName, false); }
            else{ dt0 = dt; }
            //
            int col_count = dt0.Columns.Count;
            int[] occurance = new int[col_count];
            int[] total = new int[col_count];
            string previous_mid = "";
            int r_i = 0;
            int r_last = dt0.Rows.Count - 1;
            foreach (DataRow r in dt0.Rows)
            {
                string current_mid = r[0].ToString();
                if (r_i > 0 && current_mid != previous_mid)
                {
                    string sum_data = previous_mid;
                    string percent_data = previous_mid;
                    double[] occurance_percen = intArr_divideAll(occurance, total);
                    //
                    for (int c = 1; c < occurance.Count(); c++)
                    {
                        if (skip_colIDs.Contains(c))
                        {
                            int r_i0 = r_i - 1;
                            if (r_i0 < 0) { r_i0 = r_i; }//just prevent error
                            string txt = dt0.Rows[r_i0 - 1][c].ToString();
                            sum_data += "," + txt;
                            percent_data += "," + txt;
                        }
                        else
                        {
                            sum_data += "," + occurance[c];
                            percent_data += "," + occurance_percen[c];
                        }
                    }
                    list_sum.Add(sum_data);
                    list_percent.Add(percent_data);
                    //reset
                    occurance = new int[col_count];
                    total = new int[col_count];
                }
                //-------------
                for (int c = 1; c < col_count; c++)
                {
                    if (skip_colIDs.Contains(c)) { }
                    else
                    {
                        string v = r[c].ToString();
                        occurance[c] += TheTool.getInt(r[c].ToString());
                        if (v != "") { total[c] += 1; } //exclude empthy cell
                    }
                }
                previous_mid = current_mid;
                r_i++;
                if (r_i == r_last)
                {
                    string sum_data = current_mid;
                    string percent_data = current_mid;
                    double[] occurance_percen = intArr_divideAll(occurance, total);
                    for (int c = 1; c < occurance.Count(); c++)
                    {
                        if (skip_colIDs.Contains(c))
                        {
                            string txt = dt0.Rows[r_i][c].ToString();
                            sum_data += "," + txt;
                            percent_data += "," + txt;
                        }
                        else
                        {
                            sum_data += "," + occurance[c];
                            percent_data += "," + occurance_percen[c];
                        }
                    }
                    list_sum.Add(sum_data);
                    list_percent.Add(percent_data);
                }
            }
        }

        public static double[] intArr_divideAll(int[] dividend, int[] divisor)
        {
            double[] d = new double[dividend.Count()];
            for (int i = 0; i < dividend.Count(); i++)
            {
                if (divisor[i] != 0) { d[i] = (double)dividend[i] / divisor[i]; }
            }
            return d;
        }

        public static double[] intArr_divideAll(int[] arr, int divisor)
        {
            double[] d = new double[arr.Count()];
            for (int i = 0; i < arr.Count(); i++)
            {
                d[i] = (double)arr[i] / divisor;
            }
            return d;
        }

        public static string intArr_getListString(int[] arr)
        {
            string s = "";
            for (int i = 0; i < arr.Count(); i++)
            {
                if (i > 0) { s += ","; }
                s += arr[i];
            }
            return s;
        }

        public static string doubleArr_getListString(double[] arr)
        {
            string s = "";
            for (int i = 0; i < arr.Count(); i++)
            {
                if (i > 0) { s += ","; }
                s += arr[i];
            }
            return s;
        }


        //index is row number, not id
        public static double dataTable_getAverage(DataTable dt, List<int> indices, string col_name)
        {
            List<double> sum_value = new List<double>();
            try
            {
                foreach (int i in indices)
                {
                    sum_value.Add(TheTool.getDouble(dt.Rows[i][col_name].ToString()));
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return sum_value.Average();
        }

        public static double dataTable_getAverage(DataTable dt, string col_name)
        {
            List<double> avg_value = new List<double>();
            //try
            //{
                foreach (DataRow dr in dt.Rows)
                {
                    avg_value.Add(TheTool.getDouble(dr[col_name].ToString()));
                }
                return avg_value.Average();
            //}
            //catch (Exception ex)
            //{
            //    TheSys.showError("ERROR (AVG on Column) : col=" + col_name + " count = " + dt.Columns.Count);
            //    TheSys.showError(ex);
            //    return 0;
            //}
        }

        public static double calNorm(double[] d)
        {
            double norm = 0;
            try
            {
                for(int i = 0; i < d.Count();i++){
                    norm += Math.Pow(d[i],2);
                }
                norm = Math.Sqrt(norm);
            }
            catch (Exception e) { TheSys.showError("Err: [calNorm()] " + e.ToString(), true); }
            return norm;
        }

        public static double calEuclidian(double[] d1, double[] d2)
        {
            double eucli = 0;
            try
            {
                if (d1.Count() == d2.Count())
                {
                    for (int i = 0; i < d1.Count(); i++)
                    {
                        eucli += Math.Pow(d1[i] - d2[i], 2);
                    }
                }
                eucli = Math.Sqrt(eucli);
            }
            catch (Exception e) { TheSys.showError("Err: [calEuclidian_2Joint()] " + e.ToString(), true); }
            return eucli;
        }

        public static double calEuclidian_2Joint(Joint a, Joint b)
        {
            double eucli = 0;
            try
            {
                double delta_x = a.Position.X - b.Position.X;
                delta_x = Math.Pow(delta_x, 2);
                double delta_y = a.Position.Y - b.Position.Y;
                delta_y = Math.Pow(delta_y, 2);
                double delta_z = a.Position.Z - b.Position.Z;
                delta_z = Math.Pow(delta_z, 2);
                eucli = Math.Sqrt(delta_x + delta_y + delta_z);
            }
            catch (Exception e) { TheSys.showError("Err: [calEuclidian_2Joint()] " + e.ToString(), true); }
            return eucli;
        }

        public static double calEuclidian_2Joint(Skeleton s, JointType a, JointType b)
        {
            return calEuclidian_2Joint(s.Joints[a], s.Joints[b]);
        }

        public static Boolean checkOpt(double v1, String opt, double v2)
        {
            Boolean pass = false;
            if (opt == ">" && v1 > v2) { pass = true; }
            else if (opt == "<" && v1 < v2) { pass = true; }
            else if (opt == ">=" && v1 >= v2) { pass = true; }
            else if (opt == "<=" && v1 <= v2) { pass = true; }
            else if (opt == "!=" && v1 != v2) { pass = true; }
            else if (opt == "<>" && v1 != v2) { pass = true; }
            else if (v1 == v2) { pass = true; }
            return pass;
        }

        public static Boolean checkOpt(int v1, String opt, int v2)
        {
            Boolean pass = false;
            if (opt == ">" && v1 > v2) { pass = true; }
            else if (opt == "<" && v1 < v2) { pass = true; }
            else if (opt == ">=" && v1 >= v2) { pass = true; }
            else if (opt == "<=" && v1 <= v2) { pass = true; }
            else if (opt == "!=" && v1 != v2) { pass = true; }
            else if (opt == "<>" && v1 != v2) { pass = true; }
            else { if (v1 == v2) { pass = true; } }
            return pass;
        }

        // "x + add" with maximum "max"
        static public int math_add(int x, int add, int max)
        {
            int r = x + add;
            if (r > max) { r = max; }
            return r;
        }

        static public int math_subtract(int x, int sub, int min)
        {
            int r = x - sub;
            if (r < min) { r = min; }
            return r;
        }


        //================================================================
        static public void exportExcel(string file_name, DataTable dt, Boolean removeProhibit)
        {
            try
            {
                //================================  Prepare  ================================
                Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                worksheet = null;// creating new Excelsheet in workbook
                app.Visible = true;
                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.Sheets["Sheet1"];
                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.ActiveSheet;
                worksheet.Name = file_name;// changing the name of active sheet
                //================================ Writing ================================
                excel_row = 1; excel_col = 1;
                //int dt_row = 0; int dt_col = 0;
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    worksheet.Cells[excel_row, excel_col] = dt.Columns[c].ToString();
                }
                excel_row++;
                //-------- data -----------
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    excel_col = 1;
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        worksheet.Cells[excel_row, excel_col] = dt.Rows[r][c].ToString();
                        excel_col++;
                    }
                    excel_row++;
                }
                //================================ Modify format ================================
                worksheet.Columns.AutoFit();
                worksheet.Rows.AutoFit();
            }
            catch (Exception ex)
            {
                TheSys.showError("exportEx:" + ex.ToString(), true);
            };
        }

        //static public char[] excel_prohibitCha = new char[] { ':', '\\', '/', '?', '*', '[', ']' };

        //static public string excel_eliminateProhibitCha(string txt)
        //{
        //    txt = txt.Trim(excel_prohibitCha);
        //    return txt;
        //}

        static public void exportExcel_DReport(string file_name, DataTable dt)
        {
            try
            {
                //================================  Prepare  ================================
                Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                worksheet = null;// creating new Excelsheet in workbook
                app.Visible = true;
                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.Sheets["Sheet1"];
                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.ActiveSheet;
                worksheet.Name = file_name;// changing the name of active sheet
                //===============
                worksheet.Cells.Font.Color = Color.FromArgb(255, 255, 255);
                worksheet.Cells.Interior.Color = Color.FromArgb(0, 0, 0);
                //================================ Writing ================================
                excel_row = 1; excel_col = 1;
                //int dt_row = 0; int dt_col = 0;
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    worksheet.Cells[excel_row, excel_col] = dt.Columns[c].ToString();
                    excel_col++;
                }
                excel_row++;
                //-------- data -----------
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    excel_col = 1;
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        worksheet.Cells[excel_row, excel_col] = dt.Rows[r][c].ToString();
                        excel_col++;
                    }
                    //========================
                    exportExcel_DReport_cellFormat(2, dt.Rows[r]["S_lv"].ToString());
                    exportExcel_DReport_cellFormat(3, dt.Rows[r]["P_lv"].ToString());
                    exportExcel_DReport_cellFormat(4, dt.Rows[r]["F_lv"].ToString());
                    //
                    //cell = worksheet.Cells[excel_row, 4];
                    //cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(
                    //    exportExcel_DReport_color(dt.Rows[r]["F_lv"].ToString())
                    //    );
                    //========================
                    excel_row++;
                }
                string rangeTxt = "A1:K" + excel_row;
                Microsoft.Office.Interop.Excel.Range range = worksheet.get_Range(rangeTxt, Type.Missing);
                excel_AllBorders(range.Borders);
                //================================ Modify format ================================
                worksheet.Columns.AutoFit();
                //worksheet.Columns.HorizontalAlignment = HorizontalAlignment.Left;
                //worksheet.Rows.RowHeight = 15;
                worksheet.Rows.AutoFit();
            }
            catch (Exception ex)
            {
                TheSys.showError("excel:" + ex.ToString(), true);
            };
        }

        static public Microsoft.Office.Interop.Excel.Range cell;
        static public Microsoft.Office.Interop.Excel.Borders border;
        static public void exportExcel_DReport_cellFormat(int col, string value)
        {
            cell = worksheet.Cells[excel_row, col];
            cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(
                exportExcel_DReport_color(value)
                );
            //border = cell.Borders;
            //border[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
            //    Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
        }

        static public Color exportExcel_DReport_color(string s)
        {
            if (s == "2") { return System.Drawing.Color.Red; }
            else if (s == "1") { return System.Drawing.Color.Yellow; }
            else { return System.Drawing.Color.Green; }
        }




        static public void excel_AllBorders(Microsoft.Office.Interop.Excel.Borders _borders)
        {
            _borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            _borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            _borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            _borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            _borders.Color = Color.Black;
        }

        //=============================================================================================


        //Replace "a" by "b"
        static public string string_replaceChar(string txt, string a, string b)
        {
            return txt.Replace(a, b);
        }

        public static void SortData(List<List<double>> data)
        {
            foreach (List<double> row in data) { row.Sort(); }
        }

        ////Intput row<col> 
        static public List<string> getListString(List<List<double>> list_old)
        {
            List<string> list_new = new List<string>();
            try
            {
                foreach (List<double> row in list_old)
                {
                    string line = ""; int c = 0;
                    foreach (double col in row)
                    {
                        if (c > 0) { line += ","; }
                        line += col;
                        c++;
                    }
                    list_new.Add(line);
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return list_new;
        }

        static public void exportCSV_orTXT(string file_path,List<string> list, Boolean popUp_ifFinish)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(file_path))
                {
                    foreach (string s in list)
                    {
                        file.WriteLine(s);
                    }
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save to '" + file_path + "'", "Export CSV"); }
            }
            catch (Exception ex)
            {
                TheSys.showError("CSV: unsuccessful export" + ex.ToString());
            };
        }

        static public void exportCSV_orTXT(string file_path, List<int> list_int, Boolean popUp_ifFinish)
        {
            List<string> list = new List<string>();
            foreach (int i in list_int) { list.Add(i.ToString()); }
            exportCSV_orTXT(file_path, list, popUp_ifFinish);
        }

        static public List<string> read_File_getListString(string path)
        {
            try
            {
                var logFile = File.ReadAllLines(path);
                return new List<string>(logFile);
            }
            catch { return new List<string> { }; }
        }

        static public double[][] read_File_getArrayArray(string path, Boolean skipFirstRow)
        {
            List<List<double>> list_list = read_File_getListListDouble(path,skipFirstRow);
            try
            {
                int row = list_list.Count();
                int col = list_list.First().Count();
                double[][] array_array = new double[row][];
                int r = 0;
                foreach (List<double> row_l in list_list)
                {
                    array_array[r] = row_l.ToArray();
                }
                return array_array;
            }
            catch { }
            return new double[0][];
        }

        //row<col>
        static public List<List<double>> read_File_getListListDouble(string path, Boolean skipFirstRow)
        {
            List<List<double>> col_list = new List<List<double>>();
            try
            {
                var logFile = File.ReadAllLines(path);
                int r = 0;
                foreach (string s in logFile)
                {
                    if (r > 0 || !skipFirstRow)
                    {
                        List<double> row_list = new List<double>();
                        string[] cell = TheTool.splitText(s, ",");
                        for (int i_c = 0; i_c < cell.Count(); i_c++)
                        {
                            row_list.Add(TheTool.getDouble(cell[i_c]));
                        }
                        col_list.Add(row_list);
                    }
                    r++;
                }
            }
            catch { }
            return col_list;
        }
        
        static public string read_File_getFirstLine(string path)
        {
            try
            {
                return File.ReadLines(path).First();
            }
            catch { return ""; }
        }

        static public string read_File_get1String(string path)
        {
            string txt = "";
            try
            {
                List<string> stringList = TheTool.read_File_getListString(path);
                foreach (string s in stringList)
                {
                    txt += s;
                }
            }
            catch { }
            return txt;
        }


        static public string convertBoolean_01(Boolean b)
        {
            if (b == true) { return "1"; }
            else { return "0"; }
        }

        static public int convertBoolean_01int(Boolean b)
        {
            if (b == true) { return 1; }
            else { return 0; }
        }

        static public Boolean convert01_Boolean(string txt)
        {
            if (txt == "1") { return true; }
            else { return false; }
        }

        static public String convert01_Boolean(int txt)
        {
            if (txt == 1) { return "true"; }
            else { return "false"; }
        }

        static public void writeFile(List<string> allLine, string fileURL, Boolean popUp_ifFinish)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileURL))
                {
                    foreach (string line in allLine)
                    {
                        file.WriteLine(line);
                    }
                }
                if (popUp_ifFinish == true) { System.Windows.MessageBox.Show(@"Save: " + fileURL); }
            }
            catch
            {
                TheSys.showError("Write File Error: " + fileURL);
            }
        }

        public static double calAVG(List<double> list)
        {
            try
            {
                return list.Skip(0).Take(list.Count).Average();
            }
            catch { return 0; }
        }

        public static string[] getFilePath_inFolder(string folderPath, Boolean nameonly)
        {
            try
            {
                if (nameonly)
                {
                    string[] files = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);
                    List<string> files_nm = new List<string>();
                    foreach (string a in files)
                    {
                        files_nm.Add(TheTool.getFileName_byPath(a));
                    }
                    return files_nm.ToArray();
                }
                else { return Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly); }
            }
            catch { return new string[] { }; }
        }

        public static string[] splitText(string s, string splt_txt)
        {
            try
            {
                return s.Split(new string[] { splt_txt }, StringSplitOptions.None);
            }
            catch { return new string[0]; }
        }

        public static double getNormalized(double v, double min, double max)
        {
            double r = max - min;
            if (r > 0)
            {
                v = v - min;
                v = v / r; return v;
            }
            else { return 0; }
        }

        //Output has no header : double[row, col]
        public static double[,] getArrayDouble_fromDataTable(DataTable dt)
        {
            var numRows = dt.Rows.Count;
            var numCols = dt.Columns.Count;
            double[,] arr = new double[numRows, numCols];
            for (int i_c = 0; i_c < numCols; i_c++)
            {
                for (int i_r = 0; i_r < numRows; i_r++)
                {
                    arr[i_r, i_c] = TheTool.getDouble(dt.Rows[i_r][i_c].ToString());
                }
            }
            return arr;
        }

        //Output has no header : double[row, col]
        public static double[,] getArrayDouble_fromDataList(List<String> data, Boolean input_hasHeader)
        {
            if (input_hasHeader) { data.RemoveAt(0); }
            var numRows = data.Count();
            var numCols = TheTool.splitText(data.First(), ",").Count();
            double[,] arr = new double[numRows, numCols];
            int i_r = 0;
            foreach (string str in data)
            {
                string[] cell = TheTool.splitText(str, ",");
                for (int i_c = 0; i_c < cell.Count(); i_c++)
                {
                    arr[i_r, i_c] = TheTool.getDouble(cell[i_c]);
                }
                i_r++;
            }
            return arr;
        }

        public static String string_remove(String s, String removeTxt)
        {
            return s.Replace(removeTxt, string.Empty);
        }

        public static String string_Tab(int count)
        {
            String txt = "";
            for (int i = 0; i < count; i++) { txt += "\t"; }
            return txt;
        }

        public static void saveXML(String data, String path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(data);
                // Save the document to a file and auto-indent the output.
                XmlTextWriter writer = new XmlTextWriter(path, null);
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
                writer.Close();
            }
            catch (Exception ex) { TheSys.showError(ex); TheSys.showError(data); }
        }

        public static Boolean stringExist_inList(String txt, List<String> list)
        {
            Boolean exist = false;
            foreach (String s in list) { if (String.Equals(s, txt)) { exist = true; } }
            return exist;
        }

        public static void sortComboBox(System.Windows.Controls.ComboBox combo)
        {
            combo.Items.SortDescriptions.Add(
                new System.ComponentModel.SortDescription("",
                System.ComponentModel.ListSortDirection.Ascending));
            combo.SelectedIndex = 0;
        }

        //expand all, except root
        public static void TVI_ExpandAll(TreeViewItem item, bool expand, int lv)
        {
            try
            {
                if (lv > 0 || expand == true) { item.IsExpanded = expand; }
                foreach (TreeViewItem child in item.Items)
                {
                    TVI_ExpandAll(child, expand, lv + 1);
                }
            }
            catch { }
        }

        public static int[] array_increaseIndex(int[] oldArray, int inceaseSize)
        {
            int[] newArray = new int[oldArray.Count() + inceaseSize];
            for (int i = 0; i < oldArray.Count(); i++)
            {
                newArray[i] = oldArray[i];
            }
            return newArray;
        }

        public static int[] array_addIndex(int[] oldArray, int value)
        {
            int[] newArray = array_increaseIndex(oldArray, 1);
            newArray[oldArray.Count()] = value;
            return newArray;
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        //=========================================================================
        //========= List INT ======================================================

        public static List<str_int> listInt_countDistinct(List<double> list_a)
        {
            list_a.Sort();
            List<str_int> output = new List<str_int>();
            string current_v = ""; int counter = 0;
            int i = 0;
            foreach (double a in list_a)
            {
                string s = a.ToString();
                if (i == 0) { counter = 1; current_v = s; }
                else if (s != current_v)
                {
                    //save previous
                    str_int record = new str_int();
                    record.str = s; record.i = counter;
                    output.Add(record);
                    //start new
                    counter = 1; current_v = s;
                }
                else { counter++; }
                i++;
            }
            return output;
        }

        public static List<T> list_Copy<T>(List<T> original)
        {
            List<T> new_list = new List<T>();
            new_list.AddRange(original);
            return new_list;
        }

        public static List<T> list_CutAt<T>(List<T> original, int lastID)
        {
            List<T> new_list = new List<T>();
            int i = 0;
            foreach (T o in original)
            {
                new_list.Add(o);
                i++;
                if (i == lastID) { break; } 
            }
            return new_list;
        }

        //select row base on "time" in list
        public static List<T> list_SelectRow<T>(List<T> list_original, List<int> selectedRow)
        {
            List<T> list_new = new List<T>();
            int i = 0;
            foreach (T r in list_original)
            {
                if (selectedRow.Contains(i))
                {
                    list_new.Add(r);
                }
                i++;
            }
            return list_new;
        }

        public static string math_getOptReversion(string origin, Boolean reverse)
        {
            if (reverse)
            {
                if (origin == ">") { return "<="; }
                else if (origin == ">=") { return "<"; }
                else if (origin == "<=") { return ">"; }
                else if (origin == "<") { return ">="; }
                else if (origin == "!=") { return "="; }
                else { return "!="; }
            }
            else { return origin; }
        }

        public static  Boolean checkTimePass(int moreThan, DateTime begin, DateTime now)
        {
            Boolean r = false;
            TimeSpan span = now.Subtract(begin);
            int timechange = (int)span.TotalMilliseconds;
            if (timechange > moreThan) { r = true; }
            return r;
        }

        public static List<List<double>> ListList_SwitchColumnToRow(List<List<double>> data)
        {
            try
            {
                var tmp = new List<List<double>>();
                int row = 0;
                int col = 0;
                for (col = 0; col < data[0].Count; col++)
                {
                    var tmp_list = new List<double>();
                    for (row = 0; row < data.Count; row++)
                    {
                        tmp_list.Add(data[row][col]);
                    }
                        tmp.Add(tmp_list);
                }

                return (tmp);
            }
            catch (Exception ex) { TheSys.showError(ex); return new List<List<double>>(); }
        }

        //remove column : Input must have header (output have header)
        public static List<String> data_cropCol(List<String> data, string col_first, string col_last)
        {
            List<String> result = new List<String>();
            try
            {
                String[] cell;
                //---- seek First & Last -------------------------------------
                cell = TheTool.splitText(data.First(), ",");
                int i_first = 0; int i_last = cell.Count() - 1;
                int stage = 0;
                for (int i = 0; i < cell.Count(); i++)
                {
                    if (stage == 0)
                    {
                        if (col_first == "" || cell[i] == col_first) { i_first = i; stage = 1; }
                    }
                    else
                    {
                        if (col_last == "") { break; }
                        else if (cell[i] == col_last) { i_last = i; break; }
                    }
                }
                //---- Data -------------------------------------
                foreach (String str in data)
                {
                    string s = "";
                    cell = TheTool.splitText(str, ",");
                    for (int i = i_first; i <= i_last; i++)
                    {
                        s += cell[i];
                        if (i != i_last) { s += ","; }
                    }
                    result.Add(s);
                }
            }
            catch { }
            //catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return result;//New Table
        }

        //=============================================================================
        //===== Data Table ============================================================

        //remove column by column name (e.g. "p_jump", "")
        public static DataTable dataTable_cropCol(DataTable dt, string col_first, string col_last)
        {
            DataTable dt2 = dt.Copy();
            string nm;
            int stage = 0;
            foreach (DataColumn column in dt.Columns)
            {
                nm = column.ColumnName;
                if (stage == 0 && (col_first == "" || nm == col_first)) { stage = 1; }
                if (stage != 1) { dt2.Columns.Remove(nm); }//Remove
                if (stage == 1 && nm == col_last) { stage = 2; }
            }
            return dt2;//New Table
        }

        //remove column by column index
        public static DataTable dataTable_cropCol(DataTable dt, int col_first, int col_last)
        {
            DataTable dt2 = dt.Copy();
            string nm;
            int i_col = 0;
            foreach (DataColumn column in dt.Columns)
            {
                nm = column.ColumnName;
                if (i_col < col_first || (i_col > col_last && col_last != 0)) { dt2.Columns.Remove(nm); }
                i_col++;
            }
            return dt2;//New Table
        }

        //for Double Table, round to Integer (x-digit)
        public static void dataTable_roundValue(DataTable dt, int digit)
        {
            foreach (DataColumn dc in dt.Columns)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dr[dc] = Math.Round(TheTool.getDouble(dr[dc].ToString()), digit);
                }
            }
        }

        //id on first coluumn (select Range)
        public static DataTable dataTable_selectRow_byId(DataTable dt, int start, int end)
        {
            DataTable dt2 = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                int i = TheTool.getInt(dr[0].ToString());
                if (i >= start && i <= end) { dt2.Rows.Add(dr.ItemArray); }
            }
            return dt2;
        }

        public static DataTable dataTable_selectFirstRow(List<DataTable> dt_list)
        {
            DataTable dt2 = dt_list.First().Clone();
            foreach (DataTable dt in dt_list) { dt2.Rows.Add(dt.Rows[0]); }
            return dt2;
        }

        public static DataTable dataTable_selectFirstRow_byId(DataTable dt)
        {
            DataTable dt2 = dt.Clone();
            dt2.Rows.Add(dt.Rows[0]);
            return dt2;
        }

        public static DataTable dataTable_selectRow_byIndex(DataTable dt, int start, int end)
        {
            DataTable dt2 = dt.Clone();
            int i = 0;
            if (end < start) { 
                int s = end; int e = start; 
                start = s; end = e; 
            }
            foreach (DataRow dr in dt.Rows)
            {
                if (i >= start && i <= end) { dt2.Rows.Add(dr.ItemArray); }
                i++;
            }
            return dt2;
        }

        //Table: row 1 = Min, row 2 = Max
        public static DataTable dataTable_getMaxMinTable(DataTable dt)
        {

            DataTable dt2 = dt.Clone();
            dt2.Rows.Add(dt2.NewRow());
            dt2.Rows.Add(dt2.NewRow());
            try
            {
                int col_i = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    double min = 0;
                    double max = 0;
                    int row_i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        double v = TheTool.getDouble(dr[dc].ToString());
                        if (row_i == 0) { min = v; max = v; }
                        else if (v < min) { min = v; }
                        else if (v > max) { max = v; }
                        row_i++;
                    }
                    dt2.Rows[0][col_i] = min;
                    dt2.Rows[1][col_i] = max;
                    col_i++;
                }
            }
            catch (Exception ex) { TheSys.showError(ex.ToString()); }
            return dt2;
        }


        public static Boolean dataTable_checkIfColumnExist(DataTable dt, string colName)
        {
            DataColumnCollection columns = dt.Columns;
            if (columns.Contains(colName)) { return true; }
            else { return false; }
        }

        public static DataTable dataTable_MinMaxNormalization(DataTable dt_data, DataTable dt_mm)
        {
            DataTable dt_normal = dt_data.Copy();
            List<String> result = new List<String>();
            try
            {
                foreach (DataColumn col in dt_normal.Columns)
                {
                    String col_name = col.ColumnName;
                    if (dataTable_checkIfColumnExist(dt_mm, col_name))
                    {
                        double min = TheTool.getDouble(dt_mm.Rows[0][col].ToString());
                        double max = TheTool.getDouble(dt_mm.Rows[1][col].ToString());
                        double range = max - min;
                        foreach (DataRow row in dt_normal.Rows)
                        {
                            if (range != 0)
                            {
                                double v = TheTool.getDouble(row[col].ToString());
                                v = (v - min) / range;
                                row[col] = v;
                            }
                            else { row[col] = 0; }
                        }
                    }
                }
            }
            catch (Exception ex) { TheSys.showError("Normal: " + ex.ToString()); }
            return dt_normal;
        }

        //discritize (0,1) MMNormalized Range to 10 Partition
        public static DataTable dataTable_discritize10Partition(DataTable dt)
        {
            DataTable dt2 = dt.Copy();
            try
            {
                foreach (DataColumn dc in dt2.Columns)
                {
                    foreach (DataRow dr in dt2.Rows)
                    {
                        double v = TheTool.getDouble(dr[dc].ToString());
                        v *= 10;
                        if (v > 10) { v = 10; }
                        else if (v < 0) { v = 0; }
                        else { v = Math.Floor(v); }
                        dr[dc] = v;
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return dt2;
        }

        //range is positive
        public static DataTable dataTable_partitize(DataTable dt, double range)
        {
            DataTable dt2 = dt.Copy();
            try
            {
                int i_c = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    int i_r = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        double v = TheTool.getDouble(dr[dc].ToString());
                        dt2.Rows[i_r][i_c] = getPartition(v, range);
                        i_r++;
                    }
                    i_c++;
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return dt2;
        }

        public static DataTable dataTable_removeCol(DataTable dt, List<string> col_list)
        {
            DataTable dt2 = dt.Copy();
            try
            {
                foreach (string col in col_list)
                {
                    if (dt.Columns.Contains(col))
                    {
                        dt2.Columns.Remove(col);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return dt2;
        }

        //sort by column 1 then 2, 3, 4, ....
        public static DataTable dataTable_fullSort(DataTable dt)
        {
            try
            {
                int col_count = 0;
                String col_name = "";
                foreach (DataColumn dc in dt.Columns)
                {
                    if (col_count > 0) { col_name += ", "; }
                    col_name += dc.ColumnName;
                    col_count++;
                }
                DataView dv = dt.DefaultView;
                dv.Sort = col_name;
                //TheSys.showError(col_name);
                return dv.ToTable();
            }
            catch (Exception ex) { TheSys.showError(ex); return dt; }
        }

        public static DataTable dataTable_sort(DataTable dt, string colName, Boolean desc)
        {
            if (desc) { dt.DefaultView.Sort = colName + " DESC"; }
            else { dt.DefaultView.Sort = colName + " ASC"; }
            return dt.DefaultView.ToTable();
        }

        public static List<String> dataTable_getColList(DataTable dt)
        {
            List<string> col_list = new List<string>();
            foreach (DataColumn dc in dt.Columns)
            {
                col_list.Add(dc.ColumnName);
            }
            return col_list;
        }

        //Select By Id, not index
        //key pose must start from 0
        public static List<DataTable> dataTable_split(DataTable dt, List<int[]> keypose_list)
        {
            List<DataTable> dt_list = new List<DataTable>();
            foreach (int[] keypose in keypose_list)
            {
                if (keypose.Count() > 1 && keypose[1] > keypose[0] && keypose[0] > 0)
                {
                    dt_list.Add(TheTool.dataTable_selectRow_byId(dt, keypose[0], keypose[1]));
                }
            }
            return dt_list;
        }

        //--------------------------------------------------------------------------

        public static int getPartition(double dividend, double range)
        {
            //double temp = ((dividend - (dividend % divisor)) / divisor);
            //return (int) temp;
            //int i = 0;
            //if (dividend > 0) { for (double v = dividend; v >= range; v -= range) { i++; } }
            //else if (dividend < 0) { for (double v = dividend; v <= range; v += range) { i--; } }
            //return i;
            return (int) Math.Round(dividend / range);
        }

        //string = header , 1 data row
        public static List<string> str_double_getListString(List<str_double> sd_list)
        {
            List<string> output = new List<string>();
            string header = "";
            string data = "";
            int i = 0;
            foreach (str_double sd in sd_list)
            {
                if (i > 0) { header += ","; data += ","; }
                header += sd.str;
                data += sd.v;
                i++;
            }
            output.Add(header);
            output.Add(data);
            return output;
        }

        //Data has 3 Column, 2 string 1 double
        public static List<string> str2_double_getListString(List<str2_double> s2d_list)
        {
            List<string> output = new List<string>();
            foreach (str2_double s2d in s2d_list)
            {
                output.Add(s2d.str1 + "," + s2d.str2 + "," + s2d.v);
            }
            return output;
        }

        public static double str_double_getDouble_byStr(List<str_double> sd_list, string str)
        {
            double d = 0;
            foreach (str_double sd in sd_list)
            {
                if (str == sd.str) { d = sd.v; break; }
            }
            return d;
        }

        public static void openFolder(string url)
        {
            try { Process.Start(url); }
            catch (Exception ex) { TheSys.showError(ex); }
        }

        static IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path)
        {
            return from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                   where Directory.GetDirectories(subdirectory).Length == 0
                   select subdirectory;
        }

        public static void sortList_StringWithNumeric(string[] list_s)
        {
            Array.Sort(list_s, new AlphanumComparatorFast());
        }

        // i, count
        public static List<int[]> listInt_countOccurance(List<int> i_list)
        {
            List<int[]> occurance_counter = new List<int[]>();
            i_list.Sort();
            int previous = 0; int counter = 0;
            int id = 0; int id_last = i_list.Count() - 1;
            foreach (int current in i_list)
            {
                if (id > 0)
                {
                    if (current != previous)
                    {
                        occurance_counter.Add(new int[] { previous, counter });
                        counter = 0;
                    }
                    if (id == id_last)
                    {
                        occurance_counter.Add(new int[] { current, counter + 1 });
                    }
                }
                previous = current;
                counter++;
                id++;
            }
            return occurance_counter;
        }

        //int [ Number of Keypose , Occurance ]
        public static int[] findMode(List<int[]> list_occurance)
        {
            int[] mode = new int[2]{ 0 , 0 };
            foreach(int[] candidate in list_occurance){
                if (candidate[1] > mode[1]) { mode = candidate; }
            }
            return mode;
        }


        //format 1-3,4,5-10
        public static List<int> getSelectRange(string txt)
        {
            List<int> output = new List<int>();
            string[] s1 = TheTool.splitText(txt, ",");
            for (int i = 0; i < s1.Count(); i++)
            {
                string[] s2 = TheTool.splitText(s1[i], "-");
                if (s2.Count() == 1)
                {
                    int v = TheTool.getInt(s2[0]);
                    if (output.Contains(v) == false)
                    {
                        output.Add(TheTool.getInt(s2[0]));
                    }
                }
                else if (s2.Count() > 1)
                {
                    int v_start = TheTool.getInt(s2[0]);
                    int v_end = TheTool.getInt(s2[1]);
                    for (int v = v_start; v <= v_end; v++)
                    {

                        if (output.Contains(v) == false)
                        {
                            output.Add(v);
                        }
                    }
                }
            }
            output.Sort();
            return output;
        }

        //line is seperate by "," and first record is header 
        public static DataTable convert_List_toDataTable(List<String> list){
            DataTable dt = new DataTable();
            int r = -1;
            foreach(String line in list){
                if(r < 0){
                    foreach(string h in splitText(line,",")){
                        dt.Columns.Add(h); 
                    }
                }
                else{
                    dt.Rows.Add();
                    int c = 0;
                    foreach (string h in splitText(line, ","))
                    {
                        dt.Rows[r][c] = h;
                        c++;
                    }
                }
                r++;
            }
            return dt;
        }

        //never used
        public static string string_cropText(string full, string start, string end)
        {
            string[] split1 = splitText(full, start);
            string[] split2 = splitText(split1[0], end);
            return split2[0];
        }

        public static double LinearRegression_Cal_Correlation(DataTable dt, string str1, string str2)
        {
            List<double> str2Value = new List<double>();
            List<double> str1Value = new List<double>();

            DataColumn str1Col = null;
            DataColumn str2Col = null;
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName == str1)
                    str1Col = column;
                if (column.ColumnName == str2)
                    str2Col = column;
            }
            foreach (DataRow row in dt.Rows)
            {
                str2Value.Add(double.Parse(row[str2Col].ToString()));
                str1Value.Add(double.Parse(row[str1Col].ToString()));
            }
            return (LinearRegression(str1Value.ToArray(), str2Value.ToArray(), 0, str2Value.Count));
        }

        public static double LinearRegression(double[] xVals, double[] yVals, int inclusiveStart, int exclusiveEnd)
        {

            double rsquared = 0f;
            double yintercept = 0f;
            double slope = 0f;

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;

            return (rsquared);
        }

        //target can be or
        public static double str2_double_getValue(List<str2_double> list, string s1, string s2)
        {
            double output = 0;
            foreach(str2_double l in list){
                if ((l.str1 == s1 && l.str2 == s2) || (l.str1 == s2 && l.str2 == s1))
                { 
                    output = l.v; 
                    break; 
                }
            }
            return output;
        }

        //[0] = id
        public static int[] listArr_getValueById( List<int[]> list_arr, int id)
        {
            foreach(int[] a in list_arr){
                if (a[0] == id) { return a; }
            }
            TheSys.showError("not found " + id);
            return new int[2];//assume default size = 2
        }

        public static string getString_fromList(List<int> i_list, string join_str)
        {
            string txt = "";
            if (i_list.Count > 0)
            {
                int c = 0;
                foreach (int i in i_list)
                {
                    if (c > 0)
                    {
                        txt += join_str;
                    }
                    txt += i;
                    c++;
                }
            }
            return txt;
        }

        public static string getString_fromList(List<string> i_list, string join_str)
        {
            string txt = "";
            if (i_list.Count > 0)
            {
                int c = 0;
                foreach (string i in i_list)
                {
                    if (c > 0)
                    {
                        txt += join_str;
                    }
                    txt += i;
                    c++;
                }
            }
            return txt;
        }

        //calSpherical() for double[] is exist
        public static double calSpherical(double[] target, double[] origin, Boolean Azimuthal)
        {
            double output = 0;
            try
            {
                if (target != origin)
                {
                    double x = target[0] - origin[0];
                    double y = target[1] - origin[1];
                    double z = target[2] - origin[2];
                    double r = Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2);
                    r = Math.Sqrt(r);
                    if (Azimuthal)
                    {
                        output = Math.Atan(x / z);
                        output = TheTool.RadianToDegree(output);
                    }
                    else
                    {
                        output = Math.Acos(y / r);
                        output = TheTool.RadianToDegree(output);
                    }
                }
            }
            catch (Exception ex) { TheSys.showError(ex); }
            return output;
        }

        //---------------------------------------

        //0 = rotate, 1 = tilt
        static public UKI_DataRaw rotateSkel(UKI_DataRaw skel, int angle, Boolean rotate_or_tilt)
        {
            UKI_DataRaw new_skel = new UKI_DataRaw();
            new_skel.Head = RotatePoint(skel.Head, skel.Spine, angle, rotate_or_tilt);
            new_skel.ShoulderCenter = RotatePoint(skel.ShoulderCenter, skel.Spine, angle, rotate_or_tilt);
            new_skel.ShoulderLeft = RotatePoint(skel.ShoulderLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.ElbowLeft = RotatePoint(skel.ElbowLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.WristLeft = RotatePoint(skel.WristLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.HandLeft = RotatePoint(skel.HandLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.ShoulderRight = RotatePoint(skel.ShoulderRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.ElbowRight = RotatePoint(skel.ElbowRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.WristRight = RotatePoint(skel.WristRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.HandRight = RotatePoint(skel.HandRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.Spine = RotatePoint(skel.Spine, skel.Spine, angle, rotate_or_tilt);
            new_skel.HipCenter = RotatePoint(skel.HipCenter, skel.Spine, angle, rotate_or_tilt);
            new_skel.HipLeft = RotatePoint(skel.HipLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.KneeLeft = RotatePoint(skel.KneeLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.AnkleLeft = RotatePoint(skel.AnkleLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.FootLeft = RotatePoint(skel.FootLeft, skel.Spine, angle, rotate_or_tilt);
            new_skel.HipRight = RotatePoint(skel.HipRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.KneeRight = RotatePoint(skel.KneeRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.AnkleRight = RotatePoint(skel.AnkleRight, skel.Spine, angle, rotate_or_tilt);
            new_skel.FootRight = RotatePoint(skel.FootRight, skel.Spine, angle, rotate_or_tilt);
            return new_skel;
        }

        //http://stackoverflow.com/questions/14607640/rotating-a-vector-in-3d-space
        //true = rotate_or_tilt
        static public double[] RotatePoint(double[] data_3d, double[] data_origin, int angle, Boolean isTilt_notRotate)
        {
            double radian = angle * Math.PI / 180;
            double[] p = new double[3];
            if (data_3d == data_origin)
            {
                p[0] = data_3d[0]; p[1] = data_3d[1]; p[2] = data_3d[2];
            }
            else if (isTilt_notRotate)//tilt
            {
                double x_delta = data_3d[0] - data_origin[0];
                double y_delta = data_3d[1] - data_origin[1];
                double z_delta = data_3d[2] - data_origin[2];
                p[0] = data_3d[0];
                p[1] = data_origin[1] + y_delta * Math.Cos(radian) - z_delta * Math.Sin(radian);
                p[2] = data_origin[2] + y_delta * Math.Sin(radian) + z_delta * Math.Cos(radian);
            }
            else //rotate
            {
                double x_delta = data_3d[0] - data_origin[0];
                double y_delta = data_3d[1] - data_origin[1];
                double z_delta = data_3d[2] - data_origin[2];
                p[0] = data_origin[0] + x_delta * Math.Cos(radian) + z_delta * Math.Sin(radian);
                p[1] = data_3d[1];
                p[2] = data_origin[2] - x_delta * Math.Sin(radian) + z_delta * Math.Cos(radian);
            }
            return p;
        }

        public static double getRatio(double v, double min, double max){
            return (v - min) / (max - min);
        }

        public static string printTxt(List<double> list_data, string txt_join)
        {
            string output = "";int i = 0;
            foreach(double d in list_data){
                if (i > 0) { output += txt_join; }
                output += d;
                i++;
            }
            return output;
        }

        public static void list_initialize(List<double> list, int size)
        {
            for (int i = 0; i < size; i++) { list.Add(0); }
        }

        public static List<int[]> listArray_sort(List<int[]> list, int col_id)
        {
            return list.OrderBy(x => x[col_id]).ToList();
        }

        public static double divide(double dividend,double divisor){
            if (divisor == 0) { return 0; }
            else { return dividend / divisor; }
        }

        //http://stackoverflow.com/questions/4642687/given-start-point-angles-in-each-rotational-axis-and-a-direction-calculate-end
        public static double[] getPosition_originAngleDistance(double[] origin, double distance, double angleZ, double angleY)
        {
            angleZ = angleZ * Math.PI / 180;//deg to Radian
            angleY = angleY * Math.PI / 180;//deg to Radian
            double[] output = new double[3];
            output[0] = origin[0] + distance * Math.Cos(angleY) * Math.Sin(angleZ);
            output[1] = origin[1] + distance * Math.Sin(angleY);
            output[2] = origin[2] + distance * Math.Cos(angleY) * Math.Sin(angleZ);
            return output;
        }

        public static double getNorm(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a,2) + Math.Pow(b,2));
        }

        ////Row x Col
        //public static double[][] matrix_multiplication_NxN(double[][] M1, double[][] M2, int n)
        //{
        //    double[][] output = new double[n][];
        //    for (int i = 0; i < n; i++)
        //    {
        //        output[i][] = new double[]{ M1[0][0] *  M2[0][0], M1[0][0] *  M2[0][0]};

        //    }
        //    return output;
        //}
        public static void Matrix_print(double[,] M, int round)
        {
            for(int i = 0; i < M.GetLength(0);i++){
                for(int j = 0; j < M.GetLength(1);j++){
                    TheSys.showError(Math.Round(M[i,j],round) + "_",false);
                }
                TheSys.showError("");
            }
            TheSys.showError("");
        }

        public static double[,] Matrix_Multiply(double[,] A, double[,] B)
        {
            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);
            double temp = 0;
            double[,] kHasil = new double[rA, cB];
            if (cA != rB)
            {
                Console.WriteLine("matrik can't be multiplied !!");
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += A[i, k] * B[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
            }
            return kHasil;
        }

    }

}
