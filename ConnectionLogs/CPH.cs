using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionLogs
{
    internal static class IsValid
    {
        public static bool Client(int playerIndex)
        {
            // If the player's index is -1 then execute this section
            if (playerIndex == -1)
            {
                return false;
            }

            // If the client's index value is not within the range it should be then execute this section
            if (!(1 <= playerIndex && playerIndex <= Server.MaxPlayers))
            {
                return false;
            }

            // Uses the player's index to find the player's CCSPlayerController entity and store it within playerController variable
            CCSPlayerController playerController = Utilities.GetPlayerFromIndex(playerIndex);

            // If the playerController is invalid then execute this section
            if (playerController == null)
            {
                return false;
            }

            // If the pawn associated with the playerController is invalid then execute this section
            if (playerController.PlayerPawn == null)
            {
                return false;
            }

            return true;
        }
    }
}
