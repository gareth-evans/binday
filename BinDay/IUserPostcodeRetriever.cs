using System.Threading.Tasks;

namespace BinDay
{
    public interface IUserPostcodeRetriever<in T>
    {
        Task<string> GetPostcodeAsync(T request);
    }
}