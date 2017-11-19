// -------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRebuildViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Composition.Navigation;
    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;

    using DevExpress.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The view-model to select how a workbook is to be rebuild, keeping changes, or not.
    /// </summary>
    public class WorkbookRebuildViewModel : ValueSetDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="RebuildKind"/> property.
        /// </summary>
        private RebuildKind rebuildKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookRebuildViewModel"/> class.
        /// </summary>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s that have been changed on the workbook.
        /// </param>
        /// <param name="valueSetKind">
        /// assertion that is used to determine the visibilty of the <see cref="ParameterOrOverrideBase"/> and <see cref="ParameterSubscription"/>
        /// </param>
        public WorkbookRebuildViewModel(IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets, ValueSetKind valueSetKind)
            : base(processedValueSets, valueSetKind)
        {
            this.DialogTitle = "Rebuild Workbook...";
            
            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.OkCommand = ReactiveCommand.Create();
            this.OkCommand.Subscribe(_ => this.ExecuteOk());
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle { get; private set; }

        /// <summary>
        /// Gets the Select <see cref="ICommand{T}"/>
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand{T}"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="RebuildKind"/>
        /// </summary>
        public RebuildKind RebuildKind
        {
            get { return this.rebuildKind; }
            set { this.RaiseAndSetIfChanged(ref this.rebuildKind, value); }
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOk()
        {
            this.DialogResult = new WorkbookRebuildDialogResult(true, this.RebuildKind);
        }
    }
}
