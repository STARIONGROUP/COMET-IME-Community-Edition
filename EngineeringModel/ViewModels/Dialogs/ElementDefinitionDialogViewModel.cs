// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="Option"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ElementDefinition)]
    public class ElementDefinitionDialogViewModel : CDP4CommonView.ElementDefinitionDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="IsTopElement"/> property.
        /// </summary>
        private bool isTopElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ElementDefinitionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDialogViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> that is the subject of the current view-model. This is the object
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
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ElementDefinitionDialogViewModel(ElementDefinition elementDefinition, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(elementDefinition, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(x => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ElementDefinition"/> is the top <see cref="ElementDefinition"/>
        /// of the container <see cref="Iteration"/>
        /// </summary>
        public bool IsTopElement
        {
            get { return this.isTopElement; }
            set { this.RaiseAndSetIfChanged(ref this.isTopElement, value); }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PopulatePossibleCategories();
        }


        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            if (((Iteration)this.Container).TopElement != null)
            {
                this.IsTopElement = ((Iteration)this.Container).TopElement.Iid == this.Thing.Iid; 
            }
        }

        /// <summary>
        /// Populates the <see cref="DomainOfExpertise"/> that may be owner.
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();

            var engineeringModel = (EngineeringModel)this.Container.Container;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            var iteration = this.Container as Iteration;

            if (this.IsTopElement)
            {
                iteration.TopElement = this.Thing;
            }
            else
            {
                if (iteration.TopElement != null && iteration.TopElement.Iid == this.Thing.Iid)
                {
                    iteration.TopElement = null;
                }
            }
        }

        /// <summary>
        /// Populate the possible <see cref="Category"/> for this <see cref="ElementUsage"/>
        /// </summary>
        private void PopulatePossibleCategories()
        {
            this.PossibleCategory.Clear();
            var model = (EngineeringModel)this.Container.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedCategories = new List<Category>(mrdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));
            allowedCategories.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                        .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null;
        }
    }
}
