using Microsoft.Bot.Builder;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatbot.Interfaces
{
    public interface IQueryHandler
    {
        Task<List<string>> GetStatementsAsync(ITurnContext context);
    }
}