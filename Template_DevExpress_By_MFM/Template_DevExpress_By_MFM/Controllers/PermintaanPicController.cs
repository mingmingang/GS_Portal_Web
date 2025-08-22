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
//sdadawdasdadwadas
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

        // GET: api/PermintaanPicApi
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var dataList = db.gs_track_pic
                    .AsEnumerable() // pindah ke LINQ to Objects
                    .Select(p => new PermintaanPicModel
                    {
                        pic_id = p.pic_id,
                        kry_npk = p.kry_npk,
                        pic_ah = p.pic_ah,
                        pic_status = p.pic_status,
                        pic_crea_date = p.pic_crea_date,
                        pic_crea_by = p.pic_crea_by,
                        pic_modi_date = p.pic_modi_date,
                        pic_modi_by = p.pic_modi_by
                    });

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // GET: api/PermintaanPicApi/GetCounts
        [SessionCheck]
        [HttpGet]
        [Route("api/PermintaanPicApi/GetCounts")]
        public HttpResponseMessage GetCounts()
        {
            try
            {
                var counts = new
                {
                    selesai = db.gs_track_pic.Count(p => p.pic_status == "Selesai"),
                    ditolak = db.gs_track_pic.Count(p => p.pic_status == "Ditolak"),
                    belum_diverifikasi = db.gs_track_pic.Count(p => p.pic_status == "Belum Diverifikasi")
                };

                return Request.CreateResponse(HttpStatusCode.OK, counts);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/PermintaanPicApi
        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var permintaanPic = new PermintaanPicModel();

                JsonConvert.PopulateObject(values, permintaanPic);

                Validate(permintaanPic);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                // Set default values untuk permintaan baru
                permintaanPic.pic_status = "Belum Diverifikasi";
                permintaanPic.pic_crea_date = DateTime.Now;
                permintaanPic.pic_crea_by = GetCurrentUserNpk();

                db.gs_track_pic.Add(permintaanPic);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, permintaanPic);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // PUT: api/PermintaanPicApi
        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key"); // string
                var entity = db.gs_track_pic.FirstOrDefault(p => p.pic_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var values = form.Get("values");
                JsonConvert.PopulateObject(values, entity);

                // Update modification info
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

        // DELETE: api/PermintaanPicApi
        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key"); // string
                var entity = db.gs_track_pic.FirstOrDefault(p => p.pic_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                db.gs_track_pic.Remove(entity);
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