// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageDropValidator.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.DragDrop
{
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Permission;

    /// <summary>
    /// Centralizes the validation of dropping an <see cref="ElementUsage"/> onto an <see cref="ElementDefinition"/>, so that the
    /// move (re-parent) drag-and-drop behaviour stays consistent - and safe against invalid models - across the Element Definitions
    /// browser and the Product Tree.
    /// </summary>
    public static class ElementUsageDropValidator
    {
        /// <summary>
        /// Computes the <see cref="DragDropEffects"/> for dropping an <see cref="ElementUsage"/> onto an <see cref="ElementDefinition"/>
        /// to move (re-parent) it.
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> being dragged.</param>
        /// <param name="targetElementDefinition">The <see cref="ElementDefinition"/> that is the drop target.</param>
        /// <param name="permissionService">The <see cref="IPermissionService"/> used to check write permission.</param>
        /// <returns>The resulting <see cref="DragDropEffects"/>.</returns>
        public static DragDropEffects GetDropEffect(ElementUsage elementUsage, ElementDefinition targetElementDefinition, IPermissionService permissionService)
        {
            if (elementUsage == null || targetElementDefinition == null)
            {
                return DragDropEffects.None;
            }

            // permission to move an element usage into the target definition
            if (!permissionService.CanWrite(ClassKind.ElementUsage, targetElementDefinition))
            {
                return DragDropEffects.None;
            }

            // moving between models is not supported
            if (elementUsage.TopContainer != targetElementDefinition.TopContainer)
            {
                return DragDropEffects.None;
            }

            // dropping the usage onto the definition it references, or onto a definition already used inside it, would create a containment loop
            if (elementUsage.ElementDefinition.HasUsageOf(targetElementDefinition))
            {
                return DragDropEffects.None;
            }

            // a move onto the usage's current container would be a no-op
            if (elementUsage.Container == targetElementDefinition)
            {
                return DragDropEffects.None;
            }

            return DragDropEffects.Move;
        }
    }
}
