using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheData
    {
        //Base contain instance at base
        public static Feature getFeature(string selected_feature, DataTable dt_threshold_pose1, DataTable dt_threshold_pose2)
        {
            Feature f = new Feature();
            f.name = selected_feature;
            //try
            //{
                double start = TheTool.dataTable_getAverage(dt_threshold_pose1, selected_feature);
                double end = TheTool.dataTable_getAverage(dt_threshold_pose2, selected_feature);
                double diff = start + end;
                f.v = Math.Round(diff / 2, 2);
                if (end > start) { f.opt = ">="; }
                else { f.opt = "<="; }
                //----------
                if (start > end)
                {
                    f.ceiling = Math.Round(start, 2);
                    f.floor = Math.Round(end, 2);
                }
                f.u = f.ceiling;
                f.l = f.u;
                f.momentum = Math.Round(Math.Abs(diff) / 4, 2);
            //}
            //catch (Exception ex) { TheSys.showError(ex); }
            return f;
        }
    }
}
