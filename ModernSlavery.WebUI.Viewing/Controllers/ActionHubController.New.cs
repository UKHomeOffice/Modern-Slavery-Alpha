﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Viewing.Controllers
{
    [Route("actions-to-close-the-gap")]
    public partial class ActionHubController : BaseController
    {
        #region Constructors

        public ActionHubController(
            ILogger<ActionHubController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) :
            base(logger, webService, sharedBusinessLogic)
        {
        }

        #endregion

        private bool UseNewActionHub()
        {
            return VirtualDateTime.Now >= SharedBusinessLogic.SharedOptions.ActionHubSwitchOverDate;
        }

        // GET: ActionHub
        [HttpGet]
        public IActionResult Overview()
        {
            if (UseNewActionHub()) return Overview2();

            return Overview1();
        }

        [NonAction]
        public IActionResult Overview2()
        {
            //This is required so the tagHelper looks up based on this action name not the actual OverviewAction
            RouteData.Values.Add("SitemapAction", "Overview2");
            return View("/Views/ActionHub2/Overview.cshtml");
        }

        [HttpGet("leadership-and-accountability")]
        public IActionResult Leadership()
        {
            if (UseNewActionHub()) return View("/Views/ActionHub2/Leadership.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("hiring-and-selection")]
        public IActionResult Hiring()
        {
            if (UseNewActionHub()) return View("/Views/ActionHub2/Hiring.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("talent-management-learning-and-development")]
        public IActionResult Talent()
        {
            if (UseNewActionHub()) return View("/Views/ActionHub2/Talent.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("workplace-flexibility")]
        public IActionResult Workplace()
        {
            if (UseNewActionHub()) return View("/Views/ActionHub2/Workplace.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("further-reading")]
        public IActionResult Reading()
        {
            if (UseNewActionHub()) return View("/Views/ActionHub2/Reading.cshtml");

            return new HttpNotFoundResult();
        }
    }
}