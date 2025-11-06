using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using Newtonsoft.Json;

namespace Template_DevExpress_By_MFM.Controllers
{
    #region === DTO CLASSES ===

    /// <summary>
    /// DTO untuk request update status
    /// </summary>
    public class UpdateStatusRequestDto
    {
        public string Status { get; set; }
    }

    /// <summary>
    /// DTO untuk request create Permintaan ID Card
    /// </summary>
    public class CreatePicRequestDto
    {
        public string PicAh { get; set; }
    }

    /// <summary>
    /// DTO untuk request create Permintaan Surat Keterangan
    /// </summary>
    public class CreateSkpRequestDto
    {
        public string SkpAp { get; set; }
        public string SkpKet { get; set; }
    }

    #endregion

    /// <summary>
    /// API Controller untuk proxy request ke GsTracker API
    /// PENTING: Nama controller harus sesuai dengan route: "PermintaanApi"
    /// </summary>
    [RoutePrefix("api/PermintaanApi")]
    public class PermintaanApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string GsTrackerApiBaseUrl = "http://10.19.101.146:44321/api/gstracker";
        private static readonly HttpClient _httpClient;

        static PermintaanApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Increase timeout
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion

        #region Helper Methods
        private async Task<HttpResponseMessage> ForwardJsonGetRequestToGsTrackerApi(string url, string filterByNpk = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY] Forwarding to: {url}");

                var gsTrackerResponse = await _httpClient.GetAsync(url);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[PROXY] Backend Status: {gsTrackerResponse.StatusCode}");

                // Filter data jika diperlukan (untuk role Karyawan)
                // HC dan Atasan mendapatkan semua data tanpa filter
                if (!string.IsNullOrEmpty(filterByNpk))
                {
                    gsTrackerContent = FilterResponseByNpk(gsTrackerContent, filterByNpk);
                }

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

        #region Helper Methods untuk Session Validation

        private IHttpActionResult ValidateUserAccess(string requestedNpk)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;

            if (session == null)
            {
                return Unauthorized();
            }

            var userRole = (session.userjabatan ?? "").Trim().ToLower();
            var sessionNpk = (session.npk ?? "").Trim();

            // HC dan Atasan dapat mengakses semua data
            if (userRole == "hc" || userRole == "atasan")
            {
                System.Diagnostics.Debug.WriteLine($"[ACCESS] {userRole.ToUpper()} role detected - Full access granted");
                return null; // Izinkan akses tanpa validasi NPK
            }

            // Karyawan hanya bisa akses data mereka sendiri
            if (userRole == "karyawan")
            {
                if (!sessionNpk.Equals(requestedNpk?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[SECURITY] Access denied! Session NPK: {sessionNpk}, Requested NPK: {requestedNpk}");

                    return Content(
                        HttpStatusCode.Forbidden,
                        new
                        {
                            status = false,
                            code = 403,
                            message = "Anda tidak memiliki akses untuk melihat data karyawan lain."
                        }
                    );
                }
            }

            return null;
        }

