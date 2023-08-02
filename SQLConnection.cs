using Npgsql;

namespace Crud {
    public class SQLConnection {

        private NpgsqlConnection? connection;
        public NpgsqlConnection Connection {
            get {
                if (connection is null) throw new Exception("Connection is not opened");
                return connection;
            }
        }
        private readonly string connString;

        public SQLConnection(string name, string user, string passwd) {
            connString = $"Server = localhost; User Id = {user}; Password = {passwd}; Database = {name}";
        }

        public void Close() {
            connection?.Close();
        }

        public bool Open() {
            try {
                connection = new NpgsqlConnection(connString);
                connection.Open();
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine("Error connecting to database: " + ex.Message );  
                return false;
            }
        }
        
    }
}