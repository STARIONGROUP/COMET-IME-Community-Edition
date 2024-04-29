// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOptionRowViewModel.cs" company="Starion Group S.A.">
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
    /// The row representing an <see cref="Option"/> of a <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterOptionRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOptionRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated <see cref="ParameterBase"/></param>
        /// <param name="option">The associated <see cref="Option"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container row</param>
        /// <param name="isDialogReadOnly">Value indicating whether this row should be read-only because the dialog is read-only</param>
        public ParameterOptionRowViewModel(ParameterBase parameterBase, Option option, ISession session, IRowViewModelBase<Thing> containerViewModel,  bool isDialogReadOnly = false)
            : base(parameterBase, session, option, null, containerViewModel, 0, isDialogReadOnly)
        {
            this.Name = this.ActualOption.Name;
            this.Option = this.ActualOption;
        }
    }
}