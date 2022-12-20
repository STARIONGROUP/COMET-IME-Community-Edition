// -----------------------------------------------------------------------
// <copyright file="WidgetDetailsBase.cs" company="RHEA">
// Copyright (c) 2020 RHEA Group. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget.Base
{
    using System;
    using System.Reactive;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="WidgetDetailsBase"/> is a base class for CDP4 overviews
    /// </summary>
    public abstract class WidgetDetailsBase : ReactiveObject, IDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="DialogResult"/>
        /// </summary>
        private IDialogResult dialogResult;

        /// <summary>
        /// Backing field for <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="LoadingMessage"/>
        /// </summary>
        private string loadingMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetDetailsBase"/> class.
        /// </summary>
        protected WidgetDetailsBase()
        {
            this.OnOKCommand = ReactiveCommandCreator.Create(this.OnOk);
            this.OnCancelCommand = ReactiveCommandCreator.Create(this.OnCancel);
        }

        /// <summary>
        /// Gets the on OK command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OnOKCommand { get; set; }

        /// <summary>
        /// Gets the on cancel command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OnCancelCommand { get; set; }

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
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Sets or gets the loading message
        /// </summary>
        public string LoadingMessage
        {
            get => this.loadingMessage;
            set => this.RaiseAndSetIfChanged(ref this.loadingMessage, value);
        }

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
