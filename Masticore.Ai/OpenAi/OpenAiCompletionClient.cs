using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

namespace Masticore.Ai
{
    /// <summary>
    /// OpenAI implementation of <see cref="IAiClient"/>, its exact detail is driven by the underlying <see href="https://learn.microsoft.com/en-us/azure/cognitive-services/openai/concepts/models#gpt-3-models">GPT-3 Model Type</see> and the parameters sent via <see cref="OpenAiCompletionRequest"/>
    /// </summary>
    /// <remarks>Implement <see href="https://platform.openai.com/docs/guides/completion/prompt-design">OpenAI API Docs</see> for more details</remarks>
    public class OpenAiCompletionClient : IAiClient
    {
        /// <summary>
        /// This is determined by OpenAI's API limitations
        /// </summary>
        public const int MaxTotalTokens = OpenAiCompletionRequest.DefaultMaxTokens;

        public const int MinResponseTokens = 256;

        public const int MaxRequestTokens = MaxTotalTokens - MinResponseTokens;

        protected ILogger<OpenAiCompletionClient> Logger { get; }

        /// <summary>
        /// The settings for this client
        /// </summary>
        protected OpenAiSettings Settings { get; }

        /// <summary>
        /// Factory for making clients
        /// </summary>
        protected IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Stores a reference to the web client used to make HTTP calls.
        /// </summary>
        protected HttpClient WebClient { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="OpenAiCompletionClient"/>
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="httpClientFactory"></param>
        public OpenAiCompletionClient(
            ILogger<OpenAiCompletionClient> logger,
            OpenAiSettings settings,
            IHttpClientFactory httpClientFactory)
        {
            Logger = logger;
            Settings = settings;
            HttpClientFactory = httpClientFactory;
            WebClient = httpClientFactory.CreateClient();
            WebClient.DefaultRequestHeaders.TryAddWithoutValidation("api-key", Settings.ApiKey);
        }

        /// <summary>
        /// URL to which the request is made
        /// </summary>
        public virtual string Endpoint =>
            // [OPEN AI ENDPOINT]openai/deployments/test-playground/completions
            $"{Settings.Endpoint}openai/deployments/{Settings.Deployment}/completions?api-version=2022-12-01";

        /// <summary>
        /// Make a basic request to the AI
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<OpenAiCompletionResponse> Request(string prompt)
        {
            OpenAiCompletionRequest req = new OpenAiCompletionRequest(prompt);
            OpenAiCompletionResponse response = await RequestCompletion(req);
            return response;
        }

        /// <summary>
        /// Makes a basic POST request to the completion API
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<OpenAiCompletionResponse> RequestCompletion(OpenAiCompletionRequest req)
        {
            try
            {
                // https://platform.openai.com/docs/guides/completion
                string json = req.ToJson();
                string endpoint = Endpoint;
                Logger.LogInformation($"Requesting OpenAI Completion from endpoint {endpoint}...");
                HttpRequestMessage? request = new HttpRequestMessage(new HttpMethod("POST"), endpoint);
                request.Content = new StringContent(json);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                HttpResponseMessage httpResponse = await WebClient.SendAsync(request);
                string responseText = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Bad status code {httpResponse.StatusCode} return from OpenAI API call with response: {responseText}");
                }

                OpenAiCompletionResponse response = OpenAiCompletionResponse.FromJson(responseText);
                Logger.LogInformation($"OpenAI completion succeeded with tokens {response.Usage.TotalTokens} total tokens used");
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error calling OpenAI endpoint '{Endpoint}' with error {ex.Message} and trace {ex.StackTrace}");
                throw;
            }
        }

        public static int? EstimatedTokensForCharacterCount(int? charCount)
        {
            if (charCount == null)
            {
                return null;
            }
            return charCount / 4;
        }

        /// <summary>
        /// Takes an estimated first N tokens from the given string
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public static string MaxRequestPrompt(string prompt)
        {
            int estimatedMaxLength = MaxRequestTokens * 3;
            if (prompt.Length <= estimatedMaxLength)
            {
                return prompt;
            }
            return prompt[..estimatedMaxLength];
        }

        #region IAiClient

        public async Task<AiChatResponse> RequestChat(AiChatRequest aiReq)
        {
            // There is a total token limit that we need to
            string input = MaxRequestPrompt(aiReq.Prompt);
            int estimatedPromptTokens = input.Length / 3;
            // We allow at least the min response size, and then some padding
            int maxResponseTokens = Math.Max(MaxTotalTokens - estimatedPromptTokens, MinResponseTokens) - 16;
            OpenAiCompletionRequest openAiRequest = new OpenAiCompletionRequest(input)
            {
                Temperature = aiReq.Divergence ?? OpenAiCompletionRequest.DefaultTemperature,
                MaxTokens = maxResponseTokens,
            };
            OpenAiCompletionResponse openAiResponse = await RequestCompletion(openAiRequest);
            AiChatResponse response = new AiChatResponse() { Text = openAiResponse.FirstChoice() };
            return response;
        }

        #endregion
    }

}