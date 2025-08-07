using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ManageMasterAttnController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ManageMasterAttnController()
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
                var dataList = GSDbContext.MasterAttn.ToList();
                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var master = new MasterAttn();

                JsonConvert.PopulateObject(values, master);

                Validate(master);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                //if (!string.IsNullOrEmpty(values) && values.Contains("user_pass"))
                //    master.user_pass = Helper.EncodePassword(master.user_pass, "bangcakrek");

                master.attn_createDate = DateTime.UtcNow.AddHours(7);
                master.attn_createBy = sessionLogin.fullname;

                GSDbContext.MasterAttn.Add(master);
                GSDbContext.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }

        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                //var key = Convert.ToInt64(form.Get("key"));
                var key = form.Get("key");
                var values = form.Get("values");
                var master = GSDbContext.MasterAttn.First(e => e.attn_bp_code == key);

                JsonConvert.PopulateObject(values, master);

                Validate(master);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                //if (!string.IsNullOrEmpty(values) && values.Contains("user_pass"))
                //    master.user_pass = Helper.EncodePassword(master.user_pass, "bangcakrek");
                master.attn_modifDate = DateTime.UtcNow.AddHours(7);
                master.attn_modifBy = sessionLogin.fullname;
                GSDbContext.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }

        }

        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                //var key = Convert.ToInt64(form.Get("key"));
                var key = form.Get("key");
                var order = GSDbContext.MasterAttn.First(e => e.attn_bp_code == key);

                GSDbContext.MasterAttn.Remove(order);
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