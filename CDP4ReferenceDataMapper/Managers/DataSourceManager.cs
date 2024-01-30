// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceManager.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4ReferenceDataMapper.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4ReferenceDataMapper.Data;
    using CDP4ReferenceDataMapper.GridColumns;

    using Newtonsoft.Json;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DataSourceManager"/> is manage the content a <see cref="DataTable"/> that is the data-source for a datagrid
    /// displaying the mapping table to the user as well as the dynamic columns of the data-grid
    /// </summary>
    public class DataSourceManager : ReactiveObject
    {
        /// <summary>
        /// The name of the <see cref="DataTable"/>s Iid Column
        /// </summary>
        public const string IidColumnName = "Iid";

        /// <summary>
        /// The name of the <see cref="DataTable"/>s Id Column
        /// </summary>
        public const string IdColumnName = "Id";

        /// <summary>
        /// The name of the <see cref="DataTable"/>s ParentId Column
        /// </summary>
        public const string ParentIdColumnName = "ParentId";

        /// <summary>
        /// The name of the <see cref="DataTable"/>s Label Column
        /// </summary>
        public const string LabelColumnName = "Label";

        /// <summary>
        /// The name of the <see cref="DataTable"/>s Type Column
        /// </summary>
        public const string TypeColumnName = "Type";

        /// <summary>
        /// The value of the Type column in case of a parameter mapping <see cref="DataRow"/>
        /// </summary>
        public const string ParameterMappingType = "PARAM-MAP";

        /// <summary>
        /// The value of the Type column in case of a parameter value <see cref="DataRow"/>
        /// </summary>
        public const string ParameterValueType = "PARAM-VAL";

        /// <summary>
        /// The value of the Type column in case of a parameter value <see cref="DataRow"/>
        /// </summary>
        public const string ElementDefinitionType = "ED";

        /// <summary>
        /// The value of the Type column in case of a parameter value <see cref="DataRow"/>
        /// </summary>
        public const string ElementUsageType = "EU";

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Iteration"/> that contains the data for which the mapper managing the data
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="Category"/> that the <see cref="ElementDefinition"/>s need to be a member of
        /// </summary>
        private readonly Category elementDefinitionCategory;

        /// <summary>
        /// The <see cref="ActualFiniteStateList"/> for which the mapping needs to be managed
        /// </summary>
        private readonly ActualFiniteStateList actualFiniteStateList;

        /// <summary>
        /// Backing field for <see cref="SourceParameterTypes"/>
        /// </summary>
        private ReactiveList<ParameterType> sourceParameterTypes;

        /// <summary>
        /// The <see cref="TextParameterType"/> where the mapping will be stored
        /// </summary>
        private readonly TextParameterType targetMappingParameterType;

        /// <summary>
        /// The <see cref="ScalarParameterType"/> where the mapped value will be stored
        /// </summary>
        private readonly ScalarParameterType targetValueParameterType;

        /// <summary>
        /// A <see cref="HashSet{T}"/> of type <see cref="Thing"/> that contains all the <see cref="Thing"/>s
        /// used for the creation of the <see cref="DataTable"/> property and optionally a cloned thing  when data was changed for the <see cref="Thing"/>.
        /// </summary>
        private HashSet<Thing> Things { get; set; }

        /// <summary>
        /// the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType"/>s to select as the 
        /// source of the mapping
        /// </summary>
        public ReactiveList<ParameterType> SourceParameterTypes
        {
            get => this.sourceParameterTypes;
            set => this.RaiseAndSetIfChanged(ref this.sourceParameterTypes, value);
        }

        /// <summary>
        /// Gets the <see cref="ActualStateDataGridColumn"/> for of the data source
        /// </summary>
        public ReactiveList<DataGridColumn> Columns { get; set; }

        /// <summary>
        /// Gets the <see cref="DataTable"/> for of the data source
        /// </summary>
        public DataTable DataTable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceManager"/> class.
        /// </summary>
        public DataSourceManager()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceManager"/> class.
        /// </summary>
        public DataSourceManager(Iteration iteration, Category elementDefinitionCategory, ActualFiniteStateList actualFiniteStateList, ICollection<ParameterType> sourceParameterTypes, TextParameterType targetMappingParameterType, ScalarParameterType targetValueParameterType)
        {
            this.iteration = iteration ?? throw new ArgumentNullException(nameof(iteration), $"The {nameof(iteration)} may not be null");
            this.elementDefinitionCategory = elementDefinitionCategory ?? throw new ArgumentNullException(nameof(elementDefinitionCategory), $"The {nameof(elementDefinitionCategory)} may not be null");
            this.actualFiniteStateList = actualFiniteStateList ?? throw new ArgumentNullException(nameof(actualFiniteStateList), $"The {nameof(actualFiniteStateList)} may not be null");
            this.targetMappingParameterType = targetMappingParameterType ?? throw new ArgumentNullException(nameof(targetMappingParameterType), $"The {nameof(targetMappingParameterType)} may not be null");
            this.targetValueParameterType = targetValueParameterType ?? throw new ArgumentNullException(nameof(targetValueParameterType), $"The {nameof(targetValueParameterType)} may not be null");

            this.sourceParameterTypes = new ReactiveList<ParameterType>(sourceParameterTypes);

            this.EnrichSourceParameterTypes();

            if (!this.sourceParameterTypes.Any())
            {
                throw new ArgumentException(nameof(sourceParameterTypes), $"The {nameof(sourceParameterTypes)} may not be empty");
            }

            this.Initialize();

            this.UpdateColumns();
            this.UpdateRows();
        }

        /// <summary>
        /// Gets all <see cref="ActualFiniteStateKind.MANDATORY"/> <see cref="ActualFiniteState"/>s from <see cref="actualFiniteStateList"/>
        /// </summary>
        /// <returns><see cref="IEnumerable{ActualFiniteState}"/> of the mandatory finite states</returns>
        private IEnumerable<ActualFiniteState> GetMandatoryFiniteStates()
        {
            return this.actualFiniteStateList.ActualState.Where(x => x.Kind == ActualFiniteStateKind.MANDATORY);
        }

        /// <summary>
        /// Initialize this instance of <see cref="DataSourceManager"/>
        /// </summary>
        private void Initialize()
        {
            this.Columns = new ReactiveList<DataGridColumn>();
            this.DataTable = new DataTable();
            this.Things = new HashSet<Thing>();
        }

        /// <summary>
        /// Returns the ShortName column name for a column holding the current value
        /// </summary>
        /// <param name="currentValueColumnName">The current value column</param>
        /// <returns>Name of the column holding the original value</returns>
        public string GetShortNameColumnName(string currentValueColumnName)
        {
            return $"{currentValueColumnName}_shortname";
        }

        /// <summary>
        /// Returns the original value's column name for a column holding the current value
        /// </summary>
        /// <param name="currentValueColumnName">The current value column</param>
        /// <returns>Name of the column holding the original value</returns>
        public string GetOrgValueColumnName(string currentValueColumnName)
        {
            return $"{currentValueColumnName}_orgvalue";
        }

        /// <summary>
        /// Checks if a column is an <see cref="ActualFiniteState"/> column.
        /// </summary>
        /// <param name="fieldName">The column name</param>
        /// <returns>true if the field name belongs to a mapping column, otherwise false</returns>
        public bool IsActualStateColumn(string fieldName)
        {
            return this.Columns.Any(
                x =>
                    x is ActualStateDataGridColumn actualStateDataGridColumn
                    && actualStateDataGridColumn.ActualFiniteState.ShortName == fieldName);
        }

        /// <summary>
        /// Refreshes all values for mapped state values
        /// </summary>
        /// <returns>An integer value representing the number of values that were actually updated, meaning that their value was actually changed</returns>
        public int RefreshAllMappedValues()
        {
            var updateCount = 0;

            foreach (DataRow row in this.DataTable.Rows)
            {
                if (row[TypeColumnName].ToString() == ParameterMappingType)
                {
                    foreach (var actualState in this.GetMandatoryFiniteStates())
                    {
                        if (this.TryRefreshMappedValue(row, actualState.ShortName))
                        {
                            updateCount++;
                        }
                    }
                }
            }

            return updateCount;
        }

        /// <summary>
        /// Refreshes a specific state value for a specific mapping type (PARAM-MAP) <see cref="DataRow"/>
        /// </summary>
        /// <param name="row">The <see cref="DataRow"/></param>
        /// <param name="actualState">The shortname of the <see cref="ActualFiniteState"/> to update</param>
        public bool TryRefreshMappedValue(DataRow row, string actualState)
        {
            if (row[TypeColumnName].ToString() != ParameterMappingType)
            {
                return false;
            }

            var valueRow = this.GetValueRowByMappingRow(row);
            var parameterTypeIid = row[actualState].ToString();

            if (valueRow != null)
            {
                var newPropertyValue =
                    string.IsNullOrWhiteSpace(parameterTypeIid)
                        ? null
                        : this.GetElementDefinitionParameterValueForDataRow(row, new Guid(parameterTypeIid));

                var updated = valueRow[actualState].ToString() != newPropertyValue;

                this.SetValue(actualState, valueRow, newPropertyValue);

                var newPropertyCustomName =
                    string.IsNullOrWhiteSpace(parameterTypeIid)
                        ? null
                        : this.GetElementDefinitionParameterCustomShortNameForDataRow(row, new Guid(parameterTypeIid));

                this.SetValue(this.GetShortNameColumnName(actualState), valueRow, newPropertyCustomName);

                return updated;
            }

            return false;
        }

        /// <summary>
        /// Adds and removes the <see cref="Columns"/> as well as the column definitions of the <see cref="DataTable"/>
        /// </summary>
        private void UpdateColumns()
        {
            if (this.Columns.All(x => x.FieldName != IidColumnName))
            {
                this.Columns.Add(new DataGridColumn(this) { FieldName = IidColumnName, Visible = false });
                this.DataTable.Columns.Add(IidColumnName, typeof(Guid));
                this.DataTable.Columns.Add(this.GetOrgValueColumnName(IidColumnName), typeof(Guid));
            }

            if (this.Columns.All(x => x.FieldName != IdColumnName))
            {
                this.Columns.Add(new DataGridColumn(this) { FieldName = IdColumnName, Visible = false });
                this.DataTable.Columns.Add(IdColumnName, typeof(int));
            }

            if (this.Columns.All(x => x.FieldName != ParentIdColumnName))
            {
                this.Columns.Add(new DataGridColumn(this) { FieldName = ParentIdColumnName, Visible = false });
                this.DataTable.Columns.Add(ParentIdColumnName, typeof(int));
            }

            if (this.Columns.All(x => x.FieldName != LabelColumnName))
            {
                this.Columns.Add(new DataGridColumn(this) { FieldName = LabelColumnName });
                this.DataTable.Columns.Add(LabelColumnName, typeof(string));
                this.DataTable.Columns.Add(this.GetOrgValueColumnName(LabelColumnName), typeof(string));
            }

            if (this.Columns.All(x => x.FieldName != TypeColumnName))
            {
                this.Columns.Add(new DataGridColumn(this) { FieldName = TypeColumnName, Visible = false });
                this.DataTable.Columns.Add(TypeColumnName, typeof(string));
            }

            var newActualFiniteStates =
                this.GetMandatoryFiniteStates()
                    .Except(this.Columns.OfType<ActualStateDataGridColumn>().Select(x => x.ActualFiniteState))
                    .ToList();

            newActualFiniteStates.Sort(new ActualFiniteStateComparer());

            var oldActualFiniteStates = this.Columns.OfType<ActualStateDataGridColumn>().Select(x => x.ActualFiniteState).Except(this.actualFiniteStateList.ActualState).ToList();

            foreach (var actualFiniteState in newActualFiniteStates)
            {
                var actualStateDataGridColumn = new ActualStateDataGridColumn(this, actualFiniteState) { AllowEditing = true };
                this.Columns.Add(actualStateDataGridColumn);
                this.DataTable.Columns.Add(actualStateDataGridColumn.FieldName, typeof(string));
                this.DataTable.Columns.Add(this.GetOrgValueColumnName(actualStateDataGridColumn.FieldName), typeof(string));
                this.DataTable.Columns.Add(this.GetShortNameColumnName(actualStateDataGridColumn.FieldName), typeof(string));
                this.DataTable.Columns.Add(this.GetOrgValueColumnName(this.GetShortNameColumnName(actualStateDataGridColumn.FieldName)), typeof(string));
            }

            foreach (var actualFiniteState in oldActualFiniteStates)
            {
                var actualStateDataGridColumn = this.Columns.OfType<ActualStateDataGridColumn>().SingleOrDefault(x => x.ActualFiniteState == actualFiniteState);

                if (actualStateDataGridColumn != null)
                {
                    this.Columns.Remove(actualStateDataGridColumn);
                    this.DataTable.Columns.Remove(actualStateDataGridColumn.FieldName);
                    this.DataTable.Columns.Remove(this.GetOrgValueColumnName(actualStateDataGridColumn.FieldName));
                    this.DataTable.Columns.Remove(this.GetOrgValueColumnName(this.GetShortNameColumnName(actualStateDataGridColumn.FieldName)));
                }
            }
        }

        /// <summary>
        /// Update the rows of the <see cref="DataTable"/>
        /// </summary>
        private void UpdateRows()
        {
            this.DataTable.Clear();
            this.Things.Clear();

            var rowNumber = 0;

            foreach (var elementDefinition in this.iteration.Element)
            {
                rowNumber++;
                var elementRowNumber = rowNumber;

                if (!elementDefinition.IsMemberOfCategory(this.elementDefinitionCategory))
                {
                    continue;
                }

                this.AddRow(
                    rowNumber,
                    null,
                    ElementDefinitionType,
                    $"{elementDefinition.Name} [{elementDefinition.ShortName}]",
                    elementDefinition);

                var elementUsages = this.QueryElementUsages(elementDefinition);

                foreach (var elementUsage in elementUsages)
                {
                    rowNumber++;
                    var usageRowNumber = rowNumber;

                    var elementUsageRow =
                        this.AddRow(
                            rowNumber,
                            elementRowNumber,
                            ElementUsageType,
                            $"{elementUsage.Name} [{elementUsage.ModelCode()}]",
                            elementUsage);

                    foreach (var actualState in this.GetMandatoryFiniteStates())
                    {
                        elementUsageRow[actualState.ShortName] = $"{actualState.ShortName}";
                    }

                    var valueParameters =
                        elementUsage.ParameterOverride
                            .Where(x => x.ParameterType == this.targetValueParameterType);

                    foreach (var valueParameter in valueParameters)
                    {
                        if (valueParameter.StateDependence == this.actualFiniteStateList)
                        {
                            var mappingParameter = elementUsage.ParameterOverride.FirstOrDefault(x => x.ParameterType == this.targetMappingParameterType);

                            if (mappingParameter != null)

                            {
                                rowNumber++;

                                var mappingRow =
                                    this.AddRow(
                                        rowNumber,
                                        usageRowNumber,
                                        ParameterMappingType,
                                        $"{valueParameter.ParameterType?.Name} [{valueParameter.ParameterType?.ShortName}]",
                                        mappingParameter);

                                rowNumber++;

                                var valueRow =
                                    this.AddRow(
                                        rowNumber,
                                        usageRowNumber,
                                        ParameterValueType,
                                        $"{valueParameter.ParameterType?.Name} [{valueParameter.ParameterType?.ShortName}]",
                                        valueParameter);

                                foreach (var currentValueSet in valueParameter.ValueSets.Where(x => x.ActualState.Kind == ActualFiniteStateKind.MANDATORY))
                                {
                                    var columnName = currentValueSet.ActualState.ShortName;
                                    var mappingParameterValue = mappingParameter.ValueSets.FirstOrDefault()?.Computed[0];
                                    var currentValueSetValue = currentValueSet.Computed[0];

                                    valueRow[columnName] = currentValueSetValue;

                                    if (mappingParameterValue != null)
                                    {
                                        try
                                        {
                                            var parameterToStateMapping = JsonConvert.DeserializeObject<List<ParameterToStateMapping>>(mappingParameterValue);

                                            if (parameterToStateMapping?.Any() ?? false)
                                            {
                                                var originallySavedMapping =
                                                    parameterToStateMapping
                                                        .SingleOrDefault(
                                                            x =>
                                                                x.ActualFiniteStateIid == currentValueSet.ActualState.Iid);

                                                if (currentValueSetValue == originallySavedMapping?.Value)
                                                {
                                                    valueRow[columnName] = originallySavedMapping?.Value;
                                                    valueRow[this.GetShortNameColumnName(columnName)] = originallySavedMapping?.ShortName;
                                                    mappingRow[columnName] = originallySavedMapping?.ParameterTypeIid;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            // ignored
                                        }
                                    }
                                }

                                this.CopyCurrentValuesToOrgValueColumns(valueRow);
                                this.CopyCurrentValuesToOrgValueColumns(mappingRow);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enriches the <see cref="SourceParameterTypes"/> property with previously mapped <see cref="ParameterType"/>s
        /// </summary>
        private void EnrichSourceParameterTypes()
        {
            if (!(this.iteration.Container is EngineeringModel model))
            {
                return;
            }

            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedParameterTypes = new List<ParameterType>();
            var allParameterTypes = mrdl.ParameterType.Union(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType)).ToList();

            allowedParameterTypes.AddRange(allParameterTypes.OfType<ScalarParameterType>());

            allowedParameterTypes.AddRange(
                allParameterTypes
                    .OfType<CompoundParameterType>()
                    .Where(
                        x =>
                            x.Component.Count > 1
                            && x.Component[0].ParameterType is TextParameterType
                            && x.Component[1].ParameterType is ScalarParameterType));

            foreach (var elementDefinition in this.iteration.Element)
            {
                if (!elementDefinition.IsMemberOfCategory(this.elementDefinitionCategory))
                {
                    continue;
                }

                var elementUsages = this.QueryElementUsages(elementDefinition);

                foreach (var elementUsage in elementUsages)
                {
                    var exceptParameterTypes = elementUsage.ParameterOverride
                        .Select(y => y.ParameterType);

                    var valueParameters =
                        elementUsage.ParameterOverride.Cast<ParameterOrOverrideBase>()
                            .Union(
                                elementDefinition.Parameter
                                    .Where(x => !exceptParameterTypes.Contains(x.ParameterType)))
                            .Where(x => x.ParameterType == this.targetValueParameterType);

                    foreach (var valueParameter in valueParameters)
                    {
                        if (valueParameter.StateDependence != this.actualFiniteStateList)
                        {
                            continue;
                        }

                        var mappingParameter = elementUsage.ParameterOverride.FirstOrDefault(x => x.ParameterType == this.targetMappingParameterType);

                        var mappingParameterValue = mappingParameter?.ValueSets.FirstOrDefault()?.Computed[0];

                        if (mappingParameterValue == null)
                        {
                            continue;
                        }

                        try
                        {
                            var parameterToStateMappings = JsonConvert.DeserializeObject<List<ParameterToStateMapping>>(mappingParameterValue);

                            if (parameterToStateMappings?.Any() ?? false)
                            {
                                foreach (var parameterToStateMapping in parameterToStateMappings)
                                {
                                    var parameterType = allowedParameterTypes.FirstOrDefault(x => x.Iid == parameterToStateMapping.ParameterTypeIid);

                                    if (parameterType != null && !this.SourceParameterTypes.Contains(parameterType))
                                    {
                                        this.SourceParameterTypes.Add(parameterType);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets some data on a <see cref="DataRow"/> and adds data it to the appropriate properties.
        /// </summary>
        /// <param name="columnId">The <see cref="DataRow"/>s column id.</param>
        /// <param name="parentColumnId">The <see cref="DataRow"/>s parentcolumn id.</param>
        /// <param name="type">The <see cref="DataRow"/>s type.</param>
        /// <param name="label">The <see cref="DataRow"/>s label.</param>
        /// <param name="thing">The <see cref="Thing"/> to be added to the <see cref="Things"/> property.</param>
        private DataRow AddRow(int columnId, int? parentColumnId, string type, string label, Thing thing)
        {
            var dataRow = this.DataTable.NewRow();

            dataRow[IidColumnName] = thing.Iid;
            dataRow[IdColumnName] = columnId;

            if (parentColumnId.HasValue)
            {
                dataRow[ParentIdColumnName] = parentColumnId;
            }

            dataRow[TypeColumnName] = type;
            dataRow[LabelColumnName] = label;

            foreach (var actualFiniteState in this.GetMandatoryFiniteStates())
            {
                dataRow[actualFiniteState.ShortName] = string.Empty;
            }

            this.DataTable.Rows.Add(dataRow);

            if (!this.Things.Contains(thing))
            {
                this.Things.Add(thing);
            }

            return dataRow;
        }

        /// <summary>
        /// Queries the <see cref="ElementUsage"/>s from the <see cref="iteration"/> that reference the
        /// provided <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDefinition">
        /// The subject <see cref="ElementDefinition"/>
        /// </param>
        /// <returns></returns>
        private IEnumerable<ElementUsage> QueryElementUsages(ElementDefinition elementDefinition)
        {
            return this.iteration.Element
                .SelectMany(
                    ed => ed.ContainedElement
                        .Where(eu => eu.ElementDefinition == elementDefinition));
        }

        /// <summary>
        /// Gets the currently mapped parameter value <see cref="DataRow"/> based on the parameter mapping <see cref="DataRow"/>
        /// </summary>
        /// <param name="mappingRow">The mapping <see cref="DataRow"/></param>
        /// <returns>The mapped parameter value <see cref="DataRow"/></returns>
        public DataRow GetValueRowByMappingRow(DataRow mappingRow)
        {
            var mappingRowParentId = mappingRow[ParentIdColumnName];

            var childRow =
                mappingRow
                    .Table
                    .Select($"{ParentIdColumnName} = {mappingRowParentId} and {TypeColumnName} = '{ParameterValueType}'").SingleOrDefault();

            return childRow;
        }

        /// <summary>
        /// Gets the <see cref="DataRow"/> that is the (toplevel / <see cref="ElementDefinition"/> related) row of the hierarchical tree
        /// where <paramref name="dataRow"/> is a part of.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/></param>
        /// <returns>
        /// The (toplevel / <see cref="ElementDefinition"/> related) row of the hierarchical tree where <paramref name="dataRow"/> is a part of.
        /// Returns null if the row was not found.
        /// </returns>
        public DataRow GetElementDefinitionRow(DataRow dataRow)
        {
            var currentDataRow = dataRow;

            while (currentDataRow != null)
            {
                if (currentDataRow[TypeColumnName].ToString() == ElementDefinitionType)
                {
                    return currentDataRow;
                }

                if (currentDataRow[ParentIdColumnName] == DBNull.Value)
                {
                    break;
                }

                currentDataRow = currentDataRow.Table.Select($"{IdColumnName} = {currentDataRow[ParentIdColumnName]}").SingleOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Finds the <see cref="Thing"/> that is related to a <see cref="DataRow"/>
        /// </summary>
        /// <typeparam name="T">The expected <see cref="Type"/> of the to be returned <see cref="Thing"/></typeparam>
        /// <param name="dataRow">The <see cref="DataRow"/></param>
        /// <returns>
        /// A <see cref="Thing"/> that needs to be of type <typeparamref name="T"/>.
        /// Returns null if not found, or if the found thing is not of the expected type <typeparamref name="T"/>.
        /// </returns>
        public T GetThingByDataRow<T>(DataRow dataRow) where T : Thing
        {
            if (dataRow == null)
            {
                return null;
            }

            if (dataRow[IidColumnName] == DBNull.Value)
            {
                return null;
            }

            var foundThing = this.Things.SingleOrDefault(x => x.Iid == (Guid)dataRow[IidColumnName]);

            return foundThing as T;
        }

        /// <summary>
        /// Retrieves <see cref="ParameterType"/>s for a <see cref="DataRow"/>.
        /// This row could be any row in the hierarchy of the grid control.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/></param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of type <see cref="ParameterType"/> that contains all parameters from an <see cref="ElementDefinition"/>
        /// that can be found in the list of selected source <see cref="ParameterType"/>s (<see cref="DataSourceManager.SourceParameterTypes"/>).
        /// </returns>
        public IEnumerable<ParameterType> GetSourceParameterTypesForDataRow(DataRow dataRow)
        {
            var elementDefinitionRow = this.GetElementDefinitionRow(dataRow);

            if (elementDefinitionRow == null)
            {
                return new List<ParameterType>();
            }

            var elementDefinition = this.GetThingByDataRow<ElementDefinition>(elementDefinitionRow);

            if (elementDefinition == null)
            {
                return new List<ParameterType>();
            }

            return
                this.SourceParameterTypes
                    .Where(
                        x =>
                            elementDefinition.Parameter
                                .Select(y => y.ParameterType).Contains(x));
        }

        /// <summary>
        /// Gets the computed value of a specific Parameter of the <see cref="ElementDefinition"/> that belongs to a <see cref="DataRow"/>
        /// or belongs to its root/<see cref="ElementDefinition"/> <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">
        /// The <see cref="DataRow"/>
        /// </param>
        /// <param name="parameterTypeIid">
        /// The <see cref="ParameterType.Iid"/> of the <see cref="ParameterType"/> we want to to get its computed value from.
        /// </param>
        /// <returns>
        /// The computed value as a <see cref="string"/>
        /// </returns>
        public string GetElementDefinitionParameterValueForDataRow(DataRow dataRow, Guid parameterTypeIid)
        {
            var elementDefinitionRow = this.GetElementDefinitionRow(dataRow);
            var elementDefinition = this.GetThingByDataRow<ElementDefinition>(elementDefinitionRow);

            var elementDefinitionParameter =
                elementDefinition?
                    .Parameter
                    .SingleOrDefault(x => x.ParameterType.Iid == parameterTypeIid);

            if (elementDefinitionParameter == null)
            {
                return string.Empty;
            }

            if (elementDefinitionParameter.ParameterType is CompoundParameterType)
            {
                return elementDefinitionParameter
                    .ValueSets
                    .FirstOrDefault()?
                    .ActualValue[1];
            }

            return elementDefinitionParameter?
                .ValueSets
                .FirstOrDefault()?
                .ActualValue[0];
        }

        /// <summary>
        /// Gets the display Name of a specific Parameter of the <see cref="ElementDefinition"/> that belongs to a <see cref="DataRow"/>
        /// or belongs to its root/<see cref="ElementDefinition"/> <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">
        /// The <see cref="DataRow"/>
        /// </param>
        /// <param name="parameterTypeIid">
        /// The <see cref="ParameterType.Iid"/> of the <see cref="ParameterType"/> we want to to get its computed value from.
        /// </param>
        /// <returns>
        /// The computed value as a <see cref="string"/>
        /// </returns>
        public string GetElementDefinitionParameterDisplayNameForDataRow(DataRow dataRow, Guid parameterTypeIid)
        {
            var elementDefinitionRow = this.GetElementDefinitionRow(dataRow);
            var elementDefinition = this.GetThingByDataRow<ElementDefinition>(elementDefinitionRow);

            var elementDefinitionParameter =
                elementDefinition?
                    .Parameter
                    .SingleOrDefault(x => x.ParameterType.Iid == parameterTypeIid);

            if (elementDefinitionParameter?.ParameterType is CompoundParameterType)
            {
                return elementDefinitionParameter
                    .ValueSets
                    .FirstOrDefault()?
                    .ActualValue[0];
            }

            return elementDefinitionParameter?
                .ParameterType.Name;
        }

        /// <summary>
        /// Gets the ShortName of a specific Parameter of the <see cref="ElementDefinition"/> that belongs to a <see cref="DataRow"/>
        /// or belongs to its root/<see cref="ElementDefinition"/> <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">
        /// The <see cref="DataRow"/>
        /// </param>
        /// <param name="parameterTypeIid">
        /// The <see cref="ParameterType.Iid"/> of the <see cref="ParameterType"/> we want to to get its computed value from.
        /// </param>
        /// <returns>
        /// The computed value as a <see cref="string"/>
        /// </returns>
        public string GetElementDefinitionParameterCustomShortNameForDataRow(DataRow dataRow, Guid parameterTypeIid)
        {
            var elementDefinitionRow = this.GetElementDefinitionRow(dataRow);
            var elementDefinition = this.GetThingByDataRow<ElementDefinition>(elementDefinitionRow);

            var elementDefinitionParameter =
                elementDefinition?
                    .Parameter
                    .SingleOrDefault(x => x.ParameterType.Iid == parameterTypeIid);

            if (elementDefinitionParameter?.ParameterType is CompoundParameterType)
            {
                return elementDefinitionParameter
                    .ValueSets
                    .FirstOrDefault()?
                    .ActualValue[0];
            }

            return elementDefinitionParameter?
                .ParameterType.ShortName;
        }

        /// <summary>
        /// Sets the value of a <see cref="DataColumn"/> for a specific <see cref="DataRow"/> in the <see cref="DataTable"/>.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="dataRow">The <see cref="DataRow"/></param>
        /// <param name="value">The value to be set </param>
        public void SetValue(string columnName, DataRow dataRow, string value)
        {
            dataRow[columnName] = value;
        }

        /// <summary>
        /// Copies the values in the <see cref="ActualFiniteState"/> columns to columns that contain non-editted values.
        /// These columns values are used to check for changes in the <see cref="DataTable"/> on saving.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/></param>
        private void CopyCurrentValuesToOrgValueColumns(DataRow dataRow)
        {
            foreach (var actualState in this.GetMandatoryFiniteStates())
            {
                dataRow[this.GetOrgValueColumnName(actualState.ShortName)] = dataRow[actualState.ShortName];
                dataRow[this.GetOrgValueColumnName(this.GetShortNameColumnName(actualState.ShortName))] = dataRow[this.GetShortNameColumnName(actualState.ShortName)];
            }
        }

        /// <summary>
        /// Saves changed values to the data store.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> used to write data to the data store.
        /// </param>
        /// <returns>
        /// An awaitable <see cref="Task{T}"/> of type <see cref="SaveMappingResult"/>/>
        /// </returns>
        public async Task<SaveMappingResult> TrySaveValues(ISession session)
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext);

            var hasValueChanges = this.AddValueParameterValueSetsToTransaction(transaction);
            var hasMappingChanges = this.AddMappingParameterValueSetsToTransaction(transaction);

            if (!hasValueChanges && !hasMappingChanges)
            {
                return SaveMappingResult.CreateUnChangedResult();
            }

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
                return SaveMappingResult.CreateSuccesResult();
            }
            catch (Exception ex)
            {
                var errorText = $"The update operation failed: {ex.Message}";
                logger.Error(errorText);

                return SaveMappingResult.CreateErrorResult(errorText);
            }
        }

        /// <summary>
        /// Checkes if a <see cref="DataRow"/> was changed.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/></param>
        /// <returns>true is the <see cref="DataRow"/> was changed, otherwise false.</returns>
        private bool RowHasChanges(DataRow dataRow)
        {
            foreach (var actualState in this.GetMandatoryFiniteStates())
            {
                var columnName = actualState.ShortName;

                if (dataRow[columnName].ToString() != dataRow[this.GetOrgValueColumnName(columnName)].ToString())
                {
                    return true;
                }

                if (dataRow[this.GetShortNameColumnName(columnName)].ToString() != dataRow[this.GetOrgValueColumnName(this.GetShortNameColumnName(columnName))].ToString())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all changed values in the <see cref="DataTable"/> and adds the the Value <see cref="ParameterOverride"/>'s
        /// <see cref="ParameterOverrideValueSet"/> to a <see cref="ThingTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="ThingTransaction"/></param>
        /// <returns>true if <see cref="ParameterOverrideValueSet"/>s were added to the transaction, otherwise false </returns>
        private bool AddValueParameterValueSetsToTransaction(ThingTransaction transaction)
        {
            var result = false;

            foreach (DataRow dataRow in this.DataTable.Rows)
            {
                if (dataRow[TypeColumnName].ToString() != ParameterValueType)
                {
                    continue;
                }

                foreach (var actualState in this.GetMandatoryFiniteStates())
                {
                    if (dataRow[actualState.ShortName].ToString() == dataRow[this.GetOrgValueColumnName(actualState.ShortName)].ToString())
                    {
                        continue;
                    }

                    var thing = this.GetThingByDataRow<ParameterOrOverrideBase>(dataRow);

                    if (!(thing.ValueSets.FirstOrDefault(x => x.ActualState == actualState) is ParameterOverrideValueSet valueSet))
                    {
                        continue;
                    }

                    var clonedValueSet = valueSet.Clone(false);

                    clonedValueSet.Computed[0] = dataRow[actualState.ShortName].ToString();
                    transaction.CreateOrUpdate(clonedValueSet);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all changed mappings in the <see cref="DataTable"/> and adds the Mapping <see cref="ParameterOverride"/>'s
        /// <see cref="ParameterOverrideValueSet"/> to a <see cref="ThingTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="ThingTransaction"/></param>
        /// <returns>true if <see cref="ParameterOverrideValueSet"/>s were added to the transaction, otherwise false </returns>
        private bool AddMappingParameterValueSetsToTransaction(ThingTransaction transaction)
        {
            var result = false;

            foreach (DataRow mappingRow in this.DataTable.Rows)
            {
                if (mappingRow[TypeColumnName].ToString() != ParameterMappingType)
                {
                    continue;
                }

                var valueRow = this.GetValueRowByMappingRow(mappingRow);
                var parameterToMappingList = new List<ParameterToStateMapping>();
                var mappingThing = this.GetThingByDataRow<ParameterOrOverrideBase>(mappingRow);
                var hasChanges = false;

                if (this.RowHasChanges(mappingRow) || this.RowHasChanges(valueRow))
                {
                    hasChanges = true;

                    foreach (var actualState in this.GetMandatoryFiniteStates())
                    {
                        if (mappingRow[actualState.ShortName] == DBNull.Value || string.IsNullOrEmpty(mappingRow[actualState.ShortName].ToString()))
                        {
                            continue;
                        }

                        var mappedParameterType = this.SourceParameterTypes.SingleOrDefault(x => x.Iid.ToString() == mappingRow[actualState.ShortName].ToString());

                        if (mappedParameterType == null)
                        {
                            continue;
                        }

                        var shortName = valueRow[this.GetShortNameColumnName(actualState.ShortName)].ToString();
                        var mappingParameterValue = new ParameterToStateMapping(valueRow[actualState.ShortName].ToString(), mappedParameterType, actualState, shortName);
                        parameterToMappingList.Add(mappingParameterValue);
                    }
                }

                if (!hasChanges)
                {
                    continue;
                }

                if (!(mappingThing.ValueSets.FirstOrDefault() is ParameterOverrideValueSet valueSet))
                {
                    continue;
                }

                var clonedValueSet = valueSet.Clone(false);

                if (parameterToMappingList.Any())
                {
                    clonedValueSet.Computed[0] = JsonConvert.SerializeObject(parameterToMappingList);
                }
                else
                {
                    clonedValueSet.Computed[0] = string.Empty;
                }

                transaction.CreateOrUpdate(clonedValueSet);
                result = true;
            }

            return result;
        }
    }
}
