using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using System.Xml.Serialization;
using System.IO;

namespace KeldyshPreprintSystem.Tools
{
    //public class DailySendEmailJob : IJob
    //{
    //    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    //    private static Object locker = new Object();

    //public static void PrepareSchedule()
    //{
    //    CleanMails();
    //    foreach (var mail in GetScheduledMails())
    //    {
    //        // construct job info
    //        //var builderSms = Quartz.JobBuilder.Create(typeof(Tools.DailySendSMSJob)).WithIdentity("SmsJob", "group1");
    //        //var trigBuilderSms = TriggerBuilder.Create().WithIdentity("dailyTriggerSms", "group1").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever());
    //        //sched.ScheduleJob(builderSms.Build(), trigBuilderSms.Build());

    //        ////email
    //        if (!KeldyshPreprintSystem.MvcApplication.Sched.CheckExists(new JobKey(mail.Id, "groupmail")))
    //        {
    //            logger.Info("new job " + mail.PaperId + ":" + mail.StateId);
    //            var builderEmail = Quartz.JobBuilder.Create(typeof(Tools.DailySendEmailJob)).WithIdentity(mail.Id, "groupmail");
    //            //.StartAt(DateTime.UtcNow+new TimeSpan(mail.HourInterval, 0, 0))
    //            var trigBuilderEmail = TriggerBuilder.Create().WithIdentity("dailyTriggerEmail_" + mail.Id, "groupmail").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(mail.HourInterval).RepeatForever());
    //            // schedule the job for execution
    //            KeldyshPreprintSystem.MvcApplication.Sched.ScheduleJob(builderEmail.Build(), trigBuilderEmail.Build());
    //        }
    //    }
    //}

    //private static List<MailInfo> GetScheduledMails()
    //{
    //    //lock (locker)
    //{
    //    List<MailInfo> result = new List<MailInfo>();
    //    string[] mails = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/"));
    //    foreach (string mail in mails)
    //    {
    //        MailInfo info = MailInfo.DeserializeFromXmlString(File.ReadAllText(mail));
    //        result.Add(info);
    //    }
    //    return result;
    //}
    //}

    //private static int CleanMails()
    //{
    //    lock (locker)
    //    {
    //        int result = 0;
    //        string[] mails = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/"));
    //        foreach (string mail in mails)
    //        {
    //            MailInfo info = MailInfo.DeserializeFromXmlString(File.ReadAllText(mail));
    //            if (info.IsClosed)
    //            {
    //                File.Delete(mail);
    //                result++;
    //            }
    //        }
    //        return result;
    //    }
    //}


    //public static int CleanMailsUnconditionaly()
    //{
    //    lock (locker)
    //    {
    //        int result = 0;
    //        string[] mails = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/"));
    //        foreach (string mail in mails)
    //        {
    //            File.Delete(mail);
    //            result++;
    //        }

    //        var executingJobs = KeldyshPreprintSystem.MvcApplication.Sched.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup());
    //        foreach (var job in executingJobs)
    //        {
    //            KeldyshPreprintSystem.MvcApplication.Sched.DeleteJob(job);
    //        }

    //        return result;
    //    }
    //}

    //public static int CleanMails(int paperId, int stateId)
    //{
    //    lock (locker)
    //    {
    //        int result = 0;
    //        string[] mails = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/"));
    //        foreach (string mail in mails)
    //        {
    //            MailInfo info = MailInfo.DeserializeFromXmlString(File.ReadAllText(mail));
    //            if (info.PaperId == paperId && info.StateId == stateId)
    //            {
    //                File.Delete(mail);
    //                result++;
    //            }
    //        }
    //        return result;
    //    }
    //}

    //public static int CleanMails(int paperId)
    //{
    //    lock (locker)
    //    {
    //        int result = 0;
    //        string[] mails = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/"));
    //        foreach (string mail in mails)
    //        {
    //            MailInfo info = MailInfo.DeserializeFromXmlString(File.ReadAllText(mail));
    //            if (info.PaperId == paperId)
    //            {
    //                File.Delete(mail);
    //                result++;
    //            }
    //        }
    //        return result;
    //    }
    //}

    //public static MailInfo GetMailById(string id)
    //{
    //    string xml = File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/" + id + ".xml"));
    //    return MailInfo.DeserializeFromXmlString(xml);
    //}


