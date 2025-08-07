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
    public class ManageMasterCustomerController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ManageMasterCustomerController()
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
                var dataList = GSDbContext.MasterCustomer.ToList();
                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string IDcus)
        {
            //try
            //{
            if (!string.IsNullOrEmpty(IDcus))
            {
                int idcu = Convert.ToInt32(IDcus);
                var dataList = GSDbContext.MasterCustomer.Where(e => e.customer_id == idcu).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.Created, dataList);
                //return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(HttpStatusCode.Created, "");
            }
            //}
            //catch (Exception ex)
            //{
            //    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            //}
        }

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var master = new MasterCustomer();

                JsonConvert.PopulateObject(values, master);

                Validate(master);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                //if (!string.IsNullOrEmpty(values) && values.Contains("user_pass"))
                //master.user_pass = Helper.EncodePassword(master.user_pass, "bangcakrek");

                //if (!string.IsNullOrEmpty(values) && values.Contains("customer_password"))
                //    master.customer_password = Helper.EncodePassword(master.customer_password, "bangcakrek");

                master.customer_createDate = DateTime.UtcNow.AddHours(7);
                master.customer_createBy = sessionLogin.fullname;

                GSDbContext.MasterCustomer.Add(master);
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
                var key = Convert.ToInt64(form.Get("key"));
                var values = form.Get("values");
                var master = GSDbContext.MasterCustomer.First(e => e.customer_id == key);

                JsonConvert.PopulateObject(values, master);

                Validate(master);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                //if (!string.IsNullOrEmpty(values) && values.Contains("user_pass"))
                //    master.user_pass = Helper.EncodePassword(master.user_pass, "bangcakrek");
                master.customer_modifDate = DateTime.UtcNow.AddHours(7);
                master.customer_modifBy = sessionLogin.fullname;
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
                var key = Convert.ToInt64(form.Get("key"));
                var order = GSDbContext.MasterCustomer.First(e => e.customer_id == key);

                GSDbContext.MasterCustomer.Remove(order);
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