using Chatbot.CognitiveModels;
using Chatbot.Interfaces;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Chatbot.Utility
{
    public class SimpleQueryHandler : QueryHandlerBase, ISimpleQueryHandler
    {
        public SimpleQueryHandler(ConversationState conversationState)
            : base(conversationState)
        {
        }

        public async Task<string> AddObjectTypeAsync(SimpleModel luisResult, ITurnContext context)
        {
            var objectType = luisResult.Entities.subject[0];
            // TODO: handle table name synonyms
            // TODO: handle plural/singular forms
            // TODO: handle letter casing
            var conversationData = await conversationStateAccessors.GetAsync(context, () => new ConversationData());
            conversationData.Query.From(objectType);
            return objectType;
        }

        public async Task<string> AddStatementAsync(SimpleModel luisResult, ITurnContext context)
        {
            // var statement
            return "";
        }
    }
}