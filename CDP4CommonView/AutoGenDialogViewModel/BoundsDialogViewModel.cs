// -------------------------------------------------------------------------------------------------
// <copyright file="BoundsDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.DiagramData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Bounds"/>
    /// </summary>
    public partial class BoundsDialogViewModel : DiagramThingBaseDialogViewModel<Bounds>
    {
        /// <summary>
        /// Backing field for <see cref="X"/>
        /// </summary>
        private float x;

        /// <summary>
        /// Backing field for <see cref="Y"/>
        /// </summary>
        private float y;

        /// <summary>
        /// Backing field for <see cref="Width"/>
        /// </summary>
        private float width;

        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private float height;


        /// <summary>
        /// Initializes a new instance of the <see cref="BoundsDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BoundsDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundsDialogViewModel"/> class
        /// </summary>
        /// <param name="bounds">
        /// The <see cref="Bounds"/> that is the subject of the current view-model. This is the object
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
        public BoundsDialogViewModel(Bounds bounds, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(bounds, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as DiagramElementContainer;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type DiagramElementContainer",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the X
        /// </summary>
        public virtual float X
        {
            get { return this.x; }
            set { this.RaiseAndSetIfChanged(ref this.x, value); }
        }

        /// <summary>
        /// Gets or sets the Y
        /// </summary>
        public virtual float Y
        {
            get { return this.y; }
            set { this.RaiseAndSetIfChanged(ref this.y, value); }
        }

        /// <summary>
        /// Gets or sets the Width
        /// </summary>
        public virtual float Width
        {
            get { return this.width; }
            set { this.RaiseAndSetIfChanged(ref this.width, value); }
        }

        /// <summary>
        /// Gets or sets the Height
        /// </summary>
        public virtual float Height
        {
            get { return this.height; }
            set { this.RaiseAndSetIfChanged(ref this.height, value); }
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.X = this.X;
            clone.Y = this.Y;
            clone.Width = this.Width;
            clone.Height = this.Height;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.X = this.Thing.X;
            this.Y = this.Thing.Y;
            this.Width = this.Thing.Width;
            this.Height = this.Thing.Height;
        }
    }
}
