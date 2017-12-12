// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryNoteRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="BinaryNote"/>
    /// </summary>
    public partial class BinaryNoteRowViewModel : NoteRowViewModel<BinaryNote>
    {

        /// <summary>
        /// Backing field for <see cref="Caption"/>
        /// </summary>
        private string caption;

        /// <summary>
        /// Backing field for <see cref="FileType"/>
        /// </summary>
        private FileType fileType;

        /// <summary>
        /// Backing field for <see cref="FileTypeShortName"/>
        /// </summary>
        private string fileTypeShortName;

        /// <summary>
        /// Backing field for <see cref="FileTypeName"/>
        /// </summary>
        private string fileTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryNoteRowViewModel"/> class
        /// </summary>
        /// <param name="binaryNote">The <see cref="BinaryNote"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public BinaryNoteRowViewModel(BinaryNote binaryNote, ISession session, IViewModelBase<Thing> containerViewModel) : base(binaryNote, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Caption
        /// </summary>
        public string Caption
        {
            get { return this.caption; }
            set { this.RaiseAndSetIfChanged(ref this.caption, value); }
        }

        /// <summary>
        /// Gets or sets the FileType
        /// </summary>
        public FileType FileType
        {
            get { return this.fileType; }
            set { this.RaiseAndSetIfChanged(ref this.fileType, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="FileType"/>
        /// </summary>
        public string FileTypeShortName
        {
            get { return this.fileTypeShortName; }
            set { this.RaiseAndSetIfChanged(ref this.fileTypeShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="FileType"/>
        /// </summary>
        public string FileTypeName
        {
            get { return this.fileTypeName; }
            set { this.RaiseAndSetIfChanged(ref this.fileTypeName, value); }
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
            this.Caption = this.Thing.Caption;
            this.FileType = this.Thing.FileType;
        }
    }
}
