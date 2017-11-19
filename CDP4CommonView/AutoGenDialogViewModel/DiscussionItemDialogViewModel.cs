// -------------------------------------------------------------------------------------------------
// <copyright file="DiscussionItemDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DiscussionItem"/>
    /// </summary>
    public abstract partial class DiscussionItemDialogViewModel<T> : GenericAnnotationDialogViewModel<T> where T : DiscussionItem
    {
        /// <summary>
        /// Backing field for <see cref="SelectedReplyTo"/>
        /// </summary>
        private DiscussionItem selectedReplyTo;


        /// <summary>
        /// Initializes a new instance of the <see cref="DiscussionItemDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected DiscussionItemDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscussionItemDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="discussionItem">
        /// The <see cref="DiscussionItem"/> that is the subject of the current view-model. This is the object
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
        protected DiscussionItemDialogViewModel(T discussionItem, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(discussionItem, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the SelectedReplyTo
        /// </summary>
        public virtual DiscussionItem SelectedReplyTo
        {
            get { return this.selectedReplyTo; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReplyTo, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DiscussionItem"/>s for <see cref="SelectedReplyTo"/>
        /// </summary>
        public ReactiveList<DiscussionItem> PossibleReplyTo { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedReplyTo"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedReplyToCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedReplyToCommand = this.WhenAny(vm => vm.SelectedReplyTo, v => v.Value != null);
            this.InspectSelectedReplyToCommand = ReactiveCommand.Create(canExecuteInspectSelectedReplyToCommand);
            this.InspectSelectedReplyToCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReplyTo));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ReplyTo = this.SelectedReplyTo;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleReplyTo = new ReactiveList<DiscussionItem>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedReplyTo = this.Thing.ReplyTo;
            this.PopulatePossibleReplyTo();
        }

        /// <summary>
        /// Populates the <see cref="PossibleReplyTo"/> property
        /// </summary>
        protected virtual void PopulatePossibleReplyTo()
        {
            this.PossibleReplyTo.Clear();
        }
    }
}
