// -------------------------------------------------------------------------------------------------
// <copyright file="OptionDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.ViewModels.Dialogs.Rows;

    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="Option"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.Option)]
    public class OptionDialogViewModel : CDP4CommonView.OptionDialogViewModel, IThingDialogViewModel
    {
        private bool isDefault;

        /// <summary>
        /// Backing field for <see cref="SelectedBaselineOption"/>.
        /// </summary>
        private Option selectedBaselineOption;

        /// <summary>
        /// Backing field for <see cref="ElementUsageRows"/>, built on first access so that the permission check of every
        /// <see cref="ElementUsage"/> is only paid when the Element Usages tab is shown.
        /// </summary>
        private Lazy<List<OptionElementUsageRowViewModel>> elementUsageRows;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public OptionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialogViewModel"/> class
        /// </summary>
        /// <param name="option">
        /// The <see cref="Option"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public OptionDialogViewModel(Option option, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(option, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Sets or gets the if this option is the <see cref="Iteration"/>'s default <see cref="Option"/>
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }

        /// <summary>
        /// Gets the rows that set which <see cref="ElementUsage"/>s of the <see cref="Iteration"/> are included in the <see cref="Option"/>.
        /// </summary>
        public List<OptionElementUsageRowViewModel> ElementUsageRows => this.elementUsageRows.Value;

        /// <summary>
        /// Gets the other <see cref="Option"/>s of the <see cref="Iteration"/> that <see cref="ElementUsageRows"/> can be filled from.
        /// </summary>
        public List<Option> PossibleBaselineOption { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Option"/> whose inclusions are copied to the editable <see cref="ElementUsageRows"/> when selected.
        /// </summary>
        public Option SelectedBaselineOption
        {
            get => this.selectedBaselineOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedBaselineOption, value);
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="ElementUsageRows"/> cannot be edited.
        /// A nested dialog cannot edit them since the parent dialog may still delete the <see cref="Option"/> they would refer to.
        /// </summary>
        public bool IsElementUsagesReadOnly => this.IsReadOnly || !this.isRoot;

        /// <summary>
        /// Gets the command that includes every editable <see cref="ElementUsage"/> in the <see cref="Option"/>.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CheckAllElementUsagesCommand { get; private set; }

        /// <summary>
        /// Gets the command that excludes every editable <see cref="ElementUsage"/> from the <see cref="Option"/>.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UncheckAllElementUsagesCommand { get; private set; }

        /// <summary>
        /// Gets the command that restores <see cref="ElementUsageRows"/> to the state they were in when first shown.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetElementUsagesCommand { get; private set; }

        /// <summary>
        /// Initializes the dialog.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var iteration = (Iteration)this.Container;

            this.elementUsageRows = new Lazy<List<OptionElementUsageRowViewModel>>(this.CreateElementUsageRows);
            this.PossibleBaselineOption = iteration.Option.Where(x => x.Iid != this.Thing.Iid).ToList();
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>s of this dialog.
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecute = Observable.Return(!this.IsElementUsagesReadOnly);
            this.CheckAllElementUsagesCommand = ReactiveCommandCreator.Create(() => this.SetEditableElementUsagesIncluded(_ => true), canExecute);
            this.UncheckAllElementUsagesCommand = ReactiveCommandCreator.Create(() => this.SetEditableElementUsagesIncluded(_ => false), canExecute);
            this.ResetElementUsagesCommand = ReactiveCommandCreator.Create(this.ResetElementUsages, canExecute);

            this.Disposables.Add(this.WhenAnyValue(vm => vm.SelectedBaselineOption)
                .Where(x => x != null)
                .Subscribe(baseline => this.SetEditableElementUsagesIncluded(row => row.IsIncludedIn(baseline))));
        }

        /// <summary>
        /// Populate the <see cref="OptionDialogViewModel.PossibleCategory"/> and <see cref="OptionDialogViewModel.Category"/>
        /// </summary>
        protected override void PopulateCategory()
        {
            // populate the possible categories
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;

            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var possibleRdls = mrdl.GetRequiredRdls().ToList();
            possibleRdls.Add(mrdl);

            var categories = possibleRdls.SelectMany(x => x.DefinedCategory.Where(cat => cat.PermissibleClass.Contains(ClassKind.Option))).OrderBy(x => x.Name).ToList();
            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories);

            base.PopulateCategory();
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            if (this.Thing.IsDefault != this.IsDefault)
            {
                var iteration = (Iteration)this.Container;
                iteration.DefaultOption = this.IsDefault ? this.Thing : null;
            }

            if (!this.elementUsageRows.IsValueCreated)
            {
                return;
            }

            foreach (var row in this.ElementUsageRows.Where(x => x.IsIncluded != x.WasIncluded))
            {
                var clone = row.Thing.Clone(false);

                // the live ExcludeOption may have changed since the dialog opened, so always remove before adding
                clone.ExcludeOption.RemoveAll(x => x.Iid == this.Thing.Iid);

                if (!row.IsIncluded)
                {
                    clone.ExcludeOption.Add(this.Thing);
                }

                this.transaction.CreateOrUpdate(clone);
            }
        }

        /// <summary>
        /// Creates a row for every <see cref="ElementUsage"/> of the <see cref="Iteration"/>.
        /// </summary>
        /// <returns>The rows of <see cref="ElementUsageRows"/>.</returns>
        private List<OptionElementUsageRowViewModel> CreateElementUsageRows()
        {
            var rows = ((Iteration)this.Container).Element
                .SelectMany(x => x.ContainedElement)
                .Select(x => new OptionElementUsageRowViewModel(x, this.Thing, !this.IsElementUsagesReadOnly && this.PermissionService.CanWrite(x)))
                .ToList();

            // once a row no longer matches the selected baseline, clear the selection so that picking it again re-applies it
            this.Disposables.Add(rows
                .Select(row => row.WhenAnyValue(x => x.IsIncluded).Skip(1).Where(isIncluded => this.SelectedBaselineOption != null && isIncluded != row.IsIncludedIn(this.SelectedBaselineOption)))
                .Merge()
                .Subscribe(_ => this.SelectedBaselineOption = null));

            return rows;
        }

        /// <summary>
        /// Restores every row of <see cref="ElementUsageRows"/> to its persisted state and clears <see cref="SelectedBaselineOption"/>.
        /// </summary>
        private void ResetElementUsages()
        {
            this.SelectedBaselineOption = null;

            foreach (var row in this.ElementUsageRows)
            {
                row.IsIncluded = row.WasIncluded;
            }
        }

        /// <summary>
        /// Sets <see cref="OptionElementUsageRowViewModel.IsIncluded"/> on every editable row of <see cref="ElementUsageRows"/>.
        /// </summary>
        /// <param name="isIncluded">Computes whether the <see cref="ElementUsage"/> of a row is included in the <see cref="Option"/>.</param>
        private void SetEditableElementUsagesIncluded(Func<OptionElementUsageRowViewModel, bool> isIncluded)
        {
            foreach (var row in this.ElementUsageRows.Where(x => x.IsEditable))
            {
                row.IsIncluded = isIncluded(row);
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsDefault = this.Thing.IsDefault;
        }
    }
}
