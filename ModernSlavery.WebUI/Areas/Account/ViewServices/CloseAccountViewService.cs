﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Areas.Account.ViewServices
{

    public class CloseAccountViewService : ICloseAccountViewService
    {

        public CloseAccountViewService(IUserRepository userRepository,
            IRegistrationRepository registrationRepository,
            ILogger<CloseAccountViewService> logger)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            RegistrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));
            Logger = logger;
        }

        public async Task<ModelStateDictionary> CloseAccountAsync(User userToRetire, string currentPassword, User actionByUser)
        {
            var errorState = new ModelStateDictionary();

            // ensure the user has entered their password
            bool checkPasswordResult = await UserRepository.CheckPasswordAsync(userToRetire, currentPassword);
            if (checkPasswordResult == false)
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Could not verify your current password");
                return errorState;
            }

            //Save the list of registered organisations
            List<Organisation> userOrgs = userToRetire.UserOrganisations.Select(uo => uo.Organisation).Distinct().ToList();

            // aggregated save
            await UserRepository.BeginTransactionAsync(
                async () => {
                    try
                    {
                        // update retired user registrations 
                        await RegistrationRepository.RemoveRetiredUserRegistrationsAsync(userToRetire, actionByUser);

                        // retire user
                        await UserRepository.RetireUserAsync(userToRetire);

                        // commit
                        UserRepository.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        UserRepository.RollbackTransaction();
                        Logger.LogWarning(
                            ex,
                            "Failed to retire user {UserId}. Action by user {ActionByUserId}",
                            userToRetire.UserId,
                            actionByUser.UserId);
                        throw;
                    }
                });

            if (!userToRetire.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                // Create the close account notification to user
                var sendEmails = new List<Task>();
                bool testEmail = !Config.IsProduction();
                sendEmails.Add(Emails.SendAccountClosedNotificationAsync(userToRetire.EmailAddress, testEmail));

                //Create the notification to GEO for each newly orphaned organisation
                userOrgs.Where(org => org.GetIsOrphan())
                    .ForEach(org => sendEmails.Add(Emails.SendGEOOrphanOrganisationNotificationAsync(org.OrganisationName, testEmail)));

                //Send all the notifications in parallel
                await Task.WhenAll(sendEmails);
            }

            return errorState;
        }

        #region Dependencies

        private IUserRepository UserRepository { get; }

        private IRegistrationRepository RegistrationRepository { get; }

        private ILogger<CloseAccountViewService> Logger { get; }

        #endregion

    }

}