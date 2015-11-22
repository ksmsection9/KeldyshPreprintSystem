using KeldyshPreprintSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace KeldyshPreprintSystem.Areas.Admin.Controllers
{
    public class PanelController : Controller
    {
        //
        // GET: /Admin/Panel/
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Object locker = new object();

        public ActionResult Index()
        {
            try
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    List<PaperSubmissionModel> backwards = db.PaperSubmissions.ToList();
                    foreach (var paper in backwards)
                    {
                        if (paper.Authors.Count == 0)
                            foreach (Author a in db.Authors.ToList())
                            {
                                if (a.PaperSubmissionModelId == paper.Id)
                                    paper.Authors.Add(a);
                            }
                        if (paper.Reviewer == null)
                            paper.Reviewer = db.Reviewers.FirstOrDefault(x => x.PaperSubmissionModelId == paper.Id);
                    }
                    backwards.Reverse();
                    return View(backwards);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        public ActionResult Manage(int id)
        {
            try
            {

                PanelContextModel context = Tools.PanelHelper.GetContext(id);
                //>5 meats that user account is not default
                if (WebMatrix.WebData.WebSecurity.CurrentUserId>5 && Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId) == "user" && Tools.PaperSubmissionControllerHelper.GetModel(context.paperId).Owner != WebMatrix.WebData.WebSecurity.CurrentUserName)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                return View(context);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        public ActionResult Log(string text, int paperId, int subLogId, int lastLogId, string emailToRole = "noemail")
        {
            try
            {
                //logger.Info("lastLogLogId: " + lastLogId);
                //var result = File(System.Text.UnicodeEncoding.Default.GetBytes(Tools.PanelHelper.GetLogAfterId(paperId, lastLogId)), "text");
                //DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                //d = d.AddMilliseconds(Convert.ToInt64(lastUpdateTime));
                //DateTime lastUpdate = d.ToLocalTime();

                //mail logic
                switch (emailToRole)
                {
                    case "toEditor":
                        subLogId = 0;
                        Tools.MailSender.SendMail("Новое сообщение", "Новое сообщение от " + Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)) + ".\r\n" + text, Tools.ConfigurationResourcesHelper.GetResource("mailinglist", "EditorMail"));
                        break;
                    case "toCorrector":
                        subLogId = 0;
                        Tools.MailSender.SendMail("Новое сообщение", "Новое сообщение от " + Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)) + ".\r\n" + text, Tools.ConfigurationResourcesHelper.GetResource("mailinglist", "CorrectorMail"));
                        break;
                    case "toUser":
                        subLogId = 0;
                        Tools.MailSender.SendMail("Новое сообщение", "Новое сообщение от " + Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)) + ".\r\n" + text, WebMatrix.WebData.WebSecurity.CurrentUserName);
                        break;
                    default:
                    case "noemail":
                        break;
                }
                LogEntry entry = new LogEntry();
                entry.Message = text;
                entry.subLogId = subLogId;
                entry.TimeStamp = DateTime.Now;
                entry.UserName = WebMatrix.WebData.WebSecurity.CurrentUserName;
                entry.UserRole = Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId);
                Tools.PanelHelper.LogMessage(paperId, entry);
                //Tools.PanelHelper.LogMessage(text, paperId, subLogId, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                return new HttpStatusCodeResult(HttpStatusCode.OK);
                //return Json(new { lastId = Tools.PanelHelper.GetLastId(paperId, subLogId), content = Tools.PanelHelper.GetWholeSubLogAfterId(paperId, subLogId, lastLogId) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }


        public ActionResult ToggleSms(int paperId)
        {
            try
            {
                Tools.PanelHelper.ToggleSmsForbiddanceContext(paperId);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        public FileContentResult GetFrontCoverPdf(int paperId, int libraryId = 999)
        {
            try
            {
                string path = Tools.PaperSubmissionControllerHelper.GenerateFrontCoverPdfSyncIfNotExists(paperId, libraryId);
                return File(System.IO.File.ReadAllBytes(path), "application/pdf");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        //public ActionResult GetLog(int paperId, int subLogId, int lastLogId)
        //{
        //    try
        //    {
        //        //logger.Info("lastLogLogId: " + lastLogId);
        //        //DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //        //d = d.AddMilliseconds(Convert.ToInt64(lastUpdateTime));
        //        //DateTime lastUpdate = d.ToLocalTime();
        //        //return Json(System.Text.UnicodeEncoding.Default.GetBytes(Tools.PanelHelper.GetLogAfterId(paperId, lastLogId)), "text");
        //        string contentstring = Tools.PanelHelper.GetWholeSubLogAfterId(paperId, subLogId, lastLogId);
        //        //logger.Info("content: " + contentstring);
        //        return Json(new { lastId = Tools.PanelHelper.GetLastId(paperId, subLogId).ToString(), content = contentstring }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.Message);
        //        return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
        //    }
        //}



        private void changeState(int id, int state)
        {
            lock (locker)
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    var paper = db.PaperSubmissions.First(x => x.Id == id);
                    paper.submissionState = state;
                    paper.lastStatusChangeDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
                    if (paper.Authors.Count == 0)
                        foreach (Author a in db.Authors.ToList())
                        {
                            if (a.PaperSubmissionModelId == id)
                                paper.Authors.Add(a);
                        }
                    paper.Attachment = new FakePreprintAttachment("bogus.pdf");
                    if (paper.Reviewer == null)
                        paper.Reviewer = db.Reviewers.FirstOrDefault(x => x.PaperSubmissionModelId == id);
                    db.Entry(paper).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        private bool checkRole(int state)
        {
            string[] possibleActions = Tools.EditorHelper.GetPossibleActions(state);
            string role = Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId);
            if (role == "admin")
                return true;
            foreach (string s in possibleActions)
            {
                string validRole = s.Split(':')[0];
                if (validRole == role)
                    return true;
            }
            return false;
        }

        //private void changeState(PaperSubmissionModel paper, int state)
        //{
        //    lock (locker)
        //    {
        //        using (PaperSubmissionsContext db = new PaperSubmissionsContext())
        //        {
        //            paper.submissionState = state;
        //            if (paper.Authors.Count == 0)
        //                foreach (Author a in db.Authors.ToList())
        //                {
        //                    if (a.PaperSubmissionModelId == paper.Id)
        //                        paper.Authors.Add(a);
        //                }
        //            paper.Attachment = new FakePreprintAttachment("bogus.pdf");
        //            if (paper.Reviewer == null)
        //                paper.Reviewer = db.Reviewers.FirstOrDefault(x => x.PaperSubmissionModelId == paper.Id);
        //            db.Entry(paper).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //        }
        //    }
        //}


        //Editing Proccess
        [HttpGet]
        public virtual ActionResult LoadStatusInformationDynamically(int id)
        {
            try
            {
                return PartialView("_statusInformationPartial", Tools.PaperSubmissionControllerHelper.GetModel(id));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        [HttpGet]
        public virtual ActionResult LoadLogOutputDynamically(int id)
        {
            try
            {
                return PartialView("_logOutputPartial", Tools.PanelHelper.GetContext(id));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        [HttpGet]
        public virtual ActionResult LoadPrimaryActionsDynamically(int id)
        {
            try
            {
                return PartialView("_primaryActionMenuPartial", Tools.PaperSubmissionControllerHelper.GetModel(id));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        [HttpGet]
        public virtual ActionResult LoadFileVersionDynamically(int id)
        {
            try
            {
                return PartialView("_actualVersionPartial", Tools.PaperSubmissionControllerHelper.GetModel(id));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ResetStatus(int id)
        {
            WebMatrix.WebData.WebSecurity.RequireRoles(new string[] { "admin" });
            changeState(id, 1);
            Tools.ScheduleHelper.CleanRecurringJobs(id);
            Tools.PanelHelper.LogMessage("Статус сброшен. Текущий статус: " + Tools.EditorHelper.GetStatusDescription(1) + " (статус: 1)", id, 0, "Система");
            return new RedirectResult("/Panel/Manage/" + id);
        }

        public ActionResult ClarifyContent(int id, string message)//2
        {
            try
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                int[] legitStates = { 1, 3 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 2);
                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(2) + " (статус: 2)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);
                //new sendings
                Tools.PanelHelper.SendSmsForCurrentState(paper, 2);
                //Tools.SMSSender.SendSms(Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "SmsSecondState", paper), "7" + paper.ContactPhone);
                //foreach (Author author in paper.Authors)
                //    Tools.DailySendEmailJob.AddMessage(author.Email, Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubjectSecondState", paper), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailBodySecondState", paper).Replace("@editortext@", message), new List<string>(), 24, id, 2);
                Tools.PanelHelper.SendMailForCurrentState(paper, 2, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ContentClarified(int id, string message)//3
        {
            try
            {
                int[] legitStates = { 2 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 3);
                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(3) + " (статус: 3)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);
                //new senginds
                //editorial
                //*********************************************************
                //List<string[]> editorial = Tools.ConfigurationResourcesHelper.GetEditorialMail();
                //byte[] submission = Tools.PaperSubmissionControllerHelper.CreateDocument(paper, false);
                //string path = HttpContext.Server.MapPath("~/App_Data/converted/" + Guid.NewGuid().ToString() + ".docx");
                //System.IO.File.WriteAllBytes(path, submission);
                //List<string> attachments = new List<string> { Tools.UploadsManager.Latest(paper.Id).Path, path };
                //foreach (string[] s in editorial)
                //{
                //    KeldyshPreprintSystem.Tools.MailInfo mail = new Tools.MailInfo(Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubjectEditorialBordNotification", paper, s), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailBodyEditorialBordNotification", paper, s), attachments, s[0], 0, id, 3, "notuniq");
                //    Tools.MailSender.SendMail(mail);
                //}
                //****************************************************
                //
                Tools.PanelHelper.SendSmsForCurrentState(paper, 3);
                //Tools.DailySendEmailJob.AddMessage(Tools.ConfigurationResourcesHelper.GetResourceVal("mailinglist", "EditorMail"), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubjectThirdState", paper), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailBodyThirdState", paper).Replace("@authortext@", message), attachments, 24, paper.Id, 3);
                Tools.PanelHelper.SendMailForCurrentState(paper, 3, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ContentPassedToCorrector(int id, string message)//4
        {
            try
            {
                int[] legitStates = { 1, 3, 10 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                //if (paper.submissionState == 1 || paper.submissionState == 3)
                //{
                //    List<string> attachments = new List<string>();
                //    attachments.Add(Tools.UploadsManager.Latest(id).Path);
                //    Tools.MailSender.SendMailOnMailingList(Tools.ConfigurationResourcesHelper.GetEditorialMail(), "Заяка на публикацию препринта \"" + paper.TitleRussian + "\"", "Поступила новая заявка на публикацию. Пожалуйста, ознакомьтесь с препринтом", attachments);
                //}
                changeState(id, 4);
                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(4) + " (статус: 4)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);
                //new sendings
                Tools.PanelHelper.SendSmsForCurrentState(paper, 4);
                //Tools.SMSSender.SendSms(Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "SmsForthState", paper), "7" + Tools.ConfigurationResourcesHelper.GetResourceVal("mailinglist", "CorrectorPhone"));//смс корректору
                //byte[] submission = Tools.PaperSubmissionControllerHelper.CreateDocument(paper, true);
                //string path = HttpContext.Server.MapPath("~/App_Data/converted/" + Guid.NewGuid().ToString() + ".docx");
                //System.IO.File.WriteAllBytes(path, submission);
                //List<string> attachments = new List<string> { Tools.UploadsManager.Latest(paper.Id).Path, path };
                //Tools.DailySendEmailJob.AddMessage(Tools.ConfigurationResourcesHelper.GetResourceVal("mailinglist", "CorrectorMail"), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubjectForthState", paper), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailBodyForthState", paper).Replace("@editortext@", message), attachments, 24, paper.Id, 4);
                Tools.PanelHelper.SendMailForCurrentState(paper, 4, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ContentCorrectionOver(int id, string message)//5
        {
            try
            {
                int[] legitStates = { 4 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 5);
                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(5) + " (статус: 5)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);
                //new sendings
                //Tools.SMSSender.SendSms(Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "SmsFifthState", paper), "7" + paper.ContactPhone);//смс автору
                Tools.PanelHelper.SendSmsForCurrentState(paper, 5);
                //byte[] submission = Tools.PaperSubmissionControllerHelper.CreateDocument(paper, true);
                //string path = HttpContext.Server.MapPath("~/App_Data/converted/" + Guid.NewGuid().ToString() + ".docx");
                //System.IO.File.WriteAllBytes(path, submission);
                //List<string> attachments = new List<string> { Tools.UploadsManager.Latest(paper.Id).Path, path };
                //foreach (var author in paper.Authors)
                //    Tools.DailySendEmailJob.AddMessage(author.Email, Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubjectFifthState", paper), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailBodyFifthState", paper).Replace("@correctortext@", message), attachments, 24, paper.Id, 5);
                Tools.PanelHelper.SendMailForCurrentState(paper, 5, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ContentCorrectionPassed(int id, string message = "")//6
        {
            try
            {
                int[] legitStates = { 5 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 6);
                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(6) + " (статус: 6)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);
                //new sendings
                Tools.PanelHelper.SendSmsForCurrentState(paper, 6);
                //Tools.SMSSender.SendSms(Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "SmsFifthState", paper), "7" + Tools.ConfigurationResourcesHelper.GetResourceVal("mailinglist", "EditorPhone"));//смс редактору
                //byte[] submission = Tools.PaperSubmissionControllerHelper.CreateDocument(paper, true);
                //string path = HttpContext.Server.MapPath("~/App_Data/converted/" + Guid.NewGuid().ToString() + ".docx");
                //System.IO.File.WriteAllBytes(path, submission);
                //List<string> attachments = new List<string> { Tools.UploadsManager.Latest(paper.Id).Path, path };
                //Tools.DailySendEmailJob.AddMessage(Tools.ConfigurationResourcesHelper.GetResourceVal("mailinglist", "EditorMail"), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubjectFifthState", paper), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailBodyFifthState", paper), attachments, 24, paper.Id, 6);
                Tools.PanelHelper.SendMailForCurrentState(paper, 6, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult MarkupAuthorFinishing(int id, string message = "")//7
        {
            try
            {
                int[] legitStates = { 1, 3, 6, 8 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                //if (paper.submissionState == 1 || paper.submissionState == 3)
                //{
                //    List<string> attachments = new List<string>();
                //    attachments.Add(Tools.UploadsManager.Latest(id).Path);
                //    Tools.MailSender.SendMailOnMailingList(Tools.ConfigurationResourcesHelper.GetEditorialMail(), "Заяка на публикацию препринта \"" + paper.TitleRussian + "\"", "Поступила новая заявка на публикацию. Пожалуйста, ознакомьтесь с препринтом", attachments);
                //}
                changeState(id, 7);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(7) + " (статус: 7)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 7);
                Tools.PanelHelper.SendMailForCurrentState(paper, 7, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult MarkupFinishingOver(int id, string message)//8
        {
            try
            {
                int[] legitStates = { 7 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 8);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(8) + " (статус: 8)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 8);
                Tools.PanelHelper.SendMailForCurrentState(paper, 8, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult SignalPermitted(int id, string message = "")//9
        {
            try
            {
                int[] legitStates = { 1, 3, 6, 8, 10 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                //if (paper.submissionState == 1 || paper.submissionState == 3)
                //{
                //    List<string> attachments = new List<string>();
                //    attachments.Add(Tools.UploadsManager.Latest(id).Path);
                //    Tools.MailSender.SendMailOnMailingList(Tools.ConfigurationResourcesHelper.GetEditorialMail(), "Заяка на публикацию препринта \"" + paper.TitleRussian + "\"", "Поступила новая заявка на публикацию. Пожалуйста, ознакомьтесь с препринтом", attachments);
                //}
                changeState(id, 9);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(9) + " (статус: 9)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 9);
                Tools.PanelHelper.SendMailForCurrentState(paper, 9, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult SignalReady(int id, string message = "")//10
        {
            try
            {
                int[] legitStates = { 9 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 10);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(10) + " (статус: 10)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 10);
                Tools.PanelHelper.SendMailForCurrentState(paper, 10, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult SignalCorrected(int id, string message = "")//11
        {
            try
            {
                int[] legitStates = { 10 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 11);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(11) + " (статус: 11)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 11);
                Tools.PanelHelper.SendMailForCurrentState(paper, 11, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult PrintingPermitted(int id, string message = "")//12
        {
            try
            {
                int[] legitStates = { 10 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 12);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(12) + " (статус: 12)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 12);
                Tools.PanelHelper.SendMailForCurrentState(paper, 12, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }


        public ActionResult PrintingProcessing(int id, string message = "")//13
        {
            try
            {
                int[] legitStates = { 10, 12 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 13);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(13) + " (статус: 13)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 13);
                Tools.PanelHelper.SendMailForCurrentState(paper, 13, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult WebSiteReady(int id, string message = "")//14
        {
            try
            {
                int[] legitStates = { 13 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 14);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(14) + " (статус: 14)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 14);
                Tools.PanelHelper.SendMailForCurrentState(paper, 14, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ELibraryReady(int id, string message)//15
        {
            try
            {
                int[] legitStates = { 14 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 15);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(15) + " (статус: 15)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 15);
                Tools.PanelHelper.SendMailForCurrentState(paper, 15, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ExtraTasks(int id, string message = "")//16
        {
            try
            {
                int[] legitStates = { 15, 18, 20 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 16);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(16) + " (статус: 16)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 16);
                Tools.PanelHelper.SendMailForCurrentState(paper, 16, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult WebSiteReplacement(int id, string message = "")//17
        {
            try
            {
                int[] legitStates = { 16 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 17);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(17) + " (статус: 17)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 17);
                Tools.PanelHelper.SendMailForCurrentState(paper, 17, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult WebSiteReplacementPermitted(int id, string message = "")//18
        {
            try
            {
                int[] legitStates = { 17 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 18);//because it is a state for extra tasks
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(18) + " (статус: 18)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 18);
                Tools.PanelHelper.SendMailForCurrentState(paper, 18, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }


        public ActionResult WebSiteReplacementDone(int id, string message = "")//19
        {
            try
            {
                int[] legitStates = { 18 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 16);//because it is a state for extra tasks
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(16) + " (возврат на статус: 16)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 19);
                Tools.PanelHelper.SendMailForCurrentState(paper, 19, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ExtraPrinting(int id, string message = "")//20
        {
            try
            {
                int[] legitStates = { 16 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 20);
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(20) + " (статус: 20)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 20);
                Tools.PanelHelper.SendMailForCurrentState(paper, 20, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ExtraPrintingPermitted(int id, string message = "")//21
        {
            try
            {
                int[] legitStates = { 20 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 21);
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(21) + " (статус: 21)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 21);
                Tools.PanelHelper.SendMailForCurrentState(paper, 21, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        public ActionResult ExtraPrintingDone(int id, string message = "")//22
        {
            try
            {
                int[] legitStates = { 21 };
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetModel(id);
                if (!legitStates.Contains(paper.submissionState) || !checkRole(paper.submissionState))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                changeState(id, 16);//because it is a state for extra tasks
                //clean previous scheduled mails
                Tools.ScheduleHelper.CleanRecurringJobs(id);

                Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(16) + " (возврат на статус: 16)", id, 1, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                Tools.PanelHelper.SendSmsForCurrentState(paper, 22);
                Tools.PanelHelper.SendMailForCurrentState(paper, 22, message);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

    }
}
