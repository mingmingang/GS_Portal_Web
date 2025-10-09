using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ForecastController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }

        public ForecastController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string dateFrom, string dateTo)
        {
            LoadResult loadResult = new LoadResult();

            if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
            {
                string sSQLSelect = "";

                DateTime dtGet = DateTime.ParseExact(dateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                int noOfDaysInMonth = -1;
                noOfDaysInMonth = DateTime.DaysInMonth(Convert.ToInt32(dtGet.Year), Convert.ToInt32(dtGet.Month));

                //sSQLSelect += "select a.id_forecast, (select top 1 id_recnum_forecast " +
                //    " from manage_forecast where id_forecast = a.id_forecast " +
                //    " order by update_time desc) as id_recnum_forecast " +
                //    " from manage_forecast a where a.insert_time BETWEEN '" + dateFrom + "' AND '" + dateTo + "' group by a.id_forecast order by a.id_forecast desc";

                sSQLSelect += "select a.id_forecast, a.id_forecast as id_forecast2, a.month_forecast, a.year_forecast, " +
                    " (SELECT TOP 1 ISNULL(update_time, insert_time) FROM manage_forecast where id_forecast = a.id_forecast order by update_time desc) AS update_time, " +
                    " ISNULL(SUM(a.yearly_plan), 0) as yearly_plan," +
                    " ISNULL(SUM(a.n4), 0) as n4," +
                    " ISNULL(SUM(a.n3), 0) as n3," +
                    " ISNULL(SUM(a.n2), 0) as n2 " +
                    //" insert_time" +
                   " from manage_forecast a " +
                   //" where date_forecast BETWEEN '" + dateFrom + "' AND '" + dateTo + "' " +
                   //" where date_forecast >= '" + dateFrom + "' AND date_forecast <= '" + dateTo + "' " +
                   " WHERE " +
                   //" YEAR(date_forecast) >= YEAR('" + dateFrom + "') AND YEAR(date_forecast) <= YEAR('" + dateTo + "') AND " +
                   //" MONTH(date_forecast) >= MONTH('" + dateFrom + "') AND MONTH(date_forecast) <= MONTH('" + dateTo + "') " +
                   " a.date_forecast >= (CAST(YEAR('" + dateFrom + "') AS VARCHAR) + '-' + CAST(MONTH('" + dateFrom + "') AS VARCHAR) + '-01') " +
                   " AND a.date_forecast <= (CAST(YEAR('" + dateTo + "') AS VARCHAR) + '-' + CAST(MONTH('" + dateTo + "') AS VARCHAR) + '-" + Convert.ToString(noOfDaysInMonth) + "')" +
                   " group by a.id_forecast, a.month_forecast, a.year_forecast order by a.id_forecast desc";


                //var dataList = GSDbContext.Database.SqlQuery<ListGroupManageForecast>(sSQLSelect).ToList();
                //List<ManageForecast> tempList = new List<ManageForecast>();

                //foreach (var itemList in dataList)
                //{
                //    var getDataForecast = GSDbContext.ManageForecast.Where(p => p.id_recnum_forecast == itemList.id_recnum_forecast).SingleOrDefault();

                //    tempList.Add(getDataForecast);
                //}

                var dataList = GSDbContext.Database.SqlQuery<TempManageForecast>(sSQLSelect).ToList();

                loadResult = DataSourceLoader.Load(dataList, loadOptions);
            }
            else
            {
                string sSQLSelect = "";
                //sSQLSelect += "select a.id_forecast, (select top 1 id_recnum_forecast " +
                //    " from manage_forecast where id_forecast = a.id_forecast " +
                //    " order by update_time desc) as id_recnum_forecast " +
                //    " from manage_forecast a group by a.id_forecast order by a.id_forecast desc";

                sSQLSelect += "select a.id_forecast, a.id_forecast as id_forecast2, a.month_forecast, a.year_forecast, " +
                    " (SELECT TOP 1 ISNULL(update_time, insert_time) FROM manage_forecast where id_forecast = a.id_forecast order by update_time desc) AS update_time, " +
                    " ISNULL(SUM(a.yearly_plan), 0) as yearly_plan," +
                    " ISNULL(SUM(a.n4), 0) as n4," +
                    " ISNULL(SUM(a.n3), 0) as n3," +
                    " ISNULL(SUM(a.n2), 0) as n2" +
                    //" insert_time" +
                   " from manage_forecast a " +
                   //" where insert_time BETWEEN '" + dateFrom + "' AND '" + dateTo + "' " +
                   " group by a.id_forecast, a.month_forecast, a.year_forecast order by a.id_forecast desc";

                var dataList = GSDbContext.Database.SqlQuery<TempManageForecast>(sSQLSelect).ToList();

                //var dataList = GSDbContext.Database.SqlQuery<ListGroupManageForecast>(sSQLSelect).ToList();
                //List<ManageForecast> tempList = new List<ManageForecast>();

                //foreach (var itemList in dataList)
                //{
                //    var getDataForecast = GSDbContext.ManageForecast.Where(p => p.id_recnum_forecast == itemList.id_recnum_forecast).SingleOrDefault();

                //    tempList.Add(getDataForecast);
                //}

                loadResult = DataSourceLoader.Load(dataList, loadOptions);
            }
            //var dataList = GSDbContext.ManageForecast.ToList();

            return Request.CreateResponse(loadResult);
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage ViewDetails(string id, DataSourceLoadOptions loadOptions)
        {
            var detail = GSDbContext.ManageForecast.Where(e => e.id_forecast == id).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(detail, loadOptions));
        }

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var values = form.Get("values");

            var newForecast = new ManageForecast();
            JsonConvert.PopulateObject(values, newForecast);

            Validate(newForecast);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            newForecast.insert_time = DateTime.UtcNow.AddHours(7);

            GSDbContext.ManageForecast.Add(newForecast);
            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var forecast = GSDbContext.ManageForecast.First(e => e.id_recnum_forecast == key);

            //var newForecast = new ManageForecast();
            //newForecast.id_forecast = forecast.id_forecast;
            //newForecast.month_forecast = forecast.month_forecast;
            //newForecast.year_forecast = forecast.year_forecast;
            //newForecast.pn_customer = forecast.pn_customer;
            //newForecast.lot_size = forecast.lot_size;
            //newForecast.yearly_plan = forecast.yearly_plan;
            //newForecast.n4 = forecast.n4;
            //newForecast.n3 = forecast.n3;
            //newForecast.n2 = forecast.n2;
            //newForecast.type_battery = forecast.type_battery;
            //newForecast.type_material = forecast.type_material;
            //newForecast.brand = forecast.brand;
            //newForecast.group_forecast = forecast.group_forecast;
            //newForecast.spec = forecast.spec;
            //newForecast.pn_gs = forecast.pn_gs;
            ////newForecast.insert_time = DateTime.UtcNow.AddHours(7);
            //newForecast.update_time = DateTime.UtcNow.AddHours(7);

            JsonConvert.PopulateObject(values, forecast);

            forecast.update_time = DateTime.UtcNow.AddHours(7);

            //GSDbContext.ManageForecast.Add(forecast);

            Validate(forecast);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form, string id_forecast = null)
        {
            int statusCode = 500;
            if (!String.IsNullOrEmpty(id_forecast))
            {
                var dataAll = GSDbContext.ManageForecast.Where(p => p.id_forecast == id_forecast).ToList();
                GSDbContext.ManageForecast.RemoveRange(dataAll);
                GSDbContext.SaveChanges();
                statusCode = Convert.ToInt32(HttpStatusCode.OK);
            }
            else
            {
                var key = Convert.ToInt32(form.Get("key"));
                var dataTemp = GSDbContext.ManageForecast.First(e => e.id_recnum_forecast == key);                
                GSDbContext.ManageForecast.Remove(dataTemp);                
                GSDbContext.SaveChanges();
                statusCode = Convert.ToInt32(HttpStatusCode.OK);
            }

            return Request.CreateResponse(statusCode);
        }

    }
}