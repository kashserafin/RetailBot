using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Orchestrator;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;

namespace RetailBot.Services
{
    public class BotServices : IBotServices
    {
        public OrchestratorRecognizer Dispatch { get; private set; }

        public LuisRecognizer LuisRetailBot { get; private set; }

        public LuisRecognizer LuisGeneral { get; private set; }

        public QnAMaker QnA { get; private set; }

        public BotServices(IConfiguration configuration, OrchestratorRecognizer dispatcher)
        {
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            // If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
            // will be made available in the properties collection of the RecognizerResult
            LuisRetailBot = CreateLuisRecognizer(configuration, "LuisRetailBot");
            LuisGeneral = CreateLuisRecognizer(configuration, "LuisGeneral");

            Dispatch = dispatcher;

            QnA = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAEndpointKey"],
                Host = configuration["QnAEndpointHostName"]
            });
        }

        private LuisRecognizer CreateLuisRecognizer(IConfiguration configuration, string appIdKey)
        {
            var luisApplication = new LuisApplication(
                configuration[appIdKey],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com");

            // Set the recognizer options depending on which endpoint version you want to use.
            // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                IncludeAPIResults = true,
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true,
                    Slot = configuration["LuisSlot"]
                }
            };

            return new LuisRecognizer(recognizerOptions);
        }
    }
}
