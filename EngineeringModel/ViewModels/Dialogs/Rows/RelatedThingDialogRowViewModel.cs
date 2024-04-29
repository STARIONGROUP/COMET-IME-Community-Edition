// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedThingDialogRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs.Rows
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// row-view-model used to represent a related <see cref="Thing"/> in <see cref="Dialogs.MultiRelationshipDialogViewModel"/>
    /// </summary>
    public class RelatedThingDialogRowViewModel : MultiRelationshipRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="PossibleThings"/>
        /// </summary>
        private ReactiveList<Thing> possibleThings;

        /// <summary>
        /// Backing field for <see cref="SelectedClassKind"/>
        /// </summary>
        private ClassKind selectedClassKind;

        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private Thing selectedThing;

        /// <summary>
        /// Initializes a new instance of <see cref="RelatedThingDialogRowViewModel"/> class
        /// </summary>
        /// <param name="multiRelationship">The <see cref="MultiRelationship"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel"></param>
        public RelatedThingDialogRowViewModel(MultiRelationship multiRelationship, ISession session, IViewModelBase<Thing> containerViewModel) : base(multiRelationship, session, containerViewModel)
        {
            this.PopulatePossibleClasskind();
            this.WhenAnyValue(x => x.SelectedClassKind).Subscribe(x => this.PopulatePossibleThingList());
        }

        /// <summary>
        /// Gets or sets a list of possible <see cref="Thing"/> for a given <see cref="ClassKind"/>
        /// </summary>
        public ReactiveList<Thing> PossibleThings
        {
            get => this.possibleThings;
            set => this.RaiseAndSetIfChanged(ref this.possibleThings, value);
        }

        /// <summary>
        /// Gets a list of possible <see cref="ClassKind"/>
        /// </summary>
        public ReactiveList<ClassKind> PossibleClassKind { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ClassKind"/>
        /// </summary>
        public ClassKind SelectedClassKind
        {
            get => this.selectedClassKind;
            set => this.RaiseAndSetIfChanged(ref this.selectedClassKind, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Thing"/>
        /// </summary>
        public Thing SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.CategoryDialogViewModel.PermissibleClass" /> property
        /// </summary>
        private void PopulatePossibleClasskind()
        {
            this.PossibleClassKind = new ReactiveList<ClassKind>();

            var possibleClassKinds = IterationContainmentClassType.ClassKindArray;

            this.PossibleClassKind.AddRange(possibleClassKinds);
        }

        /// <summary>
        /// Populate the <see cref="PossibleThings"/> property
        /// </summary>
        private void PopulatePossibleThingList()
        {
            this.PossibleThings = new ReactiveList<Thing>();
            var possibleSources = this.Session.Assembler.Cache.Values.Select(x => x.Value).Where(x => x.ClassKind == this.SelectedClassKind).ToList();

            // Avoid setting the same Thing as source and target
            if (this.SelectedThing != null && this.SelectedThing.ClassKind == this.SelectedClassKind)
            {
                possibleSources.Remove(possibleSources.Single(t => t == this.SelectedThing));
            }

            this.PossibleThings.AddRange(possibleSources);
        }
    }
}
