// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="QuantityKind"/>
    /// </summary>
    public abstract partial class QuantityKindRowViewModel<T> : ScalarParameterTypeRowViewModel<T> where T : QuantityKind
    {
        /// <summary>
        /// Backing field for <see cref="DefaultScale"/> property
        /// </summary>
        private MeasurementScale defaultScale;

        /// <summary>
        /// Backing field for <see cref="DefaultScaleName"/> property
        /// </summary>
        private string defaultScaleName;

        /// <summary>
        /// Backing field for <see cref="DefaultScaleShortName"/> property
        /// </summary>
        private string defaultScaleShortName;

        /// <summary>
        /// Backing field for <see cref="QuantityDimensionExpression"/> property
        /// </summary>
        private string quantityDimensionExpression;

        /// <summary>
        /// Backing field for <see cref="QuantityDimensionSymbol"/> property
        /// </summary>
        private string quantityDimensionSymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindRowViewModel{T}"/> class
        /// </summary>
        /// <param name="quantityKind">The <see cref="QuantityKind"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected QuantityKindRowViewModel(T quantityKind, ISession session, IViewModelBase<Thing> containerViewModel) : base(quantityKind, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the DefaultScale
        /// </summary>
        public MeasurementScale DefaultScale
        {
            get { return this.defaultScale; }
            set { this.RaiseAndSetIfChanged(ref this.defaultScale, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultScale"/>
        /// </summary>
        public string DefaultScaleName
        {
            get { return this.defaultScaleName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultScaleName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultScale"/>
        /// </summary>
        public string DefaultScaleShortName
        {
            get { return this.defaultScaleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultScaleShortName, value); }
        }

        /// <summary>
        /// Gets or sets the QuantityDimensionExpression
        /// </summary>
        public string QuantityDimensionExpression
        {
            get { return this.quantityDimensionExpression; }
            set { this.RaiseAndSetIfChanged(ref this.quantityDimensionExpression, value); }
        }

        /// <summary>
        /// Gets or sets the QuantityDimensionSymbol
        /// </summary>
        public string QuantityDimensionSymbol
        {
            get { return this.quantityDimensionSymbol; }
            set { this.RaiseAndSetIfChanged(ref this.quantityDimensionSymbol, value); }
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
            this.DefaultScale = this.Thing.DefaultScale;
            if (this.Thing.DefaultScale != null)
            {
                this.DefaultScaleName = this.Thing.DefaultScale.Name;
                this.DefaultScaleShortName = this.Thing.DefaultScale.ShortName;
            }
            else
            {
                this.DefaultScaleName = string.Empty;
                this.DefaultScaleShortName = string.Empty;
            }
            this.QuantityDimensionExpression = this.Thing.QuantityDimensionExpression;
            this.QuantityDimensionSymbol = this.Thing.QuantityDimensionSymbol;
        }
    }
}
