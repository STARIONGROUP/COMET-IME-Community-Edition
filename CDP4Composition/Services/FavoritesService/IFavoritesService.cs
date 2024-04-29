// -------------------------------------------------------------------------------------------------
// <copyright file="IFavoritesService.cs" company="Starion Group S.A.">
//   Copyright (c) 2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.FavoritesService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// Definition of the <see cref="IFavoritesService"/> used to load user favorite preferences from the active user.
    /// </summary>
    public interface IFavoritesService
    {
        /// <summary>
        /// Gets a collection of <see cref="Guid"/> of things in UserPreferences of the active person
        /// </summary>
        /// <param name="session">The session this is attached to.</param>
        /// <param name="type">The type of the thing to register for.</param>
        /// <returns>List of Guids of favorite things of the requested type.</returns>
        HashSet<Guid> GetFavoriteItemsCollectionByType(ISession session, Type type);

        /// <summary>
        /// Creates a subscription to a <see cref="UserPreference"/> change and returns a disposable to keep in the VM collection.
        /// </summary>
        /// <param name="session">The session this is attached to.</param>
        /// <param name="type">The type of favorite thing to listen to.</param>
        /// <param name="subscriberAction">The <see cref="Action"/>to subscribe to.</param>
        /// <returns>A <see cref="IDisposable"/> of the created observable.</returns>
        IDisposable SubscribeToChanges(ISession session, Type type, Action<HashSet<Guid>> subscriberAction);

        /// <summary>
        /// Toggles the favorite status of a thing.
        /// </summary>
        /// <typeparam name="T">The type on which to base the save.</typeparam>
        /// <param name="thing">The thing to persist/remove from persistence</param>
        /// <param name="session">The session in which to persist.</param>
        /// <returns>The empty task.</returns>
        Task ToggleFavorite<T>(ISession session, T thing) where T : Thing;
    }
}
