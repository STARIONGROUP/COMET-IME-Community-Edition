// -------------------------------------------------------------------------------------------------
// <copyright file="RuleViolationRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="RuleViolation"/>
    /// </summary>
    public partial class RuleViolationRowViewModel : RowViewModelBase<RuleViolation>
    {
        private ObservableAsPropertyHelper<string> name;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleViolationRowViewModel"/> class
        /// </summary>
        /// <param name="ruleViolation">The <see cref="RuleViolation"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RuleViolationRowViewModel(RuleViolation ruleViolation, ISession session, IViewModelBase<Thing> containerViewModel) : base(ruleViolation, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Name property of the row
        /// </summary>
        public string Name => this.name.Value;

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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.Description = this.Thing.Description;
        }

        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            this.name = this.WhenAnyValue(x => x.Description)
                            .ToProperty(this, x => x.Name, scheduler: RxApp.MainThreadScheduler);

            this.Disposables.Add(this.name);
        }
    }
}
