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
    public class LogPriceController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public LogPriceController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var dataList = GSDbContext.ManageLogPrice.ToList();
                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }


        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(ManageLogPrice form)
        {
            form.log_status = 1;
            form.log_createDate = DateTime.UtcNow.AddHours(7);
            form.log_createBy = sessionLogin.fullname;

            //Validate(form);
            //if (!ModelState.IsValid)
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            GSDbContext.ManageLogPrice.Add(form);
            GSDbContext.SaveChanges();

            return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
        }  
        

        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                var key = Convert.ToInt64(form.Get("key"));
                var order = GSDbContext.ManageLogPrice.First(e => e.id == key);

                GSDbContext.ManageLogPrice.Remove(order);
                GSDbContext.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }

        }
    }
}