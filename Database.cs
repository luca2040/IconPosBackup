using System.Data.SQLite;
using System.Diagnostics;

namespace IconPosBackup
{
    class DatabaseHelper
    {
        internal static string DB_PATH = ".\\db.sqlite";

        private const string TABLE_NAME = "IconBackups";
        private static string CONNECTION_STRING(string dbPath) => $"Data Source={dbPath ?? ".\\db.sqlite"};Version=3;";

        public static void EnsureDBExists()
        {
            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            string createTableQuery = $@"
                CREATE TABLE IF NOT EXISTS {TABLE_NAME} (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PART_ID INTEGER NOT NULL,
                    name TEXT NOT NULL,
                    type INTEGER NOT NULL,
                    var_name TEXT NOT NULL,
                    value BLOB
                );";

            using SQLiteCommand command = new(createTableQuery, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void InsertDataList(List<RegistryReadWrite.RegistryItem> items)
        {
            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            foreach (RegistryReadWrite.RegistryItem item in items)
            {
                Debug.WriteLine($"Key: {item.KeyName}, Type: {item.Type}, blob data: {item.Value}");
            }

            connection.Close();
        }
    }
}