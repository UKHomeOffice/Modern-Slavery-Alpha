﻿using ModernSlavery.Core.Abstractions;

namespace ModernSlavery.Core.Models
{

    public class GeoOrganisationRegistrationRequestTemplate : AEmailTemplate
    {

        public string Name { get; set; }

        public string Org2 { get; set; }

        public string Address { get; set; }

        public string Url { get; set; }

    }

}
