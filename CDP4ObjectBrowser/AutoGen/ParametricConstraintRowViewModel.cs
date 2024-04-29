// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="ParametricConstraint"/>
    /// </summary>
    public partial class ParametricConstraintRowViewModel : ObjectBrowserRowViewModel<ParametricConstraint>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ExpressionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel expressionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintRowViewModel"/> class
        /// </summary>
        /// <param name="parametricConstraint">The <see cref="ParametricConstraint"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public ParametricConstraintRowViewModel(ParametricConstraint parametricConstraint, ISession session, IViewModelBase<Thing> containerViewModel) : base(parametricConstraint, session, containerViewModel)
        {
            this.expressionFolder = new CDP4Composition.FolderRowViewModel("Expression", "Expression", this.Session, this);
            this.ContainedRows.Add(this.expressionFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.Expression, this.expressionFolder, this.AddExpressionRowViewModel);
        }
        /// <summary>
        /// Add an Expression row view model to the list of <see cref="BooleanExpression"/>
        /// </summary>
        /// <param name="expression">
        /// The <see cref="Expression"/> that is to be added
        /// </param>
        private IBooleanExpressionRowViewModel<BooleanExpression> AddExpressionRowViewModel(BooleanExpression expression)
        {
        var orExpression = expression as OrExpression;
        if (orExpression != null)
        {
            return new OrExpressionRowViewModel(orExpression, this.Session, this);
        }
        var notExpression = expression as NotExpression;
        if (notExpression != null)
        {
            return new NotExpressionRowViewModel(notExpression, this.Session, this);
        }
        var andExpression = expression as AndExpression;
        if (andExpression != null)
        {
            return new AndExpressionRowViewModel(andExpression, this.Session, this);
        }
        var exclusiveOrExpression = expression as ExclusiveOrExpression;
        if (exclusiveOrExpression != null)
        {
            return new ExclusiveOrExpressionRowViewModel(exclusiveOrExpression, this.Session, this);
        }
        var relationalExpression = expression as RelationalExpression;
        if (relationalExpression != null)
        {
            return new RelationalExpressionRowViewModel(relationalExpression, this.Session, this);
        }
        throw new Exception("No BooleanExpression to return");
        }
    }
}
