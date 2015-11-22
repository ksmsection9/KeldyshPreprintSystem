using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using KeldyshPreprintSystem.Models;

namespace KeldyshPreprintSystem.Tools
{
    public static class PanelHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static object lockTrigger = new Object();
        static string pathToWorkingDirectory = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads/");
        static Regex idPicker = new Regex("id=\"([0-9]*)\":([0-9]*):");

        public static MailInfo GetMailTemplateForCurrentState(int stateId)
        {
            if (Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId)))
            {
                string[] files = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId));
                if (files.Length > 0)
                {
                    string serialized = File.ReadAllText(files.First());
                    MailInfo mail = MailInfo.DeserializeFromXmlString(serialized);
                    return mail;
                }
            }
            return null;
        }

        public static List<MailInfo> GetMailsForCurrentState(int stateId, PaperSubmissionModel paper, string message)
        {
            List<MailInfo> result = new List<MailInfo>();
            string[] files = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/" + stateId));
            foreach (string file in files)
            {
                string serialized = File.ReadAllText(file);
                MailInfo mail = MailInfo.DeserializeFromXmlString(serialized);
                mail.Subject = Tools.ConfigurationResourcesHelper.GetStringWithSubstitutions(mail.Subject.Replace("@message@", message).Trim(), paper, paper.Authors.FirstOrDefault());
                mail.Body = Tools.ConfigurationResourcesHelper.GetStringWithSubstitutions(mail.Body.Replace("@message@", message).Trim(), paper, paper.Authors.FirstOrDefault());
                mail.PaperId = paper.Id;
                if (mail.IsAnyAttachments)
                    for (int i = 0; i < mail.Attachments.Count; i++)
                    {
                        if (mail.Attachments[i] == "ReplaceForPathToPaper")
                        {
                            mail.Attachments[i] = Tools.UploadsManager.Latest(paper.Id).Path;
                        }
                        if (mail.Attachments[i] == "ReplaceForPathToSubmission")
                        {
                            mail.Attachments[i] = Tools.PaperSubmissionControllerHelper.CreateOrGetExistingDocument(paper, false, false);//submission file
                        }
                    }

                switch (mail.To)
                {
                    case "toAllAuthors":
                        var authors = paper.Authors;
                        if (authors == null || authors.Count == 0)
                            break;
                        mail.To = authors.First().Email;
                        for (int i = 1; i < paper.Authors.Count; i++)
                        {
                            Author a = paper.Authors.ElementAt(i);
                            MailInfo copymail = MailInfo.DeserializeFromXmlString(serialized);
                            copymail.Body = copymail.Body.Replace("@message@", message).Trim();
                            copymail.Attachments.Clear();
                            copymail.Attachments.AddRange(mail.Attachments);
                            copymail.To = a.Email;
                            result.Add(copymail);
                        }
                        break;
                    case "toEditor":
                        var editors = Tools.AccountHelper.GetUsersInRole("admin");
                        if (editors == null || editors.Count == 0)
                            break;
                        mail.To = editors.First();
                        for (int i = 1; i < editors.Count; i++)
                        {
                            string editor = editors[i];
                            MailInfo copymail = MailInfo.DeserializeFromXmlString(serialized);
                            copymail.Body = copymail.Body.Replace("@message@", message).Trim();
                            copymail.Attachments.Clear();
                            copymail.Attachments.AddRange(mail.Attachments);
                            copymail.To = editor;
                            result.Add(copymail);
                        }
                        break;
                    case "toCorrector":
                        var correctors = Tools.AccountHelper.GetUsersInRole("corrector");
                        if (correctors == null || correctors.Count == 0)
                            break;
                        mail.To = correctors.First();
                        for (int i = 1; i < correctors.Count; i++)
                        {
                            string corrector = correctors[i];
                            MailInfo copymail = MailInfo.DeserializeFromXmlString(serialized);
                            copymail.Body = copymail.Body.Replace("@message@", message).Trim();
                            copymail.Attachments.Clear();
                            copymail.Attachments.AddRange(mail.Attachments);
                            copymail.To = corrector;
                            result.Add(copymail);
                        }
                        break;
                    case "toAuthor":
                        mail.To = paper.Owner;
                        break;
                    case "toEditorialBoard":
                        throw new NotImplementedException("Рассылка почты редколлегии не реализована");
                        break;
                    case "toTypographer":
                        var typographers = Tools.AccountHelper.GetUsersInRole("typographer");
                        if (typographers == null || typographers.Count == 0)
                            break;
                        mail.To = typographers.First();
                        for (int i = 1; i < typographers.Count; i++)
                        {
                            string typographer = typographers[i];
                            MailInfo copymail = MailInfo.DeserializeFromXmlString(serialized);
                            copymail.Body = copymail.Body.Replace("@message@", message).Trim();
                            copymail.Attachments.Clear();
                            copymail.Attachments.AddRange(mail.Attachments);
                            copymail.To = typographer;
                            result.Add(copymail);
                        }
                        break;
                    case "toInternetGuy":
                        var internetGuys = Tools.AccountHelper.GetUsersInRole("internetguy");
                        if (internetGuys == null || internetGuys.Count == 0)
                            break;
                        mail.To = internetGuys.First();
                        for (int i = 1; i < internetGuys.Count; i++)
                        {
                            string internetGuy = internetGuys[i];
                            MailInfo copymail = MailInfo.DeserializeFromXmlString(serialized);
                            copymail.Body = copymail.Body.Replace("@message@", message).Trim();
                            copymail.Attachments.Clear();
                            copymail.Attachments.AddRange(mail.Attachments);
                            copymail.To = internetGuy;
                            result.Add(copymail);
                        }
                        break;
                }
                result.Add(mail);
            }
            return result;
        }


        public static List<SmsInfo> GetSmsForCurrentState(int stateId, PaperSubmissionModel paper)
        {
            List<SmsInfo> result = new List<SmsInfo>();
            string[] files = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId));
            foreach (string file in files)
            {
                string serialized = File.ReadAllText(file);
                SmsInfo sms = SmsInfo.DeserializeFromXmlString(serialized);
                sms.PaperId = paper.Id;

                switch (sms.To)
                {
                    case "toAllAuthors":
                        sms.To = paper.ContactPhone.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                        break;
                    case "toEditor":
                        sms.To = Tools.ConfigurationResourcesHelper.GetResource("mailinglist", "EditorPhone");
                        break;
                    case "toCorrector":
                        sms.To = Tools.ConfigurationResourcesHelper.GetResource("mailinglist", "CorrectorPhone");
                        break;
                    case "toAuthor":
                        sms.To = paper.ContactPhone;
                        break;
                    case "toEditorialBoard":
                        throw new NotImplementedException("Рассылка смс редколлегии не реализована");
                    //break;
                }
                result.Add(sms);
            }
            return result;
        }

        public static SmsInfo GetSmsTemplateForCurrentState(int stateId)
        {
            if (Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId)))
            {
                string[] files = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/templates/sms/" + stateId));
                foreach (string file in files)
                {
                    string serialized = File.ReadAllText(file);
                    SmsInfo sms = SmsInfo.DeserializeFromXmlString(serialized);
                    sms.PaperId = -1;
                    return sms;
                }
            }
            return null;
        }

        public static void SendSmsForCurrentState(PaperSubmissionModel paper, int stateId)
        {
            var context = GetContext(paper.Id);
            if (context.smsForbidden)
                return;

            var messages = GetSmsForCurrentState(stateId, paper);
            foreach (SmsInfo sms in messages)
            {
                if (sms.HourInterval > 0)
                {
                    SMSSender.ScheduleMessage(sms);
                }
                else
                    SMSSender.SendSms(sms);
            }
        }


        public static void SendMailForCurrentState(PaperSubmissionModel paper, int stateId, string message)
        {
            var messages = GetMailsForCurrentState(stateId, paper, message);
            foreach (MailInfo mail in messages)
            {
                if (mail.HourInterval > 0)
                {
                    MailSender.ScheduleMessage(mail);
                }
                else
                    MailSender.SendMail(mail);
            }
        }

        public static void ToggleSmsForbiddanceContext(int paperId)
        {
            lock (lockTrigger)
            {
                var context = GetContext(paperId);
                context.smsForbidden = !context.smsForbidden;
                UpdateContext(context);
            }
        }

        public static void LogMessage(string text, int paperId, int subLogId, string username)
        {
            lock (lockTrigger)
            {
                LogEntry e = new LogEntry();
                e.Message = text;
                e.subLogId = subLogId;
                e.TimeStamp = DateTime.Now;
                e.UserName = WebMatrix.WebData.WebSecurity.CurrentUserName;
                e.UserRole = AccountHelper.GetUserRole(WebMatrix.WebData.WebSecurity.CurrentUserId);
                e.StatusChanged = true;
                LogMessage(paperId, e);
                //string path = pathToWorkingDirectory + paperId + "/log.html";
                //string message = "id=\"" + (GetLastId(paperId, subLogId) + 1) + "\":" + subLogId + ":" + DateTime.Now.ToString("yyyy.MM.dd HH:mm") + " [b]" + username + "[/b]: " + text + "\r\n";
                //File.AppendAllText(path, message);
            }
        }

        public static void LogMessage(int paperId, LogEntry e)
        {
            lock (lockTrigger)
            {
                var context = GetContext(paperId);
                context.Log.Add(e);
                UpdateContext(context);
            }
        }
   
        //public static string GetWholeSubLogAfterId(int paperId, int subLogId, int id)
        //{
        //    lock (lockTrigger)
        //    {
        //        string path = pathToWorkingDirectory + paperId + "/log.html";
        //        if (File.Exists(path))
        //        {
        //            string content = File.ReadAllText(path);
        //            var matches = idPicker.Matches(content);
        //            System.Text.StringBuilder result = new System.Text.StringBuilder();
        //            for (int i = 0; i < matches.Count; i++)
        //            {
        //                //logger.Info("getwolesublogafterid: id"+Convert.ToInt32(matches[i].Groups[1].Value) +" sublogid: "+ Convert.ToInt32(matches[i].Groups[2].Value) + " sublogpassed: "+subLogId+" id:"+ id);
        //                if (matches[i].Success && Convert.ToInt32(matches[i].Groups[1].Value) > id && (subLogId == 0 || Convert.ToInt32(matches[i].Groups[2].Value) == subLogId))
        //                {
        //                    //logger.Info(matches[i].Index + " " + content.Length);
        //                    int index = content.Length;
        //                    if (i < matches.Count - 1)
        //                        index = matches[i + 1].Index;
        //                    string partContent = content.Substring(matches[i].Index, index - matches[i].Index);
        //                    partContent = idPicker.Replace(partContent, string.Empty);
        //                    result.Append(partContent);
        //                }
        //            }
        //            return result.ToString();
        //        }
        //        return String.Empty;
        //    }
        //}

        public static int GetSubLogPartId(string role)
        {
            switch (role)
            {
                case "user":
                    return 3;
                case "corrector":
                    return 5;
                case "typographer":
                    return 11;
                case "admin":
                    return 13;
                case "internetguy":
                    return 17;
                default:
                    return 1;
            }
        }


        public static void CreateContext(PaperSubmissionModel paper)
        {
            lock (lockTrigger)
            {
                PanelContextModel context = new PanelContextModel();
                context.paperId = paper.Id;
                string path = pathToWorkingDirectory + paper.Id + "/context.xml";

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PanelContextModel));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, context);
                    File.WriteAllText(path, System.Text.Encoding.Default.GetString(stream.ToArray()));
                }
            }
        }

        public static void UpdateContext(PanelContextModel context)
        {
            lock (lockTrigger)
            {
                string path = pathToWorkingDirectory + context.paperId + "/context.xml";
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PanelContextModel));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, context);
                    File.WriteAllText(path, System.Text.Encoding.Default.GetString(stream.ToArray()));
                }
            }
        }

        public static PanelContextModel GetContext(int id)
        {
            lock (lockTrigger)
            {
                string path = pathToWorkingDirectory + id + "/context.xml";
                string content = File.ReadAllText(path);

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PanelContextModel));
                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(content)))
                {
                    PanelContextModel result = serializer.Deserialize(stream) as PanelContextModel;
                    return result;
                }
            }
        }
    }
}