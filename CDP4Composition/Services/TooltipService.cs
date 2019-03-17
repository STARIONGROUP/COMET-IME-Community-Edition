// -------------------------------------------------------------------------------------------------
// <copyright file="TooltipService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Linq;
    using System.Text;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="TooltipService"/> is to derive the tooltp text of a specific <see cref="Thing"/>
    /// </summary>
    public static class TooltipService
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns a string that represents a tooltip text for the specified <see cref="Thing"/>
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static string Tooltip(this Thing thing)
        {
            var sb = new StringBuilder();
            
            var shortNamedThing = thing as IShortNamedThing;
            if (shortNamedThing != null)
            {
                sb.AppendLine($"Short Name: {shortNamedThing.ShortName}");
            }

            var namedThing = thing as INamedThing;
            if (namedThing != null)
            {
                sb.AppendLine($"Name: {namedThing.Name}");
            }

            var ownedThing = thing as IOwnedThing;
            if (ownedThing != null)
            {
                var owner = string.Empty;
                if (ownedThing.Owner != null)
                {
                    owner = ownedThing.Owner.ShortName;
                }
                else
                {
                    owner = "NA";
                    logger.Debug($"Owner if {thing.ClassKind} null");
                }

                sb.AppendLine($"Owner: {owner}");
            }

            var categorizableThing = thing as ICategorizableThing;
            if (categorizableThing != null)
            {
                var categories = categorizableThing.Category.Any() ? string.Join(" ", categorizableThing.Category.OrderBy(x => x.ShortName).Select(x => x.ShortName)) : "-";
                sb.AppendLine($"Category: {categories}");
            }

            var modelCodeThing = thing as IModelCode;
            if (modelCodeThing != null)
            {
                var modelCode = string.Empty;

                try
                {
                    modelCode = modelCodeThing.ModelCode();
                }
                catch (Exception e)
                {
                    modelCode = "Invalid Model Code";
                    logger.Error(e);
                }

                sb.AppendLine($"Model Code: {modelCode}");
            }
            
            var definedThing = thing as DefinedThing;
            if (definedThing != null)
            {
                var definition = definedThing.Definition.FirstOrDefault();
                sb.AppendLine(definition == null
                    ? $"Definition : -"
                    : $"Definition [{definition.LanguageCode}]: {definition.Content}");
            }

            sb.Append($"Type: {thing.ClassKind}");

            return sb.ToString();
        }
    }
}