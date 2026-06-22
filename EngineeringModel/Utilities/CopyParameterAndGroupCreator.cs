// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyParameterAndGroupCreator.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The class responsible for copying a <see cref="Parameter"/> or a <see cref="ParameterGroup"/> (including the
    /// <see cref="Parameter"/>s and nested <see cref="ParameterGroup"/>s it contains) into a target <see cref="ElementDefinition"/>.
    /// The target may live in a different <see cref="Iteration"/> or <see cref="EngineeringModel"/> than the source as long as
    /// both are open in the same <see cref="ISession"/>.
    /// </summary>
    /// <remarks>
    /// Only the definition of the <see cref="Parameter"/>s is copied; their values are reset to the default value. A
    /// <see cref="Parameter"/> is skipped when its <see cref="ParameterType"/> already exists on the target
    /// <see cref="ElementDefinition"/>, or when the <see cref="ParameterType"/> (or <see cref="MeasurementScale"/>) it references
    /// is not available in the chain of <see cref="ReferenceDataLibrary"/> of the target <see cref="EngineeringModel"/>.
    /// </remarks>
    internal class CopyParameterAndGroupCreator
    {
        /// <summary>
        /// The <see cref="ISession"/> in which the copy is performed
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> that becomes the owner of the copied <see cref="Parameter"/>s
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// The clone of the target <see cref="ElementDefinition"/> that the copied <see cref="Thing"/>s are added to
        /// </summary>
        private ElementDefinition targetElementDefinitionClone;

        /// <summary>
        /// The target <see cref="Iteration"/> that the copy is performed into
        /// </summary>
        private Iteration targetIteration;

        /// <summary>
        /// The <see cref="ReferenceDataLibrary"/> available in the chain of the target <see cref="EngineeringModel"/>
        /// </summary>
        private HashSet<ReferenceDataLibrary> availableRdls;

        /// <summary>
        /// The <see cref="Guid"/>s of the <see cref="ParameterType"/>s already present on the target <see cref="ElementDefinition"/>
        /// </summary>
        private HashSet<Guid> existingParameterTypeIids;

        /// <summary>
        /// The <see cref="ThingTransaction"/> the new <see cref="Thing"/>s are registered on
        /// </summary>
        private ThingTransaction transaction;

        /// <summary>
        /// The human-readable messages describing <see cref="Parameter"/>s that were not copied
        /// </summary>
        private List<string> warnings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyParameterAndGroupCreator"/> class
        /// </summary>
        /// <param name="session">The associated <see cref="ISession"/></param>
        public CopyParameterAndGroupCreator(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Perform the copy operation of a <see cref="Parameter"/> or a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="source">
        /// The <see cref="Parameter"/> or <see cref="ParameterGroup"/> that is to be copied
        /// </param>
        /// <param name="targetElementDefinition">
        /// The <see cref="ElementDefinition"/> that the <paramref name="source"/> is to be copied into
        /// </param>
        /// <param name="targetGroup">
        /// The optional <see cref="ParameterGroup"/> of the <paramref name="targetElementDefinition"/> that the copied
        /// <see cref="Parameter"/> or top-level <see cref="ParameterGroup"/> is to be placed in. May be null to place the
        /// copy at the root of the <paramref name="targetElementDefinition"/>.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that becomes the owner of the copied <see cref="Parameter"/>s. This is the
        /// <see cref="DomainOfExpertise"/> of the user performing the paste, not the owner of the original <see cref="Parameter"/>s.
        /// </param>
        /// <returns>
        /// An awaitable <see cref="Task"/> whose result is the list of human-readable messages describing the
        /// <see cref="Parameter"/>s that could not be copied because their referenced reference data is not available in the
        /// target <see cref="EngineeringModel"/>.
        /// </returns>
        public async Task<IReadOnlyList<string>> Copy(Thing source, ElementDefinition targetElementDefinition, ParameterGroup targetGroup, DomainOfExpertise owner)
        {
            if (targetElementDefinition == null)
            {
                throw new ArgumentNullException(nameof(targetElementDefinition), "The target ElementDefinition may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owner DomainOfExpertise may not be null");
            }

            this.owner = owner;
            this.targetElementDefinitionClone = targetElementDefinition.Clone(false);
            this.targetIteration = targetElementDefinition.GetContainerOfType<Iteration>();
            this.availableRdls = this.ComputeAvailableRdls(targetElementDefinition);
            this.existingParameterTypeIids = new HashSet<Guid>(targetElementDefinition.Parameter.Select(p => p.ParameterType.Iid));
            this.warnings = new List<string>();

            var transactionContext = TransactionContextResolver.ResolveContext(targetElementDefinition);
            this.transaction = new ThingTransaction(transactionContext, this.targetElementDefinitionClone);

            bool hasChanges;

            switch (source)
            {
                case ParameterGroup parameterGroup:
                    hasChanges = this.CopyParameterGroup(parameterGroup, targetGroup);
                    break;
                case Parameter parameter:
                    hasChanges = this.CopyParameter(parameter, targetGroup);
                    break;
                default:
                    throw new ArgumentException("Only a Parameter or a ParameterGroup can be copied", nameof(source));
            }

            if (hasChanges)
            {
                await this.session.Write(this.transaction.FinalizeTransaction());
            }

            return this.warnings;
        }

        /// <summary>
        /// Copies a <see cref="ParameterGroup"/> and its contained <see cref="ParameterGroup"/>s and <see cref="Parameter"/>s
        /// </summary>
        /// <param name="source">The <see cref="ParameterGroup"/> to copy</param>
        /// <param name="targetGroup">The optional <see cref="ParameterGroup"/> to place the copied top-level group in</param>
        /// <returns>A value indicating whether anything was registered on the transaction</returns>
        private bool CopyParameterGroup(ParameterGroup source, ParameterGroup targetGroup)
        {
            var sourceGroups = new List<ParameterGroup> { source };
            sourceGroups.AddRange(source.ContainedGroup(true));

            // register the original-clone mapping so that nested containing-group references can be resolved
            var groupMap = new Dictionary<ParameterGroup, ParameterGroup>();

            foreach (var sourceGroup in sourceGroups)
            {
                var groupClone = new ParameterGroup(Guid.NewGuid(), null, null)
                {
                    Name = sourceGroup.Name
                };

                groupMap.Add(sourceGroup, groupClone);
            }

            foreach (var kvp in groupMap)
            {
                var sourceGroup = kvp.Key;
                var groupClone = kvp.Value;

                groupClone.ContainingGroup = sourceGroup.ContainingGroup != null && groupMap.TryGetValue(sourceGroup.ContainingGroup, out var mappedContainingGroup)
                    ? mappedContainingGroup
                    : targetGroup;

                this.targetElementDefinitionClone.ParameterGroup.Add(groupClone);
                this.transaction.Create(groupClone, this.targetElementDefinitionClone);
            }

            foreach (var sourceGroup in sourceGroups)
            {
                foreach (var parameter in sourceGroup.ContainedParameter())
                {
                    this.CopyParameter(parameter, groupMap[sourceGroup]);
                }
            }

            // a copied group is always created, even if all of its parameters already exist on the target
            return true;
        }

        /// <summary>
        /// Copies a single <see cref="Parameter"/> when its <see cref="ParameterType"/> is not yet present on the target and
        /// the reference data it depends on is available in the target <see cref="EngineeringModel"/>.
        /// </summary>
        /// <param name="source">The <see cref="Parameter"/> to copy</param>
        /// <param name="targetGroup">The optional <see cref="ParameterGroup"/> to place the copied <see cref="Parameter"/> in</param>
        /// <returns>A value indicating whether a <see cref="Parameter"/> was registered on the transaction</returns>
        private bool CopyParameter(Parameter source, ParameterGroup targetGroup)
        {
            if (this.existingParameterTypeIids.Contains(source.ParameterType.Iid))
            {
                // a Parameter with this ParameterType already exists on the target ElementDefinition
                return false;
            }

            if (!source.RequiredRdls.All(rdl => this.availableRdls.Contains(rdl)))
            {
                // the ParameterType or MeasurementScale lives in a reference data library that is not available in the target model
                this.warnings.Add($"The parameter '{source.ParameterType.Name}' was not copied because its reference data is not available in the target model.");
                return false;
            }

            this.existingParameterTypeIids.Add(source.ParameterType.Iid);

            // a Parameter is state-dependent on an ActualFiniteStateList of its own Iteration; that reference is dropped when
            // pasting into a different Iteration that does not contain it.
            var stateDependence = source.StateDependence != null && this.targetIteration != null && this.targetIteration.ActualFiniteStateList.Contains(source.StateDependence)
                ? source.StateDependence
                : null;

            var parameterClone = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = source.ParameterType,
                Scale = source.Scale,
                Owner = this.owner,
                IsOptionDependent = source.IsOptionDependent,
                StateDependence = stateDependence,
                ExpectsOverride = source.ExpectsOverride,
                AllowDifferentOwnerOfOverride = source.AllowDifferentOwnerOfOverride,
                Group = targetGroup
            };

            this.targetElementDefinitionClone.Parameter.Add(parameterClone);
            this.transaction.Create(parameterClone, this.targetElementDefinitionClone);

            return true;
        }

        /// <summary>
        /// Computes the <see cref="ReferenceDataLibrary"/> available in the chain of the <see cref="EngineeringModel"/> that
        /// contains the <paramref name="targetElementDefinition"/>.
        /// </summary>
        /// <param name="targetElementDefinition">The target <see cref="ElementDefinition"/></param>
        /// <returns>The set of available <see cref="ReferenceDataLibrary"/></returns>
        private HashSet<ReferenceDataLibrary> ComputeAvailableRdls(ElementDefinition targetElementDefinition)
        {
            var availableReferenceDataLibraries = new HashSet<ReferenceDataLibrary>();

            if (targetElementDefinition.TopContainer is EngineeringModel model)
            {
                var requiredRdl = model.EngineeringModelSetup.RequiredRdl.Single();
                availableReferenceDataLibraries.Add(requiredRdl);

                foreach (var referenceDataLibrary in requiredRdl.RequiredRdls)
                {
                    availableReferenceDataLibraries.Add(referenceDataLibrary);
                }
            }

            return availableReferenceDataLibraries;
        }
    }
}
