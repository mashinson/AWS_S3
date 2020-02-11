using aws.Models;
using System.Security.Policy;
using System.Threading.Tasks;

namespace aws.Services
{
    public interface IS3Service
    {
        Task<S3Response> CreateBucketAsync(string bucketName);
        Task UploadFileAsync(string bucketName);
        Task GetObjectFromS3Async(string bucketName);
        Task<string> GenerateTempLinkForFile(string bucketName);
    }
}
