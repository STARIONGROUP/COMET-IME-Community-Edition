// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramFrameViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Diagram
{
    using System;
    using System.Linq;

    using CDP4Common.DiagramData;

    using CDP4CommonView.Diagram.Views;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// Represents an <see cref="DiagramFrame"/> to be used in a Diagram
    /// </summary>
    public class DiagramFrameViewModel : NamedThingDiagramContentItemViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private double height;

        /// <summary>
        /// Backing field for <see cref="Width"/>
        /// </summary>
        private double width;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramFrameViewModel"/> class.
        /// </summary>
        /// <param name="diagramThing">The diagram thing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public DiagramFrameViewModel(DiagramFrame diagramThing, ISession session, IDiagramEditorViewModel container)
            : base(diagramThing, session, container)
        {
            this.session = session;

            this.Disposables.Add(this.WhenAnyValue(vm => vm.DisplayText).Subscribe(_ => this.SetDirty()));

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public double Height
        {
            get => this.height;
            protected set => this.RaiseAndSetIfChanged(ref this.height, value);
        }

        /// <summary>
        /// Gets or sets the Width
        /// </summary>
        public double Width
        {
            get => this.width;
            protected set => this.RaiseAndSetIfChanged(ref this.width, value);
        }

        /// <summary>
        /// Sets <see cref="RequirementDiagramContentItemViewModel.Thing"/> related properties
        /// </summary>
        private void UpdateProperties()
        {
            this.DisplayText = this.DiagramThing.Name;
            this.Height = this.DiagramThing.Bounds.First().Height;
            this.Width = this.DiagramThing.Bounds.First().Width;
        }

        /// <summary>
        /// Reinitializes the viewmodel with the Thing from cache
        /// </summary>
        public override void Reinitialize()
        {
            base.Reinitialize();
            this.UpdateProperties();
        }

        /// <summary>
        /// Sets the <see cref="IsDirty"/> property
        /// </summary>
        public override void SetDirty()
        {
            var bound = this.DiagramThing.Bounds.Single();

            this.IsDirty = this.DiagramRepresentation is DiagramFrameShape parent
                           && parent.Position != default
                           && ((float)parent.Position.X != bound.X
                               || (float)parent.Position.Y != bound.Y
                               || parent.ActualHeight != bound.Height
                               || parent.ActualWidth != bound.Width
                               || this.DisplayText != this.DiagramThing.Name);

            this.containerViewModel?.UpdateIsDirty();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the transaction with the data contained in this view-model
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        public override void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container)
        {
            var clone = this.DiagramThing.Clone(true);
            var bound = clone.Bounds.SingleOrDefault();

            if (bound != null)
            {
                this.UpdateBound(bound);
            }

            clone.Name = this.DisplayText;

            transaction.CreateOrUpdate(bound);

            container.DiagramElement.Add(clone);
            transaction.CreateOrUpdate(clone);
        }

        /// <summary>
        /// Update a <see cref="Bounds"/> with the current values
        /// </summary>
        /// <param name="bound">The <see cref="Bounds"/> to update</param>
        private void UpdateBound(Bounds bound)
        {
            if (this.DiagramRepresentation is DiagramFrameShape parent)
            {
                bound.Height = (float)parent.ActualHeight;
                bound.Width = (float)parent.ActualWidth;
                bound.X = (float)parent.Position.X;
                bound.Y = (float)parent.Position.Y;
                bound.Name = "should not have a name";
            }
        }
    }
}
