// ------------------------------------------------------------------------------------------------
// <copyright file="EventPublisher.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.EventAggregator
{
    using System;

    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// The event publisher 
    /// </summary>
    /// <remarks>
    /// This is very similar to the MessageBus and may be used in this case for view/view-model interaction.
    /// This could also have other uses if injection is used
    /// </remarks>
    public class EventPublisher : IEventPublisher
    {
        /// <summary>
        /// The collection of observers
        /// </summary>
        private readonly ConcurrentDictionary<Type, object> subjects = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Listen to a <see cref="TEvent"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of event to listen to</typeparam>
        /// <returns>The <see cref="IObservable{TEvent}"/></returns>
        public IObservable<TEvent> GetEvent<TEvent>()
        {
            var subject = (ISubject<TEvent>)this.subjects.GetOrAdd(typeof(TEvent), t => new Subject<TEvent>());
            var observable = subject.AsObservable();
            return observable;
        }

        /// <summary>
        /// Publishes the <paramref name="eventToPublish"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="eventToPublish">The event to publish</param>
        public void Publish<TEvent>(TEvent eventToPublish)
        {
            object subject;
            if (this.subjects.TryGetValue(typeof(TEvent), out subject))
            {
                ((ISubject<TEvent>)subject).OnNext(eventToPublish);
            }
        }
    }
}