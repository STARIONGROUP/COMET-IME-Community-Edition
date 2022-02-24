// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Globalization;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// The extended row class representing an <see cref="Publication"/>
    /// </summary>
    public class PublicationRowViewModel : CDP4CommonView.PublicationRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationRowViewModel"/> class
        /// </summary>
        /// <param name="publication">The <see cref="Publication"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PublicationRowViewModel(Publication publication, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(publication, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets the name of the publication node. In this case references the <see cref="Thing.CreatedOn"/> property.
        /// </summary>
        /// <returns>The <see cref="Thing.CreatedOn"/> property in YYYY-MM-DD format.</returns>
        public string Name
        {
            get { return this.Thing.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> on which the publication was published
        /// </summary>
        /// <returns></returns>
        public DateTime PublicationDate
        {
            get { return this.Thing.CreatedOn; }
        }

        /// <summary>
        /// Gets the string representation of <see cref="DomainOfExpertiese"/>s that are owners of of one or more publishedParameter(s)
        /// </summary>
        public string OwnerShortName
        {
            get { return string.Join(", ", this.Thing.Domain.Select(d => d.ShortName)); }
        }

        /// <summary>
        /// Gets the empty string to put in modelCode column. Needed to avoid slowdowns connected to binding errors.
        /// </summary>
        public string ModelCode
        {
            get { return string.Empty; }
        }
    }
}