// -------------------------------------------------------------------------------------------------
// <copyright file="SimpleParameterValueDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog view-model for the <see cref="SimpleParameterValue"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.SimpleParameterValue)]
    public class SimpleParameterValueDialogViewModel : CDP4CommonView.SimpleParameterValueDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedValue"/> property.
        /// </summary>
        private Dialogs.SimpleParameterValueRowViewModel selectedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterValueDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public SimpleParameterValueDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterValueDialogViewModel"/> class
        /// </summary>
        /// <param name="simpleParameterValue">
        /// The <see cref="SimpleParameterValue"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="IThingDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="IThingDialogViewModel"/> performs
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
        public SimpleParameterValueDialogViewModel(SimpleParameterValue simpleParameterValue, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null) 
            : base(simpleParameterValue, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedScale).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedParameterType).Subscribe(_ => this.PopulateValues());
            this.Values.CountChanged.Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Gets or sets the selected value row.
        /// </summary>
        public Dialogs.SimpleParameterValueRowViewModel SelectedValue
        {
            get { return this.selectedValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValue, value); }
        }

        /// <summary>
        /// Gets the <see cref="Dialogs.SimpleParameterValueRowViewModel"/>
        /// </summary>
        public DisposableReactiveList<Dialogs.SimpleParameterValueRowViewModel> Values { get; private set; }

        /// <summary>
        /// Initializes the dialogs
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Values = new DisposableReactiveList<Dialogs.SimpleParameterValueRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateValues();
            this.PopulatePossibleParameterType();
        }

        /// <summary>
        ///  Populates the <see cref="CDP4CommonView.SimpleParameterValueDialogViewModel.PossibleParameterType"/> property
        /// </summary>
        protected override void PopulatePossibleParameterType()
        {
            base.PopulatePossibleParameterType();
            if (this.Thing.ParameterType != null)
            {
                this.PossibleParameterType.Add(this.Thing.ParameterType);
                this.SelectedParameterType = this.PossibleParameterType.Single();
            }
            else
            {
                var model = this.ChainOfContainer.First().TopContainer as EngineeringModel;
                var containerRdl = model.EngineeringModelSetup.RequiredRdl.Single();
                if (containerRdl != null)
                {
                    var allTypes = new List<ParameterType>(containerRdl.ParameterType);
                    allTypes.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType));
                    this.PossibleParameterType.AddRange(allTypes.OrderBy(p => p.Name));
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.SimpleParameterValueDialogViewModel.PossibleScale"/> property
        /// </summary>
        protected override void PopulatePossibleScale()
        {
            base.PopulatePossibleScale();

            if (this.SelectedParameterType is QuantityKind)
            {
                foreach (var scale in ((QuantityKind)this.SelectedParameterType).AllPossibleScale)
                {
                    this.PossibleScale.Add(scale); 
                }

                this.SelectedScale = this.SelectedScale ?? this.PossibleScale.FirstOrDefault();
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            this.Thing.Value = new ValueArray<string>(this.Values.Select(x => x.Manual.ToValueSetString(x.ParameterType)));
            if (this.Thing.ParameterType is QuantityKind)
            {
                this.Thing.Scale = this.Values.Single().Scale;
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            this.OkCanExecute = this.Container != null && !this.ValidationErrors.Any() && this.Values.Count != 0;
        }

        /// <summary>
        /// Populates the values
        /// </summary>
        private void PopulateValues()
        {
            this.Values.ClearAndDispose();
            var cptPt = this.SelectedParameterType as CompoundParameterType;
            if (this.SelectedParameterType == null)
            {
                return;
            }
            if (cptPt == null)
            {
                this.Thing.ParameterType = this.SelectedParameterType;

                var row = new Dialogs.SimpleParameterValueRowViewModel(this.Thing, this.Session, this, 0, this.IsReadOnly);
                var quantityKind = this.SelectedParameterType as QuantityKind;
                row.ParameterType = this.SelectedParameterType;
                this.Values.Add(row);
                if (quantityKind == null)
                {
                    return;
                }
                row.PossibleScale.AddRange(quantityKind.AllPossibleScale);
            }
            else
            {
                for (var index = 0; index < cptPt.Component.Count; index++)
                {
                    var row = new Dialogs.SimpleParameterValueRowViewModel(this.Thing, this.Session, this, index, this.IsReadOnly);
                    var quantityKind = cptPt.Component[index].ParameterType as QuantityKind;
                    row.ParameterType = this.SelectedParameterType;
                    this.Values.Add(row);
                    if (quantityKind == null)
                    {
                        return;
                    }
                    row.PossibleScale.AddRange(quantityKind.AllPossibleScale);
                }
            }
        }
    }
}