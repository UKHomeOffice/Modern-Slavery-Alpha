using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUnconfirmedPinsController : BaseController
    {
        private readonly IDataRepository dataRepository;
        protected readonly INotificationService NotificationService;
        private readonly IOrganisationBusinessLogic organisationBusinessLogic;

        public AdminUnconfirmedPinsController(
            IOrganisationBusinessLogic organisationBusinessLogic,
            ILogger<AdminUnconfirmedPinsController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            this.organisationBusinessLogic = organisationBusinessLogic;
        }

        [HttpGet("unconfirmed-pins")]
        public async Task<IActionResult> UnconfirmedPins()
        {
            var model = await dataRepository.GetAll<UserOrganisation>()
                .Where(uo => uo.Method == RegistrationMethods.PinInPost)
                .Where(uo => uo.PINConfirmedDate == null)
                .Where(uo => uo.PIN != null)
                .OrderByDescending(uo => uo.PINConfirmedDate.Value)
                .ToListAsync();
            return View("../Admin/UnconfirmedPins", model);
        }

        [HttpGet("send-pin")]
        public async Task<IActionResult> SendPinWarning(long userId, long organisationId)
        {
            var userOrganisation = await dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            return View("../Admin/SendPinWarning", userOrganisation);
        }

        [HttpPost("send-pin")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPin(long userId, long organisationId)
        {
            var userOrganisation = await dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            if (userOrganisation.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) <
                VirtualDateTime.Now)
            {
                var newPin = organisationBusinessLogic.GeneratePINCode(false);
                userOrganisation.PIN = newPin;
            }

            userOrganisation.PINSentDate = VirtualDateTime.Now;
            await dataRepository.SaveChangesAsync();

            NotificationService.SendPinEmail(
                userOrganisation.User.EmailAddress,
                userOrganisation.PIN,
                userOrganisation.Organisation.OrganisationName);

            return View("../Admin/SendPinConfirmation", userOrganisation);
        }
    }
}