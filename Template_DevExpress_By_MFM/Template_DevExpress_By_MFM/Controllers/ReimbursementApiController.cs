using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ReimbursementApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public ReimbursementApiController()
        {
            try
            {
                db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "azet", "123");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                throw new Exception("Tidak dapat terhubung ke database.", ex);
            }
        }

        // GET: api/Reimbursement
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int? year, string statuses = null)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid atau NPK kosong");
                }

                string sessionNpk = sessionLogin.npk;

                int selectedYear = year ?? DateTime.Now.Year;
                var startDate = new DateTime(selectedYear, 1, 1);
                var endDate = startDate.AddYears(1);

                List<string> statusList = new List<string>();
                if (!string.IsNullOrEmpty(statuses))
                {
                    try
                    {
                        statusList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(statuses);
                    }
                    catch { /* biarkan kosong jika parsing gagal */ }
                }

                // Step 1: Query data mentah dengan semua kolom yang dibutuhkan
                var rawQuery = from rbm in db.gs_track_reimbursement
                               join kry in db.TlkpKaryawans
                                   on rbm.KryNpk equals kry.kry_npk into kry_join
                               from kry in kry_join.DefaultIfEmpty()

                               join dgs in db.gs_track_diagnosa
                                   on rbm.DgsId equals dgs.DgsId into dgs_join
                               from dgs in dgs_join.DefaultIfEmpty()

                               join org in db.gs_track_orang
                                   on rbm.OrgId equals org.OrgId into org_join
                               from org in org_join.DefaultIfEmpty()

                               where rbm.RbmTanggalMulai >= startDate
                                       && rbm.RbmTanggalMulai < endDate
                                       && rbm.KryNpk == sessionNpk
                               // [FIX 1] Melengkapi semua kolom yang akan ditampilkan atau dibutuhkan
                               select new
                               {
                                   // Kolom dari tabel reimbursement (rbm)
                                   rbm.RbmId,
                                   rbm.KryNpk,
                                   rbm.RbmTanggalMulai,
                                   rbm.RbmTanggalSelesai,
                                   rbm.RbmTipe,
                                   rbm.OrgId,
                                   rbm.DgsId,
                                   rbm.RsId,
                                   rbm.RbmCost,
                                   rbm.RbmStatusSubmit,
                                   rbm.RbmAlasanPembatalan,
                                   rbm.RbmCreatedBy,
                                   rbm.RbmCreatedDate,
                                   rbm.RbmModifyBy,
                                   rbm.RbmModifyDate,

                                   // Kolom dari join, dengan penanganan jika null
                                   NamaKaryawan = kry != null ? kry.kry_nama_karyawan : "N/A",
                                   StatusKawin = kry != null ? kry.kry_status_kawin : "N/A", // Contoh default "N/A"
                                   NamaDiagnosa = dgs != null ? dgs.DgsNama : "N/A",
                                   NamaPasien = org != null ? org.OrgNama : (kry != null ? kry.kry_nama_karyawan : "N/A"),
                                   HubunganPasien = org != null ? org.OrgHubungan : "Anda" // Contoh default
                               };

                if (statusList.Any())
                {
                    rawQuery = rawQuery.Where(r => statusList.Contains(r.RbmStatusSubmit));
                }

                var allDataForYear = rawQuery.ToList();

                // Step 2: Hitung Ringkasan (Tetap sama, sudah benar)
                var summary = new ReimbursementSummary();
                var summaryCalculation = allDataForYear
                    .GroupBy(item => item.RbmTipe)
                    .Select(g => new {
                        Tipe = g.Key,
                        Digunakan = g.Where(i => i.RbmStatusSubmit == "Disetujui").Sum(i => i.RbmCost ?? 0),
                        Unrealize = g.Where(i => i.RbmStatusSubmit == "Menunggu Persetujuan" || i.RbmStatusSubmit == "Belum Diverifikasi").Sum(i => i.RbmCost ?? 0)
                    }).ToList();

                foreach (var calc in summaryCalculation)
                {
                    // ... (logika switch case tetap sama, tidak perlu diubah)
                    switch (calc.Tipe)
                    {
                        case "Rawat Jalan":
                            summary.RawatJalanDigunakan = calc.Digunakan;
                            summary.RawatJalanUnrealize = calc.Unrealize;
                            break;
                        case "Rawat Inap":
                            summary.RawatInapDigunakan = calc.Digunakan;
                            summary.RawatInapUnrealize = calc.Unrealize;
                            break;
                        case "Maternity":
                            summary.MaternityDigunakan = calc.Digunakan;
                            summary.MaternityUnrealize = calc.Unrealize;
                            break;
                        case "KB":
                            summary.KbDigunakan = calc.Digunakan;
                            summary.KbUnrealize = calc.Unrealize;
                            break;
                    }
                }

                // Step 3: Mapping ke model dengan semua properti yang sudah diambil
                var modelList = allDataForYear.Select(item => new ReimbursementModel
                {
                    // [FIX 2] Melengkapi mapping ke ReimbursementModel
                    RbmId = item.RbmId,
                    KryNpk = item.KryNpk,
                    RbmTanggalMulai = item.RbmTanggalMulai,
                    RbmTanggalSelesai = item.RbmTanggalSelesai,
                    RbmTipe = item.RbmTipe,
                    OrgId = item.OrgId,
                    DgsId = item.DgsId,
                    RsId = item.RsId,
                    RbmCost = item.RbmCost,
                    RbmStatusSubmit = item.RbmStatusSubmit,
                    RbmAlasanPembatalan = item.RbmAlasanPembatalan,
                    RbmCreatedBy = item.RbmCreatedBy,
                    RbmCreatedDate = item.RbmCreatedDate,
                    RbmModifyBy = item.RbmModifyBy,
                    RbmModifyDate = item.RbmModifyDate,
                    NamaKaryawan = item.NamaKaryawan,
                    StatusKawin = item.StatusKawin,
                    NamaDiagnosa = item.NamaDiagnosa,
                    NamaPasien = item.NamaPasien,
                    HubunganPasien = item.HubunganPasien,
                    durasi = GetDurasi(item.RbmTanggalMulai, item.RbmTanggalSelesai)
                });

                // Step 4: Pakai DataSourceLoader pada data yang sudah di-map
                var loadResultForGrid = DataSourceLoader.Load(modelList, loadOptions);

                // [FIX 3] Mengembalikan objek ReimbursementLoadResult yang berisi data grid DAN summary
                var finalResult = new ReimbursementLoadResult
                {
                    data = loadResultForGrid.data,
                    totalCount = loadResultForGrid.totalCount,
                    summary = summary
                };

                //var finalResult = new
                //{
                //    data = loadResultForGrid.data,
                //    totalCount = loadResultForGrid.totalCount,
                //    summary = summary, // ini summary custom kamu
                //    groupCount = loadResultForGrid.groupCount
                //};

                return Request.CreateResponse(HttpStatusCode.OK, finalResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        private string GetDurasi(DateTime mulai, DateTime? selesai)
        {
            if (selesai.HasValue && selesai.Value.Date >= mulai.Date)
            {
                int days = (selesai.Value.Date - mulai.Date).Days + 1;
                return $"{days} hari";
            }
            return "1 hari";
        }

        // KOREKSI 3: Pindahkan #region untuk mengelompokkan semua endpoint dropdown
        #region Dropdown Data Sources

        // POST: api/Reimbursement
        [HttpGet]
        [Route("api/reimbursement/GetPasien")]
        public HttpResponseMessage GetPasien()
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (string.IsNullOrEmpty(sessionLogin?.npk))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid.");
                }

                var npk = sessionLogin.npk;

                // 1. Ambil data diri sendiri
                var self = new { OrgId = 0, OrgNama = sessionLogin.fullname, OrgHubungan = "Anda" };

                // 2. Ambil data keluarga dari tabel gs_track_orang
                var keluarga = db.gs_track_orang
                                 .Where(o => o.KryNpk == npk)
                                 .Select(o => new { o.OrgId, o.OrgNama, o.OrgHubungan })
                                 .ToList();

                // 3. Gabungkan
                var result = new List<object> { self };
                result.AddRange(keluarga);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/reimbursement/GetDiagnosa")]
        public HttpResponseMessage GetDiagnosa()
        {
            try
            {
                var result = db.gs_track_diagnosa
                               .Select(d => new { d.DgsId, d.DgsNama })
                               .OrderBy(d => d.DgsNama)
                               .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/reimbursement/getRumahSakit")]
        public HttpResponseMessage GetRumahSakit(string tipe)
        {
            try
            {
                if (string.IsNullOrEmpty(tipe))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<object>());
                }

                // Pastikan nama tabel (db.gs_track_rumah_sakit) dan nama kolom (rs_tipe, dll)
                // sudah sesuai dengan definisi di DbContext dan class model Anda.
                var result = db.gs_track_rumah_sakit
                               .Where(rs => rs.rs_tipe == tipe)
                               .Select(rs => new {
                                   RsId = rs.rs_id,       // <-- PERBAIKAN: Beri nama properti 'RsId'
                                   RsNama = rs.rs_nama    // <-- PERBAIKAN: Beri nama properti 'RsNama'
                               })
                               .OrderBy(rs => rs.RsNama) // <-- Urutkan berdasarkan properti baru
                               .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        [SessionCheck]
        [HttpGet]
        [Route("api/reimbursement/generateno")]
        public HttpResponseMessage GenerateNo()
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired, silakan login ulang.");
                }

                long newId = GenerateNoPengajuan(sessionLogin.npk);
                return Request.CreateResponse(HttpStatusCode.OK, new { RbmId = newId });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Permintaan harus berupa multipart/form-data.");
            }

            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired, silakan login ulang.");
                }

                var provider = new MultipartMemoryStreamProvider();
                Request.Content.ReadAsMultipartAsync(provider).Wait();

                var formValues = provider.Contents
                    .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "values");

                if (formValues == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data form ('values') tidak ditemukan.");
                }

                var jsonValues = formValues.ReadAsStringAsync().Result;

                // Deserialize ke DTO atau model sementara
                var model = JsonConvert.DeserializeObject<ReimbursementModel>(jsonValues);

                if (model.RbmId <= 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No. Pengajuan tidak valid atau hilang dari form.");
                }

                // --- PERUBAHAN UTAMA: CARI DAN UPDATE ---

                // 1. Cari record DRAFT yang sudah dibuat sebelumnya berdasarkan ID
                var entityToUpdate = db.gs_track_reimbursement.Find(model.RbmId);

                if (entityToUpdate == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Sesi pengajuan tidak ditemukan. Mungkin halaman terlalu lama dibuka. Silakan muat ulang halaman.");
                }

                // Pastikan NPK pengaju sama dengan NPK pemilik draft
                if (entityToUpdate.KryNpk != sessionLogin.npk)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Akses tidak diizinkan.");
                }

                // 2. Update record tersebut dengan semua data lengkap dari form
                entityToUpdate.RbmTipe = model.RbmTipe;
                entityToUpdate.OrgId = model.OrgId == 0 ? (int?)null : model.OrgId; // Handle jika OrgId 0 dari frontend berarti 'diri sendiri'
                entityToUpdate.RbmCost = model.RbmCost;
                entityToUpdate.RsId = model.RsId;
                entityToUpdate.DgsId = model.DgsId;
                entityToUpdate.RbmTanggalMulai = model.RbmTanggalMulai;
                entityToUpdate.RbmTanggalSelesai = model.RbmTanggalSelesai;
                // ... mapping properti lain seperti Dokter, dll. jika ada ...

                // 3. Ubah statusnya dari "DRAFT" menjadi status awal yang valid
                entityToUpdate.RbmStatusSubmit = "Menunggu Persetujuan";

                // 4. Set data modifikasi (opsional tapi praktik yang baik)
                entityToUpdate.RbmModifyBy = sessionLogin.npk;
                entityToUpdate.RbmModifyDate = DateTime.Now;

                // Proses file upload disini (jika ada) dan simpan path ke 'entityToUpdate'

                db.SaveChanges(); // Simpan perubahan ke DB (ini adalah operasi UPDATE)

                // Kembalikan model lengkap yang sudah di-update agar bisa digunakan di notifikasi success
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        private long GenerateNoPengajuan(string npk)
        {
            DateTime today = DateTime.Now;
            string prefix = $"{npk}{today:yyMMdd}";  // contoh: 5081756250816

            long minRange = long.Parse(prefix + "00");
            long maxRange = long.Parse(prefix + "99");

            var lastData = db.gs_track_reimbursement
                .Where(r => r.RbmId >= minRange && r.RbmId <= maxRange)
                .OrderByDescending(r => r.RbmId)
                .FirstOrDefault();

            int nextIncrement = 1;
            if (lastData != null)
            {
                string lastNo = lastData.RbmId.ToString();
                string lastIncrementStr = lastNo.Substring(lastNo.Length - 2);
                if (int.TryParse(lastIncrementStr, out int lastIncrement))
                {
                    nextIncrement = lastIncrement + 1;
                }
            }

            string newIdStr = $"{prefix}{nextIncrement:00}";
            return long.Parse(newIdStr);
        }

        //// PUT: api/Reimbursement
        //[SessionCheck]
        //[HttpPut]
        //public HttpResponseMessage Put(FormDataCollection form)
        //{
        //    try
        //    {
        //        var key = form.Get("key"); // string
        //        var entity = db.gs_track_reimbursement.FirstOrDefault(c => c.reimbursement_id == key);

        //        if (entity == null)
        //            return Request.CreateResponse(HttpStatusCode.NotFound);

        //        var values = form.Get("values");
        //        JsonConvert.PopulateObject(values, entity);

        //        Validate(entity);
        //        if (!ModelState.IsValid)
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

        //        db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.OK, entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //// DELETE: api/Reimbursement
        //[SessionCheck]
        //[HttpDelete]
        //public HttpResponseMessage Delete(FormDataCollection form)
        //{
        //    try
        //    {
        //        var key = form.Get("key"); // string
        //        var entity = db.gs_track_reimbursement.FirstOrDefault(c => c.reimbursement_id == key);

        //        if (entity == null)
        //            return Request.CreateResponse(HttpStatusCode.NotFound);

        //        db.gs_track_reimbursement.Remove(entity);
        //        db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}
    }
}
