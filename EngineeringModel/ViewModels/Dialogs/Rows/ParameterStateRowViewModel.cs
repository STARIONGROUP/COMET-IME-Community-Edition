// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterStateRowViewModel.cs" company="RHEA System S.A.">
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
    /// The row representing an <see cref="ActualFiniteState"/>
    /// </summary>
    public class ParameterStateRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterStateRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated <see cref="ParameterBase"/></param>
        /// <param name="option">The associated <see cref="Option"/></param>
        /// <param name="actualState">The associated <see cref="ActualFiniteState"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container row</param>
        /// <param name="isDialogReadOnly">Value indicating whether this row should be read-only because the dialog is read-only</param>
        public ParameterStateRowViewModel(ParameterBase parameterBase, Option option, ActualFiniteState actualState, ISession session, IRowViewModelBase<Thing> containerViewModel,  bool isDialogReadOnly = false)
            : base(parameterBase, session, option, actualState, containerViewModel, 0, isDialogReadOnly)
        {
            this.Name = this.ActualState.Name;
            this.State = this.ActualState.Name;
            this.Option = this.ActualOption;
        }
    }
}