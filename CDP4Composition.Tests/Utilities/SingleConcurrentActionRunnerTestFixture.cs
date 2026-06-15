// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleConcurrentActionRunnerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Composition.Utilities;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SingleConcurrentActionRunner"/> class
    /// </summary>
    [TestFixture]
    public class SingleConcurrentActionRunnerTestFixture
    {
        /// <summary>
        /// Polls a condition until it becomes true or the timeout elapses.
        /// </summary>
        private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMilliseconds = 5000)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!condition() && stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
            {
                await Task.Delay(20);
            }
        }

        [Test]
        public async Task Verify_that_DelayRunTaskWithInnerCancellationToken_executes_the_func_after_the_delay()
        {
            var runner = new SingleConcurrentActionRunner();
            var executed = false;

            runner.DelayRunTaskWithInnerCancellationToken(
                _ =>
                {
                    executed = true;
                    return Task.CompletedTask;
                },
                50);

            Assert.That(executed, Is.False, "the func should not be invoked synchronously");

            await WaitUntilAsync(() => executed);

            Assert.That(executed, Is.True, "the func should be invoked after the delay");
        }

        [Test]
        public async Task Verify_that_DelayRunTaskWithInnerCancellationToken_passes_a_non_cancelled_token_to_the_func()
        {
            var runner = new SingleConcurrentActionRunner();
            var wasCancellationRequested = true;
            var executed = false;

            runner.DelayRunTaskWithInnerCancellationToken(
                token =>
                {
                    wasCancellationRequested = token.IsCancellationRequested;
                    executed = true;
                    return Task.CompletedTask;
                },
                50);

            await WaitUntilAsync(() => executed);

            Assert.That(executed, Is.True);
            Assert.That(wasCancellationRequested, Is.False);
        }

        [Test]
        public async Task Verify_that_a_subsequent_call_cancels_the_previously_scheduled_func()
        {
            var runner = new SingleConcurrentActionRunner();
            var firstExecuted = false;
            var secondExecuted = false;

            runner.DelayRunTaskWithInnerCancellationToken(
                _ =>
                {
                    firstExecuted = true;
                    return Task.CompletedTask;
                },
                500);

            // reschedule before the first delay elapses, which should cancel the first task
            runner.DelayRunTaskWithInnerCancellationToken(
                _ =>
                {
                    secondExecuted = true;
                    return Task.CompletedTask;
                },
                50);

            await WaitUntilAsync(() => secondExecuted);

            // give the first (cancelled) delay enough time to have fired if it was not cancelled
            await Task.Delay(700);

            Assert.That(secondExecuted, Is.True, "the most recently scheduled func should be invoked");
            Assert.That(firstExecuted, Is.False, "the previously scheduled func should have been cancelled");
        }

        [Test]
        public async Task Verify_that_CancelCurrentTask_prevents_the_func_from_being_executed()
        {
            var runner = new SingleConcurrentActionRunner();
            var executed = false;

            runner.DelayRunTaskWithInnerCancellationToken(
                _ =>
                {
                    executed = true;
                    return Task.CompletedTask;
                },
                300);

            runner.CancelCurrentTask();

            await Task.Delay(500);

            Assert.That(executed, Is.False, "a cancelled task should not invoke the func");
        }

        [Test]
        public void Verify_that_CancelCurrentTask_does_not_throw_when_no_task_is_scheduled()
        {
            var runner = new SingleConcurrentActionRunner();

            Assert.DoesNotThrow(() => runner.CancelCurrentTask());
        }
    }
}
