// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeDialogViewModel.cs" company="RHEA System S.A.">
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

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="CompoundParameterTypeDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="CompoundParameterType"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.CompoundParameterType)]
    public class CompoundParameterTypeDialogViewModel : CDP4CommonView.CompoundParameterTypeDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public CompoundParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <param name="compoundParameterType">The <see cref="CompoundParameterType"/></param>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the log of recorded changes</param>
        /// <param name="session">The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated</param>
        /// <param name="isRoot">Assert if this <see cref="BooleanParameterTypeDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/></param>
        /// <param name="dialogKind">The kind of operation this <see cref="BooleanParameterTypeDialogViewModel"/> performs</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="container">The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog</param>
        /// <param name="chainOfContainers">The optional chain of containers that contains the <paramref name="container"/> argument</param>
        /// <exception cref="ArgumentException">
        /// The container must be of type <see cref="ReferenceDataLibrary"/>.
        /// </exception>
        public CompoundParameterTypeDialogViewModel(CompoundParameterType compoundParameterType, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(compoundParameterType, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets the possible <see cref="ParameterType"/> to be used for the components
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterType { get; private set; }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        public void UpdateOkCanExecuteStatus()
        {
            this.UpdateOkCanExecute();
        }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleParameterType = new ReactiveList<ParameterType>();
        }

        /// <summary>
        /// Initializes the commands and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateComponentCommand = ReactiveCommandCreator.Create(this.ExecuteCreateComponent, this.WhenAnyValue(x => x.IsReadOnly).Select(x => !x));

            this.DeleteComponentCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteComponent, this.WhenAnyValue(x => x.IsReadOnly).Select(x => !x));

            this.WhenAnyValue(x => x.Container).Subscribe(_ =>
            {
                this.PopulateParameterType();
                this.UpdateOkCanExecute();
            });

            this.Component.CountChanged.Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Populate the components
        /// </summary>
        protected override void PopulateComponent()
        {
            this.Component.Clear();
            foreach (ParameterTypeComponent thing in this.Thing.Component.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterTypeComponentRowViewModel(thing, this.Session, this);
                row.Index = this.Thing.Component.IndexOf(thing);
                this.Component.Add(row);
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        /// <remarks>
        /// Besides the <see cref="CompoundParameterType"/> properties, the <see cref="ParameterTypeComponent"/> are also included here
        /// </remarks>
        protected override void UpdateTransaction()
        {
            foreach (var component in this.Component)
            {
                // a new component was added in this scope
                if (component.Thing.Iid == Guid.Empty)
                {
                    this.Thing.Component.Add(component.Thing);
                    component.Thing.ShortName = component.ShortName;
                    component.Thing.ParameterType = component.ParameterType;
                    component.Thing.Scale = component.Scale;
                    this.transaction.Create(component.Thing);
                }
                else if (component.Thing.ParameterType != component.ParameterType ||
                         component.Thing.Scale != component.Scale ||
                         component.Thing.ShortName != component.ShortName)
                {
                    var clone = component.Thing.Clone(false);
                    clone.ShortName = component.ShortName;
                    clone.ParameterType = component.ParameterType;
                    clone.Scale = component.Scale;
                    this.transaction.CreateOrUpdate(clone);
                }
            }

            var deletedComponents =
                this.Thing.Component.Where(x => !this.Component.Select(y => y.Thing.Iid).Contains(x.Iid)).ToList();
            foreach (var deletedComponent in deletedComponents)
            {
                var cloneToDelete = deletedComponent.Clone(false);
                this.transaction.Delete(cloneToDelete, this.Thing);
                // remove from the ContainerList in the case of an existing thing
            }

            // this is called at the end so that any component order change may be taken into account after it was added/removed
            base.UpdateTransaction();
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            var rdl = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdl.GetRequiredRdls()) {rdl};

            var parametertypes = rdls.SelectMany(x => x.ParameterType)
                .OfType<ScalarParameterType>()
                .OrderBy(x => x.Name).ToList();

            var isAllComponentInRdl = this.Component.Select(x => x.ParameterType).All(parametertypes.Contains);
            var isAllComponentValidated = this.Component.All(component => component.ParameterType != null && (!(component.ParameterType is QuantityKind) || component.Scale != null) && !component.HasError);

            this.OkCanExecute = this.OkCanExecute && this.Component.Count > 0 && isAllComponentInRdl && isAllComponentValidated;
        }

        /// <summary>
        /// Populate the possible <see cref="ScalarParameterType"/> to be used as <see cref="ParameterTypeComponent"/>
        /// </summary>
        private void PopulateParameterType()
        {
            this.PossibleParameterType.Clear();
            var rdl = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdl.GetRequiredRdls()) {rdl};

            var parametertypes = rdls.SelectMany(x => x.ParameterType)
                .OfType<ScalarParameterType>()
                .OrderBy(x => x.Name).ToList();

            this.PossibleParameterType.AddRange(parametertypes);
        }

        /// <summary>
        /// Execute the <see cref="CreateComponentCommand"/>
        /// </summary>
        protected void ExecuteCreateComponent()
        {
            var component = new ParameterTypeComponent();
            
            var row = new ParameterTypeComponentRowViewModel(component, this.Session, this);

            this.Component.Add(row);
        }

        /// <summary>
        /// Execute the <see cref="DeleteComponentCommand"/>
        /// </summary>
        protected void ExecuteDeleteComponent()
        {
            this.Component.Remove(this.SelectedComponent);
        }
    }
}
