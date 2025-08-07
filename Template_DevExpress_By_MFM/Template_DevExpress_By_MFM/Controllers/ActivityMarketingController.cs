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
using Template_DevExpress_By_MFM.Utils;
using System.Web.Helpers;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;

namespace Template_DevExpress_By_MFM.Controllers
{
    //[System.Web.Http.Route("Forecast/{action}", Name = "Forecast")]
    public class ActivityMarketingController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }
        public GSDbContextGSTrack GSDbContextGSTrack { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ActivityMarketingController()
        {

            GSDbContext = new GSDbContext(".", "db_marketing_portal", "sa", "aangaang");
            GSDbContextGSTrack = new GSDbContextGSTrack(".", "db_marketing_portal", "sa", "aangaang");
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

        [SessionCheck]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            var query = "SELECT [idl_BP] ,[idl_attach] ,[idl_contact] ,[idl_position] ,[id_idl] ,[idl_npk] ,[id_nama] ,[idl_lokasikebrangkatan] ,[id_kegiatan] ,[namakegiatan] ,[idl_keterangan] ,[idl_tgl] ,[idl_plant] ,[bitmap] ,[jam_in] ,[jam_out] ,[jam_inout] ,[trans_credet],[idl_doc] FROM [db_gs_tracking].[dbo].[View_Activity_Mkt]";
            //var query = "SELECT inp.T$BPID+' - '+inp.T$NAMA AS idl_BP, am.activity_attach AS idl_attach, am.activity_contact AS idl_contact,                               am.activity_position AS idl_position,                               b.id_idl,                               b.idl_npk,                               b.id_nama,                               b.idl_lokasikebrangkatan,                               b.id_kegiatan,                               c.namakegiatan,                               b.idl_keterangan,                               b.idl_tgl,                               b.idl_plant,                 ISNULL(CAST(t.bitmap AS VARCHAR(MAX)), NULL) AS bitmap,    (SELECT top 1 CONVERT(VARCHAR(5), FdTime, 108)    FROM [Transaksi]    WHERE FcInOut='1'      AND id_idl = b.id_idl) AS jam_in,    (SELECT top 1 CONVERT(VARCHAR(5), FdTime, 108)    FROM [Transaksi]    WHERE FcInOut='0'      AND id_idl = b.id_idl) AS jam_out,                               (                                  (SELECT top 1 CONVERT(VARCHAR(5), FdTime, 108)                                   FROM[Transaksi]                                   WHERE FcInOut = '1'                                     AND id_idl = b.id_idl) + '-' +                                  (SELECT top 1 CONVERT(VARCHAR(5), FdTime, 108)                                   FROM[Transaksi]                                   WHERE FcInOut = '0'                                     AND id_idl = b.id_idl)) AS jam_inout,  								t.created_date as trans_credet  FROM[db_gs_tracking].[dbo].[tlkp_emp] a  JOIN t_idl b ON a.emp_npk = b.idl_npk  AND a.plant = b.idl_plant  JOIN tlkp_kegiatan_idl c ON b.id_kegiatan = c.id  LEFT JOIN[db_gs_tracking].[dbo].[Transaksi] t ON b.id_idl = t.id_idl  LEFT JOIN GSPORTAL_DEV01.[db_marketing_portal].[dbo].[t_activity_marketing] am ON t.id_idl = am.id_idl LEFT JOIN GSPORTAL_DEV01.[db_data_lake_from_infordb].[dbo].[ttccom1008888] inp ON am.activity_BP = inp.T$BPID GROUP BY b.id_idl,          b.idl_tgl,          b.idl_npk,          b.id_nama,          b.idl_lokasikebrangkatan,          b.id_kegiatan,          c.namakegiatan,          b.idl_plant,          b.idl_keterangan,          inp.T$BPID,         inp.T$NAMA,         am.activity_contact,         am.activity_position, CAST(t.bitmap AS VARCHAR(MAX)),  								t.created_date, am.activity_attach";
            ////where a.DeptSeksi like '%marketing%' or DeptSeksi like '%sales%' or DeptSeksi like '%RM %'
            var dataList = GSDbContextGSTrack.Database.SqlQuery<ManageIDLTemp>(query).ToList();

            //List<string> listTempFile = new List<string>();
            //List<ManageIDLTemp> datalist_bitmap = new List<ManageIDLTemp>();
            //foreach (var bit in dataList)
            //{
            //    //var compressedBmp = GetCompressedBitmap(Base64ToImage(bit.bitmap), 60L);
            //    ManageIDLTemp datalist_n = new ManageIDLTemp();
            //    List<string> temp_bit = new List<string>();
            //    datalist_n = bit;
            //    temp_bit.Add(bit.bitmap);
            //    if (!string.IsNullOrEmpty(bit.bitmap) && bit.bitmap != "null")
            //        datalist_n.img_bitmap = Base64ToImage(bit.bitmap);
            //    datalist_bitmap.Add(datalist_n);
            //}



            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));


        }

        [SessionCheck]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int id_idl)
        {
            var dataList = GSDbContext.ManageActivityMarketing.Where(t => t.id_idl == id_idl).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        [SessionCheck]
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

        [SessionCheck]
        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt64(form.Get("key"));
            var values = form.Get("values");
            //var master = GSDbContext.MasterUser.First(e => e.id_user == key);
            ManageIDLTemp ManageIDL = new ManageIDLTemp();
            //var itemQuotation = GSDbContextGSTrack.ManageIDL.First(e => e.id_idl == key);

            var query = "select b.id_idl, b.idl_npk, b.id_nama, b.idl_lokasikebrangkatan, b.id_kegiatan, c.namakegiatan, b.idl_keterangan, am.activity_detail, b.idl_tgl, b.idl_plant, (select top 1 CONVERT(VARCHAR(5), FdTime, 108) from [Transaksi] where FcInOut='1' and id_idl = b.id_idl) as jam_in,(select top 1 CONVERT(VARCHAR(5), FdTime, 108) from [Transaksi] where FcInOut='0' and id_idl = b.id_idl) as jam_out, ((select top 1 CONVERT(VARCHAR(5), FdTime, 108) from[Transaksi] where FcInOut = '1' and id_idl = b.id_idl) + '-' + (select top 1 CONVERT(VARCHAR(5), FdTime, 108) from[Transaksi] where FcInOut = '0' and id_idl = b.id_idl)) as jam_inout FROM[db_gs_tracking].[dbo].[tlkp_emp] a join t_idl b on a.emp_npk = b.idl_npk and a.plant = b.idl_plant join tlkp_kegiatan_idl c on b.id_kegiatan = c.id left join[db_gs_tracking].[dbo].[Transaksi] t on b.id_idl = t.id_idl group by b.id_idl, b.idl_tgl, b.idl_npk, b.id_nama, b.idl_lokasikebrangkatan, b.id_kegiatan, c.namakegiatan, b.idl_plant, b.idl_keterangan, am.activity_detail";

            //where a.DeptSeksi like '%marketing%' or DeptSeksi like '%sales%' or DeptSeksi like '%RM %'
            var itemQuotation = GSDbContextGSTrack.Database.SqlQuery<ManageIDLTemp>(query).First(p => p.id_idl == key);


            JsonConvert.PopulateObject(values, ManageIDL);
            List<ManageActivityMarketing> listTemp = new List<ManageActivityMarketing>();

            try
            {
                if (itemQuotation != null)
                {
                    listTemp.Add(new ManageActivityMarketing
                    {
                        id_idl = itemQuotation.id_idl,
                        activity_pic = itemQuotation.id_nama.Trim(),
                        activity_date = itemQuotation.idl_tgl,
                        activity_time = itemQuotation.jam_inout,
                        activity_BP = ManageIDL.idl_BP,
                        activity_contact = ManageIDL.idl_contact.Trim(),
                        activity_position = ManageIDL.idl_position.Trim(),
                        activity_type = itemQuotation.namakegiatan,
                        activity_description = itemQuotation.idl_keterangan.Trim(),
                        activity_location = itemQuotation.idl_lokasikebrangkatan,
                        activity_createDate = DateTime.UtcNow.AddHours(7),
                        activity_createBy = sessionLogin.fullname ?? "",
                        activity_status = 1
                    });


                    Validate(listTemp);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

                    GSDbContext.ManageActivityMarketing.AddRange(listTemp);
                    GSDbContext.SaveChanges();

                    return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "error:" + ex.Message.ToString());
            }
            //form.activity_modifDate = DateTime.UtcNow.AddHours(7);
            //form.activity_modifBy = sessionLogin.fullname ?? "";
            //form.activity_status = 2;

            //Validate(form);

            //if (!ModelState.IsValid)
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            //GSDbContext.SaveChanges();

            //return Request.CreateResponse(HttpStatusCode.OK);

            
                //form.activity_createDate = DateTime.UtcNow.AddHours(7);
                //form.activity_createBy = sessionLogin.fullname ?? "";
                //form.activity_status = 1;

            
        }

        [SessionCheck]
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