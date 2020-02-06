﻿using System;

namespace ModernSlavery.WebUI.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PartialAttribute : Attribute
    {

        public PartialAttribute(string partialPath)
        {
            PartialPath = partialPath;
        }

        public string PartialPath { get; set; }

    }

}
