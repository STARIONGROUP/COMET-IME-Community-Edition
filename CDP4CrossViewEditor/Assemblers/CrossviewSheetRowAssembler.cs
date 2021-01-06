namespace CDP4CrossViewEditor.Assemblers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.ViewModels;

    using CDP4CrossViewEditor.RowModels;
    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetRowAssembler"/> is to assemble a list of <see cref="IExcelRow{T}"/>
    /// that will be written to a workbook.
    /// </summary>
    public class CrossviewSheetRowAssembler
    {
        /// <summary>
        /// Backing field for the <see cref="IExcelRow{T}"/> property
        /// </summary>
        private readonly List<IExcelRow<Thing>> excelRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// The <see cref="Iteration"/> that is processed by the current <see cref="CrossviewSheetRowAssembler"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> for which the owned <see cref="Parameter"/> and <see cref="ParameterSubscription"/> are assembled.
        /// </summary>
        private readonly DomainOfExpertise owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewSheetRowAssembler"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> from which the element definition, element usages and parameters will be processed
        /// </param>
        /// <param name="owner"> The owning <see cref="DomainOfExpertise"/> </param>
        public CrossviewSheetRowAssembler(Iteration iteration, DomainOfExpertise owner)
        {
            this.iteration = iteration ?? throw new ArgumentNullException(nameof(iteration), "The iteration may not be null");
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner), "The owning DomainOfExpertise may not be null");
        }

        /// <summary>
        /// Gets the <see cref="IExcelRow{Thing}"/>s that have been assembled
        /// </summary>
        public IEnumerable<IExcelRow<Thing>> ExcelRows => this.excelRows;

        /// <summary>
        /// Assemble the <see cref="IExcelRow{Thing}"/>s
        /// </summary>
        /// <param name="processedValueSets">Values need to be restored to what the user provided</param>
        public void Assemble(IEnumerable<ElementDefinition> elementDefinitions)
        {
            //var sortedElementDefinitions = this.iteration.Element.OrderBy(x => x.Name);

            var definitionRows = elementDefinitions.Select(elementDefinition => new ElementDefinitionExcelRow(elementDefinition, this.owner, null)).ToList();

            foreach (var elementDefinitionExcelRow in definitionRows)
            {
                var deepRows = elementDefinitionExcelRow.GetContainedRowsDeep().ToList();
                this.excelRows.AddRange(deepRows);
            }
        }
    }
}
