// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetRowAssembler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CrossViewEditor.Assemblers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetRowAssembler"/> is to assemble a list of <see cref="IExcelRow{T}"/>
    /// that will be written to a workbook.
    /// </summary>
    public class CrossviewSheetRowAssembler
    {
        /// <summary>
        /// Backing field for the <see cref="IExcelRow{T}"/> property
        /// </summary>
        private readonly List<IExcelRow<Thing>> excelRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// The unique ids of the <see cref="Option"/>s each row is excluded from, by row.
        /// </summary>
        private readonly Dictionary<IExcelRow<Thing>, HashSet<Guid>> excludedOptions = new Dictionary<IExcelRow<Thing>, HashSet<Guid>>();

        /// <summary>
        /// The unique ids of the selected <see cref="Option"/>s; empty when every option is shown.
        /// </summary>
        private List<Guid> selectedOptions = new List<Guid>();

        /// <summary>
        /// Gets the <see cref="IExcelRow{Thing}"/>s that have been assembled
        /// </summary>
        public IEnumerable<IExcelRow<Thing>> ExcelRows => this.excelRows;

        /// <summary>
        /// Queries the <see cref="ElementDefinition"/>s that make up the product tree of an <see cref="Iteration"/>: its
        /// <see cref="Iteration.TopElement"/> and every <see cref="ElementDefinition"/> used below it, at any depth.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>
        /// </param>
        /// <returns>
        /// The <see cref="ElementDefinition"/>s, empty when the <see cref="Iteration"/> has no top element
        /// </returns>
        public static HashSet<ElementDefinition> QueryProductTree(Iteration iteration)
        {
            var elementDefinitions = new HashSet<ElementDefinition>();

            if (iteration.TopElement != null)
            {
                AddWithUsedElementDefinitions(iteration.TopElement, elementDefinitions);
            }

            return elementDefinitions;
        }

        /// <summary>
        /// Queries whether a selection holds every <see cref="ElementDefinition"/> of the product tree.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>
        /// </param>
        /// <param name="elementDefinitions">
        /// The unique ids of the selected <see cref="ElementDefinition"/>s
        /// </param>
        /// <returns>
        /// True when the selection covers the whole product tree, which is then followed as it changes
        /// </returns>
        public static bool CoversProductTree(Iteration iteration, IEnumerable<Guid> elementDefinitions)
        {
            var productTree = QueryProductTree(iteration);
            var selected = new HashSet<Guid>(elementDefinitions);

            return productTree.Count > 0 && productTree.All(elementDefinition => selected.Contains(elementDefinition.Iid));
        }

        /// <summary>
        /// Queries the selection a rebuild works with.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>
        /// </param>
        /// <param name="elementDefinitions">
        /// The unique ids of the <see cref="ElementDefinition"/>s that were selected
        /// </param>
        /// <param name="followProductTree">
        /// A value indicating whether the selection covered the whole product tree
        /// </param>
        /// <returns>
        /// The selection, with every <see cref="ElementDefinition"/> the product tree holds now when it is followed
        /// </returns>
        /// <remarks>
        /// The selection is a list of element definitions, so a usage of one that is new to the product tree would otherwise
        /// never appear, however often the sheets are rebuilt.
        /// </remarks>
        public static List<Guid> QuerySelection(Iteration iteration, IEnumerable<Guid> elementDefinitions, bool followProductTree)
        {
            var selection = (elementDefinitions ?? Enumerable.Empty<Guid>()).ToList();

            if (followProductTree)
            {
                selection.AddRange(QueryProductTree(iteration).Select(elementDefinition => elementDefinition.Iid).Where(iid => !selection.Contains(iid)));
            }

            return selection;
        }

        /// <summary>
        /// Assemble the <see cref="IExcelRow{Thing}"/>s.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>.
        /// </param>
        /// <param name="elementDefinitions">
        /// Selected <see cref="ElementDefinition"/>s.
        /// </param>
        /// <param name="options">
        /// The unique ids of the selected <see cref="Option"/>s. A row that every selected option excludes is left out; null or
        /// empty shows every row.
        /// </param>
        public void Assemble(Iteration iteration, IEnumerable<Guid> elementDefinitions, IEnumerable<Guid> options = null)
        {
            var selectedIids = new HashSet<Guid>(elementDefinitions);
            this.selectedOptions = options?.ToList() ?? new List<Guid>();

            if (iteration.TopElement == null)
            {
                // without a top element there is no product tree to walk, so the selection is listed as it comes
                this.AssembleWithoutProductTree(iteration, selectedIids);
                return;
            }

            var topElement = iteration.TopElement;

            // the top element is the only node that is represented by its ElementDefinition; every node below it is
            // represented by the ElementUsage through which it is used
            if (selectedIids.Contains(topElement.Iid))
            {
                this.AddRow(new ElementDefinitionExcelRow(topElement, 0), new HashSet<Guid>());
            }

            this.AssembleElementUsages(topElement, selectedIids, 0, new HashSet<ElementDefinition> { topElement }, new HashSet<Guid>());
        }

        /// <summary>
        /// Queries whether a row is part of the product tree of an <see cref="Option"/>.
        /// </summary>
        /// <param name="excelRow">
        /// A row of <see cref="ExcelRows"/>
        /// </param>
        /// <param name="option">
        /// The unique id of the <see cref="Option"/>
        /// </param>
        /// <returns>
        /// False when the row, or an <see cref="ElementUsage"/> above it, excludes the option
        /// </returns>
        public bool IsInOption(IExcelRow<Thing> excelRow, Guid option) => !this.excludedOptions[excelRow].Contains(option);

        /// <summary>
        /// Assemble the rows of the <see cref="ElementUsage"/>s of <paramref name="elementDefinition"/>, depth first, so that the
        /// order of the rows on the sheet is the order of the product tree.
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> whose <see cref="ElementUsage"/>s are being assembled
        /// </param>
        /// <param name="selectedIids">
        /// The unique ids of the <see cref="ElementDefinition"/>s that the user selected
        /// </param>
        /// <param name="level">
        /// The depth of <paramref name="elementDefinition"/> in the product tree
        /// </param>
        /// <param name="path">
        /// The <see cref="ElementDefinition"/>s on the path from the top element to <paramref name="elementDefinition"/>
        /// </param>
        /// <param name="excludedAbove">
        /// The unique ids of the <see cref="Option"/>s excluded by the <see cref="ElementUsage"/>s above
        /// <paramref name="elementDefinition"/>
        /// </param>
        private void AssembleElementUsages(ElementDefinition elementDefinition, HashSet<Guid> selectedIids, int level, HashSet<ElementDefinition> path, HashSet<Guid> excludedAbove)
        {
            foreach (var elementUsage in QueryUsages(elementDefinition))
            {
                // an option that excludes a usage excludes everything below it as well
                var excluded = new HashSet<Guid>(excludedAbove);
                excluded.UnionWith(elementUsage.ExcludeOption.Select(option => option.Iid));

                if (!this.IsInAnySelectedOption(excluded))
                {
                    continue;
                }

                if (selectedIids.Contains(elementUsage.ElementDefinition.Iid))
                {
                    this.AddRow(new ElementUsageExcelRow(elementUsage, level + 1), excluded);
                }

                // The path guards against an ElementDefinition that is used, directly or indirectly, within itself. It is a
                // path rather than a set of everything visited, because an ElementDefinition that is legitimately used in more
                // than one place has to appear once per place it is used. The branch is walked whether or not the usage itself
                // is selected, so that deselecting an intermediate element does not hide everything underneath it.
                if (path.Add(elementUsage.ElementDefinition))
                {
                    this.AssembleElementUsages(elementUsage.ElementDefinition, selectedIids, level + 1, path, excluded);
                    path.Remove(elementUsage.ElementDefinition);
                }
            }
        }

        /// <summary>
        /// Assemble the rows of the selected <see cref="ElementDefinition"/>s when the <see cref="Iteration"/> has no
        /// <see cref="Iteration.TopElement"/>, and therefore no product tree.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>
        /// </param>
        /// <param name="selectedIids">
        /// The unique ids of the <see cref="ElementDefinition"/>s that the user selected
        /// </param>
        private void AssembleWithoutProductTree(Iteration iteration, HashSet<Guid> selectedIids)
        {
            var selectedElementDefinitions = iteration.Element
                .Where(elementDefinition => selectedIids.Contains(elementDefinition.Iid));

            foreach (var elementDefinition in selectedElementDefinitions)
            {
                this.AddRow(new ElementDefinitionExcelRow(elementDefinition), new HashSet<Guid>());

                foreach (var elementUsage in QueryUsages(elementDefinition))
                {
                    var excluded = new HashSet<Guid>(elementUsage.ExcludeOption.Select(option => option.Iid));

                    if (this.IsInAnySelectedOption(excluded))
                    {
                        this.AddRow(new ElementUsageExcelRow(elementUsage), excluded);
                    }
                }
            }
        }

        /// <summary>
        /// Queries the <see cref="ElementUsage"/>s of an <see cref="ElementDefinition"/> that are part of the product tree.
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/>
        /// </param>
        /// <returns>
        /// The usages that are not being deleted and use an <see cref="ElementDefinition"/>
        /// </returns>
        private static IEnumerable<ElementUsage> QueryUsages(ElementDefinition elementDefinition) =>
            elementDefinition.ContainedElement.Where(elementUsage => elementUsage.ChangeKind != ChangeKind.Delete && elementUsage.ElementDefinition != null);

        /// <summary>
        /// Adds an <see cref="ElementDefinition"/> and, recursively, the <see cref="ElementDefinition"/>s it uses.
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> that is added
        /// </param>
        /// <param name="elementDefinitions">
        /// The <see cref="ElementDefinition"/>s found so far
        /// </param>
        private static void AddWithUsedElementDefinitions(ElementDefinition elementDefinition, HashSet<ElementDefinition> elementDefinitions)
        {
            // Add returns false for an ElementDefinition that was visited, which ends the recursion when one uses itself
            if (!elementDefinitions.Add(elementDefinition))
            {
                return;
            }

            foreach (var elementUsage in QueryUsages(elementDefinition))
            {
                AddWithUsedElementDefinitions(elementUsage.ElementDefinition, elementDefinitions);
            }
        }

        /// <summary>
        /// Adds a row together with the <see cref="Option"/>s it is excluded from.
        /// </summary>
        /// <param name="excelRow">
        /// The row
        /// </param>
        /// <param name="excluded">
        /// The unique ids of the <see cref="Option"/>s the row is excluded from
        /// </param>
        private void AddRow(IExcelRow<Thing> excelRow, HashSet<Guid> excluded)
        {
            this.excelRows.Add(excelRow);
            this.excludedOptions[excelRow] = excluded;
        }

        /// <summary>
        /// Queries whether a row is part of at least one selected <see cref="Option"/>.
        /// </summary>
        /// <param name="excluded">
        /// The unique ids of the <see cref="Option"/>s the row is excluded from
        /// </param>
        /// <returns>
        /// True when no option is selected, or when a selected option does not exclude the row
        /// </returns>
        private bool IsInAnySelectedOption(HashSet<Guid> excluded) =>
            this.selectedOptions.Count == 0 || this.selectedOptions.Any(option => !excluded.Contains(option));
    }
}
