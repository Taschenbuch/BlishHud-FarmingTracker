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
                case DrfConnectionStatus.TryReconnect:
                    return Color.Yellow;
                case DrfConnectionStatus.Connected:
                    return Color.LightGreen;
                case DrfConnectionStatus.Disconnected:
                case DrfConnectionStatus.AuthenticationFailed:
                    return RED;
                default:
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(drfConnectionStatus, nameof(DrfConnectionStatus), "white"));
                    return RED;
            }
        }

        public static string GetSettingTabDrfConnectionStatusText(
            DrfConnectionStatus drfConnectionStatus, 
            int reconnectTriesCounter, 
            int reconnectDelaySeconds)
        {
            switch (drfConnectionStatus)
            {
                case DrfConnectionStatus.Disconnected:
                    return $"Disconnected{SMILEY_VERTICAL_SPACE}:-(";
                case DrfConnectionStatus.Connecting:
                    return "Connecting...";
                case DrfConnectionStatus.TryReconnect:
                    return $"Connect failed {reconnectTriesCounter} time(s). Next reconnect try in {reconnectDelaySeconds} seconds.";
                case DrfConnectionStatus.Connected:
                    return $"Connected{SMILEY_VERTICAL_SPACE}:-)";
                case DrfConnectionStatus.AuthenticationFailed:
                    return $"Authentication failed. Add a valid DRF Token!{SMILEY_VERTICAL_SPACE}:-(";
                case DrfConnectionStatus.ModuleError:
                    return $"Module Error.{SMILEY_VERTICAL_SPACE}:-( Report bug on Discord:\nhttps://discord.com/invite/FYKN3qh";
                default:
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(drfConnectionStatus, nameof(DrfConnectionStatus), "unknown status"));
                    return $"Unknown Status.{SMILEY_VERTICAL_SPACE}:-(";
            }
        }

        public static string GetSummaryTabDrfConnectionStatusText(DrfConnectionStatus drfConnectionStatus)
        {
            return drfConnectionStatus switch
            {
                DrfConnectionStatus.Disconnected => $"DRF disconnected{SMILEY_VERTICAL_SPACE}:-(",
                DrfConnectionStatus.Connecting => $"DRF connecting...",
                DrfConnectionStatus.TryReconnect => $"DRF connect failed. Next reconnect try in a few seconds{SMILEY_VERTICAL_SPACE}:-(",
                DrfConnectionStatus.AuthenticationFailed => $"DRF authentication failed. Add a valid DRF Token!{SMILEY_VERTICAL_SPACE}:-(",
                DrfConnectionStatus.ModuleError => $"Module Error.{SMILEY_VERTICAL_SPACE}:-( Report bug on Discord:\nhttps://discord.com/invite/FYKN3qh",
                _ => "",
            };
        }

        private const string SMILEY_VERTICAL_SPACE = "  ";
        private static readonly Color RED = new Color(255, 120, 120);
    }
}
