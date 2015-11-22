using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
namespace KeldyshPreprintSystem.Tools
{
    public class PaperVersionInfo
    {
        public string Date { get; set; }
        public string Path { get; set; }
        public int PaperId { get; set; }
        public int VersionId { get; set; }
        public string SHA256 { get; set; }
        public bool PaperCorrection { get; set; }
        public string Extension
        {
            get
            {
                return FileName.Split('.').Last();
            }
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }

        public string MIME
        {
            get
            {
                switch (this.Extension)
                {
                    case "doc":
                        return "application/word";
                    case "pdf":
                        return "application/pdf";
                    case "docx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    default:
                        return System.Net.Mime.MediaTypeNames.Application.Octet;
                }
            }
        }

        public string DownloadName
        {
            get
            {
                if (PaperCorrection)
                    return "(Корректура)" + PaperId + "v" + VersionId + "." + Extension;
                else
                    return PaperId + "v" + VersionId + "." + Extension;
            }
        }


        public static PaperVersionInfo DeserializeFromXmlString(string info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PaperVersionInfo));
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(info)))
            {
                PaperVersionInfo result = serializer.Deserialize(stream) as PaperVersionInfo;
                return result;
            }
        }

        public static string SerializeVersionToXmlString(PaperVersionInfo info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PaperVersionInfo));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, info);
                return System.Text.Encoding.Default.GetString(stream.ToArray()).Replace("\r", string.Empty).Replace("\n", string.Empty);
            }
        }

        public byte[] GetBytes()
        {
            return File.ReadAllBytes(Path);
        }

        public PaperVersionInfo(int pId, int vId, string p, string hash, bool isCorrection)
        {
            PaperId = pId;
            VersionId = vId;
            Path = p;
            SHA256 = hash;
            Date = DateTime.Now.ToString("yyyy.MM.dd");
            PaperCorrection = isCorrection;
        }


        public PaperVersionInfo()
        {

        }


        public override string ToString()
        {
            return Date + " " + DownloadName;
        }
    }

    public static class UploadsManager//current scheme of a filename paperid_#v
    {
        static string pathToWorkingDirectory = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/");
        static object lockTrigger = new Object();
        static Dictionary<string, string> fileUploads = new Dictionary<string, string>();

        public static void DeleteFiles(int paperId)
        {
            if (Directory.Exists(pathToWorkingDirectory + paperId))
                Directory.Delete(pathToWorkingDirectory + paperId);
        }

        public static bool UploadFile(string guid, HttpPostedFileBase attachment)
        {
            if (fileUploads.ContainsKey(guid))
                return false;
            else
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/converted/") + guid + Path.GetExtension(attachment.FileName);
                attachment.SaveAs(path);
                fileUploads.Add(guid, path);
                return true;
            }
        }

        public static bool UploadFile(string guid, string attachment)
        {
            if (fileUploads.ContainsKey(guid))
                return false;
            else
            {
                string path = attachment;
                fileUploads.Add(guid, path);
                return true;
            }
        }

        public static HttpPostedFileBase GetUpload(string guid)
        {
            if (guid == null)
                return null;
            if (!fileUploads.ContainsKey(guid))
                return null;
            else
            {
                var file = new KeldyshPreprintSystem.Models.FakePreprintAttachment(fileUploads[guid]);
                return file;
            }
        }

        public static string GetUploadPath(string guid)
        {
            if (guid == null)
                return null;
            if (!fileUploads.ContainsKey(guid))
                return null;
            else
            {
                var file = fileUploads[guid];
                return file;
            }
        }

        public static bool RemoveUpload(string guid)
        {
            if (!fileUploads.ContainsKey(guid))
                return false;
            else
            {
                fileUploads.Remove(guid);
                return true;
            }
        }

        public static void SaveFile(int paperId, HttpPostedFileBase attachment)
        {
            if (attachment != null)
            {
                PrepareDir(paperId);
                var fileName = Path.GetFileName(attachment.FileName);
                string extension = Path.GetExtension(fileName);
                Directory.CreateDirectory(pathToWorkingDirectory + paperId);
                var path = Path.Combine(pathToWorkingDirectory + paperId, Guid.NewGuid().ToString() + extension);
                attachment.SaveAs(path);
                LogVersion(paperId, path, false);
            }
        }


        public static void SaveFile(int paperId, string guid)
        {
            PrepareDir(paperId);
            string extension = Path.GetExtension(fileUploads[guid]);
            Directory.CreateDirectory(pathToWorkingDirectory + paperId);
            var path = Path.Combine(pathToWorkingDirectory + paperId, Guid.NewGuid().ToString() + extension);
            File.Move(fileUploads[guid], path);
            fileUploads.Remove(guid);
            LogVersion(paperId, path, false);
        }

        public static void SaveCorrectorFile(int paperId, string guid)
        {
            PrepareDir(paperId);
            string extension = Path.GetExtension(fileUploads[guid]);
            Directory.CreateDirectory(pathToWorkingDirectory + paperId);
            var path = Path.Combine(pathToWorkingDirectory + paperId, Guid.NewGuid().ToString() + extension);
            File.Move(fileUploads[guid], path);
            fileUploads.Remove(guid);
            LogVersion(paperId, path, true);
        }

        private static void PrepareDir(int paperId)
        {
            if (!Directory.Exists(pathToWorkingDirectory + paperId))
            {
                Directory.CreateDirectory(pathToWorkingDirectory + paperId);
                string vLog = pathToWorkingDirectory + paperId + "\\version.log";
                var f = File.Create(vLog);
                f.Close();
            }
        }

        public static Dictionary<int, PaperVersionInfo> GetVersionHistory(int paperId)
        {
            lock (lockTrigger)
            {
                string[] versions = File.ReadAllLines(pathToWorkingDirectory + paperId + "\\version.log");
                Dictionary<int, PaperVersionInfo> versionHistory = new Dictionary<int, PaperVersionInfo>();
                foreach (string f in versions)
                {
                    PaperVersionInfo info = PaperVersionInfo.DeserializeFromXmlString(f);
                    versionHistory.Add(info.VersionId, info);
                }
                return versionHistory;
            }
        }

        public static PaperVersionInfo Latest(int paperId)
        {
            var versions = GetVersionHistory(paperId);
            return versions[versions.Keys.Max()];
        }

        public static void LogVersion(int paperId, string path, bool isCorrection)
        {
            lock (lockTrigger)
            {
                string vLog = pathToWorkingDirectory + paperId + "\\version.log";
                int versiondId = 0;
                var versions = GetVersionHistory(paperId);
                if (versions.Keys.Count > 0)
                    versiondId = versions.Keys.Max() + 1;
                System.Security.Cryptography.SHA256 hash = System.Security.Cryptography.SHA256.Create();
                PaperVersionInfo info = new PaperVersionInfo(paperId, versiondId, path, BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)), 0).Replace("-", string.Empty), isCorrection);
                File.AppendAllText(vLog, PaperVersionInfo.SerializeVersionToXmlString(info) + "\r\n");
            }
        }

        public static PaperVersionInfo GetPaperVersion(int paperId, int versionId)
        {
            var versionHistory = GetVersionHistory(paperId);
            PaperVersionInfo result;
            if (versionHistory.TryGetValue(versionId, out result))
                return result;
            else
                throw new KeyNotFoundException("Version not found");
        }
    }
}