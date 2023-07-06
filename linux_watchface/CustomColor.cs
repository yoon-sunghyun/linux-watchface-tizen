using System;
using Tizen.System;
using Xamarin.Forms;

namespace linux_watchface
{
    internal static class CustomColor
    {
        public static readonly Color Red      = Color.FromRgb(225, 110, 115);
        public static readonly Color Orange   = Color.FromRgb(210, 155, 100);
        public static readonly Color Yellow   = Color.FromRgb(230, 190, 125);
        public static readonly Color Green    = Color.FromRgb(150, 195, 120);
        public static readonly Color Blue     = Color.FromRgb( 95, 175, 240);
        public static readonly Color Purple   = Color.FromRgb(200, 120, 220);
        public static readonly Color DarkGray = Color.FromRgb( 40,  45,  50);
        public static readonly Color Gray     = Color.FromRgb( 75,  85, 100);
        public static readonly Color White    = Color.FromRgb(170, 180, 190);

        public static Color GetBatteryColor()
        {
            if (Battery.IsCharging)
                return Yellow;
            if (Battery.Level == BatteryLevelStatus.Low)
                return Orange;
            if (Battery.Level == BatteryLevelStatus.Critical)
                return Red;
            return Green;
        }

        public static Color GetDateColor(DateTime dateTime)
        {
            if (12 <= dateTime.Month || dateTime.Month < 3) // winter
                return Blue;
            if (3 <= dateTime.Month && dateTime.Month < 6)  // spring
                return Green;
            if (6 <= dateTime.Month && dateTime.Month < 9)  // summer
                return Orange;
            if (9 <= dateTime.Month && dateTime.Month < 12) // autumn
                return Red;
            return Gray;
        }

        public static Color GetTimeColor(DateTime dateTime)
        {
            if (dateTime.Hour < 5)  // night
                return Purple;
            if (dateTime.Hour < 6)  // twilight (sunrise)
                return Orange;
            if (dateTime.Hour < 18) // day
                return Blue;
            if (dateTime.Hour < 19) // twilight (sunset)
                return Orange;
            if (dateTime.Hour < 24) // night
                return Purple;
            return Gray;
        }

        public static Color GetTickColor(DateTime dateTime)
        {
            return (dateTime.Second % 2 == 0 ? White : Gray);
        }
    }
}
