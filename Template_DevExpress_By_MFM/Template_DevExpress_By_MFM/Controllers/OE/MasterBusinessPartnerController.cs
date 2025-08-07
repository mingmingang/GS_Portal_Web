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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class MasterBusinessPartnerController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public MasterBusinessPartnerController()
        {
            //GSDbContext = new GSDbContext(@"DEV-KRW\SQLEXPRESS", "db_marketing_portal", "sa", "gsmis@2017");
            GSDbContext = new GSDbContext(@"GSPORTAL-DEV01", "db_marketing_portal", "sa", "gsmis@2017");
            //VMDbContext = new VMDbContext(@"127.0.0.1", "db_vending_machine", "sa", "213020Uzi");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }


        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var dataList = GSDbContext.ManagePriceQuotation.ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }


        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post(ManagePriceQuotation form)
        {
            try
            {
                form.quotation_createDate = DateTime.UtcNow.AddHours(7);
                form.quotation_createBy = sessionLogin.fullname ?? "";
                form.quotation_status = 1;

                //Validate(form);
                //if (!ModelState.IsValid)
                //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());
                
                GSDbContext.ManagePriceQuotation.Add(form);
                GSDbContext.SaveChanges();

                return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }            
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(ManagePriceQuotation form)
        {
            try
            {
                var itemQuotation = GSDbContext.ManagePriceQuotation.First(e => e.id == form.id);

                form.quotation_modifDate = DateTime.UtcNow.AddHours(7);
                form.quotation_modifBy = sessionLogin.fullname ?? "";
                form.quotation_status = 2;

                //Validate(form);
                //            if (!ModelState.IsValid)
                //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                GSDbContext.SaveChanges();

                return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }
            

        }

        [System.Web.Http.HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form, int id)
        {
            try
            {
                //var key = Convert.ToInt32(form.Get("key"));
                //var quotation = GSDbContext.ManagePriceQuotation.First(e => e.id == key);
                var quotation = GSDbContext.ManagePriceQuotation.FirstOrDefault(e => e.id == id);
                if(quotation != null)
                {
                    GSDbContext.ManagePriceQuotation.Remove(quotation);
                    GSDbContext.SaveChanges();
                    return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Data tidak ada!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }
        }
    }
}