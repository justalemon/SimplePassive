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
