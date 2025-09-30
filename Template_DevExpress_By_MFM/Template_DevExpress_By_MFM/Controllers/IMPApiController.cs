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
    public class IMPApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker";

        // Kredensial API Sunfish
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        static IMPApiController()
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
        /// Meneruskan request POST ke Sunfish API.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardPostRequestToSunfishApi(string url, HttpContent content)
        {
            try
            {
                var sunfishResponse = await _httpClient.PostAsync(url, content);
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

        #region === ENDPOINT IMP ===

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/Summary")]
        public async Task<HttpResponseMessage> GetSummaryProxy(string npk = null)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Jika npk tidak disediakan, gunakan npk dari session
                if (string.IsNullOrWhiteSpace(npk))
                    npk = session.npk;

                // Panggil endpoint list tanpa filter status untuk mendapatkan semua data dan summary
                var requestUrl = $"{SunfishApiBaseUrl}/imp/list?npk={npk}";
                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi")]
        public async Task<HttpResponseMessage> GetProxy([FromUri] string npk = null, [FromUri] string status = null)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Jika npk tidak disediakan, gunakan npk dari session
                if (string.IsNullOrWhiteSpace(npk))
                    npk = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/imp/list?npk={npk}";

                // Tambahkan parameter status jika ada
                if (!string.IsNullOrWhiteSpace(status))
                {
                    requestUrl += $"&status={Uri.EscapeDataString(status)}";
                }

                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Endpoint lainnya tetap sama seperti sebelumnya...
        // [HttpGet] GetDetailProxy, [HttpPost] PostProxy, dll.

        #endregion
    }
}