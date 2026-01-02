using Masticore.Models;
using Masticore.Resources;
using System.Web;

namespace Masticore.Services
{
    /// <summary>
    /// Extensions to give objects their mapping to storage locations
    /// </summary>
    public static class ContainerNameExtensions
    {
        // Container names must start or end with a letter or number, and can contain only letters, numbers, and the dash (-) character.
        // Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.

        // A blob name can contain any combination of characters
        // A blob name must be at least one character long and cannot be more than 1,024 characters long, for blobs in Azure Storage
        // The Azure Storage emulator supports blob names up to 256 characters long
        // Blob names are case-sensitive
        // Reserved URL characters must be properly escaped

        public static string ToContainerName(this Organization org)
        {
            return $"org{org.UniversalId}";
        }

        public static string ToImageBlobName(this Organization org)
        {
            string encodedRoute = HttpUtility.UrlEncode(org.Route);
            return $"org{encodedRoute}/image.jpg"; // TODO: Support setting filetype from upload
        }

        public static string ToUserContainerName(this IUniversal user)
        {
            return $"user{user.UniversalId}";
        }

        public static string ToOriginalImageBlobName(this IUniversal user)
        {
            return $"image_original.jpg"; // TODO: Support setting filetype from upload
        }
    }
}
