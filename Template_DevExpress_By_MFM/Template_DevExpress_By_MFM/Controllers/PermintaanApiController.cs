using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace Template_DevExpress_By_MFM.Controllers
{
    [RoutePrefix("api/PermintaanApi")]
    public class PermintaanApiController : ApiController
    {
        // =========================================================================
        // KONFIGURASI KONEKSI KE BACKEND
        // =========================================================================
        // Pastikan IP dan Port ini sesuai dengan tempat Backend GsTracker berjalan
        //private const string GsTrackerApiBaseUrl = "http://10.19.101.146:44320/api/gstracker";
        private const string GsTrackerApiBaseUrl = "http://localhost:44320/api/gstracker"; // Gunakan ini jika localhost

        private static readonly HttpClient _httpClient;

        static PermintaanApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Timeout diperpanjang
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region DTO Classes
        // Model untuk menerima data dari body request di Frontend
        public class UpdateStatusRequestDto
        {
            public string Status { get; set; }
        }

        public class CreatePicRequestDto
        {
            public string PicAh { get; set; }
        }

        public class CreateSkpRequestDto
        {
            public string SkpAp { get; set; }
            public string SkpKet { get; set; }
        }
        #endregion

        #region Helper Methods (Security & Filtering)

        /// <summary>
        /// ✅ SECURITY GATE: Mencegah Karyawan 'mengintip' data orang lain lewat URL.
        /// </summary>
        private IHttpActionResult ValidateUserAccess(string requestedNpk)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;

            if (session == null)
            {
                return Content(HttpStatusCode.Unauthorized, new { message = "Session expired." });
            }

            var userRole = (session.userjabatan ?? "").Trim().ToLower();
            var sessionNpk = (session.npk ?? "").Trim();
            var reqNpk = (requestedNpk ?? "").Trim();

            // 1. HC dan Atasan Boleh Melihat Data Siapa Saja
            if (userRole == "hc" || userRole == "atasan")
            {
                return null; // Access Granted
            }

            // 2. Karyawan Hanya Boleh Melihat Data Sesuai NPK Login
            if (userRole == "karyawan")
            {
                if (!sessionNpk.Equals(reqNpk, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[SECURITY BLOCK] User {sessionNpk} mencoba akses data {reqNpk}");
                    return Content(HttpStatusCode.Forbidden, new
                    {
                        status = false,
                        code = 403,
                        message = "Anda tidak memiliki akses untuk melihat data karyawan lain."
                    });
                }
            }

            return null; // Access Granted (Default)
        }

        /// <summary>
        /// ✅ SAFETY NET: Filter JSON response di sisi Frontend (Double Check).
        /// Meskipun Backend sudah filter, ini memastikan Karyawan tidak menerima data orang lain.
        /// </summary>
        private string FilterResponseByNpk(string jsonResponse, string allowedNpk)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null) return jsonResponse;

                var userRole = (session.userjabatan ?? "").Trim().ToLower();

                // HC melihat semua, jangan difilter di sini
                if (userRole == "hc" || userRole == "atasan") return jsonResponse;

                // Proses Filter untuk Karyawan
                var jsonObj = JObject.Parse(jsonResponse);

                // Struktur JSON Backend biasanya: { code: 200, message: "...", data: [ { data: [...], summary: ... } ] }
                // Kita perlu masuk ke array 'data' paling luar
                var rootArray = jsonObj["data"] as JArray;

                if (rootArray != null && rootArray.Count > 0)
                {
                    var dataWrapper = rootArray[0] as JObject; // Object pembungkus
                    var itemsArray = dataWrapper?["data"] as JArray; // Array data actual

                    if (itemsArray != null)
                    {
                        var filteredItems = new JArray();
                        var summary = new JObject();

                        foreach (var item in itemsArray)
                        {
                            var itemNpk = item["kry_npk"]?.ToString() ?? "";

                            // Logika Filter: Ambil hanya jika NPK cocok dengan Session
                            if (itemNpk.Equals(allowedNpk, StringComparison.OrdinalIgnoreCase))
                            {
                                filteredItems.Add(item);

                                // Hitung ulang summary frontend based on filtered data
                                var status = item["pic_status"]?.ToString() ?? item["skp_status"]?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(status))
                                {
                                    var key = status.Replace(" ", "");
                                    summary[key] = (summary[key] != null) ? (int)summary[key] + 1 : 1;
                                }
                            }
                        }

                        // Replace data lama dengan data yang sudah difilter
                        dataWrapper["data"] = filteredItems;
                        dataWrapper["totalCount"] = filteredItems.Count;
                        dataWrapper["summary"] = summary;
                    }
                }
                return jsonObj.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FILTER ERROR] {ex.Message}");
                return jsonResponse; // Jika error parsing, kembalikan original (fail-safe)
            }
        }

        #endregion

        #region GET Endpoints (Read Data)

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
            // 1. Validasi Akses Frontend
            var accessCheck = ValidateUserAccess(npk);
            if (accessCheck != null) return Request.CreateResponse(HttpStatusCode.Forbidden, accessCheck);

            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            var userRole = (session?.userjabatan ?? "Karyawan").Trim();

            // 2. Susun Query String
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;
            if (!string.IsNullOrEmpty(ah)) queryString["ah"] = ah;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            try
            {
                var url = $"{GsTrackerApiBaseUrl}/pic?{queryString}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // ✅ KIRIM HEADER ROLE KE BACKEND
                // Ini kunci agar Backend tahu dia harus return "Semua Data" atau "Data Parsial"
                request.Headers.Add("X-User-Role", userRole);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // 3. Double Check Filtering untuk Role Karyawan (Safety Net)
                if (userRole.Equals("Karyawan", StringComparison.OrdinalIgnoreCase))
                {
                    content = FilterResponseByNpk(content, npk);
                }

                // Return JSON ke Client Frontend
                return Request.CreateResponse(response.StatusCode, JObject.Parse(content));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Gagal menghubungi server backend: " + ex.Message);
            }
        }

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
            var accessCheck = ValidateUserAccess(npk);
            if (accessCheck != null) return Request.CreateResponse(HttpStatusCode.Forbidden, accessCheck);

            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            var userRole = (session?.userjabatan ?? "Karyawan").Trim();

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["npk"] = npk;
            queryString["plant"] = plant;
            if (!string.IsNullOrEmpty(ap)) queryString["ap"] = ap;
            if (!string.IsNullOrEmpty(status)) queryString["status"] = status;
            if (!string.IsNullOrEmpty(startDate)) queryString["startDate"] = startDate;
            if (!string.IsNullOrEmpty(endDate)) queryString["endDate"] = endDate;

            try
            {
                var url = $"{GsTrackerApiBaseUrl}/skp?{queryString}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // ✅ KIRIM HEADER ROLE KE BACKEND
                request.Headers.Add("X-User-Role", userRole);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (userRole.Equals("Karyawan", StringComparison.OrdinalIgnoreCase))
                {
                    content = FilterResponseByNpk(content, npk);
                }

                return Request.CreateResponse(response.StatusCode, JObject.Parse(content));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Gagal menghubungi server backend: " + ex.Message);
            }
        }

        #endregion

        #region CREATE Endpoints (Insert Data)

        // ✅ FIXED: Semua Role (Karyawan/Atasan/HC) BISA Create Permintaan

        [HttpPost]
        [Route("pic")]
        public async Task<IHttpActionResult> CreatePermintaanPic([FromBody] CreatePicRequestDto request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

                if (request == null || string.IsNullOrEmpty(request.PicAh))
                    return Content(HttpStatusCode.BadRequest, new { message = "Alasan (PicAh) wajib diisi." });

                // Susun Payload untuk Backend
                var backendPayload = new
                {
                    EmpNpk = session.npk,
                    plant = session.plant,
                    PicAh = request.PicAh,
                    UserRole = session.userjabatan, // Kirim role untuk logging di backend
                    ValidatedByProxy = true
                };

                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{GsTrackerApiBaseUrl}/pic", content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JObject.Parse(resultString));
                }
                else
                {
                    // Pass-through error dari backend
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("skp")]
        public async Task<IHttpActionResult> CreatePermintaanSkp([FromBody] CreateSkpRequestDto request)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

                if (request == null || string.IsNullOrEmpty(request.SkpAp) || string.IsNullOrEmpty(request.SkpKet))
                    return Content(HttpStatusCode.BadRequest, new { message = "Parameter SkpAp dan SkpKet wajib diisi." });

                var backendPayload = new
                {
                    EmpNpk = session.npk,
                    plant = session.plant,
                    SkpAp = request.SkpAp,
                    SkpKet = request.SkpKet,
                    UserRole = session.userjabatan,
                    ValidatedByProxy = true
                };

                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{GsTrackerApiBaseUrl}/skp", content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JObject.Parse(resultString));
                }
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region UPDATE Endpoints (Approve/Reject)

        // ✅ FIXED: Update Status HANYA UNTUK HC

        [HttpPut]
        [Route("pic/update-status/{picId}")]
        public async Task<IHttpActionResult> UpdatePicStatus(string picId, [FromBody] UpdateStatusRequestDto request)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

            // Cek Role di Frontend
            var role = (session.userjabatan ?? "").Trim().ToLower();
            if (role != "hc")
            {
                return Content(HttpStatusCode.Forbidden, new { message = "Akses Ditolak. Hanya HC yang dapat mengubah status." });
            }

            try
            {
                var backendPayload = new
                {
                    npk = session.npk, // NPK si pengubah (HC)
                    plant = session.plant,
                    status = request.Status
                };

                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{GsTrackerApiBaseUrl}/pic/{picId}", content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JObject.Parse(resultString));
                }
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("skp/update-status/{skpId}")]
        public async Task<IHttpActionResult> UpdateSkpStatus(string skpId, [FromBody] UpdateStatusRequestDto request)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

            // Cek Role di Frontend
            var role = (session.userjabatan ?? "").Trim().ToLower();
            if (role != "hc")
            {
                return Content(HttpStatusCode.Forbidden, new { message = "Akses Ditolak. Hanya HC yang dapat mengubah status." });
            }

            try
            {
                var backendPayload = new
                {
                    npk = session.npk, // NPK si pengubah (HC)
                    plant = session.plant,
                    status = request.Status
                };

                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{GsTrackerApiBaseUrl}/skp/{skpId}", content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JObject.Parse(resultString));
                }
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region DEBUG / TEST Endpoint
        [HttpGet]
        [Route("test")]
        public IHttpActionResult TestConnection()
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            return Ok(new
            {
                message = "Controller Frontend Ready",
                backendUrl = GsTrackerApiBaseUrl,
                sessionInfo = session != null ? $"Logged in as {session.npk} ({session.userjabatan})" : "No Session"
            });
        }
        #endregion
    }
}