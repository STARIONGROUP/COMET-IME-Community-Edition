// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramRibbonPageCategoryViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels.ContextualRibbonPageViewModels
{
    using System;
    using System.Reactive.Linq;

    using CDP4Dal;

    using CDP4DiagramEditor.Events;

    using DevExpress.Xpf.Charts;

    using ReactiveUI;

    using EventKind = Events.EventKind;

    /// <summary>
    /// The purpose of the <see cref="DiagramRibbonPageCategoryViewModel"/> is to represent the view-model for <see cref="Diagram"/>s
    /// </summary>
    public class DiagramRibbonPageCategoryViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ShouldItBeVisible"/>
        /// </summary>
        private bool shouldItBeVisible;

        /// <summary>
        /// Gets a value indicating whether the Diagram page category is showing up or not
        /// </summary>
        /// <remarks>
        /// Sets Whether the Diagram page category should be visible or not based on if any DiagramEditor are opened
        /// </remarks>

        public bool ShouldItBeVisible
        {
            get => this.shouldItBeVisible;
            private set => this.RaiseAndSetIfChanged(ref this.shouldItBeVisible, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramRibbonPageCategoryViewModel"/> class.
        /// </summary>
        public DiagramRibbonPageCategoryViewModel()
        {
            this.ShouldItBeVisible = false;
            
            this.AddSubscriptions();
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var subscribeOn =
                CDPMessageBus.Current.Listen<ViewChangedEvent>(this.GetType())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(value => this.ShouldItBeVisible = value.EventKind == EventKind.Showing);
        }
    }
}
