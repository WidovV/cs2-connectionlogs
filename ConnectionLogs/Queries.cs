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

            db!.Table("Users").InsertIfNotExistAsync(values, $"`ClientName` = '{player.PlayerName}', `LastSeen` = CURRENT_TIMESTAMP()");
        }

        public static void CreateTable(MySqlDb? db)
        {
            try
            {
                Console.WriteLine("Creating table...");
                int result = db.ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS `Users` (
                                            `Id` int(11) NOT NULL AUTO_INCREMENT,
                                            `SteamId` varchar(18) NOT NULL,
                                            `ClientName` varchar(128) NOT NULL,
                                            `ConnectedAt` timestamp NULL DEFAULT current_timestamp(),
                                            `LastSeen` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
                                            `IpAddress` varchar(16) DEFAULT NULL,
                                            PRIMARY KEY (`Id`),
                                            UNIQUE KEY `SteamId` (`SteamId`)
                                        );").Result;


                Console.WriteLine("Altering table...");
                db.ExecuteNonQueryAsync(@"ALTER TABLE `Users` ADD COLUMN IF NOT EXISTS LastSeen timestamp DEFAULT current_timestamp() ON UPDATE current_timestamp()");

                Console.WriteLine("Checking for column update...");
                MySqlQueryResult? columnInfo = db.ExecuteQueryAsync(@"SELECT EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ConnectedAt';").Result;

                Console.WriteLine("Getting the first column's information");
                KeyValuePair<int, MySqlFieldValue>? pair = columnInfo.FirstOrDefault();


                Console.WriteLine("Checking if the column has the \"on update\" attribute");
                if (pair.Value.Value["EXTRA"].ToString().Contains("on update"))
                {
                    Console.WriteLine("Column has on update attribute.");
                    db.ExecuteNonQueryAsync(@"ALTER TABLE `Users` MODIFY COLUMN `ConnectedAt` timestamp NULL DEFAULT current_timestamp()");
                }

                if (result != 1)
                {
                    Console.WriteLine("Failed to create table.");
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
            }
        }

        public static List<User> GetConnectedPlayers(MySqlDb? db)
        {
            MySqlQueryResult result = db.Table("Users")
                .ExecuteQueryAsync("SELECT Id, SteamId, ClientName, ConnectedAt, LastSeen FROM `Users` ORDER BY `LastSeen` DESC LIMIT 50").Result;

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
                    ConnectedAt = DateTime.Parse(pair.Value["ConnectedAt"].ToString()),
                    LastSeen = DateTime.Parse(pair.Value["LastSeen"].ToString())
                };

                users.Add(user);
            }
            return users;
        }
    }
}