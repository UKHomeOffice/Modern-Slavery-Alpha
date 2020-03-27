﻿using System;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class RemoveOrganisationModel
    {
        public string EncOrganisationId { get; set; }
        public string EncUserId { get; set; }

        public string OrganisationName { get; set; }
        public string OrganisationAddress { get; set; }
        public string UserName { get; set; }
    }
}