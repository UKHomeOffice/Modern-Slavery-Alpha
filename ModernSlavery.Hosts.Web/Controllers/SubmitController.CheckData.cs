﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernSlavery.BusinessLogic.Classes;
using ModernSlavery.BusinessLogic.Models.Submit;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.WebUI.Models.Submit;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;

namespace ModernSlavery.WebUI.Controllers
{
    public partial class SubmitController : BaseController
    {

        #region private methods

        private async Task TryToReloadDraftContent(ReturnViewModel stashedReturnViewModel)
        {
            Draft availableDraft = await _SubmissionPresenter.GetDraftIfAvailableAsync(
                stashedReturnViewModel.OrganisationId,
                stashedReturnViewModel.AccountingDate.Year);

            if (availableDraft != null && availableDraft.HasContent())
            {
                stashedReturnViewModel.ReportInfo.Draft.ReturnViewModelContent = availableDraft.ReturnViewModelContent;
            }
        }

        private string GetReportLink(Return postedReturn)
        {
            return Url.Action(
                "Report",
                "Viewing",
                new {employerIdentifier = postedReturn.Organisation.GetEncryptedId(), year = postedReturn.AccountingDate.Year},
                "https");
        }

        private string GetSubmittedOrUpdated(Return postedReturn)
        {
            List<Return> otherReturns =
                postedReturn.Organisation.Returns
                    .Except(new[] {postedReturn})
                    .Where(r => r.AccountingDate == postedReturn.AccountingDate)
                    .ToList();
            
            return otherReturns.Count > 0 ? "updated" : "submitted";
        }

        #endregion

        #region public methods

        [HttpGet("check-data")]
        public async Task<IActionResult> CheckData()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, currentUser.UserId);

            if (stashedReturnViewModel.ReportInfo.Draft != null && !stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }


            if (!stashedReturnViewModel.HasDraftWithContent())
            {
                await TryToReloadDraftContent(stashedReturnViewModel);
            }

            if (stashedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession
                || stashedReturnViewModel.HasDraftWithContent())
            {
                Return databaseReturn = await _SubmissionPresenter.GetSubmissionByIdAsync(stashedReturnViewModel.ReturnId);

                if (databaseReturn != null)
                {
                    Return stashedReturn = _SubmissionPresenter.CreateDraftSubmissionFromViewModel(stashedReturnViewModel);
                    SubmissionChangeSummary changeSummary = _SubmissionPresenter.GetSubmissionChangeSummary(stashedReturn, databaseReturn);
                    stashedReturnViewModel.IsDifferentFromDatabase = changeSummary.HasChanged;
                    stashedReturnViewModel.ShouldProvideLateReason = changeSummary.ShouldProvideLateReason;
                }
                else
                {
                    // We have some draft info and no DB record, therefore is definitely different
                    stashedReturnViewModel.IsDifferentFromDatabase = true;
                    // Recalculate to know if they're submitting late. This is because it is possible that a draft was created BEFORE the cut-off date ("should provide late reason" would have been marked as 'false') but are completing the submission process AFTER which is when we need them to provide a late reason and the flag is expected to be 'true'.
                    stashedReturnViewModel.ShouldProvideLateReason = _SubmissionPresenter.IsHistoricSnapshotYear(
                        stashedReturnViewModel.SectorType,
                        ReportingOrganisationStartYear.Value);
                }
            }

            if (!_SubmissionPresenter.IsValidSnapshotYear(ReportingOrganisationStartYear.Value))
            {
                return new HttpBadRequestResult($"Invalid snapshot year {ReportingOrganisationStartYear.Value}");
            }

