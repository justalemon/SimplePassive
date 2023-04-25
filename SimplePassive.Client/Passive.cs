using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace SimplePassive.Client
{
    /// <summary>
    /// Client script that does the real hard work.
    /// </summary>
    public class Passive : BaseScript
    {
        #region Fields

        private Vehicle lastPlayerVehicle = null;
        private Vehicle lastHookedVehicle = null;

        #endregion

        #region Ticks

        /// <summary>
        /// Tick event that handles the collisions of Passive Mode.
        /// </summary>
        /// <returns></returns>
        [Tick]
        public async Task HandleCollisions()
        {
            // Create a text for the debug mode
            string debugText = "Passive Players: ";

            // Make sure that the player is invincible if needed
            if (Convars.MakeInvincible)
            {
                if (localVehicle != null)
                {
                    localVehicle.IsInvincible = localActivation;
                }
                if (localHooked != null)
                {
                    localHooked.IsInvincible = localActivation;
                }

                if (localVehicle != lastPlayerVehicle)
                {
                    if (lastPlayerVehicle != null && lastPlayerVehicle.Exists() && (lastPlayerVehicle.Driver == null || lastPlayerVehicle.Driver != localPed))
                    {
                        lastPlayerVehicle.IsInvincible = false;
                    }
                    lastPlayerVehicle = localVehicle;
                }
                if (localHooked != lastHookedVehicle)
                {
                    if (lastHookedVehicle != null && lastHookedVehicle.Exists() && (lastHookedVehicle.Driver == null || lastHookedVehicle.Driver != localPed))
                    {
                        lastHookedVehicle.IsInvincible = false;
                    }
                    lastHookedVehicle = localHooked;
                }
            }

            // Then, iterate over the list of players
            foreach (Player player in Players)
            {
                // Add the activation onto the debug text
                debugText += $" {player.ServerId} ({(playerActivation ? 1 : 0)})";
            }

            // Add the local activation onto the debug text
            debugText += $"\nLocal Status: {localActivation}";
            // And draw it if the debug mode is enabled
            if (Convars.Debug)
            {
                new Text(debugText, new PointF(0, 0), 0.5f).Draw();
            }

            // Finally, disable the printing during the next tick (if enabled)
            printNextTick = false;
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Saves the activation of passive mode for another player.
        /// </summary>
        /// <param name="handle">The Server Handle/ID of the player.</param>
        /// <param name="activation">The activation of that player.</param>
        [EventHandler("simplepassive:activationChanged")]
        public void ActivationChanged(int handle, bool activation)
        {
            // Just save the activation of the player
            activations[handle] = activation;
            if (Convars.Debug)
            {
                Debug.WriteLine($"Received Passive Activation of {handle} ({activation})");
            }

            // If the passive activation is for the current player
            if (handle == Game.Player.ServerId)
            {
                // Set the correct activation for drive by-s
                API.SetPlayerCanDoDriveBy(Game.Player.Handle, (!activation && Convars.DisableCombat) || !Convars.DisableCombat);
            }
        }

        /// <summary>
        /// Does some cleanup for a specific player.
        /// </summary>
        /// <param name="id">The ID of the player.</param>
        [EventHandler("simplepassive:doCleanup")]
        public void DoCleanup(int id)
        {
            // If there is an activation for the player, remove it
            if (activations.ContainsKey(id))
            {
                activations.Remove(id);
            }
        }

        #endregion

        #region Debug Commands

        /// <summary>
        /// Prints the collisions changed during the next tick.
        /// </summary>
        [Command("passiveprinttick")]
        public void ShowNextTickCommand()
        {
            // If debug mode is enabled, set printNextTick to true
            if (Convars.Debug)
            {
                printNextTick = true;
            }
        }

        #endregion
    }
}
