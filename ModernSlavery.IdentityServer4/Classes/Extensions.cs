﻿using ModernSlavery.BusinessLogic.Abstractions;
using ModernSlavery.BusinessLogic.Account.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ModernSlavery.IdentityServer4.Classes
{
    public static class CustomIdentityServerBuilderExtensions
    {

        public static IIdentityServerBuilder AddCustomUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.AddProfileService<CustomProfileService>();
            builder.AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();

            return builder;
        }

    }
}
