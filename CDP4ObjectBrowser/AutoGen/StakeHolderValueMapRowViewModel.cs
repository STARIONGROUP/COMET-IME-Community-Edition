// -------------------------------------------------------------------------------------------------
// <copyright file="StakeHolderValueMapRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="StakeHolderValueMap"/>
    /// </summary>
    public partial class StakeHolderValueMapRowViewModel : DefinedThingRowViewModel<StakeHolderValueMap>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="SettingsRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel settingsFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="StakeHolderValueMapRowViewModel"/> class
        /// </summary>
        /// <param name="stakeHolderValueMap">The <see cref="StakeHolderValueMap"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public StakeHolderValueMapRowViewModel(StakeHolderValueMap stakeHolderValueMap, ISession session, IViewModelBase<Thing> containerViewModel) : base(stakeHolderValueMap, session, containerViewModel)
        {
            this.settingsFolder = new CDP4Composition.FolderRowViewModel("Settings", "Settings", this.Session, this);
            this.ContainedRows.Add(this.settingsFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.Settings, this.settingsFolder, this.AddSettingsRowViewModel);
        }
        /// <summary>
        /// Add an Settings row view model to the list of <see cref="Settings"/>
        /// </summary>
        /// <param name="settings">
        /// The <see cref="Settings"/> that is to be added
        /// </param>
        private StakeHolderValueMapSettingsRowViewModel AddSettingsRowViewModel(StakeHolderValueMapSettings settings)
        {
            return new StakeHolderValueMapSettingsRowViewModel(settings, this.Session, this);
        }
    }
}
