// -------------------------------------------------------------------------------------------------
// <copyright file="ScriptingLaguageKindSupported.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Helpers
{
    /// <summary>
    /// <see cref="ScriptingLaguageKindSupported"/> is an Enumeration for specifying the kind of scripting language
    /// </summary>  
    public enum ScriptingLaguageKindSupported
    {
        /// <summary>
        /// Indicates the Python scripting language
        /// </summary>
        Python,

        /// <summary>
        /// Indicates the Lua scripting language
        /// </summary>
        Lua,

        /// <summary>
        /// Indicates a text file
        /// </summary>
        Text
    }
}