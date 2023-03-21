// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.ViewModels.Dialogs.Rows;

    using ReactiveUI;

    [ThingDialogViewModelExport(ClassKind.MultiRelationship)]
    public class MultiRelationshipDialogViewModel : CDP4CommonView.MultiRelationshipDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The Required Referance-Data-library for the current <see cref="Iteration" />
        /// </summary>
        private ModelReferenceDataLibrary mRdl;

        /// <summary>
        /// The backing field for <see cref="Name" /> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public MultiRelationshipDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipDialogViewModel" /> class
        /// </summary>
        /// <param name="multiRelationship">
        /// The <see cref="MultiRelationship" /> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction" /> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Thing" /> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}" /> is the root of all <see cref="DialogViewModelBase{T}" />
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}" /> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService" /> that is used to navigate to a dialog of a specific
        /// <see cref="Thing" />.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing" /> that contains the created <see cref="Thing" /> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container" /> argument
        /// </param>
        public MultiRelationshipDialogViewModel(MultiRelationship multiRelationship, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(multiRelationship, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets a list of <see cref="RelatedThingDialogRowViewModel" />
        /// </summary>
        public ReactiveList<RelatedThingDialogRowViewModel> RelatedThingRows { get; set; }

        /// <summary>
        /// Gets or sets the create entry <see cref="ICommand" /> to create a new entry for related thing
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateThingEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the delete row <see cref="ICommand" /> to delete a row from related thing list
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteRowCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Name of the current <see cref="BinaryRelationship" />
        /// </summary>
        /// <remarks>
        /// The validation of the name has been disabled
        /// </remarks>
        [ValidationOverride]
        [CDPVersion("1.2.0")]
        public override string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="RelatedThingDialogRowViewModel" />
        /// </summary>
        public RelatedThingDialogRowViewModel SelectedRelatedThingRow { get; set; }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;
            this.mRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            this.RelatedThingRows = new ReactiveList<RelatedThingDialogRowViewModel>();
        }

        /// <summary>
        /// Initializes the subscribers
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.WhenAnyValue(x => x.Name).Subscribe(x => this.UpdateOkCanExecute());

            this.CreateThingEntryCommand = ReactiveCommandCreator.Create(this.ExecuteCreateEntryCommand, this.WhenAnyValue(x => x.IsReadOnly).Select(x => !x));

            this.DeleteRowCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteRow, this.WhenAnyValue(x => x.IsReadOnly).Select(x => !x));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.RelatedThing.Clear();
            
            var relatedThings = this.RelatedThingRows.Select(x => x.SelectedThing);

            clone.RelatedThing.AddRange(relatedThings);
        }

        /// <summary>
        /// Execute the <see cref="CreateThingEntryCommand" />
        /// </summary>
        protected void ExecuteCreateEntryCommand()
        {
            var relatedThingRow = new RelatedThingDialogRowViewModel(new MultiRelationship(), this.Session, this);
            this.RelatedThingRows.Add(relatedThingRow);
        }

        /// <summary>
        /// Execute the <see cref="DeleteComponentCommand" />
        /// </summary>
        protected void ExecuteDeleteRow()
        {
            this.RelatedThingRows.Remove(this.SelectedRelatedThingRow);
        }

        /// <summary>
        /// Populate the <see cref="CDP4CommonView.RelationshipDialogViewModel{T}.PossibleCategory" /> and <see cref="Category" />
        /// properties.
        /// </summary>
        protected override void PopulateCategory()
        {
            var categories = this.mRdl.DefinedCategory.Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship)).ToList();
            categories.AddRange(this.mRdl.GetRequiredRdls().SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship)));

            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories.OrderBy(c => c.ShortName));
            base.PopulateCategory();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            this.Name = this.Thing.Name;
            this.SelectedOwner = this.Thing.Owner;

            foreach (var thing in this.Thing.RelatedThing)
            {
                var relatedThingRow = new RelatedThingDialogRowViewModel(this.Thing, this.Session, this);
                relatedThingRow.SelectedClassKind = thing.ClassKind;
                relatedThingRow.SelectedThing = thing;
                this.RelatedThingRows.Add(relatedThingRow);
            }

            this.PopulateCategory();
        }

        /// <summary>
        /// Populate the <see cref="CDP4CommonView.RelationshipDialogViewModel{T}.PossibleOwner" /> property
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();

            var engineeringModel = (EngineeringModel)this.Container.TopContainer;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
        }
    }
}
