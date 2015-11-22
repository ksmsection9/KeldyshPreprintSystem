using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using KeldyshPreprintSystem.Models;
using WebMatrix.WebData;

namespace KeldyshPreprintSystem.Controllers
{
    public class AccountController : Controller
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login"), HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "/PaperSubmission/Create", bool afterRegistration=false, bool confirmationSuccess=false, bool confirmationFailure=false)
        {
            if (User.Identity.IsAuthenticated)
            {
                return LogOut();
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AfterRegistration = afterRegistration;
            ViewBag.AfterConfirmationSuccess = confirmationSuccess;
            ViewBag.AfterConfirmationFailure = confirmationFailure;
            return View();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#"), HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl = "/PaperSubmission/Create")
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (WebSecurity.IsConfirmed(model.Email))
                    {
                        if (WebSecurity.Login(model.Email, model.Password, true))
                        {
                            string role = KeldyshPreprintSystem.Tools.AccountHelper.GetUserRole(WebSecurity.GetUserId(model.Email));//currentuserid doesnt work, it is always -1
                            logger.Info("Successful login: " + model.Email+" "+role);
                            if (role == "user" && !Tools.PaperSubmissionControllerHelper.GotAnySubmissions(model.Email))
                                return RedirectToLocal(returnUrl);
                            else
                                return RedirectToLocal("/Panel");
                        }
                        else
                            ModelState.AddModelError("", "Неправильная пара логин/пароль.");
                    }
                    else
                        if (WebSecurity.UserExists(model.Email))
                        {
                            string confirmationToken = Tools.AccountHelper.GetConfirmationToken(model.Email);
                            logger.Info("ConfirmationURL: " + "https://" + Request.Url.Host + "/Account/RegisterConfirmation?token=" + confirmationToken);
                            Tools.MailSender.SendMail("Подтверждение регистрации", "Для подтверждения своей регистрации в качестве автора, ответственного за издание препринта, перейдите по ссылке (актуальна в течение 60 минут): https://" + Request.Url.Host + "/Account/RegisterConfirmation?token=" + confirmationToken, model.Email);
                            ModelState.AddModelError("", "Почтовый адрес не подтвержден. Вход в систему невозможен до подтверждения адреса электронной почты. Повторное письмо с ссылкой для подтверждения было отправлено на адрес электронной почты " + model.Email);
                        }
                        else
                            ModelState.AddModelError("", "Неправильная пара логин/пароль.");
                    logger.Info("Unsuccessful login");
                }
                catch (Exception ex)
                {
                    logger.Error("login error: " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    try
                    {
                        string confirmationToken = WebSecurity.CreateUserAndAccount(model.Email, model.Password, new { Password = "NoPlainTextPasswords", FullName = model.FullName }, requireConfirmationToken: true);
                        ((SimpleRoleProvider)Roles.Provider).AddUsersToRoles(new[] { model.Email }, new[] { "user" });
                        logger.Info("ConfirmationURL: " + "https://" + Request.Url.Host + "/Account/RegisterConfirmation?token=" + confirmationToken);
                        Tools.MailSender.SendMail("Подтверждение регистрации", "Для подтверждения своей регистрации в качестве автора, ответственного за издание препринта, перейдите по ссылке: https://" + Request.Url.Host + "/Account/RegisterConfirmation?token=" + confirmationToken, model.Email);
                        return RedirectToAction("Login", "Account", new {afterRegistration=true});
                    }
                    catch (MembershipCreateUserException e)
                    {
                        ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                        logger.Error("Registration failed " + e.Message);
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return File(System.Text.ASCIIEncoding.Default.GetBytes(ex.Message), "text");
            }
        }

        [AllowAnonymous]
        public ActionResult RegisterSuccess(LoginModel model)
        {
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(string email)
        {
            try
            {
                if (WebSecurity.UserExists(email))
                {
                    string token = WebSecurity.GeneratePasswordResetToken(email, 60);
                    //sendmail
                    logger.Info("ResetPassURL: " + "https://" + Request.Url.Host + "/Account/ChangePassword?token=" + token);
                    Tools.MailSender.SendMail("Сброс пароля", "Для сброса пароля перейдите по следующей ссылке: https://" + Request.Url.Host + "/Account/ChangePassword?token=" + token, email);
                    return File(System.Text.ASCIIEncoding.Default.GetBytes("Письмо со ссылкой для сброса пароля было отправлено на указанный адрес электронной почты."), "text");
                }
                else
                    return File(System.Text.ASCIIEncoding.Default.GetBytes("Пользователь не найден."), "text");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return RedirectToAction("Login");
            }
        }


        [AllowAnonymous]
        public ActionResult ChangePassword(string token)
        {
            try
            {
                ViewBag.Token = token;
                return View();
            }
            catch (MembershipCreateUserException e)
            {
                ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                logger.Error("Password reset failed " + e.Message);
                return RedirectToAction("Login", "Account");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ChangePassword(string token, string password)
        {
            try
            {
                WebSecurity.ResetPassword(token, password);
                return RedirectToAction("Login", "Account");
            }
            catch (MembershipCreateUserException e)
            {
                ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                logger.Error("Password reset failed " + e.Message);
                return RedirectToAction("Login", "Account");
            }
        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string token)
        {
            try
            {
                if (WebSecurity.ConfirmAccount(token))
                {
                    //return RedirectToAction("ConfirmationSuccess");
                    return RedirectToAction("Login", "Account", new { confirmationSuccess = true });
                }
                //return RedirectToAction("ConfirmationFailure");
                return RedirectToAction("Login", "Account", new { confirmationFailure = true });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        public ActionResult ConfirmationSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return View();
        }



        private ActionResult RedirectToLocal(string returnUrl)
        {
            //if (Url.IsLocalUrl(returnUrl))
            //{
                return Redirect(returnUrl);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Home");
            //}
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Уже существует пользовать с таким логином. Пожалуйста, введите другой адрес электронной почты или воспользуйтесь сбросом пароля.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Уже существует пользовать с таким логином. Пожалуйста, введите другой адрес электронной почты или воспользуйтесь сбросом пароля.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Неправильный пароль. Пожалуйста, введите правильный пароль.";

                case MembershipCreateStatus.InvalidEmail:
                    return "Неверный адрес электронной почты. Пожалуйста, введите корректный адрес электронной почты.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "Некорректное имя пользователя. Пожалуйста, введите адрес вашей электронной почты.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "Неизвестная ошибка, обратитесь к администратору.";
            }
        }

    }
}