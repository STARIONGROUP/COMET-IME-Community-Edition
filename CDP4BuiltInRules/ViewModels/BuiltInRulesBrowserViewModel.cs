// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesBrowserViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using NLog;

    using ReactiveUI;
    
    /// <summary>
    /// The view-model that allows the user to browse the available <see cref="BuiltInRule"/>s
    /// </summary>
    public class BuiltInRulesBrowserViewModel : ReactiveObject, IPanelViewModel, IDragSource
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IRuleVerificationService"/> that provides access to the available <see cref="BuiltInRule"/>s.
        /// </summary>
        private readonly IRuleVerificationService ruleVerificationService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/> that is used to navigate to generic dialogs
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Backing field for the <see cref="SelectedRule"/> property.
        /// </summary>
        private BuiltInRuleRowViewModel selectedRule;

        /// <summary>
        /// Backing field for the <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Gets the caption of the browser
        /// </summary>
        public string Caption => "Built-In Rules Browser";

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the tooltip
        /// </summary>
        public string ToolTip => "Display available Built-in Rules";

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty => false;

        /// <summary>
        /// Gets the Data-Source
        /// </summary>
        /// <remarks>
        /// The data source is not relevant in the case of the <see cref="BuiltInRulesBrowserViewModel"/>
        /// </remarks>
        public string DataSource => string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRulesBrowserViewModel"/> class.
        /// </summary>
        /// <param name="ruleVerificationService">
        /// The <see cref="IRuleVerificationService"/> that provides access to the available <see cref="BuiltInRule"/>s.
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/> that is used to navigate to generic dialogs
        /// </param>
        public BuiltInRulesBrowserViewModel(IRuleVerificationService ruleVerificationService, IDialogNavigationService dialogNavigationService)
        {
            this.Identifier = Guid.NewGuid();
            this.ruleVerificationService = ruleVerificationService;
            this.dialogNavigationService = dialogNavigationService;

            this.BuiltInRules = new List<BuiltInRuleRowViewModel>();
            this.PopulateBuiltInRuleRowViewModel();

            var canInspect = this.WhenAnyValue(x => x.SelectedRule).Select(x => x != null);
            this.InspectCommand = ReactiveCommandCreator.Create(this.ExecuteInspectCommand, canInspect);
        }

        /// <summary>
        /// Gets or sets the selected row
        /// </summary>
        public BuiltInRuleRowViewModel SelectedRule
        {
            get => this.selectedRule;
            set => this.RaiseAndSetIfChanged(ref this.selectedRule, value);
        }

        /// <summary>
        /// Gets or sets the Create Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectCommand { get; protected set; }

        /// <summary>
        /// Gets the list of <see cref="BuiltInRuleRowViewModel"/>.
        /// </summary>
        public List<BuiltInRuleRowViewModel> BuiltInRules { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.RightGroup;

        /// <summary>
        /// Gets or sets a value indicating if the <see cref="IPanelViewModel"/> is selected
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
        }

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public void StartDrag(IDragInfo dragInfo)
        {
            var dragSource = dragInfo.Payload as IDragSource;

            if (dragSource != null)
            {
                dragSource.StartDrag(dragInfo);
            }
        }

        /// <summary>
        /// populates the <see cref="BuiltInRules"/> with <see cref="BuiltInRuleRowViewModel"/>s.
        /// </summary>
        private void PopulateBuiltInRuleRowViewModel()
        {
            foreach (var lazyRule in this.ruleVerificationService.BuiltInRules)
            {
                var row = new BuiltInRuleRowViewModel(lazyRule.Value, lazyRule.Metadata);
                this.BuiltInRules.Add(row);
            }
        }

        /// <summary>
        /// Execute the <see cref="InspectCommand"/>
        /// </summary>
        private void ExecuteInspectCommand()
        {
            if (this.SelectedRule == null)
            {
                return;
            }

            var viewmodel = new BuiltInRuleDialogViewModel(this.selectedRule.Rule);
            this.dialogNavigationService.NavigateModal(viewmodel);
        }

        /// <summary>
        /// Dispose of this <see cref="IPanelViewModel"/>
        /// </summary>
        public void Dispose()
        {
            Logger.Trace("Disposing of BuiltInRulesBrowserViewModel");
        }
    }
}
