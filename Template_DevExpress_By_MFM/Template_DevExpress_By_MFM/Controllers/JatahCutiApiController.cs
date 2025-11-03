using System;
using System.Linq;
using System.Web.Mvc;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models; // Ganti dengan namespace model Anda
using System.Net;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class JatahCutiApiController : Controller
    {
        private GSDbContextGSTrack db;

        public JatahCutiApiController()
        {
            try
            {
                // Pastikan koneksi string sudah benar
                db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "ari", "123");
            }
            catch (Exception ex)
            {
                // Log error dengan lebih detail untuk debugging
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                // Lemparkan exception agar bisa ditangani di level yang lebih tinggi jika perlu
                throw new Exception("Tidak dapat terhubung ke database.", ex);
            }
        }

        // GET: JatahCutiApi/GetData
        [HttpGet]
        public ActionResult GetData(DataSourceLoadOptions loadOptions)
        {
            // SessionCheck bisa diimplementasikan sebagai Action Filter jika Anda punya
            // [SessionCheck] 
            try
            {
                var session = System.Web.HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(new { message = "Session expired or invalid" }, JsonRequestBehavior.AllowGet);
                }

                string userNpk = session.npk;

                // Query data jatah cuti berdasarkan NPK user
                var dataList = db.gs_track_jatah_cuti
                                 .Where(jc => jc.KryNpk == userNpk);

                // Gunakan DataSourceLoader untuk memproses data sesuai loadOptions dari DevExtreme
                var loadResult = DataSourceLoader.Load(dataList, loadOptions);

                // Kembalikan data dalam format JSON
                return Json(loadResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Pastikan untuk melepaskan koneksi database saat controller selesai digunakan
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}