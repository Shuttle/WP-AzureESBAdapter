using System.Configuration;
using Shuttle.Core.Infrastructure;


namespace WP.AzureESBAdapter
{
    public class AzureESBSection : ConfigurationSection
    {

        [ConfigurationProperty("operationRetryCount", IsRequired = false, DefaultValue = 3)]
        public int OperationRetryCount
        {
            get { return (int)this["operationRetryCount"]; }
        }

        [ConfigurationProperty("transportType", IsRequired = false, DefaultValue = "")]
        public string TransportType
        {
            get { return (string)this["transportType"]; }
        }

        [ConfigurationProperty("sharedAccessKeyName", IsRequired = false, DefaultValue = "RootManageSharedAccessKey")]
        public string SharedAccessKeyName
        {
            get { return (string)this["sharedAccessKeyName"]; }
        }

        public static AzureESBConfiguration Configuration()
        {
            var section = ConfigurationSectionProvider.Open<AzureESBSection>("shuttle", "azureesb");
            var configuration = new AzureESBConfiguration();

            if (section != null)
            {
                configuration.OperationRetryCount = section.OperationRetryCount;
                configuration.TransportType = section.TransportType;
                configuration.SharedAccessKeyName = section.SharedAccessKeyName;
            }

            return configuration;
        }
    }
}