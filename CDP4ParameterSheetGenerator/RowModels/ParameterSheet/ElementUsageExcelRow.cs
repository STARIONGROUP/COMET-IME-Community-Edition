// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageExcelRow.cs" company="RHEA System S.A.">
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

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ElementUsageExcelRow"/> is to represent an <see cref="ElementUsage"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ElementUsageExcelRow : ExcelRowBase<ElementUsage>
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        /// <remarks>
        /// <see cref="ElementUsage"/>s are always at level 1.
        /// </remarks>
        private const int LevelOffset = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageExcelRow"/> class.
        /// </summary>
        /// <param name="elementUsage">
        /// The <see cref="ElementUsage"/> that is represented by the current row
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementUsage"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ElementUsageExcelRow(ElementUsage elementUsage, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(elementUsage)
        {
            this.UpdateProperties();
            
            var sortedOverrides = elementUsage.ParameterOverride.Where(po => po.Owner == owner).OrderBy(po => po.ParameterType.Name);
            this.ProcessParameterOverrides(sortedOverrides, owner, processedValueSets);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRowBase{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = string.Format("{0}{1} : {2}", new string(' ', 3), this.Thing.ShortName, this.Thing.ElementDefinition.ShortName);            
            this.Type = ParameterSheetConstants.EU;
            this.Owner = this.Thing.Owner.ShortName;            
            this.Level = LevelOffset;
            this.ModelCode = this.Thing.ModelCode();

            var categories = this.Thing.ElementDefinition.GetAllCategories().ToList();
            categories.AddRange(this.Thing.GetAllCategories());
            this.Categories = categories.Distinct().Aggregate(string.Empty, (current, cat) => current + " " + cat.ShortName).Trim();
        }

        /// <summary>
        /// Process the <see cref="ParameterOverride"/>s that are contained by the <see cref="ElementUsage"/> that is represented by the current excel row
        /// </summary>
        /// <param name="parameterOverrides">The <see cref="ParameterOverride"/>s contained by the <see cref="ElementUsage"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any ParameterOverrides
        /// in the <see cref="ElementUsage"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameterOverrides(IEnumerable<ParameterOverride> parameterOverrides, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameterOverride in parameterOverrides)
            {
                if (parameterOverride.Owner == owner)
                {
                    var parameterOverrideExcelRow = new ParameterOverrideExcelRow(parameterOverride, owner, processedValueSets);

                    if (!parameterOverride.IsOptionDependent && parameterOverride.StateDependence == null)
                    {
                        this.ContainedRows.Add(parameterOverrideExcelRow);
                        parameterOverrideExcelRow.Container = this;                        
                    }
                    else
                    {
                        var containedItems = parameterOverrideExcelRow.GetContainedRows();
                        foreach (var containedItem in containedItems)
                        {
                            this.ContainedRows.Add(containedItem);
                            containedItem.Container = this;
                        }
                    }
                }

                if (parameterOverride.Owner != owner)
                {
                    var parameterSubscription = parameterOverride.ParameterSubscription.SingleOrDefault(x => x.Owner == owner);
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
                            }continue;
                        }
                    }
                }
            }
        }
    }
}
