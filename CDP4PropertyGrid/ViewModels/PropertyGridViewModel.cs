// -------------------------------------------------------------------------------------------------
// <copyright file="PropertyGridViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.ViewModels
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Common.CommonData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation.Events;

    using CDP4Dal;

    using CDP4PropertyGrid.Views;

    /// <summary>
    /// The view-model for the <see cref="PropertyGrid"/> that displays the properties of a <see cref="Thing"/>
    /// </summary>
    [Export(typeof(IPanelViewModel))]
    public class PropertyGridViewModel : ViewModelBase<Thing>, IPanelViewModel
    {
        /// <summary>
        /// The <see cref="IDisposable"/> subscription to the <see cref="SelectedThingChangedEvent"/>
        /// </summary>
        private IDisposable selectedThingChangedSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridViewModel"/> class
        /// </summary>
        /// <param name="initialize">
        /// a value indicating whether the viewmodel should be initialized
        /// </param>
        [ImportingConstructor]
        public PropertyGridViewModel(bool initialize)
        {
            if (initialize)
            {
                this.Identifier = Guid.NewGuid();
                this.Initialize();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridViewModel"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> to display
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public PropertyGridViewModel(Thing thing, ISession session)
            : base(thing, session)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes this <see cref="PropertyGridViewModel"/>
        /// </summary>
        private void Initialize()
        {
            this.selectedThingChangedSubscription = CDPMessageBus.Current.Listen<SelectedThingChangedEvent>().Subscribe(this.ChangeViewModel);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.selectedThingChangedSubscription?.Dispose();
        }

        /// <summary>
        /// Gets the Caption
        /// </summary>
        public string Caption => "Properties";

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty => false;

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the Tooltip
        /// </summary>
        public string ToolTip => "Displays the properties of the selected thing";

        /// <summary>
        /// Gets the data-source
        /// </summary>
        public string DataSource => this.Session.DataSourceUri;

        public string TargetName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Check if a new <see cref="PropertyGridViewModel"/> should be created.
        /// </summary>
        /// <param name="selectedThingChangedEvent">
        /// The <see cref="SelectedThingChangedEvent"/>
        /// </param>
        private void ChangeViewModel(SelectedThingChangedEvent selectedThingChangedEvent)
        {
            this.Dispose();

            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    var newVm = new PropertyGridViewModel(selectedThingChangedEvent.SelectedThing, selectedThingChangedEvent.Session)
                    {
                        Identifier = this.Identifier
                    };

                    CDPMessageBus.Current.SendMessage(new ViewModelChangeEvent(newVm));
                },
                DispatcherPriority.Background);
        }
    }
}
