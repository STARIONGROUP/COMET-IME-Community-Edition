// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementDropTarget.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using NLog;

    /// <summary>
    /// A specific class that implements <see cref="IDropTarget"/>.
    /// Typically used as a property on a class that cannot implement <see cref="IDropTarget"/> itself.
    /// </summary>
    public class RequirementDropTarget : IDropTarget
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IPermissionService"/> to be used for permission checks
        /// </summary>
        private readonly IPermissionService permissionService;

        /// <summary>
        /// The injected <see cref="IThingCreator"/>
        /// </summary>
        private readonly IThingCreator thingCreator = ServiceLocator.Current.GetInstance<IThingCreator>();

        /// <summary>
        /// The <see cref="Requirement"/>
        /// </summary>
        private readonly Requirement requirement;

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Creates a new instance of <see cref="RequirementDropTarget"/>
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        public RequirementDropTarget(Requirement requirement, ISession session)
        {
            this.requirement = requirement;
            this.session = session;
            this.permissionService = session.PermissionService;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Category category)
            {
                await this.CategoryDrop(dropInfo, category);
                dropInfo.Handled = true;
            }
        }

        /// <summary>
        /// Handle the drop of a <see cref="Category"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="category">The <see cref="Category"/></param>
        private async Task CategoryDrop(IDropInfo dropInfo, Category category)
        {
            if (this.requirement.Category.Any(x => x.Equals(category)))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (this.session.OpenIterations.TryGetValue(this.requirement.GetContainerOfType<Iteration>(), out var tuple))
            {
                await this.thingCreator.ApplyCategory(category, this.requirement, this.session);
            }
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
            if (dropInfo.Payload is Category category)
            {
                this.CategoryDragOver(dropInfo, category);
            }
        }

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="Category"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="category">The <see cref="Category"/> in the payload</param>
        private void CategoryDragOver(IDropInfo dropinfo, Category category)
        {
            // check if category is in the chain of rdls
            var model = (EngineeringModel)this.requirement.TopContainer;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var rdlChains = new List<ReferenceDataLibrary> { mrdl };
            rdlChains.AddRange(mrdl.RequiredRdls);

            if (!rdlChains.Contains(category.Container))
            {
                dropinfo.Effects = DragDropEffects.None;

                Logger.Warn("A category cannot be applied as it is not available in the current set of available reference data libraries.");
                return;
            }

            if (!this.permissionService.CanWrite(this.requirement))
            {
                dropinfo.Effects = DragDropEffects.None;
                Logger.Warn("You do not have permission to apply the category.");
                return;
            }

            // A category is already applied
            if (this.requirement.Category.Any(x => x.Equals(category)))
            {
                Logger.Warn("The category is already applied.");
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            // A category's permissable classes check
            if (!category.PermissibleClass.Contains(ClassKind.Requirement))
            {
                Logger.Warn("The category cannot be applied to this type.");
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            dropinfo.Effects = DragDropEffects.Copy;
            dropinfo.Handled = true;
        }
    }
}
