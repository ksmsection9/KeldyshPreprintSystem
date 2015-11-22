using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeldyshPreprintSystem.Models;
using System.IO;
using Novacode;
using NLog;
using NLog.Config;
using System.Net;
using System.Web.Script.Serialization;

namespace KeldyshPreprintSystem.Controllers
{
    public class PaperSubmissionController : Controller
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //
        // GET: /PaperSubmission/

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
            //return View(db.PaperSubmissions.ToList());
        }

        //
        // GET: /PaperSubmission/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //
        // GET: /PaperSubmission/Create

        public ActionResult Create(string guid = null)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return View();
            }
            else
            {
                string subId;
                PaperSubmissionModel paper = Tools.PaperSubmissionControllerHelper.GetTemporarySubmission(guid, WebMatrix.WebData.WebSecurity.CurrentUserName, out subId);
                TempData["UniqueSubmissionId"] = subId;
                if (paper.Attachment != null)
                {
                    ViewBag.UploadedInfo = paper.Attachment.ContentLength;
                }
                return View("Create", paper);
            }
        }

        //
        // POST: /PaperSubmission/Create
        [HttpPost]
        public ActionResult Create(PaperSubmissionModel paper)
        {
            try
            {
                // TODO: Add insert logic here
                paper.submissionDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
                paper.lastStatusChangeDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
                paper.Owner = WebMatrix.WebData.WebSecurity.CurrentUserName;
                //file upload logic below
                string subId = (string)TempData["UniqueSubmissionId"];
                logger.Info("subid: " + subId);
                if (subId == null)
                {
                    subId = Guid.NewGuid().ToString();
                    logger.Info("subid is null. new subid: " + subId);
                }

                TempData["UniqueSubmissionId"] = subId;//kepeing in tempdata
                paper.Attachment = Tools.UploadsManager.GetUpload(subId);
                logger.Info("IsAttachmentNull:" + (paper.Attachment == null));

                if (paper.Attachment != null)
                {
                    ModelState.Remove("Attachment");
                    ViewBag.UploadedInfo = paper.Attachment.ContentLength;
                }

                if (!ModelState.IsValid)
                {
                    return View("Create", paper);
                }

                paper.submissionState = 0;//0-submission just added
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    db.PaperSubmissions.Add(paper);
                    db.Authors.AddRange(paper.Authors);
                    db.SaveChanges();
                }
                Tools.UploadsManager.SaveFile(paper.Id, subId);
                return RedirectToAction("Success", new { id = paper.Id });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                // return View();
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        public ActionResult SaveTemporarySubmission(PaperSubmissionModel paper)
        {
            //try
            //{
            paper.submissionDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
            paper.lastStatusChangeDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
            paper.Owner = WebMatrix.WebData.WebSecurity.CurrentUserName;
            string content = Tools.PaperSubmissionControllerHelper.SerializeModel(paper);
            string authors = "";
            if (paper.Authors != null)
                authors = Tools.PaperSubmissionControllerHelper.SerializeAuthors(paper.Authors.ToList());

            string subId = (string)TempData["UniqueSubmissionId"];
            logger.Info("subid: " + subId);
            if (subId != null)
            {
                TempData["UniqueSubmissionId"] = subId;//kepeing in tempdata
                paper.Attachment = Tools.UploadsManager.GetUpload(subId);
                logger.Info("IsAttachmentNull:" + (paper.Attachment == null));
                //Tools.UploadsManager.SaveFile(paper.Id, subId);
            }

            Tools.PaperSubmissionControllerHelper.SaveTemporarySubmission(content, authors, subId, WebMatrix.WebData.WebSecurity.CurrentUserName);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.Message);
            //    // return View();
            //    return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            //}
        }

        //get
        public ActionResult Success(int id)
        {
            try
            {
                int[] legitStates = { 0 };
                ViewBag.paperId = id;
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    var paper = db.PaperSubmissions.First(x => x.Id == id);
                    if (!legitStates.Contains(paper.submissionState))
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                    if (paper.submissionState == 0)
                    {
                        paper.lastStatusChangeDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
                        paper.submissionState = 1;
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
                        Tools.PanelHelper.CreateContext(paper);//creating context for panel

                        foreach (Author author in paper.Authors)
                            Tools.MailSender.SendMail(Tools.ConfigurationResourcesHelper.GetResource("messages", "EmailSubmissionNotificationTitle"), Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "EmailSubmissionNotification", author), Tools.ConfigurationResourcesHelper.GetResource("options", "EmailNotificationAddress"), author.Email);
                        Tools.SMSSender.SendSms(Tools.ConfigurationResourcesHelper.GetResourceVal("messages", "SMSSubmissionNotification", paper), "7" + paper.ContactPhone);
                        //clean previous scheduled mails
                        Tools.ScheduleHelper.CleanRecurringJobs(id);
                        //new sendings
                        Tools.PanelHelper.SendSmsForCurrentState(paper, 1);
                        Tools.PanelHelper.SendMailForCurrentState(paper, 1, "");
                        Tools.PanelHelper.LogMessage(Tools.EditorHelper.GetStatusDescription(1) + " (статус: 1)", id, 0, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                // return View();
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        public ActionResult Download(int id, string type = "docx", bool forcorrector = false)
        {
            try
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    var document = Tools.PaperSubmissionControllerHelper.CreateOrGetExistingDocument(db.PaperSubmissions.First(x => x.Id == id), forcorrector, false);
                    if (type == "docx")
                    {
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            // for example foo.bak
                            FileName = forcorrector ? "request_corrector.docx" : "request.docx",
                            // always prompt the user for downloading, set to true if you want 
                            // the browser to try to show the file inline
                            Inline = false,
                        };
                        Response.AppendHeader("Content-Disposition", cd.ToString());
                        return File(System.IO.File.ReadAllBytes(document), "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    }
                    else
                        if (type == "pdf")
                        {
                            string pdfFile = Tools.PaperSubmissionControllerHelper.ConvertDocxToPDF(document);
                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                // for example foo.bak
                                FileName = "request.pdf",
                                // always prompt the user for downloading, set to true if you want 
                                // the browser to try to show the file inline
                                Inline = false,
                            };
                            Response.AppendHeader("Content-Disposition", cd.ToString());
                            return File(pdfFile, "application/pdf");
                        }
                        else
                            return File(System.Text.ASCIIEncoding.Default.GetBytes("Указан неизвестный формат выходного файла."), "text");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }

        [AllowAnonymous()]
        public ActionResult GetFrontCover(int id, int libraryId = 999)
        {
            try
            {
                int[] args = { id, libraryId };
                return View(args);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
                //RedirectToAction("Index", db.PaperSubmissions.ToList());
            }
        }


        //private DocX ConvertHtmlToDocx(string html)
        //{
        //    string api = "1DpJ520e5RreEwodXBSd5AjxdnPCLf3Yw9pqM7k7X485EUBYup2rAdyp1ImUM5CuBLKLrX8_GO1KkfUjG5C98g";
        //    //try
        //    //{
        //        using (WebClient client = new WebClient())
        //        {
        //            string filepath = HttpContext.Server.MapPath("~/App_Data/"+Guid.NewGuid().ToString()+".htm");
        //            System.IO.File.WriteAllText(filepath, html);
        //            byte[] response = client.UploadFile(string.Format("https://api.cloudconvert.org/convert?apikey={0}&input=upload&delete=true&inputformat=htm&outputformat=docx", api), filepath );
        //            //ProcessURL url = (new JavaScriptSerializer().Deserialize<ProcessURL>(System.Text.Encoding.UTF8.GetString(response)));
        //            MemoryStream stream = new MemoryStream(response);//client.DownloadData(url.url));
        //            DocX result = DocX.Create(stream);
        //            stream.Close();
        //            System.IO.File.Delete(filepath);//cleaningup
        //            return result;
        //       }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    logger.Error(ex.Message);
        //    //    throw;
        //    //}
        //}

    }
}
