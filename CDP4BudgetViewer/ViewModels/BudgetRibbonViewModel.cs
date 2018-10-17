// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System.Diagnostics;

    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using NLog;
    using Views;

    /// <summary>
    /// The view-model for <see cref="BudgetRibbon"/> containing the controls in the "View" Page for this module
    /// </summary>
    public class BudgetRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetRibbonViewModel"/> class
        /// </summary>
        public BudgetRibbonViewModel() : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of <see cref="BudgetViewerViewModel"/>
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <returns>An instance of <see cref="BudgetViewerViewModel"/></returns>
        public static BudgetViewerViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            return new BudgetViewerViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService);
        }
    }
}