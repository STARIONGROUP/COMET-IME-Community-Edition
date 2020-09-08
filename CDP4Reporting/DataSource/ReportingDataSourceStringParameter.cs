namespace CDP4Reporting.DataSource
{
    /// <summary>
    /// Abstract base class from which all string parameter columns
    /// for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the associated <see cref="ReportingDataSourceRow"/>.
    /// </typeparam>
    internal abstract class ReportingDataSourceStringParameter<T> : ReportingDataSourceParameter<T, string>
        where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// Parses a parameter value as string.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        internal override string Parse(string value)
        {
            return value;
        }
    }
}
