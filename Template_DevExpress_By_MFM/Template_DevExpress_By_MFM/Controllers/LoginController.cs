// File: Controllers/LoginController.cs

using System;
using System.DirectoryServices; // Diaktifkan kembali untuk LDAP
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class LoginController : Controller
    {
        private readonly GSDbContextGSTrack db;

        public LoginController()
        {
            //db = new GSDbContextGSTrack(@"DESKTOP-GLBR43I", "DB_GSTRACK", "azet", "123");

            try
            {
                // LANGKAH 1: Langsung coba koneksi di konstruktor
                db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "sa", "aangaang");
                //db.Database.Connection.Open(); // Coba buka koneksi
                //db.Database.Connection.Close(); // Langsung tutup lagi jika berhasil
            }
            catch (Exception ex)
            {
                // Jika koneksi gagal, langsung catat errornya
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                // Kita tidak bisa melanjutkan jika DB gagal, jadi lempar error agar aplikasi tahu ada masalah serius.
                throw new Exception("Tidak dapat terhubung ke database.", ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db?.Dispose(); }
            base.Dispose(disposing);
        }

        #region Actions (Index, PostLogin, Logout)
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        // PERUBAHAN 1: Mengembalikan parameter usertype dan userplant
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
                // Buat pesan error yang lengkap
                string detailedError = $"NPK: {username}, Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    detailedError += $" | Inner Exception: {ex.InnerException.Message}";
                }

                System.Diagnostics.Debug.WriteLine($"LOGIN EXCEPTION: {detailedError}");

                // Kirim error detail ke response (sementara, untuk debugging)
                return Json(new
                {
                    status = false,
                    status_code = 500,
                    message = detailedError
                }, JsonRequestBehavior.AllowGet);
            
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
            // (Logika ini belum kita sentuh, asumsikan sudah benar jika diperlukan)
            // ... kode HandleGSLogin seperti sebelumnya ...
            // Penting untuk meneruskan 'plant' ke CreateUserSession
            // CreateUserSession(karyawan, plant); 
            // return Json(...)
            return Json(new { status = false, message = "Login LDAP belum diimplementasikan sepenuhnya" }, JsonRequestBehavior.AllowGet);
        }

        private ActionResult HandleLocalLogin(string npkInput, string passwordInput, string plant)
        {
            const string AppSource = "GS-TRACK-WEB";
            const string EncryptionKey = "bangcakrek";

            string cleanNpk = npkInput?.Trim() ?? string.Empty;

            // Cek input kosong
            if (string.IsNullOrWhiteSpace(cleanNpk) || string.IsNullOrWhiteSpace(passwordInput))
            {
                return Json(new { status = false, status_code = 400, message = "NPK dan Password harus diisi." }, JsonRequestBehavior.AllowGet);
            }

            // Enkripsi password
            string encryptedPassword;
            try
            {
                encryptedPassword = Helper.EncodePassword(passwordInput, EncryptionKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Encryption failed for NPK {cleanNpk}: {ex.Message}");
                throw new Exception("Proses enkripsi gagal.", ex);
            }

            // Cari user di database (hybrid password: encrypted atau plain)
            var karyawan = db.TlkpKaryawans.FirstOrDefault(k =>
                k.kry_npk.Trim().Equals(cleanNpk, StringComparison.OrdinalIgnoreCase) &&
                (k.kry_password == encryptedPassword || k.kry_password == passwordInput)
            );

            if (karyawan == null)
            {
                SaveHistoryLogin(AppSource, cleanNpk, "Local auth failed: Invalid NPK/Pass", 0, GetIpAddress());
                return Json(new { status = false, status_code = 404, message = "NPK atau Password salah." }, JsonRequestBehavior.AllowGet);
            }

            // Cek status user
            if (IsUserInactive(karyawan.kry_status))
            {
                SaveHistoryLogin(AppSource, cleanNpk, "Local auth failed: User inactive", 0, GetIpAddress());
                return Json(new { status = false, status_code = 403, message = "Akun Anda sudah tidak aktif." }, JsonRequestBehavior.AllowGet);
            }

            // Upgrade password ke format enkripsi baru jika masih plain
            if (karyawan.kry_password == passwordInput)
            {
                try
                {
                    karyawan.kry_password = encryptedPassword;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Password upgrade failed for NPK {cleanNpk}: {ex.Message}");
                }
            }

            // Buat session user + log sukses
            CreateUserSession(karyawan, plant);
            SaveHistoryLogin(AppSource, cleanNpk, "Login success via Local Auth", 1, GetIpAddress());

            return Json(new { status = true, status_code = 200 }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Helper & Session Methods

        // PERUBAHAN 3: Metode ini sekarang menerima plant yang dipilih user
        private void CreateUserSession(TlkpKaryawan karyawan, string selectedPlant)
        {
            SessionLogin session = new SessionLogin
            {
                npk = karyawan.kry_npk,
                fullname = karyawan.kry_nama_karyawan,
                userplant = selectedPlant, // Menggunakan plant dari form, bukan dari DB karyawan
                userdepartment = karyawan.kry_departemen,
                userjabatan = karyawan.kry_jabatan,
                //golongan = karyawan.kry_golongan,
                //status_kawin = karyawan.kry_status_kawin,
                login_date = DateTime.Now
            };
            Session["SHealth"] = session;
            Session.Timeout = 60;
        }

        private bool IsUserInactive(string status)
        {
            return !status.Equals("Aktif", StringComparison.OrdinalIgnoreCase);
        }

        private string GetIpAddress()
        {
            return System.Web.HttpContext.Current?.Request.ServerVariables["REMOTE_ADDR"] ?? "UNKNOWN";
        }
        #endregion

        #region External API Functions (Tidak Ada Perubahan Disini)
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