using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KeldyshPreprintSystem;
using KeldyshPreprintSystem.Controllers;
using KeldyshPreprintSystem.Models;
using System.Web.Mvc;

namespace KeldyshPreprintSystem.Tests
{
    [TestClass]
    public class PaperSubmissionTest
    {
        [TestMethod]
        public void BasicCreateViewTest()
        {
            var controller = new PaperSubmissionController();
            var result = controller.Create() as ViewResult;
            Assert.IsFalse(result.TempData.ContainsKey("UniqueSubmissionId") || result.ViewBag.Title == "Заявка на издание препринта ИПМ");
        }

        [TestMethod]
        public void BasicCreateViewWithArgTest()
        {
            string guid = Guid.NewGuid().ToString();
            System.Web.HttpContext.Current = new System.Web.HttpContext(new System.Web.HttpRequest("none", "http://localhost", ""), new System.Web.HttpResponse(null));
            Tools.UploadsManager.UploadFile(guid, "value");
            var controller = new PaperSubmissionController();
            var result = controller.Create(guid) as ViewResult;
            Assert.IsTrue(result.TempData.ContainsKey("UniqueSubmissionId") && (string)result.TempData["UniqueSubmissionId"] == "value" && result.ViewBag.UploadedInfo > 0);
        }
    }
}
