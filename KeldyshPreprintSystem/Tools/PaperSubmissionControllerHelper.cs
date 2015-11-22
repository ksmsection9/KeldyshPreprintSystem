using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using KeldyshPreprintSystem.Models;
using Novacode;
using System.Xml.Serialization;
using System.Diagnostics;

namespace KeldyshPreprintSystem.Tools
{
    public static class PaperSubmissionControllerHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static string pathToWorkingDirectory = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/users/");

        private static Object locker = new object();
        #region JSONStructure
        private class ProcessURL
        {
            public output output { get; set; }
        }

        private class output
        {
            public string url { get; set; }
        }
        #endregion

        public static string ConvertHtmlToDocx(string html)
        {
            string filepath = HttpContext.Current.Server.MapPath("~/App_Data/" + Guid.NewGuid().ToString() + ".htm");
            System.IO.File.WriteAllText(filepath, html);
            MadScripterWrappers.CloudConvert api = new MadScripterWrappers.CloudConvert(Tools.ConfigurationResourcesHelper.GetResource("options", "CloudConvertApiKey"));
            string processUrl = api.GetProcessURL("htm", "docx");
            api.UploadFile(processUrl, filepath, "docx");
            System.IO.File.Delete(filepath);//cleaningup
            string res = api.GetStatus(processUrl);
            //waiting for ready
            int counter = 4;
            while (res.IndexOf("finished") < 0 && counter >= 0)
            {
                System.Threading.Thread.Sleep(500);
                res = api.GetStatus(processUrl);
                counter--;
            }
            string url = (new JavaScriptSerializer().Deserialize<ProcessURL>(res)).output.url;
            if (!url.StartsWith("http"))
            {
                url = url.Insert(0, "https:");
            }

            using (WebClient client = new WebClient())
            {
                string respath = HttpContext.Current.Server.MapPath("~/App_Data/converted/" + Guid.NewGuid().ToString() + ".docx");
                client.DownloadFile(url, respath);
                return respath;
            }
        }

