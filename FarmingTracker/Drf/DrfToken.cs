using System;
using System.Text.RegularExpressions;

namespace FarmingTracker
{
    public class DrfToken
    {
        public static string CreateDrfTokenHintText(string drfToken)
        {
            var drfTokenFormat = ValidateFormat(drfToken);

            switch (drfTokenFormat)
            {
                case DrfTokenFormat.ValidFormat:
                    return "";
                case DrfTokenFormat.InvalidFormat:
                    return "Incomplete or invalid DRF Token format.\nExpected format:\nxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\nwith x = a-f, 0-9";
                case DrfTokenFormat.EmptyToken:
                    return "DRF Token required.\nModule wont work without it.";
                default:
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(drfTokenFormat, nameof(DrfTokenFormat), "no hint"));
                    return "";
            }
        }

        public static bool HasValidFormat(string drfToken)
        {
            return ValidateFormat(drfToken) == DrfTokenFormat.ValidFormat;
        }

        public static DrfTokenFormat ValidateFormat(string drfToken)
        {
            if (string.IsNullOrWhiteSpace(drfToken))
                return DrfTokenFormat.EmptyToken;
            
            try
            {
                if (!_drfTokenRegex.IsMatch(drfToken))
                    return DrfTokenFormat.InvalidFormat;
            }
            catch (RegexMatchTimeoutException)
            {
                Module.Logger.Error($"regex timedout for drf {nameof(_drfTokenRegex)}.");
                return DrfTokenFormat.InvalidFormat;
            }

            if (!_drfTokenRegex.IsMatch(drfToken))
                return DrfTokenFormat.InvalidFormat;

            return DrfTokenFormat.ValidFormat;
        }

        private static readonly Regex _drfTokenRegex 
            = new Regex(@"^[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{12}$", RegexOptions.None, TimeSpan.FromMilliseconds(1000));
    }
}
