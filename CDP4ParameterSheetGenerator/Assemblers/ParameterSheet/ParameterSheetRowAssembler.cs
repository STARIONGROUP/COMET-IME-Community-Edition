// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetRowAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// The purpose of the <see cref="ParameterSheetRowAssembler"/> is to assemble a list of <see cref="IExcelRow{Thing}"/>s
    /// that are to be written to a workbook. The <see cref="IExcelRow{Thing}"/>s represent the <see cref="ElementDefinition"/>s,  <see cref="ElementUsage"/>s
    /// <see cref="Parameter"/>s, <see cref="ParameterOverride"/>s,<see cref="ParameterSubscription"/>s and <see cref="ParameterValueSet"/>s that
    /// are contained in the selected <see cref="Iteration"/>.
    /// </summary>
    public class ParameterSheetRowAssembler
    {
        /// <summary>
        /// Backing field for the <see cref="IExcelRow{T}"/> property
        /// </summary>
        private readonly List<IExcelRow<Thing>> excelRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// The <see cref="Iteration"/> that is processed by the current <see cref="ParameterSheetRowAssembler"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> for which the owned <see cref="Parameter"/> and <see cref="ParameterSubscription"/> are assembled.
        /// </summary>
        private readonly DomainOfExpertise owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSheetRowAssembler"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> form which the <see cref="ElementDefinition"/>, <see cref="ElementUsage"/> and <see cref="Parameter"/>s
        /// will be processed and assembled into a <see cref="List{ParameterSheetRow}"/>
        /// </param>
        /// <param name="owner">
        /// The owning <see cref="DomainOfExpertise"/>. Only the owned <see cref="Parameter"/>s and <see cref="ParameterSubscription"/>s are taken into account.
        /// </param>
        public ParameterSheetRowAssembler(Iteration iteration, DomainOfExpertise owner)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), "The iteration may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owning DomainOfExpertise may not be null");
            }

            this.iteration = iteration;
            this.owner = owner;
        }

        /// <summary>
        /// Gets the <see cref="IExcelRow{Thing}"/>s that have been assembled
        /// </summary>
        public IEnumerable<IExcelRow<Thing>> ExcelRows
        {
            get
            {
                return this.excelRows;
            }
        }

        /// <summary>
        /// Assemble the <see cref="IExcelRow{Thing}"/>s
        /// </summary>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public void Assemble(IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            var definitionRows = new List<ElementDefinitionExcelRow>();

            var sortedElementDefinitions = this.iteration.Element.OrderBy(x => x.Name);

            foreach (var elementDefinition in sortedElementDefinitions)
            {
                var elementDefinitionExcelRow = new ElementDefinitionExcelRow(elementDefinition, this.owner, processedValueSets);
                definitionRows.Add(elementDefinitionExcelRow);
            }

            foreach (var elementDefinitionExcelRow in definitionRows)
            {
                var deeprows = elementDefinitionExcelRow.GetContainedRowsDeep().ToList();
                this.excelRows.AddRange(deeprows);
            }
        }
    }
}