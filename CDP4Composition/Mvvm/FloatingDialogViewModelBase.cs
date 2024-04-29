// -------------------------------------------------------------------------------------------------
// <copyright file="FloatingDialogViewModelBase.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Dal;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;
    using Navigation;
    using Navigation.Interfaces;
    using ReactiveUI;

    /// <summary>
    /// Base class for a generic dialog that requires interaction from the user.
    /// </summary>
    public class FloatingDialogViewModelBase<T> : ViewModelBase<T>, IFloatingDialogViewModel<T> where T : Thing
    {
        #region Private Fields
        /// <summary>
        /// The relative location of the loading animation image.
        /// </summary>
        private const string CdpLogoAnimationPath = @"\Resources\Images\comet.ico";

        /// <summary>
        /// The backing field for <see cref="AnimationUri"/>
        /// </summary>
        private string animationUri;

        /// <summary>
        /// Backing field for <see cref="DialogResult"/>
        /// </summary>
        private IDialogResult dialogResult;

        /// <summary>
        /// Backing field for the <see cref="IsBusy"/> property
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for the <see cref="LoadingMessage"/> property
        /// </summary>
        private string loadingMessage;

        /// <summary>
        /// Out property for the <see cref="HasError"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> hasError;

        /// <summary>
        /// Backing field for the <see cref="ErrorMessage"/> property.
        /// </summary>
        private string errorMessage;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModelBase"/> class.
        /// </summary>
        /// <param name="thing">The <see cref="T"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        protected FloatingDialogViewModelBase(T thing, ISession session) : base (thing, session)
        {
            this.LoadingMessage = "Loading...";

            // add the animation uri path
            this.animationUri = string.Format("{0}{1}",
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), CdpLogoAnimationPath);

            // error message handling
            this.WhenAnyValue(x => x.ErrorMessage).Select(x => !string.IsNullOrWhiteSpace(x)).ToProperty(this, x => x.HasError, out this.hasError, scheduler: RxApp.MainThreadScheduler);
        }
        #endregion

        #region public Properties
        /// <summary>
        /// Gets or sets the <see cref="IDialogResult"/>
        /// </summary>
        public IDialogResult DialogResult
        {
            get
            {
                return this.dialogResult;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.dialogResult, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog is performing an operation which is blocking.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isBusy, value);
            }
        }

        /// <summary>
        /// Gets or sets the loading panel text
        /// </summary>
        public string LoadingMessage
        {
            get
            {
                return this.loadingMessage;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.loadingMessage, value);
            }
        }

        /// <summary>
        /// Gets the animation uri
        /// </summary>
        public string AnimationUri
        {
            get
            {
                return this.animationUri;
            }
        }

        /// <summary>
        /// Gets or sets the Error Message
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.errorMessage, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ErrorMessage"/> is not empty.
        /// </summary>
        public bool HasError
        {
            get { return this.hasError.Value; }
        }
        #endregion

        /// <summary>
        /// Write the inline operations to the Data-access-layer
        /// </summary>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the operations</param>
        protected async Task DalWrite(ThingTransaction transaction)
        {
            var operationContainer = transaction.FinalizeTransaction();
            await this.Session.Write(operationContainer);
        }
    }
}
