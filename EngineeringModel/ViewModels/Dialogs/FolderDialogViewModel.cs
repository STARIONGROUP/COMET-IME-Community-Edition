// -------------------------------------------------------------------------------------------------
// <copyright file="FolderDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="FolderDialogViewModel"/> is to allow a <see cref="Folder"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Folder"/> will result in an <see cref="Folder"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Folder)]
    public class FolderDialogViewModel : CDP4CommonView.FolderDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public FolderDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderDialogViewModel"/> class.
        /// </summary>
        /// <param name="folder">
        /// The <see cref="Folder"/> that is the subject of the current view-model. This is the object
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
        /// Assert if this <see cref="FolderDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="FolderDialogViewModel"/> performs
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
        public FolderDialogViewModel(Folder folder, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(folder, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
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
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            this.CreatedOn = this.Thing.CreatedOn.Equals(DateTime.MinValue) ? DateTime.UtcNow : this.Thing.CreatedOn;

            if (this.SelectedCreator == null)
            {
                var iteration = this.Container.GetContainerOfType<Iteration>();

                if (iteration != null)
                {
                    this.Session.OpenIterations.TryGetValue(iteration, out var tuple);
                    this.SelectedCreator = tuple?.Item2;
                }
            }
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.Name).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null && !string.IsNullOrWhiteSpace(this.Name);
        }
    }
}
