using System;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BinDay
{
    public interface IDateFormatter
    {
        string CreateFriendlyDescription(DateTime date);
    }

    public class DateFormatter : IDateFormatter
    {
        private readonly IDateProvider _dateProvider;

        public DateFormatter(IDateProvider dateProvider)
        {
            _dateProvider = dateProvider;
        }

        public string CreateFriendlyDescription(DateTime date)
        {
            if (date == _dateProvider.Today) return "Today";
            if (date == _dateProvider.Today.AddDays(1)) return "Tomorrow";
            if(date < _dateProvider.Today.AddDays(7)) return $"on {date.DayOfWeek}";

            if (date.Month == _dateProvider.Today.Month)
            {
                return $"on {date.DayOfWeek} the {date.Day}{GetDateSuffix(date)}";
            }

            return $"on {date.DayOfWeek} the {date.Day}{GetDateSuffix(date)} of {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month)}";
        }

        private string GetDateSuffix(DateTime date)
        {
            switch (date.Day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}