﻿using System;
using System.Collections.Generic;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("SecurityHeaders")]

    public class SecurityHeaderOptions: Dictionary<string, string>, IOptions
    {
        public SecurityHeaderOptions():base(StringComparer.OrdinalIgnoreCase)
        {

        }

    }
}