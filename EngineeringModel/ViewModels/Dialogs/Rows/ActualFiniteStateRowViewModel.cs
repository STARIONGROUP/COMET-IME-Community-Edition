// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// The extended row class used in the <see cref="ActualFiniteStateListDialogViewModel"/> class
    /// </summary>
    public class ActualFiniteStateRowViewModel : CDP4CommonView.ActualFiniteStateRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateRowViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteState">The <see cref="ActualFiniteState"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ActualFiniteStateRowViewModel(ActualFiniteState actualFiniteState, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(actualFiniteState, session, containerViewModel)
        {
            
            this.IsDefault = this.Thing.IsDefault;
        }
        
        /// <summary>
        /// Gets a value indicating whether the current <see cref="ActualFiniteState"/> is the default
        /// </summary>
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Computes the entire row or specific property of the row is editable based on the
        /// result of the <see cref="PermissionService.CanWrite"/> method and potential
        /// conditions of the property of the Row that is being edited.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property for which the value is computed. This allows to include the
        /// specific property of the row-view-model in the computation. If the propertyname is empty
        /// then the whole row is taken into account. If a property is specified only that property
        /// is taken into account.
        /// </param>
        /// <returns>
        /// True if the row or more specific the property is editable or not.
        /// </returns>
        public override bool IsEditable(string propertyName = "")
        {
            var dialog = this.ContainerViewModel as ActualFiniteStateListDialogViewModel;
            if (dialog != null && dialog.IsReadOnly)
            {
                return false;
            }

            return true;
        }
    }
}