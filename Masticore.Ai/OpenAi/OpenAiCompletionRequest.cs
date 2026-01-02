using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Masticore.Ai
{
    /// <summary>
    /// Class representing the AI endpoint structure
    /// </summary>
    public class OpenAiCompletionRequest
    {
        /// <summary>
        /// Default temperature for the AI model; the range of determinism of the model from zero (most predictable) to one (most random)
        /// </summary>
        public const float DefaultTemperature = 0.25F;

        /// <summary>
        /// Default max tokens for the AI model; a token is a "syllable"
        /// </summary>
        public const int DefaultMaxTokens = 2048;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prompt"></param>
        public OpenAiCompletionRequest(string prompt)
        {
            Prompt = prompt;
        }

        /// <summary>
        /// Complete text to direct the AI model to action
        /// </summary>
        [JsonProperty("prompt")]
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        /// <summary>
        /// Set a limit on the number of tokens to generate in a response. The system supports a maximum of 2048 tokens shared between a given prompt and response completion. (One token is roughly 4 characters for typical English text.)
        /// </summary>
        [JsonProperty("max_tokens")]
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = DefaultMaxTokens;

        /// <summary>
        /// Between 0 and 1, Controls randomness: Lowering results in less random completions. As the temperature approaches zero, the model will become deterministic and repetitive.
        /// </summary>
        [JsonProperty("temperature")]
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = DefaultTemperature;

        /// <summary>
        /// Reduce the chance of repeating a token proportionally based on how often it has appeared in the text so far. This decreases the likelihood of repeating the exact same text in a response.
        /// </summary>
        [JsonProperty("frequency_penalty")]
        [JsonPropertyName("frequency_penalty")]
        public float FrequencyPenalty { get; set; } = 0.75F;

        /// <summary>
        /// Reduce the chance of repeating any token that has appeared in the text at all so far. This increases the likelihood of introducing new topics in a response.
        /// </summary>
        [JsonProperty("presence_penalty")]
        [JsonPropertyName("presence_penalty")]
        public float PresencePenalty { get; set; } = 0.1F;

        /// <summary>
        /// 0 to 1, Generate multiple responses, and display only the one with the best total probability across all its tokens. The unused candidates will still incur usage costs, so use this parameter carefully and make sure to set the parameters for max response length and ending triggers as well. Note that streaming will only work when this is set to 1.
        /// </summary>
        [JsonProperty("top_p")]
        [JsonPropertyName("top_p")]
        public float TopProbabilities { get; set; } = 1.0F;

        /// <summary>
        /// Make responses stop at a desired point, such as the end of a sentence or list. Specify up to four sequences where the model will stop generating further tokens in a response. The returned text will not contain the stop sequence.
        /// </summary>
        [JsonProperty("stop")]
        [JsonPropertyName("stop")]
        public string? Stop { get; set; } = null;

        /// <summary>
        /// Serialize this object to a json string
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return this.ToLowerCamelJson();
        }
    }

}