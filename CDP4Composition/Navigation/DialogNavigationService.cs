// ------------------------------------------------------------------------------------------------
// <copyright file="DialogNavigationService.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Dal.Composition;
    using Interfaces;
    using NLog;

    /// <summary>
    /// The Dialog navigation service that handles view/viewModel dialog navigation requests
    /// </summary>
    [Export(typeof(IDialogNavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DialogNavigationService : IDialogNavigationService
    {
        /// <summary>
        /// The Nlog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogNavigationService"/> class
        /// </summary>
        /// <param name="dialogViewKinds">The <see cref="IDialogView"/>s contained in the application</param>
        /// <param name="dialogViewModelKinds">The <see cref="IDialogViewModel"/>s contained in the application</param>
        [ImportingConstructor]
        public DialogNavigationService
        (
            [ImportMany] IEnumerable<Lazy<IDialogView, INameMetaData>> dialogViewKinds,
            [ImportMany] IEnumerable<Lazy<IDialogViewModel, INameMetaData>> dialogViewModelKinds
        )
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Instantiating the DialogNavigationService");

            this.FloatingThingDialog = new Dictionary<Thing, IDialogView>();
            this.DialogViewKinds = new Dictionary<string, Type>();
            this.DialogViewModelKinds = new Dictionary<string, Lazy<IDialogViewModel, INameMetaData>>();

            foreach (var dialogView in dialogViewKinds)
            {
                var dialogViewName = dialogView.Value.ToString();

                this.DialogViewKinds.Add(dialogViewName, dialogView.Value.GetType());
                logger.Trace("Add DialogView: {0}", dialogViewName);
            }

            foreach (var dialogViewModel in dialogViewModelKinds)
            {
                var dialogViewModelName = dialogViewModel.Value.ToString();

                var dialogViewModelDescribeName = dialogViewModel.Metadata.Name;
                this.DialogViewModelKinds.Add(dialogViewModelDescribeName, dialogViewModel);

                logger.Trace("Add DialogViewModel {0}", dialogViewModelName);
            }

            sw.Stop();
            logger.Debug("The DialogNavigationService was instantiated in {0} [ms]", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets or Sets the existing <see cref="IDialogView"/> in the application
        /// </summary>
        public Dictionary<string, Type> DialogViewKinds { get; private set; }

        /// <summary>
        /// Gets or sets the list of the <see cref="IDialogViewModel"/> in the application which are decorated with <see cref="INameMetaData"/>.
        /// </summary>
        public Dictionary<string, Lazy<IDialogViewModel, INameMetaData>> DialogViewModelKinds { get; set; }

        /// <summary>
        /// Gets the list of <see cref="Thing"/> which floating dialog is open
        /// </summary>
        public Dictionary<Thing, IDialogView> FloatingThingDialog { get; private set; }

        /// <summary>
        /// Navigates to the dialog associated to the <see cref="IDialogViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IDialogViewModel"/> associated to the Dialog Box to navigate to</param>
        /// <returns>The <see cref="IDialogResult"/></returns>
        public IDialogResult NavigateModal(IDialogViewModel viewModel)
        {
            var view = this.GetView(viewModel, new object[] { true });
            view.ShowDialog();

            return viewModel.DialogResult;
        }

        /// <summary>
        /// Navigates to the dialog associated to the <see cref="IDialogViewModel"/> which has its <see cref="INameMetaData.Name"/> equals to the <see cref="viewModelName"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <returns>The <see cref="IDialogResult"/>.</returns>
        public IDialogResult NavigateModal(string viewModelName)
        {
            Lazy<IDialogViewModel, INameMetaData> returned;
            if (!this.DialogViewModelKinds.TryGetValue(viewModelName, out returned))
            {
                throw new ArgumentOutOfRangeException(string.Format("The ViewModel with the human readable name {0} could not be found", viewModelName));
            }

            IDialogViewModel viewModel;

            viewModel = Activator.CreateInstance(returned.Value.GetType()) as IDialogViewModel;
            var result = this.NavigateModal(viewModel);
            viewModel?.Dispose();
            return result;
        }

        /// <summary>
        /// Navigates to the non-model dialog given a <see cref="IDialogViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IDialogViewModel"/></param>
        public void NavigateFloating(IFloatingDialogViewModel<Thing> viewModel)
        {
            if (this.FloatingThingDialog.ContainsKey(viewModel.Thing))
            {
                // floating dialog already open => Activate
                var existingView = (Window)this.FloatingThingDialog[viewModel.Thing];
                existingView.Activate();
                return;
            }

            var view = this.GetView(viewModel, new object[] { this });
            this.FloatingThingDialog.Add(viewModel.Thing, (IDialogView)view);
            view.Show();
        }

        /// <summary>
        /// Remove the <see cref="KeyValuePair{Thing, IDialogView}"/> from the open floating dialog list
        /// </summary>
        /// <param name="view">The closing <see cref="IDialogView"/></param>
        public void ClosingFloatingWindow(IDialogView view)
        {
            var kvp = this.FloatingThingDialog.SingleOrDefault(x => x.Value == view);
            if (!kvp.Equals(new KeyValuePair<Thing, IDialogView>()))
            {
                this.FloatingThingDialog.Remove(kvp.Key);
            }
        }

        /// <summary>
        /// Gets an instance of the view associated to a <see cref="IDialogViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IDialogViewModel"/></param>
        /// <param name="viewParameters">The parameters to pass to the constructor of the view</param>
        /// <returns>The view</returns>
        private Window GetView(IDialogViewModel viewModel, object[] viewParameters)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel", "The viewModel must not be null");
            }

            var viewName = this.GetViewTypeName(viewModel);
            logger.Debug("Navigate to: {0}", viewName);

            Type viewType;
            if (!this.DialogViewKinds.TryGetValue(viewName, out viewType))
            {
                throw new ArgumentOutOfRangeException(
                    "viewModel",
                    "Could not find a view associated to the view-model. Make sure the Export attribute is on the view and that the Naming convention for both view/view-model is applied.");
            }

            Window view;
            try
            {
                view = (Window)Activator.CreateInstance(viewType, viewParameters);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Make sure the view has a constructor with the desired argument", e);
            }

            view.DataContext = viewModel;
            this.SetOwner(view);

            return view;
        }

        /// <summary>
        /// Gets the fully qualified name of the <see cref="IDialogView"/> associated to the <see cref="IDialogViewModel"/>
        /// </summary>
        /// <remarks>
        /// We assume here that for a <see cref="IDialogViewModel"/> with a fully qualified name xxx.yyy.ViewModels.DialogViewModel, the counterpart view is xxx.yyy.Views.Dialog
        /// </remarks>
        /// <param name="viewModel">The <see cref="IDialogViewModel"/></param>
        /// <returns>The Fully qualified Name</returns>
        /// <exception cref="ArgumentNullException">
        /// The viewModel must not be null
        /// </exception>
        private string GetViewTypeName(IDialogViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel", "The viewModel must not be null");
            }

            var fullyQualifiedName = viewModel.ToString().Replace(".ViewModels.", ".Views.");

            // remove "ViewModel" from the name to get the View Name
            var viewName = System.Text.RegularExpressions.Regex.Replace(fullyQualifiedName, "ViewModel$", string.Empty);

            return viewName;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

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
                logger.Info(ex);
            }

            view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
    }
}