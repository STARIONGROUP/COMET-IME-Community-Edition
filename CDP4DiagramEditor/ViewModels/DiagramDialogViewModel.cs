// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DiagramCanvasDialogViewModel" /> is to allow an <see cref="DiagramCanvas" /> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="DiagramCanvas" /> will result in an <see cref="DiagramCanvas" /> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.DiagramCanvas)]
    public class DiagramCanvasDialogViewModel : CDP4CommonView.DiagramCanvasDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="LockedBy"/>
        /// </summary>
        private string lockedBy;

        /// <summary>
        /// Backing field for <see cref="IsLocked"/>
        /// </summary>
        private bool isLocked;

        /// <summary>
        /// The currently known <see cref="Person"/>
        /// </summary>
        private Person currentPerson;

        /// <summary>
        /// Allow editting of this <see cref="File"/>
        /// </summary>
        private bool allowEdit;

        /// <summary>
        /// Backing field for <see cref="Description" />
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="IsHidden" />
        /// </summary>
        private bool isHidden;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramCanvasDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DiagramCanvasDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramCanvasDialogViewModel" /> class.
        /// </summary>
        /// <param name="diagram">
        /// The <see cref="DiagramCanvas" /> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction" /> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Thing" /> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DiagramCanvasDialogViewModel" /> is the root of all <see cref="IThingDialogViewModel" />
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DiagramCanvasDialogViewModel" /> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService" />
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing" /> of the created <see cref="Thing" />
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container" /> argument
        /// </param>
        public DiagramCanvasDialogViewModel(DiagramCanvas diagram, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(diagram, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(
                vm => vm.IsLocked).Subscribe(
                x =>
                {
                    if (!this.IsLockedByAnotherPerson())
                    {
                        this.SelectedLockedBy = x ? this.currentPerson : null;
                    }
                });

            this.WhenAnyValue(
                vm => vm.SelectedLockedBy).Subscribe(
                x =>
                {
                    this.LockedBy = x?.Name;
                    this.AllowEdit = !this.IsLockedByAnotherPerson();
                });

            this.WhenAnyValue(
                    vm => vm.SelectedLockedBy)
                .Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Gets or sets the IsLocked property
        /// </summary>
        public bool AllowEdit
        {
            get { return this.allowEdit; }
            set { this.RaiseAndSetIfChanged(ref this.allowEdit, value); }
        }

        /// <summary>
        /// Gets or sets the IsLocked property
        /// </summary>
        public bool IsLocked
        {
            get { return this.isLocked; }
            set { this.RaiseAndSetIfChanged(ref this.isLocked, value); }
        }

        /// <summary>
        /// Gets or sets the LockedBy property
        /// </summary>
        public string LockedBy
        {
            get { return this.lockedBy; }
            private set { this.RaiseAndSetIfChanged(ref this.lockedBy, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets or sets the IsHidden state
        /// </summary>
        public bool IsHidden
        {
            get { return this.isHidden; }
            set { this.RaiseAndSetIfChanged(ref this.isHidden, value); }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Description = this.Thing.Description;
            this.IsHidden = this.Thing.IsHidden;

            this.currentPerson = null;

            var iteration = this.Container.GetContainerOfType<Iteration>();

            if (iteration == null)
            {
                this.currentPerson = this.Session.OpenIterations.FirstOrDefault().Value.Item2.Person;
            }
            else
            {
                if (this.Session.OpenIterations.TryGetValue(iteration, out var tuple))
                {
                    this.currentPerson = tuple?.Item2.Person;
                }
            }

            this.IsLocked = this.SelectedLockedBy != null;
            this.LockedBy = this.SelectedLockedBy?.Name;
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.Thing.CreatedOn = DateTime.UtcNow;
            }

            var clone = this.Thing;

            clone.Description = this.Description;
            clone.IsHidden = this.IsHidden;
        }

        /// <summary>
        /// Check if this <see cref="DiagramCanvas"/> is Locked by another <see cref="Person"/>
        /// </summary>
        /// <returns>true is <see cref="DiagramCanvas"/> is locked by another <see cref="Person"/>. Otherwise false</returns>
        private bool IsLockedByAnotherPerson()
        {
            if (!this.IsLocked)
            {
                return false;
            }

            if (this.currentPerson == null)
            {
                return true;
            }

            return this.SelectedLockedBy != null && this.SelectedLockedBy != this.currentPerson;
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();

            this.OkCanExecute = this.OkCanExecute
                                && !this.IsLockedByAnotherPerson();
        }
    }
}
