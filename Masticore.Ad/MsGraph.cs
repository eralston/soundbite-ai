using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Masticore.Graph
{
    /// <summary>
    /// MS Graph implementation of <see cref="IGraph"/>; only useful for chaining off user-interactive calls
    /// Consumers of this class should implement caching
    /// </summary>
    /// <remarks>This uses the <see cref="IGraphServiceClient"/> class for its underlying implementation, this simplifies some aspects, but also keeps interaction at a higher-level compared to <see cref="Ad.IGraphClient"/></remarks>
    public class MsGraph : IGraph
    {
        private ILogger<MsGraph> Logger { get; }
        public Dictionary<string, IGraphServiceClient> Clients { get; set; } = new Dictionary<string, IGraphServiceClient>();
        public ITokenAcquisition TokenAcquisition { get; }

        public MsGraph(ITokenAcquisition tokenAcquisition, ILogger<MsGraph> logger)
        {
            Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            TokenAcquisition = tokenAcquisition ?? throw new System.ArgumentNullException(nameof(tokenAcquisition));
        }

        /// <summary>
        /// Gets an authenticated IGraphServiceClient for the current user and the given scopes
        /// This does NOT cache the client, so be sure to hold onto it for the duration
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        protected async Task<IGraphServiceClient> ClientAsync(string[] scopes = null)
        {
            try
            {
                if (scopes == null)
                {
                    scopes = new string[] { "user.read", "profile", "email" };
                }

                string key = string.Join(',', scopes);
                if (!Clients.ContainsKey(key))
                {
                    string accessToken = await TokenAcquisition.GetAccessTokenForUserAsync(scopes);
                    IGraphServiceClient client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        return Task.FromResult(0);
                    }));
                    Clients[key] = client;
                }
                return Clients[key];
            }
            catch (MsalUiRequiredException ex)
            {
                Logger.LogError(ex, $"Exception acquiring token for graph service with scopes {string.Join(',', scopes)}");

                //TODO: the call below was commented out to keep track of what it was doing so it
                //  can be addressed.  It was setting a 403 response even though the call was 
                //  otherwise successful.  In most situations we do not have a graph/azure token
                //  so we need to move the the image acquisition to the sections of code were we
                //  have a valid graph/azure token. This should be moved to somewhere in the
                //  AzureAuth controller and/or have something in the UI setup to explicitly call
                //  out ot the graph apis to update the user profile information.

                //await TokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(scopes, ex);
                return null;
            }
        }

        /// <summary>
        /// Async returns the IUser of the current user
        /// </summary>
        /// <returns></returns>
        public async Task<Models.User> MyUserAsync()
        {
            Models.User user = null;
            IGraphServiceClient client = await ClientAsync();
            if (client != null)
            {
                User graphUser = await client.Me.Request().GetAsync();
                string phone = graphUser.MobilePhone ?? graphUser.BusinessPhones.FirstOrDefault();
                user = new Models.User
                {
                    UniversalId = graphUser.Id,
                    GivenName = graphUser.GivenName,
                    FamilyName = graphUser.Surname,
                    Email = graphUser.Mail,
                    Phone = phone,
                    Title = graphUser.JobTitle,
                };
            }

            return user;
        }

        /// <summary>
        /// Async returns a JPG photo of the user as a stream
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> MyPhotoAsync()
        {
            Stream photo = null;
            IGraphServiceClient client = await ClientAsync();
            if (client != null)
            {
                photo = await client.Me.Photo.Content.Request().GetAsync();
            }
            return photo;
        }
    }
}
