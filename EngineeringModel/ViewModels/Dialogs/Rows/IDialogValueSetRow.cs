// -------------------------------------------------------------------------------------------------
// <copyright file="IDialogValueSetRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.ViewModels;

    /// <summary>
    /// The interface for rows that displays <see cref="ParameterBase"/> values in a dialog
    /// </summary>
    public interface IDialogValueSetRow : IValueSetRow
    {
        /// <summary>
        /// Check that the values of this row are valid
        /// </summary>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        void CheckValues(MeasurementScale scale);
    }
}