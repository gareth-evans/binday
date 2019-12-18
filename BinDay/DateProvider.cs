using System;

namespace BinDay
{
    public class DateProvider : IDateProvider
    {
        public DateTime Today => DateTime.Today;
    }
}