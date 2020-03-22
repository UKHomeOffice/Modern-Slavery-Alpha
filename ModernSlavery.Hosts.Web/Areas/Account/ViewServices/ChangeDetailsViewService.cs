﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ChangeDetails;

namespace ModernSlavery.WebUI.Areas.Account.ViewServices
{

    public class ChangeDetailsViewService : IChangeDetailsViewService
    {

        public ChangeDetailsViewService(IUserRepository userRepo,IMapper autoMapper)
        {
            UserRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            AutoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));
        }

        private IUserRepository UserRepository { get; }
        private IMapper AutoMapper { get; }

        public async Task<bool> ChangeDetailsAsync(ChangeDetailsViewModel newDetails, User currentUser)
        {
            // map to business domain model
            var mappedDetails = AutoMapper.Map<UpdateDetailsModel>(newDetails);

            // execute update details
            return await UserRepository.UpdateDetailsAsync(currentUser, mappedDetails);
        }

    }

}
