using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Controllers;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class OrderController : ApiController
    {

        public GSDbContext GSDbContext { get; set; }

        public OrderController()
        {
            GSDbContext = new GSDbContext(@"", "", "", "");
        }
        protected override void Dispose(bool disposing)
        {
            GSDbContext.Dispose();
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string dateFrom, string dateTo)
        {
            LoadResult loadResult = new LoadResult();

            if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
            {
                string sSQLSelectHeaderDetail = "";

                //sSQLSelectHeaderDetail = "SELECT  " +
                //    " a.id_order, a.month_order, a.year_order, a.order_date, a.po_number,  " +
                //    " (SELECT TOP 1 ISNULL(update_time, insert_time) FROM manage_order where id_order = a.id_order order by update_time desc) AS update_time," +
                //    " ISNULL(SUM(a.ship_to_JKT), 0) AS ship_to_JKT," +
                //    " ISNULL(SUM(a.ship_to_BDG), 0) AS ship_to_BDG," +
                //    " ISNULL(SUM(a.ship_to_SBY), 0) AS ship_to_SBY," +
                //    " ISNULL(SUM(a.ship_to_SMG), 0) AS ship_to_SMG," +
                //    " ISNULL(SUM(a.ship_to_JKT + a.ship_to_BDG + a.ship_to_SBY + a.ship_to_SMG), 0) as total_temp, " +
                //    " ISNULL(SUM(total), 0) as total,  " +
                //    " ISNULL(SUM(a.confirm), 0) as confirm, " +
                //    " (ISNULL(SUM(a.confirm), 0) - ISNULL(SUM(total), 0)) as adjustment, " +
                //    " (SELECT TOP 1 status_order FROM manage_order WHERE id_order = a.id_order ORDER BY status_order DESC) AS status_order" +
                //    " FROM manage_order a" +
                //    " WHERE  YEAR(order_date) >= YEAR('" + dateFrom + "') AND YEAR(order_date) <= YEAR('" + dateTo + "') AND " +
                //    " MONTH(order_date) >= MONTH('" + dateFrom + "') AND MONTH(order_date) <= MONTH('" + dateTo + "')  " +
                //    " GROUP BY a.id_order, a.month_order, a.year_order, a.order_date, a.po_number ORDER BY a.id_order DESC";

                sSQLSelectHeaderDetail = "SELECT a.id_order, a.month_order, a.year_order, a.order_date, a.po_number,   " +
                    " (SELECT MAX(ISNULL(update_time, insert_time))  FROM manage_order where id_order = a.id_order) AS update_time," +
                    " ISNULL((SELECT TOP 1 SUM(ISNULL(ship_to_JKT, 0)) FROM manage_order WHERE id_order = a.id_order GROUP BY status_order order by status_order DESC),0) AS ship_to_JKT," +
                    " ISNULL((SELECT TOP 1 SUM(ISNULL(ship_to_BDG, 0)) FROM manage_order WHERE id_order = a.id_order GROUP BY status_order order by status_order DESC),0) AS ship_to_BDG," +
                    " ISNULL((SELECT TOP 1 SUM(ISNULL(ship_to_SBY, 0)) FROM manage_order WHERE id_order = a.id_order GROUP BY status_order order by status_order DESC),0) AS ship_to_SBY," +
                    " ISNULL((SELECT TOP 1 SUM(ISNULL(ship_to_SMG, 0)) FROM manage_order WHERE id_order = a.id_order GROUP BY status_order order by status_order DESC),0) AS ship_to_SMG," +
                    //" (SELECT ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) FROM manage_order WHERE id_order = a.id_order AND status_order = 0) AS total_po_ori," +
                    " ISNULL((SELECT CASE WHEN ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) < 1 THEN SUM(total)" +
                    " WHEN ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) > 0 THEN ISNULL(SUM(ship_to_JKT +ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) " +
                    " END AS total_po_ori FROM manage_order WHERE id_order = a.id_order AND status_order = 0),0) AS total_po_ori," +
                    " ISNULL(SUM(a.ship_to_JKT + a.ship_to_BDG + a.ship_to_SBY + a.ship_to_SMG), 0) as total_temp, " +
                    " ISNULL(SUM(a.confirm), 0) as confirm, " +
                    //" (SELECT TOP 1 (ISNULL(SUM(a.confirm), 0) - ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0)) FROM manage_order WHERE id_order = a.id_order GROUP BY status_order order by status_order DESC) AS adjustment, " +
                    " ISNULL((ISNULL(SUM(a.confirm), 0) - (SELECT CASE WHEN ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) < 1 THEN SUM(total)" +
                    " WHEN ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) > 0 THEN ISNULL(SUM(ship_to_JKT +ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) " +
                    " END AS total_po_ori FROM manage_order WHERE id_order = a.id_order AND status_order = 0)),0) AS adjustment, " +
                    " ISNULL((SELECT ISNULL(SUM(ship_to_JKT + ship_to_BDG + ship_to_SBY + ship_to_SMG), 0) FROM manage_order WHERE id_order = a.id_order AND status_order = 3),0) AS total_po_agreed," +
                    " (SELECT MAX(status_order) FROM manage_order WHERE id_order = a.id_order) AS status_order" +
                    " FROM manage_order a WHERE " +
                    " YEAR(order_date) >= YEAR('" + dateFrom + "') AND YEAR(order_date) <= YEAR('" + dateTo + "') AND " +
                    " MONTH(order_date) >= MONTH('" + dateFrom + "') AND MONTH(order_date) <= MONTH('" + dateTo + "')  " +
                    " GROUP BY a.id_order, a.month_order, a.year_order, a.order_date, a.po_number ORDER BY a.id_order DESC";

                var getData = GSDbContext.Database.SqlQuery<ListHeaderManageOrder>(sSQLSelectHeaderDetail).ToList();


                loadResult = DataSourceLoader.Load(getData, loadOptions);
            }
            else
            {
                //var dataList = GSDbContext.ManageOrder.ToList();
                //loadResult = DataSourceLoader.Load(dataList, loadOptions);

                string sSQLSelect = "";
                //sSQLSelect += "select a.id_order, (select top 1 id_recnum_order " +
                //    " from manage_order where id_order = a.id_order " +
                //    " order by update_time, insert_time desc) as id_recnum_order " +
                //    " from manage_order a group by a.id_order order by a.id_order desc";

                sSQLSelect += "SELECT " +
                    " a.id_order, a.month_order, a.year_order, a.order_date, a.po_number, " +
                    " (SELECT TOP 1 ISNULL(update_time, insert_time) FROM manage_order where id_order = a.id_order order by update_time desc) AS update_time, " +
                    " SUM(a.ship_to_JKT) AS ship_to_JKT, SUM(a.ship_to_BDG) AS ship_to_BDG, SUM(a.ship_to_SBY) AS ship_to_SBY, SUM(a.ship_to_SMG) AS ship_to_SMG," +
                    " (SELECT TOP 1 id_recnum_order FROM manage_order WHERE id_order = a.id_order ORDER BY update_time, insert_time DESC) AS id_recnum_order " +
                    //" a.status_order" +
                    " FROM manage_order a " +
                    //" GROUP BY a.id_order ORDER BY a.status_order DESC";
                    " GROUP BY a.id_order ORDER BY a.id_order DESC";

                var dataList = GSDbContext.Database.SqlQuery<ListHeaderManageOrder>(sSQLSelect).ToList();
                List<ManageOrder> tempList = new List<ManageOrder>();

                //foreach (var itemList in dataList)
                //{
                //    var getData = GSDbContext.ManageOrder.Where(p => p.id_recnum_order == itemList.id_recnum_order).SingleOrDefault();

                //    tempList.Add(getData);
                //}

                loadResult = DataSourceLoader.Load(tempList, loadOptions);
            }

            return Request.CreateResponse(loadResult);
        }


        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage ViewDetails(string id, DataSourceLoadOptions loadOptions)
        {
            var query = "SELECT id_recnum_order, id_order, po_number, sales_order, pn_customer, lot_size, type_battery, type_material, brand, group_order, spec, status_order, insert_time, update_time, user_input, pn_gs, month_order, year_order, " +
                " CASE WHEN confirm != 0 THEN confirm " +
                " WHEN confirm = 0 THEN total ELSE total END as total , " +
                " confirm, ship_to_JKT, ship_to_BDG, ship_to_SBY, ship_to_SMG, order_date, confirm_to_JKT, confirm_to_BDG, confirm_to_SBY, confirm_to_SMG, adjustment, file_excel " +
                " FROM dbo.manage_order WHERE id_order = '" + id + "'";
            var detail = GSDbContext.Database.SqlQuery<ManageOrder>(query).ToList();
            //var detail = GSDbContext.ManageOrder.Where(e => e.id_order == id).ToList();

            return Request.CreateResponse(DataSourceLoader.Load(detail, loadOptions));
        }


        [SessionCheck]
        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var values = form.Get("values");

            var newOrder = new ManageOrder();
            JsonConvert.PopulateObject(values, newOrder);

            Validate(newOrder);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            newOrder.insert_time = DateTime.UtcNow.AddHours(7);

            GSDbContext.ManageOrder.Add(newOrder);
            GSDbContext.SaveChanges();

            ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
            manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
            manageHistoryTransaction.type_history = "transaction_order";
            manageHistoryTransaction.desc_history = "Order added : " + newOrder.id_order;
            GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);
            GSDbContext.SaveChanges();

           

            return Request.CreateResponse(HttpStatusCode.Created);
        }


        [SessionCheck]
        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var values = form.Get("values");
            var order = GSDbContext.ManageOrder.First(e => e.id_recnum_order == key);

            var before_qtyShipBDG = order.ship_to_BDG;
            var before_qtyShipJKT= order.ship_to_JKT;
            var before_qtyShipSBY= order.ship_to_SBY;
            var before_qtyShipSMG = order.ship_to_SMG;

            JsonConvert.PopulateObject(values, order);

            var after_qtyShipBDG = order.ship_to_BDG;
            var after_qtyShipJKT = order.ship_to_JKT;
            var after_qtyShipSBY = order.ship_to_SBY;
            var after_qtyShipSMG = order.ship_to_SMG;
            order.total = (order.ship_to_JKT + order.ship_to_BDG + order.ship_to_SBY + order.ship_to_SMG);

            Validate(order);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.GetFullErrorMessage());

            order.update_time = DateTime.UtcNow.AddHours(7);

            ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
            manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
            manageHistoryTransaction.type_history = "transaction_order";
            manageHistoryTransaction.desc_history = "Order edited : " + Environment.NewLine;
            manageHistoryTransaction.desc_history += "Before : BDG " + Math.Round(before_qtyShipBDG, 2) + " --> After : BDG " + Math.Round(after_qtyShipBDG, 2) + Environment.NewLine;
            manageHistoryTransaction.desc_history += "Before : JKT " + Math.Round(before_qtyShipJKT, 2) + " --> After : JKT " + Math.Round(after_qtyShipJKT, 2) + Environment.NewLine;
            manageHistoryTransaction.desc_history += "Before : SBY " + Math.Round(before_qtyShipSBY, 2) + " --> After : SBY " + Math.Round(after_qtyShipSBY, 2) + Environment.NewLine;
            manageHistoryTransaction.desc_history += "Before : SMG " + Math.Round(before_qtyShipSMG, 2) + " --> After : SMG " + Math.Round(after_qtyShipSMG, 2) + Environment.NewLine;
            GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);

            GSDbContext.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [SessionCheck]
        [HttpDelete]
        public HttpResponseMessage Delete(FormDataCollection form, string id_order = null)
        {
            int statusCode = 500;
            if (!String.IsNullOrEmpty(id_order))
            {
                var dataAll = GSDbContext.ManageOrder.Where(p => p.id_order == id_order).ToList();
                GSDbContext.ManageOrder.RemoveRange(dataAll);
                GSDbContext.SaveChanges();
                statusCode = Convert.ToInt32(HttpStatusCode.OK);
            }
            else
            {
                var key = Convert.ToInt32(form.Get("key"));
                var order = GSDbContext.ManageOrder.First(e => e.id_recnum_order == key);

                ManageHistoryTransaction manageHistoryTransaction = new ManageHistoryTransaction();
                manageHistoryTransaction.datetime_history = DateTime.UtcNow.AddHours(7);
                manageHistoryTransaction.type_history = "transaction_order";
                manageHistoryTransaction.desc_history = "Order removed : " + order.id_order;
                GSDbContext.ManageHistoryTransaction.Add(manageHistoryTransaction);

                GSDbContext.ManageOrder.Remove(order);
                GSDbContext.SaveChanges();
                statusCode = Convert.ToInt32(HttpStatusCode.OK);
            }             
            return Request.CreateResponse(statusCode);
        }

    }
}