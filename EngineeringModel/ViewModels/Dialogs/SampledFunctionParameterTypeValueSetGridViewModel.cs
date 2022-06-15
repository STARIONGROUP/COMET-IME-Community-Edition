// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampledFunctionParameterTypeValueSetGridViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// A view model for displaying and editing the valuesets of SampledFunctionParameterType
    /// </summary>
    public class SampledFunctionParameterTypeValueSetGridViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsParameterOrOverride" />
        /// </summary>
        private bool isParameterOrOverride;

        /// <summary>
        /// The backing field for the <see cref="Switch" /> property.
        /// </summary>
        private ParameterSwitchKind? switchValue;

        /// <summary>
        /// Backing field for the <see cref="ValueColumns" />
        /// </summary>
        private ReactiveList<ParameterTypeAllocationColumn> valueColumns;

        /// <summary>
        /// Backing field for <see cref="IsValueSetEditable"/>
        /// </summary>
        private bool isValueSetEditable;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampledFunctionParameterTypeValueSetGridViewModel" /> class.
        /// </summary>
        /// <param name="thing">The <see cref="IValueSet" /></param>
        public SampledFunctionParameterTypeValueSetGridViewModel(IValueSet thing, SampledFunctionParameterType parameterType, bool isValueSetEditable)
        {
            this.ValueSet = thing;
            this.ParameterType = parameterType;
            this.IsValueSetEditable = isValueSetEditable;

            this.IsParameterOrOverride = false;

            this.ValueColumns = new ReactiveList<ParameterTypeAllocationColumn>();
        }

        /// <summary>
        /// Gets the <see cref="Thing" /> that is represented by the view-model
        /// </summary>
        public IValueSet ValueSet { get; private set; }

        /// <summary>
        /// Gets the <see cref="SampledFunctionParameterType" /> that is represented by the view-model
        /// </summary>
        public SampledFunctionParameterType ParameterType { get; private set; }

        /// <summary>
        /// Gets or sets the value table for SampledFunctionParameterType
        /// </summary>
        public DataTable ActualValueTable { get; set; }

        /// <summary>
        /// Gets or sets the value table for SampledFunctionParameterType
        /// </summary>
        public DataTable ManualValueTable { get; set; }

        /// <summary>
        /// Gets or sets the value table for SampledFunctionParameterType
        /// </summary>
        public DataTable ReferenceValueTable { get; set; }

        /// <summary>
        /// Gets or sets the value table for SampledFunctionParameterType
        /// </summary>
        public DataTable ComputedValueTable { get; set; }

        /// <summary>
        /// Gets or sets the value table for SampledFunctionParameterType
        /// </summary>
        public DataTable PublishedValueTable { get; set; }

        /// <summary>
        /// Gets or sets the value data grid columns
        /// </summary>
        public ReactiveList<ParameterTypeAllocationColumn> ValueColumns
        {
            get { return this.valueColumns; }
            set { this.RaiseAndSetIfChanged(ref this.valueColumns, value); }
        }

        /// <summary>
        /// Gets or sets the value indicating whether <see cref="PublishedValueTable" /> is used
        /// </summary>
        public bool IsParameterOrOverride
        {
            get { return this.isParameterOrOverride; }
            set { this.RaiseAndSetIfChanged(ref this.isParameterOrOverride, value); }
        }

        /// <summary>
        /// Gets or sets the value indicating whether value set is editable
        /// </summary>
        public bool IsValueSetEditable
        {
            get { return this.isValueSetEditable; }
            set { this.RaiseAndSetIfChanged(ref this.isValueSetEditable, value); }
        }

        /// <summary>
        /// Gets or sets the switch.
        /// </summary>
        public ParameterSwitchKind? Switch
        {
            get { return this.switchValue; }
            set { this.RaiseAndSetIfChanged(ref this.switchValue, value); }
        }

        /// <summary>
        /// Resets the data tables
        /// </summary>
        private void ResetDataTables()
        {
            this.ActualValueTable = this.ResetDataTable();
            this.ManualValueTable = this.ResetDataTable();
            this.ReferenceValueTable = this.ResetDataTable();
            this.ComputedValueTable = this.ResetDataTable();
            this.PublishedValueTable = this.ResetDataTable();
        }

        /// <summary>
        /// Resets a single data table
        /// </summary>>
        private DataTable ResetDataTable()
        {
            var table = new DataTable();
            table.Rows.Clear();
            table.Columns.Clear();

            return table;
        }

        /// <summary>
        /// Add a column to all the tables.
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="objectType">The type of cell object</param>
        private void AddColumnToTables(string name, Type objectType)
        {
            this.ManualValueTable.Columns.Add(name, objectType);
            this.ActualValueTable.Columns.Add(name, objectType);
            this.ReferenceValueTable.Columns.Add(name, objectType);
            this.ComputedValueTable.Columns.Add(name, objectType);
            this.PublishedValueTable.Columns.Add(name, objectType);
        }

        /// <summary>
        /// Populates the value array grid for <see cref="SampledFunctionParameterType" />
        /// </summary>
        public void PopulateSampledFunctionParameterTypeValueGrid()
        {
            this.Switch = this.ValueSet.ValueSwitch;

            this.ResetDataTables();

            this.ValueColumns.Clear();

            var columns = this.ParameterType.NumberOfValues;

            if (this.ParameterType == null)
            {
                return;
            }

            foreach (var parameterTypeAssignment in this.ParameterType.IndependentParameterType.ToList())
            {
                if (parameterTypeAssignment.ParameterType is CompoundParameterType compoundParameterType)
                {
                    // add a column for each component
                    foreach (ParameterTypeComponent parameterTypeComponent in compoundParameterType.Component)
                    {
                        var columnName = parameterTypeComponent.ShortName;

                        this.AddColumnToTables(columnName, typeof(object));

                        this.ValueColumns.Add(
                            new ParameterTypeAllocationColumn
                            {
                                FieldName = columnName,
                                DisplayName = parameterTypeComponent.Scale != null ? $"{columnName} [{parameterTypeComponent.Scale.ShortName}]" : columnName,
                                Assignment = parameterTypeAssignment
                            });
                    }
                }
                else
                {
                    var columnName = parameterTypeAssignment.ParameterType.ShortName;
                    this.AddColumnToTables(columnName, typeof(object));

                    this.ValueColumns.Add(
                        new ParameterTypeAllocationColumn
                        {
                            FieldName = columnName,
                            DisplayName = parameterTypeAssignment.MeasurementScale != null ? $"{columnName} [{parameterTypeAssignment.MeasurementScale.ShortName}]" : columnName,
                            Assignment = parameterTypeAssignment
                        });
                }
            }

            foreach (var parameterTypeAssignment in this.ParameterType.DependentParameterType.ToList())
            {
                if (parameterTypeAssignment.ParameterType is CompoundParameterType compoundParameterType)
                {
                    // add a column for each component
                    foreach (ParameterTypeComponent parameterTypeComponent in compoundParameterType.Component)
                    {
                        var columnName = parameterTypeComponent.ShortName;
                        this.AddColumnToTables(columnName, typeof(object));

                        this.ValueColumns.Add(
                            new ParameterTypeAllocationColumn
                            {
                                FieldName = columnName,
                                DisplayName = parameterTypeComponent.Scale != null ? $"{columnName} [{parameterTypeComponent.Scale.ShortName}]" : columnName,
                                Assignment = parameterTypeAssignment
                            });
                    }
                }
                else
                {
                    var columnName = parameterTypeAssignment.ParameterType.ShortName;
                    this.AddColumnToTables(columnName, typeof(object));

                    this.ValueColumns.Add(
                        new ParameterTypeAllocationColumn
                        {
                            FieldName = columnName,
                            DisplayName = parameterTypeAssignment.MeasurementScale != null ? $"{columnName} [{parameterTypeAssignment.MeasurementScale.ShortName}]" : columnName,
                            Assignment = parameterTypeAssignment
                        });
                }
            }

            this.AddRowsToTables(columns);
        }

        /// <summary>
        /// Adds rows to all tables
        /// </summary>
        /// <param name="columns">The number of columns</param>
        private void AddRowsToTables(int columns)
        {
            this.AddRowsToTable(this.ValueSet.Manual, this.ManualValueTable, columns);
            this.AddRowsToTable(this.ValueSet.ActualValue, this.ActualValueTable, columns);
            this.AddRowsToTable(this.ValueSet.Reference, this.ReferenceValueTable, columns);
            this.AddRowsToTable(this.ValueSet.Computed, this.ComputedValueTable, columns);

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                this.IsParameterOrOverride = true;
                this.AddRowsToTable(parameterValueSetBase.Published, this.PublishedValueTable, columns);
            }
        }

        /// <summary>
        /// Adds rows to a specific table
        /// </summary>
        /// <param name="valueArray">The value array with data</param>
        /// <param name="table">The data table to fill</param>
        /// <param name="columns">The number of columns</param>
        private void AddRowsToTable(ValueArray<string> valueArray, DataTable table, int columns)
        {
            foreach (var valueChunk in this.SplitValues(valueArray, columns))
            {
                var rowValue = table.NewRow();
                var valueCounter = 0;

                foreach (var value in valueChunk)
                {
                    rowValue[valueCounter] = value;
                    valueCounter++;
                }

                table.Rows.Add(rowValue);
            }
        }

        /// <summary>
        /// Updates the value sets
        /// </summary>
        public void UpdateSampledFunctionParameterValueSet(IValueSet valueset)
        {
            if (valueset is ParameterValueSetBase parameterValueSetBase)
            {
                parameterValueSetBase.Manual = this.CompileTableIntoValueArray(this.ManualValueTable);
                parameterValueSetBase.Computed = this.CompileTableIntoValueArray(this.ComputedValueTable);
                parameterValueSetBase.Reference = this.CompileTableIntoValueArray(this.ReferenceValueTable);

                var parameterSwitchKind = this.Switch;

                if (parameterSwitchKind != null)
                {
                    parameterValueSetBase.ValueSwitch = parameterSwitchKind.Value;
                }
            }
            else if (valueset is ParameterSubscriptionValueSet subscriptionValueSet)
            {
                subscriptionValueSet.Manual = this.CompileTableIntoValueArray(this.ManualValueTable);

                var parameterSwitchKind = this.Switch;

                if (parameterSwitchKind != null)
                {
                    subscriptionValueSet.ValueSwitch = parameterSwitchKind.Value;
                }
            }
        }

        /// <summary>
        /// Compiles value arrays from the data table.
        /// </summary>
        /// <param name="table">The table to collapse</param>
        /// <returns>The value array containig the new values</returns>
        public ValueArray<string> CompileTableIntoValueArray(DataTable table)
        {
            var result = new List<string>();

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var columnCounter = 0;

                foreach (var o in row.ItemArray)
                {
                    result.Add(o.ToValueSetString(this.ValueColumns[columnCounter].Assignment.ParameterType));
                    columnCounter++;
                }
            }

            return new ValueArray<string>(result);
        }

        /// <summary>
        /// Splits the valueset into chunks based on number of independent and dependent parametertype allocations
        /// </summary>
        /// <param name="values">The entire value array</param>
        /// <param name="nSize">The size of chunks to split into</param>
        /// <returns>An IEnumerable of the lists of chunks.</returns>
        private IEnumerable<List<string>> SplitValues(ValueArray<string> values, int nSize = 30)
        {
            for (var i = 0; i < values.Count; i += nSize)
            {
                yield return values.ToList().GetRange(i, Math.Min(nSize, values.Count - i));
            }
        }
    }
}
