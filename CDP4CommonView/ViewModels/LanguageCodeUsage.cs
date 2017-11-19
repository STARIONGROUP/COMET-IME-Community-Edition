// -------------------------------------------------------------------------------------------------
// <copyright file="LanguageCodeUsage.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using System.Globalization;

    /// <summary>
    /// The purpose of the <see cref="LanguageCodeUsage"/> class is to handle used language code selection in view-models
    /// </summary>
    public class LanguageCodeUsage
    {        
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageCodeUsage"/> class
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <param name="isUsed"></param>
        public LanguageCodeUsage(CultureInfo cultureInfo, bool isUsed)
        {
            this.Name = cultureInfo.Name;
            this.FullName = cultureInfo.NativeName;            
            this.IsUsed = isUsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageCodeUsage"/> class
        /// </summary>
        /// <param name="name">
        /// The name of the language code
        /// </param>
        /// <param name="fullname">
        /// The full name of the language code
        /// </param>
        /// <param name="isUsed"></param>
        public LanguageCodeUsage(string name, string fullname, bool isUsed)
        {
            this.Name = name;
            this.FullName = fullname;
            this.IsUsed = isUsed;
        }
        
        /// <summary>
        /// Gets a value indicating whether the <see cref="CultureInfo"/> is used
        /// </summary>
        public bool IsUsed { get; private set; }

        /// <summary>
        /// Gets the name of the current <see cref="CultureInfo"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the full-name of the current <see cref="CultureInfo"/>
        /// </summary>
        public string FullName { get; private set; }
    }
}