// ------------------------------------------------------------------------------------------------
// <copyright file="IModelCodeRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The interface for rows that can display a model-code
    /// </summary>
    public interface IModelCodeRowViewModel
    {
        /// <summary>
        /// Gets the model-code
        /// </summary>
        string ModelCode { get; }
    }
}