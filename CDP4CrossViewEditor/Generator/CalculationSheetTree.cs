// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetTree.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The product tree as it appears on the calculation sheet: one entry per body row, in sheet order, each with a key that
    /// identifies the row across rebuilds.
    /// </summary>
    /// <remarks>
    /// The key is the path of short names from the top element down to the row, for example <c>Sat/bat_a/cell_1</c>; the model code
    /// alone is not unique, since an element definition used in two places repeats the model codes of its children. Only the Name
    /// and Model Code columns are read, so that both sheets of a rebuild key their rows the same way.
    /// </remarks>
    public class CalculationSheetTree
    {
        /// <summary>
        /// The number of spaces the row models use to indent one level of the product tree in the Name column.
        /// </summary>
        public const int IndentationWidth = 3;

        /// <summary>
        /// The separator between the short names of a path.
        /// </summary>
        private const string PathSeparator = "/";

        /// <summary>
        /// The index of each row by its key.
        /// </summary>
        private readonly Dictionary<string, int> indexByKey;

        /// <summary>
        /// The index of the parent of each row, or -1 for a row without a parent.
        /// </summary>
        private readonly int[] parents;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSheetTree"/> class.
        /// </summary>
        /// <param name="names">
        /// The text of the Name column of each body row, indented by depth
        /// </param>
        /// <param name="modelCodes">
        /// The text of the Model Code column of each body row
        /// </param>
        public CalculationSheetTree(IReadOnlyList<string> names, IReadOnlyList<string> modelCodes)
        {
            var keys = new List<string>();
            var levels = new List<int>();
            var path = new List<string>();
            var occurrences = new Dictionary<string, int>();

            for (var index = 0; index < names.Count; index++)
            {
                var level = ParseLevel(names[index]);
                var modelCode = index < modelCodes.Count ? modelCodes[index] : null;

                while (path.Count > level)
                {
                    path.RemoveAt(path.Count - 1);
                }

                while (path.Count < level)
                {
                    path.Add("?");
                }

                path.Add(QuerySegment(names[index], modelCode));

                var key = string.Join(PathSeparator, path);
                occurrences.TryGetValue(key, out var seen);
                occurrences[key] = seen + 1;

                keys.Add(seen == 0 ? key : $"{key}#{seen + 1}");
                levels.Add(level);
            }

            this.Keys = keys;
            this.Levels = levels;
            this.indexByKey = keys.Select((key, index) => (key, index)).ToDictionary(entry => entry.key, entry => entry.index);
            this.parents = QueryParents(levels);
        }

        /// <summary>
        /// Gets the key of each body row, in sheet order.
        /// </summary>
        public IReadOnlyList<string> Keys { get; }

        /// <summary>
        /// Gets the depth of each body row in the product tree, in sheet order.
        /// </summary>
        public IReadOnlyList<int> Levels { get; }

        /// <summary>
        /// Gets the number of body rows.
        /// </summary>
        public int Count => this.Keys.Count;

        /// <summary>
        /// Parses the depth of a row from the indentation of its name.
        /// </summary>
        /// <param name="name">
        /// The text of the Name column
        /// </param>
        /// <returns>
        /// The depth of the row, zero for the top element
        /// </returns>
        public static int ParseLevel(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return 0;
            }

            var spaces = name.TakeWhile(character => character == ' ').Count();

            return spaces / IndentationWidth;
        }

        /// <summary>
        /// Queries the body rows that are below the given row in the product tree, at any depth.
        /// </summary>
        /// <param name="index">
        /// The index of the body row
        /// </param>
        /// <returns>
        /// The indices of the descendants, in sheet order
        /// </returns>
        public IEnumerable<int> Descendants(int index)
        {
            for (var candidate = index + 1; candidate < this.Count && this.Levels[candidate] > this.Levels[index]; candidate++)
            {
                yield return candidate;
            }
        }

        /// <summary>
        /// Queries the body rows directly below the given row in the product tree.
        /// </summary>
        /// <param name="index">
        /// The index of the body row
        /// </param>
        /// <returns>
        /// The indices of the direct children, in sheet order
        /// </returns>
        /// <remarks>
        /// A child is any descendant whose nearest ancestor on the sheet is this row. That is not always one level deeper: when an
        /// intermediate element is not selected, its children appear two levels below their visible ancestor.
        /// </remarks>
        public IEnumerable<int> DirectChildren(int index) => this.Descendants(index).Where(candidate => this.parents[candidate] == index);

        /// <summary>
        /// Queries the index of the body row with the given key.
        /// </summary>
        /// <param name="key">
        /// The key of the row
        /// </param>
        /// <returns>
        /// The index of the row, or -1 when no row has that key
        /// </returns>
        public int IndexOf(string key) => key != null && this.indexByKey.TryGetValue(key, out var index) ? index : -1;

        /// <summary>
        /// Queries the index of the nearest preceding row with a smaller depth, for every row.
        /// </summary>
        /// <param name="levels">
        /// The depth of each row
        /// </param>
        /// <returns>
        /// The index of the parent of each row, or -1 for a row without a parent
        /// </returns>
        private static int[] QueryParents(IReadOnlyList<int> levels)
        {
            var parents = new int[levels.Count];
            var ancestors = new Stack<int>();

            for (var index = 0; index < levels.Count; index++)
            {
                while (ancestors.Count > 0 && levels[ancestors.Peek()] >= levels[index])
                {
                    ancestors.Pop();
                }

                parents[index] = ancestors.Count > 0 ? ancestors.Peek() : -1;
                ancestors.Push(index);
            }

            return parents;
        }

        /// <summary>
        /// Queries the path segment that identifies a row among its siblings.
        /// </summary>
        /// <param name="name">
        /// The text of the Name column
        /// </param>
        /// <param name="modelCode">
        /// The text of the Model Code column
        /// </param>
        /// <returns>
        /// The short name of the element usage, which ends the model code of a usage, or the whole model code of the top element
        /// </returns>
        private static string QuerySegment(string name, string modelCode)
        {
            var code = modelCode?.Trim();

            if (string.IsNullOrEmpty(code))
            {
                return name?.Trim() ?? string.Empty;
            }

            var separator = code.LastIndexOf('.');

            return separator < 0 ? code : code.Substring(separator + 1);
        }
    }
}
