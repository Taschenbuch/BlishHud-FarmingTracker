using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ScreenBoundariesService
    {
        public static Point AdjustLocationToKeepControlInsideScreenBoundaries(Point windowAnchorCoordinates, Point controlSize, Point screenSize, WindowAnchor windowAnchor)
        {
            var xIsControlLocation = windowAnchor == WindowAnchor.TopLeft || windowAnchor == WindowAnchor.BottomLeft;
            var yIsControlLocation = windowAnchor == WindowAnchor.TopLeft || windowAnchor == WindowAnchor.TopRight;
            var x = AdjustCoordinateToKeepControlInsideScreenBoundaries(windowAnchorCoordinates.X, controlSize.X, screenSize.X, xIsControlLocation);
            var y = AdjustCoordinateToKeepControlInsideScreenBoundaries(windowAnchorCoordinates.Y, controlSize.Y, screenSize.Y, yIsControlLocation);
            return new Point(x, y);
        }

        private static int AdjustCoordinateToKeepControlInsideScreenBoundaries(int windowAnchorCoordinate, int controlHeightOrWidth, int screenHeightOrWidth, bool isControlLocation)
        {
            var controlTopOrLeftCoordinate = isControlLocation
                ? windowAnchorCoordinate
                : windowAnchorCoordinate - controlHeightOrWidth;

            var controlBottomOrRight = controlTopOrLeftCoordinate + controlHeightOrWidth;

            var controlIsTooBigForScreenDimension = controlHeightOrWidth >= screenHeightOrWidth;
            if (controlIsTooBigForScreenDimension)
                return isControlLocation 
                    ? 0
                    : screenHeightOrWidth;

            var controlIsOutsideOfTopOrLeftScreenBoundary = controlTopOrLeftCoordinate <= 0;
            if (controlIsOutsideOfTopOrLeftScreenBoundary)
                return isControlLocation
                    ? 0
                    : controlHeightOrWidth;

            var controlIsOutsideOfBottomOrRightScreenBoundary = controlBottomOrRight > screenHeightOrWidth;
            if (controlIsOutsideOfBottomOrRightScreenBoundary)
                return isControlLocation
                    ? screenHeightOrWidth - controlHeightOrWidth
                    : screenHeightOrWidth;

            return windowAnchorCoordinate;
        }
    }
}