// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewStructure.cs" company="Starion Group S.A.">
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

namespace CDP4CrossViewEditor.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The rows and parameter columns of the Crossview sheet, by key, so that a reference into that sheet can follow its target
    /// across a rebuild even when a calculation sheet shows only part of it.
    /// </summary>
    public class CrossviewStructure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewStructure"/> class.
        /// </summary>
        /// <param name="tree">
        /// The product tree of the body rows of the Crossview sheet
        /// </param>
        /// <param name="columnKeys">
        /// The key of every parameter column of the Crossview sheet
        /// </param>
        public CrossviewStructure(CalculationSheetTree tree, IReadOnlyList<string> columnKeys)
        {
            this.Tree = tree;
            this.ColumnKeys = columnKeys;
        }

        /// <summary>
        /// Gets the product tree of the body rows of the Crossview sheet.
        /// </summary>
        public CalculationSheetTree Tree { get; }

        /// <summary>
        /// Gets the key of every parameter column of the Crossview sheet, counting from its first parameter column.
        /// </summary>
        public IReadOnlyList<string> ColumnKeys { get; }

        /// <summary>
        /// Builds the structure from the content of the Crossview sheet.
        /// </summary>
        /// <param name="names">
        /// The name of each body row, indented by depth
        /// </param>
        /// <param name="modelCodes">
        /// The model code of each body row
        /// </param>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet, starting with its nested column header
        /// </param>
        /// <returns>
        /// The structure
        /// </returns>
        public static CrossviewStructure FromContent(IReadOnlyList<string> names, IReadOnlyList<string> modelCodes, object[,] crossviewContent)
        {
            var numberOfParameterColumns = Math.Max(0, crossviewContent.GetLength(1) - CrossviewSheetConstants.FixedColumns);

            return new CrossviewStructure(
                new CalculationSheetTree(names, modelCodes),
                UniqueKeys(Enumerable.Range(0, numberOfParameterColumns).Select(column => CalculationSheetBuilder.QueryColumnHeader(crossviewContent, column))));
        }

        /// <summary>
        /// Builds the model code a row of the Crossview sheet had from its short name, which reads "{definition} : {usage}" for
        /// a usage and "{definition}" for a definition.
        /// </summary>
        /// <param name="shortName">
        /// The short name as written on the Crossview sheet
        /// </param>
        /// <returns>
        /// The model code
        /// </returns>
        public static string ModelCodeFromShortName(string shortName) => shortName?.Trim().Replace(" : ", ".") ?? string.Empty;

        /// <summary>
        /// Makes a list of keys unique by numbering the repeats of a key.
        /// </summary>
        /// <param name="keys">
        /// The keys
        /// </param>
        /// <returns>
        /// The keys, where the second occurrence of a key reads "{key}#2", the third "{key}#3", and so on
        /// </returns>
        public static List<string> UniqueKeys(IEnumerable<string> keys)
        {
            var occurrences = new Dictionary<string, int>();
            var result = new List<string>();

            foreach (var key in keys.Select(key => key ?? string.Empty))
            {
                occurrences.TryGetValue(key, out var seen);
                occurrences[key] = seen + 1;
                result.Add(seen == 0 ? key : $"{key}#{seen + 1}");
            }

            return result;
        }
    }
}
