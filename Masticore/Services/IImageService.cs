using Masticore.Models;
using Masticore.Resources;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Services for handling images, logos, etc
    /// NOTE: This does NOT implement RBAC
    /// </summary>
    public interface IImageService : IService
    {
        /// <summary>
        /// Uploads an image for use as the users's avatar image.
        /// </summary>
        /// <param name="user">User whose avatar image is being uploaded.</param>
        /// <param name="stream">Stream containing the user's avatar image.</param>
        /// <returns>a URL pointing to the image that was uploaded.</returns>
        Task<string> UploadUserImageAsync(IUniversal user, Stream stream);

        /// <summary>
        /// Retrieves a URL with an access token for the user's avatar image.
        /// </summary>
        /// <param name="user">User whose avatar image URL is being sought.</param>
        /// <returns>a URL pointing to the avatar image for the specified user or <c>null</c> if the image is not found.</returns>
        Task<string> UserImageAsync(IUniversal user);

        /// <summary>
        /// Uploads an image for use as the organization's avatar image.
        /// </summary>
        /// <param name="org">Organization whose avatar image is being uploaded.</param>
        /// <param name="stream">Stream containing organization's avatar image.</param>
        /// <returns>a URL pointing to the image that was uploaded.</returns>
        Task<string> UploadOrgImageAsync(Organization org, Stream stream);

        /// <summary>
        /// Retrieves a URL with an access token for the organization's avatar image.
        /// </summary>
        /// <param name="org">Organization whose avatar image URL is being sought.</param>
        /// <returns>a URL pointing to the avatar image for the specified organization or <c>null</c> if the image is not found.</returns>
        Task<string> OrgImageAsync(Organization org);
    }
}