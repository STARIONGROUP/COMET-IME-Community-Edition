// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICustomXmlRead.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    /// <summary>
    /// Definition of the interface that allows to read custom XML parts from a workbook
    /// </summary>
    /// <typeparam name="T">
    /// The type of data that is to be read
    /// </typeparam>
    public interface ICustomXmlRead<out T> where T : CustomOfficeData
    {
        /// <summary>
        /// Reads the custom XML part from a workbook
        /// </summary>
        /// <returns>
        /// The data that is being read
        /// </returns>
        T Read();
    }

    /// <summary>
    /// Definition of the interface that allows to write custom XML parts to a workbook
    /// </summary>
    /// <typeparam name="T">
    /// The type of data that is to be written
    /// </typeparam>
    public interface ICustomXmlWrite<in T> where T : CustomOfficeData
    {
        /// <summary>
        /// Writes the custom XML part to a workbook
        /// </summary>
        /// <param name="t">
        /// The data that is being written
        /// </param>
        void Write(T t);
    }
}
