using Amazon.S3.Model;
using aws.Models;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;

namespace aws.Services
{
    public interface IS3Service
    {
        Task<S3Response> CreateBucketAsync(string bucketName);
        Task UploadFileAsync(string bucketName);
        Task DownloadObjectFromS3Async(string bucketName);
        string GenerateTempLinkForFile(string bucketName);
        Task<bool> CheckFileExists(string bucketName);
        Task<bool> CreateFolderAsync(string bucketName);
        Task DeleteFolderAsync(string bucketName);
        Task DeleteFileAsync(string bucketName);
        Task<List<S3Object>> GetFolderAsync(string bucketName);
    }
}
