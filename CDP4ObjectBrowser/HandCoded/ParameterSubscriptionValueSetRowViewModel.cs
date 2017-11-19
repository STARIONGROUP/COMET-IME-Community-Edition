// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionValueSetRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParameterSubscriptionValueSetRowViewModel"/>
    /// </summary>
    public partial class ParameterSubscriptionValueSetRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = string.Format("{{ {0} }}", string.Join(", ", this.Thing.ActualValue));
            this.ShortName = string.Format("{{ {0} }}", string.Join(", ", this.Thing.SubscribedValueSet));
        }
    }
}
