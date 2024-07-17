// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProperyHelper.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.RowModels
{
    using CDP4Common.EngineeringModelData;

    public static class PropertyHelper
    {
        /// <summary>
        /// Computes the shortname of the container <see cref="Parameter"/> or <see cref="ParameterOverride"/>
        /// </summary>
        /// <param name="parameterSubscription">
        /// The <see cref="ParameterSubscription"/> from which the ownership short-name is computed
        /// </param>
        /// <returns>
        /// The shortname of the owner
        /// </returns>
        public static string ComputeContainerOwnerShortName(ParameterSubscription parameterSubscription)
        {
            var parameterOrOverrideBase = (ParameterOrOverrideBase)parameterSubscription.Container;
            return parameterOrOverrideBase.Owner.ShortName;
        }

        /// <summary>
        /// Computes the shortname of the container <see cref="Parameter"/> or <see cref="ParameterOverride"/>
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> from which the ownership short-name is computed
        /// </param>
        /// <returns>
        /// The shortname of the owner
        /// </returns>
        public static string ComputeContainerOwnerShortName(ParameterSubscriptionValueSet parameterSubscriptionValueSet)
        {
            var parameterSubscription = (ParameterSubscription)parameterSubscriptionValueSet.Container;
            return ComputeContainerOwnerShortName(parameterSubscription);
        }
    }
}
