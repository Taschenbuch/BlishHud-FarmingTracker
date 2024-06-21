using System.Collections.Generic;

namespace FarmingTracker
{
    public class FileModel
    {
        public List<FileStat> FileItems { get; set; } = new List<FileStat>();
        public List<FileStat> FileCurrencies { get; set; } = new List<FileStat>();
        public List<int> IgnoredItemApiIds { get; set; } = new List<int>();
        public List<int> FavoriteItemApiIds { get; set; } = new List<int>();
    }
}
