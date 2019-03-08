// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementContainerChildRowComparer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Requirements.ViewModels;

    /// <summary>
    /// The <see cref="IComparer{T}"/> used to sort the child rows of the <see cref="RequirementContainerRowViewModel{T}"/>
    /// </summary>
    public class RequirementContainerChildRowComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The <see cref="DefinedThingComparer"/>
        /// </summary>
        private static ShortNameThingComparer shortNameThingComparer = new ShortNameThingComparer();

        /// <summary>
        /// The Permissible Kind of child <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        private static readonly List<Type> PermissibleRowTypes = new List<Type>
        {
            typeof(RequirementsGroupRowViewModel),
            typeof(RequirementRowViewModel),
            typeof(FolderRowViewModel)
        };

        /// <summary>
        /// Compares two <see cref="IRowViewModelBase{Thing}"/>
        /// </summary>
        /// <param name="x">The first <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <param name="y">The second <see cref="IRowViewModelBase{Thing}"/> to compare</param>
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

            var xType = x.GetType();
            var yType = y.GetType();

            if (!PermissibleRowTypes.Any(type => type.IsAssignableFrom(xType)) || !PermissibleRowTypes.Any(type => type.IsAssignableFrom(yType)))
            {
                throw new NotSupportedException("The list contains other types of row than the specified ones.");
            }

            if (typeof(RequirementRowViewModel).IsAssignableFrom(xType) && typeof(RequirementsGroupRowViewModel).IsAssignableFrom(yType))
            {
                return -1;
            }

            if (typeof(RequirementsGroupRowViewModel).IsAssignableFrom(xType) && typeof(RequirementRowViewModel).IsAssignableFrom(yType))
            {
                return 1;
            }

            if (xType == yType)
            {
                if (xType == typeof(RequirementRowViewModel))
                {
                    return this.CompareOrderValue((Requirement) x.Thing, (Requirement) y.Thing);
                }

                if (xType == typeof(RequirementsGroupRowViewModel))
                {
                    return this.CompareOrderValue((RequirementsGroup)x.Thing, (RequirementsGroup)y.Thing);
                }

                return shortNameThingComparer.Compare((IShortNamedThing)x.Thing, (IShortNamedThing)y.Thing);
            }
            
            return 1;
        }

        /// <summary>
        /// Compares 2 <see cref="Requirement"/>
        /// </summary>
        /// <param name="x">The first requirement</param>
        /// <param name="y">The second requirement</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        private int CompareOrderValue(Requirement x, Requirement y)
        {
            if (RequirementsModule.PluginSettings?.OrderSettings != null && RequirementsModule.PluginSettings.OrderSettings.ParameterType != Guid.Empty)
            {
                var xOrder = x.ParameterValue.FirstOrDefault(z => z.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType)?.Value.FirstOrDefault();
                var yOrder = y.ParameterValue.FirstOrDefault(z => z.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType)?.Value.FirstOrDefault();

                int xOrderKey, yOrderKey;
                if (xOrder != null && int.TryParse(xOrder, out xOrderKey) && yOrder != null && int.TryParse(yOrder, out yOrderKey))
                {
                    return xOrderKey > yOrderKey
                        ? 1
                        : xOrderKey < yOrderKey
                            ? -1
                            : shortNameThingComparer.Compare(x, y);
                }
            }

            return shortNameThingComparer.Compare(x, y);
        }

        /// <summary>
        /// Compares 2 <see cref="RequirementsGroup"/>
        /// </summary>
        /// <param name="x">The first group</param>
        /// <param name="y">The second group</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        private int CompareOrderValue(RequirementsGroup x, RequirementsGroup y)
        {
            if (RequirementsModule.PluginSettings?.OrderSettings != null && RequirementsModule.PluginSettings.OrderSettings.ParameterType != Guid.Empty)
            {
                var xOrder = x.ParameterValue.FirstOrDefault(z => z.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType)?.Value.FirstOrDefault();
                var yOrder = y.ParameterValue.FirstOrDefault(z => z.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType)?.Value.FirstOrDefault();

                int xOrderKey, yOrderKey;
                if (xOrder != null && int.TryParse(xOrder, out xOrderKey) && yOrder != null && int.TryParse(yOrder, out yOrderKey))
                {
                    return xOrderKey > yOrderKey
                        ? 1
                        : xOrderKey < yOrderKey
                            ? -1
                            : shortNameThingComparer.Compare(x, y);
                }
            }

            return shortNameThingComparer.Compare(x, y);
        }
    }
}