using System;
using BinDay;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Tests.Unit
{
    public class DateFormatterTests
    {
        private readonly DateTime _today = DateTime.Parse("2019-12-17");

        private DateFormatter CreateSut()
        {
            var dateProviderMock= new Mock<IDateProvider>();

            dateProviderMock
                .Setup(x => x.Today)
                .Returns(_today);

            return new DateFormatter(dateProviderMock.Object);
        }

        [TestCase("2019-12-17", "Today")]
        [TestCase("2019-12-18", "Tomorrow")]
        [TestCase("2019-12-19", "on Thursday")]
        [TestCase("2019-12-20", "on Friday")]
        [TestCase("2019-12-21", "on Saturday")]
        [TestCase("2019-12-22", "on Sunday")]
        [TestCase("2019-12-23", "on Monday")]
        [TestCase("2019-12-24", "on Tuesday the 24th")]
        [TestCase("2020-01-01", "on Wednesday the 1st of January")]
        public void Should_provide_friendly_day_description(string date, string expected)
        {
            var binDay = DateTime.Parse(date);
            var formatter = CreateSut();

            var result = formatter.CreateFriendlyDescription(binDay);

            result.ShouldBe(expected);
        }
    }
}