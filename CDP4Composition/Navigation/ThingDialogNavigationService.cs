// ------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogNavigationService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;

    using CDP4Common.CommonData;
    using CDP4Common.Operations;

    using CDP4Composition.Attributes;
    using CDP4Composition.Converters;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using DevExpress.Xpf.Core;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="ThingDialogNavigationService"/> is to enable navigation to a Window that
    /// acts as a dialog window to inspect, edit or add <see cref="Thing"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ThingDialogNavigationService"/> relies in views (Windows) and view-models to exist for the specified <see cref="Thing"/>. These views
    /// and view-models are injected by MEF. Injection relies on the fact that the vies (Windows) implement the <see cref="IThingDialogView"/> interface and
    /// that the view-models implement the <see cref="IThingDialogViewModel"/>. Both need to be decorated with the <see cref="IClassKindMetaData"/> attribute that
    /// specifies for what <see cref="Thing"/> the combination shall be used. Only one view/view-model combination shall exist in all the loaded modules/plugins
    /// per <see cref="Thing"/>.
    /// </remarks>
    [Export(typeof(IThingDialogNavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThingDialogNavigationService : IThingDialogNavigationService
    {
        /// <summary>
        /// The NLog Logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A lookup table to contains the views that have implement <see cref="IThingDialogView"/> and have been decorated
        /// with <see cref="IClassKindMetaData"/> attribute. These views collaborate with the <see cref="ThingDialogNavigationService"/>
        /// to show a modal dialog window
        /// </summary>
        /// <remarks>
        /// The views represent the user interface
        /// </remarks>
        private readonly Dictionary<ClassKind, Type> viewDictionary;

        /// <summary>
        /// A lookup table to contains the views that have implement <see cref="IThingDialogViewModel"/> and have been decorated
        /// with <see cref="IClassKindMetaData"/> attribute. These views collaborate with the <see cref="ThingDialogNavigationService"/>
        /// to show a modal dialog window.
        /// </summary>
        /// <remarks>
        /// the view-models represent the logic (data-context) of corresponding views
        /// </remarks>
        private readonly Dictionary<ClassKind, Type> viewModelDictionary;

        /// <summary>
        /// An instance of the <see cref="ISpellDictionaryService"/> used to load spelling dictionaries for a spell checker
        /// </summary>
        private readonly ISpellDictionaryService spellDictionaryService;

        /// <summary>
        /// An instance of the <see cref="ISpecialTermsService"/> used to load spelling dictionaries for a spell checker
        /// </summary>
        private readonly ISpecialTermsService specialTermService;

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDialogNavigationService"/> class.
        /// </summary>
        /// <param name="dialogViews">
        /// All the dialog views that are (MEF) injected.
        /// </param>
        /// <param name="dialogViewModels">
        /// The dialog view models that are (MEF) injected.
        /// </param>
        /// <param name="spellDictionaryService">
        /// The (MEF) injected <see cref="ISpellDictionaryService"/> that is used to load spelling dictionaries for a spell checker
        /// </param>
        /// <param name="specialTermsService">
        /// The (MEF) injected <see cref="ISpecialTermsService"/> that is used to handle textbox decoration.
        /// </param>
        [ImportingConstructor]
        public ThingDialogNavigationService([ImportMany] IEnumerable<Lazy<IThingDialogView, IClassKindMetaData>> dialogViews, [ImportMany] IEnumerable<Lazy<IThingDialogViewModel, IClassKindMetaData>> dialogViewModels, [Import] ISpellDictionaryService spellDictionaryService, [Import] ISpecialTermsService specialTermsService)
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Instantiating the ThingDialogNavigationService");

            this.viewDictionary = new Dictionary<ClassKind, Type>();
            foreach (var dialogView in dialogViews)
            {
                var classKindView = dialogView.Metadata.ClassKind;
                var viewType = dialogView.Value.GetType();
                this.viewDictionary.Add(classKindView, viewType);
                Logger.Trace("Add viewType {0} for ClassKind {1}", viewType.Name, classKindView);
            }

            this.viewModelDictionary = new Dictionary<ClassKind, Type>();
            foreach (var dialogViewModel in dialogViewModels)
            {
                var classKindViewModel = dialogViewModel.Metadata.ClassKind;
                var viewModelType = dialogViewModel.Value.GetType();
                this.viewModelDictionary.Add(classKindViewModel, viewModelType);
                Logger.Trace("Add viewModelType {0} for ClassKind {1}", viewModelType.Name, classKindViewModel);
            }

            this.spellDictionaryService = spellDictionaryService;
            this.specialTermService = specialTermsService;

            sw.Stop();
            logger.Debug("The ThingDialogNavigationService was instantiated in {0} [ms]", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Navigates to the dialog associated to the specified <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> for which a dialog window needs to be opened
        /// </param>
        /// <param name="transaction">
        /// The transaction that is used to record changes on the <see cref="Thing"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> which is used to persist the changes recorded in the <see cref="transaction"/>
        /// </param>
        /// <param name="isRoot">
        /// Assert if the <see cref="IThingDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation the <see cref="IThingDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> for the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        /// <returns>
        /// true if the dialog is confirmed, false if otherwise.
        /// </returns>
        public bool? Navigate(Thing thing, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
        {
            Utils.AssertNotNull(thing);
            Utils.AssertNotNull(session);
            if (dialogKind != ThingDialogKind.Inspect)
            {
                Utils.AssertNotNull(transaction);
            }

            // set up special terms service
            this.specialTermService.RebuildDictionaries(session);

            var viewType = this.ResolveViewType(thing.ClassKind);

            var viewParameters = new object[] { true };
            var view = (Window)Activator.CreateInstance(viewType, viewParameters);
            this.SetDialogProperties(view, thing, dialogKind);

            var viewModelType = this.ResolveViewModelType(thing.ClassKind);

            var viewModelParameters = new object[] { thing, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers };
            IThingDialogViewModel viewModel = null;

            try
            {
                viewModel = Activator.CreateInstance(viewModelType, viewModelParameters) as IThingDialogViewModel;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (viewModel != null)
            {
                viewModel.DictionaryService = this.spellDictionaryService;
                view.DataContext = viewModel;

                this.SetOwner(view);

                var iview = (IThingDialogView)view;
                var res = iview.ShowDialog();

                // clean subscription to MessageBus
                var disposableViewModel = (IDisposable)viewModel;
                iview.DataContext = null;
                disposableViewModel.Dispose();

                // TODO: implement processing of result to update status bar with results of dialog (success, failure, cancelled, like in CDP3)
                return viewModel.DialogResult;
            }
            else
            {
                return false;
            }            
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        /// <summary>
        /// Resolves the <see cref="Type"/> of the view associated to the provided <see cref="ClassKind"/>
        /// </summary>
        /// <param name="classKind">
        /// the <see cref="ClassKind"/> for which the view <see cref="Type"/> needs to be resolved
        /// </param>
        /// <returns>
        /// an instance of <see cref="Type"/>
        /// </returns>
        private Type ResolveViewType(ClassKind classKind)
        {
            Type returnedViewType;
            var viewTypeFound = this.viewDictionary.TryGetValue(classKind, out returnedViewType);
            if (!viewTypeFound)
            {
                throw new Exception(string.Format("An IThingDialogView for {0} has not been registered with the Application", classKind));
            }

            return returnedViewType;
        }

        /// <summary>
        /// Resolves the <see cref="Type"/> of the view-model associated to the provided <see cref="ClassKind"/>
        /// </summary>
        /// <param name="classKind">
        /// the <see cref="ClassKind"/> for which the view-model <see cref="Type"/> needs to be resolved
        /// </param>
        /// <returns>
        /// an instance of <see cref="Type"/>
        /// </returns>
        private Type ResolveViewModelType(ClassKind classKind)
        {
            Type returnedViewModelType;
            var viewModelTypeFound = this.viewModelDictionary.TryGetValue(classKind, out returnedViewModelType);

            if (!viewModelTypeFound)
            {
                throw new Exception(string.Format("A IThingDialogViewModel for {0} has not been registered with the Application", classKind));
            }

            return returnedViewModelType;
        }

        /// <summary>
        /// Sets the owner (parent) window on the provided view
        /// </summary>
        /// <param name="view">
        /// the view on which the owner property needs to be set
        /// </param>
        private void SetOwner(Window view)
        {
            // making use of PInvoke here covers edge cases where more than one window may have the IsActive attribute set to true
            try
            {
                var active = GetActiveWindow();

                if (Application.Current == null)
                {
                    // when running the Addin the centring is not able to determine the owner location because Application.Current == null
                    // use InteropHelper to set the Owner property of the window based on active pointer to Excel.
                    // reference http://blog.terrydai.com/show-modal-dialog-with-a-owner-which-belongs-to-another-process/
                    var helper = new WindowInteropHelper(view);
                    helper.Owner = active;
                }
                else
                {
                    var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(window => new WindowInteropHelper(window).Handle == active);
                    view.Owner = activeWindow;
                }
            }
            catch (Exception ex)
            {
                Logger.Trace(ex);
            }

            view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        /// <summary>
        /// Set Layout of the DialogWindow
        /// </summary>
        /// <param name="window">
        /// The window that is created to hold this dialog.
        /// </param>
        /// <param name="thing">
        /// The thing that this dialog represents.
        /// </param>
        /// <param name="dialogKind">
        /// The kind of dialog being created.
        /// </param>
        private void SetDialogProperties(Window window, Thing thing, ThingDialogKind dialogKind)
        {
            ThemeManager.SetThemeName(window, "Seven");
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.Background = Brushes.AliceBlue;
            window.ResizeMode = ResizeMode.NoResize;
            window.ShowInTaskbar = false;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.WindowState = WindowState.Normal;

            if (thing != null)
            {
                var titlePrefix = string.Empty;

                switch (dialogKind)
                {
                    case ThingDialogKind.Create:
                        titlePrefix = "Create";
                        break;
                    case ThingDialogKind.Inspect:
                        titlePrefix = "Inspect";
                        break;
                    case ThingDialogKind.Update:
                        titlePrefix = "Edit";
                        break;
                    default:
                        titlePrefix = string.Empty;
                        break;
                }

                var typename = new CamelCaseToSpaceConverter().Convert(thing.ClassKind, null, null, null);
                window.Title = string.Format("{0} {1}", titlePrefix, typename);
            }
        }
    }
}
