using CounterStrikeSharp.API.Core;
using Nexd.MySQL;

namespace ConnectionLogs
{
    internal class Queries
    {
        public static void InsertNewClient(MySqlDb? db, CCSPlayerController player, string ip)
        {
            MySqlQueryValue values = new MySqlQueryValue()
                                    .Add("ClientName", player.PlayerName)
                                    .Add("SteamId", player.SteamID.ToString())
                                    .Add("IpAddress", ip);

            db!.Table("Users").InsertIfNotExistAsync(values, $"`ClientName` = '{player.PlayerName}'");
        }

        private static void UpdateUser(MySqlDb db, string steamId, string clientName)
        {
            MySqlQueryValue values = new MySqlQueryValue().Add("ClientName", clientName);

            db.Table("Users").Where(MySqlQueryCondition.New("SteamId", "=", steamId)).UpdateAsync(values);
        }

        public static List<User> GetConnectedPlayers(MySqlDb? db)
        {
            MySqlQueryResult result = db.Table("Users")
                .ExecuteQueryAsync("SELECT Id, SteamId, ClientName, ConnectedAt FROM `Users` ORDER BY `ConnectedAt` DESC LIMIT 50").Result;

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