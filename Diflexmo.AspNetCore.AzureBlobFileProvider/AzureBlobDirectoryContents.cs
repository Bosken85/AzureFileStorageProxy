using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Diflexmo.AspNetCore.AzureBlobFileProvider
{
    public class AzureBlobDirectoryContents : IDirectoryContents
    {
        private readonly List<IListBlobItem> _blobs = new List<IListBlobItem>();

        public bool Exists { get; set; }

        public AzureBlobDirectoryContents(CloudBlobDirectory directory)
        {
            BlobContinuationToken continuationToken = null;

            do
            {
                var token = continuationToken;
                var response = Task.Run(() => directory.ListBlobsSegmentedAsync(token)).GetAwaiter().GetResult();
                continuationToken = response.ContinuationToken;
                _blobs.AddRange(response.Results);
            }
            while (continuationToken != null);
            Exists = _blobs.Count > 0;
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _blobs.Select(blob => new AzureBlobFileInfo(blob)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

