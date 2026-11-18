// --------------------------------------------------------------------------------------------------------------------
// <copyright file="R1C1References.cs" company="Starion Group S.A.">
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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Rewrites the cell references of a formula when the cell that holds it, and the cells it refers to, move, and converts a
    /// formula from A1 to R1C1 notation.
    /// </summary>
    /// <remarks>
    /// Excel adjusts references itself when rows are inserted or deleted, but the calculation sheet is rebuilt rather than edited,
    /// so nothing adjusts them for it. This does the equivalent: every reference is resolved to the cell it points at, that cell
    /// is located again after the rebuild, and the reference is written out anew in its original relative or absolute style.
    /// String literals are left alone.
    /// </remarks>
    public static class R1C1References
    {
        /// <summary>
        /// The text that replaces a reference to a cell that no longer exists, as Excel itself does.
        /// </summary>
        public const string BrokenReference = "#REF!";

        /// <summary>
        /// A reference in R1C1 notation, being a cell, a whole row or a whole column, optionally qualified with a sheet.
        /// </summary>
        private static readonly Regex R1C1Pattern = new Regex(
            @"(?<![A-Za-z0-9_.\]'!])(?:(?<sheet>'(?:[^']|'')+'|[A-Za-z_][A-Za-z0-9_.]*)!)?(?:R(?<row>\[-?\d+\]|\d+)?C(?<column>\[-?\d+\]|\d+)?|R(?<rowOnly>\[-?\d+\]|\d+)|C(?<columnOnly>\[-?\d+\]|\d+))(?![A-Za-z0-9_(\[])",
            RegexOptions.Compiled);

        /// <summary>
        /// A cell reference in A1 notation, optionally absolute and optionally qualified with a sheet.
        /// </summary>
        private static readonly Regex A1Pattern = new Regex(
            @"(?<![A-Za-z0-9_.\]'!])(?:(?<sheet>'(?:[^']|'')+'|[A-Za-z_][A-Za-z0-9_.]*)!)?(?<columnAbsolute>\$)?(?<column>[A-Z]{1,3})(?<rowAbsolute>\$)?(?<row>\d+)(?![A-Za-z0-9_(\[])",
            RegexOptions.Compiled);

        /// <summary>
        /// Rewrites the references of a formula for the move of its cell and of the cells it refers to.
        /// </summary>
        /// <param name="formula">
        /// The formula, in R1C1 notation
        /// </param>
        /// <param name="oldRow">
        /// The row of the cell that held the formula, counting from one
        /// </param>
        /// <param name="oldColumn">
        /// The column of the cell that held the formula, counting from one
        /// </param>
        /// <param name="newRow">
        /// The row of the cell that will hold the formula, counting from one
        /// </param>
        /// <param name="newColumn">
        /// The column of the cell that will hold the formula, counting from one
        /// </param>
        /// <param name="mapRow">
        /// Maps a referenced row of the named sheet, null for the formula's own sheet, to its new row, or to null when it no longer
        /// exists
        /// </param>
        /// <param name="mapColumn">
        /// Maps a referenced column of the named sheet, null for the formula's own sheet, to its new column, or to null when it no
        /// longer exists
        /// </param>
        /// <returns>
        /// The rewritten formula, together with what was found while rewriting it
        /// </returns>
        public static R1C1RemapResult Remap(
            string formula,
            int oldRow,
            int oldColumn,
            int newRow,
            int newColumn,
            Func<string, int, int?> mapRow,
            Func<string, int, int?> mapColumn)
        {
            var hasBrokenReference = false;
            var ranges = new List<(string Sheet, int OldFirst, int OldLast, int NewFirst, int NewLast)>();
            var sheets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var rewritten = RewriteOutsideStrings(formula, segment =>
            {
                var tokens = R1C1Pattern.Matches(segment).Cast<Match>().Select(match => new Token(match, oldRow, oldColumn)).ToList();

                if (tokens.Count == 0)
                {
                    return segment;
                }

                MarkRanges(segment, tokens);

                foreach (var token in tokens)
                {
                    token.Resolve(mapRow, mapColumn);

                    if (token.Sheet != null)
                    {
                        sheets.Add(token.Sheet);
                    }
                }

                var output = new StringBuilder();
                var position = 0;

                for (var index = 0; index < tokens.Count; index++)
                {
                    var token = tokens[index];
                    output.Append(segment, position, token.Start - position);

                    if (token.RangeEnd == null)
                    {
                        hasBrokenReference |= token.IsBroken;
                        output.Append(token.IsBroken ? BrokenReference : token.Render(newRow, newColumn));
                        position = token.End;
                        continue;
                    }

                    var end = token.RangeEnd;

                    // as Excel does when a row at the edge of a range is deleted, the range shrinks to the rows that remain
                    ShrinkRange(token, end, mapRow, mapColumn);

                    if (token.IsBroken || end.IsBroken)
                    {
                        // only a range of which nothing remains is lost as a whole
                        hasBrokenReference = true;
                        output.Append(BrokenReference);
                    }
                    else
                    {
                        output.Append(token.Render(newRow, newColumn)).Append(':').Append(end.Render(newRow, newColumn));

                        if (token.HasRow && end.HasRow && token.OldTargetRow != end.OldTargetRow)
                        {
                            ranges.Add((
                                token.Sheet,
                                Math.Min(token.OldTargetRow, end.OldTargetRow),
                                Math.Max(token.OldTargetRow, end.OldTargetRow),
                                Math.Min(token.NewTargetRow.Value, end.NewTargetRow.Value),
                                Math.Max(token.NewTargetRow.Value, end.NewTargetRow.Value)));
                        }
                    }

                    position = end.End;
                    index++;
                }

                output.Append(segment, position, segment.Length - position);

                return output.ToString();
            });

            return new R1C1RemapResult(rewritten, hasBrokenReference, ranges, sheets);
        }

        /// <summary>
        /// Converts a formula from A1 to R1C1 notation, relative to the cell that holds it.
        /// </summary>
        /// <param name="formula">
        /// The formula, in A1 notation
        /// </param>
        /// <param name="row">
        /// The row of the cell that holds the formula, counting from one
        /// </param>
        /// <param name="column">
        /// The column of the cell that holds the formula, counting from one
        /// </param>
        /// <returns>
        /// The formula in R1C1 notation
        /// </returns>
        /// <remarks>
        /// Needed for formulas that were typed into a cell formatted as text. Excel keeps those as the literal text the user
        /// typed, in A1 notation, and never evaluates them; reading them back as if they were R1C1 would turn <c>C7</c> into the
        /// whole of column 7.
        /// </remarks>
        public static string FromA1(string formula, int row, int column)
        {
            return RewriteOutsideStrings(formula, segment => A1Pattern.Replace(segment, match =>
            {
                var sheet = match.Groups["sheet"].Success ? $"{match.Groups["sheet"].Value}!" : string.Empty;
                var targetRow = int.Parse(match.Groups["row"].Value, CultureInfo.InvariantCulture);
                var targetColumn = ColumnNumber(match.Groups["column"].Value);

                var rowPart = match.Groups["rowAbsolute"].Success ? targetRow.ToString(CultureInfo.InvariantCulture) : RelativePart(targetRow - row);
                var columnPart = match.Groups["columnAbsolute"].Success ? targetColumn.ToString(CultureInfo.InvariantCulture) : RelativePart(targetColumn - column);

                return $"{sheet}R{rowPart}C{columnPart}";
            }));
        }

        /// <summary>
        /// Converts Excel column letters into a column number.
        /// </summary>
        /// <param name="letters">
        /// The column letters, for example A or AB
        /// </param>
        /// <returns>
        /// The column number, counting from one
        /// </returns>
        public static int ColumnNumber(string letters) => letters.Aggregate(0, (total, letter) => (total * 26) + (letter - 'A' + 1));

        /// <summary>
        /// Writes a relative offset in R1C1 notation.
        /// </summary>
        /// <param name="offset">
        /// The offset from the cell that holds the formula
        /// </param>
        /// <returns>
        /// Nothing for an offset of zero, otherwise the offset in square brackets
        /// </returns>
        private static string RelativePart(int offset) => offset == 0 ? string.Empty : $"[{offset.ToString(CultureInfo.InvariantCulture)}]";

        /// <summary>
        /// Moves an end of a range that points at a row or column that no longer exists inwards, to the nearest one that remains.
        /// </summary>
        /// <param name="start">
        /// The reference that starts the range
        /// </param>
        /// <param name="end">
        /// The reference that ends the range
        /// </param>
        /// <param name="mapRow">
        /// Maps a row of the named sheet to its new row
        /// </param>
        /// <param name="mapColumn">
        /// Maps a column of the named sheet to its new column
        /// </param>
        private static void ShrinkRange(Token start, Token end, Func<string, int, int?> mapRow, Func<string, int, int?> mapColumn)
        {
            if (start.HasRow && end.HasRow)
            {
                start.NewTargetRow = start.NewTargetRow ?? Nearest(start.Sheet, start.OldTargetRow, end.OldTargetRow, mapRow);
                end.NewTargetRow = end.NewTargetRow ?? Nearest(end.Sheet, end.OldTargetRow, start.OldTargetRow, mapRow);
            }

            if (start.HasColumn && end.HasColumn)
            {
                start.NewTargetColumn = start.NewTargetColumn ?? Nearest(start.Sheet, start.OldTargetColumn, end.OldTargetColumn, mapColumn);
                end.NewTargetColumn = end.NewTargetColumn ?? Nearest(end.Sheet, end.OldTargetColumn, start.OldTargetColumn, mapColumn);
            }
        }

        /// <summary>
        /// Finds the nearest row or column that still exists, walking from one end of a range towards the other.
        /// </summary>
        /// <param name="sheet">
        /// The name of the sheet, or null for the sheet of the formula itself
        /// </param>
        /// <param name="origin">
        /// The row or column that no longer exists
        /// </param>
        /// <param name="toward">
        /// The other end of the range
        /// </param>
        /// <param name="map">
        /// Maps a row or column of the named sheet to its new position
        /// </param>
        /// <returns>
        /// The new position of the nearest row or column that remains, or null when none of the range remains
        /// </returns>
        private static int? Nearest(string sheet, int origin, int toward, Func<string, int, int?> map)
        {
            var step = Math.Sign(toward - origin);

            if (step == 0)
            {
                return null;
            }

            for (var position = origin + step; position != toward + step; position += step)
            {
                var mapped = map(sheet, position);

                if (mapped != null)
                {
                    return mapped;
                }
            }

            return null;
        }

        /// <summary>
        /// Links every reference that is the first end of a range to the reference that is its other end.
        /// </summary>
        /// <param name="segment">
        /// The text the references were found in
        /// </param>
        /// <param name="tokens">
        /// The references, in the order they appear
        /// </param>
        private static void MarkRanges(string segment, IReadOnlyList<Token> tokens)
        {
            for (var index = 0; index + 1 < tokens.Count; index++)
            {
                var start = tokens[index];
                var end = tokens[index + 1];

                if (start.End < segment.Length && segment[start.End] == ':' && end.Start == start.End + 1)
                {
                    start.RangeEnd = end;

                    // the far end of a range on another sheet carries no sheet of its own
                    end.InheritSheet(start);
                    index++;
                }
            }
        }

        /// <summary>
        /// Applies a rewrite to the parts of a formula that are not string literals.
        /// </summary>
        /// <param name="formula">
        /// The formula
        /// </param>
        /// <param name="rewrite">
        /// The rewrite to apply to each part outside a string literal
        /// </param>
        /// <returns>
        /// The rewritten formula
        /// </returns>
        private static string RewriteOutsideStrings(string formula, Func<string, string> rewrite)
        {
            var output = new StringBuilder();
            var segment = new StringBuilder();
            var inString = false;

            for (var index = 0; index < formula.Length; index++)
            {
                var character = formula[index];

                if (character != '"')
                {
                    (inString ? output : segment).Append(character);
                    continue;
                }

                if (!inString)
                {
                    output.Append(rewrite(segment.ToString()));
                    segment.Clear();
                    inString = true;
                }
                else if (index + 1 < formula.Length && formula[index + 1] == '"')
                {
                    // a doubled quote is an escaped quote inside the string, not its end
                    output.Append('"');
                    index++;
                }
                else
                {
                    inString = false;
                }

                output.Append(character);
            }

            output.Append(rewrite(segment.ToString()));

            return output.ToString();
        }

        /// <summary>
        /// A single reference found in a formula, being a cell, a whole row or a whole column.
        /// </summary>
        private sealed class Token
        {
            /// <summary>
            /// A value indicating whether the row part is relative to the cell that holds the formula.
            /// </summary>
            private readonly bool rowIsRelative;

            /// <summary>
            /// A value indicating whether the column part is relative to the cell that holds the formula.
            /// </summary>
            private readonly bool columnIsRelative;

            /// <summary>
            /// The raw sheet qualifier, including its quotes, or null when the reference carries none of its own.
            /// </summary>
            private readonly string sheetText;

            /// <summary>
            /// Initializes a new instance of the <see cref="Token"/> class.
            /// </summary>
            /// <param name="match">
            /// The match the reference was found by
            /// </param>
            /// <param name="oldRow">
            /// The row of the cell that held the formula, counting from one
            /// </param>
            /// <param name="oldColumn">
            /// The column of the cell that held the formula, counting from one
            /// </param>
            public Token(Match match, int oldRow, int oldColumn)
            {
                this.Start = match.Index;
                this.End = match.Index + match.Length;

                if (match.Groups["sheet"].Success)
                {
                    this.sheetText = match.Groups["sheet"].Value;
                    this.Sheet = Unquote(this.sheetText);
                }

                var isRowOnly = match.Groups["rowOnly"].Success;
                var isColumnOnly = match.Groups["columnOnly"].Success;

                this.HasRow = !isColumnOnly;
                this.HasColumn = !isRowOnly;

                if (this.HasRow)
                {
                    var part = isRowOnly ? match.Groups["rowOnly"].Value : match.Groups["row"].Value;
                    this.OldTargetRow = ParsePart(part, oldRow, out this.rowIsRelative);
                }

                if (this.HasColumn)
                {
                    var part = isColumnOnly ? match.Groups["columnOnly"].Value : match.Groups["column"].Value;
                    this.OldTargetColumn = ParsePart(part, oldColumn, out this.columnIsRelative);
                }
            }

            /// <summary>
            /// Gets the position of the reference in the text it was found in.
            /// </summary>
            public int Start { get; }

            /// <summary>
            /// Gets the position just after the reference in the text it was found in.
            /// </summary>
            public int End { get; }

            /// <summary>
            /// Gets the name of the sheet the reference points at, or null for the sheet of the formula itself.
            /// </summary>
            public string Sheet { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the reference has a row part.
            /// </summary>
            public bool HasRow { get; }

            /// <summary>
            /// Gets a value indicating whether the reference has a column part.
            /// </summary>
            public bool HasColumn { get; }

            /// <summary>
            /// Gets the row the reference pointed at before the rebuild, counting from one.
            /// </summary>
            public int OldTargetRow { get; }

            /// <summary>
            /// Gets the column the reference pointed at before the rebuild, counting from one.
            /// </summary>
            public int OldTargetColumn { get; }

            /// <summary>
            /// Gets or sets the row the reference points at after the rebuild, or null when that row no longer exists.
            /// </summary>
            public int? NewTargetRow { get; set; }

            /// <summary>
            /// Gets or sets the column the reference points at after the rebuild, or null when that column no longer exists.
            /// </summary>
            public int? NewTargetColumn { get; set; }

            /// <summary>
            /// Gets or sets the reference that is the other end of the range this reference starts, or null when it starts none.
            /// </summary>
            public Token RangeEnd { get; set; }

            /// <summary>
            /// Gets a value indicating whether the reference points at a row or column that no longer exists.
            /// </summary>
            public bool IsBroken => (this.HasRow && this.NewTargetRow == null) || (this.HasColumn && this.NewTargetColumn == null);

            /// <summary>
            /// Takes over the sheet of the reference that starts the range this reference ends, when it has none of its own.
            /// </summary>
            /// <param name="start">
            /// The reference that starts the range
            /// </param>
            public void InheritSheet(Token start)
            {
                if (this.Sheet == null)
                {
                    this.Sheet = start.Sheet;
                }
            }

            /// <summary>
            /// Locates the cell the reference points at after the rebuild.
            /// </summary>
            /// <param name="mapRow">
            /// Maps a row of the named sheet to its new row
            /// </param>
            /// <param name="mapColumn">
            /// Maps a column of the named sheet to its new column
            /// </param>
            public void Resolve(Func<string, int, int?> mapRow, Func<string, int, int?> mapColumn)
            {
                if (this.HasRow)
                {
                    this.NewTargetRow = mapRow(this.Sheet, this.OldTargetRow);
                }

                if (this.HasColumn)
                {
                    this.NewTargetColumn = mapColumn(this.Sheet, this.OldTargetColumn);
                }
            }

            /// <summary>
            /// Writes the reference out again for its new target, in its original relative or absolute style.
            /// </summary>
            /// <param name="newRow">
            /// The row of the cell that will hold the formula, counting from one
            /// </param>
            /// <param name="newColumn">
            /// The column of the cell that will hold the formula, counting from one
            /// </param>
            /// <returns>
            /// The reference in R1C1 notation
            /// </returns>
            public string Render(int newRow, int newColumn)
            {
                // the far end of a range inherited its sheet but must not repeat it, which is why the raw qualifier is used
                var sheet = this.sheetText != null ? $"{this.sheetText}!" : string.Empty;
                var row = this.HasRow ? $"R{Part(this.rowIsRelative, this.NewTargetRow.Value, newRow)}" : string.Empty;
                var column = this.HasColumn ? $"C{Part(this.columnIsRelative, this.NewTargetColumn.Value, newColumn)}" : string.Empty;

                return $"{sheet}{row}{column}";
            }

            /// <summary>
            /// Resolves a row or column part to the row or column it points at.
            /// </summary>
            /// <param name="part">
            /// The part, being empty, an offset in square brackets, or an absolute number
            /// </param>
            /// <param name="origin">
            /// The row or column of the cell that holds the formula
            /// </param>
            /// <param name="isRelative">
            /// Set to a value indicating whether the part is relative
            /// </param>
            /// <returns>
            /// The row or column the part points at, counting from one
            /// </returns>
            private static int ParsePart(string part, int origin, out bool isRelative)
            {
                if (string.IsNullOrEmpty(part))
                {
                    isRelative = true;
                    return origin;
                }

                if (part[0] == '[')
                {
                    isRelative = true;
                    return origin + int.Parse(part.Substring(1, part.Length - 2), CultureInfo.InvariantCulture);
                }

                isRelative = false;
                return int.Parse(part, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Removes the quotes around a sheet name, and undoes the doubling of quotes inside it.
            /// </summary>
            /// <param name="sheet">
            /// The sheet qualifier as written in the formula
            /// </param>
            /// <returns>
            /// The name of the sheet
            /// </returns>
            private static string Unquote(string sheet) =>
                sheet.Length > 1 && sheet[0] == '\'' ? sheet.Substring(1, sheet.Length - 2).Replace("''", "'") : sheet;

            /// <summary>
            /// Writes a row or column part for its new target.
            /// </summary>
            /// <param name="isRelative">
            /// A value indicating whether the part was relative
            /// </param>
            /// <param name="target">
            /// The row or column the part points at, counting from one
            /// </param>
            /// <param name="origin">
            /// The row or column of the cell that will hold the formula
            /// </param>
            /// <returns>
            /// The part in R1C1 notation
            /// </returns>
            private static string Part(bool isRelative, int target, int origin) =>
                isRelative ? RelativePart(target - origin) : target.ToString(CultureInfo.InvariantCulture);
        }
    }
}
