﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SimpleQuantityKindRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
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
    /// Row class representing a <see cref="SimpleQuantityKind"/>
    /// </summary>
    public partial class SimpleQuantityKindRowViewModel : QuantityKindRowViewModel<SimpleQuantityKind>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQuantityKindRowViewModel"/> class
        /// </summary>
        /// <param name="simpleQuantityKind">The <see cref="SimpleQuantityKind"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public SimpleQuantityKindRowViewModel(SimpleQuantityKind simpleQuantityKind, ISession session, IViewModelBase<Thing> containerViewModel) : base(simpleQuantityKind, session, containerViewModel)
        {
        }


	
    }
}
