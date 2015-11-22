using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Hangfire;
using System.Xml.Serialization;
using System.IO;


namespace KeldyshPreprintSystem.Tools
{
    //public class DailySendSMSJob : IJob
    //{
    //    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    //    private static Object locker = new Object();

    //    public static void PrepareSchedule()
    //    {
    //        CleanSms();
    //        foreach (var sms in GetScheduledSms())
    //        {
    //            // construct job info
    //            //var builderSms = Quartz.JobBuilder.Create(typeof(Tools.DailySendSMSJob)).WithIdentity("SmsJob", "group1");
    //            //var trigBuilderSms = TriggerBuilder.Create().WithIdentity("dailyTriggerSms", "group1").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever());
    //            //sched.ScheduleJob(builderSms.Build(), trigBuilderSms.Build());

    //            ////sms
    //            if (!KeldyshPreprintSystem.MvcApplication.Sched.CheckExists(new JobKey(sms.Id, "groupsms")))
    //            {
    //                logger.Info("new job " + sms.PaperId + ":" + sms.StateId);
    //                var builderSms = Quartz.JobBuilder.Create(typeof(Tools.DailySendEmailJob)).WithIdentity(sms.Id, "groupsms");
    //                //.StartAt(DateTime.UtcNow+new TimeSpan(mail.HourInterval, 0, 0))
    //                var trigBuilderSms = TriggerBuilder.Create().WithIdentity("dailyTriggerSms_" + sms.Id, "groupsms").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(sms.HourInterval).RepeatForever());
    //                // schedule the job for execution
    //                KeldyshPreprintSystem.MvcApplication.Sched.ScheduleJob(builderSms.Build(), trigBuilderSms.Build());
    //            }
    //        }
    //    }

    //    private static List<SmsInfo> GetScheduledSms()
    //    {
    //        lock (locker)
    //        {
    //            List<SmsInfo> result = new List<SmsInfo>();
    //            string[] messages = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/"));
    //            foreach (string sms in messages)
    //            {
    //                SmsInfo info = SmsInfo.DeserializeFromXmlString(File.ReadAllText(sms));
    //                result.Add(info);
    //            }
    //            return result;
    //        }
    //    }

    //    private static int CleanSms()
    //    {
    //        lock (locker)
    //        {
    //            int result = 0;
    //            string[] messages = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/"));
    //            foreach (string sms in messages)
    //            {
    //                SmsInfo info = SmsInfo.DeserializeFromXmlString(File.ReadAllText(sms));
    //                if (info.IsClosed)
    //                {
    //                    File.Delete(sms);
    //                    result++;
    //                }
    //            }
    //            return result;
    //        }
    //    }

    //    public static int CleanMails(int paperId, int stateId)
    //    {
    //        lock (locker)
    //        {
    //            int result = 0;
    //            string[] messages = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/"));
    //            foreach (string sms in messages)
    //            {
    //               SmsInfo info =SmsInfo.DeserializeFromXmlString(File.ReadAllText(sms));
    //                if (info.PaperId == paperId && info.StateId == stateId)
    //                {
    //                    File.Delete(sms);
    //                    result++;
    //                }
    //            }
    //            return result;
    //        }
    //    }

    //    public static int CleanMails(int paperId)
    //    {
    //        lock (locker)
    //        {
    //            int result = 0;
    //            string[] messages = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/"));
    //            foreach (string sms in messages)
    //            {
    //                SmsInfo info = SmsInfo.DeserializeFromXmlString(File.ReadAllText(sms));
    //                if (info.PaperId == paperId)
    //                {
    //                    File.Delete(sms);
    //                    result++;
    //                }
    //            }
    //            return result;
    //        }
    //    }

    //    public static SmsInfo GetMailById(string id)
    //    {
    //        string xml = File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/" + id + ".xml"));
    //        return SmsInfo.DeserializeFromXmlString(xml);
    //    }

    //    public static void AddMessage(string to, string body, int interval, int paperId, int stateId)
    //    {
    //        string id = paperId + "_" + stateId + "_" + Guid.NewGuid().ToString();
    //        SmsInfo sms = new SmsInfo(body, to, interval, paperId, stateId, id);
    //        File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/" + id + ".xml"), SmsInfo.SerializeVersionToXmlString(sms));
    //        //sms
    //        var builderSms = Quartz.JobBuilder.Create(typeof(Tools.DailySendEmailJob)).WithIdentity(id, "groupsms");
    //        //.StartAt(DateTime.UtcNow+new TimeSpan(mail.HourInterval, 0, 0))
    //        var trigBuilderSms = TriggerBuilder.Create().WithIdentity("dailyTriggerSms_" + id, "groupsms").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(sms.HourInterval).RepeatForever());
    //        // schedule the job for execution
    //        KeldyshPreprintSystem.MvcApplication.Sched.ScheduleJob(builderSms.Build(), trigBuilderSms.Build());
    //    }

