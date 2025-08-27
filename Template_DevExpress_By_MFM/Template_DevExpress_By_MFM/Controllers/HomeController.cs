using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Template_DevExpress_By_MFM.Controllers {
    public class HomeController : Controller {

        public GSDbContext GSDbContext { get; set; }
        public HomeController()
        {

            GSDbContext = new GSDbContext(".", "db_marketing_portal", "sa", "aangaang");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }
        string FormatNumber<T>(T number, int maxDecimals = 4)
        {
            return Regex.Replace(String.Format("{0:n" + maxDecimals + "}", number),
                                 @"[" + System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "]?0+$", "");
        }

        public ActionResult Index()
        {
            return View();
        }


    }
}