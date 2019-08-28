using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileProvider.Azure
{
    public interface IStorageClient
    {
        Task<IEnumerable<AzureDocument>> GetDirectory(string container, string directory);
        Task<AzureFile> GetFile(string container, string path, MemoryStream stream);
        Task<IEnumerable<AzureDocument>> Upload(string container, string directory, ICollection<IFileInfo> files);
        Task<Dictionary<string, bool>> Delete(string container, string directory, ICollection<string> files);
    }

    public class AzureDocument
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class AzureFile
    {
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
    }

    public class StorageClient : IStorageClient
    {
        private readonly CloudBlobClient _cloudBlobClient;

        public StorageClient(IOptionsMonitor<AzureBlobOptions> azureblobOptions)
        {
            var azureblob = azureblobOptions.CurrentValue;

            if (azureblob.ConnectionString != null && CloudStorageAccount.TryParse(azureblob.ConnectionString, out var cloudStorageAccount))
            {
                _cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            }
            else if (!string.IsNullOrWhiteSpace(azureblob.BaseUri) && !string.IsNullOrWhiteSpace(azureblob.Token))
            {
                _cloudBlobClient = new CloudBlobClient(new Uri(azureblob.BaseUri), new StorageCredentials(azureblob.Token));
            }
            else
            {
                throw new ArgumentException("One of the following must be set: 'ConnectionString' or 'BaseUri'+'Token'!");
            }
        }

        public async Task<IEnumerable<AzureDocument>> GetDirectory(string container, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            if (Path.HasExtension(directory)) throw new ArgumentException($"{nameof(directory)} can't contain an extension and needs to be a directory path");

            CloudBlobContainer cloudBlobContainer = await GetBlobContainer(container);
            var directoryReference = cloudBlobContainer.GetDirectoryReference(directory);
            List<IListBlobItem> blobs = new List<IListBlobItem>();
            BlobContinuationToken continuationToken = null;

            do
            {
                var token = continuationToken;
                var response = await directoryReference.ListBlobsSegmentedAsync(token);
                continuationToken = response.ContinuationToken;
                blobs.AddRange(response.Results);
            }
            while (continuationToken != null);

            var files = blobs.OfType<CloudBlob>().Select(x => new AzureDocument
            {
                Name = !string.IsNullOrEmpty(x.Parent.Prefix) ? x.Name.Replace(x.Parent.Prefix, "") : x.Name,
                Url = x.Name
            }).ToList();

            var directories = blobs.OfType<CloudBlobDirectory>().Select(x => new AzureDocument
            {
                Name = x.Prefix.TrimEnd('/'),
                Url = x.Uri.ToString()
            }).ToList();


            files.AddRange(directories);

            return files;
        }

        public async Task<AzureFile> GetFile(string container, string path, MemoryStream stream)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!Path.HasExtension(path)) throw new ArgumentException($"{nameof(path)} cannot contain no extension and needs to be a file path");

            CloudBlobContainer cloudBlobContainer = await GetBlobContainer(container);
            var file = cloudBlobContainer.GetBlobReference(path);
            await file.DownloadToStreamAsync(stream);
            return new AzureFile
            {
                Data = stream.ToArray(),
                MimeType = GetMimeType(file.Name)
            };
        }

        public async Task<IEnumerable<AzureDocument>> Upload(string container, string directory, ICollection<IFileInfo> files)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            if (Path.HasExtension(directory)) throw new ArgumentException($"{nameof(directory)} can't contain an extension and needs to be a directory path");

            CloudBlobContainer cloudBlobContainer = await GetBlobContainer(container);

            var result = new List<AzureDocument>();
            foreach (var file in files)
            {
                var fileStorageLocation = Path.Combine(directory, file.Name);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileStorageLocation);
                cloudBlockBlob.Properties.ContentType = GetMimeType(file.Name);
                using (var fileStream = file.CreateReadStream())
                {
                    await cloudBlockBlob.UploadFromStreamAsync(fileStream);
                    result.Add(new AzureDocument
                    {
                        Name = !string.IsNullOrEmpty(cloudBlockBlob.Parent.Prefix) ? cloudBlockBlob.Name.Replace(cloudBlockBlob.Parent.Prefix, "") : cloudBlockBlob.Name,
                        Url = cloudBlockBlob.Name
                    });
                }
            }
            return result;
        }

        public async Task<Dictionary<string, bool>> Delete(string container, string directory, ICollection<string> files)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            if (Path.HasExtension(directory)) throw new ArgumentException($"{nameof(directory)} can't contain an extension and needs to be a directory path");

            CloudBlobContainer cloudBlobContainer = await GetBlobContainer(container);

            var result = new Dictionary<string, bool>();
            foreach (var file in files)
            {
                var fileStorageLocation = Path.Combine(directory, file);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileStorageLocation);
                var deleted = await cloudBlockBlob.DeleteIfExistsAsync();
                result.Add(file, deleted);
            }
            return result;
        }

        private async Task<CloudBlobContainer> GetBlobContainer(string container)
        {
            if (string.IsNullOrWhiteSpace(container)) throw new ArgumentNullException(nameof(container));
            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(container);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            return cloudBlobContainer;
        }

        private static string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
