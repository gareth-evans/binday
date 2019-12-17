using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinDay.Extensions;
using Newtonsoft.Json.Linq;
using Shared;

namespace BinDay
{
    public class SouthBucksBinDayResolver
    {
        private readonly HttpClient _httpClient;
        private readonly ITableStoragePostcodeResolver _postcodeResolver;

        public SouthBucksBinDayResolver(
            IHttpClientFactory httpClientFactory,
            ITableStoragePostcodeResolver tableStoragePostcodeResolver)
        {
            _httpClient = httpClientFactory.CreateClient();
            _postcodeResolver = tableStoragePostcodeResolver;
            _southBucksCalendar = new HardcodedSouthBucksCalendar();
        }


        private readonly HardcodedSouthBucksCalendar _southBucksCalendar;

        public async Task<BinInfo> GetBinInfoAsync(string postcode, DateTime dateTime)
        {
            // get bin week

            var binWeek = _southBucksCalendar.BinDays.SingleOrDefault(week => week.dateRange.IsInRange(dateTime.Date));
            var binDayOfWeek = await GetBinDayForPostcodeAsync(postcode);
            var date = DateTime.Now.Date.GetDateOfTheNext(binDayOfWeek);
            
            // swap date if in substitute list

            return new BinInfo
            {
                Date = date,
                Description = binWeek.binColor
            };
        }

        public async Task<DayOfWeek> GetBinDayForPostcodeAsync(string postcode)
        {
            var postCodeInfo = _postcodeResolver.GetPostcodeInfo(new Postcode(postcode));

            var binDayOfWeek = await GetBinDayOfWeekAsync(
                $"{postCodeInfo.Eastings},{postCodeInfo.Northings},{postCodeInfo.Eastings},{postCodeInfo.Northings}");

            return binDayOfWeek;
        }

