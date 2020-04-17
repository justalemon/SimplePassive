using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;

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
        public readonly Dictionary<string, bool> activations = new Dictionary<string, bool>();
        /// <summary>
        /// The activations that override the dictionary above.
        /// </summary>
        public readonly Dictionary<string, bool> overrides = new Dictionary<string, bool>();

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
        public bool GetPlayerActivation(string player)
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
            Exports.Add("setPlayerActivation", new Func<int, bool, bool>(SetPlayerActivation));
            Exports.Add("setPlayerOverride", new Func<int, bool, bool>(SetPlayerOverride));
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
            activations[player.Handle] = activation;
            TriggerClientEvent("simplepassive:activationChanged", player, activation);

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
            overrides[player.Handle] = activation;
            TriggerClientEvent("simplepassive:activationChanged", player.Handle, activation);

            // Finally, say that this succeeded
            Debug.WriteLine($"Passive Activation of {player.Handle} is now overridden ({activation})");
            return true;
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Event triggered by the Clients when they are ready to handle passive mode.
        /// </summary>
        [EventHandler("simplepassive:activationsRequested")]
        public void ActivationsRequested([FromSource]Player player)
        {
            // Iterate over the players
            foreach (Player srvPlayer in Players)
            {
                // Get the activation of the player and send it
                bool activation = GetPlayerActivation(srvPlayer.Handle);
                player.TriggerEvent("simplepassive:activationChanged", srvPlayer.Handle, activation);
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
            if (!int.TryParse(arguments[1].ToString(), out int playerID))
            {
                Debug.WriteLine("The Player ID is not a number!");
                return;
            }

            // Try to get the player
            Player player = Players[playerID];
            // If is not valid, say it and return
            if (player == null)
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
        [Command("togglepassive")]
        public void TogglePassiveCommand(int source, List<object> arguments, string raw)
        {
            // If the source is the Console or RCON, return
            if (source < 1)
            {
                Debug.WriteLine("This command can only be used by players on the server");
                return;
            }

            // Convert the source to a string
            string src = source.ToString();

            // If this player has an override active, say it and return
            if (overrides.ContainsKey(src))
            {
                Debug.WriteLine("Your Passive Mode Activation has been overriden, you can't change it");
                return;
            }

            // If the player is allowed to change the activation of itself
            if (API.IsPlayerAceAllowed(src, "simplepassive.changeself"))
            {
                // Get the activation of the player, but inverted
                bool oposite = !GetPlayerActivation(src);
                // Save it and send it to everyone
                activations[src] = oposite;
                TriggerClientEvent("simplepassive:activationChanged", src, oposite);
                Debug.WriteLine($"Player {source} set it's activation to {oposite}");
            }
        }

        #endregion
    }
}
