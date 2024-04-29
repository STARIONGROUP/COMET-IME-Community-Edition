// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExcelRow.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.RowModels
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The interface that is used to populate the ParameterSheet
    /// </summary>
    /// <typeparam name="T">
    /// The type parameter
    /// </typeparam>
    /// <remarks>
    /// A Covariant interface that allows the <see cref="Thing"/> to be of a more generic type
    /// </remarks>
    public interface IExcelRow<out T> where T : Thing
    {
        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the current <see cref="IExcelRow{T}"/>
        /// </summary>
        T Thing { get; }

        /// <summary>
        /// Gets the type of <see cref="Thing"/> that this row represents
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the level that this row is located at.
        /// </summary>
        /// <remarks>
        /// The Level property is used to apply grouping in Excel
        /// </remarks>
        int Level { get; }

        /// <summary>
        /// Gets the human readable Name of the <see cref="Parameter"/>, <see cref="ParameterOverride"/> or <see cref="ParameterSubscription"/> that is represented by the Row
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the human readable short name
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets the model code of the <see cref="Thing"/> that is represented by the current row.
        /// </summary>
        string ModelCode { get;  }

        /// <summary>
        /// Gets the computed value
        /// </summary>
        object ComputedValue { get; }

        /// <summary>
        /// Gets the manual value
        /// </summary>
        object ManualValue { get; }

        /// <summary>
        /// Gets the reference value
        /// </summary>
        object ReferenceValue { get; }

        /// <summary>
        /// Gets the switch
        /// </summary>
        string Switch { get; }

        /// <summary>
        /// Gets the actual value
        /// </summary>
        object ActualValue { get; }

        /// <summary>
        /// Gets the formula
        /// </summary>
        string Formula { get; }

        /// <summary>
        /// Gets short-name of the <see cref="ParameterTypeShortName"/> and the shot-name of the <see cref="MeasurementScale"/> if it is applicable.
        /// </summary>
        string ParameterTypeShortName { get; }

        /// <summary>
        /// Gets the <see cref="ParameterType"/> that the <see cref="Parameter"/> or <see cref="ParameterOverride"/> references.
        /// </summary>
        ParameterType ParameterType { get; }

        /// <summary>
        /// Gets the excel format depending on the parameter type that is represented
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Gets the short-name of the owning <see cref="DomainOfExpertise"/>
        /// </summary>
        string Owner { get; }

        /// <summary>
        /// Gets the short-name of the <see cref="Category"/>s that the <see cref="ICategorizableThing"/> the current
        /// row represents is a member of.
        /// </summary>
        string Categories { get; }

        /// <summary>
        /// Gets the unique id if the <see cref="Thing"/> that is represented by the row
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets the <see cref="Container"/> property.
        /// </summary>
        IExcelRow<Thing> Container { get; set; }

        /// <summary>
        /// Queries the current row for the contained rows
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row
        /// </returns>
        IEnumerable<IExcelRow<Thing>> GetContainedRows();

        /// <summary>
        /// Queries the current row for all the rows in the subtree
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row and its subtree
        /// </returns>
        IEnumerable<IExcelRow<Thing>> GetContainedRowsDeep();
    }
}
