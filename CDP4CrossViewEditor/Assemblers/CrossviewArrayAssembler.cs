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

    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    /// <summary>
    /// The purpose of the <see cref="CrossviewArrayAssembler"/> is to create the arrays that are
    /// used to populate the Parameter Sheet
    /// </summary>
    public sealed class CrossviewArrayAssembler
    {
        /// <summary>
        /// Initial fixed columns
        /// </summary>
        private const int FixedColumns = 5;

        /// <summary>
        /// Total number of columns
        /// </summary>
        private readonly int numberOfColumns;

        /// <summary>
        /// The <see cref="IExcelRow{T}"/> that are being used to populate the various arrays
        /// </summary>
        private readonly IEnumerable<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewArrayAssembler"/> class.
        /// </summary>
        /// <param name="excelRows">The excel rows</param>
        /// <param name="parameterTypes">The selection parameter types</param>
        public CrossviewArrayAssembler(IEnumerable<IExcelRow<Thing>> excelRows, IEnumerable<ParameterType> parameterTypes)
        {
            this.excelRows = excelRows;
            var parameterTypesList = parameterTypes.ToList();
            this.numberOfColumns = parameterTypesList.Count + FixedColumns;

            this.InitializeArray();
            this.PopulateContentArray(parameterTypesList);
            this.PopulateContentLockArray();
            this.PopulateContentFormatArray();
        }

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
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet
        /// </summary>
        private void InitializeArray()
        {
            this.ContentArray = new object[,] { };
            this.LockArray = new object[,] { };
            this.FormatArray = new object[,] { };
            this.FormulaArray = new object[,] { };
        }

        /// <summary>
        /// Populate the <see cref="ContentArray"/> with data
        /// </summary>
        private void PopulateContentArray(List<ParameterType> parameterTypesList)
        {
            var contentArray = new object[this.numberOfColumns];
            var contentList = new List<object[]>();

            contentArray[0] = "Name";
            contentArray[1] = "Short Name";
            contentArray[2] = "Type";
            contentArray[3] = "Owner";
            contentArray[4] = "Category";

            var index = 0;

            foreach (var parameterType in parameterTypesList)
            {
                contentArray[FixedColumns + index] = parameterType.ShortName;
                index++;
            }

            contentList.Add(contentArray);

            foreach (var excelRow in this.excelRows)
            {
                contentArray = new object[this.numberOfColumns];
                contentArray[0] = excelRow.Name;
                contentArray[1] = excelRow.ShortName;
                contentArray[2] = excelRow.Type;
                contentArray[3] = excelRow.Owner;
                contentArray[4] = excelRow.Categories;

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

                index = 0;

                foreach (var parameterType in parameterTypesList)
                {
                    foreach (var parameter in parameterList)
                    {
                        if (parameter.ParameterType != parameterType)
                        {
                            continue;
                        }

                        var valueSet = parameter.ValueSets.FirstOrDefault();
                        contentArray[FixedColumns + index] = valueSet?.ActualValue.FirstOrDefault();
                        index++;
                    }
                }

                contentList.Add(contentArray);
            }

            var columnsCount = contentArray.Length;
            var tempObjects = new object[this.excelRows.Count() + 2, columnsCount];

            for (var i = 0; i < contentList.Count; i++)
            {
                for (var j = 0; j < contentList[i].Length; j++)
                {
                    tempObjects[i, j] = contentList[i][j];
                }
            }

            this.ContentArray = tempObjects;
            this.LockArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];
            this.FormatArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];
            this.FormulaArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];
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

            //TODO Apply specific format exeption
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

            //TODO Apply specific lock exeption
        }
    }
}
