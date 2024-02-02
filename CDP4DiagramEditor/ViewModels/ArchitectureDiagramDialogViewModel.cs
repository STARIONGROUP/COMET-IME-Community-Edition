// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArchitectureDiagramDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ArchitectureDiagramDialogViewModel" /> is to allow an <see cref="ArchitectureDiagram" /> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="ArchitectureDiagram" /> will result in an <see cref="ArchitectureDiagram" /> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.ArchitectureDiagram)]
    public class ArchitectureDiagramDialogViewModel : DiagramCanvasDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchitectureDiagramDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ArchitectureDiagramDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchitectureDiagramDialogViewModel" /> class.
        /// </summary>
        /// <param name="diagram">
        /// The <see cref="ArchitectureDiagram" /> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction" /> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Thing" /> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="ArchitectureDiagramDialogViewModel" /> is the root of all <see cref="IThingDialogViewModel" />
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="ArchitectureDiagramDialogViewModel" /> performs
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
        public ArchitectureDiagramDialogViewModel(ArchitectureDiagram diagram, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(diagram, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the SelectedOwner
        /// </summary>
        public virtual DomainOfExpertise SelectedOwner
        {
            get => this.selectedOwner;
            set => this.RaiseAndSetIfChanged(ref this.selectedOwner, value);
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleOwner { get; protected set; }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedOwner = ((ArchitectureDiagram)this.Thing)?.Owner;
            this.PopulatePossibleOwner();
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            if (!(this.Thing is ArchitectureDiagram clone))
            {
                throw new InvalidOperationException("The Thing represented by this Dialog is not of the right type.");
            }

            clone.Owner = this.SelectedOwner;
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/>
        /// </summary>
        protected void PopulatePossibleOwner()
        {
            this.PossibleOwner.Clear();

            var model = this.Container.Container as EngineeringModel;

            if (model == null)
            {
                throw new InvalidOperationException("The top container is not set for this diagram");
            }

            this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name));
        }
    }
}
