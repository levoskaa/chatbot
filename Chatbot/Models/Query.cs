using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace ChatBot.StatementModels
{
    public class Query
    {
        public string ObjectType { get; set; }

        public List<Statement> statements = new List<Statement>();

        public void Reset()
        {
            ObjectType = string.Empty;
            statements.Clear();
        }

        public Statement GetByID(int ID)
        {
            return statements[ID - 1];
        }

        public void DeleteByID(int ID)
        {
            statements.RemoveAt(ID - 1);
        }

        public void UpdateStatement(int id, Statement statement)
        {
            statements[id-1] = statement;
        }

        public string List(string msg)
        {
            string all = "You haven't given me contstraints for the " + ObjectType + " so far!";
          
            if (statements?.Count > 0)
            {
                all = msg;
                foreach (Statement item in statements)
                {
                    all += Environment.NewLine + item.ToString();
                }
            }
            return all;
        }

        public void AddStatement(Statement statement)
        {
            statements.Add(statement);
        }
    }
}
