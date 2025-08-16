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
                                   HubunganPasien = org != null ? org.OrgHubungan : "Diri Sendiri" // Contoh default
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
                    data = loadResultForGrid,
                    summary = summary
                };

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

        // POST: api/Reimbursement
        //[SessionCheck]
        //[HttpPost]
        //public HttpResponseMessage Post(FormDataCollection form)
        //{
        //    try
        //    {
        //        var values = form.Get("values");
        //        var reimbursement = new ReimbursementModel();

        //        JsonConvert.PopulateObject(values, reimbursement);

        //        Validate(reimbursement);
        //        if (!ModelState.IsValid)
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

        //        reimbursement.status = "Menunggu Persetujuan";
        //        reimbursement.tanggal_pengajuan = DateTime.Now;

        //        db.gs_track_reimbursement.Add(reimbursement);
        //        db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.Created, reimbursement);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

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
