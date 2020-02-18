using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Amazon.S3.Model;
using aws.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aws.Controllers
{
    [Produces("application/json")]
    [Route("api/S3Bucket")]
    public class S3BucketController : ControllerBase
    {
        private readonly IS3Service _service;
        public S3BucketController(IS3Service service)
        {
            _service = service;
        }
        [HttpPost("{bucketName}")]
        public async Task<IActionResult> CreateBucket([FromRoute] string bucketName)
        {
            var response = await _service.CreateBucketAsync(bucketName);
            return Ok(response);
        }

        [HttpPost]
        [Route("AddFile/{bucketName}")]
        public async Task<IActionResult> AddFile([FromRoute] string bucketName)
        {
            await _service.UploadFileAsync(bucketName);
            return Ok();
        }

        [HttpGet]
        [Route("GetFile/{bucketName}")]
        public async Task<IActionResult> GetObjectFromS3Async([FromRoute] string bucketName)
        {
            await _service.DownloadObjectFromS3Async(bucketName);
            return Ok();

        }

        [HttpPost]
        [Route("GenerateLink/{bucketName}")]
        public string GenerateTempLinkForFile([FromRoute] string bucketName)
        {
           var url =  _service.GenerateTempLinkForFile(bucketName);
            return url;
        }

        [HttpPost]
        [Route("CheckFileExists/{bucketName}")]
        public async Task<bool> CheckFileExistsAsync([FromRoute] string bucketName)
        {
            var isExist = await _service.CheckFileExists(bucketName);
            return isExist;

        }

        [HttpPost]
        [Route("CreateFolder/{bucketName}")]
        public async Task<bool> CreateFolderAsync([FromRoute] string bucketName)
        {
            var isExist = await _service.CreateFolderAsync(bucketName);
            return isExist;

        }

        [HttpPost]
        [Route("DeleteFolder/{bucketName}")]
        public async Task<IActionResult> DeleteFolderAsync([FromRoute] string bucketName)
        {
             await _service.DeleteFolderAsync(bucketName);
            return Ok();

        }


        [HttpPost]
        [Route("DeleteFile/{bucketName}")]
        public async Task<IActionResult> DeleteFileAsync([FromRoute] string bucketName)
        {
            await _service.DeleteFileAsync(bucketName);
            return Ok();

        }

        [HttpPost]
        [Route("GetFiles/{bucketName}")]
        public async Task<List<S3Object>> GetFolderAsync([FromRoute] string bucketName)
        {
            var req = await _service.GetFolderAsync(bucketName);
            return req;

        }
    }
}