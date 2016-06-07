using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace P_Tracker2
{
    class TheTest
    {
        //void UKI_process()
        //{
        //    try
        //    {
        //        ////========find MinMax from UKI data =====================================
        //        //string path_input = TheURL.url_saveFolder + "test.csv";
        //        //string path_output = TheURL.url_saveFolder + "result.csv";
        //        //List<String> data_0 = TheTool.read_File_getListString(TheURL.url_saveFolder + "test.csv");
        //        //List<String> data_1 = ThePosExtract.calMinimaMaxima(data_0, 0);
        //        //data_1 = TheCSVdata.data_adjustRow(data_1, 0, 5, 5);
        //        //TheTool.exportCSV(path_output, data_1, false);

        //        ////======== Create MinMax file from .csv of all data =====================================
        //        //DataTable dt = CSVReader.ReadCSVFile(TheURL.url_saveFolder + "UKI0816_142028_PE.csv", true);
        //        //DataTable dt2 = TheCSVdata.dataTable_cropCol(dt, "p_jump", "");//select Col
        //        //DataTable dt_mm = TheCSVdata.dataTable_getMaxMinTable(dt2);//generate MM table
        //        //TheTool.export_dataTable_to_CSV(TheURL.url_saveFolder + "UKI0816_142028_MM.csv", dt_mm);

        //        ////======== PR: Variable Extraction - Test sample2.csv with [MinMax].csv =====================================
        //        string path_UKI_pe = TheURL.url_saveFolder + "sample.csv";
        //        string path_GlobalM = TheURL.url_saveFolder + "[MinMax].csv";
        //        List<String> data_1 = TheTool.read_File_getListString(path_UKI_pe);
        //        DataTable dt_mm = CSVReader.ReadCSVFile(path_GlobalM, true);
        //        TheTool.export_dataTable_to_CSV(TheURL.url_saveFolder + "temp_mm.csv", dt_mm);
        //        //--- Normalize -----
        //        List<String> data_2 = ThePosExtract.getKeyPosture(data_1);//select row                
        //        List<String> data_raw = TheTool.data_cropCol(data_2, "p_jump", "");//select col
        //        List<String> data_normal = ThePosExtract.process_Normaliz(data_raw, dt_mm);
        //        //--- Final Process ** Must use Copy List ** -----
        //        List<String> data_raw_copy = new List<String>(); data_raw_copy.AddRange(data_raw);
        //        List<String> data_normal_copy = new List<String>(); data_normal_copy.AddRange(data_normal);
        //        List<String> data_raw_change = ThePosExtract.process_calChange(data_raw_copy, false, false, true);
        //        List<String> data_normal_change = ThePosExtract.process_calChange(data_normal_copy, true, true, false);
        //        //---
        //        List<String> data_final = new List<String>();
        //        data_final.Add("RAW");
        //        data_final.AddRange(data_raw_change);
        //        data_final.Add("");
        //        data_final.Add("NORMALIZED");
        //        data_final.AddRange(data_normal_change);
        //        //--- Print -----
        //        TheTool.exportCSV(TheURL.url_saveFolder + "temp_result.csv", data_final, false);
        //        TheTool.exportCSV(TheURL.url_saveFolder + "temp_2.csv", data_2, false);
        //        TheTool.exportCSV(TheURL.url_saveFolder + "temp_r.csv", data_raw, false);
        //        TheTool.exportCSV(TheURL.url_saveFolder + "temp_rc.csv", data_raw_change, false);
        //        TheTool.exportCSV(TheURL.url_saveFolder + "temp_n.csv", data_normal, false);
        //        TheTool.exportCSV(TheURL.url_saveFolder + "temp_nc.csv", data_normal_change, false);
        //    }
        //    catch (Exception ex) { TheSys.showError(ex.ToString()); }
        //}

        //void testImport()
        //{
        //    List<UKI_DataAngular> list_data2 = TheUKI.csv_loadFileTo_DataAngular(TheURL.url_saveFolder + "test002.csv");
        //    foreach (UKI_DataAngular d in list_data2)
        //    {
        //        TheSys.showError(d.i + "_" + d.Head[0]);
        //    }
        //    //---------
        //    List<UKI_DataAngular> list_data1 = TheUKI.csv_loadFileTo_DataAngular(TheURL.url_saveFolder + "test001.csv");
        //    foreach (UKI_DataAngular d in list_data1)
        //    {
        //        TheSys.showError(d.i + "_" + d.Head[0]);
        //    }
        //}

        //void testCalEntropy_Angle()
        //{
        //    try
        //    {
        //        DataTable dt = CSVReader.ReadCSVFile(TheURL.url_saveFolder + "test.csv", true);//prepare Data
        //        dt = TheTool.dataTable_cropCol(dt, 2, 0);//del Col
        //        TheTool.dataTable_roundValue(dt, 0);// eliminate decimal
        //        //
        //        List<int> KeyPostureAt = new List<int> { 0, 45, 114, 169, 231 };
        //        List<string> final_output = new List<string>();//Header & Data
        //        final_output.Add("id," + TheUKI.getHeader("_H"));//Header
        //        //
        //        int start = -1; int end = 0;
        //        foreach (int i in KeyPostureAt)
        //        {
        //            end = i;
        //            if (start > -1)
        //            {
        //                DataTable dt_sub = TheTool.dataTable_selectRow(dt, start, end);
        //                final_output.Add(start + " - " + end + ","
        //                    + TheExternalDataConverter.MSR_calEntrophy_1Action(dt_sub, 2));
        //            }
        //            start = i;
        //        }
        //        //
        //        TheTool.exportCSV(TheURL.url_saveFolder + "testResult.csv", final_output, false);
        //    }
        //    catch (Exception ex) { TheSys.showError(ex); }
        //}
    }
}
