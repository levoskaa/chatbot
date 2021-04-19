using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatbot.Models
{
    public class _Entities
    {
        // Simple entities
        public string[] objecttype;

        // Built-in entities
        public DateTimeSpec[] datetime;
        public GeographyV2[] geographyV2;
        public double[] number;
        public string[] personName;


        // Composites
        public class _InstanceValue
        {
            public InstanceData[] date;
            public InstanceData[] personName;
            public InstanceData[] geography;
            public InstanceData[] number;
            public InstanceData[] text;
            public InstanceData[] daterange;
        }
        public class ValueClass
        {
            public DateTimeSpec[] date;
            public string[] personName;
            public GeographyV2[] geography;
            public double[] number;
            public string[] text;
            public DateTimeSpec[] daterange;
            [JsonProperty("$instance")]
            public _InstanceValue _instance;

            public bool isEmpty()
            {
                return date == null && personName == null && geography == null && number == null && text == null;
            }
        }
        public ValueClass[] value;

        public class _InstanceClause
        {
            public InstanceData[] subject;
            public InstanceData[] property;
            public InstanceData[] value;
            public InstanceData[] smaller;
            public InstanceData[] bigger;
            public InstanceData[] negated;
            public InstanceData[] between;
            public InstanceData[] around;
            public InstanceData[] referral;
        }
        public class ClauseClass
        {
            public string[] subject;
            public string[] property;
            public ValueClass[] value;
            public string[] smaller;
            public string[] bigger;
            public string[] negated;
            public string[] between;
            public string[] around;
            public string[] referral;
            [JsonProperty("$instance")]
            public _InstanceClause _instance;
        }
        public ClauseClass[] clause;

        // Instance
        public class _Instance
        {
            public InstanceData[] around;
            public InstanceData[] between;
            public InstanceData[] bigger;
            public InstanceData[] clause;
            public InstanceData[] date;
            public InstanceData[] daterange;
            public InstanceData[] datetime;
            public InstanceData[] geography;
            public InstanceData[] geographyV2;
            public InstanceData[] negated;
            public InstanceData[] number;
            public InstanceData[] objecttype;
            public InstanceData[] personName;
            public InstanceData[] property;
            public InstanceData[] referral;
            public InstanceData[] smaller;
            public InstanceData[] subject;
            public InstanceData[] text;
            public InstanceData[] value;
        }
        [JsonProperty("$instance")]
        public _Instance _instance;
    }
}
