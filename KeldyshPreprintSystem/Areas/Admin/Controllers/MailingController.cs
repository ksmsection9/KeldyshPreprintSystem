using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeldyshPreprintSystem.Tools;
using Hangfire;
using Hangfire.Storage;
namespace KeldyshPreprintSystem.Areas.Admin.Controllers
{
    public class MailingController : Controller
    {
        //
        // GET: /Admin/Mailing/

        public ActionResult Index()
        {
            List<string[]> jobs = new List<string[]>();
            var executingJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
            var recurringCount = Hangfire.JobStorage.Current.GetMonitoringApi().GetStatistics().Recurring;
            jobs.Add(new string[] { "Id", "LastExecution", "NextExecution", "Method", "LastJobState", "Cron" });
            jobs.Add(new string[] { recurringCount.ToString(), recurringCount.ToString(), recurringCount.ToString(), recurringCount.ToString(), recurringCount.ToString(), recurringCount.ToString() });
            //if (executingJobs.Count == 0)
            //    jobs.Add(new string[] { "Пусто", "Пусто", "Пусто", "Пусто", "Пусто", "Пусто" });
            foreach (var job in executingJobs)
            {
                jobs.Add(new string[] { job.Id, job.LastExecution.ToString(), job.NextExecution.ToString(), job.Job.Method.ToString(), job.LastJobState, job.Cron });
            }
            return View(jobs);
        }
    }
}
