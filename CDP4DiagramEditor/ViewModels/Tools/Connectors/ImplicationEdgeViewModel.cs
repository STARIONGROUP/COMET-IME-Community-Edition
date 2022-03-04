// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplicationEdgeViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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
    using System.Linq;
    using System.Text;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4DiagramEditor.Helpers;

    using ReactiveUI;

    /// <summary>
    /// View model for a Implication diagram edge
    /// </summary>
    public class ImplicationEdgeViewModel : DrawnDiagramEdgeViewModel, IPersistedConnector
    {
        /// <summary>
        /// Backing field for <see cref="ImplicationKind"/>
        /// </summary>
        private ImplicationKind? implicationKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicationEdgeViewModel" /> class
        /// </summary>
        /// <param name="diagramEdge">The associated <see cref="DiagramEdge" /></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">The container <see cref="IDiagramEditorViewModel"/></param>
        public ImplicationEdgeViewModel(DiagramEdge diagramEdge, ISession session, IDiagramEditorViewModel container) : base(diagramEdge, session, container)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the constraint type
        /// </summary>
        public ImplicationKind? ImplicationKind
        {
            get { return this.implicationKind; }
            set { this.RaiseAndSetIfChanged(ref this.implicationKind, value); }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> event-handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is BinaryRelationship relationship)
            {
                this.ImplicationKind = DiagramRDLHelper.GetImplicationKind(relationship);
            }
            else
            {
                this.ImplicationKind = null;
            }

            var categories = (this.Thing as BinaryRelationship)?.Category;

            var sb = new StringBuilder();

            if (categories != null && categories.Any())
            {
                sb.AppendLine($"({string.Join(", ", categories.Select(c => $"\"{c.Name}\""))})");
            }

            sb.AppendLine(this.GetImplicationText());

            this.DisplayedText = sb.ToString().Trim();
        }

        /// <summary>
        /// Gets the implication text
        /// </summary>
        /// <returns>The string with the implication</returns>
        private string GetImplicationText()
        {
            var relationship = this.Thing as BinaryRelationship;

            if (relationship == null)
            {
                return string.Empty;
            }

            var a = relationship.Source?.UserFriendlyName;

            var b = relationship.Target?.UserFriendlyName;

            switch (this.ImplicationKind)
            {
                case Helpers.ImplicationKind.AImpliesB:
                    return $"{a} implies {b}";
                case Helpers.ImplicationKind.AImpliesNotB:
                    return $"{a} implies not {b}";
                case Helpers.ImplicationKind.NotAImpliesB:
                    return $"not {a} implies {b}";
                case Helpers.ImplicationKind.NotAImpliesNotB:
                    return $"not {a} implies not {b}";
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return string.Empty;

        }
    }
}
