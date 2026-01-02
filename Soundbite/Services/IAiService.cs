using Masticore.Services;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    // TODO: Put more thought into the structure for this service
    // This is just a rough draft concept

    /// <summary>
    /// Response from AI service about a session
    /// </summary>
    public class SessionSummary
    {
        /// <summary>
        /// Route for the relevant organization
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// Route for the relevant session
        /// </summary>
        public string SessionRoute { get; set; }

        /// <summary>
        /// Suggested content for the session
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Simplified transcript of the session
        /// </summary>
        public string Transcript { get; set; }
    }

    /// <summary>
    /// The various types of recognized
    /// </summary>
    public enum SessionSummaryType
    {
        Paragraph = 0,
        Social = 10,
        Tweet = 20,
        Blog = 30,
        Newsletter = 40,
    }


    /// <summary>
    /// Interface for AI services
    /// </summary>
    public interface IAiService : IService
    {
        /// <summary>
        /// Generate some AI-powered suggested call-to-actions for the given session
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="summaryType"></param>
        /// <returns></returns>
        Task<SessionSummary> SessionSummaryAsync(string orgRoute, string sessionRoute, SessionSummaryType summaryType = SessionSummaryType.Paragraph);
    }
}
