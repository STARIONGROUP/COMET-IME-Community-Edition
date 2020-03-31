using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4DiagramEditor.ViewModels.Relation
{
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    public class RuleNavBarRelationViewModel : RuleRowViewModel<Rule>
    { /// <summary>
      /// Initializes a new instance of the <see cref="RuleNavBarItemViewModel"/> class.
      /// </summary>
      /// <param name="rule">The rule referenced by this row</param>
      /// <param name="session">The session</param>
      /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public RuleNavBarRelationViewModel(Rule rule, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(rule, session, containerViewModel)
        {
            this.InitializeCommands();
        }


        /// <summary>
        /// Gets the public command to create a relationship.
        /// </summary>
        public ReactiveCommand<object> CreateRelationshipCommand { get; private set; }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            var canCreateRelationshipBasedOnRule = this.Thing is BinaryRelationshipRule ? Observable.Return(true) :
                ((DiagramEditorViewModel)this.ContainerViewModel).SelectedItems.Changed.Select(_ => this.CanCreateRelationship());

            this.CreateRelationshipCommand = ReactiveCommand.Create(canCreateRelationshipBasedOnRule);
            this.CreateRelationshipCommand.Subscribe(_ => this.ExecuteCreateRelationshipCommand());
        }

        /// <summary>
        /// Compute whether a <see cref="Relationship"/> can be created.
        /// This is true only when two <see cref="NamedThingDiagramContentItem"/> with <see cref="Thing"/>s are selected or
        /// this Rule describes a <see cref="BinaryRelationshipRule"/>.
        /// </summary>
        /// <returns>True when two <see cref="NamedThingDiagramContentItem"/> with <see cref="Thing"/>s are selected. </returns>
        private bool CanCreateRelationship()
        {
            // Multi relationships require more than one item to be selected.
            if (((DiagramEditorViewModel)this.ContainerViewModel).SelectedItems.Count <= 1)
            {
                return false;
            }

            return ((DiagramEditorViewModel)this.ContainerViewModel).SelectedItems.All(x => x is NamedThingDiagramContentItem);
        }

        /// <summary>
        /// Executes the <see cref="CreateRelationshipCommand"/>
        /// </summary>
        private void ExecuteCreateRelationshipCommand()
        {
            var diagramVm = this.ContainerViewModel as DiagramEditorViewModel;

            if (diagramVm == null)
            {
                throw new NullReferenceException("The Relationship Editor view mdoel is not set");
            }

            if (this.Thing is BinaryRelationshipRule)
            {

                //diagramVm.PendingBinaryRelationshipRule = this.Thing as BinaryRelationshipRule;
                //diagramVm.CreateBinaryRelationshipCommandExecute();
                return;
            }

            if (this.Thing is MultiRelationshipRule)
            {
                //diagramVm.CreateMultiRelationshipCommandExecute();
                return;
            }
        }

    }
}
