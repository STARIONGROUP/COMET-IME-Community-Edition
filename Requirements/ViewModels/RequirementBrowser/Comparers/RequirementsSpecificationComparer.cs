// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationComparer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// The <see cref="IComparer{T}"/> used to sort the child rows of the <see cref="RequirementsSpecification"/>
    /// </summary>
    public class RequirementsSpecificationComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The <see cref="DefinedThingComparer"/>
        /// </summary>
        private static readonly ShortNameThingComparer shortNameThingComparer = new ShortNameThingComparer();

        /// <summary>
        /// Compares two <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="x">The first <see cref="RequirementsSpecification"/> to compare</param>
        /// <param name="y">The second <see cref="RequirementsSpecification"/> to compare</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        public int Compare(IRowViewModelBase<Thing> x, IRowViewModelBase<Thing> y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            var xspec = x.Thing as RequirementsSpecification;
            var yspec = y.Thing as RequirementsSpecification;

            if (xspec == null || yspec == null)
            {
                return 0;
            }

            if (RequirementsModule.PluginSettings?.OrderSettings != null && RequirementsModule.PluginSettings.OrderSettings.ParameterType != Guid.Empty)
            {
                var xOrder = xspec.ParameterValue.FirstOrDefault(z => z.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType)?.Value.FirstOrDefault();
                var yOrder = yspec.ParameterValue.FirstOrDefault(z => z.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType)?.Value.FirstOrDefault();

                int xOrderKey, yOrderKey;
                if (xOrder != null && int.TryParse(xOrder, out xOrderKey) && yOrder != null && int.TryParse(yOrder, out yOrderKey))
                {
                    return xOrderKey > yOrderKey 
                        ? 1
                        : xOrderKey < yOrderKey 
                            ? -1
                            : shortNameThingComparer.Compare(xspec, yspec);
                }

                return shortNameThingComparer.Compare(xspec, yspec);
            }

            return shortNameThingComparer.Compare(xspec, yspec);
        }
    }
}