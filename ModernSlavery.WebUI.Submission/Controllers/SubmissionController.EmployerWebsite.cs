﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {
        #region private methods

        private static bool IsEmployerWebsiteModified(ReturnViewModel postedReturnViewModel,
            ReturnViewModel stashedReturnViewModel)
        {
            return postedReturnViewModel.CompanyLinkToGPGInfo != stashedReturnViewModel.CompanyLinkToGPGInfo;
        }

        #endregion

        #region public methods

        [HttpGet("employer-website")]
        public async Task<IActionResult> EmployerWebsite(string returnUrl = null)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null) return SessionExpiredView();

            stashedReturnViewModel =
                await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, VirtualUser.UserId);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            if (stashedReturnViewModel.SectorType == SectorTypes.Public)
            {
                ModelState.Remove(nameof(stashedReturnViewModel.FirstName));
                ModelState.Remove(nameof(stashedReturnViewModel.LastName));
                ModelState.Remove(nameof(stashedReturnViewModel.JobTitle));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl;

            return View("EmployerWebsite", stashedReturnViewModel);
        }

        [HttpPost("employer-website")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployerWebsite(ReturnViewModel postedReturnViewModel)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null) return SessionExpiredView();

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ModelState.Include(nameof(postedReturnViewModel.CompanyLinkToGPGInfo));

            #region Keep draft file locked to this user

            await _SubmissionPresenter.KeepDraftFileLockedToUserAsync(postedReturnViewModel, CurrentUser.UserId);

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    IsEmployerWebsiteModified(postedReturnViewModel, stashedReturnViewModel);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            #endregion

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("EmployerWebsite", postedReturnViewModel);
            }

            StashModel(postedReturnViewModel);

            return RedirectToAction("CheckData");
        }

        [HttpPost("cancel-employer-website")]
        public async Task<IActionResult> CancelEmployerWebsite(ReturnViewModel postedReturnViewModel)
        {
            postedReturnViewModel.OriginatingAction = "EmployerWebsite";
            return await ManageDraftAsync(postedReturnViewModel, IsEmployerWebsiteModified);
        }

        #endregion
    }
}