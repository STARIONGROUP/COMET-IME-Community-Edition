// ------------------------------------------------------------------------------------------------
// <copyright file="WritableRequirementStateOfComplianceMethods.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

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
        public static void SetRequirementStateOfComplianceChangedEventSubscription(this IHaveWritableRequirementStateOfCompliance requirementStateOfComplianceObject, Thing thing, ICollection<IDisposable> disposables)
        {
            var requirementVerifierListener = CDPMessageBus.Current.Listen<RequirementStateOfComplianceChangedEvent>(thing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SetRequirementStateOfCompliance(requirementStateOfComplianceObject, x));

            disposables?.Add(requirementVerifierListener);

            if (thing is RelationalExpression)
            {
                var resetRequirementVerifierListener = CDPMessageBus.Current.Listen<RequirementStateOfComplianceChangedEvent>(typeof(ParameterOrOverrideBase))
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
            if (requirementStateOfComplianceObject.RequirementStateOfCompliance == RequirementStateOfCompliance.Calculating)
            {
                if (requirementStateOfComplianceChangedEvent.RequirementStateOfCompliance != RequirementStateOfCompliance.Calculating)
                {
                    Task.Delay(500).ContinueWith(_ => requirementStateOfComplianceObject.RequirementStateOfCompliance = requirementStateOfComplianceChangedEvent.RequirementStateOfCompliance);

                    return;
                }
            }

            requirementStateOfComplianceObject.RequirementStateOfCompliance = requirementStateOfComplianceChangedEvent.RequirementStateOfCompliance;
        }
    }
}
