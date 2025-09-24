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
    public class CutiApiController : ApiController
    {
        #region Konfigurasi & Properti
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/gstracker/cuti";

        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        private static readonly HttpClient _httpClient;

        static CutiApiController()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("clientid", SunfishApiClientId);
            _httpClient.DefaultRequestHeaders.Add("clientsecret", SunfishApiClientSecret);
        }
        #endregion

        #region Helper Methods untuk Proxy (Copied & Adapted)
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
        /// Meneruskan request PUT ke Sunfish API.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardPutRequestToSunfishApi(string url)
        {
            try
            {
                // Ambil body dari request asli dan teruskan
                var body = await Request.Content.ReadAsStringAsync();
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var sunfishResponse = await _httpClient.PutAsync(url, content);
                return sunfishResponse; // Langsung return response dari Sunfish
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // Opsional: Tambahkan helper untuk POST jika Anda butuh membuat pengajuan cuti baru
        // private async Task<HttpResponseMessage> ForwardPostRequestToSunfishApi(string url) { ... }

        #endregion

        #region === ENDPOINT PROXY UNTUK JATAH CUTI (LEAVE ENTITLEMENT) ===

        /// <summary>
        /// Proxy untuk mengambil daftar jatah cuti (leave entitlement).
        /// </summary>
        [SessionCheck] // Pastikan Anda memiliki atribut ini untuk memeriksa sesi login
        [HttpGet]
        [Route("api/CutiApi/listJatahCuti")]
        public async Task<HttpResponseMessage> GetJatahCutiProxy([FromUri] string emp_id, [FromUri] string leave_code = null, [FromUri] string from = null, [FromUri] string to = null)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
            }

            var employeeIdFromSession = session.npk;

            var requestUrl = $"{SunfishApiBaseUrl}/list_jatah_cuti?emp_id={employeeIdFromSession}&leave_code={leave_code}&from={from}&to={to}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        /// <summary>
        /// Proxy untuk mengupdate jatah cuti (leave entitlement).
        /// </summary>
        [SessionCheck]
        [HttpPut]
        [Route("api/CutiApi/updateJatahCuti/{empgetleave_id}")]
        public async Task<HttpResponseMessage> UpdateJatahCutiProxy(string empgetleave_id)
        {
            var requestUrl = $"{SunfishApiBaseUrl}/update_jatah_cuti/{empgetleave_id}";
            return await ForwardPutRequestToSunfishApi(requestUrl);
        }

        #endregion


        /// <summary>
        /// Proxy untuk mengambil detail cuti berdasarkan request_no.
        /// Versi baru ini memanggil endpoint detail spesifik dari Sunfish API.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/detailCuti/{id}")]
        public async Task<HttpResponseMessage> GetDetailCutiProxy(string id)
        {
            // Cek sesi (opsional jika endpoint Sunfish sudah aman)
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
            }

            // URL baru sesuai dengan endpoint Anda
            var requestUrl = $"{SunfishApiBaseUrl}/list_cuti_byid?request_no={id}";

            // Langsung teruskan request GET ke Sunfish dan kembalikan hasilnya
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }


        #region LIST TIPE CUTI (PROXY)
        /// <summary>
        /// Endpoint proxy untuk mengambil daftar semua jenis cuti dari API Sunfish.
        /// Meneruskan request dan mendukung filter opsional by company_id.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/listTipeCuti")]
        public async Task<HttpResponseMessage> GetLeaveTypesProxy([FromUri] int? company_id = null)
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
            }

            var requestUrl = $"{SunfishApiBaseUrl}/list_jenis_cuti";

            if (company_id.HasValue)
            {
                requestUrl += $"?company_id={company_id.Value}";
            }

            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region GET LAST CUTI ID (PROXY)
        /// <summary>
        /// Endpoint proxy untuk mengambil request_no terakhir dari API Sunfish.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/getLastIdCuti")]
        public async Task<HttpResponseMessage> GetLastCutiIdProxy()
        {
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
            }
            var requestUrl = $"{SunfishApiBaseUrl}/get_last_id_cuti";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region === ENDPOINT PROXY UNTUK PENGAJUAN CUTI (LEAVE REQUEST) ===

        // ... (Method GetListCutiProxy yang sudah ada) ...


        /// <summary>
        /// Proxy untuk membuat pengajuan cuti baru.
        /// Meneruskan request multipart/form-data apa adanya ke API Sunfish.
        /// </summary>
        [SessionCheck]
        [HttpPost]
        [Route("api/CutiApi/createCuti")]
        public async Task<HttpResponseMessage> CreateCutiProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/create_cuti";

            try
            {
                var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
                if (session == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
                }

                if (Request.Content.IsMimeMultipartContent())
                {
                    string root = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
                    if (!System.IO.Directory.Exists(root))
                    {
                        System.IO.Directory.CreateDirectory(root);
                    }

                    var provider = new MultipartFormDataStreamProvider(root);

                    await Request.Content.ReadAsMultipartAsync(provider);
                    if (provider.FormData["requestfor"] != null && provider.FormData["requestfor"] != session.npk) 
                    {
                        foreach (var file in provider.FileData)
                        {
                            System.IO.File.Delete(file.LocalFileName);
                        }
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Anda tidak diizinkan mengajukan cuti untuk pengguna lain.");
                    }
                }
                var sunfishResponse = await _httpClient.PostAsync(requestUrl, Request.Content);
                return sunfishResponse;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create Cuti Proxy Error: {ex}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan internal pada server proxy.");
            }
        }


        #endregion

        #region === ENDPOINT PROXY UNTUK PENGAJUAN CUTI (LEAVE REQUEST) ===

        /// <summary>
        /// Proxy untuk mengambil daftar pengajuan cuti.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/CutiApi/listCuti")]
        public async Task<HttpResponseMessage> GetListCutiProxy([FromUri] string emp_id, [FromUri] string from = null, [FromUri] string to = null, [FromUri] string request_status = "all")
        {
            // Ambil NPK dari session jika diperlukan untuk keamanan
            var session = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (session == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi tidak valid.");
            }
            var employeeIdFromSession = session.npk;

            var requestUrl = $"{SunfishApiBaseUrl}/list_cuti?emp_id={employeeIdFromSession}&from={from}&to={to}&request_status={request_status}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        #endregion
    }
}