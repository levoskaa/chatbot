using Chatbot.CognitiveModels;
using Chatbot.Interfaces;

namespace Chatbot.Utility
{
    public class SimpleQueryHandler : QueryHandlerBase, ISimpleQueryHandler
    {
        public void AddObjectType(SimpleModel luisResult)
        {
            throw new System.NotImplementedException();
        }

        public void AddConstraint(SimpleModel luisResult)
        {
            throw new System.NotImplementedException();
        }
    }
}