// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WhatIfEditRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.ViewModels
{
    using System;

    using ReactiveUI;

    /// <summary>
    /// Represents a single editable parameter value in the what-if editor grid: one row per (element usage,
    /// parameter). The value is identified by its model write-back path and the value set it resolves to, so the
    /// owner can record overrides, keep usages that share a value in sync, and indicate overrides.
    /// </summary>
    public class WhatIfEditRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Callback invoked when the value changes.
        /// </summary>
        private readonly Action<WhatIfEditRowViewModel> onValueChanged;

        private double? value;

        /// <summary>
        /// When true, the value setter updates the field without invoking <see cref="onValueChanged"/>; used to sync
        /// a sibling usage's displayed value without re-triggering override recording.
        /// </summary>
        private bool suppressCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhatIfEditRowViewModel"/> class.
        /// </summary>
        /// <param name="element">A readable identification of the element usage.</param>
        /// <param name="parameter">The parameter name/column.</param>
        /// <param name="path">The parameter write-back path.</param>
        /// <param name="original">The current (model) value.</param>
        /// <param name="onValueChanged">Callback invoked when the value changes.</param>
        public WhatIfEditRowViewModel(string element, string parameter, string path, double? original, Action<WhatIfEditRowViewModel> onValueChanged)
        {
            this.Element = element;
            this.Parameter = parameter;
            this.Path = path;
            this.Original = original;
            this.value = original;
            this.onValueChanged = onValueChanged;
        }

        /// <summary>Gets the element usage description.</summary>
        public string Element { get; }

        /// <summary>Gets the parameter name.</summary>
        public string Parameter { get; }

        /// <summary>Gets the parameter write-back path.</summary>
        public string Path { get; }

        /// <summary>Gets the current (model) value.</summary>
        public double? Original { get; }

        /// <summary>Gets or sets whether the value is a per-usage override (as opposed to a shared definition value).</summary>
        public bool IsOverride { get; set; }

        /// <summary>Gets or sets the value set this value resolves to (used to sync usages that share it).</summary>
        public Guid ValueSetIid { get; set; }

        /// <summary>Gets the current value with a trailing <c>*</c> when it is a per-usage override.</summary>
        public string CurrentDisplay =>
            (this.Original.HasValue ? this.Original.Value.ToString("0.####") : string.Empty) + (this.IsOverride ? " *" : string.Empty);

        /// <summary>Gets a value indicating whether the value has been changed from the model value.</summary>
        public bool IsEdited =>
            (this.Value.HasValue != this.Original.HasValue)
            || (this.Value.HasValue && this.Original.HasValue && Math.Abs(this.Value.Value - this.Original.Value) > 1e-9);

        /// <summary>Gets or sets the current (possibly edited) what-if value.</summary>
        public double? Value
        {
            get => this.value;

            set
            {
                this.RaiseAndSetIfChanged(ref this.value, value);

                if (!this.suppressCallback)
                {
                    this.onValueChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Sets the value without recording an override (used to keep a sibling usage in sync visually).
        /// </summary>
        /// <param name="newValue">The value to display.</param>
        public void SetValueSilently(double? newValue)
        {
            this.suppressCallback = true;
            this.Value = newValue;
            this.suppressCallback = false;
        }
    }
}
