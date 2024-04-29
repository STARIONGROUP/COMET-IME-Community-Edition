// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramElementThingRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="DiagramElementThing"/>
    /// </summary>
    public abstract partial class DiagramElementThingRowViewModel<T> : DiagramElementContainerRowViewModel<T> where T : DiagramElementThing
    {

        /// <summary>
        /// Backing field for <see cref="DepictedThing"/>
        /// </summary>
        private Thing depictedThing;

        /// <summary>
        /// Backing field for <see cref="SharedStyle"/>
        /// </summary>
        private SharedStyle sharedStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementThingRowViewModel{T}"/> class
        /// </summary>
        /// <param name="diagramElementThing">The <see cref="DiagramElementThing"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected DiagramElementThingRowViewModel(T diagramElementThing, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramElementThing, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the DepictedThing
        /// </summary>
        public Thing DepictedThing
        {
            get { return this.depictedThing; }
            set { this.RaiseAndSetIfChanged(ref this.depictedThing, value); }
        }

        /// <summary>
        /// Gets or sets the SharedStyle
        /// </summary>
        public SharedStyle SharedStyle
        {
            get { return this.sharedStyle; }
            set { this.RaiseAndSetIfChanged(ref this.sharedStyle, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.DepictedThing = this.Thing.DepictedThing;
            this.SharedStyle = this.Thing.SharedStyle;
        }
    }
}
