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

                // Parse status filter dari query string
                List<string> statusList = new List<string>();
                if (!string.IsNullOrEmpty(statuses))
                {
                    try
                    {
                        statusList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(statuses);
                    }
                    catch { /* biarkan kosong jika parsing gagal */ }
                }

                // Step 1: Query data mentah
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
                               select new
                               {
                                   rbm.RbmId,
                                   rbm.KryNpk,
                                   NamaKaryawan = kry != null ? kry.kry_nama_karyawan : "N/A",
                                   NamaPasien = org != null ? org.OrgNama : kry.kry_nama_karyawan,
                                   rbm.RbmTipe,
                                   NamaDiagnosa = dgs != null ? dgs.DgsNama : "N/A",
                                   rbm.RbmCost,
                                   rbm.RbmTanggalMulai,
                                   rbm.RbmTanggalSelesai,
                                   rbm.RbmStatusSubmit
                               };

                // Filter status kalau ada
                if (statusList.Any())
                {
                    rawQuery = rawQuery.Where(r => statusList.Contains(r.RbmStatusSubmit));
                }

                // Step 2: materialize
                var rawList = rawQuery.ToList();

                // Step 3: mapping ke model
                var modelList = rawList.Select(item => new ReimbursementModel
                {
                    RbmId = item.RbmId,
                    KryNpk = item.KryNpk,
                    NamaKaryawan = item.NamaKaryawan,
                    NamaPasien = item.NamaPasien,
                    RbmTipe = item.RbmTipe,
                    NamaDiagnosa = item.NamaDiagnosa,
                    RbmCost = item.RbmCost,
                    RbmTanggalMulai = item.RbmTanggalMulai,
                    RbmTanggalSelesai = item.RbmTanggalSelesai,
                    RbmStatusSubmit = item.RbmStatusSubmit,
                    durasi = GetDurasi(item.RbmTanggalMulai, item.RbmTanggalSelesai)
                });

                // Step 4: pakai DataSourceLoader
                var loadResult = DataSourceLoader.Load(modelList, loadOptions);

                return Request.CreateResponse(HttpStatusCode.OK, loadResult);
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

    public class PlafonCategoryViewModel
    {
        public string Title { get; set; }
        public string Plafon { get; set; }
        public string Digunakan { get; set; }
        public string Sisa { get; set; }
        public string Note { get; set; }
        public string Unrealize { get; set; }
    }
}
