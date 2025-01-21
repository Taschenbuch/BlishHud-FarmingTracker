namespace FarmingTracker
{
    public class SearchService
    {
        public static bool IncludesSearchTerm(Stat stat, string searchTerm)
        {
            searchTerm = searchTerm.ToLower().Trim();
            var searchTermMissing = string.IsNullOrWhiteSpace(searchTerm);
            return searchTermMissing || stat.Details.Name.ToLower().Contains(searchTerm); // dont apply search when searchTerm is missing
        }
    }
}
