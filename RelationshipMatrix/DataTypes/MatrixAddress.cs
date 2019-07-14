// -------------------------------------------------------------------------------------------------
// <copyright file="MatrixAddress.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
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