// -----------------------------------------------------------------------
// <copyright file="WidgetBase.cs" company="Starion Group S.A.">
// Copyright (c) 2020 Starion Group S.A. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget.Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reactive;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using CDP4Composition.Mvvm;

    using Microsoft.Win32;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="WidgetBase"/> is the abstract base class for all CDP4 Widgets
    /// </summary>
    public abstract class WidgetBase : ReactiveObject, IDisposable
    {
        /// <summary>
        /// The title.
        /// </summary>
        private string title;

        /// <summary>
        /// The unit.
        /// </summary>
        private string unit;

        /// <summary>
        /// The X-Axis' title
        /// </summary>
        private string axisXTitle;

        /// <summary>
        /// The Y-Axis' title
        /// </summary>
        private string axisYTitle;

        /// <summary>
        /// The title of the control
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        /// <summary>
        /// The unit displayed for the series
        /// </summary>
        public string Unit
        {
            get => this.unit;
            set => this.RaiseAndSetIfChanged(ref this.unit, value);
        }

        /// <summary>
        /// The X-Axis' title
        /// </summary>
        public string AxisXTitle
        {
            get => this.axisXTitle;
            set => this.RaiseAndSetIfChanged(ref this.axisXTitle, value);
        }

        /// <summary>
        /// The Y-Axis' title
        /// </summary>
        public string AxisYTitle
        {
            get => this.axisYTitle;
            set => this.RaiseAndSetIfChanged(ref this.axisYTitle, value);
        }

        /// <summary>
        /// Gets the copy image to the clipboard command
        /// </summary>
        public ReactiveCommand<Control, Unit> CopyImageToClipboardCommand { get; }

        /// <summary>
        /// Gets the save image to a file command
        /// </summary>
        public ReactiveCommand<Control, Unit> SaveImageToFileCommand { get; }

        /// <summary>
        /// Instantiates an instance of WidtgetBase
        /// </summary>
        protected WidgetBase()
        {
            this.SaveImageToFileCommand = ReactiveCommandCreator.Create<Control>(this.SaveImageToFile);
            this.CopyImageToClipboardCommand = ReactiveCommandCreator.Create<Control>(this.CopyImageToClipboard);
        }

        /// <summary>
        /// Copy the contents or <see cref="Visual"/> of a <see cref="Control"/> to the clipboard
        /// </summary>
        /// <param name="control">an instance of a <see cref="Control"/> object</param>
        protected void CopyImageToClipboard(Control control)
        {
            var actualWidth = control.ActualWidth;
            var actualHeight = control.ActualHeight;

            // init contexts
            var brush = new VisualBrush(control);
            var visual = new DrawingVisual();
            var context = visual.RenderOpen();

            context.DrawRectangle(brush, null, new Rect(0, 0, actualWidth, actualHeight));
            context.Close();

            // set up render target
            var bmp = new RenderTargetBitmap(
                (int)actualWidth,
                (int)actualHeight,
                96,
                96,
                PixelFormats.Default);

            bmp.Render(visual);

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            BitmapSource bs = encoder.Frames[0];

            Clipboard.SetImage(bs);
        }

        /// <summary>
        /// Save the contents or <see cref="Visual"/> of a <see cref="Control"/> to a file
        /// </summary>
        /// <param name="control">an instance of a <see cref="Control"/> object</param>
        protected void SaveImageToFile(Control control)
        {
            var actualWidth = control.ActualWidth;
            var actualHeight = control.ActualHeight;

            // Configure save file dialog box
            var dlg = new SaveFileDialog { FileName = "Untitled", DefaultExt = ".jpg", Filter = "JPG Images (.jpg)|*.jpg" };

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // init contexts
                var brush = new VisualBrush(control);
                var visual = new DrawingVisual();
                var context = visual.RenderOpen();

                context.DrawRectangle(
                    brush, null, new Rect(0, 0, actualWidth, actualHeight));
                context.Close();

                // set up redenr target
                var bmp = new RenderTargetBitmap(
                    (int)actualWidth,
                    (int)actualHeight,
                    96,
                    96,
                    PixelFormats.Default);

                bmp.Render(visual);

                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                // start stream
                var file = new FileStream(dlg.FileName, FileMode.Create);
                encoder.Save(file);

                // close stream
                file.Close();
            }
        }

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; } = new List<IDisposable>();

        /// <summary>
        /// Dispose disposable referenced objects
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in this.Disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
