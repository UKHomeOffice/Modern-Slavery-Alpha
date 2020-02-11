﻿using AutoMapper;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Extensions.AspNetCore;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ManageAccountController : BaseController
    {

        public ManageAccountController(ILogger<ManageAccountController> logger,
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepo,
            IWebTracker webTracker, IMapper autoMapper) :
            base(logger, cache, session, dataRepo, webTracker,autoMapper) { }

        [HttpGet]
        public IActionResult ManageAccount()
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null && IsImpersonatingUser == false)
            {
                return checkResult;
            }

            // map the user to the view model
            var model = AutoMapper.Map<ManageAccountViewModel>(currentUser);

            // check if we have any successful changes
            if (TempData.ContainsKey(nameof(AccountResources.ChangeDetailsSuccessAlert)))
            {
                ViewBag.ChangeSuccessMessage = AccountResources.ChangeDetailsSuccessAlert;
            }
            else if (TempData.ContainsKey(nameof(AccountResources.ChangePasswordSuccessAlert)))
            {
                ViewBag.ChangeSuccessMessage = AccountResources.ChangePasswordSuccessAlert;
            }

            // generate flow urls
            ViewBag.CloseAccountUrl = "";
            ViewBag.ChangeEmailUrl = "";
            ViewBag.ChangePasswordUrl = "";
            ViewBag.ChangeDetailsUrl = "";

            if (IsImpersonatingUser == false)
            {
                ViewBag.CloseAccountUrl = Url.Action<CloseAccountController>(nameof(CloseAccountController.CloseAccount));
                ViewBag.ChangeEmailUrl = Url.Action<ChangeEmailController>(nameof(ChangeEmailController.ChangeEmail));
                ViewBag.ChangePasswordUrl = Url.Action<ChangePasswordController>(nameof(ChangePasswordController.ChangePassword));
                ViewBag.ChangeDetailsUrl = Url.Action<ChangeDetailsController>(nameof(ChangeDetailsController.ChangeDetails));
            }

            // remove any change updates
            TempData.Remove(nameof(AccountResources.ChangeDetailsSuccessAlert));
            TempData.Remove(nameof(AccountResources.ChangePasswordSuccessAlert));

            return View(model);
        }

    }

}
