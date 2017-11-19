// -------------------------------------------------------------------------------------------------
// <copyright file="CitationDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Citation"/>
    /// </summary>
    public partial class CitationDialogViewModel : DialogViewModelBase<Citation>
    {
        /// <summary>
        /// Backing field for <see cref="Location"/>
        /// </summary>
        private string location;

        /// <summary>
        /// Backing field for <see cref="IsAdaptation"/>
        /// </summary>
        private bool isAdaptation;

        /// <summary>
        /// Backing field for <see cref="Remark"/>
        /// </summary>
        private string remark;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="SelectedSource"/>
        /// </summary>
        private ReferenceSource selectedSource;


        /// <summary>
        /// Initializes a new instance of the <see cref="CitationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public CitationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CitationDialogViewModel"/> class
        /// </summary>
        /// <param name="citation">
        /// The <see cref="Citation"/> that is the subject of the current view-model. This is the object
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
        public CitationDialogViewModel(Citation citation, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(citation, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Definition;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Definition",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Location
        /// </summary>
        public virtual string Location
        {
            get { return this.location; }
            set { this.RaiseAndSetIfChanged(ref this.location, value); }
        }

        /// <summary>
        /// Gets or sets the IsAdaptation
        /// </summary>
        public virtual bool IsAdaptation
        {
            get { return this.isAdaptation; }
            set { this.RaiseAndSetIfChanged(ref this.isAdaptation, value); }
        }

        /// <summary>
        /// Gets or sets the Remark
        /// </summary>
        public virtual string Remark
        {
            get { return this.remark; }
            set { this.RaiseAndSetIfChanged(ref this.remark, value); }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public virtual string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedSource
        /// </summary>
        public virtual ReferenceSource SelectedSource
        {
            get { return this.selectedSource; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSource, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ReferenceSource"/>s for <see cref="SelectedSource"/>
        /// </summary>
        public ReactiveList<ReferenceSource> PossibleSource { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSource"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedSourceCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedSourceCommand = this.WhenAny(vm => vm.SelectedSource, v => v.Value != null);
            this.InspectSelectedSourceCommand = ReactiveCommand.Create(canExecuteInspectSelectedSourceCommand);
            this.InspectSelectedSourceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSource));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Location = this.Location;
            clone.IsAdaptation = this.IsAdaptation;
            clone.Remark = this.Remark;
            clone.ShortName = this.ShortName;
            clone.Source = this.SelectedSource;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleSource = new ReactiveList<ReferenceSource>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Location = this.Thing.Location;
            this.IsAdaptation = this.Thing.IsAdaptation;
            this.Remark = this.Thing.Remark;
            this.ShortName = this.Thing.ShortName;
            this.SelectedSource = this.Thing.Source;
            this.PopulatePossibleSource();
        }

        /// <summary>
        /// Populates the <see cref="PossibleSource"/> property
        /// </summary>
        protected virtual void PopulatePossibleSource()
        {
            this.PossibleSource.Clear();
        }
    }
}
