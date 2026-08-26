// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVPanelHelper.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// The little pieces the V&amp;V Register and the V&amp;V Activities panels share, so the two cannot drift apart
    /// on them.
    /// </summary>
    public static class VandVPanelHelper
    {
        /// <summary>
        /// Asserts that the model carries all the V&amp;V reference data a write needs, warning the user when it does
        /// not.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> the write would happen in.</param>
        /// <param name="caption">The caption of the action being attempted, used on the message box.</param>
        /// <returns>true when the write may proceed.</returns>
        public static bool EnsureReferenceData(Iteration iteration, string caption)
        {
            if (VandVItemCreator.CanCreate(iteration))
            {
                return true;
            }

            DXMessageBox.Show(
                "The V&V reference data is not complete in this model yet. Run 'Set up V&V' on the Requirements ribbon tab first.",
                caption,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        /// <summary>
        /// Records which rows of a tree are expanded, keyed by the <see cref="Thing.Iid"/> each row stands for, so
        /// the state survives the rows being replaced by a rebuild.
        /// </summary>
        /// <param name="rows">The root rows.</param>
        /// <returns>The expansion state.</returns>
        public static Dictionary<Guid, bool> CaptureExpansion(IEnumerable<IRowViewModelBase<Thing>> rows)
        {
            var expansion = new Dictionary<Guid, bool>();

            foreach (var row in rows)
            {
                Capture(row, expansion);
            }

            return expansion;
        }

        /// <summary>
        /// Puts a recorded expansion state back onto a tree, leaving rows it says nothing about alone.
        /// </summary>
        /// <param name="rows">The root rows.</param>
        /// <param name="expansion">The recorded state.</param>
        public static void RestoreExpansion(IEnumerable<IRowViewModelBase<Thing>> rows, IReadOnlyDictionary<Guid, bool> expansion)
        {
            foreach (var row in rows)
            {
                Restore(row, expansion);
            }
        }

        /// <summary>
        /// Records the expansion of a row and everything below it.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="expansion">The state being built.</param>
        private static void Capture(IRowViewModelBase<Thing> row, IDictionary<Guid, bool> expansion)
        {
            expansion[row.Thing.Iid] = row.IsExpanded;

            foreach (var child in row.ContainedRows)
            {
                if (child is IRowViewModelBase<Thing> childRow)
                {
                    Capture(childRow, expansion);
                }
            }
        }

        /// <summary>
        /// Puts the recorded expansion back onto a row and everything below it.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="expansion">The recorded state.</param>
        private static void Restore(IRowViewModelBase<Thing> row, IReadOnlyDictionary<Guid, bool> expansion)
        {
            if (expansion.TryGetValue(row.Thing.Iid, out var isExpanded))
            {
                row.IsExpanded = isExpanded;
            }

            foreach (var child in row.ContainedRows)
            {
                if (child is IRowViewModelBase<Thing> childRow)
                {
                    Restore(childRow, expansion);
                }
            }
        }
    }
}
