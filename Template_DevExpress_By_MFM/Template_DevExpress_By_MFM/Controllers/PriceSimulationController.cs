using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Controllers;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class PriceSimulationController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public PriceSimulationController()
        {
            GSDbContext = new GSDbContext(@"", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            string sSQLSelectHeader;
            try
            {
                if (sessionLogin.userrole == "customer")
                {
                    sSQLSelectHeader = "select distinct(price_sim_id), cust_id, DATENAME(month,price_sim_createDate) as month_order, DATENAME(YEAR,price_sim_createDate) year_order, [price_sim_status] from [t_price_simulation] where cust_id = '"+sessionLogin.customer+"'";
                }
                else
                {
                    sSQLSelectHeader = "select distinct(price_sim_id), cust_id, DATENAME(month,price_sim_createDate) as month_order, DATENAME(YEAR,price_sim_createDate) year_order, [price_sim_status] from [t_price_simulation]";
                }
                var dataList = GSDbContext.Database.SqlQuery<ListHeaderManagePriceSimulation>(sSQLSelectHeader).ToList();
                //var dataList = GSDbContext.ManagePriceSimulation.ToList();
                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }


        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string PN)
        {
            LoadResult loadResult = new LoadResult();
            var dateNow = DateTime.UtcNow.AddHours(7);
            //dateNow.AddMonths(-3);
            var bln = dateNow.ToString("MM");
            decimal avg_lme = 0, avg_lmePrev = 0;
            decimal new_price = 0;
            //var blnAw = dateNow.ToString("yyyy") + "-" + Convert.ToInt32(bln) - 3 + "-" + dateNow.ToString("dd");

            try
            {
                

                if (sessionLogin.periodic_price.Equals("Monthly") || sessionLogin.periodic_price == "Monthly")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 20);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, 21);

                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-3).ToString("yyyy-MM-dd") + "' and '" + dateAk.AddMonths(-2).ToString("yyyy-MM-dd") + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-4).ToString("yyyy-MM-dd") + "' and '" + dateAk.AddMonths(-3).ToString("yyyy-MM-dd") + "'";
                   
                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if(checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6/10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        if(sPrevPrice != null)
                        {
                            var prev_price = sPrevPrice.prev_price;
                            new_price = (decimal)prev_price * new_price_fluct;
                        }
                        
                    }

                } 
                else if(sessionLogin.periodic_price.Equals("3 months") || sessionLogin.periodic_price == "3 months")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-2);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-3);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                        //string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where [lme_month] between " + blnAw + " and " + blnAk + "";
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-4) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        //avg_lme = (decimal)checkAverageLME.avg_lme;
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        var prev_price = sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (sessionLogin.periodic_price.Equals("6 months") || sessionLogin.periodic_price == "6 months")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-4);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-5);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-10) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-11) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        var prev_price = sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (sessionLogin.periodic_price.Equals("Monthly (AOP Cust)") || sessionLogin.periodic_price == "Monthly (AOP Cust)")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-3);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-4);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-6) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        var prev_price = sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }

                new_price = Math.Round(new_price, 3);
                return Request.CreateResponse(HttpStatusCode.Created, new_price);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int req_qty, string PN, int? L_pallet, int? S_pallet)
        {
            LoadResult loadResult = new LoadResult();
            int modL, diffL = 0, totL = 0;
            int modS, diffS = 0, totS = 0;
            int adj = 0, difA=0, difB=0; //difA=selisih terbaru, difB=selisih terbaru-1

            var AdjustQty = "";
            var tempAdjustQty="";
            var dateNow = DateTime.UtcNow.AddHours(7);
            //dateNow.AddMonths(-3);
            var bln = dateNow.ToString("MM");
            var thn = dateNow.ToString("yyyy");
            decimal avg_lme = 0, avg_lmePrev = 0;
            decimal new_price = 0, prev_price = 0;
            //var blnAw = dateNow.ToString("yyyy") + "-" + Convert.ToInt32(bln) - 3 + "-" + dateNow.ToString("dd");
            int bul = Convert.ToInt32(bln);
            int prev_bul;
            int prev_thn;
            if (bul == 1)
            {
                prev_bul = 12;
                prev_thn = Convert.ToInt32(thn) - 1;
            }
            else
            {
                prev_bul = bul - 1;
                prev_thn = Convert.ToInt32(thn);
            }


            try
            {
                if (!L_pallet.HasValue || !S_pallet.HasValue)
                {
                    if (!L_pallet.HasValue)
                    {
                        modL = 0;

                    }
                    else
                    {
                        modL = (int)(req_qty % L_pallet);
                        diffL = (int)(L_pallet - modL);
                        //if (diffL < (0.5 * L_pallet))
                        //{
                        //    adj = req_qty + diffL;
                        //    totL = (int)(req_qty / L_pallet)+1;
                        //}
                        //else
                        //{
                        //    adj = req_qty - modL;
                        //    totL = (int)(req_qty / L_pallet);
                        //}
                    }

                    if (!S_pallet.HasValue)
                    {
                        modS = 0;
                    }
                    else
                    {
                        modS = (int)(req_qty % S_pallet);
                        diffS = (int)(S_pallet - modS);
                        //if (diffS < (0.5 * S_pallet))
                        //{
                        //    adj = req_qty + diffS;
                        //    totS = (int)(req_qty / S_pallet)+1;
                        //}
                        //else
                        //{
                        //    adj = req_qty - modS;
                        //    totS = (int)(req_qty / S_pallet);
                        //}
                    }

                    if (modL < modS && modL != 0)
                    {
                        if (diffL < (0.5 * L_pallet))
                        {
                            adj = req_qty + diffL;
                            totL = (int)(req_qty / L_pallet) + 1;
                        }
                        else
                        {
                            adj = req_qty - modL;
                            totL = (int)(req_qty / L_pallet);
                        }
                        AdjustQty = adj + "||" + totL + "||" + 0;
                    }
                    else if (modL == modS)
                    {
                        AdjustQty = "seimbang";
                    }
                    else
                    {
                        if (diffS < (0.5 * S_pallet))
                        {
                            adj = req_qty + diffS;
                            totS = (int)(req_qty / S_pallet) + 1;
                        }
                        else
                        {
                            adj = req_qty - modS;
                            totS = (int)(req_qty / S_pallet);
                        }
                        AdjustQty = adj + "||" + 0 + "||" + totS;
                    }
                }
                else
                {
                    //pallet L = rows, pallet S = cols
                    int[,] x = new int[11, 11];
                    int flag = 0, kecil = 0;    //kecil = selisih terkecil
                    int temp_val = 0;
                    int closestValue = x[0, 0]; // Assume the first element is the closest initially

                    for (int i = 0; i < 11; i++) //initialize rows
                    {
                        for (int j = 0; j < 11; j++) //initialize cols
                        {
                            int val = (int)((i * L_pallet) + (j * S_pallet));
                            x[i, j] = val;
                            difA = req_qty - val;
                            if (difA == 0)
                            {
                                adj = x[i, j];
                                AdjustQty = adj + "||" + i + "||" + j;

                                flag = 1;
                                break;
                                //if (i > j)
                                //{
                                //    adj = x[i, j];
                                //    AdjustQty = adj + "||" + i + "||" + j;

                                //    flag = 1;
                                //    break;
                                //} 
                            } else
                            {
                                //dari chatGPT
                                // Check if the current value is closer to the target value than the closest value
                                if (Math.Abs(val - req_qty) <= Math.Abs(closestValue - req_qty))
                                {
                                    closestValue = val; // Update the closest value
                                    //temp_val = x[i, j];
                                    tempAdjustQty = closestValue + "||" + i + "||" + j;
                                }
                                //end chat
                                //----------------BISA PAKE YG BAWAH JUGA HASIL PERHITUNGAN OMAR hehe---------------

                                //remove negatif values
                                //if (difA < 0 && difB < 0)
                                //{
                                //    difA = difA * -1;
                                //    difB = difB * -1;
                                //}
                                //else if (difA < 0)
                                //{
                                //    difA = difA * -1;
                                //}
                                //else if (difB < 0)
                                //{
                                //    difB = difB * -1;
                                //}
                                ////end

                                //if (difA < difB)  //difA=selisih terbaru, difB=selisih terbaru-1
                                //{
                                //    if (kecil < difA)
                                //    {
                                //        kecil = difA;
                                //    }
                                //    else
                                //    {
                                //        kecil = difA;
                                //        temp_val = x[i, j];
                                //        tempAdjustQty = temp_val + "||" + i + "||" + j;
                                //    }

                                //    Console.WriteLine("temp_val: " + temp_val);
                                //    Console.WriteLine("tempAdjustQty: " + tempAdjustQty);
                                //}
                            }
                            difB = difA;

                            AdjustQty = tempAdjustQty;
                        }
                        if(flag == 1)
                        {
                            break;
                        }
                    }
                }


                if (sessionLogin.periodic_price.Equals("Monthly") || sessionLogin.periodic_price == "Monthly")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 20);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, 21);

                    //var blnAw = Convert.ToInt32(bln) - 3;
                    //var blnAk = Convert.ToInt32(bln) - 2;
                    //var blnAwPrev = Convert.ToInt32(bln) - 4;
                    //var blnAkPrev = Convert.ToInt32(bln) - 3;

                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-3).ToString("yyyy-MM-dd") + "' and '" + dateAk.AddMonths(-2).ToString("yyyy-MM-dd") + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-4).ToString("yyyy-MM-dd") + "' and '" + dateAk.AddMonths(-3).ToString("yyyy-MM-dd") + "'";
                    //string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateNow.ToString("yyyy") + "-" + blnAw + "-" + 20 + "' and '" + dateNow.ToString("yyyy") + "-" + blnAk + "-" + 21 + "'";
                    //string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateNow.ToString("yyyy") + "-" + blnAwPrev + "-" + 20 + "' and '" + dateNow.ToString("yyyy") + "-" + blnAkPrev + "-" + 21 + "'";
                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if(checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6/10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        //ini harusnya kalo 1 harga untuk 1 PN dan Customer
                        //string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' and cust_id="+sessionLogin.customer+" order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        if(sPrevPrice != null)
                        {
                            prev_price = (decimal)sPrevPrice.prev_price;
                            new_price = (decimal)prev_price * new_price_fluct;
                        }
                        
                    }

                } 
                else if(sessionLogin.periodic_price.Equals("3 months") || sessionLogin.periodic_price == "3 months")
                {
                    string sSQLSelect;
                    string sSQLSelectPrev;
                    var sCekCust = GSDbContext.MasterCustomer.Where(c => c.customer_id == sessionLogin.customer).SingleOrDefault();
                    if (sCekCust.customer_name == "GS Taiwan")
                    {

                        DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                        DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                        var endDate = dateAk.AddMonths(-3);
                        if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                        {
                            endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                        }
                        var endDatePrev = dateAk.AddMonths(-4);
                        if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                        {
                            endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                        }
                        //string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where [lme_month] between " + blnAw + " and " + blnAk + "";
                        sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDate + "'";
                        sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-6) + "' and '" + endDatePrev + "'";

                    }
                    else
                    {

                        DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                        DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                        var endDate = dateAk.AddMonths(-2);
                        if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                        {
                            endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                        }
                        var endDatePrev = dateAk.AddMonths(-3);
                        if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                        {
                            endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                        }
                        //string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where [lme_month] between " + blnAw + " and " + blnAk + "";
                        sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-4) + "' and '" + endDate + "'";
                        sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDatePrev + "'";

                    }

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        //avg_lme = (decimal)checkAverageLME.avg_lme;
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        //ini harusnya kalo 1 harga untuk 1 PN dan Customer
                        //string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' and cust_id="+sessionLogin.customer+" order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        prev_price = (decimal)sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (sessionLogin.periodic_price.Equals("6 months") || sessionLogin.periodic_price == "6 months")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-4);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-5);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-10) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-11) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        //ini harusnya kalo 1 harga untuk 1 PN dan Customer
                        //string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' and cust_id="+sessionLogin.customer+" order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        prev_price = (decimal)sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (sessionLogin.periodic_price.Equals("Monthly (AOP Cust)") || sessionLogin.periodic_price == "Monthly (AOP Cust)")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-3);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-4);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-6) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        //ini harusnya kalo 1 harga untuk 1 PN dan Customer
                        //string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' and cust_id="+sessionLogin.customer+" order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        prev_price = (decimal)sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (sessionLogin.periodic_price.Equals("No Change") || sessionLogin.periodic_price == "No Change")
                {
                    var CekPNinLogPrice = GSDbContext.ManageLogPrice.Where(e => e.log_part_number == PN && e.log_periode_int == bul && e.log_year == thn && e.cust_id == sessionLogin.customer).SingleOrDefault();
                    var CekPrevPrice = GSDbContext.ManageLogPrice.Where(e => e.log_part_number == PN && e.log_periode_int == prev_bul && e.log_year == prev_thn.ToString() && e.cust_id == sessionLogin.customer).SingleOrDefault();

                    prev_price = (decimal)CekPrevPrice.log_unit_price;
                    new_price = prev_price;
                }

                new_price = Math.Round(new_price, 3);
                #region CekGapPrice
                var gap_price_fluct = ((new_price - prev_price) / prev_price) * 100;
                if (gap_price_fluct >= 3 || gap_price_fluct <= -3)
                {
                    //diisi prev_price
                    return Request.CreateResponse(HttpStatusCode.Created, AdjustQty + "||" + prev_price);
                }
                else
                {
                    //diisi new_price
                    return Request.CreateResponse(HttpStatusCode.Created, AdjustQty + "||" + new_price);
                }
                #endregion
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }

        //[SessionCheck]
        //[HttpGet]
        //public HttpResponseMessage CekContainer(DataSourceLoadOptions loadOptions, string CustID, Boolean flag)
        //{
        //    LoadResult loadResult = new LoadResult();

        //    try
        //    {
        //        int i = 0;
        //        var noIDLastOrder = "";
        //        var dateNow = DateTime.UtcNow.AddHours(7);

        //        //string sSQLSelect = "select top 1 price_sim_id from t_price_simulation order by price_sim_id desc";
        //        string sSQLSelect = "select top 1 price_sim_id from t_price_simulation where price_sim_id like '" + CustID + "%' order by price_sim_id desc";

        //        var checkLastIDOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(sSQLSelect).SingleOrDefault();
        //        if (checkLastIDOrder != null)
        //        {   
        //            if (!string.IsNullOrEmpty(checkLastIDOrder.price_sim_id))
        //            {

        //                string sSQLSelectCheckForNow = "select top 1 price_sim_id from t_price_simulation where price_sim_id like '" + CustID + "-" + dateNow.ToString("MMMyyyy") + "%' order by price_sim_id desc";

        //                var checkLastIDWithDateNowOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(sSQLSelectCheckForNow).SingleOrDefault();

        //                if (checkLastIDWithDateNowOrder != null)
        //                {
        //                    int lastID = Convert.ToInt32(checkLastIDWithDateNowOrder.price_sim_id.Substring(16, 3)) + 1;
        //                    noIDLastOrder = checkLastIDWithDateNowOrder.price_sim_id.Substring(0, 16) + lastID.ToString().PadLeft(3, '0');
        //                }
        //                else
        //                {
        //                    int lastID = 1;
        //                    //noIDLastOrder = "OD" + dateNow.ToString("yyMMdd") + lastID.ToString().PadLeft(5, '0');
        //                    noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" + lastID.ToString().PadLeft(3, '0');
        //                }

        //            }
        //        }
        //        else
        //        {
        //            var z = 1;
        //            noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" +z.ToString().PadLeft(3, '0');
        //            //noIDLastOrder = "CUST001" + dateNow.ToString("yyMMdd") + z.ToString().PadLeft(5, '0');
        //        }
                
        //        return Request.CreateResponse(HttpStatusCode.Created, noIDLastOrder);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
        //    }
        //}

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string CustID, Boolean flag)
        {
            LoadResult loadResult = new LoadResult();

            try
            {
                int i = 0;
                var noIDLastOrder = "";
                var dateNow = DateTime.UtcNow.AddHours(7);

                //string sSQLSelect = "select top 1 price_sim_id from t_price_simulation order by price_sim_id desc";
                string sSQLSelect = "select top 1 price_sim_id from t_price_simulation where price_sim_id like '" + CustID + "%' order by price_sim_id desc";

                var checkLastIDOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(sSQLSelect).SingleOrDefault();
                if (checkLastIDOrder != null)
                {   
                    if (!string.IsNullOrEmpty(checkLastIDOrder.price_sim_id))
                    {

                        string sSQLSelectCheckForNow = "select top 1 price_sim_id from t_price_simulation where price_sim_id like '" + CustID + "-" + dateNow.ToString("MMMyyyy") + "%' order by price_sim_id desc";

                        var checkLastIDWithDateNowOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(sSQLSelectCheckForNow).SingleOrDefault();

                        if (checkLastIDWithDateNowOrder != null)
                        {
                            int lastID = Convert.ToInt32(checkLastIDWithDateNowOrder.price_sim_id.Substring(15, 3)) + 1;
                            noIDLastOrder = checkLastIDWithDateNowOrder.price_sim_id.Substring(0, 15) + lastID.ToString().PadLeft(3, '0');
                        }
                        else
                        {
                            int lastID = 1;
                            //noIDLastOrder = "OD" + dateNow.ToString("yyMMdd") + lastID.ToString().PadLeft(5, '0');
                            noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" + lastID.ToString().PadLeft(3, '0');
                        }

                    }
                }
                else
                {
                    var z = 1;
                    noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" +z.ToString().PadLeft(3, '0');
                    //noIDLastOrder = "CUST001" + dateNow.ToString("yyMMdd") + z.ToString().PadLeft(5, '0');
                }
                
                return Request.CreateResponse(HttpStatusCode.Created, noIDLastOrder);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }


        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage ViewDetails(string id, DataSourceLoadOptions loadOptions)
        {
            //var query = "SELECT id_recnum_order, id_order, po_number, sales_order, pn_customer, lot_size, type_battery, type_material, brand, group_order, spec, status_order, insert_time, update_time, user_input, pn_gs, month_order, year_order, " +
            //    " CASE WHEN confirm != 0 THEN confirm " +
            //    " WHEN confirm = 0 THEN total ELSE total END as total , " +
            //    " confirm, ship_to_JKT, ship_to_BDG, ship_to_SBY, ship_to_SMG, order_date, confirm_to_JKT, confirm_to_BDG, confirm_to_SBY, confirm_to_SMG, adjustment, file_excel " +
            //    " FROM dbo.manage_order WHERE id_order = '" + id + "'";
            var dataList = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == id).ToList();
            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            //var detail = GSDbContext.Database.SqlQuery<ManageOrder>(query).ToList();
            ////var detail = GSDbContext.ManageOrder.Where(e => e.id_order == id).ToList();

            //return Request.CreateResponse(DataSourceLoader.Load(detail, loadOptions));
        }


        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var values = form.Get("values");

            var newOrder = new ManagePriceSimulation_temp();
            JsonConvert.PopulateObject(values, newOrder);

            Validate(newOrder);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            newOrder.price_sim_createDate = DateTime.UtcNow.AddHours(7);
            newOrder.price_sim_createBy = sessionLogin.fullname;

            GSDbContext.ManagePriceSimulation_temp.Add(newOrder);
            GSDbContext.SaveChanges();

            //ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
            //manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
            //manageHistoryTransaction.type_history = "transaction_order";
            //manageHistoryTransaction.desc_history = "Order added : " + newOrder.id_order;
            //GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
            //GSDbContext.SaveChanges();

           

            return Request.CreateResponse(HttpStatusCode.Created);
        }  
        
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Post_Header(string labelID, string container)
        {
            if (!String.IsNullOrEmpty(labelID))
            {
                var checkDataDuplicate = "select top 1 price_sim_id from t_price_simulation where price_sim_id = '"+labelID+"'";
                var checkLastIDWithDateNowOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(checkDataDuplicate).SingleOrDefault();

                if (checkLastIDWithDateNowOrder != null)
                {
                    //if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data already exist!");
                }
                else
                {
                    List<ManagePriceSimulation> listManage = new List<ManagePriceSimulation>();

                    var getDataTemp = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();
                    if (getDataTemp.Count() > 0)
                    {
                        foreach (var data in getDataTemp)
                        {
                            ManagePriceSimulation Manage = new ManagePriceSimulation();
                            Manage.price_sim_adjust_qty = data.price_sim_adjust_qty;
                            Manage.price_sim_amount = data.price_sim_amount;
                            Manage.price_sim_batt_segmentation = data.price_sim_batt_segmentation;
                            Manage.price_sim_batt_weight = data.price_sim_batt_weight;
                            Manage.price_sim_createBy = data.price_sim_createBy;
                            Manage.price_sim_createDate = data.price_sim_createDate;
                            Manage.price_sim_id = labelID;
                            Manage.price_sim_new_JIS = data.price_sim_new_JIS;
                            Manage.price_sim_old_JIS = data.price_sim_old_JIS;
                            Manage.price_sim_PN = data.price_sim_PN;
                            Manage.price_sim_qty_pallet_L = data.price_sim_qty_pallet_L;
                            Manage.price_sim_qty_pallet_S = data.price_sim_qty_pallet_S;
                            Manage.price_sim_request_qty = data.price_sim_request_qty;
                            Manage.price_sim_status = 0;
                            Manage.price_sim_total_batt_weight = data.price_sim_total_batt_weight;
                            Manage.price_sim_total_container = Convert.ToInt32(container);
                            Manage.price_sim_total_pallet_L = data.price_sim_total_pallet_L;
                            Manage.price_sim_total_pallet_S = data.price_sim_total_pallet_S;
                            Manage.price_sim_unit_price = data.price_sim_unit_price;
                            Manage.cust_id = data.cust_id;

                            listManage.Add(Manage);
                        }
                        GSDbContext.ManagePriceSimulation.AddRange(listManage);
                        GSDbContext.SaveChanges();

                        GSDbContext.ManagePriceSimulation_temp.RemoveRange(getDataTemp);
                        GSDbContext.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.Created);
                    }
                    //int lastID = 1;
                    //noIDLastOrder = "OD" + dateNow.ToString("yyMMdd") + lastID.ToString().PadLeft(5, '0');
                    //noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" + lastID.ToString().PadLeft(3, '0');
                }
            }
            //var values = form.Get("values");

            //var newOrder = new ManagePriceSimulation();
            //JsonConvert.PopulateObject(values, newOrder);

            //Validate(newOrder);
            //if (!ModelState.IsValid)
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            //newOrder.price_sim_createDate = DateTime.UtcNow.AddHours(7);
            //newOrder.price_sim_createBy = sessionLogin.fullname;

            //GSDbContext.ManagePriceSimulation.Add(newOrder);
            //GSDbContext.SaveChanges();



            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [System.Web.Http.Route("api/PriceSimulation/paid-price", Name = "PaidPrice")]
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Paid_Header(string priceID)
        {
            if (!String.IsNullOrEmpty(priceID))
            {
                List<ManagePriceSimulation> listManage = new List<ManagePriceSimulation>();

                //var getDataTemp = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();
                var DataPrice = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == priceID).ToList();
                if (DataPrice.Count() > 0)
                {
                    foreach (var data in DataPrice)
                    {
                        data.price_sim_status = 4;
                        data.price_sim_modifDate = DateTime.UtcNow.AddHours(7);
                        data.price_sim_modifBy = sessionLogin.fullname;

                        GSDbContext.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                } else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data "+ priceID + " not found!");
                }
                
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [System.Web.Http.Route("api/PriceSimulation/finish-price", Name = "FinishPrice")]
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Finish_Header(string priceID)
        {
            if (!String.IsNullOrEmpty(priceID))
            {
                List<ManagePriceSimulation> listManage = new List<ManagePriceSimulation>();

                //var getDataTemp = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();
                var DataPrice = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == priceID).ToList();
                if (DataPrice.Count() > 0)
                {
                    foreach (var data in DataPrice)
                    {
                        data.price_sim_status = 3;
                        data.price_sim_modifDate = DateTime.UtcNow.AddHours(7);
                        data.price_sim_modifBy = sessionLogin.fullname;

                        GSDbContext.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                } else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data "+ priceID + " not found!");
                }
                
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [System.Web.Http.Route("api/PriceSimulation/submit-price", Name = "SubmitPrice")]
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Submit_Header(string priceID)
        {
            if (!String.IsNullOrEmpty(priceID))
            {
                List<ManagePriceSimulation> listManage = new List<ManagePriceSimulation>();

                //var getDataTemp = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();
                var DataPrice = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == priceID).ToList();
                if (DataPrice.Count() > 0)
                {
                    foreach (var data in DataPrice)
                    {
                        data.price_sim_status = 1;
                        data.price_sim_modifDate = DateTime.UtcNow.AddHours(7);
                        data.price_sim_modifBy = sessionLogin.fullname;

                        GSDbContext.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                } else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data "+ priceID + " not found!");
                }
                
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [System.Web.Http.Route("api/PriceSimulation/reject-order", Name = "RejectOrder")]
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Put_Header_Reject(string priceID, string info)
        {
            if (!String.IsNullOrEmpty(priceID))
            {
                List<ManagePriceSimulation> listManage = new List<ManagePriceSimulation>();

                //var getDataTemp = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();
                var DataPrice = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == priceID).ToList();
                if (DataPrice.Count() > 0)
                {
                    foreach (var data in DataPrice)
                    {
                        data.price_sim_status = 0;
                        data.price_sim_modifDate = DateTime.UtcNow.AddHours(7);
                        data.price_sim_modifBy = sessionLogin.fullname;
                        data.price_sim_info = info;

                        GSDbContext.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                } else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data "+ priceID + " not found!");
                }
                
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }
        
        [System.Web.Http.Route("api/PriceSimulation/approve-price", Name = "ApprovePrice")]
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Put_Header(string priceID)
        {
            if (!String.IsNullOrEmpty(priceID))
            {
                List<ManagePriceSimulation> listManage = new List<ManagePriceSimulation>();

                //var getDataTemp = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();
                var DataPrice = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == priceID).ToList();
                if (DataPrice.Count() > 0)
                {
                    foreach (var data in DataPrice)
                    {
                        data.price_sim_status = 2;
                        data.price_sim_modifDate = DateTime.UtcNow.AddHours(7);
                        data.price_sim_modifBy = sessionLogin.fullname;

                        GSDbContext.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                } else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data "+ priceID + " not found!");
                }
                
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }


        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var order = GSDbContext.ManagePriceSimulation_temp.First(e => e.recnum_id == key);

            JsonConvert.PopulateObject(values, order);

            Validate(order);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            order.price_sim_modifBy = sessionLogin.fullname;
            order.price_sim_modifDate = DateTime.UtcNow.AddHours(7);


            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        //[SessionCheck]
        //[HttpPut]
        //public HttpResponseMessage Put(FormDataCollection form)
        //{
        //    //var key = Convert.ToInt32(form.Get("key"));
        //    var key = form.Get("key");
        //    var values = form.Get("values");
        //    var order = GSDbContext.ManagePriceSimulation.Where(e => e.price_sim_id == key).ToList;

        //    var before_qtyShipBDG = order.ship_to_BDG;
        //    var before_qtyShipJKT= order.ship_to_JKT;
        //    var before_qtyShipSBY= order.ship_to_SBY;
        //    var before_qtyShipSMG = order.ship_to_SMG;

        //    JsonConvert.PopulateObject(values, order);

        //    var after_qtyShipBDG = order.ship_to_BDG;
        //    var after_qtyShipJKT = order.ship_to_JKT;
        //    var after_qtyShipSBY = order.ship_to_SBY;
        //    var after_qtyShipSMG = order.ship_to_SMG;
        //    order.total = (order.ship_to_JKT + order.ship_to_BDG + order.ship_to_SBY + order.ship_to_SMG);

        //    Validate(order);
        //    if (!ModelState.IsValid)
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

        //    order.update_time = DateTime.UtcNow.AddHours(7);

        //    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
        //    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
        //    manageHistoryTransaction.type_history = "transaction_order";
        //    manageHistoryTransaction.desc_history = "Order edited : " + Environment.NewLine;
        //    manageHistoryTransaction.desc_history += "Before : BDG " + Math.Round(before_qtyShipBDG, 2) + " --> After : BDG " + Math.Round(after_qtyShipBDG, 2) + Environment.NewLine;
        //    manageHistoryTransaction.desc_history += "Before : JKT " + Math.Round(before_qtyShipJKT, 2) + " --> After : JKT " + Math.Round(after_qtyShipJKT, 2) + Environment.NewLine;
        //    manageHistoryTransaction.desc_history += "Before : SBY " + Math.Round(before_qtyShipSBY, 2) + " --> After : SBY " + Math.Round(after_qtyShipSBY, 2) + Environment.NewLine;
        //    manageHistoryTransaction.desc_history += "Before : SMG " + Math.Round(before_qtyShipSMG, 2) + " --> After : SMG " + Math.Round(after_qtyShipSMG, 2) + Environment.NewLine;
        //    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);

        //    GSDbContext.SaveChanges();

        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}


        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form, string price_id = null)
        {
            int statusCode = 500;
            if (!String.IsNullOrEmpty(price_id))
            {
                var dataAll = GSDbContext.ManagePriceSimulation.Where(p => p.price_sim_id == price_id).ToList();
                GSDbContext.ManagePriceSimulation.RemoveRange(dataAll);
                GSDbContext.SaveChanges();
                statusCode = Convert.ToInt32(HttpStatusCode.OK);
            }
            //else
            //{
            //    var key = Convert.ToInt32(form.Get("key"));
            //    var order = GSDbContext.ManageOrder.First(e => e.id_recnum_order == key);

            //    ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
            //    manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
            //    manageHistoryTransaction.type_history = "transaction_order";
            //    manageHistoryTransaction.desc_history = "Order removed : " + order.id_order;
            //    GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);

            //    GSDbContext.ManageOrder.Remove(order);
            //    GSDbContext.SaveChanges();
            //    statusCode = Convert.ToInt32(HttpStatusCode.OK);
            //}             
            return Request.CreateResponse(statusCode);
        }

        public void SchedulerLogPrice()
        {

            var dateNow = DateTime.UtcNow.AddHours(7);
            //dateNow.AddMonths(-3);
            var bln = dateNow.ToString("MM");
            int bul = Convert.ToInt32(bln);
            decimal avg_lme = 0, avg_lmePrev = 0;
            decimal new_price = 0;

            string sSQLSelectLogPrice = "select p.cust_id, p.part_number, p.PN_old_jis, p.PN_new_jis, p.PN_batt_segmentation, c.customer_periodic_price as periodic_price from tlkp_partnumber p left join tlkp_cust c on p.cust_id = c.customer_id";
            var DataPNStoretoLogPrice = GSDbContext.Database.SqlQuery<GetLogPriceData>(sSQLSelectLogPrice).ToList();
            //var DataPNStoretoLogPrice = GSDbContext.MasterPartNumber.ToList();
            foreach (var data in DataPNStoretoLogPrice)
            {
                var PN = data.part_number;
                if (data.periodic_price.Equals("Monthly") || data.periodic_price == "Monthly")
                {
                    DateTime dd = new DateTime();
                    var CekPNinLogPrice = GSDbContext.ManageLogPrice.Where(e => e.log_part_number == PN && e.log_periode_int == bul).SingleOrDefault();
                    if (CekPNinLogPrice.log_unit_price != null)
                    {
                        if (dd.Day > 21)
                        {

                            DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 20);
                            DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, 21);

                            string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-3).ToString("yyyy-MM-dd") + "' and '" + dateAk.AddMonths(-2).ToString("yyyy-MM-dd") + "'";
                            string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-4).ToString("yyyy-MM-dd") + "' and '" + dateAk.AddMonths(-3).ToString("yyyy-MM-dd") + "'";

                            var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                            var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                            if (checkAverageLME != null && checkAverageLMEPrev != null)
                            {
                                avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                                avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                                decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                                decimal new_price_fluct = (decimal)(LME_fluct * 5 / 10) + 1;

                                //string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                                string sSQLSelectPrice = "select * from t_log_history_price where log_periode_int = " + (bul - 1) + " and log_part_number = '" + PN + "' order by log_createDate desc";
                                var sPrevPrice = GSDbContext.Database.SqlQuery<ManageLogPrice>(sSQLSelectPrice).SingleOrDefault();
                                //var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                                if (sPrevPrice != null)
                                {
                                    //var prev_price = sPrevPrice.prev_price;
                                    var prev_price = sPrevPrice.log_unit_price;
                                    new_price = (decimal)prev_price * new_price_fluct;


                                    var newOrder = new ManageLogPrice();

                                    //Validate(newOrder);
                                    //if (!ModelState.IsValid)
                                    //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());
                                    newOrder.log_part_number = data.part_number;
                                    newOrder.log_PN_old_jis = data.PN_old_jis;
                                    newOrder.log_PN_new_jis = data.PN_new_jis;
                                    newOrder.log_PN_batt_segmentation = data.PN_batt_segmentation;
                                    newOrder.log_periode = dateNow.ToString("MMM");
                                    newOrder.log_periode_int = bul;
                                    newOrder.log_unit_price = new_price;
                                    newOrder.log_createDate = DateTime.UtcNow.AddHours(7);
                                    newOrder.log_createBy = "scheduler";

                                    GSDbContext.ManageLogPrice.Add(newOrder);
                                    GSDbContext.SaveChanges();
                                }

                            }
                        }
                    }
                }
                else if (data.periodic_price.Equals("3 months") || data.periodic_price == "3 months")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-2);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-3);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    //string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where [lme_month] between " + blnAw + " and " + blnAk + "";
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-4) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        //avg_lme = (decimal)checkAverageLME.avg_lme;
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 5 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        var prev_price = sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (data.periodic_price.Equals("6 months") || data.periodic_price == "6 months")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-4);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-5);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-10) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-11) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 5 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        var prev_price = sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
                else if (data.periodic_price.Equals("Monthly (AOP Cust)") || data.periodic_price == "Monthly (AOP Cust)")
                {
                    DateTime dateAw = new DateTime(dateNow.Year, dateNow.Month, 1);
                    DateTime dateAk = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month));
                    var endDate = dateAk.AddMonths(-3);
                    if (endDate.Day != DateTime.DaysInMonth(endDate.Year, endDate.Month))
                    {
                        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    }
                    var endDatePrev = dateAk.AddMonths(-4);
                    if (endDatePrev.Day != DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month))
                    {
                        endDatePrev = new DateTime(endDatePrev.Year, endDatePrev.Month, DateTime.DaysInMonth(endDatePrev.Year, endDatePrev.Month));
                    }
                    string sSQLSelect = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-5) + "' and '" + endDate + "'";
                    string sSQLSelectPrev = "select AVG(lme_value) as avg_lme from [tlkp_lme] where convert(date,(lme_year+'-'+lme_month+'-'+lme_date)) between '" + dateAw.AddMonths(-6) + "' and '" + endDatePrev + "'";

                    var checkAverageLME = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelect).SingleOrDefault();
                    var checkAverageLMEPrev = GSDbContext.Database.SqlQuery<GetAVGLME>(sSQLSelectPrev).SingleOrDefault();
                    if (checkAverageLME != null && checkAverageLMEPrev != null)
                    {
                        avg_lme = Math.Round((decimal)checkAverageLME.avg_lme, 4);
                        avg_lmePrev = Math.Round((decimal)checkAverageLMEPrev.avg_lme, 4);

                        decimal LME_fluct = Math.Round((avg_lme - avg_lmePrev) / avg_lmePrev, 4);
                        decimal new_price_fluct = (decimal)(LME_fluct * 6 / 10) + 1;

                        string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + PN + "' order by log_createDate desc";
                        var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                        var prev_price = sPrevPrice.prev_price;
                        new_price = (decimal)prev_price * new_price_fluct;
                    }
                }
            }


            new_price = Math.Round(new_price, 3);
            var unitPrice = new ManageLogPrice();

            //Validate(newOrder);
            //if (!ModelState.IsValid)
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            unitPrice.log_unit_price = new_price;
            unitPrice.log_createDate = DateTime.UtcNow.AddHours(7);
            unitPrice.log_createBy = sessionLogin.fullname;

            GSDbContext.ManageLogPrice.Add(unitPrice);
            GSDbContext.SaveChanges();

            //return Request.CreateResponse(HttpStatusCode.Created);
        }
    }
}