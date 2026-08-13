// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEditorPluginSettings.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4DiagramEditor.Settings
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    using CDP4Composition.PluginSettingService;

    /// <summary>
    /// Represents the persisted settings of the <see cref="CDP4DiagramEditorModule"/> plugin. The inherited
    /// <see cref="PluginSettings.SavedConfigurations"/> collection holds the saved
    /// <see cref="TraceabilityConfiguration"/> presets.
    /// </summary>
    public class DiagramEditorPluginSettings : PluginSettings
    {
        /// <summary>
        /// The set of <see cref="ClassKind"/>s offered by the traceability diagram pickers, the same curated set the
        /// Relationship Matrix uses instead of every categorizable <see cref="ClassKind"/> of the model
        /// </summary>
        public static readonly IReadOnlyList<ClassKind> DefaultClassKinds = new List<ClassKind>
        {
            ClassKind.ElementDefinition,
            ClassKind.ElementUsage,
            ClassKind.NestedElement,
            ClassKind.Option,
            ClassKind.Parameter,
            ClassKind.ParametricConstraint,
            ClassKind.RequirementsSpecification,
            ClassKind.RequirementsGroup,
            ClassKind.Requirement
        };
    }
}
