using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileProvider.Azure
{
    public class AzureBlobOptions
    {
        public string BaseUri { get; set; }

        public string Token { get; set; }
        
        public string ConnectionString { get; set; }
    }
}
