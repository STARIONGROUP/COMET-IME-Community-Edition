// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindFactorDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="QuantityKindFactorDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="QuantityKindFactor"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.QuantityKindFactor)]
    public class QuantityKindFactorDialogViewModel : CDP4CommonView.QuantityKindFactorDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The selected <see cref="ReferenceDataLibrary"/>
        /// </summary>
        private ReferenceDataLibrary selectedRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindFactorDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public QuantityKindFactorDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindFactorDialogViewModel"/> class.
        /// </summary>
        /// <param name="quantityKindFactor">
        /// The <see cref="QuantityKindFactor"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="QuantityKindFactorDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="QuantityKindFactorDialogViewModel"/> performs
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
        public QuantityKindFactorDialogViewModel(QuantityKindFactor quantityKindFactor, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(quantityKindFactor, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.selectedRdl = this.ChainOfContainer.OfType<ReferenceDataLibrary>().SingleOrDefault();
            if (this.selectedRdl == null)
            {
                throw new InvalidOperationException("There is no Reference Data Library in the chain of selected container.");
            }

            this.WhenAnyValue(vm => vm.SelectedQuantityKind).Where(x => x != null).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Updates the <see cref="DialogViewModelBase{T}.OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedQuantityKind != null;
        }

        /// <summary>
        /// Populates the  PossibleQuantityKind property
        /// </summary>
        protected override void PopulatePossibleQuantityKind()
        {
            base.PopulatePossibleQuantityKind();
            var quantityKinds = new List<QuantityKind>(this.selectedRdl.ParameterType.OfType<QuantityKind>());
            quantityKinds.AddRange(this.selectedRdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType.OfType<QuantityKind>()));
            this.PossibleQuantityKind.AddRange(quantityKinds.OrderBy(c => c.Name));
        }
    }
}
