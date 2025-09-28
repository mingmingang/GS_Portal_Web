using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ListPartNumberController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ListPartNumberController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var query = "";
            //var query = "select * from tlkp_partnumber where PN_status=1";
            if (sessionLogin.userrole != "customer" && !sessionLogin.userrole.Equals("customer"))
            {
                query = "select * from tlkp_partnumber where PN_status=1";
            } else
            {
                query = "select * from tlkp_partnumber where PN_status=1 and cust_id=" + sessionLogin.customer + " and PN_category_batt = '" + sessionLogin.batt_category + "'";
            }
            
            var dataList = GSDbContext.Database.SqlQuery<MasterPartNumber>(query).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage GetDatawithJIS(DataSourceLoadOptions loadOptions, string newjis, string oldjis, string battseg)
        {
            if (newjis != null || newjis != "" || oldjis != null || oldjis != "" || battseg != null || battseg != "")
            {
                //var query = "select distinct[PN_old_jis],[PN_new_jis],[PN_status] from tlkp_partnumber where PN_status=1 and PN_new_jis like '%" + newjis + "%' and PN_old_jis like '%"+oldjis+"%' and PN_category_batt = '"+sessionLogin.batt_category+"'";
                var query = "select * from tlkp_partnumber where PN_status = 1 and PN_batt_segmentation = '" + battseg + "' and PN_new_jis like '%" + newjis + "%' and PN_old_jis like '%" + oldjis + "%' and PN_category_batt = '" + sessionLogin.batt_category + "' and cust_id = " + sessionLogin.customer + "";
                var dataList = GSDbContext.Database.SqlQuery<MasterPartNumber>(query).ToList();

                foreach (var dataParNumber in dataList)
                {
                    string sSQLSelectPrice = "select top 1 [log_unit_price] as prev_price from t_log_history_price where log_part_number='" + dataParNumber.part_number + "' order by log_createDate desc";
                    var sPrevPrice = GSDbContext.Database.SqlQuery<GetPrevPrice>(sSQLSelectPrice).SingleOrDefault();
                    if(sPrevPrice == null)
                        return Request.CreateResponse(HttpStatusCode.OK, "Unit Price not found in this type!");
                }                               

                if (dataList.Count() > 0)
                {
                    return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Part Number Data not found!");
                }

            }
            else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }


        [System.Web.Http.Route("api/ListPartNumber/list-new-jis", Name = "ListNewJIS")]
        [HttpGet]
        public HttpResponseMessage GetNewJIS(DataSourceLoadOptions loadOptions)
        {
            var query = "select distinct[PN_old_jis],[PN_new_jis],[PN_status],PN_batt_segmentation from tlkp_partnumber where PN_status=1 and cust_id=" + sessionLogin.customer;
            var dataList = GSDbContext.Database.SqlQuery<GetOldJISData>(query).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));

        }

        [System.Web.Http.Route("api/list-old-jis", Name = "ListOldJIS")]
        [HttpGet]
        public HttpResponseMessage GetOldJIS(DataSourceLoadOptions loadOptions, string jis)
        {
            if (!string.IsNullOrEmpty(jis))
            {
                var query = "select distinct[PN_old_jis],[PN_new_jis],[PN_status] from tlkp_partnumber where PN_status=1 and cust_id=" + sessionLogin.customer + " and PN_new_jis like '%" + jis + "%'";
                var dataList = GSDbContext.Database.SqlQuery<GetOldJISData>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string bpid)
        {
            if (bpid != null || bpid != "")
            {
                var query = "select * from tlkp_partnumber where PN_status=1 and part_number like '%" + bpid + "%'";
                var dataList = GSDbContext.Database.SqlQuery<MasterPartNumber>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }

        [System.Web.Http.Route("api/PriceSimulation/get-PN", Name = "GetPartNumber")]
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage GetPN(DataSourceLoadOptions loadOptions, string cust_id)
        {
            if (cust_id != null || cust_id != "")
            {
                var query = "select * from tlkp_partnumber where PN_status=1 and cust_id = '%" + cust_id + "%'";
                var dataList = GSDbContext.Database.SqlQuery<MasterPartNumber>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }
    }
}