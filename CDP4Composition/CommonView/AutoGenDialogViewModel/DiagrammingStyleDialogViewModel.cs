// -------------------------------------------------------------------------------------------------
// <copyright file="DiagrammingStyleDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DiagrammingStyle"/>
    /// </summary>
    public abstract partial class DiagrammingStyleDialogViewModel<T> : DiagramThingBaseDialogViewModel<T> where T : DiagrammingStyle
    {
        /// <summary>
        /// Backing field for <see cref="FillOpacity"/>
        /// </summary>
        private float? fillOpacity;

        /// <summary>
        /// Backing field for <see cref="StrokeWidth"/>
        /// </summary>
        private float? strokeWidth;

        /// <summary>
        /// Backing field for <see cref="StrokeOpacity"/>
        /// </summary>
        private float? strokeOpacity;

        /// <summary>
        /// Backing field for <see cref="FontSize"/>
        /// </summary>
        private float? fontSize;

        /// <summary>
        /// Backing field for <see cref="FontName"/>
        /// </summary>
        private string fontName;

        /// <summary>
        /// Backing field for <see cref="FontItalic"/>
        /// </summary>
        private bool? fontItalic;

        /// <summary>
        /// Backing field for <see cref="FontBold"/>
        /// </summary>
        private bool? fontBold;

        /// <summary>
        /// Backing field for <see cref="FontUnderline"/>
        /// </summary>
        private bool? fontUnderline;

        /// <summary>
        /// Backing field for <see cref="FontStrokeThrough"/>
        /// </summary>
        private bool? fontStrokeThrough;

        /// <summary>
        /// Backing field for <see cref="SelectedFillColor"/>
        /// </summary>
        private Color selectedFillColor;

        /// <summary>
        /// Backing field for <see cref="SelectedStrokeColor"/>
        /// </summary>
        private Color selectedStrokeColor;

        /// <summary>
        /// Backing field for <see cref="SelectedFontColor"/>
        /// </summary>
        private Color selectedFontColor;

        /// <summary>
        /// Backing field for <see cref="SelectedUsedColor"/>
        /// </summary>
        private ColorRowViewModel selectedUsedColor;


        /// <summary>
        /// Initializes a new instance of the <see cref="DiagrammingStyleDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected DiagrammingStyleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagrammingStyleDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="diagrammingStyle">
        /// The <see cref="DiagrammingStyle"/> that is the subject of the current view-model. This is the object
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
        protected DiagrammingStyleDialogViewModel(T diagrammingStyle, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(diagrammingStyle, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the FillOpacity
        /// </summary>
        public virtual float? FillOpacity
        {
            get { return this.fillOpacity; }
            set { this.RaiseAndSetIfChanged(ref this.fillOpacity, value); }
        }

        /// <summary>
        /// Gets or sets the StrokeWidth
        /// </summary>
        public virtual float? StrokeWidth
        {
            get { return this.strokeWidth; }
            set { this.RaiseAndSetIfChanged(ref this.strokeWidth, value); }
        }

        /// <summary>
        /// Gets or sets the StrokeOpacity
        /// </summary>
        public virtual float? StrokeOpacity
        {
            get { return this.strokeOpacity; }
            set { this.RaiseAndSetIfChanged(ref this.strokeOpacity, value); }
        }

        /// <summary>
        /// Gets or sets the FontSize
        /// </summary>
        public virtual float? FontSize
        {
            get { return this.fontSize; }
            set { this.RaiseAndSetIfChanged(ref this.fontSize, value); }
        }

        /// <summary>
        /// Gets or sets the FontName
        /// </summary>
        public virtual string FontName
        {
            get { return this.fontName; }
            set { this.RaiseAndSetIfChanged(ref this.fontName, value); }
        }

        /// <summary>
        /// Gets or sets the FontItalic
        /// </summary>
        public virtual bool? FontItalic
        {
            get { return this.fontItalic; }
            set { this.RaiseAndSetIfChanged(ref this.fontItalic, value); }
        }

        /// <summary>
        /// Gets or sets the FontBold
        /// </summary>
        public virtual bool? FontBold
        {
            get { return this.fontBold; }
            set { this.RaiseAndSetIfChanged(ref this.fontBold, value); }
        }

        /// <summary>
        /// Gets or sets the FontUnderline
        /// </summary>
        public virtual bool? FontUnderline
        {
            get { return this.fontUnderline; }
            set { this.RaiseAndSetIfChanged(ref this.fontUnderline, value); }
        }

        /// <summary>
        /// Gets or sets the FontStrokeThrough
        /// </summary>
        public virtual bool? FontStrokeThrough
        {
            get { return this.fontStrokeThrough; }
            set { this.RaiseAndSetIfChanged(ref this.fontStrokeThrough, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedFillColor
        /// </summary>
        public virtual Color SelectedFillColor
        {
            get { return this.selectedFillColor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFillColor, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Color"/>s for <see cref="SelectedFillColor"/>
        /// </summary>
        public ReactiveList<Color> PossibleFillColor { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedStrokeColor
        /// </summary>
        public virtual Color SelectedStrokeColor
        {
            get { return this.selectedStrokeColor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedStrokeColor, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Color"/>s for <see cref="SelectedStrokeColor"/>
        /// </summary>
        public ReactiveList<Color> PossibleStrokeColor { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedFontColor
        /// </summary>
        public virtual Color SelectedFontColor
        {
            get { return this.selectedFontColor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFontColor, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Color"/>s for <see cref="SelectedFontColor"/>
        /// </summary>
        public ReactiveList<Color> PossibleFontColor { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ColorRowViewModel"/>
        /// </summary>
        public ColorRowViewModel SelectedUsedColor
        {
            get { return this.selectedUsedColor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUsedColor, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Color"/>
        /// </summary>
        public ReactiveList<ColorRowViewModel> UsedColor { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedFillColor"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedFillColorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedStrokeColor"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedStrokeColorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedFontColor"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedFontColorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Color
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateUsedColorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Color
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteUsedColorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Color
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditUsedColorCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Color
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectUsedColorCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateUsedColorCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedUsedColorCommand = this.WhenAny(vm => vm.SelectedUsedColor, v => v.Value != null);
            var canExecuteEditSelectedUsedColorCommand = this.WhenAny(vm => vm.SelectedUsedColor, v => v.Value != null && !this.IsReadOnly);

            this.CreateUsedColorCommand = ReactiveCommandCreator.Create(canExecuteCreateUsedColorCommand);
            this.CreateUsedColorCommand.Subscribe(_ => this.ExecuteCreateCommand<Color>(this.PopulateUsedColor));

            this.DeleteUsedColorCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUsedColorCommand);
            this.DeleteUsedColorCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedUsedColor.Thing, this.PopulateUsedColor));

            this.EditUsedColorCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedUsedColorCommand);
            this.EditUsedColorCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedUsedColor.Thing, this.PopulateUsedColor));

            this.InspectUsedColorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedUsedColorCommand);
            this.InspectUsedColorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUsedColor.Thing));
            var canExecuteInspectSelectedFillColorCommand = this.WhenAny(vm => vm.SelectedFillColor, v => v.Value != null);
            this.InspectSelectedFillColorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedFillColorCommand);
            this.InspectSelectedFillColorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFillColor));
            var canExecuteInspectSelectedStrokeColorCommand = this.WhenAny(vm => vm.SelectedStrokeColor, v => v.Value != null);
            this.InspectSelectedStrokeColorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedStrokeColorCommand);
            this.InspectSelectedStrokeColorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStrokeColor));
            var canExecuteInspectSelectedFontColorCommand = this.WhenAny(vm => vm.SelectedFontColor, v => v.Value != null);
            this.InspectSelectedFontColorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedFontColorCommand);
            this.InspectSelectedFontColorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFontColor));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.FillOpacity = this.FillOpacity;
            clone.StrokeWidth = this.StrokeWidth;
            clone.StrokeOpacity = this.StrokeOpacity;
            clone.FontSize = this.FontSize;
            clone.FontName = this.FontName;
            clone.FontItalic = this.FontItalic;
            clone.FontBold = this.FontBold;
            clone.FontUnderline = this.FontUnderline;
            clone.FontStrokeThrough = this.FontStrokeThrough;
            clone.FillColor = this.SelectedFillColor;
            clone.StrokeColor = this.SelectedStrokeColor;
            clone.FontColor = this.SelectedFontColor;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleFillColor = new ReactiveList<Color>();
            this.PossibleStrokeColor = new ReactiveList<Color>();
            this.PossibleFontColor = new ReactiveList<Color>();
            this.UsedColor = new ReactiveList<ColorRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.FillOpacity = this.Thing.FillOpacity;
            this.StrokeWidth = this.Thing.StrokeWidth;
            this.StrokeOpacity = this.Thing.StrokeOpacity;
            this.FontSize = this.Thing.FontSize;
            this.FontName = this.Thing.FontName;
            this.FontItalic = this.Thing.FontItalic;
            this.FontBold = this.Thing.FontBold;
            this.FontUnderline = this.Thing.FontUnderline;
            this.FontStrokeThrough = this.Thing.FontStrokeThrough;
            this.SelectedFillColor = this.Thing.FillColor;
            this.PopulatePossibleFillColor();
            this.SelectedStrokeColor = this.Thing.StrokeColor;
            this.PopulatePossibleStrokeColor();
            this.SelectedFontColor = this.Thing.FontColor;
            this.PopulatePossibleFontColor();
            this.PopulateUsedColor();
        }

        /// <summary>
        /// Populates the <see cref="UsedColor"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateUsedColor()
        {
            this.UsedColor.Clear();
            foreach (var thing in this.Thing.UsedColor.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ColorRowViewModel(thing, this.Session, this);
                this.UsedColor.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleFillColor"/> property
        /// </summary>
        protected virtual void PopulatePossibleFillColor()
        {
            this.PossibleFillColor.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleStrokeColor"/> property
        /// </summary>
        protected virtual void PopulatePossibleStrokeColor()
        {
            this.PossibleStrokeColor.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleFontColor"/> property
        /// </summary>
        protected virtual void PopulatePossibleFontColor()
        {
            this.PossibleFontColor.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach(var usedColor in this.UsedColor)
            {
                usedColor.Dispose();
            }
        }
    }
}
