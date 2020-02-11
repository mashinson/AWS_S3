using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using aws.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;

namespace aws.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _client;
        public S3Service(IAmazonS3 client)
        {
            _client = new AmazonS3Client("", "", Amazon.RegionEndpoint.USEast1);
        }

        public async Task<S3Response> CreateBucketAsync(string bucketName)
        {
            try
            {
                if (await AmazonS3Util.DoesS3BucketExistV2Async(_client, bucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };
                
                var response = await _client.PutBucketAsync(putBucketRequest);
                    
                    return new S3Response
                    {
                        Message = response.ResponseMetadata.RequestId,
                        Status = response.HttpStatusCode
                    };
                }
            }
            catch (AmazonS3Exception e)
            {
                return new S3Response
                {
                    Status = e.StatusCode,
                    Message = e.Message
                };
            }
            catch (Exception e)
            {
                return new S3Response
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = e.Message
                };
            }

            return new S3Response
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "Something goes wrong"
            };
        }

        public async Task UploadFileAsync(string bucketName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_client);

                //Option1
                await fileTransferUtility.UploadAsync(FilePath, bucketName);

                //option2 
                await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyName);

                //option3

                using (var fileToupload = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    await fileTransferUtility.UploadAsync(fileToupload, bucketName, FileStreamUpload);
                }
                //option4
                var fileTrasferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = FilePath,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = 6291456, //6mb
                    Key = AdvencedUpload,
                    CannedACL = S3CannedACL.NoACL
                };
                fileTrasferUtilityRequest.Metadata.Add("param1", "Value1");
                fileTrasferUtilityRequest.Metadata.Add("param2", "Value2");

                await fileTransferUtility.UploadAsync(fileTrasferUtilityRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message: '{0}' when writing an object", e.Message);
            }

            catch (Exception e)
            {
                Console.WriteLine("Unknown error encountered on server. Message: '{0}' when writing an object", e.Message);
            }

        }

        public async Task GetObjectFromS3Async(string bucketName)
        {
            const string keyName = "emails.txt";
            try
            {
                var request = new GetObjectRequest
                {
                       BucketName = bucketName,
                       Key = keyName
                };

                string responseBody;

                using (var response = await _client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var reader = new StreamReader(responseStream))
                {
                    var title = response.Metadata["x-amz-meta-title"];
                    var contentType = response.Headers["Content-Type"];

                    Console.WriteLine($"object meta, Title: {title}");
                    Console.WriteLine($"Content type, type: {contentType}");

                    responseBody = reader.ReadToEnd();

                }

                var pathAndFileName = $"C:\\S3temp\\{keyName}";
                var createText = responseBody;

                File.WriteAllText(pathAndFileName, createText);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message: '{0}' when getting an object", e.Message);
            }

            catch (Exception e)
            {
                Console.WriteLine("Unknown error encountered on server. Message: '{0}' when getting an object", e.Message);
            }
        }

        public async Task<string> GenerateTempLinkForFile(string bucketName)
        {
            string url ="";
            const string keyName = "emails.txt";
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    Expires = DateTime.Now.AddMinutes(5)
                };
                url = _client.GetPreSignedURL(request);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message: '{0}' when generating temp link", e.Message);
            }

            catch (Exception e)
            {
                Console.WriteLine("Unknown error encountered on server. Message: '{0}' when generating temp link", e.Message);
            }
            return url;
        }

        private const string FilePath = "E:\\emails.txt";
        private const string UploadWithKeyName = "UploadWithKeyName";
        private const string FileStreamUpload = "FileStreamUpload";
        private const string AdvencedUpload = "AdvanceUpload";

    }
}
