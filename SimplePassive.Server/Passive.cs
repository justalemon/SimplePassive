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
        public readonly Dictionary<string, bool> activations = new Dictionary<string, bool>();

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
        public bool GetPlayerActivation(string player) => activations.ContainsKey(player) ? activations[player] : DefaultActivation;

        #endregion

        #region Network Events

        /// <summary>
        /// Event triggered when a player requests to change the passive activation of itself.
        /// </summary>
        /// <param name="player">The player that wants to change the activation.</param>
        /// <param name="activation">The new activation status that the player wants.</param>
        [EventHandler("simplepassive:changeActivation")]
        public void ChangeActivation([FromSource]Player player, bool activation)
        {
            // If the player is allowed to change the passive mode of itself
            if (API.IsPlayerAceAllowed(player.Handle, "simplepassive.changeself"))
            {
                // Save the activation
                activations[player.Handle] = activation;
                // And send it to all of the players
                TriggerClientEvent("simplepassive:activationChanged", player.Handle, activation);
                Debug.WriteLine($"Passive Activation of '{player.Name}' ({player.Handle}) is now {activation}");
            }
        }

        #endregion

        #region Commands

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

            // Get the activation of the player, but inverted
            bool oposite = !GetPlayerActivation(source.ToString());
            // Save it and send it to everyone
            activations[source.ToString()] = oposite;
            TriggerClientEvent("simplepassive:activationChanged", source.ToString(), oposite);
            Debug.WriteLine($"Player {source} set it's activation to {oposite}");
        }

        #endregion
    }
}
