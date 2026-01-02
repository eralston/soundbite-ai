namespace Masticore.Token
{
    /// <summary>
    /// Defines the contract for a token service factory which is responsible for acquiring the
    /// appropriate token service based on settings.
    /// </summary>
    public interface ITokenServiceFactory
    {
        /// <summary>
        /// Retrieves an appropriate token service based on the specified settings.
        /// </summary>
        /// <param name="settings">Settings containing information on which token service is needed.</param>
        /// <returns>a <see cref="ITokenService"/> ready for use.</returns>
        public ITokenService GetTokenService(ITokenServiceSettings settings);
    }
}