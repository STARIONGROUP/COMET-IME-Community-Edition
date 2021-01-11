// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewArrayAssembler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.Assemblers
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Generator;
    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    /// <summary>
    /// The purpose of the <see cref="CrossviewArrayAssembler"/> is to create the arrays that are
    /// used to populate the Parameter Sheet
    /// </summary>
    public sealed class CrossviewArrayAssembler
    {
        /// <summary>
        /// Total number of columns
        /// </summary>
        private int numberOfColumns = CrossviewSheetConstants.FixedColumns;

        /// <summary>
        /// The actual number of header nested layers.
        /// This might be lower than <see cref="CrossviewSheetConstants.HeaderDepth"/>
        /// if all values on a layer are missing.
        /// </summary>
        public int ActualHeaderDepth { get; private set; }

        /// <summary>
        /// The <see cref="IExcelRow{T}"/> that are being used to populate the various arrays
        /// </summary>
        private readonly IEnumerable<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// The selection parameter types <see cref= "IEnumerable{ParameterType}"/>
        /// </summary>
        private IEnumerable<ParameterType> parameterTypes;

        /// <summary>
        /// The header structure mapping parameters to indices
        /// </summary>
        internal readonly Dictionary<string, SortedDictionary<string, int>> headerDictionary
            = new Dictionary<string, SortedDictionary<string, int>>();

        /// <summary>
        /// Gets the array that contains the parameter sheet information
        /// </summary>
        public object[,] ContentArray { get; private set; }

        /// <summary>
        /// Gets the array that contains formatting information
        /// </summary>
        public object[,] FormatArray { get; private set; }

        /// <summary>
        /// Gets the array that contains the parameter sheet information
        /// </summary>
        private object[,] FormulaArray { get; set; }

        /// <summary>
        /// Gets the array that contains locked cells information
        /// </summary>
        public object[,] LockArray { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewArrayAssembler"/> class.
        /// </summary>
        /// <param name="excelRows">The excel rows</param>
        /// <param name="parameterTypes">The selection parameter types</param>
        public CrossviewArrayAssembler(IEnumerable<IExcelRow<Thing>> excelRows, IEnumerable<ParameterType> parameterTypes)
        {
            this.excelRows = excelRows;
            this.parameterTypes = parameterTypes;

            this.InitializeArrays();
            this.PopulateContentArray();
            this.PopulateContentLockArray();
            this.PopulateContentFormatArray();
        }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet
        /// </summary>
        private void InitializeArrays()
        {
            this.ContentArray = new object[,] { };
            this.LockArray = new object[,] { };
            this.FormatArray = new object[,] { };
            this.FormulaArray = new object[,] { };
        }

        /// <summary>
        /// Populate the content with data
        /// </summary>
        private void PopulateContentArray()
        {
            this.InitializeHeaderStructure();

            var contentHeader = this.GenerateContentHeader();

            this.ActualHeaderDepth = CrossviewSheetConstants.HeaderDepth;

            this.ContentArray = new object[this.excelRows.Count() + contentHeader.Count, this.numberOfColumns];
            this.LockArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];
            this.FormatArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];
            this.FormulaArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];

            var contentList = new List<object[]>();

            contentList.AddRange(contentHeader);

            contentList.AddRange(this.excelRows.Select(this.ContentRowSelector));

            for (var i = 0; i < contentList.Count; i++)
            {
                for (var j = 0; j < contentList[i].Length; j++)
                {
                    this.ContentArray[i, j] = contentList[i][j];
                }
            }
        }

        /// <summary>
        /// Initializes the header structure mapping parameters to indices
        /// </summary>
        private void InitializeHeaderStructure()
        {
            foreach (var elementDefinition in this.excelRows
                .Where(row => row.Thing is ElementDefinition)
                .Select(row => row.Thing as ElementDefinition))
            {
                foreach (var parameter in elementDefinition.Parameter
                    .Where(parameter => this.parameterTypes.Contains(parameter.ParameterType)))
                {
                    this.SetContentColumnIndex(parameter);
                }
            }

            foreach (var parameterTypeShortName in this.headerDictionary.Keys.ToList())
            {
                foreach (var measurementUnitShortName in this.headerDictionary[parameterTypeShortName].Keys.ToList())
                {
                    this.SetContentColumnIndex(
                        parameterTypeShortName,
                        measurementUnitShortName,
                        this.numberOfColumns++);
                }
            }
        }

        /// <summary>
        /// Generate content header
        /// </summary>
        /// <returns>
        /// Array that contains header specific data
        /// </returns>
        private List<object[]> GenerateContentHeader()
        {
            var contentHeader = new List<object[]>(CrossviewSheetConstants.HeaderDepth);

            var rows = new object[CrossviewSheetConstants.HeaderDepth, this.numberOfColumns];

            for (var i = 0; i < CrossviewSheetConstants.HeaderDepth; ++i)
            {
                rows[i, 0] = "Name";
                rows[i, 1] = "Short Name";
                rows[i, 2] = "Type";
                rows[i, 3] = "Owner";
                rows[i, 4] = "Category";
            }

            foreach (var parameterTypeShortName in this.headerDictionary.Keys)
            {
                foreach (var measurementUnitShortName in this.headerDictionary[parameterTypeShortName].Keys)
                {
                    var columnIndex = this.GetContentColumnIndex(
                        parameterTypeShortName,
                        measurementUnitShortName);

                    rows[0, columnIndex] = parameterTypeShortName;
                    rows[1, columnIndex] = measurementUnitShortName;
                }
            }

            for (var i = 0; i < CrossviewSheetConstants.HeaderDepth; ++i)
            {
                var row = new object[this.numberOfColumns];

                for (var j = 0; j < this.numberOfColumns; ++j)
                {
                    row[j] = rows[i, j];
                }

                contentHeader.Add(row);
            }

            return contentHeader;
        }

        /// <summary>
        /// Generate content row
        /// </summary>
        /// <param name="excelRow">
        /// Current <see cref="IExcelRow{T}"/>
        /// </param>
        /// <returns>
        /// Array that contains element specific data
        /// </returns>
        private object[] ContentRowSelector(IExcelRow<Thing> excelRow)
        {
            var contentRow = new object[this.numberOfColumns];

            contentRow[0] = excelRow.Name;
            contentRow[1] = excelRow.ShortName;
            contentRow[2] = excelRow.Type;
            contentRow[3] = excelRow.Owner;
            contentRow[4] = excelRow.Categories;

            IEnumerable<ParameterBase> parameters;

            switch (excelRow.Thing)
            {
                case ElementUsage elementUsage:
                    parameters = this.parameterTypes
                        .Select(pt => GetParameterBase(elementUsage, pt))
                        .Where(p => p != null);
                    break;

                case ElementDefinition elementDefinition:
                    parameters = this.parameterTypes
                        .Select(pt => GetParameterBase(elementDefinition, pt))
                        .Where(p => p != null);
                    break;

                default:
                    return contentRow;
            }

            foreach (var parameterBase in parameters)
            {
                var valueSet = parameterBase.ValueSets.FirstOrDefault();
                contentRow[this.GetContentColumnIndex(parameterBase)] = valueSet?.ActualValue.FirstOrDefault();
            }

            return contentRow;
        }

        /// <summary>
        /// Gets the <see cref="ParameterOverride"/> corresponding to the given <paramref name="parameterType"/>
        /// from the given <see cref="ElementUsage"/>, if present; otherwise,
        /// gets the <see cref="Parameter"/> corresponding to the given <paramref name="parameterType"/>
        /// from the <see cref="ElementUsage.ElementDefinition"/>.
        /// </summary>
        /// <param name="elementUsage">
        /// The given <see cref="ElementUsage"/>
        /// </param>
        /// <param name="parameterType">
        /// The given <see cref="ParameterType"/>
        /// </param>
        /// <returns>
        /// The <see cref="ParameterBase"/>
        /// </returns>
        private static ParameterBase GetParameterBase(ElementUsage elementUsage, ParameterType parameterType)
        {
            return elementUsage.ParameterOverride.FirstOrDefault(po => po.ParameterType == parameterType)
                   ?? GetParameterBase(elementUsage.ElementDefinition, parameterType);
        }

        /// <summary>
        /// Gets the <see cref="Parameter"/> corresponding to the given <paramref name="parameterType"/>
        /// from the given <see cref="ElementDefinition"/>.
        /// </summary>
        /// <param name="elementDefinition">
        /// The given <see cref="ElementDefinition"/>
        /// </param>
        /// <param name="parameterType">
        /// The given <see cref="ParameterType"/>
        /// </param>
        /// <returns>
        /// The <see cref="ParameterBase"/>
        /// </returns>
        private static ParameterBase GetParameterBase(ElementDefinition elementDefinition, ParameterType parameterType)
        {
            return elementDefinition.Parameter.FirstOrDefault(p => p.ParameterType == parameterType);
        }

        /// <summary>
        /// Sets the column index associated to the given <paramref name="parameterBase"/>
        /// to the given column <paramref name="index"/>
        /// </summary>
        /// <param name="parameterBase">
        /// The given <see cref="ParameterBase"/>
        /// </param>
        /// <param name="index">
        /// The given column index
        /// </param>
        private void SetContentColumnIndex(ParameterBase parameterBase, int index = -1)
        {
            this.SetContentColumnIndex(
                parameterBase.ParameterType.ShortName,
                parameterBase.Scale?.ShortName ?? "",
                index);
        }

        /// <summary>
        /// Sets the column index associated to the given arguments
        /// to the given column <paramref name="index"/>
        /// </summary>
        /// <param name="parameterTypeShortName">
        /// The given <see cref="ParameterType"/> short name
        /// </param>
        /// <param name="measurementScaleShortName">
        /// The given <see cref="MeasurementScale"/> short name
        /// </param>
        /// <param name="index">
        /// The given column index
        /// </param>
        private void SetContentColumnIndex(
            string parameterTypeShortName,
            string measurementScaleShortName,
            int index)
        {
            if (!this.headerDictionary.ContainsKey(parameterTypeShortName))
            {
                this.headerDictionary[parameterTypeShortName] = new SortedDictionary<string, int>();
            }

            var scaleDictionary = this.headerDictionary[parameterTypeShortName];

            scaleDictionary[measurementScaleShortName] = index;
        }

        /// <summary>
        /// Gets the column index associated to the given <paramref name="parameterBase"/>
        /// </summary>
        /// <param name="parameterBase">
        /// The given <see cref="ParameterBase"/>
        /// </param>
        /// <returns>
        /// The column index
        /// </returns>
        private int GetContentColumnIndex(ParameterBase parameterBase)
        {
            return this.GetContentColumnIndex(
                parameterBase.ParameterType.ShortName,
                parameterBase.Scale?.ShortName ?? "");
        }

        /// <summary>
        /// Gets the column index associated to the given arguments
        /// </summary>
        /// <param name="parameterTypeShortName">
        /// The given <see cref="ParameterType"/> short name
        /// </param>
        /// <param name="measurementScaleShortName">
        /// The given <see cref="MeasurementScale"/> short name
        /// </param>
        /// <returns>
        /// The column index
        /// </returns>
        private int GetContentColumnIndex(
            string parameterTypeShortName,
            string measurementScaleShortName)
        {
            return this.headerDictionary
                [parameterTypeShortName]
                [measurementScaleShortName];
        }

        /// <summary>
        /// Populate the <see cref="FormatArray"/> with formatting data
        /// </summary>
        private void PopulateContentFormatArray()
        {
            for (var i = this.FormatArray.GetLowerBound(1); i < this.FormatArray.GetUpperBound(1); i++)
            {
                this.FormatArray[0, i] = "@";
            }
        }

        /// <summary>
        /// Populate the <see cref="LockArray"/> to enable locking and unlocking cells
        /// </summary>
        private void PopulateContentLockArray()
        {
            for (var i = this.LockArray.GetLowerBound(1); i < this.LockArray.GetUpperBound(1); i++)
            {
                this.LockArray[0, i] = true;
            }
        }
    }
}
