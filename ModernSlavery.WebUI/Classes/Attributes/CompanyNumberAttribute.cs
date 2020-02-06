﻿using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core;

namespace ModernSlavery.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CompanyNumberAttribute : RegularExpressionAttribute
    {

        private const string pattern = @"^[0-9A-Za-z]{8}$";

        public CompanyNumberAttribute() : base(pattern)
        {
            ErrorMessage = Global.CompanyNumberRegexError;
        }

    }
}
