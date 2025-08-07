using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class UploadFileController : Controller
    {
        public GSDbContext GSDbContext { get; set; }
        public GSDbContextGSTrack GSDbContextGSTrack { get; set; }

        public UploadFileController()
        {
            //GSDbContext = new GSDbContext(@"DEV-KRW\SQLEXPRESS", "db_marketing_portal", "sa", "gsmis@2017");

            GSDbContextGSTrack = new GSDbContextGSTrack(@"", "", "", "");
            GSDbContext = new GSDbContext(@"", "", "", "");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
            GSDbContextGSTrack.Dispose();
        }
        [SessionCheck]
        public ActionResult DownloadOfferingLetter(string[] ID_ORDER)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                string custName = "";
                GSDbContext = new GSDbContext(@"GSPORTAL-DEV01", "db_marketing_portal", "sa", "gsmis@2017");
                if (ID_ORDER != null)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                    worksheet.View.ShowGridLines = false;
                    var idquot = ID_ORDER[0].Split(',');
                    var bpid = "";
                    var exc_rate = 0;
                    var lme = 0M;
                    var plastic_pp = 0M;
                    string periode = "";
                    string before_periode = "";
                    string next_periode = "";
                    string avg_periode = "";
                    string PN_cust = "";
                    for (int i=0; i < idquot.Length; i++)
                    {
                        var ssqlQuery = "SELECT * FROM db_marketing_portal.dbo.t_price_quotation WHERE id = '" + idquot[i] + "' ";
                        var sGetBPID = GSDbContext.Database.SqlQuery<ManagePriceQuotation>(ssqlQuery).FirstOrDefault();
                        if(bpid != sGetBPID.customer_bpid)
                        {
                            bpid = sGetBPID.customer_bpid;
                            periode = sGetBPID.quotation_period;
                            exc_rate = sGetBPID.ex_rate;
                            lme = (decimal)sGetBPID.LME_lead;
                            plastic_pp = (decimal)sGetBPID.plastic_pp;
                            if(bpid != null)
                            {
                                var sQuerys = "SELECT * FROM db_marketing_portal.dbo.[tlkp_PN_OEM] WHERE pn_gs = '" + sGetBPID.part_number + "' ";
                                var sGetPN = GSDbContext.Database.SqlQuery<ManagePartNumberOEM>(sQuerys).FirstOrDefault();
                                if(sGetPN != null)
                                {
                                    PN_cust = sGetPN.pn_customer;
                                }
                            }
                        }

                        //harusnya ini looping dari data yg di select
                        worksheet.Cells[15 + i + 1, 2].Value = i+1;
                        worksheet.Cells[15 + i + 1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells[15 + i + 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[15 + i + 1, 3].Value = sGetBPID.battery_type;
                        worksheet.Cells[15 + i + 1, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        worksheet.Cells[15 + i + 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[15 + i + 1, 4].Value = PN_cust;
                        worksheet.Cells[15 + i + 1, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        worksheet.Cells[15 + i + 1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[15 + i + 1, 6].Value = sGetBPID.grand_total;
                        worksheet.Cells[15 + i + 1, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells[15 + i + 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        //sampai sini

                        worksheet.Cells[15 + i + 1, 6].Style.Numberformat.Format = "#,##0";
                    }

                    #region FORMATING EXCEL
                    Int32 untilrow = 15 + idquot.Length;
                    using (var range = worksheet.Cells[15, 2, untilrow, 6])
                    {
                        ////range.Style.Font.Bold = true;
                        //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //range.Style.Fill.BackgroundColor.SetColor(Color.White);
                        //range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thick;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                        range.Style.Border.Diagonal.Style = ExcelBorderStyle.Thick;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //range.Style.Font.Color.SetColor(Color.White);
                    }

                    var allCells = worksheet.Cells[1, 1, 65, 7];
                    var cellFont = allCells.Style.Font;
                    cellFont.SetFromFont(new Font("Calibri Light", 11));
                    //cellFont.Bold = true;
                    //cellFont.Italic = true;
                    //worksheet.Cells["E16:F19"].Style.Numberformat.Format = "#,##0";

                    worksheet.Cells["B15:F15"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["B15:F15"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["B15:F15"].Style.Font.Bold = true;
                    worksheet.Cells["B15:F15"].Style.Font.Italic = true;
                    #endregion

                    var sQuery = "SELECT * FROM db_marketing_portal.dbo.[tlkp_attn_OEM] WHERE [attn_bp_code] = '" + bpid + "' ";
                    var sGetAttn = GSDbContext.Database.SqlQuery<MasterAttn>(sQuery).FirstOrDefault();
                    ////cek data Attention
                    //if (sGetAttn != null)
                    //{

                    //}

                    worksheet.Cells[2, 1].Value = sGetAttn.attn_yth;
                    //worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //worksheet.Cells[2, 1].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                    worksheet.Cells[3, 1].Value = sGetAttn.attn_customer;
                    worksheet.Cells[4, 1].Value = sGetAttn.attn_address;
                    worksheet.Cells[5, 1].Value = sGetAttn.attn_to + " " + sGetAttn.attn_name;
                    worksheet.Cells[8, 1].Value = "Re: Price List of GS Battery period " + periode + " - Process Cost Up";
                    worksheet.Cells[10, 1].Value = "Dear Sir,";
                    worksheet.Cells[11, 1].Value = "First and foremost, we would like to thank you for your ongoing cooperation with us. Refer to the price adjustment";
                    worksheet.Cells["A2:F2"].Merge = true;
                    worksheet.Cells["A3:F3"].Merge = true;
                    worksheet.Cells["A4:F4"].Merge = true;
                    worksheet.Cells["A5:F5"].Merge = true;
                    worksheet.Cells["A8:F8"].Merge = true;
                    worksheet.Cells["A11:G11"].Merge = true;
                    worksheet.Cells["A12:G12"].Merge = true;
                    worksheet.Cells["A13:F13"].Merge = true;
                    worksheet.Cells[12, 1].Value = "formula established and the latest movement of lead price which is significantly high, we would like to propose";
                    worksheet.Cells[13, 1].Value = "new price list valid from " + periode;

                    worksheet.Cells[15, 2].Value = "No";
                    worksheet.Cells[15, 3].Value = "Battery Type";
                    worksheet.Cells[15, 4].Value = "Part No.";
                    worksheet.Cells[15, 5].Value = "Old Price ( " + periode + " )";
                    worksheet.Cells[15, 6].Value = "New Price ( " + periode + " )";

                    worksheet.Cells[18 + idquot.Length, 1].Value = "Condition:";
                    worksheet.Cells[18 + idquot.Length, 1].Style.Font.Bold = true;
                    worksheet.Cells[19 + idquot.Length, 1, 19 + idquot.Length, 6].Merge = true;
                    worksheet.Cells[20 + idquot.Length, 1, 20 + idquot.Length, 6].Merge = true;
                    worksheet.Cells[21 + idquot.Length, 2, 21 + idquot.Length, 4].Merge = true;
                    //worksheet.Cells["A22:F22"].Merge = true;
                    //worksheet.Cells["A23:F23"].Merge = true;
                    //worksheet.Cells["B24:D24"].Merge = true;
                    worksheet.Cells[19 + idquot.Length, 1].Value = "1. VAT 11% is excluded in price above";
                    worksheet.Cells[20 + idquot.Length, 1].Value = "2. The above price are based on:";
                    worksheet.Cells[21 + idquot.Length, 2].Value = "a. LME and Exchange Rate for period: Average " + avg_periode;
                    //worksheet.Cells["C25:D27"].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[22 + idquot.Length, 3].Value = "Exc. Rate   : ";
                    worksheet.Cells[23 + idquot.Length, 3].Value = "LME           : ";
                    worksheet.Cells[24 + idquot.Length, 3].Value = "PP              : ";
                    worksheet.Cells[22 + idquot.Length, 4, 24 + idquot.Length, 4].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[22 + idquot.Length, 4].Value = exc_rate;
                    worksheet.Cells[23 + idquot.Length, 4].Value = lme;
                    worksheet.Cells[24 + idquot.Length, 4].Value = plastic_pp;
                    worksheet.Cells[22 + idquot.Length, 5].Value = " /USD";
                    worksheet.Cells[23 + idquot.Length, 5].Value = " USD/MT";
                    worksheet.Cells[24 + idquot.Length, 5].Value = " USD/MT";
                    worksheet.Cells[25 + idquot.Length, 2, 25 + idquot.Length, 6].Merge = true;
                    worksheet.Cells[26 + idquot.Length, 2, 26 + idquot.Length, 6].Merge = true;
                    worksheet.Cells[25 + idquot.Length, 2].Value = "b. Other raw material price used in battery manufacturer.";
                    worksheet.Cells[26 + idquot.Length, 2].Value = "c. Government regulation on monetary, taxation, transportation, etc.";
                    //cek sampai sini
                    worksheet.Cells[27 + idquot.Length, 1, 27 + idquot.Length, 6].Merge = true;
                    worksheet.Cells[28 + idquot.Length, 1, 28 + idquot.Length, 6].Merge = true;
                    worksheet.Cells[29 + idquot.Length, 1, 29 + idquot.Length, 6].Merge = true;
                    //worksheet.Cells["B29:F29"].Merge = true;
                    //worksheet.Cells["A30:F30"].Merge = true;
                    //worksheet.Cells["A31:F31"].Merge = true;
                    //worksheet.Cells["A32:F32"].Merge = true;
                    worksheet.Cells[27 + idquot.Length, 1].Value = "3. The next price adjustment (" + next_periode + "---) will be based on the LME & Exchange rate of Mar-May'23.";
                    worksheet.Cells[28 + idquot.Length, 1].Value = "4. Term of payment is 30 days after delivery.";
                    worksheet.Cells[29 + idquot.Length, 1].Value = "5. Prices are subject to change with prior notice and other conditions are subject to discuss";



                    //var fileLocation = System.IO.File.ReadAllText(Server.MapPath(@"~\Content\json_data\StatusOrder.json"));
                    //List<StatusOrder> myDeserializedObjList = (List<StatusOrder>)Newtonsoft.Json.JsonConvert.DeserializeObject(fileLocation, typeof(List<StatusOrder>));

                    
                    custName = sGetAttn.attn_customer;

                    worksheet.Cells[33 + idquot.Length, 1, 33 + idquot.Length, 4].Merge = true;
                    worksheet.Cells[38 + idquot.Length, 1, 38 + idquot.Length, 4].Merge = true;
                    worksheet.Cells[39 + idquot.Length, 1, 39 + idquot.Length, 4].Merge = true;
                    worksheet.Cells[41 + idquot.Length, 1, 41 + idquot.Length, 4].Merge = true;
                    worksheet.Cells[42 + idquot.Length, 1, 42 + idquot.Length, 4].Merge = true;
                    //worksheet.Cells["A36:D36"].Merge = true;
                    //worksheet.Cells["A41:D41"].Merge = true;
                    //worksheet.Cells["A42:D42"].Merge = true;
                    //worksheet.Cells["A44:D44"].Merge = true;
                    //worksheet.Cells["A45:D45"].Merge = true;
                    worksheet.Cells[33 + idquot.Length, 1].Value = "Sincerely yours";
                    worksheet.Cells[33 + idquot.Length, 7].Value = "Agreed by:";
                    worksheet.Cells[38 + idquot.Length, 1].Value = "Sigit Wibisono";
                    worksheet.Cells[38 + idquot.Length, 1].Style.Font.UnderLine = true;
                    worksheet.Cells[39 + idquot.Length, 1].Value = "Marketing Director";
                    var attn_name = sGetAttn.attn_name.Split('(');
                    worksheet.Cells[38 + idquot.Length, 7].Value = attn_name[0];
                    worksheet.Cells[39 + idquot.Length, 7].Value = "(" + attn_name[1];


                    //var cc = worksheet.Cells["A44:A45"];
                    //var ccFont = cc.Style.Font;
                    //ccFont.SetFromFont(new Font("Calibri Light", 8));
                    worksheet.Cells[41 + idquot.Length, 1].Value = "cc. Mr. Kusharijono ( President Director PT. GS Battery )";
                    worksheet.Cells[42 + idquot.Length, 1].Value = "    Mr. Masanori Kitamura ( Vice President Director PT. GS Battery )";
                    worksheet.Cells[41 + idquot.Length, 1].Style.Font.SetFromFont(new Font("Calibri Light", 8));
                    worksheet.Cells[42 + idquot.Length, 1].Style.Font.SetFromFont(new Font("Calibri Light", 8));
                    worksheet.Cells.AutoFitColumns(0);
                }
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "[PriceQuotation]" + custName + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".xlsx");

            }
        }

        [SessionCheck]
        public ActionResult DownloadQuotationBy_ID(string ID_ORDER)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                worksheet.View.ShowGridLines = false;

                #region FORMATING EXCEL
                using (var range = worksheet.Cells[6, 2, 65, 2])
                {
                    ////range.Style.Font.Bold = true;
                    //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Fill.BackgroundColor.SetColor(Color.White);
                    //range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                    range.Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                using (var range = worksheet.Cells[6, 3, 65, 3])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                #endregion

                var allCells = worksheet.Cells[1, 1, 65, 3];
                var cellFont = allCells.Style.Font;
                cellFont.SetFromFont(new Font("Calibri Light", 11));
                //cellFont.Bold = true;
                //cellFont.Italic = true;
                worksheet.Cells["C12:C65"].Style.Numberformat.Format = "#,##0";

                var dedesc = worksheet.Cells["B4:B5"];
                var peper = worksheet.Cells["C4:C5"];
                dedesc.Value = "DESCRIPTION";
                dedesc.Merge = true;
                worksheet.Cells["B4:C5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["B4:C5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B4:C5"].Style.Font.Bold = true;
                dedesc.Style.Border.Top.Style = ExcelBorderStyle.Thick;
                dedesc.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                dedesc.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                dedesc.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                peper.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                peper.Style.Border.Right.Style = ExcelBorderStyle.Thick;

                worksheet.Cells[1 + 3, 3].Value = "Price Period";
                worksheet.Cells[1 + 3, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1 + 3, 3].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[2 + 6, 2].Value = "Customer Name";
                worksheet.Cells[3 + 6, 2].Value = "Battery Type";
                worksheet.Cells[4 + 6, 2].Value = "Part Number";
                worksheet.Cells[5 + 7, 2].Value = "LME Lead";
                worksheet.Cells[6 + 7, 2].Value = "PREMIUM 1";
                worksheet.Cells[7 + 7, 2].Value = "PREMIUM 2";
                worksheet.Cells[8 + 7, 2].Value = "PREMIUM 3";
                worksheet.Cells[9 + 7, 2].Value = "PLASTIC PP";
                worksheet.Cells[10 + 7, 2].Value = "EX. RATE";

                worksheet.Cells[10 + 10, 2].Value = "MATERIAL COST 1";
                worksheet.Cells[10 + 10, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[10 + 10, 2].Style.Font.Bold = true;
                worksheet.Cells[11 + 10, 2].Value = "WEIGHT1";
                worksheet.Cells[12 + 10, 2].Value = "Import Duty";
                worksheet.Cells[13 + 10, 2].Value = "Handling Fee";
                worksheet.Cells[14 + 10, 2].Value = "Lead Price Premium fee 1";
                worksheet.Cells[15 + 10, 2].Value = "Lead Premium 1";
                worksheet.Cells["B24:B25"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B24:B25"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#C6E0B4"));

                worksheet.Cells[10 + 17, 2].Value = "MATERIAL COST 2";
                worksheet.Cells[10 + 17, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[10 + 17, 2].Style.Font.Bold = true;
                worksheet.Cells[16 + 12, 2].Value = "WEIGHT2";
                worksheet.Cells[17 + 12, 2].Value = "Import Duty";
                worksheet.Cells[18 + 12, 2].Value = "Handling Fee";
                worksheet.Cells[19 + 12, 2].Value = "Lead Price Premium fee 2";
                worksheet.Cells[20 + 12, 2].Value = "Lead Premium 2";
                worksheet.Cells["B31:B32"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B31:B32"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#C6E0B4"));

                worksheet.Cells[10 + 24, 2].Value = "MATERIAL COST 3";
                worksheet.Cells[10 + 24, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[10 + 24, 2].Style.Font.Bold = true;
                worksheet.Cells[21 + 14, 2].Value = "WEIGHT3";
                worksheet.Cells[22 + 14, 2].Value = "Import Duty";
                worksheet.Cells[23 + 14, 2].Value = "Handling Fee";
                worksheet.Cells[24 + 14, 2].Value = "Lead Price Premium fee 3";
                worksheet.Cells[25 + 14, 2].Value = "Lead Premium 3";
                worksheet.Cells["B38:B39"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B38:B39"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#C6E0B4"));

                worksheet.Cells[10 + 31, 2].Value = "MATERIAL COST PLASTIC";
                worksheet.Cells[10 + 31, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[10 + 31, 2].Style.Font.Bold = true;
                worksheet.Cells[26 + 16, 2].Value = "WEIGHT";
                worksheet.Cells[27 + 16, 2].Value = "Import Duty";
                worksheet.Cells[28 + 16, 2].Value = "Handling Fee";
                worksheet.Cells[29 + 16, 2].Value = "PP Price";
                worksheet.Cells[30 + 16, 2].Value = "PLASTIC";
                worksheet.Cells["B45:B46"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B45:B46"].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#C6E0B4"));

                worksheet.Cells[31 + 17, 2].Value = "SEPARATOR";
                worksheet.Cells[32 + 17, 2].Value = "Others Purchase Part";
                worksheet.Cells[33 + 17, 2].Value = "Sub Total Material Cost";

                worksheet.Cells[10 + 42, 2].Value = "PROCESS COST";
                worksheet.Cells[10 + 42, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[10 + 42, 2].Style.Font.Bold = true;
                worksheet.Cells[34 + 19, 2].Value = "PLATE";
                worksheet.Cells[35 + 19, 2].Value = "INJECTION";
                worksheet.Cells[36 + 19, 2].Value = "ASSEMBLING";
                worksheet.Cells[37 + 19, 2].Value = "CHARGING";
                worksheet.Cells[38 + 19, 2].Value = "Sub Total Process Cost";

                worksheet.Cells[39 + 20, 2].Value = "TOTAL";

                worksheet.Cells[40 + 21, 2].Value = "GENERAL CHARGE (5%)";
                worksheet.Cells[41 + 21, 2].Value = "Others (Depreciate/Tooling)";
                worksheet.Cells[42 + 21, 2].Value = "Support (CR)";

                worksheet.Cells[43 + 22, 2].Value = "GRAND TOTAL";
                worksheet.Cells[43 + 22, 2].Style.Font.Bold = true;
                worksheet.Cells[43 + 22, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                //var fileLocation = System.IO.File.ReadAllText(Server.MapPath(@"~\Content\json_data\StatusOrder.json"));
                //List<StatusOrder> myDeserializedObjList = (List<StatusOrder>)Newtonsoft.Json.JsonConvert.DeserializeObject(fileLocation, typeof(List<StatusOrder>));

                var sqlQuery = "SELECT * FROM db_marketing_portal.dbo.t_price_quotation WHERE id = '" + ID_ORDER + "' ";

                GSDbContext = new GSDbContext(@"GSPORTAL-DEV01", "db_marketing_portal", "sa", "gsmis@2017");

                var resultTable = GSDbContext.Database.SqlQuery<ManagePriceQuotation>(sqlQuery).ToList();
                //var i = 1;
                var colu = 3;
                string custName = "";

                foreach (var data in resultTable)
                {
                    worksheet.Cells[1 + 4, colu].Value = data.quotation_period.ToString();
                    worksheet.Cells[1 + 4, colu].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1 + 4, colu].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    worksheet.Cells[1 + 4, colu].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1 + 4, colu].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    worksheet.Cells[2 + 6, colu].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[2 + 6, colu].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    worksheet.Cells[2 + 6, colu].Value = data.customer_name.ToString();
                    worksheet.Cells[2 + 6, colu].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2 + 6, colu].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[2 + 6, colu].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    custName = data.customer_name.ToString();
                    worksheet.Cells[3 + 6, colu].Value = data.battery_type.ToString();
                    worksheet.Cells[4 + 6, colu].Value = data.part_number.ToString();
                    worksheet.Cells[5 + 7, colu].Value = data.LME_lead.ToString();
                    worksheet.Cells[6 + 7, colu].Value = data.premium1.ToString();
                    worksheet.Cells[7 + 7, colu].Value = data.premium2.ToString();
                    worksheet.Cells[8 + 7, colu].Value = data.premium3.ToString();
                    worksheet.Cells[9 + 7, colu].Value = data.plastic_pp.ToString();
                    worksheet.Cells[10 + 7, colu].Value = data.ex_rate.ToString();
                    worksheet.Cells[11 + 10, colu].Value = data.material_weight1.ToString();
                    worksheet.Cells[12 + 10, colu].Value = data.import_duty.ToString() + "%";
                    worksheet.Cells[13 + 10, colu].Value = data.handling_fee.ToString() + "%";
                    worksheet.Cells[14 + 10, colu].Value = data.lpp_fee1.ToString();
                    worksheet.Cells[15 + 10, colu].Value = data.lead_premium1.ToString();
                    worksheet.Cells[16 + 12, colu].Value = data.material_weight2.ToString();
                    worksheet.Cells[17 + 12, colu].Value = data.import_duty2.ToString() + "%";
                    worksheet.Cells[18 + 12, colu].Value = data.handling_fee2.ToString() + "%";
                    worksheet.Cells[19 + 12, colu].Value = data.lpp_fee2.ToString();
                    worksheet.Cells[20 + 12, colu].Value = data.lead_premium2.ToString();
                    worksheet.Cells[21 + 14, colu].Value = data.material_weight3.ToString();
                    worksheet.Cells[22 + 14, colu].Value = data.import_duty3.ToString() + "%";
                    worksheet.Cells[23 + 14, colu].Value = data.handling_fee3.ToString() + "%";
                    worksheet.Cells[24 + 14, colu].Value = data.lpp_fee3.ToString();
                    worksheet.Cells[25 + 14, colu].Value = data.lead_premium3.ToString();
                    worksheet.Cells[26 + 16, colu].Value = data.plastic_weight.ToString();
                    worksheet.Cells[27 + 16, colu].Value = data.import_duty_plastic.ToString() + "%";
                    worksheet.Cells[28 + 16, colu].Value = data.handling_fee_plastic.ToString() + "%";
                    worksheet.Cells[29 + 16, colu].Value = data.pp_price.ToString();
                    worksheet.Cells[30 + 16, colu].Value = data.plastic.ToString();
                    worksheet.Cells[31 + 17, colu].Value = data.separator.ToString();
                    worksheet.Cells[32 + 17, colu].Value = data.others_purchase.ToString();
                    worksheet.Cells[33 + 17, colu].Value = data.sub_total_mat_cost.ToString();
                    worksheet.Cells[34 + 19, colu].Value = data.process_plate.ToString();
                    worksheet.Cells[35 + 19, colu].Value = data.process_injection.ToString();
                    worksheet.Cells[36 + 19, colu].Value = data.process_assembling.ToString();
                    worksheet.Cells[37 + 19, colu].Value = data.process_charging.ToString();
                    worksheet.Cells[38 + 19, colu].Value = data.sub_total_process_cost.ToString();
                    worksheet.Cells[39 + 20, colu].Value = data.total.ToString();
                    worksheet.Cells[40 + 21, colu].Value = data.general_charge.ToString();
                    worksheet.Cells[41 + 21, colu].Value = data.others.ToString();
                    worksheet.Cells[42 + 21, colu].Value = "(" + data.support.ToString() + ")";
                    worksheet.Cells[43 + 22, colu].Value = data.grand_total.ToString();
                    worksheet.Cells[43 + 22, colu].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[43 + 22, colu].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    worksheet.Cells[43 + 22, colu].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                }


                //if (i > 0)
                //{
                //    using (var range = worksheet.Cells[2, 1, i + 1, 24])
                //    {
                //        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                //        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //    }
                //}

                worksheet.Cells.AutoFitColumns(0);
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "[PriceQuotation]" + ID_ORDER + "_" + custName + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".xlsx");
            }
        }

        [SessionCheck]
        [System.Web.Mvc.HttpPost]
        public string UploadExcel_YearlyPlan(string tahun)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            var myFile = Request.Files["myFile"];
            if (!string.IsNullOrEmpty(tahun))
            {
                if (myFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(myFile.FileName).ToLower();
                    string query = null;
                    string connString = "";

                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                    //#if DEBUG
                    var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/UploadedFiles/");
                    var log_dir = targetLocation + "log_error.txt";
                    if (!System.IO.File.Exists(log_dir))
                    {
                        System.IO.File.CreateText(log_dir).Close();
                    }                        
                    //#else
                    //            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/marketing/Content/UploadedFiles/");
                    //#endif

                    try
                    {
                        //if (!Directory.Exists(targetLocation))
                        //{
                        //    Directory.CreateDirectory(targetLocation);
                        //}
                        query = "================STARTING================";
                        System.IO.File.AppendAllText(log_dir, query + "\r\n");

                        var path = Path.Combine(targetLocation, myFile.FileName);

                        if (myFile.FileName.Contains("YEARLY"))
                        {
                            if (validFileTypes.Contains(extension))
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    path = Path.Combine(targetLocation, myFile.FileName.Replace(extension, "") + "-"  + DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss") + extension);
                                    //System.IO.File.Delete(path);
                                }


                                //Proses simpan file
                                myFile.SaveAs(path);

                                query = "progress -> file save completed to " + path;
                                System.IO.File.AppendAllText(log_dir, query + "\r\n");

                                if (extension == ".csv")
                                {
                                    DataTable dt = ConvertCSVtoDataTable(path);
                                    //ViewBag.Data = dt;
                                }
                                //Connection String to Excel Workbook  
                                else if (extension.Trim() == ".xls")
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "YearlyPlanController", null, null, tahun);
                                    //ViewBag.Data = dt;
                                }
                                else if (extension.Trim() == ".xlsx")
                                {
                                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "YearlyPlanController", null, null, tahun);
                                    //ViewBag.Data = dt;
                                }
                            }
                            else
                            {
                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + "Mohon Upload File dalam format .xls, .xlsx dan .csv";

                            }
                        }
                        else
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: " + "Template tidak sesuai. Mohon Upload File sesuai template!";
                        }
                    }
                    catch (Exception ex)
                    {
                        query = "error -> " + ex.Message.ToString();
                        System.IO.File.AppendAllText(log_dir, query + "\r\n");

                        responseAPI.code = "200";
                        responseAPI.messages = ex.Message.ToString();
                    }

                    query = "================FINISH================";
                    System.IO.File.AppendAllText(log_dir, query + "\r\n");
                }
            }

            Response.StatusCode = Convert.ToInt32(responseAPI.code);
            Response.StatusDescription = responseAPI.messages;

            return responseAPI.messages;
        }

        [SessionCheck]
        [System.Web.Mvc.HttpPost]
        public string UploadFile_ActivityMarketing(string id_idl)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            var myFile = Request.Files["myFile"];
            if (!string.IsNullOrEmpty(id_idl))
            {
                if (myFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(myFile.FileName).ToLower();
                    string query = null;
                    string connString = "";

                    //string[] validFileTypes = { ".xls", ".xlsx", ".csv" };
                    string[] validFileTypes = { ".txt", ".pdf", ".pptx", ".doc", ".docx", ".xls", ".xlsx" };

                    //#if DEBUG
                    var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/attach_activity/");
                    var log_dir = targetLocation + "log_error.txt";
                    if (!System.IO.File.Exists(log_dir))
                    {
                        System.IO.File.CreateText(log_dir).Close();
                    }
                    //#else
                    //            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/marketing/Content/UploadedFiles/");
                    //#endif

                    try
                    {
                        //if (!Directory.Exists(targetLocation))
                        //{
                        //    Directory.CreateDirectory(targetLocation);
                        //}
                        query = "================STARTING================";
                        System.IO.File.AppendAllText(log_dir, query + "\r\n");

                        var path = Path.Combine(targetLocation, myFile.FileName);

                        if (validFileTypes.Contains(extension))
                        {
                            if (System.IO.File.Exists(path))
                            {
                                path = Path.Combine(targetLocation, myFile.FileName.Replace(extension, "") + "-" + DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss") + extension);
                                //System.IO.File.Delete(path);
                            }

                            var ididl = Convert.ToInt32(id_idl);
                            var resultData = GSDbContextGSTrack.ManageIDL.Where(e => e.id_idl == ididl).SingleOrDefault();
                            try
                            {
                                if (resultData != null)
                                {

                                    //Proses simpan file
                                    myFile.SaveAs(path);

                                    query = "progress -> file save completed to " + path;
                                    System.IO.File.AppendAllText(log_dir, query + "\r\n");

                                    resultData.idl_doc = path;

                                    GSDbContextGSTrack.SaveChanges();

                                    responseAPI.code = "200";
                                    responseAPI.messages = "Process Upload Complete";

                                }
                            }
                            catch (Exception ex)
                            {
                                query = "error -> " + ex.Message.ToString();
                                System.IO.File.AppendAllText(log_dir, query + "\r\n");

                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + ex.Message.ToString();
                            }

                        }
                        else
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: " + "Mohon Upload File sesuai format yang tertera.";

                        }
                    }
                    catch (Exception ex)
                    {
                        query = "error -> " + ex.Message.ToString();
                        System.IO.File.AppendAllText(log_dir, query + "\r\n");

                        responseAPI.code = "200";
                        responseAPI.messages = "error: " + ex.Message.ToString();
                    }

                    query = "================FINISH================";
                    System.IO.File.AppendAllText(log_dir, query + "\r\n");
                }
            }

            Response.StatusCode = Convert.ToInt32(responseAPI.code);
            Response.StatusDescription = responseAPI.messages;

            return responseAPI.messages;
        }

        [SessionCheck]
        [System.Web.Mvc.HttpPost]
        public string UploadExcel_BusinessPlan(string tahun)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            var myFile = Request.Files["myFile"];
            if (!string.IsNullOrEmpty(tahun))
            {
                if (myFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(myFile.FileName).ToLower();
                    string query = null;
                    string connString = "";

                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                    //#if DEBUG
                    var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/UploadedFiles/");
                    var log_dir = targetLocation + "log_error.txt";
                    if (!System.IO.File.Exists(log_dir))
                    {
                        System.IO.File.CreateText(log_dir).Close();
                    }
                    //#else
                    //            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/marketing/Content/UploadedFiles/");
                    //#endif

                    try
                    {
                        //if (!Directory.Exists(targetLocation))
                        //{
                        //    Directory.CreateDirectory(targetLocation);
                        //}
                        query = "================STARTING================";
                        System.IO.File.AppendAllText(log_dir, query + "\r\n");

                        var path = Path.Combine(targetLocation, myFile.FileName);

                        if (myFile.FileName.Contains("BUSINESS"))
                        {
                            if (validFileTypes.Contains(extension))
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    path = Path.Combine(targetLocation, myFile.FileName.Replace(extension, "") + "-" + DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss") + extension);
                                    //System.IO.File.Delete(path);
                                }


                                //Proses simpan file
                                myFile.SaveAs(path);

                                query = "progress -> file save completed to " + path;
                                System.IO.File.AppendAllText(log_dir, query + "\r\n");

                                if (extension == ".csv")
                                {
                                    DataTable dt = ConvertCSVtoDataTable(path);
                                    //ViewBag.Data = dt;
                                }
                                //Connection String to Excel Workbook  
                                else if (extension.Trim() == ".xls")
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "BusinessPlanController", null, null, tahun);
                                    //ViewBag.Data = dt;
                                }
                                else if (extension.Trim() == ".xlsx")
                                {
                                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "BusinessPlanController", null, null, tahun);
                                    //ViewBag.Data = dt;
                                }
                            }
                            else
                            {
                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + "Mohon Upload File dalam format .xls, .xlsx dan .csv";

                            }
                        }
                        else
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: " + "Template tidak sesuai. Mohon Upload File sesuai template!";
                        }
                    }
                    catch (Exception ex)
                    {
                        query = "error -> " + ex.Message.ToString();
                        System.IO.File.AppendAllText(log_dir, query + "\r\n");

                        responseAPI.code = "200";
                        responseAPI.messages = "error: " + ex.Message.ToString();
                    }

                    query = "================FINISH================";
                    System.IO.File.AppendAllText(log_dir, query + "\r\n");
                }
            }

            Response.StatusCode = Convert.ToInt32(responseAPI.code);
            Response.StatusDescription = responseAPI.messages;

            return responseAPI.messages;
        }

        [SessionCheck]
        [System.Web.Mvc.HttpPost]
        public string UploadExcel_ItemPartNumber()
        {
            ResponseAPI responseAPI = new ResponseAPI();

            var myFile = Request.Files["myFile"];
            if (myFile.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(myFile.FileName).ToLower();
                string connString = "";

                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

//#if DEBUG
                var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/UploadedFiles/");
//#else
//            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/marketing/Content/UploadedFiles/");
//#endif

                try
                {
                    //if (!Directory.Exists(targetLocation))
                    //{
                    //    Directory.CreateDirectory(targetLocation);
                    //}

                    var path = Path.Combine(targetLocation, myFile.FileName);

                    if (myFile.FileName.Contains("PART-NUMBER"))
                    {
                        if (validFileTypes.Contains(extension))
                        {
                            if (System.IO.File.Exists(path))
                            {
                                path = Path.Combine(targetLocation, myFile.FileName.Replace(extension, "") + "-" + DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss") + extension);
                                //System.IO.File.Delete(path);
                            }

                            //Proses simpan file
                            myFile.SaveAs(path);

                            if (extension == ".csv")
                            {
                                DataTable dt = ConvertCSVtoDataTable(path);
                                ViewBag.Data = dt;
                            }
                            //Connection String to Excel Workbook  
                            else if (extension.Trim() == ".xls")
                            {
                                connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "ItemPartNumberController", null, null, null);
                            }
                            else if (extension.Trim() == ".xlsx")
                            {
                                connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "ItemPartNumberController", null, null, null);
                            }
                        }
                        else
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: " + "Mohon Upload File dalam format .xls, .xlsx dan .csv";

                        }
                    }
                    else
                    {
                        responseAPI.code = "200";
                        responseAPI.messages = "error: " + "Template tidak sesuai. Mohon Upload File sesuai template!";

                    }


                }
                catch (Exception ex)
                {
                    responseAPI.code = "200";
                    responseAPI.messages = "error: " + ex.Message.ToString();
                }
            }
           
            Response.StatusCode = Convert.ToInt32(responseAPI.code);
            Response.StatusDescription = responseAPI.messages;

            return responseAPI.messages;
        }

        [SessionCheck]
        [System.Web.Mvc.HttpPost]
        public string UploadExcelForecast(string tahun, string bulan)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            var myFile = Request.Files["myFile"];
            if (!string.IsNullOrEmpty(tahun) && !string.IsNullOrEmpty(bulan))
            {
                if (myFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(myFile.FileName).ToLower();
                    string connString = "";

                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                    //#if DEBUG
                    var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/UploadedFiles/");
                    //#else
                    //            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/marketing/Content/UploadedFiles/");
                    //#endif

                    try
                    {
                        //if (!Directory.Exists(targetLocation))
                        //{
                        //    Directory.CreateDirectory(targetLocation);
                        //}

                        var path = Path.Combine(targetLocation, myFile.FileName);

                        if (myFile.FileName.Contains("FORECAST"))
                        {
                            if (validFileTypes.Contains(extension))
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    path = Path.Combine(targetLocation, myFile.FileName.Replace(extension, "") + "-" + DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss") + extension);
                                    //System.IO.File.Delete(path);
                                }

                                //Proses simpan file
                                myFile.SaveAs(path);

                                if (extension == ".csv")
                                {
                                    DataTable dt = ConvertCSVtoDataTable(path);
                                    ViewBag.Data = dt;
                                }
                                //Connection String to Excel Workbook  
                                else if (extension.Trim() == ".xls")
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "ForecastController", tahun, bulan.ToString(), null);
                                }
                                else if (extension.Trim() == ".xlsx")
                                {
                                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "ForecastController", tahun, bulan.ToString(), null);
                                }
                            }
                            else
                            {
                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + "Mohon Upload File dalam format .xls, .xlsx dan .csv";

                            }
                        }
                        else
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: " + "Template tidak sesuai. Mohon Upload File sesuai template!";
                        }

                    }
                    catch (Exception ex)
                    {
                        responseAPI.code = "200";
                        responseAPI.messages = "error: " + ex.Message.ToString();
                    }
                }
            }
            
            Response.StatusCode = Convert.ToInt32(responseAPI.code);
            Response.StatusDescription = responseAPI.messages;

            return responseAPI.messages;
        }

        [SessionCheck]
        [System.Web.Mvc.HttpPost]
        public string UploadExcelOrder(string status, string orderid, string tahun, string bulan)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            var myFile = Request.Files["myFile"];

            if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(tahun) && !string.IsNullOrEmpty(bulan))
            {
                if (myFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(myFile.FileName).ToLower();
                    string connString = "";

                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                    //#if DEBUG
                    var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/UploadedFiles/");
                    //#else
                    //            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/marketing/Content/UploadedFiles/");
                    //#endif

                    try
                    {
                        //if (!Directory.Exists(targetLocation))
                        //{
                        //    Directory.CreateDirectory(targetLocation);
                        //}

                        var path = Path.Combine(targetLocation, myFile.FileName);

                        if (myFile.FileName.Contains("ORDER"))
                        {
                            if (validFileTypes.Contains(extension))
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    path = Path.Combine(targetLocation, myFile.FileName.Replace(extension, "") + "-" + DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss") + extension);
                                    //System.IO.File.Delete(path);
                                }

                                //Proses simpan file
                                myFile.SaveAs(path);

                                if (extension == ".csv")
                                {
                                    DataTable dt = ConvertCSVtoDataTable(path);
                                    ViewBag.Data = dt;
                                }
                                //Connection String to Excel Workbook  
                                else if (extension.Trim() == ".xls")
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "OrderController", status, orderid, tahun + "|" + bulan);                                    
                                }
                                else if (extension.Trim() == ".xlsx")
                                {
                                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                                    responseAPI = ConvertXSLXtoDataTable(path, connString, "SHEET", "OrderController", status, orderid, tahun + "|" + bulan);
                                }
                            }
                            else
                            {
                                responseAPI.code = "200";
                                responseAPI.messages = "error: Mohon Upload File dalam format .xls, .xlsx dan .csv";
                            }
                        }
                        else
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: Template tidak sesuai. Mohon Upload File sesuai template!";
                        }
                    }
                    catch (Exception ex)
                    {
                        responseAPI.code = "200";
                        responseAPI.messages = "error: " + ex.Message.ToString();
                    }
                }
            }

            Response.StatusCode = Convert.ToInt32(responseAPI.code);
            Response.StatusDescription = responseAPI.messages;

            return responseAPI.messages;
        }

        public DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            try
            {
                using (StreamReader sr = new StreamReader(strFilePath))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }

                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        if (rows.Length > 1)
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                dr[i] = rows[i].Trim();
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            return dt;
        }

        public ResponseAPI ConvertXSLXtoDataTable(string strFilePath, string connString, string sheetName, string functionName, string param1, string param2, string param3)
        {
            ResponseAPI responseAPI = new ResponseAPI();

            //LOG ERROR
            var query = "";
            var targetLocation = System.Web.HttpContext.Current.Server.MapPath("~/Content/UploadedFiles/");
            var log_dir = targetLocation + "log_error.txt";

            OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            try
            {

                query = "progress -> starting ConvertXSLXtoDataTable ....";
                System.IO.File.AppendAllText(log_dir, query + "\r\n");

                oledbConn.Open();

                query = "progress -> open OleDB ....";
                System.IO.File.AppendAllText(log_dir, query + "\r\n");

                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheetName + "$]", oledbConn))
                {
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);


                    query = "progress -> filling database OleDB ....";
                    System.IO.File.AppendAllText(log_dir, query + "\r\n");

                    dt = ds.Tables[0];
                    if (!string.IsNullOrEmpty(functionName))
                    {
                        if (functionName.Contains("BusinessPlan"))
                        {
                            responseAPI = InsertDB_BusinessPlan(ds.Tables[0], param3);
                            
                            query = "progress -> result Insert DB BusinessPlan adalah :...." + responseAPI.messages;
                            System.IO.File.AppendAllText(log_dir, query + "\r\n");
                        }
                        else if (functionName.Contains("YearlyPlan"))
                        {
                            responseAPI = InsertDB_YearlyPlan(ds.Tables[0], param3);
                           

                            query = "progress -> result Insert DB YearlyPlan adalah :...." + responseAPI.messages;
                            System.IO.File.AppendAllText(log_dir, query + "\r\n");
                        }
                        else if (functionName.Contains("ItemPartNumber"))
                        {
                            responseAPI = InsertDB_ItemPartNumber(ds.Tables[0]);
                           

                            query = "progress -> result Insert DB ItemPartNumber adalah :...." + responseAPI.messages;
                            System.IO.File.AppendAllText(log_dir, query + "\r\n");
                        }
                        else if (functionName.Contains("Forecast"))
                        {
                            responseAPI = InsertDB_Forecast(ds.Tables[0], param1, param2);
                           
                            query = "progress -> result Insert DB Forecast adalah :...." + responseAPI.messages;
                            System.IO.File.AppendAllText(log_dir, query + "\r\n");
                        }
                        else if (functionName.Contains("Order"))
                        {
                            string[] sPlitTahunBulan = param3.Split('|');
                            responseAPI = InsertDB_Order(ds.Tables[0], param1, param2, sPlitTahunBulan[0], sPlitTahunBulan[1], strFilePath);
                            
                            query = "progress -> result Insert DB Order adalah :...." + responseAPI.messages;
                            System.IO.File.AppendAllText(log_dir, query + "\r\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                query = "error catch ConvertXSLXtoDataTable -> " + ex.Message.ToString();
                System.IO.File.AppendAllText(log_dir, query + "\r\n");

                responseAPI.code = "200";
                responseAPI.messages = "error: " + ex.Message.ToString();

                ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                manageHistoryTransaction.type_history = "convertXLSXtoDataTable";
                manageHistoryTransaction.desc_history = "Convert XLSX to DataTable : " + responseAPI.messages + "\r\n\r\n";
                GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                GSDbContext.SaveChanges();
            }
            finally
            {
                oledbConn.Close();
                query = "progress -> Closed Database OLeDB";
                System.IO.File.AppendAllText(log_dir, query + "\r\n");
            }

            return responseAPI;

        }

        public ResponseAPI InsertDB_BusinessPlan(DataTable dt, string tahun)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            int iRow = 0;

            if (dt.Rows.Count > 0)
            {
                List<ManageBusinessPlan> listtempManageBusinessPlan = new List<ManageBusinessPlan>();

                try
                {
                    if (!string.IsNullOrEmpty(tahun))
                    {
                        int i = 0;
                        for (i = 0; i < dt.Rows.Count - 1; i++)
                        {
                            if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[2].ToString()))
                            {
                                //Console.WriteLine(dt.Rows[i].ItemArray);

                                ManageBusinessPlan tempManageBusinessPlan = new ManageBusinessPlan();

                                try
                                {
                                    if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[1].ToString()) && !string.IsNullOrEmpty(dt.Rows[i].ItemArray[2].ToString()))
                                    {
                                        tempManageBusinessPlan.pn_gs = dt.Rows[i].ItemArray[0].ToString();
                                        tempManageBusinessPlan.pn_customer = dt.Rows[i].ItemArray[1].ToString();

                                        var qty1 = dt.Rows[i].ItemArray[2].ToString().Replace('-', '0');
                                        var qty2 = dt.Rows[i].ItemArray[3].ToString().Replace('-', '0');
                                        var qty3 = dt.Rows[i].ItemArray[4].ToString().Replace('-', '0');
                                        var qty4 = dt.Rows[i].ItemArray[5].ToString().Replace('-', '0');
                                        var qty5 = dt.Rows[i].ItemArray[6].ToString().Replace('-', '0');
                                        var qty6 = dt.Rows[i].ItemArray[7].ToString().Replace('-', '0');
                                        var qty7 = dt.Rows[i].ItemArray[8].ToString().Replace('-', '0');
                                        var qty8 = dt.Rows[i].ItemArray[9].ToString().Replace('-', '0');
                                        var qty9 = dt.Rows[i].ItemArray[10].ToString().Replace('-', '0');
                                        var qty10 = dt.Rows[i].ItemArray[11].ToString().Replace('-', '0');
                                        var qty11 = dt.Rows[i].ItemArray[12].ToString().Replace('-', '0');
                                        var qty12 = dt.Rows[i].ItemArray[13].ToString().Replace('-', '0');
                                        var qtytotal = dt.Rows[i].ItemArray[14].ToString().Replace('-', '0');

                                        Decimal dqty_1 = 0;
                                        Decimal dqty_2 = 0;
                                        Decimal dqty_3 = 0;
                                        Decimal dqty_4 = 0;
                                        Decimal dqty_5 = 0;
                                        Decimal dqty_6 = 0;
                                        Decimal dqty_7 = 0;
                                        Decimal dqty_8 = 0;
                                        Decimal dqty_9 = 0;
                                        Decimal dqty_10 = 0;
                                        Decimal dqty_11 = 0;
                                        Decimal dqty_12 = 0;
                                        Decimal dtotal = 0;

                                        if (!string.IsNullOrEmpty(qty1))
                                            dqty_1 = Convert.ToDecimal(qty1);

                                        if (!string.IsNullOrEmpty(qty2))
                                            dqty_2 = Convert.ToDecimal(qty2);

                                        if (!string.IsNullOrEmpty(qty3))
                                            dqty_3 = Convert.ToDecimal(qty3);

                                        if (!string.IsNullOrEmpty(qty4))
                                            dqty_4 = Convert.ToDecimal(qty4);

                                        if (!string.IsNullOrEmpty(qty5))
                                            dqty_5 = Convert.ToDecimal(qty5);

                                        if (!string.IsNullOrEmpty(qty6))
                                            dqty_6 = Convert.ToDecimal(qty6);

                                        if (!string.IsNullOrEmpty(qty7))
                                            dqty_7 = Convert.ToDecimal(qty7);

                                        if (!string.IsNullOrEmpty(qty8))
                                            dqty_8 = Convert.ToDecimal(qty8);

                                        if (!string.IsNullOrEmpty(qty9))
                                            dqty_9 = Convert.ToDecimal(qty9);

                                        if (!string.IsNullOrEmpty(qty10))
                                            dqty_10 = Convert.ToDecimal(qty10);

                                        if (!string.IsNullOrEmpty(qty11))
                                            dqty_11 = Convert.ToDecimal(qty11);

                                        if (!string.IsNullOrEmpty(qty12))
                                            dqty_12 = Convert.ToDecimal(qty12);

                                        if (!string.IsNullOrEmpty(qtytotal))
                                            dtotal = Convert.ToDecimal(qtytotal);

                                        tempManageBusinessPlan.qty_1 = dqty_1;
                                        tempManageBusinessPlan.qty_2 = dqty_2;
                                        tempManageBusinessPlan.qty_3 = dqty_3;
                                        tempManageBusinessPlan.qty_4 = dqty_4;
                                        tempManageBusinessPlan.qty_5 = dqty_5;
                                        tempManageBusinessPlan.qty_6 = dqty_6;
                                        tempManageBusinessPlan.qty_7 = dqty_7;
                                        tempManageBusinessPlan.qty_8 = dqty_8;
                                        tempManageBusinessPlan.qty_9 = dqty_9;
                                        tempManageBusinessPlan.qty_10 = dqty_10;
                                        tempManageBusinessPlan.qty_11 = dqty_11;
                                        tempManageBusinessPlan.qty_12 = dqty_12;
                                        tempManageBusinessPlan.qty_total = dtotal;
                                        tempManageBusinessPlan.tahun = Convert.ToInt32(tahun);
                                        tempManageBusinessPlan.date_insert = DateTime.UtcNow.AddHours(7);
                                        listtempManageBusinessPlan.Add(tempManageBusinessPlan);
                                    }                                   
                                }
                                catch(Exception ex)
                                {
                                    responseAPI.code = "200";
                                    responseAPI.messages = ex.Message.ToString();
                                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                                    manageHistoryTransaction.type_history = "upload_file_businessplan";
                                    manageHistoryTransaction.desc_history = "Upload Business Plan : " + responseAPI.messages + "\r\n\r\n";
                                    manageHistoryTransaction.desc_history += "Row : " + iRow;
                                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                                    GSDbContext.SaveChanges();

                                    return responseAPI;
                                }
                            }
                        }

                        if (listtempManageBusinessPlan.Count > 0)
                        {
                            GSDbContext.ManageBusinessPlan.AddRange(listtempManageBusinessPlan);
                            GSDbContext.SaveChanges();
                            responseAPI.code = "200";
                            responseAPI.messages = "Process Upload Complete";
                        }
                    }
                }
                catch (Exception ex)
                {
                    responseAPI.code = "200";
                    responseAPI.messages = ex.Message.ToString();
                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                    manageHistoryTransaction.type_history = "upload_file_businessplan";
                    manageHistoryTransaction.desc_history = "Upload Business Plan : " + responseAPI.messages + "\r\n\r\n";
                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                    GSDbContext.SaveChanges();
                }
            }

            return responseAPI;
        }

        public ResponseAPI InsertDB_YearlyPlan(DataTable dt, string tahun)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            int iRow = 0;
            if (dt.Rows.Count > 0)
            {
                List<ManageYearlyPlan> listtempManageYearlyPlan = new List<ManageYearlyPlan>();

                try
                {
                    if (!string.IsNullOrEmpty(tahun))
                    {
                        int i = 0;
                        for (i = 0; i < dt.Rows.Count - 1; i++)
                        {
                            iRow = i;
                            if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[2].ToString()))
                            {
                                //Console.WriteLine(dt.Rows[i].ItemArray);

                                ManageYearlyPlan tempManageYearlyPlan = new ManageYearlyPlan();

                                try
                                {
                                    if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[1].ToString()) && !string.IsNullOrEmpty(dt.Rows[i].ItemArray[2].ToString()))
                                    {
                                        tempManageYearlyPlan.pn_gs = dt.Rows[i].ItemArray[0].ToString();
                                        tempManageYearlyPlan.pn_customer = dt.Rows[i].ItemArray[1].ToString();

                                        var qty1 = dt.Rows[i].ItemArray[2].ToString().Replace('-', '0');
                                        var qty2 = dt.Rows[i].ItemArray[3].ToString().Replace('-', '0');
                                        var qty3 = dt.Rows[i].ItemArray[4].ToString().Replace('-', '0');
                                        var qty4 = dt.Rows[i].ItemArray[5].ToString().Replace('-', '0');
                                        var qty5 = dt.Rows[i].ItemArray[6].ToString().Replace('-', '0');
                                        var qty6 = dt.Rows[i].ItemArray[7].ToString().Replace('-', '0');
                                        var qty7 = dt.Rows[i].ItemArray[8].ToString().Replace('-', '0');
                                        var qty8 = dt.Rows[i].ItemArray[9].ToString().Replace('-', '0');
                                        var qty9 = dt.Rows[i].ItemArray[10].ToString().Replace('-', '0');
                                        var qty10 = dt.Rows[i].ItemArray[11].ToString().Replace('-', '0');
                                        var qty11 = dt.Rows[i].ItemArray[12].ToString().Replace('-', '0');
                                        var qty12 = dt.Rows[i].ItemArray[13].ToString().Replace('-', '0');
                                        var qtytotal = dt.Rows[i].ItemArray[14].ToString().Replace('-', '0');

                                        Decimal dqty_1 = 0;
                                        Decimal dqty_2 = 0;
                                        Decimal dqty_3 = 0;
                                        Decimal dqty_4 = 0;
                                        Decimal dqty_5 = 0;
                                        Decimal dqty_6 = 0;
                                        Decimal dqty_7 = 0;
                                        Decimal dqty_8 = 0;
                                        Decimal dqty_9 = 0;
                                        Decimal dqty_10 = 0;
                                        Decimal dqty_11 = 0;
                                        Decimal dqty_12 = 0;
                                        Decimal dtotal = 0;

                                        if (!string.IsNullOrEmpty(qty1))
                                            dqty_1 = Convert.ToDecimal(qty1);

                                        if (!string.IsNullOrEmpty(qty2))
                                            dqty_2 = Convert.ToDecimal(qty2);

                                        if (!string.IsNullOrEmpty(qty3))
                                            dqty_3 = Convert.ToDecimal(qty3);

                                        if (!string.IsNullOrEmpty(qty4))
                                            dqty_4 = Convert.ToDecimal(qty4);

                                        if (!string.IsNullOrEmpty(qty5))
                                            dqty_5 = Convert.ToDecimal(qty5);

                                        if (!string.IsNullOrEmpty(qty6))
                                            dqty_6 = Convert.ToDecimal(qty6);

                                        if (!string.IsNullOrEmpty(qty7))
                                            dqty_7 = Convert.ToDecimal(qty7);

                                        if (!string.IsNullOrEmpty(qty8))
                                            dqty_8 = Convert.ToDecimal(qty8);

                                        if (!string.IsNullOrEmpty(qty9))
                                            dqty_9 = Convert.ToDecimal(qty9);

                                        if (!string.IsNullOrEmpty(qty10))
                                            dqty_10 = Convert.ToDecimal(qty10);

                                        if (!string.IsNullOrEmpty(qty11))
                                            dqty_11 = Convert.ToDecimal(qty11);

                                        if (!string.IsNullOrEmpty(qty12))
                                            dqty_12 = Convert.ToDecimal(qty12);

                                        if (!string.IsNullOrEmpty(qtytotal))
                                            dtotal = Convert.ToDecimal(qtytotal);

                                        tempManageYearlyPlan.qty_1 = dqty_1;
                                        tempManageYearlyPlan.qty_2 = dqty_2;
                                        tempManageYearlyPlan.qty_3 = dqty_3;
                                        tempManageYearlyPlan.qty_4 = dqty_4;
                                        tempManageYearlyPlan.qty_5 = dqty_5;
                                        tempManageYearlyPlan.qty_6 = dqty_6;
                                        tempManageYearlyPlan.qty_7 = dqty_7;
                                        tempManageYearlyPlan.qty_8 = dqty_8;
                                        tempManageYearlyPlan.qty_9 = dqty_9;
                                        tempManageYearlyPlan.qty_10 = dqty_10;
                                        tempManageYearlyPlan.qty_11 = dqty_11;
                                        tempManageYearlyPlan.qty_12 = dqty_12;
                                        tempManageYearlyPlan.qty_total = dtotal;

                                        tempManageYearlyPlan.tahun = Convert.ToInt32(tahun);
                                        tempManageYearlyPlan.date_insert = DateTime.UtcNow.AddHours(7);

                                        listtempManageYearlyPlan.Add(tempManageYearlyPlan);
                                    }                                       
                                }
                                catch (Exception ex)
                                {
                                    responseAPI.code = "200";
                                    responseAPI.messages = "error: " + ex.Message.ToString();

                                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                                    manageHistoryTransaction.type_history = "upload_file_yearlyplan";
                                    manageHistoryTransaction.desc_history = "Upload Yearly Plan : " + responseAPI.messages + "\r\n\r\n";
                                    manageHistoryTransaction.desc_history += "Row : " + iRow;
                                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                                    GSDbContext.SaveChanges();
                                }
                            }
                        }

                        if (listtempManageYearlyPlan.Count > 0)
                        {
                            GSDbContext.ManageYearlyPlan.AddRange(listtempManageYearlyPlan);
                            GSDbContext.SaveChanges();
                            responseAPI.code = "200";
                            responseAPI.messages = "Process Upload Complete";
                        }
                    }
                }
                catch (Exception ex)
                {
                    responseAPI.code = "200";
                    responseAPI.messages = "error: " + ex.Message.ToString();

                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                    manageHistoryTransaction.type_history = "upload_file_yearlyplan";
                    manageHistoryTransaction.desc_history = "Upload Yearly Plan : " + responseAPI.messages + "\r\n\r\n";
                    manageHistoryTransaction.desc_history += "Row : " + iRow;
                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                    GSDbContext.SaveChanges();
                }
            }

            return responseAPI;
        }

        public ResponseAPI InsertDB_ItemPartNumber(DataTable dt)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            int iRow = 0;
            if (dt.Rows.Count > 0)
            {
                List<ManageItemPartNumber> listtempItemPartNumber = new List<ManageItemPartNumber>();

                try
                {
                    int i = 0;
                    for (i = 0; i < dt.Rows.Count - 1; i++)
                    {
                        iRow = i;
                        if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[2].ToString()))
                        {
                            Console.WriteLine(dt.Rows[i].ItemArray);

                            ManageItemPartNumber tempItemPartNumber = new ManageItemPartNumber();

                            tempItemPartNumber.pn_gs = dt.Rows[i].ItemArray[0].ToString();
                            tempItemPartNumber.pn_customer = dt.Rows[i].ItemArray[1].ToString();

                            var qty_lot_size = dt.Rows[i].ItemArray[2].ToString().Replace('-', '0').Replace(".", "");

                            if (qty_lot_size.ToString().Contains(','))
                            {
                                var spltNominal = qty_lot_size.ToString().Split(',');
                                qty_lot_size = spltNominal[0];
                            }

                            if (qty_lot_size.ToString().Contains('.'))
                            {
                                var spltNominal = qty_lot_size.ToString().Split('.');
                                qty_lot_size = spltNominal[0];
                            }

                            //tempItemPartNumber.lot_size = Convert.ToInt32(dt.Rows[i].ItemArray[3]);
                            tempItemPartNumber.lot_size = Convert.ToInt32(qty_lot_size);
                            
                            tempItemPartNumber.battery_type = dt.Rows[i].ItemArray[3].ToString();
                            tempItemPartNumber.material_type = dt.Rows[i].ItemArray[4].ToString();
                            tempItemPartNumber.brand = dt.Rows[i].ItemArray[5].ToString();
                            tempItemPartNumber.group_item = dt.Rows[i].ItemArray[6].ToString();
                            tempItemPartNumber.spec = dt.Rows[i].ItemArray[7].ToString();

                            listtempItemPartNumber.Add(tempItemPartNumber);
                        }
                    }

                    if (listtempItemPartNumber.Count > 0)
                    {
                        GSDbContext.ManageItemPartNumber.AddRange(listtempItemPartNumber);
                        GSDbContext.SaveChanges();
                        responseAPI.code = "200";
                        responseAPI.messages = "Process Upload Complete";
                    }
                }
                catch (Exception ex)
                {
                    responseAPI.code = "200";
                    responseAPI.messages = "error: " + ex.Message.ToString();

                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                    manageHistoryTransaction.type_history = "upload_file_itempartnumber";
                    manageHistoryTransaction.desc_history = "Upload Item Part Number : " + responseAPI.messages + "\r\n\r\n";
                    manageHistoryTransaction.desc_history += "Row : " + iRow;
                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                    GSDbContext.SaveChanges();
                }
            }

            return responseAPI;
        }

        public ResponseAPI InsertDB_Forecast(DataTable dt, string tahun, string bulan)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            int iRow = 0;
            if (dt.Rows.Count > 0)
            {
                List<ManageForecast> listtempForecast = new List<ManageForecast>();

                try
                {
                    int i = 0;
                    var noIDLastForecast = "";
                    var dateNow = DateTime.UtcNow.AddHours(7);

                    string sSQLSelect = "select top 1 id_forecast from manage_forecast order by id_forecast desc";

                    var checkLastIDForecast = GSDbContext.Database.SqlQuery<GetLastForecastID>(sSQLSelect).SingleOrDefault();
                    if (checkLastIDForecast != null)
                    {
                        if (!string.IsNullOrEmpty(checkLastIDForecast.id_forecast))
                        {

                            string sSQLSelectCheckForNow = "select top 1 id_forecast from manage_forecast where id_forecast like '" + "FC" + dateNow.ToString("yyMMdd") + "%' order by id_forecast desc";

                            var checkLastIDWithDateNowForecast = GSDbContext.Database.SqlQuery<GetLastForecastID>(sSQLSelectCheckForNow).SingleOrDefault();

                            if (checkLastIDWithDateNowForecast != null)
                            {
                                int lastID = Convert.ToInt32(checkLastIDWithDateNowForecast.id_forecast.Substring(8, 5)) + 1;
                                noIDLastForecast = checkLastIDWithDateNowForecast.id_forecast.Substring(0, 8) + lastID.ToString().PadLeft(5, '0');
                            }
                            else
                            {
                                int lastID = 1;
                                noIDLastForecast = "FC" + dateNow.ToString("yyMMdd") + lastID.ToString().PadLeft(5, '0');
                            }

                        }
                    }
                    else
                    {
                        var z = 1;
                        noIDLastForecast = "FC" + dateNow.ToString("yyMMdd") + z.ToString().PadLeft(5, '0');
                    }

                    for (i = 0; i < dt.Rows.Count - 1; i++)
                    {
                        iRow = i;
                        if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[0].ToString()) && !string.IsNullOrEmpty(noIDLastForecast))
                        {
                            var pnGS = dt.Rows[i].ItemArray[0].ToString();
                            var getMasterIterPartNumber = GSDbContext.ManageItemPartNumber.Where(p => p.pn_gs == pnGS).FirstOrDefault();

                            if (getMasterIterPartNumber != null)
                            {
                                ManageForecast tempForecast = new ManageForecast();

                                tempForecast.id_forecast = noIDLastForecast;
                                tempForecast.pn_gs = dt.Rows[i].ItemArray[0].ToString();
                                tempForecast.pn_customer = getMasterIterPartNumber.pn_customer;
                                tempForecast.lot_size = getMasterIterPartNumber.lot_size;
                                tempForecast.type_battery = getMasterIterPartNumber.battery_type;
                                tempForecast.type_material = getMasterIterPartNumber.material_type;
                                tempForecast.brand = getMasterIterPartNumber.brand;
                                tempForecast.group_forecast = getMasterIterPartNumber.group_item;
                                tempForecast.spec = getMasterIterPartNumber.spec;

                                //tempForecast.month_forecast = dateNow.ToString("MMMM");
                                //tempForecast.year_forecast = dateNow.Year.ToString();
                                tempForecast.year_forecast = tahun;

                                // INSERT YEARLY PLAN FROM MASTER YEARLY PLAN. SELECT TABLE YEARLY PLAN 
                                var iTahun = Convert.ToInt32(tahun);
                                var getDataYearlyPlan = GSDbContext.ManageYearlyPlan.Where(p => p.pn_gs == tempForecast.pn_gs && p.tahun == iTahun).FirstOrDefault();
                                if (getDataYearlyPlan != null)
                                {

                                    if (bulan != null && bulan.Length < 2)
                                        bulan = "0" + bulan;

                                    if (bulan != null)
                                    {
                                        var namaBulan = "";
                                        decimal qtyYearlyplan = 0;

                                        switch (bulan)
                                        {
                                            case "01":
                                                namaBulan = "Januari";
                                                qtyYearlyplan = getDataYearlyPlan.qty_1 ?? 0;
                                                break;
                                            case "02":
                                                namaBulan = "Februari";
                                                qtyYearlyplan = getDataYearlyPlan.qty_2 ?? 0;
                                                break;
                                            case "03":
                                                namaBulan = "Maret";
                                                qtyYearlyplan = getDataYearlyPlan.qty_3 ?? 0;
                                                break;
                                            case "04":
                                                namaBulan = "April";
                                                qtyYearlyplan = getDataYearlyPlan.qty_4 ?? 0;
                                                break;
                                            case "05":
                                                namaBulan = "Mei";
                                                qtyYearlyplan = getDataYearlyPlan.qty_5 ?? 0;
                                                break;
                                            case "06":
                                                namaBulan = "Juni";
                                                qtyYearlyplan = getDataYearlyPlan.qty_6 ?? 0;
                                                break;
                                            case "07":
                                                namaBulan = "Juli";
                                                qtyYearlyplan = getDataYearlyPlan.qty_7 ?? 0;
                                                break;
                                            case "08":
                                                namaBulan = "Agustus";
                                                qtyYearlyplan = getDataYearlyPlan.qty_8 ?? 0;
                                                break;
                                            case "09":
                                                namaBulan = "September";
                                                qtyYearlyplan = getDataYearlyPlan.qty_9 ?? 0;
                                                break;
                                            case "10":
                                                namaBulan = "Oktober";
                                                qtyYearlyplan = getDataYearlyPlan.qty_10 ?? 0;
                                                break;
                                            case "11":
                                                namaBulan = "November";
                                                qtyYearlyplan = getDataYearlyPlan.qty_11 ?? 0;
                                                break;
                                            case "12":
                                                namaBulan = "Desember";
                                                qtyYearlyplan = getDataYearlyPlan.qty_12 ?? 0;
                                                break;

                                        }
                                        tempForecast.month_forecast = namaBulan;
                                        tempForecast.yearly_plan = Convert.ToDecimal(qtyYearlyplan);

                                        int noOfDaysInMonth = -1;
                                        if (tahun != null && bulan != null)
                                            noOfDaysInMonth = DateTime.DaysInMonth(Convert.ToInt32(tahun), Convert.ToInt32(bulan));

                                        DateTime dtOrder = DateTime.ParseExact(tahun + bulan + Convert.ToString(noOfDaysInMonth), "yyyyMMdd", CultureInfo.InvariantCulture);
                                        tempForecast.date_forecast = dtOrder;
                                    }
                                }
                                else
                                {
                                    tempForecast.yearly_plan = 0;
                                }


                                var qtyn2 = dt.Rows[i].ItemArray[1].ToString().Replace('-', '0');
                                var qtyn3 = dt.Rows[i].ItemArray[2].ToString().Replace('-', '0');
                                var qtyn4 = dt.Rows[i].ItemArray[3].ToString().Replace('-', '0');


                                Decimal dqty_2 = 0;
                                Decimal dqty_3 = 0;
                                Decimal dqty_4 = 0;


                                if (!string.IsNullOrEmpty(qtyn2))
                                    dqty_2 = Convert.ToDecimal(qtyn2);

                                if (!string.IsNullOrEmpty(qtyn3))
                                    dqty_3 = Convert.ToDecimal(qtyn3);

                                if (!string.IsNullOrEmpty(qtyn4))
                                    dqty_4 = Convert.ToDecimal(qtyn4);


                                tempForecast.n2 = Convert.ToDecimal(dqty_2);
                                tempForecast.n3 = Convert.ToDecimal(dqty_3);
                                tempForecast.n4 = Convert.ToDecimal(dqty_4);
                                tempForecast.insert_time = DateTime.UtcNow.AddHours(7);
                                listtempForecast.Add(tempForecast);
                            }
                            else
                            {
                                // data tidak ada
                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + "Data Master Item Part Number dengan : " + dt.Rows[i].ItemArray[0].ToString() + " Tidak ditemukan!.";
                               
                                ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                                manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                                manageHistoryTransaction.type_history = "upload_file_forecast";
                                manageHistoryTransaction.desc_history = "Upload Forecast : " + responseAPI.messages + "\r\n\r\n";
                                manageHistoryTransaction.desc_history += "Row : " + iRow;
                                GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                                GSDbContext.SaveChanges();
                            }
                        }
                    }

                    if (listtempForecast.Count > 0)
                    {
                        GSDbContext.ManageForecast.AddRange(listtempForecast);
                        GSDbContext.SaveChanges();
                        responseAPI.code = "200";
                        responseAPI.messages = "Process Upload Complete";
                    }
                }
                catch (Exception ex)
                {
                    responseAPI.code = "200";
                    responseAPI.messages = "error: " + ex.Message.ToString();

                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                    manageHistoryTransaction.type_history = "upload_file_forecast";
                    manageHistoryTransaction.desc_history = "Upload Forecast : " + responseAPI.messages + "\r\n\r\n";
                    manageHistoryTransaction.desc_history += "Row : " + iRow;
                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                    GSDbContext.SaveChanges();
                }
            }

            return responseAPI;
        }

        public ResponseAPI InsertDB_Order(DataTable dt, string status, string orderid, string tahun, string bulan, string strFilePath)
        {
            ResponseAPI responseAPI = new ResponseAPI();
            int iRow = 0;

            if (dt.Rows.Count > 0)
            {
                List<ManageOrder> listtempOrder = new List<ManageOrder>();

                try
                {
                    int i = 0;
                    var noIDLastOrder = "";
                    var dateNow = DateTime.UtcNow.AddHours(7);

                    if (string.IsNullOrEmpty(orderid))
                    {
                        string sSQLSelect = "select top 1 id_order from manage_order order by id_order desc";

                        var checkLastIDOrder = GSDbContext.Database.SqlQuery<GetLastOrderID>(sSQLSelect).SingleOrDefault();                        
                        if (checkLastIDOrder != null)
                        {
                            int iStatus = Convert.ToInt32(status);
                            if (bulan != null && bulan.Length < 2)
                                bulan = "0" + bulan;
                            var vbulan = getMonth(bulan);
                            var checkalready = GSDbContext.ManageOrder.Where(p => p.status_order == iStatus && p.year_order == tahun && p.month_order == vbulan).ToList();

                            if(checkalready.Count() > 0 )
                            {
                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + "Data Already exist !";

                                ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                                manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                                manageHistoryTransaction.type_history = "upload_file_order";
                                manageHistoryTransaction.desc_history = "Upload Order : " + responseAPI.messages + "\r\n\r\n";
                                GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                                GSDbContext.SaveChanges();

                                return responseAPI;
                            }

                            if (!string.IsNullOrEmpty(checkLastIDOrder.id_order))
                            {

                                string sSQLSelectCheckForNow = "select top 1 id_order from manage_order where id_order like '" + "OD" + dateNow.ToString("yyMMdd") + "%' order by id_order desc";

                                var checkLastIDWithDateNowOrder = GSDbContext.Database.SqlQuery<GetLastOrderID>(sSQLSelectCheckForNow).SingleOrDefault();

                                if (checkLastIDWithDateNowOrder != null)
                                {
                                    int lastID = Convert.ToInt32(checkLastIDWithDateNowOrder.id_order.Substring(8, 5)) + 1;
                                    noIDLastOrder = checkLastIDWithDateNowOrder.id_order.Substring(0, 8) + lastID.ToString().PadLeft(5, '0');
                                }
                                else
                                {
                                    int lastID = 1;
                                    noIDLastOrder = "OD" + dateNow.ToString("yyMMdd") + lastID.ToString().PadLeft(5, '0');
                                }

                            }
                        }
                        else
                        {
                            var z = 1;
                            noIDLastOrder = "OD" + dateNow.ToString("yyMMdd") + z.ToString().PadLeft(5, '0');
                        }
                    }
                    else
                    {
                        int iStatuss = Convert.ToInt32(status);
                        if (bulan != null && bulan.Length < 2)
                            bulan = "0" + bulan;
                        var vbulans = getMonth(bulan);
                        var checkalready = GSDbContext.ManageOrder.Where(p => p.status_order == iStatuss && p.year_order == tahun && p.month_order == vbulans).ToList();

                        if (checkalready.Count() > 0)
                        {
                            responseAPI.code = "200";
                            responseAPI.messages = "error: " + "Data Already exist !";

                            ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                            manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                            manageHistoryTransaction.type_history = "upload_file_order";
                            manageHistoryTransaction.desc_history = "Upload Order : " + responseAPI.messages + "\r\n\r\n";
                            GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                            GSDbContext.SaveChanges();

                            return responseAPI;
                        }

                        var checkOrderID = GSDbContext.ManageOrder.Where(p => p.id_order == orderid).ToList();
                        if(checkOrderID.Count() > 0)
                        {
                            noIDLastOrder = orderid;
                        }
                        else
                        {
                            noIDLastOrder = "Unknown Order ID";
                        }
                    }

                    if (!string.IsNullOrEmpty(strFilePath))
                    {
                        ManageDocumentOrder documentOrder = new ManageDocumentOrder();
                        documentOrder.id_order = noIDLastOrder;
                        documentOrder.doc = Utils.Helper.base64Encode(strFilePath);
                        GSDbContext.ManageDocumentOrder.Add(documentOrder);
                        GSDbContext.SaveChanges();
                    }

                    for (i = 0; i < dt.Rows.Count; i++)
                    {
                        iRow = i;
                        if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[0].ToString()) && !string.IsNullOrEmpty(noIDLastOrder))
                        {
                            var pnCustomer = dt.Rows[i].ItemArray[0].ToString();
                            var getMasterIterPartNumber = GSDbContext.ManageItemPartNumber.Where(p => p.pn_customer == pnCustomer).SingleOrDefault();

                            if (getMasterIterPartNumber != null)
                            {
                                ManageOrder tempOrder = new ManageOrder();

                                tempOrder.id_order = noIDLastOrder;
                                //tempOrder.po_number = dt.Rows[i].ItemArray[0].ToString();
                                tempOrder.sales_order = null;
                                tempOrder.pn_customer = getMasterIterPartNumber.pn_customer;
                                tempOrder.pn_gs = getMasterIterPartNumber.pn_gs;
                                tempOrder.lot_size = getMasterIterPartNumber.lot_size;
                                tempOrder.type_battery = getMasterIterPartNumber.battery_type;
                                tempOrder.type_material = getMasterIterPartNumber.material_type;
                                tempOrder.brand = getMasterIterPartNumber.brand;
                                tempOrder.group_order = getMasterIterPartNumber.group_item;
                                tempOrder.spec = getMasterIterPartNumber.spec;
                                tempOrder.status_order = Convert.ToInt32(status);
                                tempOrder.insert_time = DateTime.UtcNow.AddHours(7);
                                tempOrder.update_time = null;
                                tempOrder.user_input = null;

                                if (bulan != null && bulan.Length < 2)
                                    bulan = "0" + bulan;

                                if (bulan != null)
                                {
                                    var namaBulan = getMonth(bulan);

                                    tempOrder.month_order = namaBulan;
                                    tempOrder.year_order = tahun;

                                    int noOfDaysInMonth = -1;
                                    if (tahun != null && bulan != null)
                                        noOfDaysInMonth = DateTime.DaysInMonth(Convert.ToInt32(tahun), Convert.ToInt32(bulan));


                                    DateTime dtOrder = DateTime.ParseExact(tahun + bulan + Convert.ToString(noOfDaysInMonth), "yyyyMMdd", CultureInfo.InvariantCulture);
                                    tempOrder.order_date = dtOrder;
                                }


                                var qty_ship_to_JKT = dt.Rows[i].ItemArray[3].ToString().Replace('-', '0');
                                var qty_ship_to_BDG = dt.Rows[i].ItemArray[4].ToString().Replace('-', '0');
                                var qty_ship_to_SBY = dt.Rows[i].ItemArray[5].ToString().Replace('-', '0');
                                var qty_ship_to_SMG = dt.Rows[i].ItemArray[6].ToString().Replace('-', '0');
                                var qty_total = dt.Rows[i].ItemArray[7].ToString().Replace('-', '0');
                                var qty_confirm = dt.Rows[i].ItemArray[8].ToString().Replace('-', '0');
                                var qty_adjustment = dt.Rows[i].ItemArray[9].ToString().Replace('-', '0');


                                Decimal dqty_jkt = 0;
                                Decimal dqty_bdg = 0;
                                Decimal dqty_sby = 0;
                                Decimal dqty_smg = 0;
                                Decimal dqty_total = 0;
                                Decimal dqty_confirm = 0;
                                Decimal dqty_adjustment = 0;


                                if (!string.IsNullOrEmpty(qty_ship_to_JKT))
                                    dqty_jkt = Convert.ToDecimal(qty_ship_to_JKT);

                                if (!string.IsNullOrEmpty(qty_ship_to_BDG))
                                    dqty_bdg = Convert.ToDecimal(qty_ship_to_BDG);

                                if (!string.IsNullOrEmpty(qty_ship_to_SBY))
                                    dqty_sby = Convert.ToDecimal(qty_ship_to_SBY);

                                if (!string.IsNullOrEmpty(qty_ship_to_SMG))
                                    dqty_smg = Convert.ToDecimal(qty_ship_to_SMG);

                                if (!string.IsNullOrEmpty(qty_total))
                                    dqty_total = Convert.ToDecimal(qty_total);

                                if (!string.IsNullOrEmpty(qty_confirm))
                                    dqty_confirm = Convert.ToDecimal(qty_confirm);

                                if (!string.IsNullOrEmpty(qty_adjustment))
                                    dqty_adjustment = Convert.ToDecimal(qty_adjustment);

                                tempOrder.ship_to_JKT = dqty_jkt;
                                tempOrder.ship_to_BDG = dqty_bdg;
                                tempOrder.ship_to_SBY = dqty_sby;
                                tempOrder.ship_to_SMG = dqty_smg;
                                tempOrder.total = dqty_total;
                                tempOrder.confirm = dqty_confirm;
                                tempOrder.adjustment = dqty_adjustment;
                                listtempOrder.Add(tempOrder);
                            }
                            else
                            {
                                // data tidak ada
                                responseAPI.code = "200";
                                responseAPI.messages = "error: " + "Data Master Item Part Number/PN Customer/PN GS dengan : " + dt.Rows[i].ItemArray[0].ToString() + " Tidak ditemukan!.";
                               
                                ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                                manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                                manageHistoryTransaction.type_history = "upload_file_order";
                                manageHistoryTransaction.desc_history = "Upload Order : " + responseAPI.messages + "\r\n\r\n";
                                manageHistoryTransaction.desc_history += "Row : " + iRow;
                                GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                                GSDbContext.SaveChanges();
                            }
                        }
                    }

                    if (listtempOrder.Count > 0)
                    {
                        GSDbContext.ManageOrder.AddRange(listtempOrder);
                        GSDbContext.SaveChanges();
                        responseAPI.code = "200";
                        responseAPI.messages = "Process Upload Complete";

                        var getDataListEmail = GSDbContext.ManageEmail.Where(p => p.category == "MKT_TO_PPC").FirstOrDefault();
                        if(getDataListEmail != null)
                        {
                            if(getDataListEmail.email_to.Length > 0 && getDataListEmail.email_from.Length > 0)
                            {
                                if (getDataListEmail.email_to.Substring(0, 1) == ",")
                                    getDataListEmail.email_to = getDataListEmail.email_to.Substring(0, getDataListEmail.email_to.Length - 1);

                                if (getDataListEmail.email_from.Substring(0, 1) == ",")
                                    getDataListEmail.email_from = getDataListEmail.email_from.Substring(0, getDataListEmail.email_from.Length - 1);

                                if (getDataListEmail.email_to.Substring(getDataListEmail.email_to.Length - 1, 1) == ",")
                                    getDataListEmail.email_to = getDataListEmail.email_to.Substring(0, getDataListEmail.email_to.Length - 1);

                                if (getDataListEmail.email_from.Substring(getDataListEmail.email_from.Length - 1, 1) == ",")
                                    getDataListEmail.email_from = getDataListEmail.email_from.Substring(0, getDataListEmail.email_from.Length - 1);

                                ManageController.SendEmail_Notification(getDataListEmail.email_to, getDataListEmail.email_from, "New Order From Marketing Division : " + noIDLastOrder, "New Order has been released by Marketing Division!", noIDLastOrder, "https://gs-scheduler.gs.astra.co.id/Login?order=" + noIDLastOrder, strFilePath);
                            }                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    responseAPI.code = "200";
                    responseAPI.messages = "error: " + ex.Message.ToString();

                    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                    manageHistoryTransaction.type_history = "upload_file_order";
                    manageHistoryTransaction.desc_history = "Upload Order : " + ex.Message.ToString() + "\r\n\r\n";
                    manageHistoryTransaction.desc_history += "Row : " + iRow;
                    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
                    GSDbContext.SaveChanges();
                }
            }

            return responseAPI;
        }

        [SessionCheck]
        public FileResult DownloadTemplateBusinessPlan()
        {
            //#if DEBUG
            string file = Server.MapPath("~/Content/file-template/");
            //#else
            //            string file = Server.MapPath("~/marketing/Content/file-template/");
            //#endif
            if (!Directory.Exists(file))
            {
                Directory.CreateDirectory(file);
            }
            file = file + "TEMPLATE-UPLOAD-BUSINESS-PLAN.xlsx";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, "application/octet-stream", Path.GetFileName(file));
        }

        [SessionCheck]
        public FileResult DownloadTemplateYearlyPlan()
        {
//#if DEBUG
            string file = Server.MapPath("~/Content/file-template/");
//#else
//            string file = Server.MapPath("~/marketing/Content/file-template/");
//#endif
            if (!Directory.Exists(file))
            {
                Directory.CreateDirectory(file);
            }
            file = file + "TEMPLATE-UPLOAD-YEARLY-PLAN.xlsx";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, "application/octet-stream", Path.GetFileName(file));
        }

        [SessionCheck]
        public FileResult DownloadTemplateItemPartNumber()
        {
//#if DEBUG
            string file = Server.MapPath("~/Content/file-template/");
//#else
//            string file = Server.MapPath("~/marketing/Content/file-template/");
//#endif
            if (!Directory.Exists(file))
            {
                Directory.CreateDirectory(file);
            }
            file = file + "TEMPLATE-UPLOAD-MASTER-PART-NUMBER.xlsx";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, "application/octet-stream", Path.GetFileName(file));
        }

        [SessionCheck]
        public FileResult DownloadTemplateForecast()
        {
//#if DEBUG
            string file = Server.MapPath("~/Content/file-template/");
//#else

//            string file = Server.MapPath("~/Content/file-template/");
//#endif
            //if (!Directory.Exists(file))
            //{
            //    Directory.CreateDirectory(file);
            //}
            file = file + "TEMPLATE-UPLOAD-FORECAST.xls";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, "application/octet-stream", Path.GetFileName(file));
        }

        [SessionCheck]
        public FileResult DownloadTemplateOrder()
        {
//#if DEBUG
            string file = Server.MapPath("~/Content/file-template/");
//#else
//            string file = Server.MapPath("~/marketing/Content/file-template/");
//#endif
            //if (!Directory.Exists(file))
            //{
            //    Directory.CreateDirectory(file);
            //}
            file = file + "TEMPLATE-UPLOAD-ORDER.xlsx";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, "application/octet-stream", Path.GetFileName(file));
        }

        private string getMonth(string month)
        {
            string namaBulan = "";
            switch (month)
            {
                case "01":
                    namaBulan = "Januari";
                    break;
                case "02":
                    namaBulan = "Februari";
                    break;
                case "03":
                    namaBulan = "Maret";
                    break;
                case "04":
                    namaBulan = "April";
                    break;
                case "05":
                    namaBulan = "Mei";
                    break;
                case "06":
                    namaBulan = "Juni";
                    break;
                case "07":
                    namaBulan = "Juli";
                    break;
                case "08":
                    namaBulan = "Agustus";
                    break;
                case "09":
                    namaBulan = "September";
                    break;
                case "10":
                    namaBulan = "Oktober";
                    break;
                case "11":
                    namaBulan = "November";
                    break;
                case "12":
                    namaBulan = "Desember";
                    break;

            }

            return namaBulan;
        }

        [SessionCheck]
        public ActionResult DownloadFromOrderBy_ID(string ID_ORDER)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                worksheet.Cells[1, 1].Value = "ORDER ID";
                worksheet.Cells[1, 2].Value = "PN CUSTOMER";
                worksheet.Cells[1, 3].Value = "PN GS";
                worksheet.Cells[1, 4].Value = "BATTERY TYPE";
                worksheet.Cells[1, 5].Value = "MATERIAL TYPE";
                worksheet.Cells[1, 6].Value = "BRAND";
                worksheet.Cells[1, 7].Value = "GROUP";
                worksheet.Cells[1, 8].Value = "MONTH";
                worksheet.Cells[1, 9].Value = "YEAR";
                worksheet.Cells[1, 10].Value = "JKT";
                worksheet.Cells[1, 11].Value = "BDG";
                worksheet.Cells[1, 12].Value = "SBY";
                worksheet.Cells[1, 13].Value = "SMG";
                worksheet.Cells[1, 14].Value = "TOTAL";
                worksheet.Cells[1, 15].Value = "CONFIRM JKT";
                worksheet.Cells[1, 16].Value = "CONFIRM BDG";
                worksheet.Cells[1, 17].Value = "CONFIRM SBY";
                worksheet.Cells[1, 18].Value = "CONFIRM SMG";
                worksheet.Cells[1, 19].Value = "CONFIRM";
                worksheet.Cells[1, 20].Value = "ADJUSTMENT";
                worksheet.Cells[1, 21].Value = "STATUS";
                worksheet.Cells[1, 22].Value = "UPLOAD DATE";
                worksheet.Cells[1, 23].Value = "INSERT DATE";
                worksheet.Cells[1, 24].Value = "LAST UPDATE";

                #region FORMATING EXCEL
                using (var range = worksheet.Cells[1, 1, 1, 24])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                #endregion

                var fileLocation = System.IO.File.ReadAllText(Server.MapPath(@"~\Content\json_data\StatusOrder.json"));
                List<StatusOrder> myDeserializedObjList = (List<StatusOrder>)Newtonsoft.Json.JsonConvert.DeserializeObject(fileLocation, typeof(List<StatusOrder>));

                var sqlQuery = "SELECT * " +
                    " FROM db_marketing_portal.dbo.manage_order WHERE id_order = '" + ID_ORDER + "' ";
                var resultTable = GSDbContext.Database.SqlQuery<ManageOrder>(sqlQuery).ToList();
                var i = 0;   

                foreach (var data in resultTable)
                {
                    worksheet.Cells[2 + i, 1].Value = data.id_order.ToString();
                    worksheet.Cells[2 + i, 2].Value = data.pn_customer.ToString();
                    worksheet.Cells[2 + i, 3].Value = data.pn_gs.ToString();
                    worksheet.Cells[2 + i, 4].Value = data.type_battery.ToString();
                    worksheet.Cells[2 + i, 5].Value = data.type_material.ToString();
                    worksheet.Cells[2 + i, 6].Value = data.brand.ToString();
                    worksheet.Cells[2 + i, 7].Value = data.group_order.ToString();
                    worksheet.Cells[2 + i, 8].Value = data.month_order.ToString();
                    worksheet.Cells[2 + i, 9].Value = data.year_order.ToString();
                    if(data.ship_to_JKT > 0)
                    {
                        worksheet.Cells[2 + i, 10].Value = Convert.ToDecimal(data.ship_to_JKT.ToString());
                    }
                    else if(data.ship_to_JKT == 0)
                    {
                        worksheet.Cells[2 + i, 10].Value = 0;
                    }
                    if (data.ship_to_BDG > 0)
                    {
                        worksheet.Cells[2 + i, 11].Value = Convert.ToDecimal(data.ship_to_BDG.ToString());
                    }
                    else if (data.ship_to_BDG == 0)
                    {
                        worksheet.Cells[2 + i, 11].Value = 0;
                    }
                    if (data.ship_to_SBY > 0)
                    {
                        worksheet.Cells[2 + i, 12].Value = Convert.ToDecimal(data.ship_to_SBY.ToString());
                    }
                    else if (data.ship_to_SBY == 0)
                    {
                        worksheet.Cells[2 + i, 12].Value = 0;
                    }
                    if (data.ship_to_SMG > 0)
                    {
                        worksheet.Cells[2 + i, 13].Value = Convert.ToDecimal(data.ship_to_SMG.ToString());
                    }
                    else if (data.ship_to_SMG == 0)
                    {
                        worksheet.Cells[2 + i, 13].Value = 0;
                    }
                    if (data.total > 0)
                    {
                        worksheet.Cells[2 + i, 14].Value = Convert.ToDecimal(data.total.ToString());
                    }
                    else if (data.total == 0)
                    {
                        worksheet.Cells[2 + i, 14].Value = 0;
                    }
                    if (data.confirm_to_JKT > 0)
                    {
                        worksheet.Cells[2 + i, 15].Value = Convert.ToDecimal(data.confirm_to_JKT.ToString());
                    }
                    else if (data.confirm_to_JKT == 0)
                    {
                        worksheet.Cells[2 + i, 15].Value = 0;
                    }
                    if (data.confirm_to_BDG > 0)
                    {
                        worksheet.Cells[2 + i, 16].Value = Convert.ToDecimal(data.confirm_to_BDG.ToString());
                    }
                    else if (data.confirm_to_BDG == 0)
                    {
                        worksheet.Cells[2 + i, 16].Value = 0;
                    }
                    if (data.confirm_to_SBY > 0)
                    {
                        worksheet.Cells[2 + i, 17].Value = Convert.ToDecimal(data.confirm_to_SBY.ToString());
                    }
                    else if (data.confirm_to_SBY == 0)
                    {
                        worksheet.Cells[2 + i, 17].Value = 0;
                    }
                    if (data.confirm_to_SMG > 0)
                    {
                        worksheet.Cells[2 + i, 18].Value = Convert.ToDecimal(data.confirm_to_SMG.ToString());
                    }
                    else if (data.confirm_to_SMG == 0)
                    {
                        worksheet.Cells[2 + i, 18].Value = 0;
                    }
                    if (data.confirm > 0)
                    {
                        worksheet.Cells[2 + i, 19].Value = Convert.ToDecimal(data.confirm.ToString());
                    }
                    else if (data.confirm == 0)
                    {
                        worksheet.Cells[2 + i, 19].Value = 0;
                    }
                    if (data.adjustment > 0)
                    {
                        worksheet.Cells[2 + i, 20].Value = Convert.ToDecimal(data.adjustment.ToString());
                    }
                    else if (data.adjustment == 0)
                    {
                        worksheet.Cells[2 + i, 20].Value = 0;
                    }

                    var dataStatus = myDeserializedObjList.Where(p => p.ID == data.status_order).SingleOrDefault();
                    if(dataStatus!= null)
                        worksheet.Cells[2 + i, 21].Value = dataStatus.Name.ToString();
                    worksheet.Cells[2 + i, 22].Value = data.order_date.ToString();
                    worksheet.Cells[2 + i, 23].Value = data.insert_time.ToString();
                    worksheet.Cells[2 + i, 24].Value = data.update_time.ToString();
                    i++;
                }


                if (i > 0)
                {
                    using (var range = worksheet.Cells[2, 1, i + 1, 24])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }

                worksheet.Cells.AutoFitColumns(0);
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DOWNLOAD-ORDER-"+ID_ORDER+".xlsx");
            }
        }

        [SessionCheck]
        public FileResult DownloadAttachmentIDL(string doc)
        {
            //#if DEBUG
            //string file = Server.MapPath("~/Content/attach_activity/");
            //#else
            //            string file = Server.MapPath("~/marketing/Content/file-template/");
            //#endif
            //if (!Directory.Exists(file))
            //{
            //    Directory.CreateDirectory(file);
            //}
            //file = file + "TEMPLATE-UPLOAD-ORDER.xlsx";
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(doc, "application/octet-stream", Path.GetFileName(doc));
        }

        [SessionCheck]
        public ActionResult DownloadItemNumberFromOrder()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                worksheet.Cells[1, 1].Value = "PN CUSTOMER";
                worksheet.Cells[1, 2].Value = "PN GS";
                worksheet.Cells[1, 3].Value = "DESCRIPTION";
                worksheet.Cells[1, 4].Value = "PO JKT";
                worksheet.Cells[1, 5].Value = "PO BDG";
                worksheet.Cells[1, 6].Value = "PO SBY";
                worksheet.Cells[1, 7].Value = "PO SMG";
                worksheet.Cells[1, 8].Value = "TOTAL";
                worksheet.Cells[1, 9].Value = "KONFIRMASI";
                worksheet.Cells[1, 10].Value = "ADJUSTMENT";
                worksheet.Cells[1, 11].Value = " ";
                worksheet.Cells[2, 12].Value = "*KETERANGAN: HARAP MENGISI PADA KOLOM BERWARNA ORANGE !";

                // PROTECT COLUMN FOR NOT EDITABLE
                worksheet.Protection.IsProtected = true;
                worksheet.Column(4).Style.Locked = false;
                worksheet.Column(5).Style.Locked = false;
                worksheet.Column(6).Style.Locked = false;
                worksheet.Column(7).Style.Locked = false;
                worksheet.Column(8).Style.Locked = false;
                worksheet.Cells[1, 4].Style.Locked = true;
                worksheet.Cells[1, 5].Style.Locked = true;
                worksheet.Cells[1, 6].Style.Locked = true;
                worksheet.Cells[1, 7].Style.Locked = true;
                worksheet.Cells[1, 8].Style.Locked = true;

                #region FORMATING EXCEL
                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                using (var range = worksheet.Cells[2, 12, 3, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                using (var range = worksheet.Cells[1, 4, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                //using (var range = worksheet.Cells[1, 10, 1, 10])
                //{
                //    range.Style.Font.Bold = true;
                //    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                //    range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                //    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                //    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                //    //range.Style.Font.Color.SetColor(Color.White);
                //}
                #endregion

                var sqlQuery = "SELECT pn_customer AS PN_CUSTOMER, pn_gs AS PN_GS FROM manage_item_partnumber ";
                var resultTable = GSDbContext.Database.SqlQuery<TemplateItemPartNumber_untukOrder>(sqlQuery).ToList();
                var i = 0;
                foreach (var data in resultTable)
                {
                    worksheet.Cells[2 + i, 1].Value = data.PN_CUSTOMER.ToString();
                    worksheet.Cells[2 + i, 2].Value = data.PN_GS.ToString();
                    i++;
                }

                // FORMAT COLOR FOR INPUT DISABLED
                for (var p = 1; p <= 10; p++)
                {
                    if (p != 4 && p != 5 && p != 6 && p != 7 && p != 8)
                    {
                        using (var range = worksheet.Cells[2, p, i + 1, 10])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.Silver);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            //range.Style.Font.Color.SetColor(Color.White);
                        }
                    }
                }

                // FORMAT COLOR FOR INPUT ENABLED
                for (var p = 4; p <= 8; p++)
                {
                    if (p == 4 || p == 5 || p == 6 || p == 7 || p == 8)
                    {
                        using (var range = worksheet.Cells[2, p, i + 1, 8])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            //range.Style.Font.Color.SetColor(Color.White);
                        }
                    }
                }


                if (i > 0)
                {
                    using (var range = worksheet.Cells[2, 1, i + 1, 10])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }                

                worksheet.Cells.AutoFitColumns(0);
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TEMPLATE-UPLOAD-ORDER.xlsx");
            }
        }

        [SessionCheck]
        public ActionResult DownloadItemNumberFromForecast()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                worksheet.Cells[1, 1].Value = "PN GS";
                worksheet.Cells[1, 2].Value = "N2";
                worksheet.Cells[1, 3].Value = "N3";
                worksheet.Cells[1, 4].Value = "N4";
                worksheet.Cells[1, 5].Value = "N2 DATE";
                worksheet.Cells[2, 5].Value = "31/05/2022";
                worksheet.Cells[1, 6].Value = "N3 DATE";
                worksheet.Cells[2, 6].Value = "30/06/2022";
                worksheet.Cells[1, 7].Value = "N4 DATE";
                worksheet.Cells[2, 7].Value = "31/07/2022";

                #region FORMATING EXCEL
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                #endregion

                var sqlQuery = "SELECT pn_customer AS PN_CUSTOMER, pn_gs AS PN_GS FROM manage_item_partnumber ";
                var resultTable = GSDbContext.Database.SqlQuery<TemplateItemPartNumber_untukOrder>(sqlQuery).ToList();
                var i = 0;
                foreach (var data in resultTable)
                {
                    worksheet.Cells[2 + i, 1].Value = data.PN_GS.ToString();
                    i++;
                }

                if (i > 0)
                {
                    using (var range = worksheet.Cells[2, 1, i + 1, 7])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }                    

                worksheet.Cells.AutoFitColumns(0);
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TEMPLATE-UPLOAD-FORECAST.xlsx");
            }
        }

        [SessionCheck]
        public ActionResult DownloadItemNumberFromBusinessPlan()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                worksheet.Cells[1, 1].Value = "PN GS";
                worksheet.Cells[1, 2].Value = "PN CUSTOMER";
                worksheet.Cells[1, 3].Value = "Jan";
                worksheet.Cells[1, 4].Value = "Feb";
                worksheet.Cells[1, 5].Value = "Mar";
                worksheet.Cells[1, 6].Value = "Apr";
                worksheet.Cells[1, 7].Value = "May";
                worksheet.Cells[1, 8].Value = "Jun";
                worksheet.Cells[1, 9].Value = "Jul";
                worksheet.Cells[1, 10].Value = "Aug";
                worksheet.Cells[1, 11].Value = "Sept";
                worksheet.Cells[1, 12].Value = "Okt";
                worksheet.Cells[1, 13].Value = "Nov";
                worksheet.Cells[1, 14].Value = "Des";
                worksheet.Cells[1, 15].Value = "TOTAL";

                #region FORMATING EXCEL
                using (var range = worksheet.Cells[1, 1, 1, 15])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                #endregion

                var sqlQuery = "SELECT pn_customer AS PN_CUSTOMER, pn_gs AS PN_GS FROM manage_item_partnumber ";
                var resultTable = GSDbContext.Database.SqlQuery<TemplateItemPartNumber_untukOrder>(sqlQuery).ToList();
                var i = 0;
                foreach (var data in resultTable)
                {
                    worksheet.Cells[2 + i, 1].Value = data.PN_GS.ToString();
                    worksheet.Cells[2 + i, 2].Value = data.PN_CUSTOMER.ToString();
                    worksheet.Cells[2 + i, 15].Value = 0;
                    i++;
                }

                if (i > 0)
                {
                    using (var range = worksheet.Cells[2, 1, i + 1, 15])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }

                worksheet.Cells.AutoFitColumns(0);
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TEMPLATE-UPLOAD-BUSINESS-PLAN.xlsx");
            }
        }

        [SessionCheck]
        public ActionResult DownloadItemNumberFromYearlyPlan()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SHEET");
                worksheet.Cells[1, 1].Value = "PN GS";
                worksheet.Cells[1, 2].Value = "PN CUSTOMER";
                worksheet.Cells[1, 3].Value = "Jan";
                worksheet.Cells[1, 4].Value = "Feb";
                worksheet.Cells[1, 5].Value = "Mar";
                worksheet.Cells[1, 6].Value = "Apr";
                worksheet.Cells[1, 7].Value = "May";
                worksheet.Cells[1, 8].Value = "Jun";
                worksheet.Cells[1, 9].Value = "Jul";
                worksheet.Cells[1, 10].Value = "Aug";
                worksheet.Cells[1, 11].Value = "Sept";
                worksheet.Cells[1, 12].Value = "Okt";
                worksheet.Cells[1, 13].Value = "Nov";
                worksheet.Cells[1, 14].Value = "Des";
                worksheet.Cells[1, 15].Value = "TOTAL";

                #region FORMATING EXCEL
                using (var range = worksheet.Cells[1, 1, 1, 15])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                #endregion

                var sqlQuery = "SELECT pn_customer AS PN_CUSTOMER, pn_gs AS PN_GS FROM manage_item_partnumber ";
                var resultTable = GSDbContext.Database.SqlQuery<TemplateItemPartNumber_untukOrder>(sqlQuery).ToList();
                var i = 0;
                foreach (var data in resultTable)
                {
                    worksheet.Cells[2 + i, 1].Value = data.PN_GS.ToString();
                    worksheet.Cells[2 + i, 2].Value = data.PN_CUSTOMER.ToString();
                    worksheet.Cells[2 + i, 15].Value = 0;
                    i++;
                }

                if (i > 0)
                {
                    using (var range = worksheet.Cells[2, 1, i + 1, 15])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }

                worksheet.Cells.AutoFitColumns(0);
                byte[] byteData = package.GetAsByteArray();
                return File(byteData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TEMPLATE-UPLOAD-YEARLY-PLAN.xlsx");
            }
        }
    }
}