using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web; // Diperlukan untuk HttpContext
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils; // Pastikan namespace ini sesuai dengan proyek Anda

namespace Template_DevExpress_By_MFM.Controllers
{
    public class SuratJaminanApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string SunfishApiBaseUrl = "http://10.19.101.146:44321/api/Sunfish"; // Pastikan port ini sesuai saat debug

        // Kredensial API Sunfish
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        static SuratJaminanApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
        }
        #endregion

        #region Helper Methods untuk Proxy
        /// <summary>
        /// Meneruskan request GET yang mengembalikan JSON ke Sunfish API.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardJsonGetRequestToSunfishApi(string url)
        {
            try
            {
                var sunfishResponse = await _httpClient.GetAsync(url);
                var sunfishContent = await sunfishResponse.Content.ReadAsStringAsync();

                var proxyResponse = Request.CreateResponse(sunfishResponse.StatusCode);
                proxyResponse.Content = new StringContent(sunfishContent, Encoding.UTF8, "application/json");

                return proxyResponse;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sunfish API connection error: {ex.ToString()}");
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, $"Tidak dapat terhubung ke service Sunfish. {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Proxy error: {ex.ToString()}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan pada server saat memproses permintaan.");
            }
        }

        /// <summary>
        /// Meneruskan request GET yang mengembalikan konten mentah (seperti file/gambar) ke Sunfish API.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardRawGetRequestToSunfishApi(string url)
        {
            try
            {
                var sunfishResponse = await _httpClient.GetAsync(url);

                // Langsung teruskan respons dari API tujuan, termasuk status code, content, dan header.
                return sunfishResponse;
            }
            catch (HttpRequestException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, $"Tidak dapat terhubung ke service Sunfish. {ex.Message}");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region === ENDPOINT YANG SUDAH ADA - DIPERBAIKI UNTUK MENGGUNAKAN EMPID DARI SESSION ===

        [SessionCheck]
        [HttpGet]
        [Route("api/SuratJaminanApi/getSuratJaminanList")]
        public async Task<HttpResponseMessage> GetSuratJaminanListProxy([FromUri] string tab = "Semua")
        {
            // [PERBAIKAN] Mengambil empid dari session, bukan dari parameter
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }

            var empid = sessionLogin.empid;  // Gunakan empid (DO170059) seperti di Reimbursement
            var plant = sessionLogin.userplant;

            var requestUrl = $"{SunfishApiBaseUrl}/getSuratJaminanList/{empid}/{plant}?tab={Uri.EscapeDataString(tab)}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/SuratJaminanApi/getSuratJaminanListFilter")]
        public async Task<HttpResponseMessage> GetSuratJaminanListFilterProxy([FromUri] int year, [FromUri] string tipe, [FromUri] string tab = "Semua")
        {
            // [PERBAIKAN] Mengambil empid dari session, bukan dari parameter
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }

            var empid = sessionLogin.empid;  // Gunakan empid (DO170059) seperti di Reimbursement
            var plant = sessionLogin.userplant;

            var requestUrl = $"{SunfishApiBaseUrl}/getSuratJaminanListFilter/{empid}/{plant}?year={year}&tipe={Uri.EscapeDataString(tipe)}&tab={Uri.EscapeDataString(tab)}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region === ENDPOINT BARU UNTUK HALAMAN DETAIL ===

        /// <summary>
        /// Proxy untuk mengambil detail surat jaminan. empid diambil dari session untuk keamanan.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/SuratJaminanApi/getSuratJaminanDetail/{id}")]
        public async Task<HttpResponseMessage> GetSuratJaminanDetailProxy(int id)
        {
            // [PERBAIKAN] Mengambil empid dari session SHealth yang benar
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }
            // Gunakan empid (DO170059) konsisten dengan Reimbursement
            var empid = sessionLogin.empid;
            var plant = sessionLogin.userplant;

            var requestUrl = $"{SunfishApiBaseUrl}/getSuratJaminanDetail/{id}/{empid}/{plant}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region === ENDPOINT BARU (MASTER DATA & LAINNYA) ===

        [SessionCheck]
        [HttpGet]
        [Route("api/SuratJaminanApi/generateSuratJaminanNo")]
        public async Task<HttpResponseMessage> GenerateSuratJaminanNoProxy()
        {
            // [PERBAIKAN] Mengambil empid dari session, bukan dari parameter
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }

            var empid = sessionLogin.empid;  // Gunakan empid (DO170059) seperti di Reimbursement

            var requestUrl = $"{SunfishApiBaseUrl}/generateSuratJaminanNo/{empid}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/SuratJaminanApi/createSuratJaminan")]
        public async Task<HttpResponseMessage> CreateSuratJaminanProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/createSuratJaminan";
            try
            {
                // Meneruskan konten multipart/form-data apa adanya
                var sunfishResponse = await _httpClient.PostAsync(requestUrl, Request.Content);
                return sunfishResponse;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion
    }
}