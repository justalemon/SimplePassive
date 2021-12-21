using CitizenFX.Core.Native;
using System;

namespace SimplePassive
{
    /// <summary>
    /// A Class for quick access to Convars.
    /// </summary>
    public static class Convars
    {
        /// <summary>
        /// The default activation for new players.
        /// </summary>
        public static bool Default => Convert.ToBoolean(API.GetConvarInt("simplepassive_default", 0));
        /// <summary>
        /// If debugging information should be shown on the console and client screen.
        /// </summary>
        public static bool Debug => Convert.ToBoolean(API.GetConvarInt("simplepassive_debug", 0));
        /// <summary>
        /// If the ped should be made invincible when in passive.
        /// </summary>
        public static bool MakeInvincible => Convert.ToBoolean(API.GetConvarInt("simplepassive_makeinvincible", 0));

#if CLIENT
        /// <summary>
        /// The height of the entity debug markers.
        /// </summary>
        public static int DebugHeight => API.GetConvarInt("simplepassive_debugheight", 1);
        /// <summary>
        /// The Alpha/Transparency for entities that have passive mode enabled.
        /// </summary>
        public static int Alpha => API.GetConvarInt("simplepassive_alpha", 200);
        /// <summary>
        /// If the combat features should be disabled when passive is enabled.
        /// </summary>
        public static bool DisableCombat => Convert.ToBoolean(API.GetConvarInt("simplepassive_disablecombat", 0));
#endif
    }
}
