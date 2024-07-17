// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeDefinitionMappingRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Requirements.ReqIFDal;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The row view model for the <see cref="AttributeDefinition"/> of a <see cref="SpecType"/>
    /// </summary>
    public class AttributeDefinitionMappingRowViewModel : MappingRowViewModelBase<AttributeDefinition>
    {
        /// <summary>
        /// The mapped <see cref="ParameterType"/>
        /// </summary>
        public readonly ParameterType ParameterType;

        /// <summary>
        /// Backing field for <see cref="AttributeDefinitionMapKind"/>
        /// </summary>
        private AttributeDefinitionMapKind attributeDefinitionMapKind;


        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeDefinitionMappingRowViewModel"/> class
        /// </summary>
        /// <param name="attributeDefinition">The <see cref="AttributeDefinition"/></param>
        /// <param name="mappedParameterType">The <see cref="ParameterType"/> associated to this <see cref="AttributeDefinition"/></param>
        /// <param name="refreshValidation">The action that refreshes the IsRequirement property for the other child rows.</param>
        public AttributeDefinitionMappingRowViewModel(AttributeDefinition attributeDefinition, ParameterType mappedParameterType, Action refreshValidation)
            : base(attributeDefinition)
        {
            this.ParameterType = mappedParameterType;
            this.WhenAnyValue(x => x.AttributeDefinitionMapKind).Subscribe(x =>
            {
                this.UpdateIsMapped();
                refreshValidation();
            });
        }

        /// <summary>
        /// Gets or sets the <see cref="AttributeDefinitionMapKind"/> representing the kind of mapping of this <see cref="AttributeDefinitionMappingRowViewModel.Identifiable"/>
        /// </summary>
        public AttributeDefinitionMapKind AttributeDefinitionMapKind
        {
            get { return this.attributeDefinitionMapKind; }
            set { this.RaiseAndSetIfChanged(ref this.attributeDefinitionMapKind, value); }
        }

        /// <summary>
        /// Update the <see cref="AttributeDefinitionMappingRowViewModel.IsMapped"/> property
        /// </summary>
        protected override void UpdateIsMapped()
        {
            this.IsMapped = this.AttributeDefinitionMapKind != AttributeDefinitionMapKind.PARAMETER_VALUE || this.ParameterType != null;
        }
}
}