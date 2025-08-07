using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ListBPController : ApiController
    {
        public GSDbContext GSDbContext { get; set; }

        public ListBPController()
        {

            GSDbContext = new GSDbContext("", "", "", "");
        }

        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {

            //var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$CCNT as CCNT, b.T$JOBT as JOBT from dbo.ttccom1008888 a join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT";
            //var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$NAMA as cust_name, a.T$CCNT as CCNT, a.T$CADR as CADR, b.T$JOBT as JOBT, b.T$FULN as FULN, c.T$NAMA as alamat2, c.T$NAMC as alamat from dbo.ttccom1008888 a join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT join dbo.ttccom1308888 c on a.T$CADR = c.T$CADR";
            var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$NAMA as cust_name, a.T$CCNT as CCNT, a.T$CADR as CADR, c.T$NAMA as alamat2, c.T$NAMC as alamat, c.T$CCIT as city, c.T$CCTY as country from dbo.ttccom1008888 a join dbo.ttccom1308888 c on a.T$CADR = c.T$CADR left join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT where a.T$BPID not like 'S%' and (a.T$BPID like 'OE%' or a.T$BPID like 'GP%')";
            var dataList = GSDbContext.Database.SqlQuery<ManageBP>(query).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string bpid)
        {
            if (bpid != null || bpid != "")
            {
                //var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$NAMA as cust_name, a.T$CCNT as CCNT, a.T$CADR as CADR, b.T$JOBT as JOBT, b.T$FULN as FULN, c.T$NAMA as alamat2, c.T$NAMC as alamat from dbo.ttccom1008888 a join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT join dbo.ttccom1308888 c on a.T$CADR = c.T$CADR where a.T$BPID='" + bpid + "'";
                var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$NAMA as cust_name, a.T$CCNT as CCNT, a.T$CADR as CADR, c.T$NAMA as alamat2, c.T$NAMC as alamat, c.T$CCIT as city, c.T$CCTY as country from dbo.ttccom1008888 a join dbo.ttccom1308888 c on a.T$CADR = c.T$CADR left join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT where a.T$BPID not like 'S%' and (a.T$BPID like 'OE%' or a.T$BPID like 'GP%') and a.T$BPID='" + bpid + "'";
                var dataList = GSDbContext.Database.SqlQuery<ManageBP>(query).ToList();

                return Request.CreateResponse(DataSourceLoader.Load(dataList, loadOptions));
            } else
            {
                return Request.CreateResponse(DataSourceLoader.Load("", loadOptions));
            }
            //var query = "select a.T$BPID as BPID, a.T$BPID+' - '+a.T$NAMA as display_name, a.T$CCNT as CCNT, b.T$JOBT as JOBT from dbo.ttccom1008888 a join dbo.ttccom1408888 b on a.T$CCNT = b.T$CCNT";
        }
    }
}