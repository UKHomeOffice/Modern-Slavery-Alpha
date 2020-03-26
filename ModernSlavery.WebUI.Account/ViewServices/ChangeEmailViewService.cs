﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessLogic.Account.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.WebUI.Account.Controllers;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models.ChangeEmail;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.ViewServices
{

    public class ChangeEmailViewService : IChangeEmailViewService
    {

        public ChangeEmailViewService(SharedOptions sharedOptions, IUserRepository userRepo, IUrlHelper urlHelper, ISendEmailService sendEmailService)
        {
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            UserRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            UrlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            SendEmailService = sendEmailService;
        }

        private readonly SharedOptions SharedOptions;

        private IUserRepository UserRepository { get; }
        private IUrlHelper UrlHelper { get; }
        private ISendEmailService SendEmailService { get; }


        public async Task<ModelStateDictionary> InitiateChangeEmailAsync(string newEmailAddress, User currentUser)
        {
            var errorState = new ModelStateDictionary();

            // ensure the email address is not the same as existing email address
            if (newEmailAddress.ToLower() == currentUser.EmailAddress.ToLower())
            {
                errorState.AddModelError(nameof(ChangeEmailViewModel.EmailAddress), AccountResources.ChangeEmailAddressMustDiffer);
                return errorState;
            }

            // ensure the email address isn't already in use
            User existingUser = await UserRepository.FindByEmailAsync(newEmailAddress, UserStatuses.New, UserStatuses.Active);
            if (existingUser != null)
            {
                // check if the email address is already an active user
                errorState.AddModelError(
                    nameof(ChangeEmailViewModel.EmailAddress),
                    "The email provided is already used by an active account");
                return errorState;
            }

            // send verification email to the new email address
            await SendChangeEmailPendingVerificationAsync(newEmailAddress, currentUser);

            return errorState;
        }

        public async Task<ModelStateDictionary> CompleteChangeEmailAsync(string code, User currentUser)
        {
            var errorState = new ModelStateDictionary();
            var changeEmailToken = Encryption.DecryptModel<ChangeEmailVerificationToken>(code);

            // ensure token hasn't expired
            DateTime verifyExpiryDate = changeEmailToken.TokenTimestamp.AddHours(SharedOptions.EmailVerificationExpiryHours);
            if (verifyExpiryDate < VirtualDateTime.Now)
            {
                errorState.AddModelError(
                    nameof(CompleteChangeEmailAsync),
                    "Cannot complete the change email process because your verify url has expired.");
                return errorState;
            }

            // make sure we are logged in as the changing user
            if (currentUser.UserId != changeEmailToken.UserId)
            {
                throw new Exception(
                    $"{nameof(CompleteChangeEmailAsync)}: Cannot complete the change email process because the logged in user id ({currentUser.UserId}) did not match the verify token user id ({changeEmailToken.UserId})");
            }

            // if we are the same user and same email address then change email is complete
            if (currentUser.UserId == changeEmailToken.UserId
                && changeEmailToken.NewEmailAddress.ToLower() == currentUser.EmailAddress.ToLower())
            {
                return errorState;
            }

            // ensure the new email address isn't already new or active
            User newUser = await UserRepository.FindByEmailAsync(changeEmailToken.NewEmailAddress, UserStatuses.New, UserStatuses.Active);
            if (newUser != null)
            {
                errorState.AddModelError(
                    nameof(CompleteChangeEmailAsync),
                    "Cannot complete the change email process because the new email address has been registered since this change was requested.");
                return errorState;
            }

            // store old email
            string oldEmailAddress = currentUser.EmailAddress;

            // update user email
            await UserRepository.UpdateEmailAsync(currentUser, changeEmailToken.NewEmailAddress);

            // notify old email the change is complete
            await SendChangeEmailCompletedAsync(oldEmailAddress, changeEmailToken.NewEmailAddress);

            return errorState;
        }

        private async Task SendChangeEmailPendingVerificationAsync(string newEmailAddress, User userToVerify)
        {
            // generate verify code
            string code = CreateEmailVerificationCode(newEmailAddress, userToVerify);

            // generate the verify url
            string returnVerifyUrl = GenerateChangeEmailVerificationUrl(code);

            // queue email
            await SendEmailService.SendChangeEmailPendingVerificationAsync(returnVerifyUrl, newEmailAddress);
        }

        private async Task SendChangeEmailCompletedAsync(string newOldAddress, string newEmailAddress)
        {
            // send to new email
            await SendEmailService.SendChangeEmailCompletedNotificationAsync(newOldAddress);

            // send to old email
            await SendEmailService.SendChangeEmailCompletedVerificationAsync(newEmailAddress);
        }

        private string CreateEmailVerificationCode(string newEmailAddress, User user)
        {
            return Encryption.EncryptModel(
                new ChangeEmailVerificationToken {
                    UserId = user.UserId, NewEmailAddress = newEmailAddress.ToLower(), TokenTimestamp = VirtualDateTime.Now
                });
        }

        private string GenerateChangeEmailVerificationUrl(string code)
        {
            // generate the return url
            return UrlHelper.Action<ChangeEmailController>(
                nameof(ChangeEmailController.VerifyChangeEmail),
                new {code},
                "https");
        }
    }

}