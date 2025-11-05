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
                var sunfishContent = await sunfishResponse.Content.ReadAsStringAsync();

                var proxyResponse = Request.CreateResponse(sunfishResponse.StatusCode);
                proxyResponse.Content = new StringContent(sunfishContent, Encoding.UTF8, "application/json");

                return proxyResponse;
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

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/{id}")]
        public async Task<HttpResponseMessage> GetDetailProxy(int id)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var npk = session.npk;
                var requestUrl = $"{SunfishApiBaseUrl}/imp/detail/{id}/{npk}";

                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi")]
        public async Task<HttpResponseMessage> PostProxy()
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Cek jika request adalah multipart form data
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported Media Type");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                IMPCreateRequest request = null;
                HttpContent fileContent = null;

                // Process form data
                foreach (var content in provider.Contents)
                {
                    if (content.Headers.ContentDisposition.Name == "\"values\"")
                    {
                        var jsonString = await content.ReadAsStringAsync();
                        request = Newtonsoft.Json.JsonConvert.DeserializeObject<IMPCreateRequest>(jsonString);
                    }
                    else if (content.Headers.ContentDisposition.Name == "\"imp_berkas_lampiran\"")
                    {
                        fileContent = content;
                    }
                }

                if (request == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data tidak valid");

                // Set NPK dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.imp_npk))
                    request.imp_npk = session.npk;

                // Process file upload jika ada
                if (fileContent != null)
                {
                    var fileName = fileContent.Headers.ContentDisposition.FileName?.Replace("\"", "");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var fileData = await fileContent.ReadAsByteArrayAsync();
                        // Simpan file atau process sesuai kebutuhan
                        request.imp_berkas_lampiran = fileName;
                        // Anda bisa menyimpan fileData ke storage atau database
                    }
                }

                var requestUrl = $"{SunfishApiBaseUrl}/imp/create";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, stringContent);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/approve-by-atasan")]
        public async Task<HttpResponseMessage> ApproveBySupervisorProxy([FromBody] IMPApprovalRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/imp/approve-by-atasan";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/reject-by-atasan")]
        public async Task<HttpResponseMessage> RejectBySupervisorProxy([FromBody] IMPRejectionRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/imp/reject-by-atasan";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/approve-by-hc")]
        public async Task<HttpResponseMessage> ApproveByHCProxy([FromBody] IMPApprovalRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/imp/approve-by-hc";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/reject-by-hc")]
        public async Task<HttpResponseMessage> RejectByHCProxy([FromBody] IMPRejectionRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/imp/reject-by-hc";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/update-berangkat-aktual")]
        public async Task<HttpResponseMessage> UpdateBerangkatAktualProxy([FromBody] IMPUpdateBerangkatRequest request)
        {
            try
            {
                var requestUrl = $"{SunfishApiBaseUrl}/imp/update-berangkat-aktual";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/update-kembali-aktual")]
        public async Task<HttpResponseMessage> UpdateKembaliAktualProxy([FromBody] IMPUpdateKembaliRequest request)
        {
            try
            {
                var requestUrl = $"{SunfishApiBaseUrl}/imp/update-kembali-aktual";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/IMPApi/cancel")]
        public async Task<HttpResponseMessage> CancelImpProxy([FromBody] IMPCancelRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/imp/cancel";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Model Classes untuk Request

        public class IMPCreateRequest
        {
            public string imp_npk { get; set; }
            public string imp_jenis_kegiatan { get; set; }
            public string imp_waktu_izin { get; set; }
            public DateTime? imp_tanggal_berangkat { get; set; }
            public TimeSpan? imp_waktu_berangkat { get; set; }
            public DateTime? imp_tanggal_kembali { get; set; }
            public TimeSpan? imp_waktu_kembali { get; set; }
            public string imp_keterangan { get; set; }
            public string imp_shift { get; set; }
            public string imp_berkas_lampiran { get; set; }
        }

        public class IMPApprovalRequest
        {
            public int imp_id { get; set; }
            public string modified_by { get; set; }
        }

        public class IMPRejectionRequest
        {
            public int imp_id { get; set; }
            public string alasan_penolakan { get; set; }
            public string modified_by { get; set; }
        }

        public class IMPUpdateBerangkatRequest
        {
            public int imp_id { get; set; }
        }

        public class IMPUpdateKembaliRequest
        {
            public int imp_id { get; set; }
        }

        public class IMPCancelRequest
        {
            public int imp_id { get; set; }
            public string alasan_pembatalan { get; set; }
            public string modified_by { get; set; }
        }

        #endregion
    }
}