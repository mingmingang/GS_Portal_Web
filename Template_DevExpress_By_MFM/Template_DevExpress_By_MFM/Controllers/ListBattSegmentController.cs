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

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ListBattSegmentController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ListBattSegmentController()
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
                query = "select PN_batt_segmentation from tlkp_partnumber where PN_status = 1 group by PN_batt_segmentation";
            } else
            {
                query = "select distinct(PN_batt_segmentation) from tlkp_partnumber where PN_status=1 and cust_id=" + sessionLogin.customer + " and PN_category_batt = '" + sessionLogin.batt_category + "'";
            }
            
            var dataList = GSDbContext.Database.SqlQuery<GetBattSegmentData>(query).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

    }
}