// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public abstract partial class ParameterOrOverrideBaseRowViewModel<T> : ParameterBaseRowViewModel<T>, IParameterOrOverrideBaseRowViewModel<T> where T :ParameterOrOverrideBase
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ParameterSubscriptionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterSubscriptionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterOrOverrideBase">The <see cref="ParameterOrOverrideBase"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected ParameterOrOverrideBaseRowViewModel(T parameterOrOverrideBase, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterOrOverrideBase, session, containerViewModel)
        {
            this.parameterSubscriptionFolder = new CDP4Composition.FolderRowViewModel("Parameter Subscription", "Parameter Subscription", this.Session, this);
            this.ContainedRows.Add(this.parameterSubscriptionFolder);
            this.UpdateProperties();
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
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.ParameterSubscription, this.parameterSubscriptionFolder, this.AddParameterSubscriptionRowViewModel);
        }
        /// <summary>
        /// Add an Parameter Subscription row view model to the list of <see cref="ParameterSubscription"/>
        /// </summary>
        /// <param name="parameterSubscription">
        /// The <see cref="ParameterSubscription"/> that is to be added
        /// </param>
        private ParameterSubscriptionRowViewModel AddParameterSubscriptionRowViewModel(ParameterSubscription parameterSubscription)
        {
            return new ParameterSubscriptionRowViewModel(parameterSubscription, this.Session, this);
        }
    }
}
