﻿using System;
using Autofac;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Account
{
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
            //TODO: Add IOptions parameters
        }

        public void Register(IDependencyBuilder builder)
        {
            builder.Autofac.RegisterType<AuthorisationBusinessLogic>().As<IAuthorisationBusinessLogic>().SingleInstance();
            builder.Autofac.RegisterType<AuthenticationBusinessLogic>().As<IAuthenticationBusinessLogic>().SingleInstance();
            builder.Autofac.RegisterType<AccountService>().As<IAccountService>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}