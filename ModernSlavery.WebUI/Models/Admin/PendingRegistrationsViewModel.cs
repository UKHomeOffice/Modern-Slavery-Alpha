﻿using System.Collections.Generic;
using ModernSlavery.Database;

namespace ModernSlavery.WebUI.Models.Admin
{
    public class PendingRegistrationsViewModel
    {

        public List<UserOrganisation> PublicSectorUserOrganisations { get; set; }
        public List<UserOrganisation> NonUkAddressUserOrganisations { get; set; }
        public List<UserOrganisation> ManuallyRegisteredUserOrganisations { get; set; }

    }
}
