using System;
using System.IO; // Needed for Path operations
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
    public class CutiApiController : ApiController
    {
        #region Configuration & Properties
        // --- This section is well-structured and correct ---
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker/cuti";
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        private static readonly HttpClient _httpClient;

        // Static constructor for initializing HttpClient. This is the recommended pattern.
        static CutiApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
        }
        #endregion

        #region Helper Methods for Proxying
        // --- Helper methods are good for reducing code duplication ---
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
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, $"Could not connect to the Sunfish service. {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Proxy error: {ex.ToString()}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An internal server error occurred while processing the request.");
            }
        }
        #endregion

        #region Leave Entitlement Proxy Endpoints
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/listJatahCuti")]
        public async Task<HttpResponseMessage> GetJatahCutiProxy([FromUri] string emp_id, [FromUri] string leave_code = null, [FromUri] string from = null, [FromUri] string to = null)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null) return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");

            var requestUrl = $"{SunfishApiBaseUrl}/list_jatah_cuti?emp_id={emp_id}&leave_code={leave_code}&from={from}&to={to}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region Leave Request Proxy Endpoints
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/listCuti")]
        public async Task<HttpResponseMessage> GetListCutiProxy([FromUri] string emp_id, [FromUri] string from = null, [FromUri] string to = null, [FromUri] string request_status = "all")
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null) return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");

            var requestUrl = $"{SunfishApiBaseUrl}/list_cuti?emp_id={emp_id}&from={from}&to={to}&request_status={request_status}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/detailCuti/{id}")]
        public async Task<HttpResponseMessage> GetDetailCutiProxy(string id)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null) return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");

            var requestUrl = $"{SunfishApiBaseUrl}/list_cuti_byid?request_no={id}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/createCuti")]
        public async Task<HttpResponseMessage> CreateCutiProxy()
        {
            // Your existing CreateCutiProxy logic goes here. It seems correct.
            var requestUrl = $"{SunfishApiBaseUrl}/create_cuti";
            // ... the rest of your implementation ...
            return await Task.FromResult(Request.CreateResponse(HttpStatusCode.NotImplemented)); // Placeholder
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/editCuti")]
        public async Task<HttpResponseMessage> EditCutiProxy()
        {
            // Your existing EditCutiProxy logic goes here. It also seems correct.
            var requestUrl = $"{SunfishApiBaseUrl}/edit_cuti";
            // ... the rest of your implementation ...
            return await Task.FromResult(Request.CreateResponse(HttpStatusCode.NotImplemented)); // Placeholder
        }
        #endregion

        #region Other Proxy Endpoints
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/listTipeCuti")]
        public async Task<HttpResponseMessage> GetLeaveTypesProxy([FromUri] int? company_id = null)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null) return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");

            var requestUrl = $"{SunfishApiBaseUrl}/list_jenis_cuti";
            if (company_id.HasValue)
            {
                requestUrl += $"?company_id={company_id.Value}";
            }
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/getLastIdCuti")]
        public async Task<HttpResponseMessage> GetLastCutiIdProxy()
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null) return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");

            var requestUrl = $"{SunfishApiBaseUrl}/get_last_id_cuti";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region GET ATTACHMENT FILE (PROXY)
        /// <summary>
        /// Proxies a request to fetch an attachment file from the Sunfish API.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/getLampiran/{fileName}")]
        public async Task<HttpResponseMessage> GetLampiranProxy(string fileName)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "File name cannot be empty.");
            }

            // --- THE SYNTAX ERROR WAS HERE ---
            // The if (_httpClient == null) check is no longer needed because the static constructor
            // guarantees that _httpClient is initialized. If it failed, you'd get a TypeInitializationException.

            var requestUrl = $"{SunfishApiBaseUrl}/getLampiran/{fileName}";

            try
            {
                // _httpClient is guaranteed to be initialized here by the static constructor.
                using (var gsapiResponse = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!gsapiResponse.IsSuccessStatusCode)
                    {
                        string errorContent = await gsapiResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Sunfish API Error ({gsapiResponse.StatusCode}): {errorContent}");
                        return Request.CreateErrorResponse(gsapiResponse.StatusCode, $"Failed to retrieve the file from the main server: {gsapiResponse.ReasonPhrase}");
                    }

                    byte[] fileBytes = await gsapiResponse.Content.ReadAsByteArrayAsync();

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(fileBytes)
                    };

                    // Copy critical headers from the original response to the new response
                    response.Content.Headers.ContentType = gsapiResponse.Content.Headers.ContentType;
                    response.Content.Headers.ContentDisposition = gsapiResponse.Content.Headers.ContentDisposition;

                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLampiranProxy network error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Could not connect to the file service.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLampiranProxy general error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An internal error occurred on the proxy server.");
            }
        }
        #endregion
    }
}