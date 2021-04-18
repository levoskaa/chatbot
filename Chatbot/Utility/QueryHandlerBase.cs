using Chatbot.Interfaces;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatbot.Utility
{
    public abstract class QueryHandlerBase : IQueryHandler
    {
        protected readonly IStatePropertyAccessor<ConversationData> conversationStateAccessors;

        public QueryHandlerBase(ConversationState conversationState)
        {
            conversationStateAccessors = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
        }

        public async Task<List<string>> GetStatementsAsync(ITurnContext context)
        {
            var conversationData = await conversationStateAccessors.GetAsync(context, () => new ConversationData());
            return conversationData.Statements;
        }
    }
}