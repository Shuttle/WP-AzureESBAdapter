using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP.AzureESBAdapter
{
    public class AzureESBConfiguration : IAzureESBConfiguration
    {
        public AzureESBConfiguration()
        {
            OperationRetryCount = 3;
            SharedAccessKeyName = "RootManageSharedAccessKey";
            TransportType = string.Empty;

        }
        public int OperationRetryCount { get; set; }
        public string SharedAccessKeyName { get; set; }
        public string TransportType { get; set; }
    }
}