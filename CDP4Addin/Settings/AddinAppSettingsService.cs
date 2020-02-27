// -------------------------------------------------------------------------------------------------
// <copyright file="AddinAppSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4AddinCE.Settings
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Services.AppSettingService;

    /// <summary>
    /// An <see cref="AddinAppSettingsService"/> hold the settings from addin settings <see cref="AppSettingsService{AddinAppSettings}"/>
    /// </summary>
    [Export(typeof(IAppSettingsService<AddinAppSettings>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AddinAppSettingsService : AppSettingsService<AddinAppSettings>
    {
    }
}
