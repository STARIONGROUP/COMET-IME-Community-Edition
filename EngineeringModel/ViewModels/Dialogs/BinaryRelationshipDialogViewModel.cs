// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// The view-model used to create, edit or inspect a <see cref="BinaryRelationship"/> from the associated dialog.
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.BinaryRelationship)]
    public class BinaryRelationshipDialogViewModel : CDP4CommonView.BinaryRelationshipDialogViewModel, IThingDialogViewModel
    {
        #region private fields

        /// <summary>
        /// Backinf field for <see cref="IsSourceSelected"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> isSourceSelected;

        /// <summary>
        /// The Required Referance-Data-library for the current <see cref="Iteration"/>
        /// </summary>
        private ModelReferenceDataLibrary mRdl;

        /// <summary>
        /// Backinf field for <see cref="SelectedSourceClasskind"/>
        /// </summary>
        private ClassKind selectedSourceClasskind;

        /// <summary>
        /// Backinf field for <see cref="SelectedTargetClasskind"/>
        /// </summary>
        private ClassKind selectedTargetClasskind;

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BinaryRelationshipDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipDialogViewModel"/> class
        /// </summary>
        /// <param name="binaryRelationship">
        /// The <see cref="BinaryRelationship"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public BinaryRelationshipDialogViewModel(BinaryRelationship binaryRelationship, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(binaryRelationship, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        #endregion

        #region public Properties

        /// <summary>
        /// Gets or sets the possible <see cref="ClassKind"/>
        /// </summary>
        public List<ClassKind> PossibleClassKind { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ClassKind"/> of the source
        /// </summary>
        public ClassKind SelectedSourceClasskind
        {
            get { return this.selectedSourceClasskind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSourceClasskind, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ClassKind"/> of the target
        /// </summary>
        public ClassKind SelectedTargetClasskind
        {
            get { return this.selectedTargetClasskind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTargetClasskind, value); }
        }

        #endregion

        #region Overriden methods
        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;
            this.mRdl = model.EngineeringModelSetup.RequiredRdl.Single();
        }

        /// <summary>
        /// Initializes the subscribers
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.WhenAnyValue(x => x.SelectedSourceClasskind).Subscribe(x => { this.PopulatePossibleSource(); });
            this.WhenAnyValue(x => x.SelectedTargetClasskind).Subscribe(x => { this.PopulatePossibleTarget(); });
            this.WhenAnyValue(x => x.SelectedSource).Subscribe(x => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.SelectedTarget).Subscribe(x => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(x => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Populate the <see cref="PossibleSource"/> property
        /// </summary>
        protected override void PopulatePossibleSource()
        {
            base.PopulatePossibleSource();

            var possibleSources = this.Session.Assembler.Cache.Values.Select(x => x.Value).Where(x => x.ClassKind == this.SelectedSourceClasskind).ToList();

            // Avoid setting the same Thing as source and target
            if (this.SelectedTarget != null && this.SelectedTarget.ClassKind == this.SelectedSourceClasskind)
            {
                possibleSources.Remove(possibleSources.Single(t => t == this.SelectedTarget));
            }

            this.PossibleSource.AddRange(possibleSources);
        }

        /// <summary>
        /// Populate the <see cref="PossibleTarget"/> property
        /// </summary>
        protected override void PopulatePossibleTarget()
        {
            base.PopulatePossibleTarget();
            var possibleTargets = this.Session.Assembler.Cache.Values.Select(x => x.Value).Where(x => x.ClassKind == this.SelectedTargetClasskind).ToList();
            
            // Avoid setting the same Thing as source and target
            if (this.SelectedSource != null && this.SelectedSource.ClassKind == this.SelectedTargetClasskind)
            {
                possibleTargets.Remove(possibleTargets.Single(t => t == this.SelectedSource));
            }

            this.PossibleTarget.AddRange(possibleTargets);
        }

        /// <summary>
        /// Populate the <see cref="PossibleOwner"/> property
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();

            var engineeringModel = (EngineeringModel)this.Container.TopContainer;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
        }

        /// <summary>
        /// Populate the <see cref="PossibleCategory"/> and <see cref="Category"/> properties.
        /// </summary>
        protected override void PopulateCategory()
        {
            var categories = this.mRdl.DefinedCategory.Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship)).ToList();
            categories.AddRange(this.mRdl.GetRequiredRdls().SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship)));

            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories.OrderBy(c => c.ShortName));
            base.PopulateCategory();
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedSource != null && this.SelectedTarget != null &&
                                this.SelectedOwner != null;
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            this.PopulatePossibleClasskind();
            this.SelectedSource = this.Thing.Source;
            this.SelectedTarget = this.Thing.Target;
            this.SelectedOwner = this.Thing.Owner;

            if (this.SelectedSource != null)
            {
                this.SelectedSourceClasskind = this.SelectedSource.ClassKind;
            }

            if (this.SelectedTarget != null)
            {
                this.SelectedTargetClasskind = this.SelectedTarget.ClassKind;
            }

            this.PopulateCategory();
        }

        #endregion

        /// <summary>
        /// Populates the <see cref="CategoryDialogViewModel.PermissibleClass"/> property
        /// </summary>
        private void PopulatePossibleClasskind()
        {
            this.PossibleClassKind = new List<ClassKind>();
            var allClassKinds = typeof(Thing).Assembly.GetTypes().Where(t => typeof(Thing).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .OrderBy(x => x.Name).Select(x => (ClassKind)Enum.Parse(typeof(ClassKind), x.Name));
            this.PossibleClassKind.AddRange(allClassKinds);
        }
    }
}