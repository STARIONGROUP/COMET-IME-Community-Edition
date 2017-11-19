// ------------------------------------------------------------------------------------------------
// <copyright file="IThingDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using DevExpress.Xpf.SpellChecker;

    /// <summary>
    /// The Thing Dialog ViewModel interface.
    /// </summary>
    public interface IThingDialogViewModel
    {
        /// <summary>
        /// Gets or sets the dialog result.
        /// </summary>
        bool? DialogResult { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISpellDictionaryService"/> that is used to set the dictionaries for the Spell Checker
        /// </summary>
        ISpellDictionaryService DictionaryService { get; set; }

        /// <summary>
        /// Gets the <see cref="SpellChecker"/> that is used to perform spell checking in views.
        /// </summary>
        SpellChecker SpellChecker { get;  }
    }
}
