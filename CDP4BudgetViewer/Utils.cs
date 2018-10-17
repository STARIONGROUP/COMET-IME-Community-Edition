// -------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    public static class  Utils
    {
        /// <summary>
        /// Gets the actual value associated to the <paramref name="pt"/> for a <see cref="ElementUsage"/> 
        /// </summary>
        /// <param name="usage">The <see cref="ElementUsage"/></param>
        /// <param name="pt">The <see cref="ParameterType"/></param>
        /// <param name="state">The state</param>
        /// <returns>The value</returns>
        public static string GetActualValue(this ElementUsage usage, ParameterType pt, ActualFiniteState state, Option option, DomainOfExpertise domain)
        {
            var parameterOrOverride = (ParameterOrOverrideBase)usage.ParameterOverride.FirstOrDefault(x => x.ParameterType.Iid == pt.Iid)
                                      ?? usage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.Iid == pt.Iid);

            if (parameterOrOverride == null)
            {
                // no value found associated to the parameter-type
                return null;
            }

            var parameterBase = (ParameterBase)parameterOrOverride.ParameterSubscription.FirstOrDefault(x => x.Owner.Iid == domain.Iid) ?? parameterOrOverride;
            var valueset = parameterBase.ValueSets.FirstOrDefault(x => (x.ActualOption == null || x.ActualOption.Iid == option.Iid) && (x.ActualState == null || x.ActualState.Iid == state.Iid));

            return valueset?.ActualValue.FirstOrDefault();
        }

        /// <summary>
        /// Gets the actual float value associated to the <paramref name="pt"/> for a <see cref="ElementUsage"/> 
        /// </summary>
        /// <param name="usage">The <see cref="ElementUsage"/></param>
        /// <param name="pt">The <see cref="QuantityKind"/></param>
        /// <param name="state">The state</param>
        /// <param name="option">The value associated to the option to get</param>
        /// <param name="domain">The current domain</param>
        /// <returns>The converted float value</returns>
        public static float? GetFloatActualValue(this ElementUsage usage, QuantityKind pt, ActualFiniteState state, Option option, DomainOfExpertise domain)
        {
            var value = GetActualValue(usage, pt, state, option, domain);
            float converted;
            if (float.TryParse(value, out converted))
            {
                return converted;
            }

            return null;
        }

        /// <summary>
        /// Gets the scale associated to the parameter of type <paramref name="pt"/>
        /// </summary>
        /// <param name="usage">The usage to get the scale from</param>
        /// <param name="pt">The parameter-type</param>
        /// <returns>The <see cref="MeasurementScale"/></returns>
        public static MeasurementScale GetScale(this ElementUsage usage, QuantityKind pt)
        {
            return usage.ElementDefinition.Parameter.FirstOrDefault(x => x.ParameterType.Iid == pt.Iid)?.Scale;
        }
    }
}
