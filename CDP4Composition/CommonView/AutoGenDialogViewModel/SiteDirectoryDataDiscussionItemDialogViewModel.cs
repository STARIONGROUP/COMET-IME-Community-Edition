// -------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryDataDiscussionItemDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="SiteDirectoryDataDiscussionItem"/>
    /// </summary>
    public partial class SiteDirectoryDataDiscussionItemDialogViewModel : DiscussionItemDialogViewModel<SiteDirectoryDataDiscussionItem>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedAuthor"/>
        /// </summary>
        private Person selectedAuthor;


        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryDataDiscussionItemDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public SiteDirectoryDataDiscussionItemDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryDataDiscussionItemDialogViewModel"/> class
        /// </summary>
        /// <param name="siteDirectoryDataDiscussionItem">
        /// The <see cref="SiteDirectoryDataDiscussionItem"/> that is the subject of the current view-model. This is the object
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
        public SiteDirectoryDataDiscussionItemDialogViewModel(SiteDirectoryDataDiscussionItem siteDirectoryDataDiscussionItem, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(siteDirectoryDataDiscussionItem, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as SiteDirectoryDataAnnotation;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type SiteDirectoryDataAnnotation",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedAuthor
        /// </summary>
        public virtual Person SelectedAuthor
        {
            get { return this.selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedAuthor, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Person"/>s for <see cref="SelectedAuthor"/>
        /// </summary>
        public ReactiveList<Person> PossibleAuthor { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedAuthor"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedAuthorCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedAuthorCommand = this.WhenAny(vm => vm.SelectedAuthor, v => v.Value != null);
            this.InspectSelectedAuthorCommand = ReactiveCommand.Create(canExecuteInspectSelectedAuthorCommand);
            this.InspectSelectedAuthorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedAuthor));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Author = this.SelectedAuthor;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleAuthor = new ReactiveList<Person>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedAuthor = this.Thing.Author;
            this.PopulatePossibleAuthor();
        }

        /// <summary>
        /// Populates the <see cref="PossibleAuthor"/> property
        /// </summary>
        protected virtual void PopulatePossibleAuthor()
        {
            this.PossibleAuthor.Clear();
        }
    }
}
