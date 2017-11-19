// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="TelephoneNumberDialogViewModel"/> is to allow a <see cref="TelephoneNumber"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="TelephoneNumber"/> will result in an <see cref="TelephoneNumber"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.TelephoneNumber)]
    public class TelephoneNumberDialogViewModel : CDP4CommonView.TelephoneNumberDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumberDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public TelephoneNumberDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumberDialogViewModel"/> class.
        /// </summary>
        /// <param name="telephoneNumber">
        /// The <see cref="TelephoneNumber"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="container">The container <see cref="Thing"/> for the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public TelephoneNumberDialogViewModel(TelephoneNumber telephoneNumber, ThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(telephoneNumber, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        #endregion

        /// <summary>
        /// Gets or sets a value indicating if this is the default telephone number
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }

        /// <summary>
        /// Update the properties of this dialog
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            var person = (Person)this.Container;
            this.IsDefault = (person.DefaultTelephoneNumber != null) && person.DefaultTelephoneNumber.Iid == this.Thing.Iid;
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var person = (Person)this.Container;

            if (this.IsDefault)
            {
                person.DefaultTelephoneNumber = this.Thing;
            }
            else if (person.DefaultTelephoneNumber != null && person.DefaultTelephoneNumber.Iid == this.Thing.Iid && !this.IsDefault)
            {
                person.DefaultTelephoneNumber = null;
            }
        }
    }
}
