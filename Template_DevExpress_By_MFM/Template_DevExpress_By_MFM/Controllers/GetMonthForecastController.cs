using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class GetMonthForecastController : ApiController
    {

        public GetMonthForecastController()
        {

        }

        protected override void Dispose(bool disposing)
        {

        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string date, string forecast) //2022-01-31T00:00:00
        {
            var dtUpdate = DateTime.ParseExact(date.Substring(0, 19), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
            var kurangForecast = forecast.Replace("N", "");
            var updateDate = DateTime.Parse(dtUpdate).AddMonths(Convert.ToInt32(kurangForecast));

            var monthList = DateTimeFormatInfo
               .InvariantInfo.MonthNames.ToList();
            var monthName = monthList[updateDate.Month - 1];

            var monthListArray = new List<ListMonths>();
            monthListArray.Add(new ListMonths()
            {
                months = monthName
            });

            return Request.CreateResponse(DataSourceLoader.Load(monthListArray, loadOptions));
        }

    }
}