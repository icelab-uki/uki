using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;

namespace P_Tracker2
{
    class TheSound
    {
        public static Boolean mute = false;

        public static void soundPlay(string sound)
        {
            try
            {
                if (mute == false && sound != "")
                {
                    SoundPlayer soundPlayer = new SoundPlayer(sound);
                    soundPlayer.Play();
                }
            }
            catch {}
        }

        //public static void soundStop()
        //{
        //    try
        //    {
        //        soundPlayer.Stop();
        //    }
        //    catch { }
        //}


        //===================================================================================
        //======= SPEECH =================

        //public static SoundPlayer soundPlayer = new SoundPlayer();

        public static void speechPlay(string speech)
        {
            try
            {
                if (mute == false && speech != "")
                {
                    SoundPlayer soundPlayer = new SoundPlayer(speech);
                    soundPlayer.Play();
                }
            }
            catch { }
        }

    }
}