        public static PaperSubmissionModel GetModel(int id)
        {
            lock (locker)
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    var paper = db.PaperSubmissions.First(x => x.Id == id);
                    if (paper.Authors.Count == 0)
                        foreach (Author a in db.Authors.ToList())
                        {
                            if (a.PaperSubmissionModelId == id)
                                paper.Authors.Add(a);
                        }
                    paper.Attachment = new FakePreprintAttachment("bogus.pdf");
                    if (paper.Reviewer == null)
                        paper.Reviewer = db.Reviewers.FirstOrDefault(x => x.PaperSubmissionModelId == id);
                    return paper;
                }
            }
        }

        public static PaperSubmissionModel DeserializeModel(string paperInfo, string authorsInfo, string attachmentGuid)
        {
            PaperSubmissionModel result;
            XmlSerializer paperSerializer = new XmlSerializer(typeof(PaperSubmissionModel));
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(paperInfo)))
            {
                result = paperSerializer.Deserialize(stream) as PaperSubmissionModel;
            }

            if (!string.IsNullOrWhiteSpace(authorsInfo))
            {
                List<Author> authors;
                XmlSerializer authorsSerializer = new XmlSerializer(typeof(List<Author>));
                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(authorsInfo)))
                {
                    authors = authorsSerializer.Deserialize(stream) as List<Author>;
                    result.Authors = authors;
                }
            }

            if (!string.IsNullOrWhiteSpace(attachmentGuid))
            {
                //attachments are really sleazy
                //not sure if it should be saved at all in temp sumbission
                result.Attachment = Tools.UploadsManager.GetUpload(attachmentGuid);
            }

            return result;
        }

        public static string SerializeModel(PaperSubmissionModel paper)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PaperSubmissionModel));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, paper);
                return System.Text.Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static string SerializeAuthors(List<Author> authors)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Author>));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, authors);
                return System.Text.Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static bool GotAnySubmissions(int userId)
        {
            lock (locker)
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    List<PaperSubmissionModel> papers = db.PaperSubmissions.ToList();
                    foreach (var paper in papers)
                        if (WebMatrix.WebData.WebSecurity.GetUserId(paper.Owner) == userId)
                            return true;
                }
                return false;
            }
        }

        public static bool GotAnySubmissions(string username)
        {
            lock (locker)
            {
                using (PaperSubmissionsContext db = new PaperSubmissionsContext())
                {
                    List<PaperSubmissionModel> papers = db.PaperSubmissions.ToList();
                    foreach (var paper in papers)
                        if (paper.Owner == username)
                            return true;
                }
                return false;
            }
        }

        public static void SaveTemporarySubmission(string paperContent, string authorsContent, string attachmentGuid, string username)
        {
            string path = pathToWorkingDirectory + username;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string filename = Guid.NewGuid().ToString() + ".xml";
            string attachmentPath = string.Empty;
            if (!string.IsNullOrEmpty(attachmentGuid))
            {
                attachmentPath = Tools.UploadsManager.GetUploadPath(attachmentGuid);
                if (attachmentPath == null)
                    throw new InvalidOperationException("attachment path is null");
            }
            File.WriteAllText(path + "/" + filename, paperContent + "\t\n\t\n\t\n\t" + authorsContent + "\t\n\t\n\t\n\t" + attachmentGuid + ";" + attachmentPath);
        }

        public static PaperSubmissionModel GetTemporarySubmission(string guid, string username, out string attachmentGuid)
        {
            try
            {
                string path = pathToWorkingDirectory + username + "/" + guid + ".xml";
                string paperContent = File.ReadAllText(path);
                string[] splitContent = paperContent.Split(new string[] { "\t\n\t\n\t\n\t" }, StringSplitOptions.None);
                
                attachmentGuid = null;
                if (splitContent[2].Length > 1)
                    attachmentGuid = splitContent[2].Split(new char[] { ';' })[0];
                if (!string.IsNullOrEmpty(attachmentGuid))
                {
                    string attachmentPath = splitContent[2].Split(new char[] { ';' })[1];
                    string storedAttachmentPath = Tools.UploadsManager.GetUploadPath(attachmentGuid);
                    if (string.IsNullOrEmpty(storedAttachmentPath))
                    {
                        Tools.UploadsManager.UploadFile(attachmentGuid, attachmentPath);
                    }
                    else
                        if (storedAttachmentPath != attachmentPath)
                            throw new InvalidOperationException("Inconsistent paths to attachment.");
                }
                return DeserializeModel(splitContent[0], splitContent[1], attachmentGuid);
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidDataException("Invalid file format");
            }
        }

        public static string[] GetAllTemporarySubmissions(string username)
        {
            string path = pathToWorkingDirectory + username;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string[] result = Directory.GetFiles(path);
            for (int i = 0; i < result.Length; i++)
                result[i] = Path.GetFileNameWithoutExtension(result[i]);
            return result;
        }

        public static string GetCyrillicId(int paperId)
        {
            //paperId is zerobased 0 -- ЗВК-ААА
            char[] alphabet = new char[] { 'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З', 'И', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Э', 'Ю', 'Я' };
            int thirdSymbol = paperId % alphabet.Length;
            int secondSymbol = (paperId / alphabet.Length);
            int firstSymbol = (secondSymbol / alphabet.Length);
            if (secondSymbol > alphabet.Length)
                secondSymbol = (secondSymbol / alphabet.Length) % alphabet.Length;
            return "ЗВК-" + alphabet[firstSymbol] + alphabet[secondSymbol] + alphabet[thirdSymbol];
        }

        public static string ConvertDocxToPDF(string filepath)
        {
            //string filepath = HttpContext.Current.Server.MapPath("~/App_Data/" + Guid.NewGuid().ToString() + ".docx");
            //System.IO.File.WriteAllBytes(filepath, docx);
            MadScripterWrappers.CloudConvert api = new MadScripterWrappers.CloudConvert(Tools.ConfigurationResourcesHelper.GetResource("options", "CloudConvertApiKey"));
            string processUrl = api.GetProcessURL("docx", "pdf");
            api.UploadFile(processUrl, filepath, "pdf");
            System.IO.File.Delete(filepath);//cleaningup
            string res = api.GetStatus(processUrl);
            //waiting for ready
            int counter = 4;
            while (res.IndexOf("finished") < 0 && counter >= 0)
            {
                System.Threading.Thread.Sleep(500);
                res = api.GetStatus(processUrl);
                counter--;
            }
            string url = (new JavaScriptSerializer().Deserialize<ProcessURL>(res)).output.url;
            if (!url.StartsWith("http"))
            {
                url = url.Insert(0, "https:");
            }

            using (WebClient client = new WebClient())
            {
                string respath = HttpContext.Current.Server.MapPath("~/App_Data/converted/" + Guid.NewGuid().ToString() + ".pdf");
                client.DownloadFile(url, respath);
                return respath;
            }
        }

        public static string GenerateFrontCoverPdfSyncIfNotExists(int id, int libraryId = 999)
        {
            string pathToPdf = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/" + id + "/" + "front_cover" + libraryId + ".pdf");
            if (!File.Exists(pathToPdf))
            {
                string[] args = new string[] { System.Web.HttpContext.Current.Server.MapPath("~/App_Data/bin/wkhtmltopdf.exe"), "https://" + System.Web.HttpContext.Current.Request.Url.Host + ":" + System.Web.HttpContext.Current.Request.Url.Port + "/PaperSubmission/GetFrontCover?id=" + id + "&libraryId=" + libraryId + " " + pathToPdf };
                logger.Info("args: " + args[0] + " " + args[1]);
                ProcessStartInfo info = new ProcessStartInfo(args[0], args[1]);
                info.CreateNoWindow = true;
                Process.Start(info).WaitForExit();
            }
            return pathToPdf;
        }

        public static string GenerateFrontCoverJpgSyncIfNotExists(int id, int libraryId = 999)
        {
            string pathToJpg = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/" + id + "/" + "front_cover_" + libraryId + ".jpg");
            if (!File.Exists(pathToJpg))
            {
                string[] args = new string[] { System.Web.HttpContext.Current.Server.MapPath("~/App_Data/bin/wkhtmltopdf.exe"), "https://" + System.Web.HttpContext.Current.Request.Url.Host + ":" + System.Web.HttpContext.Current.Request.Url.Port + "/PaperSubmission/GetFrontCover?id=" + id + "&libraryId=" + libraryId + " " + pathToJpg };
                logger.Info("args: " + args[0] + " " + args[1]);
                ProcessStartInfo info = new ProcessStartInfo(args[0], args[1]);
                info.CreateNoWindow = true;
                Process.Start(info).WaitForExit();
            }
            return pathToJpg;
        }

        public static string GenerateFrontCoverPdf(int id, int libraryId = 999)
        {
            string pathToPdf = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/" + id + "/" + "front_cover" + libraryId + ".pdf");
            string[] args = new string[] { System.Web.HttpContext.Current.Server.MapPath("~/App_Data/bin/wkhtmltopdf.exe"), "https://" + System.Web.HttpContext.Current.Request.Url.Host + ":" + System.Web.HttpContext.Current.Request.Url.Port + "/PaperSubmission/GetFrontCover?id=" + id + "&libraryId=" + libraryId + " " + pathToPdf };
            System.Threading.Tasks.Task.Factory.StartNew(new Action<object>(x => { ProcessStartInfo info = new ProcessStartInfo(args[0], args[1]); info.CreateNoWindow = true; Process.Start(info); }), args);
            return pathToPdf;
        }

        public static string GenerateFrontCoverImage(int id, int libraryId = 999)
        {
            string pathToJpg = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/" + id + "/" + "front_cover" + libraryId + ".jpg");
            string[] args = new string[] { System.Web.HttpContext.Current.Server.MapPath("~/App_Data/bin/wkhtmltoimage.exe"), "https://" + System.Web.HttpContext.Current.Request.Url.Host + ":" + System.Web.HttpContext.Current.Request.Url.Port + "/PaperSubmission/GetFrontCover?id=" + id + "&libraryId=" + libraryId + " " + pathToJpg };
            System.Threading.Tasks.Task.Factory.StartNew(new Action<object>(x => { ProcessStartInfo info = new ProcessStartInfo(args[0], args[1]); info.CreateNoWindow = true; Process.Start(info); }), args);
            return pathToJpg;
        }

        public static string CreateOrGetExistingDocument(PaperSubmissionModel paper, bool correctorVersion, bool overrideExisting)
        {
            //first of all performing check whether the file already exists
            string filename = correctorVersion ? "submission_for_corrector.docx" : "submission.docx";
            string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/" + paper.Id + "/" + filename);
            if (File.Exists(path) && !overrideExisting)
                return path;

            string pathToDocTemplate = "~/App_Data/request_template.docx";
            if (correctorVersion)
                pathToDocTemplate = "~/App_Data/request_template_corrector.docx";
            using (DocX document = DocX.Load(HttpContext.Current.Server.MapPath(pathToDocTemplate)))
            {
                Row r = document.Tables[0].Rows[1];
                for (int i = 0; i < paper.Authors.Count - 1; i++)//there is always 2 rows(one for title, one for author), so adding insufficient amount of rows
                    document.Tables[0].InsertRow(r, 1);
                List<Author> authors = paper.Authors.ToList();
                if (authors.Count > 0)
                    for (int i = 0; i < document.Tables[0].RowCount - 1; i++)
                    {
                        Row currentRow = document.Tables[0].Rows[i + 1];
                        //string shortNameRussian = authors[i].LastnameRussian + " " + authors[i].FirstnameRussian[0] + ".";
                        //if (authors[i].PatronymRussian.Length > 0)
                        //    shortNameRussian += authors[i].PatronymRussian[0] + ".";
                        currentRow.Cells[0].Paragraphs[0].InsertText(authors[i].LastnameRussian + " " + authors[i].FirstnameRussian + " " + authors[i].PatronymRussian);
                        //p1.IndentationBefore = 0.0f;
                        //string shortNameEnglish = authors[i].LastnameEnglish + " " + authors[i].FirstnameEnglish[0] + ".";
                        //if (authors[i].PatronymEnglish.Length > 0)
                        //    shortNameEnglish += authors[i].PatronymEnglish[0] + ".";
                        currentRow.Cells[1].Paragraphs[0].InsertText(authors[i].LastnameEnglish + " " + authors[i].FirstnameEnglish + " " + authors[i].PatronymEnglish);

                        currentRow.Cells[2].Paragraphs[0].InsertText(authors[i].PlaceOfWork);
                        currentRow.Cells[3].Paragraphs[0].InsertText(authors[i].Email + " " + authors[i].PersonalWeb);
                        string spin = String.IsNullOrEmpty(authors[i].SPIN) ? "Нет" : authors[i].SPIN;
                        string orcid = String.IsNullOrEmpty(authors[i].ORCID) ? "Нет" : authors[i].ORCID;
                        string researcherid = String.IsNullOrEmpty(authors[i].ResearcherID) ? "Не указан" : authors[i].ResearcherID;
                        currentRow.Cells[4].Paragraphs[0].InsertText("SPIN " + spin + ";\r\nORCID " + orcid + ";\r\nResearcherID " + researcherid);
                    }

                document.ReplaceText("@TitleRussian@", paper.TitleRussian, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@TitleEnglish@", paper.TitleEnglish, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@KeywordsRussian@", paper.KeywordsRussian.Replace(";", "; "), false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@KeywordsEnglish@", paper.KeywordsEnglish.Replace(";", "; "), false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@Languages@", paper.Languages, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@NumOfPages@", paper.NumberOfPages.ToString(), false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@NumOfAuthorsCopies@", paper.NumberOfAuthorsCopies.ToString(), false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@FieldOfResearch@", FieldOfResearchConverter.GetRussian(paper.FieldOfResearch), false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@UDK@", paper.UDK, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@FinancialSupport@", paper.FinancialSupport, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                document.ReplaceText("@Contacts@", paper.ContactName + ", " + paper.ContactPhone, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //signature fields
                int signatureTableIndex = 10;
                if (correctorVersion)
                    signatureTableIndex = 12;
                for (int i = 0; i < (paper.Authors.Count / 2); i++)//there is always 1 row, so adding insufficient amount of rows
                    document.Tables[signatureTableIndex].InsertRow(1);
                string signature = document.Tables[signatureTableIndex].Rows[0].Cells[0].Paragraphs[0].Text;//first cell always containing signature field
                int signatureCount = 0;
                document.Tables[signatureTableIndex].Rows[0].Cells[0].Paragraphs[0].RemoveText(0);
                document.Tables[signatureTableIndex].Rows[0].Cells[0].Paragraphs[0].InsertText(signature.Replace("@PartName@", authors[0].FirstnameRussian.FirstOrDefault() + "." + (authors[0].PatronymRussian != null ? authors[0].PatronymRussian.FirstOrDefault() + "." : string.Empty) + " " + authors[0].LastnameRussian));
                foreach (var row in document.Tables[signatureTableIndex].Rows)
                {
                    //there is just two columns, so i think it is better the way it is than for(int i=0; i<=1; i++)
                    //this is code for first column
                    if (signatureCount > 0)
                        row.Cells[0].Paragraphs[0].InsertText(signature.Replace("@PartName@", authors[signatureCount].FirstnameRussian.FirstOrDefault() + "." + (authors[signatureCount].PatronymRussian != null ? authors[signatureCount].PatronymRussian.FirstOrDefault() + "." : string.Empty) + " " + authors[signatureCount].LastnameRussian));
                    signatureCount++;
                    if (signatureCount == paper.Authors.Count)
                        break;
                    //this is code for second column
                    row.Cells[1].Paragraphs[0].InsertText(signature.Replace("@PartName@", authors[signatureCount].FirstnameRussian.FirstOrDefault() + "." + (authors[signatureCount].PatronymRussian != null ? authors[signatureCount].PatronymRussian.FirstOrDefault() + "." : string.Empty) + " " + authors[signatureCount].LastnameRussian));
                    signatureCount++;
                    if (signatureCount == paper.Authors.Count)
                        break;
                }

                if (correctorVersion)
                {
                    DocX add = DocX.Load(ConvertHtmlToDocx("<html><div><table><tr><td>" + paper.AbstractRussian + "</td><td>" + paper.AbstractEnglish + "</td></tr></table></div><br/><hr/><br/><div><table><tr><td>" + paper.Bibliography + "</td></tr></table></div><br/><hr/><br/><div><table><tr><td>" + paper.Review + "</td></tr></table></div></html>"));

                    foreach (Paragraph p in add.Tables[0].Rows[0].Cells[0].Paragraphs)//abstract russian
                    {
                        p.IndentationFirstLine = 0.0f;
                        document.Tables[2].Rows[1].Cells[0].InsertParagraph(p);
                    }
                    foreach (Paragraph p in add.Tables[0].Rows[0].Cells[1].Paragraphs)//abstract english
                    {
                        p.IndentationFirstLine = 0.0f;
                        document.Tables[2].Rows[1].Cells[1].InsertParagraph(p);
                    }

                    foreach (Paragraph p in add.Tables[1].Rows[0].Cells[0].Paragraphs)//bibliography
                    {
                        p.IndentationFirstLine = 0.0f;
                        document.Tables[9].Rows[0].Cells[0].InsertParagraph(p);
                    }

                    foreach (Paragraph p in add.Tables[2].Rows[0].Cells[0].Paragraphs)//review
                    {
                        p.IndentationFirstLine = 0.0f;
                        document.InsertParagraph(p);
                    }

                    add.Dispose();
                    DocX end = DocX.Load(HttpContext.Current.Server.MapPath("~/App_Data/request_template_ending.docx"));
                    end.ReplaceText("@JobPosition@", paper.Reviewer.JobTitle, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    string reviewerPartName = paper.Reviewer.Degree + " " + paper.Reviewer.Firstname.FirstOrDefault() + "." + (paper.Reviewer.Patronym != null ? paper.Reviewer.Patronym.FirstOrDefault() + "." : string.Empty) + " " + paper.Reviewer.Lastname;
                    end.ReplaceText("@ReviewerSignature@", reviewerPartName, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    document.InsertDocument(end);
                    end.Dispose();
                }
                else
                {
                    DocX add = DocX.Load(ConvertHtmlToDocx("<html>" + paper.Review + "</html>"));
                    document.InsertDocument(add);
                    add.Dispose();
                    DocX end = DocX.Load(HttpContext.Current.Server.MapPath("~/App_Data/request_template_ending.docx"));
                    end.ReplaceText("@JobPosition@", paper.Reviewer.JobTitle, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    string reviewerPartName = paper.Reviewer.Degree + " " + paper.Reviewer.Firstname.FirstOrDefault() + "." + (paper.Reviewer.Patronym != null ? paper.Reviewer.Patronym.FirstOrDefault() + "." : string.Empty) + " " + paper.Reviewer.Lastname;
                    end.ReplaceText("@ReviewerSignature@", reviewerPartName, false, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    document.InsertDocument(end);
                    end.Dispose();
                }
                document.SaveAs(path);
                return path;
                //using (var stream = new MemoryStream())
                //{
                //    // Save changes made to this document
                //    document.SaveAs(stream);
                //    return stream.ToArray();
                //}
            }
        }
    }
}