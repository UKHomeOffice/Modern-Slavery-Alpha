﻿using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Hosts
{
    [Options("DataProtection")]
    public class DataProtectionOptions : IOptions
    {
        public string Type { get; set; }
        public string AzureConnectionString { get; set; }
        public string KeyName { get; set; }
        public string ApplicationDiscriminator { get; set; }
        public string Container { get; set; }
        public string? KeyFilepath { get; set; }
    }
}