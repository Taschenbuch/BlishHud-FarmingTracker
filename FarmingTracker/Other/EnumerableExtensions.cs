using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }
    }
}
