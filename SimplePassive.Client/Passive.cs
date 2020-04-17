using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
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
        public readonly Dictionary<string, bool> activations = new Dictionary<string, bool>();

        #endregion

        #region Constructor

        public Passive()
        {
            // Tell the server that this client is ready to work
            TriggerServerEvent("simplepassive:activationsRequested");
        }

        #endregion

        #region Tools

        /// <summary>
        /// Gets the activation of passive mode for a specific player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True, False or the default value.</returns>
        public bool GetPlayerActivation(string player) => activations.ContainsKey(player) ? activations[player] : Convert.ToBoolean(API.GetConvarInt("simplepassive_default", 0));

        #endregion

        #region Ticks

        /// <summary>
        /// Tick event that handles the collisions of Passive Mode.
        /// </summary>
        /// <returns></returns>
        [Tick]
        public async Task HandleCollisions()
        {
            // Set the alpha of the player vehicle to maximum if is present
            if (Game.Player.Character.CurrentVehicle != null)
            {
                API.SetEntityAlpha(Game.Player.Character.CurrentVehicle.Handle, 255, 0);
            }

            // Get the activation of the local player and id of it
            string localID = Game.Player.ServerId.ToString();
            bool localActivation = GetPlayerActivation(localID);

            // Iterate over the list of players
            foreach (Player player in Players)
            {
                // Convert the ID of the player to a string
                string remoteID = player.ServerId.ToString();

                // Get the correct activation for this player
                bool playerActivation = GetPlayerActivation(remoteID);
                bool disableCollisions = playerActivation || localActivation;

                // Select the correct entity for the local and other player
                Entity local = (Entity)Game.Player.Character.CurrentVehicle ?? Game.Player.Character;
                Entity other = (Entity)player.Character.CurrentVehicle ?? player.Character;

                // If this player is not the same as the local one
                if (player != Game.Player)
                {
                    // Set the correct alpha for the other entity (just in case the resource restarted with passive enabled)
                    API.SetEntityAlpha(other.Handle, disableCollisions && !API.GetIsTaskActive(player.Character.Handle, 2) && Game.Player.Character.CurrentVehicle != other ? 200 : 255, 0);

                    // If passive mode is activated by the other or local player
                    if (disableCollisions)
                    {
                        // If the other player is using a vehicle, we are seated on it and we are not the driver, continue
                        if (player.Character.CurrentVehicle != null &&
                            API.IsPedInVehicle(player.Character.CurrentVehicle.Handle, Game.Player.Character.Handle, false) &&
                            player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) != Game.Player.Character)
                        {
                            continue;
                        }

                        // Otherwise, set the collisions for the entities
                        API.SetEntityNoCollisionEntity(local.Handle, other.Handle, true);
                        API.SetEntityNoCollisionEntity(other.Handle, local.Handle, true);
                    }
                }
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
        public void ActivationChanged(string handle, bool activation)
        {
            // Just save the activation of the player
            activations[handle] = activation;
            Debug.WriteLine($"Received Passive Activation of {handle} ({activation})");
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
            foreach (KeyValuePair<string, bool> activation in activations)
            {
                Debug.WriteLine($"{activation.Key} set to {activation.Value}");
            }
        }

#endif

        #endregion
    }
}
