using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using Hangfire.Storage;

namespace KeldyshPreprintSystem.Tools
{
    public static class ScheduleHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void CleanRecurringJobs(int paperId)
        {
            var jobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var job in jobs)
            {
                string[] ids = job.Id.Split('_');//0-paperId 1-stateId 2- GUID
                if (ids[0] == paperId.ToString())
                {
                    RecurringJob.RemoveIfExists(job.Id);
                    logger.Info(job.Id + " was removed");
                }
            }
        }
    }
}
