using Chatbot.CognitiveModels;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using SqlKata;
using System.Threading.Tasks;

namespace Chatbot.Interfaces
{
    public interface ISimpleQueryHandler : IQueryHandler
    {
        Task<string> AddObjectTypeAsync(SimpleModel luisResult, ITurnContext context);

        Task<string> AddStatementAsync(SimpleModel luisResult, ITurnContext context);

        Task AddStatementAsync(Statement statement, Query query, ITurnContext context);
    }
}