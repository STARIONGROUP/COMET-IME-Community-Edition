// -------------------------------------------------------------------------------------------------
// <copyright file="DecompositionRuleDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;

    using DevExpress.XtraPrinting.Native;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DecompositionRuleDialogViewModel"/> is to allow an <see cref="DecompositionRule"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="BinaryRelationshipRule"/> will result in an <see cref="BinaryRelationshipRule"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.DecompositionRule)]
    public class DecompositionRuleDialogViewModel : CDP4CommonView.DecompositionRuleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Back value for <see cref="HasLibrary"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasLibrary;

        /// <summary>
        /// Back value for <see cref="MaxContainedString"/>
        /// </summary>
        private string maxContainedString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompositionRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DecompositionRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompositionRuleDialogViewModel"/> class.
        /// </summary>
        /// <param name="decompositionRule">
        /// The <see cref="DecompositionRule"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="MultiRelationshipRuleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="MultiRelationshipRuleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public DecompositionRuleDialogViewModel(DecompositionRule decompositionRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(decompositionRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the Rdl has been selected
        /// </summary>
        public bool HasLibrary
        {
            get { return this.hasLibrary.Value; }
        }

        /// <summary>
        /// Gets or sets the MaxContainedString
        /// </summary>
        public virtual string MaxContainedString
        {
            get { return this.maxContainedString; }
            set { this.RaiseAndSetIfChanged(ref this.maxContainedString, value); }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field
        /// </remarks>
        public override string this[string columnName]
        {
            get
            {
                if (columnName == "MaxContainedString")
                {
                    if (this.MaxContainedString != null && !this.MaxContainedString.IsEmpty())
                    {
                        int result;
                        if (int.TryParse(this.MaxContainedString, out result))
                        {
                            if (result < this.MinContained)
                            {
                                return "The number should be higher than the minimum value.";
                            }
                        }
                        else
                        {
                            return "Value can not be converted to integer number";
                        }
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes the commands and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.Container)
                .Select(x => x != null)
                .ToProperty(this, x => x.HasLibrary, out this.hasLibrary);

            this.WhenAnyValue(x => x.Container).Subscribe(_ =>
            {
                this.PopulateContainedCategory();
                this.PopulatePossibleContainingCategory();
                this.UpdateOkCanExecute();
            });
            this.WhenAnyValue(x => x.ContainedCategory).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.SelectedContainingCategory).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.MaxContainedString).Subscribe(_ => { this.UpdateMaxContained(); this.UpdateOkCanExecute(); });
        }

        /// <summary>
        /// Populate the <see cref="CDP4CommonView.DecompositionRuleDialogViewModel.PossibleContainingCategory"/> property
        /// </summary>
        protected override void PopulatePossibleContainingCategory()
        {
            base.PopulatePossibleContainingCategory();
            if (this.Container == null)
            {
                return;
            }

            var rdlContainer = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdlContainer.GetRequiredRdls()) { rdlContainer };
            var categories =
                rdls.SelectMany(x => x.DefinedCategory)
                    .Where(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition))
                    .OrderBy(x => x.Name);

            this.PossibleContainingCategory.AddRange(categories);
        }

        /// <summary>
        /// Populate the <see cref="CDP4CommonView.DecompositionRuleDialogViewModel.PossibleContainedCategory"/> property
        /// </summary>
        protected override void PopulateContainedCategory()
        {
            this.PossibleContainedCategory.Clear();
            this.ContainedCategory.Clear();
            if (this.Container == null)
            {
                return;
            }

            var rdlContainer = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdlContainer.GetRequiredRdls()) { rdlContainer };
            var categories =
                rdls.SelectMany(x => x.DefinedCategory)
                    .Where(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition))
                    .OrderBy(x => x.Name);

            this.PossibleContainedCategory.AddRange(categories);
            base.PopulateContainedCategory();
        }

        /// <summary>
        /// Updates the <see cref="CDP4CommonView.DecompositionRuleDialogViewModel.MaxContained"/> property
        /// </summary>
        private void UpdateMaxContained()
        {
            int result;
            if (this.MaxContainedString == null || this.MaxContainedString.IsEmpty())
            {
                this.MaxContained = null;
            }
            else if (int.TryParse(this.MaxContainedString, out result) || result > 0)
            {
                this.MaxContained = result;
            }
        }

        /// <summary>
        /// Update the <see cref="DialogViewModelBase{T}.OkCanExecute"/> value
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedContainingCategory != null &&
                                this.ContainedCategory.Count > 0 && this["MaxContainedString"].IsEmpty();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            if (this.Thing.MaxContained != null)
            {
                this.MaxContainedString = this.Thing.MaxContained.ToString();
            }
        }
    }
}