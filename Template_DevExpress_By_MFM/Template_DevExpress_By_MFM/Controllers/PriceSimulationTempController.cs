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
    public class PriceSimulationTempController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public PriceSimulationTempController()
        {
            GSDbContext = new GSDbContext(@"", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var dataList = new List<ManagePriceSimulation_temp>();
            try
            {
                if(sessionLogin.userrole == "customer")
                {
                    dataList = GSDbContext.ManagePriceSimulation_temp.Where(e => e.cust_id == sessionLogin.customer).ToList();

                } else
                {
                    dataList = GSDbContext.ManagePriceSimulation_temp.ToList();

                }
                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }


        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string CustID, Boolean flag)
        {
            LoadResult loadResult = new LoadResult();

            try
            {
                int i = 0;
                var noIDLastOrder = "";
                var dateNow = DateTime.UtcNow.AddHours(7);

                string sSQLSelect = "select top 1 price_sim_id from t_price_simulation order by price_sim_id desc";

                var checkLastIDOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(sSQLSelect).SingleOrDefault();
                if (checkLastIDOrder != null)
                {   
                    if (!string.IsNullOrEmpty(checkLastIDOrder.price_sim_id))
                    {

                        //string sSQLSelectCheckForNow = "select top 1 id_order from manage_order where id_order like '" + "OD" + dateNow.ToString("yyMMdd") + "%' order by id_order desc";
                        string sSQLSelectCheckForNow = "select top 1 price_sim_id from t_price_simulation where price_sim_id like '" + "CUST01" + dateNow.ToString("MMMyyyy") + "%' order by price_sim_id desc";

                        var checkLastIDWithDateNowOrder = GSDbContext.Database.SqlQuery<GetLastPriceID>(sSQLSelectCheckForNow).SingleOrDefault();

                        if (checkLastIDWithDateNowOrder != null)
                        {
                            int lastID = Convert.ToInt32(checkLastIDWithDateNowOrder.price_sim_id.Substring(15, 3)) + 1;
                            noIDLastOrder = checkLastIDWithDateNowOrder.price_sim_id.Substring(0, 15) + lastID.ToString().PadLeft(3, '0');
                        }
                        else
                        {
                            int lastID = 1;
                            //noIDLastOrder = "OD" + dateNow.ToString("yyMMdd") + lastID.ToString().PadLeft(5, '0');
                            noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" + lastID.ToString().PadLeft(3, '0');
                        }

                    }
                }
                else
                {
                    var z = 1;
                    noIDLastOrder = CustID + "-" + dateNow.ToString("MMMyyyy") + "-" +z.ToString().PadLeft(3, '0');
                    //noIDLastOrder = "CUST001" + dateNow.ToString("yyMMdd") + z.ToString().PadLeft(5, '0');
                }
                
                return Request.CreateResponse(HttpStatusCode.Created, noIDLastOrder);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }


        //[SessionCheck]
        //[HttpGet]
        //public HttpResponseMessage ViewDetails(string id, DataSourceLoadOptions loadOptions)
        //{
        //    var query = "SELECT id_recnum_order, id_order, po_number, sales_order, pn_customer, lot_size, type_battery, type_material, brand, group_order, spec, status_order, insert_time, update_time, user_input, pn_gs, month_order, year_order, " +
        //        " CASE WHEN confirm != 0 THEN confirm " +
        //        " WHEN confirm = 0 THEN total ELSE total END as total , " +
        //        " confirm, ship_to_JKT, ship_to_BDG, ship_to_SBY, ship_to_SMG, order_date, confirm_to_JKT, confirm_to_BDG, confirm_to_SBY, confirm_to_SMG, adjustment, file_excel " +
        //        " FROM dbo.manage_order WHERE id_order = '" + id + "'";
        //    var detail = GSDbContext.Database.SqlQuery<ManageOrder>(query).ToList();
        //    //var detail = GSDbContext.ManageOrder.Where(e => e.id_order == id).ToList();

        //    return Request.CreateResponse(DataSourceLoader.Load(detail, loadOptions));
        //}


        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var order = GSDbContext.ManagePriceSimulation_temp.First(e => e.recnum_id == key);

            JsonConvert.PopulateObject(values, order);

            Validate(order);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            order.price_sim_modifBy = sessionLogin.fullname;
            order.price_sim_modifDate = DateTime.UtcNow.AddHours(7);


            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var values = form.Get("values");

            var newOrder = new ManagePriceSimulation_temp();
            JsonConvert.PopulateObject(values, newOrder);

            Validate(newOrder);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            newOrder.price_sim_createDate = DateTime.UtcNow.AddHours(7);
            newOrder.price_sim_createBy = sessionLogin.fullname;
            newOrder.cust_id = sessionLogin.customer;
            newOrder.price_sim_status = 1;

            GSDbContext.ManagePriceSimulation_temp.Add(newOrder);
            GSDbContext.SaveChanges();

            //ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
            //manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
            //manageHistoryTransaction.type_history = "transaction_order";
            //manageHistoryTransaction.desc_history = "Order added : " + newOrder.id_order;
            //GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
            //GSDbContext.SaveChanges();

           

            return Request.CreateResponse(HttpStatusCode.Created);
        }  
        
        
        //[SessionCheck]
        //[HttpDelete]
        //public HttpResponseMessage Delete(FormDataCollection form, string id_order = null)
        //{
        //    int statusCode = 500;
        //    if (!String.IsNullOrEmpty(id_order))
        //    {
        //        var dataAll = GSDbContext.ManageOrder.Where(p => p.id_order == id_order).ToList();
        //        GSDbContext.ManageOrder.RemoveRange(dataAll);
        //        GSDbContext.SaveChanges();
        //        statusCode = Convert.ToInt32(HttpStatusCode.OK);
        //    }
        //    else
        //    {
        //        var key = Convert.ToInt32(form.Get("key"));
        //        var order = GSDbContext.ManageOrder.First(e => e.id_recnum_order == key);

        //        ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
        //        manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
        //        manageHistoryTransaction.type_history = "transaction_order";
        //        manageHistoryTransaction.desc_history = "Order removed : " + order.id_order;
        //        GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);

        //        GSDbContext.ManageOrder.Remove(order);
        //        GSDbContext.SaveChanges();
        //        statusCode = Convert.ToInt32(HttpStatusCode.OK);
        //    }             
        //    return Request.CreateResponse(statusCode);
        //}



        //[System.Web.Http.Route("api/delete-price-temp", Name = "DeletePriceTemp")]
        [SessionCheck]
        [System.Web.Http.HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                var key = Convert.ToInt32(form.Get("key"));
                //var quotation = GSDbContext.ManagePriceQuotation.First(e => e.id == key);
                var order = GSDbContext.ManagePriceSimulation_temp.FirstOrDefault(e => e.recnum_id == key);
                if (order != null)
                {
                    GSDbContext.ManagePriceSimulation_temp.Remove(order);
                    GSDbContext.SaveChanges();
                    return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Data not found!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }
        }
    }
}