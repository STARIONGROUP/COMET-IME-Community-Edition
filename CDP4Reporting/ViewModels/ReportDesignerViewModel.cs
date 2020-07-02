// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Reporting.ViewModels
{

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    /// <summary>
    /// The view-model for the Relationship Editor that lets users edit Relationships between any 2 objects.
    /// </summary>
    public class ReportDesignerViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Reporting";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesignerViewModel"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> to display
        /// </param>
        /// <param name="participant">The <see cref="Participant"/> that this open <see cref="Iteration"/> is tied to.</param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="thingDialogNavigationService"></param>
        /// <param name="panelNavigationService">The panel navigation service.</param>
        public ReportDesignerViewModel(Iteration thing, Participant participant, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
