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
    public class ManageMasterKursController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }
        public GSDbContextGSTrack GSDbContextGSTrack { get; set; }

        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

        public ManageMasterKursController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
            //GSDbContextGSTrack.Dispose();
        }


        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string tahoen)
        {
            if (!string.IsNullOrEmpty(tahoen))
            {
                var query = "DECLARE @_sql  AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) DECLARE @_colsjual AS NVARCHAR(MAX) DECLARE @_colsbeli AS NVARCHAR(MAX) DECLARE @_colsavg AS NVARCHAR(MAX) SET @_cols = STUFF( (SELECT ',' + QUOTENAME(T.bln, '[]')   FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'') SET @_colsjual = STUFF((SELECT ',' + QUOTENAME(T.bln + '_JUAL', '[]')    FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsbeli = STUFF((SELECT ',' + QUOTENAME(T.bln + '_BELI', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsavg = STUFF((SELECT ',' + QUOTENAME(T.bln + '_AVG', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  set @_sql = ' SELECT tanggal, sum([JAN_JUAL]) [JAN_JUAL],sum([FEB_JUAL]) [FEB_JUAL],sum([MAR_JUAL]) [MAR_JUAL],sum([APR_JUAL]) [APR_JUAL],sum([MAY_JUAL]) [MAY_JUAL],sum([JUN_JUAL])[JUN_JUAL],sum([JUL_JUAL])[JUL_JUAL] ,sum([AUG_JUAL]) [AUG_JUAL],sum([SEP_JUAL]) [SEP_JUAL],sum([OCT_JUAL]) [OCT_JUAL],sum([NOV_JUAL]) [NOV_JUAL],sum([DEC_JUAL]) [DEC_JUAL], sum([JAN_BELI]) [JAN_BELI],sum([FEB_BELI]) [FEB_BELI],sum([MAR_BELI]) [MAR_BELI],sum([APR_BELI]) [APR_BELI],sum([MAY_BELI]) [MAY_BELI],sum([JUN_BELI])[JUN_BELI],sum([JUL_BELI])[JUL_BELI] ,sum([AUG_BELI]) [AUG_BELI],sum([SEP_BELI]) [SEP_BELI],sum([OCT_BELI]) [OCT_BELI],sum([NOV_BELI]) [NOV_BELI],sum([DEC_BELI]) [DEC_BELI], sum([JAN_AVG]) [JAN_AVG],sum([FEB_AVG]) [FEB_AVG],sum([MAR_AVG]) [MAR_AVG],sum([APR_AVG]) [APR_AVG],sum([MAY_AVG]) [MAY_AVG],sum([JUN_AVG])[JUN_AVG],sum([JUL_AVG])[JUL_AVG] ,sum([AUG_AVG]) [AUG_AVG],sum([SEP_AVG]) [SEP_AVG],sum([OCT_AVG]) [OCT_AVG],sum([NOV_AVG]) [NOV_AVG],sum([DEC_AVG]) [DEC_AVG]  FROM(SELECT convert(int, tanggal) as tanggal, (bulan.bln + ''_JUAL'') AS bln_JUAL, (bulan.bln + ''_BELI'') AS bln_BELI,  (bulan.bln + ''_AVG'') AS bln_AVG, kurs.kurs_jual,  kurs.kurs_beli, kurs.kurs_average from tlkp_tanggal as t     FULL OUTER JOIN tlkp_kurs AS kurs on t.tanggal = kurs.kurs_date and kurs.kurs_year = ''" + tahoen + "'' full outer join tlkp_bulan as bulan on kurs.kurs_month = bulan.id     where tanggal is not null ) AS source PIVOT(AVG(kurs_jual) FOR bln_JUAL IN(' + @_colsjual + ')) AS pivot_table_jual PIVOT(AVG(kurs_beli) FOR bln_BELI IN(' + @_colsbeli + ')) AS pivot_table_beli PIVOT(AVG(kurs_average) FOR bln_AVG IN(' + @_colsavg + ')) AS pivot_table_avg GROUP BY tanggal ORDER BY pivot_table_avg.tanggal ASC;  '   exec(@_sql)";
                //var query = "DECLARE @_sql  AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) DECLARE @_colsjual AS NVARCHAR(MAX) DECLARE @_colsbeli AS NVARCHAR(MAX) DECLARE @_colsavg AS NVARCHAR(MAX) SET @_cols = STUFF((SELECT ',' + QUOTENAME(T.bln, '[]')   FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsjual = STUFF((SELECT ',' + QUOTENAME(T.bln + '_JUAL', '[]')    FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsbeli = STUFF((SELECT ',' + QUOTENAME(T.bln + '_BELI', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsavg = STUFF((SELECT ',' + QUOTENAME(T.bln + '_AVG', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  set @_sql = 'SELECT * FROM(SELECT convert(int, tanggal) as tanggal, (bulan.bln + ''_JUAL'') AS bln_JUAL, kurs.kurs_jual, (bulan.bln + ''_BELI'') AS bln_BELI, kurs.kurs_beli, (bulan.bln + ''_AVG'') AS bln_AVG, kurs.kurs_average from tlkp_tanggal as t     FULL OUTER JOIN tlkp_kurs AS kurs on t.tanggal = kurs.kurs_date and kurs.kurs_year = ''"+tahoen+"'' full outer join tlkp_bulan as bulan on kurs.kurs_month = bulan.id     where tanggal is not null) AS source PIVOT(MAX(kurs_jual) FOR bln_JUAL IN(' + @_colsjual + ')) AS pivot_table_jual PIVOT(MAX(kurs_beli) FOR bln_BELI IN(' + @_colsbeli + ')) AS pivot_table_beli PIVOT(MAX(kurs_average) FOR bln_AVG IN(' + @_colsavg + ')) AS pivot_table_avg ORDER BY pivot_table_avg.tanggal ASC;  '  exec(@_sql)";
                //var query = "DECLARE @_sql AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) SET   @_cols = STUFF(    (      SELECT         ',' + QUOTENAME(T.bln, '[]')       FROM         tlkp_bulan AS T       ORDER BY         T.id asc FOR XML PATH(''),         TYPE    ).value('.', 'NVARCHAR(MAX)'),     1,     1,     ''  ) set   @_sql = 'SELECT * FROM(SELECT idLME.lme_id, convert(int, tanggal) as tanggal, bulan.bln, tt.lme_value from tlkp_tanggal as t    FULL OUTER JOIN tlkp_lme AS tt on t.id = tt.lme_date and tt.lme_year = ''"+ thn +"''    full outer join tlkp_bulan as bulan on tt.lme_month = bulan.id outer APPLY (		SELECT top 1 lme_id from tlkp_lme as t		where t.lme_id = tt.lme_id	) as idLME    where tanggal is not null) AS source PIVOT(MAX(lme_value) FOR bln IN(' + @_cols + ')) AS pivot_table ORDER BY pivot_table.tanggal ASC; ' exec(@_sql)";
                var dataList = GSDbContext.Database.SqlQuery<MappingKurs>(query).ToList();
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
            //var query = "select convert(int, t.tanggal) as tanggal, tt.kurs_jual, tt.kurs_beli, tt.kurs_average  from tlkp_tanggal as t    FULL OUTER JOIN tlkp_kurs AS tt on t.id = tt.kurs_date and tt.kurs_year = '2023' where t.tanggal is not null";
            //var query = "DECLARE @_sql  AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) DECLARE @_colsjual AS NVARCHAR(MAX) DECLARE @_colsbeli AS NVARCHAR(MAX) DECLARE @_colsavg AS NVARCHAR(MAX) SET @_cols = STUFF((SELECT ',' + QUOTENAME(T.bln, '[]')   FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsjual = STUFF((SELECT ',' + QUOTENAME(T.bln + '_JUAL', '[]')    FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsbeli = STUFF((SELECT ',' + QUOTENAME(T.bln + '_BELI', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsavg = STUFF((SELECT ',' + QUOTENAME(T.bln + '_AVG', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  set @_sql = 'SELECT * FROM(SELECT convert(int, tanggal) as tanggal, (bulan.bln + ''_JUAL'') AS bln_JUAL, kurs.kurs_jual, (bulan.bln + ''_BELI'') AS bln_BELI, kurs.kurs_beli, (bulan.bln + ''_AVG'') AS bln_AVG, kurs.kurs_average from tlkp_tanggal as t     FULL OUTER JOIN tlkp_kurs AS kurs on t.tanggal = kurs.kurs_date and kurs.kurs_year = ''" + DateTime.UtcNow.AddHours(7).ToString("yyyy") + "'' full outer join tlkp_bulan as bulan on kurs.kurs_month = bulan.id     where tanggal is not null) AS source PIVOT(MAX(kurs_jual) FOR bln_JUAL IN(' + @_colsjual + ')) AS pivot_table_jual PIVOT(MAX(kurs_beli) FOR bln_BELI IN(' + @_colsbeli + ')) AS pivot_table_beli PIVOT(MAX(kurs_average) FOR bln_AVG IN(' + @_colsavg + ')) AS pivot_table_avg ORDER BY pivot_table_avg.tanggal ASC;  '  exec(@_sql)";
            var query = "DECLARE @_sql  AS NVARCHAR(MAX) DECLARE @_cols AS NVARCHAR(MAX) DECLARE @_colsjual AS NVARCHAR(MAX) DECLARE @_colsbeli AS NVARCHAR(MAX) DECLARE @_colsavg AS NVARCHAR(MAX) SET @_cols = STUFF( (SELECT ',' + QUOTENAME(T.bln, '[]')   FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'') SET @_colsjual = STUFF((SELECT ',' + QUOTENAME(T.bln + '_JUAL', '[]')    FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsbeli = STUFF((SELECT ',' + QUOTENAME(T.bln + '_BELI', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  SET @_colsavg = STUFF((SELECT ',' + QUOTENAME(T.bln + '_AVG', '[]')     FROM tlkp_bulan AS T ORDER BY T.id asc            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'),1,1,'')  set @_sql = ' SELECT tanggal, sum([JAN_JUAL]) [JAN_JUAL],sum([FEB_JUAL]) [FEB_JUAL],sum([MAR_JUAL]) [MAR_JUAL],sum([APR_JUAL]) [APR_JUAL],sum([MAY_JUAL]) [MAY_JUAL],sum([JUN_JUAL])[JUN_JUAL],sum([JUL_JUAL])[JUL_JUAL] ,sum([AUG_JUAL]) [AUG_JUAL],sum([SEP_JUAL]) [SEP_JUAL],sum([OCT_JUAL]) [OCT_JUAL],sum([NOV_JUAL]) [NOV_JUAL],sum([DEC_JUAL]) [DEC_JUAL], sum([JAN_BELI]) [JAN_BELI],sum([FEB_BELI]) [FEB_BELI],sum([MAR_BELI]) [MAR_BELI],sum([APR_BELI]) [APR_BELI],sum([MAY_BELI]) [MAY_BELI],sum([JUN_BELI])[JUN_BELI],sum([JUL_BELI])[JUL_BELI] ,sum([AUG_BELI]) [AUG_BELI],sum([SEP_BELI]) [SEP_BELI],sum([OCT_BELI]) [OCT_BELI],sum([NOV_BELI]) [NOV_BELI],sum([DEC_BELI]) [DEC_BELI], sum([JAN_AVG]) [JAN_AVG],sum([FEB_AVG]) [FEB_AVG],sum([MAR_AVG]) [MAR_AVG],sum([APR_AVG]) [APR_AVG],sum([MAY_AVG]) [MAY_AVG],sum([JUN_AVG])[JUN_AVG],sum([JUL_AVG])[JUL_AVG] ,sum([AUG_AVG]) [AUG_AVG],sum([SEP_AVG]) [SEP_AVG],sum([OCT_AVG]) [OCT_AVG],sum([NOV_AVG]) [NOV_AVG],sum([DEC_AVG]) [DEC_AVG]  FROM(SELECT convert(int, tanggal) as tanggal, (bulan.bln + ''_JUAL'') AS bln_JUAL, (bulan.bln + ''_BELI'') AS bln_BELI,  (bulan.bln + ''_AVG'') AS bln_AVG, kurs.kurs_jual,  kurs.kurs_beli, kurs.kurs_average from tlkp_tanggal as t     FULL OUTER JOIN tlkp_kurs AS kurs on t.tanggal = kurs.kurs_date and kurs.kurs_year = ''" + DateTime.UtcNow.AddHours(7).ToString("yyyy") + "'' full outer join tlkp_bulan as bulan on kurs.kurs_month = bulan.id     where tanggal is not null ) AS source PIVOT(AVG(kurs_jual) FOR bln_JUAL IN(' + @_colsjual + ')) AS pivot_table_jual PIVOT(AVG(kurs_beli) FOR bln_BELI IN(' + @_colsbeli + ')) AS pivot_table_beli PIVOT(AVG(kurs_average) FOR bln_AVG IN(' + @_colsavg + ')) AS pivot_table_avg GROUP BY tanggal ORDER BY pivot_table_avg.tanggal ASC;  '   exec(@_sql)";
            var dataList = GSDbContext.Database.SqlQuery<MappingKurs>(query).ToList();
            //var dataList = GSDbContext.MasterKurs.ToList();
            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }


        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var id_kurs = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var dateNow = DateTime.UtcNow.AddHours(7).ToString("yyyy");
            var splitMonth = values.Replace("\"", "").Replace("{", "").Replace("}", "").Split(':');
            
            //JsonConvert.PopulateObject(values, MasterLME_input);
            //var getMM = splitMonth[0];
            //var getValue = splitMonth[1];
            //var getMonth = GSDbContext.MasterBulan.First(i => i.bln == getMM);

            var getMM = splitMonth[0].Substring(0, 3);
            var getInfoMM = splitMonth[0].Split('_');
            var getValue = splitMonth[1];
            var getMonth = GSDbContext.MasterBulan.First(i => i.bln == getMM);

            //var master = GSDbContext.MasterKurs.Where(e => e.kurs_date == id_kurs.ToString() && e.kurs_month == getMonth.id.ToString() && e.kurs_year == thn).ToList();

            var master = GSDbContext.MasterKurs.Where(e => e.kurs_date == id_kurs.ToString() && e.kurs_month == getMonth.id.ToString() && e.kurs_year == dateNow).ToList();
            if (master.Count() > 0)
            {
                foreach (var data in master)
                {
                    var getDate = data.kurs_date;
                    if (data.kurs_id != null)
                    {
                        //update data Kurs
                        if (getInfoMM[1].Equals("JUAL") || getInfoMM[1] == "JUAL")
                        {
                            data.kurs_jual = Convert.ToDecimal(getValue);
                            if (data.kurs_beli != null)
                            {
                                //data.kurs_average = Math.Round((decimal)(Convert.ToDecimal(getValue) + data.kurs_beli) / 2, 2);
                                var avg = (Convert.ToDecimal(getValue) + data.kurs_beli) / 2;
                                var Savg = String.Format("{0:F2}", avg);
                                data.kurs_average = Convert.ToDecimal(Savg);
                            }
                        }
                        else if (getInfoMM[1].Equals("BELI") || getInfoMM[1] == "BELI")
                        {
                            data.kurs_beli = Convert.ToDecimal(getValue);
                            if (data.kurs_jual != null)
                            {
                                var avg = (Convert.ToDecimal(getValue) + data.kurs_jual) / 2;
                                var Savg = String.Format("{0:F2}", avg);
                                //data.kurs_average = (decimal?)Math.Round((double)avg, 2);
                                data.kurs_average = Convert.ToDecimal(Savg);
                            }
                        }
                        else if (getInfoMM[1].Equals("AVG") || getInfoMM[1] == "AVG")
                        {
                            data.kurs_average = Convert.ToDecimal(getValue);
                        }
                        data.kurs_modifDate = DateTime.UtcNow.AddHours(7);
                        data.kurs_modifBy = sessionLogin.fullname ?? "";
                        GSDbContext.SaveChanges();
                    }
                    else
                    {
                        //insert data Kurs
                        MasterKurs MasterKurs_input = new MasterKurs();
                        if (getInfoMM[1].Equals("JUAL") || getInfoMM[1] == "JUAL")
                        {
                            MasterKurs_input.kurs_jual = Convert.ToDecimal(getValue);
                        }
                        else if (getInfoMM[1].Equals("BELI") || getInfoMM[1] == "BELI")
                        {
                            MasterKurs_input.kurs_beli = Convert.ToDecimal(getValue);
                        }
                        else if (getInfoMM[1].Equals("AVG") || getInfoMM[1] == "AVG")
                        {
                            MasterKurs_input.kurs_average = Convert.ToDecimal(getValue);
                        }

                        MasterKurs_input.kurs_month = getMonth.id.ToString();
                        MasterKurs_input.kurs_date = id_kurs.ToString();
                        MasterKurs_input.kurs_year = dateNow;
                        MasterKurs_input.kurs_createDate = DateTime.UtcNow.AddHours(7);
                        MasterKurs_input.kurs_createBy = sessionLogin.fullname ?? "";
                        MasterKurs_input.kurs_status = 1;

                        GSDbContext.MasterKurs.Add(MasterKurs_input);
                        GSDbContext.SaveChanges();
                    }


                }


            }
            else
            {
                //insert data Kurs
                MasterKurs MasterKurs_input = new MasterKurs();
                if (getInfoMM[1].Equals("JUAL") || getInfoMM[1] == "JUAL")
                {
                    MasterKurs_input.kurs_jual = Convert.ToDecimal(getValue);
                }
                else if (getInfoMM[1].Equals("BELI") || getInfoMM[1] == "BELI")
                {
                    MasterKurs_input.kurs_beli = Convert.ToDecimal(getValue);
                }
                else if (getInfoMM[1].Equals("AVG") || getInfoMM[1] == "AVG")
                {
                    MasterKurs_input.kurs_average = Convert.ToDecimal(getValue);
                }

                MasterKurs_input.kurs_month = getMonth.id.ToString();
                MasterKurs_input.kurs_date = id_kurs.ToString();
                MasterKurs_input.kurs_year = dateNow;
                MasterKurs_input.kurs_createDate = DateTime.UtcNow.AddHours(7);
                MasterKurs_input.kurs_createBy = sessionLogin.fullname ?? "";
                MasterKurs_input.kurs_status = 1;

                GSDbContext.MasterKurs.Add(MasterKurs_input);
                GSDbContext.SaveChanges();
            }


            Validate(form);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage Put(FormDataCollection form, string tahoen)
        {
            var id_kurs = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var splitMonth = values.Replace("\"", "").Replace("{", "").Replace("}", "").Split(':');
            
            //JsonConvert.PopulateObject(values, MasterLME_input);
            var getMM = splitMonth[0].Substring(0, 3);
            var getInfoMM = splitMonth[0].Split('_');
            var getValue = splitMonth[1];
            var getMonth = GSDbContext.MasterBulan.First(i => i.bln == getMM);

            var master = GSDbContext.MasterKurs.Where(e => e.kurs_date == id_kurs.ToString() && e.kurs_month == getMonth.id.ToString() && e.kurs_year == tahoen).ToList();
            if (master.Count() > 0)
            {
                foreach(var data in master)
                {   
                    var getDate = data.kurs_date;
                    if (data.kurs_id != null)
                    {
                        //update data Kurs
                        if (getInfoMM[1].Equals("JUAL") || getInfoMM[1] == "JUAL")
                        {
                            data.kurs_jual = Convert.ToDecimal(getValue);
                            if(data.kurs_beli != null)
                            {                                
                                //data.kurs_average = Math.Round((decimal)(Convert.ToDecimal(getValue) + data.kurs_beli) / 2, 2);
                                var avg = (Convert.ToDecimal(getValue) + data.kurs_beli) / 2;
                                var Savg = String.Format("{0:F2}", avg);
                                data.kurs_average = Convert.ToDecimal(Savg);
                            }
                        }
                        else if (getInfoMM[1].Equals("BELI") || getInfoMM[1] == "BELI")
                        {
                            data.kurs_beli = Convert.ToDecimal(getValue);
                            if (data.kurs_jual != null)
                            {
                                var avg = (Convert.ToDecimal(getValue) + data.kurs_jual) / 2;
                                var Savg = String.Format("{0:F2}", avg);
                                //data.kurs_average = (decimal?)Math.Round((double)avg, 2);
                                data.kurs_average = Convert.ToDecimal(Savg);
                            }
                        }
                        else if (getInfoMM[1].Equals("AVG") || getInfoMM[1] == "AVG")
                        {
                            data.kurs_average = Convert.ToDecimal(getValue);
                        }
                        data.kurs_modifDate = DateTime.UtcNow.AddHours(7);
                        data.kurs_modifBy = sessionLogin.fullname ?? "";
                        GSDbContext.SaveChanges();
                    }
                    else
                    {
                        //insert data Kurs
                        MasterKurs MasterKurs_input = new MasterKurs();
                        if (getInfoMM[1].Equals("JUAL") || getInfoMM[1] == "JUAL")
                        {
                            MasterKurs_input.kurs_jual = Convert.ToDecimal(getValue);
                        }
                        else if (getInfoMM[1].Equals("BELI") || getInfoMM[1] == "BELI")
                        {
                            MasterKurs_input.kurs_beli = Convert.ToDecimal(getValue);
                        }
                        else if (getInfoMM[1].Equals("AVG") || getInfoMM[1] == "AVG")
                        {
                            MasterKurs_input.kurs_average = Convert.ToDecimal(getValue);
                        }

                        MasterKurs_input.kurs_month = getMonth.id.ToString();
                        MasterKurs_input.kurs_date = id_kurs.ToString();
                        MasterKurs_input.kurs_year = tahoen;
                        MasterKurs_input.kurs_createDate = DateTime.UtcNow.AddHours(7);
                        MasterKurs_input.kurs_createBy = sessionLogin.fullname ?? "";
                        MasterKurs_input.kurs_status = 1;

                        GSDbContext.MasterKurs.Add(MasterKurs_input);
                        GSDbContext.SaveChanges();
                    }


                }
                

            } else
            {
                //insert data Kurs
                MasterKurs MasterKurs_input = new MasterKurs();
                if (getInfoMM[1].Equals("JUAL") || getInfoMM[1] == "JUAL")
                {
                    MasterKurs_input.kurs_jual = Convert.ToDecimal(getValue);
                } else if (getInfoMM[1].Equals("BELI") || getInfoMM[1] == "BELI")
                {
                    MasterKurs_input.kurs_beli = Convert.ToDecimal(getValue);
                } else if (getInfoMM[1].Equals("AVG") || getInfoMM[1] == "AVG")
                {
                    MasterKurs_input.kurs_average = Convert.ToDecimal(getValue);
                }

                MasterKurs_input.kurs_month = getMonth.id.ToString();
                MasterKurs_input.kurs_date = id_kurs.ToString();
                MasterKurs_input.kurs_year = tahoen;
                MasterKurs_input.kurs_createDate = DateTime.UtcNow.AddHours(7);
                MasterKurs_input.kurs_createBy = sessionLogin.fullname ?? "";
                MasterKurs_input.kurs_status = 1;

                GSDbContext.MasterKurs.Add(MasterKurs_input);
                GSDbContext.SaveChanges();
            }

            
            Validate(form);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            return Request.CreateErrorResponse(HttpStatusCode.Created, "success");
        }


    }
}