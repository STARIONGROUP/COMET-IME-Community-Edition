// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;

    /// <summary>
    /// row-view-model that represent a <see cref="Parameter"/> in the product tree
    /// </summary>
    public class ParameterRowViewModel : ParameterOrOverrideBaseRowViewModel, IDropTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRowViewModel"/> class
        /// </summary>
        /// <param name="parameter">The associated <see cref="Parameter"/></param>
        /// <param name="option">The actual <see cref="Option"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">the container view-model that contains this row</param>
        public ParameterRowViewModel(Parameter parameter, Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameter, option, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSetBase"/> for an <see cref="Option"/> (if this <see cref="ParameterOrOverrideBase"/> is option dependent) and a <see cref="ActualFiniteState"/> (if it is state dependent)
        /// </summary>
        /// <param name="actualState">The <see cref="ActualFiniteState"/></param>
        /// <param name="actualOption">The <see cref="Option"/></param>
        /// <returns>The <see cref="ParameterValueSetBase"/> if a value is defined for the <see cref="Option"/></returns>
        protected override ParameterValueSetBase GetValueSet(ActualFiniteState actualState = null, Option actualOption = null)
        {
            var isStateDependent = this.StateDependence != null;
            var parameter = (Parameter)this.Thing;
            var valueset = parameter.ValueSet.SingleOrDefault(x => (!isStateDependent || (x.ActualState == actualState)) && (!this.IsOptionDependent || (x.ActualOption == actualOption)));

            return valueset;
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
        public override void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.All;
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is RelationalExpression expression && this.ThingCreator.IsCreateBinaryRelationshipForRequirementVerificationAllowed(this.Thing, expression))
            {
                this.DragOver(dropInfo, expression);

                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Update the drag state when the payload is a <see cref="RelationalExpression"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="expression">The <see cref="RelationalExpression"/> payload</param>
        private void DragOver(IDropInfo dropInfo, RelationalExpression expression)
        {
            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is RelationalExpression expression && this.ThingCreator.IsCreateBinaryRelationshipForRequirementVerificationAllowed(this.Thing, expression))
            {
                await this.Drop(this.Thing, expression);
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="RelationalExpression"/>
        /// </summary>
        /// <param name="parameter">
        /// The <see cref="ParameterOrOverrideBase"/>
        /// </param>
        /// <param name="expression">
        /// The <see cref="RelationalExpression"/>
        /// </param>
        private async Task Drop(ParameterOrOverrideBase parameter, RelationalExpression expression)
        {
            await this.CreateBinaryRelationship(parameter, expression);
        }
    }
}
