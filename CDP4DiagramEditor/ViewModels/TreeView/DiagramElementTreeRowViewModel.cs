// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramElementTreeRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.TreeView
{
    using System;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Utilities;

    using ReactiveUI;

    /// <summary>
    /// View Model for tree view rows in diagram control
    /// </summary>
    public class DiagramElementTreeRowViewModel : ReactiveObject, IDiagramElementTreeRowViewModel
    {
        /// <summary>
        /// The container view model
        /// </summary>
        private readonly IDiagramEditorViewModel container;

        /// <summary>
        /// Backing field for <see cref="Children" />
        /// </summary>
        private DisposableReactiveList<IDiagramElementTreeRowViewModel> children;

        /// <summary>
        /// Backing field for disposables (mainly listeners)
        /// </summary>
        private DisposableReactiveList<IDisposable> disposables;

        /// <summary>
        /// Backing field of <see cref="IsDirty" />
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Backing field for <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Thing" />
        /// </summary>
        private Thing thing;

        /// <summary>
        /// Backing field for <see cref="ThingDiagramItemViewModel" />
        /// </summary>
        private IDiagramItemOrConnector thingDiagramItemViewModel;

        /// <summary>
        /// Instantiates a new instance of <see cref="DiagramElementTreeRowViewModel" />
        /// </summary>
        public DiagramElementTreeRowViewModel(Thing thing, IDiagramEditorViewModel container, IDiagramItemOrConnector thingDiagramItemViewModel)
        {
            this.container = container;
            this.Disposables = new DisposableReactiveList<IDisposable>();
            this.Children = new DisposableReactiveList<IDiagramElementTreeRowViewModel> { ChangeTrackingEnabled = true };

            this.Thing = thing;
            this.ThingDiagramItemViewModel = thingDiagramItemViewModel;

            this.WhenAnyValue(vm => vm.ThingDiagramItemViewModel.IsDirty).Subscribe(_ =>
            {
                if (this.ThingDiagramItemViewModel is NamedThingDiagramContentItemViewModel)
                {
                    this.IsDirty = this.ThingDiagramItemViewModel.IsDirty;
                }
            });

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets whether this row is dirty.
        /// </summary>
        public bool IsDirty
        {
            get { return this.isDirty; }
            set { this.RaiseAndSetIfChanged(ref this.isDirty, value); }
        }

        /// <summary>
        /// Gets or sets the collection of children diagram item rows from element tree view.
        /// </summary>
        public DisposableReactiveList<IDisposable> Disposables
        {
            get { return this.disposables; }
            set { this.RaiseAndSetIfChanged(ref this.disposables, value); }
        }

        /// <summary>
        /// Gets or sets the represented diagram viewmodel
        /// </summary>
        public IDiagramItemOrConnector ThingDiagramItemViewModel
        {
            get { return this.thingDiagramItemViewModel; }
            set { this.RaiseAndSetIfChanged(ref this.thingDiagramItemViewModel, value); }
        }

        /// <summary>
        /// Gets or sets the represented Thing
        /// </summary>
        public Thing Thing
        {
            get { return this.thing; }
            set { this.RaiseAndSetIfChanged(ref this.thing, value); }
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the collection of children diagram item rows from element tree view.
        /// </summary>
        public DisposableReactiveList<IDiagramElementTreeRowViewModel> Children
        {
            get { return this.children; }
            set { this.RaiseAndSetIfChanged(ref this.children, value); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Children.ClearAndDispose();
            this.Disposables.ClearAndDispose();
        }

        /// <summary>
        /// Updates the properties of this node
        /// </summary>
        private void UpdateProperties()
        {
            this.SetName();
            this.SetChildren();
        }

        /// <summary>
        /// Sets the name
        /// </summary>
        protected virtual void SetName()
        {
            if(this.Thing == null)
            {
                this.Name = "No Thing assigned. Possible model corruption.";
                return;
            }

            switch (this.Thing)
            {
                case DiagramPort port:
                    this.Name = $"<<{(port.DepictedThing as ElementUsage)?.InterfaceEnd} Port>> {(port.DepictedThing as ElementUsage)?.Name} ({(port.DepictedThing as ElementUsage)?.ShortName})";
                    break;
                case DiagramObject doc:
                    this.Name = $"<<{doc.ClassKind}>> {doc.Name} ({doc.DepictedThing?.UserFriendlyShortName})";
                    break;
                case DiagramEdge edge:
                    this.Name = edge.DepictedThing == null ? $"<<SimpleConnector>> {edge.Name}" : $"<<{edge.DepictedThing.ClassKind}>> {edge.DepictedThing.UserFriendlyName} ({edge.DepictedThing.UserFriendlyShortName})";
                    break;
                case Parameter param:
                    this.Name = param.ParameterType?.Name;
                    break;
                case BinaryRelationship br:
                    this.Name = $"<<{br.ClassKind}>> {br.UserFriendlyName} ({br.UserFriendlyShortName})";
                    break;
                default:
                    this.Name = $"<<{this.Thing.ClassKind}>> {(this.Thing as DefinedThing)?.Name ?? "Undefined Object"} ({(this.Thing as DefinedThing)?.ShortName ?? "undefined"})";
                    break;
            }
        }

        /// <summary>
        /// Sets the children nodes
        /// </summary>
        protected virtual void SetChildren()
        {
            this.Children.ClearAndDispose();

            if (this.Thing == null)
            {
                return;
            }

            if (this.Thing is IOwnedThing owned)
            {
                this.Children.Add(new DiagramElementTreeRowViewModel(owned.Owner, this.container, null));
            }

            if (this.Thing is ICategorizableThing categorizableThing)
            {
                foreach (var cat in categorizableThing.Category)
                {
                    this.Children.Add(new DiagramElementTreeRowViewModel(cat, this.container, null));
                }
            }

            switch (this.Thing)
            {
                case DiagramEdge edge:
                    if (edge.DepictedThing != null)
                    {
                        this.Children.Add(new DiagramElementTreeRowViewModel(edge.DepictedThing, this.container, null));
                    }

                    break;
                case DiagramPort port:
                    this.Children.Add(new DiagramElementTreeRowViewModel(port.DepictedThing, this.container, null));
                    break;
                case DiagramObject ae:
                    this.Children.Add(new DiagramElementTreeRowViewModel(ae.DepictedThing, this.container, null));
                    break;
                case ElementDefinition ed:
                    foreach (var parameter in ed.Parameter)
                    {
                        this.Children.Add(new DiagramElementTreeRowViewModel(parameter, this.container, null));
                    }

                    break;
                case Parameter param:
                    if (param.StateDependence != null)
                    {
                        this.Children.Add(new DiagramElementTreeRowViewModel(param.StateDependence, this.container, null));
                    }

                    break;
            }

            // add thing preferences
            var pref = ThingPreferenceHelper.GetThingPreferenceDictionary(this.Thing, out var dictionary);

            if (pref)
            {
                foreach (var kvp in dictionary)
                {
                    this.Children.Add(new PropertyTreeRowViewModel(null, this.container, this.ThingDiagramItemViewModel, $"{kvp.Key} = {kvp.Value}"));
                }
            }
        }
    }

    /// <summary>
    /// A row view model for a property of a thing
    /// </summary>
    public class PropertyTreeRowViewModel : DiagramElementTreeRowViewModel
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="PropertyTreeRowViewModel" />
        /// </summary>
        public PropertyTreeRowViewModel(Thing thing, IDiagramEditorViewModel container, IDiagramItemOrConnector thingDiagramItemViewModel, string value) : base(thing, container, thingDiagramItemViewModel)
        {
            this.Name = value;
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Sets the name
        /// </summary>
        protected override void SetName()
        {
            return;
        }

        /// <summary>
        /// Sets the children nodes
        /// </summary>
        protected override void SetChildren()
        {
            return;
        }
    }
}