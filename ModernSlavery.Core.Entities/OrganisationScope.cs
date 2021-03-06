﻿using System;
using System.Collections.Generic;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    public class OrganisationScope
    {
        public OrganisationScope()
        {
            Organisations = new HashSet<Organisation>();
        }

        public long OrganisationScopeId { get; set; }
        public long OrganisationId { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }
        public DateTime ScopeStatusDate { get; set; } = VirtualDateTime.Now;

        public RegisterStatuses RegisterStatus { get; set; }
        public DateTime RegisterStatusDate { get; set; } = VirtualDateTime.Now;
        public string ContactFirstname { get; set; }
        public string ContactLastname { get; set; }
        public string ContactEmailAddress { get; set; }
        public bool? ReadGuidance { get; set; }
        public string Reason { get; set; }
        public string CampaignId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public ScopeRowStatuses Status { get; set; }
        public string StatusDetails { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual ICollection<Organisation> Organisations { get; set; }
    }
}