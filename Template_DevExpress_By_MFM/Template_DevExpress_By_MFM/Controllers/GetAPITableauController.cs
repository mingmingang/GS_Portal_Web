using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;
using RestSharp;
using System.Net;
using System.IO;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class GetAPITableauController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }

        public GetAPITableauController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string urltab)
        {
            if (urltab != null || urltab != "")
            {
                string url_api = "http://gsview.gs.astra.co.id/trusted?username=gsmarketing";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_api);
                myReq.Method = "POST";
                myReq.PreAuthenticate = true;
                myReq.Accept = "/";
                //myReq.Headers.Add("Authorization", ("Bearer " + dataTokenExternal.access_token));
                myReq.ContentType = "application/json";
                string responseFromServer = "";
                var result = new JSON_Token_GSVIEW();
                //JSON_Token_GSVIEW result;
                string datatok = null;
                List<dynamic> dataToken = new List<dynamic>();
                try
                {
                    using (WebResponse response = myReq.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                            if(responseFromServer.Length > 2)
                            {
                                for (int i = 0; i < responseFromServer.Length; i++)
                                {
                                    datatok += responseFromServer[i].ToString();
                                }
                            } else
                            {
                                datatok = responseFromServer;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    responseFromServer = ex.Message.ToString();
                    if (ex.Message.Contains("(401) Unauthorized"))
                    {
                        //RefreshTokenExternal();
                        Console.WriteLine(ex.Message.ToString());
                    }
                    else
                    {
                        Console.WriteLine(ex.Message.ToString());
                        //throw;
                    }
                }

                return Request.CreateResponse(DataSourceLoader.Load(responseFromServer, loadOptions));
            }
            else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
            //var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$CCNT as CCNT, b.T$JOBT as JOBT from dbo.ttccom1008888 a join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT";
        }

        //public void API_External_Get_Report_Week()
        //{
        //    GSDbContext = new GSDbContext(@"GSPORTAL-DEV01", "db_pv_solarpanel", "sa", "gsmis@2017");

        //    var DateNow = DateTime.UtcNow.AddHours(7).AddDays(-6).ToString("yyyy-MM-dd");
        //    var Interval = "Week";

        //    var dataTokenExternal = GSDbContext.MasterPVTokenExternalModel.SingleOrDefault();

        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
        //           | SecurityProtocolType.Tls11
        //           | SecurityProtocolType.Tls12
        //           | SecurityProtocolType.Ssl3;

        //    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

        //    string url_api = "https://uiapi.sunnyportal.com/api/v1/measurements/7516892/energybalance?dateBeginLocal=" + DateNow + "&interval=" + Interval;
        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url_api);
        //    myReq.Method = "GET";
        //    myReq.PreAuthenticate = true;
        //    myReq.Accept = "/";
        //    myReq.Headers.Add("Authorization", ("Bearer " + dataTokenExternal.access_token));

        //    string responseFromServer = "";
        //    try
        //    {
        //        using (WebResponse response = myReq.GetResponse())
        //        {
        //            using (Stream stream = response.GetResponseStream())
        //            {
        //                StreamReader reader = new StreamReader(stream);
        //                responseFromServer = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains("(401) Unauthorized"))
        //        {
        //            RefreshTokenExternal();
        //        }
        //        else
        //        {
        //            Console.WriteLine(ex.Message.ToString());
        //            throw;
        //        }
        //    }

        //    List<ReportChartBarEnergyBalanceModelWeek> ListReportChartBarEnergyBalanceModel = new List<ReportChartBarEnergyBalanceModelWeek>();


        //    if (responseFromServer != null && responseFromServer != "{}")
        //    {
        //        APIReportDateEnergyBalance result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(APIReportDateEnergyBalance)) as APIReportDateEnergyBalance;
        //        if (result != null)
        //        {
        //            if (result.detail != null && result.detail.Count() > 0)
        //            {
        //                foreach (var data in result.detail)
        //                {
        //                    var checkAlreadyData = GSDbContext.ReportChartBarEnergyBalanceModelWeek.Where(p => p.timeUtc == data.timeUtc).SingleOrDefault();
        //                    if (checkAlreadyData == null)
        //                    {
        //                        ReportChartBarEnergyBalanceModelWeek report = new ReportChartBarEnergyBalanceModelWeek();
        //                        report.id_recnum_pv_grup = 1;
        //                        report.batteryCharging = data.batteryCharging;
        //                        //report.batteryDischarging = data.batteryDischarging;
        //                        report.directConsumption = data.directConsumption;
        //                        report.pvGeneration = data.pvGeneration;
        //                        report.totalConsumption = data.totalConsumption;
        //                        report.totalGeneration = data.totalGeneration;
        //                        report.timeUtc = data.timeUtc;
        //                        report.insert_date = DateTime.UtcNow.AddHours(7);
        //                        ListReportChartBarEnergyBalanceModel.Add(report);
        //                    }
        //                    else
        //                    {
        //                        checkAlreadyData.batteryCharging = data.batteryCharging;
        //                        //checkAlreadyData.batteryDischarging = data.batteryDischarging;
        //                        checkAlreadyData.directConsumption = data.directConsumption;
        //                        checkAlreadyData.pvGeneration = data.pvGeneration;
        //                        checkAlreadyData.totalConsumption = data.totalConsumption;
        //                        checkAlreadyData.totalGeneration = data.totalGeneration;
        //                        checkAlreadyData.insert_date = DateTime.UtcNow.AddHours(7);
        //                        GSDbContext.SaveChanges();
        //                    }
        //                }

        //                if (ListReportChartBarEnergyBalanceModel.Count() > 0)
        //                {
        //                    GSDbContext.ReportChartBarEnergyBalanceModelWeek.AddRange(ListReportChartBarEnergyBalanceModel);
        //                    GSDbContext.SaveChanges();
        //                }
        //            }
        //        }
        //    }
        //}
    }
}