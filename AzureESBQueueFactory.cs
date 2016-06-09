using System;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;

namespace WP.AzureESBAdapter
{
    public class AzureESBQueueFactory : IQueueFactory
    {
        public AzureESBConfiguration Configuration { get; private set; }

        public AzureESBQueueFactory()
            : this(AzureESBSection.Configuration())
        {
        }

        public AzureESBQueueFactory(AzureESBConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string Scheme
        {
            get { return "azureesb"; }
        }

        public IQueue Create(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            return new AzureESBQueue(uri, Configuration);
        }

        public bool CanCreate(Uri uri)
        {
            Guard.AgainstNull(uri, "uri");

            return Scheme.Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase);
        }
    }

}
