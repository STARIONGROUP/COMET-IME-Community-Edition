// -------------------------------------------------------------------------------------------------
// <copyright file="ImeAppSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using CDP4Composition.Services.AppSettingService;
using System.Collections.Generic;

namespace CDP4IME.Settings
{
    public class ImeAppSettings : AppSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ImeAppSettings"/> class
        /// </summary>
        public ImeAppSettings(bool initializeDefaults = false)
        {
            this.Init(initializeDefaults);
        }
    }
}
