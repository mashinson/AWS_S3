using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
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
            await _service.GetObjectFromS3Async(bucketName);
            return Ok();

        }

        [HttpPost]
        [Route("GenerateLink/{bucketName}")]
        public async Task<string> GenerateTempLinkForFile([FromRoute] string bucketName)
        {
           var url = await _service.GenerateTempLinkForFile(bucketName);
            return url;

        }
    }
}