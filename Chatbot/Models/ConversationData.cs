﻿using SqlKata;
using System.Collections.Generic;

namespace Chatbot.Models
{
    public class ConversationData
    {
        public Query Query { get; set; }
        public List<string> Statements { get; set; }
        public bool ObjectTypeKnown { get; set; } = false;
        public CognitiveModel ModelBeingUsed { get; set; } = CognitiveModel.Complex;
        public string CurrentIntent { get; set; }
    }
}