// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveCommandCreator.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using ReactiveUI;

    /// <summary>
    /// Creates <see cref="ReactiveCommandCreator"/>s
    /// </summary>
    public class ReactiveCommandCreator
    {
        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> Create()
        {
            return ReactiveCommand.Create(() => { });
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <param name="canExecute">
        /// An optional observable that dictates the availability of the command for execution.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> Create(IObservable<bool> canExecute)
        {
            return ReactiveCommand.Create(() => { }, canExecute);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <param name="canExecute">
        /// An optional observable that dictates the availability of the command for execution.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<T, Unit> Create<T>(Action<T> execute, IObservable<bool> canExecute = null)
        {
            return ReactiveCommand.Create(execute, canExecute);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <param name="canExecute">
        /// An optional observable that dictates the availability of the command for execution.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> Create(Action execute, IObservable<bool> canExecute = null)
        {
            return ReactiveCommand.Create(execute, canExecute);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <param name="scheduler">
        /// A scheduler that is used to surface events. Defaults to <c>RxApp.MainThreadScheduler</c>.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<T, Unit> CreateAsyncTask<T>(Func<T, Task> execute, IScheduler scheduler)
        {
            return ReactiveCommand.CreateFromTask(execute, null, scheduler);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <param name="scheduler">
        /// A scheduler that is used to surface events. Defaults to <c>RxApp.MainThreadScheduler</c>.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> CreateAsyncTask(Func<Task> execute, IScheduler scheduler)
        {
            return ReactiveCommand.CreateFromTask(execute, null, scheduler);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <param name="canExecute">
        /// An optional observable that dictates the availability of the command for execution.
        /// </param>
        /// <param name="scheduler">
        /// A scheduler that is used to surface events. Defaults to <c>RxApp.MainThreadScheduler</c>.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> CreateAsyncTask(Func<Task> execute, IObservable<bool> canExecute, IScheduler scheduler)
        {
            return ReactiveCommand.CreateFromTask(execute, canExecute, scheduler);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<T, Unit> CreateAsyncTask<T>(Func<T, Task> execute)
        {
            return ReactiveCommand.CreateFromTask(execute);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> CreateAsyncTask(Func<Task> execute)
        {
            return ReactiveCommand.CreateFromTask(execute);
        }

        /// <summary>
        /// Create a <see cref="ReactiveCommandCreator"/>
        /// </summary>
        /// <param name="execute">
        /// The action to execute whenever the command is executed.
        /// </param>
        /// <param name="canExecute">
        /// An optional observable that dictates the availability of the command for execution.
        /// </param>
        /// <returns>The <see cref="ReactiveCommandCreator"/></returns>
        public static ReactiveCommand<Unit, Unit> CreateAsyncTask(Func<Task> execute, IObservable<bool> canExecute)
        {
            return ReactiveCommand.CreateFromTask(execute, canExecute);
        }
    }
}
