using System;

namespace FarmingTracker
{
    public class ExceptionService
    {
        public static string GetExceptionSummary(Exception e)
        {
            var summary = GetSingleExceptionSummary(e);
            var innerException = e.InnerException;
            while (innerException != null)
            {
                summary = $"{summary} -> {GetSingleExceptionSummary(innerException)}";
                innerException = innerException.InnerException;
            }

            return summary;
        }

        private static string GetSingleExceptionSummary(Exception e)
        {
            return $"{e.GetType().Name}: {e.Message}";
        }
    }
}
