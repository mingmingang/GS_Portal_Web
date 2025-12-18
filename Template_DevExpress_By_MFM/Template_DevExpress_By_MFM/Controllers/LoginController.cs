using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils; // Pastikan namespace Utils ada jika menggunakan helper lain

namespace Template_DevExpress_By_MFM.Controllers
{
    public class LoginController : Controller
    {
        #region 1. Sunfish API Configuration

        // Sesuaikan URL dan Port dengan environment Anda
        //private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker/login";
        private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api/gstracker/login"; // Production IP

        // Kredensial API
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // Instance HttpClient Static (Best Practice)
        private static readonly HttpClient _httpClient;

        static LoginController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Tambahkan Header Client ID & Secret
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
        }

        #endregion

        #region 2. Main Actions (Index, PostLogin, Logout)

        public ActionResult Index()
        {
            // Cek jika sudah login, redirect ke Dashboard/Home
            if (Session["SHealth"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult PostLogin(string username, string userpass, string usertype, string plant)
        {
            try
            {
                // Validasi Input Dasar
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userpass) || string.IsNullOrEmpty(usertype) || string.IsNullOrEmpty(plant))
                {
                    return Json(new { status = false, status_code = 400, message = "Semua field wajib diisi." });
                }

                switch (usertype)
                {
                    case "GS":
                        // Placeholder untuk Login LDAP/AD Masa Depan
                        return Json(new { status = false, message = "Login via LDAP/AD belum diaktifkan." }, JsonRequestBehavior.AllowGet);

                    case "Local":
                        // Login menggunakan NPK ke Sunfish API
                        return HandleLocalLogin(username, plant);

                    default:
                        return Json(new { status = false, status_code = 400, message = "Tipe login tidak valid." });
                }
            }
            catch (Exception ex)
            {
                string detailedError = $"NPK: {username}, Plant: {plant}, Error: {ex.Message}";
                if (ex.InnerException != null) detailedError += $" | Inner: {ex.InnerException.Message}";

                System.Diagnostics.Debug.WriteLine($"LOGIN EXCEPTION: {detailedError}");
                return Json(new { status = false, status_code = 500, message = detailedError }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Logout()
        {
            var npk = (Session["SHealth"] as SessionLogin)?.npk ?? "Unknown User";

            // Hapus Session
            Session.Clear();
            Session.Abandon();

            // Hapus Cookie Session ASP.NET
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-10);
            }

            SaveHistoryLogin("GS-REIMBURSE-APP", npk, "Logout success", 1, GetIpAddress());
            return RedirectToAction("Index", "Login");
        }

        #endregion

        #region 3. Login Logic Handlers (Updated)

        /// <summary>
        /// Menangani login lokal dengan memanggil API cek_login_sunfish.
        /// </summary>
        private ActionResult HandleLocalLogin(string npkInput, string plant)
        {
            const string AppSource = "GS-REIMBURSE-APP";
            string cleanNpk = npkInput?.Trim() ?? string.Empty;

            // A. Validasi Plant Code
            var validPlants = new[] { "J", "K", "S" }; // Jakarta, Karawang, Sunter
            if (!validPlants.Contains(plant))
            {
                return Json(new { status = false, status_code = 400, message = $"Plant code '{plant}' tidak valid. Harus J, K, atau S." });
            }

            // B. Call API Authentication
            string authApiUrl = $"{SunfishApiBaseUrl}/cek_login_sunfish/{cleanNpk}/{plant}";
            SunfishAuthResponse authResult = null;

            try
            {
                var authResponse = _httpClient.GetAsync(authApiUrl).Result;

                if (!authResponse.IsSuccessStatusCode)
                {
                    SaveHistoryLogin(AppSource, cleanNpk, $"API Error: {authResponse.StatusCode}", 0, GetIpAddress());
                    return Json(new { status = false, status_code = 500, message = "Gagal terhubung ke server otentikasi." });
                }

                var authContent = authResponse.Content.ReadAsStringAsync().Result;
                authResult = JsonConvert.DeserializeObject<SunfishAuthResponse>(authContent);
            }
            catch (Exception ex)
            {
                SaveHistoryLogin(AppSource, cleanNpk, $"Exception: {ex.Message}", 0, GetIpAddress());
                return Json(new { status = false, status_code = 500, message = "Terjadi kesalahan sistem saat menghubungi API." });
            }

            // C. Validasi Data Hasil API
            var authData = authResult?.Data?.FirstOrDefault();
            var meta = authResult?.Meta?.FirstOrDefault();

            if (authData == null || meta?.Code != 200)
            {
                string apiErrorMessage = meta?.Message ?? "NPK tidak ditemukan atau status tidak aktif.";
                SaveHistoryLogin(AppSource, cleanNpk, $"Login Failed: {apiErrorMessage}", 0, GetIpAddress());
                return Json(new { status = false, status_code = 404, message = apiErrorMessage });
            }

            // D. Logic Penentuan Role (GA/HC/Atasan/Karyawan)
            var availableRoles = new List<string>();

            if (authData.role_options != null && authData.role_options.Count > 0)
            {
                foreach (var roleString in authData.role_options)
                {
                    // --- FIX HERE: Memecah string jika backend mengirim "Karyawan, HC" ---
                    if (roleString.Contains(","))
                    {
                        var splitRoles = roleString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var r in splitRoles)
                        {
                            availableRoles.Add(r.Trim());
                        }
                    }
                    else
                    {
                        availableRoles.Add(roleString);
                    }
                }
            }

            // Tambahkan "Karyawan" sebagai fallback jika belum ada
            if (!availableRoles.Contains("Karyawan"))
            {
                availableRoles.Add("Karyawan");
            }

            // Hapus duplikat
            availableRoles = availableRoles.Distinct().ToList();

            // Cek apakah user perlu memilih role? 
            bool needsRoleSelection = availableRoles.Count > 1;

            if (needsRoleSelection)
            {
                // Simpan state login di Session sementara
                Session["PendingLoginAuth"] = authData;
                Session["PendingLoginPlant"] = plant;
                Session.Timeout = 5; // 5 Menit waktu memilih role

                return Json(new
                {
                    status = true,
                    status_code = 201, // 201: Created/Prompt Selection
                    action = "CHOOSE_ROLE",
                    roles = availableRoles,
                    message = "Silakan pilih role login Anda"
                });
            }
            else
            {
                // Single Role: Langsung Login
                string primaryRole = availableRoles.FirstOrDefault() ?? "Karyawan";

                // Buat Session Aplikasi
                CreateUserSession(authData, plant, primaryRole, availableRoles);
                SaveHistoryLogin(AppSource, authData.emp_no, $"Login success as {primaryRole}", 1, GetIpAddress());

                return Json(new
                {
                    status = true,
                    status_code = 200,
                    action = "REDIRECT",
                    message = "Login berhasil."
                });
            }
        }

        /// <summary>
        /// Dipanggil ketika user memilih role dari Pop-up (jika role > 1).
        /// </summary>
        [HttpPost]
        public ActionResult FinalizeLogin(string selectedRole)
        {
            // Ambil data dari temporary session
            var authData = Session["PendingLoginAuth"] as SunfishAuthData;
            var plant = Session["PendingLoginPlant"] as string;

            if (authData == null || string.IsNullOrEmpty(selectedRole) || plant == null)
            {
                return Json(new { status = false, message = "Sesi login kedaluwarsa. Silakan login ulang." });
            }

            // Re-build available roles logic
            var availableRoles = new List<string>();
            if (authData.role_options != null)
            {
                // FIX JUGA DI SINI AGAR KONSISTEN
                foreach (var roleString in authData.role_options)
                {
                    if (roleString.Contains(","))
                    {
                        var splitRoles = roleString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var r in splitRoles) availableRoles.Add(r.Trim());
                    }
                    else
                    {
                        availableRoles.Add(roleString);
                    }
                }
            }
            if (!availableRoles.Contains("Karyawan")) availableRoles.Add("Karyawan");

            // Validasi: Apakah role yang dipilih valid untuk user ini?
            if (!availableRoles.Contains(selectedRole))
            {
                return Json(new { status = false, message = "Role yang dipilih tidak valid." });
            }

            // Create Final Session
            CreateUserSession(authData, plant, selectedRole, availableRoles);

            // Log Success
            SaveHistoryLogin("GS-REIMBURSE-APP", authData.emp_no, $"Login success as {selectedRole}", 1, GetIpAddress());

            // Bersihkan temporary session
            Session.Remove("PendingLoginAuth");
            Session.Remove("PendingLoginPlant");

            return Json(new
            {
                status = true,
                status_code = 200,
                message = $"Login berhasil sebagai {selectedRole}"
            });
        }

