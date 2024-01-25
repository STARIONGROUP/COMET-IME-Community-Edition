// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WritableRequirementStateOfComplianceExtensions.cs" company="RHEA System S.A.">
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

namespace CDP4Requirements.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Requirements.ViewModels.RequirementBrowser;

    using CDP4RequirementsVerification;
    using CDP4RequirementsVerification.Events;

    using ReactiveUI;

    /// <summary>
    /// This class contains methods for specific <see cref="IHaveWritableRequirementStateOfCompliance"/> related functionality 
    /// </summary>
    public static class WritableRequirementStateOfComplianceExtensions
    {
        /// <summary>
        /// Creates a reactive subscriptions for a <see cref="IHaveWritableRequirementStateOfCompliance"/> object
        /// </summary>
        /// <param name="requirementStateOfComplianceObject">The <see cref="IHaveWritableRequirementStateOfCompliance"/></param>
        /// <param name="thing">The <see cref="Thing"/> that te subscription is set for</param>
        /// <param name="disposables">The <see cref="ICollection{IDisposable}"/> where the subscription must be added to to avoid memory leaks</param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public static void SetRequirementStateOfComplianceChangedEventSubscription(this IHaveWritableRequirementStateOfCompliance requirementStateOfComplianceObject, Thing thing, ICollection<IDisposable> disposables, ICDPMessageBus messageBus)
        {
            var requirementVerifierListener = messageBus.Listen<RequirementStateOfComplianceChangedEvent>(thing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SetRequirementStateOfCompliance(requirementStateOfComplianceObject, x));

            disposables?.Add(requirementVerifierListener);

            if (thing is RelationalExpression)
            {
                var resetRequirementVerifierListener = messageBus.Listen<RequirementStateOfComplianceChangedEvent>(typeof(ParameterOrOverrideBase))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => { ResetRequirementStateOfComplianceTree(requirementStateOfComplianceObject); });

                disposables?.Add(resetRequirementVerifierListener);
            }
        }

        /// <summary>
        /// Reset the <see cref="RequirementStateOfCompliance"/> tree of ViewModels that implement <see cref="IHaveWritableRequirementStateOfCompliance"/> and <see cref="IHaveContainerViewModel"/> to <see cref="RequirementStateOfCompliance.Unknown"/>
        /// </summary>
        /// <param name="requirementStateOfComplianceObject">The <see cref="IHaveWritableRequirementStateOfCompliance"/> implementation</param>
        private static void ResetRequirementStateOfComplianceTree(IHaveWritableRequirementStateOfCompliance requirementStateOfComplianceObject)
        {
            if (requirementStateOfComplianceObject is IHaveContainerViewModel containerViewModel)
            {
                containerViewModel.ResetRequirementStateOfComplianceTree();
            }
        }

        /// <summary>
        /// Actually sets the <see cref="IHaveWritableRequirementStateOfCompliance.RequirementStateOfCompliance"/> property.
        /// </summary>
        /// <param name="requirementStateOfComplianceObject">The <see cref="IHaveWritableRequirementStateOfCompliance"/> implementation</param>
        /// <param name="requirementStateOfComplianceChangedEvent">The <see cref="RequirementStateOfComplianceChangedEvent"/></param>
        private static void SetRequirementStateOfCompliance(IHaveWritableRequirementStateOfCompliance requirementStateOfComplianceObject, RequirementStateOfComplianceChangedEvent requirementStateOfComplianceChangedEvent)
        {
            requirementStateOfComplianceObject.RequirementStateOfCompliance = requirementStateOfComplianceChangedEvent.RequirementStateOfCompliance;
        }
    }
}
