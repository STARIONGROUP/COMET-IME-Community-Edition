// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionValueRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.ViewModels;
    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a Value of the <see cref="RelationalExpression"/> that contains it.
    /// </summary>
    public class RelationalExpressionValueRowViewModel : PrimitiveRow<string>, IValueSetRow
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// The <see cref="ParameterType"/> of the container <see cref="RelationalExpression"/>
        /// </summary>
        private ParameterType parameterType;

        public RelationalExpressionValueRowViewModel(ParameterType parameterType)
        {
            this.parameterType = parameterType;
            this.ParameterTypeClassKind = parameterType.ClassKind;
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                var enumPt = this.parameterType as EnumerationParameterType;
                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
            }
        }

        /// <summary>
        /// Gets the list of possible <see cref="EnumerationValueDefinition"/> for this <see cref="ParameterType"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinition> EnumerationValueDefinition
        {
            get
            {
                var enumValues = new ReactiveList<EnumerationValueDefinition>();

                var enumPt = this.parameterType as EnumerationParameterType;
                if (enumPt != null)
                {
                    enumValues.AddRange(enumPt.ValueDefinition);
                }

                return enumValues;
            }
        }
    }
}
