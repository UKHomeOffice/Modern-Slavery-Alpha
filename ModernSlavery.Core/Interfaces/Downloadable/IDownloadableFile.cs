﻿using System;

namespace ModernSlavery.Core.Interfaces.Downloadable
{
    public interface IDownloadableFile : IDownloadableItem
    {
        string Type { get; }
        string Title { get; }
        string Description { get; }
        DateTime? Modified { get; set; }
    }
}