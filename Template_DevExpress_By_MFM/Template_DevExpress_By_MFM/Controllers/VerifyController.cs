using System;
using System.Text;
using System.Web.Mvc;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class VerifyController : Controller
    {
        [AllowAnonymous]
        public ActionResult SuratJaminan(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    ViewBag.Error = "Parameter QR code tidak valid.";
                    ViewBag.ErrorDetail = "Nomor surat tidak ditemukan dalam QR code.";
                    return View();
                }

                string decodedNoRequest = DecodeBase64(id);

                if (string.IsNullOrWhiteSpace(decodedNoRequest))
                {
                    ViewBag.Error = "QR code tidak dapat dibaca.";
                    ViewBag.ErrorDetail = "Format encoding tidak valid.";
                    return View();
                }

                ViewBag.DecodedId = decodedNoRequest;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Terjadi kesalahan saat memverifikasi surat.";
                ViewBag.ErrorDetail = ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Helper method untuk decode Base64 string
        /// </summary>
        private string DecodeBase64(string base64EncodedData)
        {
            try
            {
                base64EncodedData = base64EncodedData.Replace('-', '+').Replace('_', '/');

                switch (base64EncodedData.Length % 4)
                {
                    case 2: base64EncodedData += "=="; break;
                    case 3: base64EncodedData += "="; break;
                }

                var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}