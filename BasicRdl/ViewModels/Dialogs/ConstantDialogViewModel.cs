// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantDialogViewModel.cs" company="RHEA System S.A.">
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ConstantDialogViewModel"/> is to allow a <see cref="Constant"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Constant"/> will result in an <see cref="Constant"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Constant)]
    public class ConstantDialogViewModel : CDP4CommonView.ConstantDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ConstantDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDialogViewModel"/> class.
        /// </summary>
        /// <param name="constant">
        /// The <see cref="Constant"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CategoryDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CategoryDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ConstantDialogViewModel(Constant constant, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(constant, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedScale).Subscribe(_ => this.UpdateOkCanExecute());
            this.Value.ItemChanged.WhenAnyPropertyChanged().Subscribe(_ => this.UpdateOkCanExecute());

            this.WhenAnyValue(vm => vm.Container).Skip(1).Subscribe(_ => this.PopulatePossibleParameterType());
            this.WhenAnyValue(vm => vm.SelectedParameterType).Subscribe(_ => 
            { 
                this.PopulatePossibleScale(); 
                this.PopulateValue(); 
                this.UpdateOkCanExecute();
            });
        }

        /// <summary>
        /// Gets a value indicating whether the ParameterType property is ReadOnly.
        /// </summary>
        public bool IsParameterTypeReadOnly => this.IsReadOnly || this.dialogKind == ThingDialogKind.Update;

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PopulatePossibleCategory();
        }

        /// <summary>
        ///  Populates the <see cref="CDP4CommonView.SimpleParameterValueDialogViewModel.PossibleParameterType"/> property
        /// </summary>
        protected override void PopulatePossibleParameterType()
        {
            base.PopulatePossibleParameterType();
            var rdl = (ReferenceDataLibrary)this.Container;
            if (rdl == null)
            {
                return;
            }

            var possiblePt = rdl.ParameterType.ToList();
            possiblePt.AddRange(rdl.GetRequiredRdls().SelectMany(x => x.ParameterType));

            this.PossibleParameterType.AddRange(possiblePt.OrderBy(p => p.ShortName).ThenBy(p => p.Name));
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.SimpleParameterValueDialogViewModel.PossibleScale"/> property
        /// </summary>
        protected override void PopulatePossibleScale()
        {
            base.PopulatePossibleScale();

            var quantityKind = this.SelectedParameterType as QuantityKind;
            if (quantityKind == null)
            {
                return;
            }

            foreach (var scale in quantityKind.AllPossibleScale)
            {
                this.PossibleScale.Add(scale);
            }

            if (this.SelectedScale == null)
            {
                this.SelectedScale = this.PossibleScale.FirstOrDefault();
            }
        }

        /// <summary>
        /// populate the possible <see cref="Category"/>
        /// </summary>
        private void PopulatePossibleCategory()
        {
            this.PossibleCategory.Clear();
            var container = this.Container as ReferenceDataLibrary;
            if (container == null)
            {
                return;
            }

            var allowedCategories = new List<Category>(container.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));
            allowedCategories.AddRange(container.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                        .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Repopulates the <see cref="Value"/> property according to the SelectedParameterType
        /// </summary>
        protected override void PopulateValue()
        {
            base.PopulateValue();

            if (this.Value.Any() || this.SelectedParameterType == null)
            {
                return;
            }

            for (var i = 0; i < this.SelectedParameterType.NumberOfValues; i++)
            {
                this.Value.Add(new PrimitiveRow<string> { Index = i, Value = string.Empty });
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedParameterType != null && this.Value.Any() && !this.Value.Any(x => string.IsNullOrEmpty(x.Value));
        }
    }
}
