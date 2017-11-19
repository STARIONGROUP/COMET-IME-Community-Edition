// -------------------------------------------------------------------------------------------------
// <copyright file="TextDataProvider.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Data;
    using NLog;

    /// <summary>
    /// Provides a text read from a File Source
    /// </summary>
    public class TextDataProvider : DataSourceProvider
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets the path of the file
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets the Result of the data provider
        /// </summary>
        public object Result { get; private set; }

        /// <summary>
        /// Query the text from the file
        /// </summary>
        protected override void BeginQuery()
        {
            try
            {
                Logger.Debug(this.Uri);

                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.Combine(assemblyPath, this.Uri);
                this.Result = File.ReadAllText(path);

                var extension = this.Uri.Substring(this.Uri.Length -4);
                if (extension == ".rtf")
                {
                    // Use the RichTextBox to convert the RTF code to plain text.
                    var rtBox = new System.Windows.Forms.RichTextBox { Rtf = (string)this.Result };
                    this.Result = rtBox.Text;
                }

                OnQueryFinished(this.Result, null, null, null);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                this.Result = e;
                OnQueryFinished(null, e, null, null);
            }
        }
    }
}