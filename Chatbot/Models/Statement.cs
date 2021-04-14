using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatbot.Models
{
    public class Statement
    {
        public string Text { get; set; }

        public string Subject { get; set; }

        public string Property { get; set; }

        public string[] Value { get; set; }

        public bool Negated { get; set; }

        public bool Smaller { get; set; }

        public bool Bigger { get; set; }

        public bool MultipleValues { get; set; }

        public bool DateValues { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
