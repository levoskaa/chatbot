using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Recognizers
{
    public class ComplexStatementRecognizer : IRecognizer
    {
        private readonly LuisRecognizer recognizer;

        public ComplexStatementRecognizer(IConfiguration configuration)
        {
            var luisIsConfigured = !string.IsNullOrEmpty(configuration["Luis:Complex:AppId"]) && !string.IsNullOrEmpty(configuration["Luis:APIKey"]) && !string.IsNullOrEmpty(configuration["Luis:APIHostName"]);
            if (luisIsConfigured)
            {
                var luisApplication = new LuisApplication(
                    configuration["Luis:Complex:AppId"],
                    configuration["Luis:APIKey"],
                    "https://" + configuration["Luis:APIHostName"]);
                // Set the recognizer options depending on which endpoint version you want to use.
                // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
                var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
                {
                    PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                    {
                        IncludeInstanceData = true,
                    }
                };

                recognizer = new LuisRecognizer(recognizerOptions);
            }
        }

        // Returns true if luis is configured in the appsettings.json and initialized.
        public virtual bool IsConfigured => recognizer != null;

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await recognizer.RecognizeAsync(turnContext, cancellationToken);
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            return await recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
        }
    }
}