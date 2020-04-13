using CitizenFX.Core;
using CitizenFX.Core.Native;
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
            }
        }

        #endregion
    }
}
