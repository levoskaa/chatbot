using Chatbot.CognitiveModels;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Chatbot.Interfaces
{
    public interface ISimpleQueryHandler : IQueryHandler
    {
        Task<string> AddObjectTypeAsync(SimpleModel luisResult, ITurnContext context);

        Task<string> AddStatementAsync(SimpleModel luisResult, ITurnContext context);
    }
}