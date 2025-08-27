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

        public GSDbContextGSTrack db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "sa", "aangaang");
        public ManageController()
        {
            if (sessionLogin != null)
            {

                GSDbContext = new GSDbContext("", "", "", "");
            }
            else
            {
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

        protected override void OnException(ExceptionContext filterContext)
        {
            //Do your logging
            // and redirect / return error view
            filterContext.ExceptionHandled = true;
            // If the exception occured in an ajax call. Send a json response back
            // (you need to parse this and display to user as needed at client side)
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
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

        // AREA MANAGE Reimbursement Obat Karyawan
        [SessionCheck]
        public ActionResult ManageReimbursementKaryawan(int? tahun)
        {
            if (sessionLogin != null)
            {
                // Ambil data dari properti objek sessionLogin dan masukkan ke ViewBag
                ViewBag.EmployeeGolongan = sessionLogin.golongan;
                ViewBag.EmployeeStatusKawin = sessionLogin.statusKawin; // atau StatusKawin
                ViewBag.EmployeeCreatedDate = sessionLogin.createdDate; // atau CreatedDate
            }

            ViewBag.ActiveMenu = "Reimbursement";
            return View();
        }

        [SessionCheck] // Pastikan session valid sebelum menampilkan halaman
        public ActionResult ManageAddReimbursement()
        {
            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                // Redirect ke halaman login jika session tidak ada
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Npk = sessionLogin.npk;
            ViewBag.NamaKaryawan = sessionLogin.fullname;

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
                return RedirectToAction("ManageReimbursementAtasan");
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

        // AREA MANAGE BusinessPlan
        [SessionCheck]
        public ActionResult ListManageBusinessPlan()
        {
            return View();
        }

        [SessionCheck]
        public ActionResult ManageCutiKaryawan()
        {
            ViewBag.ActiveMenu = "Cuti";
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
            var model = new CutiModel
            {
                cuti_id = "LVR" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                status = "Menunggu Persetujuan",
                tanggal_pengajuan = DateTime.Now
            };
            return View(model);
        }



        public ActionResult ManageDetailCuti(string id)
        {
            // 1. Ambil sesi dan periksa apakah pengguna sudah login
            var session = HttpContext.Session["SHealth"] as SessionLogin;
            if (session == null)
            {
                // Jika tidak ada sesi, arahkan ke halaman login
                return RedirectToAction("Login", "Account"); // Sesuaikan dengan halaman login Anda
            }

            // 2. Validasi ID (kode asli Anda)
            if (string.IsNullOrEmpty(id))
            {
                // Menggunakan BadRequest lebih sesuai untuk parameter yang hilang
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 3. Logika untuk menentukan URL "Kembali" berdasarkan role
            string backUrl;
            // PENTING: Sesuaikan `session.role` dan `"Atasan"` dengan properti dan nilai di kelas SessionLogin Anda
            if (session.userjabatan == "Atasan")
            {
                // Halaman daftar cuti yang perlu persetujuan (untuk Atasan)
                backUrl = Url.Action("ManageCutiAtasan", "Manage");
            }
            else
            {
                // Halaman riwayat pengajuan cuti pribadi (untuk Karyawan)
                backUrl = Url.Action("ManageCutiKaryawan", "Manage");
            }

            // 4. Kirim URL yang sudah ditentukan ke View
            ViewBag.BackUrl = backUrl;

            // 5. Ambil data cuti dari database (kode asli Anda)
            var cuti = db.gs_track_cuti.FirstOrDefault(c => c.cuti_id == id);
            if (cuti == null)
            {
                return HttpNotFound();
            }

            // 6. Kirim model 'cuti' dan ViewBag ke View
            return View(cuti);
        }

        public ActionResult ManagePembatalanCuti(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var cuti = db.gs_track_cuti.FirstOrDefault(c => c.cuti_id == id);
            if (cuti == null)
                return HttpNotFound();

            return View(cuti); 
        }


    }
}