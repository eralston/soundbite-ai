//using Masticore.Tests;
//using Xunit;

//namespace Masticore.Ai.Tests
//{
//    /// <summary>
//    /// Unit tests against the OpenAI Completion API directly
//    /// </summary>
//    public class OpenAiCompletionTests
//    {
//        [Fact]
//        public async void BasicCompletion()
//        {
//            IHttpClientFactory factory = Masticore.Tests.Extensions.CreateHttpClientFactory();

//            // TODO: Move credentials to something more secret
//            OpenAiSettings settings = new OpenAiSettings()
//            {
//                Region = "southcentralus",
//                ApiKey = "[OPEN AI API KEY]",
//                Endpoint = "[OPEN AI ENDPOINT]",
//                Deployment = "test-playground"
//            };
//            OpenAiCompletionClient client = new OpenAiCompletionClient(ThrowIfErrorLogger<OpenAiCompletionClient>.Instance, settings, factory);
//            OpenAiCompletionRequest req = new OpenAiCompletionRequest("What is the airspeed velocity of an unlaiden swallow?")
//            {
//                // Make the output deterministic
//                Temperature = 0,
//                MaxTokens = 2030,
//            };
//            OpenAiCompletionResponse response = await client.RequestCompletion(req);

//            // This result could theoretically change over time without this unit test changing
//            Assert.Equal("The airspeed velocity of an unlaiden swallow is about 1 meter per second.", response.Choices[0].Text.Trim());
//            //Assert.Equal("The airspeed velocity of an unlaiden swallow is about 24 miles per hour.", response.Choices[0].Text.Trim());
//        }
//    }
//}