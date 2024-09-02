using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class WindowAnchorService
    {
        public static Point ConvertBetweenControlAndWindowAnchorLocation(Point location, Point controlSize, ConvertLocation convertLocation, WindowAnchor windowAnchor)
        {
            var xIsControlLocation = windowAnchor == WindowAnchor.TopLeft || windowAnchor == WindowAnchor.BottomLeft;
            var yIsControlLocation = windowAnchor == WindowAnchor.TopLeft || windowAnchor == WindowAnchor.TopRight;
            var x = ConvertCoordinate(location.X, controlSize.X, xIsControlLocation, convertLocation);
            var y = ConvertCoordinate(location.Y, controlSize.Y, yIsControlLocation, convertLocation);
            return new Point(x, y);
        }

        private static int ConvertCoordinate(int coordinate, int controlWidthOrHeight, bool isControlLocation, ConvertLocation convertLocation)
        {
            if (isControlLocation)
                return coordinate;

            return convertLocation == ConvertLocation.ToWindowAnchorLocation
                ? coordinate + controlWidthOrHeight
                : coordinate - controlWidthOrHeight;
        }
    }
}
