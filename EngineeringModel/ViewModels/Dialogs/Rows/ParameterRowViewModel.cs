// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    public class ParameterRowViewModel : ParameterOrOverrideBaseRowViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRowViewModel"/> class.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container row.
        /// </param>
        /// <param name="isDialogReadOnly">
        /// Value indicating whether this row should be read-only because the dialog is read-only
        /// </param>
        public ParameterRowViewModel(Parameter parameter, ISession session, IDialogViewModelBase<Parameter> containerViewModel, bool isDialogReadOnly = false)
            : base(parameter, session, containerViewModel, isDialogReadOnly)
        {
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// The get parameter value set.
        /// </summary>
        /// <param name="parameter">The parameter to update</param>
        /// <remarks>
        /// This is used by the Parameter Dialog to update its ValueSet
        /// </remarks>
        public void UpdateParameterValueSet(Parameter parameter)
        {
            foreach (var valueSet in parameter.ValueSet)
            {
                this.UpdateValueSets(valueSet);
            }
        }
        #endregion
    }
}