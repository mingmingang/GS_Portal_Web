using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using LinqKit;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;


namespace Template_DevExpress_By_MFM.Controllers
{
    public class IMPApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public IMPApiController()
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

        // GET: api/IMP/Summary - Endpoint baru untuk ringkasan status IMP
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/Summary")]
        public HttpResponseMessage GetSummary(string npk = null)
        {
            try
            {
                var session = System.Web.HttpContext.Current.Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
                string userRole = session?.userjabatan;

                // Query data IMP
                IQueryable<IMPModel> query = db.gs_track_imp;

                // Filter berdasarkan NPK hanya untuk user biasa
                if (userRole != "Atasan" && userRole != "HC1")
                {
                    if (string.IsNullOrWhiteSpace(npk))
                    {
                        npk = session?.npk;
                    }

                    if (!string.IsNullOrWhiteSpace(npk))
                    {
                        query = query.Where(i => i.imp_npk == npk);
                    }
                }

                var dataList = query.AsEnumerable();

                // Hitung jumlah berdasarkan status (menggunakan logika yang sama dengan frontend)
                int disetujui = dataList.Count(i =>
                    !string.IsNullOrEmpty(i.imp_status) &&
                    System.Text.RegularExpressions.Regex.IsMatch(i.imp_status, "^(.*(selesai|disetujui|terlaksana).*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                int ditolak = dataList.Count(i =>
                    !string.IsNullOrEmpty(i.imp_status) &&
                    System.Text.RegularExpressions.Regex.IsMatch(i.imp_status, "^(.*(ditolak).*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                int menunggu = dataList.Count(i =>
                    !string.IsNullOrEmpty(i.imp_status) &&
                    System.Text.RegularExpressions.Regex.IsMatch(i.imp_status, "^(.*(menunggu).*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                int verifikasi = dataList.Count(i =>
                    !string.IsNullOrEmpty(i.imp_status) &&
                    System.Text.RegularExpressions.Regex.IsMatch(i.imp_status, "^(.*(belum|verifikasi).*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                // Kembalikan hasil ringkasan
                var summary = new
                {
                    disetujui = disetujui,
                    ditolak = ditolak,
                    menunggu = menunggu,
                    verifikasi = verifikasi
                };

                return Request.CreateResponse(HttpStatusCode.OK, summary);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // GET: api/IMP - Endpoint yang sudah ada dengan filter status
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string npk = null, string status = null)
        {
            try
            {
                var session = System.Web.HttpContext.Current.Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
                string userRole = session?.userjabatan;

                // Query dasar
                IQueryable<IMPModel> query = db.gs_track_imp;

                // Filter berdasarkan NPK hanya untuk user biasa
                if (userRole != "Atasan" && userRole != "HC1")
                {
                    // Jika bukan Atasan atau HC1, gunakan npk dari session
                    if (string.IsNullOrWhiteSpace(npk))
                    {
                        npk = session?.npk;
                    }

                    if (!string.IsNullOrWhiteSpace(npk))
                    {
                        query = query.Where(i => i.imp_npk == npk);
                    }
                }

                // Filter berdasarkan status (jika ada parameter status)
                if (!string.IsNullOrWhiteSpace(status))
                {
                    // Split status jika multiple (dipisah koma)
                    var statusList = status.Split(',').Select(s => s.Trim()).ToList();

                    // Buat ekspresi OR untuk semua status yang dipilih
                    var predicate = PredicateBuilder.False<IMPModel>();

                    foreach (var statusItem in statusList)
                    {
                        var currentStatus = statusItem;
                        predicate = predicate.Or(i =>
                            (currentStatus == "Disetujui" && (i.imp_status.Contains("disetujui") || i.imp_status.Contains("selesai") || i.imp_status.Contains("terlaksana"))) ||
                            (currentStatus == "Menunggu Persetujuan" && i.imp_status.Contains("menunggu")) ||
                            (currentStatus == "Belum Diverifikasi" && (i.imp_status.Contains("belum") || i.imp_status.Contains("verifikasi"))) ||
                            (currentStatus == "Ditolak" && i.imp_status.Contains("ditolak")) ||
                            // Fallback: jika tidak cocok dengan kategori di atas, gunakan exact match
                            i.imp_status.Equals(currentStatus, StringComparison.OrdinalIgnoreCase)
                        );
                    }

                    query = query.Where(predicate);
                }

                var dataList = query
                    .AsEnumerable()
                    .Select(i => new IMPModel
                    {
                        imp_id = i.imp_id,
                        imp_no_request = i.imp_no_request,
                        imp_npk = i.imp_npk,
                        imp_jenis_kegiatan = i.imp_jenis_kegiatan,
                        imp_tanggal_berangkat = i.imp_tanggal_berangkat,
                        imp_waktu_berangkat = i.imp_waktu_berangkat,
                        imp_tanggal_kembali = i.imp_tanggal_kembali,
                        imp_waktu_kembali = i.imp_waktu_kembali,
                        imp_lokasi = i.imp_lokasi,
                        imp_keterangan = i.imp_keterangan,
                        imp_berkas_lampiran = i.imp_berkas_lampiran,
                        imp_status = i.imp_status,
                        imp_created_by = i.imp_created_by,
                        imp_created_date = i.imp_created_date,
                        imp_modif_by = i.imp_modif_by,
                        imp_modif_date = i.imp_modif_date,
                        imp_berangkat_aktual = i.imp_berangkat_aktual,
                        imp_kembali_aktual = i.imp_kembali_aktual,
                        imp_alasan_penolakan = i.imp_alasan_penolakan
                    });

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // GET: api/IMPApi/{id} - Endpoint untuk mengambil detail IMP berdasarkan ID
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/{id}")]
        public HttpResponseMessage GetDetail(int id)
        {
            try
            {
                var imp = db.gs_track_imp.FirstOrDefault(i => i.imp_id == id);

                if (imp == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data IMP tidak ditemukan.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, imp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/IMP
        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var imp = new IMPModel();

                JsonConvert.PopulateObject(values, imp);

                Validate(imp);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                imp.imp_status = "Menunggu Persetujuan";
                imp.imp_created_date = DateTime.Now;

                db.gs_track_imp.Add(imp);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, imp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // PUT: api/IMP
        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                var key = Convert.ToInt32(form.Get("key")); // imp_id adalah int
                var entity = db.gs_track_imp.FirstOrDefault(i => i.imp_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var values = form.Get("values");
                JsonConvert.PopulateObject(values, entity);

                Validate(entity);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                entity.imp_modif_date = DateTime.Now;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // DELETE: api/IMP
        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                var key = Convert.ToInt32(form.Get("key")); // imp_id adalah int
                var entity = db.gs_track_imp.FirstOrDefault(i => i.imp_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                db.gs_track_imp.Remove(entity);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // GET: api/IMPApi/DownloadAttachment/{id}
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/DownloadAttachment/{id}")]
        public HttpResponseMessage DownloadAttachment(int id)
        {
            try
            {
                var imp = db.gs_track_imp.FirstOrDefault(i => i.imp_id == id);

                if (imp == null || string.IsNullOrEmpty(imp.imp_berkas_lampiran))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Lampiran tidak ditemukan.");
                }

                // Check if it's a base64 data URL
                if (imp.imp_berkas_lampiran.StartsWith("data:image/"))
                {
                    // Extract the base64 data and content type
                    var base64Data = imp.imp_berkas_lampiran.Split(',')[1];
                    var contentType = imp.imp_berkas_lampiran.Split(';')[0].Split(':')[1];

                    // Convert base64 to byte array
                    var bytes = Convert.FromBase64String(base64Data);

                    // Create response
                    var result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new ByteArrayContent(bytes);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = $"lampiran_imp_{id}.{contentType.Split('/')[1]}"
                        };

                    return result;
                }
                else
                {
                    // Handle regular file URLs
                    var result = new HttpResponseMessage(HttpStatusCode.Redirect);
                    result.Headers.Location = new Uri(imp.imp_berkas_lampiran);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/IMPApi/ApproveIMP/{id}
        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/ApproveIMP/{id}")]
        public HttpResponseMessage ApproveIMP(int id, [FromBody] ApproveRejectRequest request)
        {
            try
            {
                var session = System.Web.HttpContext.Current.Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
                if (session == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var imp = db.gs_track_imp.FirstOrDefault(i => i.imp_id == id);
                if (imp == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data IMP tidak ditemukan");

                // Update status berdasarkan role
                imp.imp_status = request.Status;
                imp.imp_modif_by = session.fullname;
                imp.imp_modif_date = DateTime.Now;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { message = "IMP berhasil disetujui" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/IMPApi/RejectIMP/{id}
        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/RejectIMP/{id}")]
        public HttpResponseMessage RejectIMP(int id, [FromBody] RejectRequest request)
        {
            try
            {
                var session = System.Web.HttpContext.Current.Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
                if (session == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var imp = db.gs_track_imp.FirstOrDefault(i => i.imp_id == id);
                if (imp == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data IMP tidak ditemukan");

                // Update status dan alasan penolakan
                imp.imp_status = "Ditolak";
                imp.imp_modif_by = session.fullname;
                imp.imp_modif_date = DateTime.Now;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { message = "IMP berhasil ditolak" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Model untuk request
        public class ApproveRejectRequest
        {
            public string ApprovedBy { get; set; }
            public string Status { get; set; }
        }

        public class RejectRequest
        {
            public string RejectedBy { get; set; }
            public string Alasan { get; set; }
        }
    }
}