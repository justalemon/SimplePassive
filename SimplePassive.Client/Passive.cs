using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;

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
        /// <summary>
        /// If the local player has passive enabled or disabled.
        /// </summary>
        public bool localActivation = Convert.ToBoolean(API.GetConvarInt("simplepassive_default", 0));

        #endregion
    }
}
