using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(KeldyshPreprintSystem.Startup))]

namespace KeldyshPreprintSystem
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfire(config =>
            {
                config.UseSqlServerStorage("ScheduleDB");
                config.UseServer();
                config.UseAuthorizationFilters(new[] { new MyRestrictiveAuthorizationFilter() });
                config.UseDashboardPath("/hangfire");
            });
        }
    }

    public class MyRestrictiveAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(System.Collections.Generic.IDictionary<string, object> owinEnvironment)
        {
            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return WebMatrix.WebData.WebSecurity.IsAuthenticated;
        }
    }
}
