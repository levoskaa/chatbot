using Chatbot.CognitiveModels;
using Chatbot.Interfaces;

namespace Chatbot.Utility
{
    public class ComplexQueryHandler : QueryHandlerBase, IComplexQueryHandler
    {
        public void AddConstraint(ComplexModel luisResult)
        {
            throw new System.NotImplementedException();
        }
    }
}