﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PointRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="Point"/>
    /// </summary>
    public partial class PointRowViewModel : DiagramThingBaseRowViewModel<Point>
    {

        /// <summary>
        /// Backing field for <see cref="X"/>
        /// </summary>
        private float x;

        /// <summary>
        /// Backing field for <see cref="Y"/>
        /// </summary>
        private float y;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointRowViewModel"/> class
        /// </summary>
        /// <param name="point">The <see cref="Point"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public PointRowViewModel(Point point, ISession session, IViewModelBase<Thing> containerViewModel) : base(point, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the X
        /// </summary>
        public float X
        {
            get { return this.x; }
            set { this.RaiseAndSetIfChanged(ref this.x, value); }
        }

        /// <summary>
        /// Gets or sets the Y
        /// </summary>
        public float Y
        {
            get { return this.y; }
            set { this.RaiseAndSetIfChanged(ref this.y, value); }
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
            this.X = this.Thing.X;
            this.Y = this.Thing.Y;
        }
    }
}
