// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemEngineeringModelDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// Represents the view-model for the menu-item based on the <see cref="EngineeringModel"/>s available which opens a <see cref="IPanelViewModel"/>
    /// </summary>
    public class RibbonMenuItemEngineeringModelDependentViewModel : RibbonMenuItemViewModelBase
    {
        /// <summary>
        /// The <see cref="EngineeringModel"/> represented 
        /// </summary>
        public readonly EngineeringModel Model;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// The Function returning an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        private readonly Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemEngineeringModelDependentViewModel"/> class
        /// </summary>
        /// <param name="model">
        /// The <see cref="EngineeringModel"/> associated to this menu item view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="instantiatePanelViewModelFunction">The function that creates an instance of the <see cref="IPanelViewModel"/> for this menu-item</param>
        public RibbonMenuItemEngineeringModelDependentViewModel(EngineeringModel model, ISession session, Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPanelViewModel> instantiatePanelViewModelFunction)
            : base(session.DataSourceUri + ": " + model.EngineeringModelSetup.ShortName, session)
        {
            this.Model = model;
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;
            this.SetProperties();
        }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are opened
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// Gets the name of this <see cref="RibbonMenuItemEngineeringModelDependentViewModel"/>
        /// </summary>
        public string Name
        {
            get { return this.name; }
            private set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets the description of this <see cref="RibbonMenuItemEngineeringModelDependentViewModel"/>
        /// </summary>
        public string Description
        {
            get { return this.description; }
            private set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Returns an instance of <see cref="IPanelViewModel"/> which is <see cref="Model"/> dependent
        /// </summary>
        /// <returns>An instance of <see cref="IPanelViewModel"/></returns>
        protected override IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.Model.Iteration.First(), this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService);
        }

        /// <summary>
        /// Set the properties
        /// </summary>
        private void SetProperties()
        {
            this.Name = this.Model.EngineeringModelSetup.Name;
            this.Description = this.Model.EngineeringModelSetup.Definition.Any()
                ? this.Model.EngineeringModelSetup.Definition.First().Content
                : string.Empty;
        }
    }
}