using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class DashboardGetBarForecastWithParamController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }
        public DashboardGetBarForecastWithParamController()
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

            var queryWhereTahun = "";
            var queryWhereBulan = "";

            var queryWhereBatteryType = "";
            var queryWhereMaterialtype = "";
            var queryWhereBrand = "";
            var queryWhereGroupPO = "";
            var queryWhereGroupFC = "";


            if (tahun != null && tahun != "" && tahun != "undefined")
            {
                queryWhereTahun = " WHERE " +
                   " DATEPART(YEAR, year_forecast) = " + tahun;
                   //queryWhereTahun = tahun;
            }
            else
            {
                //queryWhereTahun = "DATEPART(YEAR, GETDATE())";
                queryWhereTahun = "";
            }

            if (bulan != null && bulan != "" && bulan != "undefined")
            {
                queryWhereBulan = " AND DATEPART(MONTH, date_forecast) = " + bulan;
            }

            // DATALIST VERTICAL ROW

            if (battery_type != null)
                queryWhereBatteryType = " AND type_battery = '" + battery_type + "' ";

            if (material_type != null)
                queryWhereMaterialtype = " AND type_material = '" + material_type + "' ";

            if (brandgroup != null)
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

            //var sQLBoardForecast = "SELECT year_forecast, month_forecast, date_forecast, ISNULL(CAST(SUM(yearly_plan) AS DECIMAL) , 0)  AS yearly_plan, " +
            //    " ISNULL(CAST(SUM(n4) AS DECIMAL), 0) AS n4, ISNULL(CAST(SUM(n3) AS DECIMAL), 0) AS n3, " +
            //    " ISNULL(CAST(SUM(n2) AS DECIMAL), 0) AS n2, " +
            //    " (SELECT ISNULL(CAST(sum(total) AS DECIMAL), 0) AS order_qty FROM manage_order b WHERE b.year_order = year_forecast and b.month_order = month_forecast and b.status_order = 3  ) AS order_qty " +
            //    " FROM manage_forecast " +
            //    " WHERE DATEPART(YEAR, year_forecast) = " +
            //    queryWhereTahun +
            //    queryWhereBulan +
            //    queryWhereBatteryType +
            //    queryWhereMaterialtype +
            //    queryWhereBrand +
            //    queryWhereGroupFC + 
            //    " GROUP BY year_forecast, month_forecast, date_forecast " +
            //    " ORDER BY date_forecast ASC";

            var sQLBoardForecast = "SELECT a.year_forecast, a.month_forecast, a.date_forecast," +
                " ISNULL(CAST(SUM(a.yearly_plan) AS DECIMAL), 0)  AS yearly_plan," +
                " ISNULL(CAST(SUM(a.n4) AS DECIMAL), 0) AS n4," +
                " ISNULL(CAST(SUM(a.n3) AS DECIMAL), 0) AS n3," +
                " ISNULL(CAST(SUM(a.n2) AS DECIMAL), 0) AS n2," +
                " (SELECT ISNULL(CAST(sum(total) AS DECIMAL), 0) AS order_qty FROM manage_order b WHERE b.year_order = year_forecast and b.month_order = month_forecast and b.status_order = 3  ) AS order_qty," +
                " (SELECT ISNULL(CAST(SUM(n2) AS DECIMAL), 0) FROM manage_forecast" +
                " where date_forecast = EOMONTH(DATEADD(MONTH, -2, a.date_forecast))) AS n2_FIX," +
                " (SELECT ISNULL(CAST(SUM(n3) AS DECIMAL), 0) FROM manage_forecast" +
                " where date_forecast = EOMONTH(DATEADD(MONTH, -3, a.date_forecast))) AS n3_FIX," +
                " (SELECT ISNULL(CAST(SUM(n4) AS DECIMAL), 0) FROM manage_forecast" +
                " where date_forecast = EOMONTH(DATEADD(MONTH, -4, a.date_forecast))) AS n4_FIX," +
                " (SELECT DISTINCT(date_forecast) FROM manage_forecast" +
                " where date_forecast = EOMONTH(DATEADD(MONTH, -2, a.date_forecast))) AS date_n2_FIX," +
                " (SELECT DISTINCT(date_forecast) FROM manage_forecast" +
                " where date_forecast = EOMONTH(DATEADD(MONTH, -3, a.date_forecast))) AS date_n3_FIX," +
                " (SELECT DISTINCT(date_forecast) FROM manage_forecast" +
                " where date_forecast = EOMONTH(DATEADD(MONTH, -4, a.date_forecast))) AS date_n4_FIX" +
                " FROM manage_forecast  a" +
                queryWhereTahun +
                queryWhereBulan +
                queryWhereBatteryType +
                queryWhereMaterialtype +
                queryWhereBrand +
                queryWhereGroupFC +
                " GROUP BY a.year_forecast, a.month_forecast, a.date_forecast" +
                " ORDER BY a.date_forecast ASC ";

            var dataChartForecast = GSDbContext.Database.SqlQuery<DashboardBarForecastModel>(sQLBoardForecast).ToList();

           loadResult = DataSourceLoader.Load(dataChartForecast, loadOptions);

            return Request.CreateResponse(loadResult);
        }

    }
}