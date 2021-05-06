using SqlKata;
using System.Collections.Generic;

namespace Chatbot.Models
{
    public class ConversationData
    {
        public string SpecifiedObjectType { get; set; }
        public Query Query { get; set; }
        public List<Statement> Statements { get; set; } = new List<Statement>();
        public bool ObjectTypeKnown { get; set; } = false;
        public CognitiveModel ModelBeingUsed { get; set; } = CognitiveModel.Complex;
        public string CurrentIntent { get; set; }
        public string CurrentTableName { get; internal set; }
        public string StatementToEdit { get; set; }
        public bool Edited { get; set; }
    }
}