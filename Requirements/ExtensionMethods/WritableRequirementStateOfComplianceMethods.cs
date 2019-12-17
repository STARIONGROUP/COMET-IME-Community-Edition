// ------------------------------------------------------------------------------------------------
// <copyright file="WritableRequirementStateOfComplianceMethods.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;

    using CDP4Dal;

    using CDP4Requirements.Events;
    using CDP4Requirements.ViewModels.RequirementBrowser;

    using ReactiveUI;

    /// <summary>
    /// This class contains methods for specific <see cref="IHaveWritableRequirementStateOfCompliance"/> related functionality 
    /// </summary>
    public static class WritableRequirementStateOfComplianceMethods
    {
        /// <summary>
        /// Creates a reactive subscription for a <see cref="IHaveWritableRequirementStateOfCompliance"/> object
        /// </summary>
        /// <param name="requirementStateOfComplianceObject">The <see cref="IHaveWritableRequirementStateOfCompliance"/></param>
        /// <param name="thing">The <see cref="Thing"/> that te subscription is set for</param>
        /// <param name="disposables">The <see cref="ICollection{IDisposable}"/> where the subscription must be added to to avoid memory leaks</param>
        public static void SetRequirementStateOfComplianceChangedEventSubscription(this IHaveWritableRequirementStateOfCompliance requirementStateOfComplianceObject, Thing thing, ICollection<IDisposable> disposables)
        {
            var requirementVerifierListener = CDPMessageBus.Current.Listen<RequirementStateOfComplianceChangedEvent>(thing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    x =>
                    {
                        if (requirementStateOfComplianceObject.RequirementStateOfCompliance == RequirementStateOfCompliance.Calculating)
                        {
                            if (x.RequirementStateOfCompliance != RequirementStateOfCompliance.Calculating)
                            {
                                Task.Delay(500).ContinueWith(_ => requirementStateOfComplianceObject.RequirementStateOfCompliance = x.RequirementStateOfCompliance);

                                return;
                            }
                        }

                        requirementStateOfComplianceObject.RequirementStateOfCompliance = x.RequirementStateOfCompliance;
                    });

            disposables?.Add(requirementVerifierListener);
        }
    }
}
