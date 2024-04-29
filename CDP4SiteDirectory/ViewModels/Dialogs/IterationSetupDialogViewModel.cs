// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4SiteDirectory.Views;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="IterationSetupDialog"/> view used to create, edit or inspect an <see cref="IterationSetup"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.IterationSetup)]
    public class IterationSetupDialogViewModel : CDP4CommonView.IterationSetupDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSourceIterationSetupRow"/>
        /// </summary>
        private IterationSetupListBoxItem selectedSourceIterationSetupRow;

        #region Constructor

        /// <summary>
        /// Initialize a new instance of the <see cref="IterationSetupDialogViewModel"/> class
        /// </summary>
        public IterationSetupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupDialogViewModel"/> class
        /// </summary>
        /// <param name="iterationSetup">
        /// The <see cref="IterationSetup"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="thingTransaction">
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
        public IterationSetupDialogViewModel(IterationSetup iterationSetup, IThingTransaction thingTransaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(iterationSetup, thingTransaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(x => x.SelectedSourceIterationSetupRow)
                .Subscribe(x =>
                {
                    this.SelectedSourceIterationSetup = (x == null) ? null : x.IterationSetup;
                    this.UpdateOkCanExecute();
                });
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the time of creation
        /// </summary>
        public DateTime? NullableCreatedOn
        {
            get { return (this.dialogKind == ThingDialogKind.Create) ? null : (DateTime?)this.CreatedOn; }
        }

        /// <summary>
        /// Gets the iteration number
        /// </summary>
        public int? NullableIterationNumber
        {
            get { return (this.dialogKind == ThingDialogKind.Create) ? null : (int?)this.IterationNumber; }
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModelSetup"/>'s name
        /// </summary>
        public string ModelName
        {
            get
            {
                var model = (EngineeringModelSetup)this.Container;
                return model.Name;
            }
        }

        /// <summary>
        /// Gets the possible <see cref="IterationSetupListBoxItem"/>
        /// </summary>
        public ReactiveList<IterationSetupListBoxItem> PossibleSourceIterationSetupRow { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="IterationSetupListBoxItem"/>
        /// </summary>
        public IterationSetupListBoxItem SelectedSourceIterationSetupRow
        {
            get { return this.selectedSourceIterationSetupRow; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSourceIterationSetupRow, value); }
        }
        #endregion

        #region Dialog Base Override

        /// <summary>
        /// Populate the possible source <see cref="IterationSetup"/>
        /// </summary>
        protected override void PopulatePossibleSourceIterationSetup()
        {
            this.PossibleSourceIterationSetupRow = new ReactiveList<IterationSetupListBoxItem>();
            var modelsetup = (EngineeringModelSetup)this.Container;
            this.PossibleSourceIterationSetupRow.AddRange(modelsetup.IterationSetup.OrderBy(x => x.IterationNumber).Where(x => !x.IsDeleted).Select(x => new IterationSetupListBoxItem(x)));
            this.SelectedSourceIterationSetupRow = (this.Thing.SourceIterationSetup == null) ? null : this.PossibleSourceIterationSetupRow.SingleOrDefault(x => x.IterationSetup == this.Thing.SourceIterationSetup);
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.SelectedSourceIterationSetupRow = this.PossibleSourceIterationSetupRow.LastOrDefault();
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.Thing.IterationIid = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> value for this dialog
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && (this.dialogKind != ThingDialogKind.Create || this.SelectedSourceIterationSetup != null);
        }
        #endregion
    }
}