using System.Collections;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Transactions;

namespace IconPosBackup
{
    class DatabaseHelper
    {
        internal static string DB_PATH = ".\\db.sqlite";

        private const string TABLE_NAME = "IconBackups";
        private static string CONNECTION_STRING(string dbPath) => $"Data Source={dbPath ?? ".\\db.sqlite"};Version=3;";

        public static void EnsureDBExists()
        {
            string directory = Path.GetDirectoryName(DB_PATH) ?? "";
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

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

        public static List<RegistryReadWrite.RegistryItem> ReadBackupItem(ulong? backupId)
        {
            if (backupId == null) return [];

            List<RegistryReadWrite.RegistryItem> items = [];

            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            string query = $@"
                SELECT type, var_name, value 
                FROM {TABLE_NAME} 
                WHERE PART_ID = @BackupId;";

            using (SQLiteCommand cmd = new(query, connection))
            {
                cmd.Parameters.AddWithValue("@BackupId", backupId);

                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    long varType = (long)reader["type"];

                    if (reader["value"] is byte[] blobData)
                        switch (varType)
                        {
                            case 0: // REG_DWORD (Integer)
                                if (blobData != null && blobData.Length > 0)
                                {
                                    int value = 0;

                                    string hexString = BitConverter.ToString(blobData).Replace("-", " ");
                                    Debug.WriteLine($"Hex Data: {hexString}");

                                    if (blobData.Length == 4)
                                        value = BitConverter.ToInt32(blobData, 0);
                                    else if (blobData.Length == 2)
                                        value = BitConverter.ToInt16(blobData, 0);
                                    else if (blobData.Length == 1)
                                        value = blobData[0];

                                    items.Add(new RegistryReadWrite.RegistryItem
                                    {
                                        Type = varType,
                                        KeyName = reader["var_name"]?.ToString(),
                                        Value = value
                                    });
                                }
                                break;

                            case 1: // REG_SZ (String)
                                items.Add(new RegistryReadWrite.RegistryItem
                                {
                                    Type = varType,
                                    KeyName = reader["var_name"]?.ToString(),
                                    Value = blobData != null ? System.Text.Encoding.UTF8.GetString(blobData) : string.Empty
                                });
                                break;

                            case 2: // REG_BINARY (Byte Array)
                                items.Add(new RegistryReadWrite.RegistryItem
                                {
                                    Type = varType,
                                    KeyName = reader["var_name"]?.ToString(),
                                    Value = blobData ?? []
                                });
                                break;
                        }

                }
            }

            return items;
        }


        public static void InsertDataList(List<RegistryReadWrite.RegistryItem> items, string BackupName)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            using SQLiteTransaction transaction = connection.BeginTransaction();

            try
            {
                ulong partId = 0;

                string partIdQuery = $"SELECT COALESCE(MAX(PART_ID), 0) FROM {TABLE_NAME};";

                using (SQLiteCommand cmd = new(partIdQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    partId = (ulong)(Convert.ToInt64(result) + 1);
                }

                string insertQuery = $@"
                    INSERT INTO {TABLE_NAME} (PART_ID, name, type, var_name, value)
                    VALUES (@PartId, @Name, @Type, @VarName, @Value);";

                using (SQLiteCommand cmd = new(insertQuery, connection))
                {
                    foreach (var item in items)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@PartId", partId);
                        cmd.Parameters.AddWithValue("@Name", BackupName);
                        cmd.Parameters.AddWithValue("@Type", item.Type);
                        cmd.Parameters.AddWithValue("@VarName", item.KeyName);
                        cmd.Parameters.AddWithValue("@Value", item.Value ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine($"Error inserting backup in DB: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        public static ObservableCollection<IconPosBackupItem> GetItemsViewList()
        {
            ObservableCollection<IconPosBackupItem> returnList = [];

            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            string query = $@"SELECT DISTINCT PART_ID, name FROM {TABLE_NAME};";

            using (SQLiteCommand cmd = new(query, connection))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ulong partId = (ulong)reader.GetInt64(0);
                    string name = reader.GetString(1);

                    IconPosBackupItem itemToAdd = new() { Title = name, Id = partId };
                    returnList.Add(itemToAdd);
                }
            }

            connection.Close();
            return returnList;
        }

        public static void RenameBackup(ulong? backupId, string newName)
        {
            if (newName == null || backupId == null) return;

            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            using SQLiteTransaction transaction = connection.BeginTransaction();

            try
            {
                string renameQuery = $@"
                    UPDATE {TABLE_NAME}
                    SET name = @NewName
                    WHERE PART_ID = @BackupId;";

                using (SQLiteCommand cmd = new(renameQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@NewName", newName);
                    cmd.Parameters.AddWithValue("@BackupId", backupId);

                    int renamedRows = cmd.ExecuteNonQuery();
                    Debug.WriteLine($"Renamed {renamedRows} rows.");
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine($"Error renaming backup in DB: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        public static void DeleteBackup(ulong? id)
        {
            if (id == null) return;

            using SQLiteConnection connection = new(CONNECTION_STRING(DB_PATH));
            connection.Open();

            using SQLiteTransaction transaction = connection.BeginTransaction();

            try
            {
                string renameQuery = $@"
                    DELETE FROM {TABLE_NAME}
                    WHERE PART_ID = @BackupId;";

                using (SQLiteCommand cmd = new(renameQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@BackupId", id);

                    int renamedRows = cmd.ExecuteNonQuery();
                    Debug.WriteLine($"Deleted {renamedRows} rows.");
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine($"Error renaming backup in DB: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}