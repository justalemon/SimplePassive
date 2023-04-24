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
        /// Command that toggles the passive mode activation of the player.
        /// </summary>
        [Command("passivetoggle")]
        public void ToggleCommand(int source, List<object> arguments, string raw)
        {
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
