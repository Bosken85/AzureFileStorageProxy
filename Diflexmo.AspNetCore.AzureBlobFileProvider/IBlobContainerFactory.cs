using Microsoft.WindowsAzure.Storage.Blob;

namespace Diflexmo.AspNetCore.AzureBlobFileProvider
{
    public interface IBlobContainerFactory
    {
        CloudBlobContainer GetContainer(string subpath);
        string TransformPath(string subpath);
    }
}
