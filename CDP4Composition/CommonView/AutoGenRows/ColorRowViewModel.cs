﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ColorRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="Color"/>
    /// </summary>
    public partial class ColorRowViewModel : DiagramThingBaseRowViewModel<Color>
    {

        /// <summary>
        /// Backing field for <see cref="Red"/>
        /// </summary>
        private int red;

        /// <summary>
        /// Backing field for <see cref="Green"/>
        /// </summary>
        private int green;

        /// <summary>
        /// Backing field for <see cref="Blue"/>
        /// </summary>
        private int blue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRowViewModel"/> class
        /// </summary>
        /// <param name="color">The <see cref="Color"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ColorRowViewModel(Color color, ISession session, IViewModelBase<Thing> containerViewModel) : base(color, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Red
        /// </summary>
        public int Red
        {
            get { return this.red; }
            set { this.RaiseAndSetIfChanged(ref this.red, value); }
        }

        /// <summary>
        /// Gets or sets the Green
        /// </summary>
        public int Green
        {
            get { return this.green; }
            set { this.RaiseAndSetIfChanged(ref this.green, value); }
        }

        /// <summary>
        /// Gets or sets the Blue
        /// </summary>
        public int Blue
        {
            get { return this.blue; }
            set { this.RaiseAndSetIfChanged(ref this.blue, value); }
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
            this.Red = this.Thing.Red;
            this.Green = this.Thing.Green;
            this.Blue = this.Thing.Blue;
        }
    }
}