    //public static void AddMessage(List<string[]> mailingList, string subject, string body, List<string> attachments, int interval, int paperId, int stateId)
    //{
    //    string from = Tools.ConfigurationResourcesHelper.GetResourceVal("options", "EmailNotificationAddress");
    //    foreach (string[] s in mailingList)
    //    {
    //        string id = paperId + "_" + stateId + "_" + Guid.NewGuid().ToString();
    //        MailInfo mail = new MailInfo(subject, body, attachments, from, s[0], interval, paperId, stateId, id);
    //        File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/mail/" + id + ".xml"), MailInfo.SerializeVersionToXmlString(mail));
    //        //email
    //        var builderEmail = Quartz.JobBuilder.Create(typeof(Tools.DailySendEmailJob)).WithIdentity(id, "groupmail");
    //        //.StartAt(DateTime.UtcNow+new TimeSpan(mail.HourInterval, 0, 0))
    //        var trigBuilderEmail = TriggerBuilder.Create().WithIdentity("dailyTriggerEmail_" + id, "groupmail").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(mail.HourInterval).RepeatForever());
    //        // schedule the job for execution
    //        KeldyshPreprintSystem.MvcApplication.Sched.ScheduleJob(builderEmail.Build(), trigBuilderEmail.Build());
    //    }
    //}



    //public void Execute(IJobExecutionContext context)
    //{
    //    //CleanMails();
    //    var emailsToSend = GetScheduledMails();
    //    string id = context.JobDetail.Key.Name;
    //    for (int i = 0; i < emailsToSend.Count; i++)
    //        if (emailsToSend[i].Id == id)
    //            MailSender.SendMail(emailsToSend[i]);
    //    //}
    //}

    public class MailInfo
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public List<string> Attachments { get; set; }
        public int HourInterval { get; set; }
        public bool IsClosed { get; set; }
        public int PaperId { get; set; }
        public int StateId { get; set; }
        public DateTime CreationDate { get; set; }
        public string Id { get; set; }

        //public MailMessage MailMessage
        //{
        //    get
        //    {
        //        MailMessage message = new MailMessage(this.From, this.To)
        //        {
        //            Subject = this.Subject,
        //            BodyEncoding = Encoding.UTF8,
        //            Body = this.Body,
        //            IsBodyHtml = false,
        //            SubjectEncoding = Encoding.UTF8
        //        };

        //        //NOT IMPLEMENTED
        //        //foreach (string path in Attachments)
        //        //    message.Attachments.Add(new Attachment(path));

        //        return message;
        //    }
        //}

        public MailInfo()
        {

        }

        public MailInfo(string subject, string body, List<string> attachments, string from, string to, int interval, int paperId, int stateId, string id)
        {
            Subject = subject;
            Body = body;
            Attachments = new List<string>();
            Attachments.AddRange(attachments);
            From = from;
            To = to;
            HourInterval = interval;
            CreationDate = DateTime.Now;
            PaperId = paperId;
            StateId = stateId;
            Id = id;
        }

        public MailInfo(string subject, string body, List<string> attachments, string to, int interval, int paperId, int stateId, string id)
        {
            Subject = subject;
            Body = body;
            Attachments = new List<string>();
            Attachments.AddRange(attachments);
            From = Tools.ConfigurationResourcesHelper.GetResource("options", "EmailNotificationAddress");
            To = to;
            HourInterval = interval;
            PaperId = paperId;
            StateId = stateId;
            Id = id;
            CreationDate = DateTime.Now;
        }

        public MailInfo(string subject, string body, string from, string to, int interval, int paperId, int stateId, string id)
        {
            Subject = subject;
            Body = body;
            Attachments = new List<string>();
            From = from;
            To = to;
            HourInterval = interval;
            PaperId = paperId;
            StateId = stateId;
            Id = id;
            CreationDate = DateTime.Now;
        }