        #endregion

        #region 4. Helper & Session Methods (CreateUserSession Updated)

        private void CreateUserSession(
            SunfishAuthData authData,
            string plant,
            string selectedRole,
            List<string> availableRoles)
        {
            // 1. Parse Golongan dari Grade Code (e.g., "1F" -> 1)
            int? parsedGolongan = null;
            if (!string.IsNullOrEmpty(authData.grade_code))
            {
                Match match = Regex.Match(authData.grade_code, @"^\d+");
                if (match.Success && int.TryParse(match.Value, out int gol))
                {
                    parsedGolongan = gol;
                }
            }

            // 2. Parse Status Kawin (1 = Kawin, 0 = Lajang/Unknown)
            string statusKawin = "Lajang";
            if (authData.maritalstatus.HasValue && authData.maritalstatus.Value == 1)
            {
                statusKawin = "Kawin";
            }

            // 3. Mapping ke Session Object
            SessionLogin session = new SessionLogin
            {
                // Identitas Karyawan
                empid = authData.emp_id,
                npk = authData.emp_no,
                fullname = authData.full_name,

                // Data Organisasi & Lokasi
                userplant = plant,
                plant = plant,
                userdepartment = authData.dept_name,
                dept_code = authData.dept_code,
                dept_id = authData.dept_id,
                company_id = authData.company_id ?? 0,

                // Data Jabatan & Role
                userjabatan = selectedRole,
                pos_name_id = authData.pos_name_id,
                pos_name_en = authData.pos_name_en,
                pos_level = authData.pos_level ?? 0,

                // Role Management
                selectedRole = selectedRole,
                availableRoles = availableRoles,

                // Data Personal Lainnya
                login_date = DateTime.Now,
                golongan = parsedGolongan,
                statusKawin = statusKawin,
                createdDate = authData.created_date ?? DateTime.MinValue,
                phone = authData.phone,
                photo = authData.photo,

                // Default Values
                userrole = selectedRole
            };

            // Simpan ke Session ASP.NET
            Session["SHealth"] = session;
            Session.Timeout = 60; // Session timeout 60 menit

            System.Diagnostics.Debug.WriteLine($"[SESSION CREATED] NPK: {session.npk} | Dept: {session.userdepartment} | Role: {session.userjabatan}");
        }

