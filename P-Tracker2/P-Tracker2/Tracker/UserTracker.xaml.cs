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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

using System.IO;
using System.Data;
using System.Collections.ObjectModel;

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Diagnostics;
using System.Threading;


namespace P_Tracker2
{

    public partial class UserTracker : Window
    {
        public KinectSensor sensor = null;
        int kinectSelect = 1;//first camera

        DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
        ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        //----------- Table -----------------
        public DateTime startTime;//Start Table Track
        public DateTime thisTime;//Current
        string thisTimeString = "0";
        //--- for check fps
        public int thisTime0 = 0;
        public int rowTotal = 0;//thisTime0_row
        public int lastTime0 = 0;
        public int lastTime0_row = 0;

        public int backupTime_next0 = 0;//next Back up Time : backup to temp
        public int backupEvery = 300;//back up every .... sec

        //-------- Color -------------
        static SolidColorBrush color_Edge = Brushes.Yellow;
        static SolidColorBrush color_txtReady = Brushes.Blue;
        static SolidColorBrush color_SkelBG = Brushes.Black;
        //--------------
        public Pen trackedBonePen = new Pen(Brushes.SteelBlue, 6);// Pen used for drawing bones that are currently tracked
        public Brush centerPointBrush = Brushes.Yellow;// Brush used to draw skeleton center point
        public Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));// Brush used for drawing joints that are currently tracked
        public Brush inferredJointBrush = Brushes.Yellow;// Brush used for drawing joints that are currently inferred
        public Pen inferredBonePen = new Pen(Brushes.Gray, 1);// Pen used for drawing bones that are currently inferred
        public Brush HipVirtualBrush = Brushes.LawnGreen;
        //----------------------------
        public float RenderWidth = 640.0f;// Width of output drawing
        public float RenderHeight = 480.0f;// Height of our output drawing
        public double JointThickness = 3;// Thickness of drawn joint lines     
        public double BodyCenterThickness = 10;// Thickness of body center ellipse
        public double ClipBoundsThickness = 10;// Thickness of clip edge rectangles
        public double HipVirtualThickness = 5;// Thickness of body center ellipse
        //-----------------------------
        JointType joint_ShoulderCenter = JointType.ShoulderCenter;
        public TotalCounter totalCounter = new TotalCounter();

        public UserTracker()
        {
            InitializeComponent();
            loadSetting();
            statusSet();
            //---------- Initial Variable --------------
            startTime = DateTime.Now;
            thisTime = DateTime.Now;
            time_thread = new Thread(timeThread);
            time_thread.Start();
            //----------- Initial GUI -----------
            setUpGUI();
            //---------------------------------
            comboKinectSelect_reset();
            comboFollow_setup();
            setupComboStream(); comboStream.SelectedIndex = 0;
            //----
            TheTool_micro.reset(false);
            TheTool_micro.sendCmd(MicroCmd.cmd_hello);
            //if(!checkMute.IsChecked.Value){TheSound.speechPlay(TheURL.sp_welcome);}
        }


        //-------------------------------------------------
        //===== TimeThread =======
        Boolean time_thread_run = true;
        public Thread time_thread;
        public int sec_since_start = 0;

        void timeThread()
        {
            while (time_thread_run)
            {
                Thread.Sleep(1000);
                this.Dispatcher.Invoke((Action)(() => { time_checkAlert(); }));
                sec_since_start++;
            }
        }

        //-------------------------------------------------

        void setUpGUI()
        {
            initialSmoothSlider();
            SmoothingEnable(false);
            butAngle2.Content = "0°";
            //----------- set Combobox -----------
            //Set Decimal Combobox
            for (int i = 0; i <= 10; i++) { comboDecimal.Items.Add(i.ToString()); }
            comboDecimal.SelectedIndex = decimalNum;
            //
            setComboView();
            setComboExport();
            //Set Snap Combobox
            comboSnap.Items.Add("0"); comboSnap.Items.Add("3");
            comboSnap.Items.Add("5"); comboSnap.Items.Add("10");//Set Decimal Combobox
            comboSnap.SelectedIndex = 1;
            greenScrnBGSetup();
        }

        DepthImageFrame depthFrame = null;
        ColorImageFrame colorFrame = null;

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            depthFrame = e.OpenDepthImageFrame();
            colorFrame = e.OpenColorImageFrame();
            if (depthFrame != null && colorFrame != null)
            {
                //----------- Bg
                try { if (check_toDraw(1) == true) { prepareDraw_BG(); } }
                catch { TheSys.showError("Error: BG", true); }
                //----------- Depth
                try { if (check_toDraw(2) == true) { sensorFrame_Depth(); } }
                catch { TheSys.showError("Error: DepthFrame", true); }
                //----------- Color
                try { if (check_toDraw(3) == true) { sensorFrame_Color(); } }
                catch { TheSys.showError("Error: ColorFrame", true); }
                //----------- Skeleton
                try { sensorFrame_Skeleton(e); }
                catch { }
                //catch (Exception ex) { TheSys.showError("SkelFrame Error: " + ex.Message, true); }
                //-------- After Collect all Frame >> Display Data
                set_showImage();

                if (checkGreenScrn.IsChecked == true) { greenScreen(); }
            }
        }



        private void butDetector_Click(object sender, RoutedEventArgs e)
        {
            callDetector();
        }

        void callDetector()
        {
            try
            {
                if (form_Detector == null)
                {
                    form_Detector = new Detector(this);
                    form_Detector.Show();
                }
            }
            catch (Exception e) { TheSys.showError("CallD:" + e.Message, true); }
        }

        //-------------- Rendering : Event Handler : Getting Kinect Frame -------------
        //---choice of BG
        List<string> choice_bg = new List<string> { "Null", "Black", "Depth", "Color" };

        void setViewCombo_each(ComboBox each)
        {
            foreach (string choice in choice_bg)
            {
                each.Items.Add(choice);
            }
        }

        void setComboView()
        {
            setViewCombo_each(comboV1); setViewCombo_each(comboV2);
            setViewCombo_each(comboV3); setViewCombo_each(comboV4);
            comboV1.SelectedIndex = 2;
            comboV2.SelectedIndex = 3;
            comboV3.SelectedIndex = 0;
            comboV4.SelectedIndex = 0;
        }

        //Don't prepare BG frame if no one need
        Boolean check_toDraw(int index)
        {
            Boolean draw = true;
            if (comboV1.SelectedIndex != index && comboV2.SelectedIndex != index
                    && comboV3.SelectedIndex != index && comboV4.SelectedIndex != index)
            { draw = false; }
            return draw;
        }


        void set_showImage()
        {
            try
            {
                if (checkGreenScrn.IsChecked == true)
                {
                    setViewGreenScreen();
                }
                else
                {
                    setView_each(ImageKinect1_bg, ImageKinect1_Edge, ImageKinect1_Skel_Bone, ImageKinect1_Skel_Joint, comboV1, checkVEdge1, checkVBone1, checkVJoint1);
                }
                setView_each(ImageKinect2_bg, ImageKinect2_Edge, ImageKinect2_Skel_Bone, ImageKinect2_Skel_Joint, comboV2, checkVEdge2, checkVBone2, checkVJoint2);
                setView_each(ImageKinect3_bg, ImageKinect3_Edge, ImageKinect3_Skel_Bone, ImageKinect3_Skel_Joint, comboV3, checkVEdge3, checkVBone3, checkVJoint3);
                setView_each(ImageKinect4_bg, ImageKinect4_Edge, ImageKinect4_Skel_Bone, ImageKinect4_Skel_Joint, comboV4, checkVEdge4, checkVBone4, checkVJoint4);
            }
            catch { TheSys.showError("Error: ShowImg", true); }
        }


        void setViewGreenScreen()
        {
            //GreenScrn_object.Source = this.imageSource_Color;
            ImageKinect1_bg.Source = null;
            ImageKinect1_Edge.Source = null;
            setView_each_each(ImageKinect1_Skel_Bone, checkVBone1, this.imageSource_Skel_Bone);
            setView_each_each(ImageKinect1_Skel_Joint, checkVJoint1, this.imageSource_Skel_Joint);
        }


        void setView_each(Image img_bg, Image img_edge, Image img_bone, Image img_joint
            , ComboBox bg, CheckBox edge, CheckBox bone, CheckBox joint)
        {
            if (bg.SelectedIndex == 0)
            {
                img_bg.Source = null;
                img_edge.Source = null;
                img_joint.Source = null;
            }
            else
            {
                if (bg.SelectedIndex == 1)
                {
                    img_bg.Source = this.imageSource_bgColor;

                }
                else if (bg.SelectedIndex == 2)
                {
                    img_bg.Source = this.imageSource_Depth;
                }
                else if (bg.SelectedIndex == 3)
                {
                    img_bg.Source = this.imageSource_Color;
                }
                setView_each_each(img_edge, edge, this.imageSource_edge);
                setView_each_each(img_bone, bone, this.imageSource_Skel_Bone);
                setView_each_each(img_joint, joint, this.imageSource_Skel_Joint);
            }
        }

        void setView_each_each(Image img, CheckBox combo, ImageSource source)
        {
            if (combo.IsChecked == true) { img.Source = source; }
            else { img.Source = null; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            // Create an image source that we can use in our image control
            this.drawingGroup_Skel_Joint = new DrawingGroup();
            this.imageSource_Skel_Joint = new DrawingImage(this.drawingGroup_Skel_Joint);
            this.drawingGroup_Skel_Bone = new DrawingGroup();
            this.imageSource_Skel_Bone = new DrawingImage(this.drawingGroup_Skel_Bone);
            this.drawingGroup_bgColor = new DrawingGroup();
            this.imageSource_bgColor = new DrawingImage(this.drawingGroup_bgColor);
            this.drawingGroup_edge = new DrawingGroup();
            this.imageSource_edge = new DrawingImage(this.drawingGroup_edge);
            //-------- Hot Key ---------------
            this.KeyUp += new KeyEventHandler(hotKey);
            //this.AddHandler(UserTracker.KeyDownEvent, new RoutedEventHandler(HandleHandledKeyDown), true);

        }


        void hotKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                do_track();
            }
            else if (e.Key == Key.F1)
            {
                do_checkSeat(); checkBox_swtich(checkSeat);
                return;
            }
            else if (e.Key == Key.F2)
            {
                do_checkClose(); checkBox_swtich(checkClose);
                return;
            }
            else if (e.Key == Key.F3)
            {
                do_faceTrack(); checkBox_swtich(checkFace);
                return;
            }
        }

        void checkBox_swtich(CheckBox cb)
        {
            if (cb.IsChecked.Value == true) { cb.IsChecked = false; }
            else { cb.IsChecked = true; }
        }
        //=========================================================================
        //============= Shutdown Operation ==========================================
        /* using only Sensor.Stop() might cause Error*/

        // Execute shutdown tasks
        private void Window_Closed(object sender, EventArgs e)
        {
            time_thread_run = false;
            TheStore.mainWindow.Show();
            shutdown_operation();
        }

        void shutdown_operation()
        {
            if (null != this.sensor)
            {
                //stop Thread
                this.sensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                //reset Mode & stop Stream
                close_Mode();
                FaceClosed();
                //Close down
                this.sensor.Stop();//This may cause Freezing Error
                this.sensor = null;
            }
            if (form_Detector != null) { form_Detector.confirmOnClose = false; form_Detector.Close(); }
            if (form_UKI != null) { form_UKI.Close(); }
            if (form_FaceDetector != null) { form_FaceDetector.Close(); }
        }

        //Stop tracking
        void close_Mode()
        {
            //Reset Value
            this.sensor.DepthStream.Range = DepthRange.Default;
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            //Stop Function
            this.sensor.SkeletonStream.Disable();
            this.sensor.ColorStream.Disable();
            this.sensor.DepthStream.Disable();
        }

        //=========================================================================
        //============= GUI Operation ==========================================
        public Boolean is_Tracking = false;
        Boolean in_Drawing = true;

        // Click Track Button
        private void bTrack_Click(object sender, RoutedEventArgs e)
        {
            do_track();
        }


        Boolean is_Save = false;//File is save?

        void do_track()
        {
            if (this.sensor == null)
            {
                //If no sensor yet >> Initialize
                kinectInitialize();
                InitializeFaceDraw();//Initialize First, use or not is later
                //------------------------
                //after Successful Initialize
                if (this.sensor != null)
                {
                    openKinectMenu();
                    kinectSensorOn();
                    openFrameStream();
                    setUpSlideAngle();
                    //InitializeFace3DDraw();
                    checkSmooth.IsEnabled = true;
                    sensorTilt();//Tile to 0 Degree
                }
            }
            if (this.sensor != null)
            {
                do_checkSeat();
                do_checkClose();
                statusSet();
                //--------------------------------------
                if (is_Tracking == false)
                {
                    Boolean go_on = false;
                    if (rowTotal > 0 && is_Save == false)
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Table will be reset."
                                        , "Are you sure?", System.Windows.MessageBoxButton.OKCancel);
                        if (result == System.Windows.MessageBoxResult.OK)
                        {
                            //if (is_Save == false) { backup_temp_UnSave(); }
                            go_on = true;
                        }
                    }
                    else
                    {
                        go_on = true;
                    }
                    //-----------------------------------------------
                    if (go_on == true)
                    {
                        kinectTableReset(); is_Tracking = true;
                        bTrack.Content = "Stop";
                        //is_Save = false;
                    }
                }
                else
                {
                    is_Tracking = false;
                    bTrack.Content = "Track";
                    is_Save = askToSave();
                    if (is_Save == false)
                    {
                        backup_temp();//Keep Temp for Unsave file when stop
                        backup_switch();//Switch for next backup file
                    }
                    //txtErr.Content = listPerson.Count() + "||" + listPersonD.Count();
                }
            }

            /* Stop Function is removed,
             * After, Kinect is Initialize, it will not stop
             */
        }


        void openKinectMenu()
        {
            butIntruderAlert.IsEnabled = true;
        }

        private void click_butExit(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Windows[0].Close();
        }

        //------------------------------------------------------------------------
        //------------- Decimal ------------------------------------------
        int decimalNum = 4;//default digit
        double getRound(double value)
        {
            if (decimalNum > 9) { return value; }//No need to round
            else { return Math.Round(value, decimalNum); }
        }

        private void comboDecimal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboDecimal.SelectedItem != null)
            {
                decimalNum = TheTool.getInt(comboDecimal);
            }
            else { decimalNum = 4; }
        }
        //--------------------------------------------------------------------------------
        //-------------------- Kinect Angle ------------------------------------------

        public int kinectAngle = 0;
        void setUpSlideAngle()
        {
            if (this.sensor != null)
            {
                slideAngle.IsEnabled = true;
                slideAngle.Minimum = this.sensor.MinElevationAngle;
                slideAngle.Maximum = this.sensor.MaxElevationAngle;
                slideAngle.Value = 0;
                kinectAngle = 0;
            }
        }

        private void slideAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int angle = (int)slideAngle.Value;
                if (angle >= this.sensor.MinElevationAngle &&
                    angle <= this.sensor.MaxElevationAngle)
                {
                    this.kinectAngle = angle;
                    txtAngle.Content = angle.ToString() + "°";
                }
                butAngle.Foreground = color_txtReady;
            }
            catch { }
        }

        private void butAngle_Click(object sender, RoutedEventArgs e)
        {
            sensorTilt();
            butAngle.Foreground = Brushes.Black;
        }

        private void butAngle2_Click(object sender, RoutedEventArgs e)
        {
            slideAngle.Value = 0;
            this.kinectAngle = 0;
            sensorTilt();
            butAngle.Foreground = Brushes.Black;
        }

        void sensorTilt()
        {
            if (this.sensor != null)
            {
                try
                {
                    this.sensor.ElevationAngle = this.kinectAngle;
                    txtAngle.Content = this.kinectAngle.ToString();
                }
                catch { }
            }
        }

        //====================================================================================
        //============================= Kinect Sensoring ========================================

        void comboKinectSelect_reset()
        {
            try
            {
                int kinectConnect = KinectSensor.KinectSensors.Count();
                for (int i = 1; i <= kinectConnect; i++) { comboKinect.Items.Add(i.ToString()); }//Decimal Combobox
            }
            catch { TheSys.showError("Error: Fail Counting Kinect", true); }
        }

        private void comboKinect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboKinect.SelectedItem != null)
                {
                    kinectSelect = TheTool.getInt(comboKinect);
                }
                else { kinectSelect = 1; }
                //txtErr.Content = kinectSelect.ToString();
            }
            catch { }
        }

        //first connect Kinect, no sensor yet
        void kinectInitialize()
        {
            try
            {
                int i = 1;
                int n = kinectSelect;
                // Look through all sensors and select N camera
                foreach (var potentialSensor in KinectSensor.KinectSensors)
                {
                    if (potentialSensor.Status == KinectStatus.Connected)
                    {
                        if (i >= n)
                        {
                            this.sensor = potentialSensor;
                            return;
                        }
                        else { i++; }
                    }
                }
                if (null == this.sensor) { txtStatus.Content = "Status: Kinect not found"; }
            }
            catch (Exception ex) { TheSys.showError("kinect:" + ex.ToString(), true); }
        }

        private void kinectSensorOn()
        {
            try
            {
                this.sensor.Start();
            }
            catch (IOException)
            {
                this.sensor = null;
                TheSys.showError("Error: Kinect not found", true);
            }
        }


        private void openFrameStream()
        {
            try
            {
                if (this.sensor != null)
                {
                    this.sensor.SkeletonStream.Enable();
                    newKinectReady();
                    this.sensor.AllFramesReady += KinectSensorOnAllFramesReady;
                }
            }
            catch { }
        }

        void changeSteam()
        {
            int code = comboStream.SelectedIndex;
            if (code == 1)
            {
                ColorFormat = ColorImageFormat.RgbResolution1280x960Fps12;
                DepthFormat = DepthImageFormat.Resolution640x480Fps30;
            }
            else if (code == 2)
            {
                ColorFormat = ColorImageFormat.YuvResolution640x480Fps15;
                DepthFormat = DepthImageFormat.Resolution80x60Fps30;
            }
            else
            {
                ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
                DepthFormat = DepthImageFormat.Resolution640x480Fps30;
            }
            newKinectReady();
        }

        void setupComboStream()
        {
            comboStream.Items.Add("SD 640x480");
            comboStream.Items.Add("HD");
            comboStream.Items.Add("LD");
        }


        //=============================================================================
        //=============== Kinect Mode Operation =============================

        //Set Status txt
        void statusSet()
        {
            String txt = "Status: ";
            if (this.sensor != null)
            {
                if (mode_seat == true) { txt += "Seated"; }
                else { txt += "Normal"; }
                if (mode_close == true) { txt += ", Closed"; }
            }
            else { txt += "Off"; }
            txtStatus.Content = txt;
        }

        //-------------------------------------------------------------------
        //----------- Seat mode -------------

        Boolean mode_seat = false;

        //Check box - Set Seated Mode
        private void checkSeat_Checked_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            do_checkSeat();
        }

        void do_checkSeat()
        {
            mode_seat = checkSeat.IsChecked.Value;
            kinectModeSet_Seat();
            statusSet();
        }

        void kinectModeSet_Seat()
        {
            if (this.sensor != null)
            {
                if (mode_seat == false)
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
            }
        }

        //-------------------------------------------------------------------
        //----------- Closed  mode -------------
        Boolean mode_close = false;

        private void checkClose_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            do_checkClose();
        }

        void do_checkClose()
        {
            mode_close = checkClose.IsChecked.Value;
            kinectModeSet_Closed();
            statusSet();
        }

        void kinectModeSet_Closed()
        {
            if (this.sensor != null)
            {
                try
                {
                    if (mode_close == false)
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                        this.sensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                        this.sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                }
                catch { TheSys.showError("Error: H/W not support", true); }
            }
        }


        //=========================================================================
        //============= Table Operation ==========================================

        // ----- Parallel Data --------
        List<Person> listPerson = new List<Person>();//Value as Number
        ObservableCollection<PersonString> listJointString = new ObservableCollection<PersonString>(); //Value as String
        //** List is not capable to contain Data gor Dynamic GridView, ObservableCollection needed

        //Start Kinect, already Initialize Sensor
        private void kinectTableReset()
        {
            totalCounter.reset();
            clearList();
            setGridShow();
            this.rowTotal = 0;
            txtStart.Content = "Start: ";
            txtEnd.Content = "End: ";
            txtAvgFps.Content = "Avg. fps: 0";
            txtRow.Content = "Row: 0";
            txtTime.Content = "Time: 0";
        }

        void clearList()
        {
            listPerson.Clear();
            listJointString.Clear();
        }


        //==============================================================================
        //============================ Get Data from Kinect ============================

        //------ Color 
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private WriteableBitmap imageSource_Color;
        private byte[] colorImageData;
        private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;
        //------ Depth
        BitmapSource imageSource_Depth;
        //------ Skel
        private DrawingImage imageSource_bgColor;
        private DrawingGroup drawingGroup_bgColor;
        private DrawingImage imageSource_Skel_Joint;
        private DrawingGroup drawingGroup_Skel_Joint;
        private DrawingImage imageSource_Skel_Bone;
        private DrawingGroup drawingGroup_Skel_Bone;
        private DrawingImage imageSource_edge;
        private DrawingGroup drawingGroup_edge;
        //------------------------------------------------------------------

        //Get Color from Kinect
        private void sensorFrame_Color()
        {
            // Make a copy of the color frame for displaying.
            var haveNewFormat = this.currentColorImageFormat != colorFrame.Format;
            if (haveNewFormat)
            {
                this.currentColorImageFormat = colorFrame.Format;
                this.colorImageData = new byte[colorFrame.PixelDataLength];
                this.imageSource_Color = new WriteableBitmap(
                    colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
            }

            colorFrame.CopyPixelDataTo(this.colorImageData);
            this.imageSource_Color.WritePixels(
                new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                this.colorImageData,
                colorFrame.Width * Bgr32BytesPerPixel,
                0);
        }


        //Get Depth from Kinect
        private void sensorFrame_Depth()
        {
            short[] pixelData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(pixelData);
            int stride = depthFrame.Width * depthFrame.BytesPerPixel;
            imageSource_Depth = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96,
            PixelFormats.Gray16, null,
            pixelData, stride);
        }


        //Get Skel from Kinect
        private void sensorFrame_Skeleton(AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            //------- Get Skeleton -------
            Skeleton[] skeletons = new Skeleton[0];// 1 Skeleton / Person
            using (SkeletonFrame skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
                else
                {
                    if (form_Detector != null)
                    {
                        form_Detector.is_toBreak = false;
                    }
                }
            }
            useFrame_Skelton(skeletons);
        }

        //============================================================================
        public Boolean person_detected = false;//whether some human detected
        int person_index = 0;//To check index of Person in Loop


        //====================================================================
        //============ PP: Nearest point Method ================
        /* First thePerson not mean index = 1
         * First person might by index 6,
         * but when another come, second person can become index 6 instead
         * In order to track specific person
         * some point of body need to be used to calculate the "Nearest" >> Shoulder Center
         */
        double follow_x = 0; double follow_y = 0; double follow_z = 0;
        void follow_Reset() { follow_x = 0; follow_y = 0; follow_z = 0; }
        double first_x = 0; double first_y = 0; double first_z = 0;
        void first_Reset() { first_x = 0; first_y = 0; first_z = 0; }
        //-----------------------------------------------------------------------------
        public TimeSpan time_span;
        public int time_span_sec = 0;

        //Main Process
        void useFrame_Skelton(Skeleton[] skeletons)
        {
            using (DrawingContext dc_joint = this.drawingGroup_Skel_Joint.Open())
            {
                using (DrawingContext dc_bone = this.drawingGroup_Skel_Bone.Open())
                {
                    using (DrawingContext dc_edge = this.drawingGroup_edge.Open())
                    {
                        // Draw a transparent background to set the render size
                        System.Windows.Rect bound = new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight);
                        dc_joint.DrawRectangle(Brushes.Transparent, null, bound);
                        dc_bone.DrawRectangle(Brushes.Transparent, null, bound);
                        dc_edge.DrawRectangle(Brushes.Transparent, null, bound);
                        person_detected = false;

                        if (skeletons.Length != 0)
                        {
                            //---------- First thePerson not mean index = 1 ------------------
                            Skeleton mainPerson = getPersonMain_ifExist(skeletons);//seek Main Person
                            //===========================================================
                            if (mainPerson != null)
                            {
                                person_detected = true;
                                thisTime = DateTime.Now;
                                if (form_UKI != null) { form_UKI.process(mainPerson); }
                                //go though All person
                                foreach (Skeleton eachPerson in skeletons)
                                {
                                    if (checkPersonExist(eachPerson) == true)
                                    {
                                        if ((checkHideOthSkel.IsChecked.Value == true && eachPerson.Equals(mainPerson))
                                            || checkHideOthSkel.IsChecked.Value == false)
                                        {
                                            if (eachPerson.Equals(mainPerson))
                                            {
                                                do_detectState(eachPerson);
                                                if (mainPerson.TrackingState == SkeletonTrackingState.Tracked)
                                                {
                                                    time_span = thisTime.Subtract(startTime);
                                                    time_span_sec = (int)time_span.TotalSeconds;
                                                    if (is_Tracking == true)
                                                    {
                                                        //Table Track : only Main Person
                                                        list_addPerson(mainPerson);//add Data to list & grid
                                                    }
                                                }
                                                //processFaceData(); ** CRITICAL ERROR **
                                                this.trackedBonePen = new Pen(Brushes.SteelBlue, 6); //color for this
                                            }
                                            else
                                            {
                                                this.trackedBonePen = new Pen(Brushes.SeaGreen, 6); //color for non-Main
                                            }
                                            //-----------------------------------
                                            if (in_Drawing == true)
                                            {
                                                draw_Person(eachPerson, dc_joint, dc_bone);//Draw person
                                                RenderClippedEdges(eachPerson, dc_edge);//Draw Edge
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // prevent drawing outside of our render area 
                        this.drawingGroup_Skel_Joint.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));
                    }
                }
            }
        }


        //=================================================================
        //================= Follow First Person ==================
        string[] follow_method = new string[] { 
            "Nearest: Last X,Y,Z"
            ,"Nearest: First X,Y,Z"
            ,"Nearest: (0,0,0)"
            ,"Nearest: (0,0)"
            ,"Least Z"
            ,"First Index"
        };

        void comboFollow_setup()
        {
            foreach (string each in follow_method)
            {
                comboFollow.Items.Add(each);
            }
            comboFollow.SelectedIndex = 4;
            follow_Reset();
            first_Reset();
        }

        //PP: Nearest Point Method
        //Following Main Person
        Skeleton getPersonMain_ifExist(Skeleton[] skeletons)
        {
            try
            {
                Skeleton mainPerson = null;
                if (comboFollow.SelectedIndex == 0) { mainPerson = followMethod_Near_Dynamic(skeletons, 0); }
                else if (comboFollow.SelectedIndex == 1) { mainPerson = followMethod_Near_Fix(skeletons, first_x, first_y, first_z); }
                else if (comboFollow.SelectedIndex == 3) { mainPerson = followMethod_Near_Fix(skeletons, 0, 0, 0); }
                else if (comboFollow.SelectedIndex == 2) { mainPerson = followMethod_Near_Fix(skeletons, 0, 0); }
                else if (comboFollow.SelectedIndex == 4) { mainPerson = followMethod_Near_Dynamic(skeletons, comboFollow.SelectedIndex); }
                else { mainPerson = followMethod_FirstIndex(skeletons); }
                return mainPerson;
            }
            catch { TheSys.showError("Error: on following Main Person", true); return null; }
        }

        //Method 0: Follow Nearest Last Frame (Least Manhattan Distance)
        //Method 3: Follow Least Z
        Skeleton followMethod_Near_Dynamic(Skeleton[] skeletons, int method)
        {//Main Person is who has Least (x_nearest_diff + y_nearest_diff)
            person_index = 0;//index of current person
            Skeleton mainPerson = null;
            //--------
            double least_value = 88888;//<< any Num, but make sure any first Person can have less test value
            double test_value = 99999;
            double x; double y; double z;
            foreach (Skeleton eachPerson in skeletons)
            {
                if (checkPersonExist(eachPerson) == true)
                {
                    x = eachPerson.Joints[joint_ShoulderCenter].Position.X;
                    y = eachPerson.Joints[joint_ShoulderCenter].Position.Y;
                    z = eachPerson.Joints[joint_ShoulderCenter].Position.Z;
                    if (method == 4) { test_value = eachPerson.Joints[joint_ShoulderCenter].Position.Z * 100; }
                    else
                    {
                        test_value = (Math.Abs(x - follow_x)) + (Math.Abs(y - follow_y)) + (Math.Abs(z - follow_z));
                        TheSys.showError(test_value.ToString(), true);
                    }
                    if (test_value < least_value)
                    {
                        //Nearer is detected
                        mainPerson = eachPerson;
                        least_value = test_value;
                        txtSkel.Content = "Skel: " + person_index;
                        this.follow_x = x;
                        this.follow_y = y;
                        this.follow_z = z;
                    }
                }
                person_index++;
            }
            return mainPerson;
        }


        //Follow Method Fix
        Skeleton followMethod_Near_Fix(Skeleton[] skeletons, double x_fix, double y_fix)
        {
            person_index = 0;//index of current person
            Skeleton mainPerson = null;
            //--------
            double least_value = 88888;//<< any Num, but make sure any first Person can have less test value
            double test_value = 99999;
            double x; double y;
            foreach (Skeleton eachPerson in skeletons)
            {
                if (checkPersonExist(eachPerson) == true)
                {
                    x = eachPerson.Joints[joint_ShoulderCenter].Position.X;
                    y = eachPerson.Joints[joint_ShoulderCenter].Position.Y;
                    test_value = (Math.Abs(x - x_fix)) + (Math.Abs(y - y_fix));
                    //txtErr.Content = test_value;
                    if (test_value < least_value)
                    {
                        //Nearer is detected
                        mainPerson = eachPerson;
                        least_value = test_value;
                        txtSkel.Content = "Skel: " + person_index;
                    }
                }
                person_index++;
            }
            return mainPerson;
        }

        //Follow Method Fix
        Skeleton followMethod_Near_Fix(Skeleton[] skeletons, double x_fix, double y_fix, double z_fix)
        {
            person_index = 0;//index of current person
            Skeleton mainPerson = null;
            //--------
            double least_value = 88888;//<< any Num, but make sure any first Person can have less test value
            double test_value = 99999;
            double x; double y; double z;
            foreach (Skeleton eachPerson in skeletons)
            {
                if (checkPersonExist(eachPerson) == true)
                {
                    x = eachPerson.Joints[joint_ShoulderCenter].Position.X;
                    y = eachPerson.Joints[joint_ShoulderCenter].Position.Y;
                    z = eachPerson.Joints[joint_ShoulderCenter].Position.Z;
                    test_value = (Math.Abs(x - x_fix)) + (Math.Abs(y - y_fix)) + (Math.Abs(z - z_fix));
                    //txtErr.Content = test_value;
                    if (test_value < least_value)
                    {
                        //Nearer is detected
                        mainPerson = eachPerson;
                        least_value = test_value;
                        txtSkel.Content = "Skel: " + person_index;
                        if (first_x == 0 && first_y == 0 && first_z == 0) { first_x = x; first_y = y; first_z = z; }
                    }
                }
                person_index++;
            }
            return mainPerson;
        }

        //Follow first Index
        Skeleton followMethod_FirstIndex(Skeleton[] skeletons)
        {
            Skeleton mainPerson = null;
            person_index = 0;//index of current person
            foreach (Skeleton eachPerson in skeletons)
            {
                if (checkPersonExist(eachPerson) == true)
                {
                    mainPerson = eachPerson;
                    txtSkel.Content = "Skel: " + person_index;
                    break;
                }
                person_index++;
            }
            return mainPerson;
        }
        //============================================================================
        //Drawing Bg
        void prepareDraw_BG()
        {
            using (DrawingContext dc = this.drawingGroup_bgColor.Open())
            {
                dc.DrawRectangle(color_SkelBG, null, new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }


        //========================================================================
        //============ Skel Draw Operation ==========

        void list_addPerson(Skeleton thePerson)
        {
            try
            {

                if (rowTotal == 0)
                {
                    startTime = thisTime;
                    txtStart.Content = "Start: " + startTime.ToString("HH:mm:ss");
                    this.thisTime0 = 0;
                    this.lastTime0 = 0;
                    this.lastTime0_row = 0;
                    this.backupTime_next0 = backupEvery;
                }
                //--------
                this.thisTime0 = time_span_sec;
                txtTime.Content = "Time: " + thisTime0;
                txtEnd.Content = "End: " + thisTime.ToString("HH:mm:ss"); ;
                thisTimeString = thisTime.ToString("ddHHmmssff");
                ////--- fps : Speed : Frame / sec
                if (thisTime0 - lastTime0 >= 1)// 1 Second Pass
                {
                    double fps = rowTotal - lastTime0_row;
                    txtfps.Content = "fps: " + fps;
                    lastTime0_row = rowTotal;
                    lastTime0 = thisTime0;
                    if (thisTime0 > 0) { txtAvgFps.Content = "Avg. fps: " + rowTotal / thisTime0; }
                }
                if (thisTime0 > backupTime_next0)
                {
                    backup_temp();//Keep Temp file as Back up
                    backupTime_next0 = thisTime0 + backupEvery;
                }
                //===============================================================
                //========== Get Joint/Person List =============
                Person newJoint = new Person();
                newJoint.TimeTrack = thisTimeString;
                newJoint.TimeSinceStart = ((double)time_span.TotalSeconds).ToString();
                newJoint.Class = classMark;
                newJoint.Head = getJointSet(thePerson, JointType.Head);
                newJoint.ShoulderCenter = getJointSet(thePerson, JointType.ShoulderCenter);
                newJoint.ShoulderLeft = getJointSet(thePerson, JointType.ShoulderLeft);
                newJoint.ElbowLeft = getJointSet(thePerson, JointType.ElbowLeft);
                newJoint.WristLeft = getJointSet(thePerson, JointType.WristLeft);
                newJoint.ShoulderRight = getJointSet(thePerson, JointType.ShoulderRight);
                newJoint.ElbowRight = getJointSet(thePerson, JointType.ElbowRight);
                newJoint.WristRight = getJointSet(thePerson, JointType.WristRight);
                //
                if (checkLossy.IsChecked == false)
                {
                    newJoint.HandLeft = getJointSet(thePerson, JointType.HandLeft);
                    newJoint.HandRight = getJointSet(thePerson, JointType.HandRight);
                }
                //-------------------
                if (mode_seat == false)
                {
                    newJoint.Spine = getJointSet(thePerson, JointType.Spine);
                    newJoint.HipLeft = getJointSet(thePerson, JointType.HipLeft);
                    newJoint.KneeLeft = getJointSet(thePerson, JointType.KneeLeft);
                    newJoint.AnkleLeft = getJointSet(thePerson, JointType.AnkleLeft);
                    newJoint.HipRight = getJointSet(thePerson, JointType.HipRight);
                    newJoint.KneeRight = getJointSet(thePerson, JointType.KneeRight);
                    newJoint.AnkleRight = getJointSet(thePerson, JointType.AnkleRight);
                    //
                    if (checkLossy.IsChecked == false)
                    {
                        newJoint.HipCenter = getJointSet(thePerson, JointType.HipCenter);
                        newJoint.FootLeft = getJointSet(thePerson, JointType.FootLeft);
                        newJoint.FootRight = getJointSet(thePerson, JointType.FootRight);
                    }
                }
                //-----------------
                //Person Get PersonD
                if (form_Detector != null && form_Detector.getBase == true)
                {
                    PersonD pd = form_Detector.getPersonDetect();
                    totalCounter.addData(pd);
                    newJoint.personD = pd;
                }

                listPerson.Add(newJoint);

                //========= Get JointString/PersonString List ======================
                if (checkHide.IsChecked.Value == false)
                {
                    PersonString newJointString = TheTool_Person.getPersonString(newJoint);
                    listJointString.Add(newJointString);
                }
                //==============================================================
                //-------- Move Cursor --------
                if (checkHide.IsChecked.Value == false) { grid.ScrollIntoView(grid.Items[grid.Items.Count - 2]); }
                //-------- Update Number ----
                rowTotal++;
                txtRow.Content = "Row: " + rowTotal.ToString();
                if (form_UKI == null)
                {
                    txtTimeBig.Content = thisTime.ToString("HH:mm:ss.ff") + " (" + rowTotal + "," + thisTime0 + ")";
                }
            }
            catch { TheSys.showError("Error: AddSkel", true); }
        }

        public void showTimeMeter(string text)
        {
            txtTimeBig.Content = text;
        }

        //For Always Show
        double[] getJointSet(Skeleton skel, JointType type)
        {
            Joint theJoint = skel.Joints[type];
            SkeletonPoint position = theJoint.Position;
            double[] jointSet = jointSet = new double[4] { 
                    getJointConfident(theJoint),
                    getRound(position.X),
                    getRound(position.Y),
                    getRound(position.Z)};
            return jointSet;
        }

        double getJointConfident(Joint theJoint)
        {
            //stackoverflow.com/questions/13831956/the-value-of-joint-if-the-body-was-out-of-range
            double confident = 0;
            if (theJoint.TrackingState == JointTrackingState.Tracked)
            {
                confident = 1;
            }
            else if (theJoint.TrackingState == JointTrackingState.Inferred)
            {
                confident = .5;
            }
            return confident;
        }



        Boolean checkPersonExist(Skeleton thePerson)
        {
            if (thePerson.TrackingState == SkeletonTrackingState.Tracked) { return true; }
            else { return false; }
        }

        public Boolean show_VHip = false;

        void draw_Person(Skeleton thePerson, DrawingContext dc_joint, DrawingContext dc_Bone)
        {
            if (thePerson.TrackingState == SkeletonTrackingState.Tracked)
            {
                this.DrawBonesAndJoints(thePerson, dc_joint, dc_Bone);
                if (checkBCenter.IsChecked.Value)
                {
                    dc_joint.DrawEllipse(
                        this.centerPointBrush,
                        null,
                        this.SkeletonPointToScreen(thePerson.Position),
                        BodyCenterThickness,
                        BodyCenterThickness);
                }
                //Draw Virtual Hip
                if (this.form_Detector != null && show_VHip == true)
                {
                    try
                    {
                        SkeletonPoint spoint = new SkeletonPoint();
                        spoint.X = (float)(form_Detector.hip_virtual_base[0]);
                        spoint.Y = (float)(form_Detector.hip_virtual_base[1]);
                        spoint.Z = (float)(form_Detector.hip_virtual_base[2]);
                        System.Windows.Point p = this.SkeletonPointToScreen(spoint);
                        //txtErr.Content = spoint.X + "," + spoint.Y + "," + p.X + "," + p.Y;
                        //
                        dc_joint.DrawEllipse(
                            this.HipVirtualBrush,
                            null,
                            this.SkeletonPointToScreen(spoint),
                            HipVirtualThickness,
                            HipVirtualThickness);
                    }
                    catch { }
                }
            }
            else if (thePerson.TrackingState == SkeletonTrackingState.PositionOnly)
            {
                dc_joint.DrawEllipse(
                    this.centerPointBrush,
                    null,
                    this.SkeletonPointToScreen(thePerson.Position),
                    BodyCenterThickness,
                    BodyCenterThickness);
            }
        }


        /// Draws indicators to show which edges are clipping skeleton data
        private void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    color_Edge,
                    null,
                    new System.Windows.Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    color_Edge,
                    null,
                    new System.Windows.Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    color_Edge,
                    null,
                    new System.Windows.Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    color_Edge,
                    null,
                    new System.Windows.Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }


        /// Draws a skeleton's bones and joints
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext dc_joint, DrawingContext dc_Bone)
        {
            this.DrawBone(skeleton, dc_Bone, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, dc_Bone, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, dc_Bone, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, dc_Bone, JointType.ShoulderCenter, JointType.ShoulderRight);
            //
            this.DrawBone(skeleton, dc_Bone, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, dc_Bone, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, dc_Bone, JointType.WristLeft, JointType.HandLeft);
            //
            this.DrawBone(skeleton, dc_Bone, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, dc_Bone, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, dc_Bone, JointType.WristRight, JointType.HandRight);
            //-----------------------------------------------
            if (mode_seat == false)//Optional : To Optimize Performance
            {
                this.DrawBone(skeleton, dc_Bone, JointType.Spine, JointType.HipCenter);
                this.DrawBone(skeleton, dc_Bone, JointType.HipCenter, JointType.HipLeft);
                this.DrawBone(skeleton, dc_Bone, JointType.HipCenter, JointType.HipRight);
                //
                this.DrawBone(skeleton, dc_Bone, JointType.HipLeft, JointType.KneeLeft);
                this.DrawBone(skeleton, dc_Bone, JointType.KneeLeft, JointType.AnkleLeft);
                this.DrawBone(skeleton, dc_Bone, JointType.AnkleLeft, JointType.FootLeft);
                // Right Leg
                this.DrawBone(skeleton, dc_Bone, JointType.HipRight, JointType.KneeRight);
                this.DrawBone(skeleton, dc_Bone, JointType.KneeRight, JointType.AnkleRight);
                this.DrawBone(skeleton, dc_Bone, JointType.AnkleRight, JointType.FootRight);
            }

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }
                //--------- Draw -------------
                if (drawBrush != null)
                {
                    dc_joint.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }


        /// Draws a bone line between two joints
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            //Don't draw if out of bound
            if (checkJointIsInBound(joint0.Position) == false || checkJointIsInBound(joint1.Position) == false)
            {
                return;
            }

            // Don't draw If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }


            //==============================================
            Pen drawPen = this.inferredBonePen;// bones are inferred unless BOTH joints are tracked
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            //Draw a Line between dot
            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        //Draw a Line between dot
        private System.Windows.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Windows.Point(depthPoint.X, depthPoint.Y);
        }


        Boolean checkJointIsInBound(SkeletonPoint skelpoint)
        {
            Boolean inBound = false;
            System.Windows.Point point = this.SkeletonPointToScreen(skelpoint);
            if (point.X <= RenderWidth && point.X >= 0
                && point.Y <= RenderHeight && point.Y >= 0)
            {
                inBound = true;
            }
            return inBound;
        }

        //========================================================================
        //============ Smoothing Operation ==========
        //http://cm-bloggers.blogspot.com/2011/07/kinect-sdk-smoothing-skeleton-data.html
        //http://msdn.microsoft.com/en-us/library/jj131024.aspx


        TransformSmoothParameters parameters = new TransformSmoothParameters();
        public float smooth_Smooth = 0f;
        public float smooth_Correct = 0f;
        public float smooth_Predict = 0f;
        public float smooth_Jitter = 0f;
        public float smooth_Davia = 0f;

        void initialSmoothSlider()
        {
            smooth_Smooth = .5f;
            smooth_Correct = .5f;
            smooth_Predict = .5f;
            smooth_Jitter = .05f;
            smooth_Davia = .04f;
            slide_Smooth.Value = smooth_Smooth;
            slide_Correct.Value = smooth_Correct;
            slide_Predict.Value = smooth_Predict;
            slide_Jitter.Value = smooth_Jitter;
            slide_Davia.Value = smooth_Davia;
            txtSmooth.Content = "Smoothing: " + smooth_Smooth.ToString();
            txtCorrect.Content = "Correction: " + smooth_Correct.ToString();
            txtPredict.Content = "Prediction: " + smooth_Predict.ToString();
            txtJitter.Content = "Jitter Radius: " + smooth_Jitter.ToString();
            txtDavia.Content = "Max Deviation Radius: " + smooth_Davia.ToString();
        }

        private void slide_Smooth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float ff = (float)Math.Round(slide_Smooth.Value, 2);
            if (ff > .99) { ff = .99f; }
            smooth_Smooth = ff;
            txtSmooth.Content = "Smoothing: " + smooth_Smooth.ToString();
            butSmoothColor(Brushes.BlueViolet);
        }

        private void slide_Correct_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            smooth_Correct = (float)Math.Round(slide_Correct.Value, 2);
            txtCorrect.Content = "Correction: " + smooth_Correct.ToString();
            butSmoothColor(Brushes.BlueViolet);
        }
        private void slide_Predict_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            smooth_Predict = (float)Math.Round(slide_Predict.Value, 2);
            txtPredict.Content = "Prediction: " + smooth_Predict.ToString();
            butSmoothColor(Brushes.BlueViolet);
        }
        private void slide_Jitter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            smooth_Jitter = (float)Math.Round(slide_Jitter.Value, 2);
            txtJitter.Content = "Jitter Radius: " + smooth_Jitter.ToString();
            butSmoothColor(Brushes.BlueViolet);
        }
        private void slide_Davia_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            smooth_Davia = (float)Math.Round(slide_Davia.Value, 2);
            txtDavia.Content = "Max Deviation Radius: " + smooth_Davia.ToString();
            butSmoothColor(Brushes.BlueViolet);
        }


        void butSmoothColor(SolidColorBrush color)
        {
            try { if (butSmooth.IsEnabled) { butSmooth.Foreground = color; } }
            catch { }
        }

        private void checkSmooth_Checked_UnChecked(object sender, RoutedEventArgs e)
        {
            SmoothingEnable(checkSmooth.IsChecked.Value);
        }

        void SmoothingEnable(Boolean enable)
        {
            slide_Smooth.IsEnabled = enable;
            slide_Correct.IsEnabled = enable;
            slide_Predict.IsEnabled = enable;
            slide_Jitter.IsEnabled = enable;
            slide_Davia.IsEnabled = enable;
            if (enable == true) { butSmoothColor(Brushes.BlueViolet); }
            else { SmoothingStop(); butSmoothColor(Brushes.Black); }
            butSmooth.IsEnabled = enable;
        }

        void SmoothingStop()
        {
            try
            {
                this.sensor.SkeletonStream.Disable();
                this.sensor.SkeletonStream.Enable();
            }
            catch { }
        }

        private void butSmooth_Click(object sender, RoutedEventArgs e)
        {
            do_Smooth();
        }

        void do_Smooth()
        {
            try
            {
                if (this.sensor != null)
                {
                    parameters.Smoothing = smooth_Smooth;
                    parameters.Correction = smooth_Correct;
                    parameters.Prediction = smooth_Predict;
                    parameters.JitterRadius = smooth_Jitter;
                    parameters.MaxDeviationRadius = smooth_Davia;
                    this.sensor.SkeletonStream.Enable(parameters);
                    butSmoothColor(Brushes.Black);
                }
            }
            catch { }
        }


        //============ Face Draw Operation ==========

        void InitializeFaceDraw()
        {
            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            //faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooser.Start();
        }

        private void FaceClosed()
        {
            //faceTrackingViewer.Dispose();
        }

        //===================================================
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            this.sensor = kinectChangedEventArgs.NewSensor;
            newKinectReady();
        }

        void newKinectReady()
        {
            try
            {
                if (this.sensor != null)
                {
                    this.sensor.ColorStream.Enable(ColorFormat);
                    this.sensor.DepthStream.Enable(DepthFormat);
                    greenScreen_Setup();
                }
            }
            catch { }
        }

        private void checkFace_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            do_faceTrack();
        }

        void do_faceTrack()
        {
            TheStore.faceTrack = checkFace.IsChecked.Value;
            if (TheStore.faceTrack == false)
            {
                FaceClosed();
                //faceTrackingViewer.Opacity = 0;
            }
            else { 
                //faceTrackingViewer.Opacity = 1; 
            }
        }

        //==========================================================================================
        //==================== Export ======================================================

        void setComboExport()
        {
            comboExport.Items.Add("CSV basic");
            comboExport.Items.Add("CSV +Detector");
            comboExport.Items.Add("CSV +Euclidian");
            comboExport.Items.Add("Excel");
            comboExport.Items.Add("txt for O3KNS ");
            comboExport.Items.Add("set CSV + txt + note + png");
            comboExport.SelectedIndex = 5;
        }

        Boolean askToSave()
        {
            Boolean save = false;
            if (thisTime0 > 180)//Only if more than 3 min record
            {
                System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save"
                                            , "Save?", System.Windows.MessageBoxButton.YesNo);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    export();
                    save = true;
                }
            }
            return save;
        }

        private void butExport_Click(object sender, RoutedEventArgs e)
        {
            export();
        }

        void export()
        {
            try
            {
                //Month+Date+Hour+Minute+Second+milli
                //string file_name = DateTime.Now.ToString("MMdd_HHmmss");
                string file_name = startTime.ToString("MMdd_HHmmss") + thisTime.ToString("_HHmmss");
                //-----------
                if (comboExport.SelectedIndex == 0) { export_csv_byFullPath(TheURL.url_saveFolder + file_name + ".csv", false, false, true); }
                else if (comboExport.SelectedIndex == 1) { export_csv_byFullPath(TheURL.url_saveFolder + file_name + "_d.csv", true, false, true); }
                else if (comboExport.SelectedIndex == 2) { export_csv_byFullPath(TheURL.url_saveFolder + file_name + "_e.csv", false, true, true); }
                else if (comboExport.SelectedIndex == 3) { export_excel(file_name); }
                else if (comboExport.SelectedIndex == 4) { export_txtO3KNS(file_name + "_O3KNS", true); }
                else if (comboExport.SelectedIndex == 5) { export_set1(file_name); }
                is_Save = true;
            }
            catch { TheSys.showError("Error: export", true); }
        }

        void export_set1(string file_name)
        {
            String path_folder = TheURL.url_saveFolder + file_name;
            TheTool.Folder_CreateIfMissing(path_folder);
            Boolean detectorColumn = false; if (form_Detector != null) { detectorColumn = true; }
            //
            string file_name2 = file_name + @"\" + file_name;//Save in Folder
            export_csv_byFullPath(TheURL.url_saveFolder + file_name2 + "_d.csv", detectorColumn, false, false);
            export_txtO3KNS(file_name2 + "_O3KNS", false);
            export_txtNote(file_name2 + "_note", false); exportPNG(file_name2, false);
            //
            System.Windows.MessageBox.Show(@"Save All to 'file\" + file_name + "\'", "Export CSV + txt + note + png");
        }

        void exportPNG(string file_name, Boolean popUp_ifFinish)
        {
            TheTool.SaveScreen(TheURL.url_saveFolder + file_name + ".png", popUp_ifFinish);
        }

        void export_txtO3KNS(string file_name, Boolean popUp_ifFinish)
        {
            string cid = txtCamera.Text;
            TheTool.trackPersonID = cid;
            TheTool_Person.trackPersonID = cid;
            TheTool.exportTxt_O3KNS(file_name, listPerson, popUp_ifFinish);
        }

        void export_txtNote(string file_name, Boolean popUp_ifFinish)
        {
            TheTool.exportTxtNote(file_name, this, popUp_ifFinish);
        }

        void export_excel(string file_name)
        {
            String txtTime = startTime.ToString("HH:mm:ss") + " - " + thisTime.ToString("HH:mm:ss tt");
            String txtTotalTime = thisTime0.ToString();
            String txtTotalRow = rowTotal.ToString();
            TheTool.exportExcel_UserTracker(file_name, listPerson, mode_seat
                , checkLossy.IsChecked.Value, txtTime, txtTotalTime, txtTotalRow);
        }

        void export_csv_byFullPath(string file_path, Boolean detector, Boolean euclidean, Boolean popUp_whenFinish)
        {
            TheTool.exportCSV_listPerson(file_path, listPerson,
                detector, euclidean, mode_seat,
                    checkLossy.IsChecked.Value, popUp_whenFinish);
        }


        //------------------ Back up + Switch --------------------------
        Boolean temp_switch = true;//There are 2 temp file, save by switching
        string backup_file_name = "temp1";

        void backup_temp()
        {
            if (rowTotal > 0)
            {
                //some data must exist
                export_csv_byFullPath(TheURL.url_1_sys + backup_file_name + ".csv", false, false, false);
                if (form_Detector != null)
                {
                    form_Detector.saveReportData(backup_file_name + "R", false);
                }
            }
        }


        void backup_switch()
        {
            if (rowTotal > 0)
            {
                if (temp_switch == true) { backup_file_name = "temp2"; temp_switch = false; }
                else { backup_file_name = "temp1"; temp_switch = true; }
            }//some data must exist
        }


        //==========================================================================================
        //==================== Menu Bar  ======================================================

        private void menu_full(object sender, RoutedEventArgs e)
        {
            do_fullScreen();
        }

        public void do_fullScreen()
        {
            try
            {
                this.WindowState = WindowState.Maximized;
                comboV2.SelectedIndex = 0;
                comboV3.SelectedIndex = 0;
                comboV4.SelectedIndex = 0;
                gui_Top.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                gui_Top.ColumnDefinitions[2].Width = new GridLength(3, GridUnitType.Star);
                gui_TopRow.RowDefinitions[2].Height = new GridLength(0);
                gui_TopCol.ColumnDefinitions[2].Width = new GridLength(0);
                gui_Buttom.ColumnDefinitions[2].Width = new GridLength(750);
            }
            catch { }
        }

        void do_fullScreen2()
        {
            try
            {
                this.WindowState = WindowState.Maximized;
                comboV2.SelectedIndex = 0;
                comboV3.SelectedIndex = 0;
                comboV4.SelectedIndex = 0;
                gui_TopCol.ColumnDefinitions[2].Width = new GridLength(0);
                gui_Buttom.ColumnDefinitions[2].Width = new GridLength(750);
            }
            catch { }
        }

        private void but_menu_officeSyn(object sender, RoutedEventArgs e)
        {
            callDetector();
            //checkHideDebug.IsChecked = true;
            //form_Detector.checkOnTop.IsChecked = true;
            checkHide.IsChecked = true;
            checkSeat.IsChecked = true;
            checkFace.IsChecked = false;
            //checkSmooth.IsChecked = true;
            comboV2.SelectedIndex = 0;
            do_fullScreen();
            //
            do_track();
            //do_Smooth();
            slideAngle.Value = -5; sensorTilt();
        }

        private void butOpenFolderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(TheURL.url_saveFolder);
            }
            catch { }
        }

        //===============================================
        //======== Snap shot =============

        int snap_Countdown = 0;
        Boolean is_wait_snapShot = false;
        public Thread snapshot_thread;

        private void butSnapClick(object sender, RoutedEventArgs e)
        {
            if (is_wait_snapShot == false)
            {
                snap_Countdown = TheTool.getInt(comboSnap);
                snapshot_thread = new Thread(snapShotThread);
                TheTool.change_ImgSource(snapImg, @"pack://application:,,,/P_Tracker2;component/img/Snap2.png");
                snapshot_thread.Start();
            }
        }

        void snapShot()
        {
            try
            {
                string file_name = DateTime.Now.ToString("MMdd_HHmmss_ff");
                exportPNG(file_name, true);
            }
            catch { TheSys.showError("Error: Snap shot", true); }
        }

        void snapShotThread()
        {
            while (snap_Countdown > 0)
            {
                this.Dispatcher.Invoke((Action)(() => { txtSnap.Content = snap_Countdown; }));
                Thread.Sleep(1000);
                snap_Countdown--;
            }
            this.Dispatcher.Invoke((Action)(() =>
            {
                txtSnap.Content = "";
                TheTool.change_ImgSource(snapImg, @"pack://application:,,,/P_Tracker2;component/img/Snap1.png");
            }));
            is_wait_snapShot = false;
            snapShot();
        }

        //===============================================================================
        //============ Intruder Alert =====================================================

        int alert_Countdown = 0;//Loop time
        int alert_Every = 20;
        int alert_EarlyStop = 5;//If we want to stop before Cooldown == 0
        Boolean run_Alert = false;//Intruder_Alert

        private void butAlert(object sender, RoutedEventArgs e)
        {
            if (run_Alert == false) { run_Alert = true; alert_Countdown = 3; butIntruderAlert.IsChecked = true; }
            else { run_Alert = false; TheTool.soundStop(); butIntruderAlert.IsChecked = false; }
        }

        public void time_checkAlert()
        {
            if (run_Alert == true)
            {
                //Loop Continuous
                if (person_detected == true && alert_Countdown < 0)
                {
                    TheTool.soundPlay(TheURL.sound_alert);
                    alert_Countdown = alert_Every;
                }
                //Early Stop
                else if (person_detected == false && alert_Countdown > alert_EarlyStop)
                {
                    alert_Countdown = alert_EarlyStop;
                }
                //Stop
                else if (person_detected == false && alert_Countdown < 0)
                {
                    TheTool.soundStop();
                }
                alert_Countdown--;
            }
        }

        //==============================================================
        //=========== Sit-Stand Detector : SST ==================
        public Detector form_Detector = null;

        void do_detectState(Skeleton eachPerson)
        {
            if (form_Detector != null)
            {
                form_Detector.detect(eachPerson);
            }
        }

        //==============================================================
        //===========  Hidden Data Grid ==================

        private void checkHide_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            setGridShow();
        }

        void setGridShow()
        {
            if (checkHide.IsChecked.Value == false)
            {
                grid.ItemsSource = listJointString;
            }
            else { grid.ItemsSource = null; }
        }

        private void checkFlip_check_uncheck(object sender, RoutedEventArgs e)
        {
            try
            {
                Boolean flip = checkFlip.IsChecked.Value;
                int flip0 = 1;
                if (flip) { flip0 = -1; }
                imageFlip(ImageKinect1_bg, flip0);
                imageFlip(ImageKinect1_Edge, flip0);
                imageFlip(ImageKinect1_Skel_Bone, flip0);
                imageFlip(ImageKinect1_Skel_Joint, flip0);
                imageFlip(ImageKinect2_bg, flip0);
                imageFlip(ImageKinect2_Edge, flip0);
                imageFlip(ImageKinect2_Skel_Bone, flip0);
                imageFlip(ImageKinect2_Skel_Joint, flip0);
                imageFlip(ImageKinect3_bg, flip0);
                imageFlip(ImageKinect3_Edge, flip0);
                imageFlip(ImageKinect3_Skel_Bone, flip0);
                imageFlip(ImageKinect3_Skel_Joint, flip0);
                imageFlip(ImageKinect4_bg, flip0);
                imageFlip(ImageKinect4_Edge, flip0);
                imageFlip(ImageKinect4_Skel_Bone, flip0);
                imageFlip(ImageKinect4_Skel_Joint, flip0);
            }
            catch { }
        }

        void imageFlip(Image img, int flip0)
        {
            img.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleX = flip0;
            //flipTrans.ScaleY = flip0;
            img.RenderTransform = flipTrans;
        }

        string classMark = "S";

        private void checkClass_Checked(object sender, RoutedEventArgs e)
        {
            string c = "S";
            if (checkClass.IsChecked == true) { c = "M"; }
            classMark = c;
        }

        private void comboStream_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeSteam();
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = TheTool.IsTextNumeric(e.Text);
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            backupEvery = TheTool.getInt(txtBackup);
        }


        //======================================================================================
        //======================================================================================
        //============== Green Screen ==========================================================

        private DepthImagePixel[] depthPixels;// Intermediate storage for the depth data received from the sensor
        private byte[] colorPixels;// Intermediate storage for the color data received from the camera
        private ColorImagePoint[] colorCoordinates;// Intermediate storage for the depth to color mapping
        private int depthWidth;// Width of the depth image
        private int depthHeight;// Height of the depth image
        private int opaquePixelValue = -1;// Indicates opaque in an opacity mask
        private int colorToDepthDivisor;// Inverse scaling factor between color and depth
        private int[] greenScreenPixelData;// Intermediate storage for the green screen opacity mask
        private WriteableBitmap gScrn_colorBitmap;// Bitmap that will hold color information
        private WriteableBitmap gScrn_playerOpacityMaskImage = null;// Bitmap that will hold opacity mask information

        //After Sensor & Color & Depth is setup
        void greenScreen_Setup()
        {
            try
            {
                if (null != this.sensor)
                {
                    this.depthWidth = this.sensor.DepthStream.FrameWidth;
                    this.depthHeight = this.sensor.DepthStream.FrameHeight;
                    int colorWidth = this.sensor.ColorStream.FrameWidth;
                    int colorHeight = this.sensor.ColorStream.FrameHeight;
                    this.colorToDepthDivisor = colorWidth / this.depthWidth;

                    // Allocate space to put the  pixels we'll receive
                    this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                    this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                    this.greenScreenPixelData = new int[this.sensor.DepthStream.FramePixelDataLength];
                    this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

                    // This is the bitmap we'll display on-screen
                    this.gScrn_colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    // where we'll put the image data
                    this.GreenScrn_object.Source = this.gScrn_colorBitmap;
                }
            }
            catch (Exception ex) { TheSys.showError("GScrn: " + ex.Message, true); }
        }

        private void greenScreen()
        {
            try
            {
                // Copy the pixel data from the image to a temporary array
                depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
                colorFrame.CopyPixelDataTo(this.colorPixels);
                //--------------------------------------------------------------------------------
                this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                        DepthFormat,
                        this.depthPixels,
                        ColorFormat,
                        this.colorCoordinates);
                Array.Clear(this.greenScreenPixelData, 0, this.greenScreenPixelData.Length);
                for (int y = 0; y < this.depthHeight; ++y)
                {
                    for (int x = 0; x < this.depthWidth; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * this.depthWidth);
                        DepthImagePixel depthPixel = this.depthPixels[depthIndex];
                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, do green screen
                        if (player > 0)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = this.colorCoordinates[depthIndex];
                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
                            int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;
                            if (colorInDepthX > 0 && colorInDepthX < this.depthWidth && colorInDepthY >= 0 && colorInDepthY < this.depthHeight)
                            {
                                // calculate index into the green screen pixel array
                                int greenScreenIndex = colorInDepthX + (colorInDepthY * this.depthWidth);
                                // set opaque
                                this.greenScreenPixelData[greenScreenIndex] = opaquePixelValue;
                                this.greenScreenPixelData[greenScreenIndex - 1] = opaquePixelValue;
                            }
                        }
                    }
                }
                //--------------------------------------------------------------------------------
                // Write the pixel data into our bitmap
                this.gScrn_colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.gScrn_colorBitmap.PixelWidth, this.gScrn_colorBitmap.PixelHeight),
                    this.colorPixels,
                    this.gScrn_colorBitmap.PixelWidth * sizeof(int),
                    0);

                if (this.gScrn_playerOpacityMaskImage == null)
                {
                    this.gScrn_playerOpacityMaskImage = new WriteableBitmap(
                        this.depthWidth,
                        this.depthHeight,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null);

                    GreenScrn_object.OpacityMask = new ImageBrush { ImageSource = this.gScrn_playerOpacityMaskImage };
                }

                this.gScrn_playerOpacityMaskImage.WritePixels(
                    new Int32Rect(0, 0, this.depthWidth, this.depthHeight),
                    this.greenScreenPixelData,
                    this.depthWidth * ((this.gScrn_playerOpacityMaskImage.Format.BitsPerPixel + 7) / 8),
                    0);
            }
            catch (Exception ex) { TheSys.showError("GScrn: " + ex.Message, true); }
        }

        Boolean greenScrn_FirstTime = true;

        private void checkGreenScrn_Checked(object sender, RoutedEventArgs e)
        {
            if (checkGreenScrn.IsChecked == true)
            {
                GreenScrn_bg.Visibility = Visibility.Visible;
                GreenScrn_object.Visibility = Visibility.Visible;
                if (greenScrn_FirstTime == true)
                {
                    checkVJoint1.IsChecked = false;
                    comboV2.SelectedIndex = 0;
                    gui_Top.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    gui_Top.ColumnDefinitions[2].Width = new GridLength(3, GridUnitType.Star);
                    gui_TopRow.RowDefinitions[2].Height = new GridLength(0);
                    gui_TopCol.ColumnDefinitions[2].Width = new GridLength(0);
                    greenScrn_FirstTime = false;
                }
            }
            else
            {
                GreenScrn_bg.Visibility = Visibility.Hidden;
                GreenScrn_bg.Visibility = Visibility.Hidden;
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            if (checkGScrnSkel.IsChecked == true)
            {
                checkVJoint1.IsChecked = true;
                checkVBone1.IsChecked = true;
            }
            else
            {
                checkVJoint1.IsChecked = false;
                checkVBone1.IsChecked = false;
            }
        }

        void greenScrnBGSetup()
        {
            TheTool.change_ImgSource_inDebug(GreenScrn_bg, @"greenScrn/Background.png");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(TheURL.url_0_root + TheURL.url_1_greenScrn);
            }
            catch { }
        }

        private void checkBox1_Checked_1(object sender, RoutedEventArgs e)
        {
            if (checkHideDebug.IsChecked == true)
            {
                TheSys.debugger.Hide(); TheSys.debugger_hide = true;
            }
            else
            {
                TheSys.debugger.Show(); TheSys.debugger_hide = false;
            }
        }


        Boolean browse_FirstTime = true;
        private void butGScren_Reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                if (browse_FirstTime == true) { path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "greenScrn"); }
                Nullable<bool> openDialog = TheTool.openFileDialog(false, ".png", path, "PNG (*.png) |*.png");
                if (openDialog == true)
                {
                    //TheSys.showError(TheTool.dialog.FileName,true);
                    TheTool.change_ImgSource(GreenScrn_bg, TheTool.dialog.FileName);
                    browse_FirstTime = false;
                }
            }
            catch { }
        }

        //==============================================


        private void but_menu_mySetting(object sender, RoutedEventArgs e)
        {
            MySetting.readSetting();
            callDetector();
            if (MySetting.loadSetting)
            {
                try
                {
                    checkMute.IsChecked = TheTool.convert01_Boolean(MySetting.mute);
                    //slideAngle.Value = TheTool.getInt(MySetting.kAngle);
                    checkSeat.IsChecked = TheTool.convert01_Boolean(MySetting.kCheck_seat);
                    checkClose.IsChecked = TheTool.convert01_Boolean(MySetting.kCheck_close);
                    checkFace.IsChecked = TheTool.convert01_Boolean(MySetting.kCheck_face);
                    checkFlip.IsChecked = TheTool.convert01_Boolean(MySetting.kCheck_flip);
                    //Kinect Smooth
                    checkSmooth.IsChecked = TheTool.convert01_Boolean(MySetting.kSmooth);
                    slide_Smooth.Value = TheTool.getDouble(MySetting.kSmooth_S);
                    slide_Predict.Value = TheTool.getDouble(MySetting.kSmooth_P);
                    slide_Correct.Value = TheTool.getDouble(MySetting.kSmooth_C);
                    slide_Jitter.Value = TheTool.getDouble(MySetting.kSmooth_J);
                    slide_Davia.Value = TheTool.getDouble(MySetting.kSmooth_M);
                    //
                    checkHide.IsChecked = TheTool.convert01_Boolean(MySetting.hideTableData);
                    checkHideDebug.IsChecked = TheTool.convert01_Boolean(MySetting.hideDebug);
                    //
                    comboV1.SelectedIndex = TheTool.getInt(MySetting.v1);
                    comboV2.SelectedIndex = TheTool.getInt(MySetting.v2);
                    comboV3.SelectedIndex = TheTool.getInt(MySetting.v3);
                    comboV4.SelectedIndex = TheTool.getInt(MySetting.v4);
                    //
                    checkVJoint1.IsChecked = TheTool.convert01_Boolean(MySetting.view1_j);
                    checkVBone1.IsChecked = TheTool.convert01_Boolean(MySetting.view1_b);
                    checkVEdge1.IsChecked = TheTool.convert01_Boolean(MySetting.view1_e);
                    checkVJoint2.IsChecked = TheTool.convert01_Boolean(MySetting.view2_j);
                    checkVBone2.IsChecked = TheTool.convert01_Boolean(MySetting.view2_b);
                    checkVEdge2.IsChecked = TheTool.convert01_Boolean(MySetting.view2_e);
                    checkVJoint3.IsChecked = TheTool.convert01_Boolean(MySetting.view3_j);
                    checkVBone3.IsChecked = TheTool.convert01_Boolean(MySetting.view3_b);
                    checkVEdge3.IsChecked = TheTool.convert01_Boolean(MySetting.view3_e);
                    checkVJoint4.IsChecked = TheTool.convert01_Boolean(MySetting.view4_j);
                    checkVBone4.IsChecked = TheTool.convert01_Boolean(MySetting.view4_b);
                    checkVEdge4.IsChecked = TheTool.convert01_Boolean(MySetting.view4_e);
                    //
                    comboStream.SelectedIndex = TheTool.getInt(MySetting.stream);
                    comboFollow.SelectedIndex = TheTool.getInt(MySetting.follow);
                    txtBackup.Text = MySetting.backup;
                    comboDecimal.SelectedIndex = TheTool.getInt(MySetting.digit);
                    //--------------------
                    form_Detector.checkOnTop.IsChecked = TheTool.convert01_Boolean(MySetting.d_onTop);
                    form_Detector.checkPOSmonitor.IsChecked = TheTool.convert01_Boolean(MySetting.d_POS);
                    form_Detector.checkSound.IsChecked = TheTool.convert01_Boolean(MySetting.alarm_off);
                    form_Detector.checkSpeech.IsChecked = TheTool.convert01_Boolean(MySetting.speech_off);
                    form_Detector.txtFrameAgree.Text = MySetting.d_concensus;
                    form_Detector.txtWait.Text = MySetting.d_BPcooldown;
                }
                catch (Exception ex) { TheSys.showError("Error Load Setting:" + ex.Message); }
            }
            do_fullScreen();
            do_Smooth();
            do_track();
            slideAngle.Value = TheTool.getInt(MySetting.kAngle); sensorTilt();
        }


        private void butMySet_save_Click(object sender, RoutedEventArgs e)
        {
            MySetting.userTracker = this;
            MySetting.saveSetting();
        }


        private void butMySet_view_Click(object sender, RoutedEventArgs e)
        {
            MySetting.viewSetting();
        }

        private void checkMute_Checked_1(object sender, RoutedEventArgs e)
        {
            TheSound.mute = checkMute.IsChecked.Value;
        }

        public void loadSetting()
        {
            if (MySetting.loadSetting)
            {
                checkMute.IsChecked = TheTool.convert01_Boolean(MySetting.mute);
            }
        }

        //==============================================
        public GraphVisualizer graphVisualizer = null;

        private void butGraph_Click(object sender, RoutedEventArgs e)
        {
            graphVisualizer = new GraphVisualizer();
            graphVisualizer.Show();
        }



        private void click_butThesis(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_ows_Thesis); }
            catch { }
        }

        private void click_butECTI(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_ows_ECTI); }
            catch { }
        }

        private void click_butAPCC(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_ows_APCC); }
            catch { }
        }

        private void click_butHandbook(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_ows_Handbook); }
            catch { }
        }

        private void click_butHandbookTech(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_ows_Handbook_tech); }
            catch { }
        }

        //---------

        private void uki_butHandbook_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_uki_Handbook); }
            catch { }
        }

        private void uki_butCIG2015_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_uki_CIG2015); }
            catch { }
        }

        private void uki_butGCCE2015_1_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_uki_GCCE2015_1); }
            catch { }
        }

        private void uki_butGCCE2015_2_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(TheURL.url_uki_GCCE2015_2); }
            catch { }
        }


        //==============================================================
        //=========== Sit-Stand Detector : SST ==================
        public UKI form_UKI = null;

        void callUKI()
        {
            if (form_UKI == null)
            {
                form_UKI = new UKI(this);
                form_UKI.Show();
            }
        }

        private void butUKI_Click(object sender, RoutedEventArgs e)
        {
            callUKI();
        }

        private void but_menu_UKI(object sender, RoutedEventArgs e)
        {
            startUKI();
        }

        public void startUKI()
        {
            do_fullScreen2();
            callUKI();
            do_track();
            do_track();
            //checkFace.IsChecked = true;
            //checkVJoint1.IsChecked = false;
            //comboStream.SelectedIndex = 2; changeSteam();
        }

        private void butMAPEditor_Click(object sender, RoutedEventArgs e)
        {
            new Editor("").Show();
        }


        //=========================================================
        //------ Face Data --------------------

        public List<FaceData> listFaceData = new List<FaceData>();//Value as Number

        String get_FaceData_header(Boolean is3D)
        {
            string txt = "";
            for (int i = 1; i <= 121; i++)
            {
                if (i > 1) { txt += ","; }
                if (is3D) { txt += "J" + i + "x,J" + i + "y,J" + i + "z"; }
                else { txt += "," + "J" + i + "x,J" + i + "y"; }
            }
            return txt;
        }

        String get_FaceAU_header()
        {
            return "AU_LipRaiser,AU_JawLower,AU_LipStretcher,AU_BrowLower,AU_LipCornerDepressor,AU_BrowRaiser"; ;
        }

        void processFaceData()
        {
            if (checkFace.IsChecked.Value && form_FaceDetector != null)
            {
                try
                {
                    if (FaceTrackingViewer.current_faceData != null
                        && FaceTrackingViewer.current_faceData.skeletonTrackingState != SkeletonTrackingState.NotTracked
                        && FaceTrackingViewer.current_faceData.lastFaceTrackSucceeded)
                    {
                        FaceData newFaceData = new FaceData();
                        newFaceData.timeString = thisTimeString;
                        //////--- Raw 2D
                        //for (int i = 0; i < FaceTrackingViewer.current_faceData.facePoints.Count; i++)
                        //{
                        //    newFaceData.point_2D.Add(-FaceTrackingViewer.current_faceData.facePoints[i].X);
                        //    newFaceData.point_2D.Add(-FaceTrackingViewer.current_faceData.facePoints[i].Y);
                        //}
                        //--- Raw 3D
                        for (int i = 0; i < 121; i++)
                        {
                            Vector3DF v = FaceTrackingViewer.current_frame.Get3DShape()[i];
                            newFaceData.point_3D.Add(v.X);
                            newFaceData.point_3D.Add(v.Y);
                            newFaceData.point_3D.Add(v.Z);
                        }
                        if (FaceTrackingViewer.current_frame != null)
                        {
                            //--- AU
                            EnumIndexableCollection<AnimationUnit, float> au = FaceTrackingViewer.current_frame.GetAnimationUnitCoefficients();
                            for (int i = 0; i < 6; i++)
                            {
                                newFaceData.au_data.Add(au[i]);
                            }
                            //--- head 3D
                            newFaceData.head3D[0] = FaceTrackingViewer.current_frame.Rotation.X;
                            newFaceData.head3D[1] = FaceTrackingViewer.current_frame.Rotation.Y;
                            newFaceData.head3D[2] = FaceTrackingViewer.current_frame.Rotation.Z;
                        }

                        //-------------------------------
                        form_FaceDetector.process(newFaceData);
                        //
                        if (form_FaceDetector.collectData)
                        {
                            newFaceData.emotion = form_FaceDetector.currentEmotion;
                            if (listFaceData.Count > 0) { newFaceData.millisecFromStart = (int)(thisTime.Subtract(form_FaceDetector.startTime).TotalMilliseconds);}
                            listFaceData.Add(newFaceData);
                            form_FaceDetector.updateInfo();
                        }
                        //------------------------------
                    }
                }
                catch (Exception ex) { TheSys.showError(ex); }
            }
        }

        public void export_faceData()
        {
            string file_name = startTime.ToString("MMdd_HHmmss") + thisTime.ToString("_HHmmss");
            string path_folder_face = TheURL.url_saveFolder + file_name + "_Face";
            TheTool.Folder_CreateIfMissing(path_folder_face);
            //== Data For Analysis =======================================
            TheTool.exportCSV_orTXT(path_folder_face + @"\" + file_name + "_faceData3D.csv", getFaceData(true), false);
            //== Data For Analysis =======================================
            //TheTool.exportCSV(path_folder_face + @"\" + file_name + "_faceData2D.csv", getFaceData(false), false);
            //== Data For Analysis =======================================
            TheTool.exportCSV_orTXT(path_folder_face + @"\" + file_name + "_faceDataAU.csv", getFaceAU(), false);
            //== Data for Visualize 3D =======================================
            TheTool.exportCSV_orTXT(path_folder_face + @"\" + file_name + "_faceView3D.csv", getFaceView(true), false);
            //== Data for Visualize 2D =======================================
            //TheTool.exportCSV(path_folder_face + @"\" + file_name + "_faceView2D.csv", getFaceView(false), false);
            //== Data for Visualize 2D =======================================
            TheTool.exportCSV_orTXT(path_folder_face + @"\" + file_name + "_3D.csv", getFace3D(), false);
            //== Data for Visualize 2D =======================================
            TheTool.exportCSV_orTXT(path_folder_face + @"\" + file_name + "_faceEmotion.csv", getFaceEmotion(), false);

            System.Windows.MessageBox.Show(@"Save to '" + path_folder_face + "'", "Export CSV");
        }

        //if not = 2D
        List<String> getFaceData(Boolean is3D)
        {
            List<String> faceData = new List<String>();
            faceData.Add(get_FaceData_header(is3D));
            foreach (FaceData face in listFaceData)
            {
                String data_line = "";
                int i = 0;
                if (is3D)
                {
                    foreach (float f in face.point_3D)
                    {
                        if (i > 0) { data_line += ","; } data_line += f.ToString();
                        i++;
                    }
                }
                //else { 
                //    foreach (float f in face.point_2D) {
                //        if (i > 0) { data_line += ","; } data_line += f.ToString(); i++;
                //    } 
                //}
                faceData.Add(data_line);
            }
            return faceData;
        }

        List<String> getFaceAU()
        {
            List<String> faceData = new List<String>();
            faceData.Add(get_FaceAU_header());
            foreach (FaceData face in listFaceData)
            {
                String data_line = "";
                int i = 0;
                foreach (float f in face.au_data)
                {
                    if (i > 0) { data_line += ","; }
                    data_line += f.ToString();
                    i++;
                }
                faceData.Add(data_line);
            }
            return faceData;
        }

        List<String> getFaceView(Boolean is3D)
        {
            List<String> faceView = new List<String>();
            int range = 3;
            if (!is3D) { range = 2; }
            foreach (FaceData face in listFaceData)
            {
                faceView.Add(face.timeString);
                int i = 1;
                String data_line = "";
                List<float> targetList = face.point_3D;
                //if (!is3D) { targetList = face.point_2D; }
                foreach (float f in targetList)
                {
                    if (i % range == 0)
                    {
                        data_line += f.ToString();
                        faceView.Add(data_line); data_line = "";
                    }
                    else { data_line += f.ToString() + ","; }
                    i++;
                }
            }
            return faceView;
        }


        List<String> getFaceEmotion()
        {
            List<String> data = new List<String>();
            data.Add("Time,Emotion");
            foreach (FaceData face in listFaceData)
            {
                data.Add(face.millisecFromStart + "," + face.emotion.ToString());
            }
            return data;
        }

        List<String> getFace3D()
        {
            List<String> data = new List<String>();
            data.Add("Pitch,Yaw,Roll");
            foreach (FaceData face in listFaceData)
            {
                data.Add(face.head3D[0] + "," + face.head3D[1] + "," + face.head3D[2]);
            }
            return data;
        }
        
        //**********************************************************************
        //*** Face Detector ******************************************************
        public FaceDetector form_FaceDetector = null;

        private void butFaceDetector_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (form_FaceDetector == null)
                {
                    //comboStream.SelectedIndex = 2;
                    do_fullScreen();
                    do_track();
                    do_track();
                    checkFace.IsChecked = true;
                    form_FaceDetector = new FaceDetector(this);
                    form_FaceDetector.Show();
                }
            }
            catch (Exception ex) { TheSys.showError("FaceDetector:" + ex.Message, true); }
        }

        private void butFaceDetector2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (form_FaceDetector == null)
                {
                    checkFace.IsChecked = true;
                    form_FaceDetector = new FaceDetector(this);
                    form_FaceDetector.Show();
                }
            }
            catch (Exception ex) { TheSys.showError("FaceDetector:" + ex.Message, true); }
        }

        //**********************************************************************

    }
}
