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
        private readonly Dictionary<string, int> headerDictionary = new Dictionary<string, int>();

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
            foreach (var parameterType in this.parameterTypes)
            {
                this.headerDictionary[parameterType.ShortName] = this.numberOfColumns++;
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
            var contentHeader = new List<object[]>();

            var row = new object[this.numberOfColumns];

            row[0] = "Name";
            row[1] = "Short Name";
            row[2] = "Type";
            row[3] = "Owner";
            row[4] = "Category";

            foreach (var parameterShortName in this.headerDictionary.Keys)
            {
                row[this.headerDictionary[parameterShortName]] = parameterShortName;
            }

            contentHeader.Add(row);

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

            var parameterList = new List<ParameterBase>();

            switch (excelRow.Thing)
            {
                case ElementDefinition elementDefinition:
                    parameterList = elementDefinition.Parameter.ToList<ParameterBase>();
                    break;

                case ElementUsage elementUsage:
                    parameterList = elementUsage.ParameterOverride.ToList<ParameterBase>();
                    break;
            }

            foreach (var parameterType in this.parameterTypes)
            {
                foreach (var parameter in parameterList)
                {
                    if (parameter.ParameterType != parameterType)
                    {
                        continue;
                    }

                    var valueSet = parameter.ValueSets.FirstOrDefault();
                    contentRow[this.GetContentRowIndex(parameter)] = valueSet?.ActualValue.FirstOrDefault();
                }
            }

            return contentRow;
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
        private int GetContentRowIndex(ParameterBase parameterBase)
        {
            return this.headerDictionary[parameterBase.ParameterType.ShortName];
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
