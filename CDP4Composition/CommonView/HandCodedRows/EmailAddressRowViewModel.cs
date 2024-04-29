// ------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="EmailAddressRowViewModel"/>
    /// </summary>
    public partial class EmailAddressRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Gets or sets a value indicating whether this is the default email address
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }
    }
}