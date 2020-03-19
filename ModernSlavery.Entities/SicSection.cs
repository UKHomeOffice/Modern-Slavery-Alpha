﻿using System;
using System.Collections.Generic;
using ModernSlavery.Extensions;

namespace ModernSlavery.Entities
{
    public partial class SicSection
    {
        public SicSection()
        {
            SicCodes = new HashSet<SicCode>();
        }

        public string SicSectionId { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual ICollection<SicCode> SicCodes { get; set; }
    }
}