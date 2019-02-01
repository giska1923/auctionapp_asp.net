using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Aukcija.Models;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace Aukcija.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your account has been changed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";

            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.Email = user.Email;
            ViewBag.TokenSum = user.TokenSum;
            return View();
        }

        static string HttpPost(string url, string[] paramName, string[] paramVal)
        {
            HttpWebRequest req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            // Build a string with all the params, properly encoded.
            // We assume that the arrays paramName and paramVal are
            // of equal length:
            StringBuilder paramz = new StringBuilder();
            for (int i = 0; i < paramName.Length; i++)
            {
                paramz.Append(paramName[i]);
                paramz.Append("=");
                paramz.Append(HttpUtility.UrlEncode(paramVal[i]));
                paramz.Append("&");
            }

            // Encode the parameters as form data:
            byte[] formData =
                UTF8Encoding.UTF8.GetBytes(paramz.ToString());
            req.ContentLength = formData.Length;

            // Send the request:
            using (Stream post = req.GetRequestStream())
            {
                post.Write(formData, 0, formData.Length);
            }

            // Pick up the response:
            string result = null;
            using (HttpWebResponse resp = req.GetResponse()
                                          as HttpWebResponse)
            {
                StreamReader reader =
                    new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }

            return result;
        }

        static string HttpPost2(string url, string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Host = "api.centili.com:443";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            byte[] postBytes = System.Text.UTF8Encoding.UTF8.GetBytes(json);
            httpWebRequest.ContentLength = postBytes.Length;

            using (Stream streamWriter = httpWebRequest.GetRequestStream())
            {
                streamWriter.Write(postBytes, 0, postBytes.Length);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string result;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            var obj = new JavaScriptSerializer().Deserialize<object>(result);
            return "";
        }

        static string HttpGet(string url)
        {
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            string result = null;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }
            return result;
        }

        //Treba da se napravi View
        [Authorize(Roles = "User")]
        public ActionResult BuyTokens(string package)
        {
            TokenOrder order = null;
            using (Entities db = new Entities())
            {
                string userid = User.Identity.GetUserId();
                var token = db.Token.FirstOrDefault();
                int tokenNumber = 0;
                switch (package)
                {
                    case "silver":
                        tokenNumber = (int)token.NumS;
                        break;
                    case "gold":
                        tokenNumber = (int)token.NumG;
                        break;
                    case "premium":
                        tokenNumber = (int)token.NumP;
                        break;
                    default:
                        return RedirectToAction("Index");
                }
                decimal price = 0;
                switch (token.ActiveCurrency)
                {
                    case "RSD":
                        price = tokenNumber * (decimal)token.RSD;
                        break;
                    case "USD":
                        price = tokenNumber * (decimal)token.USD;
                        break;
                    case "EUR":
                        price = tokenNumber * (decimal)token.EUR;
                        break;
                }
                order = new TokenOrder()
                {
                    GUID = Guid.NewGuid(),
                    IdUser = userid,
                    TokenNum = tokenNumber,
                    Status = "SU",
                    PackageP = price
                };
                db.TokenOrder.Add(order);
                db.SaveChanges();
            }
            string url = "https://stage.centili.com/widget/WidgetModule?api=589838e3b63aa93505034f3563b32c42&clientid="
                + order.GUID.ToString() + "&amount=" + ((decimal)(order.PackageP)).ToString("0.##")
                + "&country=rs&theme=black";
            return Redirect(url);
            //string[] paramsn =
            //{
            //    "apikey", "country", "reference", "price", "returnurl", "theme"
            //};
            //string[] vals =
            //{
            //    "589838e3b63aa93505034f3563b32c42", "rs", order.GUID.ToString(), ((decimal)(order.PackageP)).ToString("0.##"), "http://localhost:50065", "black"
            //};
            //string url = "http://api.centili.com/api/payment/1_4/transaction";
            //object jsonobj = new { apikey = "589838e3b63aa93505034f3563b32c42", msisdn = "381658203636", country = "rs", reference  = order.GUID.ToString(), price = ((decimal)(order.PackageP)).ToString("0.##"), returnurl = "http://localhost:50065", theme = "black"};
            //string json = new JavaScriptSerializer().Serialize(jsonobj);
            //HttpPost2(url, json);
            //return RedirectToAction("Home", "Index", null);
        }

        public ActionResult NotificationManager(string service, string transactionid, string phone, decimal? enduserprice, string status, string clientid, string errormessage, string country, string mno, string mnocode, decimal? revenue, int? amount, string revenuecurrency)
        {
            int a = 2;
            a++;
            return RedirectToAction("Home", "Index", null);
        }

        public ActionResult BuyTokensRedir(string status, string clientid)
        {
            using (Entities db = new Entities())
            {
                var guid = new Guid(clientid);
                var order = db.TokenOrder.Where(x => x.GUID.Equals(guid)).FirstOrDefault();
                switch (status)
                {
                    case "success":
                        order.Status = "CO";
                        break;
                    case "failed":
                        order.Status = "CA";
                        break;
                }
                var user = db.AspNetUsers.Where(x => x.Id == order.IdUser).FirstOrDefault();
                user.TokenSum += order.TokenNum;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Home", null);
        }

        //Treba da se napravi View
        [Authorize(Roles = "User")]
        public ActionResult ListTokenOrder()
        {
            List<TokenOrder> list = null;
            using (Entities db = new Entities())
            {
                string userid = User.Identity.GetUserId();
                var order = db.TokenOrder.Where(x => x.IdUser == userid);
                list = order.ToList();
            }
            return View(list);
        }

        // GET: /Manage/Index
        //public async Task<ActionResult> Index(ManageMessageId? message)
        //{
        //    ViewBag.StatusMessage =
        //        message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
        //        : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
        //        : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
        //        : message == ManageMessageId.Error ? "An error has occurred."
        //        : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
        //        : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
        //        : "";
        //
        //    var userId = User.Identity.GetUserId();
        //    var model = new IndexViewModel
        //    {
        //        HasPassword = HasPassword(),
        //        PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
        //        TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
        //        Logins = await UserManager.GetLoginsAsync(userId),
        //        BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
        //    };
        //    return View(model);
        //}

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        public ActionResult EditAccount()
        {
            var id = User.Identity.GetUserId();
            using (var db = new Entities())
            {
                AspNetUser user = db.AspNetUsers.Where(u => u.Id == id)
                    .FirstOrDefault();
                ViewBag.Email = user.Email;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAccount(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            if (model.changePassword && model.OldPassword != null && model.NewPassword != null)
            {
                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                }
                else
                {
                    return RedirectToAction("Index", new { Message = ManageMessageId.Error });
                }
            }

            var id = User.Identity.GetUserId();
            using (var db = new Entities()) 
            {
                foreach(var user in db.AspNetUsers.Where(u => u.Id == id))
                {
                    ViewBag.FirstName = user.FirstName = model.FirstName;
                    ViewBag.LastName = user.LastName = model.LastName;
                    ViewBag.Email = user.Email = model.Email;
                    user.UserName = model.Email;
                }
                db.SaveChanges();
            }

            var user2 = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user2 != null)
            {
                await SignInManager.SignInAsync(user2, isPersistent: false, rememberBrowser: false);
            }
            ModelState.Clear();
            return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}