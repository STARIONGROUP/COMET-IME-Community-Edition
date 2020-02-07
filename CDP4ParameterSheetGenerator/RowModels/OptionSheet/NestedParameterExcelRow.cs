// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterExcelRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using CDP4Common;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Exceptions;
    using CDP4Common.SiteDirectoryData;
    using CDP4ParameterSheetGenerator.RowModels;
    using System;

    /// <summary>
    /// The purpose of the <see cref="NestedParameterExcelRow"/> is to represent <see cref="NestedParameter"/>s in the 
    /// <see cref="Option"/> Sheet
    /// </summary>
    public class NestedParameterExcelRow : ExcelRowBase<NestedParameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NestedParameterExcelRow"/> class.
        /// </summary>
        /// <param name="nestedParameter">
        /// The <see cref="NestedParameter"/> that is represented by the current row view-model
        /// </param>
        /// <param name="levelOffset">
        /// Integer that refers to the leveloffset
        /// </param>
        public NestedParameterExcelRow(NestedParameter nestedParameter, int levelOffset = -1)
            : base(nestedParameter)
        {
            this.SetLevel(levelOffset);
            this.UpdateProperties();
        }

        /// <summary>
        /// The <see cref="levelOffset"/> that is represented by the current row view-model
        /// </summary>
        /// <param name="levelOffset">
        /// Integer that refers to the leveloffset
        /// </param>
        private void SetLevel(int levelOffset)
        {
            this.Level = levelOffset + 1;
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRowBase{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();

            var spaces = new string(' ', 3 * Math.Abs(this.Level));
            this.Name = $"{spaces}{this.Thing.AssociatedParameter.ParameterType.Name}";

            this.ShortName = this.QueryParameterTypeShortname(false);
            this.ActualValue = this.Thing.ActualValue;
            this.Type = this.QueryRowType(this.Thing.AssociatedParameter);
            this.Owner = this.QueryOwner(this.Thing);
            this.ModelCode = this.Thing.Path;
            this.ParameterType = this.Thing.AssociatedParameter.ParameterType;
            this.ParameterTypeShortName = this.QueryParameterTypeShortname(true);
        }

        /// <summary>
        /// Queries the shortname of the <see cref="ParameterType"/> of the associated <see cref="Parameter"/>
        /// that is represented by the current <see cref="NestedParameter"/>.
        /// </summary>
        /// <returns>
        /// the shortname of the parametertype, or concatenation of the shortname of the parametertype and component
        /// in case the referenced parametertype is a compound parametertype.
        /// </returns>
        private string QueryParameterTypeShortname(bool inlcudeScale)
        {
            var compoundParameterType = this.Thing.AssociatedParameter.ParameterType as CompoundParameterType;
            if (compoundParameterType == null)
            {
                if (inlcudeScale && this.Thing.AssociatedParameter.Scale != null)
                {
                    return $"{this.Thing.AssociatedParameter.ParameterType.ShortName} [{this.Thing.AssociatedParameter.Scale.ShortName}]";
                }

                return this.Thing.AssociatedParameter.ParameterType.ShortName;
            }
            else
            {
                if (inlcudeScale && this.Thing.Component.Scale != null)
                {
                    return $"{this.Thing.AssociatedParameter.ParameterType.ShortName}.{this.Thing.Component.ShortName} [{this.Thing.Component.Scale.ShortName}]";
                }

                return $"{this.Thing.AssociatedParameter.ParameterType.ShortName}.{this.Thing.Component.ShortName}";
            }
        }

        /// <summary>
        /// Queries the owner short-name of the <paramref name="nestedParameter"/>
        /// </summary>
        /// <param name="nestedParameter">
        /// The <see cref="NestedParameter"/> for which the owner short-name is queried
        /// </param>
        /// <returns>
        /// The short-name of the owner
        /// </returns>
        private string QueryOwner(NestedParameter nestedParameter)
        {
            var owner = string.Empty;
            var parameterSubscription = nestedParameter.AssociatedParameter as ParameterSubscription;
            if (parameterSubscription != null)
            {
                var parameter = (ParameterOrOverrideBase)parameterSubscription.Container;
                owner = $"{nestedParameter.Owner.ShortName} [{parameter.Owner.ShortName}]";
            }
            else
            {
                owner = nestedParameter.Owner.ShortName;
            }

            return owner;
        }

        /// <summary>
        /// Queries the type of row based on the type of <see cref="ParameterBase"/>
        /// that is being represented by the current row.
        /// </summary>
        /// <param name="associatedParameter">
        /// The <see cref="ParameterBase"/> that is being represented by the current row view-model
        /// </param>
        /// <returns>
        /// the type of row
        /// </returns>
        private string QueryRowType(ParameterBase associatedParameter)
        {
            if (associatedParameter is Parameter)
            {
                return OptionSheetConstants.NP;
            }

            if (associatedParameter is ParameterOverride)
            {
                return OptionSheetConstants.NPO;
            }

            if (associatedParameter is ParameterSubscription)
            {
                return OptionSheetConstants.NPS;
            }

            throw new NestedElementTreeException($"The {associatedParameter.GetType()} type in the NestedElement Tree is not supported.");
        }
    }
}