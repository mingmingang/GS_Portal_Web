using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class IMPApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api/gstracker";

        // Kredensial API Sunfish
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        // Path upload file IMP di server - DIUBAH MENJADI RELATIVE PATH
        private static readonly string ImpMainUploadPath;

        static IMPApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);

            // Inisialisasi path upload - DIUBAH
            ImpMainUploadPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/IMP");

            // Pastikan folder upload ada
            if (!Directory.Exists(ImpMainUploadPath))
            {
                Directory.CreateDirectory(ImpMainUploadPath);
            }
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

        /// <summary>
        /// Helper method untuk meneruskan request GET apa saja (file/image/JSON) ke Sunfish API
        /// </summary>
        private async Task<HttpResponseMessage> ForwardAnyGetRequestToSunfishApi(string url)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Making request to Sunfish: {url}");

                using (var client = new HttpClient())
                {
                    // Add required headers
                    client.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
                    client.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Get response with headers first
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                    System.Diagnostics.Debug.WriteLine($"Sunfish Response Status: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        // Create the response message
                        var result = new HttpResponseMessage(response.StatusCode);

                        // Copy content
                        var contentStream = await response.Content.ReadAsStreamAsync();
                        result.Content = new StreamContent(contentStream);

                        // Copy content headers
                        foreach (var header in response.Content.Headers)
                        {
                            result.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        return result;
                    }
                    else
                    {
                        // Return the error response as-is
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Sunfish Error Content: {errorContent}");

                        return Request.CreateErrorResponse(response.StatusCode,
                            $"Sunfish API returned: {response.StatusCode} - {errorContent}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HttpRequestException: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway,
                    $"Tidak dapat terhubung ke service Sunfish: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Request timeout: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.RequestTimeout,
                    "Request timeout ke Sunfish API");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General exception: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    $"Error: {ex.Message}");
            }
        }
        #endregion

        #region === ENDPOINT IMP ===

        // ===================================================================
        // === PROXY: GET /api/IMPApi/listbawahan (GET IMP BAWAHAN)
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/listbawahan")]
        public async Task<HttpResponseMessage> GetImpBawahanProxy(
        [FromUri] string supervisor_npk = null,
        [FromUri] string status = null,
        [FromUri] string tahun = null)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Gunakan NPK dari session jika tidak ada parameter
                if (string.IsNullOrWhiteSpace(supervisor_npk))
                    supervisor_npk = session.npk;

                // Build URL sederhana
                var requestUrl = $"{SunfishApiBaseUrl}/imp/listbawahan?supervisor_npk={Uri.EscapeDataString(supervisor_npk)}";

                if (!string.IsNullOrWhiteSpace(status) && status != "all")
                    requestUrl += $"&status={Uri.EscapeDataString(status)}";

                if (!string.IsNullOrWhiteSpace(tahun))
                    requestUrl += $"&tahun={Uri.EscapeDataString(tahun)}";

                Console.WriteLine($"Proxy Request: {requestUrl}");

                // Panggil backend langsung
                var response = await _httpClient.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Backend Response: {response.StatusCode}");

                // Kembalikan response AS-IS dari backend
                return new HttpResponseMessage(response.StatusCode)
                {
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Proxy Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    $"Error: {ex.Message}");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IMPApi/all-applicants (FILTER BY TAHUN)
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/all-applicants")]
        public async Task<HttpResponseMessage> GetAllImpApplicantsProxy(
            [FromUri] string status = null,
            [FromUri] string tahun = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET ALL IMP APPLICANTS PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Build URL untuk backend API - DIUBAH: menghilangkan duplikasi path
                var requestUrl = $"{SunfishApiBaseUrl}/imp/all-applicants";

                // Tambahkan parameter jika ada
                var queryParams = new System.Collections.Generic.List<string>();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                }

                if (!string.IsNullOrWhiteSpace(tahun))
                {
                    queryParams.Add($"tahun={Uri.EscapeDataString(tahun)}");
                }

                if (queryParams.Count > 0)
                {
                    requestUrl += "?" + string.Join("&", queryParams);
                }

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");

                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllImpApplicantsProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET ALL IMP APPLICANTS PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IMPApi/detail/approval/{id}
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/detail/approval/{id}")]
        public async Task<HttpResponseMessage> GetImpDetailApprovalProxy(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET IMP DETAIL APPROVAL PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Build URL untuk backend API
                var requestUrl = $"{SunfishApiBaseUrl}/imp/detail/approval/{id}";

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");

                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpDetailApprovalProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET IMP DETAIL APPROVAL PROXY ===");
            }
        }

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

                // DIUBAH: URL yang benar untuk list IMP
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

                // DIUBAH: URL yang benar untuk list IMP
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
                // DIUBAH: URL yang benar untuk detail IMP
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
                System.Diagnostics.Debug.WriteLine("=== START IMP API PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Session is null");
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");
                }

                // Cek jika request adalah multipart form data
                if (!Request.Content.IsMimeMultipartContent())
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Not multipart content");
                    return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported Media Type");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                System.Diagnostics.Debug.WriteLine($"Multipart contents: {provider.Contents.Count}");

                // Buat objek request untuk dikirim sebagai "values"
                var request = new IMPCreateRequest();
                HttpContent fileContent = null;

                // Process form data
                foreach (var content in provider.Contents)
                {
                    var fieldName = content.Headers.ContentDisposition.Name?.Replace("\"", "");
                    var fieldValue = await content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"Processing field: {fieldName} = {fieldValue}");

                    switch (fieldName)
                    {
                        case "imp_npk":
                            request.imp_npk = fieldValue;
                            break;
                        case "imp_jenis_kegiatan":
                            request.imp_jenis_kegiatan = fieldValue;
                            break;
                        case "imp_waktu_izin":
                            request.imp_waktu_izin = fieldValue;
                            break;
                        case "imp_shift":
                            request.imp_shift = fieldValue;
                            break;
                        case "imp_waktu_berangkat":
                            if (DateTime.TryParse(fieldValue, out var berangkat))
                                request.imp_waktu_berangkat = berangkat;
                            break;
                        case "imp_waktu_kembali":
                            if (DateTime.TryParse(fieldValue, out var kembali))
                                request.imp_waktu_kembali = kembali;
                            break;
                        case "imp_keterangan":
                            request.imp_keterangan = fieldValue;
                            break;
                        case "imp_no_request":
                            request.imp_no_request = fieldValue;
                            break;
                        case "imp_created_by":
                            request.imp_created_by = fieldValue;
                            break;
                        case "imp_berangkat_aktual":
                            if (DateTime.TryParse(fieldValue, out var berangkatAktual))
                                request.imp_berangkat_aktual = berangkatAktual;
                            break;
                        case "imp_kembali_aktual":
                            if (DateTime.TryParse(fieldValue, out var kembaliAktual))
                                request.imp_kembali_aktual = kembaliAktual;
                            break;
                        case "imp_berkas_lampiran":
                            fileContent = content;
                            break;
                    }
                }

                // Validasi
                if (string.IsNullOrWhiteSpace(request.imp_npk))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "NPK harus diisi.");

                if (string.IsNullOrWhiteSpace(request.imp_jenis_kegiatan))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Jenis kegiatan harus diisi.");

                // Set default values
                if (string.IsNullOrWhiteSpace(request.imp_npk))
                    request.imp_npk = session.npk;

                if (string.IsNullOrWhiteSpace(request.imp_created_by))
                    request.imp_created_by = session.npk;

                // Process file
                if (fileContent != null)
                {
                    var fileName = fileContent.Headers.ContentDisposition.FileName?.Replace("\"", "");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var fileData = await fileContent.ReadAsByteArrayAsync();
                        if (fileData.Length > 2 * 1024 * 1024)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Ukuran file melebihi 2MB");

                        var fileExtension = Path.GetExtension(fileName);
                        var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";
                        var filePath = Path.Combine(ImpMainUploadPath, uniqueFileName);
                        File.WriteAllBytes(filePath, fileData);
                        request.imp_berkas_lampiran = uniqueFileName;
                    }
                }

                // BUAT MULTIPART CONTENT YANG BERISI "values" DAN FILE
                var multipartContent = new MultipartFormDataContent();

                // Tambahkan field "values" yang berisi JSON
                var jsonValues = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                multipartContent.Add(new StringContent(jsonValues, Encoding.UTF8, "application/json"), "values");

                // Tambahkan file jika ada
                if (fileContent != null && !string.IsNullOrEmpty(request.imp_berkas_lampiran))
                {
                    var fileBytes = File.ReadAllBytes(Path.Combine(ImpMainUploadPath, request.imp_berkas_lampiran));
                    var fileContentToSend = new ByteArrayContent(fileBytes);
                    fileContentToSend.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContentToSend, "imp_berkas_lampiran", request.imp_berkas_lampiran);
                }

                // Forward ke Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/imp/create";
                System.Diagnostics.Debug.WriteLine($"Forwarding to: {requestUrl}");

                return await ForwardPostRequestToSunfishApi(requestUrl, multipartContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PostProxy: {ex}");
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

                // DIUBAH: URL yang benar untuk approve by atasan
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

                // DIUBAH: URL yang benar untuk reject by atasan
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

                // DIUBAH: URL yang benar untuk approve by HC
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

                // DIUBAH: URL yang benar untuk reject by HC
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
                // DIUBAH: URL yang benar untuk update berangkat aktual
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
                // DIUBAH: URL yang benar untuk update kembali aktual
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

                // DIUBAH: URL yang benar untuk cancel IMP
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

        #region --- IMP File & Image Serving Proxy ---

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/file/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetImpFileProxy(int id, string fileKey)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var npk = session.npk;
                if (string.IsNullOrWhiteSpace(npk))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "NPK tidak ditemukan di session.");

                // DEBUG: Log the request
                System.Diagnostics.Debug.WriteLine($"=== FILE REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"ID: {id}, FileKey: {fileKey}, NPK: {npk}");

                // DIUBAH: Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/imp/file/{id}/{fileKey}/{npk}";

                System.Diagnostics.Debug.WriteLine($"Forwarding to Sunfish: {requestUrl}");

                var response = await ForwardAnyGetRequestToSunfishApi(requestUrl);
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpFileProxy: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/pdfInfo/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetImpPdfInfoProxy(int id, string fileKey)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var npk = session.npk;
                // DIUBAH: Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/imp/pdfInfo/{id}/{fileKey}/{npk}";

                System.Diagnostics.Debug.WriteLine($"Forwarding PDF info request to: {requestUrl}");
                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpPdfInfoProxy: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/pdfImage/{imageName}")]
        public async Task<HttpResponseMessage> GetImpPdfImageProxy(string imageName)
        {
            try
            {
                // DIUBAH: Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/imp/pdfImage/{imageName}";
                System.Diagnostics.Debug.WriteLine($"Forwarding PDF image request to: {requestUrl}");
                return await ForwardAnyGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpPdfImageProxy: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region --- IMP File & Image Serving Proxy (Tanpa Akses NPK) ---

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/file/approval/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetImpFileProxyForApproval(int id, string fileKey)
        {
            try
            {
                // Hapus query string dari fileKey jika ada
                int queryIndex = fileKey.IndexOf('?');
                if (queryIndex > 0)
                {
                    fileKey = fileKey.Substring(0, queryIndex);
                }

                // DIUBAH: Menggunakan endpoint tanpa NPK untuk approval
                var requestUrl = $"{SunfishApiBaseUrl}/imp/file/{id}/{fileKey}";

                System.Diagnostics.Debug.WriteLine($"=== FILE REQUEST (NO NPK) ===");
                System.Diagnostics.Debug.WriteLine($"ID: {id}, FileKey: {fileKey}");
                System.Diagnostics.Debug.WriteLine($"Forwarding to Sunfish: {requestUrl}");
                System.Diagnostics.Debug.WriteLine($"Request URI: {Request.RequestUri}");

                var response = await ForwardAnyGetRequestToSunfishApi(requestUrl);
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpFileProxyForApproval: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/pdfInfo/approval/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetImpPdfInfoProxyForApproval(int id, string fileKey)
        {
            try
            {
                // DIUBAH: Menggunakan endpoint tanpa NPK untuk approval
                var requestUrl = $"{SunfishApiBaseUrl}/imp/pdfInfo/{id}/{fileKey}";

                System.Diagnostics.Debug.WriteLine($"Forwarding PDF info request to: {requestUrl}");
                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpPdfInfoProxyForApproval: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // GetImpPdfImageProxy tidak diubah karena route-nya sudah unik (berbeda parameter)
        [SessionCheck]
        [HttpGet]
        [Route("api/IMPApi/pdfImage/{imageName}")]
        public async Task<HttpResponseMessage> GetImpPdfImageProxyApproval(string imageName)
        {
            try
            {
                // DIUBAH: Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/imp/pdfImage/{imageName}";
                System.Diagnostics.Debug.WriteLine($"Forwarding PDF image request to: {requestUrl}");
                return await ForwardAnyGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetImpPdfImageProxy: {ex.Message}");
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
            public DateTime? imp_waktu_berangkat { get; set; }
            public DateTime? imp_waktu_kembali { get; set; }
            public string imp_keterangan { get; set; }
            public string imp_shift { get; set; }
            public string imp_berkas_lampiran { get; set; }
            public string imp_no_request { get; set; }
            public string imp_created_by { get; set; }
            public DateTime? imp_berangkat_aktual { get; set; }
            public DateTime? imp_kembali_aktual { get; set; }
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