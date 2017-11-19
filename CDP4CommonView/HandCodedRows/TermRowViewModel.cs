// ------------------------------------------------------------------------------------------------
// <copyright file="TermRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="TermRowViewModel"/>
    /// </summary>
    public partial class TermRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="DefinitionValue"/>
        /// </summary>
        private string definitionValue;

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        /// <remarks>
        /// The unique definition in for a language
        /// </remarks>
        public string DefinitionValue
        {
            get { return this.definitionValue; }
            set { this.RaiseAndSetIfChanged(ref this.definitionValue, value); }
        }
    }
}