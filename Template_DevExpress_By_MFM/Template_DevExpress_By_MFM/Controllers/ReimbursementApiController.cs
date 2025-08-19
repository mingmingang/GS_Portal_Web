using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetDetail/{id}")]
        public HttpResponseMessage GetDetail(long id)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid atau NPK kosong.");
                }
                string sessionNpk = sessionLogin.npk;

                // TAHAP 1: Ambil data dari database ke dalam objek anonim
                var rawData = (from rbm in db.gs_track_reimbursement
                               join kry in db.TlkpKaryawans
                                   on rbm.KryNpk equals kry.kry_npk into kry_join
                               from kry in kry_join.DefaultIfEmpty()

                               join dgs in db.gs_track_diagnosa
                                   on rbm.DgsId equals dgs.DgsId into dgs_join
                               from dgs in dgs_join.DefaultIfEmpty()

                               join org in db.gs_track_orang
                                   on rbm.OrgId equals org.OrgId into org_join
                               from org in org_join.DefaultIfEmpty()

                               join rs in db.gs_track_rumah_sakit
                                   on rbm.RsId equals rs.rs_id into rs_join
                               from rs in rs_join.DefaultIfEmpty()

                               where rbm.RbmId == id && rbm.KryNpk == sessionNpk
                               select new // <--- Ini adalah objek anonim
                               {
                                   // Ambil semua field yang dibutuhkan
                                   rbm.RbmId,
                                   rbm.KryNpk,
                                   rbm.RbmTanggalMulai,
                                   rbm.RbmTanggalSelesai,
                                   rbm.RbmTipe,
                                   rbm.OrgId,
                                   rbm.DgsId,
                                   rbm.RsId,
                                   rbm.RbmDokter,
                                   rbm.RbmCost,
                                   rbm.RbmStatusSubmit,
                                   rbm.RbmAlasanPembatalan,
                                   rbm.RbmDiagnosaOther,
                                   rbm.RbmCreatedBy,
                                   rbm.RbmCreatedDate,

                                   // Lakukan null-check di sini
                                   NamaKaryawan = kry != null ? kry.kry_nama_karyawan : "N/A",
                                   NamaDiagnosa = dgs != null ? dgs.DgsNama : "N/A",
                                   NamaPasien = org != null ? org.OrgNama : (kry != null ? kry.kry_nama_karyawan : "Anda"),
                                   NamaRumahSakit = rs != null ? rs.rs_nama : "N/A",
                                   HubunganPasien = org != null ? org.OrgHubungan : "Diri Sendiri"
                               }).FirstOrDefault(); // <-- Eksekusi query di database SEKARANG

                if (rawData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, $"Reimbursement dengan ID {id} tidak ditemukan.");
                }

                // TAHAP 2: Setelah data ada di memori, mapping ke ReimbursementModel
                var reimbursementDetail = new ReimbursementModel
                {
                    RbmId = rawData.RbmId,
                    KryNpk = rawData.KryNpk,
                    RbmTanggalMulai = rawData.RbmTanggalMulai,
                    RbmTanggalSelesai = rawData.RbmTanggalSelesai,
                    RbmTipe = rawData.RbmTipe,
                    OrgId = rawData.OrgId,
                    DgsId = rawData.DgsId,
                    RsId = rawData.RsId,
                    RbmDokter = rawData.RbmDokter,
                    RbmCost = rawData.RbmCost,
                    RbmStatusSubmit = rawData.RbmStatusSubmit,
                    RbmAlasanPembatalan = rawData.RbmAlasanPembatalan,
                    RbmDiagnosaOther = rawData.RbmDiagnosaOther,
                    RbmCreatedBy = rawData.RbmCreatedBy,
                    RbmCreatedDate = rawData.RbmCreatedDate,

                    // Properti NotMapped
                    NamaKaryawan = rawData.NamaKaryawan,
                    NamaDiagnosa = rawData.NamaDiagnosa,
                    NamaPasien = rawData.NamaPasien,
                    NamaRumahSakit = rawData.NamaRumahSakit,
                    HubunganPasien = rawData.HubunganPasien
                };

                return Request.CreateResponse(HttpStatusCode.OK, reimbursementDetail);
            }
            catch (Exception ex)
            {
                // PENTING: Jangan lupa kembalikan ke versi production setelah debugging selesai
                // return Request.CreateResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan saat mengambil data detail.");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()); // Biarkan ini untuk sementara
            }
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

        //[SessionCheck]
        //[HttpPost]
        //public async Task<HttpResponseMessage> Post() // <-- Ubah metode menjadi async Task
        //{
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Permintaan harus berupa multipart/form-data.");
        //    }

        //    try
        //    {
        //        var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
        //        if (sessionLogin == null)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired, silakan login ulang.");
        //        }

        //        var provider = new MultipartMemoryStreamProvider();
        //        await Request.Content.ReadAsMultipartAsync(provider); // <-- Gunakan await

        //        // 1. Ambil data JSON 'values' dulu
        //        var formValuesContent = provider.Contents
        //            .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "values");

        //        if (formValuesContent == null)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data form ('values') tidak ditemukan.");
        //        }

        //        var jsonValues = await formValuesContent.ReadAsStringAsync();
        //        var model = JsonConvert.DeserializeObject<ReimbursementModel>(jsonValues);

        //        // ... (Validasi RbmId dan cari entity tetap sama)
        //        var entityToUpdate = db.gs_track_reimbursement.Find(model.RbmId);

        //        // 2. Proses semua file: validasi, baca, dan konversi ke Base64
        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".pdf" };
        //        var fileContents = provider.Contents.Where(c => c.Headers.ContentDisposition.FileName != null).ToList();

        //        foreach (var file in fileContents)
        //        {
        //            var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
        //            var fieldName = file.Headers.ContentDisposition.Name.Trim('\"'); // Nama input file (e.g., "kwitansiFile")

        //            // Validasi ekstensi file
        //            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        //            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        //            {
        //                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
        //                    $"Tipe file tidak diizinkan: '{fileName}'. Hanya file JPG dan PDF yang diperbolehkan.");
        //            }

        //            // Baca konten file sebagai byte array
        //            var fileBytes = await file.ReadAsByteArrayAsync();

        //            // Konversi ke Base64 string
        //            var base64String = Convert.ToBase64String(fileBytes);

        //            // Masukkan data URI prefix agar file bisa ditampilkan langsung di browser/HTML
        //            var mimeType = extension == ".pdf" ? "application/pdf" : "image/jpeg";
        //            var dataUri = $"data:{mimeType};base64,{base64String}";

        //            // Simpan Base64 string ke properti yang sesuai di entity
        //            switch (fieldName)
        //            {
        //                case "kwitansiFile":
        //                    entityToUpdate.KwitansiFile = dataUri;
        //                    break;
        //                case "rincianObatFile":
        //                    entityToUpdate.RincianObatFile = dataUri;
        //                    break;
        //                case "hasilLabFile":
        //                    entityToUpdate.HasilLabFile = dataUri;
        //                    break;
        //                case "resumeMedis":
        //                    entityToUpdate.ResumeMedisFile = dataUri;
        //                    break;
        //            }
        //        }

        //        // 3. Mapping sisa data dari form ke entity
        //        entityToUpdate.RbmTipe = model.RbmTipe;
        //        entityToUpdate.OrgId = model.OrgId;
        //        entityToUpdate.RbmCost = model.RbmCost;
        //        entityToUpdate.RsId = model.RsId;
        //        entityToUpdate.RbmDokter = model.RbmDokter; // Pastikan JSON-nya memiliki key 'RbmDokter'
        //        entityToUpdate.RbmTanggalMulai = model.RbmTanggalMulai;
        //        if (model.RbmTanggalSelesai.HasValue)
        //        {
        //            entityToUpdate.RbmTanggalSelesai = model.RbmTanggalSelesai.Value;
        //        }
        //        else
        //        {
        //            entityToUpdate.RbmTanggalSelesai = null;
        //        }
        //        entityToUpdate.DgsId = model.DgsId;

        //        if (model.DgsId == "1")
        //        {
        //            entityToUpdate.RbmDiagnosaOther = model.RbmDiagnosaOther;
        //        }
        //        else
        //        {
        //            entityToUpdate.RbmDiagnosaOther = null;
        //        }

        //        entityToUpdate.RbmStatusSubmit = "Menunggu Persetujuan";
        //        entityToUpdate.RbmModifyBy = sessionLogin.npk;
        //        entityToUpdate.RbmModifyDate = DateTime.Now;

        //        // 4. Simpan ke database
        //        db.SaveChanges(); // atau await db.SaveChangesAsync() jika menggunakan EF async

        //        var responseModel = new { RbmId = entityToUpdate.RbmId };
        //        return Request.CreateResponse(HttpStatusCode.OK, responseModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
        //    }
        //}

        [SessionCheck]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
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
                await Request.Content.ReadAsMultipartAsync(provider);

                var formValuesContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "values");
                if (formValuesContent == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data form ('values') tidak ditemukan.");
                }

                var jsonValues = await formValuesContent.ReadAsStringAsync();
                var formData = JObject.Parse(jsonValues);
                var newEntity = new ReimbursementModel();

                // ============================
                // Mapping dengan parsing aman
                // ============================

                // RbmId (PK wajib diisi, pastikan unik)
                if (long.TryParse(formData["RbmId"]?.ToString(), out long rbmId))
                    newEntity.RbmId = rbmId;
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RbmId wajib diisi.");

                // KryNpk dari session
                if (string.IsNullOrWhiteSpace(sessionLogin.npk))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "NPK dari session kosong.");
                newEntity.KryNpk = sessionLogin.npk;

                // RbmTanggalMulai (wajib)
                if (DateTime.TryParseExact(formData["RbmTanggalMulai"]?.ToString(),
                                           "yyyy-MM-dd",
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out DateTime tglMulai))
                {
                    newEntity.RbmTanggalMulai = tglMulai;
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Format tanggal mulai tidak valid (yyyy-MM-dd).");
                }

                // RbmTipe (wajib, max 50 char)
                newEntity.RbmTipe = formData["RbmTipe"]?.ToString();
                if (string.IsNullOrWhiteSpace(newEntity.RbmTipe))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RbmTipe wajib diisi.");

                // OrgId (nullable, kalau 0 dianggap null)
                if (long.TryParse(formData["OrgId"]?.ToString(), out long orgId))
                    newEntity.OrgId = orgId == 0 ? (long?)null : orgId;

                // RsId (wajib)
                if (int.TryParse(formData["RsId"]?.ToString(), out int rsId))
                    newEntity.RsId = rsId;
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RsId wajib diisi.");

                // RbmCost (wajib, decimal 18,2)
                if (decimal.TryParse(formData["RbmCost"]?.ToString(), out decimal rbmCost))
                    newEntity.RbmCost = rbmCost;
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RbmCost wajib diisi.");

                // DgsId (wajib, varchar(10))
                newEntity.DgsId = formData["DgsId"]?.ToString();
                if (string.IsNullOrWhiteSpace(newEntity.DgsId))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "DgsId wajib diisi.");

                // RbmDokter (wajib, varchar(255))
                newEntity.RbmDokter = formData["RbmDokter"]?.ToString();
                if (string.IsNullOrWhiteSpace(newEntity.RbmDokter))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RbmDokter wajib diisi.");

                // RbmTanggalSelesai (opsional)
                var tglSelesaiStr = formData["RbmTanggalSelesai"]?.ToString();
                if (!string.IsNullOrWhiteSpace(tglSelesaiStr) &&
                    DateTime.TryParseExact(tglSelesaiStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tglSelesai))
                {
                    newEntity.RbmTanggalSelesai = tglSelesai;
                }

                // Diagnosa Other hanya kalau DgsId == "1"
                if (newEntity.DgsId == "1")
                {
                    newEntity.RbmDiagnosaOther = formData["RbmDiagnosaOther"]?.ToString();
                }

                // ============================
                // Proses file upload
                // ============================
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".pdf" };
                var fileContents = provider.Contents.Where(c => c.Headers.ContentDisposition.FileName != null).ToList();

                foreach (var file in fileContents)
                {
                    var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                    if (string.IsNullOrEmpty(fileName)) continue;

                    var fieldName = file.Headers.ContentDisposition.Name.Trim('\"');
                    var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

                    if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"Tipe file tidak diizinkan: '{fileName}'.");
                    }

                    var fileBytes = await file.ReadAsByteArrayAsync();
                    var base64String = Convert.ToBase64String(fileBytes);
                    var mimeType = MimeMapping.GetMimeMapping(fileName);
                    var dataUri = $"data:{mimeType};base64,{base64String}";

                    switch (fieldName)
                    {
                        case "kwitansiFile": newEntity.KwitansiFile = dataUri; break;
                        case "rincianObatFile": newEntity.RincianObatFile = dataUri; break;
                        case "hasilLabFile": newEntity.HasilLabFile = dataUri; break;
                        case "resumeMedis": newEntity.ResumeMedisFile = dataUri; break;
                    }
                }

                // ============================
                // Default values
                // ============================
                newEntity.RbmStatusSubmit = "Menunggu Persetujuan";
                newEntity.RbmCreatedBy = sessionLogin.npk;
                newEntity.RbmCreatedDate = DateTime.Now;

                // ============================
                // Save ke DB
                // ============================
                db.gs_track_reimbursement.Add(newEntity);
                await db.SaveChangesAsync();

                var responseModel = new
                {
                    RbmId = newEntity.RbmId,
                    Message = "Data berhasil disimpan."
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseModel);
            }
            catch (Exception ex)
            {
                // Ambil pesan inner exception supaya tahu error SQL dari EF
                var inner = ex.InnerException?.InnerException?.Message
                            ?? ex.InnerException?.Message
                            ?? "";

                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    $"Error: {ex.Message}\nInner: {inner}\nStack: {ex.StackTrace}"
                );
            }
        }

        private long GenerateNoPengajuan(string npk)
        {
            DateTime today = DateTime.Now;
            string prefix = $"{npk}{today:yyMMdd}";

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

        [SessionCheck]
        [HttpPut] // Menggunakan HttpPut karena ini adalah update status
        [Route("api/ReimbursementApi/Cancel")]
        public HttpResponseMessage CancelReimbursement([FromBody] CancelRequestModel model)
        {
            if (model == null || model.RbmId <= 0 || string.IsNullOrWhiteSpace(model.AlasanPembatalan))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Data tidak valid. Pastikan ID dan Alasan Pembatalan terisi." });
            }

            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Sesi tidak valid." });
                }
                string sessionNpk = sessionLogin.npk;

                // Cari data reimbursement di database
                var reimbursement = db.gs_track_reimbursement.FirstOrDefault(r => r.RbmId == model.RbmId);

                if (reimbursement == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = $"Pengajuan dengan ID {model.RbmId} tidak ditemukan." });
                }

                // PERIKSA KEPEMILIKAN: Pastikan yang membatalkan adalah pemilik pengajuan
                if (reimbursement.KryNpk != sessionNpk)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Anda tidak memiliki hak untuk membatalkan pengajuan ini." });
                }

                var allowedStatuses = new List<string> { "Menunggu Persetujuan", "Belum Diverifikasi" };

                // PERIKSA STATUS: Hanya bisa dibatalkan jika statusnya masih "Menunggu" atau "Draft"
                // Sesuaikan dengan nama status di sistem Anda, contoh: "Menunggu Approval", "Submitted", dll.
                if (!allowedStatuses.Contains(reimbursement.RbmStatusSubmit))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = $"Pengajuan ini tidak dapat dibatalkan karena sudah diproses (Status: {reimbursement.RbmStatusSubmit})." });
                }

                // UPDATE DATA
                reimbursement.RbmStatusSubmit = "Dibatalkan"; // Sesuai permintaan
                reimbursement.RbmAlasanPembatalan = model.AlasanPembatalan;
                reimbursement.RbmModifyBy = sessionNpk; // Sesuai permintaan
                reimbursement.RbmModifyDate = DateTime.Now; // Sesuai permintaan

                db.gs_track_reimbursement.AddOrUpdate(reimbursement);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Pengajuan berhasil dibatalkan.", RbmId = reimbursement.RbmId });
            }
            catch (Exception ex)
            {
                // Logging error (opsional tapi sangat disarankan)
                // Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = "Terjadi kesalahan internal: " + ex.Message });
            }
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
