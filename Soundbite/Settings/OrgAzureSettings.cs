using Masticore;
using Newtonsoft.Json;

namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// Defines azure-specific settings available at an organization level.
    /// </summary>
    [CodeGenModel]
    public class OrgAzureSettings
    {
        #region Constants

        const string StringSentinel = "*********";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a flag indicating whether secure data can be exported in JSON
        /// </summary>        
        [CodeGenField(Ignore = true)]
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool AllowSecureDataJsonExport { get; set; } = false;

        /// <summary>
        /// Gets or sets the Azure TenantID associated with the organization.
        /// </summary>        
        [CodeGenField(IsNullable = true)]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether Teams notifications are enabled for the organization.
        /// </summary>
        public bool EnableTeamsNotifications { get; set; } = false;

        /// <summary>
        /// Gets or sets the azure media service associated with the organization.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string MediaServiceAccountName { get; set; }

        /// <summary>
        /// Gets or sets the media service resource group associated with the organization.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string MediaServiceResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the encoding transform to use for single bit rate encoding.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string SingleBitRateTransformName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the streaming endpoint associated with the organization.  This 
        /// value is used to build out the streaming URLs for video assets serviced by AMS. Changing
        /// this value does not update existing records.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string StreamingEndpointUrlPrefix { get; set; }

        /// <summary>
        /// Gets or sets the ACS from e-mail; this needs to match an email configured in ACS
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string AcsFromEmail { get; set; } = null;

        /// <summary>
        /// Gets or sets the ACS from name; this needs to match the name configured for the given email in ACS
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string AcsFromName { get; set; } = null;

        #endregion

        #region Secure Properties

        /***********************************************************************************************
         * NOTE: properties in this section are secured using a bit of a JSON "hack" that ensures secure
         *       data is not emitted when serializing outgoing JSON, but is set when reading incomming 
         *       JSON. The property itself has a JsonIgnore attribute which keeps data from being 
         *       serialized.  It then has "Setter" property that uses the JsonProperty attribute set to
         *       the name of the original property so it can "read" incomming JSON data and set the 
         *       original property accordingly.
         **********************************************************************************************/

        /// <summary>
        /// Gets or sets the Azure Communication Service (ACS) connection string
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [CodeGenField(IsNullable = true)]
        public string AcsConnectionString { get; set; } = null;

        [JsonProperty("acsConnectionString")]
        [System.Text.Json.Serialization.JsonPropertyName("acsConnectionString")]
        [CodeGenField(Ignore = true)]
        public string AcsConnectionStringSetter
        {
            get => AllowSecureDataJsonExport ? AcsConnectionString : SecureValue(AcsConnectionString);
            set => AcsConnectionString = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Imports properties from <paramref name="azureSettings"/> ensuring that protected 
        /// properties are set only when a value is imported.
        /// </summary>
        /// <param name="azureSettings">Settings to import over the current settings.</param>
        public void ImportFrom(OrgAzureSettings azureSettings)
        {
            // Copy properties
            TenantId = azureSettings.TenantId?.NullIfEmpty();
            EnableTeamsNotifications = azureSettings.EnableTeamsNotifications;
            MediaServiceAccountName = azureSettings.MediaServiceAccountName?.NullIfEmpty();
            MediaServiceResourceGroupName = azureSettings.MediaServiceResourceGroupName?.NullIfEmpty();
            SingleBitRateTransformName = azureSettings.SingleBitRateTransformName?.NullIfEmpty();
            StreamingEndpointUrlPrefix = azureSettings.StreamingEndpointUrlPrefix?.NullIfEmpty();
            AcsFromEmail = azureSettings.AcsFromEmail?.NullIfEmpty();
            AcsFromName = azureSettings.AcsFromName?.NullIfEmpty();

            // Copy protected properties
            AcsConnectionString = GetSecureValue(azureSettings.AcsConnectionString, AcsConnectionString);

            // Ensure Validate Settings
            if (!string.IsNullOrEmpty(StreamingEndpointUrlPrefix) && !StreamingEndpointUrlPrefix.EndsWith("/"))
            {
                StreamingEndpointUrlPrefix += "/";
            }
        }

        /// <summary>
        /// If the value is empty, return empty; otherwise, make it with a sentinel value to make it anonymous
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string SecureValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return StringSentinel;
        }

        /// <summary>
        /// Based on <paramref name="newValue"/> being potentially a secure string that means fallback to old value, return the value of this field
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        private static string GetSecureValue(string newValue, string oldValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return null;
            }

            // Beware existing string sentinel values lingering in the DB
            if (oldValue == StringSentinel)
            {
                if (newValue == StringSentinel)
                {
                    return null;
                }
                else
                {
                    return newValue;
                }
            }

            if (newValue == StringSentinel)
            {
                return oldValue;
            }

            return newValue;
        }

        #endregion
    }
}