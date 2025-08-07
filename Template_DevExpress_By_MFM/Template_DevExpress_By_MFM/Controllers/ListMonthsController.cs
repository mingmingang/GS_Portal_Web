using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ListMonthsController : ApiController
    {

        public ListMonthsController()
        {

        }

        protected override void Dispose(bool disposing)
        {

        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var monthList = DateTimeFormatInfo
                .InvariantInfo.MonthNames
                .Select((monthName, index) => new ListMonths
                { 
                    id_recnum_month = (index + 1).ToString(),
                    months = monthName
                });

            return Request.CreateResponse(DataSourceLoader.Load(monthList, loadOptions));
        }

    }
}