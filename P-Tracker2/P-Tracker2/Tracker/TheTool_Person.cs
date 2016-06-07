using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace P_Tracker2
{
    class TheTool_Person
    {

        //======================== Getting Data ==================
        //Default Mode = 20
        //Seat Mode = 10
        //Lossy Mode = 15
        //Seat Lossy Mode = 8

        /*
        * Head;
        * ShoulderCenter;
        * ShoulderLeft;
        * ShoulderRight;
        * ElbowLeft;
        * ElbowRight;
        * WristLeft;
        * WristRight;
        * HandLeft;
        * HandRight;
        * Spine;
        * HipCenter;
        * HipLeft;
        * HipRight;
        * KneeLeft;
        * KneeRight;
        * AnkleLeft;
        * AnkleRight;
        * FootLeft;
        * FootRight;
        */

        static public List<String> get_personAttribute_name (Boolean mode_seat,Boolean lossy)
        {
            List<String> list_attribute_name = new List<String> {};
            list_attribute_name.Add("Head");
            list_attribute_name.Add("ShoulderCenter");
            list_attribute_name.Add("ShoulderLeft");
            list_attribute_name.Add("ShoulderRight");
            list_attribute_name.Add("ElbowLeft");
            list_attribute_name.Add("ElbowRight");
            list_attribute_name.Add("WristLeft");
            list_attribute_name.Add("WristRight");
            if (lossy == false) { 
                list_attribute_name.Add("HandLeft");
                list_attribute_name.Add("HandRight");
            }
            if (mode_seat == false)
            {
                list_attribute_name.Add("Spine");
                if (lossy == false)
                {
                    list_attribute_name.Add("HipCenter");
                }
                list_attribute_name.Add("HipLeft");
                list_attribute_name.Add("HipRight");
                list_attribute_name.Add("KneeLeft");
                list_attribute_name.Add("KneeRight");
                list_attribute_name.Add("AnkleLeft");
                list_attribute_name.Add("AnkleRight");
                if (lossy == false)
                {
                    list_attribute_name.Add("FootLeft");
                    list_attribute_name.Add("FootRight");
                }
            }
            return list_attribute_name;
        }

        static public List<double[]> get_personAttribute_data(Person p, Boolean mode_seat, Boolean lossy)
        {
             List<double[]> list_attribute_data = new List<double[]>{};
             list_attribute_data.Add(p.Head);
             list_attribute_data.Add(p.ShoulderCenter);
             list_attribute_data.Add(p.ShoulderLeft);
             list_attribute_data.Add(p.ShoulderRight);
             list_attribute_data.Add(p.ElbowLeft);
             list_attribute_data.Add(p.ElbowRight);
             list_attribute_data.Add(p.WristLeft);
             list_attribute_data.Add(p.WristRight);
             if (lossy == false)
             {
                 list_attribute_data.Add(p.HandLeft);
                 list_attribute_data.Add(p.HandRight);
             }
             if (mode_seat == false)
             {
                 list_attribute_data.Add(p.Spine);
                 if (lossy == false)
                 {
                     list_attribute_data.Add(p.HipCenter);
                 }
                 list_attribute_data.Add(p.HipLeft);
                 list_attribute_data.Add(p.HipRight);
                 list_attribute_data.Add(p.KneeLeft);
                 list_attribute_data.Add(p.KneeRight);
                 list_attribute_data.Add(p.AnkleLeft);
                 list_attribute_data.Add(p.AnkleRight);
                 if (lossy == false)
                 {
                     list_attribute_data.Add(p.FootLeft);
                     list_attribute_data.Add(p.FootRight);
                 }
             }
             return list_attribute_data;
        }


        //------------------------------------------------
        //--- for make PersonString ----

        public static string trackPersonID = "A";

        static public PersonString getPersonString(Person thePerson)
        {
            PersonString jointString = new PersonString()
            {
                //=================== No Null Possible =====================
                TimeTrack = thePerson.TimeTrack,
                //-------------------
                Head = getString(thePerson.Head),
                ShoulderCenter = getString(thePerson.ShoulderCenter),
                //--------
                ShoulderLeft = getString(thePerson.ShoulderLeft),
                ElbowLeft = getString(thePerson.ElbowLeft),
                WristLeft = getString(thePerson.WristLeft),
                HandLeft = getString(thePerson.HandLeft),
                //--------
                ShoulderRight = getString(thePerson.ShoulderRight),
                ElbowRight = getString(thePerson.ElbowRight),
                WristRight = getString(thePerson.WristRight),
                HandRight = getString(thePerson.HandRight),
                //-------------------
                Spine = getString(thePerson.Spine),
                HipCenter = getString(thePerson.HipCenter),
                HipLeft = getString(thePerson.HipLeft),
                KneeLeft = getString(thePerson.KneeLeft),
                AnkleLeft = getString(thePerson.AnkleLeft),
                FootLeft = getString(thePerson.FootLeft),
                //--------
                HipRight = getString(thePerson.HipRight),
                KneeRight = getString(thePerson.KneeRight),
                AnkleRight = getString(thePerson.AnkleRight),
                FootRight = getString(thePerson.FootRight)
            };
            return jointString;
        }

        //Double[] to 1 String
        static public string getString(double[] a)
        {
            return string.Join(" ", a );
        }

        
    }
}
