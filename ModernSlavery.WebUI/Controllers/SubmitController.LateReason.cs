﻿using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Models.Submit;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        #region public methods

        [HttpGet("late-reason")]
        public IActionResult LateReason()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            ModelState.Clear();

            return View(stashedReturnViewModel);
        }

        [HttpPost("late-reason")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LateReason(ReturnViewModel postedReturnViewModel)
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            postedReturnViewModel.ReportInfo.Draft = stashedReturnViewModel.ReportInfo.Draft;

            postedReturnViewModel.LateReason = postedReturnViewModel.LateReason?.Trim();

            ModelState.Include(nameof(postedReturnViewModel.LateReason), nameof(postedReturnViewModel.EHRCResponse));

            return !ModelState.IsValid
                ? View(postedReturnViewModel)
                : await CheckData(postedReturnViewModel);
        }

        #endregion

    }
}
