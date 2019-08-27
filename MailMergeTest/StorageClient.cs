using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MailMergeTest
{
    public class StorageClient
    {
        public async Task<string> UploadFileToBlobAsync(string containerName, string fileName, byte[] fileData, string fileMimeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));
                if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
                if (fileData == null || fileData.Length <= 0) throw new ArgumentNullException(nameof(fileData));
                if (string.IsNullOrWhiteSpace(fileMimeType)) throw new ArgumentNullException(nameof(fileMimeType));

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(@"UseDevelopmentStorage=true");
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                    await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                cloudBlockBlob.Properties.ContentType = fileMimeType;
                await cloudBlockBlob.UploadFromByteArrayAsync(fileData, 0, fileData.Length);
                return cloudBlockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}