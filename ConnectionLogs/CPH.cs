using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionLogs
{
    internal static class IsClient
    {
        // Returns true if the client is a bot
        /// <summary>
        /// Determines if the given player is a bot or not.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True if the player is a bot, false if the player is a human.</returns>
        public static bool Bot(CCSPlayerController player)
        {
            // If the player is a bot then execute this section
            if ((player.Flags & (1 << 8)) != 0)
            {
                return true;
            }

            // If the player is not a bot but a human then return false
            return false;
        }

        /// <summary>
        /// Determines whether a player is human or not.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True if the player is human, false otherwise.</returns>
        public static bool Human(CCSPlayerController player)
        {
            // If the player is not a bot then execute this section
            if ((player.Flags & (1 << 8)) != 0)
            {
                return false;
            }

            // If the player is a bot and thereby not a human then return false
            return true;
        }
    }
}
