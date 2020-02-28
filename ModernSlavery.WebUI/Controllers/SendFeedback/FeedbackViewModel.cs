﻿using System.Collections.Generic;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ModernSlavery.WebUI.Controllers.SendFeedback
{

    public class FeedbackViewModel : GovUkViewModel
    {
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "How easy is this service to use",
            NameWithinSentence = "how easy this service is to use"
        )]
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Select how easy this service is to use."
        )]
        public HowEasyIsThisServiceToUse? HowEasyIsThisServiceToUse { get; set; }

        public List<HowDidYouHearAboutGpg> HowDidYouHearAboutGpg { get; set; }
        
        public string OtherSourceText { get; set; }

        public List<WhyVisitGpgSite> WhyVisitGpgSite { get; set; }
        
        public string OtherReasonText { get; set; }

        public List<WhoAreYou> WhoAreYou { get; set; }
        
        public string OtherPersonText { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Details",
            NameWithinSentence = "details"
        )]
        [GovUkValidateCharacterCount(
            MaxCharacters = 2000
        )]
        public string Details { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }
    }

    public enum HowEasyIsThisServiceToUse
    {
        [GovUkRadioCheckboxLabelText(Text = "Very easy")]
        VeryEasy = 0,

        [GovUkRadioCheckboxLabelText(Text = "Easy")]
        Easy = 1,

        [GovUkRadioCheckboxLabelText(Text = "Neither easy nor difficult")]
        Neutral = 2,

        [GovUkRadioCheckboxLabelText(Text = "Difficult")]
        Difficult = 3,

        [GovUkRadioCheckboxLabelText(Text = "Very difficult")]
        VeryDifficult = 4
    }

    public enum HowDidYouHearAboutGpg
    {
        [GovUkRadioCheckboxLabelText(Text = "News article")]
        NewsArticle,

        [GovUkRadioCheckboxLabelText(Text = "Social media")]
        SocialMedia,

        [GovUkRadioCheckboxLabelText(Text = "Company intranet")]
        CompanyIntranet,

        [GovUkRadioCheckboxLabelText(Text = "Employer union")]
        EmployerUnion,

        [GovUkRadioCheckboxLabelText(Text = "Internet search for a company")]
        InternetSearch,

        [GovUkRadioCheckboxLabelText(Text = "Charity")]
        Charity,

        [GovUkRadioCheckboxLabelText(Text = "Lobby group")]
        LobbyGroup,

        [GovUkRadioCheckboxLabelText(Text = "By having to report Modern Slavery statement")]
        Report,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other
    }

    public enum WhyVisitGpgSite
    {
        [GovUkRadioCheckboxLabelText(Text = "I wanted to find out what Modern Slavery is")]
        FindOutAboutGpg,

        [GovUkRadioCheckboxLabelText(Text = "I reported my organisation's Modern Slavery statement")]
        ReportOrganisationGpgData,

        [GovUkRadioCheckboxLabelText(Text = "I wanted to understand how I can close my organisation's Modern Slavery")]
        CloseOrganisationGpg,

        [GovUkRadioCheckboxLabelText(Text = "I viewed a specific organisation's Modern Slavery")]
        ViewSpecificOrganisationGpg,

        [GovUkRadioCheckboxLabelText(Text = "I wanted to know what action other organisations are taking to eliminate Modern Slavery")]
        ActionsToCloseGpg,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other
    }

    public enum WhoAreYou
    {
        [GovUkRadioCheckboxLabelText(Text = "An employee interested in your organisation’s Modern Slavery statement?")]
        EmployeeInterestedInOrganisationData,

        [GovUkRadioCheckboxLabelText(Text = "A manager involved in Modern Slavery reporting or diversity and inclusion?")]
        ManagerInvolvedInGpgReport,

        [GovUkRadioCheckboxLabelText(Text = "A person responsible for reporting your organisation’s Modern Slavery?")]
        ResponsibleForReportingGpg,

        [GovUkRadioCheckboxLabelText(Text = "A person interested in Modern Slavery generally?")]
        PersonInterestedInGeneralGpg,

        [GovUkRadioCheckboxLabelText(Text = "A person interested in a specific organisation’s Modern Slavery?")]
        PersonInterestedInSpecificOrganisationGpg,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other
    }

}
