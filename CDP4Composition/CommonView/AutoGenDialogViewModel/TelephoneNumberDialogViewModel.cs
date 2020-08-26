// -------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="TelephoneNumber"/>
    /// </summary>
    public partial class TelephoneNumberDialogViewModel : DialogViewModelBase<TelephoneNumber>
    {
        /// <summary>
        /// Backing field for <see cref="TelephoneNumber"/>
        /// </summary>
        private string telephoneNumber;


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
        /// Initializes a new instance of the <see cref="TelephoneNumberDialogViewModel"/> class
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
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public TelephoneNumberDialogViewModel(TelephoneNumber telephoneNumber, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(telephoneNumber, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Person;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Person",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the TelephoneNumber
        /// </summary>
        public virtual string TelephoneNumber
        {
            get { return this.telephoneNumber; }
            set { this.RaiseAndSetIfChanged(ref this.telephoneNumber, value); }
        }

        /// <summary>
        /// Backing field for VcardType
        /// </summary>
        public ReactiveList<VcardTelephoneNumberKind> vcardType;

        /// <summary>
        /// Gets or sets the VcardType
        /// </summary>
        public ReactiveList<VcardTelephoneNumberKind> VcardType
        {
            get { return this.vcardType; }
            set { this.RaiseAndSetIfChanged(ref this.vcardType, value); }
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Value = this.TelephoneNumber;
            clone.VcardType.Clear();
            clone.VcardType.AddRange(this.VcardType);
 
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.VcardType = new ReactiveList<VcardTelephoneNumberKind>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.TelephoneNumber = this.Thing.Value;
            this.PopulateVcardType();
        }

        /// <summary>
        /// Populates the <see cref="VcardType"/> property
        /// </summary>
        protected virtual void PopulateVcardType()
        {
            this.VcardType.Clear();
            foreach(var value in this.Thing.VcardType)
            {
                this.VcardType.Add(value);
            }
        }
    }
}
