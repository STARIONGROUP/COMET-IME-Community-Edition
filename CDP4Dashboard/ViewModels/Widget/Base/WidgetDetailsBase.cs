// -----------------------------------------------------------------------
// <copyright file="WidgetDetailsBase.cs" company="RHEA">
// Copyright (c) 2020 RHEA Group. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget.Base
{
    using System;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="WidgetDetailsBase"/> is a base class for CDP4 overviews
    /// </summary>
    public abstract class WidgetDetailsBase : ReactiveObject, IDialogViewModel
    {
        /// <summary>
        /// The title.
        /// </summary>
        private string title;

        private IDialogResult dialogResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetDetailsBase"/> class.
        /// </summary>
        protected WidgetDetailsBase()
        {
            this.OnOKCommand = ReactiveCommand.Create();
            this.OnOKCommand.Subscribe(_ => this.OnOk());

            this.OnCancelCommand = ReactiveCommand.Create();
            this.OnCancelCommand.Subscribe(_ => this.OnCancel());
        }

        /// <summary>
        /// Gets the on OK command.
        /// </summary>
        public ReactiveCommand<object> OnOKCommand { get; set; }

        /// <summary>
        /// Gets the on cancel command.
        /// </summary>
        public ReactiveCommand<object> OnCancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the title of the widget
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        /// <summary>
        /// Sets or gets the dialog result
        /// </summary>
        public IDialogResult DialogResult
        {
            get => this.dialogResult;
            set => this.RaiseAndSetIfChanged(ref this.dialogResult, value);
        }

        /// <summary>
        /// Gets or sets the busy indicator
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Sets or gets the loading message
        /// </summary>
        public string LoadingMessage { get; set; }

        /// <summary>
        /// The OK command
        /// </summary>
        protected virtual void OnOk()
        {
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// The cancel command
        /// </summary>
        private void OnCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// The Dispose implementation
        /// </summary>
        public abstract void Dispose();
    }
}
