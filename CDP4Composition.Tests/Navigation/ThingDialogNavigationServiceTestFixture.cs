// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogNavigationServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Navigation
{
    using System;    
    using System.Collections.Generic;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using DevExpress.Xpf.SpellChecker;
    using Moq;
    using NUnit.Framework;
    
    /// <summary>
    /// Suite of test for the <see cref="ThingDialogViewExportAttribute"/> attribute
    /// </summary>
    [TestFixture]
    public class ThingDialogNavigationServiceTestFixture
    {
        private Mock<ISession> session;
        private TestView simpleUnitDialogView;
        private TestViewModel simpleUnitDialogViewModel;
        private SimpleUnit simpleUnit;
        private ThingTransaction transaction;
        private List<Lazy<IThingDialogView, IClassKindMetaData>> lazyviews;
        private List<Lazy<IThingDialogViewModel, IClassKindMetaData>> lazyviewmodels;
        private ISpellDictionaryService spellDictionaryService;
        private ISpecialTermsService specialTermService;
            
        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            var uri = new Uri("http://rheagroup.com");
            var siteDir = new SiteDirectory(Guid.NewGuid(), null, uri);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDir);
            this.simpleUnit = new SimpleUnit(Guid.NewGuid(), null, uri);

            var transactionContext = TransactionContextResolver.ResolveContext(siteDir);
            this.transaction = new ThingTransaction(transactionContext);

            this.simpleUnitDialogView = new TestView();
            var viewExportAttribute = new ThingDialogViewExportAttribute(ClassKind.SimpleUnit);
            var lazyview = new Lazy<IThingDialogView, IClassKindMetaData>(() => this.simpleUnitDialogView, viewExportAttribute);

            this.simpleUnitDialogViewModel = new TestViewModel(this.simpleUnit, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null);
            var viewModelExportAttribute = new ThingDialogViewModelExportAttribute(ClassKind.SimpleUnit);
            var lazyviewmodel = new Lazy<IThingDialogViewModel, IClassKindMetaData>(() => this.simpleUnitDialogViewModel, viewModelExportAttribute);

            this.lazyviews = new List<Lazy<IThingDialogView, IClassKindMetaData>>();
            this.lazyviews.Add(lazyview);

            this.lazyviewmodels = new List<Lazy<IThingDialogViewModel, IClassKindMetaData>>();
            this.lazyviewmodels.Add(lazyviewmodel);

            this.specialTermService = new SpecialTermsService();
        }

        [Test, RequiresSTA]
        public void VerifyThatTheNavigationServiceReturnsTrue()
        {
            var navigationService = new ThingDialogNavigationService(this.lazyviews, this.lazyviewmodels, this.spellDictionaryService, this.specialTermService);
            var result = navigationService.Navigate(this.simpleUnit, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null);
            Assert.IsTrue(result.Value);
        }

        [Test, RequiresSTA]
        public void Verify_that_when_on_Navigate_Thing_is_null_an_ArgumentNullException_is_thrown()
        {
            var navigationService = new ThingDialogNavigationService(this.lazyviews, this.lazyviewmodels, this.spellDictionaryService, this.specialTermService);

            Assert.Throws<ArgumentNullException>(() => navigationService.Navigate(null, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null));
        }

        [Test, RequiresSTA]
        public void Verify_that_when_on_Navigate_Session_is_null_an_ArgumentNullException_is_thrown()
        {
            var navigationService = new ThingDialogNavigationService(this.lazyviews, this.lazyviewmodels, this.spellDictionaryService, this.specialTermService);
            
            Assert.Throws<ArgumentNullException>(() => navigationService.Navigate(this.simpleUnit, this.transaction, null, true, ThingDialogKind.Create, null, null));
        }

        [Test, RequiresSTA]
        public void Verify_that_when_on_Navigate_Transaction_is_null_for_Create_or_Update_an_ArgumentNullException_is_thrown()
        {
            var navigationService = new ThingDialogNavigationService(this.lazyviews, this.lazyviewmodels, this.spellDictionaryService, this.specialTermService);

            Assert.Throws<ArgumentNullException>(() => navigationService.Navigate(this.simpleUnit, null, this.session.Object, true, ThingDialogKind.Create, null, null));
            Assert.Throws<ArgumentNullException>(() => navigationService.Navigate(this.simpleUnit, null, this.session.Object, true, ThingDialogKind.Update, null, null));
            Assert.DoesNotThrow(() => navigationService.Navigate(this.simpleUnit, null, this.session.Object, true, ThingDialogKind.Inspect, null, null));
        }

        /// <summary>
        /// implementation of <see cref="IThingDialogView"/> for testing purposes
        /// </summary>
        [ThingDialogViewExport(ClassKind.SimpleUnit)]
        public class TestView : Window, IThingDialogView
        {
            public TestView()
            {
            }

            public TestView(bool initializeComponent)
            {
            }

            public object DataContext { get; set; }

            public bool? ShowDialog()
            {
                return true;
            }
        }

        /// <summary>
        /// implementation of <see cref="IThingDialogViewModel"/> for testing purposes
        /// </summary>
        [ThingDialogViewModelExport(ClassKind.SimpleUnit)]
        public class TestViewModel : IThingDialogViewModel, IDisposable
        {
            private Thing thing;

            private ThingTransaction transaction;

            private ISession session;

            private bool? result;

            private SpellChecker spellChecker;

            public TestViewModel(Thing thing, ThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService nav, Thing container = null, List<Thing> chainOfContainers = null)
            {
                this.thing = thing;
                this.transaction = transaction;
                this.session = session;
                this.spellChecker = new SpellChecker();
            }

            public bool? DialogResult 
            {
                get { return true; }

                set
                {
                    this.result = value;
                }
            }

            public void Dispose()
            {
                return;
            }

            public ISpellDictionaryService DictionaryService { get; set; }

            /// <summary>
            /// Gets the <see cref="SpellChecker"/> instance that the <see cref="SpellingCheckerService"/> provides
            /// </summary>
            public SpellChecker SpellChecker
            {
                get
                {
                    return this.spellChecker;
                }
            }
        }
    }
}
