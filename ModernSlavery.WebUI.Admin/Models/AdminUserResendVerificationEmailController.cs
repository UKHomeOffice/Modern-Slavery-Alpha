﻿using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.WebUI.Admin.Models
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUserResendVerificationEmailController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;
        private readonly ISendEmailService emailSender;

        public AdminUserResendVerificationEmailController(IDataRepository dataRepository, AuditLogger auditLogger, ISendEmailService emailSender)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.emailSender = emailSender;
        }

        [HttpGet("user/{id}/resend-verification-email")]
        public IActionResult ResendVerificationEmailGet(long id)
        {
            User user = dataRepository.Get<User>(id);

            var viewModel = new AdminResendVerificationEmailViewModel { User = user };

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "This user's email address has already been verified");
            }

            return View("ResendVerificationEmail", viewModel);
        }

        [HttpPost("user/{id}/resend-verification-email")]
        public IActionResult ResendVerificationEmailPost(long id, AdminResendVerificationEmailViewModel viewModel)
        {
            User user = dataRepository.Get<User>(id);
            viewModel.User = user;

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder, 
                    "This user's email address has already been verified");
                return View("ResendVerificationEmail", viewModel);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                return View("ResendVerificationEmail", viewModel);
            }

            auditLogger.AuditChangeToUser(
                this,
                AuditedAction.AdminResendVerificationEmail,
                user,
                new
                {
                    Reason = viewModel.Reason
                }
                );

            string verifyCode = Encryption.EncryptQuerystring(user.UserId + ":" + user.Created.ToSmallDateTime());

            user.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
            user.EmailVerifySendDate = VirtualDateTime.Now;
            dataRepository.SaveChangesAsync().Wait();

            string verifyUrl = Url.Action("VerifyEmail", "Register", new { code = verifyCode }, "https");

            if (!emailSender.SendCreateAccountPendingVerificationAsync(verifyUrl, user.EmailAddress).Result)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "Error whilst re-sending verification email. Please try again in a few minutes.");
                return View("ResendVerificationEmail", viewModel);
            }

            return View("VerificationEmailSent", user);
        }

    }
}