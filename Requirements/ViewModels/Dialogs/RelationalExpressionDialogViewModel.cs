// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog view-model for the <see cref="RelationalExpression"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.RelationalExpression)]
    public class RelationalExpressionDialogViewModel : CDP4CommonView.RelationalExpressionDialogViewModel, IThingDialogViewModel
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RelationalExpressionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionDialogViewModel"/> class
        /// </summary>
        /// <param name="relationalExpression">
        /// The <see cref="RelationalExpression"/> that is the subject of the current view-model. This is the object
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
        public RelationalExpressionDialogViewModel(RelationalExpression relationalExpression, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null) 
            : base(relationalExpression, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedScale).Subscribe(_ => this.UpdateOkCanExecute());
            this.Value.ChangeTrackingEnabled = true;
            this.Value.ItemChanged.Subscribe(_ => this.UpdateOkCanExecute());
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
            get { return this.IsReadOnly || this.dialogKind == ThingDialogKind.Update;  }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Populates the <see cref="PossibleParameterType"/> property
        /// </summary>
        protected override void PopulatePossibleParameterType()
        {
            base.PopulatePossibleParameterType();
            var model = this.ChainOfContainer.First().TopContainer as EngineeringModel;
            if (model == null)
            {
                return;
            }

            var modelRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var parametertypes = modelRdl.ParameterType.ToList();
            if (modelRdl.RequiredRdl != null)
            {
                parametertypes.AddRange(modelRdl.RequiredRdl.ParameterType.Except(parametertypes));
            }

            this.PossibleParameterType.AddRange(parametertypes.OrderBy(p => p.ShortName));

            if (this.SelectedParameterType == null)
            {
                this.SelectedParameterType = this.PossibleParameterType.FirstOrDefault();
            }
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
        /// Repopulates the <see cref="Value"/> property according to the SelectedParameterType
        /// </summary>
        protected override void PopulateValue()
        {
            if (this.SelectedParameterType == null)
            {
                base.PopulateValue();
                return;
            }

            this.Value.Clear();
            for (var i = 0; i < this.SelectedParameterType.NumberOfValues; i++)
            {
                var value = (this.Thing.Value.Count() > i) ? this.Thing.Value[i] : string.Empty;
                this.Value.Add(new RelationalExpressionValueRowViewModel(this.SelectedParameterType) { Index = i, Value = value });
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedParameterType != null && !this.Value.Any(x => string.IsNullOrEmpty(x.Value));
        }

        #endregion
    }
}
