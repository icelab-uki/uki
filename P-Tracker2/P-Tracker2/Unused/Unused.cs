// To Keep Unused Code


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    class Unused
    {

        ///<summary>
        ///Smoothing 0.5
        ///Higher values correspond to more smoothing and a value of 0 causes the raw data to be returned. 
        ///Increasing smoothing tends to increase latency. Values must be in the range [0, 1.0].
        /// ---------------
        ///Correction 0.5
        ///Lower values are slower to correct towards the raw data and appear smoother, 
        ///while higher values correct toward the raw data more quickly. 
        ///Values must be in the range [0, 1.0].
        ///---------------
        ///Prediction	Specifies the number of predicted frames.
        ///---------------
        ///Jitter Radius	0.05
        ///The default value of 0.05 represents 5cm. 
        ///Any jitter beyond the radius is clamped to the radius.
        ///---------------
        ///Maximum Deviation Radius	0.04
        ///Specifies the maximum radius that filter positions can deviate from raw data, in meters.
        ///</summary>
        ///
//        //DataTable table1 = null;
//        void createTable()
//        {
//            try
//            {
//                //table1 = new DataTable();
//                //table1.Columns.Add("id", typeof(string));
//                //table1.Columns.Add("name", typeof(string));
//                //table1.Rows.Add("1", "test");
//                //table1.Rows.Add("1", "test");
//                //table1.Rows.Add("1", "test");
//                //table1.AcceptChanges();

//                //show.ItemsSource = table1.AsDataView();

//                //txtStatus.Content = table1.Rows.Count.ToString();

//                //show = table1.DefaultView();
//                //show.Items.Clear();
//                //show.ItemsSource = table1.AsDataView();//DataGrid.ItemsSource = DataTable;

//                //------------------------------
//                //show.ItemsSource = loadCollectionData();
//                show.Items.Clear();
//                show.ItemsSource = listJointString;
//            }
//            catch (Exception ex) { MessageBox.Show(ex.Message); }
//        }



//        private void kinectStart()
//        {
//            //// Create the drawing group we'll use for drawing
//            //this.drawingGroup = new DrawingGroup();

//            //// Create an image source that we can use in our image control
//            //this.imageSource = new DrawingImage(this.drawingGroup);

//            //// Display the drawing using our image control
//            //Image.Source = this.imageSource;


//            //-------- Start Sensor -------------------------------

//            // Look through all sensors and start the first connected one.
//            foreach (var potentialSensor in KinectSensor.KinectSensors)
//            {
//                if (potentialSensor.Status == KinectStatus.Connected)
//                {
//                    this.sensor = potentialSensor;
//                    break;
//                }
//            }

//            //if (null != this.sensor)
//            //{
//            //    // Turn on the skeleton stream to receive skeleton frames
//            //    this.sensor.SkeletonStream.Enable();

//            //    // Add an event handler to be called whenever there is new color frame data
//            //    this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

//            //    // Start the sensor!
//            //    try
//            //    {
//            //        this.sensor.Start();
//            //    }
//            //    catch (IOException)
//            //    {
//            //        this.sensor = null;
//            //    }
//            //}

//            //if (null == this.sensor)
//            //{
//            //    this.txtStatus.Content = "Status: Off";
//            //}
//            //else { this.txtStatus.Content = "Status: Normal Mode"; }

//            createTable();
//            bTrack.Content = "Stop";
//        }

//        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
//        {
//            if (skeletons.Length != 0)
//            {
//                foreach (Skeleton skel in skeletons)
//                {
//                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
//                    {
//                        //this.DrawBonesAndJoints(skel, dc);
//                    }
//                    else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
//                    {

//                    }
//                }
//            }
//        }

//    }

//    public void addJointData()
//    {
//        try
//        {
//            //listJointString.Add(new JointString() { ID = "A" });

//            Joint newJoint = new Joint()
//            {
//                ID = "A",
//                Head = new float[4] { 1, 1, 1, 1 }
//            };
//            listJoint.Add(newJoint);
//            //
//            JointString newJointString = newJoint.getJointString();
//            listJointString.Add(newJointString);
//        }
        
//        //------ Export Excel
//        //List for write each Excel row 
//        static public List<string> getStringList(Person p)
//        {
//            List<string> list = new List<string>();
//            list.Add(p.ID);
//            for (int i = 0; i < 4; i++) { list.Add(p.Head[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.Neck[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.ShoulderLeft[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.ElbowLeft[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.HandLeft[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.ShoulderRight[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.ElbowRight[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.HandRight[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.Torso[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.HipLeft[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.KneeLeft[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.FootLeft[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.HipRight[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.KneeRight[i].ToString()); }
//            for (int i = 0; i < 4; i++) { list.Add(p.FootRight[i].ToString()); }
//            return list;
//        }catch (Exception ex) { }
//    }


//static public void exportExcel(String theName, List<Person> listJoint)
//        {
//            try
//            {
//                //-----------------  Prepare  ---------------------------------
//                // creating Excel Application
//                Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
//                // creating new WorkBook within Excel application
//                Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
//                // creating new Excelsheet in workbook
//                Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
//                // see the excel sheet behind the program
//                app.Visible = true;
//                // get the reference of first sheet. By default its name is Sheet1.
//                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.Sheets["Sheet1"];
//                worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.ActiveSheet;
//                // changing the name of active sheet
//                worksheet.Name = theName;
//                //----------------- Writing ---------------------------------
//                // storing header part in Excel
//                int excel_row = 1;//initial at Row....
//                int excel_col = 1;

//                //Header
//                worksheet.Cells[excel_row, excel_col] = "ID"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "Head"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "Neck"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "ShoulderLeft"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "ElbowLeft"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "HandLeft"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "ShoulderRight"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "ElbowRight"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "HandRight"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "Torso"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "HipLeft"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "KneeLeft"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "FootLeft"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "HipRight"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "KneeRight"; excel_col++;
//                worksheet.Cells[excel_row, excel_col] = "FootRight"; excel_col++;
//                excel_row++;

//                //Data
//                foreach (Person p in listJoint)
//                {
//                    //excel_col = 1;
//                    //List<string> rowData = getStringList(p);
//                    //foreach (string txt in rowData)
//                    //{
//                    //    worksheet.Cells[excel_row, excel_col] = txt;
//                    //    excel_col++;
//                    //}
//                    //excel_row++;
//                    excel_col = 1;
//                    worksheet.Cells[excel_row, excel_col] = p.ID;
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.Head[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.Neck[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.ShoulderLeft[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.ElbowLeft[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.HandLeft[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.ShoulderRight[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.ElbowRight[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.HandRight[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.Torso[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.HipLeft[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.KneeLeft[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.FootLeft[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.HipRight[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.KneeRight[i].ToString(); }
//                    excel_col++; for (int i = 0; i < 4; i++) { worksheet.Cells[excel_row, excel_col] = p.FootRight[i].ToString(); }
//                    excel_row++;
//                }

//                //----------------------------- Modify format ------------------------------------
//                worksheet.Columns.AutoFit();
//                //worksheet.Columns.HorizontalAlignment = HorizontalAlignment.Left;
//                worksheet.Rows.RowHeight = 15;
//                //worksheet.Rows.AutoFit();


//                try
//                {
//                    worksheet.Cells.Font.Color = Color.FromArgb(255, 255, 255);
//                    worksheet.Cells.Interior.Color = Color.FromArgb(0, 0, 0);
//                }
//                catch (Exception ex) { };

//                // save the application
//                //workbook.SaveAs("c:\\output.xlsx",Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive , Type.Missing, Type.Missing, Type.Missing, Type.Missing);

//                // Exit from the application
//                //app.Quit();
//            }
//            catch (Exception e) { };
//        }




////No Tracked Skel, use 0 Data
//        static public Person noTrack(string ID0)
//        {
//            return new Person()
//            {
//                ID = ID0,
//                Head = new double[4] { 0, 0, 0, 0 },
//                Neck = new double[4] { 0, 0, 0, 0 },
//                ShoulderLeft = new double[4] { 0, 0, 0, 0 },
//                ElbowLeft = new double[4] { 0, 0, 0, 0 },
//                HandLeft = new double[4] { 0, 0, 0, 0 },
//                ShoulderRight = new double[4] { 0, 0, 0, 0 },
//                ElbowRight = new double[4] { 0, 0, 0, 0 },
//                HandRight = new double[4] { 0, 0, 0, 0 },

//                Torso = new double[4] { 0, 0, 0, 0 },
//                HipLeft = new double[4] { 0, 0, 0, 0 },
//                KneeLeft = new double[4] { 0, 0, 0, 0 },
//                FootLeft = new double[4] { 0, 0, 0, 0 },
//                HipRight = new double[4] { 0, 0, 0, 0 },
//                KneeRight = new double[4] { 0, 0, 0, 0 },
//                FootRight = new double[4] { 0, 0, 0, 0 },
//            };


        //static public void exportExcel(List<Person> listJoint, Boolean mode_seat, String txtTime, String txtTotalTime, String txtTotalRow)
        //{
        //    try
        //    {
        //        //---------------- Body -------------------------
        //        //Column Header
        //        worksheet.Cells[excel_row, excel_col] = "ID"; excel_col++;
        //        //String[] header = new String[] { "Head", "ShoulderCenter"
        //        //    , "ShoulderLeft", "ElbowLeft", "WristLeft", "HandLeft"
        //        //    , "AnkleRight", "ElbowRight", "WristRight", "HandRight"
        //        //    , "Spine", "HipCenter"
        //        //    , "HipLeft", "KneeLeft", "AnkleLeft", "FootLeft"
        //        //    , "HipRight", "KneeRight", "AnkleRight", "FootRight"};
        //        addHeader(header);
        //        excel_row++;
        //        //Data
        //        foreach (Person p in listJoint)
        //        {
        //            excel_col = 1;
        //            worksheet.Cells[excel_row, excel_col] = p.ID;
        //            //-------------
        //            //double[][] data = new double[][] {p.Head,p.ShoulderCenter
        //            //    ,p.ShoulderLeft,p.ElbowLeft,p.WristLeft,p.HandLeft
        //            //    ,p.ShoulderRight,p.ElbowRight,p.WristRight,p.HandRight
        //            //    ,p.Spine,p.HipCenter
        //            //    ,p.HipLeft,p.KneeLeft,p.AnkleLeft,p.FootLeft
        //            //    ,p.HipRight,p.KneeRight,p.AnkleRight,p.FootRight                        
        //            //};
        //            addData(data);
        //            excel_row++;
        //        }
        //}


        //void InitializeFaceDraw()
        //{
        //    ////------------ ERROR
        //    //var faceTrackingViewerBinding = new Binding("Kinect") { Source = this.sensor };
        //    //faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
        //    //try
        //    //{
        //    //    this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
        //    //    this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
        //    //    try
        //    //    { // This will throw on non Kinect For Windows devices.
        //    //        this.sensor.DepthStream.Range = DepthRange.Near;
        //    //        this.sensor.SkeletonStream.EnableTrackingInNearRange = true;
        //    //    }
        //    //    catch {}
        //    //    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
        //    //    this.sensor.SkeletonStream.Enable();
        //    //}
        //    //catch { }

        //    //----------------
        //}



        //=========================================================================
        //================= Person Tool ====================================

        //static public PersonString getPersonString(Person thePerson)
        //{
        //    PersonString jointString = new PersonString()
        //    {
        //        //=================== If Null Possible =====================
        //        TimeTrack = getStringfillNull(thePerson.TimeTrack),
        //        ID = getStringfillNull(thePerson.ID),
        //        //-------------------
        //        Head = getStringfillNull(thePerson.Head),
        //        ShoulderCenter = getStringfillNull(thePerson.ShoulderCenter),
        //        //--------
        //        ShoulderLeft = getStringfillNull(thePerson.ShoulderLeft),
        //        ElbowLeft = getStringfillNull(thePerson.ElbowLeft),
        //        WristLeft = getStringfillNull(thePerson.WristLeft),
        //        HandLeft = getStringfillNull(thePerson.HandLeft),
        //        //--------
        //        ShoulderRight = getStringfillNull(thePerson.ShoulderRight),
        //        ElbowRight = getStringfillNull(thePerson.ElbowRight),
        //        WristRight = getStringfillNull(thePerson.WristRight),
        //        HandRight = getStringfillNull(thePerson.HandRight),
        //        //-------------------
        //        Spine = getStringfillNull(thePerson.Spine),
        //        HipCenter = getStringfillNull(thePerson.HipCenter),
        //        HipLeft = getStringfillNull(thePerson.HipLeft),
        //        KneeLeft = getStringfillNull(thePerson.KneeLeft),
        //        AnkleLeft = getStringfillNull(thePerson.AnkleLeft),
        //        FootLeft = getStringfillNull(thePerson.FootLeft),
        //        //--------
        //        HipRight = getStringfillNull(thePerson.HipRight),
        //        KneeRight = getStringfillNull(thePerson.KneeRight),
        //        AnkleRight = getStringfillNull(thePerson.AnkleRight),
        //        FootRight = getStringfillNull(thePerson.FootRight)

        //    };
        //    return jointString;
        //}

        //static public string getStringfillNull(string a)
        //{
        //    if (a != null) { return a; } else { return "0"; }
        //}

        //static public string getStringfillNull(double[] a)
        //{
        //    if (a != null) { return string.Join(" ", a); } else { return "0"; }
        //}



        //static public PersonString getPersonString(Person thePerson)
        //{
        //    //=================== No Null Possible =====================
        //    PersonString jointString = new PersonString();
        //    jointString.TimeTrack = thePerson.TimeTrack;
        //    jointString.ID = thePerson.ID;
        //    //-------------------
        //    jointString.Head = getString(thePerson.Head);
        //    jointString.ShoulderCenter = getString(thePerson.ShoulderCenter);
        //    jointString.ShoulderLeft = getString(thePerson.ShoulderLeft);
        //    jointString.ElbowLeft = getString(thePerson.ElbowLeft);
        //    jointString.WristLeft = getString(thePerson.WristLeft);
        //    jointString.HandLeft = getString(thePerson.HandLeft);
        //    //--------
        //    jointString.ShoulderRight = getString(thePerson.ShoulderRight);
        //    jointString.ElbowRight = getString(thePerson.ElbowRight);
        //    jointString.WristRight = getString(thePerson.WristRight);
        //    jointString.HandRight = getString(thePerson.HandRight);
        //    //-------------------
        //    jointString.Spine = getString(thePerson.Spine);
        //    jointString.HipCenter = getString(thePerson.HipCenter);
        //    jointString.HipLeft = getString(thePerson.HipLeft);
        //    jointString.KneeLeft = getString(thePerson.KneeLeft);
        //    jointString.AnkleLeft = getString(thePerson.AnkleLeft);
        //    jointString.FootLeft = getString(thePerson.FootLeft);
        //    jointString.HipRight = getString(thePerson.HipRight);
        //    jointString.KneeRight = getString(thePerson.KneeRight);
        //    jointString.AnkleRight = getString(thePerson.AnkleRight);
        //    jointString.FootRight = getString(thePerson.FootRight);
        //    return jointString;
        //}


        //=============================================================================================
        //===================== Prt Scrn : Print Screen ===================================================

        ////For window Form
        //public void CaptureScreen(double x, double y, double width, double height)
        //{
        //    int ix, iy, iw, ih;
        //    ix = Convert.ToInt32(x);
        //    iy = Convert.ToInt32(y);
        //    iw = Convert.ToInt32(width);
        //    ih = Convert.ToInt32(height);
        //    Bitmap image = new Bitmap(iw, ih,
        //           System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    Graphics g = Graphics.FromImage(image);
        //    g.CopyFromScreen(ix, iy, ix, iy,
        //             new System.Drawing.Size(iw, ih),
        //             CopyPixelOperation.SourceCopy);
        //    SaveFileDialog dlg = new SaveFileDialog();
        //    dlg.DefaultExt = "png";
        //    dlg.Filter = "Png Files|*.png";
        //    DialogResult res = dlg.ShowDialog();
        //    if (res == System.Windows.Forms.DialogResult.OK)
        //        image.Save(dlg.FileName, ImageFormat.Png);
        //}

        ////For window Form
        //private static Bitmap bmpScreenshot;
        //private static Graphics gfxScreenshot;        
        //static public void snapShot(string file_name)
        //{
        //    bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
        //    gfxScreenshot = Graphics.FromImage(bmpScreenshot);
        //    gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size,CopyPixelOperation.SourceCopy);
        //    bmpScreenshot.Save("C:\\ThaiCreate\\Capture.jpg", ImageFormat.Jpeg);
        //    gfxScreenshot.Dispose();
        //}


        //static public void snapShot(string file_name, Image image)
        //{
        //    try
        //    {
        //        string path = @"file\" + file_name + ".bmp";
        //        image.Save(path);  
        //    }
        //    catch { }
        //}

        //static public void snapShot(string file_name, BitmapSource image)
        //{
        //    try
        //    {
        //        //string path = @"file\" + file_name + ".tif";
        //        System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
        //        var encoder = new PngBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(image));
        //        using (var stream = dialog.OpenFile())
        //            encoder.Save(stream);
        //    }
        //    catch { }
        //}

        //static public void snapShot(string file_name, Canvas workspace)
        //{
        //    try
        //    {
        //        string path = @"file\" + file_name + ".tif";
        //        FileStream fs = new FileStream(path, FileMode.Create);
        //        RenderTargetBitmap bmp = new RenderTargetBitmap((int)workspace.ActualWidth,
        //            (int)workspace.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);
        //        bmp.Render(workspace);
        //        BitmapEncoder encoder = new TiffBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(bmp));
        //        encoder.Save(fs);
        //        fs.Close();
        //    }
        //    catch { }
        //}


        ////Work if Index of skeleton is stable
        //Skeleton getPersonMain_ifExist(Skeleton[] skeletons)
        //{
        //    try
        //    {
        //        Skeleton mainPerson = null;
        //        if (person_index_follow > 0)
        //        {
        //            if (checkPersonExist(skeletons[person_index - 1]) == true)
        //            {
        //                mainPerson = skeletons[person_index - 1];
        //                person_detected = true;
        //            }
        //        }
        //        return mainPerson;
        //    }
        //    catch { return null; txtErr.Content = "Error: on following Main Person"; }
        //}


        //private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        //{

        //    KinectSensor newSensor = kinectChangedEventArgs.NewSensor;
        //    if (newSensor != null)
        //    {
        //        try
        //        {
        //            newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
        //            newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
        //            //try
        //            //{ // This will throw on non Kinect For Windows devices.
        //            //    newSensor.DepthStream.Range = DepthRange.Near;
        //            //    newSensor.SkeletonStream.EnableTrackingInNearRange = true;
        //            //}
        //            //catch (InvalidOperationException)
        //            //{
        //            //    newSensor.DepthStream.Range = DepthRange.Default;
        //            //    newSensor.SkeletonStream.EnableTrackingInNearRange = false;
        //            //}
        //            //newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
        //            //newSensor.SkeletonStream.Enable();
        //            ////---------
        //            //checkSeat.IsChecked = true;
        //            //checkClose.IsChecked = true;
        //        }
        //        catch { }
        //    }
        //}

        //From TheTool
        //static double math_pi_180 = 180 / Math.PI;
        //public static double cal_Pitch(double FirstXPos, double FirstYPos, double FirstZPos, double SecXPos, double SecYPos, double SecZPos)
        //{
        //    double PitchAngle = 0;
        //    double r = 0;
        //    double XDifference, YDifference, ZDifference = 0;
        //    double DifferenceSquared = 0;

        //    XDifference = FirstXPos - SecXPos;//Calculates distance from Points 
        //    YDifference = FirstYPos - SecYPos;
        //    ZDifference = FirstZPos - SecZPos;

        //    DifferenceSquared = Math.Pow(XDifference, 2) + Math.Pow(YDifference, 2) + Math.Pow(ZDifference, 2);

        //    r = Math.Sqrt(DifferenceSquared);

        //    PitchAngle = (Math.Acos(ZDifference / r));

        //    //PitchAngle = ((PitchAngle * 180 / Math.PI) - 90) * -1; //Converts to Degrees as easier to recognise visually 
        //    PitchAngle = ((PitchAngle * math_pi_180) - 90) * -1;
        //    return PitchAngle;
        //}



        //void subTable_calAllcolumn(int fileID, DataTable dt)
        //{
        //    try
        //    {
        //        //txtErr.Content = dt.Rows.Count + "||" + dt.Columns.Count;
        //        //build Column First
        //        columnAdd_forJoints(dt, "_R");
        //        columnAdd_forJoints(dt, "_RZ");
        //        columnAdd_forJoints(dt, "_Eu");
        //        //
        //        double r; double mean;
        //        double variance; double sd;
        //        List<double> list_R; List<double> list_RZ;
        //        //
        //        double r0; double rz;
        //        double rz_min = 999; double rz_max = -999;
        //        double rz_swing;
        //        //
        //        int rowCount = dt.Rows.Count;
        //        foreach (string joint in joint_list)//Column first method
        //        {
        //            //----------------------------------------------------
        //            //------ Cal AvgC -----------------------------
        //            double avg_C = TheTool_Stat.calAvg_byCol(dt, joint + "_c", 4);
        //            dataTable.Rows[fileID][joint + "_AvgC"] = avg_C;
        //            //----------------------------------------------------
        //            //------ Cal R -----------------------------
        //            list_R = new List<double> { };
        //            for (int row = 0; row < rowCount; row++)
        //            {
        //                r = subTable_calEach_column(dt, joint, row);
        //                list_R.Add(r);
        //                dt.Rows[row][joint + "_R"] = r.ToString();
        //            }
        //            //------------------------------------------------------
        //            mean = TheTool_Stat.calMean(list_R);
        //            variance = TheTool_Stat.calVariance(list_R, mean);
        //            sd = Math.Sqrt(variance);
        //            //-----------
        //            dataTable.Rows[fileID][joint + "_V"] = Math.Round(variance, 4).ToString();
        //            //------ Cal RZ -----------------------------
        //            //Cal Z
        //            list_RZ = new List<double> { };
        //            for (int row = 0; row < rowCount; row++)
        //            {
        //                r0 = double.Parse(dt.Rows[row][joint + "_R"].ToString());
        //                rz = TheTool_Stat.calZScore(r0, mean, sd, 4);
        //                list_R.Add(rz);
        //                dt.Rows[row][joint + "_RZ"] = rz.ToString();
        //                if (r0 > rz_max) { rz_max = r0; }
        //                if (r0 < rz_min) { rz_min = r0; }
        //            }
        //            rz_swing = rz_max - rz_min;
        //            dataTable.Rows[fileID][joint + "_Swing"] = Math.Round(rz_swing, 4).ToString();
        //            //------------------------------------------------------
        //        }

        //    }
        //    catch { }
        //}


        //--------- [ver.131030] Using Norm ---------------
        //void subTable_calAllcolumn(int fileID, DataTable dt)
        //{
        //    try
        //    {
        //        //txtErr.Content = dt.Rows.Count + "||" + dt.Columns.Count;
        //        //build Column First
        //        string col_Norm = "_R";
        //        columnAdd_forJoints(dt, col_Norm);
        //        string col_Dist = "_D";
        //        columnAdd_forJoints(dt, col_Dist);
        //        //string col_DistZ = "_DZ";
        //        //columnAdd_forJoints(dt, col_DistZ);
        //        //------ For R (Norm)
        //        List<double> list_R;
        //        double r;
        //        double mean; double variance;
        //        //double sd;
        //        double r_swing;
        //        //------ For Distance
        //        List<double> list_D;
        //        double dist;
        //        double d_mean; double d_sd;
        //        double d_variance;
        //        //------ For Distance Z-Score
        //        double d_p80;
        //        //List<double> list_DZ;
        //        //double dist0; double distZ;
        //        //--------------
        //        //List<double> list_RZ;
        //        //double r0; double rz;
        //        //double rz_min = 999; double rz_max = -999;
        //        //double rz_swing;
        //        //--------------
        //        int rowCount = dt.Rows.Count;
        //        foreach (string joint in joint_list)//Column first method
        //        {
        //            dist = 999;
        //            list_R = new List<double> { };
        //            list_D = new List<double> { };
        //            //list_DZ = new List<double> { };
        //            //-------------------------------------------
        //            //------ Cal AvgC -----------------------------
        //            double avg_C = TheTool_Stat.calAvg_byCol(dt, joint + "_c", 4);
        //            dataTable.Rows[fileID][joint + "_AvgC"] = avg_C;
        //            //------------------------------------------
        //            //------ Cal R -----------------------------
        //            for (int row = 0; row < rowCount; row++)
        //            {
        //                r = subTable_calEach_column(dt, joint, row);
        //                list_R.Add(r);
        //                dt.Rows[row][joint + col_Norm] = r.ToString();
        //                //--------- Eucludian ------------------
        //                if (row == 0) { dt.Rows[row][joint + col_Dist] = "0"; }
        //                else
        //                {
        //                    dist = Math.Abs(r - dist);
        //                    dt.Rows[row][joint + col_Dist] = Math.Round(dist, 4);
        //                    list_D.Add(dist);
        //                }
        //                dist = r;
        //            }
        //            r_swing = list_R.Max() - list_R.Min();
        //            dataTable.Rows[fileID][joint + "_Swing"] = Math.Round(r_swing, 4).ToString();
        //            //------------------------------------------------------
        //            mean = TheTool_Stat.calMean(list_R);
        //            variance = TheTool_Stat.calVariance(list_R, mean);
        //            //sd = Math.Sqrt(variance);
        //            //-----------
        //            d_variance = Math.Round(variance, 4);
        //            dataTable.Rows[fileID][joint + "_V"] = d_variance;
        //            d_mean = Math.Round(list_D.Average(), 4);
        //            dataTable.Rows[fileID][joint + "_AvgDist"] = d_mean.ToString();
        //            d_sd = Math.Sqrt(d_variance);
        //            dataTable.Rows[fileID][joint + "_MaxDist"] = Math.Round(list_D.Max(), 4).ToString();
        //            //------ Cal DZ -----------------------------
        //            //list_DZ = new List<double> { };
        //            //for (int row = 0; row < rowCount; row++)
        //            //{
        //            //    dist = double.Parse(dt.Rows[row][joint + col_Dist].ToString());
        //            //    distZ = TheTool_Stat.calZScore(dist, d_mean, d_sd, 4);
        //            //    list_R.Add(distZ);
        //            //    //dt.Rows[row][joint + "_RZ"] = distZ.ToString();
        //            //}
        //            //------------------------------------------------------
        //            //At 80 Percentilt
        //            d_p80 = (0.842 * d_sd) - d_mean;
        //            dataTable.Rows[fileID][joint + "_P80Dist"] = Math.Round(d_p80, 4).ToString();

        //        }

        //    }
        //    catch { }
        //}


        ////file that tell what are original name
        ////http://bytes.com/topic/c-sharp/answers/539531-how-copy-datatable
        //void exportNameCodeList()
        //{
        //    try
        //    {
        //        DataTable dt = dataTable.Copy();
        //        //---------------------------
        //        string v;
        //        foreach (DataRow r in dt.Rows)
        //        {
        //            v = r[col_path].ToString();
        //            v = TheTool.getFileName_byPath(v);
        //            r[col_path] = v;
        //        }
        //        //---------------------------
        //        dt.Columns[col_id].ColumnName = "filename";
        //        dt.Columns[col_path].ColumnName = "original";
        //        dt.Columns.Remove(col_random);
        //        TheTool.export_dataTable_to_CSV(@"file\[NameCode]\[List].csv", dt);
        //    }
        //    catch { }
        //}


        //------- Norm Theory -------------
        //double subTable_calEach_column(DataTable dt,string joint,int i)
        //{
        //    try
        //    {
        //        double x = double.Parse(dt.Rows[i][joint + "_x"].ToString());
        //        double y = double.Parse(dt.Rows[i][joint + "_y"].ToString());
        //        double z = double.Parse(dt.Rows[i][joint + "_z"].ToString());
        //        return TheTool_Stat.calNorm3D(x, y, z, 4);
        //    }
        //    catch { return 0; }
        //}

        ////=====================================================================================

        ////Automatic change to original table
        ////Length from Waist to ShoudlerCenter
        //static public double cal_Kinect_SleeveLength(DataTable dt, int digit)
        //{
        //    double bodyScale = 0;
        //    try
        //    {
        //        double should_neck;
        //        double should_elbow;
        //        double elbow_waist;
        //        //
        //        double[] point1;
        //        double[] point2;
        //        //------------
        //        List<double> d = new List<double>();
        //        for (int row = 0; row < dt.Rows.Count; row++)
        //        {
        //            point1 = getDouble_fromJoint(dt, row, "ShoulderCenter");
        //            point2 = getDouble_fromJoint(dt, row, "ShoulderLeft");
        //            should_neck = TheTool_Stat.calEuclidean(point1, point2, digit);
        //            //------------------------
        //            point1 = getDouble_fromJoint(dt, row, "ElbowLeft");
        //            should_elbow = TheTool_Stat.calEuclidean(point1, point2, digit);
        //            //------------------------
        //            point2 = getDouble_fromJoint(dt, row, "WristLeft");
        //            elbow_waist = TheTool_Stat.calEuclidean(point1, point2, digit);
        //            //------------------------
        //            bodyScale = should_neck + should_elbow + elbow_waist;
        //            break;//test 1 row
        //        }
        //    }
        //    catch (Exception e) { TheSys.showError(e.ToString(), true); }
        //    return bodyScale;
        //}


        ////Automatic change to original table
        ////Length from Waist to ShoudlerCenter
        //static public double cal_Kinect_ShoulderLength(DataTable dt, int digit)
        //{
        //    double should_length = 0;
        //    try
        //    {
        //        double[] point1;
        //        double[] point2;
        //        //------------
        //        List<double> d = new List<double>();
        //        for (int row = 0; row < dt.Rows.Count; row++)
        //        {
        //            point1 = getDouble_fromJoint(dt, row, "ShoulderRight");
        //            point2 = getDouble_fromJoint(dt, row, "ShoulderLeft");
        //            d.Add(TheTool_Stat.calEuclidean(point1, point2, digit));
        //        }
        //        should_length = d.Average();
        //    }
        //    catch (Exception e) { TheSys.showError(e.ToString(), true); }
        //    return Math.Round(should_length, digit);
        //}


        //void subTable_normalize()
        //{
        //    if (normaliz_method == 1)
        //    {
        //        List<string> col_list = TheTool.getListJointXYZ();
        //        TheTool_Stat.normalize_table_MinMax(this.sub_table, col_list, decimalNum);
        //    }
        //    else if (normaliz_method == 2)
        //    {
        //        double bodyScale = TheTool.cal_Kinect_SleeveLength(this.sub_table, decimalNum);
        //        List<string> col_list = TheTool.getListJointXYZ();
        //        TheTool_Stat.normalize_table_byValue(this.sub_table, col_list, bodyScale, decimalNum);
        //        //TheSys.showError("test: " + bodyScale.ToString());
        //    }
        //    else if (normaliz_method == 3)
        //    {
        //        double shouldLength = TheTool.cal_Kinect_ShoulderLength(this.sub_table, decimalNum);
        //        List<string> col_list = TheTool.getListJointXYZ();
        //        TheTool_Stat.normalize_table_byValue(this.sub_table, col_list, shouldLength, decimalNum);
        //        TheSys.showError("test: " + shouldLength.ToString(), true);
        //    }
        //}
        ////=====================================================================================

        //PersonD(){
        //    //public string time = "";
        ////public int range_base = 0;
        ////public int range_cur = 0;
        ////public int facing_deg_base = 0;
        ////public int facing_deg_cur = 0;
        ////public int pitch_angle= 0;
        ////public int pitch_angleBase= 0;
        ////public int roll_angle= 0;
        ////public int roll_angleBase= 0;
        ////public int shouldRL_balance= 0;
        ////public int shouldRL_balanceBase= 0;
        ////public int bodyBend_angle= 0;
        ////public int bodyBend_angleBase= 0;

        //}

        //public PersonD getPersonDetect()
        //{
        //    PersonD personD = new PersonD();
        //    //personD.time = thisTimeString;
        //    //personD.range_base = this.range_base;
        //    //personD.range_cur = this.range_cur;
        //    //personD.facing_deg_base = this.facing_deg_base;
        //    //personD.facing_deg_cur = this.facing_deg_cur;
        //    //personD.pitch_angle = this.pitch_angle;
        //    //personD.pitch_angleBase = this.pitch_angleBase;
        //    //personD.roll_angle = this.roll_angle;
        //    //personD.roll_angleBase = this.roll_angleBase;
        //    //personD.shouldRL_balance = this.shouldRL_balance;
        //    //personD.shouldRL_balanceBase = this.shouldRL_balanceBase;
        //    //personD.bodyBend_angle = this.bodyBend_angle;
        //    //personD.bodyBend_angleBase = this.bodyBend_angleBase;
        //    //--------------------------------
        //}


        //Bend + Roll ==============================================
        //-------------
        //if (roll_angle >= 8)
        //{
        //    roll_agree++;
        //    if (roll_agree > total_agree_consensus) { txt += ", Roll-R"; }
        //}
        //else if (roll_angle <= -8)
        //{
        //    roll_agree--;
        //    if (roll_agree < -total_agree_consensus) { txt += ", Roll-L"; }
        //}
        //else { roll_agree = 0; }


        ////------------- BEND ----------------------
        //if (bodyBend_angle >= bend_sensitive)
        //{
        //    bend_agree++;
        //    if (bend_agree > total_agree_consensus) { 
        //        state += ", Bend-R"; bend_flag = 1;
        //        a_isBend = switch_n_alert(ref a_isBend, true, MicroCmd.cmd_bend_r);
        //    }
        //}
        //else if (bodyBend_angle <= -bend_sensitive)
        //{
        //    bend_agree--;
        //    if (bend_agree < -total_agree_consensus) { 
        //        state += ", Bend-L"; bend_flag = -1;
        //        a_isBend = switch_n_alert(ref a_isBend, true, MicroCmd.cmd_bend_l);
        //    }
        //}
        //else {
        //    a_isBend = switch_n_alert(ref a_isBend, false, MicroCmd.cmd_clearLCD);
        //    bend_agree = 0;
        //    bend_flag = 0;
        //    ////----------------------------------------
        //    //if (shouldRL_balance >= 5)
        //    //{
        //    //    shouldBal_agree++;
        //    //    if (shouldBal_agree > total_agree_consensus) { txt += ", Balance-R"; }
        //    //}
        //    //else if (shouldRL_balance <= -5)
        //    //{
        //    //    shouldBal_agree--;
        //    //    if (shouldBal_agree < -total_agree_consensus) { txt += ", Balance-L"; }
        //    //}
        //    //else { shouldBal_agree = 0; }
        //}



        ////If >10 min data Unsave
        //void backup_temp_UnSave()
        //{
        //    if (thisTime0 > 600) {
        //        string file_name = "temp" + startTime.ToString("_HHmm") + thisTime.ToString("_HHmm");
        //        export_csv(backup_file_name, false, false, false);
        //    }
        //}



        //// HHmmssff >> HH:mm:ss.ff
        //string getTimeFormat(string rawFormat)
        //{
        //    try
        //    {
        //        return rawFormat.Substring(0, 2)
        //            + ":" + rawFormat.Substring(2, 2)
        //            + ":" + rawFormat.Substring(4, 2);
        //    }
        //    catch { return ""; }
        //}



        //public XmlDataProvider dataProvider { get; set; }
        //public void buildTree2(String path)
        //{
        //    try
        //    {
        //        dataProvider = FindResource("mapping") as XmlDataProvider;//connect to xaml
        //        var xmlDocument = new XmlDocument();
        //        xmlDocument.Load(path);
        //        dataProvider.Document = xmlDocument;
        //    }
        //    catch (Exception ex) { TheSys.showError(ex.ToString()); }
        //}


        

        //==============================================================================================
        //==============================================================================================


        //void sendKey(){
        //    //Thread.Sleep(1000);
        //    //InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_P);
        //    //Thread.Sleep(1000);
        //    //InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_L);
        //    //InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_L); Thread.Sleep(100);
        //    //InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_L); Thread.Sleep(100);
        //    //InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_L); Thread.Sleep(100);
        //    //InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_L); Thread.Sleep(100);
        //    //---------------------------------------------------------
        //    //Thread.Sleep(1000);
        //    //TheKeySender3.KeyPress(Keys.A);
        //    //TheKeySender3.KeyDown(Keys.W);
        //    //---------------------------------------------------------
        //    //Thread.Sleep(2000);
        //    //Input input = new Input();
        //    //input.KeyboardFilterMode = KeyboardFilterMode.All;
        //    //input.Load();
        //    //////-----------
        //    ////input.MoveMouseTo(5, 5);  // Please note this doesn't use the driver to move the mouse; it uses System.Windows.Forms.Cursor.Position
        //    ////input.MoveMouseBy(25, 25); //  Same as above ^
        //    //input.SendLeftClick();
        //    //Thread.Sleep(100);
        //    //input.SendLeftClick();
        //    //Thread.Sleep(100);
        //    //input.SendLeftClick();
        //    ////Thread.Sleep(1000);
        //    ////input.SendRightClick();
        //    ////-----------
        //    //input.KeyPressDelay = 100; // See below for explanation; not necessary in non-game apps
        //    ////input.SendKeys(Interceptor.Keys.Enter);  // Presses the ENTER key down and then up (this constitutes a key press)
        //    ////input.SendKeys(Interceptor.Keys.A);
        //    ////input.SendKeys(Interceptor.Keys.B);
        //    ////input.SendKeys(Interceptor.Keys.C);
        //    ////input.SendKeys(Interceptor.Keys.D);
        //    ////input.SendKeys(Interceptor.Keys.E);
        //    ////input.SendKeys(Interceptor.Keys.F);
        //    ////input.SendKeys(Interceptor.Keys.G);
        //    ////input.SendKeys(Interceptor.Keys.H);
        //    ////input.SendKeys(Interceptor.Keys.I);
        //    ////input.SendKeys(Interceptor.Keys.J);
        //    ////input.SendKeys(Interceptor.Keys.K);

        //    //input.SendKey(Interceptor.Keys.A, KeyState.Down);
        //    ////-----------
        //    //// Or you can do the same thing above using these two lines of code
        //    ////input.SendKey(Interceptor.Keys.W, KeyState.Down);
        //    ////Thread.Sleep(100); // For use in games, be sure to sleep the thread so the game can capture all events. A lagging game cannot process input quickly, and you so you may have to adjust this to as much as 40 millisecond delay. Outside of a game, a delay of even 0 milliseconds can work (instant key presses).
        //    ////input.SendKey(Interceptor.Keys.W, KeyState.Up);

        //    ////-----------
        //    ////input.SendText("hello, I am typing!");
        //    ////input.SendText("hello, I am typing!");

        //    /////* All these following characters / numbers / symbols work */
        //    ////input.SendText("abcdefghijklmnopqrstuvwxyz");
        //    ////input.SendText("1234567890");
        //    ////input.SendText("!@#$%^&*()");
        //    ////input.SendText("[]\\;',./");
        //    ////input.SendText("{}|:\"<>?");

        //    //// And finally
        //    //input.Unload();
        //}

        //class TheKeySender
        //{

        //    //    [DllImport("user32.dll", EntryPoint = "PostMessageA")]
        //    //    static extern bool PostMessage(
        //    //        IntPtr hWnd,
        //    //        uint msg,
        //    //        int wParam,
        //    //        int lParam
        //    //        );

        //    //    const uint WM_KEYDOWN = 0x100;

        //    //    const int WM_a = 0x41;
        //    //    const int WM_b = 0x42;
        //    //    const int WM_c = 0x43;

        //    //    static public string processTarget = "Untitled - Notepad";

        //    //    static public void sendKey(Key k)
        //    //    {            
        //    //        //geekswithblogs.net/omtalsania7/archive/2013/01/10/ui-automation-automating-key-strokes-using-.net-and-win32-api.aspx
        //    //        IntPtr handle = UIAutomationHelper.FindChildWindow(null,
        //    //                             processTarget, "edit", null);
        //    //        UIAutomationHelper.PressKeys(handle, new[] { k });
        //    //        //UIAutomationHelper.PressKeys( handle, new[] { Key.H, Key.E, Key.L, Key.L, Key.O, Key.Enter });
        //    //    }
        //    //}

        //    //public class UIAutomationHelper
        //    //{
        //    //    /// Find a window specified by the window title
        //    //    public static IntPtr FindWindow(string windowName)
        //    //    {
        //    //        return Win32.FindWindow(null, windowName);
        //    //    }

        //    //    /// Find a window specified by the class name as well as the window title
        //    //    public static IntPtr FindWindow(string className, string windowName)
        //    //    {
        //    //        return Win32.FindWindow(className, windowName);
        //    //    }

        //    //    /// Finds a window specified by the window title and set focues on that window
        //    //    public static IntPtr FindWindowAndFocus(string windowName)
        //    //    {
        //    //        return UIAutomationHelper.FindWindowAndFocus(null, windowName);
        //    //    }

        //    //    /// Finds a window specified by the class name and window title and set focuses on that window
        //    //    public static IntPtr FindWindowAndFocus(string className, string windowName)
        //    //    {
        //    //        IntPtr hWindow = Win32.FindWindow(className, windowName);
        //    //        Win32.SetForegroundWindow(hWindow);
        //    //        return hWindow;
        //    //    }

        //    //    /// Finds a child window
        //    //    public static IntPtr FindChildWindow(String windowName, String childWindowName)
        //    //    {
        //    //        return UIAutomationHelper.FindChildWindow(null, windowName, null, childWindowName);
        //    //    }

        //    //    /// Finds a child window
        //    //    public static IntPtr FindChildWindow(String className, String windowName,
        //    //                                String childClassName, String childWindowName)
        //    //    {
        //    //        IntPtr hWindow = Win32.FindWindow(className, windowName);
        //    //        IntPtr hWindowEx = Win32.FindWindowEx(hWindow,
        //    //                           IntPtr.Zero, childClassName, childWindowName);
        //    //        return hWindowEx;
        //    //    }

        //    //    /// Simulates pressing a key
        //    //    public static void PressKey(IntPtr hWindow, System.Windows.Input.Key key)
        //    //    {
        //    //        Win32.PostMessage(hWindow, Win32.WM_KEYDOWN,
        //    //                          System.Windows.Input.KeyInterop.VirtualKeyFromKey(key),
        //    //                          0);
        //    //    }

        //    //    /// Simulates pressing several keys
        //    //    public static void PressKeys(IntPtr hWindow,
        //    //                       IEnumerable<System.Windows.Input.Key> keys)
        //    //    {
        //    //        foreach (var key in keys)
        //    //        {
        //    //            UIAutomationHelper.PressKey(hWindow, key);
        //    //        }
        //    //    }

        //    //}

        //    //public class Win32
        //    //{
        //    //    public const int WM_KEYDOWN = 0x100;
        //    //    public const int WM_KEYUP = 0x101;

        //    //    [DllImport("User32.dll", SetLastError = true)]
        //    //    public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        //    //    [DllImport("user32.dll", SetLastError = true)]
        //    //    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter,
        //    //                                                 string lpszClass, string lpszWindow);

        //    //    [DllImport("User32.dll", SetLastError = true)]
        //    //    public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        //    //    [return: MarshalAs(UnmanagedType.Bool)]
        //    //    [DllImport("user32.dll", SetLastError = true)]
        //    //    public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        //}


        //class TheKeySender3
        //{

        //    [DllImport("user32.dll")]
        //    static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        //    public enum KeybdEventFlag : int
        //    {
        //        KEYEVENTF_EXTENDEDKEY = 0x01,
        //        KEYEVENTF_KEYUP = 0x02,
        //    }

        //    public static void KeyDown(Keys key)
        //    {
        //        keybd_event((byte)key, 0x45, (int)KeybdEventFlag.KEYEVENTF_EXTENDEDKEY, 0);
        //    }

        //    public static void KeyUp(Keys key)
        //    {
        //        keybd_event((byte)key, 0x45, (int)KeybdEventFlag.KEYEVENTF_EXTENDEDKEY | (int)KeybdEventFlag.KEYEVENTF_KEYUP, 0);
        //    }

        //    public static void KeyPress(Keys key)
        //    {
        //        KeyDown(key);
        //        KeyUp(key);
        //    }

        //}

        //void bodylength()
        //{
        //    //user_height
        //    double user_leglong_l = TheTool.calEuclidian_2Joint(
        //        posture_current.Joints[JointType.HipLeft],
        //        posture_current.Joints[JointType.AnkleLeft]);
        //    double user_leglong_r = TheTool.calEuclidian_2Joint(
        //        posture_current.Joints[JointType.HipRight],
        //        posture_current.Joints[JointType.AnkleRight]);
        //    initial_leglength = (user_leglong_l + user_leglong_r) / 2;
        //    //------------
        //    double user_thlong_l = TheTool.calEuclidian_2Joint(
        //        posture_current.Joints[JointType.HipLeft],
        //        posture_current.Joints[JointType.KneeLeft]);
        //    double user_thlong_r = TheTool.calEuclidian_2Joint(
        //        posture_current.Joints[JointType.HipRight],
        //        posture_current.Joints[JointType.KneeRight]);
        //    initial_thighlength = (user_thlong_l + user_thlong_r) / 2;
        //    //
        //    double user_arm_l = TheTool.calEuclidian_2Joint(
        //        posture_current.Joints[JointType.ShoulderLeft],
        //        posture_current.Joints[JointType.HandLeft]);
        //    double user_arm_r = TheTool.calEuclidian_2Joint(
        //        posture_current.Joints[JointType.ShoulderRight],
        //        posture_current.Joints[JointType.HandRight]);
        //    initial_armlength = (user_arm_l + user_arm_r) / 2;
        //}


        //****************************************************************
        //*** UKI ***************************************************

        //int temp_input_step_sub = 0;
        ////Added Posture
        //Boolean map_processInput_onDatabase(m_If i, m_Detection d)
        //{
        //    Boolean pass = false;
        //    foreach (m_Motion m in list_motions)
        //    {
        //        if (m.enabled && i.v == m.name)
        //        {
        //            temp_input_step_sub = 0;
        //            foreach (m_If i2 in m.inputs)
        //            {
        //                if (temp_input_step_sub >= d.input_step_sub)
        //                {
        //                    pass = map_processInput(i2, d, true);
        //                    if (!pass) { break; }// 1 condition failed, skip the rest
        //                }
        //                temp_input_step_sub++;
        //            }
        //            break;
        //        }
        //    }
        //    return pass;
        //}





    }

}


