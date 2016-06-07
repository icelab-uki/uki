using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace P_Tracker2
{
    class TheTool_Draw
    {
        static public Point pointToNewPoint(double x, double y, int angle_degree, int d)
        {
            double angle_radian = convert_Degree_Radian(angle_degree);
            //TheSys.showError(angle_degree+":"+Math.Cos(angle_degree) + "", true);
            return new Point(x + (d * Math.Cos(angle_radian)), y + (d * Math.Sin(angle_radian)));
        }

        static public double convert_Degree_Radian(int degree)
        {
            return degree * (Math.PI / 180.0);
        }
    }
}
