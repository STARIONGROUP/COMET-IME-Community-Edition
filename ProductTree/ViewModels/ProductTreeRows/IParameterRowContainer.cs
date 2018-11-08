// ------------------------------------------------------------------------------------------------
// <copyright file="IParameterRowContainer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The interface for rows that contains rows representing a <see cref="ParameterGroup"/> or <see cref="ParameterBase"/>
    /// </summary>
    public interface IParameterRowContainer : IModelCodeRowViewModel
    {
        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        void UpdateParameterBasePosition(ParameterBase parameterBase);

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup"/></param>
        void UpdateParameterGroupPosition(ParameterGroup parameterGroup);
    }
}