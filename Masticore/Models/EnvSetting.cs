namespace Masticore
{
    /// <summary>
    /// Model for storing information about an environment setting.  This model exists
    /// to help store environment settings in a centralied data store where they can be
    /// easily managed across multiple azure resources.
    /// </summary>
    public class EnvSetting
    {
        #region Fields

        private string _groupKey = null;

        #endregion

        /// <summary>
        /// Gets or sets the unique ID of the setting.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name of the setting.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the setting to help understand what it is for.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value to help logically group settings for retrieval.
        /// </summary>
        public string GroupKey
        {
            get => string.IsNullOrWhiteSpace(_groupKey) ? null : _groupKey;
            set => _groupKey = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// Gets or sets a value indicating that a setting is secure and the value should
        /// only be transmitted from client to server but never server to client. 
        /// </summary>
        public bool IsSecure { get; set; }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        public string Value { get; set; }
    }
}