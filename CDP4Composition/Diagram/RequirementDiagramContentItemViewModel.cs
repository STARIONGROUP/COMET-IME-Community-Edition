// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementDiagramContentItemViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Diagram
{
    using System.Linq;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents an <see cref="Requirement"/> to be used in a Diagram
    /// </summary>
    public class RequirementDiagramContentItemViewModel : NamedThingDiagramContentItemViewModel //, IDiagramContentItemChildren
    {
        /// <summary>
        /// Backing field for <see cref="IsDefinitionCollapsed"/>
        /// </summary>
        private bool isDefinitionCollapsed;

        /// <summary>
        /// Backing field for <see cref="IsSimpleParameterValueGroupCollapsed"/>
        /// </summary>
        private bool isSimpleParameterValueGroupCollapsed;

        /// <summary>
        /// Backing field for <see cref="IsParametricConstraintGroupCollapsed"/>
        /// </summary>
        private bool isParametricConstraintGroupCollapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDiagramContentItemViewModel"/> class.
        /// </summary>
        /// <param name="diagramThing">The diagram thing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public RequirementDiagramContentItemViewModel(DiagramObject diagramThing, ISession session, IDiagramEditorViewModel container)
            : base(diagramThing, session, container)
        {
            this.session = session;

            if (diagramThing.DepictedThing is Requirement requirement)
            {
                this.DropTarget = new RequirementDropTarget(requirement, this.session);
            }

            this.UpdateProperties();

            this.IsDefinitionCollapsed = true;
            this.IsSimpleParameterValueGroupCollapsed = true;
            this.IsParametricConstraintGroupCollapsed = true;
        }

        /// <summary>
        /// Gets or sets the definition child of the <see cref="RequirementDiagramContentItemViewModel"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> DefinitionChildren { get; set; } = new();

        /// <summary>
        /// Gets or sets the SimpleParameterValue children of the <see cref="RequirementDiagramContentItemViewModel"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> SimpleParameterValueChildren { get; set; } = new();

        /// <summary>
        /// Gets or sets the ParametricConstraint children of the <see cref="RequirementDiagramContentItemViewModel"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> ParametricConstraintChildren { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the definition group is collapsed
        /// </summary>
        public bool IsDefinitionCollapsed
        {
            get => this.isDefinitionCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isDefinitionCollapsed, value);
        }

        /// <summary>
        /// Gets or sets whether the SimpleParameterValue group is collapsed
        /// </summary>
        public bool IsSimpleParameterValueGroupCollapsed
        {
            get => this.isSimpleParameterValueGroupCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isSimpleParameterValueGroupCollapsed, value);
        }

        /// <summary>
        /// Gets or sets whether the definition group is collapsed
        /// </summary>
        public bool IsParametricConstraintGroupCollapsed
        {
            get => this.isParametricConstraintGroupCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isParametricConstraintGroupCollapsed, value);
        }

        /// <summary>
        /// Sets <see cref="RequirementDiagramContentItemViewModel.Thing"/> related properties
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is Requirement requirement)
            {
                this.DefinitionChildren.Clear();
                this.SimpleParameterValueChildren.Clear();
                this.ParametricConstraintChildren.Clear();

                var definition = requirement.Definition.FirstOrDefault();

                if (definition != null)
                {
                    this.DefinitionChildren.Add(new DiagramContentItemDefinitionRowViewModel(definition, this.session, null));
                }

                foreach (var parameter in requirement.ParameterValue.OrderBy(x => x.ParameterType.Name))
                {
                    var parameterRowViewModel = new DiagramContentItemSimpleParameterValueRowViewModel(parameter, this.session, null);
                    this.SimpleParameterValueChildren.Add(parameterRowViewModel);
                }

                foreach (var constraint in requirement.ParametricConstraint.ToList())
                {
                    var parameterRowViewModel = new DiagramContentItemParametricConstraintRowViewModel(constraint, this.session, null);
                    this.ParametricConstraintChildren.Add(parameterRowViewModel);
                }
            }
        }

        ///// <summary>
        ///// Gets or sets the Children of the <see cref="RequirementDiagramContentItemViewModel"/>
        ///// </summary>
        //public ReactiveList<IDiagramContentItemChild> DiagramContentItemChildren { get; set; } = new();

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
    }
}
