using ConnectedPlayers;
using MySqlConnector;

namespace ConnectionLogs
{
    internal class Queries
    {
        private static MySqlConnection Connection => Database.GetConnection();

        /// <summary>
        /// Checks if a connection to the database can be established and creates a table if it doesn't exist.
        /// </summary>
        /// <returns>True if a connection to the database was established and the table was created, false otherwise.</returns>
        private static bool DatabaseConnected()
        {
            try
            {
                using MySqlConnection connection = Database.GetConnection();
                CreateTable(connection);
                connection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts a new user into the database or updates an existing user's client name.
        /// </summary>
        /// <param name="steamId">The Steam ID of the user.</param>
        /// <param name="clientName">The name of the client.</param>
        public static void InsertUser(string steamId, string clientName, string ipAddress)
        {
            if (!DatabaseConnected())
            {
                return;
            }

            if (UserExists(steamId))
            {
                UpdateUser(steamId, clientName);
                return;
            }

            using MySqlConnection connection = Database.GetConnection();

            using MySqlCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Users (SteamId, ClientName, IpAddress) VALUES (@steamId, @clientName, @ipAddress);";
            command.Parameters.AddWithValue("@steamId", steamId);
            command.Parameters.AddWithValue("@clientName", clientName);
            command.Parameters.AddWithValue("@ipAddress", ipAddress);

            
            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Checks if a user with the given Steam ID exists in the database.
        /// </summary>
        /// <param name="steamId">The Steam ID of the user to check.</param>
        /// <returns>True if the user exists, false otherwise.</returns>
        public static bool UserExists(string steamId)
        {
            using MySqlConnection connection = Database.GetConnection();

            using MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE SteamId = @steamId;";
            command.Parameters.AddWithValue("@steamId", steamId);

            
            var result = command.ExecuteScalar();
            connection.Close();

            return Convert.ToInt32(result) > 0;
        }


        /// <summary>
        /// Updates the user with the given Steam ID and client name in the database.
        /// </summary>
        /// <param name="steamId">The Steam ID of the user to update.</param>
        /// <param name="clientName">The name of the client to update for the user.</param>
        private static void UpdateUser(string steamId, string clientName)
        {
            using MySqlConnection connection = Database.GetConnection();

            using MySqlCommand command = connection.CreateCommand();
            // It should update the ConnectedAt automatically, but yeah it doesn't work
            command.CommandText = "UPDATE Users SET ClientName = @clientName, ConnectedAt = CURRENT_TIMESTAMP WHERE SteamId = @steamId;";
            command.Parameters.AddWithValue("@steamId", steamId);
            // Escpae the shit out of this
            command.Parameters.AddWithValue("@clientName", MySqlHelper.EscapeString(clientName));

            command.ExecuteNonQuery();
            connection.Close();
        }

        // This shouldn't be static, but i'm making the call inside of a static method, so yeah 
        /// <summary>
        /// Creates a table named "Users" in the provided MySQL connection if it doesn't exist already.
        /// The table has columns for Id (auto-incrementing integer), SteamId (string), ClientName (string), and ConnectedAt (timestamp).
        /// SteamId is set as a unique key.
        /// </summary>
        /// <param name="connection">The MySqlConnection object to use for creating the table.</param>
        private static void CreateTable(MySqlConnection connection)
        {
            using MySqlCommand command = connection.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS `Users` (
                                        `Id` int(11) NOT NULL AUTO_INCREMENT,
                                        `SteamId` varchar(18) NOT NULL,
                                        `ClientName` varchar(128) NOT NULL,
                                        `ConnectedAt` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
                                        `IpAddress` varchar(16) DEFAULT NULL,
                                        PRIMARY KEY (`Id`),
                                        UNIQUE KEY `SteamId` (`SteamId`)
                                    );";

            command.ExecuteNonQuery();
            command.Dispose();
        }

        /// <summary>
        /// Retrieves a list of the 50 most recently connected users from the database.
        /// </summary>
        /// <returns>A list of User objects representing the connected players.</returns>
        public static List<User> GetConnectedPlayers()
        {
            if (!DatabaseConnected())
            {
                return new();
            }

            using MySqlConnection connection = Database.GetConnection();

            using MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, SteamId, ClientName, ConnectedAt FROM Users ORDER BY ConnectedAt DESC LIMIT 50;";

            
            MySqlDataReader reader = command.ExecuteReader();
            List<User> users = new();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32("Id"),
                    SteamId = reader.GetString("SteamId"),
                    ClientName = reader.GetString("ClientName"),
                    ConnectedAt = reader.GetDateTime("ConnectedAt")
                });
            }

            connection.Close();

            return users;
        }
    }
}