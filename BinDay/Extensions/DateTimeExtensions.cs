using System;

namespace BinDay.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetDateOfTheNext(this DateTime date, DayOfWeek dayOfWeek)
        {
            var shift = dayOfWeek - date.DayOfWeek < 0
                ? (int)(7 - date.DayOfWeek) + (int)dayOfWeek
                : dayOfWeek - date.DayOfWeek;

            return date.AddDays(shift);
        }
    }
}