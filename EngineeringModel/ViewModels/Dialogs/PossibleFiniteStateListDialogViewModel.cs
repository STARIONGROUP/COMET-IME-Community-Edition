﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListDialogViewModel.cs" company="Starion Group S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using ReactiveUI;

    using CDP4Composition.Services;

    using System.Windows;

    using CommonServiceLocator;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="PossibleFiniteStateList"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.PossibleFiniteStateList)]
    public class PossibleFiniteStateListDialogViewModel : CDP4CommonView.PossibleFiniteStateListDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The <see cref="IMessageBoxService"/> used to show user messages.
        /// </summary>
        private IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PossibleFiniteStateListDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialogViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteStateList">
        /// The <see cref="PossibleFiniteStateList"/> that is the subject of the current view-model. This is the object
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
        /// <param name="messageBoxService">
        /// The <see cref="IMessageBoxService"/>.
        /// </param>
        public PossibleFiniteStateListDialogViewModel(PossibleFiniteStateList possibleFiniteStateList, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(possibleFiniteStateList, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        
        /// <summary>
        /// Gets the <see cref="ICommand"/> to set the default <see cref="PossibleFiniteState"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetDefaultStateCommand { get; private set; } 

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.PossibleState.CountChanged.Subscribe(_ => this.UpdateOkCanExecute());

            this.SetDefaultStateCommand = ReactiveCommandCreator.Create(
                this.ExecuteSetDefaultCommand,
                this.WhenAnyValue(x => x.SelectedPossibleState).Select(x => x != null && !this.IsReadOnly));
        }

        /// <summary>
        /// Populate the <see cref="PossibleCategory"/> and <see cref="Category"/>
        /// </summary>
        protected override void PopulateCategory()
        {
            // populate the possible categories
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;

            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var possibleRdls = mrdl.GetRequiredRdls().ToList();
            possibleRdls.Add(mrdl);

            var categories = possibleRdls.SelectMany(x => x.DefinedCategory.Where(cat => cat.PermissibleClass.Contains(ClassKind.PossibleFiniteStateList))).OrderBy(x => x.Name).ToList();
            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories);

            base.PopulateCategory();
        }

        /// <summary>
        /// Populate the possible owners
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;
            this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain.OrderBy(d => d.Name));
        }

        /// <summary>
        /// Populate the possible <see cref="PossibleFiniteState"/> rows
        /// </summary>
        protected override void PopulatePossibleState()
        {
            this.PossibleState.Clear();

            var defaultStateIid = (this.Thing.DefaultState == null) ? Guid.Empty : this.Thing.DefaultState.Iid;
            foreach (PossibleFiniteState thing in this.Thing.PossibleState.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PossibleFiniteStateRowViewModel(thing, this.Session, this);
                this.PossibleState.Add(row);
                row.Index = this.Thing.PossibleState.IndexOf(thing);
                row.IsDefault = thing.Iid == defaultStateIid;
            }

            this.SelectedDefaultState = this.PossibleState.Select(s => s.Thing).SingleOrDefault(x => x.Iid == defaultStateIid);
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null && this.PossibleState.Count > 0;
        }

        /// <summary>
        /// Checks if the Delete command is Allowed.
        /// </summary>
        /// <returns>If the Delete command is Allowed or not. Default true.</returns>
        protected override bool IsExecuteDeleteCommandAllowed()
        {
            var iteration = this.Container as Iteration;
            var iterationElements = iteration.Element;

            var possibleFiniteStateToDelete = this.SelectedPossibleState.Thing;

            var ParametersWithStateDependencies = 0;
            ParametersWithStateDependencies = iterationElements.SelectMany(elem =>
                          elem.Parameter.Where(param =>
                          param.StateDependence != null).SelectMany(param =>
                          param.StateDependence.PossibleFiniteStateList.SelectMany(finiteStateList =>
                          finiteStateList.PossibleState.Where(finiteState =>
                          finiteState == possibleFiniteStateToDelete)))).Count();

            if(ParametersWithStateDependencies > 0)
            {
                var message = $"Deleting a {nameof(PossibleFiniteState)} will delete ALL parameter values that may be dependent on this through {nameof(ActualFiniteStateList)} that use it." +
                     "\r\n\r\nCare should be taken not to delete states and their dependent parameter values in the product tree inadvertently." +
                     "\r\n\r\nAre you sure you want to delete these?"+
                     $"\r\n\r\n{ParametersWithStateDependencies} parameter(s) will be affected by this deletion";

                if (this.messageBoxService.Show(message, "Deleting Finite State", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    return true;
                }
                return false;
            }

            return true;
        }


        /// <summary>
        /// Executes the <see cref="SetDefaultStateCommand"/>
        /// </summary>
        private void ExecuteSetDefaultCommand()
        {
            foreach (PossibleFiniteStateRowViewModel row in this.PossibleState)
            {
                row.IsDefault = false;
            }

            ((PossibleFiniteStateRowViewModel)this.SelectedPossibleState).IsDefault = true;
            this.SelectedDefaultState = this.SelectedPossibleState.Thing;
        }
    }
}