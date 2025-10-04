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
    /// <summary>
    /// API Controller untuk proxy request ke GsTracker API
    /// PENTING: Nama controller harus sesuai dengan route: "PermintaanApi"
    /// </summary>
    [RoutePrefix("api/PermintaanApi")]
    public class PermintaanApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string GsTrackerApiBaseUrl = "http://localhost:44320/api/gstracker";
        private static readonly HttpClient _httpClient;

        static PermintaanApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion

        #region Helper Methods
        private async Task<HttpResponseMessage> ForwardJsonGetRequestToGsTrackerApi(string url)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY] Forwarding to: {url}");

                var gsTrackerResponse = await _httpClient.GetAsync(url);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[PROXY] Backend Status: {gsTrackerResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[PROXY] Backend Response: {gsTrackerContent}");

                var proxyResponse = Request.CreateResponse(gsTrackerResponse.StatusCode);
                proxyResponse.Content = new StringContent(gsTrackerContent, Encoding.UTF8, "application/json");

                return proxyResponse;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY ERROR] Connection failed: {ex.Message}");
                return Request.CreateErrorResponse(
                    HttpStatusCode.BadGateway,
                    $"Tidak dapat terhubung ke service GsTracker. {ex.Message}"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY ERROR] Unexpected error: {ex.ToString()}");
                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    "Terjadi kesalahan pada server saat memproses permintaan."
                );
            }
        }
        #endregion

        #region === ENDPOINT PERMINTAAN PIC ===

        /// <summary>
        /// GET: api/PermintaanApi/pic/000299/K?startDate=2025-01-01&endDate=2025-12-31
        /// </summary>
        [HttpGet]
        [Route("pic/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetPermintaanPicListProxy(
            string npk,
            string plant,
            [FromUri] string ah = null,
            [FromUri] string status = null,
            [FromUri] string startDate = null,
            [FromUri] string endDate = null)
        {
            System.Diagnostics.Debug.WriteLine("=== PIC PROXY CALLED ===");
            System.Diagnostics.Debug.WriteLine($"NPK: {npk}, Plant: {plant}");
            System.Diagnostics.Debug.WriteLine($"Dates: {startDate} to {endDate}");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;

            if (!string.IsNullOrEmpty(ah)) queryString["ah"] = ah;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            var requestUrl = $"{GsTrackerApiBaseUrl}/pic?{queryString.ToString()}";

            return await ForwardJsonGetRequestToGsTrackerApi(requestUrl);
        }

        #endregion

        #region === ENDPOINT PERMINTAAN SKP ===

        /// <summary>
        /// GET: api/PermintaanApi/skp/000299/K?startDate=2025-01-01&endDate=2025-12-31
        /// </summary>
        [HttpGet]
        [Route("skp/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetPermintaanSkpListProxy(
            string npk,
            string plant,
            [FromUri] string ap = null,
            [FromUri] string status = null,
            [FromUri] string startDate = null,
            [FromUri] string endDate = null)
        {
            System.Diagnostics.Debug.WriteLine("=== SKP PROXY CALLED ===");
            System.Diagnostics.Debug.WriteLine($"NPK: {npk}, Plant: {plant}");
            System.Diagnostics.Debug.WriteLine($"Dates: {startDate} to {endDate}");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;

            if (!string.IsNullOrEmpty(ap)) queryString["ap"] = ap;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            var requestUrl = $"{GsTrackerApiBaseUrl}/skp?{queryString.ToString()}";

            return await ForwardJsonGetRequestToGsTrackerApi(requestUrl);
        }

        #endregion

        #region === TEST ENDPOINT ===

        /// <summary>
        /// Test endpoint untuk memastikan controller bisa diakses
        /// GET: api/PermintaanApi/test
        /// </summary>
        [HttpGet]
        [Route("test")]
        public IHttpActionResult TestEndpoint()
        {
            return Ok(new
            {
                message = "PermintaanApi Controller is working!",
                timestamp = DateTime.Now,
                baseUrl = GsTrackerApiBaseUrl
            });
        }

        #endregion
    }
}