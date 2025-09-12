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
using System.Web;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class CutiApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public CutiApiController()
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

        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            try
            {
                var values = form.Get("values");
                var cuti = new CutiModel();

                // Generate cuti_id seperti sebelumnya...
                var datePart = DateTime.Now.ToString("yyyyMMdd");
                var lastCuti = db.gs_track_cuti
                               .Where(c => c.cuti_id.StartsWith("LVR" + datePart))
                               .OrderByDescending(c => c.cuti_id)
                               .FirstOrDefault();

                int lastNumber = lastCuti != null ?
                    int.Parse(lastCuti.cuti_id.Substring(11, 4)) : 0; // ambil 4 digit terakhir

                cuti.cuti_id = "LVR" + datePart + (lastNumber + 1).ToString("D4");



                // Isi properti dari JSON ke model
                JsonConvert.PopulateObject(values, cuti);

                // **Set kry_npk dari session user (ubah sesuai session kamu)**
                var logSession = HttpContext.Current.Session["SHealth"] as Template_DevExpress_By_MFM.Models.SessionLogin;
                if (logSession != null)
                {
                    cuti.kry_npk = logSession.npk;  // Contoh: pastikan property npk ada di session
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User tidak ditemukan di session.");
                }

                cuti.status = "Menunggu Persetujuan";
                cuti.tanggal_pengajuan = DateTime.Now;
                cuti.mulai_dari = cuti.tanggal_awal;
                cuti.sampai_dengan = cuti.tanggal_akhir;

                if(cuti.tipe_cuti == "Cuti Pribadi")
                {
                    cuti.sub_tipe_cuti = "CP - Cuti Pribadi";
                } 
                if(cuti.tipe_cuti == "Cuti Besar")
                {
                    cuti.sub_tipe_cuti = "CB - Cuti Besar";
                }

                if (cuti.tanggal_akhir < cuti.tanggal_awal)
                {
                    ModelState.AddModelError("tanggal_akhir", "Tanggal selesai harus setelah tanggal mulai");
                }


                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                db.gs_track_cuti.Add(cuti);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, new
                {
                    cuti.cuti_id,
                    cuti.tipe_cuti,
                    cuti.sub_tipe_cuti,
                    cuti.mulai_dari,
                    cuti.sampai_dengan,
                    cuti.tanggal_awal,
                    cuti.tanggal_akhir,
                    cuti.durasi,
                    cuti.status
                });
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, inner.Message);
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

        [HttpGet]
        [Route("api/CutiApi/GenerateCutiId")]
        public HttpResponseMessage GenerateCutiId()
        {
            try
            {
                // Format: LVR + tanggal (yyMMdd) + increment 4 digit
                string prefix = "LVR" + DateTime.Now.ToString("yyMMdd");

                // Cari nomor terakhir di database
                var lastCuti = db.gs_track_cuti
                                .Where(c => c.cuti_id.StartsWith(prefix))
                                .OrderByDescending(c => c.cuti_id)
                                .FirstOrDefault();

                int lastNumber = 0;
                if (lastCuti != null)
                {
                    // Ambil 4 digit terakhir
                    string lastDigits = lastCuti.cuti_id.Substring(prefix.Length);
                    int.TryParse(lastDigits, out lastNumber);
                }

                // Generate nomor baru
                string newId = prefix + (lastNumber + 1).ToString("D4");

                return Request.CreateResponse(HttpStatusCode.OK, newId);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
