// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// A row representing a <see cref="PossibleFiniteState"/>
    /// </summary>
    public class PossibleFiniteStateRowViewModel : CDP4CommonView.PossibleFiniteStateRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateRowViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteState">The <see cref="PossibleFiniteState"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PossibleFiniteStateRowViewModel(PossibleFiniteState possibleFiniteState, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(possibleFiniteState, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ActualFiniteState"/> is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.IsDefault = this.Thing.IsDefault;
        }
    }
}