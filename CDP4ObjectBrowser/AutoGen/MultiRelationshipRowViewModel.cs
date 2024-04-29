// -------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="MultiRelationship"/>
    /// </summary>
    public partial class MultiRelationshipRowViewModel : RelationshipRowViewModel<MultiRelationship>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRowViewModel"/> class
        /// </summary>
        /// <param name="multiRelationship">The <see cref="MultiRelationship"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public MultiRelationshipRowViewModel(MultiRelationship multiRelationship, ISession session, IViewModelBase<Thing> containerViewModel) : base(multiRelationship, session, containerViewModel)
        {
            this.UpdateColumnValues();
        }

    }
}
