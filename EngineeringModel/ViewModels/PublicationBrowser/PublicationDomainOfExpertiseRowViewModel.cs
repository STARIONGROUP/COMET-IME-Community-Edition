// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationDomainOfExpertiseRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4EngineeringModel.ViewModels.PublicationBrowser;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="DomainOfExpertiseRowViewModel" /> view
    /// </summary>
    public class PublicationDomainOfExpertiseRowViewModel : CDP4CommonView.DomainOfExpertiseRowViewModel, IPublishableRow
    {
        /// <summary>
        /// Backing field for <see cref="IsEmpty" />
        /// </summary>
        private bool isEmpty = true;

        /// <summary>
        /// Backing field for <see cref="ToBePublished" />
        /// </summary>
        private bool toBePublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4CommonView.DomainOfExpertiseRowViewModel" /> class
        /// </summary>
        /// <param name="domainOfExpertise">The <see cref="DomainOfExpertise" /> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">
        /// The <see cref="IViewModelBase{T}" /> that is the container of this
        /// <see cref="IRowViewModelBase{T}" />
        /// </param>
        public PublicationDomainOfExpertiseRowViewModel(DomainOfExpertise domainOfExpertise, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(domainOfExpertise, session, containerViewModel)
        {
            this.Disposables.Add(this.WhenAnyValue(vm => vm.ToBePublished).Subscribe(_ => this.ToBePublishedChanged()));
            this.Disposables.Add(this.ContainedRows.IsEmptyChanged.Subscribe(_ => this.SetIsEmpty()));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the row is to be published.
        /// </summary>
        public bool IsEmpty
        {
            get { return this.isEmpty; }
            set { this.RaiseAndSetIfChanged(ref this.isEmpty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the row is to be published.
        /// </summary>
        public bool ToBePublished
        {
            get { return this.toBePublished; }
            set { this.RaiseAndSetIfChanged(ref this.toBePublished, value); }
        }

        /// <summary>
        /// Set whether this row is Empty
        /// </summary>
        private void SetIsEmpty()
        {
            this.IsEmpty = this.ContainedRows.Count == 0;
        }

        /// <summary>
        /// Exute the change to publication selection.
        /// </summary>
        private void ToBePublishedChanged()
        {
            foreach (var row in this.ContainedRows.OfType<IPublishableRow>())
            {
                row.ToBePublished = this.ToBePublished;
            }
        }
    }
}
