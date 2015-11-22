using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using KeldyshPreprintSystem.Models;
using WebMatrix.WebData;

namespace KeldyshPreprintSystem.Tools
{
    public static class AccountHelper
    {
        public static string GetUserRole(int userId)
        {
            using (var db = new LoginModelsContext())
            {
                try
                {
                    var tsqlQuery = string.Format("SELECT [RoleName] FROM [webpages_Roles] WHERE [RoleId] IN (SELECT [RoleId] FROM [webpages_UsersInRoles] WHERE [UserId] = {0})", userId.ToString());
                    return db.Database.SqlQuery<string>(tsqlQuery).FirstOrDefault();
                }
                catch { return "user"; }
            }
        }

        public static string GetUserName(int userId)
        {
            using (var db = new LoginModelsContext())
            {
                try
                {
                    var tsqlQuery = string.Format("SELECT [UserName] FROM [LoginModels] WHERE [UserId] = {0})", userId.ToString());
                    return db.Database.SqlQuery<string>(tsqlQuery).FirstOrDefault();
                }
                catch { return string.Empty; }
            }
        }

        public static ReadOnlyCollection<string> GetAllRoles()
        {
            using (var db = new LoginModelsContext())
            {
                try
                {
                    var tsqlQuery = string.Format("SELECT [RoleName] FROM [webpages_Roles]");
                    return new ReadOnlyCollection<string>(db.Database.SqlQuery<string>(tsqlQuery).ToList());
                }
                catch { return null; }
            }
        }

        public static string GetConfirmationToken(string email)
        {
            using (var db = new LoginModelsContext())
            {
                var tsqlQuery = string.Format("SELECT [ConfirmationToken] FROM [webpages_Membership] WHERE [UserId] IN (SELECT [UserId] FROM [LoginModels] WHERE [Email] LIKE '{0}')", email);
                return db.Database.SqlQuery<string>(tsqlQuery).First();
            }
        }

        public static void SetUserRole(string userName, string roleName)
        {
            var roles = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
            roles.RemoveUsersFromRoles(new string[] { userName }, roles.GetRolesForUser(userName));
            roles.AddUsersToRoles(new string[] { userName }, new string[] { roleName });
        }

        public static ReadOnlyCollection<string> GetUsersInRole(string role)
        {
            using (var db = new LoginModelsContext())
            {
                try
                {
                    var roleIdQuery = string.Format("SELECT [RoleId] FROM [webpages_Roles] WHERE [RoleName] LIKE '{0}'", role);
                    int roleId = db.Database.SqlQuery<int>(roleIdQuery).FirstOrDefault();
                    var tsqlQuery = string.Format("SELECT [Email] FROM [LoginModels] WHERE [UserId] IN (SELECT [UserId] FROM [webpages_UsersInRoles] WHERE [RoleId] = {0})", roleId);
                    return new ReadOnlyCollection<string>(db.Database.SqlQuery<string>(tsqlQuery).ToList());
                }
                catch { return null; }
            }
        }

        public static string GetRoleTranslation(string role)
        {
            switch (role)
            {
                case "user":
                    return "Автор";
                case "corrector":
                    return "Корректор";
                case "typographer":
                    return "Полиграфист";
                case "admin":
                    return "Редактор";
                case "internetguy":
                    return "Интернетчик";
                default:
                    return "Не определено";
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ReadOnlyCollection<string> GetAccounts()
        {
            using (var db = new LoginModelsContext())
            {
                var tsqlQuery = string.Format("SELECT [Email] FROM [LoginModels]");
                return new ReadOnlyCollection<string>(db.Database.SqlQuery<string>(tsqlQuery).ToList());
            }
        }

        public static bool SetNewAccountCredentials(string newUserName, string newPassWord)
        {
            string token = WebSecurity.GeneratePasswordResetToken(newUserName);
            return WebSecurity.ResetPassword(token, newPassWord);
        }
    }
}