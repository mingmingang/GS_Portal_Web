// ================================================================
// PERMINTAAN API CONTROLLER - FIXED VERSION
// ================================================================
// ✅ CHANGES:
// 1. Ubah validasi CREATE - izinkan semua role membuat permintaan
// 2. GET endpoints tetap sama (sudah benar)
// 3. UPDATE endpoints tetap HC-only (sudah benar)
// ================================================================

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
    [RoutePrefix("api/PermintaanApi")]
    public class PermintaanApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string GsTrackerApiBaseUrl = "http://localhost:44320/api/gstracker";
        private static readonly HttpClient _httpClient;

        static PermintaanApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion

        #region DTO Classes (Tidak Berubah)
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

        #region Helper Methods (Tidak Berubah - Sudah Benar)

        // ✅ CORRECT: GET validation - filter by role
        private IHttpActionResult ValidateUserAccess(string requestedNpk)
        {
            var session = HttpContext.Current.Session["SHealth"] as SessionLogin;

            if (session == null)
            {
                return Unauthorized();
            }

            var userRole = (session.userjabatan ?? "").Trim().ToLower();
            var sessionNpk = (session.npk ?? "").Trim();

            // ✅ HC dan Atasan dapat mengakses semua data
            if (userRole == "hc" || userRole == "atasan")
            {
                System.Diagnostics.Debug.WriteLine($"[ACCESS] {userRole.ToUpper()} role detected - Full access granted");
                return null;
            }

            // ✅ Karyawan hanya bisa akses data mereka sendiri
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

        // ✅ CORRECT: Filter response - HC dan Atasan lihat semua
        private string FilterResponseByNpk(string jsonResponse, string allowedNpk)
        {
            try
            {
                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null) return jsonResponse;

                var userRole = (session.userjabatan ?? "").Trim().ToLower();

                // ✅ HC dan Atasan melihat semua data tanpa filter
                if (userRole == "hc" || userRole == "atasan")
                {
                    System.Diagnostics.Debug.WriteLine($"[FILTER] {userRole.ToUpper()} role - No filtering applied");
                    return jsonResponse;
                }

                // ✅ Hanya filter untuk role Karyawan
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
                        var filteredItems = new JArray();

                        foreach (var item in itemsArray)
                        {
                            var kryNpk = item["kry_npk"]?.ToString() ?? "";

                            if (kryNpk.Equals(allowedNpk, StringComparison.OrdinalIgnoreCase))
                            {
                                filteredItems.Add(item);
                            }
                        }

                        var totalCount = filteredItems.Count;
                        var summary = new JObject();

                        foreach (var item in filteredItems)
                        {
                            var status = item["pic_status"]?.ToString() ?? item["skp_status"]?.ToString() ?? "";

                            if (!string.IsNullOrEmpty(status))
                            {
                                var statusKey = status.Replace(" ", "");
                                if (summary[statusKey] == null)
                                    summary[statusKey] = 0;
                                summary[statusKey] = (int)summary[statusKey] + 1;
                            }
                        }

                        firstItem["data"] = filteredItems;
                        firstItem["totalCount"] = totalCount;
                        firstItem["summary"] = summary;
                    }
                }

                return jsonObj.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FILTER ERROR] {ex.Message}");
                return jsonResponse;
            }
        }

        #endregion

        #region GET Endpoints (Tidak Berubah - Sudah Benar)

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
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("X-User-Role", userRole);

                var gsTrackerResponse = await _httpClient.SendAsync(request);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

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
                return Request.CreateErrorResponse(
                    HttpStatusCode.BadGateway,
                    $"Tidak dapat terhubung ke service GsTracker. {ex.Message}"
                );
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
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("X-User-Role", userRole);

                var gsTrackerResponse = await _httpClient.SendAsync(request);
                var gsTrackerContent = await gsTrackerResponse.Content.ReadAsStringAsync();

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
                return Request.CreateErrorResponse(
                    HttpStatusCode.BadGateway,
                    $"Tidak dapat terhubung ke service GsTracker. {ex.Message}"
                );
            }
        }

        #endregion

        #region CREATE Endpoints (DIPERBAIKI)

        /// <summary>
        /// ✅ FIXED: Create Permintaan ID Card
        /// Semua role (Karyawan, Atasan, HC) bisa membuat permintaan
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
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC ERROR] Session is null");
                    return Content(HttpStatusCode.Unauthorized, new
                    {
                        status = false,
                        code = 401,
                        message = "Session expired. Silakan login kembali."
                    });
                }

                var userRole = session.userjabatan ?? "";
                var npk = session.npk ?? "";
                var plant = session.plant ?? "";

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] Session Details:");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] - NPK: {npk}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] - Plant: {plant}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC DEBUG] - Role: '{userRole}'");

                // ✅ FIXED: HAPUS validasi role yang membatasi
                // Semua role bisa membuat permintaan
                // ❌ BEFORE:
                // if (userRole.Equals("hc", StringComparison.OrdinalIgnoreCase) ||
                //     userRole.Equals("atasan", StringComparison.OrdinalIgnoreCase))
                // {
                //     return Forbidden("Hanya karyawan yang dapat membuat permintaan");
                // }

                // ✅ AFTER: Tidak ada validasi role
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] All roles allowed - Current role: {userRole}");

                // Validasi request body
                if (request == null)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Request body tidak boleh kosong"
                    });
                }

                if (string.IsNullOrEmpty(request.PicAh))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'PicAh' harus diisi"
                    });
                }

                var validAh = new[] { "Hilang", "Rusak", "Mutasi" };
                if (!validAh.Contains(request.PicAh))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'PicAh' harus salah satu dari: Hilang, Rusak, Mutasi"
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Request validation passed - PicAh: {request.PicAh}");

                var gsTrackerRequest = new
                {
                    EmpNpk = npk,
                    plant = plant,
                    PicAh = request.PicAh,
                    UserRole = userRole,
                    ValidatedByProxy = true
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"{GsTrackerApiBaseUrl}/pic";
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Forwarding to: {url}");
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Body: {jsonContent}");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[CREATE PIC] Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    System.Diagnostics.Debug.WriteLine($"[CREATE PIC SUCCESS]");
                    return Ok(result);
                }
                else
                {
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
                System.Diagnostics.Debug.WriteLine($"[CREATE PIC ERROR] {ex.ToString()}");
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    code = 500,
                    message = "Terjadi kesalahan tidak terduga pada server."
                });
            }
        }

        /// <summary>
        /// ✅ FIXED: Create Permintaan Surat Keterangan
        /// Semua role (Karyawan, Atasan, HC) bisa membuat permintaan
        /// </summary>
        [HttpPost]
        [Route("skp")]
        public async Task<IHttpActionResult> CreatePermintaanSkp([FromBody] CreateSkpRequestDto request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] === START PROCESS ===");

                var session = HttpContext.Current.Session["SHealth"] as SessionLogin;
                if (session == null)
                {
                    return Content(HttpStatusCode.Unauthorized, new
                    {
                        status = false,
                        code = 401,
                        message = "Session expired. Silakan login kembali."
                    });
                }

                var userRole = session.userjabatan ?? "";
                var npk = session.npk ?? "";
                var plant = session.plant ?? "";

                System.Diagnostics.Debug.WriteLine($"[CREATE SKP DEBUG] - NPK: {npk}, Role: '{userRole}'");

                // ✅ FIXED: HAPUS validasi role yang membatasi
                // ❌ BEFORE:
                // if (userRole.Equals("hc") || userRole.Equals("atasan"))
                // {
                //     return Forbidden("Hanya karyawan yang dapat membuat permintaan");
                // }

                // ✅ AFTER: Tidak ada validasi role
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] All roles allowed");

                if (request == null)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Request body tidak boleh kosong"
                    });
                }

                if (string.IsNullOrEmpty(request.SkpAp))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'SkpAp' (Alasan Permintaan) harus diisi"
                    });
                }

                if (string.IsNullOrEmpty(request.SkpKet))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'SkpKet' (Keterangan) harus diisi"
                    });
                }

                var validAp = new[] {
                    "Surat Keterangan Aktif Kerja",
                    "Surat Keterangan Aktif Bekerja untuk Keperluan Anak",
                    "Pengurusan KPR",
                    "Pengurusan Passport",
                    "Pengurusan Visa"
                };

                if (!validAp.Contains(request.SkpAp))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'SkpAp' tidak valid"
                    });
                }

                var gsTrackerRequest = new
                {
                    EmpNpk = npk,
                    plant = plant,
                    SkpAp = request.SkpAp,
                    SkpKet = request.SkpKet,
                    UserRole = userRole,
                    ValidatedByProxy = true
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"{GsTrackerApiBaseUrl}/skp";
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP] Forwarding to: {url}");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return Ok(result);
                }
                else
                {
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
                System.Diagnostics.Debug.WriteLine($"[CREATE SKP ERROR] {ex.ToString()}");
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    code = 500,
                    message = "Terjadi kesalahan tidak terduga pada server."
                });
            }
        }

        #endregion

        #region UPDATE Endpoints (Tidak Berubah - HC Only)

        /// <summary>
        /// ✅ CORRECT: Update status PIC - HC only
        /// Tidak perlu diubah
        /// </summary>
        [HttpPut]
        [Route("pic/update-status/{picId}")]
        public async Task<IHttpActionResult> UpdatePicStatus(string picId, [FromBody] UpdateStatusRequestDto request)
        {
            try
            {
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

                if (request == null || string.IsNullOrEmpty(request.Status))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'status' is required"
                    });
                }

                var gsTrackerRequest = new
                {
                    npk = session.npk,
                    plant = session.plant,
                    status = request.Status
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"{GsTrackerApiBaseUrl}/pic/{picId}";
                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

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
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// ✅ CORRECT: Update status SKP - HC only
        /// Tidak perlu diubah
        /// </summary>
        [HttpPut]
        [Route("skp/update-status/{skpId}")]
        public async Task<IHttpActionResult> UpdateSkpStatus(string skpId, [FromBody] UpdateStatusRequestDto request)
        {
            try
            {
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

                if (request == null || string.IsNullOrEmpty(request.Status))
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        status = false,
                        code = 400,
                        message = "Parameter 'status' is required"
                    });
                }

                var gsTrackerRequest = new
                {
                    npk = session.npk,
                    plant = session.plant,
                    status = request.Status
                };

                var jsonContent = JsonConvert.SerializeObject(gsTrackerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"{GsTrackerApiBaseUrl}/skp/{skpId}";
                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

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
                return InternalServerError(ex);
            }
        }

        #endregion

        #region Test Endpoint (Tidak Berubah)

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