// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSessionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SiteRdlSessionRowViewModel"/> is to represent a <see cref="Session"/> 
    /// and its <see cref="SiteDirectory"/>
    /// </summary>
    public class SiteRdlSessionRowViewModel : RowViewModelBase<Thing>
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortname;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlSessionRowViewModel"/> class.
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> that is represented by the row-view-model
        /// </param>
        /// <param name="session">
        /// The <see cref="CDP4Dal.Session"/> that is represented by the row-view-model
        /// </param>
        /// <param name="containerViewModel">
        /// The container <see cref="IViewModelBase{T}"/>
        /// </param>
        public SiteRdlSessionRowViewModel(SiteDirectory siteDirectory, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(siteDirectory, session, containerViewModel)
        {
            this.Name = siteDirectory.Name;
            this.ShortName = siteDirectory.ShortName;

            foreach (var siteRdl in siteDirectory.SiteReferenceDataLibrary)
            {
                this.AddSiteRdlRowViewModel(siteRdl);
            }
        }

        /// <summary>
        /// Gets or sets the Name of the <see cref="SiteDirectory"/> that contains this RDL
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the Name of the <see cref="SiteDirectory"/> that contains this RDL
        /// </summary>
        public string ShortName
        {
            get { return this.shortname; }
            set { this.RaiseAndSetIfChanged(ref this.shortname, value); }
        }

        /// <summary>
        /// Add a <see cref="SiteRdlRowViewModel"/> to the list of contained rows
        /// </summary>
        /// <param name="rdl">
        /// The <see cref="SiteReferenceDataLibrary"/> that is to be added
        /// </param>
        private void AddSiteRdlRowViewModel(SiteReferenceDataLibrary rdl)
        {
            if (this.ContainedRows.All(x => x.Thing != rdl))
            {
                var row = new SiteRdlRowViewModel(rdl, this.Session, this);
                this.ContainedRows.Add(row);
            }
        }
    }
}
