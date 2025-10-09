using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System.Web.Helpers;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ManageMasterLMEController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }
        public GSDbContextGSTrack GSDbContextGSTrack { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ManageMasterLMEController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
            GSDbContextGSTrack = new GSDbContextGSTrack("", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
            GSDbContextGSTrack.Dispose();
        }

        private Image GetCompressedBitmap(Bitmap bmp, long quality)
        {
            using (var mss = new MemoryStream())
            {
                EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == ImageFormat.Jpeg.Guid);
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = qualityParam;
                bmp.Save(mss, imageCodec, parameters);
                return Image.FromStream(mss);
            }
        }
        public Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string thn)
        {
            if (!string.IsNullOrEmpty(thn))
            {
                var query = "DECLARE @_sql  AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) SET @_cols = STUFF((SELECT ',' + QUOTENAME(T.bln, '[]')                    FROM tlkp_bulan AS T                    ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  set @_sql = 'SELECT * FROM(SELECT convert(int, tanggal) as tanggal, bulan.bln, tt.lme_value from tlkp_tanggal as t    FULL OUTER JOIN tlkp_lme AS tt on t.id = tt.lme_date and tt.lme_year = ''" + thn + "''    full outer join tlkp_bulan as bulan on tt.lme_month = bulan.id    where tanggal is not null) AS source PIVOT(MAX(lme_value) FOR bln IN(' + @_cols + ')) AS pivot_table ORDER BY pivot_table.tanggal ASC; ' exec(@_sql)";

                //var query = "DECLARE @_sql AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) SET   @_cols = STUFF(    (      SELECT         ',' + QUOTENAME(T.bln, '[]')       FROM         tlkp_bulan AS T       ORDER BY         T.id asc FOR XML PATH(''),         TYPE    ).value('.', 'NVARCHAR(MAX)'),     1,     1,     ''  ) set   @_sql = 'SELECT * FROM(SELECT idLME.lme_id, convert(int, tanggal) as tanggal, bulan.bln, tt.lme_value from tlkp_tanggal as t    FULL OUTER JOIN tlkp_lme AS tt on t.id = tt.lme_date and tt.lme_year = ''"+ thn +"''    full outer join tlkp_bulan as bulan on tt.lme_month = bulan.id outer APPLY (		SELECT top 1 lme_id from tlkp_lme as t		where t.lme_id = tt.lme_id	) as idLME    where tanggal is not null) AS source PIVOT(MAX(lme_value) FOR bln IN(' + @_cols + ')) AS pivot_table ORDER BY pivot_table.tanggal ASC; ' exec(@_sql)";
                var dataList = GSDbContext.Database.SqlQuery<MappingLME>(query).ToList();
                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            }
            else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }

        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var query = "DECLARE @_sql AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) SET   @_cols = STUFF(    (      SELECT         ',' + QUOTENAME(T.bln, '[]')       FROM         tlkp_bulan AS T       ORDER BY         T.id asc FOR XML PATH(''),         TYPE    ).value('.', 'NVARCHAR(MAX)'),     1,     1,     ''  ) set   @_sql = 'SELECT * FROM(SELECT idLME.lme_id, convert(int, tanggal) as tanggal, bulan.bln, tt.lme_value from tlkp_tanggal as t    FULL OUTER JOIN tlkp_lme AS tt on t.id = tt.lme_date and tt.lme_year = ''"+ DateTime.UtcNow.AddHours(7).ToString("yyyy") +"''    full outer join tlkp_bulan as bulan on tt.lme_month = bulan.id outer APPLY (		SELECT top 1 lme_id from tlkp_lme as t		where t.lme_id = tt.lme_id	) as idLME    where tanggal is not null) AS source PIVOT(MAX(lme_value) FOR bln IN(' + @_cols + ')) AS pivot_table ORDER BY pivot_table.tanggal ASC; ' exec(@_sql)";
            var dataList = GSDbContext.Database.SqlQuery<MappingLME>(query).ToList();
            //var dataList = GSDbContext.MasterKurs.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        //[System.Web.Http.HttpGet]
        //public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int id_idl)
        //{
        //    var dataList = GSDbContext.ManageActivityMarketing.Where(t => t.id_idl == id_idl).ToList();

        //    return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        //}


        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post(ManageActivityMarketing form)
        {
            try
            {
                form.activity_createDate = DateTime.UtcNow.AddHours(7);
                form.activity_createBy = sessionLogin.fullname ?? "";
                form.activity_status = 1;

                Validate(form);
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());
                
                GSDbContext.ManageActivityMarketing.Add(form);
                GSDbContext.SaveChanges();

                return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }            
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var id_lme = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var dateNow = DateTime.UtcNow.AddHours(7).ToString("yyyy");
            var splitMonth = values.Replace("\"", "").Replace("{", "").Replace("}", "").Split(':');
            
            //JsonConvert.PopulateObject(values, MasterLME_input);
            var getMM = splitMonth[0];
            var getValue = splitMonth[1];
            var getMonth = GSDbContext.MasterBulan.First(i => i.bln == getMM);

            var master = GSDbContext.MasterLME.Where(e => e.lme_date == id_lme.ToString() && e.lme_month == getMonth.id.ToString() && e.lme_year == dateNow).ToList();
            if (master.Count() > 0)
            {
                foreach(var data in master)
                {   
                    var getDate = data.lme_date;
                    //if(data.lme_date == getDate && data.lme_month == getMonth.id.ToString())
                    if(data.lme_id != null)
                    {
                        //update data LME\
                        data.lme_value = Convert.ToDecimal(getValue);
                        data.lme_modifDate = DateTime.UtcNow.AddHours(7);
                        data.lme_modifBy = sessionLogin.fullname ?? "";
                        GSDbContext.SaveChanges();
                    }
                    else
                    {
                        //insert data LME
                        MasterLME MasterLME_input = new MasterLME();
                        MasterLME_input.lme_value = Convert.ToDecimal(getValue);
                        MasterLME_input.lme_month = getMonth.id.ToString();
                        MasterLME_input.lme_date = id_lme.ToString();
                        MasterLME_input.lme_year = dateNow;
                        MasterLME_input.lme_createDate = DateTime.UtcNow.AddHours(7);
                        MasterLME_input.lme_createBy = sessionLogin.fullname ?? "";
                        MasterLME_input.lme_status = 1;

                        GSDbContext.MasterLME.Add(MasterLME_input);
                        GSDbContext.SaveChanges();
                    }

                    
                }
                

            } else
            {
                //insert data LME
                MasterLME MasterLME_input = new MasterLME();
                MasterLME_input.lme_value = Convert.ToDecimal(getValue);
                MasterLME_input.lme_month = getMonth.id.ToString();
                MasterLME_input.lme_date = id_lme.ToString();
                MasterLME_input.lme_year = dateNow;
                MasterLME_input.lme_createDate = DateTime.UtcNow.AddHours(7);
                MasterLME_input.lme_createBy = sessionLogin.fullname ?? "";
                MasterLME_input.lme_status = 1;

                GSDbContext.MasterLME.Add(MasterLME_input);
                GSDbContext.SaveChanges();
            }

            
            Validate(form);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(FormDataCollection form, string thn)
        {
            var id_lme = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var splitMonth = values.Replace("\"", "").Replace("{", "").Replace("}", "").Split(':');
            
            //JsonConvert.PopulateObject(values, MasterLME_input);
            var getMM = splitMonth[0];
            var getValue = splitMonth[1];
            var getMonth = GSDbContext.MasterBulan.First(i => i.bln == getMM);

            var master = GSDbContext.MasterLME.Where(e => e.lme_date == id_lme.ToString() && e.lme_month == getMonth.id.ToString() && e.lme_year == thn).ToList();
            if (master.Count() > 0)
            {
                foreach(var data in master)
                {   
                    var getDate = data.lme_date;
                    //if(data.lme_date == getDate && data.lme_month == getMonth.id.ToString())
                    if(data.lme_id != null)
                    {
                        //update data LME\
                        data.lme_value = Convert.ToDecimal(getValue);
                        data.lme_modifDate = DateTime.UtcNow.AddHours(7);
                        data.lme_modifBy = sessionLogin.fullname ?? "";
                        GSDbContext.SaveChanges();
                    }
                    else
                    {
                        //insert data LME
                        MasterLME MasterLME_input = new MasterLME();
                        MasterLME_input.lme_value = Convert.ToDecimal(getValue);
                        MasterLME_input.lme_month = getMonth.id.ToString();
                        MasterLME_input.lme_date = id_lme.ToString();
                        MasterLME_input.lme_year = thn;
                        MasterLME_input.lme_createDate = DateTime.UtcNow.AddHours(7);
                        MasterLME_input.lme_createBy = sessionLogin.fullname ?? "";
                        MasterLME_input.lme_status = 1;

                        GSDbContext.MasterLME.Add(MasterLME_input);
                        GSDbContext.SaveChanges();
                    }

                    
                }
                

            } else
            {
                //insert data LME
                MasterLME MasterLME_input = new MasterLME();
                MasterLME_input.lme_value = Convert.ToDecimal(getValue);
                MasterLME_input.lme_month = getMonth.id.ToString();
                MasterLME_input.lme_date = id_lme.ToString();
                MasterLME_input.lme_year = thn;
                MasterLME_input.lme_createDate = DateTime.UtcNow.AddHours(7);
                MasterLME_input.lme_createBy = sessionLogin.fullname ?? "";
                MasterLME_input.lme_status = 1;

                GSDbContext.MasterLME.Add(MasterLME_input);
                GSDbContext.SaveChanges();
            }

            
            Validate(form);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
        }

        [System.Web.Http.HttpDelete]
        public void Delete(FormDataCollection form) 
        {
            var key = Convert.ToInt32(form.Get("key"));
            var quotation = GSDbContext.ManageActivityMarketing.First(e => e.activity_id == key);

            GSDbContext.ManageActivityMarketing.Remove(quotation);
            GSDbContext.SaveChanges();
        }

    }
}