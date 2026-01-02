namespace Soundbite.TokenGen
{
    /// <summary>
    /// Enumeration identifying the "level" at which token settings are defined.
    /// </summary>
    public enum TokenSecurityLevel
    {
        Default = 0,
        Tenant = 10,
        Organization = 20
    }

    /// <summary>
    /// Enumeration of the various mechanism for manging token security in the application.
    /// </summary>
    public enum TokenSecurityType
    {
        /// <summary>
        /// Denotes that the signing type is not set, is unknown, or should be inherited.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes that the signing mechanism uses a secret key value.
        /// </summary>
        SecretKey = 10,

        /// <summary>
        /// Denotes that the signing mecahnism uses a certificate.
        /// </summary>
        Certificate = 20
    }    
}
