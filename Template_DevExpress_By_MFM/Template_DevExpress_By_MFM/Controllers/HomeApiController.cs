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
    public class HomeApiController : ApiController
    {
        #region Konfigurasi & Properti
//        private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api/gstracker/home";
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker/home";

        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        private static readonly HttpClient _httpClient;

        static HomeApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
        }
        #endregion

        #region Helper Methods untuk Proxy (Copied & Adapted)
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
        /// Meneruskan request PUT ke Sunfish API.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardPutRequestToSunfishApi(string url)
        {
            try
            {
                // Ambil body dari request asli dan teruskan
                var body = await Request.Content.ReadAsStringAsync();
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var sunfishResponse = await _httpClient.PutAsync(url, content);
                return sunfishResponse; // Langsung return response dari Sunfish
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // Opsional: Tambahkan helper untuk POST jika Anda butuh membuat pengajuan cuti baru
        // private async Task<HttpResponseMessage> ForwardPostRequestToSunfishApi(string url) { ... }

        #endregion

        #region === ENDPOINT PROXY CEK STATUS KONEKSI BACKEND ===

        /// <summary>
        /// Proxy untuk mengambil daftar jatah cuti (leave entitlement).
        /// </summary>
        [SessionCheck] // Pastikan Anda memiliki atribut ini untuk memeriksa sesi login
        [HttpGet]
        [Route("api/HomeApi/shoot")]
        public async Task<HttpResponseMessage> CheckPing()
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
            }

            var requestUrl = $"{SunfishApiBaseUrl}/shoot";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        #endregion
    }
}