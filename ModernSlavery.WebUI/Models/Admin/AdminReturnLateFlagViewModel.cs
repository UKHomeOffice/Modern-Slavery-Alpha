using System;
using ModernSlavery.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ModernSlavery.WebUI.Models.Admin
{
    public class AdminReturnLateFlagViewModel : GovUkViewModel
    {

        public Return Return { get; set; }

        public bool? NewLateFlag { set; get; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