    //    public static void AddMessage(SmsInfo sms)
    //    {
    //        string id = sms.PaperId + "_" + sms.StateId + "_" + Guid.NewGuid().ToString();
    //        File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/sms/" + id + ".xml"), SmsInfo.SerializeVersionToXmlString(sms));
    //        //sms
    //        var builderSms = Quartz.JobBuilder.Create(typeof(Tools.DailySendEmailJob)).WithIdentity(id, "groupsms");
    //        //.StartAt(DateTime.UtcNow+new TimeSpan(mail.HourInterval, 0, 0))
    //        var trigBuilderSms = TriggerBuilder.Create().WithIdentity("dailyTriggerSms_" + id, "groupsms").StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(sms.HourInterval).RepeatForever());
    //        // schedule the job for execution
    //        KeldyshPreprintSystem.MvcApplication.Sched.ScheduleJob(builderSms.Build(), trigBuilderSms.Build());
    //    }

    //    public void Execute(IJobExecutionContext context)
    //    {
    //        CleanSms();
    //        var smsToSend = GetScheduledSms();
    //        string id = context.JobDetail.Key.Name;
    //        for (int i = 0; i < smsToSend.Count; i++)
    //            if (smsToSend[i].Id == id)
    //                SMSSender.SendSms(smsToSend[i]);
    //    }
    //}

    public class SMSResponse
    {
        public string ID { get; set; }
        public string Message { get; set; }
    }

    public class SmsInfo
    {
        public string Body { get; set; }
        public string To { get; set; }
        public int HourInterval { get; set; }
        public bool IsClosed { get; set; }
        public int PaperId { get; set; }
        public int StateId { get; set; }
        public DateTime CreationDate { get; set; }
        public string Id { get; set; }



        public SmsInfo()
        {

        }

        public SmsInfo(string body, string to, int interval, int paperId, int stateId, string id)
        {
            Body = body;
            To = to;
            HourInterval = interval;
            PaperId = paperId;
            StateId = stateId;
            Id = id;
            CreationDate = DateTime.Now;
        }



        public static SmsInfo DeserializeFromXmlString(string info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SmsInfo));
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(info)))
            {
                SmsInfo result = serializer.Deserialize(stream) as SmsInfo;
                return result;
            }
        }

        public static string SerializeVersionToXmlString(SmsInfo info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SmsInfo));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, info);
                return System.Text.Encoding.Default.GetString(stream.ToArray());
            }
        }
    }

    public static class SMSSender
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static string user = Tools.ConfigurationResourcesHelper.GetResource("options", "SMSAccount");
        static string password = Tools.ConfigurationResourcesHelper.GetResource("options", "SMSPassword");
        static string sign = Tools.ConfigurationResourcesHelper.GetResource("options", "SMSSign");



        public static void ScheduleMessage(string to, string body, TimeSpan interval, int paperId, int stateId)
        {
            string id = paperId + "_" + stateId + "_" + Guid.NewGuid().ToString();
            SmsInfo sms = new SmsInfo(body, to, Convert.ToInt32(interval.TotalHours), paperId, stateId, id);
            string cronString = String.Empty;
            cronString = Cron.Daily(15);
            RecurringJob.AddOrUpdate(id, () => SMSSender.SendSms(sms), cronString);
        }

        public static void ScheduleMessage(SmsInfo sms)
        {
            string id = sms.PaperId + "_" + sms.StateId + "_" + Guid.NewGuid().ToString();
            sms.Id = id;
            string cronString = String.Empty;
            cronString = Cron.Daily(15);
            RecurringJob.AddOrUpdate(id, () => SMSSender.SendSms(sms), cronString);
        }


        public static SMSResponse SendSms(string message, string phone)
        {
            phone = phone.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
            try
            {
                Regex responseParse = new Regex("([0-9])*=(.)*");
                using (WebClient client = new WebClient())
                {
                    Uri url = new Uri("https://gate.smsaero.ru/send/?user=" + user + "&password=" + password + "&to=" + phone + "&text=" + message + "&from=" + sign);
                    string response = client.DownloadString(url);
                    Match m = responseParse.Match(response);
                    if (m.Success)
                    {
                        string id = m.Groups[0].Value;
                        string responseMessage = m.Groups[1].Value;
                        SMSResponse result = new SMSResponse();
                        result.ID = id;
                        result.Message = responseMessage;
                        logger.Info("SMS sent to " + phone + " message: " + message);
                        return result;
                    }
                    else
                        throw new InvalidOperationException("Unrecognized sms service response: " + response);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Sms sending error: " + ex.Message + " number: " + phone + " message: " + message);
                return null;
            }
        }

        public static SMSResponse SendSms(SmsInfo info)
        {
            string to = info.To.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
            try
            {
                Regex responseParse = new Regex("([0-9])*=(.)*");
                using (WebClient client = new WebClient())
                {
                    Uri url = new Uri("https://gate.smsaero.ru/send/?user=" + user + "&password=" + password + "&to=" + to + "&text=" + info.Body + "&from=" + sign);
                    string response = client.DownloadString(url);
                    Match m = responseParse.Match(response);
                    if (m.Success)
                    {
                        string id = m.Groups[0].Value;
                        string responseMessage = m.Groups[1].Value;
                        SMSResponse result = new SMSResponse();
                        result.ID = id;
                        result.Message = responseMessage;
                        logger.Info("SMS sent to " + to + " message: " + info.Body);
                        return result;
                    }
                    else
                        throw new InvalidOperationException("Unrecognized sms service response: " + response);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Sms sending error: " + ex.Message + " number: " + to + " message: " + info.Body);
                return null;
            }
        }
    }
}