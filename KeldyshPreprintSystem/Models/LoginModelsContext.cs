using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace KeldyshPreprintSystem.Models
{
    public class LoginModelsContext : DbContext
    {
        public LoginModelsContext(): base("LoginDB")
        {

        }

        public DbSet<LoginModel> LoginModels { get; set; }
    }
}