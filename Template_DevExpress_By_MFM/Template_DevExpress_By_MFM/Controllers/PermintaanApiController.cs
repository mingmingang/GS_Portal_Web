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
        //private const string GsTrackerApiBaseUrl = "http://10.19.101.146:44320/api/gstracker";
        private const string GsTrackerApiBaseUrl = "http://localhost:44320/api/gstracker";

        private static readonly HttpClient _httpClient;

        static PermintaanApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region DTO Classes
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
        public class UpdateDataSkpDto
        {
            public string SkpAp { get; set; }  // Alasan Permintaan
            public string SkpKet { get; set; } // Keterangan
        }
        #endregion

        #region Helper Methods (Security & Filtering)

        // ✅ SECURITY GATE: Mencegah Karyawan 'mengintip' data orang lain
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
            if (userRole == "hc" || userRole == "atasan") return null;

            // 2. Karyawan Hanya Boleh Melihat Data Sesuai NPK Login
            if (userRole == "karyawan")
            {
                if (!sessionNpk.Equals(reqNpk, StringComparison.OrdinalIgnoreCase))
                {
                    return Content(HttpStatusCode.Forbidden, new
                    {
                        status = false,
                        code = 403,
                        message = "Anda tidak memiliki akses untuk melihat data karyawan lain."
                    });
                }
            }

            return null;
        }

        // ✅ SAFETY NET: Filter JSON response di sisi Frontend
        private string FilterResponseByNpk(string jsonResponse, string allowedNpk)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null) return jsonResponse;

                var userRole = (session.userjabatan ?? "").Trim().ToLower();
                if (userRole == "hc" || userRole == "atasan") return jsonResponse;

                var jsonObj = JObject.Parse(jsonResponse);
                var rootArray = jsonObj["data"] as JArray;

                if (rootArray != null && rootArray.Count > 0)
                {
                    var dataWrapper = rootArray[0] as JObject;
                    var itemsArray = dataWrapper?["data"] as JArray;

                    if (itemsArray != null)
                    {
                        var filteredItems = new JArray();
                        var summary = new JObject();

                        foreach (var item in itemsArray)
                        {
                            var itemNpk = item["kry_npk"]?.ToString() ?? "";
                            if (itemNpk.Equals(allowedNpk, StringComparison.OrdinalIgnoreCase))
                            {
                                filteredItems.Add(item);
                                var status = item["pic_status"]?.ToString() ?? item["skp_status"]?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(status))
                                {
                                    var key = status.Replace(" ", "");
                                    summary[key] = (summary[key] != null) ? (int)summary[key] + 1 : 1;
                                }
                            }
                        }

                        dataWrapper["data"] = filteredItems;
                        dataWrapper["totalCount"] = filteredItems.Count;
                        dataWrapper["summary"] = summary;
                    }
                }
                return jsonObj.ToString();
            }
            catch (Exception)
            {
                return jsonResponse;
            }
        }

        #endregion

        #region GET Endpoints (Read Data)

        [HttpGet]
        [Route("pic/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetPermintaanPicListProxy(
            string npk, string plant, [FromUri] string ah = null, [FromUri] string status = null,
            [FromUri] string startDate = null, [FromUri] string endDate = null)
        {
            var accessCheck = ValidateUserAccess(npk);
            if (accessCheck != null) return Request.CreateResponse(HttpStatusCode.Forbidden, accessCheck);

            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            var userRole = (session?.userjabatan ?? "Karyawan").Trim();

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
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Backend Error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("skp/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetPermintaanSkpListProxy(
            string npk, string plant, [FromUri] string ap = null, [FromUri] string status = null,
            [FromUri] string startDate = null, [FromUri] string endDate = null)
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
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Backend Error: " + ex.Message);
            }
        }

        #endregion

        #region CREATE Endpoints (Insert Data)

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

                var backendPayload = new
                {
                    EmpNpk = session.npk,
                    plant = session.plant,
                    PicAh = request.PicAh,
                    UserRole = session.userjabatan,
                    ValidatedByProxy = true
                };

                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{GsTrackerApiBaseUrl}/pic", content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return Ok(JObject.Parse(resultString));
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex) { return InternalServerError(ex); }
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

                if (response.IsSuccessStatusCode) return Ok(JObject.Parse(resultString));
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }

        #endregion

        #region UPDATE Endpoints (Approve/Reject) & Update Keperluan

        [HttpPut]
        [Route("pic/update-status/{picId}")]
        public async Task<IHttpActionResult> UpdatePicStatus(string picId, [FromBody] UpdateStatusRequestDto request)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

            var role = (session.userjabatan ?? "").Trim().ToLower();
            if (role != "hc") return Content(HttpStatusCode.Forbidden, new { message = "Akses Ditolak. Hanya HC yang dapat mengubah status." });

            try
            {
                var backendPayload = new { npk = session.npk, plant = session.plant, status = request.Status };
                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{GsTrackerApiBaseUrl}/pic/{picId}", content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return Ok(JObject.Parse(resultString));
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }

        // Pastikan route constraint {id:long} agar URL bersih dan hanya menerima angka
        [HttpPut]
        [Route("skp/update-status/{id:long}")]
        public async Task<IHttpActionResult> UpdateSkpStatus(long id, [FromBody] UpdateStatusRequestDto request)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

            var role = (session.userjabatan ?? "").Trim().ToLower();
            if (role != "hc") return Content(HttpStatusCode.Forbidden, new { message = "Akses Ditolak. Hanya HC yang dapat mengubah status." });

            try
            {
                // Persiapkan payload body (JSON) untuk Backend
                var backendPayload = new { npk = session.npk, plant = session.plant, status = request.Status };
                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");

                // --- PERBAIKAN DI SINI ---
                // Kirim 'id' (angka) sebagai parameter query string
                // URL Backend: api/gstracker/skp/update-status?skpId=12345
                string targetUrl = $"{GsTrackerApiBaseUrl}/skp/update-status?skpId={id}";

                // Kirim Request ke Backend
                var response = await _httpClient.PutAsync(targetUrl, content);

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
        [HttpPost]
        [Route("skp/update-data/{id:long}")]
        public async Task<IHttpActionResult> UpdateSkpData(long id, [FromBody] UpdateDataSkpDto request)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
            if (session == null) return Content(HttpStatusCode.Unauthorized, new { message = "Session Expired" });

            var role = (session.userjabatan ?? "").Trim().ToLower();
            if (role != "hc") return Content(HttpStatusCode.Forbidden, new { message = "Akses Ditolak. Hanya HC yang dapat mengubah data dokumen." });

            if (request == null) return Content(HttpStatusCode.BadRequest, new { message = "Data tidak valid." });

            try
            {
                var backendPayload = new
                {
                    npk = session.npk,
                    plant = session.plant,
                    skp_ap = request.SkpAp,
                    skp_ket = request.SkpKet
                };

                var content = new StringContent(JsonConvert.SerializeObject(backendPayload), Encoding.UTF8, "application/json");

                // UBAH URL target query string jika diperlukan, tapi payload ada di body
                // Note: Backend menerima skpId via query string dan data via body
                string targetUrl = $"{GsTrackerApiBaseUrl}/skp/update-data?skpId={id}";

                // UBAH DARI PutAsync KE PostAsync
                var response = await _httpClient.PostAsync(targetUrl, content);
                var resultString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return Ok(JObject.Parse(resultString));
                else
                {
                    try { return Content(response.StatusCode, JObject.Parse(resultString)); }
                    catch { return Content(response.StatusCode, new { message = resultString }); }
                }
            }
            catch (Exception ex) { return InternalServerError(ex); }
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