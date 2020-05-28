using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace SimplePassive.Client
{
    /// <summary>
    /// Extensions for accessing some stuff quickly.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the vehicle that is being hooked.
        /// </summary>
        /// <param name="vehicle">The cargobob, truck or towtruck.</param>
        /// <returns>The Vehicle that is being hooked, null if there is nothing.</returns>
        public static Vehicle GetHookedVehicle(this Vehicle vehicle)
        {
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
    }
}
