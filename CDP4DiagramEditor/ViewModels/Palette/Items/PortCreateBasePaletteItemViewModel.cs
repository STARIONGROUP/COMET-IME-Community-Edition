// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortCreateBasePaletteItemViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.Palette
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using CommonServiceLocator;

    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Base class for all port creation palette items
    /// </summary>
    public abstract class PortCreateBasePaletteItemViewModel : PaletteItemBaseViewModel
    {
        /// <summary>
        /// The <see cref="IThingSelectorDialogService"/>
        /// </summary>
        private readonly IThingSelectorDialogService thingSelectorDialogService = ServiceLocator.Current.GetInstance<IThingSelectorDialogService>();

        /// <summary>
        /// The <see cref="IThingCreator"/>
        /// </summary>
        private readonly IThingCreator thingCreator = ServiceLocator.Current.GetInstance<IThingCreator>();

        /// <summary>
        /// Gets the palette group this item belongs to
        /// </summary>
        public override PaletteGroup Group => PaletteGroup.PortsAndInterfaces;

        /// <summary>
        /// Gets the list of supported diagram types. When a supertype is listed all subtypes are also supported.
        /// </summary>
        public override List<Type> SupportedDiagramTypes => new() { typeof(ArchitectureDiagram) };

        /// <summary>
        /// Creates a port of a certain kind
        /// </summary>
        /// <param name="kind">The <see cref="InterfaceEndKind"/> of the port</param>
        /// <returns>Empty task</returns>
        protected async Task CreatePort(InterfaceEndKind kind)
        {
            if (this.editorViewModel.SelectedItem is DiagramContentItem { Content: PortContainerDiagramContentItemViewModel container } target)
            {
                var iteration = (Iteration)this.editorViewModel.Thing.Container;

                // grab the ED from which to make a port from
                var result = this.thingSelectorDialogService.SelectThing(iteration.Element,
                    new List<string> { "Name", "ShortName" });

                if (result != null)
                {
                    await this.thingCreator.CreateElementUsage((ElementDefinition)container.Thing, result, this.editorViewModel.Session.QuerySelectedDomainOfExpertise(iteration), this.editorViewModel.Session, kind);
                }
            }
        }
    }
}
