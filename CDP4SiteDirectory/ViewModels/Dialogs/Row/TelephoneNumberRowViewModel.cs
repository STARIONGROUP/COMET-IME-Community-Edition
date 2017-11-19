// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

    public class TelephoneNumberRowViewModel : CDP4CommonView.TelephoneNumberRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumberRowViewModel"/> class
        /// </summary>
        /// <param name="telephoneNumber">The <see cref="TelephoneNumber"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public TelephoneNumberRowViewModel(TelephoneNumber telephoneNumber, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(telephoneNumber, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.VcardType = string.Join(", ", this.Thing.VcardType.Select(x => x.ToString()));
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
    }
}