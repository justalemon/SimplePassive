using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System.Drawing;

namespace SimplePassive.Client
{
    /// <summary>
    /// Extensions for accessing some stuff quickly.
    /// </summary>
    public static class Extensions
    {
        #region Vehicle

        /// <summary>
        /// Gets the vehicle that is being hooked.
        /// </summary>
        /// <param name="vehicle">The cargobob, truck or towtruck.</param>
        /// <returns>The Vehicle that is being hooked, null if there is nothing.</returns>
        public static Vehicle GetHookedVehicle(this Vehicle vehicle)
        {
            // If the vehicle is invalid, return
            if (vehicle == null || !vehicle.Exists())
            {
                return null;
            }

            // Start by trying to get the vehicle attached as a trailer
            int trailer = 0;
            if (API.GetVehicleTrailerVehicle(vehicle.Handle, ref trailer))
            {
                return Entity.FromHandle(trailer) as Vehicle;
            }

            // Try to get a hooked cargobob vehicle and return it if there is somehing
            Vehicle cargobobHook = Entity.FromHandle(API.GetVehicleAttachedToCargobob(vehicle.Handle)) as Vehicle;
            if (cargobobHook != null && cargobobHook.Exists())
            {
                return cargobobHook;
            }

            // Then, try to get it as a tow truck and return it if it does
            Vehicle towHooked = Entity.FromHandle(API.GetEntityAttachedToTowTruck(vehicle.Handle)) as Vehicle;
            if (towHooked != null && towHooked.Exists())
            {
                return towHooked;
            }

            // If we got here, just send nothing
            return null;
        }

        #endregion

        #region Entities

        /// <summary>
        /// Draws a debug symbol on top of the entity.
        /// </summary>
        /// <param name="entity">The entity to use.</param>
        public static void DrawDebugMarker(this Entity entity, int r, int g, int b)
        {
            // If the entity does not exists, return
            if (entity == null || !entity.Exists())
            {
                return;
            }

            // If the entity is on the screen
            if (entity.IsOnScreen)
            {
                // Get the position 
                PointF pos = Screen.WorldToScreen(entity.Position);
                // And draw a text on the same position
                new Text(entity.Handle.ToString(), pos, 1)
                {
                    Color = Color.FromArgb(r, g, b),
                    Centered = true
                }.Draw();
            }
        }
        /// <summary>
        /// Disables the collisions between two entities during the next frame.
        /// </summary>
        /// <param name="one">The first entity.</param>
        /// <param name="two">The second entity.</param>
        public static void DisableCollisionsThisFrame(this Entity one, Entity two, bool print)
        {
            // If one of the entities is null, return
            if (one == null || two == null)
            {
                return;
            }

            // Otherwise, just disable the collisions
            API.SetEntityNoCollisionEntity(one.Handle, two.Handle, true);
            API.SetEntityNoCollisionEntity(two.Handle, one.Handle, true);

            // If we need to print the handles of the entities, do it
            if (print)
            {
                Debug.WriteLine($"Disabled collisions between {one.Handle} and {two.Handle}");
            }
        }

        #endregion
    }
}
