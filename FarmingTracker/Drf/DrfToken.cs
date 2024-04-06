using System;
using System.Text.RegularExpressions;

namespace FarmingTracker
{
    public class DrfToken
    {
        public static bool HasValidFormat(string drfToken)
        {
            return ValidateFormat(drfToken) == DrfTokenFormat.ValidFormat;
        }

        public static DrfTokenFormat ValidateFormat(string drfToken)
        {
            if (string.IsNullOrWhiteSpace(drfToken))
                return DrfTokenFormat.EmptyToken;

            if (!_drfTokenRegex.IsMatch(drfToken))
                return DrfTokenFormat.InvalidFormat;

            return DrfTokenFormat.ValidFormat;
        }

        private static readonly Regex _drfTokenRegex = new Regex(@"^[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{12}$");
    }
}
