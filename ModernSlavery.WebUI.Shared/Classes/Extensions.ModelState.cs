﻿using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Reflection;
using System.ComponentModel;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public static partial class Extensions
    {
        public static void CleanModelErrors<TModel>(this Controller controller)
        {
            Type containerType = typeof(TModel);
            //Save the old modelstate
            var oldModelState = new ModelStateDictionary();
            foreach (KeyValuePair<string, ModelStateEntry> modelState in controller.ModelState)
            {
                string propertyName = modelState.Key;
                foreach (ModelError error in modelState.Value.Errors)
                {
                    bool exists = oldModelState.Any(
                        m => m.Key == propertyName && m.Value.Errors.Any(e => e.ErrorMessage == error.ErrorMessage));

                    //add the inline message if it doesnt already exist
                    if (!exists)
                    {
                        oldModelState.AddModelError(propertyName, error.ErrorMessage);
                    }
                }
            }

            //Clear the model state ready for refill
            controller.ModelState.Clear();

            foreach (KeyValuePair<string, ModelStateEntry> modelState in oldModelState)
            {
                //Get the property name
                string propertyName = modelState.Key;

                //Get the validation attributes
                PropertyInfo propertyInfo = string.IsNullOrWhiteSpace(propertyName) ? null : containerType.GetPropertyInfo(propertyName);
                List<ValidationAttribute> attributes = propertyInfo == null
                    ? null
                    : propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), false).ToList<ValidationAttribute>();

                //Get the display name
                var displayNameAttribute =
                    propertyInfo?.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
                var displayAttribute =
                    propertyInfo?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                string displayName = displayNameAttribute != null ? displayNameAttribute.DisplayName :
                    displayAttribute != null ? displayAttribute.Name : propertyName;

                foreach (ModelError error in modelState.Value.Errors)
                {
                    string title = string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;
                    string description = !string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;

                    if (error.ErrorMessage.Like("The value * is not valid for *."))
                    {
                        title = "There's a problem with your values.";
                        description = "The value here is invalid.";
                    }

                    if (attributes == null || !attributes.Any())
                    {
                        goto addModelError;
                    }

                    ValidationAttribute attribute = attributes.FirstOrDefault(a => a.FormatErrorMessage(displayName) == error.ErrorMessage);
                    if (attribute == null)
                    {
                        goto addModelError;
                    }

                    string validatorKey = $"{containerType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                    CustomErrorMessage customError = CustomErrorMessages.GetValidationError(validatorKey);
                    if (customError == null)
                    {
                        goto addModelError;
                    }

                    title = attribute.FormatError(customError.Title, displayName);
                    description = attribute.FormatError(customError.Description, displayName);

                addModelError:

                    //add the summary message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(title)
                        && !controller.ModelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                    {
                        controller.ModelState.AddModelError("", title);
                    }

                    //add the inline message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(description)
                        && !string.IsNullOrWhiteSpace(propertyName)
                        && !controller.ModelState.Any(
                            m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                    {
                        controller.ModelState.AddModelError(propertyName, description);
                    }
                }
            }
        }

        //Removes all but the specified properties from the model state
        public static void Include(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (string key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (properties.ContainsI(key))
                {
                    continue;
                }

                modelState.Remove(key);
            }
        }

        //Removes all the specified properties from the model state
        public static void Exclude(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (string key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!properties.ContainsI(key))
                {
                    continue;
                }

                modelState.Remove(key);
            }
        }

        public static void AddModelError(this ModelStateDictionary modelState,
            int errorCode,
            string propertyName = null,
            object parameters = null)
        {
            //Try and get the custom error
            CustomErrorMessage customError = CustomErrorMessages.GetError(errorCode);
            if (customError == null)
            {
                throw new ArgumentException("errorCode", "Cannot find custom error message with this code");
            }

            //Add the error to the modelstate
            string title = customError.Title;
            string description = customError.Description;

            //Resolve the parameters
            if (parameters != null)
            {
                title = parameters.Resolve(title);
                description = parameters.Resolve(description);
            }

            //add the summary message if it doesnt already exist
            if (!string.IsNullOrWhiteSpace(title) && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
            {
                modelState.AddModelError("", title);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                //If no property then add description as second line of summary
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    if (!string.IsNullOrWhiteSpace(title)
                        && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                    {
                        modelState.AddModelError("", title);
                    }
                }

                //add the inline message if it doesnt already exist
                else if (!modelState.Any(m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                {
                    modelState.AddModelError(propertyName, description);
                }
            }
        }
    }
}
