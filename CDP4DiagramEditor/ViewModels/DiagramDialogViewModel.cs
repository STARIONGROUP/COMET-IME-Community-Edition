// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;

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
        /// Backing field for <see cref="Description" />
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="PublicationState" />
        /// </summary>
        private PublicationState publicationState;

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
        /// Gets or sets the PublicationState
        /// </summary>
        public PublicationState PublicationState
        {
            get { return this.publicationState; }
            set { this.RaiseAndSetIfChanged(ref this.publicationState, value); }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Description = this.Thing.Description;
            this.PublicationState = this.Thing.PublicationState;
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
            clone.PublicationState = this.PublicationState;
        }
    }
}
