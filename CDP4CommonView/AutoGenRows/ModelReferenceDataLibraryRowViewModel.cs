// -------------------------------------------------------------------------------------------------
// <copyright file="ModelReferenceDataLibraryRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="ModelReferenceDataLibrary"/>
    /// </summary>
    public partial class ModelReferenceDataLibraryRowViewModel : ReferenceDataLibraryRowViewModel<ModelReferenceDataLibrary>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelReferenceDataLibraryRowViewModel"/> class
        /// </summary>
        /// <param name="modelReferenceDataLibrary">The <see cref="ModelReferenceDataLibrary"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ModelReferenceDataLibraryRowViewModel(ModelReferenceDataLibrary modelReferenceDataLibrary, ISession session, IViewModelBase<Thing> containerViewModel) : base(modelReferenceDataLibrary, session, containerViewModel)
        {
        }


	
    }
}
