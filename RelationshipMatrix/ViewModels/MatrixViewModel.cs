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
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The view-model associated to the actual relationship matrix that makes up rows and columns
    /// </summary>
    public class MatrixViewModel : ReactiveObject
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The fieldname for the name column
        /// </summary>
        public const string ROW_NAME_COLUMN = "CDP4_THING_NAME";

        /// <summary>
        /// The header for the name column
        /// </summary>
        public const string CDP4_NAME_HEADER = "Source1/Source2";

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The current <see cref="Iteration"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="SelectedCell"/>
        /// </summary>
        private object selectedCell;

        /// <summary>
        /// Backing field for <see cref="CanCreateSource1ToSource2"/>
        /// </summary>
        private bool canCreateSource1ToSource2;

        /// <summary>
        /// Backing field for <see cref="CanCreateSource2ToSource1"/>
        /// </summary>
        private bool canCreateSource2ToSource1;

        /// <summary>
        /// Backing field for <see cref="CanDelete"/>
        /// </summary>
        private bool canDelete;

        /// <summary>
        /// Backing field for <see cref="IsVisibleDelete1To2"/>
        /// </summary>
        private bool isVisibleDelete1To2;

        /// <summary>
        /// Backing field for <see cref="IsVisibleDelete2To1"/>
        /// </summary>
        private bool isVisibleDelete2To1;

        /// <summary>
        /// Backing field for <see cref="IsVisibleDeleteAll"/>
        /// </summary>
        private bool isVisibleDeleteAll;

        /// <summary>
        /// Dictionary that contains the current cells
        /// </summary>
        private readonly Dictionary<string, MatrixCellViewModel> currentCells = new Dictionary<string, MatrixCellViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixViewModel"/> class
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="settings">The module settings</param>
        public MatrixViewModel(ISession session, Iteration iteration, RelationshipMatrixPluginSettings settings)
        {
            this.Records = new ReactiveList<ExpandoObject>();
            this.Columns = new ReactiveList<ColumnDefinition>();
            this.session = session;
            this.iteration = iteration;

            this.CreateSource1ToSource2Link = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanCreateSource1ToSource2),
                x => this.CreateRelationship(RelationshipDirectionKind.RowThingToColumnThing),
                RxApp.MainThreadScheduler);

            this.CreateSource2ToSource1Link = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanCreateSource2ToSource1),
                x => this.CreateRelationship(RelationshipDirectionKind.ColumnThingToRowThing),
                RxApp.MainThreadScheduler);

            this.CreateSource1ToSource2Link.ThrownExceptions.Subscribe(x => logger.Error("The relationship could not be created"));
            this.CreateSource2ToSource1Link.ThrownExceptions.Subscribe(x => logger.Error("The relationship could not be created"));

            this.DeleteSource1ToSource2Link = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanDelete),
                x => this.DeleteRelationship(RelationshipDirectionKind.RowThingToColumnThing),
                RxApp.MainThreadScheduler);

            this.DeleteSource2ToSource1Link = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanDelete),
                x => this.DeleteRelationship(RelationshipDirectionKind.ColumnThingToRowThing),
                RxApp.MainThreadScheduler);

            this.DeleteAllRelationships = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanDelete),
                x => this.DeleteRelationship(RelationshipDirectionKind.BiDirectional),
                RxApp.MainThreadScheduler);

            this.DeleteSource1ToSource2Link.ThrownExceptions.Subscribe(x => logger.Error("The relationship could not be deleted"));
            this.DeleteSource2ToSource1Link.ThrownExceptions.Subscribe(x => logger.Error("The relationship could not be deleted"));
            this.DeleteAllRelationships.ThrownExceptions.Subscribe(x => logger.Error("The relationships could not be deleted"));

            this.WhenAnyValue(x => x.SelectedCell).Subscribe(_ => this.ComputeCommandCanExecute());
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship"/> can be created from source1 to source2
        /// </summary>
        public bool CanCreateSource1ToSource2
        {
            get { return this.canCreateSource1ToSource2; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateSource1ToSource2, value); }
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship"/> can be created from source2 to source1
        /// </summary>
        public bool CanCreateSource2ToSource1
        {
            get { return this.canCreateSource2ToSource1; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateSource2ToSource1, value); }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="BinaryRelationship"/>s can be deleted
        /// </summary>
        public bool CanDelete
        {
            get { return this.canDelete; }
            private set { this.RaiseAndSetIfChanged(ref this.canDelete, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="DeleteSource1ToSource2Link"/> button is visible
        /// </summary>
        public bool IsVisibleDelete1To2
        {
            get { return this.isVisibleDelete1To2; }
            private set { this.RaiseAndSetIfChanged(ref this.isVisibleDelete1To2, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="DeleteSource2ToSource1Link"/> button is visible
        /// </summary>
        public bool IsVisibleDelete2To1
        {
            get { return this.isVisibleDelete2To1; }
            private set { this.RaiseAndSetIfChanged(ref this.isVisibleDelete2To1, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="DeleteAllRelationships"/> button is visible
        /// </summary>
        public bool IsVisibleDeleteAll
        {
            get { return this.isVisibleDeleteAll; }
            private set { this.RaiseAndSetIfChanged(ref this.isVisibleDeleteAll, value); }
        }

        /// <summary>
        /// Gets the element-definition overview
        /// </summary>
        public ReactiveList<ExpandoObject> Records { get; private set; }

        /// <summary>
        /// Gets the <see cref="Option"/> name
        /// </summary>
        public ReactiveList<ColumnDefinition> Columns { get; private set; }

        /// <summary>
        /// Gets the command to create a <see cref="BinaryRelationship"/> from source1 to source2
        /// </summary>
        public ReactiveCommand<Unit> CreateSource1ToSource2Link { get; private set; }

        /// <summary>
        /// Gets the command to create a <see cref="BinaryRelationship"/> from source2 to source1
        /// </summary>
        public ReactiveCommand<Unit> CreateSource2ToSource1Link { get; private set; }

        /// <summary>
        /// Gets the command to delete a <see cref="BinaryRelationship"/> from source1 to source2
        /// </summary>
        public ReactiveCommand<Unit> DeleteSource1ToSource2Link { get; private set; }

        /// <summary>
        /// Gets the command to delete a <see cref="BinaryRelationship"/> from source2 to source1
        /// </summary>
        public ReactiveCommand<Unit> DeleteSource2ToSource1Link { get; private set; }

        /// <summary>
        /// Gets the command to delete all <see cref="BinaryRelationship"/> currently displayed
        /// </summary>
        public ReactiveCommand<Unit> DeleteAllRelationships { get; private set; }

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
        /// Rebuilds the matrix
        /// </summary>
        /// <param name="source1">The first source of <see cref="Thing"/></param>
        /// <param name="source1Cat">The filter category for <paramref name="source1"/></param>
        /// <param name="source2">The second source of <see cref="Thing"/></param>
        /// <param name="source2Cat">The filter category for <paramref name="source2"/></param>
        /// <param name="relationshipRule">The <see cref="BinaryRelationshipRule"/></param>
        public void RebuildMatrix(ClassKind? source1, IReadOnlyList<Category> source1Cat, ClassKind? source2, IReadOnlyList<Category> source2Cat, BinaryRelationshipRule relationshipRule)
        {
            this.Columns.Clear();
            this.Records.Clear();
            this.Title = "-";

            if (!source1.HasValue || !source2.HasValue || relationshipRule == null)
            {
                return;
            }

            this.Title = relationshipRule.Name;
            var things = this.session.Assembler.Cache
                .Where(x => x.Key.Iteration.HasValue && x.Key.Iteration.Value == this.iteration.Iid)
                .Select(x => x.Value.Value)
                .Where(x => x.ClassKind == source2.Value || x.ClassKind == source1.Value).ToList();
            List<DefinedThing> source1Thing;
            List<DefinedThing> source2Thing;

            try
            {
                source1Thing = things.Where(x => x.ClassKind == source1.Value).Cast<DefinedThing>().ToList();
                source2Thing = things.Where(x => x.ClassKind == source2.Value).Cast<DefinedThing>().ToList();
            }
            catch (InvalidCastException)
            {
                return;
            }

            if (source1Thing.Count == 0 || source2Thing.Count == 0)
            {
                return;
            }

            var source2ToUse = new List<DefinedThing>(this.FilterSourcebyCategory(source2Thing, source2Cat).OrderBy(x => x.Name));
            var source1ToUse = new List<DefinedThing>(this.FilterSourcebyCategory(source1Thing, source1Cat).OrderBy(x => x.Name));

            if (source1ToUse.Count == 0 || source2ToUse.Count == 0)
            {
                return;
            }

            this.CreateColumns(source2ToUse);

            // generates records
            var relationships = this.QueryBinaryRelationshipInContext(relationshipRule).ToList();
            foreach (var definedThing in source1ToUse)
            {
                this.Records.Add(this.ComputeRow(definedThing, source2ToUse, relationships, relationshipRule));
            }
        }

        /// <summary>
        /// Refresh the content of the cells
        /// </summary>
        public void RefreshMatrix(BinaryRelationshipRule relationshipRule)
        {
            var updatedRelationships = this.QueryBinaryRelationshipInContext(relationshipRule).ToList();
            var displayedRelationships = this.currentCells.Values.SelectMany(x => x.Relationships).ToList();

            var olds = displayedRelationships.Except(updatedRelationships);
            var newRel = updatedRelationships.Except(displayedRelationships);

            foreach (var relationship in olds.Union(newRel))
            {
                var definedSource = relationship.Source as DefinedThing;
                var definedTarget = relationship.Target as DefinedThing;

                if (definedSource == null || definedTarget == null)
                {
                    continue;
                }

                var sourceRow = (IDictionary<string, object>)this.QueryRow(relationship.Source.Iid);
                var targetRow = (IDictionary<string, object>)this.QueryRow(relationship.Target.Iid);
                if (sourceRow != null)
                {
                    var cellvalue = this.ComputeCell(definedSource, definedTarget, updatedRelationships, relationshipRule);
                    sourceRow[definedTarget.ShortName] = cellvalue;
                    this.UpdateCurrentCell(definedSource, definedTarget, cellvalue);
                }

                if (targetRow != null)
                {
                    var cellvalue = this.ComputeCell(definedTarget, definedSource, updatedRelationships, relationshipRule);
                    targetRow[definedSource.ShortName] = cellvalue;
                    this.UpdateCurrentCell(definedTarget, definedSource, cellvalue);
                }
            }
        }

        /// <summary>
        /// Creates the column
        /// </summary>
        /// <param name="source">The <see cref="Thing"/> representing the columns</param>
        private void CreateColumns(IReadOnlyList<Thing> source)
        {
            // column that contains the name of the thing to display for each row
            this.Columns.Add(new ColumnDefinition(CDP4_NAME_HEADER, ROW_NAME_COLUMN));

            var definedThingSource = source.Cast<DefinedThing>().OrderBy(x => x.Name);
            foreach (var definedThing in definedThingSource)
            {
                if (this.Columns.Any(x => x.FieldName == definedThing.ShortName))
                {
                    // skip duplicated shortname
                    continue;
                }

                    // Set Fieldname to use as iid
                this.Columns.Add(new ColumnDefinition(definedThing));
            }
        }

        /// <summary>
        /// Computes a row
        /// </summary>
        /// <param name="rowThing">The <see cref="Thing"/> of the current row</param>
        /// <param name="columnThings">The <see cref="Thing"/>s displayed in the columns</param>
        /// <param name="relationships">The current set of <see cref="BinaryRelationship"/></param>
        /// <param name="relationshipRule">The current <see cref="BinaryRelationshipRule"/></param>
        /// <returns>The <see cref="ExpandoObject"/> that corresponds to a row</returns>
        private ExpandoObject ComputeRow(DefinedThing rowThing, IReadOnlyList<DefinedThing> columnThings, IReadOnlyList<BinaryRelationship> relationships, BinaryRelationshipRule relationshipRule)
        {
            var record = new ExpandoObject();
            var dic = (IDictionary<string, object>)record;

            dic.Add(ROW_NAME_COLUMN, new MatrixCellViewModel(rowThing, null, null, relationshipRule));

            foreach (var definedThing in columnThings)
            {
                if (this.Columns.All(x => x.ThingId != definedThing.Iid))
                {
                    continue;
                }

                var cellvalue = this.ComputeCell(rowThing, definedThing, relationships, relationshipRule);
                dic.Add(definedThing.ShortName, cellvalue);

                this.UpdateCurrentCell(rowThing, definedThing, cellvalue);
            }

            return (ExpandoObject)dic;
        }

        /// <summary>
        /// Update the current cell cache <see cref="currentCells"/>
        /// </summary>
        /// <param name="row">The thing in the current row context</param>
        /// <param name="col">The thing in the current column context</param>
        /// <param name="cellvalue">The cell object</param>
        private void UpdateCurrentCell(DefinedThing row, DefinedThing col, MatrixCellViewModel cellvalue)
        {
            var cellRef = $"{row.Iid}_{col.Iid}";
            if (this.currentCells.ContainsKey(cellRef))
            {
                this.currentCells[cellRef] = cellvalue;
            }
            else
            {
                this.currentCells.Add(cellRef, cellvalue);
            }
        }

        /// <summary>
        /// Computes a <see cref="MatrixViewModel"/> representing a cell
        /// </summary>
        /// <param name="rowThing">The <see cref="Thing"/> of the current row</param>
        /// <param name="columnThing">The <see cref="Thing"/> of the current column</param>
        /// <param name="relationships">The current sets of <see cref="BinaryRelationship"/></param>
        /// <param name="relationshipRule">The current <see cref="BinaryRelationshipRule"/></param>
        /// <returns>The <see cref="MatrixCellViewModel"/></returns>
        private MatrixCellViewModel ComputeCell(DefinedThing rowThing, DefinedThing columnThing, IReadOnlyList<BinaryRelationship> relationships, BinaryRelationshipRule relationshipRule)
        {
            var relationship = relationships.Where(x => (x.Source?.Iid == rowThing.Iid || x.Source?.Iid == columnThing.Iid) && (x.Target?.Iid == rowThing.Iid || x.Target?.Iid == columnThing.Iid)).ToList();

            var cellvalue = new MatrixCellViewModel(rowThing, columnThing, null, relationshipRule);
            if (relationship.Count > 0)
            {
                cellvalue = new MatrixCellViewModel(rowThing, columnThing, relationship, relationshipRule);
            }

            return cellvalue;
        }

        /// <summary>
        /// Query a row by using the represented thing identifier
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <returns>The <see cref="ExpandoObject"/> representing the row</returns>
        private ExpandoObject QueryRow(Guid id)
        {
            var rows = this.Records.Cast<IDictionary<string, object>>().ToList();
            foreach (var row in rows)
            {
                var firstcell = row.Values.OfType<MatrixCellViewModel>().FirstOrDefault();
                if (firstcell != null && firstcell.Source1.Iid == id)
                {
                    return (ExpandoObject)row;
                }
            }

            return null;
        }

        /// <summary>
        /// Filters the <paramref name="source"/> with the <paramref name="sourceCat"/>
        /// </summary>
        /// <param name="source">The <see cref="Thing"/> to filter</param>
        /// <param name="sourceCat">The filter <see cref="Category"/></param>
        /// <returns>The filtered <see cref="Thing"/></returns>
        private IEnumerable<DefinedThing> FilterSourcebyCategory(IReadOnlyList<DefinedThing> source, IReadOnlyList<Category> sourceCat)
        {
            var source2CatThing = new List<DefinedThing>();
            if (sourceCat == null || sourceCat.Count == 0)
            {
                return source;
            }

            foreach (ICategorizableThing thing in source)
            {
                if (thing.Category.Intersect(sourceCat).Any())
                {
                    source2CatThing.Add((DefinedThing)thing);
                }
            }

            return source2CatThing;
        }

        /// <summary>
        /// Queries the <see cref="BinaryRelationship"/> within the current context
        /// </summary>
        /// <param name="rule">The selected <see cref="BinaryRelationshipRule"/></param>
        /// <returns>The list of <see cref="BinaryRelationship"/></returns>
        private IEnumerable<BinaryRelationship> QueryBinaryRelationshipInContext(BinaryRelationshipRule rule)
        {
            return this.iteration.Relationship.OfType<BinaryRelationship>().Where(x => x.Category.Contains(rule.RelationshipCategory));
        }

        /// <summary>
        /// Compute the boolean that determines the available commands for a selected cell
        /// </summary>
        private void ComputeCommandCanExecute()
        {
            var vm = this.SelectedCell as MatrixCellViewModel;
            if (vm == null || vm.Source2 == null)
            {
                this.CanCreateSource1ToSource2 = false;
                this.CanCreateSource2ToSource1 = false;
                this.CanDelete = false;
                this.IsVisibleDelete1To2 = false;
                this.IsVisibleDelete2To1 = false;
                this.IsVisibleDeleteAll = false;
                return;
            }

            var canWrite = this.session.PermissionService.CanWrite(ClassKind.BinaryRelationship, this.iteration);

            this.CanCreateSource1ToSource2 = canWrite && vm.Source1.Iid != vm.Source2.Iid && vm.RelationshipDirection != RelationshipDirectionKind.RowThingToColumnThing && vm.RelationshipDirection != RelationshipDirectionKind.BiDirectional;
            this.CanCreateSource2ToSource1 = canWrite && vm.Source1.Iid != vm.Source2.Iid && vm.RelationshipDirection != RelationshipDirectionKind.ColumnThingToRowThing && vm.RelationshipDirection != RelationshipDirectionKind.BiDirectional;
            this.CanDelete = canWrite && vm.RelationshipDirection != RelationshipDirectionKind.None;

            this.IsVisibleDeleteAll = this.CanDelete && vm.RelationshipDirection == RelationshipDirectionKind.BiDirectional;
            this.IsVisibleDelete1To2 = this.IsVisibleDeleteAll || (this.CanDelete && vm.RelationshipDirection == RelationshipDirectionKind.RowThingToColumnThing);
            this.IsVisibleDelete2To1 = this.IsVisibleDeleteAll || (this.CanDelete && vm.RelationshipDirection == RelationshipDirectionKind.ColumnThingToRowThing);
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationship"/> for the selected cell
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

            var relationship = new BinaryRelationship(Guid.NewGuid(), null, null) { Owner = this.session.OpenIterations[this.iteration].Item1 };
            relationship.Category.Add(vm.Rule.RelationshipCategory);

            relationship.Source = direction == RelationshipDirectionKind.RowThingToColumnThing ? vm.Source1 : vm.Source2;
            relationship.Target = direction == RelationshipDirectionKind.RowThingToColumnThing ? vm.Source2 : vm.Source1;

            var iterationClone = this.iteration.Clone(false);
            iterationClone.Relationship.Add(relationship);

            var context = TransactionContextResolver.ResolveContext(relationship);
            var transaction = new ThingTransaction(context, iterationClone);
            transaction.Create(relationship, iterationClone);

            return this.session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Deletes a <see cref="BinaryRelationship"/> for the selected pair of thing
        /// </summary>
        /// <param name="direction">The direction of the relationship to delete</param>
        /// <returns>The task</returns>
        private Task DeleteRelationship(RelationshipDirectionKind direction)
        {
            var vm = this.SelectedCell as MatrixCellViewModel;
            if (vm == null)
            {
                return Task.FromResult(0);
            }

            var iterationClone = this.iteration.Clone(false);
            var context = TransactionContextResolver.ResolveContext(iterationClone);
            var transaction = new ThingTransaction(context, iterationClone);
            foreach (var binaryRelationship in vm.Relationships)
            {
                var clone = binaryRelationship.Clone(false);
                if (vm.RelationshipDirection != RelationshipDirectionKind.BiDirectional || direction == RelationshipDirectionKind.BiDirectional)
                {
                    // delete every relationship
                    transaction.Delete(clone);
                }
                else if (direction == RelationshipDirectionKind.RowThingToColumnThing && vm.Source1.Iid == binaryRelationship.Source.Iid)
                {
                    transaction.Delete(clone);
                }
                else if (direction == RelationshipDirectionKind.ColumnThingToRowThing && vm.Source2.Iid == binaryRelationship.Source.Iid)
                {
                    transaction.Delete(clone);
                }
            }

            return this.session.Write(transaction.FinalizeTransaction());
        }
    }
}
