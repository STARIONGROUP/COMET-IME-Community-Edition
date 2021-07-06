// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEdgeRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
    /// Row class representing a <see cref="DiagramEdge"/>
    /// </summary>
    public partial class DiagramEdgeRowViewModel : DiagramElementThingRowViewModel<DiagramEdge>
    {
        /// <summary>
        /// Backing field for <see cref="Source"/> property
        /// </summary>
        private DiagramElementThing source;

        /// <summary>
        /// Backing field for <see cref="SourceName"/> property
        /// </summary>
        private string sourceName;

        /// <summary>
        /// Backing field for <see cref="Target"/> property
        /// </summary>
        private DiagramElementThing target;

        /// <summary>
        /// Backing field for <see cref="TargetName"/> property
        /// </summary>
        private string targetName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEdgeRowViewModel"/> class
        /// </summary>
        /// <param name="diagramEdge">The <see cref="DiagramEdge"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DiagramEdgeRowViewModel(DiagramEdge diagramEdge, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramEdge, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Source
        /// </summary>
        public DiagramElementThing Source
        {
            get { return this.source; }
            set { this.RaiseAndSetIfChanged(ref this.source, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Source"/>
        /// </summary>
        public string SourceName
        {
            get { return this.sourceName; }
            set { this.RaiseAndSetIfChanged(ref this.sourceName, value); }
        }

        /// <summary>
        /// Gets or sets the Target
        /// </summary>
        public DiagramElementThing Target
        {
            get { return this.target; }
            set { this.RaiseAndSetIfChanged(ref this.target, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Target"/>
        /// </summary>
        public string TargetName
        {
            get { return this.targetName; }
            set { this.RaiseAndSetIfChanged(ref this.targetName, value); }
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
            this.Source = this.Thing.Source;
            if (this.Thing.Source != null)
            {
                this.SourceName = this.Thing.Source.Name;
            }
            else
            {
                this.SourceName = string.Empty;
            }
            this.Target = this.Thing.Target;
            if (this.Thing.Target != null)
            {
                this.TargetName = this.Thing.Target.Name;
            }
            else
            {
                this.TargetName = string.Empty;
            }
        }
    }
}
