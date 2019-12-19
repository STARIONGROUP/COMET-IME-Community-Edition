// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementsSpecificationEditor
{
    using System;
    using System.Reactive.Linq;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4CommonView.EventAggregator;
    using CDP4CommonView.ViewModels;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Requirements.Utils;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="RequirementRowViewModel"/> is the view-model that represents a <see cref="Requirement"/> in the <see cref="RequirementsSpecificationEditorViewModel"/>
    /// </summary>
    public class RequirementRowViewModel : CDP4CommonView.RequirementRowViewModel, IBreadCrumb
    {
        /// <summary>
        /// Backing field for the <see cref="BreadCrumb"/> property
        /// </summary>
        private string breadCrumb;

        /// <summary>
        /// Backing field for the <see cref="Categories"/> property
        /// </summary>
        private string categories;

        /// <summary>
        /// Backing field for the <see cref="DefinitionContent"/> property
        /// </summary>
        private string definitionContent;

        /// <summary>
        /// the content of the <see cref="Definition"/> 
        /// </summary>
        private string originalDefinitionContent;

        /// <summary>
        /// Backing field for the <see cref="IsDirty"/> property
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Backing field for <see cref="EventPublisher"/>
        /// </summary>
        private EventPublisher eventPublisher;

        /// <summary>
        /// The <see cref="Definition"/> to display
        /// </summary>
        private Definition definition;

        /// <summary>
        /// The subscription on <see cref="definition"/>
        /// </summary>
        private IDisposable definitionSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementRowViewModel"/> class
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RequirementRowViewModel(Requirement requirement, ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(requirement, session, containerViewModel)
        {
            this.InitializeCommands();
            this.EventPublisher = new EventPublisher();

            this.WhenAnyValue(vm => vm.DefinitionContent).Subscribe(
                _ =>
                {
                    var originalDefinition = this.Thing.Definition.FirstOrDefault();
                    var originalDefinitionText = originalDefinition == null ? String.Empty : originalDefinition.Content;
                    this.IsDirty = this.DefinitionContent != originalDefinitionText;
                });

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="EventPublisher"/>
        /// </summary>
        public EventPublisher EventPublisher
        {
            get { return this.eventPublisher; }
            private set { this.RaiseAndSetIfChanged(ref this.eventPublisher, value); }
        }

        /// <summary>
        /// Gets or sets the Save <see cref="ReactiveCommand"/> to save the content of the note.
        /// </summary>
        public ReactiveCommand<Unit> SaveCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Cancel <see cref="ReactiveCommand"/> to save the content of the note.
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the content of the <see cref="Definition"/> of the <see cref="Requirement"/>
        /// </summary>
        public string DefinitionContent
        {
            get { return this.definitionContent; }
            set { this.RaiseAndSetIfChanged(ref this.definitionContent, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="BreadCrumb"/> property
        /// </summary>
        /// <remarks>
        /// The <see cref="BreadCrumb"/> property is used for sorting
        /// </remarks>
        public string BreadCrumb
        {
            get { return this.breadCrumb; }
            set { this.RaiseAndSetIfChanged(ref this.breadCrumb, value); }
        }

        /// <summary>
        /// Gets or sets the categories of the <see cref="Requirement"/> as comma separated short names
        /// </summary>
        public string Categories
        {
            get { return this.categories; }
            set { this.RaiseAndSetIfChanged(ref this.categories, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the current row is dirty or not
        /// </summary>
        public bool IsDirty
        {
            get { return this.isDirty; }
            set { this.RaiseAndSetIfChanged(ref this.isDirty, value); }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Dispose of this row
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.definitionSubscription != null)
            {
                this.definitionSubscription.Dispose();
            }

            this.SaveCommand.Dispose();
            this.CancelCommand.Dispose();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.OwnerShortName = this.Thing.Owner != null ? this.Thing.Owner.ShortName : string.Empty;

            this.SetDefinitionContent();

            this.SetCategories();

            this.BreadCrumb = this.Thing.BreadCrumb();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler for this <see cref="definition"/>
        /// </summary>
        /// <param name="defEvent">The <see cref="ObjectChangedEvent"/></param>
        private void OnDefinitionUpdate(ObjectChangedEvent defEvent)
        {
            if (defEvent.EventKind == EventKind.Removed)
            {
                this.definition = null;
                this.definitionSubscription.Dispose();
            }

            this.SetDefinitionContent();
        }

        /// <summary>
        /// Sets the value of the <see cref="DefinitionContent"/> property.
        /// </summary>
        private void SetDefinitionContent()
        {
            if (this.definition == null)
            {
                this.definition = this.Thing.Definition.FirstOrDefault();
                if (this.definition == null)
                {
                    return;
                }

                this.definitionSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.definition)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.OnDefinitionUpdate);
            }

            this.originalDefinitionContent = this.definition.Content;
            this.DefinitionContent = this.originalDefinitionContent;
        }

        /// <summary>
        /// Set the <see cref="Categories"/> property based on the <see cref="Requirement.Category"/> property
        /// </summary>
        private void SetCategories()
        {
            if (!this.Thing.Category.Any())
            {
                this.Categories = "-";
            }
            else
            {
                this.Categories = string.Join(", ", this.Thing.Category.Select(x => x.ShortName));
            }
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>s.
        /// </summary>
        private void InitializeCommands()
        {
            var canSave = this.WhenAnyValue(vm => vm.IsDirty).Select(_ => this.IsDirty);
            this.SaveCommand =
                ReactiveCommand.CreateAsyncTask(canSave, x => this.WriteDefinitionUpdate(), RxApp.MainThreadScheduler);

            this.CancelCommand = ReactiveCommand.Create(canSave);
            this.CancelCommand.Subscribe(_ => this.ResetContent(true));
        }

        /// <summary>
        /// Resets the content to the content of the first <see cref="Definition"/> of the <see cref="Requirement"/>
        /// </summary>
        /// <param name="withWarning">
        /// If true, a warning message will be displayed.
        /// </param>
        private void ResetContent(bool withWarning)
        {
            // give a warning. If user cancels the changes are not reset.
            if (withWarning)
            {
                var yesNoDialog = new YesNoDialogViewModel("Cancel Edit",
                    "Press Yes if you want to cancel the edit. All changes will be lost");
                var dialogResult = this.dialogNavigationService.NavigateModal(yesNoDialog);
                if (dialogResult != null && dialogResult.Result != null && dialogResult.Result.Value)
                {
                    this.DefinitionContent = this.originalDefinitionContent;
                    this.EventPublisher.Publish(new ConfirmationEvent(true));
                }
            }
        }

        /// <summary>
        /// Writes updates to the <see cref="Definition"/> to the data-soource
        /// </summary>
        private async Task WriteDefinitionUpdate()
        {
            var definition = this.Thing.Definition.FirstOrDefault();
            if (definition != null)
            {
                var definitionClone = definition.Clone(false);
                definitionClone.Content = this.DefinitionContent;

                await this.DalWrite(definitionClone);

                if (!this.HasError)
                {
                    this.EventPublisher.Publish(new ConfirmationEvent(true));
                }
            }
        }
    }
}