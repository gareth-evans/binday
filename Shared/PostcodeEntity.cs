using Microsoft.Azure.Cosmos.Table;

namespace Shared
{
    public class PostcodeEntity : TableEntity
    {
        public string Postcode { get; set; }
        public int Eastings { get; set; }
        public int Northings { get; set; }
        public string AdminDistrictCode { get; set; }
        public string AdminDistrictName { get; set; }

        public PostcodeEntity()
        {
        }

        public PostcodeEntity(
            string postcode,
            int eastings,
            int northings,
            string adminDistrictCode,
            string adminDistrictName)
        {
            var match = PostCodeRegex.Match(postcode);
            PartitionKey = match.Groups[3].Value;
            RowKey = postcode;
            Postcode = postcode;
            Eastings = eastings;
            Northings = northings;
            AdminDistrictCode = adminDistrictCode;
            AdminDistrictName = adminDistrictName;
        }
    }
}
