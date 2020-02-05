﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Classes
{
    public class BaseController : ControllerExtension
    {

        #region Constructors

        public BaseController(
            ILogger logger,
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker) : base(cache, session)
        {
            _logger = logger;
            DataRepository = dataRepository;
            WebTracker = webTracker;
        }

        #endregion

        public string EmployerBackUrl
        {
            get => Session["EmployerBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("EmployerBackUrl");
                }
                else
                {
                    Session["EmployerBackUrl"] = value;
                }
            }
        }

        public string ReportBackUrl
        {
            get => Session["ReportBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("ReportBackUrl");
                }
                else
                {
                    Session["ReportBackUrl"] = value;
                }
            }
        }

        private void SaveHistory()
        {
            List<string> history = PageHistory;
            try
            {
                string previousPage = UrlReferrer == null || !RequestUrl.Host.Equals(UrlReferrer.Host) ? null : UrlReferrer.PathAndQuery;
                string currentPage = RequestUrl.PathAndQuery;

                int currentIndex = history.IndexOf(currentPage);
                int previousIndex = string.IsNullOrWhiteSpace(previousPage) ? -2 : history.IndexOf(previousPage);

                if (previousIndex == -2)
                {
                    history.Clear();
                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == -1 && previousIndex == 0)
                {
                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == -1)
                {
                    history.Clear();
                    if (previousIndex == -1)
                    {
                        history.Insert(0, previousPage);
                    }

                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == 0 && previousIndex == 1)
                {
                    return;
                }

                if (currentIndex > previousIndex)
                {
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        history.RemoveAt(i);
                    }
                }
            }
            finally
            {
                PageHistory = history;
            }
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            # region logic before action goes here

            //Pass the controller object into the ViewData 
            var controller = context.Controller as Controller;
            controller.ViewData["Controller"] = controller;

            if (Global.DisablePageCaching)
            {
                //Disable page caching
                context.HttpContext.DisableResponseCache();
            }

            #endregion

            await base.OnActionExecutionAsync(context, next); // the actual action

            #region logic after the action goes here

            //Save the history and action/controller names
            SaveHistory();

            LastAction = ActionName;
            LastController = ControllerName;

            #endregion
        }

        /// <summary>
        ///     returns true if previous action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        protected bool WasAction(string actionName, string controllerName = null, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                controllerName = ControllerName;
            }

            return !(UrlReferrer == null) && UrlReferrer.PathAndQuery.EqualsI(Url.Action(actionName, controllerName, routeValues));
        }

        protected bool WasAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                string actionUrl = actionUrls[i].TrimI(@" /\");
                string actionName = actionUrl.AfterFirst("/");
                string controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeparator: false);
                if (WasAction(actionName, controllerName))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool IsAction(string actionName, string controllerName = null)
        {
            return actionName.EqualsI(ActionName) && (controllerName.EqualsI(ControllerName) || string.IsNullOrWhiteSpace(controllerName));
        }

        protected bool IsAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                string actionUrl = actionUrls[i].TrimI(@" /\");
                string actionName = actionUrl.AfterFirst("/");
                string controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeparator: false);
                if (IsAction(actionName, controllerName))
                {
                    return true;
                }
            }

            return false;
        }

        [NonAction]
        public IActionResult LogoutUser(string redirectUrl = null)
        {
            //If impersonating then stop
            if (ImpersonatedUserId > 0)
            {
                ImpersonatedUserId = 0;
                OriginalUser = null;
                return new RedirectToActionResult(nameof(OrganisationController.ManageOrganisations), "Organisation", null);
            }

            //otherwise actually logout
            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return SignOut("Cookies", "oidc");
            }

            var properties = new AuthenticationProperties {RedirectUri = redirectUrl};
            return SignOut(properties, "Cookies", "oidc");
        }


        protected async Task IncrementRetryCountAsync(string retryLockKey, int expiryMinutes)
        {
            int count = await Cache.GetAsync<int>($"{UserHostAddress}:{retryLockKey}:Count");
            count++;
            if (count >= 3)
            {
                await CreateRetryLockAsync(retryLockKey, expiryMinutes);
            }

            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}:Count", count, VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        protected async Task CreateRetryLockAsync(string retryLockKey, int expiryMinutes)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}", VirtualDateTime.Now, VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        protected async Task<TimeSpan> GetRetryLockRemainingTimeAsync(string retryLockKey, int expiryMinutes)
        {
            if (Global.SkipSpamProtection)
            {
                return TimeSpan.Zero;
            }

            DateTime lockDate = await Cache.GetAsync<DateTime>($"{UserHostAddress}:{retryLockKey}");
            TimeSpan remainingTime =
                lockDate == DateTime.MinValue ? TimeSpan.Zero : lockDate.AddMinutes(expiryMinutes) - VirtualDateTime.Now;
            return remainingTime;
        }

        protected async Task ClearRetryLocksAsync(string retryLockKey)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
        }
        
        #region Dependencies

        protected ILogger _logger;

        public IDataRepository DataRepository { get; protected set; }
        public IWebTracker WebTracker { get; }

        #endregion

        #region Properties

        public long ReportingOrganisationId
        {
            get => Session["ReportingOrganisationId"].ToInt64();
            set
            {
                _ReportingOrganisation = null;
                ReportingOrganisationStartYear = null;
                Session["ReportingOrganisationId"] = value;
            }
        }

        public int? ReportingOrganisationStartYear
        {
            get => Session["ReportingOrganisationReportStartYear"].ToInt32();
            set => Session["ReportingOrganisationReportStartYear"] = value;
        }

        public string PendingFasttrackCodes
        {
            get => (string) Session["PendingFasttrackCodes"];
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("PendingFasttrackCodes");
                }
                else
                {
                    Session["PendingFasttrackCodes"] = value;
                }
            }
        }

        private Organisation _ReportingOrganisation;

        public Organisation ReportingOrganisation
        {
            get
            {
                if (_ReportingOrganisation == null && ReportingOrganisationId > 0)
                {
                    _ReportingOrganisation = DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.OrganisationId == ReportingOrganisationId);
                }

                return _ReportingOrganisation;
            }
            set
            {
                _ReportingOrganisation = value;
                ReportingOrganisationId = value == null ? 0 : value.OrganisationId;
            }
        }

        public virtual User CurrentUser => VirtualUser;

        public bool IsTrustedIP =>
            string.IsNullOrWhiteSpace(Global.TrustedIPDomains) || UserHostAddress.IsTrustedAddress(Global.TrustedIPDomains.SplitI());

        public bool IsAdministrator => CurrentUser.IsAdministrator();
        public bool IsSuperAdministrator => IsTrustedIP && CurrentUser.IsSuperAdministrator();
        public bool IsDatabaseAdministrator => IsTrustedIP && CurrentUser.IsDatabaseAdministrator();

        public bool IsTestUser => CurrentUser.EmailAddress.StartsWithI(Global.TestPrefix);
        public bool IsImpersonatingUser => OriginalUser != null && OriginalUser.IsAdministrator();

        protected User VirtualUser =>
            User.Identity.IsAuthenticated
                ? ImpersonatedUserId > 0 ? DataRepository.Get<User>(ImpersonatedUserId) : DataRepository.FindUser(User)
                : null;

        #endregion

        #region Authorisation Methods

        private User _OriginalUser;

        protected User OriginalUser
        {
            get
            {
                if (_OriginalUser == null)
                {
                    long userId = Session["OriginalUser"].ToInt64();
                    if (userId > 0)
                    {
                        _OriginalUser = DataRepository.Get<User>(userId);
                    }
                }

                return _OriginalUser;
            }

            set
            {
                if (value == null)
                {
                    Session.Remove("OriginalUser");
                }
                else
                {
                    Session["OriginalUser"] = value.UserId;
                }
            }
        }

        protected long ImpersonatedUserId
        {
            get => Session["ImpersonatedUserId"].ToInt64();
            set => Session["ImpersonatedUserId"] = value;
        }


        protected IActionResult CheckUserRegisteredOk(out User currentUser)
        {
            currentUser = null;

            //Ensure user is logged in submit or rest of registration
            if (!User.Identity.IsAuthenticated)
            {
                //Allow anonymous users when starting registration
                if (IsAnyAction("Register/AboutYou", "Register/VerifyEmail"))
                {
                    return null;
                }

                //Allow anonymous users when resetting password
                if (IsAnyAction("Register/PasswordReset", "Register/NewPassword"))
                {
                    return null;
                }

                //Otherwise ask the user to login
                return new ChallengeResult();
            }

            //Always allow the viewing controller
            if (this is ViewingController)
            {
                return null;
            }

            //Ensure we get a valid user from the database
            currentUser = VirtualUser;
            if (currentUser == null)
            {
                throw new IdentityNotMappedException();
            }

            // When user status is retired
            if (currentUser.Status == UserStatuses.Retired)
            {
                return new ChallengeResult();
            }

            //When email not verified
            if (currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                //If email not sent
                if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
                {
                    if (IsAnyAction("Register/VerifyEmail"))
                    {
                        return null;
                    }

                    //Tell them to verify email
                    return View("CustomError", new ErrorViewModel(1100));
                }

                //If verification code has expired
                if (currentUser.EmailVerifySendDate.Value.AddHours(Global.EmailVerificationExpiryHours) < VirtualDateTime.Now)
                {
                    if (IsAnyAction("Register/VerifyEmail"))
                    {
                        return null;
                    }

                    //prompt user to click to request a new one
                    return View("CustomError", new ErrorViewModel(1101));
                }

                //If code min time hasnt elapsed 
                TimeSpan remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(Global.EmailVerificationMinResendHours)
                                         - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    //Process the code if there is one
                    if (IsAnyAction("Register/VerifyEmail") && !string.IsNullOrWhiteSpace(Request.Query["code"]))
                    {
                        return null;
                    }

                    //tell them to wait
                    return View("CustomError", new ErrorViewModel(1102, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                }

                //if the code is still valid but min sent time has elapsed
                if (IsAnyAction("Register/VerifyEmail", "Register/EmailConfirmed"))
                {
                    return null;
                }

                //Prompt user to request a new verification code
                return View("CustomError", new ErrorViewModel(1103));
            }

            //Ensure admins always routed to their home page
            if (currentUser.IsAdministrator())
            {
                if (IsAnyAction(
                    "Register/VerifyEmail",
                    "Register/EmailConfirmed",
                    "Register/ReviewRequest",
                    "Register/ConfirmCancellation",
                    "Register/RequestAccepted",
                    "Register/RequestCancelled",
                    "Register/ReviewDUNSNumber"))
                {
                    return null;
                }

                return RedirectToAction("Home", "Admin");
                //return View("CustomError", new ErrorViewModel(1117));
            }

            //Ensure admin pages only available to administrators

            if (ControllerName.EqualsI("admin")
                || IsAnyAction(
                    "Register/ReviewRequest",
                    "Register/ReviewDUNSNumber",
                    "Register /ConfirmCancellation",
                    "Register/RequestAccepted",
                    "Register/RequestCancelled"))
            {
                return new HttpForbiddenResult($"User {CurrentUser?.EmailAddress} is not an administrator");
            }

            //Allow all steps from email confirmed to organisation chosen
            if (IsAnyAction(
                "Register/EmailConfirmed",
                "Register/OrganisationType",
                "Register/OrganisationSearch",
                "Register/ChooseOrganisation",
                "Register/AddOrganisation",
                "Register/SelectOrganisation",
                "Register/AddAddress",
                "Register/AddSector",
                "Register/AddContact",
                "Register/ConfirmOrganisation",
                "Register/RequestReceived",
                "Register/EnterFasttrackCodes"))
            {
                return null;
            }

            //Always allow users to manage their account
            if (IsAnyAction(
                "ManageAccount/ManageAccount",
                "ChangeEmail/ChangeEmail",
                "ChangeEmail/ChangeEmailPending",
                "ChangeEmail/VerifyChangeEmail",
                "ChangeEmail/ChangeEmailFailed",
                "ChangeEmail/CompleteChangeEmailAsync",
                "ChangeDetails/ChangeDetails",
                "ChangePassword/ChangePassword",
                "CloseAccount/CloseAccount"))
            {
                return null;
            }

            //Always allow user home or remove registration page 
            if (IsAnyAction(
                "Organisation/ManageOrganisations",
                "Organisation/RemoveOrganisation",
                "Organisation/RemoveOrganisationPost",
                "Organisation/ManageOrganisation",
                "Organisation/ChangeOrganisationScope",
                "Organisation/ActivateOrganisation",
                "Organisation/ReportForOrganisation",
                "Organisation/DeclareScope"))
            {
                return null;
            }

            // if the user doesn't have a selected an organisation then go to the ManageOrgs page
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == ReportingOrganisationId);
            if (userOrg == null)
            {
                _logger.LogWarning(
                    $"Cannot find UserOrganisation for user {currentUser.UserId} and organisation {ReportingOrganisationId}");

                return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
            }

            if (userOrg.Organisation.SectorType == SectorTypes.Private)
            {
                if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
                {
                    //If pin never sent then go to resend point
                    if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    {
                        if (IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                        {
                            return null;
                        }

                        return RedirectToAction("PINSent", "Register");
                    }

                    //If PIN sent and expired then prompt to request a new pin
                    if (userOrg.PINSentDate.Value.AddDays(Global.PinInPostExpiryDays) < VirtualDateTime.Now)
                    {
                        if (IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                        {
                            return null;
                        }

                        return View("CustomError", new ErrorViewModel(1106));
                    }

                    //If PIN resends are allowed and currently on PIN send page then allow it to continue
                    TimeSpan remainingTime = userOrg.PINSentDate.Value.AddDays(Global.PinInPostMinRepostDays) - VirtualDateTime.Now;
                    if (remainingTime <= TimeSpan.Zero && IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                    {
                        return null;
                    }

                    //If PIN Not expired redirect to ActivateService where they can either enter the same pin or request a new one 
                    if (IsAnyAction("Register/RequestPIN"))
                    {
                        return View("CustomError", new ErrorViewModel(1120, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                    }

                    if (IsAnyAction("Register/ActivateService"))
                    {
                        return null;
                    }

                    return RedirectToAction("ActivateService", "Register");
                }
            }

            //Ensure user has completed the registration process
            //If user is fully registered then start submit process
            if (this is RegisterController)
            {
                if (IsAnyAction("Register/RequestReceived"))
                {
                    return null;
                }

                if (IsAnyAction("Register/ServiceActivated") && WasAnyAction("Register/ActivateService", "Register/ConfirmOrganisation"))
                {
                    return null;
                }

                return View("CustomError", new ErrorViewModel(1109));
            }

            //Ensure pending manual registrations always redirected back to home
            if (userOrg.PINConfirmedDate == null)
            {
                _logger.LogWarning(
                    $"UserOrganisation for user {userOrg.UserId} and organisation {userOrg.OrganisationId} PIN is not confirmed");
                return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
            }

            return null;
        }

        #endregion


        #region Public fields

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

        public string LastAction
        {
            get => Session["LastAction"] as string;
            set => Session["LastAction"] = value;
        }

        public string LastController
        {
            get => Session["LastController"] as string;
            set => Session["LastController"] = value;
        }

        #endregion

        #region Exception handling methods

        [NonAction]
        public void AddModelError(int errorCode, string propertyName = null, object parameters = null)
        {
            ModelState.AddModelError(errorCode, propertyName, parameters);
        }

        protected ActionResult SessionExpiredView()
        {
            // create the session expired error model
            var errorModel = new ErrorViewModel(1134);

            // return the custom error view
            return View("CustomError", errorModel);
        }

        #endregion

    }
}