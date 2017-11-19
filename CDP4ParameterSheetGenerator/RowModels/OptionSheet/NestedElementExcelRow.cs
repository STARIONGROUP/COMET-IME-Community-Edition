// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedElementExcelRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="NestedElementExcelRow"/> is to represent <see cref="NestedElement"/>s in the 
    /// <see cref="Option"/> Sheet
    /// </summary>
    public class NestedElementExcelRow : ExcelRowBase<NestedElement>  
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NestedElementExcelRow"/> class.
        /// </summary>
        /// <param name="nestedElement">
        /// The <see cref="NestedElement"/> that is represented by the current row view-model
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any <see cref="NestedParameter"/>
        /// in the <see cref="NestedElement"/> that is represented by the current row.
        /// </param>
        public NestedElementExcelRow(NestedElement nestedElement, DomainOfExpertise owner)
            : base(nestedElement)
        {
            this.UpdateProperties();

            var nestedParameters = nestedElement.NestedParameter.ToList();
            this.ProcessNestedParameters(nestedParameters, owner);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRowBase{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.Type = OptionSheetConstants.NE;
            this.Owner = this.Thing.Owner.ShortName;
            this.ModelCode = this.Thing.ShortName;

            var elementUsage = this.Thing.ElementUsage.LastOrDefault();
            if (elementUsage != null)
            {
                this.Categories = elementUsage.GetAllCategoryShortNames();
            }
        }

        /// <summary>
        /// Process the <see cref="NestedParameter"/>s that are contained by the <see cref="NestedElement"/> that is represented by the current excel row        
        /// </summary>
        /// <param name="nestedParameters">The <see cref="NestedParameter"/>s contained by the <see cref="NestedElement"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any <see cref="NestedParameter"/> or <see cref="NestedElement"/>
        /// in the <see cref="NestedElement"/> that is represented by the current row.
        /// </param>        
        private void ProcessNestedParameters(IEnumerable<NestedParameter> nestedParameters, DomainOfExpertise owner)
        {
            var orderedParameters = nestedParameters.OrderBy(np => np.Path);

            foreach (var nestedParameter in orderedParameters)
            {
                if (nestedParameter.Owner == owner)
                {
                    if (nestedParameter.ActualState != null && nestedParameter.ActualState.Kind == ActualFiniteStateKind.FORBIDDEN)
                    {
                        continue;
                    }

                    var nestedParameterExcelRow = new NestedParameterExcelRow(nestedParameter);
                    this.ContainedRows.Add(nestedParameterExcelRow);                    
                }
            }
        }
    }
}