        /// <summary>
        /// Filter response JSON untuk hanya menampilkan data NPK yang sesuai session
        /// HC dan Atasan tidak difilter - melihat semua data
        /// </summary>
        private string FilterResponseByNpk(string jsonResponse, string allowedNpk)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null) return jsonResponse;

                var userRole = (session.userjabatan ?? "").Trim().ToLower();

                // HC dan Atasan melihat semua data tanpa filter
                if (userRole == "hc" || userRole == "atasan")
                {
                    System.Diagnostics.Debug.WriteLine($"[FILTER] {userRole.ToUpper()} role - No filtering applied");
                    return jsonResponse;
                }

                // Hanya filter untuk role Karyawan
                if (userRole != "karyawan") return jsonResponse;

                System.Diagnostics.Debug.WriteLine($"[FILTER] Karyawan role - Filtering for NPK: {allowedNpk}");

                var jsonObj = JObject.Parse(jsonResponse);
                var dataArray = jsonObj["data"] as JArray;

                if (dataArray != null && dataArray.Count > 0)
                {
                    var firstItem = dataArray[0] as JObject;
                    var itemsArray = firstItem?["data"] as JArray;

                    if (itemsArray != null)
                    {
                        // ✅ CRITICAL FIX: Filter HANYA berdasarkan NPK
                        // PRESERVE SEMUA FIELD - Jangan hapus apapun!
                        var filteredItems = new JArray();

                        foreach (var item in itemsArray)
                        {
                            var kryNpk = item["kry_npk"]?.ToString() ?? "";

                            // Cek apakah NPK cocok
                            if (kryNpk.Equals(allowedNpk, StringComparison.OrdinalIgnoreCase))
                            {
                                // ✅ PENTING: Tambahkan SELURUH item tanpa modifikasi
                                // Jangan clone atau manipulasi - langsung add original object
                                filteredItems.Add(item);

                                // Debug: Verify HC fields masih ada
                                var skpModiName = item["skp_modi_by_name"]?.ToString();
                                var skpModiJabatan = item["skp_modi_by_jabatan"]?.ToString();
                                var picModiName = item["pic_modi_by_name"]?.ToString();
                                var picModiJabatan = item["pic_modi_by_jabatan"]?.ToString();

                                System.Diagnostics.Debug.WriteLine($"[FILTER] ✅ Item added for NPK {kryNpk}");
                                System.Diagnostics.Debug.WriteLine($"[FILTER]    - skp_modi_by_name: '{skpModiName}'");
                                System.Diagnostics.Debug.WriteLine($"[FILTER]    - skp_modi_by_jabatan: '{skpModiJabatan}'");
                                System.Diagnostics.Debug.WriteLine($"[FILTER]    - pic_modi_by_name: '{picModiName}'");
                                System.Diagnostics.Debug.WriteLine($"[FILTER]    - pic_modi_by_jabatan: '{picModiJabatan}'");
                                System.Diagnostics.Debug.WriteLine($"[FILTER]    - Total properties: {(item as JObject)?.Properties().Count()}");
                            }
                        }

                        // Update totalCount dan summary
                        var totalCount = filteredItems.Count;
                        var summary = new JObject();

                        foreach (var item in filteredItems)
                        {
                            // Support both PIC and SKP status fields
                            var status = item["pic_status"]?.ToString() ?? item["skp_status"]?.ToString() ?? "";

                            if (!string.IsNullOrEmpty(status))
                            {
                                var statusKey = status.Replace(" ", "");

                                if (summary[statusKey] == null)
                                    summary[statusKey] = 0;

                                summary[statusKey] = (int)summary[statusKey] + 1;
                            }
                        }

                        // Update data array dengan filtered items
                        firstItem["data"] = filteredItems;
                        firstItem["totalCount"] = totalCount;
                        firstItem["summary"] = summary;

                        System.Diagnostics.Debug.WriteLine($"[FILTER] ✅ Filtering complete:");
                        System.Diagnostics.Debug.WriteLine($"[FILTER]    - Original items: {itemsArray.Count}");
                        System.Diagnostics.Debug.WriteLine($"[FILTER]    - Filtered items: {totalCount}");

                        if (filteredItems.Count > 0)
                        {
                            var firstFiltered = filteredItems[0] as JObject;
                            System.Diagnostics.Debug.WriteLine($"[FILTER]    - Properties preserved: {firstFiltered?.Properties().Count()}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[FILTER] ⚠️ No items array found");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[FILTER] ⚠️ No data array found");
                }

                var result = jsonObj.ToString();
                System.Diagnostics.Debug.WriteLine($"[FILTER] Returning filtered JSON (length: {result.Length})");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FILTER ERROR] ❌ {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FILTER ERROR] Stack Trace: {ex.StackTrace}");

                // Return original response jika error
                return jsonResponse;
            }
        }

        #endregion

        #region === ENDPOINT PERMINTAAN PIC (WITH VALIDATION) ===
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
            var validationResult = ValidateUserAccess(npk);
            if (validationResult != null)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new
                {
                    status = false,
                    code = 403,
                    message = "Anda tidak memiliki akses untuk melihat data karyawan lain."
                });
            }

            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            var userRole = (session?.userjabatan ?? "").Trim();

            System.Diagnostics.Debug.WriteLine($"[PROXY GET] User Role from session: {userRole}");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;

            if (!string.IsNullOrEmpty(ah)) queryString["ah"] = ah;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            var requestUrl = $"{GsTrackerApiBaseUrl}/pic?{queryString.ToString()}";

            try
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY] Forwarding to: {requestUrl}");

                // ✅ KIRIM ROLE KE BACKEND VIA HEADER
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("X-User-Role", userRole);

                var gsTrackerResponse = await _httpClient.SendAsync(request);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[PROXY] Backend Status: {gsTrackerResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[PROXY] Backend returned data");

                // ❌ HAPUS FILTER - Backend sudah handle berdasarkan role
                // Filter hanya untuk extra security di proxy level
                var userRoleLower = userRole.ToLower();
                if (userRoleLower == "karyawan")
                {
                    gsTrackerContent = FilterResponseByNpk(gsTrackerContent, npk);
                }

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

        /// <summary>
        /// Create Permintaan ID Card
        /// POST: api/PermintaanApi/pic
        /// </summary>
        [HttpPost]
        [Route("pic")]
        public async Task<IHttpActionResult> CreatePermintaanPic([FromBody] CreatePicRequestDto request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] === START PROCESS ===");

                // Validasi session
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC ERROR] Session is null - User not logged in");
                    return Content(HttpStatusCode.Unauthorized, new
                    {
                        status = false,
                        code = 401,
                        message = "Session expired. Silakan login kembali."
                    });
                }

                // Debug session information
                var userRole = session.userjabatan ?? "";
                var npk = session.npk ?? "";
                var plant = session.plant ?? "";

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] Session Details:");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] - NPK: {npk}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] - Plant: {plant}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] - Role: '{userRole}'");

                // APPROACH BARU: Izinkan semua role yang bukan HC/Atasan
                if (userRole.Equals("hc", StringComparison.OrdinalIgnoreCase) ||
                    userRole.Equals("atasan", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC ACCESS DENIED] Role '{userRole}' cannot create requests");
                    return Content(HttpStatusCode.Forbidden, new
                    {
                        status = false,
                        code = 403,
                        message = $"Anda sedang login sebagai '{userRole}'. Hanya karyawan yang dapat membuat permintaan ID Card."
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Role validation passed");

                // Validasi request body
                if (request == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC ERROR] Request body is null");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Request body tidak boleh kosong"
                    });
                }

                if (string.IsNullOrEmpty(request.PicAh))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC ERROR] PicAh parameter is empty");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'PicAh' harus diisi"
                    });
                }

                // Validasi PicAh value
                var validAh = new[] { "Hilang", "Rusak", "Mutasi" };
                if (!validAh.Contains(request.PicAh))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC ERROR] Invalid PicAh value: {request.PicAh}");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'PicAh' harus salah satu dari: Hilang, Rusak, Mutasi"
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Request validation passed - PicAh: {request.PicAh}");

                // PERBAIKAN PENTING: Kirim UserRole dari session ke backend
                var gsTrackerRequest = new
                {
                    EmpNpk = npk,
                    plant = plant,
                    PicAh = request.PicAh,
                    UserRole = userRole, // Kirim role user dari session
                    ValidatedByProxy = true
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Forward ke GsTracker API
                var url = $"{GsTrackerApiBaseUrl}/pic";
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Forwarding to: {url}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Body: {jsonContent}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] User Role (to backend): {userRole}, NPK: {npk}");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Response Body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC SUCCESS] Permintaan berhasil dibuat");
                    return Ok(result);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC BACKEND ERROR] Backend returned error: {(int)response.StatusCode}");

                    // Try to parse error response
                    try
                    {
                        var errorResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return Content(response.StatusCode, errorResult);
                    }
                    catch
                    {
                        return Content(response.StatusCode, new
                        {
                            status = false,
                            code = (int)response.StatusCode,
                            message = responseContent
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC UNEXPECTED ERROR] {ex.ToString()}");
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    code = 500,
                    message = "Terjadi kesalahan tidak terduga pada server."
                });
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] === END PROCESS ===");
            }
        }
        #endregion

        #region === ENDPOINT PERMINTAAN SKP (WITH VALIDATION) ===

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
            var validationResult = ValidateUserAccess(npk);
            if (validationResult != null)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new
                {
                    status = false,
                    code = 403,
                    message = "Anda tidak memiliki akses untuk melihat data karyawan lain."
                });
            }

            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            var userRole = (session?.userjabatan ?? "").Trim();

            System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP] User Role from session: {userRole}");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;

            if (!string.IsNullOrEmpty(ap)) queryString["ap"] = ap;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            var requestUrl = $"{GsTrackerApiBaseUrl}/skp?{queryString.ToString()}";

            try
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP] Forwarding to: {requestUrl}");

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("X-User-Role", userRole);

                var gsTrackerResponse = await _httpClient.SendAsync(request);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP] Backend Status: {gsTrackerResponse.StatusCode}");

                // ✅ CRITICAL DEBUG: Log backend response SEBELUM filter
                System.Diagnostics.Debug.WriteLine($"[BACKEND RAW] Response length: {gsTrackerContent.Length}");

                // Parse dan check first item
                try
                {
                    var testParse = JObject.Parse(gsTrackerContent);
                    var testData = testParse["data"]?[0]?["data"]?[0];
                    if (testData != null)
                    {
                        var propertyCount = (testData as JObject)?.Properties().Count() ?? 0;
                        var modiName = testData["skp_modi_by_name"]?.ToString() ?? "NULL";
                        var modiJabatan = testData["skp_modi_by_jabatan"]?.ToString() ?? "NULL";

                        System.Diagnostics.Debug.WriteLine($"[BACKEND RAW] First item properties: {propertyCount}");
                        System.Diagnostics.Debug.WriteLine($"[BACKEND RAW] skp_modi_by_name: '{modiName}'");
                        System.Diagnostics.Debug.WriteLine($"[BACKEND RAW] skp_modi_by_jabatan: '{modiJabatan}'");

                        // List all properties
                        if (testData is JObject jObj)
                        {
                            System.Diagnostics.Debug.WriteLine($"[BACKEND RAW] All properties:");
                            foreach (var prop in jObj.Properties())
                            {
                                System.Diagnostics.Debug.WriteLine($"[BACKEND RAW]   - {prop.Name}: {prop.Value}");
                            }
                        }
                    }
                }
                catch (Exception parseEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[BACKEND RAW] Parse error: {parseEx.Message}");
                }

                // Apply filter hanya untuk Karyawan
                var userRoleLower = userRole.ToLower();
                if (userRoleLower == "karyawan")
                {
                    System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP] Applying filter for Karyawan role");
                    gsTrackerContent = FilterResponseByNpk(gsTrackerContent, npk);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP] No filter applied for role: {userRole}");
                }

                var proxyResponse = Request.CreateResponse(gsTrackerResponse.StatusCode);
                proxyResponse.Content = new StringContent(gsTrackerContent, Encoding.UTF8, "application/json");

                return proxyResponse;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP ERROR] Connection failed: {ex.Message}");
                return Request.CreateErrorResponse(
                    HttpStatusCode.BadGateway,
                    $"Tidak dapat terhubung ke service GsTracker. {ex.Message}"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PROXY GET SKP ERROR] Unexpected error: {ex.ToString()}");
                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    "Terjadi kesalahan pada server saat memproses permintaan."
                );
            }
        }

        /// <summary>
        /// Create Permintaan Surat Keterangan
        /// POST: api/PermintaanApi/skp
        /// </summary>
        [HttpPost]
        [Route("skp")]
        public async Task<IHttpActionResult> CreatePermintaanSkp([FromBody] CreateSkpRequestDto request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] === START PROCESS ===");

                // Validasi session
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP ERROR] Session is null - User not logged in");
                    return Content(HttpStatusCode.Unauthorized, new
                    {
                        status = false,
                        code = 401,
                        message = "Session expired. Silakan login kembali."
                    });
                }

                // Debug session information
                var userRole = session.userjabatan ?? "";
                var npk = session.npk ?? "";
                var plant = session.plant ?? "";

                System.Diagnostics.Debug.WriteLine($"[CREATE SKP DEBUG] Session Details:");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP DEBUG] - NPK: {npk}");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP DEBUG] - Plant: {plant}");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP DEBUG] - Role: '{userRole}'");

                // APPROACH BARU: Izinkan semua role yang bukan HC/Atasan (SAMA SEPERTI PIC)
                if (userRole.Equals("hc", StringComparison.OrdinalIgnoreCase) ||
                    userRole.Equals("atasan", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP ACCESS DENIED] Role '{userRole}' cannot create requests");
                    return Content(HttpStatusCode.Forbidden, new
                    {
                        status = false,
                        code = 403,
                        message = $"Anda sedang login sebagai '{userRole}'. Hanya karyawan yang dapat membuat permintaan Surat Keterangan."
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Role validation passed");

                // Validasi request body
                if (request == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP ERROR] Request body is null");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Request body tidak boleh kosong"
                    });
                }

                if (string.IsNullOrEmpty(request.SkpAp))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP ERROR] SkpAp parameter is empty");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'SkpAp' (Alasan Permintaan) harus diisi"
                    });
                }

                if (string.IsNullOrEmpty(request.SkpKet))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP ERROR] SkpKet parameter is empty");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'SkpKet' (Keterangan) harus diisi"
                    });
                }

                // Validasi SkpAp value
                var validAp = new[] {
                    "Surat Keterangan Aktif Kerja",
                    "Surat Keterangan Aktif Bekerja untuk Keperluan Anak",
                    "Pengurusan KPR",
                    "Pengurusan Passport",
                    "Pengurusan Visa"
                };
                if (!validAp.Contains(request.SkpAp))
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP ERROR] Invalid SkpAp value: {request.SkpAp}");
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'SkpAp' harus salah satu dari: " + string.Join(", ", validAp)
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Request validation passed");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] SkpAp: {request.SkpAp}");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] SkpKet: {request.SkpKet.Substring(0, Math.Min(50, request.SkpKet.Length))}...");

                // PERBAIKAN PENTING: Kirim UserRole dari session ke backend (SAMA SEPERTI PIC)
                var gsTrackerRequest = new
                {
                    EmpNpk = npk,
                    plant = plant,
                    SkpAp = request.SkpAp,
                    SkpKet = request.SkpKet,
                    UserRole = userRole, // Kirim role user dari session
                    ValidatedByProxy = true
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Forward ke GsTracker API
                var url = $"{GsTrackerApiBaseUrl}/skp";
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Forwarding to: {url}");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Body: {jsonContent}");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] User Role (to backend): {userRole}, NPK: {npk}");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Response Body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP SUCCESS] Permintaan berhasil dibuat");
                    return Ok(result);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CREATE SKP BACKEND ERROR] Backend returned error: {(int)response.StatusCode}");

                    // Try to parse error response
                    try
                    {
                        var errorResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return Content(response.StatusCode, errorResult);
                    }
                    catch
                    {
                        return Content(response.StatusCode, new
                        {
                            status = false,
                            code = (int)response.StatusCode,
                            message = responseContent
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP UNEXPECTED ERROR] {ex.ToString()}");
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    code = 500,
                    message = "Terjadi kesalahan tidak terduga pada server."
                });
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] === END PROCESS ===");
            }
        }

        #endregion

        #region === ENDPOINT UPDATE STATUS (HC ONLY) ===

        /// <summary>
        /// Update status Permintaan ID Card (PIC)
        /// PUT: api/PermintaanApi/pic/update-status/{picId}
        /// </summary>
        [HttpPut]
        [Route("pic/update-status/{picId}")]
        public async Task<IHttpActionResult> UpdatePicStatus(string picId, [FromBody] UpdateStatusRequestDto request)
        {
            try
            {
                // Validasi session dan role HC
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    return Unauthorized();
                }

                var userRole = (session.userjabatan ?? "").Trim().ToLower();
                if (userRole != "hc")
                {
                    return Content(HttpStatusCode.Forbidden, new
                    {
                        status = false,
                        code = 403,
                        message = "Hanya HC yang dapat mengubah status permintaan"
                    });
                }

                // Validasi request body
                if (request == null || string.IsNullOrEmpty(request.Status))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'status' is required"
                    });
                }

                // Prepare request body untuk GsTracker API
                var gsTrackerRequest = new
                {
                    npk = session.npk,
                    plant = session.plant,
                    status = request.Status
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Forward ke GsTracker API
                var url = $"{GsTrackerApiBaseUrl}/pic/{picId}";
                System.Diagnostics.Debug.WriteLine($"[UPDATE PIC] Forwarding to: {url}");
                System.Diagnostics.Debug.WriteLine($"[UPDATE PIC] Body: {jsonContent}");

                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[UPDATE PIC] Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[UPDATE PIC] Response Body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return Ok(result);
                }
                else
                {
                    return Content(response.StatusCode, new
                    {
                        status = false,
                        message = "Gagal mengupdate status permintaan",
                        detail = responseContent
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UPDATE PIC ERROR] {ex.ToString()}");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update status Permintaan Surat Keterangan (SKP)
        /// PUT: api/PermintaanApi/skp/update-status/{skpId}
        /// </summary>
        [HttpPut]
        [Route("skp/update-status/{skpId}")]
        public async Task<IHttpActionResult> UpdateSkpStatus(string skpId, [FromBody] UpdateStatusRequestDto request)
        {
            try
            {
                // Validasi session dan role HC
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    return Unauthorized();
                }

                var userRole = (session.userjabatan ?? "").Trim().ToLower();
                if (userRole != "hc")
                {
                    return Content(HttpStatusCode.Forbidden, new
                    {
                        status = false,
                        code = 403,
                        message = "Hanya HC yang dapat mengubah status permintaan"
                    });
                }

                // Validasi request body
                if (request == null || string.IsNullOrEmpty(request.Status))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'status' is required"
                    });
                }

                // Prepare request body untuk GsTracker API
                var gsTrackerRequest = new
                {
                    npk = session.npk,
                    plant = session.plant,
                    status = request.Status
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Forward ke GsTracker API
                var url = $"{GsTrackerApiBaseUrl}/skp/{skpId}";
                System.Diagnostics.Debug.WriteLine($"[UPDATE SKP] Forwarding to: {url}");
                System.Diagnostics.Debug.WriteLine($"[UPDATE SKP] Body: {jsonContent}");

                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[UPDATE SKP] Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[UPDATE SKP] Response Body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return Ok(result);
                }
                else
                {
                    return Content(response.StatusCode, new
                    {
                        status = false,
                        message = "Gagal mengupdate status permintaan",
                        detail = responseContent
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UPDATE SKP ERROR] {ex.ToString()}");
                return InternalServerError(ex);
            }
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
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            var userRole = session?.userjabatan ?? "Unknown";

            return Ok(new
            {
                message = "PermintaanApi Controller is working!",
                timestamp = DateTime.Now,
                baseUrl = GsTrackerApiBaseUrl,
                currentRole = userRole,
                sessionActive = session != null
            });
        }

        #endregion
    }
}