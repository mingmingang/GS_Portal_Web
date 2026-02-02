using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using PagedList;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Data.OleDb;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Web.Routing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Formatting;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ManageController : Controller
    {
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
        public GSDbContext GSDbContext { get; set; }
        // Sesuaikan URL dan Port dengan environment Anda
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker/login";
        //private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api/gstracker/login"; // Production IP

        // Kredensial API
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";


        public GSDbContextGSTrack db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "ari", "123");
        public ManageController()
        {
            if (sessionLogin != null)
            {
                GSDbContext = new GSDbContext("", "", "", "");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ SESSION IS NULL IN MANAGE CONTROLLER!");
                RedirectToAction("Index", "Login");
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (sessionLogin != null)
            {
                GSDbContext.Dispose();
            }
            else
            {
                RedirectToAction("Index", "Login");
            }
        }
        /**
                protected override void OnException(ExceptionContext filterContext)
                {
                    //Do your logging
                    // and redirect / return error view
                    filterContext.ExceptionHandled = true;
                    // If the exception occured in an ajax call. Send a json response back
                    // (you need to parse this and display to user as needed at client side)
                    if (filterContext.HttpContext.Request
        
        s["X-Requested-With"] == "XMLHttpRequest")
                    {
                        filterContext.Result = new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                            Data = new { Error = true, Message = filterContext.Exception.Message }
                        };
                        filterContext.HttpContext.Response.StatusCode = 500; // Set as needed
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Login" }, { "action", "Index" } });
                        //Assuming the view exists in the "~/Views/Shared" folder
                    }
                }
*/
        protected override void OnException(ExceptionContext filterContext)
        {
            // TAMBAHKAN LOGGING DETAIL
            System.Diagnostics.Debug.WriteLine("=== EXCEPTION HANDLER TRIGGERED ===");
            System.Diagnostics.Debug.WriteLine($"Controller: {filterContext.RouteData.Values["controller"]}");
            System.Diagnostics.Debug.WriteLine($"Action: {filterContext.RouteData.Values["action"]}");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {filterContext.Exception.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Exception Message: {filterContext.Exception.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {filterContext.Exception.StackTrace}");

            if (filterContext.Exception.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {filterContext.Exception.InnerException.Message}");
            }

            filterContext.ExceptionHandled = true;

            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { Error = true, Message = filterContext.Exception.Message }
                };
                filterContext.HttpContext.Response.StatusCode = 500;
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "controller", "Login" }, { "action", "Index" } }
                );
            }
        }

        // AREA MANAGE Reimbursement Obat Karyawan
        [SessionCheck]
        public ActionResult ManageReimbursementKaryawan(int? tahun)
        {
            // 1. Buat instance ViewModel. Ini WAJIB dilakukan di awal.
            var viewModel = new ReimbursementViewModel();

            if (sessionLogin != null)
            {
                // Ambil data dari properti objek sessionLogin
                ViewBag.EmployeeGolongan = sessionLogin.golongan;
                ViewBag.EmployeeStatusKawin = sessionLogin.statusKawin;
                ViewBag.ActiveMenu = "Reimbursement"; // Pindahkan ini ke dalam 'if' jika hanya relevan saat login
                viewModel.EmpId = sessionLogin.empid;
                viewModel.EmpNo = sessionLogin.npk;
                viewModel.Plant = sessionLogin.userplant;

                // 2. Isi properti ViewModel, bukan ViewBag.
                //    Pastikan sessionLogin.createdDate adalah tipe DateTime atau DateTime?
                if (sessionLogin.createdDate != null)
                {
                    // Format tanggal ke "yyyy-MM-dd" agar mudah dibaca oleh JavaScript
                    viewModel.KryCreatedDate = ((DateTime)sessionLogin.createdDate).ToString("yyyy-MM-dd");
                }
                else
                {
                    // Jika tanggal null di session, kirim string kosong.
                    viewModel.KryCreatedDate = string.Empty;
                }
            }
            else
            {
                // Jika tidak ada session, mungkin redirect ke halaman login
                // Untuk sekarang, kita pastikan view tetap berjalan dengan tanggal kosong.
                viewModel.KryCreatedDate = string.Empty;
                // return RedirectToAction("Login", "Account"); // <- Contoh redirect
            }

            // 3. Kirim 'viewModel' ke View.
            //    Ini adalah perubahan paling penting. @Model di view sekarang tidak akan null.
            return View(viewModel);
        }

        [SessionCheck] // Pastikan session valid sebelum menampilkan halaman
        public ActionResult ManageAddReimbursement(string type, string reimCode)
        {
            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                // Redirect ke halaman login jika session tidak ada
                return RedirectToAction("Index", "Login"); // Pastikan ini halaman login yang benar
            }

            // 1. Validasi parameter yang masuk
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(reimCode))
            {
                // Jika parameter tidak lengkap, kirim pesan error dan kembali ke halaman utama reimbursement
                TempData["ErrorMessage"] = "Jenis klaim tidak valid atau tidak lengkap. Silakan coba lagi.";
                return RedirectToAction("ManageReimbursementKaryawan"); // Ganti dengan nama action halaman utama reimbursement
            }

            // 2. Kirim SEMUA data yang dibutuhkan oleh View
            ViewBag.ActiveMenu = "Reimbursement";
            ViewBag.Npk = sessionLogin.npk;
            ViewBag.EmpId = sessionLogin.empid;
            ViewBag.NamaKaryawan = sessionLogin.fullname;
            ViewBag.Plant = sessionLogin.userplant; // Pastikan sessionLogin memiliki properti 'userplant'

            // 3. Masukkan parameter dari URL ke ViewBag
            ViewBag.JenisClaimName = type;      // Nama klaim untuk judul & dikirim ke API
            ViewBag.JenisClaimId = reimCode;    // Kode klaim (reim_code) untuk dikirim ke API

            // 4. (Opsional tapi direkomendasikan) Set judul halaman secara dinamis
            ViewBag.Title = $"Tambah Pengajuan: {type}";

            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailReimbursement(long? id)
        {
            // Pengecekan sederhana: jika tidak ada id di query string,
            // halaman tidak bisa dibuka langsung.
            if (!id.HasValue)
            {
                // Opsi 1: Redirect ke halaman daftar dengan pesan error
                TempData["ErrorMessage"] = "Silakan pilih item dari daftar untuk melihat detail.";
                return RedirectToAction("ManageReimbursementKaryawan");

                // Opsi 2: Tampilkan pesan error langsung di view
                // ViewBag.Error = "ID Reimbursement tidak ditemukan.";
            }

            ViewBag.ActiveMenu = "Reimbursement";

            // Simpan ID di ViewBag agar bisa dibaca JS, JIKA diperlukan (tapi kita akan baca dari URL).
            ViewBag.ReimbursementId = id;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageCancelReimbursement(long? id)
        {
            // Pengecekan sederhana: jika tidak ada id di query string,
            // halaman tidak bisa dibuka langsung.
            if (!id.HasValue)
            {
                // Opsi 1: Redirect ke halaman daftar dengan pesan error
                TempData["ErrorMessage"] = "Silakan pilih item dari daftar untuk melihat detail.";
                return RedirectToAction("ManageReimbursementKaryawan");

                // Opsi 2: Tampilkan pesan error langsung di view
                // ViewBag.Error = "ID Reimbursement tidak ditemukan.";
            }

            ViewBag.ActiveMenu = "Reimbursement";

            // Simpan ID di ViewBag agar bisa dibaca JS, JIKA diperlukan (tapi kita akan baca dari URL).
            ViewBag.ReimbursementId = id;

            return View();
        }

        // AREA MANAGE Reimbursement Obat Atasan
        [SessionCheck]
        public ActionResult ManageReimbursementAtasanAndHC2(int? tahun)
        {
            if (sessionLogin != null)
            {
                ViewBag.EmployeeJabatan = sessionLogin.userjabatan;
            }

            ViewBag.ActiveMenu = "Reimbursement";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailReimbursementAtasanAndHC2(long? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Silakan pilih item dari daftar untuk melihat detail.";
                return RedirectToAction("ManageReimbursementAtasanAndHC2");
            }

            if (sessionLogin != null)
            {
                ViewBag.EmployeeJabatan = sessionLogin.userjabatan;
            }

            ViewBag.ActiveMenu = "Reimbursement";

            ViewBag.ReimbursementId = id;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageRejectReimbursement(long? id)
        {
            // Pengecekan sederhana: jika tidak ada id di query string,
            // halaman tidak bisa dibuka langsung.
            if (!id.HasValue)
            {
                // Opsi 1: Redirect ke halaman daftar dengan pesan error
                TempData["ErrorMessage"] = "Silakan pilih item dari daftar untuk melihat detail.";
                return RedirectToAction("ManageReimbursementAtasan");

                // Opsi 2: Tampilkan pesan error langsung di view
                // ViewBag.Error = "ID Reimbursement tidak ditemukan.";
            }

            ViewBag.ActiveMenu = "Reimbursement";

            // Simpan ID di ViewBag agar bisa dibaca JS, JIKA diperlukan (tapi kita akan baca dari URL).
            ViewBag.ReimbursementId = id;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageSuratJaminanKaryawan(int? tahun)
        {
            // 1. Buat instance ViewModel. Ini WAJIB dilakukan di awal.
            var viewModel = new SuratJaminanViewModel();

            if (sessionLogin != null)
            {
                // Ambil data dari properti objek sessionLogin
                ViewBag.EmployeeGolongan = sessionLogin.golongan;
                ViewBag.EmployeeStatusKawin = sessionLogin.statusKawin;
                ViewBag.ActiveMenu = "SuratJaminan"; // Pindahkan ini ke dalam 'if' jika hanya relevan saat login
                viewModel.EmpId = sessionLogin.npk;
                viewModel.Plant = sessionLogin.userplant;

                // 2. Isi properti ViewModel, bukan ViewBag.
                //    Pastikan sessionLogin.createdDate adalah tipe DateTime atau DateTime?
                if (sessionLogin.createdDate != null)
                {
                    // Format tanggal ke "yyyy-MM-dd" agar mudah dibaca oleh JavaScript
                    viewModel.KryCreatedDate = ((DateTime)sessionLogin.createdDate).ToString("yyyy-MM-dd");
                }
                else
                {
                    // Jika tanggal null di session, kirim string kosong.
                    viewModel.KryCreatedDate = string.Empty;
                }
            }
            else
            {
                // Jika tidak ada session, mungkin redirect ke halaman login
                // Untuk sekarang, kita pastikan view tetap berjalan dengan tanggal kosong.
                viewModel.KryCreatedDate = string.Empty;
                // return RedirectToAction("Login", "Account"); // <- Contoh redirect
            }

            // 3. Kirim 'viewModel' ke View.
            //    Ini adalah perubahan paling penting. @Model di view sekarang tidak akan null.
            return View(viewModel);
        }

        [SessionCheck]
        public ActionResult ManageDetailSuratJaminan(int? id)
        {
            // Pengecekan sederhana: jika tidak ada id di query string,
            // halaman tidak bisa dibuka langsung.
            if (!id.HasValue)
            {
                // Opsi 1: Redirect ke halaman daftar dengan pesan error
                TempData["ErrorMessage"] = "Silakan pilih item dari daftar untuk melihat detail.";
                return RedirectToAction("ManageSuratJaminanKaryawan");

                // Opsi 2: Tampilkan pesan error langsung di view
                // ViewBag.Error = "ID Surat Jaminan tidak ditemukan.";
            }

            ViewBag.ActiveMenu = "SuratJaminan";

            // Simpan ID di ViewBag agar bisa dibaca JS, JIKA diperlukan (tapi kita akan baca dari URL).
            ViewBag.SuratJaminanId = id;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageCetakJaminan(int? id)
        {
            // Pengecekan ID wajib ada
            if (!id.HasValue)
            {
                // Redirect ke halaman daftar dengan pesan error
                TempData["ErrorMessage"] = "Silakan pilih item dari daftar untuk mencetak surat jaminan.";
                return RedirectToAction("ManageSuratJaminanKaryawan");
            }

            // Set active menu untuk navigasi
            ViewBag.ActiveMenu = "SuratJaminan";

            // Simpan ID di ViewBag untuk keperluan debugging atau tambahan lainnya
            ViewBag.SuratJaminanId = id;

            // Optional: Validasi apakah user memiliki akses ke surat ini
            // Bisa ditambahkan pengecekan apakah surat ini milik user yang sedang login
            if (sessionLogin != null)
            {
                ViewBag.EmployeeNpk = sessionLogin.npk;
                ViewBag.EmployeeName = sessionLogin.fullname;

                // Bisa ditambahkan log untuk audit trail
                // LogActivity($"User {sessionLogin.npk} mengakses print surat jaminan ID: {id}");
            }

            // Optional: Set data tambahan jika diperlukan
            ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.PageTitle = "Cetak Surat Jaminan";

            return View();
        }

        [SessionCheck] // Pastikan session valid sebelum menampilkan halaman
        public ActionResult ManageAddSuratJaminan()
        {
            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                // Redirect ke halaman login jika session tidak ada
                return RedirectToAction("Index", "Login");
            }

            ViewBag.ActiveMenu = "SuratJaminan";
            ViewBag.Npk = sessionLogin.npk;
            ViewBag.NamaKaryawan = sessionLogin.fullname;

            return View();
        }

        [SessionCheck]
        public ActionResult ManagePreviewJaminan()
        {
            // Set active menu untuk navigasi
            ViewBag.ActiveMenu = "SuratJaminan";

            if (sessionLogin != null)
            {
                ViewBag.EmployeeNpk = sessionLogin.npk;
                ViewBag.EmployeeName = sessionLogin.fullname;

                // FIX: Gunakan dept_name dari session
                ViewBag.EmployeeDept = sessionLogin.userdepartment ?? "-";

                // FIX: Tambahkan Jabatan Asli (Pos Name), bukan Role
                ViewBag.EmployeeJabatan = sessionLogin.pos_name_id ?? sessionLogin.pos_name_en ?? "-";
            }

            // Set data tambahan
            ViewBag.CurrentDate = DateTime.Now.ToString("dd MMMM yyyy"); // Format tanggal dipercantik
            ViewBag.PageTitle = "Preview Surat Jaminan";

            return View();
        }

        // ===================================================================
        // === HALAMAN UTAMA: PERMINTAAN BERKAS KARYAWAN
        // ===================================================================
        [SessionCheck]
        public ActionResult ManagePermintaanBerkasKaryawan()
        {
            try
            {
                // Debug 1: Cek session
                System.Diagnostics.Debug.WriteLine("=== DEBUG START ===");
                System.Diagnostics.Debug.WriteLine($"Session is null: {sessionLogin == null}");

                if (sessionLogin == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Session is NULL!");
                    return RedirectToAction("Index", "Login");
                }

                // Debug 2: Cek properti session
                System.Diagnostics.Debug.WriteLine($"NPK: {sessionLogin.npk}");
                System.Diagnostics.Debug.WriteLine($"Plant: {sessionLogin.plant}");
                System.Diagnostics.Debug.WriteLine($"Fullname: {sessionLogin.fullname}");

                // Pastikan semua ViewBag diisi dengan benar
                ViewBag.ActiveMenu = "PermintaanBerkas";
                ViewBag.Npk = sessionLogin.npk ?? "000000";
                ViewBag.Plant = sessionLogin.plant ?? "K";
                ViewBag.NamaKaryawan = sessionLogin.fullname ?? "Guest";

                System.Diagnostics.Debug.WriteLine("=== DEBUG END - SUCCESS ===");

                return View();
            }
            catch (Exception ex)
            {
                // Log error detail
                System.Diagnostics.Debug.WriteLine($"=== ERROR CAUGHT ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Redirect ke halaman error atau login
                return RedirectToAction("Index", "Login");
            }
        }
        // ===================================================================
        // === HALAMAN ADD PERMINTAAN ID CARD
        // ===================================================================
        [SessionCheck]
        public ActionResult ManageAddPermintaanIdCard()
        {
            try
            {
                if (sessionLogin == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.ActiveMenu = "PermintaanBerkas";
                ViewBag.Npk = sessionLogin.npk ?? "000000";
                ViewBag.Nama = sessionLogin.fullname ?? "Guest";
                ViewBag.Plant = sessionLogin.plant ?? "K";

                System.Diagnostics.Debug.WriteLine("=== ADD PERMINTAAN ID CARD (GET) ===");
                System.Diagnostics.Debug.WriteLine($"NPK: {ViewBag.Npk}");
                System.Diagnostics.Debug.WriteLine($"Nama: {ViewBag.Nama}");
                System.Diagnostics.Debug.WriteLine($"Plant: {ViewBag.Plant}");

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ManageAddPermintaanIdCard (GET): {ex.Message}");
                return RedirectToAction("Index", "Login");
            }
        }

        // ===================================================================
        // === HALAMAN ADD PERMINTAAN SURAT KETERANGAN (GET)
        // ===================================================================
        [SessionCheck]
        public ActionResult ManageAddPermintaanSuratKeterangan()
        {
            try
            {
                if (sessionLogin == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.ActiveMenu = "PermintaanBerkas";
                ViewBag.Npk = sessionLogin.npk ?? "000000";
                ViewBag.Nama = sessionLogin.fullname ?? "Guest";
                ViewBag.Plant = sessionLogin.plant ?? "K";
                ViewBag.Departemen = sessionLogin.userdepartment ?? "IT Department"; // TAMBAHKAN INI
                ViewBag.Jabatan = sessionLogin.userjabatan ?? "Staff"; // TAMBAHKAN INI

                System.Diagnostics.Debug.WriteLine("=== ADD PERMINTAAN SURAT KETERANGAN (GET) ===");
                System.Diagnostics.Debug.WriteLine($"NPK: {ViewBag.Npk}");
                System.Diagnostics.Debug.WriteLine($"Nama: {ViewBag.Nama}");
                System.Diagnostics.Debug.WriteLine($"Plant: {ViewBag.Plant}");
                System.Diagnostics.Debug.WriteLine($"Departemen: {ViewBag.Departemen}");
                System.Diagnostics.Debug.WriteLine($"Jabatan: {ViewBag.Jabatan}");

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ManageAddPermintaanSuratKeterangan (GET): {ex.Message}");
                return RedirectToAction("Index", "Login");
            }
        }

        [SessionCheck]
        public ActionResult ManagePreviewSuratKeterangan(string skpId, string alasanPermintaan, string keterangan, string fromAdd = "false")
        {
            try
            {
                if (sessionLogin == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Ambil role untuk logic tombol Back
                var userRole = (sessionLogin.userjabatan ?? "").Trim().ToLower();

                // --- DATA DASAR ---
                ViewBag.ActiveMenu = "PermintaanBerkas";
                ViewBag.Npk = sessionLogin.npk ?? "000000";
                ViewBag.Nama = sessionLogin.fullname ?? "Guest";
                ViewBag.Plant = sessionLogin.plant ?? "K";

                // --- FIX DEPARTMENT ---
                string realDept = sessionLogin.userdepartment ?? "-";
                ViewBag.EmployeeDept = realDept;
                ViewBag.Departemen = realDept;

                // --- FIX JABATAN ---
                ViewBag.Jabatan = sessionLogin.pos_name_id ?? sessionLogin.pos_name_en ?? "-";

                // --- TAMBAHAN DATA UNTUK SURAT (Dari Session) ---
                ViewBag.StartDate = sessionLogin.start_date ?? "-"; // Mengambil dari session login yang sudah kita fix sebelumnya
                ViewBag.StatusKaryawan = sessionLogin.statusKaryawan ?? "Tetap"; // Default Tetap jika null

                // --- ROLE SYSTEM ---
                ViewBag.UserRole = sessionLogin.userjabatan ?? "Karyawan";

                // --- LOGIC BACK URL ---
                string backUrl;
                if (fromAdd == "true")
                {
                    backUrl = Url.Action("ManageAddPermintaanSuratKeterangan", "Manage");
                }
                else
                {
                    if (userRole == "hc")
                    {
                        backUrl = Url.Action("ManagePermintaanBerkasHC", "Manage");
                    }
                    else
                    {
                        backUrl = Url.Action("ManagePermintaanBerkasKaryawan", "Manage", new { tab = "sk" });
                    }
                }

                ViewBag.BackUrl = backUrl;
                ViewBag.SkpId = skpId;
                ViewBag.AlasanPermintaan = alasanPermintaan;
                ViewBag.Keterangan = keterangan;
                ViewBag.FromAdd = fromAdd;

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Preview Surat Keterangan: {ex.Message}");
                return RedirectToAction("ManagePermintaanBerkasKaryawan", new { tab = "sk" });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetKaryawanDate(string npk, string plant)
        {
            try
            {
                if (string.IsNullOrEmpty(npk) || string.IsNullOrEmpty(plant))
                    return Json(new { status = false, message = "Invalid params" }, JsonRequestBehavior.AllowGet);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
                    client.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);

                    // Panggil API Sunfish yang sudah ada (Cek Login) untuk dapat detail user
                    string apiUrl = $"{SunfishApiBaseUrl}/cek_login_sunfish/{npk}/{plant}";
                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<SunfishAuthResponse>(jsonString);

                        if (result != null && result.Data != null && result.Data.Count > 0)
                        {
                            // Ambil Start Date dari API
                            var userDate = result.Data[0].start_date;
                            return Json(new { status = true, start_date = userDate }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(new { status = false, message = "Data not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ManagePermintaanBerkasHC()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== DEBUG HC VIEW ===");
                System.Diagnostics.Debug.WriteLine($"Accessing HC View");

                if (sessionLogin == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.ActiveMenu = "Permintaan";
                ViewBag.Npk = sessionLogin.npk ?? "000000";
                ViewBag.Plant = sessionLogin.plant ?? "K";
                ViewBag.NamaKaryawan = sessionLogin.fullname ?? "Guest";

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error HC View: {ex.Message}");
                return RedirectToAction("Index", "Login");
            }
        }

        // AREA MANAGE BusinessPlan
        [SessionCheck]
        public ActionResult ManageOvertimeKaryawan()
        {
            ViewBag.ActiveMenu = "Lembur";
            return View();
        }
        
        [SessionCheck]
        public ActionResult ManageOvertimeHc()
        {
            ViewBag.ActiveMenu = "Lembur";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailOverTime()
        {
            ViewBag.ActiveMenu = "Lembur";
            return View();
        }


        // === Untuk halaman IMP
        [SessionCheck]
        public ActionResult ManageAddIMP()
        {
            ViewBag.ActiveMenu = "IzinMeninggalkan";

            // Ambil data dari session
            var session = Session["SHealth"] as SessionLogin;
            if (session != null)
            {
                ViewBag.Npk = session.npk;
                ViewBag.NamaKaryawan = session.fullname;
            }
            else
            {
                // Fallback values jika session tidak tersedia
                ViewBag.Npk = "NPK_TIDAK_DITEMUKAN";
                ViewBag.NamaKaryawan = "Nama Tidak Ditemukan";
            }

            return View();
        }

        [SessionCheck]
        public ActionResult ManageIMPKaryawan()
        {
            ViewBag.ActiveMenu = "IzinMeninggalkan";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailIMP()
        {
            ViewBag.ActiveMenu = "IzinMeninggalkan";
            return View();
        }

        // IMP Atasan dan HC1
        [SessionCheck]
        public ActionResult ManageIMPAtasanAndHC1()
        {
            ViewBag.ActiveMenu = "IzinMeninggalkan";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailIMPAtasanAndHC1(int? id)
        {
            ViewBag.ActiveMenu = "IzinMeninggalkan";

            // Ambil session
            var session = System.Web.HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null)
            {
                // Redirect ke login atau halaman yang sesuai jika session invalid
                return RedirectToAction("Login", "Account");
            }

            // Inject role user ke ViewBag
            var userRole = (session.userjabatan ?? "").ToString();
            ViewBag.UserRole = userRole;

            // Default values
            ViewBag.CanApprove = false;
            ViewBag.ImpId = id;
            ViewBag.ImpStatus = null;

            // Jika id null, tetap render view — front-end akan menolak bila id tidak tersedia
            if (!id.HasValue)
            {
                return View();
            }

            try
            {
                using (var db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "sa", "polman"))
                {
                    var imp = db.gs_track_imp.FirstOrDefault(i => i.imp_id == id.Value);
                    if (imp == null)
                    {
                        // Bila IMP tidak ditemukan, tetap render view (frontend akan menampilkan error)
                        return View();
                    }

                    // normalisasi status & role utk pengecekan
                    var statusNorm = (imp.imp_status ?? "").ToLowerInvariant();
                    var roleNorm = (userRole ?? "").ToLowerInvariant();

                    // Aturan: Atasan -> bila menunggu ; HC1 -> bila belum/verifikasi
                    var canApprove = false;
                    if (roleNorm == "atasan" && statusNorm.Contains("menunggu"))
                    {
                        canApprove = true;
                    }
                    else if (roleNorm == "hc" && (statusNorm.Contains("belum") || statusNorm.Contains("verifikasi")))
                    {
                        canApprove = true;
                    }

                    // Opsional: Anda bisa membuka akses untuk role lain dengan menambah kondisi di sini
                    ViewBag.CanApprove = canApprove;
                    ViewBag.ImpId = imp.imp_id;
                    ViewBag.ImpStatus = imp.imp_status;
                }
            }
            catch (Exception ex)
            {
                // Log error jika perlu
                System.Diagnostics.Debug.WriteLine("Error ManageDetailIMPAtasanAndHC1: " + ex.Message);
                // tetap render view (frontend akan menampilkan pesan error saat memanggil API)
            }

            return View();
        }

        // === Untuk halaman IDL
        [SessionCheck]
        public ActionResult ManageAddIDL()
        {
            ViewBag.ActiveMenu = "IzinDinasLuar";

            // Ambil data dari session
            var session = Session["SHealth"] as SessionLogin;
            if (session != null)
            {
                ViewBag.Npk = session.npk;
                ViewBag.NamaKaryawan = session.fullname;
            }
            else
            {
                // Fallback values jika session tidak tersedia
                ViewBag.Npk = "NPK_TIDAK_DITEMUKAN";
                ViewBag.NamaKaryawan = "Nama Tidak Ditemukan";
            }

            return View();
        }

        [SessionCheck]
        public ActionResult ManageIDLKaryawan()
        {
            ViewBag.ActiveMenu = "IzinDinasLuar";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailIDL()
        {
            ViewBag.ActiveMenu = "IzinDinasLuar";
            return View();
        }

        // IDL Atasan dan GA
        [SessionCheck]
        public ActionResult ManageIDLAtasanAndGA()
        {
            ViewBag.ActiveMenu = "IzinDinasLuar";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailIDLAtasanAndGA(int? id)
        {
            ViewBag.ActiveMenu = "IzinDinasLuar";

            // Ambil session
            var session = System.Web.HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null)
            {
                // Redirect ke login atau halaman yang sesuai jika session invalid
                return RedirectToAction("Login", "Account");
            }

            // Inject role user ke ViewBag
            var userRole = (session.userjabatan ?? "").ToString();
            ViewBag.UserRole = userRole;

            // Default values
            ViewBag.CanApprove = false;
            ViewBag.IdlId = id;
            ViewBag.IdlStatus = null;

            // Jika id null, tetap render view — front-end akan menolak bila id tidak tersedia
            if (!id.HasValue)
            {
                return View();
            }

            try
            {
                // Karena kita menggunakan proxy API dan tidak mengakses database langsung,
                // kita hanya akan mengatur ViewBag untuk role saja.
                // Frontend akan melakukan pengecekan status melalui API

                // Normalisasi role untuk pengecekan
                var roleNorm = (userRole ?? "").ToLowerInvariant();

                // Untuk IDL, alur approval:
                // 1. Atasan -> bisa approve/reject status "Menunggu Persetujuan Atasan"
                // 2. GA -> bisa approve/reject status "Belum Diverifikasi GA"

                // Set flag berdasarkan role
                if (roleNorm == "atasan" || roleNorm.Contains("supervisor") ||
                    roleNorm.Contains("manager") || roleNorm.Contains("lead"))
                {
                    // Atasan: bisa approve jika status "Menunggu Persetujuan Atasan"
                    // Frontend akan cek status via API
                    ViewBag.CanApprove = true;
                    ViewBag.UserRoleType = "atasan";
                }
                else if (roleNorm.Contains("ga") || roleNorm.Contains("general affair") ||
                         roleNorm.Contains("admin") || roleNorm.Contains("administrator"))
                {
                    // GA: bisa approve jika status "Belum Diverifikasi GA"
                    // Frontend akan cek status via API
                    ViewBag.CanApprove = true;
                    ViewBag.UserRoleType = "ga";
                }
                else
                {
                    // Role lainnya tidak bisa approve
                    ViewBag.CanApprove = false;
                    ViewBag.UserRoleType = "viewer";
                }

                ViewBag.IdlId = id;
            }
            catch (Exception ex)
            {
                // Log error jika perlu
                System.Diagnostics.Debug.WriteLine("Error ManageDetailIDLAtasanAndGA: " + ex.Message);
                // tetap render view (frontend akan menampilkan pesan error saat memanggil API)
            }

            return View();
        }

        // AREA MANAGE BusinessPlan
        [SessionCheck]
        public ActionResult ListManageBusinessPlan()
        {
            return View();
        }

        [SessionCheck]
        public ActionResult ManageKehadiranKaryawan()
        {
            ViewBag.ActiveMenu = "Kehadiran";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageKehadiranHC()
        {
            ViewBag.ActiveMenu = "Kehadiran";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageCutiKaryawan()
        {
           
            return View();
        }

        [SessionCheck]
        public ActionResult ManageCutiAtasan()
        {
            ViewBag.ActiveMenu = "Cuti";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageCutiHC1()
        {
            ViewBag.ActiveMenu = "Cuti";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageAddCuti()
        {
            ViewBag.ActiveMenu = "Cuti";
            return View();
        }

        [SessionCheck]
        public ActionResult ManageTambahCuti(string tipe = null, int? sisa = null)
        {
            var logSession = Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
            if (logSession == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Title = "Tambah Cuti";
            ViewBag.Npk = logSession.npk;
            ViewBag.Fullname = logSession.fullname;
            ViewBag.EmpId = logSession.empid;
            ViewBag.SelectedTipeCuti = tipe;
            ViewBag.SisaCuti = sisa ?? 0;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageDetailCuti(string id)
        {
            var session = HttpContext.Session["SHealth"] as SessionLogin;
            if (session == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string backUrl;
            if (session.userjabatan == "Atasan")
            {
                backUrl = Url.Action("ManageCutiAtasan", "Manage"); 
            }
            else
            {
                backUrl = Url.Action("ManageCutiKaryawan", "Manage"); 
            }
            ViewBag.BackUrl = backUrl;

            ViewBag.CutiId = id;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageApprovalCuti(string id)
        {
            // 1. Cek Session
            var session = HttpContext.Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
            if (session == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Validasi ID
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ManageCutiKaryawan", "Manage");
            }

            string backUrl = Url.Action("ManageCutiKaryawan", "Manage");

            ViewBag.BackUrl = backUrl;
            ViewBag.CutiId = id;

            return View();
        }

        [SessionCheck]
        public ActionResult ManageEditCuti(string id,int? sisa = null)
        {
            var session = HttpContext.Session["SHealth"] as SessionLogin;
            if (session == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string backUrl;
            if (session.userjabatan == "Atasan")
            {
                backUrl = Url.Action("ManageCutiAtasan", "Manage");
            }
            else
            {
                backUrl = Url.Action("ManageCutiKaryawan", "Manage");
            }
            ViewBag.BackUrl = backUrl;

            ViewBag.CutiId = id;
            ViewBag.SisaCuti = sisa ?? 0;

            return View();
        }


        public ActionResult ManagePembatalanCuti(string id)
        {
            var session = HttpContext.Session["SHealth"] as SessionLogin;
            if (session == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string backUrl;
            if (session.userjabatan == "Atasan")
            {
                backUrl = Url.Action("ManageCutiAtasan", "Manage");
            }
            else
            {
                backUrl = Url.Action("ManageCutiKaryawan", "Manage");
            }
            ViewBag.BackUrl = backUrl;

            ViewBag.CutiId = id;

            return View();
        }


    }
}