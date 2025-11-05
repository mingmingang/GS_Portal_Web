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


    }
}