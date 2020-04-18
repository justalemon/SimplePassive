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
        #region Fields

        /// <summary>
        /// The activation of passive mode for specific players.
        /// </summary>
        public readonly Dictionary<int, bool> activations = new Dictionary<int, bool>();
        /// <summary>
        /// The activations that override the dictionary above.
        /// </summary>
        public readonly Dictionary<int, bool> overrides = new Dictionary<int, bool>();

        #endregion

        #region Properties

        /// <summary>
        /// If the local player has passive enabled or disabled.
        /// </summary>
        public bool DefaultActivation => Convert.ToBoolean(API.GetConvarInt("simplepassive_default", 0));

        #endregion

        #region Tools

        /// <summary>
        /// Gets the activation of passive mode for a specific player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True, False or the default value.</returns>
        public bool GetPlayerActivation(int player)
        {
            // If the player has an override active, return that
            if (overrides.ContainsKey(player))
            {
                return overrides[player];
            }
            // If there is no override but there is a custom status, return that
            else if (activations.ContainsKey(player))
            {
                return activations[player];
            }
            // Otherwise, just return the default value
            return DefaultActivation;
        }

        #endregion

        #region Constructor

        public Passive()
        {
            Exports.Add("getPlayerActivation", new Func<int, bool>(GetPlayerActivation));
            Exports.Add("setPlayerActivation", new Func<int, bool, bool>(SetPlayerActivation));
            Exports.Add("isPlayerOverriden", new Func<int, bool>((id) => overrides.ContainsKey(id)));
            Exports.Add("setPlayerOverride", new Func<int, bool, bool>(SetPlayerOverride));
            Exports.Add("clearOverride", new Func<int, bool>(ClearOverride));
        }

        #endregion

        #region Export

        /// <summary>
        /// Sets the Passive Mode activation of a player.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="activation">The new activation status.</param>
        public bool SetPlayerActivation(int id, bool activation)
        {
            // Try to get the player
            Player player = Players[id];
            // If is not valid, return
            if (player == null)
            {
                return false;
            }

            // Otherwise, save the new activation and send it
            activations[id] = activation;
            TriggerClientEvent("simplepassive:activationChanged", id, activation);

            Debug.WriteLine($"Passive Activation of '{player.Name}' ({player.Handle}) is now {activation}");
            return true;
        }

        /// <summary>
        /// Overrides the activation of a player.
        /// </summary>
        /// <param name="id">The ID of the player.</param>
        /// <param name="activation">The desired activation.</param>
        /// <returns>True if we succeeded, False otherwise.</returns>
        public bool SetPlayerOverride(int id, bool activation)
        {
            // Try to get the player
            Player player = Players[id];
            // If is not valid, return
            if (player == null)
            {
                return false;
            }
            // Add the override and send it
            overrides[id] = activation;
            TriggerClientEvent("simplepassive:activationChanged", id, activation);

            // Finally, say that this succeeded
            Debug.WriteLine($"Passive Activation of {player.Handle} is now overridden ({activation})");
            return true;
        }

        /// <summary>
        /// Clears the override of a player.
        /// </summary>
        /// <param name="id">The target player.</param>
        /// <returns>True if the override was cleared, False if no override was present.</returns>
        public bool ClearOverride(int id)
        {
            // If is present, remove it and return
            if (overrides.ContainsKey(id))
            {
                overrides.Remove(id);
                Debug.WriteLine($"Passive Mode Override of {id} was removed");
                return true;
            }
            // Otherwise, say that is not present
            return false;
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Does some cleanup when a player leaves.
        /// </summary>
        [EventHandler("playerDropped")]
        public void PlayerDropped([FromSource]Player player, string reason)
        {
            // Convert the handle to an int
            int id = int.Parse(player.Handle);
            // Tell the client to the cleanup
            TriggerClientEvent("simplepassive:doCleanup", id);

            // If there is an activation or override for the player, remove it
            if (activations.ContainsKey(id))
            {
                activations.Remove(id);
            }

            if (overrides.ContainsKey(id))
            {
                overrides.Remove(id);
            }
        }

        /// <summary>
        /// Event triggered when a Client/Player has been initialized and is ready to work.
        /// </summary>
        [EventHandler("simplepassive:initialized")]
        public void Initialized([FromSource]Player player)
        {
            // Convert the ID to an int
            int id = int.Parse(player.Handle);

            // Start by saving the activation of the local player
            activations[id] = DefaultActivation;
            TriggerClientEvent("simplepassive:activationChanged", id, DefaultActivation);

            // Iterate over the players
            foreach (Player srvPlayer in Players)
            {
                // Convert the handle to an int
                int handle = int.Parse(srvPlayer.Handle);
                // Get the activation of the player and send it
                bool activation = GetPlayerActivation(handle);
                player.TriggerEvent("simplepassive:activationChanged", handle, activation);
            }
            Debug.WriteLine($"Player '{player.Name}' ({player.Handle}) received all passive activations");
        }

        #endregion

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
