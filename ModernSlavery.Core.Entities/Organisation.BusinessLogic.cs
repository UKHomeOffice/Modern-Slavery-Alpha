using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    [DebuggerDisplay("{OrganisationName},{Status}")]
    public partial class Organisation
    {
        #region Overrides
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var target = (Organisation)obj;
            return OrganisationId == target.OrganisationId;
        }

        public override int GetHashCode()
        {
            return OrganisationId.GetHashCode();
        }

        public override string ToString()
        {
            return $"ref:{EmployerReference}, name:{OrganisationName}";
        }
        #endregion

        #region SicCode
        public string GetLatestSicSource()
        {
            return OrganisationSicCodes.Where(s =>
                    s.Retired == null).OrderByDescending(s=>s.Created).FirstOrDefault()
                ?.Source;
        }
        public string GetSicSource(DateTime accountingDate)
        {
            return OrganisationSicCodes.FirstOrDefault(s =>
                    s.Created < accountingDate && (s.Retired == null || s.Retired.Value > accountingDate))
                ?.Source;
        }
        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="accountingDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public IEnumerable<OrganisationSicCode> GetSicCodes(DateTime accountingDate)
        {
            return OrganisationSicCodes.Where(s =>
                s.Created < accountingDate && (s.Retired == null || s.Retired.Value > accountingDate));
        }

        public IEnumerable<OrganisationSicCode> GetLatestSicCodes()
        {
            return OrganisationSicCodes.Where(s =>s.Retired == null).OrderByDescending(s=>s.Created);
        }

        public IEnumerable<int> GetLatestSicCodeIds(string delimiter = ", ")
        {
            return GetLatestSicCodes().Select(s => s.SicCodeId);
        }
        public string GetLatestSicCodeIdsString(string delimiter = ", ")
        {
            return GetLatestSicCodeIds().OrderBy(s => s).ToDelimitedString(delimiter);
        }

        public string GetSicCodeIdsString(DateTime accountingDate, string delimiter = ", ")
        {
            return GetSicCodes(accountingDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId).ToDelimitedString(delimiter);
        }
        #endregion

        #region SicSection
        public IEnumerable<SicSection> GetSicSections(DateTime accountingDate)
        {
            return GetSicSections(GetSicCodes(accountingDate));
        }

        public IEnumerable<SicSection> GetLatestSicSections()
        {
            return GetSicSections(GetLatestSicCodes());
        }
        public IEnumerable<SicSection> GetSicSections(IEnumerable<OrganisationSicCode> organisationSicCodes)
        {
            return organisationSicCodes.Select(s => s.SicCode.SicSection).Distinct();
        }

        public string GetSicSectionIdsString(DateTime accountingDate, string delimiter = ", ")
        {
            var organisationSicCodes = GetSicCodes(accountingDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSectionId).UniqueI().OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }

        public string GetSicSectorsString(IEnumerable<SicSection> sicSectors, string delimiter = ", ")
        {
            return sicSectors.Select(s => s.Description.Trim())
                .UniqueI()
                .OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }

        public string GetSicSectorsString(DateTime accountingDate, string delimiter = ", ")
        {
            return GetSicSectorsString(GetSicSections(accountingDate), delimiter);
        }

        public string GetLatestSicSectorsString(string delimiter = ", ")
        {
            var organisationSicCodes = GetLatestSicCodes();
            return GetSicSectorsString(GetLatestSicSections(), delimiter);
        }
        #endregion

        #region Name
        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="accountingDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public OrganisationName GetName(DateTime accountingDate)
        {
            return OrganisationNames.Where(n => n.Created < accountingDate)
                .OrderByDescending(n => n.Created)
                .FirstOrDefault();
        }
        public OrganisationName GetLatestName()
        {
            return OrganisationNames
                .OrderByDescending(n => n.Created)
                .FirstOrDefault();
        }
        #endregion

        #region Address
        /// <summary>
        ///     Returns the latest address before specified date/time
        /// </summary>
        /// <param name="accountingDate">Ignore address changes after this date/time - if empty returns the latest address</param>
        /// <returns>The address of the organisation</returns>
        public OrganisationAddress GetAddress(DateTime accountingDate, AddressStatuses status = AddressStatuses.Active)
        {
            var addressStatus = OrganisationAddresses.SelectMany(a =>
                    a.AddressStatuses.Where(s => s.Status == status && s.StatusDate < accountingDate))
                .OrderByDescending(s => s.StatusDate)
                .FirstOrDefault();

            if (addressStatus != null && addressStatus.Address.Status == status) return addressStatus.Address;

            if (LatestAddress != null && LatestAddress.Status == status) return LatestAddress;

            return null;
        }

        public OrganisationAddress GetLatestAddress(AddressStatuses status = AddressStatuses.Active)
        {
            var addressStatus = OrganisationAddresses.SelectMany(a =>
                    a.AddressStatuses.Where(s => s.Status == status))
                .OrderByDescending(s => s.StatusDate)
                .FirstOrDefault();

            if (addressStatus != null && addressStatus.Address.Status == status) return addressStatus.Address;

            if (LatestAddress != null && LatestAddress.Status == status) return LatestAddress;

            return null;
        }
        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="accountingDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public string GetAddressString(DateTime accountingDate, AddressStatuses status = AddressStatuses.Active,
            string delimiter = ", ")
        {
            var address = GetAddress(accountingDate, status);

            return address?.GetAddressString(delimiter);
        }
        #endregion

        #region Returns
        //Returns the latest return for the specified accounting year or the latest ever if no accounting year is 
        public Return GetReturn(int year)
        {
            return Returns.Where(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate.Year == year)
                .OrderByDescending(r => r.StatusDate)
                .FirstOrDefault();
        }

        public IEnumerable<Return> GetSubmittedReports()
        {
            return Returns.Where(r => r.Status == ReturnStatuses.Submitted)
                .OrderByDescending(r => r.AccountingDate);
        }
        #endregion

        #region Scope
        public bool GetIsInscope(DateTime accountingDate)
        {
            return !GetScopeStatus(accountingDate).IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.OutOfScope);
        }

        public OrganisationScope GetLatestScope()
        {
            return OrganisationScopes.OrderByDescending(s=>s.SnapshotDate).FirstOrDefault(orgScope =>
                orgScope.Status == ScopeRowStatuses.Active);
        }

        //Returns the scope for the specified accounting date
        public OrganisationScope GetScope(DateTime accountingStartDate)
        {
            return OrganisationScopes.FirstOrDefault(s =>
                s.Status == ScopeRowStatuses.Active && s.SnapshotDate == accountingStartDate);
        }


        public ScopeStatuses GetScopeStatus(DateTime accountingStartDate)
        {
            var scope = GetScope(accountingStartDate);
            return scope == null ? ScopeStatuses.Unknown : scope.ScopeStatus;
        }


        public OrganisationScope GetScope(int snapshotYear)
        {
            return OrganisationScopes.FirstOrDefault(orgScope =>
                orgScope.Status == ScopeRowStatuses.Active
                && orgScope.SnapshotDate.Year == snapshotYear);
        }

        public OrganisationScope GetScopeOrThrow(int snapshotYear)
        {
            var organisationScope = GetScope(snapshotYear);

            if (organisationScope == null)
                throw new ArgumentOutOfRangeException(
                    $"Cannot find an scope with status 'Active' for snapshotYear '{snapshotYear}' linked to organisation '{OrganisationName}', employerReference '{EmployerReference}'.");

            return organisationScope;
        }
        #endregion

        #region Status
        public OrganisationStatus PreviousStatus
        {
            get
            {
                return OrganisationStatuses
                    .OrderByDescending(os => os.StatusDate).Skip(1)
                    .FirstOrDefault();
            }
        }
        public bool IsActive()
        {
            return Status == Entities.OrganisationStatuses.Active;
        }

        public bool IsPending()
        {
            return Status == Entities.OrganisationStatuses.Pending;
        }

        public bool GetWasDissolvedBefore(DateTime? accountingDate=null)
        {
            return DateOfCessation != null && (accountingDate==null || DateOfCessation < accountingDate.Value);
        }

        public bool GetIsCurrentlyDissolved()
        {
            return DateOfCessation != null;
        }

        public void SetStatus(OrganisationStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;

            OrganisationStatuses.Add(
                new OrganisationStatus
                {
                    OrganisationId = OrganisationId,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        /// <summary>
        ///     Returns true if organisation has been made an orphan and is in scope
        /// </summary>
        public void RevertToLastStatus(long byUserId, string details = null)
        {
            var previousStatus = PreviousStatus
                                 ?? throw new InvalidOperationException(
                                     $"The list of Statuses for Organisation '{OrganisationName}' employerReference '{EmployerReference}' isn't long enough to perform a '{nameof(RevertToLastStatus)}' command. It needs to have at least 2 statuses so these can reverted.");

            SetStatus(previousStatus.Status, byUserId, details);
        }
        #endregion

        #region Security Codes
        /// <summary>
        ///     Method to modify the security code expiring information (create/extend/expire). It additionally timestamps such
        ///     change.
        /// </summary>
        /// <param name="securityCodeExpiryDateTime"></param>
        public void SetSecurityCodeExpiryDate(DateTime securityCodeExpiryDateTime)
        {
            if (SecurityCode == null)
                throw new Exception(
                    "Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null");

            SecurityCodeExpiryDateTime = securityCodeExpiryDateTime;
            SecurityCodeCreatedDateTime = VirtualDateTime.Now;
        }

        public bool HasSecurityCodeExpired()
        {
            return SecurityCodeExpiryDateTime < VirtualDateTime.Now;
        }
        #endregion

        #region Registration
        public string GetRegistrationStatus()
        {
            var reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINConfirmedDate != null);
            if (reg != null) return $"Registered {reg.PINConfirmedDate?.ToFriendly(false)}";

            reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINSentDate != null && uo.PINConfirmedDate == null);
            if (reg != null) return "Awaiting PIN";

            reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo =>
                    uo.PINSentDate == null && uo.PINConfirmedDate == null && uo.Method == RegistrationMethods.Manual);
            if (reg != null) return "Awaiting Approval";

            return "No registrations";
        }

        public UserOrganisation GetLatestRegistration()
        {
            return UserOrganisations.Where(uo => uo.PINConfirmedDate != null)
                .OrderByDescending(uo => uo.PINConfirmedDate)
                .FirstOrDefault();
        }
        #endregion

    }
}