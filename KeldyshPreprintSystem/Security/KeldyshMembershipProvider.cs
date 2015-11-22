using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;

//namespace KeldyshPreprintSystem.Security
//{
//    public class KeldyshMembershipProvider : MembershipProvider
//    {
//        #region Overrides of MembershipProvider

//        /// <summary>
//        /// Verifies that the specified user name and password exist in the data source.
//        /// </summary>
//        /// <returns>
//        /// true if the specified username and password are valid; otherwise, false.
//        /// </returns>
//        /// <param name="username">The name of the user to validate. </param><param name="password">The password for the specified user. </param>
//        public override bool ValidateUser(string username, string password)
//        {
//            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
//                return false;

//            //sql interaction
//            //string srtQry = @"SELECT table_name, column_name, data_type, data_length FROM USER_TAB_COLUMNS WHERE table_name = 'persons'";
//            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["KeldyshEmployees"].ConnectionString;
//            //using (SqlConnection conn = new SqlConnection(connectionString))
//            //{
//            //    using (SqlCommand objCommand = new SqlCommand(srtQry, conn))
//            //    {
//            //        objCommand.CommandType = CommandType.Text;
//            //        DataTable dt = new DataTable();
//            //        SqlDataAdapter adp = new SqlDataAdapter(objCommand);
//            //        conn.Open();
//            //        adp.Fill(dt);
//            //        if (dt != null)
//            //        {
//            //            dt.WriteXml(HttpContext.Current.Server.MapPath("~/App_Data/db.xml"));
//            //        }
//            //    }
//            //}
//            return true;
//        }

//        #endregion

//        #region Overrides of MembershipProvider that throw NotImplementedException

//        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
//        {
//            throw new NotImplementedException();
//        }

//        public override string GetPassword(string username, string answer)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool ChangePassword(string username, string oldPassword, string newPassword)
//        {
//            throw new NotImplementedException();
//        }

//        public override string ResetPassword(string username, string answer)
//        {
//            throw new NotImplementedException();
//        }

//        public override void UpdateUser(MembershipUser user)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool UnlockUser(string userName)
//        {
//            throw new NotImplementedException();
//        }

//        public override string GetUserNameByEmail(string email)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool DeleteUser(string username, bool deleteAllRelatedData)
//        {
//            throw new NotImplementedException();
//        }

//        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
//        {
//            throw new NotImplementedException();
//        }

//        public override int GetNumberOfUsersOnline()
//        {
//            throw new NotImplementedException();
//        }

//        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
//        {
//            throw new NotImplementedException();
//        }

//        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool EnablePasswordRetrieval
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override bool EnablePasswordReset
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override bool RequiresQuestionAndAnswer
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override string ApplicationName
//        {
//            get { throw new NotImplementedException(); }
//            set { throw new NotImplementedException(); }
//        }

//        public override int MaxInvalidPasswordAttempts
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override int PasswordAttemptWindow
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override bool RequiresUniqueEmail
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override MembershipPasswordFormat PasswordFormat
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override int MinRequiredPasswordLength
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override int MinRequiredNonAlphanumericCharacters
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override string PasswordStrengthRegularExpression
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
//        {
//            throw new NotImplementedException();
//        }

//        public override MembershipUser GetUser(string username, bool userIsOnline)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion
//    }
//}