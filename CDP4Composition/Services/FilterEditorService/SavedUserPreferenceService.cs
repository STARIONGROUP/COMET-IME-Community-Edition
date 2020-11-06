// -------------------------------------------------------------------------------------------------
// <copyright file="SavedUserPreferenceService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.FilterEditorService
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using Newtonsoft.Json;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Definition of the <see cref="ISavedUserPreferenceService"/> used to load user filter preferences from the active user.
    /// </summary>
    [Export(typeof(ISavedUserPreferenceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SavedUserPreferenceService : ISavedUserPreferenceService
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets a <see cref="T"/> in UserPreferences of the active person
        /// </summary>
        /// <param name="session">The session this is attached to.</param>
        /// <param name="userPreferenceKey">The key of the <see cref="UserPreference"/></param>
        /// <returns>A <see cref="T"/> for a specific <see cref="UserPreference.ShortName"/>.</returns>
        public T GetFilterEditorCollection<T>(ISession session, string userPreferenceKey) where T : class
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), $"The {nameof(session)} may not be null");
            }

            var userPreference = this.GetUserPreference(session, userPreferenceKey);

            return this.GetSavedUserPreferences<T>(userPreference);
        }

        /// <summary>
        /// Creates a subscription to a <see cref="UserPreference"/> change and returns a disposable to keep in the VM collection.
        /// </summary>
        /// <param name="session">The session this is attached to.</param>
        /// <param name="userPreferenceKey">The key to subscribe to</param>
        /// <param name="subscriberAction">The <see cref="Action"/>to subscribe to.</param>
        /// <returns>A <see cref="IDisposable"/> of the created observable.</returns>
        public IDisposable SubscribeToChanges<T>(ISession session, string userPreferenceKey, Action<T> subscriberAction) where T : class
        {
            return
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(UserPreference))
                    .Where(
                        objectChange =>
                        {
                            var userPreference = (UserPreference)objectChange.ChangedThing;

                            return userPreference.Container == session.ActivePerson &&
                                   userPreference.ShortName == userPreferenceKey;
                        })
                    .Select(o => this.GetSavedUserPreferences<T>((UserPreference)o.ChangedThing))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(subscriberAction);
        }

        /// <summary>
        /// Saves the <see cref="T"/> as a value for a specific key
        /// </summary>
        /// <param name="session">The session in which to persist.</param>
        /// <param name="userPreferenceKey">The key for which we want to store <see cref="UserPreference.Value"/></param>
        /// <param name="userPreferenceValue">The <see cref="T"/></param>
        /// <returns>An awaitable task.</returns>
        public async Task SaveUserPreference<T>(ISession session, string userPreferenceKey, T userPreferenceValue) where T : class
        {
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(session.ActivePerson));

            var preference = this.GetUserPreference(session, userPreferenceKey);

            var userClone = session.ActivePerson.Clone(false);

            if (preference == null)
            {
                // if property not there, create it and add
                preference = new UserPreference(Guid.NewGuid(), null, null)
                {
                    ShortName = userPreferenceKey
                };

                userClone.UserPreference.Add(preference);
                transaction.CreateOrUpdate(userClone);
            }
            else
            {
                preference = preference.Clone(false);
            }

            var values = JsonConvert.SerializeObject(userPreferenceValue);

            preference.Value = values;
            transaction.CreateOrUpdate(preference);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The inline update operation failed: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get the <see cref="UserPreference"/> by shortname. Null if it does not exist.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="preferenceKey">The shortName by which to get the preference.</param>
        /// <returns>The <see cref="UserPreference"/> or null if it does not exist in the current user.</returns>
        private UserPreference GetUserPreference(ISession session, string preferenceKey)
        {
            var preference =
                session.ActivePerson.UserPreference.FirstOrDefault(userPreference => userPreference.ShortName == preferenceKey);

            return preference;
        }

        /// <summary>
        /// Converts the value of a <see cref="UserPreference"/> into <see cref="T"/>.
        /// </summary>
        /// <param name="userPreference">
        /// A <see cref="UserPreference"/> with a <see cref="T"/> serialized as JSON
        /// </param>
        /// <returns>
        /// If found <see cref="T"/> that was stored in the Value property of <see cref="UserPreference"/>, otherwise false.
        /// </returns>
        private T GetSavedUserPreferences<T>(UserPreference userPreference) where T : class
        {
            if (userPreference == null)
            {
                return null;
            }

            var userPreferenceValue = userPreference.Value;

            if (string.IsNullOrWhiteSpace(userPreferenceValue))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(userPreferenceValue);
        }
    }
}