        private static readonly Regex DayOfWeekRegex = new Regex("Monday|Tueday|Wednesday|Thursday|Friday",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private async Task<DayOfWeek> GetBinDayOfWeekAsync(string locationId)
        {
            var url =
                $"https://isa.chiltern.gov.uk/localview/ServiceProxies/Feed/Rest.svc/SBDC%20Waste%20Rounds%20Calendar%20Search/Location/{locationId}?sortByDistance=true&format=jsonp";

            using (var response = await _httpClient.GetAsync(url))
            {
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                var title = jObject["channel"]["item"][0]["title"]?.Value<string>();

                var match = DayOfWeekRegex.Match(title);
                var day = match.Groups[0].Value;

                return Enum.Parse<DayOfWeek>(day);
            }
        }

        private static class Bins
        {
            public const string Grey = nameof(Grey);
            public const string Blue = nameof(Blue);
        }

        private static (DateRange dateRange, string binColor) CreateBinWeek(string start, string end, string binColor)
        {
            return (new DateRange(start, end), binColor);
        }

        public string GetBinName(DateTime date)
        {
            var justDate = date.Date;

            foreach (var (range, binColor) in _southBucksCalendar.BinDays)
            {
                if (range.IsInRange(justDate))
                {
                    return binColor;
                }
            }

            return null;
        }


        public interface IHardcodedSouthBucksCalendar
        {
            IDictionary<DateTime, DateTime> DayChanges { get; }
            IList<(DateRange dateRange, string binColor)> BinDays { get; }
        }

        public class HardcodedSouthBucksCalendar : IHardcodedSouthBucksCalendar
        {
            public IDictionary<DateTime, DateTime> DayChanges { get; } = new Dictionary<DateTime, DateTime>
            {
                // Christmas
                {DateTime.Parse("2019-12-25"), DateTime.Parse("2019-12-27")},
                {DateTime.Parse("2019-12-26"), DateTime.Parse("2019-12-28")},
                {DateTime.Parse("2019-12-27"), DateTime.Parse("2019-12-30")},
                {DateTime.Parse("2019-12-30"), DateTime.Parse("2019-12-31")},
                {DateTime.Parse("2019-12-31"), DateTime.Parse("2020-01-02")},
                {DateTime.Parse("2020-01-01"), DateTime.Parse("2020-01-03")},
                {DateTime.Parse("2020-01-02"), DateTime.Parse("2020-01-04")},
                {DateTime.Parse("2020-01-03"), DateTime.Parse("2020-01-06")},
                {DateTime.Parse("2020-01-06"), DateTime.Parse("2020-01-07")},
                {DateTime.Parse("2020-01-07"), DateTime.Parse("2020-01-08")},
                {DateTime.Parse("2020-01-08"), DateTime.Parse("2020-01-09")},
                {DateTime.Parse("2020-01-09"), DateTime.Parse("2020-01-10")},
                {DateTime.Parse("2020-01-10"), DateTime.Parse("2020-01-11")},

                // April
                {DateTime.Parse("2020-04-10"), DateTime.Parse("2020-04-11")},
                {DateTime.Parse("2020-04-13"), DateTime.Parse("2020-04-14")},
                {DateTime.Parse("2020-04-14"), DateTime.Parse("2020-04-15")},
                {DateTime.Parse("2020-04-15"), DateTime.Parse("2020-04-16")},
                {DateTime.Parse("2020-04-16"), DateTime.Parse("2020-04-17")},
                {DateTime.Parse("2020-04-17"), DateTime.Parse("2020-04-18")},

                // May
                {DateTime.Parse("2020-05-08"), DateTime.Parse("2020-05-09")},
                {DateTime.Parse("2020-05-25"), DateTime.Parse("2020-05-26")},
                {DateTime.Parse("2020-05-26"), DateTime.Parse("2020-05-27")},
                {DateTime.Parse("2020-05-27"), DateTime.Parse("2020-05-28")},
                {DateTime.Parse("2020-05-28"), DateTime.Parse("2020-05-29")},
                {DateTime.Parse("2020-05-29"), DateTime.Parse("2020-05-30")},

                // August
                {DateTime.Parse("2020-08-31"), DateTime.Parse("2020-09-01")},
                {DateTime.Parse("2020-09-01"), DateTime.Parse("2020-09-02")},
                {DateTime.Parse("2020-09-02"), DateTime.Parse("2020-09-03")},
                {DateTime.Parse("2020-09-03"), DateTime.Parse("2020-09-04")},
                {DateTime.Parse("2020-09-04"), DateTime.Parse("2020-09-05")},
            };

            public IList<(DateRange dateRange, string binColor)> BinDays { get; } =
                new List<(DateRange dateRange, string binColor)>
                {
                    CreateBinWeek("2019-12-02", "2019-12-06", Bins.Grey),
                    CreateBinWeek("2019-12-07", "2019-12-13", Bins.Blue),
                    CreateBinWeek("2019-12-14", "2019-12-20", Bins.Grey),
                    CreateBinWeek("2019-12-21", "2019-12-30", Bins.Blue),
                    CreateBinWeek("2019-12-31", "2020-01-06", Bins.Grey),
                    CreateBinWeek("2020-01-07", "2020-01-11", Bins.Blue),
                    CreateBinWeek("2020-01-12", "2020-01-17", Bins.Grey),
                    CreateBinWeek("2020-01-18", "2020-01-24", Bins.Blue),
                    CreateBinWeek("2020-01-25", "2020-01-31", Bins.Grey),
                    CreateBinWeek("2020-02-01", "2020-02-07", Bins.Blue),
                    CreateBinWeek("2020-02-08", "2020-02-14", Bins.Grey),
                    CreateBinWeek("2020-02-15", "2020-02-21", Bins.Blue),
                    CreateBinWeek("2020-02-22", "2020-02-28", Bins.Grey),
                    CreateBinWeek("2020-02-29", "2020-03-06", Bins.Blue),
                    CreateBinWeek("2020-03-07", "2020-03-13", Bins.Grey),
                    CreateBinWeek("2020-03-14", "2020-03-20", Bins.Blue),
                    CreateBinWeek("2020-03-21", "2020-03-27", Bins.Grey),
                    CreateBinWeek("2020-03-28", "2020-04-03", Bins.Blue),
                    CreateBinWeek("2020-04-04", "2020-04-11", Bins.Grey),
                    CreateBinWeek("2020-04-12", "2020-04-18", Bins.Blue),
                    CreateBinWeek("2020-04-19", "2020-04-24", Bins.Grey),
                    CreateBinWeek("2020-04-25", "2020-05-01", Bins.Blue),
                    CreateBinWeek("2020-05-02", "2020-05-09", Bins.Grey),
                    CreateBinWeek("2020-05-10", "2020-05-15", Bins.Blue),
                    CreateBinWeek("2020-05-16", "2020-05-22", Bins.Grey),
                    CreateBinWeek("2020-05-23", "2020-05-30", Bins.Blue),
                    CreateBinWeek("2020-05-31", "2020-06-05", Bins.Grey),
                    CreateBinWeek("2020-06-06", "2020-06-12", Bins.Blue),
                    CreateBinWeek("2020-06-13", "2020-06-19", Bins.Grey),
                    CreateBinWeek("2020-06-20", "2020-06-26", Bins.Blue),
                    CreateBinWeek("2020-06-27", "2020-07-03", Bins.Grey),
                    CreateBinWeek("2020-07-04", "2020-07-10", Bins.Blue),
                    CreateBinWeek("2020-07-11", "2020-07-17", Bins.Grey),
                    CreateBinWeek("2020-07-18", "2020-07-24", Bins.Blue),
                    CreateBinWeek("2020-07-25", "2020-07-31", Bins.Grey),
                    CreateBinWeek("2020-08-01", "2020-08-07", Bins.Blue),
                    CreateBinWeek("2020-08-08", "2020-08-14", Bins.Grey),
                    CreateBinWeek("2020-08-15", "2020-08-21", Bins.Blue),
                    CreateBinWeek("2020-08-22", "2020-08-28", Bins.Grey),
                    CreateBinWeek("2020-08-29", "2020-09-05", Bins.Blue),
                    CreateBinWeek("2020-09-06", "2020-09-11", Bins.Grey),
                    CreateBinWeek("2020-09-12", "2020-09-18", Bins.Blue),
                    CreateBinWeek("2020-09-19", "2020-09-25", Bins.Grey),
                    CreateBinWeek("2020-09-26", "2020-10-02", Bins.Blue),
                    CreateBinWeek("2020-10-03", "2020-10-09", Bins.Grey),
                    CreateBinWeek("2020-10-10", "2020-10-16", Bins.Blue),
                    CreateBinWeek("2020-10-17", "2020-10-23", Bins.Grey),
                    CreateBinWeek("2020-10-24", "2020-10-30", Bins.Blue),
                    CreateBinWeek("2020-10-31", "2020-11-06", Bins.Grey),
                    CreateBinWeek("2020-11-07", "2020-11-13", Bins.Blue),
                    CreateBinWeek("2020-11-14", "2020-11-20", Bins.Grey),
                    CreateBinWeek("2020-11-21", "2020-11-27", Bins.Blue),
                    CreateBinWeek("2020-11-28", "2020-11-30", Bins.Grey)
                };
        }
    }
}