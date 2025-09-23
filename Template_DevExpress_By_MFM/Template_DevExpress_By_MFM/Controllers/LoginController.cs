// File: Controllers/LoginController.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions; // <-- Ditambahkan untuk menggunakan Regex
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class LoginController : Controller
    {
        // ... (Kode lainnya tetap sama) ...

        #region Sunfish API Configuration

        // Ganti URL ini dengan URL tempat Sunfish API Anda berjalan.
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker/login";
        private const string SunfishMasterDataApiUrl = "http://localhost:44320/api/Sunfish";

        // Kredensial API Sunfish.
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // Instance HttpClient yang statis untuk digunakan kembali
        private static readonly HttpClient _httpClient;

        // Static constructor untuk menginisialisasi HttpClient sekali saja.
        static LoginController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
        }
        #endregion

        #region Actions (Index, PostLogin, Logout)
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostLogin(string username, string userpass, string usertype, string plant)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userpass) || string.IsNullOrEmpty(usertype) || string.IsNullOrEmpty(plant))
                {
                    return Json(new { status = false, status_code = 400, message = "Semua field wajib diisi." });
                }

                switch (usertype)
                {
                    case "GS":
                        return HandleGSLogin(username, userpass, plant);
                    case "Local":
                        // NOTE: Password diabaikan saat memanggil HandleLocalLogin karena otentikasi API hanya menggunakan NPK/emp_id.
                        return HandleLocalLogin(username, plant);
                    default:
                        return Json(new { status = false, status_code = 400, message = "Tipe login tidak valid." });
                }
            }
            catch (Exception ex)
            {
                string detailedError = $"NPK: {username}, Plant: {plant}, Error: {ex.Message}";
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

        /// <summary>
        /// Handles local login process by calling Sunfish APIs.
        /// 1. Authenticates user NPK via `cek_login_sunfish` API.
        /// 2. Fetches detailed employee data via `getListEmp` API.
        /// 3. Creates user session and handles role selection for supervisors.
        /// </summary>
        private ActionResult HandleLocalLogin(string npkInput, string plant)
        {
            const string AppSource = "GS-REIMBURSE-APP";
            string cleanNpk = npkInput?.Trim() ?? string.Empty;

            // --- 1. OTENTIKASI ---
            // (Panggilan ke API cek_login_sunfish dan validasi hasilnya tetap sama)
            string authApiUrl = $"{SunfishApiBaseUrl}/cek_login_sunfish/{cleanNpk}/{plant}";
            // ... Panggil API dan dapatkan authData & meta ...
            var authResponse = _httpClient.GetAsync(authApiUrl).Result;
            var authContent = authResponse.Content.ReadAsStringAsync().Result;
            var authResult = JsonConvert.DeserializeObject<SunfishAuthResponse>(authContent);
            var authData = authResult?.Data?.FirstOrDefault();
            var meta = authResult?.Meta?.FirstOrDefault();

            if (authData == null || meta?.Code != 200)
            {
                string apiErrorMessage = meta?.Message ?? "Kredensial tidak valid.";
                SaveHistoryLogin(AppSource, cleanNpk, $"Sunfish auth failed: {apiErrorMessage}", 0, GetIpAddress());
                return Json(new { status = false, status_code = 404, message = apiErrorMessage });
            }

            // --- 2. AMBIL DATA DETAIL ---
            var detailApiUrl = $"{SunfishMasterDataApiUrl}/getListEmp";
            // ... Panggil API dan dapatkan allEmployeesResponse ...
            var detailResponse = _httpClient.GetAsync(detailApiUrl).Result;
            var detailContent = detailResponse.Content.ReadAsStringAsync().Result;
            var allEmployeesResponse = JsonConvert.DeserializeObject<SunfishEmployeeListResponse>(detailContent);

            // ============================ PERBAIKAN DI SINI ============================
            // Kita tetap menggunakan emp_id yang unik untuk mencari, karena ini paling andal.
            var employeeDetail = allEmployeesResponse?.Data?.FirstOrDefault(e =>
                e.emp_id?.Trim().Equals(authData.emp_id, StringComparison.OrdinalIgnoreCase) == true
            );

            // HAPUS BLOK IF BERIKUT:
            // Blok pengecekan silang plant DIHAPUS karena kita memutuskan untuk
            // mempercayai hasil dari API otentikasi sebagai sumber kebenaran utama.
            // if (employeeDetail != null && employeeDetail.work_location?.Trim() != plant.Trim())
            // {
            //     // ... kode error sinkronisasi kritis ...
            // }
            // ===========================================================================

            if (employeeDetail == null)
            {
                // Kondisi ini tetap penting. Artinya API otentikasi menemukan user, 
                // tapi user tersebut tidak ada di daftar master getListEmp.
                SaveHistoryLogin(AppSource, cleanNpk, "Auth success, but emp_id from auth response was not found in getListEmp.", 0, GetIpAddress());
                return Json(new { status = false, status_code = 404, message = "Otentikasi berhasil, namun data master karyawan tidak sinkron." });
            }

            // --- 3. PROSES SESSION (tidak ada perubahan) ---
            var jabatan = employeeDetail.position?.Trim().ToUpper();
            var supervisorRoles = new List<string> { "SUPERVISOR", "SECTION HEAD" };

            if (!string.IsNullOrEmpty(jabatan) && supervisorRoles.Contains(jabatan))
            {
                Session["PendingLoginDetail"] = employeeDetail;
                Session["PendingLoginAuth"] = authData;
                Session["PendingLoginPlant"] = plant; // Tetap gunakan plant yang diinput user
                Session.Timeout = 5;
                var availableRoles = new List<string> { employeeDetail.position, "Karyawan" };
                return Json(new { status = true, status_code = 201, action = "CHOOSE_ROLE", roles = availableRoles });
            }
            else
            {
                if (string.IsNullOrEmpty(employeeDetail.position))
                {
                    employeeDetail.position = "Karyawan";
                }
                CreateUserSession(employeeDetail, authData, plant); // Tetap gunakan plant yang diinput user
                SaveHistoryLogin(AppSource, authData.emp_no, "Login success via Sunfish API", 1, GetIpAddress());
                return Json(new { status = true, status_code = 200, action = "REDIRECT" });
            }
        }

        [HttpPost]
        public ActionResult FinalizeLogin(string selectedRole)
        {
            // ... (logika ini tetap sama)
            var employeeDetail = Session["PendingLoginDetail"] as SunfishEmployeeDetail;
            var authData = Session["PendingLoginAuth"] as SunfishAuthData;
            var plant = Session["PendingLoginPlant"] as string;

            if (employeeDetail == null || authData == null || string.IsNullOrEmpty(selectedRole) || plant == null)
            {
                return Json(new { status = false, message = "Sesi login tidak valid atau telah kedaluwarsa." });
            }

            employeeDetail.position = selectedRole;
            CreateUserSession(employeeDetail, authData, plant);

            // *** PERUBAHAN LOGGING: Gunakan emp_id dari data auth yang valid ***
            SaveHistoryLogin("GS-REIMBURSE-APP", authData.emp_id, $"Login success as {selectedRole}", 1, GetIpAddress());

            Session.Remove("PendingLoginDetail");
            Session.Remove("PendingLoginAuth");
            Session.Remove("PendingLoginPlant");

            return Json(new { status = true, status_code = 200 });
        }

        #endregion

        #region Helper & Session Methods

        private void CreateUserSession(SunfishEmployeeDetail employeeDetail, SunfishAuthData authData, string plant)
        {
            int? parsedGolongan = null;
            if (!string.IsNullOrEmpty(employeeDetail.grade_category))
            {
                Match match = Regex.Match(employeeDetail.grade_category, @"\d+$");
                if (match.Success && int.TryParse(match.Value, out int gol))
                {
                    parsedGolongan = gol;
                }
            }

            SessionLogin session = new SessionLogin
            {
                npk = authData.emp_id,
                fullname = employeeDetail.full_name,
                userplant = plant,
                userdepartment = employeeDetail.department_name,
                userjabatan = employeeDetail.position,
                login_date = DateTime.Now,
                golongan = parsedGolongan,
                statusKawin = (employeeDetail.marital_status == 1) ? "Kawin" : "Lajang",
                createdDate = employeeDetail.start_date,

                company_id = authData.company_id,
                phone = authData.phone,
                photo = authData.photo,
                pos_level = authData.pos_level
            };

            Session["SHealth"] = session;
            Session.Timeout = 60;
        }

        private string GetIpAddress()
        {
            return System.Web.HttpContext.Current?.Request.ServerVariables["REMOTE_ADDR"] ?? "UNKNOWN";
        }
        #endregion

        // ... (Region External API Functions (Unchanged) tetap sama) ...
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