using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace KeldyshPreprintSystem.Areas.Admin.Controllers
{
    public class SettingsController : Controller
    {
        //
        // GET: /Admin/Settings/
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            WebSecurity.RequireRoles(new string[] { "admin" });
            return View(Tools.ConfigurationResourcesHelper.Resources);
        }

        public ActionResult SetOption(string area, string key, string value)
        {
            WebSecurity.RequireRoles(new string[] { "admin" });
            string result = Tools.ConfigurationResourcesHelper.SetResourceVal(area, key, value);
            return File(System.Text.ASCIIEncoding.Default.GetBytes(result), "text");
        }

        public ActionResult ChangeLoginPassword(string username, string password)
        {
            WebSecurity.RequireRoles(new string[] { "admin" });
            if (Tools.AccountHelper.SetNewAccountCredentials(username, password))
                return new HttpStatusCodeResult(200);
            else
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.InternalServerError);
        }

        public ActionResult ChangeUserRole()
        {
            return View();
        }

        public ActionResult SetUserRole(string username, string role)
        {
            try
            {
                Tools.AccountHelper.SetUserRole(username, role);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return new HttpStatusCodeResult(500);
            }
        }

    
        public ActionResult SaveStatusMailTemplate(int stateId, string subject, string body, string roleMail, int interval, bool attachPaper, bool attachSubmission, string sms)
        {
            try
            {
                List<string> attachments = new List<string>();
                if (attachPaper)
                    attachments.Add("ReplaceForPathToPaper");
                if (attachSubmission)
                    attachments.Add("ReplaceForPathToSubmission");
                subject = subject.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                body = body.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                sms = sms.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                Tools.MailInfo mail = new Tools.MailInfo(subject, body, attachments, roleMail, interval, -1, stateId, "StatusMailTemplate");
                string serialized = Tools.MailInfo.SerializeVersionToXmlString(mail);
                if (System.IO.Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId)))
                {
                    System.IO.Directory.Delete(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId), true);
                }
                System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId));
                System.IO.File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId + "/" + Guid.NewGuid().ToString()), serialized);

                Tools.SmsInfo smsMessage = new Tools.SmsInfo(sms, roleMail, interval, -1, stateId, "StatusSmsTemplate");
                string serializedSms = Tools.SmsInfo.SerializeVersionToXmlString(smsMessage);
                if (System.IO.Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId)))
                {
                    System.IO.Directory.Delete(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId), true);
                }
                System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId));
                System.IO.File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId + "/" + Guid.NewGuid().ToString()), serializedSms);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return new HttpStatusCodeResult(500);
            }
        }
    }
}