            this.StashModel(stashedReturnViewModel);
            return View("CheckData", stashedReturnViewModel);
        }

        [HttpPost("check-data")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckData(ReturnViewModel postedReturnViewModel)
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

            if (postedReturnViewModel.SectorType == SectorTypes.Public)
            {
                ModelState.Exclude(
                    nameof(postedReturnViewModel.FirstName),
                    nameof(postedReturnViewModel.LastName),
                    nameof(postedReturnViewModel.JobTitle));
            }

            Return postedReturn = _SubmissionPresenter.CreateDraftSubmissionFromViewModel(postedReturnViewModel);

            SubmissionChangeSummary changeSummary = null;
            Return databaseReturn = await _SubmissionPresenter.GetSubmissionByIdAsync(postedReturnViewModel.ReturnId);
            if (databaseReturn != null)
            {
                changeSummary = _SubmissionPresenter.GetSubmissionChangeSummary(postedReturn, databaseReturn);

                if (!changeSummary.HasChanged)
                {
                    return new HttpBadRequestResult("Submission has no changes");
                }

                if (!changeSummary.FiguresChanged)
                {
                    // If the figure have not changed
                    //   e.g. if the only change was to the URL / person reporting
                    // Then don't apply a NEW late flag
                    // But DO continue to apply an EXISTING late flag
                    // So, in summary, "If the figure have not changed, copy the old late flag"
                    postedReturn.IsLateSubmission = databaseReturn.IsLateSubmission;
                }

                postedReturn.Modifications = changeSummary.Modifications;

                databaseReturn.SetStatus(ReturnStatuses.Retired, OriginalUser == null ? currentUser.UserId : OriginalUser.UserId);
            }

            if (databaseReturn == null || !changeSummary.ShouldProvideLateReason)
            {
                ModelState.Remove(nameof(postedReturnViewModel.LateReason));
                ModelState.Remove(nameof(postedReturnViewModel.EHRCResponse));
            }

            ModelState.Remove("ReportInfo.Draft");

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CheckData", postedReturnViewModel);
            }

            if (databaseReturn == null || databaseReturn.Status == ReturnStatuses.Retired)
            {
                SharedBusinessLogic.DataRepository.Insert(postedReturn);
            }

            postedReturn.SetStatus(ReturnStatuses.Submitted, OriginalUser?.UserId ?? currentUser.UserId);

            Organisation organisationFromDatabase = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .FirstOrDefaultAsync(o => o.OrganisationId == postedReturnViewModel.OrganisationId);

            organisationFromDatabase.Returns.Add(postedReturn);

            if (_SubmissionPresenter.ShouldUpdateLatestReturn(organisationFromDatabase, ReportingOrganisationStartYear.Value))
            {
                organisationFromDatabase.LatestReturn = postedReturn;
            }

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            if (!currentUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                await _SubmissionPresenter.SubmissionService.SubmissionBusinessLogic.SubmissionLog.WriteAsync(
                    new SubmissionLogModel {
                        StatusDate = VirtualDateTime.Now,
                        Status = postedReturn.Status,
                        Details = "",
                        Sector = postedReturn.Organisation.SectorType,
                        ReturnId = postedReturn.ReturnId,
                        AccountingDate = postedReturn.AccountingDate.ToShortDateString(),
                        OrganisationId = postedReturn.OrganisationId,
                        EmployerName = postedReturn.Organisation.OrganisationName,
                        Address = postedReturn.Organisation.LatestAddress?.GetAddressString("," + Environment.NewLine),
                        CompanyNumber = postedReturn.Organisation.CompanyNumber,
                        SicCodes = postedReturn.Organisation.GetSicCodeIdsString(postedReturn.StatusDate, "," + Environment.NewLine),
                        DiffMeanHourlyPayPercent = postedReturn.DiffMeanHourlyPayPercent,
                        DiffMedianHourlyPercent = postedReturn.DiffMedianHourlyPercent,
                        DiffMeanBonusPercent = postedReturn.DiffMeanBonusPercent,
                        DiffMedianBonusPercent = postedReturn.DiffMedianBonusPercent,
                        MaleMedianBonusPayPercent = postedReturn.MaleMedianBonusPayPercent,
                        FemaleMedianBonusPayPercent = postedReturn.FemaleMedianBonusPayPercent,
                        MaleLowerPayBand = postedReturn.MaleLowerPayBand,
                        FemaleLowerPayBand = postedReturn.FemaleLowerPayBand,
                        MaleMiddlePayBand = postedReturn.MaleMiddlePayBand,
                        FemaleMiddlePayBand = postedReturn.FemaleMiddlePayBand,
                        MaleUpperPayBand = postedReturn.MaleUpperPayBand,
                        FemaleUpperPayBand = postedReturn.FemaleUpperPayBand,
                        MaleUpperQuartilePayBand = postedReturn.MaleUpperQuartilePayBand,
                        FemaleUpperQuartilePayBand = postedReturn.FemaleUpperQuartilePayBand,
                        CompanyLinkToGPGInfo = postedReturn.CompanyLinkToGPGInfo,
                        ResponsiblePerson = postedReturn.ResponsiblePerson,
                        UserFirstname = currentUser.Firstname,
                        UserLastname = currentUser.Lastname,
                        UserJobtitle = currentUser.JobTitle,
                        UserEmail = currentUser.EmailAddress,
                        ContactFirstName = currentUser.ContactFirstName,
                        ContactLastName = currentUser.ContactLastName,
                        ContactJobTitle = currentUser.ContactJobTitle,
                        ContactOrganisation = currentUser.ContactOrganisation,
                        ContactPhoneNumber = currentUser.ContactPhoneNumber,
                        Created = postedReturn.Created,
                        Modified = postedReturn.Modified,
                        Browser = HttpContext.GetBrowser() ?? "No browser in the request",
                        SessionId = Session.SessionID
                    });
            }

            //This is required for the submission complete page
            postedReturnViewModel.EncryptedOrganisationId = postedReturn.Organisation.GetEncryptedId();
            this.StashModel(postedReturnViewModel);

            if (SharedBusinessLogic.SharedOptions.EnableSubmitAlerts
                && postedReturn.Organisation.Returns.Count(r => r.AccountingDate == postedReturn.AccountingDate) == 1
                && !currentUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                await _SubmissionService.SharedBusinessLogic.SendEmailService.SendGeoMessageAsync(
                    "GPG Data Submission Notification",
                    $"GPG data was submitted for first time in {postedReturn.AccountingDate.Year} by '{postedReturn.Organisation.OrganisationName}' on {postedReturn.StatusDate.ToShortDateString()}\n\n See {Url.Action("Report", "Viewing", new {employerIdentifier = postedReturnViewModel.EncryptedOrganisationId, year = postedReturn.AccountingDate.Year}, "https")}",
                    currentUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
            }

            _SubmissionService.SharedBusinessLogic.NotificationService.SendSuccessfulSubmissionEmailToRegisteredUsers(
                postedReturn,
                GetReportLink(postedReturn),
                GetSubmittedOrUpdated(postedReturn));

            await _SubmissionPresenter.DiscardDraftFileAsync(postedReturnViewModel);

            return RedirectToAction("SubmissionComplete");
        }

        [HttpPost("cancel-check-data")]
        public async Task<IActionResult> CancelCheckData(ReturnViewModel postedReturnViewModel)
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

            postedReturnViewModel.OriginatingAction = "CheckData";
            bool hasDraftChanged = postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession;
            return await PresentUserTheOptionOfSaveDraftOrIgnoreAsync(postedReturnViewModel, hasDraftChanged);
        }

        #endregion

    }
}
