using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class SearchService
    {
        public static (List<Stat> items, List<Stat> currencies) FilterBySearchTerm(List<Stat> items, List<Stat> currencies, string searchTerm)
        {
            var searchTermExists = !string.IsNullOrWhiteSpace(searchTerm);
            if (searchTermExists)
            {
                currencies = FilterBySearchTerm(currencies, searchTerm);
                items = FilterBySearchTerm(items, searchTerm);
            }

            return (items, currencies);
        }

        private static List<Stat> FilterBySearchTerm(List<Stat> stats, string searchTerm)
        {
            searchTerm = searchTerm.ToLower().Trim();

            return stats
                .Where(s => s.Details.Name.ToLower().Contains(searchTerm))
                .ToList();
        }
    }
}
