using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class Regression
    {

        public static double RegressionFct(DataTable dt, string str1, string str2)
        {
            List<double>    str2Value = new List<double>();
            List<double>    str1Value = new List<double>();

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
            
            return (LinearRegression(str1Value.ToArray(), str2Value.ToArray(), str1Value.Count, str2Value.Count));
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
    }
}
