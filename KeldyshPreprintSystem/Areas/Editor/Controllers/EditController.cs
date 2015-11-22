using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using NLog.Config;
using System.Net;
using KeldyshPreprintSystem.Models;
using System.IO;
using WebMatrix.WebData;
namespace KeldyshPreprintSystem.Areas.Editor.Controllers
{
    public class EditController : Controller
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //
        // GET: /Editor/Editor/

        public ActionResult Index()
        {
            try
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    List<PaperSubmissionModel> backwards = db.PaperSubmissions.ToList();
                    backwards.Reverse();
                    return View(backwards);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return View();
        }


        public ActionResult Submission(int id = 0)
        {
            if (id > 0)
            {
                try
                {
                    using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                    {
                        PaperSubmissionModel submission = db.PaperSubmissions.First(x => x.Id == id);
                        if (Tools.AccountHelper.GetUserRole(WebSecurity.CurrentUserId) == "user")
                            WebSecurity.RequireUser(submission.Owner);
                        if (submission.Authors.Count == 0)
                            foreach (Author a in db.Authors.ToList())
                            {
                                if (a.PaperSubmissionModelId == id)
                                    submission.Authors.Add(a);
                            }
                        if (submission.Reviewer == null)
                            submission.Reviewer = db.Reviewers.FirstOrDefault(x => x.PaperSubmissionModelId == id);
                        //submission.Attachment = new PreprintAttachment(Server.MapPath("~/App_Data/uploads/" + id + "/latest.pdf"));
                        //.Server.MapPath("~/App_Data/uploads/" + paper.Id)
                        return View(submission);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return RedirectToAction("Index", "Edit");
                }
            }
            else
                return RedirectToAction("Index", "Edit");
        }

        [HttpPost]
        public ActionResult Submission(PaperSubmissionModel paper)
        {
            try
            {
                //paper.Attachment = new PreprintAttachment(Server.MapPath("~/App_Data/uploads/" + paper.Id + "/latest.pdf"));
                //if (ModelState.Values.Count == 1 && ModelState.IsValidField("Attachment"))
                if (paper == null)
                    throw new ArgumentNullException("paper");
                if (paper.Attachment == null)
                    ModelState.Remove("Attachment");
                // TODO: Add insert logic here
                if (!ModelState.IsValid)
                {
                    return View("Submission", paper);
                }

                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    PaperSubmissionModel submission = db.PaperSubmissions.Find(paper.Id);
                    foreach (Author a in submission.Authors.ToList())
                    {
                        db.Entry(a).State = System.Data.Entity.EntityState.Deleted;
                    }
                    //dealing with attachments
                    if (paper.Attachment != null)
                    {
                        Tools.UploadsManager.SaveFile(paper.Id, paper.Attachment);
                        submission.Attachment = paper.Attachment;
                    }
                    else
                    {
                        submission.Attachment = new FakePreprintAttachment("bogus.pdf");
                    }

                    submission.Reviewer = db.Reviewers.Find(paper.Reviewer.Id);
                    db.SaveChanges();
                }

                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    PaperSubmissionModel submission = db.PaperSubmissions.Find(paper.Id);
                    Reviewer reviewer = db.Reviewers.Find(submission.Reviewer.Id);
                    reviewer.PaperSubmissionModelId = submission.Id;
                    reviewer.PaperSubmissionModel = submission;
                    db.Entry(reviewer).State = System.Data.Entity.EntityState.Modified;
                    submission.Reviewer = reviewer;
                    submission.Authors = paper.Authors;
                    foreach (Author a in submission.Authors)
                    {
                        a.PaperSubmissionModelId = submission.Id;
                        a.PaperSubmissionModel = submission;
                        db.Authors.Add(a);
                    }

                    submission.TitleEnglish = paper.TitleEnglish;
                    submission.TitleRussian = paper.TitleRussian;
                    submission.AbstractEnglish = paper.AbstractEnglish;
                    submission.AbstractRussian = paper.AbstractRussian;
                    submission.Bibliography = paper.Bibliography;
                    submission.ContactName = paper.ContactName;
                    submission.ContactPhone = paper.ContactPhone;
                    submission.FieldOfResearch = paper.FieldOfResearch;
                    submission.FinancialSupport = paper.FinancialSupport;
                    submission.KeywordsEnglish = paper.KeywordsEnglish;
                    submission.KeywordsRussian = paper.KeywordsRussian;
                    submission.Languages = paper.Languages;
                    submission.NumberOfAuthorsCopies = paper.NumberOfAuthorsCopies;
                    submission.NumberOfPages = paper.NumberOfPages;
                    submission.Review = paper.Review;
                    submission.UDK = paper.UDK;
                    if (paper.Attachment != null)
                        submission.Attachment = paper.Attachment;
                    else
                        submission.Attachment = new FakePreprintAttachment("bogus.pdf");
                    db.Entry(submission).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["IsDataSaved"] = "True";
                    if (submission.submissionState > 0)
                        Tools.PanelHelper.LogMessage("Заявка отредактирована.", paper.Id, 0, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
                    //updating submission files
                    //making 2 conversions at a time might be not good
                    //Tools.PaperSubmissionControllerHelper.CreateOrGetExistingDocument(paper, false, true);
                    //Tools.PaperSubmissionControllerHelper.CreateOrGetExistingDocument(paper, true, true);
                    //instead just deleting necessary files
                    string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/" + paper.Id + "/");
                    if (System.IO.File.Exists(path + "submission.docx"))
                        System.IO.File.Delete(path + "submission.docx");
                    if (System.IO.File.Exists(path + "submission_for_corrector.docx"))
                        System.IO.File.Delete(path + "submission_for_corrector.docx");
                    return View("Submission", paper);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                TempData["IsDataSaved"] = "False";
                return View("Submission", paper);
                //return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }


        public ActionResult Delete(int paperId)
        {
            WebMatrix.WebData.WebSecurity.RequireRoles(new string[] { "admin" });
            try
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    PaperSubmissionModel submission = db.PaperSubmissions.First(x => x.Id == paperId);
                    var authorsToRemove = submission.Authors.ToList();
                    foreach (var a in authorsToRemove)
                        db.Entry(a).State = System.Data.Entity.EntityState.Deleted;
                    submission.Authors.Clear();
                    db.Entry(submission).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                Tools.UploadsManager.DeleteFiles(paperId);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return RedirectToAction("Index");
        }

        public ActionResult DownloadFile(int paperId, int versionId)
        {
            try
            {
                var paperVersion = Tools.UploadsManager.GetPaperVersion(paperId, versionId);
                return File(paperVersion.GetBytes(), paperVersion.MIME, paperVersion.DownloadName);
            }
            catch (Exception ex)
            {
                logger.Error("DownloadFile Error downloading file " + ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes("Ошибка загрузки файла " + ex.Message), "text");
            }
        }
    }
}
