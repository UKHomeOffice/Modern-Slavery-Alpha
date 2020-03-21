using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Models.Scope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.BusinessLogic;
using ModernSlavery.WebUI.Presenters;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Controllers
{

    [Route("scope")]
    public class ScopeController : BaseController
    {

        #region Constructors

        public ScopeController(
            IScopePresenter scopeUI,
            ILogger<ScopeController> logger, IWebService webService, ICommonBusinessLogic commonBusinessLogic) : base(logger, webService, commonBusinessLogic)
        {
            _commonBusinessLogic = commonBusinessLogic;
            ScopePresentation = scopeUI;
        }

        #endregion

        #region Dependencies
        public ICommonBusinessLogic _commonBusinessLogic { get; set; }

        public IScopePresenter ScopePresentation { get; }

        #endregion

        [HttpGet("out")]
        public async Task<IActionResult> OutOfScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var currentStateModel = this.UnstashModel<ScopingViewModel>();
            EnterCodesViewModel model = currentStateModel?.EnterCodes ?? new EnterCodesViewModel();

            // when spamlocked then return a CustomError view
            TimeSpan remainingTime = await GetRetryLockRemainingTimeAsync("lastScopeCode", CommonBusinessLogic.GlobalOptions.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1125, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            PendingFasttrackCodes = null;

            // show the view
            return View("EnterCodes", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out")]
        public async Task<IActionResult> OutOfScope(EnterCodesViewModel model)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            // When Spamlocked then return a CustomError view
            TimeSpan remainingTime = await GetRetryLockRemainingTimeAsync("lastScopeCode", CommonBusinessLogic.GlobalOptions.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1125, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            // the following fields are validatable at this stage
            ModelState.Include(
                nameof(EnterCodesViewModel.EmployerReference),
                nameof(EnterCodesViewModel.SecurityToken));

            // When ModelState is Not Valid Then Return the EnterCodes View
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<EnterCodesViewModel>();
                return View("EnterCodes", model);
            }

            // Generate the state model
            ScopingViewModel stateModel = await ScopePresentation.CreateScopingViewModelAsync(model, CurrentUser);

            if (stateModel == null)
            {
                await IncrementRetryCountAsync("lastScopeCode", CommonBusinessLogic.GlobalOptions.LockoutMinutes);
                ModelState.AddModelError(3027);
                this.CleanModelErrors<EnterCodesViewModel>();
                return View("EnterCodes", model);
            }


            //Clear the retry locks
            await ClearRetryLocksAsync("lastScopeCode");

            // set the back link
            stateModel.StartUrl = Url.Action("OutOfScope");

            // set the journey to out-of-scope
            stateModel.IsOutOfScopeJourney = true;

            // save the state to the session cache
            this.StashModel(stateModel);

            // when security code has expired then redirect to the CodeExpired action
            if (stateModel.IsSecurityCodeExpired)
            {
                return View("CodeExpired", stateModel);
            }


            //When on out-of-scope journey and any previous explicit scope then tell user scope is known
            if (!stateModel.IsChangeJourney
                && (
                    stateModel.LastScope != null && stateModel.LastScope.ScopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope)
                    || stateModel.ThisScope != null
                    && stateModel.ThisScope.ScopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope)
                )
            )
            {
                return View("ScopeKnown", stateModel);
            }

            // redirect to next step
            return RedirectToAction("ConfirmOutOfScopeDetails");
        }

        [Authorize]
        [HttpGet("in")]
        public async Task<IActionResult> InScope()
        {
            await TrackPageViewAsync();

            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        [Authorize]
        [HttpGet("in/confirm")]
        public IActionResult ConfirmInScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            // else redirect to ConfirmDetails action
            return View("ConfirmInScope", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("in/confirm")]
        public async Task<IActionResult> ConfirmInScope(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>(true);
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            ApplyUserContactDetails(CurrentUser, stateModel);

            // Save user as in scope
            var snapshotYears = new HashSet<int> {stateModel.AccountingDate.Year};
            await ScopePresentation.SaveScopesAsync(stateModel, snapshotYears);

            var organisation = CommonBusinessLogic.DataRepository.Get<Organisation>(stateModel.OrganisationId);
            DateTime currentSnapshotDate = _commonBusinessLogic.GetAccountingStartDate(organisation.SectorType);
            if (stateModel.AccountingDate == currentSnapshotDate)
            {
                IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (string emailAddress in emailAddressesForOrganisation)
                {
                    _commonBusinessLogic.NotificationService.SendScopeChangeInEmail(emailAddress, organisation.OrganisationName);
                }
            }

            //Start new user registration
            return RedirectToAction(
                "ManageOrganisation",
                "Organisation",
                new {id = Encryption.EncryptQuerystring(stateModel.OrganisationId.ToString())});
        }


        [HttpGet("out/confirm-employer")]
        public IActionResult ConfirmOutOfScopeDetails()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            // else redirect to ConfirmDetails action
            return View("ConfirmOutOfScopeDetails", stateModel);
        }

        [HttpGet("out/questions")]
        public IActionResult EnterOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            return View("EnterOutOfScopeAnswers", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/questions")]
        public IActionResult EnterOutOfScopeAnswers(EnterAnswersViewModel enterAnswersModel)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            // update the state model
            var stateModel = this.UnstashModel<ScopingViewModel>();
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            // update the state
            stateModel.EnterAnswers = enterAnswersModel;
            this.StashModel(stateModel);
            var fields = new List<string>();

            // when the user is not logged in then validate the contact details
            if (CurrentUser == null)
            {
                fields.Add(nameof(EnterAnswersViewModel.FirstName));
                fields.Add(nameof(EnterAnswersViewModel.LastName));
                fields.Add(nameof(EnterAnswersViewModel.EmailAddress));
                fields.Add(nameof(EnterAnswersViewModel.ConfirmEmailAddress));
            }

            // the following fields are validatable at this stage
            fields.Add(nameof(EnterAnswersViewModel.Reason));
            if (enterAnswersModel.Reason == "Other")
            {
                fields.Add(nameof(EnterAnswersViewModel.OtherReason));
            }

            fields.Add(nameof(EnterAnswersViewModel.ReadGuidance));

            ModelState.Include(fields.ToArray());

            // validate the details
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ScopingViewModel>();
                return View("EnterOutOfScopeAnswers", stateModel);
            }

            //Ensure email is always lower case
            if (!string.IsNullOrEmpty(enterAnswersModel.EmailAddress))
            {
                enterAnswersModel.EmailAddress = enterAnswersModel.EmailAddress.ToLower();
            }

            this.StashModel(stateModel);

            //Start new user registration
            return RedirectToAction("ConfirmOutOfScopeAnswers");
        }

        [HttpGet("out/confirm-answers")]
        public IActionResult ConfirmOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            return View("ConfirmOutOfScopeAnswers", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/confirm-answers")]
        public async Task<IActionResult> ConfirmOutOfScopeAnswers(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            ApplyUserContactDetails(CurrentUser, stateModel);

            // Save user as out of scope
            var snapshotYears = new HashSet<int> {stateModel.AccountingDate.Year};
            if (!stateModel.IsChangeJourney)
            {
                snapshotYears.Add(stateModel.AccountingDate.Year - 1);
            }

            await ScopePresentation.SaveScopesAsync(stateModel, snapshotYears);

            this.StashModel(stateModel);

            var organisation = CommonBusinessLogic.DataRepository.Get<Organisation>(stateModel.OrganisationId);
            DateTime currentSnapshotDate = _commonBusinessLogic.GetAccountingStartDate(organisation.SectorType);
            if (stateModel.AccountingDate == currentSnapshotDate)
            {
                IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (string emailAddress in emailAddressesForOrganisation)
                {
                    _commonBusinessLogic.NotificationService.SendScopeChangeOutEmail(emailAddress, organisation.OrganisationName);
                }
            }

            //Start new user registration
            return RedirectToAction("FinishOutOfScope", "Scope");
        }

        [HttpGet("out/finish")]
        public IActionResult FinishOutOfScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            this.StashModel(stateModel);

            //Complete
            return View("FinishOutOfScope", stateModel);
        }

        [HttpGet("register")]
        public IActionResult RegisterOrManage()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            var stateModel = this.UnstashModel<ScopingViewModel>(true);
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            //if user has already registered then manage that organisation
            if (stateModel.UserIsRegistered)
            {
                return RedirectToAction(
                    "ManageOrganisation",
                    "Organisation",
                    new {id = Encryption.EncryptQuerystring(stateModel.OrganisationId.ToString())});
            }

            // when not auth then save codes and return ManageOrganisations redirect
            if (!stateModel.IsSecurityCodeExpired)
            {
                PendingFasttrackCodes =
                    $"{stateModel.EnterCodes.EmployerReference}:{stateModel.EnterCodes.SecurityToken}:{stateModel.EnterAnswers?.FirstName}:{stateModel.EnterAnswers?.LastName}:{stateModel.EnterAnswers?.EmailAddress}";
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        private void ApplyUserContactDetails(User currentUser, ScopingViewModel model)
        {
            // when logged in then override contact details
            if (currentUser != null)
            {
                model.EnterAnswers.FirstName = currentUser.Firstname;
                model.EnterAnswers.LastName = currentUser.Lastname;
                model.EnterAnswers.EmailAddress = currentUser.EmailAddress;
            }
        }

    }

}
