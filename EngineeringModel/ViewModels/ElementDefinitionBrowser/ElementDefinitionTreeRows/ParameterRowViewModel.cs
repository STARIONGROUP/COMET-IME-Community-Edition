// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// The parameter row view model.
    /// </summary>
    public class ParameterRowViewModel : ParameterOrOverrideBaseRowViewModel, IDropTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRowViewModel"/> class.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container row.
        /// </param>
        /// <param name="isReadOnly">
        /// A value indicating whether this row shall be made read-only in the current context.
        /// </param>
        public ParameterRowViewModel(Parameter parameter, ISession session, IViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameter, session, containerViewModel, isReadOnly)
        {
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
            // moving the paramenter into a group of the same element definition
            if (dropInfo.Payload is Parameter parameter)
            {
                this.DragOver(dropInfo, parameter);

                return;
            }

            // moving the group into a group of the same element definition
            if (dropInfo.Payload is ParameterGroup group && (group.Container == this.Thing.Container))
            {
                this.DragOver(dropInfo, group);

                return;
            }

            if (dropInfo.Payload is RelationalExpression expression && (expression.ParameterType.Iid == this.Thing?.ParameterType.Iid))
            {
                this.DragOver(dropInfo, expression);

                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            // moving 
            if (dropInfo.Payload is Parameter parameter)
            {
                await this.Drop(dropInfo, parameter);
            }

            // moving the group into this parameter's group
            if (dropInfo.Payload is ParameterGroup group && (dropInfo.Effects == DragDropEffects.Move))
            {
                await this.Drop(dropInfo, group);
            }

            if (dropInfo.Payload is RelationalExpression expression && (expression.ParameterType.Iid == this.Thing?.ParameterType.Iid))
            {
                await this.Drop(dropInfo, expression);
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Update the drag state when the payload is a <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="parameter">The <see cref="Parameter"/> payload</param>
        private void DragOver(IDropInfo dropInfo, Parameter parameter)
        {
            if (parameter.Container == this.Thing.Container)
            {
                if (!this.PermissionService.CanWrite(parameter))
                {
                    dropInfo.Effects = DragDropEffects.None;

                    return;
                }

                dropInfo.Effects = DragDropEffects.Move;

                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Update the drag state when the payload is a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="group">The <see cref="ParameterGroup"/> payload</param>
        private void DragOver(IDropInfo dropInfo, ParameterGroup group)
        {
            if (group.Container == this.Thing.Container)
            {
                if (this.Thing.Group == null)
                {
                    dropInfo.Effects = DragDropEffects.Move;

                    return;
                }

                var containedGroups = group.ContainedGroup(true).ToList();

                if ((@group != this.Thing.Group) && !containedGroups.Contains(this.Thing.Group))
                {
                    if (!this.PermissionService.CanWrite(group))
                    {
                        dropInfo.Effects = DragDropEffects.None;

                        return;
                    }

                    dropInfo.Effects = DragDropEffects.Move;

                    return;
                }
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
        /// Performs the drop operation when the payload is a <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        /// <param name="parameter">
        /// The <see cref="Parameter"/> payload
        /// </param>
        private async Task Drop(IDropInfo dropInfo, Parameter parameter)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var clone = parameter.Clone(false);
                clone.Group = this.Thing.Group;
                await this.DalWrite(clone);
            }
        }

        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        /// <param name="group">
        /// The <see cref="ParameterGroup"/> payload
        /// </param>
        private async Task Drop(IDropInfo dropInfo, ParameterGroup group)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var clone = group.Clone(false);
                clone.ContainingGroup = this.Thing.Group;
                await this.DalWrite(clone);
            }
        }

        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="RelationalExpression"/>
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        /// <param name="expression">
        /// The <see cref="RelationalExpression"/> payload
        /// </param>
        private async Task Drop(IDropInfo dropInfo, RelationalExpression expression)
        {
            if (expression.ParameterType.Iid == this.Thing?.ParameterType.Iid)
            {
                MessageBox.Show("That hurts man!", "Ow", MessageBoxButton.OK);
            }
        }
    }
}
