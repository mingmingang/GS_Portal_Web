// File: Controllers/LoginController.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class LoginController : Controller
    {
        // Data is now loaded from a JSON file and cached.
        private static List<EmployeeJson> _employees;
        private static readonly object _lock = new object();

        public LoginController()
        {
            // Load employee data from JSON file only once.
            if (_employees == null)
            {
                lock (_lock)
                {
                    if (_employees == null)
                    {
                        try
                        {
                            // Assumes DataEmployee.json is in the App_Data folder.
                            string filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/JsonMasterData/DataEmployee.json");
                            string jsonData = System.IO.File.ReadAllText(filePath);
                            _employees = JsonConvert.DeserializeObject<List<EmployeeJson>>(jsonData);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"FATAL: Failed to load or parse DataEmployee.json. {ex.Message}");
                            // If the employee data can't be loaded, the application can't function.
                            throw new Exception("Tidak dapat memuat data karyawan.", ex);
                        }
                    }
                }
            }
        }

        #region Actions (Index, PostLogin, Logout)
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostLogin(string username, string userpass, string usertype, string userplant)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userpass) || string.IsNullOrEmpty(usertype) || string.IsNullOrEmpty(userplant))
                {
                    return Json(new { status = false, status_code = 400, message = "Semua field wajib diisi." });
                }

                switch (usertype)
                {
                    case "GS":
                        return HandleGSLogin(username, userpass, userplant);
                    case "Local":
                        return HandleLocalLogin(username, userpass, userplant);
                    default:
                        return Json(new { status = false, status_code = 400, message = "Tipe login tidak valid." });
                }
            }
            catch (Exception ex)
            {
                string detailedError = $"NPK: {username}, Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    detailedError += $" | Inner Exception: {ex.InnerException.Message}";
                }
                System.Diagnostics.Debug.WriteLine($"LOGIN EXCEPTION: {detailedError}");
                return Json(new { status = false, status_code = 500, message = detailedError }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Logout()
        {
            var npk = (Session["SHealth"] as SessionLogin)?.npk ?? "Unknown User";
            Session.Clear();
            Session.Abandon();
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-10);
            }
            SaveHistoryLogin("GS-REIMBURSE-APP", npk, "Logout success", 1, GetIpAddress());
            return RedirectToAction("Index", "Login");
        }
        #endregion

        #region Login Handlers

        private ActionResult HandleGSLogin(string npk, string password, string plant)
        {
            return Json(new { status = false, message = "Login LDAP belum diimplementasikan sepenuhnya" }, JsonRequestBehavior.AllowGet);
        }

        //private ActionResult HandleLocalLogin(string npkInput, string passwordInput, string plant)
        //{
        //    const string AppSource = "GS-TRACK-WEB";
        //    string cleanNpk = npkInput?.Trim() ?? string.Empty;

        //    if (string.IsNullOrWhiteSpace(cleanNpk) || string.IsNullOrWhiteSpace(passwordInput))
        //    {
        //        return Json(new { status = false, status_code = 400, message = "NPK dan Password harus diisi." });
        //    }

        //    // Authenticate against the list loaded from JSON.
        //    // NOTE: Assuming password is the same as emp_no since it's not in the JSON file.
        //    var karyawan = _employees.FirstOrDefault(k =>
        //        k.emp_no.Trim().Equals(cleanNpk, StringComparison.OrdinalIgnoreCase) &&
        //        k.emp_no == passwordInput
        //    );

        //    if (karyawan == null)
        //    {
        //        SaveHistoryLogin(AppSource, cleanNpk, "Local auth failed: Invalid NPK/Pass", 0, GetIpAddress());
        //        return Json(new { status = false, status_code = 404, message = "NPK atau Password salah." });
        //    }

        //    if (IsUserInactive(karyawan.status))
        //    {
        //        SaveHistoryLogin(AppSource, cleanNpk, "Local auth failed: User inactive", 0, GetIpAddress());
        //        return Json(new { status = false, status_code = 403, message = "Akun Anda sudah tidak aktif." });
        //    }

        //    var jabatan = karyawan.pos_name_id?.Trim().ToUpper();
        //    // Users with Supervisor position get to choose their role.
        //    var roles = new List<string> { "SUPERVISOR" };

        //    if (!string.IsNullOrEmpty(jabatan) && roles.Contains(jabatan))
        //    {
        //        Session["PendingLoginUser"] = karyawan;
        //        Session["PendingLoginPlant"] = plant;
        //        Session.Timeout = 5;

        //        var availableRoles = new List<string> { karyawan.pos_name_id, "Karyawan" };
        //        return Json(new
        //        {
        //            status = true,
        //            status_code = 201,
        //            action = "CHOOSE_ROLE",
        //            roles = availableRoles
        //        });
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(karyawan.pos_name_id))
        //        {
        //            karyawan.pos_name_id = "Karyawan";
        //        }
        //        CreateUserSession(karyawan, plant);
        //        SaveHistoryLogin(AppSource, cleanNpk, "Login success via Local Auth", 1, GetIpAddress());
        //        return Json(new { status = true, status_code = 200, action = "REDIRECT" });
        //    }
        //}

        private ActionResult HandleLocalLogin(string npkInput, string passwordInput, string plant)
        {
            const string AppSource = "GS-TRACK-WEB";
            string cleanNpk = npkInput?.Trim() ?? string.Empty;

            // Validasi sekarang cukup memeriksa NPK saja, karena password diabaikan
            if (string.IsNullOrWhiteSpace(cleanNpk))
            {
                return Json(new { status = false, status_code = 400, message = "NPK harus diisi." });
            }

            // --- INI PERUBAHAN UTAMANYA ---
            // Sekarang, sistem hanya mencari karyawan berdasarkan NPK. 
            // Kondisi pengecekan password telah dihapus.
            var karyawan = _employees.FirstOrDefault(k =>
                k.emp_no.Trim().Equals(cleanNpk, StringComparison.OrdinalIgnoreCase)
            );

            // Jika NPK tidak ditemukan
            if (karyawan == null)
            {
                SaveHistoryLogin(AppSource, cleanNpk, "Local auth failed: Invalid NPK", 0, GetIpAddress());
                // Pesan error diubah agar lebih sesuai, karena password tidak lagi relevan
                return Json(new { status = false, status_code = 404, message = "NPK tidak ditemukan." });
            }

            // Pengecekan status aktif (tidak berubah)
            if (IsUserInactive(karyawan.status))
            {
                SaveHistoryLogin(AppSource, cleanNpk, "Local auth failed: User inactive", 0, GetIpAddress());
                return Json(new { status = false, status_code = 403, message = "Akun Anda sudah tidak aktif." });
            }

            // Logika pemilihan peran untuk Supervisor (tidak berubah)
            var jabatan = karyawan.pos_name_id?.Trim().ToUpper();
            var roles = new List<string> { "SUPERVISOR" };

            if (!string.IsNullOrEmpty(jabatan) && roles.Contains(jabatan))
            {
                Session["PendingLoginUser"] = karyawan;
                Session["PendingLoginPlant"] = plant;
                Session.Timeout = 5;

                var availableRoles = new List<string> { karyawan.pos_name_id, "Karyawan" };
                return Json(new
                {
                    status = true,
                    status_code = 201,
                    action = "CHOOSE_ROLE",
                    roles = availableRoles
                });
            }
            else
            {
                if (string.IsNullOrEmpty(karyawan.pos_name_id))
                {
                    karyawan.pos_name_id = "Karyawan";
                }
                CreateUserSession(karyawan, plant);
                SaveHistoryLogin(AppSource, cleanNpk, "Login success via Local Auth", 1, GetIpAddress());
                return Json(new { status = true, status_code = 200, action = "REDIRECT" });
            }
        }

        [HttpPost]
        public ActionResult FinalizeLogin(string selectedRole)
        {
            var karyawan = Session["PendingLoginUser"] as EmployeeJson;
            var plant = Session["PendingLoginPlant"] as string;

            if (karyawan == null || string.IsNullOrEmpty(selectedRole))
            {
                return Json(new { status = false, message = "Sesi login tidak valid atau telah kedaluwarsa." });
            }

            // Set the chosen position name for this session only.
            karyawan.pos_name_id = selectedRole;

            CreateUserSession(karyawan, plant);
            SaveHistoryLogin("GS-TRACK-WEB", karyawan.emp_no, $"Login success as {selectedRole}", 1, GetIpAddress());

            Session.Remove("PendingLoginUser");
            Session.Remove("PendingLoginPlant");

            return Json(new { status = true, status_code = 200 });
        }


        #endregion

        #region Helper & Session Methods

        private void CreateUserSession(EmployeeJson karyawan, string selectedPlant)
        {
            SessionLogin session = new SessionLogin
            {
                npk = karyawan.emp_no,
                fullname = karyawan.Full_Name,
                userplant = selectedPlant,
                userdepartment = karyawan.departemen,
                userjabatan = karyawan.pos_name_id,
                login_date = DateTime.Now,
                golongan = karyawan.golongan,
                statusKawin = karyawan.status_kawin,
                createdDate = karyawan.created_date
            };
            Session["SHealth"] = session;
            Session.Timeout = 60;
        }

        /// <summary>
        /// Checks if user is inactive based on status from JSON.
        /// Status '1' is considered active.
        /// </summary>
        private bool IsUserInactive(int status)
        {
            return status != 1;
        }

        private string GetIpAddress()
        {
            return System.Web.HttpContext.Current?.Request.ServerVariables["REMOTE_ADDR"] ?? "UNKNOWN";
        }
        #endregion

        #region External API Functions (Unchanged)
        public bool SaveHistoryLogin(string program, string username, string reason, int status_login, string ip_source)
        {
            Boolean bResult = false;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

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
                    Console.WriteLine("Error in SaveHistoryLogin: " + ex.Message.ToString());
                    return false;
                }

                if (responseFromServer != null)
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(JsonApi_Result)) as JsonApi_Result;
                    if (result != null && result.meta[0].code == 200 && result.meta[0].status == "success")
                    {
                        bResult = true;
                    }
                    else { bResult = false; }
                }
            }
            return bResult;
        }

        public string GenerateToken()
        {
            var sToken = "";
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            var user = ReadFile(0, "C:/tex.txt");
            var pass = ReadFile(1, "C:/tex.txt");
            var grant = ReadFile(2, "C:/tex.txt");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(grant))
            {
                Console.WriteLine("Error reading credential file for token generation.");
                return "";
            }

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
                Console.WriteLine("Error in GenerateToken: " + ex.Message.ToString());
                return "";
            }

            if (responseFromServer != null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(APIModel)) as APIModel;
                if (result != null && !string.IsNullOrEmpty(result.access_token))
                    sToken = result.access_token;
            }
            return sToken;
        }

        public string ReadFile(int urutan, string locdir)
        {
            var sResult = "";
            try
            {
                if (!System.IO.File.Exists(locdir))
                {
                    Console.WriteLine("Credential file not found at: " + locdir);
                    return "";
                }

                using (var sr = new StreamReader(locdir))
                {
                    var text = sr.ReadToEnd();
                    var sVar = text.Split(';');
                    if (sVar.Length > urutan)
                    {
                        sResult = sVar[urutan];
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return sResult;
        }
        #endregion
    }
}