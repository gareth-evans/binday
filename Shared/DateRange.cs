using System;

namespace Shared
{
    public class DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateRange(string start, string end) 
            : this(DateTime.Parse(start), DateTime.Parse(end))
        {
        }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public bool IsInRange(DateTime date)
        {
            return date >= Start && date <= End;
        }
    }
}