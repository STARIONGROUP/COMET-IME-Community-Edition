// -------------------------------------------------------------------------------------------------
// <copyright file="TextParameterTypeRowViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="TextParameterType"/>
    /// </summary>
    public partial class TextParameterTypeRowViewModel : ScalarParameterTypeRowViewModel<TextParameterType>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TextParameterTypeRowViewModel"/> class
        /// </summary>
        /// <param name="textParameterType">The <see cref="TextParameterType"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public TextParameterTypeRowViewModel(TextParameterType textParameterType, ISession session, IViewModelBase<Thing> containerViewModel) : base(textParameterType, session, containerViewModel)
        {
        }


	
    }
}
