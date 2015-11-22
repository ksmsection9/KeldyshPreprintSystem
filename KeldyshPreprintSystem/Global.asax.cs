using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;
using NLog.Config;

namespace KeldyshPreprintSystem
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            logger.Info("Application Start");
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //ModelBinders.Binders.Add(typeof(string), new TrimModelBinder());
        }

        protected void Application_Error()
        {
            logger.Info("Application Error");
        }


        protected void Application_End()
        {
            logger.Info("Application End");
        }
    }

    //public class TrimModelBinder : IModelBinder
    //{
    //    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    //    {
    //        var shouldPerformRequestValidation = controllerContext.Controller.ValidateRequest && bindingContext.ModelMetadata.RequestValidationEnabled;
    //        var unvalidatedValueProvider = bindingContext.ValueProvider as IUnvalidatedValueProvider;

    //        ValueProviderResult valueResult = null;
    //        if (unvalidatedValueProvider != null)
    //            valueResult = unvalidatedValueProvider.GetValue(bindingContext.ModelName, !shouldPerformRequestValidation);
    //        else
    //            valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

    //        if (valueResult == null || string.IsNullOrEmpty(valueResult.AttemptedValue))
    //            return null;
    //        return valueResult.AttemptedValue.Trim();
    //    }
    //}
}