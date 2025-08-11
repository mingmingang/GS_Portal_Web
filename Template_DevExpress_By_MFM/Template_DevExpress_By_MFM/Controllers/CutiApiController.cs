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
    public class CutiApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public CutiApiController()
        {
            try
            {
                db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "sa", "aangaang");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                throw new Exception("Tidak dapat terhubung ke database.", ex);
            }
        }

        // GET: api/Cuti
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var dataList = db.gs_track_cuti
      .AsEnumerable() // pindah ke LINQ to Objects
      .Select(c => new CutiModel
      {
          cuti_id = c.cuti_id,
          kry_npk = c.kry_npk,
          tipe_cuti = c.tipe_cuti,
          sub_tipe_cuti = c.sub_tipe_cuti,
          mulai_dari = c.mulai_dari,
          sampai_dengan = c.sampai_dengan,
          durasi = c.durasi,
          status = c.status,
          alasan = c.alasan,
          lampiran = c.lampiran,
          tanggal_pengajuan = c.tanggal_pengajuan,
          masa_berlaku_cuti = c.masa_berlaku_cuti,
          jenis_cuti = c.jenis_cuti,
          tanggal_akhir = c.tanggal_akhir,
          tanggal_awal = c.tanggal_awal
      });

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/Cuti
        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var cuti = new CutiModel();

                JsonConvert.PopulateObject(values, cuti);

                Validate(cuti);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                cuti.status = "Menunggu Persetujuan";
                cuti.tanggal_pengajuan = DateTime.Now;

                db.gs_track_cuti.Add(cuti);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, cuti);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // PUT: api/Cuti
        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key"); // string
                var entity = db.gs_track_cuti.FirstOrDefault(c => c.cuti_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var values = form.Get("values");
                JsonConvert.PopulateObject(values, entity);

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

        // DELETE: api/Cuti
        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form)
        {
            try
            {
                var key = form.Get("key"); // string
                var entity = db.gs_track_cuti.FirstOrDefault(c => c.cuti_id == key);

                if (entity == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                db.gs_track_cuti.Remove(entity);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
