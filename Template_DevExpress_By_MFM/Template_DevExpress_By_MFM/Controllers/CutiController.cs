using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class CutiController : Controller
    {
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        [SessionCheck]
        public ActionResult Index(int page = 1)
        {
            int pageSize = 10;
            var data = GetDummyCutiList();

            int totalData = data.Count;
            var pagedData = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalData = totalData;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.ActiveMenu = "Cuti";

            return View(pagedData);
        }


        [SessionCheck]
        public ActionResult Cuti(int page = 1)
        {
            int pageSize = 10;

            List<CutiModel> data = new List<CutiModel>();
            var random = new Random();
            var statusOptions = new[] { "Disetujui", "Menunggu Persetujuan", "Ditolak" };
            var tipeCutiOptions = new[] { "Cuti Pribadi", "Cuti Besar" };
            var permohonanOptions = new[]
            {
        "Ingin liburan",
        "Healing Time",
        "Liburan ke kampung",
        "Jalan-jalan",
        "Pergi Ke Jawa",
        "Urusan Keluarga",
        "Izin Cuti Ingin Ke Rumah",
        "Istirahat",
        "Recovery kesehatan",
        "Acara keluarga"
    };

            var startDate = new DateTime(2025, 5, 12);

            for (int i = 1; i <= 50; i++)
            {
                var daysToAdd = i * 2;
                var cutiDate = startDate.AddDays(daysToAdd % 30);

                data.Add(new CutiModel
                {
                    NoPengajuan = $"LY20250023{i.ToString().PadLeft(2, '0')}",
                    Permohonan = permohonanOptions[random.Next(permohonanOptions.Length)],
                    TipeCuti = tipeCutiOptions[random.Next(tipeCutiOptions.Length)],
                    TanggalCuti = cutiDate,
                    Durasi = "1 hari",
                    Status = statusOptions[random.Next(statusOptions.Length)],
                    Pembatalan = "🌠️"
                });
            }

            // Total data & data untuk halaman saat ini
            int totalData = data.Count;
            var pagedData = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalData = totalData;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.ActiveMenu = "Cuti";

            return View(pagedData);
        }

        [SessionCheck]
        public ActionResult AddCuti()
        {
            var model = new CutiModel
            {
                NoPengajuan = "LVR" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            ViewBag.ActiveMenu = "Cuti";
            return View(model);
        }


        [SessionCheck]
        public ActionResult Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var data = GetDummyCutiList();
            var cuti = data.FirstOrDefault(c => c.NoPengajuan == id);

            if (cuti == null)
                return HttpNotFound();
            ViewBag.ActiveMenu = "Cuti";
            return View(cuti);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCuti(CutiModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Hitung durasi otomatis
                    var diff = (model.TanggalSelesai - model.TanggalMulai).Days + 1;
                    model.Durasi = diff + " hari";
                    model.Status = "Menunggu Persetujuan";

                    // Simpan ke database atau list
                    // ...

                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data");
                }
            }
            ViewBag.ActiveMenu = "Cuti";
            return View(model);
        }

        private List<CutiModel> GetDummyCutiList()
        {
            var data = new List<CutiModel>();
            var random = new Random();
            var statusOptions = new[] { "Disetujui", "Menunggu Persetujuan", "Ditolak" };
            var tipeCutiOptions = new[] { "Cuti Pribadi", "Cuti Besar" };
            var permohonanOptions = new[]
            {
        "Ingin liburan", "Healing Time", "Liburan ke kampung",
        "Jalan-jalan", "Pergi Ke Jawa", "Urusan Keluarga",
        "Izin Cuti Ingin Ke Rumah", "Istirahat", "Recovery kesehatan", "Acara keluarga"
    };

            var startDate = new DateTime(2025, 5, 12);

            for (int i = 1; i <= 50; i++)
            {
                var daysToAdd = i * 2;
                var cutiDate = startDate.AddDays(daysToAdd % 30);

                data.Add(new CutiModel
                {
                    NoPengajuan = $"LY20250023{i.ToString().PadLeft(2, '0')}",
                    Permohonan = permohonanOptions[random.Next(permohonanOptions.Length)],
                    TipeCuti = tipeCutiOptions[random.Next(tipeCutiOptions.Length)],
                    TanggalCuti = cutiDate,
                    Durasi = "1 hari",
                    Status = statusOptions[random.Next(statusOptions.Length)],
                    Pembatalan = ""
                });
            }

            return data;
        }

        // GET: /Cuti/PembatalanCuti/LY2025002301
        public ActionResult PembatalanCuti(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dummyData = GetDummyCutiList();
            var cuti = dummyData.FirstOrDefault(c => c.NoPengajuan == id);

            if (cuti == null)
            {
                return HttpNotFound();
            }

            cuti.RangeTanggalCuti = $"{cuti.TanggalCuti:dd MMM yyyy} - {cuti.TanggalCuti.AddDays(1):dd MMM yyyy}";

            // Simulasikan list tanggal cuti
            ViewBag.TanggalList = new List<DateTime>
    {
        new DateTime(2025, 5, 19),
        new DateTime(2025, 5, 20),
        new DateTime(2025, 5, 21)
    };
            ViewBag.ActiveMenu = "Cuti";
            return View(cuti);
        }

        // POST: /Cuti/PembatalanCuti
        [HttpPost]
        public ActionResult PembatalanCuti(CutiModel model, string Alasan, string[] TanggalDipilih)
        {
            if (ModelState.IsValid)
            {
                // Simulasi proses
                System.Diagnostics.Debug.WriteLine("NoPengajuan: " + model.NoPengajuan);
                System.Diagnostics.Debug.WriteLine("Alasan: " + Alasan);
                System.Diagnostics.Debug.WriteLine("Tanggal yang dibatalkan: " + string.Join(", ", TanggalDipilih ?? new string[0]));

                // Simulasi redirect
                return RedirectToAction("Index");
            }

            // Re-populate dummy tanggal jika error
            ViewBag.TanggalList = new List<DateTime>
    {
        new DateTime(2025, 5, 19),
        new DateTime(2025, 5, 20),
        new DateTime(2025, 5, 21)
    };
            ViewBag.ActiveMenu = "Cuti";
            return View(model);
        }

        [SessionCheck]
        public ActionResult Atasan(int page = 1)
        {
            int pageSize = 10;

            // Ambil semua data dummy
            var data = GetDummyCutiList();

            // Filter hanya cuti yang masih menunggu persetujuan (Diproses)
            var filteredData = data.Where(c => c.Status == "Menunggu Persetujuan").ToList();

            int totalData = filteredData.Count;

            // Pagination
            var pagedData = filteredData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Untuk tampilan info halaman
            ViewBag.Total = totalData;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.From = ((page - 1) * pageSize) + 1;
            ViewBag.To = Math.Min(page * pageSize, totalData);
            ViewBag.ActiveMenu = "Cuti";

            return View(pagedData);
        }

        [SessionCheck]
        public ActionResult Approval(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var data = GetDummyCutiList();
            var cuti = data.FirstOrDefault(c => c.NoPengajuan == id);
            ViewBag.ActiveMenu = "Cuti";
            return View(cuti);
        }

        [HttpPost]
        [SessionCheck]
        public ActionResult Setujui(int id)
        {
            // Simulasi update data status
            // TODO: Update ke DB
            TempData["Success"] = "Cuti berhasil disetujui.";
            return RedirectToAction("AtasanCuti");
        }

        [HttpPost]
        [SessionCheck]
        public ActionResult Tolak(int id)
        {
            // Simulasi update data status
            // TODO: Update ke DB
            TempData["Error"] = "Cuti telah ditolak.";
            return RedirectToAction("AtasanCuti");
        }





    }
}