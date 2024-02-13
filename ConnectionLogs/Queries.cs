using CounterStrikeSharp.API.Core;
using Nexd.MySQL;

namespace ConnectionLogs
{
    internal class Queries
    {
        public static void InsertNewClient(MySqlDb? db, CCSPlayerController player)
        {
            MySqlQueryValue values = new MySqlQueryValue()
                                    .Add("ClientName", player.PlayerName)
                                    .Add("SteamId", player.SteamID.ToString())
                                    .Add("IpAddress", player.IpAddress.Split(':')[0]);

            db!.Table("Users").InsertIfNotExistAsync(values, $"`ClientName` = '{player.PlayerName}', `LastSeen` = CURRENT_TIMESTAMP()");
        }

        public static void CreateTable(MySqlDb? db)
        {
            Task.Run(async () =>
            {
                try
                {
                    int result = await db.ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS `Users` (
                                            `Id` int(11) NOT NULL AUTO_INCREMENT,
                                            `SteamId` BIGINT NOT NULL,
                                            `ClientName` varchar(128) NOT NULL,
                                            `ConnectedAt` timestamp NULL DEFAULT current_timestamp(),
                                            `LastSeen` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
                                            `IpAddress` varchar(16) DEFAULT NULL,
                                            PRIMARY KEY (`Id`),
                                            UNIQUE KEY `SteamId` (`SteamId`)
                                        );");

                    if (result == 1)
                    {
                        Console.WriteLine("Created table `Users`");
                        return;
                    }

                    await db.ExecuteNonQueryAsync(@"ALTER TABLE `Users` ADD COLUMN IF NOT EXISTS LastSeen timestamp DEFAULT current_timestamp() ON UPDATE current_timestamp()");

                    MySqlQueryResult? columnInfo = await db.ExecuteQueryAsync(@"SELECT EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ConnectedAt';");

                    KeyValuePair<int, MySqlFieldValue>? pair = columnInfo.FirstOrDefault();


                    if (pair.Value.Value["EXTRA"].ToString().Contains("on update"))
                    {
                        await db.ExecuteNonQueryAsync(@"ALTER TABLE `Users` MODIFY COLUMN `ConnectedAt` timestamp NULL DEFAULT current_timestamp()");
                        Console.WriteLine("Updated column `ConnectedAt´ in `Users` table.");
                    }


                    MySqlQueryResult? steamidDatatype = await db.ExecuteQueryAsync(@"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'SteamId';");

                    KeyValuePair<int, MySqlFieldValue>? steamidPair = steamidDatatype.FirstOrDefault();

                    if (steamidPair.Value.Value["DATA_TYPE"].ToString() != "bigint")
                    {
                        await db.ExecuteNonQueryAsync(@"ALTER TABLE `Users` MODIFY COLUMN `SteamId` BIGINT NOT NULL");
                        Console.WriteLine("Updated column `SteamId´ in `Users` table.");
                    }
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }
            });
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
                    SteamId = ulong.Parse(pair.Value["SteamId"]),
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