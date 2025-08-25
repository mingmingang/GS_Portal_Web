using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Template_DevExpress_By_MFM.Controllers {
    public class HomeController : Controller {

        public GSDbContext GSDbContext { get; set; }
        public HomeController()
        {

            GSDbContext = new GSDbContext(".", "db_marketing_portal", "sa", "aangaang");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }
        string FormatNumber<T>(T number, int maxDecimals = 4)
        {
            return Regex.Replace(String.Format("{0:n" + maxDecimals + "}", number),
                                 @"[" + System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "]?0+$", "");
        }

        [SessionCheck]
        public ActionResult Index() {           

            //string tempMobile = string.Empty;
            //string tempProduct = string.Empty;
            //_ICharts.ProductWiseSales(out tempMobile, out tempProduct);
            //ViewBag.MobileCount_List = tempMobile.Trim();
            //ViewBag.Productname_List = tempProduct.Trim();


            // QUERY DASHBOARD GET BUSINESS PLAN IN YEAR
            string sQBoardBusinessPlan = "SELECT TOP 1 CAST(tahun AS VARCHAR) as tahun, ISNULL(sum(CAST(qty_total AS DECIMAL)),0) AS total FROM manage_business_plan " +
                " WHERE tahun = DATEPART(YEAR, GETDATE()) " +
                " GROUP BY tahun ORDER BY tahun DESC";
            var dataBoardBusinessPlan = GSDbContext.Database.SqlQuery<DashboardCardModel>(sQBoardBusinessPlan).SingleOrDefault();

            // QUERY DASHBOARD GET YEARLY PLAN IN YEAR
            string sQBoardYearlyPlan = "SELECT TOP 1 CAST(tahun AS VARCHAR) as tahun, ISNULL(sum(CAST(qty_total AS DECIMAL)),0) AS total FROM manage_yearly_plan" +
                " WHERE tahun = DATEPART(YEAR, GETDATE()) " +
                " GROUP BY tahun";
            var dataBoardYearlyPlan = GSDbContext.Database.SqlQuery<DashboardCardModel>(sQBoardYearlyPlan).SingleOrDefault();

            // QUERY DASHBOARD GET FORECAST IN YEAR
            string sQBoardForecast = "SELECT ISNULL(CAST(SUM(n2 + n3 + n4) AS DECIMAL), 0) AS total, CAST(year_forecast AS VARCHAR) as tahun FROM manage_forecast" +
                " WHERE year_forecast = DATEPART(YEAR, GETDATE()) " +
                " GROUP BY year_forecast";
            var dataBoardForecast = GSDbContext.Database.SqlQuery<DashboardCardModel>(sQBoardForecast).SingleOrDefault();

            // QUERY DASHBOARD GET PO ORIGINAL IN YEAR
            //string sQBoardPOOriginal = "SELECT SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS total, CAST(year_order AS VARCHAR) as tahun FROM manage_order " +
            //    " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 0 " +
            //    " GROUP BY year_order";
            string sQBoardPOOriginal = "SELECT ISNULL(CAST(SUM(total) AS DECIMAL),0) as total, CAST(year_order AS VARCHAR) as tahun FROM manage_order " +
                " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 0 " +
                " GROUP BY year_order";
            var dataBoardPOOriginal = GSDbContext.Database.SqlQuery<DashboardCardModel>(sQBoardPOOriginal).SingleOrDefault();

            // QUERY DASHBOARD GET PO AGREED IN YEAR
            //string sQBoardPOAgreed = "SELECT SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS total, CAST(year_order AS VARCHAR) as tahun FROM manage_order " +
            //    " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 3 " +
            //    " GROUP BY year_order";
            string sQBoardPOAgreed = "SELECT ISNULL(CAST(SUM(total) AS DECIMAL),0) as total, CAST(year_order AS VARCHAR) as tahun FROM manage_order " +
                " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 3 " +
                " GROUP BY year_order";
            var dataBoardPOAgreed = GSDbContext.Database.SqlQuery<DashboardCardModel>(sQBoardPOAgreed).SingleOrDefault();

            // QUERY DASHBOARD GET ACTUAL DELIVERY IN YEAR
            //string sQBoardPODelivery = "SELECT SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS total_order, year_order FROM manage_order " +
            //    " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 3-- status PO Agreed " +
            //    " GROUP BY year_order";
            //var dataBoardPODelivery = GSDbContext.Database.SqlQuery<TempManageForecast>(sQBoardPODelivery).ToList();

            ViewBag.dataBoardBusinessPlan = dataBoardBusinessPlan == null ? "0" : FormatNumber(dataBoardBusinessPlan.total);
            ViewBag.dataBoardYearlyPlan = dataBoardYearlyPlan == null ? "0" : FormatNumber(dataBoardYearlyPlan.total);
            ViewBag.dataBoardForecast = dataBoardForecast == null ? "0" : FormatNumber(dataBoardForecast.total);
            ViewBag.dataBoardPOOriginal = dataBoardPOOriginal == null ? "0" : FormatNumber(dataBoardPOOriginal.total);
            ViewBag.dataBoardPOAgreed = dataBoardPOAgreed == null ? "0" : FormatNumber(dataBoardPOAgreed.total);


            // DATALIST VERTICAL ROW

            //var sQLBoardForecast = "SELECT 'Forecast' as transaksi, year_forecast AS tahun, month_forecast AS bulan, CAST(DATEPART(MONTH,date_forecast) AS INT) AS urutanbulan, SUM(n4+n3+n2) AS total FROM manage_forecast " +
            //    " WHERE year_forecast = DATEPART(YEAR, GETDATE()) " +
            //    " GROUP BY year_forecast, month_forecast, date_forecast";
            //var dataChartForecast = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardForecast).ToList();
                      
            //var sQLBoardPOOriginal = "SELECT 'PO Original' as transaksi, year_order AS tahun, month_order as bulan, CAST(DATEPART(MONTH,order_date) AS INT) AS urutanbulan, SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS total FROM manage_order " +
            //    " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 0 " +
            //    " GROUP BY year_order, month_order, order_date";
            //var dataChartPOOriginal = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardPOOriginal).ToList();

            //var sQLBoardPOAgreed = "SELECT 'PO Agreed' as transaksi, year_order AS tahun, month_order as bulan, CAST(DATEPART(MONTH,order_date) AS INT) AS urutanbulan, SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS total FROM manage_order " +
            //    " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 3 " +
            //    " GROUP BY year_order, month_order, order_date";
            //var dataChartPOAgreed = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardPOAgreed).ToList();

            //var sQLBoardPOAchievementDelivery = "SELECT 'Ach. Delivery' as transaksi, year_order AS tahun, month_order as bulan, CAST(DATEPART(MONTH,order_date) AS INT) AS urutanbulan, SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS total FROM manage_order " +
            //    " WHERE year_order = DATEPART(YEAR, GETDATE()) AND status_order = 4 " +
            //    " GROUP BY year_order, month_order, order_date";
            //var dataChartPOAchievementDelivery = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardPOAchievementDelivery).ToList();


            //// DATALIST HORIZONTAL ROW
            //List<DashboardChartModel> listTempDashboardYearlyplan = new List<DashboardChartModel>();
            //DashboardChartModel datamodelTempChart = new DashboardChartModel();
            //var sQLBoardYearlyPlan = "SELECT 'Yearly Plan' as transaksi, CAST(tahun AS VARCHAR) as tahun, sum(qty_1) AS januari, " +
            //    " sum(qty_2) AS februari, sum(qty_3) AS maret, sum(qty_4) AS april, sum(qty_5) AS mei, sum(qty_6) AS juni, sum(qty_7) AS juli, " +
            //    " sum(qty_8) AS agustus, sum(qty_9) AS september, sum(qty_10) AS oktober, sum(qty_11) AS november, sum(qty_12) AS desember " +
            //    " FROM manage_yearly_plan " +
            //    " WHERE tahun = DATEPART(YEAR, GETDATE()) GROUP BY tahun";
            //var dataChartYearlyPlan = GSDbContext.Database.SqlQuery<DashboardTempChartYBModel>(sQLBoardYearlyPlan).SingleOrDefault();
            
            //if(dataChartYearlyPlan != null)
            //{
            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 1;
            //    datamodelTempChart.bulan = "Januari";
            //    datamodelTempChart.total = dataChartYearlyPlan.januari;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 2;
            //    datamodelTempChart.bulan = "Februari";
            //    datamodelTempChart.total = dataChartYearlyPlan.februari;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 3;
            //    datamodelTempChart.bulan = "Maret";
            //    datamodelTempChart.total = dataChartYearlyPlan.maret;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 4;
            //    datamodelTempChart.bulan = "April";
            //    datamodelTempChart.total = dataChartYearlyPlan.april;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 5;
            //    datamodelTempChart.bulan = "Mei";
            //    datamodelTempChart.total = dataChartYearlyPlan.mei;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 6;
            //    datamodelTempChart.bulan = "Juni";
            //    datamodelTempChart.total = dataChartYearlyPlan.juni;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 7;
            //    datamodelTempChart.bulan = "Juli";
            //    datamodelTempChart.total = dataChartYearlyPlan.juli;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 8;
            //    datamodelTempChart.bulan = "Agustus";
            //    datamodelTempChart.total = dataChartYearlyPlan.agustus;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 9;
            //    datamodelTempChart.bulan = "September";
            //    datamodelTempChart.total = dataChartYearlyPlan.september;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 10;
            //    datamodelTempChart.bulan = "Oktober";
            //    datamodelTempChart.total = dataChartYearlyPlan.oktober;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 11;
            //    datamodelTempChart.bulan = "November";
            //    datamodelTempChart.total = dataChartYearlyPlan.november;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();

            //    datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
            //    datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
            //    datamodelTempChart.urutanbulan = 12;
            //    datamodelTempChart.bulan = "Desember";
            //    datamodelTempChart.total = dataChartYearlyPlan.desember;
            //    listTempDashboardYearlyplan.Add(datamodelTempChart);
            //    datamodelTempChart = new DashboardChartModel();
            //}


            //List<DashboardChartModel> listTempDashboardBusinessPlan = new List<DashboardChartModel>();
            //DashboardChartModel datamodelTempBusinessPlanChart = new DashboardChartModel();
            //var sQLBoardBusinessPlan = "SELECT 'Business Plan' as transaksi, CAST(tahun AS VARCHAR) as tahun, sum(qty_1) AS januari, " +
            //    " sum(qty_2) AS februari, sum(qty_3) AS maret, sum(qty_4) AS april, sum(qty_5) AS mei, sum(qty_6) AS juni, " +
            //    " sum(qty_7) AS juli, sum(qty_8) AS agustus, sum(qty_9) AS september, sum(qty_10) AS oktober, sum(qty_11) AS november, sum(qty_12) AS desember " +
            //    " FROM manage_business_plan " +
            //    " WHERE tahun = DATEPART(YEAR, GETDATE()) GROUP BY tahun";
            //var dataChartBusinessPlan = GSDbContext.Database.SqlQuery<DashboardTempChartYBModel>(sQLBoardBusinessPlan).SingleOrDefault();

            //if (dataChartBusinessPlan != null)
            //{
            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 1;
            //    datamodelTempBusinessPlanChart.bulan = "Januari";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.januari;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 2;
            //    datamodelTempBusinessPlanChart.bulan = "Februari";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.februari;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 3;
            //    datamodelTempBusinessPlanChart.bulan = "Maret";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.maret;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 4;
            //    datamodelTempBusinessPlanChart.bulan = "April";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.april;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 5;
            //    datamodelTempBusinessPlanChart.bulan = "Mei";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.mei;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 6;
            //    datamodelTempBusinessPlanChart.bulan = "Juni";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.juni;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 7;
            //    datamodelTempBusinessPlanChart.bulan = "Juli";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.juli;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 8;
            //    datamodelTempBusinessPlanChart.bulan = "Agustus";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.agustus;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 9;
            //    datamodelTempBusinessPlanChart.bulan = "September";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.september;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 10;
            //    datamodelTempBusinessPlanChart.bulan = "Oktober";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.oktober;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 11;
            //    datamodelTempBusinessPlanChart.bulan = "November";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.november;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();

            //    datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
            //    datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
            //    datamodelTempChart.urutanbulan = 12;
            //    datamodelTempBusinessPlanChart.bulan = "Desember";
            //    datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.desember;
            //    listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
            //    datamodelTempBusinessPlanChart = new DashboardChartModel();
            //}


            //List<DashboardChartModel> dashboardChartModels = new List<DashboardChartModel>();

            //dashboardChartModels.AddRange(dataChartForecast);
            //dashboardChartModels.AddRange(dataChartPOOriginal);
            //dashboardChartModels.AddRange(dataChartPOAgreed);
            //dashboardChartModels.AddRange(dataChartPOAchievementDelivery);
            //dashboardChartModels.AddRange(listTempDashboardYearlyplan);
            //dashboardChartModels.AddRange(listTempDashboardBusinessPlan);


            //return View(SampleData.OilProductionData);
            //return View(dashboardChartModels.OrderBy(p => p.urutanbulan));


            return View();
        }
    }
}