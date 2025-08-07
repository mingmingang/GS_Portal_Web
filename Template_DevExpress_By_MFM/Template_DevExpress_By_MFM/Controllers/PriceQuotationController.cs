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
using System.Globalization;

namespace Template_DevExpress_By_MFM.Controllers
{
    //[System.Web.Http.Route("Forecast/{action}", Name = "Forecast")]
    public class PriceQuotationController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public PriceQuotationController()
        {
            GSDbContext = new GSDbContext(@"", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }


        [SessionCheck]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var dataList = GSDbContext.ManagePriceQuotation.ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        [SessionCheck]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string bpid)
        {
            var dataList = GSDbContext.ManagePriceQuotation.Where(p => p.customer_bpid == bpid).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }


        [SessionCheck]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post(ManagePriceQuotation form)
        {
            try
            {
                //if (!string.IsNullOrEmpty(form.quotation_period))
                //{
                //    //form.quotation_period = DateTime.ParseExact(form.quotation_period.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("MMM `yy");
                //    var peri1 = DateTime.ParseExact(form.quotation_period.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("MMM `yy");

                //    var thn1 = peri1.slice(peri1.length - 2);
                //    var thn2 = peri2.slice(peri2.length - 2);
                //    var final_price_per;
                //    if (thn1 == thn2)
                //    {
                //        final_price_per = peri1.slice(0, 3); +"-" + peri2;
                //    }
                //    else
                //    {
                //        final_price_per = peri1 + "-" + peri2;
                //    }
                //}
                //form.quotation_period = "MMM `yy"
                  //  DateTime.ParseExact(form.wo_date.Substring(0, 24), "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("MMM `yy")

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

        [SessionCheck]
        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(ManagePriceQuotation form)
        {
            try
            {
                if (form != null)
                {
                    var itemQuotation = GSDbContext.ManagePriceQuotation.First(e => e.id == form.id);

                    itemQuotation.quotation_period = form.quotation_period;
                    itemQuotation.customer_name = form.customer_name;
                    itemQuotation.battery_type = form.battery_type;
                    itemQuotation.part_number = form.part_number;
                    itemQuotation.LME_lead = form.LME_lead;
                    itemQuotation.premium1 = form.premium1;
                    itemQuotation.premium2 = form.premium2;
                    itemQuotation.premium3 = form.premium3;
                    itemQuotation.plastic_pp = form.plastic_pp;
                    itemQuotation.ex_rate = form.ex_rate;
                    itemQuotation.material_weight1 = form.material_weight1;
                    itemQuotation.import_duty = form.import_duty;
                    itemQuotation.handling_fee = form.handling_fee;
                    itemQuotation.lpp_fee1 = form.lpp_fee1;
                    itemQuotation.lead_premium1 = form.lead_premium1;
                    itemQuotation.material_weight2 = form.material_weight2;
                    itemQuotation.import_duty2 = form.import_duty2;
                    itemQuotation.handling_fee2 = form.handling_fee2;
                    itemQuotation.lpp_fee2 = form.lpp_fee2;
                    itemQuotation.lead_premium2 = form.lead_premium2;
                    itemQuotation.material_weight3 = form.material_weight3;
                    itemQuotation.import_duty3 = form.import_duty3;
                    itemQuotation.handling_fee3 = form.handling_fee3;
                    itemQuotation.lpp_fee3 = form.lpp_fee3;
                    itemQuotation.lead_premium3 = form.lead_premium3;
                    itemQuotation.plastic_weight = form.plastic_weight;
                    itemQuotation.import_duty_plastic = form.import_duty_plastic;
                    itemQuotation.handling_fee_plastic = form.handling_fee_plastic;
                    itemQuotation.pp_price = form.pp_price;
                    itemQuotation.plastic = form.plastic;
                    itemQuotation.separator = form.separator;
                    itemQuotation.others_purchase = form.others_purchase;
                    itemQuotation.sub_total_mat_cost = form.sub_total_mat_cost;
                    itemQuotation.process_plate = form.process_plate;
                    itemQuotation.process_injection = form.process_injection;
                    itemQuotation.process_assembling = form.process_assembling;
                    itemQuotation.process_charging = form.process_charging;
                    itemQuotation.sub_total_process_cost = form.sub_total_process_cost;
                    itemQuotation.total = form.total;
                    itemQuotation.general_charge = form.general_charge;
                    itemQuotation.others = form.others;
                    itemQuotation.support = form.support;
                    itemQuotation.grand_total = form.grand_total;

                    itemQuotation.quotation_modifDate = DateTime.UtcNow.AddHours(7);
                    itemQuotation.quotation_modifBy = sessionLogin.fullname ?? "";
                    itemQuotation.quotation_status = 2;

                    Validate(itemQuotation);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                    GSDbContext.SaveChanges();

                    return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
                }
                else
                {
                    //jsonAPI.code = 401;
                    //jsonAPI.status = "error";
                    //jsonAPI.message = "Form is Empty";

                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Form is empty!");
                }

            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }
            

        }

        [SessionCheck]
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

        [SessionCheck]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int id)
        {
            try
            {
                if (id.ToString() != null)
                {
                    var listData = GSDbContext.ManagePriceQuotation.Where(e => e.id == id).ToList();


                    if (listData != null)
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(listData, loadOptions));
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.OK, "Data tidak ada!");
                    }
                }
                else
                {
                    return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }            
        }
    }
}