// -------------------------------------------------------------------------------------------------
// <copyright file="RuleRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The rule row view model that represents a <see cref="Rule"/>.
    /// </summary>
    public class RuleRowViewModel : CDP4CommonView.RuleRowViewModel<Rule>
    {
        /// <summary>
        /// Backing field for the <see cref="ContainerRdl"/> property
        /// </summary>
        private string containerRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleRowViewModel"/> class.
        /// </summary>
        /// <param name="rule">The rule referenced by this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public RuleRowViewModel(Rule rule, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(rule, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Container RDL ShortName.
        /// </summary>
        public string ContainerRdl
        {
            get
            {
                return this.containerRdl;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.containerRdl, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the current view-model
        /// </summary>
        public string ClassKind { get; private set; }

        /// <summary>
        /// Updates the properties of the current view-model
        /// </summary>
        private void UpdateProperties()
        {
            var container = this.Thing.Container as ReferenceDataLibrary;
            this.ClassKind = this.Thing.ClassKind.ToString();
            this.ContainerRdl = container == null ? string.Empty : container.ShortName;
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
    }
}
