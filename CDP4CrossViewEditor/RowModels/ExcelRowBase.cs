// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelRowBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.RowModels
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    /// <summary>
    /// An abstract super class from which the Excel rows derive
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="Thing"/> that is represented by the row
    /// </typeparam>
    public abstract class ExcelRowBase<T> : IExcelRow<T> where T : Thing
    {
        /// <summary>
        /// Backing field for the <see cref="ContainedRows"/> property.
        /// </summary>
        protected readonly List<IExcelRow<Thing>> ContainedRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelRowBase{T}"/> class.
        /// </summary>
        /// <param name="thing">
        /// The <see cref="thing"/> that is represented by the current row.
        /// </param>
        protected ExcelRowBase(T thing)
        {
            this.Thing = thing;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by row.
        /// </summary>
        public T Thing { get; private set; }

        /// <summary>
        /// Gets or sets the human readable name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the human readable short name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the model code of the <see cref="Thing"/> that is represented by the current row.
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// Gets or sets the level that this row is located at.
        /// </summary>
        /// <remarks>
        /// The Level property is used to apply grouping in Excel
        /// </remarks>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the type of <see cref="Thing"/> that this row represents
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Container"/> property.
        /// </summary>
        public IExcelRow<Thing> Container { get; set; }

        /// <summary>
        /// Queries the current row for the contained rows
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row
        /// </returns>
        public IEnumerable<IExcelRow<Thing>> GetContainedRows()
        {
            return this.ContainedRows;
        }

        /// <summary>
        /// Queries the current row for all the rows in the subtree
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row and its subtree
        /// </returns>
        public IEnumerable<IExcelRow<Thing>> GetContainedRowsDeep()
        {
            var resultList = new List<IExcelRow<Thing>> { this };
            var elementUsageRows = this.ContainedRows.OfType<ElementUsageExcelRow>().ToList();

            foreach (var containedRow in elementUsageRows.OrderBy(x => x.Name))
            {
                resultList.AddRange(containedRow.GetContainedRowsDeep());
            }

            var leftOverRows = this.ContainedRows.Except(elementUsageRows);

            foreach (var containedRow in leftOverRows)
            {
                resultList.AddRange(containedRow.GetContainedRowsDeep());
            }

            return resultList;
        }

        /// <summary>
        /// Gets or sets the computed value
        /// </summary>
        public object ComputedValue { get; set; }

        /// <summary>
        /// Gets or sets the manual value
        /// </summary>
        public object ManualValue { get; set; }

        /// <summary>
        /// Gets or sets the reference value
        /// </summary>
        public object ReferenceValue { get; set; }

        /// <summary>
        /// Gets or sets the switch
        /// </summary>
        public string Switch { get; set; }

        /// <summary>
        /// Gets or sets the actual value
        /// </summary>
        public object ActualValue { get; set; }

        /// <summary>
        /// Gets or sets the formula
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Gets or sets short-name of the <see cref="ParameterTypeShortName"/> and possible the <see cref="MeasurementScale"/>
        /// </summary>
        public string ParameterTypeShortName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType"/> that the <see cref="Parameter"/> or <see cref="ParameterOverride"/> references.
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the excel format depending on the parameter type that is represented
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the short-name of the owning <see cref="DomainOfExpertise"/>
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the unique id if the <see cref="Thing"/> that is represented by the row
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Gets or sets the short-name of the <see cref="Category"/>s that the <see cref="ICategorizableThing"/> the current
        /// row represents is a member of.
        /// </summary>
        public string Categories { get; set; }
    }
}
