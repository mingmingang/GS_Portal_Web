using System;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class LoginController : Controller
    {
        public GSDbContext GSDbContext { get; set; }
        public GSDbContext GSDbContextDev { get; set; }

        public LoginController()
        {

            GSDbContext = new GSDbContext(".", "db_marketing_portal", "sa", "aangaang");
            GSDbContextDev = new GSDbContext(".", "db_marketing_portal", "sa", "aangaang");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PostLogin(string username, string userpass, string usertype, string userplant, string userdepartment)
        {
            bool hasil = false;
            var hasilCode = 0;
            if (!string.IsNullOrEmpty(usertype) && usertype == "GS")
            {
                var initLDAPPath = "dc=gs, dc=astra, dc=co, dc=id";
                var initLDAPServer = "10.19.48.7";
                var initShortDomainName = "gs";
                var DomainAndUsername = "";
                var strCommu = "LDAP://" + initLDAPServer + "/" + initLDAPPath;
                DomainAndUsername = initShortDomainName + @"\" + username;

                var entry = new DirectoryEntry(strCommu, DomainAndUsername, userpass);
                try
                {
                    var search = new DirectorySearcher(entry);
                    SearchResult result;
                    search.Filter = "(SAMAccountName=" + username + ")";
                    search.PropertiesToLoad.Add("cn");
                    result = search.FindOne();

                    if (result != null)
                    {
                        var passEncrypt = Template_DevExpress_By_MFM.Utils.Helper.EncodePassword(userpass, "bangcakrek");
                        var checkData = GSDbContext.MasterUser.Where(p => p.user_nama == username && p.user_pass == passEncrypt).SingleOrDefault();
                        if (checkData != null)
                        {
                            if (checkData.user_status == 1)
                            {
                                var role = checkData.user_role;
                                hasil = true;
                                hasilCode = 200;

                                SessionLogin session = new SessionLogin();
                                session.npk = checkData.user_npk;
                                session.fullname = checkData.user_nama;
                                session.userrole = checkData.user_role;
                                session.userdepartment = userdepartment;
                                session.userplant = checkData.shift_plant;
                                session.login_date = DateTime.UtcNow.AddHours(7);
                                Session["SHealth"] = session;

                                // CARA PANGGIL FUNCTION
                                // SAVE LOG LOGIN
                                var ipAddress = System.Web.HttpContext.Current != null ? System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : "";
                                bool bsavelog = SaveHistoryLogin("GS-ORDER", username, "success", 1, ipAddress);

                                return Json(new { status = hasil, status_code = hasilCode }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                // CARA PANGGIL FUNCTION
                                // SAVE LOG LOGIN
                                var ipAddress = System.Web.HttpContext.Current != null ? System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : "";
                                bool bsavelog = SaveHistoryLogin("GS-ORDER", username, "failed", 0, ipAddress);

                                hasilCode = 403;
                                hasil = false;
                            }

                        }
                        else
                        {
                            MasterUser masterUser = new MasterUser();
                            masterUser.user_nama = username;
                            masterUser.user_pass = passEncrypt;
                            masterUser.user_status = 0;
                            masterUser.user_createBy = username;
                            masterUser.user_createDate = DateTime.UtcNow.AddHours(7);
                            masterUser.shift_plant = userplant;
                            masterUser.user_role = "";

                            GSDbContext.MasterUser.Add(masterUser);
                            GSDbContext.SaveChanges();
                            hasilCode = 403;
                            hasil = false;
                        }

                    }
                    else
                    {
                        // CARA PANGGIL FUNCTION
                        // SAVE LOG LOGIN
                        var ipAddress = System.Web.HttpContext.Current != null ? System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : "";
                        bool bsavelog = SaveHistoryLogin("GS-ORDER", username, "failed", 0, ipAddress);

                        hasilCode = 404;
                        hasil = false;
                    }
                }
                catch (Exception ex)
                {
                    // CARA PANGGIL FUNCTION
                    // SAVE LOG LOGIN
                    var ipAddress = System.Web.HttpContext.Current != null ? System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : "";
                    bool bsavelog = SaveHistoryLogin("GS-ORDER", username, "failed", 0, ipAddress);

                    hasilCode = 500;
                    hasil = false;
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else if (!string.IsNullOrEmpty(usertype) && usertype == "Local")
            {
                try
                {
                    if (username == "admin" && userpass == "213020")
                    {
                        hasil = true;
                        hasilCode = 200;

                        SessionLogin session = new SessionLogin();
                        session.npk = "010830";
                        session.fullname = "superadmin";
                        session.userrole = "superadmin";
                        session.userplant = userplant;
                        session.userdepartment = userdepartment;
                        session.login_date = DateTime.UtcNow.AddHours(7);
                        Session["SHealth"] = session;
                        return Json(new { status = hasil, status_code = hasilCode }, JsonRequestBehavior.AllowGet);
                    }
                    else if (username == "karyawan" && userpass == "aang")
                    {
                        hasil = true;
                        hasilCode = 200;

                        SessionLogin session = new SessionLogin();
                        session.npk = "010830";
                        session.fullname = "superadmin";
                        session.userrole = "karyawan";
                        session.userplant = userplant;
                        session.userdepartment = userdepartment;
                        session.login_date = DateTime.UtcNow.AddHours(7);
                        Session["SHealth"] = session;
                        return Json(new { status = hasil, status_code = hasilCode }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var passEncrypt = Template_DevExpress_By_MFM.Utils.Helper.EncodePassword(userpass, "bangcakrek");

                        var checkData = GSDbContext.MasterUser.Where(p => p.user_npk == username && p.user_pass == passEncrypt && p.user_role != "customer").SingleOrDefault();

                        if (checkData != null)
                        {
                            hasil = true;
                            hasilCode = 200;

                            SessionLogin session = new SessionLogin();
                            session.npk = checkData.user_npk;
                            session.fullname = checkData.user_nama;
                            session.userrole = checkData.user_role;
                            session.userplant = checkData.shift_plant;
                            session.userdepartment = userdepartment;
                            session.login_date = DateTime.UtcNow.AddHours(7);
                            Session["SHealth"] = session;
                        }
                        else
                        {
                            hasilCode = 404;
                            hasil = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    hasilCode = 500;
                    hasil = false;
                    Console.WriteLine(ex.Message.ToString());
                }
            }            
            else if (!string.IsNullOrEmpty(usertype) && usertype == "Customer")
            {
                try
                {
                    //if (username == "admin" && userpass == "213020")
                    //{
                    //    hasil = true;
                    //    hasilCode = 200;

                    //    SessionLogin session = new SessionLogin();
                    //    session.npk = "010830";
                    //    session.fullname = "superadmin";
                    //    session.userrole = "superadmin";
                    //    session.userplant = userplant;
                    //    session.login_date = DateTime.UtcNow.AddHours(7);
                    //    Session["SHealth"] = session;
                    //    return Json(new { status = hasil, status_code = hasilCode }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{
                    //}
                    var passEncrypt = Template_DevExpress_By_MFM.Utils.Helper.EncodePassword(userpass, "bangcakrek");

                    var checkData = GSDbContext.MasterUser.Where(p => p.user_npk == username && p.user_pass == passEncrypt).SingleOrDefault();

                    if (checkData != null)
                    {
                        var getCust = GSDbContextDev.MasterCustomer.Where(p => p.customer_id == checkData.ref_id_cust && p.customer_name == checkData.user_nama).SingleOrDefault();

                        hasil = true;
                        hasilCode = 200;

                        SessionLogin session = new SessionLogin();
                        session.fullname = checkData.user_nama;
                        session.npk = checkData.user_npk;
                        session.periodic_price = getCust.customer_periodic_price;
                        session.batt_category = getCust.customer_batt_category;
                        session.country = Convert.ToInt32(getCust.country_id);
                        session.customer = Convert.ToInt32(checkData.ref_id_cust);
                        session.userrole = checkData.user_role;
                        session.userplant = userplant;
                        session.login_date = DateTime.UtcNow.AddHours(7);
                        Session["SHealth"] = session;
                    }
                    else
                    {
                        hasilCode = 404;
                        hasil = false;
                    }
                }
                catch (Exception ex)
                {
                    hasilCode = 500;
                    hasil = false;
                    Console.WriteLine(ex.Message.ToString());
                }
            }

            return Json(new { status = hasil, status_code = hasilCode }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            Session["SHealth"] = null;
            return RedirectToAction("", "Login");
        }



        // FUNCTION LAST LOGIN
        public bool SaveHistoryLogin(string program, string username, string reason, int status_login, string ip_source)
        {
            Boolean bResult = false;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            var token = GenerateToken();

            var clientID = ReadFile(5, "C:/tex.txt");
            var clientSecret = ReadFile(6, "C:/tex.txt");


            if (!string.IsNullOrEmpty(token))
                bResult = true;

            if (bResult)
            {
                string url_api = "https://gs-api.gs.astra.co.id/api/log/last_login";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_api);
                myReq.Method = "POST";
                myReq.ContentType = "application/x-www-form-urlencoded";
                myReq.Headers.Add("Authorization", ("Bearer " + token));
                myReq.Headers.Add("clientid", clientID);
                myReq.Headers.Add("clientsecret", clientSecret);
                string myData = "program=" + HttpUtility.UrlEncode(program) + "&username=" + HttpUtility.UrlEncode(username) + "&reason=" + HttpUtility.UrlEncode(reason) + "&status_login=" + HttpUtility.UrlEncode(status_login.ToString()) + "&ip_source=" + HttpUtility.UrlEncode(ip_source);

                string responseFromServer = "";
                try
                {
                    myReq.ContentLength = myData.Length;
                    using (var dataStream = myReq.GetRequestStream())
                    {
                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                    }
                    using (WebResponse response = myReq.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    throw;
                }

                if (responseFromServer != null)
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(JsonApi_Result)) as JsonApi_Result;
                    if (result != null)
                    {
                        if (result.meta[0].code == 200 && result.meta[0].status == "success")
                        {
                            bResult = true;
                        }
                        else
                        {
                            bResult = false;
                        }
                    }
                    else
                    {
                        bResult = false;
                    }
                }
            }

            return bResult;
        }

        public string GenerateToken()
        {
            var sToken = "";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            var user = ReadFile(0, "C:/tex.txt");
            var pass = ReadFile(1, "C:/tex.txt");
            var grant = ReadFile(2, "C:/tex.txt");

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            string url_api = "https://gs-api.gs.astra.co.id/generate-token";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_api);
            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            string myData = "username=" + HttpUtility.UrlEncode(user) + "&password=" + HttpUtility.UrlEncode(pass) + "&grant_type=" + HttpUtility.UrlEncode(grant);

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = myData.Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                }
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                throw;
            }

            if (responseFromServer != null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(APIModel)) as APIModel;
                if (result != null)
                    if (!string.IsNullOrEmpty(result.access_token))
                        sToken = result.access_token;
            }

            return sToken;
        }

        public string ReadFile(int urutan, string locdir)
        {
            var sResult = "";
            try
            {
                using (var sr = new StreamReader(locdir))
                {
                    var text = sr.ReadToEnd();
                    var sVar = text.Split(';');
                    sResult = sVar[urutan];
                    sr.Dispose();
                    sr.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return sResult;
        }
    }
}
