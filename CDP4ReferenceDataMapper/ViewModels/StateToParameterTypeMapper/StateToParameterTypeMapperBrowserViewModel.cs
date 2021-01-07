// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ReferenceDataMapper.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// A vew-model that allows a user to map parameters that are not state dependent to other state dependent parameters
    /// </summary>
    public class StateToParameterTypeMapperBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for the <see cref="SelectedElementDefinitionCategory"/>
        /// </summary>
        private Category selectedElementDefinitionCategory;

        /// <summary>
        /// Backing field for the <see cref="SelectedActualFiniteStateList"/>
        /// </summary>
        private ActualFiniteStateList selectedActualFiniteStateList;

        /// <summary>
        /// Backing field for the <see cref="SelectedSourceParameterTypeCategory"/>
        /// </summary>
        private Category selectedSourceParameterTypeCategory;

        /// <summary>
        /// Backing field for the <see cref="SelectedTargetMappingParameterType"/>
        /// </summary>
        private TextParameterType selectedTargetMappingParameterType;

        /// <summary>
        /// Backing field for the <see cref="SelectedTargetValueParameterType"/>
        /// </summary>
        private ScalarParameterType selectedTargetValueParameterType;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Actual Finite State to ParameterType mapping";

        /// <summary>
        /// Initializes a new instance of the <see cref="StateToParameterTypeMapperBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="EngineeringModel"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public StateToParameterTypeMapperBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.PossibleElementDefinitionCategory = new ReactiveList<Category>();
            this.PossibleActualFiniteStateList = new ReactiveList<ActualFiniteStateList>();
            this.PossibleSourceParameterTypeCategory = new ReactiveList<Category>();
            this.PossibleTargetMappingParameterType = new ReactiveList<TextParameterType>();
            this.PossibleTargetValueParameterType = new ReactiveList<ScalarParameterType>();

            //this.AddSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup => this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>();

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Category"/> used to filter the <see cref="ElementDefinition"/>
        /// </summary>
        public Category SelectedElementDefinitionCategory
        {
            get => this.selectedElementDefinitionCategory;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementDefinitionCategory, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{Category}"/> from which the <see cref="ElementDefinition"/> <see cref="Category"/> can be selected
        /// </summary>
        public ReactiveList<Category> PossibleElementDefinitionCategory { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteStateList"/>
        /// </summary>
        public ActualFiniteStateList SelectedActualFiniteStateList
        {
            get => this.selectedActualFiniteStateList;
            set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteStateList, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{ActualFiniteStateList}"/> from which the <see cref="ActualFiniteStateList"/> can be selected
        /// </summary>
        public ReactiveList<ActualFiniteStateList> PossibleActualFiniteStateList { get; private set; }

        /// <summary>
        /// Gets or sets the selected source <see cref="ParameterType"/> <see cref="Category"/>
        /// </summary>
        public Category SelectedSourceParameterTypeCategory
        {
            get => this.selectedSourceParameterTypeCategory;
            set => this.RaiseAndSetIfChanged(ref this.selectedSourceParameterTypeCategory, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{Category}"/> from which the <see cref="ParameterType"/> <see cref="Category"/> can be selected
        /// </summary>
        public ReactiveList<Category> PossibleSourceParameterTypeCategory { get; private set; }

        /// <summary>
        /// Gets or sets the selected target <see cref="TextParameterType"/> that is to select a <see cref="Parameter"/> in which the
        /// mapping is to be stored
        /// </summary>
        public TextParameterType SelectedTargetMappingParameterType
        {
            get => this.selectedTargetMappingParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedTargetMappingParameterType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{ScalarParameterType}"/> from which the <see cref="TextParameterType"/>
        /// </summary>
        public ReactiveList<TextParameterType> PossibleTargetMappingParameterType { get; private set; }

        /// <summary>
        /// Gets or sets the selected target <see cref="ScalarParameterType"/> that is to select a <see cref="Parameter"/> in which the
        /// mapped (manual) value is to be stored
        /// </summary>
        public ScalarParameterType SelectedTargetValueParameterType
        {
            get => this.selectedTargetValueParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedTargetValueParameterType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{ScalarParameterType}"/> from which the <see cref="ScalarParameterType"/>
        /// </summary>
        public ReactiveList<ScalarParameterType> PossibleTargetValueParameterType { get; private set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            //custom commands go here
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";

            this.PopulatePossibleElementDefinitionCategory();
            this.PopulatePossibleActualFiniteStateList();
            this.PopulatePossibleSourceParameterTypeCategory();
            this.PopulatePossibleTargetMappingParameterType();
            this.PopulatePossibleTargetValueParameterType();
        }

        /// <summary>
        /// Populates the <see cref="PossibleElementDefinitionCategory"/> with <see cref="Category"/> applicable to <see cref="ClassKind.ElementDefinition"/>
        /// </summary>
        private void PopulatePossibleElementDefinitionCategory()
        {
            this.PossibleElementDefinitionCategory.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedCategories = new List<Category>(mrdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(ClassKind.ElementDefinition)));
            allowedCategories.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory).Where(c => c.PermissibleClass.Contains(ClassKind.ElementDefinition)));

            this.PossibleElementDefinitionCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populates the <see cref="PossibleActualFiniteStateList"/> with <see cref="ActualFiniteStateList"/> in the current <see cref="Iteration"/>
        /// </summary>
        private void PopulatePossibleActualFiniteStateList()
        {
            this.PossibleActualFiniteStateList.Clear();
            
            this.PossibleActualFiniteStateList.AddRange(this.Thing.ActualFiniteStateList.OrderBy(x => x.ShortName));
        }

        /// <summary>
        /// Populates the <see cref="PossibleSourceParameterTypeCategory"/> with <see cref="Category"/> applicable to <see cref="ClassKind.ParameterType"/>
        /// </summary>
        /// <remarks>
        /// Since we are using data that originated from a catalogue, the reference data cannot come from a model rdl, only from a chain of SiteReferenceDataLibrary
        /// </remarks>
        private void PopulatePossibleSourceParameterTypeCategory()
        {
            this.PossibleSourceParameterTypeCategory.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var possiblePermissibleClasses = typeof(Thing).Assembly.GetTypes().Where(t => typeof(ScalarParameterType).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).OrderBy(x => x.Name).Select(x => (ClassKind)Enum.Parse(typeof(ClassKind), x.Name)).ToList();
            var allCategories = new List<Category>(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)).OrderBy(x => x.ShortName);
            
            foreach (var category in allCategories)
            {
                var hasMatch = category.PermissibleClass
                    .Intersect(possiblePermissibleClasses)
                    .Any();

                if (hasMatch)
                {
                    this.PossibleSourceParameterTypeCategory.Add(category);
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleTargetMappingParameterType"/> with <see cref="ScalarParameterType"/>
        /// </summary>
        private void PopulatePossibleTargetMappingParameterType()
        {
            this.PossibleTargetMappingParameterType.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedParameterTypes = new List<TextParameterType>(mrdl.ParameterType.OfType<TextParameterType>());
            allowedParameterTypes.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType).OfType<TextParameterType>());

            this.PossibleTargetMappingParameterType.AddRange(allowedParameterTypes.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populates the <see cref="PossibleTargetValueParameterType"/> with <see cref="ScalarParameterType"/>
        /// </summary>
        private void PopulatePossibleTargetValueParameterType()
        {
            this.PossibleTargetValueParameterType.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedParameterTypes = new List<ScalarParameterType>(mrdl.ParameterType.OfType<ScalarParameterType>());
            allowedParameterTypes.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType).OfType<ScalarParameterType>());

            this.PossibleTargetValueParameterType.AddRange(allowedParameterTypes.OrderBy(c => c.ShortName));
        }
    }
}
