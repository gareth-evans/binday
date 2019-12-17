using System;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Shared;
using Microsoft.Extensions.Logging;

namespace BinDay
{
    public interface ITableStoragePostcodeResolver
    {
        PostcodeEntity GetPostcodeInfo(Postcode postcode);
    }

    public class TableStoragePostcodeResolver : ITableStoragePostcodeResolver
    {
        private readonly ILogger<TableStoragePostcodeResolver> _log;

        public TableStoragePostcodeResolver(ILogger<TableStoragePostcodeResolver> log)
        {
           _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public PostcodeEntity GetPostcodeInfo(Postcode postcode)
        {
            var connectionString = Environment.GetEnvironmentVariable("TableStorageConnectionString");
            var tableClient = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference("Postcodes");

            var query = new TableQuery<PostcodeEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, postcode.OutwardCode),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, postcode.Value.Replace(" ", ""))));

            var result = tableRef.ExecuteQuery(query).FirstOrDefault();

            return result;
        }
    }
}