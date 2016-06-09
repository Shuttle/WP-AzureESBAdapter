using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;

namespace WP.AzureESBAdapter
{
    public class AzureESBUriParser
    {
        internal const string SCHEME = "azureesb";
        public string Queue { get; private set; }
        public string ConnectionString { get; private set; }

        public AzureESBUriParser(Uri uri, IAzureESBConfiguration config)
        {
            Guard.AgainstNull(uri, "uri");
            Guard.AgainstNull(config, "config");

            if (!uri.Scheme.Equals(SCHEME, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidSchemeException(SCHEME, uri.ToString());
            }

            StringBuilder connectionString = new StringBuilder(@"Endpoint=sb://");
            connectionString.Append(uri.Host + ";");
            connectionString.Append("SharedAccessKeyName=" + config.SharedAccessKeyName + ";");
            connectionString.Append(uri.LocalPath.Substring(uri.LocalPath.IndexOf("SharedAccessKey")));
            if (config.TransportType != string.Empty)
                connectionString.Append(";TransportType=" + config.TransportType);

            ConnectionString = connectionString.ToString();

            Queue = uri.PathAndQuery.Substring(1, uri.PathAndQuery.IndexOf(';') - 1);
        }
    }
}
