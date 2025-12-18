using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Mvc; // Pastikan ini using System.Web.MVC

namespace Template_DevExpress_By_MFM.Controllers
{
    // Pastikan ini class Controller biasa (MVC), BUKAN ApiController
    public class CutiController : Controller
    {

        [HttpGet]
        public async Task<ActionResult> GetFileProxy(string fileName)
        {
            if (Session["SHealth"] == null)
            {
                return Content("Sesi Anda telah habis. Silakan refresh halaman dan login ulang.");
            }

            try
            {
                // 2. CONFIG: Samakan dengan yang ada di ApiController
                string backendBaseUrl = "http://10.19.101.146:44320/api/gstracker/cuti"; // Sesuaikan port GSTRACKER kamu
                //string backendBaseUrl = "http://localhost:44320/api/gstracker/cuti"; // Sesuaikan port GSTRACKER kamu
                string clientID = "GSBattery-5+nzLK0woWSZc1JDl9bylDoLx/Hzhs";
                string clientSecret = "5+nzLK0woWSZc1JDl9bylDoLx/HzhsmegK2KqWqp67OgoYYYX/ncDpc3VpQAAKhbSeJh1CjkIrms+pDt1UlRZMC985mBXUJ1YYPV";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("clientid", clientID);
                    client.DefaultRequestHeaders.Add("clientsecret", clientSecret);
                    string url = $"{backendBaseUrl}/get_lampiran?fileName={fileName}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        string contentType = response.Content.Headers.ContentType.MediaType;
                        Response.AddHeader("Content-Disposition", $"inline; filename=\"{fileName}\"");
                        return File(stream, contentType);
                    }
                    else
                    {
                        string err = await response.Content.ReadAsStringAsync();
                        return Content($"Gagal mengambil file dari server backend. Status: {response.StatusCode}. Info: {err}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                return Content($"Terjadi Error Proxy: {ex.Message}");
            }
        }
    }
}