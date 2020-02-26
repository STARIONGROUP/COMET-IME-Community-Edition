// -------------------------------------------------------------------------------------------------
// <copyright file="IAppSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    /// <summary>
    /// Definition of the <see cref="IAppSettingsService{T}"/> used to load application specific settings
    /// </summary>
    public interface IAppSettingsService<T>
    {
        /// <summary>
        /// Holder of application settings
        /// </summary>
        T AppSettings { get; }

        /// <summary>
        /// Writes the <see cref="AppSettings"/> to disk
        /// </summary>
        void Save();
    }
}
