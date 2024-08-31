namespace FarmingTracker
{
    // do not remove this. this is stored in a blish setting
    public class FloatPoint
    {
        public FloatPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"x: {X} y: {Y}";
        }

        public float X { get; set; }
        public float Y { get; set; }
    }
}
