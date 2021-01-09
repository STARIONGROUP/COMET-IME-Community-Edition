// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceManager.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.StateToParameterTypeMapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm.Types;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="DataSourceManager"/> is manage the content a <see cref="DataTable"/> that is the data-source for a datagrid
    /// displaying the mapping table to the user as well as the dynamic columns of the data-grid
    /// </summary>
    public class DataSourceManager : IDisposable
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        private List<IDisposable> disposables;

        /// <summary>
        /// The <see cref="Iteration"/> that contains the data for which the mapper managing the data
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The <see cref="Category"/> that the <see cref="ElementDefinition"/>s need to be a member of
        /// </summary>
        private Category elementDefinitionCategory;

        /// <summary>
        /// The <see cref="ActualFiniteStateList"/> for which the mapping needs to be managed
        /// </summary>
        private ActualFiniteStateList actualFiniteStateList;

        /// <summary>
        /// The <see cref="Category"/> the <see cref="ParameterType"/> needs to be a member of to function as
        /// source of the mapping
        /// </summary>
        private Category sourceParameterTypeCategory;

        /// <summary>
        /// The <see cref="TextParameterType"/> where the mapping will be stored
        /// </summary>
        private TextParameterType targetMappingParameterType;

        /// <summary>
        /// The <see cref="ScalarParameterType"/> where the mapped value will be stored
        /// </summary>
        private ScalarParameterType targetValueParameterType;

        public DataSourceManager()
        {
            this.Columns = new DisposableReactiveList<DataGridColumn>();
            this.DataTable = new DataTable();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceManager"/> class.
        /// </summary>
        public DataSourceManager(Iteration iteration, Category elementDefinitionCategory, ActualFiniteStateList actualFiniteStateList, Category sourceParameterTypeCategory, TextParameterType targetMappingParameterType, ScalarParameterType targetValueParameterType)
        {
            this.iteration = iteration ?? throw new ArgumentNullException(nameof(iteration),$"The {nameof(iteration)} may not be null");
            this.elementDefinitionCategory = elementDefinitionCategory ?? throw new ArgumentNullException(nameof(elementDefinitionCategory), $"The {nameof(elementDefinitionCategory)} may not be null");
            this.actualFiniteStateList = actualFiniteStateList ?? throw new ArgumentNullException(nameof(actualFiniteStateList), $"The {nameof(actualFiniteStateList)} may not be null");
            this.sourceParameterTypeCategory = sourceParameterTypeCategory ?? throw new ArgumentNullException(nameof(sourceParameterTypeCategory), $"The {nameof(sourceParameterTypeCategory)} may not be null");
            this.targetMappingParameterType = targetMappingParameterType ?? throw new ArgumentNullException(nameof(targetMappingParameterType), $"The {nameof(targetMappingParameterType)} may not be null");
            this.targetValueParameterType = targetValueParameterType ?? throw new ArgumentNullException(nameof(targetValueParameterType), $"The {nameof(targetValueParameterType)} may not be null");

            this.disposables = new List<IDisposable>();

            this.Columns = new DisposableReactiveList<DataGridColumn>();
            this.DataTable = new DataTable();

            this.UpdateColumns();
            this.UpdateRows();

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="ActualStateDataGridColumn"/> for of the data source
        /// </summary>
        public DisposableReactiveList<DataGridColumn> Columns { get; set; }

        /// <summary>
        /// Gets the <see cref="DataTable"/> for of the data source
        /// </summary>
        public DataTable DataTable { get; set; }

        /// <summary>
        /// Adds and removes the <see cref="Columns"/> as well as the column definitions of the <see cref="DataTable"/>
        /// </summary>
        public void UpdateColumns()
        {
            if (!this.Columns.Any(x => x.FieldName == "Label"))
            {
                this.Columns.Add(new DataGridColumn { FieldName = "Label" });
                this.DataTable.Columns.Add("Label", typeof(string));
            }
        
            var newActualFiniteStates = this.actualFiniteStateList.ActualState.Except(this.Columns.OfType<ActualStateDataGridColumn>().Select(x => x.ActualFiniteState)).ToList();
            var oldActualFiniteStates = this.Columns.OfType<ActualStateDataGridColumn>().Select(x => x.ActualFiniteState).Except(this.actualFiniteStateList.ActualState).ToList();

            foreach (var actualFiniteState in newActualFiniteStates)
            {
                var actualStateDataGridColumn = new ActualStateDataGridColumn(actualFiniteState);
                this.Columns.Add(actualStateDataGridColumn);
                this.DataTable.Columns.Add(actualStateDataGridColumn.FieldName, typeof(string));
            }

            foreach (var actualFiniteState in oldActualFiniteStates)
            {
                var actualStateDataGridColumn = this.Columns.OfType<ActualStateDataGridColumn>().SingleOrDefault(x => x.ActualFiniteState == actualFiniteState);
                {
                    if (actualStateDataGridColumn != null)
                    {
                        this.Columns.RemoveAndDispose(actualStateDataGridColumn);
                        this.DataTable.Columns.Remove(actualStateDataGridColumn.FieldName);
                    }
                }
            }

            if (!this.Columns.Any(x => x.FieldName == "Type"))
            {
                this.Columns.Add(new DataGridColumn { FieldName = "Type" });
                this.DataTable.Columns.Add("Type", typeof(string));
            }
        }

        /// <summary>
        /// Update the rows of the <see cref="DataTable"/>
        /// </summary>
        public void UpdateRows()
        {
            foreach (var elementDefinition in this.iteration.Element)
            {
                if (elementDefinition.IsMemberOfCategory(this.elementDefinitionCategory)) ;
                {
                    var elementDefinitionRow = this.DataTable.NewRow();
                    elementDefinitionRow["Label"] = $"{elementDefinition.Name} - {elementDefinition.ShortName}";
                    elementDefinitionRow["Type"] = "ED";
                    this.DataTable.Rows.Add(elementDefinitionRow);

                    var elementUsages = this.QueryElementUsages(elementDefinition);

                    foreach (var elementUsage in elementUsages)
                    {
                        var elementUsageRow = this.DataTable.NewRow();
                        elementUsageRow["Label"] = $"  ▹ {elementUsage.ModelCode()}";
                        elementDefinitionRow["Type"] = "EU";
                        foreach (var actualState in this.actualFiniteStateList.ActualState)
                        {
                            elementUsageRow[actualState.ShortName] = $"{actualState.ShortName}";
                        }
                        this.DataTable.Rows.Add(elementUsageRow);

                        foreach (var parameterOverride in elementUsage.ParameterOverride)
                        {
                            if (parameterOverride.StateDependence == this.actualFiniteStateList)
                            {
                                var parameterOverrideMapRow = this.DataTable.NewRow();
                                parameterOverrideMapRow["Label"] = $"    ▹ MAP - {parameterOverride.ParameterType?.ShortName}";
                                parameterOverrideMapRow["Type"] = "PO-MAP";
                                foreach (var valueSet in parameterOverride.ValueSet)
                                {
                                    var columnName = valueSet.ActualState.ShortName;
                                    parameterOverrideMapRow[columnName] = valueSet.Manual;
                                }
                                this.DataTable.Rows.Add(parameterOverrideMapRow);

                                var parameterOverrideValueRow = this.DataTable.NewRow();
                                parameterOverrideValueRow["Label"] = $"    ▹ VAL - {parameterOverride.ParameterType?.ShortName}";
                                parameterOverrideValueRow["Type"] = "PO-VAL";
                                foreach (var valueSet in parameterOverride.ValueSet)
                                {
                                    var columnName = valueSet.ActualState.ShortName;
                                    parameterOverrideMapRow[columnName] = valueSet.Manual[0];
                                }
                                this.DataTable.Rows.Add(parameterOverrideValueRow);
                            }
                        }
                    }
                }
            }
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
            foreach (var ed in this.iteration.Element)
            {
                foreach (var elementUsage in ed.ContainedElement)
                {
                    if (elementUsage.ElementDefinition == elementDefinition)
                    {
                        yield return elementUsage;
                    }
                }
            }
        }

        /// <summary>
        /// Performs freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in this.disposables)
            {
                disposable.Dispose();
            }
        }

        private void AddSubscriptions()
        {
        }
    }
}
