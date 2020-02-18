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
            _client = new AmazonS3Client("AKIAWNFNEWXKDNRHTFWB", "l84wz+brmRSOz2xQRIGPQKxA0GtPvWLKxphZ7oHK", Amazon.RegionEndpoint.USEast1);
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
                await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyNameAndFolder);
                ////Option1
                //await fileTransferUtility.UploadAsync(FilePath, bucketName);

                ////option2 
                //await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyName);

                ////option3

                //using (var fileToupload = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                //{
                //    await fileTransferUtility.UploadAsync(fileToupload, bucketName, FileStreamUpload);
                //}
                ////option4
                //var fileTrasferUtilityRequest = new TransferUtilityUploadRequest
                //{
                //    BucketName = bucketName,
                //    FilePath = FilePath,
                //    StorageClass = S3StorageClass.Standard,
                //    PartSize = 6291456, //6mb
                //    Key = AdvencedUpload,
                //    CannedACL = S3CannedACL.NoACL
                //};
                //fileTrasferUtilityRequest.Metadata.Add("param1", "Value1");
                //fileTrasferUtilityRequest.Metadata.Add("param2", "Value2");

                //await fileTransferUtility.UploadAsync(fileTrasferUtilityRequest);


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

        public async Task DownloadObjectFromS3Async(string bucketName)
        {
            const string keyName = "emails.txt";
            var path= $"C:\\S3temp\\";
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
                    path += keyName;
                }
             
                File.WriteAllText(path, responseBody);
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

        public string GenerateTempLinkForFile(string bucketName)
        {
            const string keyName = "emails.txt";

            string url ="";
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    Expires = DateTime.Now.AddMinutes(5)
                };
                url =  _client.GetPreSignedURL(request);
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

        public async Task<bool> CheckFileExists(string bucketName)
        {
            string objectName = "folder/";
            try
            {
                var request = new GetObjectMetadataRequest()
                {
                    BucketName = bucketName,
                    Key = objectName
                };

                var response = await _client.GetObjectMetadataAsync(request);

                return true;
            }

            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return false;

                //status wasn't not found, so throw the exception
                throw;
            }
        }

        public async Task<bool> CreateFolderAsync(string bucketName)
        {
            string folderName = "SSS/folder";

            var folderKey = folderName + "/"; //end the folder name with "/"

            try
            {
                var request = new PutObjectRequest()
                {
                    Key = folderKey,
                    BucketName = bucketName,
                    ContentBody = string.Empty,
                    StorageClass = S3StorageClass.Standard,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.None
                };

                var response = await _client.PutObjectAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                return false;
                throw ex;
            }
        }
        public async Task DeleteFolderAsync(string bucketName)
        {
            var folderName = "SSS/";
            try
            {
                var listObjectRequest = new ListObjectsRequest()
                {
                    BucketName = bucketName,
                    Prefix = folderName
                };
                var response = await _client.ListObjectsAsync(listObjectRequest);

               
                var deleteRequest = new DeleteObjectsRequest()
                { 
                    BucketName = bucketName  
                };

                foreach (var item in response.S3Objects)
                {
                    var key = new KeyVersion()
                    {
                        Key = item.Key
                    };
                    deleteRequest.Objects.Add(key);
                }

                // Add the folder itself to be deleted as well
                var prefixKey = new KeyVersion()
                {
                    Key = folderName
                };
                deleteRequest.Objects.Add(prefixKey);

                var deleteResponse = await _client.DeleteObjectsAsync(deleteRequest);
            }
            catch (AmazonS3Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteFileAsync(string bucketName)
        {
            var objectName = "folder/Book.txt";

            try
            {
                var deleteRequest = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = objectName
                };
                var deleteResponse = await _client.DeleteObjectAsync(deleteRequest);
            }
            catch (AmazonS3Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<S3Object>> GetFolderAsync(string bucketName)
        {
            var objectName = "folder/";
            var listObjectRequest = new ListObjectsRequest()
            {
                BucketName = bucketName,
                Prefix = objectName
            };
            var response = await _client.ListObjectsAsync(listObjectRequest);
            return response.S3Objects;
        }

        private const string FilePath = "E:\\emails.txt";
        private const string UploadWithKeyName = "UploadWithKeyName";
        private const string FileStreamUpload = "FileStreamUpload";

        private const string UploadWithKeyNameAndFolder = "SSS/UploadWithKeyName";
        private const string AdvencedUpload = "AdvanceUpload";

    }
}
