// -------------------------------------------------------------------------------------------------
// <copyright file="EnumValueMappingRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The row used to map an <see cref="EnumValue"/> to a <see cref="EnumerationValueDefinition"/>
    /// </summary>
    public class EnumValueMappingRowViewModel : MappingRowViewModelBase<EnumValue>
    {
        /// <summary>
        /// The parent row
        /// </summary>
        private readonly DatatypeDefinitionMappingRowViewModel parentRow;

        /// <summary>
        /// Backing field for <see cref="MappedThing"/>
        /// </summary>
        private EnumerationValueDefinition mappedThing;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValueMappingRowViewModel"/> class
        /// </summary>
        /// <param name="enumValue">The <see cref="EnumValue"/> to map</param>
        /// <param name="parentRow">The parent <see cref="DatatypeDefinitionMappingRowViewModel"/> row </param>
        public EnumValueMappingRowViewModel(EnumValue enumValue, DatatypeDefinitionMappingRowViewModel parentRow) : base(enumValue)
        {
            this.parentRow = parentRow;
            this.PossibleThings = new ReactiveList<EnumerationValueDefinition>();
            this.WhenAnyValue(x => x.MappedThing).Subscribe(_ => this.parentRow.UpdateMappingStatus());
        }

        /// <summary>
        /// Gets or sets the mapped <see cref="EnumerationValueDefinition"/>
        /// </summary>
        public EnumerationValueDefinition MappedThing
        {
            get { return this.mappedThing; }
            set { this.RaiseAndSetIfChanged(ref this.mappedThing, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="EnumerationValueDefinition"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinition> PossibleThings { get; private set; }

        /// <summary>
        /// Populate the possible <see cref="EnumerationValueDefinition"/> list
        /// </summary>
        public void PopulatePossibleEnumValueDefinition()
        {
            this.PossibleThings.Clear();
            if (this.parentRow.MappedThing == null)
            {
                this.MappedThing = null;
                return;
            }

            this.PossibleThings.AddRange(((EnumerationParameterType)this.parentRow.MappedThing).ValueDefinition);

            // Make a suggestion by comparing EnumerationParameterType.ShortName - EnumValue.Properties.Item.OtherContent
            var reqIfLitteral = this.Identifiable.Properties.OtherContent.ToLower();

            var interSectRes = this.PossibleThings.Select(x => x.ShortName.ToLower().Intersect(reqIfLitteral).Count()).ToList();
            var intersectMax = interSectRes.Max();

            var occurenceNumber = interSectRes.Count(x => x == intersectMax);
            if (occurenceNumber == 1)
            {
                var suggestionIndex = interSectRes.FindIndex(x => x == interSectRes.Max());
                this.MappedThing = this.PossibleThings.ElementAt(suggestionIndex);
            }
            else
            {
                this.MappedThing = null;
            }
        }

        /// <summary>
        /// Update the <see cref="EnumValueMappingRowViewModel.IsMapped"/> property
        /// </summary>
        protected override void UpdateIsMapped()
        {
            this.IsMapped = this.MappedThing != null;
        }
    }
}