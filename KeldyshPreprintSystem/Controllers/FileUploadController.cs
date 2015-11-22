using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KeldyshPreprintSystem.Controllers
{
    public class FileUploadController : Controller
    {
        //
        // GET: /FileUpload/
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult DeleteUploadedFile()
        {
            try
            {
                logger.Info("Removing upload...");
                string guid = (string)TempData["UniqueSubmissionId"];
                logger.Info("guid: " + guid);
                bool result = Tools.UploadsManager.RemoveUpload(guid);
                logger.Info("Upload removed: " + result + " for guid " + guid);
                TempData["UniqueSubmissionId"] = guid;
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                // return View();
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        public ActionResult SaveUploadedFile()
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                logger.Info("Saving file...");
                string guid = (string)TempData["UniqueSubmissionId"];
                logger.Info("guid: " + guid);
                if (guid == null)
                {
                    guid = Guid.NewGuid().ToString();
                    logger.Info("guid is null. new guid: " + guid);
                }
                TempData["UniqueSubmissionId"] = guid;

                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    Tools.UploadsManager.RemoveUpload(guid);//delete previous
                    bool response = Tools.UploadsManager.UploadFile(guid, file);
                    logger.Info("guid: " + guid + " response: " + response);
                }
            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
                logger.Error(ex.Message);
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName });
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
            }
        }

        public ActionResult UpdatePreprintVersion(int paperId)
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                logger.Info("Saving file...");
                string guid = (string)TempData["UniqueSubmissionId"];
                logger.Info("guid: " + guid);
                if (guid == null)
                {
                    guid = Guid.NewGuid().ToString();
                    logger.Info("guid is null. new guid: " + guid);
                }
                TempData["UniqueSubmissionId"] = guid;
                ViewBag.UploadedInfo = "file uploaded";
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    Tools.UploadsManager.RemoveUpload(guid);//delete previous
                    bool response = Tools.UploadsManager.UploadFile(guid, file);
                    logger.Info("guid: " + guid + " response: " + response);
                }
                Tools.UploadsManager.SaveFile(paperId, guid);
                int latest = Tools.UploadsManager.Latest(paperId).VersionId;
                Tools.PanelHelper.LogMessage("Обновлен файл препринта. [url=https://" + Request.Url.Host + "/Editor/Edit/DownloadFile?paperId=" + paperId + "&versionId=" + latest + "]Скачать новую версию.[/url]", paperId, 0, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
                logger.Error(ex.Message);
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName });
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
            }
        }

        public ActionResult UpdateCorrectorFile(int paperId)
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                logger.Info("Saving file...");
                string guid = (string)TempData["UniqueSubmissionId"];
                logger.Info("guid: " + guid);
                if (guid == null)
                {
                    guid = Guid.NewGuid().ToString();
                    logger.Info("guid is null. new guid: " + guid);
                }
                TempData["UniqueSubmissionId"] = guid;
                ViewBag.UploadedInfo = "file uploaded";
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    Tools.UploadsManager.RemoveUpload(guid);//delete previous
                    bool response = Tools.UploadsManager.UploadFile(guid, file);
                    logger.Info("guid: " + guid + " response: " + response);
                }
                Tools.UploadsManager.SaveFile(paperId, guid);
                int latest = Tools.UploadsManager.Latest(paperId).VersionId;
                Tools.PanelHelper.LogMessage("Обновлен файл препринта. [url=https://" + Request.Url.Host + "/Editor/Edit/DownloadFile?paperId=" + paperId + "&versionId=" + latest + "]Скачать новую версию.[/url]", paperId, 0, Tools.AccountHelper.GetRoleTranslation(Tools.AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId)));
            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
                logger.Error(ex.Message);
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName });
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
            }
        }
    }
}
