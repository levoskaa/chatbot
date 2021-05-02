using Chatbot.CognitiveModels;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Chatbot.Interfaces
{
    public interface IComplexQueryHandler : IQueryHandler
    {
        Task<string> AddObjectTypeAsync(ComplexModel luisResult, ITurnContext context);

        Task<string> AddStatementAsync(ComplexModel luisResult, ITurnContext context);
    }
}