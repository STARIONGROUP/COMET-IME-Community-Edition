// -------------------------------------------------------------------------------------------------
// <copyright file="StringRange.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.UserControls
{
    /// <summary>
    /// Provides a starting position of a string range and its length.
    /// </summary>
    public class StringRange
    {
        /// <summary>
        /// First character in range, zero based
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Number of characters in range
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringRange"/> class
        /// </summary>
        public StringRange(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="StringRange"/> class
        /// </summary>
        /// <param name="start">
        /// The start of the <see cref="StringRange"/>
        /// </param>
        /// <param name="length">
        /// the length of the <see cref="StringRange"/>
        /// </param>
        public StringRange(int start, int length)
        {
            this.Start = start;
            this.Length = length;
        }
    }
}
