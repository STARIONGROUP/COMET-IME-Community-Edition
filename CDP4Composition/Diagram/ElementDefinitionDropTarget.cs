// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDropTarget.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
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

namespace CDP4Composition.Diagram
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

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
    public class ElementDefinitionDropTarget : IDropTarget
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
        /// The <see cref="ElementDefinition"/>
        /// </summary>
        private readonly ElementDefinition elementDefinition;

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Creates a new instance of <see cref="ElementDefinitionDropTarget"/>
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        public ElementDefinitionDropTarget(ElementDefinition elementDefinition, ISession session)
        {
            this.elementDefinition = elementDefinition;
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
            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
            {
                await this.ParameterDrop(dropInfo, parameterTypeAndScale);
            }
        }

        /// <summary>
        /// Handle the drop of a <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="parameterTypeAndScale">The <see cref="Tuple{ParameterType, MeasurementScale}"/></param>
        private async Task ParameterDrop(IDropInfo dropInfo, Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
        {
            var parameterType = parameterTypeAndScale.Item1;
            var measeurementScale = parameterTypeAndScale.Item2;

            if (this.elementDefinition.Parameter.Any(x => x.ParameterType == parameterType))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (this.session.OpenIterations.TryGetValue(this.elementDefinition.GetContainerOfType<Iteration>(), out var tuple))
            {
                await this.thingCreator.CreateParameter(this.elementDefinition, null, parameterType, measeurementScale, tuple?.Item1, this.session);
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
            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
            {
                this.ParameterDragOver(dropInfo, parameterTypeAndScale);
            }
        }

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="tuple">The <see cref="Tuple{ParameterType, MeasurementScale}"/> in the payload</param>
        private void ParameterDragOver(IDropInfo dropinfo, Tuple<ParameterType, MeasurementScale> tuple)
        {
            // check if parameter type is in the chain of rdls
            var model = (EngineeringModel)this.elementDefinition.TopContainer;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var rdlChains = new List<ReferenceDataLibrary> { mrdl };
            rdlChains.AddRange(mrdl.RequiredRdls);

            if (!rdlChains.Contains(tuple.Item1.Container))
            {
                dropinfo.Effects = DragDropEffects.None;
                Logger.Warn("A parameter with the current parameter type cannot be created as the parameter type does not belong to the available libraries.");
                return;
            }

            if (!this.permissionService.CanWrite(CDP4Common.CommonData.ClassKind.Parameter, this.elementDefinition))
            {
                dropinfo.Effects = DragDropEffects.None;
                Logger.Warn("You do not have permission to create a parameter in this element definition.");
                return;
            }

            var parameterType = tuple.Item1;

            // A parameter that references the drag-over ParameterType already exists
            if (this.elementDefinition.Parameter.Any(x => x.ParameterType == parameterType))
            {
                Logger.Warn("A parameter with the current parameter type already exists.");
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            dropinfo.Effects = DragDropEffects.Copy;
            dropinfo.Handled = true;
        }
    }
}
