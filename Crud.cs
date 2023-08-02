using Npgsql;
using System.Data;

namespace Crud {
    class Crud {
        private readonly SQLConnection pgAdapter;
        public List<string> Tables {
            get {
                List<string?> tableList = new();
                DataTable tables = Read("table_name", "information_schema.tables", "table_schema", "public");
                foreach (DataRow table in tables.Rows) {
                    tableList.Add(table["table_name"].ToString());
                }

                List<string> notNullList = new();
                foreach (var table in tableList) {
                    if (table is not null) {
                        notNullList.Add(table);
                    }
                }

                return notNullList;
            }
        }

        public Crud(string databaseName) {
            pgAdapter = new(name: databaseName, user:"ExAdmin", passwd:"exadmin");
        }

        public void OpenConnection() {
            bool opened = pgAdapter.Open();  
            if (opened) Console.WriteLine("Connection done succesfully");
        }

        public void CloseConnection() {
            pgAdapter.Close();
            Console.WriteLine("Connection closed");
        }

        private NpgsqlCommand NewQuery(string content) {
            return new NpgsqlCommand(content, pgAdapter.Connection);
        }

        public void Create(string table, List<dynamic> values, List<string> columns) {
            // Query: INSERT INTO {table} ({columns}) VALUES ({values})

            List<dynamic> newValues = new();

            values.ForEach((value) => newValues.Add(QuoteIfString(value)));

            if (columns.Count != newValues.Count) 
                throw new DataException($"The values and the fields given doesn't match properly ({columns.Count} =/= {newValues.Count})");
            
            string columnQuery = $"({string.Join(",", columns)})";
            string valuesQuery = $"({string.Join(",", newValues )})";
            string query = $"INSERT INTO {table} {columnQuery} values {valuesQuery}";
            Console.WriteLine($"Query: {query}");

            NpgsqlCommand command = NewQuery(query);
            command.ExecuteNonQuery();
        }

        public void Create(string table, List<dynamic> values) {
            List<string> columns = GetColumns(table);
            columns.RemoveAll((column) => column == "id");
            Create(table, values, columns);
        }

        public DataTable Read(string table, string column = "*", string misc = "") {
            // Query: SELECT {columns} FROM {table}

            string query = $"SELECT {column} FROM {table} {misc}";
            
            NpgsqlCommand command = NewQuery(query);
            NpgsqlDataAdapter adapter = new (command);
            DataTable data = new();
            adapter.Fill(data);
            
            Console.WriteLine($"Query: {query}");

            return data;
        }

        public DataTable Read(string column, string table, string cond, dynamic equal, string misc="") {
            // Query: SELECT {columns} FROM {table} WHERE {conditions}

            equal = QuoteIfString(equal);
            string query = $"SELECT {column} FROM {table} WHERE {cond} = {equal} {misc}";

            NpgsqlCommand command = NewQuery(query);
            NpgsqlDataAdapter adapter = new (command);
            DataTable data = new();
            adapter.Fill(data);
            
            Console.WriteLine($"Query: {query}");

            return data;
        }

        public void Update(string table, List<string> fields, List<dynamic> values, string cond, dynamic equal, string misc = "") {
            // Query: UPDATE {table} SET [ {field} = {value} ] WHERE {condition}

            if (fields.Count != values.Count) 
                throw new DataException($"The values and the fields given doesn't match properly ({fields.Count} =/= {values.Count})");

            equal = QuoteIfString(equal);
            var setlist = fields.Zip(values, (f,v) => new {field = f, value = v});

            List<string> changeList = new();
            foreach (var set in setlist) {
                changeList.Add($"{set.field} = {QuoteIfString(set.value)}");
            }
            string changes = string.Join(", ", changeList);
            string query = $"UPDATE {table} SET {changes} WHERE {cond} = {equal} {misc}";
            Console.WriteLine($"Query: {query}");

            NpgsqlCommand command = NewQuery(query);
            command.ExecuteNonQuery();
        }

        public void Update(string table, List<dynamic> values, string cond, dynamic equal, string misc = "") {
            List<string> columns = GetColumns(table);
            columns.RemoveAll((column) => column == "id");
            Update(
                table: table,
                fields: columns,
                values: values,
                cond: cond,
                equal: equal,
                misc: misc
            );
        }

        public void Delete(string table, string cond, dynamic equal, string misc = "") {
            // Query: DELETE FROM {table} WHERE {condition}
            equal = QuoteIfString(equal);
            string query = $"DELETE FROM {table} WHERE {cond} = {equal} {misc}";
            Console.WriteLine($"Query: {query}");

            NpgsqlCommand command = NewQuery(query);
            command.ExecuteNonQuery();
        }

        public void Empty(string table, string misc="") {
            // Query: DELETE FROM {table}
            string query = $"DELETE FROM {table} {misc}";
            NpgsqlCommand command = NewQuery(query);
            Console.WriteLine($"Query: {query}");

            command.ExecuteNonQuery();
        }

        public List<string> GetColumns(string table) {
            List<string> columnList = new(); 
            DataTable tester = Read(column:"*", table:table, misc:"limit 1");
            foreach (DataColumn column in tester.Columns) {
                columnList.Add(column.ColumnName);
            }
            return columnList;
        }

        public static dynamic QuoteIfString(dynamic input, char character = '\'') {
            if (input is string)  input = $"{character}{input}{character}";
            return input;
        }
    }
}