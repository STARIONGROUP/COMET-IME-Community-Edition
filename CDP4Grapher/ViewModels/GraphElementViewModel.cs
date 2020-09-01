// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphElementViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Utilities;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DevExpress.Mvvm.Native;
    using DevExpress.Xpf.NavBar;

    using ReactiveUI;

    /// <summary>
    /// Represents a diagram element that holds a <see cref="NestedElement"/>
    /// </summary>
    public class GraphElementViewModel : ReactiveObject
    {
        /// <summary>
        /// The represented <see cref="NestedElement"/>
        /// </summary>
        public NestedElement Thing { get; set; }

        /// <summary>
        /// Backing field the <see cref="Name"/> property
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field the <see cref="ShortName"/> property
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field the <see cref="OwnerShortName"/> property
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field the <see cref="Category"/> property
        /// </summary>
        private string category = "-";

        /// <summary>
        /// Backing field the <see cref="ElementPath"/>
        /// </summary>
        private string elementPath;

        /// <summary>
        /// Holds the actual option
        /// </summary>
        private Option actualOption;

        /// <summary>
        /// Gets this respresented <see cref="NestedElement"/> Path
        /// </summary>
        public string ElementPath
        {
            get => this.elementPath;
            private set => this.RaiseAndSetIfChanged(ref this.elementPath, value);
        }

        /// <summary>
        /// Holds the revision number when the <see cref="Thing"/> was last updated
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Gets or sets the Name of the represented <see cref="Thing"/>
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or Sets the ShortName of the represented <see cref="Thing"/>
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets or Sets the <see cref="DomainOfExpertise"/> ShortName of the represented <see cref="Thing"/>
        /// </summary>
        public string OwnerShortName
        {
            get => this.ownerShortName;
            set => this.RaiseAndSetIfChanged(ref this.ownerShortName, value);
        }

        /// <summary>
        /// Gets or Sets the comma separated Categories of the represented <see cref="Thing"/>
        /// </summary>
        public string Category
        {
            get => this.category;
            set => this.RaiseAndSetIfChanged(ref this.category, value);
        }

        /// <summary>
        /// Gets or sets the listener of the <see cref="NestedElementElement"/>
        /// </summary>
        public IDisposable NestedElementElementListener { get; set; }

        /// <summary>
        /// Gets or Sets the NestedElementElement Associated with the <see cref="Thing"/>
        /// </summary>
        public ElementBase NestedElementElement { get; set; }

        /// <summary>
        /// Instanciate a <see cref="GrapherViewModel"/> updating its property with the given <see cref="NestedElement"/> property
        /// </summary>
        /// <param name="nestedElement">The represented nested element</param>
        /// <param name="actualOption">The option</param>
        public GraphElementViewModel(NestedElement nestedElement, Option actualOption)
        {
            this.Thing = nestedElement;
            this.actualOption = actualOption;
            this.NestedElementElement = (ElementBase) nestedElement.ElementUsage.LastOrDefault() ?? nestedElement.RootElement;
            this.RegisterSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Computes the full path of the element
        /// </summary>
        /// <returns>The element path</returns>
        private string ComputePath()
        {
            var elementPathStringBuilder = new StringBuilder();
            elementPathStringBuilder.Append($"{this.actualOption.ShortName}.");

            if (this.NestedElementElement is ElementDefinition elementDefinition)
            {
                elementPathStringBuilder.Append(elementDefinition.ShortName);
            }
            else
            {
                elementPathStringBuilder.Append($"{this.Thing.RootElement.ShortName}.");

                foreach (ElementUsage element in this.Thing.ElementUsage)
                {
                    elementPathStringBuilder.Append($"{element.Name}");

                    if (element != this.Thing.ElementUsage.Last())
                    {
                        elementPathStringBuilder.Append('.');
                    }
                }
            }

            return elementPathStringBuilder.ToString();
        }

        /// <summary>
        /// Update all properties value
        /// </summary>
        private void UpdateProperties()
        {
            this.RevisionNumber = this.NestedElementElement.RevisionNumber;
            this.Name = this.NestedElementElement.Name;
            this.ShortName = this.NestedElementElement.ShortName;
            this.OwnerShortName = this.NestedElementElement.Owner.ShortName;
            var categories = this.NestedElementElement.GetAllCategoryShortNames();
            this.Category = string.IsNullOrWhiteSpace(categories) ? "-" : categories;
            this.ElementPath = this.ComputePath();
        }

        /// <summary>
        /// Register the necessary Subscription for this <see cref="GraphElementViewModel"/> 
        /// </summary>
        private void RegisterSubscriptions()
        {
            this.NestedElementElementListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.NestedElementElement)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());
        }
    }
}
