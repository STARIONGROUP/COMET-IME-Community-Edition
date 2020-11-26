// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionFilterSelectionDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ParameterSubscriptionFilterSelectionDialogViewModel"/> is to provide selection criteria
    /// to select Parameters for which <see cref="ParameterSubscription"/>s need to be created
    /// </summary>
    public class ParameterSubscriptionFilterSelectionDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/> property
        /// </summary>
        private ReactiveList<Category> selectedCategories;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/> property
        /// </summary>
        private ReactiveList<DomainOfExpertise> selectedOwners;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterTypes"/> property
        /// </summary>
        private ReactiveList<ParameterType> selectedParameterTypes;

        /// <summary>
        /// Backing field for <see cref="IsUncategorizedIncluded"/> property
        /// </summary>
        private bool isUncategorizedIncluded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionFilterSelectionDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterTypes"></param>
        /// <param name="categories"></param>
        /// <param name="domainOfExpertises"></param>
        public ParameterSubscriptionFilterSelectionDialogViewModel(IEnumerable<ParameterType> parameterTypes, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises)
        {
            this.PossibleParameterTypes = parameterTypes.OrderBy(x => x.Name);
            this.PossibleCategories = categories.OrderBy(x => x.Name);
            this.PossibleOwner = domainOfExpertises.OrderBy(x => x.Name);

            this.selectedCategories = new ReactiveList<Category>();
            this.selectedOwners = new ReactiveList<DomainOfExpertise>();
            this.selectedParameterTypes = new ReactiveList<ParameterType>();

            this.InitializeReactiveCommands();
        }

        /// <summary>
        /// Gets the possible <see cref="Category"/>s the user can select from
        /// </summary>
        public IEnumerable<Category> PossibleCategories { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="DomainOfExpertise"/>s the user can select from
        /// </summary>
        public IEnumerable<DomainOfExpertise> PossibleOwner { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="ParameterType"/>s the user can select from
        /// </summary>

        public IEnumerable<ParameterType> PossibleParameterTypes { get; private set; }
        
        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> SelectedCategories
        {
            get => this.selectedCategories;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategories, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise"/>s
        /// </summary>
        public ReactiveList<DomainOfExpertise> SelectedOwners
        {
            get => this.selectedOwners;
            set => this.RaiseAndSetIfChanged(ref this.selectedOwners, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterType"/>s
        /// </summary>
        public ReactiveList<ParameterType> SelectedParameterTypes
        {
            get => this.selectedParameterTypes;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterTypes, value);
        }

        /// <summary>
        /// Gets or sets a value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </summary>
        public bool IsUncategorizedIncluded
        {
            get => this.isUncategorizedIncluded;
            set => this.RaiseAndSetIfChanged(ref this.isUncategorizedIncluded, value);
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// The initialize reactive commands.
        /// </summary>
        private void InitializeReactiveCommands()
        {
            var canOk = this.WhenAnyValue(x=> x.SelectedCategories.Count,
                x => x.SelectedOwners.Count,
                x => x.SelectedParameterTypes.Count,
                (category, owner, parameterType) => 
                    category != 0 && owner != 0 && parameterType != 0);

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
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
            this.DialogResult = new ParameterSubscriptionFilterSelectionResult(true, this.IsUncategorizedIncluded, this.selectedParameterTypes, this.selectedCategories, this.selectedOwners);
        }
    }
}
