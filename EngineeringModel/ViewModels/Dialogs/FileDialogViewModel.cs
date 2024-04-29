// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
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

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="FileDialogViewModel"/> is to allow a <see cref="File"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="File"/> will result in an <see cref="File"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.File)]
    public class FileDialogViewModel : CDP4CommonView.FileDialogViewModel, IThingDialogViewModel
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
        /// The currently active FileRevision
        /// </summary>
        private FileRevision currentFileRevision;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public FileDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDialogViewModel"/> class.
        /// </summary>
        /// <param name="file">
        /// The <see cref="File"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name=""></param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="FileDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="FileDialogViewModel"/> performs
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
        public FileDialogViewModel(File file, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, 
            IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(file, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.PopulatePossibleCategories();

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
                    vm => vm.SelectedOwner,
                    vm => vm.SelectedLockedBy,
                    vm => vm.FileRevision)
                .Subscribe(_ => this.UpdateOkCanExecute());

            this.Disposables.Add(this.FileRevision.Changed.Subscribe(_ => this.UpdateOkCanExecute()));
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
        /// Gets or sets the CurrentFileRevision property
        /// </summary>
        public FileRevision CurrentFileRevision
        {
            get { return this.currentFileRevision; }
            private set { this.RaiseAndSetIfChanged(ref this.currentFileRevision, value); }
        }

        /// <summary>
        /// Populate the possible <see cref="Category"/> for this <see cref="File"/>
        /// </summary>
        private void PopulatePossibleCategories()
        {
            this.PossibleCategory.Clear();

            var model = this.Container.GetContainerOfType<EngineeringModel>();

            if (model == null)
            {
                throw new InvalidOperationException("The container is not set for this File.");
            }

            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedCategories = new List<Category>(mrdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            allowedCategories.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populate the possible owners
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var engineeringModel = this.Container.GetContainerOfType<EngineeringModel>();
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
        }

        /// <summary>
        /// Populates the <see cref="FileRevision"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected override void PopulateFileRevision()
        {
            this.CurrentFileRevision = null;

            var dummyFile = new File();

            var notDeletedCurrentFileRevisions = 
                this.Thing.FileRevision
                    .Where(t => t.ChangeKind != ChangeKind.Delete)
                    .Select(x => x.Clone(false))
                    .ToList();

            if (notDeletedCurrentFileRevisions.Any())
            {
                dummyFile.FileRevision.AddRange(notDeletedCurrentFileRevisions);
                this.CurrentFileRevision = this.Thing.FileRevision.SingleOrDefault(x => x.Iid == dummyFile.CurrentFileRevision.Iid);
            }

            base.PopulateFileRevision();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

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

            this.SelectedOwner = 
                this.SelectedOwner ?? 
                (iteration == null ? null : this.Session.QuerySelectedDomainOfExpertise(iteration));

            this.CurrentFileRevision = this.Thing.CurrentFileRevision;
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Check if this <see cref="File"/> is Locked by another <see cref="Person"/>
        /// </summary>
        /// <returns>true is <see cref="File"/> is locked by another <see cref="Person"/>. Otherwise false</returns>
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
                                && (this.SelectedOwner != null)
                                && !this.IsLockedByAnotherPerson()
                                && this.FileRevision.Any();
        }
    }
}
