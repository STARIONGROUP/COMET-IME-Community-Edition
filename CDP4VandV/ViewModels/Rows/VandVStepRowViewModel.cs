// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVStepRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.ViewModels.Rows
{
    using CDP4VandV.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// One procedure step shown as a row beneath its V&amp;V item, so the Procedure view reads as the procedure
    /// itself rather than a count of steps.
    /// </summary>
    public class VandVStepRowViewModel : RowViewModelBase<Requirement>
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="StepAction"/>
        /// </summary>
        private string stepAction;

        /// <summary>
        /// Backing field for <see cref="StepExpected"/>
        /// </summary>
        private string stepExpected;

        /// <summary>
        /// Backing field for <see cref="StepActual"/>
        /// </summary>
        private string stepActual;

        /// <summary>
        /// Backing field for <see cref="StepResult"/>
        /// </summary>
        private string stepResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVStepRowViewModel"/> class.
        /// </summary>
        /// <param name="step">The step <see cref="Requirement"/>.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="containerViewModel">The owning V&amp;V item row.</param>
        public VandVStepRowViewModel(Requirement step, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(step, session, containerViewModel)
        {
            this.SetProperties();
        }

        /// <summary>Gets the step name.</summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets the step short-name.</summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>Gets what the operator must do.</summary>
        public string StepAction
        {
            get => this.stepAction;
            private set => this.RaiseAndSetIfChanged(ref this.stepAction, value);
        }

        /// <summary>Gets what should be observed if the step passes.</summary>
        public string StepExpected
        {
            get => this.stepExpected;
            private set => this.RaiseAndSetIfChanged(ref this.stepExpected, value);
        }

        /// <summary>Gets what was actually observed.</summary>
        public string StepActual
        {
            get => this.stepActual;
            private set => this.RaiseAndSetIfChanged(ref this.stepActual, value);
        }

        /// <summary>Gets the step outcome.</summary>
        public string StepResult
        {
            get => this.stepResult;
            private set => this.RaiseAndSetIfChanged(ref this.stepResult, value);
        }

        /// <summary>
        /// Refreshes the projected properties when the underlying step changes.
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.SetProperties();
        }

        /// <summary>
        /// Updates the projected properties from the underlying step.
        /// </summary>
        private void SetProperties()
        {
            this.ShortName = this.Thing.ShortName;
            this.Name = this.Thing.Name;
            this.StepAction = VandVCoverageQuery.Attribute(this.Thing, "vnv_step_action");
            this.StepExpected = VandVCoverageQuery.Attribute(this.Thing, "vnv_step_expected");
            this.StepActual = VandVCoverageQuery.Attribute(this.Thing, "vnv_step_actual");
            this.StepResult = VandVCoverageQuery.Attribute(this.Thing, "vnv_step_result");
        }
    }
}
