using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileProvider.Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;

namespace FileProvider.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IStorageClient _storageClient;

        public FilesController(IStorageClient storageClient)
        {
            _storageClient = storageClient;
        }

        [HttpGet("{*path:directory}")]
        public async Task<IActionResult> GetDirectory(string path)
        {
            var directory = await this._storageClient.GetDirectory("kevin", path);
            directory.ToList().ForEach(x => x.Url = $"{Request.Scheme}:/{Request.Host.Value}/files/{x.Url}");

            return Ok(directory);
        }

        [HttpGet("{*path:file}")]
        public async Task<IActionResult> GetFile(string path)
        {
            using (var stream = new MemoryStream())
            {
                var file = await this._storageClient.GetFile("kevin", path, stream);
                return File(file.Data, file.MimeType);

            }
        }

        // POST api/values
        [HttpPost("{*directory:directory}")]
        public async Task<IActionResult> Post(string directory, [FromBody] ICollection<IFileInfo> files)
        {
            var result  = await this._storageClient.Upload("kevin", directory, files);
            return Ok(result);
        }

        // PUT api/values/5
        [HttpPut("{*path:file}")]
        public void Put(string path, [FromBody] string value)
        {
        }

        [HttpDelete("{*path:directory}")]
        public void DeleteFolder(int id)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{*path:file}")]
        public void DeleteFile(int id)
        {
        }
    }
}
