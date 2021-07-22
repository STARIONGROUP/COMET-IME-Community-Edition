// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Smiechowski Nathanael
//
//    This file is part of CDP4-IME Community Edition
//
//    The CDP4-IME Community Edition is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation, either version 3 of the
//    License, or(at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program; If not, see http://www.gnu.org/licenses/
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4Dashboard.ViewModels.Widget;
    using CDP4Dashboard.ViewModels.Widget.Base;
    using CDP4Dashboard.Views.Widget;

    using DevExpress.Xpf.LayoutControl;

    using ReactiveUI;

    /// <summary>
    /// The view-model  of the Team Composition Browser that shows all the persons that make
    /// up a team of an <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class DashboardBrowserViewModel : ModellingThingBrowserViewModelBase, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Dashboard";

        /// <summary>
        /// Used for lock() in <see cref="ResizeControlsInternal"/> so only 1 resize action is allowed at a time
        /// </summary>
        private object resizeLocker = new object();

        /// <summary>
        /// The default widget width
        /// </summary>
        private const double widgetWidth = 250;

        /// <summary>
        /// The defaul widget height
        /// </summary>
        private const double widgetHeight = 200;

        /// <summary>
        /// The default widget margin used for calculations
        /// </summary>
        private const double widgetMargin = 50;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// The list of Widgets
        /// </summary>
        private ReactiveList<UserControl> widgets;

        /// <summary>
        /// The ViewModel that belongs to the <see cref="DashboardBrowserViewModel.MaximizedElement"/>
        /// </summary>
        private IIterationTrackParameterViewModel currentMaximizedViewModel => this.MaximizedElement?.DataContext as IIterationTrackParameterViewModel;

        /// <summary>
        /// BackingField for ActualWidth
        /// </summary>
        private double actualWidth;

        /// <summary>
        /// BackingField for ActualWidth
        /// </summary>
        private double actualHeight;

        /// <summary>
        /// BackingField for MainOrientation
        /// </summary>
        private Orientation mainOrientation;

        /// <summary>
        /// BackingField for MaximizedElement
        /// </summary>
        private UserControl maximizedElement;

        /// <summary>
        /// BackingField for MaximizedElementPosition
        /// </summary>
        private MaximizedElementPosition maximizedElementPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> containing the given <see cref="EngineeringModel"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/>
        /// The <see cref="IPanelNavigationService"/> that allows to navigate to Panels
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/>
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public DashboardBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}";
            this.ToolTip = $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";
            this.Widgets = new ReactiveList<UserControl>();

            this.WhenAnyValue(x => x.ActualHeight, x => x.ActualWidth).Subscribe(_ => this.ResizeControls());
            this.WhenAnyValue(x => x.MainOrientation).Subscribe(this.OnOrientationChange);

            this.ResizeControls();
        }

        /// <summary>
        /// Sets properties based on the current Main <see cref="Orientation"/>
        /// </summary>
        private void OnOrientationChange(Orientation orientation)
        {
            this.MaximizedElementPosition = orientation == Orientation.Horizontal ? MaximizedElementPosition.Left : MaximizedElementPosition.Top;
        }

        /// <summary>
        /// Adds a widget to the UI
        /// </summary>
        /// <typeparam name="TThing">The <see cref="ParameterOrOverrideBase"/></typeparam>
        /// <typeparam name="TValueSet">The <see cref="ParameterValueSetBase"/></typeparam>
        /// <param name="iterationTrackParameter">The <see cref="IterationTrackParameter"/></param>
        private void AddWidget<TThing, TValueSet>(IterationTrackParameter iterationTrackParameter)
            where TThing : ParameterOrOverrideBase
            where TValueSet : ParameterValueSetBase
        {
            var iterationTrackParameterViewModel = new IterationTrackParameterViewModel<TThing, TValueSet>(this.Session, this.Thing, iterationTrackParameter);

            iterationTrackParameterViewModel.WhenAnyValue(x => x.ChartVisible)
                .Subscribe(x =>
                {
                    if (this.currentMaximizedViewModel == iterationTrackParameterViewModel && x == Visibility.Collapsed)
                    {
                        this.MinimizeCurrentMaximizedWidget();
                    }

                    if (this.currentMaximizedViewModel != iterationTrackParameterViewModel && x == Visibility.Visible)
                    {
                        this.MaximizeWidget(iterationTrackParameterViewModel);
                    }

                    this.ResizeControls();
                });

            this.Disposables.Add(iterationTrackParameterViewModel.OnDeleteCommand.Subscribe(x => this.RemoveWidget(iterationTrackParameterViewModel)));

            this.Widgets.Insert(Math.Max(this.Widgets.Count - 1, 0), this.GetIterationTrackWidget(iterationTrackParameterViewModel));
            this.ResizeControls();
        }

        /// <summary>
        /// Remove a widget from the UI
        /// </summary>
        /// <typeparam name="TThing">The <see cref="ParameterOrOverrideBase"/></typeparam>
        /// <typeparam name="TValueSet">The <see cref="ParameterValueSetBase"/></typeparam>
        /// <param name="iterationTrackParameterViewModel">The Widgets's ViewModel</param>
        private void RemoveWidget<TThing, TValueSet>(IterationTrackParameterViewModel<TThing, TValueSet> iterationTrackParameterViewModel) where TThing : ParameterOrOverrideBase where TValueSet : ParameterValueSetBase
        {
            var widget = this.Widgets.SingleOrDefault(x => x.DataContext == iterationTrackParameterViewModel);

            if (widget != null)
            {
                this.Widgets.Remove(widget);
            }
        }

        /// <summary>
        /// Create and return a new <see cref="IterationTrackParameterView"/>
        /// </summary>
        /// <param name="widgetViewModel">The <see cref="WidgetBase"/> where a View has to be create for.</param>
        /// <returns>A new <see cref="IterationTrackParameterView"/></returns>
        private IterationTrackParameterView GetIterationTrackWidget(WidgetBase widgetViewModel)
        {
            return new IterationTrackParameterView { DataContext = widgetViewModel };
        }

        /// <summary>
        /// Minimize the current Maximized Widget
        /// </summary>
        private void MinimizeCurrentMaximizedWidget()
        {
            if (this.currentMaximizedViewModel != null)
            {
                this.currentMaximizedViewModel.ChartVisible = Visibility.Collapsed;
                this.MaximizedElement = null;
            }
        }

        /// <summary>
        /// Maximized the Widget
        /// </summary>
        /// <typeparam name="TThing">The <see cref="ParameterOrOverrideBase"/></typeparam>
        /// <typeparam name="TValueSet">The <see cref="ParameterValueSetBase"/></typeparam>
        /// <param name="newMaximizedIterationTrackParameterViewModel"> The ViewModel of the widget to be maximized</param>
        private void MaximizeWidget<TThing, TValueSet>(IterationTrackParameterViewModel<TThing, TValueSet> newMaximizedIterationTrackParameterViewModel) where TThing : ParameterOrOverrideBase where TValueSet : ParameterValueSetBase
        {
            this.MinimizeCurrentMaximizedWidget();
            var tile = this.Widgets.SingleOrDefault(x => x.DataContext == newMaximizedIterationTrackParameterViewModel);

            if (tile != null)
            {
                this.MaximizedElement = tile;
            }
        }

        /// <summary>
        /// Check the sizing (UI) of the currently active Widgets
        /// </summary>
        private void ResizeControls()
        {
            if (this.currentMaximizedViewModel != null)
            {
                this.MainOrientation = this.ActualWidth > this.ActualHeight ? Orientation.Horizontal : Orientation.Vertical;
            }
            else
            {
                this.MainOrientation = Orientation.Vertical;
            }

            this.ResizeControlsInternal();
        }

        /// <summary>
        /// A second, deeper level of refreshing controls that can also be ran async
        /// </summary>
        private void ResizeControlsInternal()
        {
            lock (this.resizeLocker)
            {
                var totalHeight = this.ActualHeight;
                var totalWidth = this.ActualWidth;

                var dummyWidget = this.Widgets.SingleOrDefault(x => x is DummyParameterView);

                if (dummyWidget == null)
                {
                    dummyWidget = new DummyParameterView();
                    this.Widgets.Add(dummyWidget);
                }
                else
                {
                    this.Widgets.Move(this.Widgets.IndexOf(dummyWidget), this.Widgets.Count - 1);
                }

                foreach (var tile in this.Widgets)
                {
                    if (tile == this.MaximizedElement)
                    {
                        if (this.mainOrientation == Orientation.Horizontal)
                        {
                            tile.Height = Math.Max(totalHeight - widgetMargin, widgetHeight);
                            tile.Width = Math.Max(totalWidth - (this.Widgets.Count > 1 ? widgetWidth + widgetMargin : widgetMargin), widgetWidth);
                        }
                        else
                        {
                            tile.Height = Math.Max(totalHeight - (this.Widgets.Count > 1 ? widgetHeight + widgetMargin : widgetMargin), widgetHeight);
                            tile.Width = Math.Max(totalWidth - widgetMargin, widgetWidth);
                        }
                    }
                    else
                    {
                        tile.Height = widgetHeight;
                        tile.Width = widgetWidth;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently MaximizedElement. In other words: The Widget that shows the chart.
        /// </summary>
        public UserControl MaximizedElement
        {
            get => this.maximizedElement;
            private set => this.RaiseAndSetIfChanged(ref this.maximizedElement, value);
        }

        /// <summary>
        /// Gets or sets the position of the MaximizedElement in its container UI element
        /// </summary>
        public MaximizedElementPosition MaximizedElementPosition
        {
            get => this.maximizedElementPosition;
            private set => this.RaiseAndSetIfChanged(ref this.maximizedElementPosition, value);
        }

        /// <summary>
        /// Gets or sets the main <see cref="Orientation"/> for the Widgets
        /// </summary>
        public Orientation MainOrientation
        {
            get => this.mainOrientation;
            set => this.RaiseAndSetIfChanged(ref this.mainOrientation, value);
        }

        /// <summary>
        /// Gets or sets the ActualWidth of the Dashboard's UI'
        /// </summary>
        public double ActualWidth
        {
            get => this.actualWidth;
            set => this.RaiseAndSetIfChanged(ref this.actualWidth, value);
        }

        /// <summary>
        /// Gets or sets the ActualHeight of the Dashboard's UI'
        /// </summary>
        public double ActualHeight
        {
            get => this.actualHeight;
            set => this.RaiseAndSetIfChanged(ref this.actualHeight, value);
        }

        /// <summary>
        /// Gets or sets the list of Widgets
        /// </summary>
        public ReactiveList<UserControl> Widgets
        {
            get => this.widgets;
            private set => this.RaiseAndSetIfChanged(ref this.widgets, value);
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup => this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>();

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Dashboard", "", this.CreateCommand, MenuItemKind.Create));
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="annotation">Not implemented</param>
        protected override void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Actions to perform during DragOver
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> data</param>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Parameter parameter && this.FindIteration(parameter.Container) == this.Thing)
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else if (dropInfo.Payload is ParameterOverride parameterOverride && this.FindIteration(parameterOverride.Container) == this.Thing)
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Actions to perform on a Drop 
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> data</param>
        /// <returns>A <see cref="Task"/> to be executed async</returns>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.Payload is ParameterOrOverrideBase parameterOrOverrideBase))
            {
                return;
            }

            var iterartionTrackParameter = new IterationTrackParameter(parameterOrOverrideBase);

            var result = this.DialogNavigationService.NavigateModal(new IterationTrackParameterDetailViewModel(iterartionTrackParameter));

            if (result.Result == true)
            {
                if (dropInfo.Payload is Parameter parameter && this.FindIteration(parameter.Container) == this.Thing)
                {
                    this.AddWidget<Parameter, ParameterValueSet>(iterartionTrackParameter);
                }
                else if (dropInfo.Payload is ParameterOverride parameterOverride && this.FindIteration(parameterOverride.Container) == this.Thing)
                {
                    this.AddWidget<ParameterOverride, ParameterOverrideValueSet>(iterartionTrackParameter);
                }
            }
        }

        /// <summary>
        /// Find this <see cref="Thing"/>'s parent <see cref="Iteration"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> who's parent Iteration we seek</param>
        /// <returns>The <see cref="Iteration"/></returns>
        private Iteration FindIteration(Thing thing)
        {
            while (true)
            {
                if (thing != null)
                {
                    if (thing is Iteration iteration)
                    {
                        return iteration;
                    }

                    thing = thing.Container;
                    continue;
                }

                return null;
            }
        }
    }
}