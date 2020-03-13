// -----------------------------------------------------------------------
// <copyright file="IterationTrackParameterViewModel.cs" company="RHEA">
// Copyright (c) 2020 RHEA Group. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Utilities;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;

    using CDP4Dashboard.ViewModels.Charts;
    using CDP4Dashboard.ViewModels.Widget.Base;

    using CDP4RequirementsVerification;
    using CDP4RequirementsVerification.Verifiers;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The iteration track parameter view model.
    /// </summary>
    public class IterationTrackParameterViewModel<TThing, TValueSet> : WidgetBase, IIterationTrackParameterViewModel
        where TThing : ParameterOrOverrideBase
        where TValueSet : ParameterValueSetBase

    {
        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession Session;

        /// <summary>
        /// The <see cref="IterationTrackParameter"/> instance that is used for this widget
        /// </summary>
        private IterationTrackParameter iterationTrackParameter;

        /// <summary>
        /// The Logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The block visibility.
        /// </summary>
        private Visibility blockVisible;

        /// <summary>
        /// The chart visibility.
        /// </summary>
        private Visibility chartVisible;

        /// <summary>
        /// The current value.
        /// </summary>
        private string currentValue;

        /// <summary>
        /// The model code.
        /// </summary>
        private string modelCode;

        /// <summary>
        /// The percentage change.
        /// </summary>
        private string percentageChange;

        /// <summary>
        /// The previous value.
        /// </summary>
        private string previousValue;

        /// <summary>
        /// The status indicator.
        /// </summary>
        private string statusIndicator;

        /// <summary>
        /// The status indicator color.
        /// </summary>
        private Brush statusIndicatorColor;

        /// <summary>
        /// The tool tip of the widget
        /// </summary>
        private string tooltip;

        /// <summary>
        /// The collection of <see cref="LineSeries"/>
        /// </summary>
        private ReactiveList<LineSeries> lineSeriesCollection;

        /// <summary>
        /// The current <see cref="Iteration"/>
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The cache that stores the listeners that allow this row to react on update on the associated <see cref="IValueSet"/>
        /// </summary>
        private readonly Dictionary<IValueSet, IDisposable> valueSetListeners = new Dictionary<IValueSet, IDisposable>();

        /// <summary>
        /// Gets the Delete command
        /// </summary>
        public ReactiveCommand<object> OnDeleteCommand { get; }

        /// <summary>
        /// Gets the Show chart command
        /// </summary>
        public ReactiveCommand<object> OnToggleChartVisibilityCommand { get; }

        /// <summary>
        /// Gets the Refresh data command
        /// </summary>
        public ReactiveCommand<object> OnRefreshCommand { get; }

        /// <summary>
        /// Gets the Copy data command
        /// </summary>
        public ReactiveCommand<object> OnCopyDataCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationTrackParameterViewModel{TThing, TValueSet}"/> class.
        /// The constructor
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> </param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="iterationTrackParameter">The <see cref="IterationTrackParameter"/> instance used for this widget</param>
        public IterationTrackParameterViewModel(ISession session, Iteration iteration, IterationTrackParameter iterationTrackParameter)
        {
            this.lineSeriesCollection = new ReactiveList<LineSeries>();
            this.OnDeleteCommand = ReactiveCommand.Create();

            this.OnToggleChartVisibilityCommand = ReactiveCommand.Create();
            this.OnToggleChartVisibilityCommand.Subscribe(_ => this.ToggleChartVisibility());

            this.OnRefreshCommand = ReactiveCommand.Create();
            this.OnRefreshCommand.Subscribe(_ => this.RefreshData());

            this.OnCopyDataCommand = ReactiveCommand.Create();
            this.OnCopyDataCommand.Subscribe(_ => this.CopyTextToClipboard());

            this.Session = session;
            this.iteration = iteration;
            this.iterationTrackParameter = iterationTrackParameter;

            this.BlockVisible = Visibility.Visible;
            this.ChartVisible = Visibility.Collapsed;

            this.ModelCode = this.iterationTrackParameter.ModelCode;
            this.Title = this.iterationTrackParameter.ControlTitle;
            this.Unit = this.iterationTrackParameter.UnitSymbol;
            this.AxisXTitle = "Revisions";
            this.AxisYTitle = this.Unit;

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(iterationTrackParameter)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.RefreshData()));

            this.OnRefreshCommand.Execute(null);
        }

        /// <summary>
        /// Gets or sets the list of <see cref="LineSeries"/> objects
        /// </summary>
        public ReactiveList<LineSeries> LineSeriesCollection
        {
            get => this.lineSeriesCollection;
            set => this.RaiseAndSetIfChanged(ref this.lineSeriesCollection, value);
        }

        /// <summary>
        /// Is block visible
        /// </summary>
        public Visibility BlockVisible
        {
            get => this.blockVisible;
            set => this.RaiseAndSetIfChanged(ref this.blockVisible, value);
        }

        /// <summary>
        /// is chart visible
        /// </summary>
        public Visibility ChartVisible
        {
            get => this.chartVisible;
            set => this.RaiseAndSetIfChanged(ref this.chartVisible, value);
        }

        /// <summary>
        /// Current value
        /// </summary>
        public string CurrentValue
        {
            get => this.currentValue;
            set => this.RaiseAndSetIfChanged(ref this.currentValue, value);
        }

        /// <summary>
        /// The trackparameter object
        /// </summary>
        public IterationTrackParameter IterationTrackParameter
        {
            get => this.iterationTrackParameter;
            set => this.RaiseAndSetIfChanged(ref this.iterationTrackParameter, value);
        }

        /// <summary>
        /// The model code
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Percentage change
        /// </summary>
        public string PercentageChange
        {
            get => this.percentageChange;
            set => this.RaiseAndSetIfChanged(ref this.percentageChange, value);
        }

        /// <summary>
        /// Previous value
        /// </summary>
        public string PreviousValue
        {
            get => this.previousValue;
            set => this.RaiseAndSetIfChanged(ref this.previousValue, value);
        }

        /// <summary>
        /// The status indicator
        /// </summary>
        public string StatusIndicator
        {
            get => this.statusIndicator;
            set => this.RaiseAndSetIfChanged(ref this.statusIndicator, value);
        }

        /// <summary>
        /// The color of the text indicating whether the value went up or down
        /// </summary>
        public Brush StatusIndicatorColor
        {
            get => this.statusIndicatorColor;
            set => this.RaiseAndSetIfChanged(ref this.statusIndicatorColor, value);
        }

        /// <summary>
        /// Gets or sets the tooltip
        /// </summary>
        public string ToolTip
        {
            get => this.tooltip;
            set => this.RaiseAndSetIfChanged(ref this.tooltip, value);
        }

        /// <summary>
        /// Update the listeners for the <see cref="ParameterValueSetBase"/> of this widget
        /// </summary>
        private void UpdateValueSetListeners()
        {
            var currentValueSet = this.valueSetListeners.Keys.OfType<ParameterValueSetBase>().ToList();

            var updatedValueSet = new List<IValueSet>();

            var valueset = this.iterationTrackParameter.ParameterOrOverride.ValueSets;
            updatedValueSet.AddRange(valueset);

            var addedValueSet = updatedValueSet.Except(currentValueSet);
            var removedValueSet = currentValueSet.Except(updatedValueSet);

            foreach (var parameterValueSet in addedValueSet)
            {
                var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterValueSet)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.iterationTrackParameter.ParameterOrOverride.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshData());

                this.valueSetListeners.Add(parameterValueSet, listener);
            }

            foreach (var parameterValueSet in removedValueSet)
            {
                if (this.valueSetListeners.TryGetValue(parameterValueSet, out var listener))
                {
                    listener.Dispose();
                }

                this.valueSetListeners.Remove(parameterValueSet);
            }
        }

        /// <summary>
        /// Verify the <see cref="IterationTrackParameter"/>'s Requirements'
        /// </summary>
        private async void VerifyRequirements()
        {
            var binaryRelationshipsToVerify =
                this.iterationTrackParameter.ParameterOrOverride.QueryRelationships
                    .OfType<BinaryRelationship>()
                    .Where(x => x.Target is RelationalExpression)
                    .ToList();

            if (binaryRelationshipsToVerify.Any())
            {
                var complianceStates = new List<RequirementStateOfCompliance>();

                foreach (var binaryRelationship in binaryRelationshipsToVerify)
                {
                    complianceStates.Add(await new RelationalExpressionVerifier(binaryRelationship.Target as RelationalExpression)
                        .VerifyRequirementStateOfCompliance(new List<BinaryRelationship> { binaryRelationship }, this.iteration));
                }

                if (complianceStates.All(x => x == RequirementStateOfCompliance.Pass))
                {
                    this.StatusIndicatorColor = RequirementStateOfCompliance.Pass.GetBrush();
                }
                else if (complianceStates.All(x => x == RequirementStateOfCompliance.Failed))
                {
                    this.StatusIndicatorColor = RequirementStateOfCompliance.Failed.GetBrush();
                }
                else
                {
                    this.StatusIndicatorColor = RequirementStateOfCompliance.Inconclusive.GetBrush();
                }
            }
        }

        /// <summary>
        /// Asynchronously refreshes the Widget's data 
        /// </summary>
        private async void RefreshData()
        {
            this.StatusIndicatorColor = CDP4Color.Inconclusive.GetBrush();
            this.CurrentValue = "Calculating...";

            //Read the revisions for this tracked ParameterOrOverride. Results are in the cache afterwards
            await this.Session.Read(this.IterationTrackParameter.ParameterOrOverride,
                new DalQueryAttributes
                {
                    FromRevisionTimestamp = DateTime.MinValue,
                    ToRevisionTimestamp = DateTime.Now
                });

            this.ModelCode =
                this.Session.Assembler.Cache
                    .Where(x => new[] { ClassKind.Parameter, ClassKind.ParameterOverride }.Contains(x.Value.Value.ClassKind)
                                && x.Value.Value.Iid.Equals(this.iterationTrackParameter.ParameterOrOverride.Iid))
                    .Select(x => x.Value.Value)
                    .FirstOrDefault()?.UserFriendlyName;

            //Get the currently active Parameter(Override)ValueSets from the cache
            var parameterValueSets =
                this.Session.Assembler.Cache
                    .Where(x => new[] { ClassKind.ParameterValueSet, ClassKind.ParameterOverrideValueSet }.Contains(x.Value.Value.ClassKind)
                                && x.Value.Value.Container.Iid.Equals(this.iterationTrackParameter.ParameterOrOverride.Iid))
                    .Select(x => x.Value.Value)
                    .Cast<TValueSet>()
                    .ToList();

            // Read the Revisions for all needed Parameter(Override)ValueSets from the DataSource
            await this.Session.Read(parameterValueSets,
                new DalQueryAttributes
                {
                    FromRevisionTimestamp = DateTime.MinValue,
                    ToRevisionTimestamp = DateTime.Now
                });

            // Get the Revisions for all needed Parameter(Override)ValueSets from the cache
            parameterValueSets =
                this.Session.Assembler.Cache
                    .Where(x => new[] { ClassKind.ParameterValueSet, ClassKind.ParameterOverrideValueSet }.Contains(x.Value.Value.ClassKind)
                                && x.Value.Value.Container.Iid.Equals(this.iterationTrackParameter.ParameterOrOverride.Iid))
                    .Select(x => x.Value.Value)
                    .Cast<TValueSet>()
                    .ToList();

            // Create the list of WidgetData instances
            var widgetDataList =
                parameterValueSets
                    .Where(x => x.Published.ToString().Equals(x.ActualValue.ToString()))
                    .Select(x => new WidgetData
                    {
                        ParameterOrOverride = (TThing) x.Container,
                        Option = x.ActualOption,
                        ActualFiniteState = x.ActualState,
                        Value = x.Published,
                        RevisionNumber = x.RevisionNumber
                    }).Union(
                        parameterValueSets
                            .SelectMany(x => x.Revisions.Values.OfType<TValueSet>().Where(v => v.Published.ToString().Equals(v.ActualValue.ToString())), (x, y) =>
                                new WidgetData
                                {
                                    ParameterOrOverride = (TThing) x.Container,
                                    Option = y.ActualOption,
                                    ActualFiniteState = y.ActualState,
                                    Value = y.Published,
                                    RevisionNumber = y.RevisionNumber
                                }
                            ))
                    .OrderBy(x => x.ParameterOrOverride)
                    .ThenBy(x => x.RevisionNumber)
                    .ThenBy(x => x.Option?.Name ?? "")
                    .ThenBy(x => x.ActualFiniteState?.Name ?? "")
                    .ToList();

            //Create the LineSeries instances
            var lineSeries = widgetDataList.GroupBy(
                    x => (x.ParameterOrOverride, x.ParameterName, x.OptionName, x.StateName),
                    x => new Line
                    {
                        RevisionNumber = x.RevisionNumber,
                        Value = x.Value.First().ToString().Replace("-", "0")
                    },
                    (serie, lines) => new LineSeries
                    {
                        ParameterName = serie.ParameterName,
                        OptionName = serie.OptionName,
                        StateName = serie.StateName,
                        Lines = lines.OrderBy(x => x.RevisionNumber).ToList()
                    })
                .ToList();

            this.lineSeriesCollection.Clear();
            this.lineSeriesCollection.AddRange(lineSeries);

            this.RefreshViewValues();
            this.UpdateValueSetListeners();
            this.VerifyRequirements();
        }

        /// <summary>
        /// Refresh the values in the widget's UI
        /// </summary>
        private void RefreshViewValues()
        {
            this.StatusIndicatorColor = CDP4Color.Inconclusive.GetBrush();

            var percentChange = "-";
            var latestTwoValues = this.LineSeriesCollection.FirstOrDefault()?.Lines.OrderByDescending(x => x.RevisionNumber).Take(2).ToList();

            var currentVal = latestTwoValues?.FirstOrDefault()?.Value?.ToString() ?? "-";
            var previousVal = latestTwoValues?.LastOrDefault()?.Value?.ToString() ?? "-";

            if (double.TryParse(currentVal, out var currValDouble))
            {
                if (double.TryParse(previousVal, out var prevValDouble))
                {
                    var percent = (currValDouble - prevValDouble) / prevValDouble * 100.0;

                    if (percent > 0)
                    {
                        this.StatusIndicator = "▲";
                    }
                    else if (percent < 0)
                    {
                        this.StatusIndicator = "▼";
                    }
                    else
                    {
                        this.StatusIndicator = "=";
                    }

                    percentChange = string.Format(CultureInfo.InvariantCulture, "{0:#.##}", percent);
                }
            }

            this.CurrentValue = $"{currentVal} [{this.Unit}]";
            this.PreviousValue = $"{previousVal} [{this.Unit}]";
            this.PercentageChange = $"{this.StatusIndicator} {percentChange}% from {this.PreviousValue}";
        }

        /// <summary>
        /// Constructs the excel formatted text
        /// </summary>
        /// <returns>The excel formatted text</returns>
        private string ConstructTableString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Join("\t", new LineSeries().GetExportHeaders()));

            // add parameters
            foreach (var series in this.LineSeriesCollection)
            {
                foreach (var dataRow in series.GetExportData())
                {
                    sb.AppendLine(string.Join("\t", dataRow));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Copies the data into a excel formatted text
        /// </summary>
        private void CopyTextToClipboard()
        {
            var tableString = this.ConstructTableString();

            // send to clipboard
            Clipboard.SetData(DataFormats.UnicodeText, tableString);
        }

        /// <summary>
        /// Actions to perform when a widget <see cref="OnToggleChartVisibilityCommand"/> command is called
        /// </summary>
        protected void ToggleChartVisibility()
        {
            this.ChartVisible =
                this.ChartVisible == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
    }
}
