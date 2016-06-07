using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    public class FaceData
    {
        public string timeString = "";
        //https://msdn.microsoft.com/en-us/library/jj130970.aspx
        //121 points on face
        public List<float> point_3D = new List<float>();//J1x,J1y,J1z,J2x,J2y,J2z
        //public List<float> point_2D = new List<float>();//J1x,J1y,J2x,J2y
        public List<float> au_data = new List<float>();//AU_LipRaiser,AU_JawLower,AU_LipStretcher,AU_BrowLower,AU_LipCornerDepressor,AU_BrowRaiser
        public int emotion = 0;
        public int millisecFromStart = 0;

        public double[] head3D = new double[3];//Pitch, Yaw, Roll (X,Y,Z)

        /* 121 Joints  m
        TopSkull = 0,
        TopRightForehead = 1,
        MiddleTopDipUpperLip = 7,
        AboveChin = 9,
        BottomOfChin = 10,
        RightOfRightEyebrow = 15,
        MiddleTopOfRightEyebrow = 16,
        LeftOfRightEyebrow = 17,
        MiddleBottomOfRightEyebrow = 18,
        AboveMidUpperRightEyelid = 19,
        OuterCornerOfRightEye = 20,
        MiddleTopRightEyelid = 21,
        MiddleBottomRightEyelid = 22,
        InnerCornerRightEye = 23,
        UnderMidBottomRightEyelid = 24,
        RightSideOfChin = 30,
        OutsideRightCornerMouth = 31,
        RightOfChin = 32,
        RightTopDipUpperLip = 33,
        TopLeftForehead = 34,
        MiddleTopLowerLip = 40,
        MiddleBottomLowerLip = 41,
        LeftOfLeftEyebrow = 48,
        MiddleTopOfLeftEyebrow = 49,
        RightOfLeftEyebrow = 50,
        MiddleBottomOfLeftEyebrow = 51,
        AboveMidUpperLeftEyelid = 52,
        OuterCornerOfLeftEye = 53,
        MiddleTopLeftEyelid = 54,
        MiddleBottomLeftEyelid = 55,
        InnerCornerLeftEye = 56,
        UnderMidBottomLeftEyelid = 57,
        LeftSideOfCheek = 63,
        OutsideLeftCornerMouth = 64,
        LeftOfChin = 65,
        LeftTopDipUpperLip = 66,
        OuterTopRightPupil = 67,
        OuterBottomRightPupil = 68,
        OuterTopLeftPupil = 69,
        OuterBottomLeftPupil = 70,
        InnerTopRightPupil = 71,
        InnerBottomRightPupil = 72,
        InnerTopLeftPupil = 73,
        InnerBottomLeftPupil = 74,
        RightTopUpperLip = 79,
        LeftTopUpperLip = 80,
        RightBottomUpperLip = 81,
        LeftBottomUpperLip = 82,
        RightTopLowerLip = 83,
        LeftTopLowerLip = 84,
        RightBottomLowerLip = 85,
        LeftBottomLowerLip = 86,
        MiddleBottomUpperLip = 87,
        LeftCornerMouth = 88,
        RightCornerMouth = 89,
        BottomOfRightCheek = 90,
        BottomOfLeftCheek = 91,
        AboveThreeFourthRightEyelid = 95,
        AboveThreeFourthLeftEyelid = 96,
        ThreeFourthTopRightEyelid = 97,
        ThreeFourthTopLeftEyelid = 98,
        ThreeFourthBottomRightEyelid = 99,
        ThreeFourthBottomLeftEyelid = 100,
        BelowThreeFourthRightEyelid = 101,
        BelowThreeFourthLeftEyelid = 102,
        AboveOneFourthRightEyelid = 103,
        AboveOneFourthLeftEyelid = 104,
        OneFourthTopRightEyelid = 105,
        OneFourthTopLeftEyelid = 106,
        OneFourthBottomRightEyelid = 107,
        OneFourthBottomLeftEyelid = 108,
         */
    }
}
