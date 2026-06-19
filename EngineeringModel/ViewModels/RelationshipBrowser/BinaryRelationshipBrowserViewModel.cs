// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipBrowserViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    /// <summary>
    /// The view-model for the dedicated <see cref="BinaryRelationship"/> browser
    /// </summary>
    public class BinaryRelationshipBrowserViewModel : RelationshipBrowserViewModel<BinaryRelationship, BinaryRelationshipRowViewModel>
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        public const string PanelCaptionText = "Binary Relationships";

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/></param>
        public BinaryRelationshipBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.RelationshipCreator.SelectedRelationshipCreator = this.RelationshipCreator.BinaryRelationshipCreator;
        }

        /// <summary>
        /// Gets the caption of the panel
        /// </summary>
        protected override string PanelCaption => PanelCaptionText;

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="BinaryRelationship"/>
        /// </summary>
        protected override ClassKind RelationshipClassKind => ClassKind.BinaryRelationship;

        /// <summary>
        /// Gets the label of the create context-menu item
        /// </summary>
        protected override string CreateMenuItemLabel => "Create a Binary Relationship";

        /// <summary>
        /// Creates the row view-model that represents the provided <paramref name="relationship"/>
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/> that the row will represent</param>
        /// <returns>The <see cref="BinaryRelationshipRowViewModel"/> representing the <paramref name="relationship"/></returns>
        protected override BinaryRelationshipRowViewModel CreateRow(BinaryRelationship relationship)
        {
            return new BinaryRelationshipRowViewModel(relationship, this.Session, this);
        }
    }
}
