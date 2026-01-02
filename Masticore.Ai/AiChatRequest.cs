namespace Masticore.Ai
{
    // Initially based on meddling with Azure OpenAI Studio in Jan 2023

    /// <summary>
    /// Basic request object, agnostic to AI provider
    /// </summary>
    public class AiChatRequest
    {
        public AiChatRequest(string prompt)
        {
            Validator.ArgNotNullOrEmpty(nameof(prompt), prompt);
            Prompt = prompt;
        }

        public string Prompt { get; set; }

        /// <summary>
        /// Level of "non-determinism" from zero (least) to most (one)
        /// </summary>
        public float? Divergence { get; set; }

        /// <summary>
        /// Estimate max length of characters in the response; not a perfect measure so expect variance
        /// </summary>
        public int? EstimatedMaxCharacters { get; set; }
    }

}