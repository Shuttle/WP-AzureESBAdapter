using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP.AzureESBAdapter
{
    public interface IAzureESBConfiguration
    {
        int OperationRetryCount { get; set; }
        string TransportType { get; set; }
        string SharedAccessKeyName { get; set; }
    }
}
