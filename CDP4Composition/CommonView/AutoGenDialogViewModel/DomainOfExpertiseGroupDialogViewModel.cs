// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseGroupDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    /// dialog-view-model class representing a <see cref="DomainOfExpertiseGroup"/>
    /// </summary>
    public partial class DomainOfExpertiseGroupDialogViewModel : DefinedThingDialogViewModel<DomainOfExpertiseGroup>
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;


        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseGroupDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DomainOfExpertiseGroupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseGroupDialogViewModel"/> class
        /// </summary>
        /// <param name="domainOfExpertiseGroup">
        /// The <see cref="DomainOfExpertiseGroup"/> that is the subject of the current view-model. This is the object
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
        public DomainOfExpertiseGroupDialogViewModel(DomainOfExpertiseGroup domainOfExpertiseGroup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(domainOfExpertiseGroup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as SiteDirectory;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type SiteDirectory",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }
        
        /// <summary>
        /// Backing field for <see cref="Domain"/>s
        /// </summary>
        private ReactiveList<DomainOfExpertise> domain;

        /// <summary>
        /// Gets or sets the list of selected <see cref="DomainOfExpertise"/>s
        /// </summary>
        public ReactiveList<DomainOfExpertise> Domain 
        { 
            get { return this.domain; } 
            set { this.RaiseAndSetIfChanged(ref this.domain, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="DomainOfExpertise"/> for <see cref="Domain"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleDomain { get; protected set; }

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

            clone.IsDeprecated = this.IsDeprecated;
            clone.Domain.Clear();
            clone.Domain.AddRange(this.Domain);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Domain = new ReactiveList<DomainOfExpertise>();
            this.PossibleDomain = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.PopulateDomain();
        }

        /// <summary>
        /// Populates the <see cref="Domain"/> property
        /// </summary>
        protected virtual void PopulateDomain()
        {
            this.Domain.Clear();

            foreach (var value in this.Thing.Domain)
            {
                this.Domain.Add(value);
            }
        } 
    }
}
