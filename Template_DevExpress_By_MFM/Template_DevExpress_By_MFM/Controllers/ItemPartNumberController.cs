using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System.Web.Helpers;
using System.Web;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ItemPartNumberController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }

        public ItemPartNumberController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var dataList = GSDbContext.ManageItemPartNumber.ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }


        [SessionCheck]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var values = form.Get("values");

            var itemPartNumber = new ManageItemPartNumber();
            JsonConvert.PopulateObject(values, itemPartNumber);

            itemPartNumber.date_insert = DateTime.UtcNow.AddHours(7);

            Validate(itemPartNumber);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());


            GSDbContext.ManageItemPartNumber.Add(itemPartNumber);
            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [SessionCheck]
        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var itemPartNumber = GSDbContext.ManageItemPartNumber.First(e => e.id_recnum_min == key);
            JsonConvert.PopulateObject(values, itemPartNumber);

            itemPartNumber.date_update = DateTime.UtcNow.AddHours(7);

            Validate(itemPartNumber);

            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [SessionCheck]
        [System.Web.Http.HttpDelete]
        public void Delete(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var employee = GSDbContext.ManageItemPartNumber.First(e => e.id_recnum_min == key);

            GSDbContext.ManageItemPartNumber.Remove(employee);
            GSDbContext.SaveChanges();
        }

    }
}