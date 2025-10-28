using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ListYearsController : ApiController
    {

        public ListYearsController()
        {

        }

        protected override void Dispose(bool disposing)
        {

        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            const int numberOfYears = 21;
            var startYear = DateTime.UtcNow.AddHours(7).AddYears(-3).Year;
            var endYear = startYear + numberOfYears;

            var yearList = new List<ListYears>();
            for (var i = startYear; i < endYear; i++)
            {
                yearList.Add(new ListYears()
                {
                    years = i.ToString()
                });
            }

            return Request.CreateResponse(DataSourceLoader.Load(yearList, loadOptions));
        }

    }
}