using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Template_DevExpress_By_MFM.Utils
{
    public class SessionCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            if (session["SHealth"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                { "Controller", "Login" },
                // Ganti "" dengan nama Action Method untuk login, biasanya "Index" atau "Login"
                { "Action", "Index" }
                    });

                session["NotAuthorized"] = true;
            }
        }
    }
}