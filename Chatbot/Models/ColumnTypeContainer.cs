using System;
using System.Collections.Generic;

namespace Chatbot.Models
{
    public class ColumnTypeContainer
    {
        private static ColumnTypeContainer instance = null;
        private static readonly object padlock = new object();

        ColumnTypeContainer()
        {
        }

        public static ColumnTypeContainer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ColumnTypeContainer();
                    }
                    return instance;
                }
            }
        }

        private Dictionary<string, Dictionary<string, string>> columnNamesByTableName = new Dictionary<string, Dictionary<string, string>>();

        public void AddColumn(string tableName, string columnName, string columnType) {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            if (columnNamesByTableName.ContainsKey(tableName))
            {
                var table = columnNamesByTableName.GetValueOrDefault(tableName);
                table.Add(columnName, columnType);
            }
            else {

                temp.Add(columnName, columnType);
                columnNamesByTableName.Add(tableName, temp);
            }
        }

        public bool IsString(Statement statement)
        {
            try
            {
                var tableName = columnNamesByTableName.GetValueOrDefault(statement.Subject);
                if (tableName != null)
                {
                    var column = tableName.GetValueOrDefault(statement.Property);
                    if (column.Contains("nvarchar")) return true;
                    return false;
                }
            }
            catch(Exception e){
                Console.Out.WriteLine(e.Message);
            }
            return false;
        }
    }
}
