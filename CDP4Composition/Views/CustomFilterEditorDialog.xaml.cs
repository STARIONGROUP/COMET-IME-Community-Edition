// -------------------------------------------------------------------------------------------------
// <copyright file="CustomFilterEditorDialog.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Views
{
    using System;
    using System.Reactive.Linq;

    using CDP4Composition.Attributes;
    using CDP4Composition.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using DevExpress.Xpf.Core.FilteringUI;

    /// <summary>
    /// Interaction logic for CustomFilterEditorDialog.xaml
    /// </summary>
    [DialogViewExport("CustomFilterEditorDialog", "The custom filter editor dialog")]
    public partial class CustomFilterEditorDialog : IDialogView
    {
        /// <summary>
        /// Instanciates the <see cref="CustomFilterEditorDialog"/>
        /// </summary>
        public CustomFilterEditorDialog()
        {
        }

        /// <summary>
        /// Instanciates the <see cref="CustomFilterEditorDialog"/>
        /// </summary>
        public CustomFilterEditorDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }

            var messageBus = CommonServiceLocator.ServiceLocator.Current.GetInstance<ICDPMessageBus>();

            var subscription = messageBus.Listen<ApplyFilterEvent>()
                .Subscribe(_ => this.FilterEditor.ApplyFilter());

            this.Closed += (sender, e) =>
            {
                subscription.Dispose();
            };
        }

        /// <summary>
        /// EventHandler that routes the <see cref="FilterEditorControl"/>'s OnQueryOperators event to the ViewModel.
        /// A direct binding using EventToCommand in the view unfortunately doesn't work in all cases.
        /// That's why a non MVVM construction was used here.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="FilterEditorQueryOperatorsEventArgs"/></param>
        private async void FilterEditor_OnQueryOperators(object sender, FilterEditorQueryOperatorsEventArgs e)
        {
            if (this.DataContext is CustomFilterEditorDialogViewModel customFilterEditorDialogViewModel)
            {
                await customFilterEditorDialogViewModel.QueryOperatorsCommand.Execute(e);
            }
        }
    }
}
