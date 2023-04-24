using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;

namespace SimplePassive.Server
{
    /// <summary>
    /// Script that handle the Passive Mode between the clients.
    /// </summary>
    public class Passive : BaseScript
    {
        #region Commands

        /// <summary>
        /// Overrides the passive mode activation of a player.
        /// </summary>
        [Command("passiveoverride", Restricted = true)]
        public void OverrideCommand(int source, List<object> arguments, string raw)
        {
            // If no player or activation was specified, say it and return
            if (arguments.Count < 2)
            {
                Debug.WriteLine("You need to specify the Player ID and desired Activation!");
                return;
            }

            // Try to convert the first value to an int
            // If we failed, return
            if (!int.TryParse(arguments[0].ToString(), out int playerID))
            {
                Debug.WriteLine("The Player ID is not a number!");
                return;
            }

            // If the player is not valid, say it and return
            if (Players[playerID] == null)
            {
                Debug.WriteLine("The Player specified is not present.");
                return;
            }

            // Try to convert the second value to an int
            // If we failed, return
            if (!int.TryParse(arguments[1].ToString(), out int value))
            {
                Debug.WriteLine("The activation needs to be 0 or 1!");
                return;
            }

            // If we got here, convert the activation to a boolean and set it
            bool activation = Convert.ToBoolean(value);
            SetPlayerOverride(playerID, activation);
        }

        /// <summary>
        /// Clears the passive mode override for a player.
        /// </summary>
        [Command("passiveclear", Restricted = true)]
        public void Clear(int source, List<object> arguments, string raw)
        {
            // If there are no arguments, return
            if (arguments.Count == 0)
            {
                Debug.WriteLine("You need to specify the ID of a Player!");
                return;
            }

            // Try to convert the first value to an int
            // If we failed, return
            if (!int.TryParse(arguments[0].ToString(), out int id))
            {
                Debug.WriteLine("The Player ID is not a number!");
                return;
            }

            // If the player is not valid, say it and return
            if (Players[id] == null)
            {
                Debug.WriteLine("The Player specified is not valid.");
                return;
            }

            // Now, time to remove the Override
            if (!ClearOverride(id))
            {
                Debug.WriteLine($"The player {id} does not has a Passive Mode Override");
            }
        }

        /// <summary>
        /// Shows the current overrides in the server.
        /// </summary>
        [Command("passiveoverrides", Restricted = true)]
        public void OverridesCommand()
        {
            // If there are no overrides set, say it and return
            if (overrides.Count == 0)
            {
                Debug.WriteLine($"There are no Passive Mode Overrides in place.");
                return;
            }

            // Otherwise, list them one by one
            Debug.WriteLine($"Current Passive Mode Overrides:");
            foreach (var activation in overrides)
            {
                Debug.WriteLine($"\t{activation.Key} set to {activation.Value}");
            }
        }

        /// <summary>
        /// Command that toggles the passive mode activation of the player.
        /// </summary>
        [Command("passivetoggle")]
        public void ToggleCommand(int source, List<object> arguments, string raw)
        {
            // If the source is the Console or RCON, return
            if (source < 1)
            {
                Debug.WriteLine("This command can only be used by players on the server");
                return;
            }

            // If this player has an override active, say it and return
            if (overrides.ContainsKey(source))
            {
                Debug.WriteLine("Your Passive Mode Activation has been overriden, you can't change it");
                return;
            }

            // If the player is allowed to change the activation of itself
            if (API.IsPlayerAceAllowed(source.ToString(), "simplepassive.changeself"))
            {
                // Get the activation of the player, but inverted
                bool oposite = !GetPlayerActivation(source);
                // Save it and send it to everyone
                activations[source] = oposite;
                TriggerClientEvent("simplepassive:activationChanged", source, oposite);
                Debug.WriteLine($"Player {source} set it's activation to {oposite}");
            }
        }

        #endregion
    }
}
