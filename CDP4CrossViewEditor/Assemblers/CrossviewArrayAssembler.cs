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

    using HeaderColumn = System.ValueTuple<string, string, string, string, string, string>;

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
        /// The header structure mapping parameter layer short names to indices:
        /// ParameterType -> ParameterTypeComponent -> MeasurementScale -> Option -> ActualFiniteStateList -> ActualFiniteState -> index
        /// </summary>
        internal readonly
            Dictionary<string,
                SortedDictionary<string,
                    SortedDictionary<string,
                        SortedDictionary<string,
                            SortedDictionary<string,
                                SortedDictionary<string,
                                    int>>>>>> headerDictionary
            = new Dictionary<string,
                SortedDictionary<string,
                    SortedDictionary<string,
                        SortedDictionary<string,
                            SortedDictionary<string,
                                SortedDictionary<string,
                                int>>>>>>();

        /// <summary>
        /// Gets the array that contains the parameter sheet information
        /// </summary>
        public object[,] ContentArray { get; private set; }

        /// <summary>
        /// Gets the array that contains formatting information
        /// </summary>
        public object[,] FormatArray { get; private set; }

        /// <summary>
        /// Gets the array that contains locked cells information
        /// </summary>
        public object[,] LockArray { get; private set; }

        /// <summary>
        /// Gets the array that contains name information
        /// </summary>
        public object[,] NamesArray { get; private set; }

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
            this.NamesArray = new object[,] { };
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
            this.NamesArray = new object[this.ContentArray.GetLength(0), this.ContentArray.GetLength(1)];

            var contentList = new List<object[]>();
            var namesList = new List<object[]>();

            foreach (var content in contentHeader)
            {
                contentList.Add(content);
                namesList.Add(new object[this.numberOfColumns]);
            }

            foreach (var (content, names) in this.excelRows.Select(this.ContentRowSelector))
            {
                contentList.Add(content);
                namesList.Add(names);
            }

            for (var i = 0; i < contentList.Count; i++)
            {
                for (var j = 0; j < contentList[i].Length; j++)
                {
                    this.ContentArray[i, j] = contentList[i][j];
                    this.NamesArray[i, j] = namesList[i][j];
                }
            }
        }

        /// <summary>
        /// Gets all header columns
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{HeaderColumn}"/>
        /// </returns>
        private IEnumerable<HeaderColumn> GetHeaderColumns()
        {
            foreach (var parameterTypeShortName in this.headerDictionary.Keys.ToList())
            {
                var ptcDictionary = this.headerDictionary[parameterTypeShortName];

                foreach (var parameterTypeComponentShortName in ptcDictionary.Keys.ToList())
                {
                    var msDictionary = ptcDictionary[parameterTypeComponentShortName];

                    foreach (var measurementScaleShortName in msDictionary.Keys.ToList())
                    {
                        var oDictionary = msDictionary[measurementScaleShortName];

                        foreach (var optionShortName in oDictionary.Keys.ToList())
                        {
                            var afslDictionary = oDictionary[optionShortName];

                            foreach (var actualFiniteStateListShortName in afslDictionary.Keys.ToList())
                            {
                                var afsDictionary = afslDictionary[actualFiniteStateListShortName];

                                foreach (var actualFiniteStateShortName in afsDictionary.Keys.ToList())
                                {
                                    yield return (parameterTypeShortName,
                                        parameterTypeComponentShortName,
                                        measurementScaleShortName,
                                        optionShortName,
                                        actualFiniteStateListShortName,
                                        actualFiniteStateShortName);
                                }
                            }
                        }
                    }
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
                    foreach (var valueSet in parameter.ValueSet)
                    {
                        this.SetContentColumnIndex(valueSet);
                    }
                }
            }

            foreach (var headerColumn in this.GetHeaderColumns())
            {
                this.SetContentColumnIndex(headerColumn, this.numberOfColumns++);
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

            foreach (var headerColumn in this.GetHeaderColumns())
            {
                var columnIndex = this.GetContentColumnIndex(headerColumn);

                (
                    rows[0, columnIndex],
                    rows[1, columnIndex],
                    rows[2, columnIndex],
                    rows[3, columnIndex],
                    rows[4, columnIndex],
                    rows[5, columnIndex]
                    ) = headerColumn;

                // add square brackets to MeasurementScale short name
                rows[2, columnIndex] = $"[{rows[2, columnIndex]}]";
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
        /// Generate content row and names
        /// </summary>
        /// <param name="excelRow">
        /// Current <see cref="IExcelRow{T}"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.ValueTuple"/> that contains element specific data
        /// </returns>
        private (object[] content, object[] names) ContentRowSelector(IExcelRow<Thing> excelRow)
        {
            var contentRow = new object[this.numberOfColumns];
            var namesRow = new object[this.numberOfColumns];

            contentRow[0] = excelRow.Name;
            contentRow[1] = excelRow.ShortName;
            contentRow[2] = excelRow.Type;
            contentRow[3] = excelRow.Owner;
            contentRow[4] = excelRow.Categories;

            IEnumerable<ParameterOrOverrideBase> parameters;

            switch (excelRow.Thing)
            {
                case ElementUsage elementUsage:
                    parameters = this.parameterTypes
                        .Select(pt => GetParameterOrOverrideBase(elementUsage, pt));

                    break;

                case ElementDefinition elementDefinition:
                    parameters = this.parameterTypes
                        .Select(pt => GetParameterOrOverrideBase(elementDefinition, pt));

                    break;

                default:
                    return (contentRow, namesRow);
            }

            foreach (var parameterOrOverrideBase in parameters.Where(p => p != null))
            {
                foreach (var parameterValueSetBase in parameterOrOverrideBase.ValueSets.Cast<ParameterValueSetBase>())
                {
                    if (parameterOrOverrideBase.ParameterType is CompoundParameterType compoundParameterType)
                    {
                        for (var i = 0; i < compoundParameterType.NumberOfValues; ++i)
                        {
                            var component = compoundParameterType.Component[i];
                            var value = parameterValueSetBase.ActualValue[i];

                            var index = this.GetContentColumnIndex(parameterValueSetBase, component);
                            contentRow[index] = value;
                            namesRow[index] = $"{CrossviewSheetConstants.CrossviewSheetName}_{parameterValueSetBase.ModelCode(i)}";
                        }
                    }
                    else
                    {
                        var index = this.GetContentColumnIndex(parameterValueSetBase);
                        contentRow[index] = parameterValueSetBase.ActualValue.First();
                        namesRow[index] = $"{CrossviewSheetConstants.CrossviewSheetName}_{parameterValueSetBase.ModelCode()}";
                    }
                }
            }

            return (contentRow, namesRow);
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
        /// The <see cref="ParameterOrOverrideBase"/>
        /// </returns>
        private static ParameterOrOverrideBase GetParameterOrOverrideBase(ElementUsage elementUsage, ParameterType parameterType)
        {
            return elementUsage.ParameterOverride.FirstOrDefault(po => po.ParameterType == parameterType)
                   ?? GetParameterOrOverrideBase(elementUsage.ElementDefinition, parameterType);
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
        /// The <see cref="ParameterOrOverrideBase"/>
        /// </returns>
        private static ParameterOrOverrideBase GetParameterOrOverrideBase(ElementDefinition elementDefinition, ParameterType parameterType)
        {
            return elementDefinition.Parameter.FirstOrDefault(p => p.ParameterType == parameterType);
        }

        /// <summary>
        /// Initializes the column index associated to the given <paramref name="parameterValueSet"/>
        /// </summary>
        /// <param name="parameterValueSet">
        /// The given <see cref="ParameterValueSet"/>
        /// </param>
        private void SetContentColumnIndex(ParameterValueSetBase parameterValueSet)
        {
            var parameter = (Parameter)parameterValueSet.Container;

            if (parameter.ParameterType is CompoundParameterType compoundParameterType)
            {
                // NOTE: call Select so that 'component' has a proper type, not object
                foreach (var component in compoundParameterType.Component.Select(x => x))
                {
                    this.SetContentColumnIndex((
                            parameter.ParameterType.ShortName,
                            component.ParameterType.ShortName,
                            component.Scale?.ShortName ?? "",
                            parameterValueSet.ActualOption?.ShortName ?? "",
                            parameter.StateDependence?.ShortName ?? "",
                            parameterValueSet.ActualState?.ShortName ?? ""),
                        -1);
                }
            }
            else
            {
                this.SetContentColumnIndex((
                        parameter.ParameterType.ShortName,
                        "",
                        parameter.Scale?.ShortName ?? "",
                        parameterValueSet.ActualOption?.ShortName ?? "",
                        parameter.StateDependence?.ShortName ?? "",
                        parameterValueSet.ActualState?.ShortName ?? ""),
                    -1);
            }
        }

        /// <summary>
        /// Sets the column index associated to the given <paramref name="headerColumn"/>
        /// to the given column <paramref name="index"/>
        /// </summary>
        /// <param name="headerColumn">
        /// The given <see cref="HeaderColumn"/>
        /// </param>
        /// <param name="index">
        /// The given column index
        /// </param>
        private void SetContentColumnIndex(HeaderColumn headerColumn, int index)
        {
            var (parameterTypeShortName,
                parameterTypeComponentShortName,
                measurementScaleShortName,
                optionShortName,
                actualFiniteStateListShortName,
                actualFiniteStateShortName) = headerColumn;

            if (!this.headerDictionary.ContainsKey(parameterTypeShortName))
            {
                this.headerDictionary[parameterTypeShortName]
                    = new SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, int>>>>>();
            }

            var ptcDictionary = this.headerDictionary[parameterTypeShortName];

            if (!ptcDictionary.ContainsKey(parameterTypeComponentShortName))
            {
                ptcDictionary[parameterTypeComponentShortName]
                    = new SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, int>>>>();
            }

            var msDictionary = ptcDictionary[parameterTypeComponentShortName];

            if (!msDictionary.ContainsKey(measurementScaleShortName))
            {
                msDictionary[measurementScaleShortName]
                    = new SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, int>>>();
            }

            var optionDictionary = msDictionary[measurementScaleShortName];

            if (!optionDictionary.ContainsKey(optionShortName))
            {
                optionDictionary[optionShortName]
                    = new SortedDictionary<string, SortedDictionary<string, int>>();
            }

            var afslDictionary = optionDictionary[optionShortName];

            if (!afslDictionary.ContainsKey(actualFiniteStateListShortName))
            {
                afslDictionary[actualFiniteStateListShortName]
                    = new SortedDictionary<string, int>();
            }

            var afsDictionary = afslDictionary[actualFiniteStateListShortName];

            afsDictionary[actualFiniteStateShortName] = index;
        }

        /// <summary>
        /// Gets the column index associated to the given <paramref name="parameterValueSetBase"/>
        /// </summary>
        /// <param name="parameterValueSetBase">
        /// The given <see cref="ParameterValueSetBase"/>
        /// </param>
        /// <param name="component"></param>
        /// <returns>
        /// The column index
        /// </returns>
        private int GetContentColumnIndex(ParameterValueSetBase parameterValueSetBase, ParameterTypeComponent component = null)
        {
            var parameterOrOverrideBase = (ParameterOrOverrideBase)parameterValueSetBase.Container;

            if (component != null)
            {
                return this.GetContentColumnIndex((
                    parameterOrOverrideBase.ParameterType.ShortName,
                    component.ParameterType.ShortName,
                    component.Scale?.ShortName ?? "",
                    parameterValueSetBase.ActualOption?.ShortName ?? "",
                    parameterOrOverrideBase.StateDependence?.ShortName ?? "",
                    parameterValueSetBase.ActualState?.ShortName ?? ""));
            }

            return this.GetContentColumnIndex((
                parameterOrOverrideBase.ParameterType.ShortName,
                "",
                parameterOrOverrideBase.Scale?.ShortName ?? "",
                parameterValueSetBase.ActualOption?.ShortName ?? "",
                parameterOrOverrideBase.StateDependence?.ShortName ?? "",
                parameterValueSetBase.ActualState?.ShortName ?? ""));
        }

        /// <summary>
        /// Gets the column index associated to the given <paramref name="headerColumn"/>
        /// </summary>
        /// <param name="headerColumn">
        /// The given <see cref="HeaderColumn"/>
        /// </param>
        /// <returns>
        /// The column index
        /// </returns>
        private int GetContentColumnIndex(HeaderColumn headerColumn)
        {
            var (parameterTypeShortName,
                parameterTypeComponentShortName,
                measurementScaleShortName,
                optionShortName,
                actualFiniteStateListShortName,
                actualFiniteStateShortName) = headerColumn;

            return this.headerDictionary
                [parameterTypeShortName]
                [parameterTypeComponentShortName]
                [measurementScaleShortName]
                [optionShortName]
                [actualFiniteStateListShortName]
                [actualFiniteStateShortName];
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
