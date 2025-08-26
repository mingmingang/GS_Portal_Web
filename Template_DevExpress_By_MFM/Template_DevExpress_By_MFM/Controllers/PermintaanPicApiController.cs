using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class PermintaanPicApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public PermintaanPicApiController()
        {
            try
            {
                db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "sa", "polman");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                throw new Exception("Tidak dapat terhubung ke database.", ex);
            }
        }

        private string GenerateNextPicId()
        {
            const string prefix = "PIC";
            var lastRequest = db.gs_track_pic
                .Where(p => p.pic_id.StartsWith(prefix))
                .OrderByDescending(p => p.pic_id)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastRequest != null)
            {
                string numericPart = lastRequest.pic_id.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"{prefix}{nextNumber:D3}";
        }

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] PermintaanPicModel newRequest)
        {
            try
            {
                if (newRequest == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data permintaan yang dikirim tidak valid atau kosong.");
                }

                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());
                }

                if (string.IsNullOrEmpty(newRequest.pic_ah))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Alasan pengajuan wajib diisi.");
                }

                newRequest.pic_id = GenerateNextPicId();
                newRequest.kry_npk = GetCurrentUserNpk();
                newRequest.pic_status = "Belum Diverifikasi";
                newRequest.pic_crea_date = DateTime.Now;
                newRequest.pic_crea_by = GetCurrentUserNpk();

                db.gs_track_pic.Add(newRequest);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, newRequest);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // File: PermintaanPicApiController.cs

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int? year = null, string statuses = null)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid atau NPK tidak ditemukan.");
                }

                // [PERBAIKAN] Ambil jabatan pengguna dari sesi
                string userJabatan = sessionLogin.userjabatan;
                string userNpk = sessionLogin.npk;

                // Mulai query dasar
                var query = from pic in db.gs_track_pic
                            join karyawan in db.TlkpKaryawans on pic.kry_npk equals karyawan.kry_npk into gj
                            from subKaryawan in gj.DefaultIfEmpty()
                            select new
                            {
                                pic.pic_id,
                                pic.kry_npk,
                                kry_nama_karyawan = subKaryawan == null ? "N/A" : subKaryawan.kry_nama_karyawan,
                                pic.pic_ah,
                                pic.pic_status,
                                pic.pic_crea_date
                            };

                // [PERBAIKAN] Terapkan filter NPK hanya jika pengguna adalah Karyawan
                if (userJabatan?.Equals("Karyawan", StringComparison.OrdinalIgnoreCase) == true)
                {
                    query = query.Where(p => p.kry_npk == userNpk);
                }
                // Jika bukan Karyawan (misal: HC1), maka tidak ada filter NPK, sehingga semua data akan diambil.

                // Terapkan filter TAHUN jika ada
                if (year.HasValue)
                {
                    query = query.Where(p => p.pic_crea_date.HasValue && p.pic_crea_date.Value.Year == year.Value);
                }

                // Terapkan filter STATUS jika ada
                if (!string.IsNullOrEmpty(statuses) && statuses != "[]")
                {
                    var statusList = JsonConvert.DeserializeObject<List<string>>(statuses);
                    if (statusList != null && statusList.Any())
                    {
                        query = query.Where(p => statusList.Contains(p.pic_status));
                    }
                }

                return Request.CreateResponse(DataSourceLoader.Load(query, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // File: PermintaanPicApiController.cs

        [SessionCheck]
        [HttpGet]
        [Route("api/PermintaanPicApi/GetCounts")]
        public HttpResponseMessage GetCounts()
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid atau NPK tidak ditemukan.");
                }

                // [PERBAIKAN] Ambil jabatan dan NPK dari sesi
                string userJabatan = sessionLogin.userjabatan;
                string userNpk = sessionLogin.npk;

                // [PERBAIKAN] Buat query dasar yang bisa dimodifikasi
                IQueryable<PermintaanPicModel> query = db.gs_track_pic;

                // [PERBAIKAN] Jika pengguna adalah Karyawan, filter berdasarkan NPK mereka.
                // Jika bukan (misal: HC1), jangan filter, sehingga semua data akan dihitung.
                if (userJabatan?.Equals("Karyawan", StringComparison.OrdinalIgnoreCase) == true)
                {
                    query = query.Where(p => p.kry_npk == userNpk);
                }

                // Hitung status dari query yang sudah difilter (atau tidak difilter untuk HC1)
                var counts = new
                {
                    selesai = query.Count(p => p.pic_status == "Selesai"),
                    ditolak = query.Count(p => p.pic_status == "Ditolak"),
                    belum_diverifikasi = query.Count(p => p.pic_status == "Belum Diverifikasi")
                };

                return Request.CreateResponse(HttpStatusCode.OK, counts);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key");
                var entity = db.gs_track_pic.FirstOrDefault(p => p.pic_id == key);
                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var values = form.Get("values");
                JsonConvert.PopulateObject(values, entity);

                entity.pic_modi_date = DateTime.Now;
                entity.pic_modi_by = GetCurrentUserNpk();

                Validate(entity);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string GetCurrentUserNpk()
        {
            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
            return sessionLogin?.npk ?? "SYSTEM";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}