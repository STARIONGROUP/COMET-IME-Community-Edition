// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramContentItemParameterRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Diagram
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="Parameter"/>
    /// </summary>
    public class DiagramContentItemParameterRowViewModel : CDP4CommonView.ParameterRowViewModel, IDiagramContentItemChild
    {
        /// <summary>
        /// Backing field for <see cref="DiagramContentItemChildString"/>
        /// </summary>
        private string diagramContentItemString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramContentItemParameterRowViewModel"/> class
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DiagramContentItemParameterRowViewModel(Parameter parameter, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameter, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The textual representation of this <see cref="DiagramContentItemParameterRowViewModel"/>
        /// </summary>
        public string DiagramContentItemChildString
        {
            get => this.diagramContentItemString;
            private set => this.RaiseAndSetIfChanged(ref this.diagramContentItemString, value);
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateDiagramContentItemString();
        }

        /// <summary>
        /// Updates the <see cref="DiagramContentItemChildString"/> property
        /// </summary>
        private void UpdateDiagramContentItemString()
        {
            var stringBuilder = new StringBuilder();

            this.AddParameterString(this.Thing, stringBuilder);

            this.DiagramContentItemChildString = stringBuilder.ToString();
        }

        /// <summary>
        /// Adds a <see cref="string"/> representation of a <see cref="Parameter"/> and values in its <see cref="ParameterValueSet"/>s
        /// to a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/></param>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/></param>
        private void AddParameterString(Parameter parameter, StringBuilder stringBuilder)
        {
            stringBuilder.Append(parameter.ParameterType.Name);
            stringBuilder.Append($" [{parameter.Scale?.ShortName ?? "-"}] [{parameter.Owner?.ShortName ?? "-"}]");

            foreach (var valueset in parameter.ValueSet.OrderBy(x => x.ActualOption?.Name).ThenBy(x => x.ActualState?.Name))
            {
                if (parameter.IsOptionDependent || this.Thing.StateDependence != null)
                {
                    stringBuilder.AppendLine();
                }

                if (parameter.IsOptionDependent)
                {
                    stringBuilder.Append(valueset.ActualOption.Name);
                }

                if (parameter.StateDependence != null)
                {
                    stringBuilder.Append(valueset.ActualState.Name);
                }

                if (parameter.ParameterType is CompoundParameterType compoundParameterType)
                {
                    foreach (var component in compoundParameterType.Component.ToList())
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.Append("  - ");
                        stringBuilder.Append(component.ShortName);
                        stringBuilder.Append($" [{component.Scale?.ShortName ?? "-"}]");
                        stringBuilder.Append(" = ");
                        stringBuilder.Append(valueset.Published[component.Index]);
                    }
                }
                else
                {
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(valueset.Published[0]);
                }
            }
        }
    }

    /// <summary>
    /// Row class representing a <see cref="SimpleParameterValue"/>
    /// </summary>
    public class DiagramContentItemSimpleParameterValueRowViewModel : CDP4CommonView.SimpleParameterValueRowViewModel, IDiagramContentItemChild
    {
        /// <summary>
        /// Backing field for <see cref="DiagramContentItemChildString"/>
        /// </summary>
        private string diagramContentItemString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramContentItemSimpleParameterValueRowViewModel"/> class
        /// </summary>
        /// <param name="parameter">The <see cref="SimpleParameterValue"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DiagramContentItemSimpleParameterValueRowViewModel(SimpleParameterValue parameter, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameter, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The textual representation of this <see cref="DiagramContentItemParameterRowViewModel"/>
        /// </summary>
        public string DiagramContentItemChildString
        {
            get => this.diagramContentItemString;
            private set => this.RaiseAndSetIfChanged(ref this.diagramContentItemString, value);
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateDiagramContentItemString();
        }

        /// <summary>
        /// Updates the <see cref="DiagramContentItemChildString"/> property
        /// </summary>
        private void UpdateDiagramContentItemString()
        {
            var stringBuilder = new StringBuilder();

            this.AddParameterString(this.Thing, stringBuilder);

            this.DiagramContentItemChildString = stringBuilder.ToString();
        }

        /// <summary>
        /// Adds a <see cref="string"/> representation of a <see cref="Parameter"/> and values in its <see cref="ParameterValueSet"/>s
        /// to a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="parameter">The <see cref="SimpleParameterValue"/></param>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/></param>
        private void AddParameterString(SimpleParameterValue parameter, StringBuilder stringBuilder)
        {
            stringBuilder.Append(parameter.ParameterType.Name);
            stringBuilder.Append($" [{parameter.Scale?.ShortName ?? "-"}] [{parameter.Owner?.ShortName ?? "-"}]");

            if (parameter.ParameterType is CompoundParameterType compoundParameterType)
            {
                foreach (var component in compoundParameterType.Component.ToList())
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("  - ");
                    stringBuilder.Append(component.ShortName);
                    stringBuilder.Append($" [{component.Scale?.ShortName ?? "-"}]");
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(parameter.Value[component.Index]);
                }
            }
            else
            {
                stringBuilder.Append(" = ");
                stringBuilder.Append(parameter.Value[0]);
            }
        }
    }

    /// <summary>
    /// Row class representing a <see cref="SimpleParameterValue"/>
    /// </summary>
    public class DiagramContentItemParametricConstraintRowViewModel : CDP4CommonView.ParametricConstraintRowViewModel, IDiagramContentItemChild
    {
        /// <summary>
        /// Backing field for <see cref="DiagramContentItemChildString"/>
        /// </summary>
        private string diagramContentItemString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramContentItemParametricConstraintRowViewModel"/> class
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DiagramContentItemParametricConstraintRowViewModel(ParametricConstraint constraint, ISession session, IViewModelBase<Thing> containerViewModel) : base(constraint, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The textual representation of this <see cref="DiagramContentItemParametricConstraintRowViewModel"/>
        /// </summary>
        public string DiagramContentItemChildString
        {
            get => this.diagramContentItemString;
            private set => this.RaiseAndSetIfChanged(ref this.diagramContentItemString, value);
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateDiagramContentItemString();
        }

        /// <summary>
        /// Updates the <see cref="DiagramContentItemChildString"/> property
        /// </summary>
        private void UpdateDiagramContentItemString()
        {
            var stringBuilder = new StringBuilder();

            this.AddConstraintString(this.Thing, stringBuilder);
        }

        /// <summary>
        /// Adds a <see cref="string"/> representation of a <see cref="ParametricConstraint"/>
        /// to a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint"/></param>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/></param>
        private void AddConstraintString(ParametricConstraint constraint, StringBuilder stringBuilder)
        {
            // TODO: add sdk extension method for constructing the expression
            this.DiagramContentItemChildString = "N/A";
        }
    }

    /// <summary>
    /// Row class representing a <see cref="Definition"/>
    /// </summary>
    public class DiagramContentItemDefinitionRowViewModel : CDP4CommonView.DefinitionRowViewModel, IDiagramContentItemChild
    {
        /// <summary>
        /// Backing field for <see cref="DiagramContentItemChildString"/>
        /// </summary>
        private string diagramContentItemString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionRowViewModel"/> class
        /// </summary>
        /// <param name="definition">The <see cref="Definition"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DiagramContentItemDefinitionRowViewModel(Definition definition, ISession session, IViewModelBase<Thing> containerViewModel) : base(definition, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The textual representation of this <see cref="DiagramContentItemDefinitionRowViewModel"/>
        /// </summary>
        public string DiagramContentItemChildString
        {
            get => this.diagramContentItemString;
            private set => this.RaiseAndSetIfChanged(ref this.diagramContentItemString, value);
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateDiagramContentItemString();
        }

        /// <summary>
        /// Updates the <see cref="DiagramContentItemChildString"/> property
        /// </summary>
        private void UpdateDiagramContentItemString()
        {
            this.DiagramContentItemChildString = this.Thing.Content;
        }
    }
}
