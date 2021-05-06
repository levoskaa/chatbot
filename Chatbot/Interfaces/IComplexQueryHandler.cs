using Chatbot.CognitiveModels;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using SqlKata;
using System.Threading.Tasks;

namespace Chatbot.Interfaces
{
    public interface IComplexQueryHandler : IQueryHandler
    {
        Task<string> AddObjectTypeAsync(ComplexModel luisResult, ITurnContext context);

        Task<string> AddStatementAsync(ComplexModel luisResult, ITurnContext context);

        Task AddStatementAsync(Statement statement, Query query, ITurnContext context);
    }
}