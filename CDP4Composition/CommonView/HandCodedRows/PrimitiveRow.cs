// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System.ComponentModel;
    using CDP4Composition.Services;
    using ReactiveUI;

    /// <summary>
    /// The rows used for ordered primitive type property
    /// </summary>
    /// <typeparam name="T">The type of value</typeparam>
    public class PrimitiveRow<T> : ReactiveObject, IDataErrorInfo
    {
        /// <summary>
        /// backing field for <see cref="Index"/>
        /// </summary>
        private long index;

        /// <summary>
        /// backing field for <see cref="Value"/>
        /// </summary>
        private T value;

        /// <summary>
        /// Gets or sets the index of the value
        /// </summary>
        public long Index
        {
            get { return this.index; }
            set { this.RaiseAndSetIfChanged(ref this.index, value); }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public T Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field
        /// </remarks>
        public string this[string columnName]
        {
            get { return ValidationService.ValidateProperty(columnName, this); }
        }

        /// <summary>
        /// Gets or sets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// Gets or sets an error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; set; }
    }
}