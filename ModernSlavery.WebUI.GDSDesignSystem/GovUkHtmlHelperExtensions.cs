using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators;
using ModernSlavery.WebUI.GDSDesignSystem.Interfaces;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem
{
    public static class GovUkHtmlHelperExtensions
    {
        public static IHtmlContent GovUkBackLink(
            this IHtmlHelper htmlHelper,
            BackLinkViewModel backLinkViewModel)
        {
            return htmlHelper.Partial("~/Partials/BackLink.cshtml", backLinkViewModel);
        }

        public static IHtmlContent GovUkBreadcrumbs(
            this IHtmlHelper htmlHelper,
            BreadcrumbsViewModel breadcrumbsViewModel)
        {
            return htmlHelper.Partial("~/Partials/Breadcrumbs.cshtml", breadcrumbsViewModel);
        }

        public static IHtmlContent GovUkButton(
            this IHtmlHelper htmlHelper,
            ButtonViewModel buttonViewModel)
        {
            return htmlHelper.Partial("~/Partials/Button.cshtml", buttonViewModel);
        }

        public static IHtmlContent GovUkCharacterCount(
            this IHtmlHelper htmlHelper,
            CharacterCountViewModel characterCountViewModel)
        {
            return htmlHelper.Partial("~/Partials/CharacterCount.cshtml", characterCountViewModel);
        }

        public static IHtmlContent GovUkCharacterCountFor<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> propertyLambdaExpression,
            int? rows = null,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null)
            where TModel : GovUkViewModel
        {
            return CharacterCountHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                rows,
                labelOptions,
                hintOptions,
                formGroupOptions);
        }

        public static IHtmlContent GovUkCheckboxes(
            this IHtmlHelper htmlHelper,
            CheckboxesViewModel checkboxesViewModel)
        {
            return htmlHelper.Partial("~/Partials/Checkboxes.cshtml", checkboxesViewModel);
        }

        public static IHtmlContent GovUkCheckboxesFor<TModel, TPropertyListItem>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, List<TPropertyListItem>>> propertyLambdaExpression,
            FieldsetViewModel fieldsetOptions = null,
            HintViewModel hintOptions = null,
            Dictionary<TPropertyListItem, Func<object, object>> conditionalOptions = null
        )
            where TModel : GovUkViewModel
            where TPropertyListItem : struct, IConvertible // A fairly good check that TPropertyListItem is an Enum
        {
            return CheckboxesHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                fieldsetOptions,
                hintOptions,
                conditionalOptions);
        }

        public static IHtmlContent GovUkCheckboxItem(
            this IHtmlHelper htmlHelper,
            CheckboxItemViewModel checkboxItemViewModel)
        {
            return htmlHelper.Partial("~/Partials/CheckboxItem.cshtml", checkboxItemViewModel);
        }

        public static IHtmlContent GovUkCheckboxItemFor<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, bool>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            Conditional conditional = null,
            bool disabled = false)
        {
            return CheckboxItemHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                labelOptions,
                hintOptions,
                conditional,
                disabled);
        }

        public static IHtmlContent GovUkErrorMessage(
            this IHtmlHelper htmlHelper,
            ErrorMessageViewModel errorMessageViewModel)
        {
            return htmlHelper.Partial("~/Partials/ErrorMessage.cshtml", errorMessageViewModel);
        }

        public static IHtmlContent GovUkErrorSummary(
            this IHtmlHelper htmlHelper,
            ErrorSummaryViewModel errorSummaryViewModel)
        {
            return htmlHelper.Partial("~/Partials/ErrorSummary.cshtml", errorSummaryViewModel);
        }

        public static IHtmlContent GovUkErrorSummary<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            string[] optionalOrderOfPropertyNamesInTheView = null)
            where TModel : GovUkViewModel
        {
            // Give 'optionalOrderOfPropertiesInTheView' a default value (of an empty array)
            var orderOfPropertyNamesInTheView = optionalOrderOfPropertyNamesInTheView ?? new string[0];

            return ErrorSummaryHtmlGenerator.GenerateHtml(htmlHelper, orderOfPropertyNamesInTheView);
        }

        public static IHtmlContent GovUkFieldset(
            this IHtmlHelper htmlHelper,
            FieldsetViewModel fieldsetViewModel)
        {
            return htmlHelper.Partial("~/Partials/Fieldset.cshtml", fieldsetViewModel);
        }

        public static IHtmlContent GovUkFooter(
            this IHtmlHelper htmlHelper,
            FooterViewModel footerViewModel)
        {
            return htmlHelper.Partial("~/Partials/Footer.cshtml", footerViewModel);
        }

        public static IHtmlContent GovUkHeader(
            this IHtmlHelper htmlHelper,
            HeaderViewModel headerViewModel)
        {
            return htmlHelper.Partial("~/Partials/Header.cshtml", headerViewModel);
        }

        public static IHtmlContent GovUkHint(
            this IHtmlHelper htmlHelper,
            HintViewModel hintViewModel)
        {
            return htmlHelper.Partial("~/Partials/Hint.cshtml", hintViewModel);
        }

        public static IHtmlContent GovUkHtmlText(
            this IHtmlHelper htmlHelper,
            IHtmlText htmlText)
        {
            return htmlHelper.Partial("~/Partials/HtmlText.cshtml", htmlText);
        }

        public static IHtmlContent GovUkInsetText(
            this IHtmlHelper htmlHelper,
            InsetTextViewModel insetTextViewModel)
        {
            return htmlHelper.Partial("~/Partials/InsetText.cshtml", insetTextViewModel);
        }

        public static IHtmlContent GovUkItem(
            this IHtmlHelper htmlHelper,
            ItemViewModel itemViewModel)
        {
            return htmlHelper.Partial("~/Partials/Item.cshtml", itemViewModel);
        }

        public static IHtmlContent GovUkItemSet(
            this IHtmlHelper htmlHelper,
            ItemSetViewModel itemSetViewModel)
        {
            return htmlHelper.Partial("~/Partials/ItemSet.cshtml", itemSetViewModel);
        }

        public static IHtmlContent GovUkLabel(
            this IHtmlHelper htmlHelper,
            LabelViewModel labelViewModel)
        {
            return htmlHelper.Partial("~/Partials/Label.cshtml", labelViewModel);
        }

        public static IHtmlContent GovUkLegend(
            this IHtmlHelper htmlHelper,
            LegendViewModel legendViewModel)
        {
            return htmlHelper.Partial("~/Partials/Legend.cshtml", legendViewModel);
        }

        public static IHtmlContent GovUkPhaseBanner(
            this IHtmlHelper htmlHelper,
            PhaseBannerViewModel phaseBannerViewModel)
        {
            return htmlHelper.Partial("~/Partials/PhaseBanner.cshtml", phaseBannerViewModel);
        }

        public static IHtmlContent GovUkRadios(
            this IHtmlHelper htmlHelper,
            RadiosViewModel radioItemViewModel)
        {
            return htmlHelper.Partial("~/Partials/Radios.cshtml", radioItemViewModel);
        }

        public static IHtmlContent GovUkRadiosFor<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression,
            FieldsetViewModel fieldsetOptions = null,
            HintViewModel hintOptions = null)
            where TModel : GovUkViewModel
        {
            return RadiosHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                fieldsetOptions,
                hintOptions);
        }

        public static IHtmlContent GovUkRadioItem(
            this IHtmlHelper htmlHelper,
            RadioItemViewModel radioItemViewModel)
        {
            return htmlHelper.Partial("~/Partials/RadioItem.cshtml", radioItemViewModel);
        }

        public static IHtmlContent GovUkTag(
            this IHtmlHelper htmlHelper,
            TagViewModel tagViewModel)
        {
            return htmlHelper.Partial("~/Partials/Tag.cshtml", tagViewModel);
        }

        public static IHtmlContent GovUkTextArea(
            this IHtmlHelper htmlHelper,
            TextAreaViewModel textAreaViewModel)
        {
            return htmlHelper.Partial("~/Partials/TextArea.cshtml", textAreaViewModel);
        }

        public static IHtmlContent GovUkTextAreaFor<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> propertyLambdaExpression,
            int? rows = null,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null)
            where TModel : GovUkViewModel
        {
            return TextAreaHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                rows,
                labelOptions,
                hintOptions,
                formGroupOptions);
        }

        public static IHtmlContent GovUkTextInput(
            this IHtmlHelper htmlHelper,
            TextInputViewModel textInputViewModel)
        {
            return htmlHelper.Partial("~/Partials/TextInput.cshtml", textInputViewModel);
        }

        public static IHtmlContent GovUkTextInputFor<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null,
            string classes = null,
            TextInputAppendixViewModel textInputAppendix = null)
            where TModel : GovUkViewModel
        {
            return TextInputHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                labelOptions,
                hintOptions,
                formGroupOptions,
                classes,
                textInputAppendix);
        }

        public static IHtmlContent GovUkTextInputFor<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, int?>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null,
            string classes = null,
            TextInputAppendixViewModel textInputAppendix = null)
            where TModel : GovUkViewModel
        {
            return TextInputHtmlGenerator.GenerateHtml(
                htmlHelper,
                propertyLambdaExpression,
                labelOptions,
                hintOptions,
                formGroupOptions,
                classes,
                textInputAppendix);
        }
    }
}