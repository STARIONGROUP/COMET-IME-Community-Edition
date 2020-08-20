// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceDialogViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;    
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Utilities;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ReferenceSourceDialogViewModel"/> is to allow a <see cref="ReferenceSource"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="ReferenceSource"/> will result in an <see cref="ReferenceSource"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.ReferenceSource)]
    public class ReferenceSourceDialogViewModel : CDP4CommonView.ReferenceSourceDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The backing field for <see cref="ShortName"/> property.
        /// </summary>
        private string shortName;
        
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ReferenceSourceDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceDialogViewModel"/> class.
        /// </summary>
        /// <param name="referenceSource">
        /// The <see cref="ReferenceSource"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="ReferenceSourceDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="ReferenceSourceDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ReferenceSourceDialogViewModel(ReferenceSource referenceSource, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(referenceSource, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulatePossiblePublishedIn());            
        }
        #endregion

        /// <summary>
        /// Initializes the properties of this <see cref="ReferenceSource"/> view-model
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.PopulatePossibleLanguageCode();
        }
        
        /// <summary>
        /// Gets or sets the ShortName of the current <see cref="ReferenceSource"/>
        /// </summary>
        /// <remarks>
        /// The validation of the shortname has been disabled
        /// </remarks>
        [ValidationOverride(false)]
        public override string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }
        
        /// <summary>
        /// Gets the possible <see cref="LanguageCodeUsage"/>
        /// </summary>
        public ReactiveList<string> PossibleLanguage { get; private set; }

        /// <summary>
        /// Populates the <see cref="PossibleLanguage"/> <see cref="ReactiveList{T}"/>
        /// </summary>
        private void PopulatePossibleLanguageCode()
        {
            this.PossibleLanguage = new ReactiveList<string>();

            foreach (var cultureInfo in CultureInfoUtility.CultureInfoAvailable)
            {
                this.PossibleLanguage.Add(cultureInfo.Name);
            }
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.ReferenceSourceDialogViewModel.PossiblePublisher"/> property
        /// </summary>
        protected override void PopulatePossiblePublisher()
        {
            base.PopulatePossiblePublisher();

            var siteDirectory = this.Session.RetrieveSiteDirectory();
            
            this.PossiblePublisher.AddRange(siteDirectory.Organization);
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.ReferenceSourceDialogViewModel.PossiblePublishedIn"/> property
        /// </summary>
        protected override void PopulatePossiblePublishedIn()
        {
            base.PopulatePossiblePublishedIn();

            var rdlContainer = this.Container as ReferenceDataLibrary;

            if (rdlContainer != null)
            {
                var allPossibleReferenceSources = new List<ReferenceSource>(rdlContainer.ReferenceSource);

                foreach (var rdl in rdlContainer.GetRequiredRdls())
                {
                    allPossibleReferenceSources.AddRange(rdl.ReferenceSource);
                }

                var currentReferenceSource = allPossibleReferenceSources.SingleOrDefault(x => x.Iid == this.Thing.Iid);
                if (currentReferenceSource != null)
                {
                    allPossibleReferenceSources.Remove(currentReferenceSource);
                }
                
                allPossibleReferenceSources.OrderBy(x => x.ShortName);

                this.PossiblePublishedIn.AddRange(allPossibleReferenceSources);
            }
        }
    }
}
