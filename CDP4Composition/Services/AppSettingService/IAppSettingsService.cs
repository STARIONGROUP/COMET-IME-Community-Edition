// -------------------------------------------------------------------------------------------------
// <copyright file="IAppSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    public interface IAppSettingsService
    {
        /// <summary>
        /// Reads the <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">A type of <see cref="AppSettings"/></typeparam>
        /// <returns>
        /// An instance of <see cref="AppSettings"/>
        /// </returns>
        T Read<T>() where T : AppSettings;

        /// <summary>
        /// Writes the <see cref="AppSettings"/> to disk
        /// </summary>
        /// <param name="appSettings">
        /// The <see cref="AppSettings"/> that will be persisted
        /// </param>
        void Write<T>(T appSettings) where T : AppSettings;
    }
}
