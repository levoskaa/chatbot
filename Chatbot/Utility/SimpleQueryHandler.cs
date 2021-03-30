using Chatbot.CognitiveModels;
using Chatbot.Interfaces;
using System.Collections.Generic;

namespace Chatbot.Utility
{
    public class SimpleQueryHandler : ISimpleQueryHandler
    {
        public List<string> Constraints => throw new System.NotImplementedException();

        public void AddConstraint(SimpleModel luisResult)
        {
            throw new System.NotImplementedException();
        }
    }
}