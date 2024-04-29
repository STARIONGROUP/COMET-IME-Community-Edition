// ------------------------------------------------------------------------------------------------
// <copyright file="IEventPublisher.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.EventAggregator
{
    using System;

    /// <summary>
    /// The interface for an event-publisher
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a <see cref="TEvent"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="sampleEvent">The <see cref="TEvent"/> to publish</param>
        void Publish<TEvent>(TEvent sampleEvent);

        /// <summary>
        /// Listens to the <see cref="TEvent"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of event to listen to</typeparam>
        /// <returns>The <see cref="IObservable{TEvent}"/></returns>
        IObservable<TEvent> GetEvent<TEvent>();
    }
}