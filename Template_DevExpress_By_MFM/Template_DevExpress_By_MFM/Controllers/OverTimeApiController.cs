using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class OverTimeController : ApiController
    {
        // ================================
        // KONSTANTA API
        // ================================
        private const string API_BASE = "http://10.19.101.6/sfapi";
        private const string MAIN_API_URL = API_BASE + "/index.cfm";     // AUTH + DATA
        private const string AUTH_URL = API_BASE + "/oauth.cfm";        // REFRESH TOKEN

        private const string CLIENT_ID = "C14656D2FEB733395E70BCB4B855ADD1BA525D55-7FAF6DEC4AC92A135C6CE90C58DC651005AD21BB";
        private const string CLIENT_SECRET = "B8CAF1E353C61CA544A53370E67336789DFD5996-101843E818707008F1482EEBC5FDCFB1A358297F";
        private const string AUTH_CODE = "Basic C14656D2FEB733395E70BCB4B855ADD1BA525D55-7FAF6DEC4AC92A135C6CE90C58DC651005AD21BB:B8CAF1E353C61CA544A53370E67336789DFD5996-101843E818707008F1482EEBC5FDCFB1A358297F";
        private const string SCOPE = "/gsbattery_FULL_getOvtStatus";
        private const string COMPANY_CODE = "gsbattery";

        //Konfigurasi & Properti
        private const string SunfishApiBaseUrl = "http://localhost:44320/api";
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";


        private static readonly HttpClient _httpClient;

        static OverTimeController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);

            //_httpClient.DefaultRequestHeaders.Add("client_id", CLIENT_ID);
            //_httpClient.DefaultRequestHeaders.Add("client_secret", CLIENT_SECRET);
        }

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

        private async Task<HttpResponseMessage> ForwardJsonPostToSunfishApi(string url, object body)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                var proxyResponse = Request.CreateResponse(response.StatusCode);
                proxyResponse.Content = new StringContent(responseBody, Encoding.UTF8, "application/json");

                return proxyResponse;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway,
                    $"Proxy Sunfish API error: {ex.Message}");
            }
        }


        [SessionCheck]
        [HttpGet]
        [Route("api/OvertimeApi/auth")]
        public async Task<HttpResponseMessage> GetOvertimeAuth()
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");

            // Encode redirect url untuk keamanan
            var redirectUri = HttpUtility.UrlEncode("http://10.19.101.6/sfapi/oauth.cfm?callback");

            // SUSUN URL EXACT PERSIS SEPERTI POSTMAN
            var requestUrl = $"{MAIN_API_URL}" +
                             $"?endpoint=authorize" +
                             $"&response_type=json" +
                             $"&client_id={CLIENT_ID}" +
                             $"&redirect_uri={redirectUri}" +
                             $"&scope={HttpUtility.UrlEncode(SCOPE)}";

            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/OvertimeApi/token")]
        public async Task<HttpResponseMessage> PostToken([FromBody] TokenDto model)
        {
            try
            {
                string code = model.code;

                string url = $"{MAIN_API_URL}?endpoint=token";

                using (var client = new HttpClient())
                {
                    // EXACT HEADER SEPERTI POSTMAN
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization",
                        "Basic C14656D2FEB733395E70BCB4B855ADD1BA525D55-7FAF6DEC4AC92A135C6CE90C58DC651005AD21BB:B8CAF1E353C61CA544A53370E67336789DFD5996-101843E818707008F1482EEBC5FDCFB1A358297F"
                    );

                    // EXACT BODY SEPERTI POSTMAN (x-www-form-urlencoded)
                    var bodyData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Code", code),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

                    var content = new FormUrlEncodedContent(bodyData);

                    // DEBUG (cek value code benar)
                    foreach (var pair in bodyData)
                        Debug.WriteLine(pair.Key + " = " + pair.Value);

                    // EXECUTE CALL EXACTLY AS POSTMAN
                    var response = await client.PostAsync(url, content);

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [SessionCheck]
        [HttpPost]
        [Route("api/OvertimeApi/refresh")]
        public async Task<HttpResponseMessage> RefreshToken([FromBody] RefreshDto model)
        {
            try
            {
                string refreshToken = model.refresh_token;

                string url = $"{MAIN_API_URL}?endpoint=Refresh_token";

                using (var client = new HttpClient())
                {
                    // HEADER: (Postman tidak pakai Authorization untuk refresh)
                    client.DefaultRequestHeaders.Clear();

                    var bodyData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),
                new KeyValuePair<string, string>("client_id", CLIENT_ID),
                new KeyValuePair<string, string>("grant_type", "Refresh_token") // CASE SENSITIVE
            };

                    // DEBUG
                    foreach (var p in bodyData)
                        Debug.WriteLine($"BODY: {p.Key} = {p.Value}");

                    var content = new FormUrlEncodedContent(bodyData);

                    var response = await client.PostAsync(url, content);

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [SessionCheck]
        [HttpPost]
        [Route("api/OvertimeApi/getStatus")]
        public async Task<HttpResponseMessage> GetOvertimeStatus(GetStatusDto model)
        {
            try
            {
                // Ambil access token dari frontend
                var accessToken = HttpContext.Current.Request.Headers["Authorization"]
                    ?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(accessToken))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Missing access token");

                string url =
                    $"{MAIN_API_URL}?endpoint=/gsbattery_FULL_getOvtStatus" +
                    $"&var_nik={model.nik}" +
                    $"&var_com={model.com}" +
                    $"&var_tgl={model.tgl}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                    var response = await client.GetAsync(url);
                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/OvertimeApi/list")]
        public async Task<HttpResponseMessage> GetOvertimeList([FromBody] object model)
        {
            string apiUrl = $"{SunfishApiBaseUrl}/gstracker/ovt/ListOvertime";

            return await ForwardJsonPostToSunfishApi(apiUrl, model);
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/OvertimeApi/listHc")]
        public async Task<HttpResponseMessage> GetOvertimeListHc([FromBody] object model)
        {
            string apiUrl = $"{SunfishApiBaseUrl}/gstracker/ovt/ListOvertimeHc";

            return await ForwardJsonPostToSunfishApi(apiUrl, model);
        }

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

    }
}
