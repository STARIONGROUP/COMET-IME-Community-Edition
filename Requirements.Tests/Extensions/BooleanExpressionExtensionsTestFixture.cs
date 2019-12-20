// -------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionExtensionsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Requirements.Extensions;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests for the <see cref="BooleanExpressionExtensions"/> class
    /// </summary>
    [TestFixture]
    internal class BooleanExpressionExtensionsTestFixture
    {
        private Mock<ISession> session;
        private FakeViewModelBase viewModelBase;

        private ParametricConstraint parametricConstraint;
        private Mock<IRowViewModelBase<ParametricConstraint>> parametricConstraintViewModel;

        private AndExpression andExpression;
        private Mock<IRowViewModelBase<AndExpression>> andExpressionViewModel;
        private RelationalExpression andRelationalExpression1;
        private Mock<IRowViewModelBase<RelationalExpression>> andRelationalExpression1ViewModel;
        private RelationalExpression andRelationalExpression2;
        private Mock<IRowViewModelBase<RelationalExpression>> andRelationalExpression2ViewModel;

        private OrExpression orExpression;
        private Mock<IRowViewModelBase<OrExpression>> orExpressionViewModel;
        private RelationalExpression orRelationalExpression1;
        private Mock<IRowViewModelBase<RelationalExpression>> orRelationalExpression1ViewModel;
        private RelationalExpression orRelationalExpression2;
        private Mock<IRowViewModelBase<RelationalExpression>> orRelationalExpression2ViewModel;

        private ExclusiveOrExpression exclusiveOrExpression;
        private Mock<IRowViewModelBase<ExclusiveOrExpression>> exclusiveOrExpressionViewModel;

        private NotExpression notExpression;
        private Mock<IRowViewModelBase<NotExpression>> notExpressionViewModel;

        private RelationalExpression freeRelationalExpression;
        private Mock<IRowViewModelBase<RelationalExpression>> freeRelationalExpressionViewModel;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.viewModelBase = new FakeViewModelBase(this.session.Object);

            this.andExpression = new AndExpression();
            this.SetClassKind(this.andExpression, ClassKind.AndExpression);

            this.andExpressionViewModel = new Mock<IRowViewModelBase<AndExpression>>();
            this.andExpressionViewModel.SetupGet(x => x.Thing).Returns(this.andExpression);

            this.andRelationalExpression1 = new RelationalExpression();
            this.andRelationalExpression1ViewModel = new Mock<IRowViewModelBase<RelationalExpression>>();
            this.andRelationalExpression1ViewModel.SetupGet(x => x.ContainedRows).Returns(new ReactiveList<IRowViewModelBase<Thing>>());
            this.andRelationalExpression1ViewModel.SetupGet(x => x.Thing).Returns(this.andRelationalExpression1);
            this.andRelationalExpression1ViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.andExpressionViewModel.Object);

            this.andRelationalExpression2 = new RelationalExpression();
            this.andRelationalExpression2ViewModel = new Mock<IRowViewModelBase<RelationalExpression>>();
            this.andRelationalExpression2ViewModel.SetupGet(x => x.ContainedRows).Returns(new ReactiveList<IRowViewModelBase<Thing>>());
            this.andRelationalExpression2ViewModel.SetupGet(x => x.Thing).Returns(this.andRelationalExpression2);
            this.andRelationalExpression2ViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.andExpressionViewModel.Object);

            this.andExpressionViewModel.SetupGet(x => x.ContainedRows).Returns(
                new ReactiveList<IRowViewModelBase<Thing>>
                {
                    this.andRelationalExpression1ViewModel.Object,
                    this.andRelationalExpression2ViewModel.Object
                });

            this.orExpression = new OrExpression();
            this.SetClassKind(this.orExpression, ClassKind.OrExpression);
            this.orExpressionViewModel = new Mock<IRowViewModelBase<OrExpression>>();
            this.orExpressionViewModel.SetupGet(x => x.Thing).Returns(this.orExpression);

            this.orRelationalExpression1 = new RelationalExpression();
            this.orRelationalExpression1ViewModel = new Mock<IRowViewModelBase<RelationalExpression>>();
            this.orRelationalExpression1ViewModel.SetupGet(x => x.ContainedRows).Returns(new ReactiveList<IRowViewModelBase<Thing>>());
            this.orRelationalExpression1ViewModel.SetupGet(x => x.Thing).Returns(this.orRelationalExpression1);
            this.orRelationalExpression1ViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.orExpressionViewModel.Object);

            this.orRelationalExpression2 = new RelationalExpression();
            this.orRelationalExpression2ViewModel = new Mock<IRowViewModelBase<RelationalExpression>>();
            this.orRelationalExpression2ViewModel.SetupGet(x => x.ContainedRows).Returns(new ReactiveList<IRowViewModelBase<Thing>>());
            this.orRelationalExpression2ViewModel.SetupGet(x => x.Thing).Returns(this.orRelationalExpression2);
            this.orRelationalExpression2ViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.orExpressionViewModel.Object);

            this.orExpressionViewModel.SetupGet(x => x.ContainedRows).Returns(
                new ReactiveList<IRowViewModelBase<Thing>>
                {
                    this.orRelationalExpression1ViewModel.Object,
                    this.orRelationalExpression2ViewModel.Object
                });

            this.exclusiveOrExpression = new ExclusiveOrExpression();
            this.SetClassKind(this.exclusiveOrExpression, ClassKind.ExclusiveOrExpression);
            this.exclusiveOrExpressionViewModel = new Mock<IRowViewModelBase<ExclusiveOrExpression>>();
            this.exclusiveOrExpressionViewModel.SetupGet(x => x.Thing).Returns(this.exclusiveOrExpression);

            this.exclusiveOrExpressionViewModel.SetupGet(x => x.ContainedRows).Returns(
                new ReactiveList<IRowViewModelBase<Thing>>
                {
                    this.orExpressionViewModel.Object,
                    this.andExpressionViewModel.Object,
                });

            this.notExpression = new NotExpression();
            this.SetClassKind(this.notExpression, ClassKind.NotExpression);
            this.notExpressionViewModel = new Mock<IRowViewModelBase<NotExpression>>();
            this.notExpressionViewModel.SetupGet(x => x.Thing).Returns(this.notExpression);

            this.notExpressionViewModel.SetupGet(x => x.ContainedRows).Returns(
                new ReactiveList<IRowViewModelBase<Thing>>
                {
                    this.exclusiveOrExpressionViewModel.Object
                });

            this.freeRelationalExpression = new RelationalExpression();
            this.freeRelationalExpressionViewModel = new Mock<IRowViewModelBase<RelationalExpression>>();
            this.freeRelationalExpressionViewModel.SetupGet(x => x.Thing).Returns(this.freeRelationalExpression);
            this.freeRelationalExpressionViewModel.SetupGet(x => x.ContainedRows).Returns(new ReactiveList<IRowViewModelBase<Thing>>());

            this.FillRelationalExpression(this.andRelationalExpression1, "length", 180);
            this.FillRelationalExpression(this.andRelationalExpression2, "width", 40);
            this.FillRelationalExpression(this.orRelationalExpression1, "mass", 100);
            this.FillRelationalExpression(this.orRelationalExpression2, "accel", "pretty_fast");
            this.FillRelationalExpression(this.freeRelationalExpression, "comment", "lx_is_awesome");

            this.exclusiveOrExpressionViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.notExpressionViewModel.Object);
            this.orExpressionViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.exclusiveOrExpressionViewModel.Object);
            this.andExpressionViewModel.SetupGet(x => x.ContainerViewModel).Returns(this.exclusiveOrExpressionViewModel.Object);

            this.parametricConstraint = new ParametricConstraint();
            this.SetClassKind(this.notExpression, ClassKind.ParametricConstraint);

            this.parametricConstraintViewModel = new Mock<IRowViewModelBase<ParametricConstraint>>();
            this.parametricConstraintViewModel.SetupGet(x => x.Thing).Returns(this.parametricConstraint);

            this.parametricConstraintViewModel.SetupGet(x => x.ContainedRows).Returns(
                new ReactiveList<IRowViewModelBase<Thing>>
                {
                    this.notExpressionViewModel.Object,
                    this.freeRelationalExpressionViewModel.Object
                });
        }

        [Test]
        [TestCaseSource(nameof(GetClassesForAllBooleanExpressionTypes))]
        public void VerifyThatAllAvailableBooleanExpressionsReturnAViewModel(BooleanExpression booleanExpression)
        {
            Assert.IsNotNull(booleanExpression.GetBooleanExpressionViewModel(this.viewModelBase));
        }

        [Test]
        public void VerifyExpressionStrings()
        {
            Assert.AreEqual("(length = 180) AND (width = 40)", this.andExpressionViewModel.Object.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.andExpression));
            Assert.AreEqual("(mass = 100) OR (accel = pretty_fast)", this.orExpressionViewModel.Object.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.orExpression));
            Assert.AreEqual("((mass = 100) OR (accel = pretty_fast)) XOR ((length = 180) AND (width = 40))", this.exclusiveOrExpressionViewModel.Object.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.exclusiveOrExpression));
            Assert.AreEqual("NOT (((mass = 100) OR (accel = pretty_fast)) XOR ((length = 180) AND (width = 40)))", this.notExpressionViewModel.Object.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.notExpression));
            Assert.AreEqual(" NOT (((mass = 100) OR (accel = pretty_fast)) XOR ((length = 180) AND (width = 40))) AND (comment = lx_is_awesome)", this.parametricConstraintViewModel.Object.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.parametricConstraint));
        }

        /// <summary>
        /// Fake ViewModelBase, because we can't set a ISession
        /// </summary>
        private class FakeViewModelBase : IViewModelBase<Thing>, IISession
        {
            public FakeViewModelBase(ISession session)
            {
                this.Session = session;
            }

            /// <summary>
            /// Gets the <see cref="Thing"/> that is represented by the current <see cref="IViewModelBase{T}"/>
            /// </summary>
            public Thing Thing { get; }

            /// <summary>
            /// Dispose of the <see cref="IDisposable"/> objects
            /// </summary>
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the <see cref="T:CDP4Dal.ISession" />
            /// </summary>
            public ISession Session { get; }
        }

        /// <summary>
        /// Fills properties on a <see cref="RelationalExpression"/>
        /// </summary>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/></param>
        /// <param name="parameterShortName">The <see cref="Parameter"/>'s ShortName'</param>
        /// <param name="value">The <see cref="Parameter"/>'s Value</param>
        private void FillRelationalExpression(RelationalExpression relationalExpression, string parameterShortName, object value)
        {
            relationalExpression.ParameterType = new SimpleQuantityKind { ShortName = parameterShortName };

            this.SetClassKind(relationalExpression, ClassKind.RelationalExpression);

            relationalExpression.RelationalOperator = RelationalOperatorKind.EQ;
            relationalExpression.Value = new ValueArray<string>(new[] { value.ToString() });
        }

        /// <summary>
        /// Sets the readonly ClassKind property of an object using reflection 
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the object</typeparam>
        /// <param name="obj">The object</param>
        /// <param name="classKind">The <see cref="ClassKind"/></param>
        private void SetClassKind<T>(T obj, ClassKind classKind)
        {
            var field = typeof(RelationalExpression).GetField("<ClassKind>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(obj, classKind);
        }

        /// <summary>
        /// Gets all classes that are inherited from <see cref="BooleanExpression"/>
        /// </summary>
        /// <returns>The <see cref="IEnumerable{BooleanExpression}"/></returns>
        public static IEnumerable<BooleanExpression> GetClassesForAllBooleanExpressionTypes()
        {
            foreach (var type in
                Assembly.GetAssembly(typeof(BooleanExpression)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BooleanExpression))))
            {
                yield return (BooleanExpression) Activator.CreateInstance(type);
            }
        }
    }
}
