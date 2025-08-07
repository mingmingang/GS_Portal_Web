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
    public class ListPNController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }
        public GSDbContext GSDbContextMKT { get; set; }

        public ListPNController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
            GSDbContextMKT = new GSDbContext("", "", "", "");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
            GSDbContextMKT.Dispose();
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var query = "select LTRIM(T$ITEM) as item, T$DSCA as descrip from ttcibd0018888 where LTRIM(T$ITEM) like 'F-%'";
            var dataList = GSDbContext.Database.SqlQuery<ManagePN>(query).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string bpid)
        {
            if (bpid != null || bpid != "")
            {
                var query = "select LTRIM(T$ITEM) as item, T$DSCA as descrip from ttcibd0018888 where LTRIM(T$ITEM) like 'F-%' and T$ITEM like '%" + bpid + "'%";
                var dataList = GSDbContext.Database.SqlQuery<ManagePN>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }

        [HttpGet]
        public HttpResponseMessage GetPN(DataSourceLoadOptions loadOptions, string pn)
        {
            if (pn != null || pn != "")
            {
                var query = "select * from [tlkp_type_OEM] where item like '%" + pn + "%'";
                var dataList = GSDbContextMKT.Database.SqlQuery<ManageTypeOEM>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
        }
    }
}