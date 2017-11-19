// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionExcelRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ElementDefinitionExcelRow"/> is to represent an <see cref="ElementDefinition"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ElementDefinitionExcelRow : ExcelRowBase<ElementDefinition>
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        /// <remarks>
        /// <see cref="ElementUsage"/>s are always at level 1.
        /// </remarks>
        private const int LevelOffset = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionExcelRow"/> class.
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> that is represented by the current row
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ElementDefinitionExcelRow(ElementDefinition elementDefinition, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(elementDefinition)
        {
            this.UpdateProperties();

            var sortedParameters = elementDefinition.Parameter.OrderBy(parameter => parameter.ParameterType.Name);
            this.ProcessParameters(sortedParameters, owner, processedValueSets);

            var sortedParameterGroups = elementDefinition.ParameterGroup.Where(pg => pg.ContainingGroup == null);
            this.ProcessParameterGroups(sortedParameterGroups, owner, processedValueSets);

            var sortedElementUSages = elementDefinition.ContainedElement.OrderBy(elementUsage => elementUsage.Name);
            this.ProcessElementUsages(sortedElementUSages, owner, processedValueSets);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRowBase{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.Type = ParameterSheetConstants.ED;
            this.Owner = this.Thing.Owner.ShortName;            
            this.Level = LevelOffset;
            this.ModelCode = this.Thing.ModelCode();
            this.Categories = this.Thing.GetAllCategoryShortNames();
        }

        /// <summary>
        /// Process the <see cref="Parameter"/>s that are contained by the <see cref="ElementDefinition"/> that is represented by the current excel row
        /// and that are NOT "contained" by a <see cref="ParameterGroup"/>.
        /// </summary>
        /// <param name="parameters">
        /// The <see cref="Parameter"/>s contained by the <see cref="ElementDefinition"/>.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameters(IEnumerable<Parameter> parameters, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Owner == owner && parameter.Group == null)
                {
                    var parameterExcelRow = new ParameterExcelRow(parameter, processedValueSets);

                    if (!parameter.IsOptionDependent && parameter.StateDependence == null)
                    {
                        this.ContainedRows.Add(parameterExcelRow);
                        parameterExcelRow.Container = this;                        
                    }
                    else
                    { 
                        var containedItems = parameterExcelRow.GetContainedRows();
                        foreach (var containedItem in containedItems)
                        {
                            this.ContainedRows.Add(containedItem);
                            containedItem.Container = this;
                        }
                    }
                }

                if (parameter.Owner != owner && parameter.Group == null)
                {
                    var parameterSubscription = parameter.ParameterSubscription.SingleOrDefault(x => x.Owner == owner);
                    if (parameterSubscription != null)
                    {
                        var parameterSubscriptionExcelRow = new ParameterSubscriptionExcelRow(parameterSubscription, processedValueSets);

                        if (!parameterSubscription.IsOptionDependent && parameterSubscription.StateDependence == null)
                        {
                            this.ContainedRows.Add(parameterSubscriptionExcelRow);
                            parameterSubscriptionExcelRow.Container = this;                            
                        }
                        else                        
                        {
                            var containedItems = parameterSubscriptionExcelRow.GetContainedRows();
                            foreach (var containedItem in containedItems)
                            {
                                this.ContainedRows.Add(containedItem);
                                containedItem.Container = this;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process the <see cref="ParameterGroup"/> that are contained by the <see cref="ElementDefinition"/> that is represented by the current excel row
        /// and that are NOT "contained" by a <see cref="ParameterGroup"/>.
        /// </summary>
        /// <param name="parameterGroups">The <see cref="ParameterGroup"/>s contained by the <see cref="ElementDefinition"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameterGroups(IEnumerable<ParameterGroup> parameterGroups, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameterGroup in parameterGroups)
            {
                var row = new ParameterGroupExcelRow(parameterGroup, owner, processedValueSets);
                this.ContainedRows.Add(row);
                row.Container = this;
            }
        }

        /// <summary>
        /// Process the <see cref="ElementUsage"/>s that are contained by the <see cref="ElementDefinition"/> that is represented by the current excel row
        /// </summary>
        /// <param name="elementUsages">The <see cref="ElementUsage"/>s contained by the <see cref="ElementDefinition"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessElementUsages(IEnumerable<ElementUsage> elementUsages, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var elementUsage in elementUsages)
            {
                var row = new ElementUsageExcelRow(elementUsage, owner, processedValueSets);
                this.ContainedRows.Add(row);
                row.Container = this;
            }
        }
    }
}
