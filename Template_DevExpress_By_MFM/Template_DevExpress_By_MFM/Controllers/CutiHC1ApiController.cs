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
using System.Data.Entity;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class CutiHC1ApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public CutiHC1ApiController()
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
                // Ambil session, pastikan atasan yang login
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Session expired");
                }

                // (Opsional) Tambahkan validasi di sini untuk memastikan hanya role atasan yang bisa mengakses

                var dataList = db.gs_track_cuti
                    .Where(c => c.status == "Menunggu Verifikasi") // Filter data dengan status "Menunggu Persetujuan"
                    .AsEnumerable()
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
                        // Anda bisa menambahkan properti lain jika perlu, misal nama karyawan
                    });

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


       
    }


}
