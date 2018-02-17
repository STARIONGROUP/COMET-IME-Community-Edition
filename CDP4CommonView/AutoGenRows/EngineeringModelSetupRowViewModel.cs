// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public partial class EngineeringModelSetupRowViewModel : DefinedThingRowViewModel<EngineeringModelSetup>
    {

        /// <summary>
        /// Backing field for <see cref="Kind"/>
        /// </summary>
        private EngineeringModelKind kind;

        /// <summary>
        /// Backing field for <see cref="StudyPhase"/>
        /// </summary>
        private StudyPhaseKind studyPhase;

        /// <summary>
        /// Backing field for <see cref="EngineeringModelIid"/>
        /// </summary>
        private Guid engineeringModelIid;

        /// <summary>
        /// Backing field for <see cref="SourceEngineeringModelSetupIid"/>
        /// </summary>
        private Guid sourceEngineeringModelSetupIid;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">The <see cref="EngineeringModelSetup"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public EngineeringModelSetupRowViewModel(EngineeringModelSetup engineeringModelSetup, ISession session, IViewModelBase<Thing> containerViewModel) : base(engineeringModelSetup, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Kind
        /// </summary>
        public EngineeringModelKind Kind
        {
            get { return this.kind; }
            set { this.RaiseAndSetIfChanged(ref this.kind, value); }
        }

        /// <summary>
        /// Gets or sets the StudyPhase
        /// </summary>
        public StudyPhaseKind StudyPhase
        {
            get { return this.studyPhase; }
            set { this.RaiseAndSetIfChanged(ref this.studyPhase, value); }
        }

        /// <summary>
        /// Gets or sets the EngineeringModelIid
        /// </summary>
        public Guid EngineeringModelIid
        {
            get { return this.engineeringModelIid; }
            set { this.RaiseAndSetIfChanged(ref this.engineeringModelIid, value); }
        }

        /// <summary>
        /// Gets or sets the SourceEngineeringModelSetupIid
        /// </summary>
        public Guid SourceEngineeringModelSetupIid
        {
            get { return this.sourceEngineeringModelSetupIid; }
            set { this.RaiseAndSetIfChanged(ref this.sourceEngineeringModelSetupIid, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.Kind = this.Thing.Kind;
            this.StudyPhase = this.Thing.StudyPhase;
            this.EngineeringModelIid = this.Thing.EngineeringModelIid;
            if(this.Thing.SourceEngineeringModelSetupIid.HasValue)
            {
                this.SourceEngineeringModelSetupIid = this.Thing.SourceEngineeringModelSetupIid.Value;
            }
        }
    }
}
