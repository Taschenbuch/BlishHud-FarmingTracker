using Blish_HUD;
using System;

namespace FarmingTracker
{
    public class Interval
    {
        public Interval(TimeSpan intervalTimeSpan)
        {
            // do not store times as timespans. timespan has good readability as parameter, but bad performance for comparison and other operations.
            _intervalInMilliseconds = intervalTimeSpan.TotalMilliseconds; 
            UpdateIntervalEnd();
        }

        public bool HasEnded()
        {
            if (_intervalEndInMilliseconds > GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds)
                return false;

            UpdateIntervalEnd();
            return true;
        }

        private void UpdateIntervalEnd()
        {
            _intervalEndInMilliseconds = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds + _intervalInMilliseconds;
        }

        private readonly double _intervalInMilliseconds;
        private double _intervalEndInMilliseconds;
    }
}
