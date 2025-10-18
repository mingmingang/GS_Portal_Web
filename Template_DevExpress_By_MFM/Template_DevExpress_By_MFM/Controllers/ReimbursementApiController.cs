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
    public class ReimbursementApiController : ApiController
    {
        //private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        #region Konfigurasi & Properti
        private const string SunfishApiBaseUrl = "http://localhost:44320/api/Sunfish";
        private const string GsTrackerApiBaseUrl = "http://localhost:44320/api/gstracker";

        // Kredensial API Sunfish
        private const string SunfishApiClientId = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
        private const string SunfishApiClientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

        // HttpClient di-instantiate sekali dan digunakan kembali.
        private static readonly HttpClient _httpClient;

        static ReimbursementApiController()
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
        /// Meneruskan request GET yang mengembalikan konten mentah (seperti file/gambar) ke Sunfish API.
        /// </summary>
        private async Task<HttpResponseMessage> ForwardRawGetRequestToSunfishApi(string url)
        {
            try
            {
                var sunfishResponse = await _httpClient.GetAsync(url);

                return sunfishResponse;
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

        #region === ENDPOINT REIMBURSEMENT ===

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getReimbursementSummary/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetReimbursementSummaryProxy(string npk, string plant, int? year)
        {
            var requestUrl = $"{SunfishApiBaseUrl}/getReimbursementSummary/{npk}/{plant}?year={year}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        /// <summary>
        /// Proxy untuk mengambil data saldo (Plafon, Sisa, Digunakan) dari GsTracker API.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        // --- PERUBAHAN ROUTE: Menangkap {year} dan {semester} ---
        [Route("api/ReimbursementApi/list_saldo_reimburse/{npk}/{year}/{semester}")]
        // --- PERUBAHAN PARAMETER: Menerima 'year' dan 'semester' dari route ---
        public async Task<HttpResponseMessage> GetSaldoReimburseProxy(string npk, int year, string semester)
        {
            // API ini menggunakan GsTrackerApiBaseUrl
            // URL target sudah sesuai dengan parameter yang diterima
            var requestUrl = $"{GsTrackerApiBaseUrl}/reimburse/list_saldo_reimburse/{npk}/{year}/{semester}";

            // Meneruskan panggilan
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getReimbursementList/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetReimbursementListProxy(string npk, string plant, [FromUri] int? year, [FromUri] string tab = "Semua")
        {
            var requestUrl = $"{SunfishApiBaseUrl}/getReimbursementList/{npk}/{plant}?year={year}&tab={Uri.EscapeDataString(tab)}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getReimType/{npk}/{plant}")]
        public async Task<HttpResponseMessage> GetReimTypeProxy(string npk, string plant)
        {
            var requestUrl = $"{SunfishApiBaseUrl}/getReimTypeGsTrack/{npk}/{plant}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        /// <summary>
        /// Proxy untuk mengambil detail reimbursement. NPK dan Plant diambil dari session untuk keamanan.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getReimbursementDetail/{id}")]
        public async Task<HttpResponseMessage> GetReimbursementDetailProxy(int id)
        {
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            var npk = sessionLogin.empid;
            var plant = sessionLogin.userplant;

            var requestUrl = $"{SunfishApiBaseUrl}/getReimbursementDetail/{id}/{npk}/{plant}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        /// <summary>
        /// Proxy untuk mengambil file lampiran (PDF, JPG, PNG).
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/reimbursementFile/{id}/{fileKey}")]
        public async Task<HttpResponseMessage> GetReimbursementFileProxy(int id, string fileKey)
        {
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir.");
            }
            var npk = sessionLogin.empid;

            var requestUrl = $"{SunfishApiBaseUrl}/reimbursementFile/{id}/{fileKey}/{npk}";
            return await ForwardRawGetRequestToSunfishApi(requestUrl);
        }

        /// <summary>
        /// Proxy untuk mengambil info jumlah halaman PDF.
        /// </summary>
        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/reimbursementPdfInfo/{id}/{fileKey}")]
        public async Task<HttpResponseMessage> GetReimbursementPdfInfoProxy(int id, string fileKey)
        {
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Sesi Anda telah berakhir.");
            }
            var npk = sessionLogin.empid;

            var requestUrl = $"{SunfishApiBaseUrl}/reimbursementPdfInfo/{id}/{fileKey}/{npk}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        /// <summary>
        /// Proxy untuk mengambil gambar halaman PDF yang sudah dikonversi.
        /// </summary>
        [HttpGet]
        [Route("api/ReimbursementApi/reimbursementPdfImage/{imageName}")]
        public async Task<HttpResponseMessage> GetReimbursementPdfImageProxy(string imageName)
        {
            var requestUrl = $"{SunfishApiBaseUrl}/reimbursementPdfImage/{imageName}";

            return await ForwardRawGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpPost]
        [Route("api/ReimbursementApi/createReimbursement")]
        public async Task<HttpResponseMessage> CreateReimbursementProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/createReimbursement";
            try
            {
                var sunfishResponse = await _httpClient.PostAsync(requestUrl, Request.Content);
                return sunfishResponse;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SessionCheck]
        [HttpPut]
        [Route("api/ReimbursementApi/cancelReimbursement")]
        public async Task<HttpResponseMessage> CancelReimbursementProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/cancelReimbursement";
            try
            {
                var body = await Request.Content.ReadAsStringAsync();
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var sunfishResponse = await _httpClient.PutAsync(requestUrl, content);
                return sunfishResponse;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/generateReimbursementNo/{npk}")]
        public async Task<HttpResponseMessage> GenerateReimbursementNoProxy(string npk)
        {
            var requestUrl = $"{SunfishApiBaseUrl}/generateReimbursementNo/{npk}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion

        #region === ENDPOINT MASTER DATA & LAINNYA ===

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getDoctorHospital")]
        public async Task<HttpResponseMessage> GetDoctorHospitalProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/getDoctorHospital";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getDisease")]
        public async Task<HttpResponseMessage> GetDiseaseProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/getDisease";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/list_keluarga")]
        public async Task<HttpResponseMessage> GetKeluargaProxy()
        {
            var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
            var empid = sessionLogin.empid;

            var requestUrl = $"{GsTrackerApiBaseUrl}/pusaka/list_keluarga/{empid}";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/getListEmp")]
        public async Task<HttpResponseMessage> GetListEmpProxy()
        {
            var requestUrl = $"{SunfishApiBaseUrl}/getListEmpGsTrack";
            return await ForwardJsonGetRequestToSunfishApi(requestUrl);
        }
        #endregion
    }
}