﻿using System;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminHomepageViewModel
    {
        public bool IsSuperAdministrator { get; set; }
        public bool IsDatabaseAdministrator { get; set; }
        public bool IsDowngradedDueToIpRestrictions { get; set; }
        public int FeedbackCount { get; set; }
        public DateTime? LatestFeedbackDate { get; set; }
    }
}