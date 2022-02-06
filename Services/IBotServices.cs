using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Orchestrator;
using Microsoft.Bot.Builder.AI.QnA;

namespace RetailBot.Services
{
    public interface IBotServices
    {
        OrchestratorRecognizer Dispatch { get; }

        LuisRecognizer LuisRetailBot { get; }

        LuisRecognizer LuisGeneral { get; }

        QnAMaker QnA { get; }
    }
}
