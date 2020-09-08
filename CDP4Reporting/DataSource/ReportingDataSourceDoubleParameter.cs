namespace CDP4Reporting.DataSource
{
    using System.Globalization;

    /// <summary>
    /// Abstract base class from which all double parameter columns
    /// for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the associated <see cref="ReportingDataSourceRow"/>.
    /// </typeparam>
    internal class ReportingDataSourceDoubleParameter<T> : ReportingDataSourceParameter<T, double>
        where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// Parses a parameter value as double.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        internal override double Parse(string value)
        {
            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num);
            return num;
        }
    }
}
