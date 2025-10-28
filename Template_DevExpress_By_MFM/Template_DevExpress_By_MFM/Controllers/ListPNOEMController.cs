using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ListPNOEMController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }
        public GSDbContext GSDbContextMKT { get; set; }

        public ListPNOEMController()
        {
            GSDbContext = new GSDbContext("", "", "", "");
            GSDbContextMKT = new GSDbContext("", "", "", "");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var query = "select * from tlkp_PN_OEM where PN_status = 1";
            var dataList = GSDbContextMKT.Database.SqlQuery<ManagePartNumberOEM>(query).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string bpid)
        {
            if (!string.IsNullOrEmpty(bpid))
            {
                var query = "select * from tlkp_PN_OEM where PN_status = 1 and pn_bpid like '%" + bpid + "%'";
                var dataList = GSDbContextMKT.Database.SqlQuery<ManagePartNumberOEM>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string pn, string bpid)
        {
            if (!string.IsNullOrEmpty(pn))
            {
                //var query = "select * from [tlkp_type_OEM] where item like '%" + pn + "%'";
                var query = "select * from tlkp_PN_OEM where PN_status = 1 and pn_bpid like '%" + bpid + "%' and pn_gs = '"+pn+"'";
                var dataList = GSDbContextMKT.Database.SqlQuery<ManagePartNumberOEM>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }
    }
}