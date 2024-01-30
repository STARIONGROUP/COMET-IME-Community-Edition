// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FavoritesService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.FavoritesService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Definition of the <see cref="IFavoritesService"/> used to load user favorite preferences from the active user.
    /// </summary>
    [Export(typeof(IFavoritesService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FavoritesService : IFavoritesService
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Type map for shortnames of UserPreferences holding favorite types.
        /// </summary>
        private readonly IReadOnlyDictionary<Type, string> favoritesTypeMap = new Dictionary<Type, string>()
        {
            { typeof(ParameterType), "FrequentlyUsedParameterTypes" }
        };

        /// <summary>
        /// Gets a collection of <see cref="Guid"/> of things in UserPreferences of the active person
        /// </summary>
        /// <param name="type">The type of the thing to register for.</param>
        /// <param name="session">The session for which to get the favorites for.</param>
        /// <returns>List of Guids of favorite things of the requested type.</returns>
        public HashSet<Guid> GetFavoriteItemsCollectionByType(ISession session, Type type)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), $"The {nameof(session)} may not be null");
            }

            var shortName = this.GetPreferenceShortname(type);

            if (string.IsNullOrWhiteSpace(shortName))
            {
                return new HashSet<Guid>();
            }

            var favoritePreference = this.GetFavoritePreference(session, shortName);

            return this.GetIidsFromUserPreference(favoritePreference);
        }

        /// <summary>
        /// Creates a subscription to a <see cref="UserPreference"/> change and returns a disposable to keep in the VM collection.
        /// </summary>
        /// <param name="session">The session this is attached to.</param>
        /// <param name="type">The type of favorite thing to listen to.</param>
        /// <param name="subscriberAction">The <see cref="Action"/>to subscribe to.</param>
        /// <returns>A <see cref="IDisposable"/> of the created observable.</returns>
        public IDisposable SubscribeToChanges(ISession session, Type type, Action<HashSet<Guid>> subscriberAction)
        {
            return
                session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(UserPreference))
                    .Where(
                        objectChange =>
                        {
                            var userPreference = (UserPreference)objectChange.ChangedThing;

                            return userPreference.Container == session.ActivePerson &&
                                   userPreference.ShortName == this.GetPreferenceShortname(type);
                        })
                    .Select(o => this.GetIidsFromUserPreference((UserPreference)o.ChangedThing))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(subscriberAction);
        }

        /// <summary>
        /// Toggles the favorite status of a thing.
        /// </summary>
        /// <typeparam name="T">The type on which to base the save.</typeparam>
        /// <param name="thing">The thing to persist/remove from persistence</param>
        /// <param name="session">The session in which to persist.</param>
        /// <returns>The empty task.</returns>
        public async Task ToggleFavorite<T>(ISession session, T thing) where T : Thing
        {
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(session.ActivePerson));

            var preferenceShortname = this.GetPreferenceShortname(typeof(T));
            var preference = this.GetFavoritePreference(session, preferenceShortname);

            var userClone = session.ActivePerson.Clone(false);

            HashSet<Guid> valueSet;

            if (preference == null)
            {
                // if property not there, create it and add
                valueSet = new HashSet<Guid>
                {
                    thing.Iid
                };

                preference = new UserPreference(Guid.NewGuid(), null, null)
                {
                    ShortName = preferenceShortname
                };

                userClone.UserPreference.Add(preference);
                transaction.CreateOrUpdate(userClone);
            }
            else
            {
                // if property is there, see if thing is in the array, true => remove, false => append
                valueSet = this.GetIidsFromUserPreference(preference);

                if (valueSet.Contains(thing.Iid))
                {
                    valueSet.Remove(thing.Iid);
                }
                else
                {
                    valueSet.Add(thing.Iid);
                }

                preference = preference.Clone(false);
            }

            preference.Value = string.Join(",", valueSet);
            transaction.CreateOrUpdate(preference);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The inline update operation failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Get the <see cref="UserPreference"/> by shortname. Null if it does not exist.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="shortName">The shortName by which to get the preference.</param>
        /// <returns>The <see cref="UserPreference"/> or null if it does not exist in the current user.</returns>
        private UserPreference GetFavoritePreference(ISession session, string shortName)
        {
            var favoritePreference =
                session.ActivePerson.UserPreference.FirstOrDefault(userPreference => userPreference.ShortName == shortName);

            return favoritePreference;
        }

        /// <summary>
        /// Converts value array of <see cref="UserPreference"/> value into list of <see cref="Guid"/>.
        /// </summary>
        /// <param name="userPreference">
        /// A <see cref="UserPreference"/> with a comma seperated list of string Guids.
        /// </param>
        /// <returns>
        /// HashSet of <see cref="Guid"/>s that were stored in the Value property of <see cref="UserPreference"/>.
        /// </returns>
        private HashSet<Guid> GetIidsFromUserPreference(UserPreference userPreference)
        {
            var result = new HashSet<Guid>();

            if (userPreference == null)
            {
                return result;
            }

            var userPreferenceValue = userPreference.Value;

            if (string.IsNullOrWhiteSpace(userPreferenceValue))
            {
                return result;
            }

            var values = userPreferenceValue.Split(',');

            foreach (var iidString in values)
            {
                if (Guid.TryParse(iidString.Trim(), out var iid))
                {
                    result.Add(iid);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the shortname of the user preference based on <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type to use for retrieving the shortname from the map.</param>
        /// <returns>The shortname of the setting.</returns>
        private string GetPreferenceShortname(Type type)
        {
            if (this.favoritesTypeMap.TryGetValue(type, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
