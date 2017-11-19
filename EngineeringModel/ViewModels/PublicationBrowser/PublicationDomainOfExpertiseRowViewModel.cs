// -------------------------------------------------------------------------------------------------
// <copyright file="PublicationDomainOfExpertiseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using PublicationBrowser;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="DomainOfExpertiseRowViewModel"/> view
    /// </summary>
    public class PublicationDomainOfExpertiseRowViewModel : CDP4CommonView.DomainOfExpertiseRowViewModel, IPublishableRow
    {
        private bool toBePublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4CommonView.DomainOfExpertiseRowViewModel"/> class
        /// </summary>
        /// <param name="domainOfExpertise">The <see cref="DomainOfExpertise"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{T}"/></param>
        public PublicationDomainOfExpertiseRowViewModel(DomainOfExpertise domainOfExpertise, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(domainOfExpertise, session, containerViewModel)
        {
            this.WhenAnyValue(vm => vm.ToBePublished).Subscribe(_ => this.ToBePublishedChanged());
        }

        /// <summary>
        /// Exute the change to publication selection.
        /// </summary>
        private void ToBePublishedChanged()
        {
            Parallel.ForEach(this.ContainedRows, row =>
            {
                ((IPublishableRow)row).ToBePublished = this.ToBePublished;
            });
        }

        /// <summary>
        /// Gets or sets a value indicating whether the row is to be published.
        /// </summary>
        public bool ToBePublished
        {
            get { return this.toBePublished; }
            set { this.RaiseAndSetIfChanged(ref this.toBePublished, value); }
        }
    }
}
