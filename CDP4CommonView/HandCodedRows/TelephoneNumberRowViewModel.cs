// ------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="TelephoneNumberRowViewModel"/>
    /// </summary>
    public partial class TelephoneNumberRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="VcardType"/>
        /// </summary>
        private string vcardType;

        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Gets or sets a value indicating if this is the default phone number
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }

        /// <summary>
        /// Gets or sets the applicable types
        /// </summary>
        public string VcardType
        {
            get { return this.vcardType; }
            set { this.RaiseAndSetIfChanged(ref this.vcardType, value); }
        }
    }
}