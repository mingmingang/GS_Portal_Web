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
    public class OverTimeApiController : ApiController
    {
        #region Konfigurasi & Properti
        //private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api";
        private const string SunfishApiBaseUrl = "http://localhost:44320/api";

        // Kredensial API Sunfish
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        static OverTimeApiController()
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

        /// <summary>
        /// Mengambil data lembur dari LemburController (Back-end) melalui mekanisme Proxy.
        /// </summary>
        [System.Web.Http.Route("api/OverTimeApi/GetOverTimeData")]
        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetOverTimeData()
        {
            var targetUrl = $"{SunfishApiBaseUrl}/Lembur/getLembur";  
            return await ForwardJsonGetRequestToSunfishApi(targetUrl);
        }

        [System.Web.Http.Route("api/OverTimeApi/GetOverTimeById")]
        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetOverTimeById(string id)
        {
            var targetUrl = $"{SunfishApiBaseUrl}/Lembur/GetLemburById?id={id}";
            return await ForwardJsonGetRequestToSunfishApi(targetUrl);
        }


        #endregion
    }
}