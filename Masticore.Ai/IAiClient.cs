namespace Masticore.Ai
{
    /// <summary>
    /// Basic structure of a stateless AI client that can request on behalf of the app
    /// </summary>
    public interface IAiClient
    {
        /// <summary>
        /// Reaches out to the AI for a basic chat-like call and response
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<AiChatResponse> RequestChat(AiChatRequest req);
    }
}