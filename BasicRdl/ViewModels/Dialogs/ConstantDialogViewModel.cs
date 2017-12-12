// -------------------------------------------------------------------------------------------------
// <copyright file="ConstantDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ConstantDialogViewModel"/> is to allow a <see cref="Constant"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Constant"/> will result in an <see cref="Constant"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Constant)]
    public class ConstantDialogViewModel : CDP4CommonView.ConstantDialogViewModel, IThingDialogViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ConstantDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDialogViewModel"/> class.
        /// </summary>
        /// <param name="constant">
        /// The <see cref="Constant"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CategoryDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CategoryDialogViewModel"/> performs
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
        public ConstantDialogViewModel(Constant constant, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(constant, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedScale).Subscribe(_ => this.UpdateOkCanExecute());
            this.Value.ChangeTrackingEnabled = true;
            this.Value.ItemChanged.Subscribe(_ => this.UpdateOkCanExecute());

            this.WhenAnyValue(vm => vm.Container).Skip(1).Subscribe(_ => this.PopulatePossibleParameterType());
            this.WhenAnyValue(vm => vm.SelectedParameterType).Subscribe(_ => 
            { 
                this.PopulatePossibleScale(); 
                this.PopulateValue(); 
                this.UpdateOkCanExecute();
            });
        }

        #endregion

        #region Properties & Commands

        /// <summary>
        /// Gets a value indicating whether the ParameterType property is ReadOnly.
        /// </summary>
        public bool IsParameterTypeReadOnly
        {
            get
            {
                return this.IsReadOnly || this.dialogKind == ThingDialogKind.Update;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PopulatePossibleCategory();
        }

        /// <summary>
        ///  Populates the <see cref="CDP4CommonView.SimpleParameterValueDialogViewModel.PossibleParameterType"/> property
        /// </summary>
        protected override void PopulatePossibleParameterType()
        {
            base.PopulatePossibleParameterType();
            var rdl = (ReferenceDataLibrary)this.Container;
            if (rdl == null)
            {
                return;
            }

            var possiblePt = rdl.ParameterType.ToList();
            possiblePt.AddRange(rdl.GetRequiredRdls().SelectMany(x => x.ParameterType));

            this.PossibleParameterType.AddRange(possiblePt.OrderBy(p => p.ShortName).ThenBy(p => p.Name));
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.SimpleParameterValueDialogViewModel.PossibleScale"/> property
        /// </summary>
        protected override void PopulatePossibleScale()
        {
            base.PopulatePossibleScale();

            var quantityKind = this.SelectedParameterType as QuantityKind;
            if (quantityKind == null)
            {
                return;
            }

            foreach (var scale in quantityKind.AllPossibleScale)
            {
                this.PossibleScale.Add(scale);
            }

            if (this.SelectedScale == null)
            {
                this.SelectedScale = this.PossibleScale.FirstOrDefault();
            }
        }

        /// <summary>
        /// populate the possible <see cref="Category"/>
        /// </summary>
        private void PopulatePossibleCategory()
        {
            this.PossibleCategory.Clear();
            var container = this.Container as ReferenceDataLibrary;
            if (container == null)
            {
                return;
            }

            var allowedCategories = new List<Category>(container.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));
            allowedCategories.AddRange(container.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                        .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Repopulates the <see cref="Value"/> property according to the SelectedParameterType
        /// </summary>
        protected override void PopulateValue()
        {
            base.PopulateValue();

            if (this.Value.Any() || this.SelectedParameterType == null)
            {
                return;
            }

            for (var i = 0; i < this.SelectedParameterType.NumberOfValues; i++)
            {
                this.Value.Add(new PrimitiveRow<string> { Index = i, Value = string.Empty });
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedParameterType != null && this.Value.Any() && !this.Value.Any(x => string.IsNullOrEmpty(x.Value));
        }

        #endregion
    }
}
