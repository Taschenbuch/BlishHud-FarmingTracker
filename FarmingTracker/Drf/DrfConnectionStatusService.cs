using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class DrfConnectionStatusService
    {
        public static Color GetDrfConnectionStatusTextColor(DrfConnectionStatus drfConnectionStatus)
        {
            switch (drfConnectionStatus)
            {
                case DrfConnectionStatus.Connecting:
                    return Color.Yellow;
                case DrfConnectionStatus.Connected:
                    return Color.LightGreen;
                case DrfConnectionStatus.Disconnected:
                case DrfConnectionStatus.AuthenticationFailed:
                    return RED;
                default:
                    Module.Logger.Error($"Fallback: white. Because switch case missing or should not be be handled here: {nameof(drfConnectionStatus)}.{drfConnectionStatus}.");
                    return RED;
            }
        }

        public static string GetDrfConnectionStatusText(DrfConnectionStatus drfConnectionStatus)
        {
            var smileyVerticalSpace = "  ";
            switch (drfConnectionStatus)
            {
                case DrfConnectionStatus.Disconnected:
                    return $"Disconnected{smileyVerticalSpace}:-(";
                case DrfConnectionStatus.Connecting:
                    return "Connecting...";
                case DrfConnectionStatus.Connected:
                    return $"Connected{smileyVerticalSpace}:-)";
                case DrfConnectionStatus.AuthenticationFailed:
                    return $"Authentication failed. Add a valid DRF Token!{smileyVerticalSpace}:-(";
                case DrfConnectionStatus.ModuleError:
                    return $"Module Error.{smileyVerticalSpace}:-( Report bug on Discord: https://discord.com/invite/FYKN3qh";
                default:
                    Module.Logger.Error($"Fallback: Unknown Status. Because switch case missing or should not be be handled here: {nameof(drfConnectionStatus)}.{drfConnectionStatus}.");
                    return $"Unknown Status.{smileyVerticalSpace}:-(";
            }
        }

        private static readonly Color RED = new Color(255, 120, 120);
    }
}
