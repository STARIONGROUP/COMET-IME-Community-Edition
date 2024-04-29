// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// The parameter row view model.
    /// </summary>
    public class ParameterOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideRowViewModel"/> class. 
        /// </summary>
        /// <param name="parameterOverride">
        /// The parameter Override.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container view-model.
        /// </param>
        /// <param name="isDialogReadOnly">
        /// Value indicating whether this row should be read-only because the dialog is read-only
        /// </param>
        public ParameterOverrideRowViewModel(ParameterOverride parameterOverride, ISession session, IDialogViewModelBase<ParameterOverride> containerViewModel, bool isDialogReadOnly = false)
            : base(parameterOverride, session, containerViewModel, isDialogReadOnly)
        {
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Update the value set of a <see cref="ParameterOverride"/>
        /// </summary>
        /// <param name="parameterOverride">The parameter Override</param>
        /// <remarks>
        /// This is used by the ParameterOverride Dialog to update its ValueSet
        /// </remarks>
        public void UpdateParameterOverrideValueSet(ParameterOverride parameterOverride)
        {
            foreach (var valueSet in parameterOverride.ValueSet)
            {
                this.UpdateValueSets(valueSet);
            }
        }
        #endregion
    }
}
