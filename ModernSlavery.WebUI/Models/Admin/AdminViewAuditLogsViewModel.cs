using System.Collections.Generic;
using ModernSlavery.Database;
using ModernSlavery.Database.Models;
using GovUkDesignSystem;

namespace ModernSlavery.WebUI.Models.Admin
{
    public class AdminViewAuditLogsViewModel : GovUkViewModel
    {

        public IEnumerable<AuditLog> AuditLogs { get; set; }
        public Database.Organisation Organisation { get; set; }
        public User User { get; set; }

    }
}
