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
    public class IDLApiController : ApiController
    {
        #region Konfigurasi & Properti
        //private const string SunfishApiBaseUrl = "http://10.19.101.146:44320/api/gstracker";
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker";

        // Kredensial API Sunfish
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        // Path upload file IDL di server
        private static readonly string IdlMainUploadPath;

        static IDLApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);

            // Inisialisasi path upload
            IdlMainUploadPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/IDL");

            // Pastikan folder upload ada
            if (!Directory.Exists(IdlMainUploadPath))
            {
                Directory.CreateDirectory(IdlMainUploadPath);
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

        #region === ENDPOINT IDL ===
        // ===================================================================
        // === PROXY: GET /api/IDLApi/list_kegiatan
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/list_kegiatan")]
        public async Task<HttpResponseMessage> GetIdlKegiatanListProxy()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET IDL KEGIATAN LIST PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Build URL untuk backend API (sesuai endpoint yang tersedia di Sunfish API)
                var requestUrl = $"{SunfishApiBaseUrl}/idl/list_kegiatan";

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");

                // Gunakan metode helper yang sudah ada untuk meneruskan request GET
                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlKegiatanListProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET IDL KEGIATAN LIST PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IDLApi/all-applicants (FILTER BY TAHUN)
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/all-applicants")]
        public async Task<HttpResponseMessage> GetAllIdlApplicantsProxy(
            [FromUri] string status = null,
            [FromUri] string tahun = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET ALL IDL APPLICANTS PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Build URL untuk backend API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/all-applicants";

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
                System.Diagnostics.Debug.WriteLine($"Error in GetAllIdlApplicantsProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET ALL IDL APPLICANTS PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IDLApi/list
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/list")]
        public async Task<HttpResponseMessage> GetIdlListProxy(
            [FromUri] string npk = null,
            [FromUri] string status = null,
            [FromUri] string tahun = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET IDL LIST PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Jika npk tidak disediakan, gunakan npk dari session
                if (string.IsNullOrWhiteSpace(npk))
                    npk = session.npk;

                // Validasi parameter wajib
                if (string.IsNullOrWhiteSpace(npk))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Parameter 'npk' diperlukan.");
                }

                // Build URL untuk backend API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/list?npk={Uri.EscapeDataString(npk)}";

                // Tambahkan parameter opsional jika ada
                var queryParams = new List<string>();

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
                    requestUrl += "&" + string.Join("&", queryParams);
                }

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");
                System.Diagnostics.Debug.WriteLine($"Parameters - npk: {npk}, status: {status}, tahun: {tahun}");

                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlListProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET IDL LIST PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IDLApi/listbawahan
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/listbawahan")]
        public async Task<HttpResponseMessage> GetIdlBawahanProxy(
            [FromUri] string supervisor_npk,
            [FromUri] string status = null,
            [FromUri] string tahun = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET IDL BAWAHAN PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Validasi parameter wajib
                if (string.IsNullOrWhiteSpace(supervisor_npk))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Parameter 'supervisor_npk' diperlukan.");
                }

                // Build URL untuk backend API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/listbawahan?supervisor_npk={Uri.EscapeDataString(supervisor_npk)}";

                // Tambahkan parameter opsional jika ada
                var queryParams = new List<string>();

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
                    requestUrl += "&" + string.Join("&", queryParams);
                }

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");
                System.Diagnostics.Debug.WriteLine($"Parameters - supervisor_npk: {supervisor_npk}, status: {status}, tahun: {tahun}");

                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlBawahanProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET IDL BAWAHAN PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IDLApi/detail/{id}
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/detail/{id}")]
        public async Task<HttpResponseMessage> GetIdlDetailProxy(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET IDL DETAIL PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var npk = session.npk;

                // Build URL untuk backend API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/detail/{id}/{npk}";

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");

                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlDetailProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET IDL DETAIL PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: GET /api/IDLApi/detail/approval/{id}
        // ===================================================================
        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/detail/approval/{id}")]
        public async Task<HttpResponseMessage> GetIdlDetailApprovalProxy(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START GET IDL DETAIL APPROVAL PROXY ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Build URL untuk backend API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/detail/approval/{id}";

                System.Diagnostics.Debug.WriteLine($"Forwarding request to: {requestUrl}");

                var response = await ForwardJsonGetRequestToSunfishApi(requestUrl);

                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlDetailApprovalProxy: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + ex.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("=== END GET IDL DETAIL APPROVAL PROXY ===");
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/create
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/create")]
        public async Task<HttpResponseMessage> CreateIdlProxy()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== START CREATE IDL PROXY ===");

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
                var request = new IDLCreateRequest();
                HttpContent fileContent = null;

                // Process form data
                foreach (var content in provider.Contents)
                {
                    var fieldName = content.Headers.ContentDisposition.Name?.Replace("\"", "");
                    var fieldValue = await content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"Processing field: {fieldName} = {fieldValue}");

                    switch (fieldName)
                    {
                        case "idl_npk":
                            request.idl_npk = fieldValue;
                            break;
                        case "idl_jenis_kegiatan":
                            request.idl_jenis_kegiatan = fieldValue;
                            break;
                        case "idl_kategori_kendaraan":
                            request.idl_kategori_kendaraan = fieldValue;
                            break;
                        case "idl_waktu_berangkat":
                            if (DateTime.TryParse(fieldValue, out var berangkat))
                                request.idl_waktu_berangkat = berangkat;
                            break;
                        case "idl_waktu_kembali":
                            if (DateTime.TryParse(fieldValue, out var kembali))
                                request.idl_waktu_kembali = kembali;
                            break;
                        case "idl_lokasi_pertama":
                            request.idl_lokasi_pertama = fieldValue;
                            break;
                        case "idl_lokasi_kedua":
                            request.idl_lokasi_kedua = fieldValue;
                            break;
                        case "idl_lokasi_ketiga":
                            request.idl_lokasi_ketiga = fieldValue;
                            break;
                        case "idl_keterangan":
                            request.idl_keterangan = fieldValue;
                            break;
                        case "idl_created_by":
                            request.idl_created_by = fieldValue;
                            break;
                        case "idl_sopir":
                            request.idl_sopir = fieldValue;
                            break;
                        case "idl_no_polisi":
                            request.idl_no_polisi = fieldValue;
                            break;
                        case "idl_berkas_lampiran":
                            fileContent = content;
                            break;
                    }
                }

                // Validasi
                if (string.IsNullOrWhiteSpace(request.idl_npk))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "NPK harus diisi.");

                if (string.IsNullOrWhiteSpace(request.idl_jenis_kegiatan))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Jenis kegiatan harus diisi.");

                // Set default values
                if (string.IsNullOrWhiteSpace(request.idl_npk))
                    request.idl_npk = session.npk;

                if (string.IsNullOrWhiteSpace(request.idl_created_by))
                    request.idl_created_by = session.npk;

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
                        var filePath = Path.Combine(IdlMainUploadPath, uniqueFileName);
                        File.WriteAllBytes(filePath, fileData);
                        request.idl_berkas_lampiran = uniqueFileName;
                    }
                }

                // BUAT MULTIPART CONTENT YANG BERISI "values" DAN FILE
                var multipartContent = new MultipartFormDataContent();

                // Tambahkan field "values" yang berisi JSON
                var jsonValues = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                multipartContent.Add(new StringContent(jsonValues, Encoding.UTF8, "application/json"), "values");

                // Tambahkan file jika ada
                if (fileContent != null && !string.IsNullOrEmpty(request.idl_berkas_lampiran))
                {
                    var fileBytes = File.ReadAllBytes(Path.Combine(IdlMainUploadPath, request.idl_berkas_lampiran));
                    var fileContentToSend = new ByteArrayContent(fileBytes);
                    fileContentToSend.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContentToSend, "idl_berkas_lampiran", request.idl_berkas_lampiran);
                }

                // Forward ke Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/create";
                System.Diagnostics.Debug.WriteLine($"Forwarding to: {requestUrl}");

                return await ForwardPostRequestToSunfishApi(requestUrl, multipartContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateIdlProxy: {ex}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/approve-by-atasan
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/approve-by-atasan")]
        public async Task<HttpResponseMessage> ApproveBySupervisorProxy([FromBody] IDLApprovalRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/approve-by-atasan";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/reject-by-atasan
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/reject-by-atasan")]
        public async Task<HttpResponseMessage> RejectBySupervisorProxy([FromBody] IDLRejectionRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/reject-by-atasan";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/approve-by-ga
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/approve-by-ga")]
        public async Task<HttpResponseMessage> ApproveByGAProxy([FromBody] IDLGAApprovalRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/approve-by-ga";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/reject-by-ga
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/reject-by-ga")]
        public async Task<HttpResponseMessage> RejectByGAProxy([FromBody] IDLRejectionRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/reject-by-ga";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/complete
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/complete")]
        public async Task<HttpResponseMessage> CompleteIdlProxy([FromBody] IDLCompleteRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/complete";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/cancel
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/cancel")]
        public async Task<HttpResponseMessage> CancelIdlProxy([FromBody] IDLCancelRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set modified_by dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.modified_by))
                    request.modified_by = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/cancel";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/absensi
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/absensi")]
        public async Task<HttpResponseMessage> RecordAbsensiProxy([FromBody] IDLAbsensiRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set npk dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.npk))
                    request.npk = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/absensi";
                var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await ForwardPostRequestToSunfishApi(requestUrl, content);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===================================================================
        // === PROXY: POST /api/IDLApi/selesai
        // ===================================================================
        [SessionCheck]
        [HttpPost]
        [Route("api/IDLApi/selesai")]
        public async Task<HttpResponseMessage> MarkAsSelesaiProxy([FromBody] IDLSelesaiRequest request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                // Set npk dari session jika tidak disediakan
                if (string.IsNullOrWhiteSpace(request.npk))
                    request.npk = session.npk;

                var requestUrl = $"{SunfishApiBaseUrl}/idl/selesai";
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

        #region --- IDL File & Image Serving Proxy ---

        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/file/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetIdlFileProxy(int id, string fileKey)
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

                // Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/file/{id}/{fileKey}/{npk}";

                System.Diagnostics.Debug.WriteLine($"Forwarding to Sunfish: {requestUrl}");

                var response = await ForwardAnyGetRequestToSunfishApi(requestUrl);
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlFileProxy: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/pdfInfo/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetIdlPdfInfoProxy(int id, string fileKey)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid");

                var npk = session.npk;
                // Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/pdfInfo/{id}/{fileKey}/{npk}";

                System.Diagnostics.Debug.WriteLine($"Forwarding PDF info request to: {requestUrl}");
                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlPdfInfoProxy: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/pdfImage/{imageName}")]
        public async Task<HttpResponseMessage> GetIdlPdfImageProxy(string imageName)
        {
            try
            {
                // Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/pdfImage/{imageName}";
                System.Diagnostics.Debug.WriteLine($"Forwarding PDF image request to: {requestUrl}");
                return await ForwardAnyGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlPdfImageProxy: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region --- IDL File & Image Serving Proxy (Tanpa Akses NPK) ---

        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/file/approval/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetIdlFileProxyForApproval(int id, string fileKey)
        {
            try
            {
                // Hapus query string dari fileKey jika ada
                int queryIndex = fileKey.IndexOf('?');
                if (queryIndex > 0)
                {
                    fileKey = fileKey.Substring(0, queryIndex);
                }

                // Menggunakan endpoint tanpa NPK untuk approval
                var requestUrl = $"{SunfishApiBaseUrl}/idl/file/{id}/{fileKey}";

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
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlFileProxyForApproval: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/pdfInfo/approval/{id:int}/{fileKey}")]
        public async Task<HttpResponseMessage> GetIdlPdfInfoProxyForApproval(int id, string fileKey)
        {
            try
            {
                // Menggunakan endpoint tanpa NPK untuk approval
                var requestUrl = $"{SunfishApiBaseUrl}/idl/pdfInfo/{id}/{fileKey}";

                System.Diagnostics.Debug.WriteLine($"Forwarding PDF info request to: {requestUrl}");
                return await ForwardJsonGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlPdfInfoProxyForApproval: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/IDLApi/pdfImage/{imageName}")]
        public async Task<HttpResponseMessage> GetIdlPdfImageProxyApproval(string imageName)
        {
            try
            {
                // Format URL yang BENAR untuk Sunfish API
                var requestUrl = $"{SunfishApiBaseUrl}/idl/pdfImage/{imageName}";
                System.Diagnostics.Debug.WriteLine($"Forwarding PDF image request to: {requestUrl}");
                return await ForwardAnyGetRequestToSunfishApi(requestUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdlPdfImageProxyApproval: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Model Classes untuk Request

        public class IDLCreateRequest
        {
            public string idl_npk { get; set; }
            public string idl_jenis_kegiatan { get; set; }
            public string idl_kategori_kendaraan { get; set; }
            public DateTime? idl_waktu_berangkat { get; set; }
            public DateTime? idl_waktu_kembali { get; set; }
            public string idl_lokasi_pertama { get; set; }
            public string idl_lokasi_kedua { get; set; }
            public string idl_lokasi_ketiga { get; set; }
            public string idl_keterangan { get; set; }
            public string idl_berkas_lampiran { get; set; }
            public string idl_created_by { get; set; }
            public string idl_sopir { get; set; }
            public string idl_no_polisi { get; set; }
        }

        public class IDLApprovalRequest
        {
            public int idl_id { get; set; }
            public string modified_by { get; set; }
        }

        public class IDLGAApprovalRequest
        {
            public int idl_id { get; set; }
            public string modified_by { get; set; }
            public string idl_sopir { get; set; }
            public string idl_no_polisi { get; set; }
        }

        public class IDLRejectionRequest
        {
            public int idl_id { get; set; }
            public string alasan_penolakan { get; set; }
            public string modified_by { get; set; }
        }

        public class IDLCompleteRequest
        {
            public int idl_id { get; set; }
            public string modified_by { get; set; }
        }

        public class IDLCancelRequest
        {
            public int idl_id { get; set; }
            public string alasan_pembatalan { get; set; }
            public string modified_by { get; set; }
        }

        public class IDLAbsensiRequest
        {
            public int idl_id { get; set; }
            public string npk { get; set; }
            public int lokasi_ke { get; set; }
            public string lokasi_aktual { get; set; }
        }

        public class IDLSelesaiRequest
        {
            public int idl_id { get; set; }
            public string npk { get; set; }
            public string lokasi_akhir { get; set; }
        }

        #endregion
    }
}