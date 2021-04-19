using Chatbot.CognitiveModels;
using Chatbot.Models;

namespace Chatbot.Interfaces
{
    public interface IComplexQueryHandler : IQueryHandler
    {
        void AddConstraint(ComplexModel luisResult);
        public (string, Statement) HandleStatement(ComplexModel luisResult);
    }
}