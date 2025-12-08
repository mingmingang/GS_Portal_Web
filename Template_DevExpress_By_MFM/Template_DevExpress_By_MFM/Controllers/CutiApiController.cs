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
        [Route("api/CutiApi/listUnverifiedCuti")]
        public async Task<HttpResponseMessage> GetListUnverifiedCutiProxy([FromUri] int? year = null)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");
            }
            string empIdApprover = session.npk;


            var requestUrl = $"{SunfishApiBaseUrl}/list_unverified_by_year/{empIdApprover}";

            if (year.HasValue)
            {
                requestUrl += $"/{year.Value}";
            }
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        // 1. Ubah Route agar unik dan deskriptif
        [Route("api/CutiApi/listPartiallyApprovedCuti")]
        public async Task<HttpResponseMessage> GetListPartiallyApprovedCutiProxy([FromUri] int? year = null) // 2. Ubah nama method
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid session.");
            }

            // 3. Ubah URL target ke endpoint 'partially_approved' di Sunfish API
            var requestUrl = $"{SunfishApiBaseUrl}/list_partially_approved_by_year";

            // Logika untuk menambahkan parameter tahun tetap sama
            if (year.HasValue)
            {
                requestUrl += $"/{year.Value}";
            }

            // Meneruskan request ke URL yang telah dibangun dan mengembalikan responsnya
            // Tidak ada perubahan di baris ini
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
            // 1. Tentukan URL API tujuan (Sunfish)
            var requestUrl = $"{SunfishApiBaseUrl}/create_cuti";

            // Pastikan Anda sudah menginisialisasi _httpClient di constructor controller Anda.
            // Contoh: private static readonly HttpClient _httpClient = new HttpClient();

            try
            {
                // 2. Cukup teruskan request dari client (Request.Content) ke API tujuan.
                //    HttpClient akan secara otomatis menangani header seperti Content-Type
                //    dan mengirimkan body request (termasuk file) apa adanya.
                var sunfishResponse = await _httpClient.PostAsync(requestUrl, Request.Content);

                // 3. Kembalikan respons dari API tujuan langsung ke client.
                //    Jika Sunfish mengembalikan error, error itu akan diteruskan.
                //    Jika Sunfish mengembalikan sukses, sukses itu yang akan diteruskan.
                return sunfishResponse;
            }
            catch (HttpRequestException ex)
            {
                // Tangani error koneksi (misalnya, jika server Sunfish tidak dapat dihubungi)
                System.Diagnostics.Debug.WriteLine($"Proxy Error to Sunfish: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, $"Tidak dapat terhubung ke server tujuan: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Tangani error tak terduga lainnya
                System.Diagnostics.Debug.WriteLine($"Unexpected Proxy Error: {ex}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan internal pada server proxy.");
            }
        }

        [SessionCheck]
        [HttpPost] // Anda bisa juga menggunakan [HttpPut] jika lebih sesuai dengan standar REST Anda
        [Route("api/CutiApi/editCuti")]
        public async Task<HttpResponseMessage> EditCutiProxy()
        {
            // 1. Tentukan URL API tujuan (Sunfish) untuk proses edit
            var requestUrl = $"{SunfishApiBaseUrl}/edit_cuti";

            // Asumsi _httpClient sudah tersedia di controller Anda
            try
            {
                // 2. Teruskan request dari client (Request.Content) langsung ke API tujuan.
                //    Ini akan membawa semua data form, termasuk file jika ada yang diubah.
                var sunfishResponse = await _httpClient.PostAsync(requestUrl, Request.Content);

                // 3. Kembalikan respons dari API tujuan (Sunfish) langsung ke client/browser.
                return sunfishResponse;
            }
            catch (HttpRequestException ex)
            {
                // Tangani jika ada masalah koneksi ke server Sunfish
                System.Diagnostics.Debug.WriteLine($"Proxy Error to Sunfish (Edit): {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, $"Tidak dapat terhubung ke server tujuan: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Tangani error tak terduga lainnya
                System.Diagnostics.Debug.WriteLine($"Unexpected Proxy Error (Edit): {ex}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan internal pada server proxy.");
            }
        }

        // Di dalam CutiApiController.cs (API Internal Anda)

        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/cancelCuti")]
        public async Task<HttpResponseMessage> CancelCutiProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/cancel_cuti";

            try
            {
                string jsonContent = await Request.Content.ReadAsStringAsync();

                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var responseFromGSTracker = await _httpClient.PostAsync(requestUrl, httpContent);

                return responseFromGSTracker;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Proxy Error (Cancel): {ex}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan internal pada server proxy.");
            }
        }


        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/verifyCuti")]
        public async Task<HttpResponseMessage> VerifyCutiProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/verify_cuti";
            try
            {
                string jsonContent = await Request.Content.ReadAsStringAsync();
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync(requestUrl, httpContent);
            }
            catch (Exception ex)
            {
                // Logging error
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Proxy error for VerifyCuti.");
            }
        }

        // Proxy untuk Persetujuan Tingkat Pertama (Atasan)
        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/approveCuti")]
        public async Task<HttpResponseMessage> ApproveCutiProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/approve_cuti";
            try
            {
                string jsonContent = await Request.Content.ReadAsStringAsync();
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync(requestUrl, httpContent);
            }
            catch (Exception ex)
            {
                // Logging error
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Proxy error for ApproveCuti.");
            }
        }


        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/fullyApproveCuti")]
        public async Task<HttpResponseMessage> FullyApproveCutiProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/fully_approve_cuti";
            try
            {
                string jsonContent = await Request.Content.ReadAsStringAsync();
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync(requestUrl, httpContent);
            }
            catch (Exception ex)
            {
                // Logging error
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Proxy error for FullyApproveCuti.");
            }
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
        // Di File: Template_DevExpress_By_MFM / Controllers / CutiApiController.cs

        [System.Web.Http.Route("api/CutiApi/getLampiran/{fileName}")]
        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetLampiranProxy(string fileName)
        {
            try
            {
                // 1. VALIDASI SESSION (Pencegah Null Reference)
                var session = System.Web.HttpContext.Current.Session;
                if (session == null || session["SHealth"] == null) // Sesuaikan key session login kamu
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi habis.");
                }

                // 2. SETUP CREDENTIAL
                // Ambil dari session atau Hardcode sementara untuk tes
                string clientID = "123456"; // GANTI DENGAN CLIENT ID GSTRACKER YANG BENAR
                string clientSecret = "123456"; // GANTI DENGAN CLIENT SECRET YANG BENAR

                // 3. SETUP ALAMAT BACKEND (Pencegah Null Reference pada URI)
                // Pastikan ini alamat tempat GSTRACKER jalan (misal localhost:1234)
                string backendBaseUrl = "http://localhost:44383/";

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(backendBaseUrl);
                    client.DefaultRequestHeaders.Add("clientid", clientID);
                    client.DefaultRequestHeaders.Add("clientsecret", clientSecret);

                    // 4. PANGGIL API BACKEND (Pakai Query String supaya aman dari titik)
                    string requestUrl = $"api/gstracker/cuti/get_lampiran?fileName={fileName}";

                    var backendResponse = await client.GetAsync(requestUrl);

                    if (backendResponse.IsSuccessStatusCode)
                    {
                        // Teruskan file stream ke browser
                        var stream = await backendResponse.Content.ReadAsStreamAsync();
                        var result = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StreamContent(stream)
                        };
                        result.Content.Headers.ContentType = backendResponse.Content.Headers.ContentType;
                        result.Content.Headers.ContentDisposition = backendResponse.Content.Headers.ContentDisposition;
                        return result;
                    }
                    else
                    {
                        // Jika error, baca pesan error dari backend
                        var errContent = await backendResponse.Content.ReadAsStringAsync();
                        return Request.CreateErrorResponse(backendResponse.StatusCode, "Backend Error: " + errContent);
                    }
                }
            }
            catch (Exception ex)
            {
                // Tangkap error supaya tidak kuning (YSOD) di browser
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Frontend Error: " + ex.Message);
            }
        }
        #endregion
    }
}