// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InlineEditContentUserControl.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Core;
    using EventAggregator;
    using ReactiveUI;
    using ViewModels;

    /// <summary>
    /// Interaction logic for InlineEditContentUserControl
    /// </summary>
    public partial class InlineEditContentUserControl
    {
        #region DependencyProperties

        /// <summary>
        /// The dependency property that allows setting of which mode to start in.
        /// </summary>
        public static readonly DependencyProperty StartInPreviewModeProperty = DependencyProperty.Register("StartInPreviewMode", typeof(bool), typeof(InlineEditContentUserControl), new FrameworkPropertyMetadata(false, OnStartInPreviewModeChanged));

        /// <summary>
        /// The dependency property that allows setting of which mode to start in.
        /// </summary>
        public static readonly DependencyProperty InlineModeProperty = DependencyProperty.Register("InlineMode", typeof(bool), typeof(InlineEditContentUserControl));

        /// <summary>
        /// The dependency property that allows setting of which mode to start in.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(InlineEditContentUserControl));

        /// <summary>
        /// The dependency property that allows setting the <see cref="IEventPublisher"/>
        /// </summary>
        public static readonly DependencyProperty EventPublisherProperty = DependencyProperty.Register("EventPublisher", typeof(IEventPublisher), typeof(InlineEditContentUserControl), new FrameworkPropertyMetadata(EventPublisherChanged));

        /// <summary>
        /// The dependency property that allows setting the content
        /// </summary>
        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(InlineEditContentUserControl), new FrameworkPropertyMetadata(""));

        /// <summary>
        /// The dependency property that allows setting the save <see cref="ICommand"/>
        /// </summary>
        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(InlineEditContentUserControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The dependency property that allows setting the cancel <see cref="ICommand"/>
        /// </summary>
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(InlineEditContentUserControl), new FrameworkPropertyMetadata(null));

        #endregion

        /// <summary>
        /// The observable for <see cref="ConfirmationEvent"/>
        /// </summary>
        private IDisposable confirmationEventObservable;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineEditContentUserControl"/> class.
        /// </summary>
        public InlineEditContentUserControl()
        {
            this.InitializeComponent();
            this.SetViewMode(false);

            this.Loaded += (s, e) =>
            {
                var parent = Window.GetWindow(this);
                if (parent != null)
                {
                    parent.Closing += (s1, e1) =>
                    {
                        if (this.confirmationEventObservable != null)
                        {
                            this.confirmationEventObservable.Dispose();
                        }
                    };
                }
            };
        }

        #region DependencyProperties
        /// <summary>
        /// Gets or sets the save <see cref="ICommand"/>
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                return (ICommand)this.GetValue(SaveCommandProperty);
            }

            set
            {
                this.SetValue(SaveCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the cancel <see cref="ICommand"/>
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                return (ICommand)this.GetValue(CancelCommandProperty);
            }

            set
            {
                this.SetValue(CancelCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to start in preview mode.
        /// </summary>
        /// <value>
        ///   true if start in preview mode is set; otherwise, false.
        /// </value>
        public bool StartInPreviewMode
        {
            get
            {
                return (bool)this.GetValue(StartInPreviewModeProperty);
            }

            set
            {
                this.SetValue(StartInPreviewModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to operate in inline mode.
        /// </summary>
        /// <value>
        ///   true if start in inline mode is set; otherwise, false.
        /// </value>
        public bool InlineMode
        {
            get
            {
                return (bool)this.GetValue(InlineModeProperty);
            }

            set
            {
                this.SetValue(InlineModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEventPublisher"/>
        /// </summary>
        public IEventPublisher EventPublisher
        {
            get
            {
                return (IEventPublisher)this.GetValue(EventPublisherProperty);
            }

            set
            {
                this.SetValue(EventPublisherProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to operate in read only mode.
        /// </summary>
        /// <value>
        ///   true if readonly mode is set; otherwise, false.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return (bool)this.GetValue(IsReadOnlyProperty);
            }

            set
            {
                this.SetValue(IsReadOnlyProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        public string ContentText
        {
            get
            {
                return (string)this.GetValue(ContentTextProperty);
            }

            set
            {
                this.SetValue(ContentTextProperty, value);
            }
        }
        #endregion

        /// <summary>
        /// Called when the <see cref="EventPublisher"/> is changed
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void EventPublisherChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            var noteControl = (InlineEditContentUserControl)caller;

            noteControl.SetEventPublisher((IEventPublisher)e.NewValue);
        }

        /// <summary>
        /// Called when start in preview mode changed.
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnStartInPreviewModeChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            var noteControl = caller as InlineEditContentUserControl;

            noteControl.SetViewMode((bool)e.NewValue);
        }

        /// <summary>
        /// The set the <see cref="IEventPublisher"/>
        /// </summary>
        /// <param name="publisher">
        /// The inline mode.
        /// </param>
        private void SetEventPublisher(IEventPublisher publisher)
        {
            if (this.confirmationEventObservable != null)
            {
                this.confirmationEventObservable.Dispose();
            }

            this.EventPublisher = publisher;

            if (this.EventPublisher == null)
            {
                return;
            }

            this.confirmationEventObservable = this.EventPublisher.GetEvent<ConfirmationEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    confirmation =>
                    {
                        if (confirmation.IsConfirmed)
                        {
                            this.SetPreviewMode();
                        }
                    });
        }

        /// <summary>
        /// Sets the view mode.
        /// </summary>
        /// <param name="startInPreviewModel">if set to true start in preview model.</param>
        private void SetViewMode(bool startInPreviewModel)
        {
            if (startInPreviewModel)
            {
                this.SetPreviewMode();
            }
            else
            {
                this.SetEditMode();
            }
        }

        /// <summary>
        /// Handles the OnClick event of the Bold control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void Bold_OnClick(object sender, ItemClickEventArgs e)
        {
            this.SetTextWithUndo(string.Concat("**", this.ContentEditBox.SelectedText.Trim(), "**"));
            this.ContentEditBox.CaretIndex = this.ContentEditBox.CaretIndex + 2;
        }

        /// <summary>
        /// Handles the OnClick event of the Italics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void Italics_OnClick(object sender, ItemClickEventArgs e)
        { 
            this.SetTextWithUndo(string.Concat("*", this.ContentEditBox.SelectedText.Trim(), "*"));
            this.ContentEditBox.CaretIndex = this.ContentEditBox.CaretIndex + 1;
        }

        /// <summary>
        /// Handles the OnClick event of the Link control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void Link_OnClick(object sender, ItemClickEventArgs e)
        {
            this.SetTextWithUndo(string.Concat("[", this.ContentEditBox.SelectedText.Trim(), "](https://example.com)"));
            this.ContentEditBox.CaretIndex = this.ContentEditBox.CaretIndex + 1;
        }

        /// <summary>
        /// Handles the OnClick event of the Code control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void Code_OnClick(object sender, ItemClickEventArgs e)
        {
            this.SetTextWithUndo(string.Concat("```", this.ContentEditBox.SelectedText.Trim(), "```"));
            this.ContentEditBox.CaretIndex = this.ContentEditBox.CaretIndex + 3;
        }

        /// <summary>
        /// Handles the OnClick event of the BulletList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void BulletList_OnClick(object sender, ItemClickEventArgs e)
        {
            this.SetTextWithUndo(string.Concat("\r\n\r\n- ", this.ContentEditBox.SelectedText));
        }

        /// <summary>
        /// Handles the OnClick event of the NumberList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void NumberList_OnClick(object sender, ItemClickEventArgs e)
        {
            this.SetTextWithUndo(string.Concat("\r\n\r\n1. ", this.ContentEditBox.SelectedText));
        }

        /// <summary>
        /// Simulate user input on new text to enable internal undo stack.
        /// </summary>
        /// <param name="newText">The new text for the text box.</param>
        private void SetTextWithUndo(string newText)
        {
            // store clipboard data
            var clipboardBackup = Clipboard.GetDataObject();

            Clipboard.SetText(newText);
            this.ContentEditBox.Paste();

            // restore clipboard data
            if (clipboardBackup != null)
            {
                Clipboard.SetDataObject(clipboardBackup);
            }
        }

        /// <summary>
        /// Handles the OnClick event of the Preview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private void Preview_OnClick(object sender, ItemClickEventArgs e)
        {
            this.SetPreviewMode();
        }

        /// <summary>
        /// Handles the OnClick event of the Edit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetEditMode();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        private void SetEditMode()
        {
            this.EditButton.SetZIndex(200);
            this.EditButton.Visibility = Visibility.Collapsed;

            this.FlowDocumentScrollViewer.SetZIndex(100);
            this.FlowDocumentScrollViewer.Visibility = Visibility.Collapsed;
            this.ContentEditBox.SetZIndex(1000);
            this.ToolBarControl.SetZIndex(1000);
            this.ContentEditBox.Visibility = Visibility.Visible;
            this.ToolBarControl.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets the preview mode.
        /// </summary>
        private void SetPreviewMode()
        {
            this.EditButton.SetZIndex(2000);
            this.EditButton.Visibility = Visibility.Visible;

            this.FlowDocumentScrollViewer.SetZIndex(1000);
            this.FlowDocumentScrollViewer.Visibility = Visibility.Visible;
            this.ContentEditBox.SetZIndex(100);
            this.ToolBarControl.SetZIndex(100);
            this.ContentEditBox.Visibility = Visibility.Collapsed;
            this.ToolBarControl.Visibility = Visibility.Collapsed;
        }
    }
}