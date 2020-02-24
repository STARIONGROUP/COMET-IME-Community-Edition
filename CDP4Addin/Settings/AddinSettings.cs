// -------------------------------------------------------------------------------------------------
// <copyright file="AddinSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using CDP4Composition.Services.AppSettingService;

namespace CDP4AddinCE.Settings
{
    public class AddinSettings : AppSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddinSettings"/> class
        /// </summary>
        public AddinSettings(bool initializeDefaults = false)
        {
            this.Init(initializeDefaults);
        }
    }
}
