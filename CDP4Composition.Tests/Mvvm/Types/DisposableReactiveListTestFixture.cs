// -------------------------------------------------------------------------------------------------
// <copyright file="DisposableReactiveListTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using CDP4Composition.Mvvm.Types;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Contains tests to check proper working of the <see cref="DisposableReactiveList{T}" /> class.
    /// </summary>
    [TestFixture]
    public class DisposableReactiveListTestFixture
    {
        private Mock<IDisposable> disposable1;
        private Mock<IDisposable> disposable2;
        private DisposableReactiveList<IDisposable> disposableReactiveList;

        [SetUp]
        public void Setup()
        {
            this.disposable1 = new Mock<IDisposable>();
            this.disposable2 = new Mock<IDisposable>();
            this.disposableReactiveList = new DisposableReactiveList<IDisposable> {this.disposable1.Object, this.disposable2.Object};
        }

        [Test]
        public void VerifyThatClearAndDisposeDisposesItems()
        {
            this.disposableReactiveList.ClearAndDispose();
            this.disposable1.Verify(m => m.Dispose(), Times.Once());
            this.disposable2.Verify(m => m.Dispose(), Times.Once());
            CollectionAssert.IsEmpty(this.disposableReactiveList);
        }

        [Test]
        public void VerifyThatClearWithoutDisposeDoesNotDisposeItems()
        {
            this.disposableReactiveList.ClearWithoutDispose();
            this.disposable1.Verify(m => m.Dispose(), Times.Never());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            CollectionAssert.IsEmpty(this.disposableReactiveList);
        }

        [Test]
        public void VerifyThatRemoveAndDisposeDisposesItems()
        {
            this.disposableReactiveList.RemoveAndDispose(this.disposable1.Object);
            this.disposable1.Verify(m => m.Dispose(), Times.Once());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            Assert.That(this.disposableReactiveList.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatRemoveWithoutDisposeDoesNotDisposeItems()
        {
            this.disposableReactiveList.RemoveWithoutDispose(this.disposable1.Object);
            this.disposable1.Verify(m => m.Dispose(), Times.Never());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            Assert.That(this.disposableReactiveList.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatRemoveAllAndDisposeDisposesItems()
        {
            this.disposableReactiveList.RemoveAllAndDispose(new[] { this.disposable1.Object });
            this.disposable1.Verify(m => m.Dispose(), Times.Once());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            Assert.That(this.disposableReactiveList.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatRemoveAllWithoutDisposeDoesNotDisposeItems()
        {
            this.disposableReactiveList.RemoveAllWithoutDispose(new[] { this.disposable1.Object, this.disposable2.Object });
            this.disposable1.Verify(m => m.Dispose(), Times.Never());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            CollectionAssert.IsEmpty(this.disposableReactiveList);
        }

        [Test]
        public void VerifyThatRemoveAtAndDisposeDisposesItems()
        {
            this.disposableReactiveList.RemoveAtAndDispose(0);
            this.disposable1.Verify(m => m.Dispose(), Times.Once());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            Assert.That(this.disposableReactiveList.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatRemoveAtWithoutDisposeDoesNotDisposeItems()
        {
            this.disposableReactiveList.RemoveAtWithoutDispose(1);
            this.disposable1.Verify(m => m.Dispose(), Times.Never());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            Assert.That(this.disposableReactiveList.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatRemoveRangeAndDisposeDisposesItems()
        {
            this.disposableReactiveList.RemoveRangeAndDispose(0, 1);
            this.disposable1.Verify(m => m.Dispose(), Times.Once());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            Assert.That(this.disposableReactiveList.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatRemoveRangeWithoutDisposeDoesNotDisposeItems()
        {
            this.disposableReactiveList.RemoveRangeWithoutDispose(0, 2);
            this.disposable1.Verify(m => m.Dispose(), Times.Never());
            this.disposable2.Verify(m => m.Dispose(), Times.Never());
            CollectionAssert.IsEmpty(this.disposableReactiveList);
        }

        [Test]
        public void VerifyThatClearMethodIsNotSupported()
        {
            var methodInfo = typeof(DisposableReactiveList<IDisposable>).GetMethod(nameof(ReactiveList<IDisposable>.Clear));
            Assert.IsNotNull(methodInfo);

            var exception = Assert.Throws<TargetInvocationException>(() => methodInfo.Invoke(this.disposableReactiveList, null));
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        }

        [Test]
        public void VerifyThatRemoveMethodIsNotSupported()
        {
            var methodInfo = typeof(DisposableReactiveList<IDisposable>).GetMethod(nameof(ReactiveList<IDisposable>.Remove));
            Assert.IsNotNull(methodInfo);

            var exception = Assert.Throws<TargetInvocationException>(() => methodInfo.Invoke(this.disposableReactiveList, new object[] { this.disposable1.Object }));
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        }

        [Test]
        public void VerifyThatRemoveAtMethodIsNotSupported()
        {
            var methodInfo = typeof(DisposableReactiveList<IDisposable>).GetMethod(nameof(ReactiveList<IDisposable>.RemoveAt));
            Assert.IsNotNull(methodInfo);

            var exception = Assert.Throws<TargetInvocationException>(() => methodInfo.Invoke(this.disposableReactiveList, new object[] { 0 }));
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        }

        [Test]
        public void VerifyThatRemoveAllMethodIsNotSupported()
        {
            var methodInfo = typeof(DisposableReactiveList<IDisposable>).GetMethod(nameof(ReactiveList<IDisposable>.RemoveAll));
            Assert.IsNotNull(methodInfo);

            var exception = Assert.Throws<TargetInvocationException>(() => methodInfo.Invoke(this.disposableReactiveList, new object[] { new [] { this.disposable1.Object }}));
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        }

        [Test]
        public void VerifyThatRemoveRangeMethodIsNotSupported()
        {
            var methodInfo = typeof(DisposableReactiveList<IDisposable>).GetMethod(nameof(ReactiveList<IDisposable>.RemoveRange));
            Assert.IsNotNull(methodInfo);

            var exception = Assert.Throws<TargetInvocationException>(() => methodInfo.Invoke(this.disposableReactiveList, new object[] { 0, 2 }));
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOf<NotSupportedException>(exception.InnerException);
        }

        [Test]
        public void VerifyThatReactiveListMethodsAreAsExpected()
        {
            var expectedMethods = new[]
            {
                "Add", 
                "AddRange", 
                "BinarySearch", 
                "BinarySearch", 
                "BinarySearch", 
                "Clear", 
                "Contains", 
                "CopyTo", 
                "Equals", 
                "GetEnumerator", 
                "GetHashCode", 
                "GetType", 
                "IndexOf", 
                "Insert", 
                "InsertRange", 
                "Move", 
                "Remove", 
                "RemoveAll", 
                "RemoveAt", 
                "RemoveRange", 
                "Reset", 
                "Sort", 
                "Sort", 
                "Sort", 
                "SuppressChangeNotifications", 
                "ToString"
            }
                .GroupBy(x => x, x => x)
                .Select(x => new KeyValuePair<string, int>(x.Key, x.Count()))
                .ToList();

            var overriddenMethods = new List<string>
            {
                "Clear",
                "Remove",
                "RemoveAll",
                "RemoveAt",
                "RemoveRange"
            };

            var currentMethods = typeof(ReactiveList<IDisposable>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.Name.StartsWith("get_")
                            && !x.Name.StartsWith("set_")
                            && !x.Name.StartsWith("add_")
                            && !x.Name.StartsWith("remove_")
                            )
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .GroupBy(x => x, x => x)
                .Select(x => new KeyValuePair<string, int>(x.Key, x.Count()))
                .ToList();

            foreach (var overriddenMethod in overriddenMethods)
            {
                CollectionAssert.Contains(currentMethods.Select(x => x.Key), overriddenMethod, 
                    $"Overridden method {nameof(ReactiveList<IDisposable>)}.{overriddenMethod} is not found anymore. Please refactor {nameof(DisposableReactiveList<IDisposable>)}. (see: https://github.com/RHEAGROUP/CDP4-IME-Community-Edition/wiki/MVVM#disposablereactivelistt)");
            }

            var message = $"Method changes found in {nameof(ReactiveList<IDisposable>)}. Please check if {nameof(DisposableReactiveList<IDisposable>)} needs to be refactored, since all methods that result in removal of objects from {nameof(DisposableReactiveList<IDisposable>)} should be overridden. (see: https://github.com/RHEAGROUP/CDP4-IME-Community-Edition/wiki/MVVM#disposablereactivelistt)";

            Assert.AreEqual(22, currentMethods.Count, message);
            CollectionAssert.AreEquivalent(expectedMethods, currentMethods, message);
        }
    }
}
