using System.Web;
using System.Web.Mvc;

namespace KeldyshPreprintSystem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireHttpsAttribute());
            filters.Add(new AuthorizeAttribute());
            var filter = new KeldyshPreprintSystem.Filter.InitializeSimpleMembershipAttribute();
            filter.OnActionExecuting(null);
            filters.Add(filter);
        }
    }
}