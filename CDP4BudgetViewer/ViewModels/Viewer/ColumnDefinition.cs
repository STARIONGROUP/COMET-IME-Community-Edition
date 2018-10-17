// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnDefinition.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    /// <summary>
    /// A view-model for dynamic column definition
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinition"/> class
        /// </summary>
        /// <param name="header">The header</param>
        /// <param name="fieldname">The field-name</param>
        public ColumnDefinition(string header, string fieldname)
        {
            this.Header = header;
            this.FieldName = fieldname;
        }

        /// <summary>
        /// Gets the header to display
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Gets the name of the fieldname for this column
        /// </summary>
        public string FieldName { get; }
    }
}
