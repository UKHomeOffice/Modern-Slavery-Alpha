﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.WebJob
{

    public partial class Functions
    {

        //Set presumed scope of previous years and current years
        public async Task SetPresumedScopes([TimerTrigger("01:00:00:00", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //Initialise any unknown scope statuses
                HashSet<Organisation> changedOrgs = await _ScopeBusinessLogic.SetScopeStatusesAsync();

                //Initialise the presumed scoped
                changedOrgs.AddRange(await _ScopeBusinessLogic.SetPresumedScopesAsync());

                //Update the search indexes
                if (changedOrgs.Count > 0)
                {
                    await SearchBusinessLogic.UpdateSearchIndexAsync(changedOrgs.ToArray());
                }

                log.LogDebug($"Executed {nameof(SetPresumedScopes)} successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(SetPresumedScopes)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

    }

}
