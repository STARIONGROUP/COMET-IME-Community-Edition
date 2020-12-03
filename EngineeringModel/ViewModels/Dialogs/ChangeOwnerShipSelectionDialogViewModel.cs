// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeOwnershipSelectionDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ChangeOwnershipSelectionDialogViewModel"/> is to facilitate the selection
    /// of the new owner for an <see cref="IOwnedThing"/> and it's contained <see cref="IOwnedThing"/>
    /// </summary>
    /// <remarks>
    /// this allows a user to change the ownership of an <see cref="ElementDefinition"/> and it's contained <see cref="Parameter"/>s
    /// </remarks>
    public class ChangeOwnershipSelectionDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedOwner"/> property.
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for the <see cref="IsContainedItemChangeOwnershipSelected"/> property.
        /// </summary>
        private bool isContainedItemChangeOwnershipSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeOwnershipSelectionDialogViewModel"/> class
        /// </summary>
        /// <param name="domainOfExpertises">
        /// A <see cref="IEnumerable{DomainOfExpertise}"/> from which the <see cref="SelectedOwner"/> needs to be selected
        /// </param>
        public ChangeOwnershipSelectionDialogViewModel(IEnumerable<DomainOfExpertise> domainOfExpertises)
        {
            if (domainOfExpertises == null)
            {
                throw new ArgumentNullException(nameof(domainOfExpertises), $"The {nameof(domainOfExpertises)} may not be null");
            }

            this.PossibleOwner = domainOfExpertises.OrderBy(x => x.Name);

            this.InitializeReactiveCommands();
        }

        /// <summary>
        /// Gets the possible <see cref="DomainOfExpertise"/>s the user can select from
        /// </summary>
        public IEnumerable<DomainOfExpertise> PossibleOwner { get; private set; }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s
        /// </summary>
        private void InitializeReactiveCommands()
        {
            var canOk = this.WhenAny(x => x.SelectedOwner,owner => owner != null);

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }

        /// <summary>
        /// Gets or sets a value indicating whether the contained <see cref="IOwnedThing"/>s need their owner to
        /// be updated tot the <see cref="SelectedOwner"/> as well.
        /// </summary>
        public bool IsContainedItemChangeOwnershipSelected
        {
            get => this.isContainedItemChangeOwnershipSelected;
            set => this.RaiseAndSetIfChanged(ref this.isContainedItemChangeOwnershipSelected, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise"/>s
        /// </summary>
        public DomainOfExpertise SelectedOwner
        {
            get => this.selectedOwner;
            set => this.RaiseAndSetIfChanged(ref this.selectedOwner, value);
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/> 
        /// </summary>
        private void ExecuteOk()
        {
            this.DialogResult = new ChangeOwnershipSelectionResult(true, this.SelectedOwner, this.IsContainedItemChangeOwnershipSelected);
        }
    }
}