        private string GetIpAddress()
        {
            return System.Web.HttpContext.Current?.Request.ServerVariables["REMOTE_ADDR"] ?? "UNKNOWN";
        }

        #endregion

        #region 5. External API Functions (Boilerplate / Legacy)

        public bool SaveHistoryLogin(string program, string username, string reason, int status_login, string ip_source)
        {
            Boolean bResult = false;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                var token = GenerateToken();
                var clientID = ReadFile(5, "C:/tex.txt");
                var clientSecret = ReadFile(6, "C:/tex.txt");

                if (!string.IsNullOrEmpty(token))
                {
                    string url_api = "https://gs-api.gs.astra.co.id/api/log/last_login";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_api);
                    myReq.Method = "POST";
                    myReq.ContentType = "application/x-www-form-urlencoded";
                    myReq.Headers.Add("Authorization", ("Bearer " + token));
                    myReq.Headers.Add("clientid", clientID);
                    myReq.Headers.Add("clientsecret", clientSecret);

                    string myData = "program=" + HttpUtility.UrlEncode(program) +
                                    "&username=" + HttpUtility.UrlEncode(username) +
                                    "&reason=" + HttpUtility.UrlEncode(reason) +
                                    "&status_login=" + HttpUtility.UrlEncode(status_login.ToString()) +
                                    "&ip_source=" + HttpUtility.UrlEncode(ip_source);

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
                            string responseFromServer = reader.ReadToEnd();
                            if (responseFromServer.Contains("\"code\":200")) bResult = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SaveHistoryLogin: " + ex.Message);
                return false;
            }
            return bResult;
        }

        public string GenerateToken()
        {
            var sToken = "";
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                var user = ReadFile(0, "C:/tex.txt");
                var pass = ReadFile(1, "C:/tex.txt");
                var grant = ReadFile(2, "C:/tex.txt");

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) return "";

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                string url_api = "https://gs-api.gs.astra.co.id/generate-token";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_api);
                myReq.Method = "POST";
                myReq.ContentType = "application/x-www-form-urlencoded";
                string myData = "username=" + HttpUtility.UrlEncode(user) + "&password=" + HttpUtility.UrlEncode(pass) + "&grant_type=" + HttpUtility.UrlEncode(grant);

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
                        string responseFromServer = reader.ReadToEnd();

                        var definition = new { access_token = "" };
                        var result = JsonConvert.DeserializeAnonymousType(responseFromServer, definition);
                        if (result != null) sToken = result.access_token;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GenerateToken: " + ex.Message);
                return "";
            }
            return sToken;
        }

        public string ReadFile(int urutan, string locdir)
        {
            var sResult = "";
            try
            {
                if (!System.IO.File.Exists(locdir)) return "";

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
                Console.WriteLine(e.Message);
            }
            return sResult;
        }
        #endregion
    }
}