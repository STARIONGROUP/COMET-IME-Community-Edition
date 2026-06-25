// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViolatingThingRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row that represents a <see cref="Thing"/> that violates a <see cref="CDP4Common.EngineeringModelData.Rule"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="RuleViolation"/> only carries the <see cref="System.Guid"/> identifiers of the violating
    /// <see cref="Thing"/>s. This row resolves each identifier to the actual <see cref="Thing"/> so that its
    /// human friendly name, short name, owner and type can be presented in dedicated, sortable columns instead
    /// of only an identifier buried in a sentence.
    /// </remarks>
    public class ViolatingThingRowViewModel : RowViewModelBase<Thing>
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="OwnerName"/>
        /// </summary>
        private string ownerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViolatingThingRowViewModel"/> class.
        /// </summary>
        /// <param name="violatingThing">
        /// The <see cref="Thing"/> that violates a rule and that is represented by the current row-view-model.
        /// </param>
        /// <param name="session">
        /// The current active <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The view-model that is the container of the current row-view-model.
        /// </param>
        public ViolatingThingRowViewModel(Thing violatingThing, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(violatingThing, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the human friendly name of the violating <see cref="Thing"/>
        /// </summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets the short name of the violating <see cref="Thing"/>
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets the short name of the owning domain of expertise of the violating <see cref="Thing"/>, if any
        /// </summary>
        public string OwnerName
        {
            get => this.ownerName;
            private set => this.RaiseAndSetIfChanged(ref this.ownerName, value);
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row from the violating <see cref="Thing"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = this.Thing is INamedThing namedThing ? namedThing.Name : this.Thing.UserFriendlyName;
            this.ShortName = this.Thing is IShortNamedThing shortNamedThing ? shortNamedThing.ShortName : this.Thing.UserFriendlyShortName;
            this.OwnerName = this.Thing is IOwnedThing ownedThing && ownedThing.Owner != null ? ownedThing.Owner.ShortName : string.Empty;
            this.Tooltip = $"{this.RowType}: {this.Name} [{this.ShortName}]";
        }
    }
}
