using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data;

namespace Template_DevExpress_By_MFM.Controllers {
    public class DashboardGetBarWithParamController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }
        public DashboardGetBarWithParamController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
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
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string tahun, string bulan, string battery_type, string material_type, string brandgroup)
        {
            LoadResult loadResult = new LoadResult();
  
            var queryWhereTahunForecast = "";            
            var queryWhereTahunOrder = "";            
            var queryWhereBulanForecast = "";            
            var queryWhereBulanOrder = "";            
            var queryWhereBatteryType = "";            
            var queryWhereMaterialtype = "";            
            var queryWhereBrand = "";            
            var queryWhereGroupPO = "";            
            var queryWhereGroupFC = "";            


            if (tahun != null)
            {
                queryWhereTahunForecast = " year_forecast = '" + tahun + "' ";
                queryWhereTahunOrder = " year_order = '" + tahun + "' ";
            }
            else
            {
                tahun = DateTime.UtcNow.AddHours(7).ToString("yyyy");
                queryWhereTahunForecast = " year_forecast = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy") + "'";
                queryWhereTahunOrder = " year_order = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy") + "'";
            }

            if (bulan != null)
            {
                queryWhereBulanForecast = " AND  DATEPART(MONTH, date_forecast) = '" + bulan + "' ";
                queryWhereBulanOrder = " AND  DATEPART(MONTH, order_date) = '" + bulan + "' ";
            }
            else
            {
                queryWhereBulanForecast = " ";
            }

            if (battery_type != null)
                queryWhereBatteryType = " AND type_battery = '"+ battery_type + "' ";

            if (material_type != null)
                queryWhereMaterialtype = " AND type_material = '" + material_type + "' ";

            if(brandgroup != null)
            {
                var splitBrandGroup = brandgroup.Split('|');
                if (!string.IsNullOrEmpty(splitBrandGroup[0].ToString()))
                    queryWhereBrand = " AND brand = '" + splitBrandGroup[0].ToString() + "' ";

                if (!string.IsNullOrEmpty(splitBrandGroup[1].ToString()))
                {
                    queryWhereGroupFC = " AND group_forecast = '" + splitBrandGroup[1].ToString() + "' ";
                    queryWhereGroupPO = " AND group_order = '" + splitBrandGroup[1].ToString() + "' ";
                }
            }            

            // DATALIST VERTICAL ROW

            //var sQLBoardForecast = "SELECT 'Forecast' as transaksi, year_forecast AS tahun, month_forecast AS bulan, CAST(DATEPART(MONTH,date_forecast) AS INT) AS urutanbulan, ISNULL(CAST(SUM(n4+n3+n2) AS DECIMAL),0) AS total FROM manage_forecast " +
            var sQLBoardForecast = "SELECT 'Forecast' as transaksi, a.year_forecast AS tahun, a.month_forecast AS bulan, CAST(DATEPART(MONTH, a.date_forecast) AS INT) AS urutanbulan, " +
                "  (SELECT ISNULL(CAST(SUM(n2) AS DECIMAL),0) FROM manage_forecast WHERE date_forecast = EOMONTH(DATEADD(MONTH, -2, a.date_forecast))  ) AS total  " +
                " FROM manage_forecast a " +
                " WHERE " + queryWhereTahunForecast + " " + queryWhereBulanForecast + queryWhereBatteryType + queryWhereMaterialtype + queryWhereBrand + queryWhereGroupFC +
                " GROUP BY a.year_forecast, a.month_forecast, a.date_forecast";
            var dataChartForecast = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardForecast).ToList();

            var sQLBoardPOOriginal = "SELECT 'PO Original' as transaksi, year_order AS tahun, month_order as bulan, CAST(DATEPART(MONTH,order_date) AS INT) AS urutanbulan, ISNULL(CAST(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS DECIMAL),0) AS total FROM manage_order " +
                 " WHERE " + queryWhereTahunOrder + " " + queryWhereBulanOrder + " AND status_order = 0 " + queryWhereBatteryType + queryWhereMaterialtype + queryWhereBrand + queryWhereGroupPO +
                " GROUP BY year_order, month_order, order_date";
            var dataChartPOOriginal = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardPOOriginal).ToList();

            var sQLBoardPOAgreed = "SELECT 'PO Agreed' as transaksi, year_order AS tahun, month_order as bulan, CAST(DATEPART(MONTH,order_date) AS INT) AS urutanbulan, ISNULL(CAST(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS DECIMAL),0) AS total FROM manage_order " +
                " WHERE " + queryWhereTahunOrder + " " + queryWhereBulanOrder + " " + " AND status_order = 3 " + queryWhereBatteryType + queryWhereMaterialtype + queryWhereBrand + queryWhereGroupPO +
                " GROUP BY year_order, month_order, order_date";
            var dataChartPOAgreed = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardPOAgreed).ToList();

            var sQLBoardPOAchievementDelivery = "SELECT 'Ach. Delivery' as transaksi, year_order AS tahun, month_order as bulan, CAST(DATEPART(MONTH,order_date) AS INT) AS urutanbulan, ISNULL(CAST(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG) AS DECIMAL),0) AS total FROM manage_order " +
                " WHERE " + queryWhereTahunOrder + " " + queryWhereBulanOrder + " AND status_order = 4 " + queryWhereBatteryType + queryWhereMaterialtype + queryWhereBrand + queryWhereGroupPO +
                " GROUP BY year_order, month_order, order_date";
            var dataChartPOAchievementDelivery = GSDbContext.Database.SqlQuery<DashboardChartModel>(sQLBoardPOAchievementDelivery).ToList();


            // DATALIST HORIZONTAL ROW
            List<DashboardChartModel> listTempDashboardYearlyplan = new List<DashboardChartModel>();
            DashboardChartModel datamodelTempChart = new DashboardChartModel();
            var sQLBoardYearlyPlan = "SELECT 'Yearly Plan' as transaksi, CAST(tahun AS VARCHAR) as tahun, ISNULL(CAST(sum(qty_1) AS DECIMAL),0) AS januari, " +
                " ISNULL(CAST(sum(qty_2) AS DECIMAL),0) AS februari, ISNULL(CAST(sum(qty_3) AS DECIMAL),0) AS maret, ISNULL(CAST(sum(qty_4) AS DECIMAL),0) AS april, ISNULL(CAST(sum(qty_5) AS DECIMAL),0) AS mei, ISNULL(CAST(sum(qty_6) AS DECIMAL),0) AS juni, ISNULL(CAST(sum(qty_7) AS DECIMAL),0) AS juli, " +
                " ISNULL(CAST(sum(qty_8) AS DECIMAL),0) AS agustus, ISNULL(CAST(sum(qty_9) AS DECIMAL),0) AS september, ISNULL(CAST(sum(qty_10) AS DECIMAL),0) AS oktober, ISNULL(CAST(sum(qty_11) AS DECIMAL),0) AS november, ISNULL(CAST(sum(qty_12) AS DECIMAL),0) AS desember " +
                " FROM manage_yearly_plan " +
                " WHERE tahun = '" + tahun + "' GROUP BY tahun";
            var dataChartYearlyPlan = GSDbContext.Database.SqlQuery<DashboardTempChartYBModel>(sQLBoardYearlyPlan).SingleOrDefault();

            if (dataChartYearlyPlan != null)
            {
                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 1;
                datamodelTempChart.bulan = "Januari";
                datamodelTempChart.total = dataChartYearlyPlan.januari;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 2;
                datamodelTempChart.bulan = "Februari";
                datamodelTempChart.total = dataChartYearlyPlan.februari;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 3;
                datamodelTempChart.bulan = "Maret";
                datamodelTempChart.total = dataChartYearlyPlan.maret;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 4;
                datamodelTempChart.bulan = "April";
                datamodelTempChart.total = dataChartYearlyPlan.april;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 5;
                datamodelTempChart.bulan = "Mei";
                datamodelTempChart.total = dataChartYearlyPlan.mei;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 6;
                datamodelTempChart.bulan = "Juni";
                datamodelTempChart.total = dataChartYearlyPlan.juni;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 7;
                datamodelTempChart.bulan = "Juli";
                datamodelTempChart.total = dataChartYearlyPlan.juli;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 8;
                datamodelTempChart.bulan = "Agustus";
                datamodelTempChart.total = dataChartYearlyPlan.agustus;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 9;
                datamodelTempChart.bulan = "September";
                datamodelTempChart.total = dataChartYearlyPlan.september;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 10;
                datamodelTempChart.bulan = "Oktober";
                datamodelTempChart.total = dataChartYearlyPlan.oktober;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 11;
                datamodelTempChart.bulan = "November";
                datamodelTempChart.total = dataChartYearlyPlan.november;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();

                datamodelTempChart.transaksi = dataChartYearlyPlan.transaksi;
                datamodelTempChart.tahun = dataChartYearlyPlan.tahun;
                datamodelTempChart.urutanbulan = 12;
                datamodelTempChart.bulan = "Desember";
                datamodelTempChart.total = dataChartYearlyPlan.desember;
                listTempDashboardYearlyplan.Add(datamodelTempChart);
                datamodelTempChart = new DashboardChartModel();
            }


            List<DashboardChartModel> listTempDashboardBusinessPlan = new List<DashboardChartModel>();
            DashboardChartModel datamodelTempBusinessPlanChart = new DashboardChartModel();
            var sQLBoardBusinessPlan = "SELECT 'Business Plan' as transaksi, CAST(tahun AS VARCHAR) as tahun, ISNULL(CAST(sum(qty_1) AS DECIMAL),0) AS januari, " +
                " ISNULL(CAST(sum(qty_2) AS DECIMAL),0) AS februari, ISNULL(CAST(sum(qty_3) AS DECIMAL),0) AS maret, ISNULL(CAST(sum(qty_4) AS DECIMAL),0) AS april, ISNULL(CAST(sum(qty_5) AS DECIMAL),0) AS mei, ISNULL(CAST(sum(qty_6) AS DECIMAL),0) AS juni, " +
                " ISNULL(CAST(sum(qty_7) AS DECIMAL),0) AS juli, ISNULL(CAST(sum(qty_8) AS DECIMAL),0) AS agustus, ISNULL(CAST(sum(qty_9) AS DECIMAL),0) AS september, ISNULL(CAST(sum(qty_10) AS DECIMAL),0) AS oktober, ISNULL(CAST(sum(qty_11) AS DECIMAL),0) AS november, ISNULL(CAST(sum(qty_12) AS DECIMAL),0) AS desember " +
                " FROM manage_business_plan " +
                " WHERE tahun = '" + tahun + "' GROUP BY tahun";
            var dataChartBusinessPlan = GSDbContext.Database.SqlQuery<DashboardTempChartYBModel>(sQLBoardBusinessPlan).SingleOrDefault();

            if (dataChartBusinessPlan != null)
            {
                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 1;
                datamodelTempBusinessPlanChart.bulan = "Januari";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.januari;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 2;
                datamodelTempBusinessPlanChart.bulan = "Februari";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.februari;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 3;
                datamodelTempBusinessPlanChart.bulan = "Maret";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.maret;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 4;
                datamodelTempBusinessPlanChart.bulan = "April";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.april;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 5;
                datamodelTempBusinessPlanChart.bulan = "Mei";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.mei;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 6;
                datamodelTempBusinessPlanChart.bulan = "Juni";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.juni;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 7;
                datamodelTempBusinessPlanChart.bulan = "Juli";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.juli;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 8;
                datamodelTempBusinessPlanChart.bulan = "Agustus";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.agustus;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 9;
                datamodelTempBusinessPlanChart.bulan = "September";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.september;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 10;
                datamodelTempBusinessPlanChart.bulan = "Oktober";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.oktober;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 11;
                datamodelTempBusinessPlanChart.bulan = "November";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.november;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();

                datamodelTempBusinessPlanChart.transaksi = dataChartBusinessPlan.transaksi;
                datamodelTempBusinessPlanChart.tahun = dataChartBusinessPlan.tahun;
                datamodelTempChart.urutanbulan = 12;
                datamodelTempBusinessPlanChart.bulan = "Desember";
                datamodelTempBusinessPlanChart.total = dataChartBusinessPlan.desember;
                listTempDashboardBusinessPlan.Add(datamodelTempBusinessPlanChart);
                datamodelTempBusinessPlanChart = new DashboardChartModel();
            }


            List<DashboardChartModel> dashboardChartModels = new List<DashboardChartModel>();

            dashboardChartModels.AddRange(dataChartForecast);
            dashboardChartModels.AddRange(dataChartPOOriginal);
            dashboardChartModels.AddRange(dataChartPOAgreed);
            dashboardChartModels.AddRange(dataChartPOAchievementDelivery);
            dashboardChartModels.AddRange(listTempDashboardYearlyplan);
            dashboardChartModels.AddRange(listTempDashboardBusinessPlan);

            loadResult = DataSourceLoader.Load(dashboardChartModels.OrderBy(p => p.urutanbulan), loadOptions);

            return Request.CreateResponse(loadResult);
        }

    }
}