        public bool IsAnyAttachments
        {
            get
            {
                if (Attachments.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public static MailInfo DeserializeFromXmlString(string info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MailInfo));
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(info)))
            {
                MailInfo result = serializer.Deserialize(stream) as MailInfo;
                return result;
            }
        }

        public static string SerializeVersionToXmlString(MailInfo info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MailInfo));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, info);
                return System.Text.Encoding.Default.GetString(stream.ToArray());
            }
        }

    }

    public static class MailSender
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void ScheduleMessage(string to, string subject, string body, List<string> attachments, TimeSpan interval, int paperId, int stateId)
        {
            string from = Tools.ConfigurationResourcesHelper.GetResource("options", "EmailNotificationAddress");
            string id = paperId + "_" + stateId + "_" + Guid.NewGuid().ToString();
            MailInfo mail = new MailInfo(subject, body, attachments, from, to, Convert.ToInt32(interval.TotalHours), paperId, stateId, id);
            string cronString = String.Empty;
            cronString = Cron.Daily(12);
            //cronString = Cron.Minutely();
            //not implemented interval
            RecurringJob.AddOrUpdate(id, () => MailSender.SendMail(mail), cronString);
            RecurringJob.Trigger(id);
        }

        public static void ScheduleMessage(MailInfo mail)
        {
            string from = Tools.ConfigurationResourcesHelper.GetResource("options", "EmailNotificationAddress");
            string id = mail.PaperId + "_" + mail.StateId + "_" + Guid.NewGuid().ToString();
            mail.Id = id;
            mail.From = from;
            string cronString = String.Empty;
            cronString = Cron.Daily(12);
            //cronString = Cron.Minutely();
            //not implemented interval
            //string serializedMail = MailInfo.SerializeVersionToXmlString(mail);
            RecurringJob.AddOrUpdate(id, () => MailSender.SendMail(mail), cronString);
            RecurringJob.Trigger(id);
        }

        public static void SendMail(string subject, string body, string to)
        {
            string from = Tools.ConfigurationResourcesHelper.GetResource("options", "EmailNotificationAddress");
            try
            {
                MailMessage message = new MailMessage(from, to)
                {
                    Subject = subject,
                    BodyEncoding = Encoding.UTF8,
                    Body = body,
                    IsBodyHtml = false,
                    SubjectEncoding = Encoding.UTF8
                };
                SmtpClient client = new SmtpClient();
                client.Send(message);
                logger.Info("Email sent to " + to);
            }
            catch (Exception ex)
            {
                logger.Error("Mail send exception " + ex.Message);
            }
        }

        public static void SendMail(string mail)
        {
            MailInfo info = MailInfo.DeserializeFromXmlString(mail);
            string from = info.From;
            try
            {
                MailMessage message = new MailMessage(info.From, info.To)
                {
                    Subject = info.Subject,
                    BodyEncoding = Encoding.UTF8,
                    Body = info.Body,
                    IsBodyHtml = false,
                    SubjectEncoding = Encoding.UTF8
                };
                SmtpClient client = new SmtpClient();
                client.Send(message);
                logger.Info("Email sent to " + info.To);
            }
            catch (Exception ex)
            {
                logger.Error("Mail send exception " + ex.Message);
            }
        }

        public static void SendMail(MailInfo info)
        {
            string from = info.From;
            try
            {
                MailMessage message = new MailMessage(info.From, info.To)
                {
                    Subject = info.Subject,
                    BodyEncoding = Encoding.UTF8,
                    Body = info.Body,
                    IsBodyHtml = false,
                    SubjectEncoding = Encoding.UTF8
                };
                SmtpClient client = new SmtpClient();
                client.Send(message);
                logger.Info("Email sent to " + info.To);
            }
            catch (Exception ex)
            {
                logger.Error("Mail send exception " + ex.Message);
            }
        }

        public static void SendMail(string subject, string body, string from, string to)
        {
            try
            {
                MailMessage message = new MailMessage(from, to)
                {
                    Subject = subject,
                    BodyEncoding = Encoding.UTF8,
                    Body = body,
                    IsBodyHtml = false,
                    SubjectEncoding = Encoding.UTF8
                };
                SmtpClient client = new SmtpClient();
                client.Send(message);
                logger.Info("Email sent to " + to);
            }
            catch (Exception ex)
            {
                logger.Error("Mail send exception" + ex.Message);
            }
        }

        public static void SendMailOnMailingList(List<string[]> mailingList, string subject, string body, List<string> attachments)
        {
            string from = Tools.ConfigurationResourcesHelper.GetResource("options", "EmailNotificationAddress");
            foreach (string[] recipient in mailingList)
            {
                try
                {
                    MailMessage message = new MailMessage(from, recipient[0])
                    {
                        Subject = subject,
                        BodyEncoding = Encoding.UTF8,
                        Body = recipient[1] + "\r\n" + body,
                        IsBodyHtml = false,
                        SubjectEncoding = Encoding.UTF8,
                    };

                    foreach (string path in attachments)
                    {
                        message.Attachments.Add(new Attachment(path));
                    }
                    SmtpClient client = new SmtpClient();
                    client.Send(message);
                    logger.Info("Email sent to " + recipient[0]);
                }
                catch (Exception ex)
                {
                    logger.Error("Mail send exception" + ex.Message);
                }
            }
        }
    }
}