// -------------------------------------------------------------------------------------------------
// <copyright file="ValidationOverrideAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;

    /// <summary>
    /// The attribute applicable to properties to force validation service override.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ValidationOverrideAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationOverrideAttribute"/> class.
        /// </summary>
        /// <param name="isValidationEnabled">
        /// A bool indicating whether the validation for this property is still enabled.
        /// </param>
        /// <param name="validationOverrideName">
        /// The name of the rule that has to be used in the override.
        /// </param>
        public ValidationOverrideAttribute(bool isValidationEnabled = false, string validationOverrideName = null)
        {
            this.IsValidationEnabled = isValidationEnabled;

            if (this.IsValidationEnabled && validationOverrideName == null)
            {
                throw new InvalidOperationException("If the validation is enabled for this ValidationOverride a rule name must be provided.");
            }

            this.ValidationOverrideName = validationOverrideName;
        }

        /// <summary>
        /// Gets a value indicating whether validation is still enabled for this property.
        /// </summary>
        public bool IsValidationEnabled { get; private set; }
        
        /// <summary>
        /// Gets the name of the rule that should be used for this overrides.
        /// </summary>
        public string ValidationOverrideName { get; private set; }
    }
}
