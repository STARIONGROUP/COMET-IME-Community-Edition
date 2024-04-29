// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationDomainOfExpertiseRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;

    /// <summary>
    /// the class used by the <see cref="EngineeringModelSetupDialogViewModel"/> to add the active domains
    /// </summary>
    public class DomainOfExpertiseRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsUncheckable"/>
        /// </summary>
        private bool isUncheckable;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseRowViewModel"/> class
        /// </summary>
        /// <param name="domain">The <see cref="DomainOfExpertise"/> represented</param>
        /// <param name="isUncheckable"></param>
        public DomainOfExpertiseRowViewModel(DomainOfExpertise domain, bool isUncheckable)
        {
            this.Domain = domain;
            this.IsUncheckable = isUncheckable;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row may be unchecked
        /// </summary>
        public bool IsUncheckable
        {
            get { return this.isUncheckable; }
            private set { this.RaiseAndSetIfChanged(ref this.isUncheckable, value); }
        }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/> represented
        /// </summary>
        public DomainOfExpertise Domain { get; private set; }
    }
}