using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class SuratJaminanApiController : ApiController
    {
        #region Konfigurasi & Properti
//        private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api/Sunfish";
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/Sunfish";

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
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }

            var empid = sessionLogin.empid;
            var plant = sessionLogin.userplant;

            var requestUrl = $"{SunfishApiBaseUrl}/getSuratJaminanList/{empid}/{plant}?tab={Uri.EscapeDataString(tab)}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/SuratJaminanApi/getSuratJaminanListFilter")]
        public async Task<HttpResponseMessage> GetSuratJaminanListFilterProxy([FromUri] int year, [FromUri] string tipe, [FromUri] string tab = "Semua")
        {
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }

            var empid = sessionLogin.empid;
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
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }
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
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir. Silakan login kembali.");
            }

            var empid = sessionLogin.empid;

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
                var sunfishResponse = await _httpClient.PostAsync(requestUrl, Request.Content);
                return sunfishResponse;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region === ENDPOINT VERIFIKASI QR (PUBLIC - TIDAK PERLU SESSION) ===

        /// <summary>
        /// Proxy untuk verifikasi surat jaminan via QR code (public access).
        /// Endpoint ini TIDAK menggunakan [SessionCheck] karena diakses dari luar sistem.
        /// </summary>
        [HttpGet]
        [Route("api/SuratJaminanApi/getSuratJaminanByNoRequest")]
        public async Task<HttpResponseMessage> GetSuratJaminanByNoRequestProxy([FromUri] string noRequest)
        {
            if (string.IsNullOrWhiteSpace(noRequest))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Parameter 'noRequest' diperlukan.");
            }

            // URL-encode parameter untuk menangani karakter khusus seperti '/'
            var encodedNoRequest = Uri.EscapeDataString(noRequest);
            var requestUrl = $"{SunfishApiBaseUrl}/getSuratJaminanByNoRequest?noRequest={encodedNoRequest}";

            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        #endregion
    }
}