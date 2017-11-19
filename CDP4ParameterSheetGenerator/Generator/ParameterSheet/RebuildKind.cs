// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RebuildKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Generator
{
    /// <summary>
    /// Enumeration datatype that defines the possible kinds of rebuild procedures
    /// </summary>
    public enum RebuildKind
    {
        /// <summary>
        /// Rebuild the sheet keeping the changes that were introduced by the user
        /// </summary>
        KeepChanges,

        /// <summary>
        /// Rebuild the sheet not keeping any changes that were introduced by the user
        /// </summary>
        Clean
    }
}
