// ------------------------------------------------------------------------------------------------
// <copyright file="MatrixViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4RelationshipMatrix.DataTypes;
    using CDP4RelationshipMatrix.Settings;

    using DevExpress.Xpf.Grid;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view-model associated to the actual relationship matrix that makes up rows and columns
    /// </summary>
    public class MatrixViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The fieldname for the name column
        /// </summary>
        public const string ROW_NAME_COLUMN = "CDP4_THING_NAME";

        /// <summary>
        /// The fieldname for the shortname column
        /// </summary>
        public const string ROW_SHORTNAME_COLUMN = "CDP4_THING_SHORTNAME";

        /// <summary>
        /// The header for the name column
        /// </summary>
        public const string CDP4_NAME_HEADER = "";

        /// <summary>
        /// The <see cref="ISession" />
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The current <see cref="Iteration" />
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// Backing field for <see cref="Title" />
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="SelectedColumnDetails" />
        /// </summary>
        private string selectedColumnDetails;

        /// <summary>
        /// Backing field for <see cref="SelectedRowDetails" />
        /// </summary>
        private string selectedRowDetails;

        /// <summary>
        /// Backing field for <see cref="SelectedCellDetails" />
        /// </summary>
        private string selectedCellDetails;

        /// <summary>
        /// Backing field for <see cref="SelectedCell" />
        /// </summary>
        private object selectedCell;

        /// <summary>
        /// Backing field for <see cref="CanCreateSourceYToSourceX" />
        /// </summary>
        private bool canCreateSourceYToSourceX;

        /// <summary>
        /// Backing field for <see cref="CanCreateSourceXToSourceY" />
        /// </summary>
        private bool canCreateSourceXToSourceY;

        /// <summary>
        /// Backing field for <see cref="CanDelete" />
        /// </summary>
        private bool canDelete;

        /// <summary>
        /// Backing field for <see cref="IsVisibleDeleteYToX" />
        /// </summary>
        private bool isVisibleDeleteYToX;

        /// <summary>
        /// Backing field for <see cref="IsVisibleDeleteXToY" />
        /// </summary>
        private bool isVisibleDeleteXToY;

        /// <summary>
        /// Backing field for <see cref="IsVisibleDeleteAll" />
        /// </summary>
        private bool isVisibleDeleteAll;

        /// <summary>
        /// Dictionary that contains the current cells
        /// </summary>
        private readonly Dictionary<string, MatrixCellViewModel> currentCells =
            new Dictionary<string, MatrixCellViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixViewModel" /> class
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="settings">The module settings</param>
        public MatrixViewModel(ISession session, Iteration iteration, RelationshipMatrixPluginSettings settings)
        {
            this.Disposables = new List<IDisposable>();

            this.Records = new ReactiveList<IDictionary<string, MatrixCellViewModel>>();
            this.Columns = new ReactiveList<ColumnDefinition>();

            this.session = session;
            this.iteration = iteration;

            this.CreateSourceYToSourceXLink = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanCreateSourceYToSourceX),
                x => this.CreateRelationship(RelationshipDirectionKind.RowThingToColumnThing),
                RxApp.MainThreadScheduler);

            this.CreateSourceXToSourceYLink = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanCreateSourceXToSourceY),
                x => this.CreateRelationship(RelationshipDirectionKind.ColumnThingToRowThing),
                RxApp.MainThreadScheduler);

            this.DeleteSourceYToSourceXLink = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanDelete),
                x => this.DeleteRelationship(RelationshipDirectionKind.RowThingToColumnThing),
                RxApp.MainThreadScheduler);

            this.DeleteSourceXToSourceYLink = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanDelete),
                x => this.DeleteRelationship(RelationshipDirectionKind.ColumnThingToRowThing),
                RxApp.MainThreadScheduler);

            this.DeleteAllRelationships = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanDelete),
                x => this.DeleteRelationship(RelationshipDirectionKind.BiDirectional),
                RxApp.MainThreadScheduler);

            this.ProcessCellCommand =
                ReactiveCommand.CreateAsyncTask(x => this.ProcessCellCommandExecute((List<object>) x),
                    RxApp.MainThreadScheduler);

            this.ProcessAltCellCommand =
                ReactiveCommand.CreateAsyncTask(x => this.ProcessAltCellCommandExecute((List<object>) x),
                    RxApp.MainThreadScheduler);

            this.ProcessAltControlCellCommand = ReactiveCommand.CreateAsyncTask(
                x => this.ProcessAltControlCellCommandExecute((List<object>) x), RxApp.MainThreadScheduler);

            this.ToggleColumnHighlightCommand = ReactiveCommand.CreateAsyncTask(
                x => this.ToggleColumnHighlightCommandExecute(x as GridColumn), RxApp.MainThreadScheduler);

            this.MouseDownCommand = ReactiveCommand.Create();
            this.MouseDownCommand.Subscribe(x => this.MouseDownCommandExecute((MatrixAddress) x));

            this.SubscribeCommandExceptions();

            this.WhenAnyValue(x => x.SelectedCell).Subscribe(_ => this.ComputeCommandCanExecute());
        }

        /// <summary>
        /// Gets the list of <see cref="IDisposable" /> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="BinaryRelationship" /> can be created from sourceY to sourceX
        /// </summary>
        public bool CanCreateSourceYToSourceX
        {
            get { return this.canCreateSourceYToSourceX; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateSourceYToSourceX, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="BinaryRelationship" /> can be created from sourceX to sourceY
        /// </summary>
        public bool CanCreateSourceXToSourceY
        {
            get { return this.canCreateSourceXToSourceY; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateSourceXToSourceY, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="BinaryRelationship" />s can be deleted
        /// </summary>
        public bool CanDelete
        {
            get { return this.canDelete; }
            private set { this.RaiseAndSetIfChanged(ref this.canDelete, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="DeleteSourceYToSourceXLink" /> button is visible
        /// </summary>
        public bool IsVisibleDeleteYToX
        {
            get { return this.isVisibleDeleteYToX; }
            private set { this.RaiseAndSetIfChanged(ref this.isVisibleDeleteYToX, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="DeleteSourceXToSourceYLink" /> button is visible
        /// </summary>
        public bool IsVisibleDeleteXToY
        {
            get { return this.isVisibleDeleteXToY; }
            private set { this.RaiseAndSetIfChanged(ref this.isVisibleDeleteXToY, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="DeleteAllRelationships" /> button is visible
        /// </summary>
        public bool IsVisibleDeleteAll
        {
            get { return this.isVisibleDeleteAll; }
            private set { this.RaiseAndSetIfChanged(ref this.isVisibleDeleteAll, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether deprecated items are shown
        /// </summary>
        public bool IsDeprecatedDisplayed { get; set; } = !FilterStringService.FilterString.IsFilterActive;

        /// <summary>
        /// Gets or sets the records used for row construction.
        /// </summary>
        public ReactiveList<IDictionary<string, MatrixCellViewModel>> Records { get; set; }

        /// <summary>
        /// Gets or sets the column definitions.
        /// </summary>
        public ReactiveList<ColumnDefinition> Columns { get; set; }

        /// <summary>
        /// Gets the command to create a <see cref="BinaryRelationship" /> from sourceY to sourceX
        /// </summary>
        public ReactiveCommand<Unit> CreateSourceYToSourceXLink { get; private set; }

        /// <summary>
        /// Gets the command to create a <see cref="BinaryRelationship" /> from sourceX to sourceY
        /// </summary>
        public ReactiveCommand<Unit> CreateSourceXToSourceYLink { get; private set; }

        /// <summary>
        /// Gets the command to delete a <see cref="BinaryRelationship" /> from sourceY to sourceX
        /// </summary>
        public ReactiveCommand<Unit> DeleteSourceYToSourceXLink { get; private set; }

        /// <summary>
        /// Gets the command to delete a <see cref="BinaryRelationship" /> from sourceX to sourceY
        /// </summary>
        public ReactiveCommand<Unit> DeleteSourceXToSourceYLink { get; private set; }

        /// <summary>
        /// Gets the command to delete all <see cref="BinaryRelationship" /> currently displayed
        /// </summary>
        public ReactiveCommand<Unit> DeleteAllRelationships { get; private set; }

        /// <summary>
        /// Gets the command to process cell double click.
        /// </summary>
        public ReactiveCommand<Unit> ProcessCellCommand { get; private set; }

        /// <summary>
        /// Gets the command to process Alt + cell double click.
        /// </summary>
        public ReactiveCommand<Unit> ProcessAltCellCommand { get; private set; }

        /// <summary>
        /// Gets the command to process Alt + Ctrl + cell double click.
        /// </summary>
        public ReactiveCommand<Unit> ProcessAltControlCellCommand { get; private set; }

        /// <summary>
        /// Gets the command to process the mousedown click.
        /// </summary>
        public ReactiveCommand<object> MouseDownCommand { get; private set; }

        /// <summary>
        /// Gets the command to process column highlight toggle.
        /// </summary>
        public ReactiveCommand<Unit> ToggleColumnHighlightCommand { get; private set; }

        /// <summary>
        /// Gets or sets the selected cell
        /// </summary>
        public object SelectedCell
        {
            get { return this.selectedCell; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCell, value); }
        }

        /// <summary>
        /// Gets the title
        /// </summary>
        public string Title
        {
            get { return this.title; }
            private set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        /// <summary>
        /// Gets or sets the string that represents the details of the selected column
        /// </summary>
        public string SelectedColumnDetails
        {
            get { return this.selectedColumnDetails; }
            private set { this.RaiseAndSetIfChanged(ref this.selectedColumnDetails, value); }
        }

        /// <summary>
        /// Gets or sets the string that represents the details of the selected item
        /// </summary>
        public string SelectedRowDetails
        {
            get { return this.selectedRowDetails; }
            private set { this.RaiseAndSetIfChanged(ref this.selectedRowDetails, value); }
        }

        /// <summary>
        /// Gets or sets the string that represents the details of the selected cell
        /// </summary>
        public string SelectedCellDetails
        {
            get { return this.selectedCellDetails; }
            private set { this.RaiseAndSetIfChanged(ref this.selectedCellDetails, value); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // Clear all property values that maybe have been set
                    // when the class was instantiated
                    if (this.Disposables != null)
                    {
                        foreach (var disposable in this.Disposables)
                        {
                            disposable.Dispose();
                        }
                    }
                    else
                    {
                        logger.Trace("The Disposables collection of the {0} is null", this.GetType().Name);
                    }
                }

                // Indicate that the instance has been disposed.
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Rebuilds the matrix
        /// </summary>
        /// <param name="sourceY">The <see cref="SourceConfigurationViewModel" /> of the Y axis.</param>
        /// <param name="sourceX">The <see cref="SourceConfigurationViewModel" /> of the X axis.</param>
        /// <param name="relationshipRule">The <see cref="BinaryRelationshipRule" /></param>
        /// <param name="showRelatedOnly">Show only the related rows and columns.</param>
        public void RebuildMatrix(SourceConfigurationViewModel sourceY, SourceConfigurationViewModel sourceX,
            BinaryRelationshipRule relationshipRule, bool showRelatedOnly)
        {
            this.Columns.Clear();
            this.Records.Clear();

            var unfilteredRecords = new List<IDictionary<string, MatrixCellViewModel>>();

            this.Title = "-";

            if (!sourceY.SelectedClassKind.HasValue || !sourceX.SelectedClassKind.HasValue || relationshipRule == null)
            {
                return;
            }

            this.Title = relationshipRule.Name;

            var things = this.session.Assembler.Cache
                .Where(x => x.Key.Iteration.HasValue && x.Key.Iteration.Value == this.iteration.Iid)
                .Select(x => x.Value.Value)
                .Where(x => x.ClassKind == sourceX.SelectedClassKind.Value ||
                            x.ClassKind == sourceY.SelectedClassKind.Value).ToList();

            List<DefinedThing> sourceXThing;
            List<DefinedThing> sourceYThing;

            try
            {
                sourceYThing = things.Where(x => x.ClassKind == sourceY.SelectedClassKind.Value).Cast<DefinedThing>()
                    .ToList();

                sourceXThing = things.Where(x => x.ClassKind == sourceX.SelectedClassKind.Value).Cast<DefinedThing>()
                    .ToList();
            }
            catch (InvalidCastException)
            {
                return;
            }

            if (sourceYThing.Count == 0 || sourceXThing.Count == 0)
            {
                return;
            }

            var sourceXToUse = new List<DefinedThing>(this.FilterAndSortSourceByCategory(sourceXThing, sourceX));

            var sourceYToUse = new List<DefinedThing>(this.FilterAndSortSourceByCategory(sourceYThing, sourceY));

            if (sourceYToUse.Count == 0 || sourceXToUse.Count == 0)
            {
                return;
            }

            // get relationships
            var relationships = this.QueryBinaryRelationshipInContext(relationshipRule).ToList();

            // create columns
            var unfilteredColumns =
                this.CreateColumns(sourceXToUse, sourceX.SelectedDisplayKind, showRelatedOnly, relationships);

            // create rows
            foreach (var definedThing in sourceYToUse)
            {
                if (showRelatedOnly && !relationships.Any(x =>
                        x.Source.Iid.Equals(definedThing.Iid) || x.Target.Iid.Equals(definedThing.Iid)))
                {
                    // if thing is ont in any relationships, skip
                    continue;
                }

                unfilteredRecords.Add(this.ComputeRow(definedThing, sourceXToUse, relationships, relationshipRule,
                    sourceY.SelectedDisplayKind, unfilteredColumns));
            }

            // secondary filter remove unwanted rows and columns
            if (showRelatedOnly)
            {
                this.Columns.AddRange(unfilteredColumns.Where(x => x.RelationshipCount != 0).ToList());

                this.Records.AddRange(unfilteredRecords.Where(x =>
                    x.Any(c => c.Value.RelationshipDirection != RelationshipDirectionKind.None)));
            }
            else
            {
                this.Columns.AddRange(unfilteredColumns);
                this.Records.AddRange(unfilteredRecords);
            }
        }

        /// <summary>
        /// Refresh the content of the cells
        /// </summary>
        public void RefreshMatrix(BinaryRelationshipRule relationshipRule)
        {
            var updatedRelationships = this.QueryBinaryRelationshipInContext(relationshipRule).ToList();
            var displayedRelationships = this.currentCells.Values.SelectMany(x => x.Relationships).ToList();

            var oldRelationships = displayedRelationships.Except(updatedRelationships);
            var newRelationships = updatedRelationships.Except(displayedRelationships);

            foreach (var relationship in oldRelationships.Union(newRelationships))
            {
                var definedSource = relationship.Source as DefinedThing;
                var definedTarget = relationship.Target as DefinedThing;

                if (definedSource == null || definedTarget == null)
                {
                    continue;
                }

                var sourceRow = this.QueryRow(relationship.Source.Iid);
                var targetRow = this.QueryRow(relationship.Target.Iid);

                if (sourceRow != null)
                {
                    var cellValue = this.ComputeCell(definedSource, definedTarget, updatedRelationships,
                        relationshipRule);

                    sourceRow[definedTarget.ShortName] = cellValue;
                    this.UpdateCurrentCell(definedSource, definedTarget, cellValue);
                }

                if (targetRow != null)
                {
                    var cellValue = this.ComputeCell(definedTarget, definedSource, updatedRelationships,
                        relationshipRule);

                    targetRow[definedSource.ShortName] = cellValue;

                    this.UpdateCurrentCell(definedTarget, definedSource, cellValue);
                }
            }
        }

        /// <summary>
        /// Subscribes to exceptions thrown on command executions to log errors.
        /// </summary>
        private void SubscribeCommandExceptions()
        {
            this.Disposables.Add(this.CreateSourceYToSourceXLink.ThrownExceptions.Subscribe(x =>
                logger.Error("The relationship could not be created")));

            this.Disposables.Add(this.CreateSourceXToSourceYLink.ThrownExceptions.Subscribe(x =>
                logger.Error("The relationship could not be created")));

            this.Disposables.Add(this.DeleteSourceYToSourceXLink.ThrownExceptions.Subscribe(x =>
                logger.Error("The relationship could not be deleted")));

            this.Disposables.Add(this.DeleteSourceXToSourceYLink.ThrownExceptions.Subscribe(x =>
                logger.Error("The relationship could not be deleted")));

            this.Disposables.Add(this.DeleteAllRelationships.ThrownExceptions.Subscribe(x =>
                logger.Error("The relationships could not be deleted")));
        }

        /// <summary>
        /// Creates the column definitions based on the source.
        /// </summary>
        /// <param name="source">The <see cref="Thing" /> representing the columns</param>
        /// <param name="displayKind">The <see cref="DisplayKind" /> of the column.</param>
        /// <param name="showRelatedOnly">Indicate whether to only show related elements.</param>
        /// <param name="relationships">The list of all relationships.</param>
        private IList<ColumnDefinition> CreateColumns(IReadOnlyList<DefinedThing> source, DisplayKind displayKind,
            bool showRelatedOnly, IList<BinaryRelationship> relationships)
        {
            var columns = new List<ColumnDefinition>();

            foreach (var definedThing in source.DistinctBy(x => x.ShortName))
            {
                if (showRelatedOnly && !relationships.Any(x =>
                        x.Source.Iid.Equals(definedThing.Iid) || x.Target.Iid.Equals(definedThing.Iid)))
                {
                    // if thing is not in any relationships, skip
                    continue;
                }

                if (columns.Any(x => x.FieldName == definedThing.ShortName))
                {
                    // skip duplicated shortname
                    continue;
                }

                // Set fieldname to use as iid
                columns.Add(new ColumnDefinition(definedThing, displayKind));
            }

            if (columns.Any())
            {
                // column that contains the name of the thing to display for each row
                columns.Insert(0, new ColumnDefinition(CDP4_NAME_HEADER, ROW_NAME_COLUMN, true));
            }

            return columns;
        }

        /// <summary>
        /// Computes a row
        /// </summary>
        /// <param name="rowThing">The <see cref="Thing" /> of the current row</param>
        /// <param name="columnThings">The <see cref="Thing" />s displayed in the columns</param>
        /// <param name="relationships">The current set of <see cref="BinaryRelationship" /></param>
        /// <param name="relationshipRule">The current <see cref="BinaryRelationshipRule" /></param>
        /// <param name="displayKind">The <see cref="DisplayKind" /> of the current Row.</param>
        /// <param name="columnDefinitions">The defined columns.</param>
        /// <returns>The <see cref="IDictionary{TKey,TValue}" /> that corresponds to a row</returns>
        private IDictionary<string, MatrixCellViewModel> ComputeRow(DefinedThing rowThing,
            IReadOnlyList<DefinedThing> columnThings,
            IReadOnlyList<BinaryRelationship> relationships, BinaryRelationshipRule relationshipRule,
            DisplayKind displayKind, IList<ColumnDefinition> columnDefinitions)
        {
            var record = new Dictionary<string, MatrixCellViewModel>
            {
                { ROW_NAME_COLUMN, new MatrixCellViewModel(rowThing, null, null, relationshipRule, displayKind) }
            };

            foreach (var definedThing in columnThings)
            {
                if (columnDefinitions.All(x => !x.ThingId.Equals(definedThing.Iid)))
                {
                    continue;
                }

                var cellValue = this.ComputeCell(rowThing, definedThing, relationships, relationshipRule);
                record.Add(definedThing.ShortName, cellValue);

                this.UpdateCurrentCell(rowThing, definedThing, cellValue);

                if (cellValue.RelationshipDirection != RelationshipDirectionKind.None)
                {
                    columnDefinitions.Single(s => s.ThingId.Equals(definedThing.Iid)).RelationshipCount++;
                }
            }

            return record;
        }

        /// <summary>
        /// Update the current cell cache <see cref="currentCells" />
        /// </summary>
        /// <param name="row">The thing in the current row context</param>
        /// <param name="col">The thing in the current column context</param>
        /// <param name="cellValue">The cell object</param>
        private void UpdateCurrentCell(DefinedThing row, DefinedThing col, MatrixCellViewModel cellValue)
        {
            var cellRef = $"{row.Iid}_{col.Iid}";

            if (this.currentCells.ContainsKey(cellRef))
            {
                this.currentCells[cellRef] = cellValue;
            }
            else
            {
                this.currentCells.Add(cellRef, cellValue);
            }
        }

        /// <summary>
        /// Computes a <see cref="MatrixViewModel" /> representing a cell
        /// </summary>
        /// <param name="rowThing">The <see cref="Thing" /> of the current row</param>
        /// <param name="columnThing">The <see cref="Thing" /> of the current column</param>
        /// <param name="relationships">The current sets of <see cref="BinaryRelationship" /></param>
        /// <param name="relationshipRule">The current <see cref="BinaryRelationshipRule" /></param>
        /// <returns>The <see cref="MatrixCellViewModel" /></returns>
        private MatrixCellViewModel ComputeCell(DefinedThing rowThing, DefinedThing columnThing,
            IReadOnlyList<BinaryRelationship> relationships, BinaryRelationshipRule relationshipRule)
        {
            var relationship = relationships.Where(x =>
                (x.Source?.Iid == rowThing.Iid || x.Source?.Iid == columnThing.Iid) &&
                (x.Target?.Iid == rowThing.Iid || x.Target?.Iid == columnThing.Iid)).ToList();

            var cellValue = relationship.Count > 0 
                ? new MatrixCellViewModel(rowThing, columnThing, relationship, relationshipRule)
                : new MatrixCellViewModel(rowThing, columnThing, null, relationshipRule);

            return cellValue;
        }

        /// <summary>
        /// Query a row by using the represented thing identifier
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <returns>The <see cref="ExpandoObject" /> representing the row</returns>
        private IDictionary<string, MatrixCellViewModel> QueryRow(Guid id)
        {
            var rows = this.Records.ToList();

            foreach (var row in rows)
            {
                var firstCell = row.Values.OfType<MatrixCellViewModel>().FirstOrDefault();

                if (firstCell != null && firstCell.SourceY.Iid == id)
                {
                    return row;
                }
            }

            return null;
        }

        /// <summary>
        /// Filters and sort the <paramref name="source" /> with the <paramref name="sourceConfigurationViewModel" />
        /// </summary>
        /// <param name="source">The <see cref="Thing" /> to filter</param>
        /// <param name="sourceConfigurationViewModel">The filter and sort settings</param>
        /// <returns>The filtered <see cref="Thing" /></returns>
        private IEnumerable<DefinedThing> FilterAndSortSourceByCategory(IReadOnlyList<DefinedThing> source,
            SourceConfigurationViewModel sourceConfigurationViewModel)
        {
            var sourceXCatThing = new List<DefinedThing>();

            if (sourceConfigurationViewModel?.SelectedCategories == null ||
                sourceConfigurationViewModel.SelectedCategories.Count == 0)
            {
                return sourceXCatThing;
            }

            foreach (var definedThing in source)
            {
                if (!this.IsDefinedThingDisplayAllowed(definedThing))
                {
                    continue;
                }

                var thing = (ICategorizableThing) definedThing;

                if (RelationshipMatrixViewModel.IsCategoryApplicableToConfiguration(thing, sourceConfigurationViewModel)
                )
                {
                    sourceXCatThing.Add(definedThing);
                }
            }

            if (sourceConfigurationViewModel.SelectedSortOrder == SortOrder.Ascending)
            {
                sourceXCatThing = sourceConfigurationViewModel.SelectedSortKind == DisplayKind.Name
                    ? sourceXCatThing.OrderBy(x => x.Name).ToList()
                    : sourceXCatThing.OrderBy(x => x.ShortName).ToList();
            }
            else
            {
                sourceXCatThing = sourceConfigurationViewModel.SelectedSortKind == DisplayKind.Name
                    ? sourceXCatThing.OrderByDescending(x => x.Name).ToList()
                    : sourceXCatThing.OrderByDescending(x => x.ShortName).ToList();
            }

            return sourceXCatThing;
        }

        /// <summary>
        /// Checks if a<see cref="DefinedThing" /> is allowed to be displayed in the UI
        /// </summary>
        /// <param name="definedThing">
        /// The <see cref="DefinedThing" /> to check
        /// </param>
        /// <returns>
        /// true is display is allowed
        /// </returns>
        private bool IsDefinedThingDisplayAllowed(DefinedThing definedThing)
        {
            if (this.IsDeprecatedDisplayed)
            {
                return true;
            }

            if (!(definedThing is IDeprecatableThing deprecatableThing))
            {
                return true;
            }

            return !deprecatableThing.IsDeprecated;
        }

        /// <summary>
        /// Queries the <see cref="BinaryRelationship" /> within the current context
        /// </summary>
        /// <param name="rule">The selected <see cref="BinaryRelationshipRule" /></param>
        /// <returns>The list of <see cref="BinaryRelationship" /></returns>
        private IEnumerable<BinaryRelationship> QueryBinaryRelationshipInContext(BinaryRelationshipRule rule)
        {
            return this.iteration.Relationship.OfType<BinaryRelationship>()
                .Where(x => x.Category.Contains(rule.RelationshipCategory));
        }

        /// <summary>
        /// Compute the boolean that determines the available commands for a selected cell
        /// </summary>
        private void ComputeCommandCanExecute()
        {
            var vm = this.SelectedCell as MatrixCellViewModel;
            this.SetPropertiesOnSelectedItemChanged(vm);

            if (vm?.SourceX == null)
            {
                this.CanCreateSourceYToSourceX = false;
                this.CanCreateSourceXToSourceY = false;
                this.CanDelete = false;
                this.IsVisibleDeleteYToX = false;
                this.IsVisibleDeleteXToY = false;
                this.IsVisibleDeleteAll = false;

                return;
            }

            var canWrite = this.session.PermissionService.CanWrite(ClassKind.BinaryRelationship, this.iteration);

            this.CanCreateSourceYToSourceX = canWrite && vm.SourceY.Iid != vm.SourceX.Iid &&
                                             vm.RelationshipDirection !=
                                             RelationshipDirectionKind.RowThingToColumnThing &&
                                             vm.RelationshipDirection != RelationshipDirectionKind.BiDirectional;

            this.CanCreateSourceXToSourceY = canWrite && vm.SourceY.Iid != vm.SourceX.Iid &&
                                             vm.RelationshipDirection !=
                                             RelationshipDirectionKind.ColumnThingToRowThing &&
                                             vm.RelationshipDirection != RelationshipDirectionKind.BiDirectional;

            this.CanDelete = canWrite && vm.RelationshipDirection != RelationshipDirectionKind.None;

            this.IsVisibleDeleteAll = this.CanDelete &&
                                      vm.RelationshipDirection == RelationshipDirectionKind.BiDirectional;

            this.IsVisibleDeleteYToX = this.IsVisibleDeleteAll ||
                                       this.CanDelete && vm.RelationshipDirection ==
                                       RelationshipDirectionKind.RowThingToColumnThing;

            this.IsVisibleDeleteXToY = this.IsVisibleDeleteAll ||
                                       this.CanDelete && vm.RelationshipDirection ==
                                       RelationshipDirectionKind.ColumnThingToRowThing;
        }

        /// <summary>
        /// Executes a creation or deletion of a y-x relationship based on the state of a double clicked cell.
        /// </summary>
        /// <param name="cellInfo">The array of cell information.</param>
        private async Task ProcessCellCommandExecute(List<object> cellInfo)
        {
            // if not a relationship cell do nothing
            if (cellInfo?[1] == null || cellInfo[1].Equals(CDP4_NAME_HEADER))
            {
                return;
            }

            // if relationship from y to x does not exist create one, if it does, delete it
            if (this.CanCreateSourceYToSourceX)
            {
                await this.CreateSourceYToSourceXLink.ExecuteAsync();

                return;
            }

            if (this.CanDelete && this.IsVisibleDeleteYToX)
            {
                await this.DeleteSourceYToSourceXLink.ExecuteAsync();

                return;
            }
        }

        /// <summary>
        /// Executes a creation or deletion of a x-y relationship based on the state of a double clicked cell.
        /// </summary>
        /// <param name="cellInfo">The array of cell information.</param>
        private async Task ProcessAltCellCommandExecute(List<object> cellInfo)
        {
            if (cellInfo?[1] == null || cellInfo[1].Equals(CDP4_NAME_HEADER))
            {
                return;
            }

            // if relationship from x to y does not exist create one, if it does, delete it
            if (this.CanCreateSourceXToSourceY)
            {
                await this.CreateSourceXToSourceYLink.ExecuteAsync();

                return;
            }

            if (this.CanDelete && this.IsVisibleDeleteXToY)
            {
                await this.DeleteSourceXToSourceYLink.ExecuteAsync();

                return;
            }
        }

        /// <summary>
        /// Executes a deletion of all relationships based on the state of a double clicked cell.
        /// </summary>
        /// <param name="cellInfo">The array of cell information.</param>
        private async Task ProcessAltControlCellCommandExecute(List<object> cellInfo)
        {
            if (cellInfo?[1] == null || cellInfo[1].Equals(CDP4_NAME_HEADER))
            {
                return;
            }

            // if bidirectional
            if (this.CanDelete && this.IsVisibleDeleteAll)
            {
                await this.DeleteAllRelationships.ExecuteAsync();

                return;
            }

            // if 1 to 2
            if (this.CanDelete && this.IsVisibleDeleteYToX)
            {
                await this.DeleteSourceYToSourceXLink.ExecuteAsync();

                return;
            }

            // if 2 - 1
            if (this.CanDelete && this.IsVisibleDeleteXToY)
            {
                await this.DeleteSourceXToSourceYLink.ExecuteAsync();

                return;
            }
        }

        /// <summary>
        /// Executes the <see cref="MouseDownCommand" />
        /// </summary>
        /// <param name="matrixAddress">
        /// the address of the selected cell
        /// </param>
        private void MouseDownCommandExecute(MatrixAddress matrixAddress)
        {
            //Only if row is null, otherwise SelectedCell PropertyChanged handles setting properties
            if (matrixAddress.Row is null)
            {
                this.SetRowAndColumnPropertiesOnSelectedItemChanged(matrixAddress);
            }
        }

        /// <summary>
        /// Sets the info detail properties
        /// </summary>
        /// <param name="vm"><see cref="MatrixCellViewModel"/> that represents the currently selected grid cell</param>
        private void SetPropertiesOnSelectedItemChanged(MatrixCellViewModel vm)
        {
            this.SelectedCellDetails = string.Empty;

            if (vm == null)
            {
                return;
            }

            var selectedRow = this.Records.FirstOrDefault(x => x.SingleOrDefault(y => y.Value == vm).Key != null);
            if (selectedRow != null)
            {
                var thing = vm.SourceX as DefinedThing;

                var matrixAddress = new MatrixAddress
                {
                    Column = thing?.Name ?? string.Empty,
                    Row = this.Records.IndexOf(selectedRow)
                };

                this.SetRowAndColumnPropertiesOnSelectedItemChanged(matrixAddress);
            }

            var toolTip = new StringBuilder();
            foreach (var relationship in vm.Relationships)
            {
                if (toolTip.Length > 0)
                {
                    toolTip.AppendLine("");
                    toolTip.AppendLine("");
                }

                toolTip.AppendLine(relationship.Source.Equals(vm.SourceY) ? "------------->>>" : "<<<-------------");
                toolTip.Append(relationship.Tooltip());
            }

            this.SelectedCellDetails = toolTip.ToString();
        }

        /// <summary>
        /// Sets properties when the selected item changes
        /// </summary>
        /// <param name="matrixAddress">
        /// the address of the selected cell
        /// </param>
        private void SetRowAndColumnPropertiesOnSelectedItemChanged(MatrixAddress matrixAddress)
        {
            if (matrixAddress == null)
            {
                this.SelectedColumnDetails = string.Empty;
                this.SelectedRowDetails = string.Empty;

                return;
            }

            var columnDefinition = this.Columns.SingleOrDefault(c => c.FieldName == matrixAddress.Column);
            this.SelectedColumnDetails = columnDefinition?.ToolTip;

            if (matrixAddress.Row != null)
            {
                var row = this.Records[matrixAddress.Row.Value];

                var firstColumn = row.FirstOrDefault();
                this.SelectedRowDetails = firstColumn
                                              .Value?
                                              .Tooltip ??
                                          string.Empty;
            }
        }

        /// <summary>
        /// Executes a toggle of column highlight command
        /// </summary>
        /// <param name="column">The column.</param>
        private Task ToggleColumnHighlightCommandExecute(GridColumn column)
        {
            if (column != null)
            {
                var vm = column.DataContext as ColumnDefinition;
                vm?.ToggleHighlight();
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationship" /> for the selected cell
        /// </summary>
        /// <param name="direction">The direction fo the relationship to create</param>
        /// <returns>The task</returns>
        private Task CreateRelationship(RelationshipDirectionKind direction)
        {
            var vm = this.SelectedCell as MatrixCellViewModel;

            if (vm == null)
            {
                return Task.FromResult(0);
            }

            var relationship = new BinaryRelationship(Guid.NewGuid(), null, null)
                { Owner = this.session.OpenIterations[this.iteration].Item1 };

            relationship.Category.Add(vm.Rule.RelationshipCategory);

            relationship.Source =
                direction == RelationshipDirectionKind.RowThingToColumnThing ? vm.SourceY : vm.SourceX;

            relationship.Target =
                direction == RelationshipDirectionKind.RowThingToColumnThing ? vm.SourceX : vm.SourceY;

            var iterationClone = this.iteration.Clone(false);
            iterationClone.Relationship.Add(relationship);

            var context = TransactionContextResolver.ResolveContext(relationship);
            var transaction = new ThingTransaction(context, iterationClone);
            transaction.Create(relationship, iterationClone);

            return this.session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Deletes a <see cref="BinaryRelationship" /> for the selected pair of thing
        /// </summary>
        /// <param name="direction">The direction of the relationship to delete</param>
        /// <returns>The task</returns>
        private Task DeleteRelationship(RelationshipDirectionKind direction)
        {
            if (!(this.SelectedCell is MatrixCellViewModel vm))
            {
                return Task.FromResult(0);
            }

            var iterationClone = this.iteration.Clone(false);
            var context = TransactionContextResolver.ResolveContext(iterationClone);
            var transaction = new ThingTransaction(context, iterationClone);

            foreach (var binaryRelationship in vm.Relationships)
            {
                var clone = binaryRelationship.Clone(false);

                if (vm.RelationshipDirection != RelationshipDirectionKind.BiDirectional ||
                    direction == RelationshipDirectionKind.BiDirectional)
                {
                    // delete every relationship
                    transaction.Delete(clone);
                }
                else if (direction == RelationshipDirectionKind.RowThingToColumnThing &&
                         vm.SourceY.Iid == binaryRelationship.Source.Iid)
                {
                    transaction.Delete(clone);
                }
                else if (direction == RelationshipDirectionKind.ColumnThingToRowThing &&
                         vm.SourceX.Iid == binaryRelationship.Source.Iid)
                {
                    transaction.Delete(clone);
                }
            }

            return this.session.Write(transaction.FinalizeTransaction());
        }
    }
}
