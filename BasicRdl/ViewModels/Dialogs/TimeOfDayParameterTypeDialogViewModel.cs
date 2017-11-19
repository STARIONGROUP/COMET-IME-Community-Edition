// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeOfDayParameterTypeDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="TimeOfDayParameterTypeDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="TimeOfDayParameterType"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.TimeOfDayParameterType)]
    public class TimeOfDayParameterTypeDialogViewModel : CDP4CommonView.TimeOfDayParameterTypeDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOfDayParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public TimeOfDayParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOfDayParameterTypeDialogViewModel"/> class
        /// </summary>
        /// <param name="timeOfDayParameterType">
        /// The <see cref="TimeOfDayParameterType"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="thingTransaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="thingDialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public TimeOfDayParameterTypeDialogViewModel(TimeOfDayParameterType timeOfDayParameterType, IThingTransaction thingTransaction, ISession session, bool isRoot, ThingDialogKind thingDialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(timeOfDayParameterType, thingTransaction, session, isRoot, thingDialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
    }
}