﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Models;
using Microsoft.Azure.Search;

namespace ModernSlavery.Core.Classes
{
    public class SicCodeSearchRepository : ASearchRepository<SicCodeSearchModel>
    {

        public SicCodeSearchRepository(ISearchServiceClient searchServiceClient, bool disabled=false) : base(searchServiceClient, disabled)
        {
            if (Disabled) return;

            _suggesterName = "sicCodeSuggester";

            _searchIndexClient = new Lazy<Task<ISearchIndexClient>>(
                async () => {
                    ISearchServiceClient serviceClient = await _searchServiceClient.Value;
                    return serviceClient.Indexes?.GetClient(Global.SicCodesIndexName);
                });
        }

        public override Task<SicCodeSearchModel> GetAsync(string key, string selectFields = null)
        {
            throw new NotImplementedException();
        }

        public override async Task<IList<SicCodeSearchModel>> ListAsync(string selectFields = null)
        {
            return await ListWorkAsync(nameof(SicCodeSearchModel.SicCodeId));
        }

        public override Task<long> GetDocumentCountAsync()
        {
            throw new NotImplementedException();
        }

    }
}
