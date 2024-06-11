using System;
using System.Diagnostics;

namespace FarmingTracker
{
    public class FarmingDuration
    {
        public FarmingDuration()
        {
            _stopwatch.Restart();
        }

        public TimeSpan Elapsed 
        { 
            get => _value + _stopwatch.Elapsed; 
            set => _value = value; 
        }

        public void Restart()
        {
            _stopwatch.Restart();
            _value = TimeSpan.Zero;
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private TimeSpan _value;
    }
}
