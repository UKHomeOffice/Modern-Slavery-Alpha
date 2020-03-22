﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using AutoMapper;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Extensions;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.IdServer.Classes;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.IdServer
{
    public class AppDependencyModule: IDependencyModule
    {
        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;

        private readonly ILogger _Logger;
        private readonly SharedOptions _sharedOptions;
        private readonly StorageOptions _storageOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;

        public AppDependencyModule(ILogger<AppDependencyModule> logger, SharedOptions sharedOptions, StorageOptions storageOptions, DistributedCacheOptions distributedCacheOptions, DataProtectionOptions dataProtectionOptions)
        {
            _Logger = logger;
            _sharedOptions = sharedOptions;
            _storageOptions = storageOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
        }


        public void Bind(ContainerBuilder builder, IServiceCollection services)
        {
            #region Configure identity server

            services.AddSingleton<IEventSink, AuditEventSink>();

            var clients = new Clients(_sharedOptions);
            var resources = new Resources(_sharedOptions);

            var identityServer = services.AddIdentityServer(
                    options => {
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                        options.UserInteraction.LoginUrl = "/sign-in";
                        options.UserInteraction.LogoutUrl = "/sign-out";
                        options.UserInteraction.ErrorUrl = "/error";
                    })
                .AddInMemoryClients(clients.Get())
                .AddInMemoryIdentityResources(resources.GetIdentityResources())
                //.AddInMemoryApiResources(Resources.GetApiResources())
                .AddCustomUserStore();

            if (Debugger.IsAttached || _sharedOptions.IsDevelopment() || _sharedOptions.IsLocal())
                identityServer.AddDeveloperSigningCredential();
            else
                identityServer.AddSigningCredential(LoadCertificate(_sharedOptions));

            #endregion

            //Allow caching of http responses
            services.AddResponseCaching();

            //This is to allow access to the current http context anywhere
            services.AddHttpContextAccessor();

            services.AddControllersWithViews();

            var mvcBuilder = services.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal()) mvcBuilder.AddRazorRuntimeCompilation();

            //Add the distributed cache and data protection
            services.AddDistributedCache(_distributedCacheOptions).AddDataProtection(_dataProtectionOptions);

            //Add app insights tracking
            services.AddApplicationInsightsTelemetry(_sharedOptions.APPINSIGHTS_INSTRUMENTATIONKEY);

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            //Register the database dependencies
            builder.RegisterDependencyModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.RegisterDependencyModule<FileStorageDependencyModule>();

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register log records (without key filtering)
            builder.RegisterType<UserLogger>().As<IUserLogger>().SingleInstance();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();

            // Initialise AutoMapper
            MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                // register all out mapper profiles (classes/mappers/*)
                // config.AddMaps(typeof(MvcApplication));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);

        }

        private X509Certificate2 LoadCertificate(SharedOptions sharedOptions)
        {
            //Load the site certificate
            string certThumprint = sharedOptions.WEBSITE_LOAD_CERTIFICATES.SplitI(";").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(certThumprint))
            {
                certThumprint = _sharedOptions.CertThumprint.SplitI(";").FirstOrDefault();
            }

            X509Certificate2 cert = null;
            if (!string.IsNullOrWhiteSpace(certThumprint))
            {
                cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);
                _Logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from thumbprint '{certThumprint}'");
            }
            else
            {
                string certPath = Path.Combine(Directory.GetCurrentDirectory(), @"LocalHost.pfx");
                cert = HttpsCertificate.LoadCertificateFromFile(certPath, "LocalHost");
                _Logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from file '{certPath}'");
            }

            if (sharedOptions.CertExpiresWarningDays > 0)
            {
                DateTime expires = cert.GetExpirationDateString().ToDateTime();
                if (expires < VirtualDateTime.UtcNow)
                {
                    _Logger.LogError(
                        $"The website certificate for '{sharedOptions.EXTERNAL_HOST}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
                }
                else
                {
                    TimeSpan remainingTime = expires - VirtualDateTime.Now;

                    if (expires < VirtualDateTime.UtcNow.AddDays(sharedOptions.CertExpiresWarningDays))
                    {
                        _Logger.LogWarning(
                            $"The website certificate for '{sharedOptions.SiteAuthority}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
                    }
                }
            }

            return cert;
        }

    }
}
