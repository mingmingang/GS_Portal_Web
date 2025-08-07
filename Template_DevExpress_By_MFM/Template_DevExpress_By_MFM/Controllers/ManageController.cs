using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using PagedList;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Data.OleDb;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Web.Routing;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ManageController : Controller
    {
        private SessionLogin sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
        public GSDbContext GSDbContext { get; set; }

        public ManageController()
        {
            if (sessionLogin != null)
            {

                GSDbContext = new GSDbContext("", "", "", "");
            }
            else
            {
                RedirectToAction("Index", "Login");
            }

        }
        protected override void Dispose(bool disposing)
        {
            if (sessionLogin != null)
            {
                GSDbContext.Dispose();
            }
            else
            {
                RedirectToAction("Index", "Login");
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            //Do your logging
            // and redirect / return error view
            filterContext.ExceptionHandled = true;
            // If the exception occured in an ajax call. Send a json response back
            // (you need to parse this and display to user as needed at client side)
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { Error = true, Message = filterContext.Exception.Message }
                };
                filterContext.HttpContext.Response.StatusCode = 500; // Set as needed
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Login" }, { "action", "Index" } });
                //Assuming the view exists in the "~/Views/Shared" folder
            }
        }

        // AREA MANAGE BusinessPlan
        [SessionCheck]
        public ActionResult ListManageBusinessPlan()
        {
            return View();
        }

        // AREA MANAGE YearlyPlan
        [SessionCheck]
        public ActionResult ListManageYearlyPlan()
        {
            return View();
        }
        
        // AREA MANAGE ItemPartNumber
        [SessionCheck]
        public ActionResult ListManageItemPartNumber()
        {
            return View();
        }

        // AREA MANAGE FORECAST
        [SessionCheck]
        public ActionResult ListManageForecast()
        {
            return View();
        }

        // AREA MANAGE ORDER
        [SessionCheck]
        public ActionResult ListManageOrder()
        {
            return View();
        }

        // AREA MANAGE PRICE SIMULATION
        [SessionCheck]
        public ActionResult ListManagePriceSimulation()
        {
            return View();
        }

        // AREA MANAGE PRICE QUOTATAION
        [SessionCheck]
        public ActionResult ListManagePriceQuotation()
        {
            return View();
        }

        // AREA MANAGE ACTIVITY MARKETING
        [SessionCheck]
        public ActionResult ListManageActivityMarketing()
        {
            return View();
        }

        // AREA MANAGE LME
        [SessionCheck]
        public ActionResult ManageMasterLME()
        {
            return View();
        }

        // AREA MANAGE Kurs
        [SessionCheck]
        public ActionResult ManageMasterKurs()
        {
            return View();
        }

        // AREA MANAGE USER ACCOUNT
        [SessionCheck]
        public ActionResult ManageMasterUser()
        {
            return View();
        }

        // AREA MANAGE CUSTOMER
        [SessionCheck]
        public ActionResult ManageMasterCustomer()
        {
            return View();
        }

        // AREA MANAGE PART NUMBER
        [SessionCheck]
        public ActionResult ManageMasterPartNumber()
        {
            return View();
        }

        // AREA MANAGE COUNTRY
        [SessionCheck]
        public ActionResult ManageMasterCountry()
        {
            return View();
        }

        // AREA MANAGE EMAIL
        [SessionCheck]
        public ActionResult ManageMasterEmail()
        {
            return View();
        }

        // AREA MANAGE ORDEL
        [SessionCheck]
        public ActionResult OrdervsDelivery()
        {
            return View();
        }

        // AREA MANAGE LOG PRICE
        [SessionCheck]
        public ActionResult LogPrice()
        {
            return View();
        }

        // AREA MANAGE TYPE BATTERY
        [SessionCheck]
        public ActionResult ManageMasterType()
        {
            return View();
        }

        // AREA MANAGE ATTENTION
        [SessionCheck]
        public ActionResult ManageMasterAttn()
        {
            return View();
        }

        [SessionCheck]
        public ActionResult CheckAlreadyYearlyPlan(int tahun)
        {
            string sResponseResult = "Success";
            int sResponseCode = 200;
            var checkYearlyPlan = GSDbContext.ManageYearlyPlan.Where(p => p.tahun == tahun).ToList();
            if(checkYearlyPlan.Count() == 0)
            {
                sResponseCode = 500;
                sResponseResult = "Yearly plan is Empty of the year of your choice! Please upload yearly plan first.";
            }

            return Json(new { responsecode = sResponseCode, responseresult = sResponseResult }, JsonRequestBehavior.AllowGet);
        }


        public void SendEmail(string emailTo, string messagesubject, string messageTitle, string messageBody, string messageURL)
        {
            string to = emailTo;
            string from = "noreply@gs.astra.co.id";
            MailMessage message = new MailMessage(from, to);
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = true;
            var credential = new NetworkCredential
            {
                UserName = "noreply@gs.astra.co.id",
                Password = "gsgs12345%"
            };
            client.Credentials = credential;
            client.Host = "10.19.25.5";
            client.Port = 465;
            //client.EnableSsl = true; 

            message.Subject = messagesubject;
            message.IsBodyHtml = true;

            message.Body = @"<body style='margin: 0;padding: 0;-webkit-text-size-adjust: 100%;background-color: #dde5e7;color: #000000'>
              <table style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;min-width: 320px;Margin: 0 auto;background-color: #dde5e7;width:100%' cellpadding='0' cellspacing='0'>
              <tbody>
              <tr style='vertical-align: top'>
                <td style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
            <div class='u-row-container' style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:arial,helvetica,sans-serif;' align='left'>

              <table height='0px' align='center' border='0' cellpadding='0' cellspacing='0' width='100%' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%'>
                <tbody>
                  <tr style='vertical-align: top'>
                    <td style='word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%'>
                      <span>&#160;</span>
                    </td>
                  </tr>
                </tbody>
              </table>

                  </td>
                </tr>
              </tbody>
            </table>

              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>
                </div>
              </div>
            </div>



            <div class='u-row-container' style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table id='u_content_image_8' style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:33px 10px;font-family:arial,helvetica,sans-serif;' align='left'>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
              <tr>
                <td style='padding-right: 0px;padding-left: 0px;' align='center'>
                  <a href='#'>
                  <img align='center' border='0' src='https://iili.io/6yhfea.png' alt='Logo' title='Logo' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 30%;' width='122.8' class='v-src-width v-src-max-width'/>
                  </a>
                </td>
              </tr>
            </table>

                  </td>
                </tr>
              </tbody>
            </table>

              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>

                </div>
              </div>
            </div>



            <div class='u-row-container' style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>

            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table id='u_content_heading_6' style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:40px 10px 6px;font-family:arial,helvetica,sans-serif;' align='left'>

              <h1 style='margin: 0px; color: #000000; line-height: 140%; text-align: center; word-wrap: break-word; font-weight: normal; font-family: tahoma,arial,helvetica,sans-serif; font-size: 32px;'>
                " + messageTitle + @"
              </h1>

                  </td>
                </tr>
              </tbody>
            </table>

            <table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:arial,helvetica,sans-serif;' align='left'>

              <div style='line-height: 140%; text-align: center; word-wrap: break-word;'>
             <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 18px; line-height: 25.2px;'><strong>Order Number : " + messageBody + @"</strong></span></p>
                <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 18px; line-height: 25.2px;'><strong>Open Portal</strong></span></p>
                <a href='" + messageURL + @"' target='_blank'>
                <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 18px; line-height: 25.2px;'><strong>" + messageURL + @"</strong></span></p>
                </a>
              </div>

                  </td>
                </tr>
              </tbody>
            </table>
              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>
                </div>
              </div>
            </div>

<div  style='padding: 0px;background-color: transparent'>
  <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #34495e;'>
    <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
<div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
  <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
  <div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
  
<table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
  <tbody>
    <tr>
      <td  style='overflow-wrap:break-word;word-break:break-word;padding:60px 10px 0px;font-family:arial,helvetica,sans-serif;' align='left'>
        
      </td>
    </tr>
  </tbody>
</table>

<table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
  <tbody>
    <tr>
      <td  style='overflow-wrap:break-word;word-break:break-word;padding:27px 10px 30px;font-family:arial,helvetica,sans-serif;' align='left'>
        
<div align='center'>
  <div style='text-align: center; display: table; max-width:155px;'>
    <table align='left' border='0' cellspacing='0' cellpadding='0' width='32' height='32' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 20px'>
      <tbody><tr style='vertical-align: top'><td align='left' valign='middle' style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
       
      </td></tr>
    </tbody></table>
    <table align='left' border='0' cellspacing='0' cellpadding='0' width='32' height='32' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 20px'>
      <tbody><tr style='vertical-align: top'><td align='left' valign='middle' style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
        
      </td></tr>
    </tbody></table>
    <table align='left' border='0' cellspacing='0' cellpadding='0' width='32' height='32' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 0px'>
      <tbody><tr style='vertical-align: top'><td align='left' valign='middle' style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
       
      </td></tr>
    </tbody></table>
  </div>
</div>

      </td>
    </tr>
  </tbody>
</table>

</div>
  </div>
</div>
    </div>
  </div>
</div>

            <div style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:15px 10px;font-family:arial,helvetica,sans-serif;' align='left'>

              <div style='color: #7e8c8d; line-height: 140%; text-align: center; word-wrap: break-word;'>
                <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 14px; line-height: 19.6px; font-family: arial, helvetica, sans-serif;'>&copy;2022 PT. GS Battery | Karawang West Java - Indonesia</span></p>
              </div>

                  </td>
                </tr>
              </tbody>
            </table>

              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>
                </div>
              </div>
            </div>


                </td>
              </tr>
              </tbody>
              </table>
            </body>
            ";



            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendEmail(): {0}",
                        ex.ToString());
            }

        }

        public static void SendEmail_Notification(string emailTo, string cc, string messagesubject, string messageTitle, string messageBody, string messageURL, string file_url)
        {

            string to = emailTo;
            string from = "noreply@gs.astra.co.id";
            MailMessage message = new MailMessage(from, to);
            message.CC.Add(cc);
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = true;
            var credential = new NetworkCredential
            {
                UserName = "noreply@gs.astra.co.id",
                Password = "gsgs12345%"
            };
            client.Credentials = credential;
            client.Host = "10.19.25.5";
            client.Port = 465;
            //client.EnableSsl = true; 

            message.Subject = messagesubject;
            message.IsBodyHtml = true;

            message.Body = @"<body style='margin: 0;padding: 0;-webkit-text-size-adjust: 100%;background-color: #dde5e7;color: #000000'>
              <table style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;min-width: 320px;Margin: 0 auto;background-color: #dde5e7;width:100%' cellpadding='0' cellspacing='0'>
              <tbody>
              <tr style='vertical-align: top'>
                <td style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
            <div class='u-row-container' style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:arial,helvetica,sans-serif;' align='left'>

              <table height='0px' align='center' border='0' cellpadding='0' cellspacing='0' width='100%' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #BBBBBB;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%'>
                <tbody>
                  <tr style='vertical-align: top'>
                    <td style='word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%'>
                      <span>&#160;</span>
                    </td>
                  </tr>
                </tbody>
              </table>

                  </td>
                </tr>
              </tbody>
            </table>

              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>
                </div>
              </div>
            </div>



            <div class='u-row-container' style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table id='u_content_image_8' style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:33px 10px;font-family:arial,helvetica,sans-serif;' align='left'>

            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
              <tr>
                <td style='padding-right: 0px;padding-left: 0px;' align='center'>
                  <a href='#'>
                  <img align='center' border='0' src='https://iili.io/6yhfea.png' alt='Logo' title='Logo' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 30%;' width='122.8' class='v-src-width v-src-max-width'/>
                  </a>
                </td>
              </tr>
            </table>

                  </td>
                </tr>
              </tbody>
            </table>

              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>

                </div>
              </div>
            </div>



            <div class='u-row-container' style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>

            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table id='u_content_heading_6' style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:40px 10px 6px;font-family:arial,helvetica,sans-serif;' align='left'>

              <h1 style='margin: 0px; color: #000000; line-height: 140%; text-align: center; word-wrap: break-word; font-weight: normal; font-family: tahoma,arial,helvetica,sans-serif; font-size: 32px;'>
                " + messageTitle + @"
              </h1>

                  </td>
                </tr>
              </tbody>
            </table>

            <table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:arial,helvetica,sans-serif;' align='left'>

              <div style='line-height: 140%; text-align: center; word-wrap: break-word;'>
             <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 18px; line-height: 25.2px;'><strong>Order Number : " + messageBody + @"</strong></span></p>
                <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 18px; line-height: 25.2px;'><strong>Open Portal</strong></span></p>
                <a href='" + messageURL + @"' target='_blank'>
                <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 18px; line-height: 25.2px;'><strong>" + messageURL + @"</strong></span></p>
                </a>
              </div>

                  </td>
                </tr>
              </tbody>
            </table>
              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>
                </div>
              </div>
            </div>

<div  style='padding: 0px;background-color: transparent'>
  <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #34495e;'>
    <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
<div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
  <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
  <div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
  
<table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
  <tbody>
    <tr>
      <td  style='overflow-wrap:break-word;word-break:break-word;padding:60px 10px 0px;font-family:arial,helvetica,sans-serif;' align='left'>
        
      </td>
    </tr>
  </tbody>
</table>

<table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
  <tbody>
    <tr>
      <td  style='overflow-wrap:break-word;word-break:break-word;padding:27px 10px 30px;font-family:arial,helvetica,sans-serif;' align='left'>
        
<div align='center'>
  <div style='text-align: center; display: table; max-width:155px;'>
    <table align='left' border='0' cellspacing='0' cellpadding='0' width='32' height='32' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 20px'>
      <tbody><tr style='vertical-align: top'><td align='left' valign='middle' style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
       
      </td></tr>
    </tbody></table>
    <table align='left' border='0' cellspacing='0' cellpadding='0' width='32' height='32' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 20px'>
      <tbody><tr style='vertical-align: top'><td align='left' valign='middle' style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
        
      </td></tr>
    </tbody></table>
    <table align='left' border='0' cellspacing='0' cellpadding='0' width='32' height='32' style='border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 0px'>
      <tbody><tr style='vertical-align: top'><td align='left' valign='middle' style='word-break: break-word;border-collapse: collapse !important;vertical-align: top'>
       
      </td></tr>
    </tbody></table>
  </div>
</div>

      </td>
    </tr>
  </tbody>
</table>

</div>
  </div>
</div>
    </div>
  </div>
</div>

            <div style='padding: 0px;background-color: transparent'>
              <div class='u-row' style='Margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;'>
                <div style='border-collapse: collapse;display: table;width: 100%;background-color: transparent;'>
            <div class='u-col u-col-100' style='max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;'>
              <div style='width: 100% !important;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'>
              <!--[if (!mso)&(!IE)]><!--><div style='padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;border-radius: 0px;-webkit-border-radius: 0px; -moz-border-radius: 0px;'><!--<![endif]-->

            <table style='font-family:arial,helvetica,sans-serif;' role='presentation' cellpadding='0' cellspacing='0' width='100%' border='0'>
              <tbody>
                <tr>
                  <td class='v-container-padding-padding' style='overflow-wrap:break-word;word-break:break-word;padding:15px 10px;font-family:arial,helvetica,sans-serif;' align='left'>

              <div style='color: #7e8c8d; line-height: 140%; text-align: center; word-wrap: break-word;'>
                <p style='font-size: 14px; line-height: 140%;'><span style='font-size: 14px; line-height: 19.6px; font-family: arial, helvetica, sans-serif;'>&copy;2022 PT. GS Battery | Karawang West Java - Indonesia</span></p>
              </div>

                  </td>
                </tr>
              </tbody>
            </table>

              <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->
              </div>
            </div>
                </div>
              </div>
            </div>


                </td>
              </tr>
              </tbody>
              </table>
            </body>
            ";

            DataTable dtNew = ReadExcelFile("SHEET", file_url);

            message.Attachments.Add(GetAttachment(dtNew, "SHEET", "ORDER_UPLOADED_" + messageBody + ".xlsx"));

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendEmail(): {0}",
                        ex.ToString());
            }
        }


        public static Attachment GetAttachment(DataTable dataTable, string sheetnameTarget, string targetFilename)
        {
            MemoryStream outputStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(outputStream))
            {
                ExcelWorksheet facilityWorksheet = package.Workbook.Worksheets.Add(sheetnameTarget);
                facilityWorksheet.Cells.LoadFromDataTable(dataTable, true);

                var i = dataTable.Rows.Count;

                // PROTECT COLUMN FOR NOT EDITABLE
                facilityWorksheet.Protection.IsProtected = true;
                facilityWorksheet.Column(9).Style.Locked = false;
                facilityWorksheet.Cells[1, 9].Style.Locked = true;

                #region FORMATING EXCEL
                using (var range = facilityWorksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                using (var range = facilityWorksheet.Cells[2, 12, 3, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }

                using (var range = facilityWorksheet.Cells[1, 10, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }
                #endregion


                // FORMAT COLOR FOR INPUT DISABLED
                for (var p = 1; p <= 10; p++)
                {
                    if (p != 9)
                    {
                        using (var range = facilityWorksheet.Cells[2, p, i + 1, 10])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.Silver);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            //range.Style.Font.Color.SetColor(Color.White);
                        }
                    }
                }

                // FORMAT COLOR FOR INPUT ENABLE
                using (var range = facilityWorksheet.Cells[2, 9, i + 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //range.Style.Font.Color.SetColor(Color.White);
                }


                if (i > 0)
                {
                    using (var range = facilityWorksheet.Cells[2, 1, i + 1, 10])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }

                facilityWorksheet.Cells.AutoFitColumns(0);

                package.Save();
            }

            outputStream.Position = 0;
            Attachment attachment = new Attachment(outputStream, targetFilename, "application/vnd.ms-excel");

            return attachment;
        }

        private static DataTable ReadExcelFile(string sheetName, string path)
        {

            using (OleDbConnection conn = new OleDbConnection())
            {
                DataTable dt = new DataTable();
                string Import_FileName = path;
                string fileExtension = Path.GetExtension(Import_FileName);
                if (fileExtension == ".xls")
                    conn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Import_FileName + ";" + "Extended Properties='Excel 8.0;HDR=YES;'";
                if (fileExtension == ".xlsx")
                    conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Import_FileName + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                using (OleDbCommand comm = new OleDbCommand())
                {
                    comm.CommandText = "Select * from [" + sheetName + "$]";
                    comm.Connection = conn;
                    using (OleDbDataAdapter da = new OleDbDataAdapter())
                    {
                        da.SelectCommand = comm;
                        da.Fill(dt);
                        conn.Close();
                        return dt;
                    }
                }
            }
        }
    }
}