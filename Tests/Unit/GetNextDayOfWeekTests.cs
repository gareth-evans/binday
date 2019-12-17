using System;
using BinDay.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Tests.Unit
{
    public class GetNextDayOfWeekTests
    {
        [TestCase("2019-12-04", DayOfWeek.Wednesday, "2019-12-04")]
        [TestCase("2019-12-04", DayOfWeek.Friday, "2019-12-06")]
        [TestCase("2019-12-04", DayOfWeek.Sunday, "2019-12-08")]
        [TestCase("2019-12-04", DayOfWeek.Monday, "2019-12-09")]
        [TestCase("2019-12-04", DayOfWeek.Tuesday, "2019-12-10")]
        public void Should_get_next_day_of_week_after_date(string date, DayOfWeek dayOfWeek, string expected)
        {
            var source = DateTime.Parse(date);
            var expectedDate = DateTime.Parse(expected);
            var result = source.GetDateOfTheNext(dayOfWeek);

            result.ShouldBe(expectedDate);
        }
    }
}