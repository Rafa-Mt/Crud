using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using System.Windows.Markup;

namespace Crud {
    class Program {
        static void Main(string[] args) {

            string dbName;

            if (args.Length <= 0) dbName = "Estudiante";
            else dbName = args[0];

            Crud database = new(databaseName: dbName);

            try {
                database.OpenConnection();
                var tables = database.Tables;

                Console.WriteLine();
                PrintList(tables);
                Console.WriteLine();

                PrintDataTable(database.Read(
                    table: "carrera",
                    column: "*"
                ));              

                // * Create 

                PrintDataTable(database.Read(
                    table: "carrera",
                    column: "*",
                    cond: "nombre",
                    equal: "Mechanic"
                ));
                database.Create(
                    table: "carrera",
                    values: new() {"Mechanic", 14}
                );
                PrintDataTable(database.Read(
                    table: "carrera",
                    column: "*",
                    cond: "nombre",
                    equal: "Mechanic"
                ));

                Thread.Sleep(5000);
                Console.WriteLine("\n####################################\n");

                // * Update

                PrintDataTable(database.Read(
                    table:"carrera",
                    column:"*",
                    cond: "id",
                    equal: 11
                ));

                database.Update(
                    table:"carrera", 
                    values: new List<dynamic>(){"Example", 11}, 
                    cond: "id", 
                    equal: 11
                );

                PrintDataTable(database.Read(
                    table:"carrera",
                    column:"*",
                    cond: "id",
                    equal: 11
                ));

                Thread.Sleep(5000);
                Console.WriteLine("\n####################################\n");

                // * Delete 
                PrintDataTable(database.Read(
                    table: "carrera",
                    column: "*",
                    cond: "nombre",
                    equal: "Mechanic"
                ));
                database.Delete(
                    table: "carrera",
                    cond: "nombre",
                    equal: "Mechanic"
                );
                PrintDataTable(database.Read(
                    table: "carrera",
                    column: "*",
                    cond: "nombre",
                    equal: "Mechanic"
                ));               
            }
            finally {
                database.CloseConnection();
            }
        }

        static void PrintDataTable(DataTable table,  string name="") {
            Console.WriteLine($"{name}:");
            Console.WriteLine($"{table.Select().Count()} entries");
            foreach (DataRow row in table.Rows) {
                Console.WriteLine("{ ");
                foreach (DataColumn column in table.Columns) {
                    Console.WriteLine($"\t{column.ColumnName}: {row[column.ColumnName]} ");
                }
                Console.WriteLine("},");
            }
            Console.WriteLine("==============================================================================================");
        }

        static void PrintList(List<string> input) {
            Console.Write("[ ");
            if (input.Count > 0) Console.Write(string.Join(", ", input));
            Console.Write(" ]\n");
        }

        static void PrintAllTables(Crud db) {
            var tables = db.Tables;
            foreach (var table in tables ) {
                var detailedTables = db.Read(table);
                PrintDataTable(detailedTables, table);
            }
        }
    }
}