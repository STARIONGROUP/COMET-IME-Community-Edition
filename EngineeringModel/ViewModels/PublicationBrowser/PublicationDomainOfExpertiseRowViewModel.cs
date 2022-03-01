// -------------------------------------------------------------------------------------------------
// <copyright file="PublicationDomainOfExpertiseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
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
        /// <summary>
        /// Backinf field for <see cref="ToBePublished"/>
        /// </summary>
        private bool toBePublished;

        /// <summary>
        /// Backinf field for <see cref="IsEmpty"/>
        /// </summary>
        private bool isEmpty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4CommonView.DomainOfExpertiseRowViewModel"/> class
        /// </summary>
        /// <param name="domainOfExpertise">The <see cref="DomainOfExpertise"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{T}"/></param>
        public PublicationDomainOfExpertiseRowViewModel(DomainOfExpertise domainOfExpertise, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(domainOfExpertise, session, containerViewModel)
        {
            this.Disposables.Add(this.WhenAnyValue(vm => vm.ToBePublished).Subscribe(_ => this.ToBePublishedChanged()));
            this.Disposables.Add(this.ContainedRows.IsEmptyChanged.Subscribe(_ => this.SetIsEmpty()));
        }

        /// <summary>
        /// Set whether this row is Empty
        /// </summary>
        private void SetIsEmpty()
        {
            this.IsEmpty = this.ContainedRows.Count == 0;
        }

        /// <summary>
        /// Exute the change to publication selection.
        /// </summary>
        private void ToBePublishedChanged()
        {
            foreach (var row in this.ContainedRows.OfType<IPublishableRow>())
            {
                row.ToBePublished = this.ToBePublished;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the row is to be published.
        /// </summary>
        public bool ToBePublished
        {
            get { return this.toBePublished; }
            set { this.RaiseAndSetIfChanged(ref this.toBePublished, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the row is to be published.
        /// </summary>
        public bool IsEmpty
        {
            get { return this.isEmpty; }
            set { this.RaiseAndSetIfChanged(ref this.isEmpty, value); }
        }
    }
}
