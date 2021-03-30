using Chatbot.CognitiveModels;
using Chatbot.Interfaces;
using System.Collections.Generic;

namespace Chatbot.Utility
{
    public class ComplexQueryHandler : IComplexQueryHandler
    {
        public List<string> Constraints => throw new System.NotImplementedException();

        public void AddConstraint(ComplexModel luisResult)
        {
            throw new System.NotImplementedException();
        }
    }
}