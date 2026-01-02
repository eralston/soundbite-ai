using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Masticore.Ai
{
    /// <summary>
    /// The response from the AI service
    /// </summary>
    public class OpenAiCompletionResponse
    {
        /// <summary>
        /// Creates a new instance of <see cref="OpenAiCompletionResponse"/>
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static OpenAiCompletionResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<OpenAiCompletionResponse>(json);
        }

        public class ChoiceObject
        {
            [JsonProperty("text")]
            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonProperty("index")]
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonProperty("finish_reason")]
            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; }

            [JsonProperty("logprobs")]
            [JsonPropertyName("logprobs")]
            public object LogProbs { get; set; }
        }

        public class UsageObject
        {
            [JsonProperty("completion_tokens")]
            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonProperty("prompt_tokens")]
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonProperty("total_tokens")]
            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("@object")]
        [JsonPropertyName("@object")]
        public string @Object { get; set; }

        [JsonProperty("created")]
        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonProperty("model")]
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonProperty("choices")]
        [JsonPropertyName("choices")]
        public List<ChoiceObject> Choices { get; set; }

        [JsonProperty("usage")]
        [JsonPropertyName("usage")]
        public UsageObject Usage { get; set; }

        /// <summary>
        /// The first choice from the response, appropriate for most cases
        /// </summary>
        public string? FirstChoice() { return Choices?.FirstOrDefault()?.Text; }
    }
}