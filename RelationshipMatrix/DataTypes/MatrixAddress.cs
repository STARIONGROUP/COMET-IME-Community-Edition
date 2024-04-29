// -------------------------------------------------------------------------------------------------
// <copyright file="MatrixAddress.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.DataTypes
{
    /// <summary>
    /// Represents a cell address of the matrix
    /// </summary>
    public class MatrixAddress
    {
        /// <summary>
        /// Represents the row identifier of an item in a matrix
        /// </summary>
        public int? Row;

        /// <summary>
        /// Represents the column identifier if an item in a matrix
        /// </summary>
        public string Column;
    }
}