﻿// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.ViewModels
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    public class RelationshipEditorRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipEditorRibbonViewModel"/> class
        /// </summary>
        public RelationshipEditorRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of <see cref="RelationshipEditorRibbonViewModel"/>
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <returns>An instance of <see cref="RelationshipEditorViewModel"/></returns>
        public static RelationshipEditorViewModel InstantiatePanelViewModel(Iteration iteration, ISession session,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            var model = iteration.Container as EngineeringModel;
            if (model == null)
            {
                throw new InvalidOperationException("The container of an Iteration cannot be anything else than an Engineering Model.");
            }

            var participant = model.GetActiveParticipant(session.ActivePerson);
            if (participant == null)
            {
                throw new InvalidOperationException("The Participant in an engineering model cannot be null");
            }

            return new RelationshipEditorViewModel(iteration, participant, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService);
        }
    }
}
