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

        /// <summary>
        /// The activation of passive mode for specific players.
        /// </summary>
        public readonly Dictionary<int, bool> activations = new Dictionary<int, bool>();

        #endregion

        #region Constructor

        public Passive()
        {
            // Add the exports
            Exports.Add("getActivation", new Func<bool>(() => GetPlayerActivation(Game.Player.ServerId)));
            Exports.Add("setActivation", new Action<bool>(SetPassiveActivation));
            // And tell the server that this client is ready to work
            TriggerServerEvent("simplepassive:initialized");
        }

        #endregion

        #region Tools

        /// <summary>
        /// Gets the activation of passive mode for a specific player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True, False or the default value.</returns>
        public bool GetPlayerActivation(int player) => activations.ContainsKey(player) ? activations[player] : Convars.Default;

        #endregion

        #region Exports

        public void SetPassiveActivation(bool activation)
        {
            // Tell the server to change the activation of the current player
            if (Convars.Debug)
            {
                Debug.WriteLine($"Requesting server to change the activation to {activation}");
            }
            TriggerServerEvent("simplepassive:setPassive", activation);
        }

        #endregion

        #region Ticks

        /// <summary>
        /// Tick event that handles the collisions of Passive Mode.
        /// </summary>
        /// <returns></returns>
        [Tick]
        public async Task HandleCollisions()
        {
            // Create some references to the local player ped and vehicle
            Player localPlayer = Game.Player;
            Ped localPed = localPlayer.Character;
            Vehicle localVehicle = localPed.CurrentVehicle;
            Vehicle localHooked = localVehicle?.GetHookedVehicle();

            // Set the alpha of the player vehicle to maximum if is present
            if (localVehicle != null)
            {
                API.ResetEntityAlpha(localVehicle.Handle);
            }

            // Create a text for the debug mode
            string debugText = "Passive Players: ";

            // Get the activation of the local player for later use
            bool localActivation = GetPlayerActivation(localPlayer.ServerId);

            // If the local player has passive mode enabled
            if (localActivation)
            {
                // If the player is not allowed to fight other players
                if (Convars.DisableCombat)
                {
                    // There are some values that we set on the activationChanged event
                    // If is not on this chunk, is probably on that event

                    // Disable the firing of weapons
                    API.DisablePlayerFiring(localPlayer.Handle, true);
                    // And disable the controls related to attacking
                    Game.DisableControlThisFrame(0, Control.MeleeAttack1);
                    Game.DisableControlThisFrame(0, Control.MeleeAttack2);
                    Game.DisableControlThisFrame(0, Control.Attack);
                    Game.DisableControlThisFrame(0, Control.Attack2);
                    Game.DisableControlThisFrame(0, Control.VehicleAttack);
                    Game.DisableControlThisFrame(0, Control.VehicleAttack2);
                    Game.DisableControlThisFrame(0, Control.VehiclePassengerAttack);
                    Game.DisableControlThisFrame(0, Control.VehicleFlyAttack);
                    Game.DisableControlThisFrame(0, Control.VehicleFlyAttack2);
                }
            }

            // Then, iterate over the list of players
            foreach (Player player in Players)
            {
                // Get the correct activation for this player
                bool playerActivation = GetPlayerActivation(player.ServerId);
                bool disableCollisions = playerActivation || localActivation;

                // Add the activation onto the debug text
                debugText += $" {player.ServerId} ({(playerActivation ? 1 : 0)})";

                // Save the ped and vehicle of the other player
                Ped otherPed = player.Character;
                Vehicle otherVehicle = otherPed.CurrentVehicle;
                Vehicle otherHooked = otherVehicle?.GetHookedVehicle();

                // If the player is the same as the local one, skip this iteration
                if (player == localPlayer)
                {
                    continue;
                }

                // Select the correct entities for both players
                Entity other = (Entity)player.Character.CurrentVehicle ?? player.Character;

                // Set the correct alpha for the other entities (just in case the resource restarted with passive enabled)
                API.SetEntityAlpha(other.Handle, disableCollisions && !API.GetIsTaskActive(player.Character.Handle, 2) && Game.Player.Character.CurrentVehicle != other ? Convars.Alpha : 255, 0);

                // If passive mode is activated by the other or local player
                if (disableCollisions)
                {
                    // If the other player is using a vehicle, we are seated on it and we are not the driver, continue
                    if (otherVehicle != null &&
                        API.IsPedInVehicle(otherVehicle.Handle, localPed.Handle, false) &&
                        otherVehicle.GetPedOnSeat(VehicleSeat.Driver) != localPed)
                    {
                        continue;
                    }

                    // Otherwise, disable the collisions

                    // Local Player vs Other Player
                    localPed.DisableCollisionsThisFrame(otherPed);
                    // Local Player vs Other Vehicle (if present)
                    localPed.DisableCollisionsThisFrame(otherVehicle);
                    // Local Player vs Other Hooked (if present)
                    localPed.DisableCollisionsThisFrame(otherHooked);

                    // Local Vehicle vs Other Player
                    localVehicle?.DisableCollisionsThisFrame(otherPed);
                    // Local Vehicle vs Other Vehicle (if present)
                    localVehicle?.DisableCollisionsThisFrame(otherVehicle);
                    // Local Vehicle vs Other Hooked (if present)
                    localVehicle?.DisableCollisionsThisFrame(otherHooked);

                    // Local Hooked vs Other Player
                    localHooked?.DisableCollisionsThisFrame(otherPed);
                    // Local Hooked vs Other Vehicle (if present)
                    localHooked?.DisableCollisionsThisFrame(otherVehicle);
                    // Local Hooked vs Other Hooked (if present)
                    localHooked?.DisableCollisionsThisFrame(otherHooked);


                    // Other Player vs Local Player
                    otherPed.DisableCollisionsThisFrame(localPed);
                    // Other Player vs Local Vehicle (if present)
                    otherPed.DisableCollisionsThisFrame(localVehicle);
                    // Other Player vs Local Hooked (if present)
                    otherPed.DisableCollisionsThisFrame(localHooked);

                    // Other Vehicle vs Local Player
                    otherVehicle?.DisableCollisionsThisFrame(localPed);
                    // Other Vehicle vs Local Vehicle (if present)
                    otherVehicle?.DisableCollisionsThisFrame(localVehicle);
                    // Other Vehicle vs Local Hooked (if present)
                    otherVehicle?.DisableCollisionsThisFrame(localHooked);

                    // Other Hooked vs Local Player
                    otherHooked?.DisableCollisionsThisFrame(localPed);
                    // Other Hooked vs Local Vehicle (if present)
                    otherHooked?.DisableCollisionsThisFrame(localVehicle);
                    // Other Hooked vs Local Hooked (if present)
                    otherHooked?.DisableCollisionsThisFrame(localHooked);
                }
            }

            // Add the local activation onto the debug text
            debugText += $"\nLocal Status: {localActivation}";
            // And draw it if the debug mode is enabled
            if (Convars.Debug)
            {
                new Text(debugText, new PointF(0, 0), 0.5f).Draw();
            }
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

#if DEBUG

        /// <summary>
        /// Lists the activations that the Client knows.
        /// </summary>
        [Command("listactivations")]
        public void ListActivationsCommand()
        {
            // Just iterate and print every single one of them
            Debug.WriteLine("Known Activations:");
            foreach (KeyValuePair<int, bool> activation in activations)
            {
                Debug.WriteLine($"{activation.Key} set to {activation.Value}");
            }
        }

#endif

        #endregion
    }
}
