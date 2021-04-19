using Chatbot.CognitiveModels;
using Chatbot.Interfaces;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Chatbot.Utility
{
    public class ComplexQueryHandler : QueryHandlerBase, IComplexQueryHandler
    {
        public ComplexQueryHandler(ConversationState conversationState)
            : base(conversationState)
        {
        }

        public Task<string> AddObjectTypeAsync(ComplexModel luisResult, ITurnContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> AddStatementAsync(ComplexModel luisResult, ITurnContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}