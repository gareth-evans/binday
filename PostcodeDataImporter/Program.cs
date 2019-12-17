using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.Azure.Cosmos.Table;
using Shared;

namespace PostcodeDataImporter
{
    class Program
    {
        private const string BasePath = @"C:\Users\graz1\Downloads\codepo_gb";

        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var districts = GetDistricts().ToDictionary(x => x.Code, x => x.Name);

            var tableClient = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=binday;AccountKey=nR7ts25+CvJQuq5PSUb5H3bMQz8Awi82b2LenQnrWnFpwLYt8DrPeOjKqxgYTQCK+AYtco5nMN0t8lbMjIzjOg==;EndpointSuffix=core.windows.net").CreateCloudTableClient();

            var tableRef = tableClient.GetTableReference("Postcodes");
            var postcodeEntities = GetPostcodeEntities(districts).ToArray();
            
            foreach (var batchSet in postcodeEntities.GroupBy(x => x.PartitionKey).SelectMany(x => x.InSetsOf(100)).InSetsOf(30))
            {
                IList<Task<TableBatchResult>> batchTasks = new List<Task<TableBatchResult>>();

                foreach (var batch in batchSet)
                {
                    var batchOperation = new TableBatchOperation();

                    foreach (var postcodeEntity in batch)
                    {
                        batchOperation.Add(TableOperation.InsertOrReplace(postcodeEntity));
                    }

                    batchTasks.Add(
                        tableRef.ExecuteBatchAsync(batchOperation)
                            .ContinueWith(t =>
                            {
                                Console.WriteLine(
                                        $"Inserted {batch[0].Postcode}-{batch[batch.Count - 1].Postcode}");
                                return t.Result;
                            }));
            
                }

                await Task.WhenAll(batchTasks);
            }

            Console.ReadLine();
        }

        private static IEnumerable<PostcodeEntity> GetPostcodeEntities(IDictionary<string, string> districts)
        {
            var files = Directory.GetFiles(Path.Combine(BasePath, @"Data\CSV\"));
            foreach (var file in files)
            {
                Console.WriteLine($"Reading file {Path.GetFileName(file)}...");

                using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateCsvReader(stream);

                while (reader.Read())
                {
                    var postcode = reader.GetString(0).Replace(" ", "");
                    var adminDistrictCode = reader.GetString(8);
                    var eastings = int.Parse(reader.GetString(2));
                    var northings = int.Parse(reader.GetString(3));

                    if (string.IsNullOrWhiteSpace(postcode)) continue;

                    if (string.IsNullOrWhiteSpace(adminDistrictCode))
                    {
                        Console.Error.WriteLine($"There was no admin district code for {postcode}");
                        continue;
                    }

                    if (!districts.ContainsKey(adminDistrictCode))
                    {
                        throw new Exception($"Admin district code '{adminDistrictCode}' was not found");
                    }

                    yield return new PostcodeEntity(
                        postcode,
                        eastings,
                        northings,
                        adminDistrictCode,
                        districts[adminDistrictCode]
                    );
                }

                reader.Close();
                stream.Close();
            }
        }

        private static IEnumerable<District> GetDistricts()
        {
            using var stream = File.Open(Path.Combine(BasePath, @"Doc\Codelist.xlsx"), FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            do
            {
                //if (!includedTabs.Contains(reader.Name)) continue;

                while (reader.Read())
                {
                    var code = reader.GetString(1);
                    var name = reader.GetString(0);

                    if (string.IsNullOrWhiteSpace(code)) continue;
                    if(name?.Contains("(DET)") == true) continue;

                    yield return new District
                    {
                        Code = code,
                        Name = name
                    };
                }
            } while (reader.NextResult());
        }


        public class District
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }


        public class HealthAuthority
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        private static IEnumerable<HealthAuthority> GetHealthAuthorities()
        {
            using var stream = File.Open(@"C:\Users\graz1\Downloads\codepo_gb\Doc\NHS_Codelist.xls", FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                while (reader.Read())
                {
                    var code = reader.GetString(0);
                    var name = reader.GetString(1);
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    yield return new HealthAuthority
                    {
                        Code = code,
                        Name = name
                    };
                }
            } while (reader.NextResult());
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
        {
            List<T> toReturn = new List<T>(max);
            foreach (var item in source)
            {
                toReturn.Add(item);
                if (toReturn.Count == max)
                {
                    yield return toReturn;
                    toReturn = new List<T>(max);
                }
            }
            if (toReturn.Any())
            {
                yield return toReturn;
            }
        }
    }
}
