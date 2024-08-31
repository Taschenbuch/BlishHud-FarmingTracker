using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ConvertCoordinatesService
    {
        public static Point ConvertRelativeToAbsoluteCoordinates(FloatPoint relative, Point screenSize)
        {
            return ConvertRelativeToAbsoluteCoordinates(relative.X, relative.Y, screenSize.X, screenSize.Y);
        }

        public static Point ConvertRelativeToAbsoluteCoordinates(float xRelative, float yRelative, int screenWidth, int screenHeight)
        {
            var xAbsolute = (int)(screenWidth * xRelative);
            var yAbsolute = (int)(screenHeight * yRelative);
            return new Point(xAbsolute, yAbsolute);
        }

        public static FloatPoint ConvertAbsoluteToRelativeCoordinates(Point absolute, Point screenSize)
        {
            var relative = ConvertAbsoluteToRelativeCoordinates(absolute.X, absolute.Y, screenSize.X, screenSize.Y);
            return new FloatPoint(relative.xRelative, relative.yRelative);
        }

        public static (float xRelative, float yRelative) ConvertAbsoluteToRelativeCoordinates(int xAbsolute, int yAbsolute, int screenWidth, int screenHeight)
        {
            var xRelative = ConvertAbsoluteToRelativeCoordinate(xAbsolute, screenWidth);
            var yRelative = ConvertAbsoluteToRelativeCoordinate(yAbsolute, screenHeight);
            return (xRelative, yRelative);
        }

        private static float ConvertAbsoluteToRelativeCoordinate(int absolute, int screenWidthOrHeight)
        {
            var hasRealisticScreenDimensions = screenWidthOrHeight > 0; 
            return hasRealisticScreenDimensions
                ? (float)absolute / screenWidthOrHeight
                : 0; // this probably messes up coordinates though. it resets them to left and/or top. may happen when SpriteScreen is initalized to x: 0, y: 0 at first.
        }
    }
}