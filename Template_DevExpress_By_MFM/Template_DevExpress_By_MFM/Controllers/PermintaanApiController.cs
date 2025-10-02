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
    public class PermintaanController : ApiController
    {
        #region Konfigurasi & Properti
        // Base URL untuk API GsTracker
        private const string GsTrackerApiBaseUrl = "http://localhost:44320/api/gstracker";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        static PermintaanController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion

        #region Helper Methods untuk Proxy GsTracker API
        /// <summary>
        /// Meneruskan request GET yang mengembalikan JSON ke GsTracker API.
        /// Menghindari double wrapping data dengan mengambil langsung dari response BE.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardJsonGetRequestToGsTrackerApi(string url)
        {
            try
            {
                var gsTrackerResponse = await _httpClient.GetAsync(url);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

                // PERBAIKAN: Langsung teruskan response dari BE tanpa modifikasi
                // BE sudah mengirim format: { code: 200, message: "Success", data: [...] }
                var proxyResponse = Request.CreateResponse(gsTrackerResponse.StatusCode);
                proxyResponse.Content = new StringContent(gsTrackerContent, Encoding.UTF8, "application/json");

                return proxyResponse;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"GsTracker API connection error: {ex.ToString()}");
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, $"Tidak dapat terhubung ke service GsTracker. {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Proxy error in PermintaanController: {ex.ToString()}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan pada server saat memproses permintaan.");
            }
        }
        #endregion

        #region === ENDPOINT PERMINTAAN PIC ===

        /// <summary>
        /// Proxy untuk mengambil daftar Permintaan PIC dari GsTracker API.
        /// GET: api/PermintaanApi/pic/{npk}/{plant}
        /// Query params: ah, status, startDate, endDate
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/PermintaanApi/pic/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetPermintaanPicListProxy(
            string npk,
            string plant,
            [FromUri] string ah = null,
            [FromUri] string status = null,
            [FromUri] string startDate = null,
            [FromUri] string endDate = null)
        {
            // Bangun query string untuk semua parameter
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;

            if (!string.IsNullOrEmpty(ah)) queryString["ah"] = ah;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            var requestUrl = $"{GsTrackerApiBaseUrl}/pic?{queryString.ToString()}";

            System.Diagnostics.Debug.WriteLine($"Forwarding PIC request to: {requestUrl}");

            return await ForwardJsonGetRequestToGsTrackerApi(requestUrl);
        }

        #endregion

        #region === ENDPOINT PERMINTAAN SKP ===

        /// <summary>
        /// Proxy untuk mengambil daftar Permintaan SKP dari GsTracker API.
        /// GET: api/PermintaanApi/skp/{npk}/{plant}
        /// Query params: ap, status, startDate, endDate
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/PermintaanApi/skp/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetPermintaanSkpListProxy(
            string npk,
            string plant,
            [FromUri] string ap = null,
            [FromUri] string status = null,
            [FromUri] string startDate = null,
            [FromUri] string endDate = null)
        {
            // Bangun query string untuk semua parameter
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;

            if (!string.IsNullOrEmpty(ap)) queryString["ap"] = ap;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            var requestUrl = $"{GsTrackerApiBaseUrl}/skp?{queryString.ToString()}";

            System.Diagnostics.Debug.WriteLine($"Forwarding SKP request to: {requestUrl}");

            return await ForwardJsonGetRequestToGsTrackerApi(requestUrl);
        }

        #endregion
    }
}