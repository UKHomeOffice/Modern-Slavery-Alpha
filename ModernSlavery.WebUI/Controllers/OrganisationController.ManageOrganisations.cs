﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Models.Organisation;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Classes;
using ModernSlavery.WebUI.Models.Organisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;

namespace ModernSlavery.WebUI.Controllers
{

    public partial class OrganisationController : BaseController
    {

        [Authorize]
        [HttpGet("~/manage-organisations/{id}")]
        public async Task<IActionResult> ManageOrganisation(string id)
        {
            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // clear the stash
            this.ClearStash();

            //Get the current snapshot date
            DateTime currentSnapshotDate = SubmissionService.GetCurrentSnapshotDate(userOrg.Organisation.SectorType);

            //Make sure we have an explicit scope for last and year for organisations new to this year
            if (userOrg.PINConfirmedDate != null && userOrg.Organisation.Created >= currentSnapshotDate)
            {
                ScopeStatuses scopeStatus =
                    await ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYearAsync(organisationId, currentSnapshotDate.Year - 1);
                if (!scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
                {
                    return RedirectToAction(nameof(DeclareScope), "Organisation", new {id});
                }
            }

            // get any associated users for the current org
            List<UserOrganisation> associatedUserOrgs = userOrg.GetAssociatedUsers().ToList();

            // get all editable reports
            List<ReportInfoModel> reportInfos = await SubmissionService.GetAllEditableReportsAsync(userOrg, currentSnapshotDate);

            // build the view model
            var model = new ManageOrganisationModel {
                CurrentUserOrg = userOrg,
                AssociatedUserOrgs = associatedUserOrgs,
                EncCurrentOrgId = Encryption.EncryptQuerystring(organisationId.ToString()),
                ReportInfoModels = reportInfos.OrderBy(r => r.ReportingStartDate).ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpGet("~/manage-organisations")]
        public IActionResult ManageOrganisations()
        {
            //Clear all the stashes
            this.ClearAllStashes();

            //Remove any previous searches from the cache
            PrivateSectorRepository.ClearSearch();

            //Reset the current reporting organisation
            ReportingOrganisation = null;

            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null && IsImpersonatingUser == false)
            {
                return checkResult;
            }

            // check if the user has accepted the privacy statement (unless admin or impersonating)
            if (!IsImpersonatingUser && !base.CurrentUser.IsAdministrator())
            {
                DateTime? hasReadPrivacy = currentUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.Value < CommonBusinessLogic.GlobalOptions.PrivacyChangedDate)
                {
                    return RedirectToAction(nameof(HomeController.PrivacyPolicy), "Home");
                }
            }
            
            //create the new view model 
            IOrderedEnumerable<UserOrganisation> model = currentUser.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName);
            return View(nameof(ManageOrganisations), model);
        }

    }

}
