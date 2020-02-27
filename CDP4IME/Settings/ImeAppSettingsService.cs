// -------------------------------------------------------------------------------------------------
// <copyright file="ImeAppSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Settings
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Services.AppSettingService;

    /// <summary>
    /// An <see cref="ImeAppSettingsService"/> hold the settings from application <see cref="AppSettingsService{ImeAppSettings}"/>
    /// </summary>
    [Export(typeof(IAppSettingsService<ImeAppSettings>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImeAppSettingsService : AppSettingsService<ImeAppSettings>
    {
    }
}
