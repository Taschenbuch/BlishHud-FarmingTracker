using System;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class FileModel
    {
        public TimeSpan FarmingDuration { get; set; }
        public List<FileStat> FileItems { get; set; } = new List<FileStat>();
        public List<FileStat> FileCurrencies { get; set; } = new List<FileStat>();
    }
}
