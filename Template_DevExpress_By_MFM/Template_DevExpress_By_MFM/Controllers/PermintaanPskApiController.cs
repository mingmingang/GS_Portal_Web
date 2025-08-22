using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class PermintaanPskApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public PermintaanPskApiController()
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

        // GET: api/PermintaanPskApi
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var dataList = db.gs_track_psk
                    .AsEnumerable() // pindah ke LINQ to Objects
                    .Select(p => new PermintaanPskModel
                    {
                        psk_id = p.psk_id,
                        kry_npk = p.kry_npk,
                        psk_ap = p.psk_ap,
                        psk_ket = p.psk_ket,
                        psk_status = p.psk_status,
                        psk_crea_date = p.psk_crea_date,
                        psk_crea_by = p.psk_crea_by,
                        psk_modi_date = p.psk_modi_date,
                        psk_modi_by = p.psk_modi_by
                    });

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // GET: api/PermintaanPskApi/GetCounts
        [SessionCheck]
        [HttpGet]
        [Route("api/PermintaanPskApi/GetCounts")]
        public HttpResponseMessage GetCounts()
        {
            try
            {
                var counts = new
                {
                    selesai = db.gs_track_psk.Count(p => p.psk_status == "Selesai"),
                    ditolak = db.gs_track_psk.Count(p => p.psk_status == "Ditolak"),
                    belum_diverifikasi = db.gs_track_psk.Count(p => p.psk_status == "Belum Diverifikasi")
                };

                return Request.CreateResponse(HttpStatusCode.OK, counts);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/PermintaanPskApi
        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var permintaanPsk = new PermintaanPskModel();

                JsonConvert.PopulateObject(values, permintaanPsk);

                Validate(permintaanPsk);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                // Set default values untuk permintaan baru
                permintaanPsk.psk_status = "Belum Diverifikasi";
                permintaanPsk.psk_crea_date = DateTime.Now;
                permintaanPsk.psk_crea_by = GetCurrentUserNpk();

                db.gs_track_psk.Add(permintaanPsk);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, permintaanPsk);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // PUT: api/PermintaanPskApi
        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key"); // string
                var entity = db.gs_track_psk.FirstOrDefault(p => p.psk_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var values = form.Get("values");
                JsonConvert.PopulateObject(values, entity);

                // Update modification info
                entity.psk_modi_date = DateTime.Now;
                entity.psk_modi_by = GetCurrentUserNpk();

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

        // DELETE: api/PermintaanPskApi
        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key"); // string
                var entity = db.gs_track_psk.FirstOrDefault(p => p.psk_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                db.gs_track_psk.Remove(entity);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Helper method untuk get current user NPK
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