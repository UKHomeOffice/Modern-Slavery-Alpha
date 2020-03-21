﻿using System;
using System.Collections.Generic;

namespace ModernSlavery.WebUI.Models.Search
{
    [Serializable]
    public class DownloadViewModel
    {

        public List<Download> Downloads { get; set; }

        public class Download
        {

            public string Title { get; set; }
            public string Count { get; set; }
            public string Size { get; set; }
            public string Url { get; set; }
            public string Extension { get; set; }

        }

    }
}
