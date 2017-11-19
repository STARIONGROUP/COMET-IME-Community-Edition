// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterUsageKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The different kinds of a parameter usage 
    /// </summary>
    public enum ParameterUsageKind
    {
        /// <summary>
        /// The <see cref="ParameterOrOverrideBase"/> is neither used or owned by the active <see cref="DomainOfExpertise"/>
        /// </summary>
        Unused = 0,

        /// <summary>
        /// The <see cref="ParameterOrOverrideBase"/> has subscription from other <see cref="DomainOfExpertise"/> than the active one
        /// </summary>
        SubscribedByOthers = 1,

        /// <summary>
        /// The active <see cref="DomainOfExpertise"/> has subscribed to the <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        Subscribed = 2
    }
}