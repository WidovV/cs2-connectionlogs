using CounterStrikeSharp.API.Core;
using Nexd.MySQL;

namespace ConnectionLogs
{
    internal class Queries
    {
        public static void InsertNewClient(MySqlDb Db, CCSPlayerController player)
        {
            MySqlQueryValue values = new MySqlQueryValue()
                                    .Add("ClientName", player.PlayerName)
                                    .Add("SteamId", player.SteamID.ToString());

            Db!.Table("Users").InsertIfNotExistAsync(values, $"`ClientName` = '{player.PlayerName}'");
        }

        /// <summary>
        /// Updates the user with the given Steam ID and client name in the database.
        /// </summary>
        /// <param name="steamId">The Steam ID of the user to update.</param>
        /// <param name="clientName">The name of the client to update for the user.</param>
        private static void UpdateUser(MySqlDb Db, string steamId, string clientName)
        {
            MySqlQueryValue values = new MySqlQueryValue()
                                    .Add("ClientName", clientName);

            Db.Table("Users").Where(MySqlQueryCondition.New("SteamId", "=", steamId)).UpdateAsync(values);
        }

        // This shouldn't be static, but i'm making the call inside of a static method, so yeah 
        /// <summary>
        /// Creates a table named "Users" in the provided MySQL connection if it doesn't exist already.
        /// The table has columns for Id (auto-incrementing integer), SteamId (string), ClientName (string), and ConnectedAt (timestamp).
        /// SteamId is set as a unique key.
        /// </summary>
        /// <param name="connection">The MySqlConnection object to use for creating the table.</param>


        /// <summary>
        /// Retrieves a list of the 50 most recently connected users from the database.
        /// </summary>
        /// <returns>A list of User objects representing the connected players.</returns>
        public static List<User> GetConnectedPlayers(MySqlDb Db)
        {
            MySqlQueryResult result = Db.Table("Users").ExecuteQueryAsync("SELECT Id, SteamId, ClientName, ConnectedAt FROM `Users` ORDER BY `ConnectedAt` DESC LIMIT 50").Result;

            if (result.Rows < 1)
            {
                return new();
            }

            List<User> users = new();

            foreach (KeyValuePair<int, MySqlFieldValue> pair in result)
            {
                User user = new()
                {
                    Id = Convert.ToInt32(pair.Value["Id"]),
                    SteamId = pair.Value["SteamId"].ToString(),
                    ClientName = pair.Value["ClientName"].ToString(),
                    ConnectedAt = DateTime.Parse(pair.Value["ConnectedAt"].ToString())
                };

                users.Add(user);
            }
            return users;
        }
    }
}