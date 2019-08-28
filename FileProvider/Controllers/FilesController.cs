using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileProvider.Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace FileProvider.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IStorageClient _storageClient;

        public FilesController(IStorageClient storageClient, IUrlHelper urlHelper)
        {
            _storageClient = storageClient;
        }

        [HttpGet("{*path:directory}")]
        public async Task<IActionResult> GetDirectory(string path)
        {
            var directory = await this._storageClient.GetDirectory("container", path);
            directory.ToList().ForEach(x=> x.Url = Url.Action("GetFile", "Files", new { path = x.Url}));

            return Ok(directory);
        }

        [HttpGet("{*path:file}")]
        public IActionResult GetFile(string path)
        {
            return Ok();
        }

        // POST api/values
        [HttpPost("{*path:file}")]
        public void Post(string path, [FromBody] string value)
        {
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
