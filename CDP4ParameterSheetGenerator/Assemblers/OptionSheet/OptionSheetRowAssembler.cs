// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionSheetRowAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="OptionSheetRowAssembler"/> is to assemble a list of <see cref="IExcelRow{Thing}"/>s
    /// that are to be written to a workbook. The <see cref="IExcelRow{Thing}"/>s represent the <see cref="ElementDefinition"/>s,  <see cref="ElementUsage"/>s
    /// <see cref="Parameter"/>s, <see cref="ParameterOverride"/>s,<see cref="ParameterSubscription"/>s and <see cref="ParameterValueSet"/>s that
    /// are contained in the selected <see cref="Iteration"/>.
    /// </summary>
    public class OptionSheetRowAssembler
    {
        /// <summary>
        /// Backing field for the <see cref="IExcelRow{T}"/> property
        /// </summary>
        private readonly List<IExcelRow<Thing>> excelRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// The <see cref="Iteration"/> that is processed by the current <see cref="OptionSheetRowAssembler"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="Option"/> that is processed by the current <see cref="OptionSheetRowAssembler"/>
        /// </summary>
        private readonly Option option;

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> for which the owned <see cref="Parameter"/> and <see cref="ParameterSubscription"/> are assembled.
        /// </summary>
        private readonly DomainOfExpertise owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionSheetRowAssembler"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> form which the <see cref="ElementDefinition"/>, <see cref="ElementUsage"/> and <see cref="Parameter"/>s
        /// will be processed and assembled into a <see cref="List{ParameterSheetRow}"/>
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the rows are assembled
        /// </param>
        /// <param name="owner">
        /// The owning <see cref="DomainOfExpertise"/>. Only the owned <see cref="Parameter"/>s and <see cref="ParameterSubscription"/>s are taken into account.
        /// </param>
        public OptionSheetRowAssembler(Iteration iteration, Option option, DomainOfExpertise owner)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), "The iteration may not be null");
            }

            if (option == null)
            {
                throw new ArgumentNullException(nameof(option), "The option may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owning DomainOfExpertise may not be null");
            }

            this.iteration = iteration;
            this.option = option;
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
        public void Assemble()
        {
            var nestedElementExcelRows = new List<NestedElementExcelRow>();

            IEnumerable<NestedElement> nestedElements;
            if (this.iteration.TopElement == null)
            {
                nestedElements = Enumerable.Empty<NestedElement>();
            }
            else
            {
                var nestedElementTreeGenerator = new NestedElementTreeGenerator();
                nestedElements = nestedElementTreeGenerator.Generate(this.option, this.owner, false);
            }

            // Order the nestedElements by shortname. The Shortname of an NestedElement 
            // is related to the path of a NestedElement
            nestedElements = nestedElements.OrderBy(ne => ne.ShortName);

            foreach (var nestedElement in nestedElements)
            {
                var nestedElementExcelRow = new NestedElementExcelRow(nestedElement, this.owner);
                nestedElementExcelRows.Add(nestedElementExcelRow);
            }

            foreach (var nestedElementExcelRow in nestedElementExcelRows)
            {
                var deeprows = nestedElementExcelRow.GetContainedRowsDeep();

                foreach (var itm in deeprows)
                {
                    var splitlevel = itm.ModelCode.Split('\\')[0].Split('.').Length;

                    if (itm.Categories == null)
                    {
                        splitlevel += 1;
                    }

                    var spaces = new string(' ', 3 * Math.Abs(splitlevel - 1));

                    switch (itm)
                    {
                        case NestedElementExcelRow row:
                            row.Name = $"{spaces}{itm.Name}";
                            break;
                        case NestedParameterExcelRow row:
                            row.Name = $"{spaces}{itm.Name}";
                            break;
                    }
                }

                this.excelRows.AddRange(deeprows);
            }
        }
    }
}