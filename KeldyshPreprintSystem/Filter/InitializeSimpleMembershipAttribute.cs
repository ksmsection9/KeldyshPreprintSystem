using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using KeldyshPreprintSystem.Models;
using System.Web.Security;

namespace KeldyshPreprintSystem.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<LoginModelsContext>(null);
                try
                {
                    using (var context = new LoginModelsContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }
                    WebSecurity.InitializeDatabaseConnection("LoginDB", "LoginModels", "UserId", "Email", autoCreateTables: true);
                    InitializeDefault();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }

            private void InitializeDefault()
            {
                var roles = (SimpleRoleProvider)Roles.Provider;
                var membership = (SimpleMembershipProvider)Membership.Provider;

                if (!roles.RoleExists("admin"))
                {
                    roles.CreateRole("admin");
                }

                //if (!roles.RoleExists("editor"))
                //{
                //    roles.CreateRole("editor");
                //}

                if (!roles.RoleExists("corrector"))
                {
                    roles.CreateRole("corrector");
                }

                if (!roles.RoleExists("user"))
                {
                    roles.CreateRole("user");
                }

                if (!roles.RoleExists("internetguy"))
                {
                    roles.CreateRole("internetguy");
                }

                if (!roles.RoleExists("typographer"))
                {
                    roles.CreateRole("typographer");
                }

                if (membership.GetUser("admin@localhost.localhost", false) == null)
                {
                    WebSecurity.CreateUserAndAccount("admin@localhost.localhost", "superadmin", new { Password = "NoPlainTextPasswords", FullName = "" }, false);
                    roles.AddUsersToRoles(new[] { "admin@localhost.localhost" }, new[] { "admin" });
                }

                if (membership.GetUser("author@localhost.localhost", false) == null)
                {
                    WebSecurity.CreateUserAndAccount("author@localhost.localhost", "superauthor", new { Password = "NoPlainTextPasswords", FullName = "" }, false);
                    roles.AddUsersToRoles(new[] { "author@localhost.localhost" }, new[] { "user" });
                }

                if (membership.GetUser("corrector@localhost.localhost", false) == null)
                {
                    WebSecurity.CreateUserAndAccount("corrector@localhost.localhost", "supercorrector", new { Password = "NoPlainTextPasswords", FullName = "" }, false);
                    roles.AddUsersToRoles(new[] { "corrector@localhost.localhost" }, new[] { "corrector" });
                }

                if (membership.GetUser("internetguy@localhost.localhost", false) == null)
                {
                    WebSecurity.CreateUserAndAccount("internetguy@localhost.localhost", "superinternetguy", new { Password = "NoPlainTextPasswords", FullName = "" }, false);
                    roles.AddUsersToRoles(new[] { "internetguy@localhost.localhost" }, new[] { "internetguy" });
                }

                if (membership.GetUser("typographer@localhost.localhost", false) == null)
                {
                    WebSecurity.CreateUserAndAccount("typographer@localhost.localhost", "supertypographer", new { Password = "NoPlainTextPasswords", FullName = "" }, false);
                    roles.AddUsersToRoles(new[] { "typographer@localhost.localhost" }, new[] { "typographer" });
                }
            }
        }
    }
}