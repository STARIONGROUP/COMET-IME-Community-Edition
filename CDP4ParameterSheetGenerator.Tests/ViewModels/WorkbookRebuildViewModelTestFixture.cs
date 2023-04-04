// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRebuildViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4ParameterSheetGenerator.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using CDP4Composition.Utilities;
    using CDP4Composition.ViewModels;

    using CDP4ParameterSheetGenerator.ViewModels;
    
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookRebuildViewModel"/> class
    /// </summary>
    [TestFixture]
    public class WorkbookRebuildViewModelTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri;
        
        private Dictionary<Guid, ProcessedValueSet> processedValueSets;

        private TextParameterType textParameterType;
        private Parameter parameter;
        private ParameterOverride parameterOverride;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.uri = new Uri("http://www.rheagroup.com");

            this.textParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri);
            this.textParameterType.Name = "text";
            this.textParameterType.ShortName = "TXT";

            var satellite = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            satellite.ShortName = "Sat";
            satellite.Name = "Satellite";

            var battery = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            battery.ShortName = "Bat";
            battery.Name = "Battery";

            var elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);
            elementUsage.ElementDefinition = battery;

            satellite.ContainedElement.Add(elementUsage);

            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            this.parameter.ParameterType = this.textParameterType;

            satellite.Parameter.Add(this.parameter);
            
            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri);
            this.parameterOverride.Parameter = this.parameter;

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            parameterValueSet.Manual = new ValueArray<string>(new List<string> { "1" });
            parameterValueSet.Reference = new ValueArray<string>(new List<string> { "2" });
            parameterValueSet.Computed = new ValueArray<string>(new List<string> { "3" });
            parameterValueSet.Formula = new ValueArray<string>(new List<string> { "-" });
            parameterValueSet.Published = new ValueArray<string>(new List<string>{ "-" });
            parameterValueSet.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.parameter.ValueSet.Add(parameterValueSet);

            var parameterOverrideValueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri);
            parameterOverrideValueSet.Manual = new ValueArray<string>(new List<string> { "1.1" });
            parameterOverrideValueSet.Reference = new ValueArray<string>(new List<string> { "2.1" });
            parameterOverrideValueSet.Computed = new ValueArray<string>(new List<string> { "3.1" });
            parameterOverrideValueSet.Formula = new ValueArray<string>(new List<string> { "-" });
            parameterOverrideValueSet.Published = new ValueArray<string>(new List<string> { "-" });
            parameterOverrideValueSet.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.parameterOverride.ValueSet.Add(parameterOverrideValueSet);
            parameterOverrideValueSet.ParameterValueSet = parameterValueSet;

            elementUsage.ParameterOverride.Add(this.parameterOverride);

            var parameterSubscribtionValueSet = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri);
            parameterSubscribtionValueSet.Manual = new ValueArray<string>(new List<string> { "1.2" });            
            parameterSubscribtionValueSet.ValueSwitch = ParameterSwitchKind.MANUAL;
            parameterSubscribtionValueSet.SubscribedValueSet = parameterValueSet;
            
            var parameterSubscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri);
            this.parameter.ParameterSubscription.Add(parameterSubscription);
            parameterSubscription.ValueSet.Add(parameterSubscribtionValueSet);

            this.processedValueSets = new Dictionary<Guid, ProcessedValueSet>();

            var valueSetValue = new ValueSetValues(0, this.textParameterType, ParameterSwitchKind.COMPUTED, "a gazilion", "a gazilion", "a gazilion", "a gazilion");

            var parameterValueSetProcessedValueSet = new ProcessedValueSet(parameterValueSet, ValidationResultKind.Valid);
            parameterValueSetProcessedValueSet.UpdateClone(valueSetValue);

            var parameterOverrideValueSetProcessedValueSet = new ProcessedValueSet(parameterOverrideValueSet, ValidationResultKind.Valid);
            parameterOverrideValueSetProcessedValueSet.UpdateClone(valueSetValue);

            var parameterSubscribtionValueSetProcessedValueSet = new ProcessedValueSet(parameterSubscribtionValueSet, ValidationResultKind.Valid);
            parameterSubscribtionValueSetProcessedValueSet.UpdateClone(valueSetValue);

            this.processedValueSets.Add(parameterValueSetProcessedValueSet.OriginalThing.Iid, parameterValueSetProcessedValueSet);
            this.processedValueSets.Add(parameterOverrideValueSetProcessedValueSet.OriginalThing.Iid, parameterOverrideValueSetProcessedValueSet);
            this.processedValueSets.Add(parameterSubscribtionValueSetProcessedValueSet.OriginalThing.Iid, parameterSubscribtionValueSetProcessedValueSet);
        }
        
        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new WorkbookRebuildViewModel(this.processedValueSets, ValueSetKind.All);
            Assert.AreEqual("Rebuild Workbook...", viewModel.DialogTitle);
            Assert.AreEqual(2, viewModel.ParameterOrOverrideSubmitParameterRowViewModels.Count);
            Assert.AreEqual(1, viewModel.ParameterSubscriptionSubmitParameterRowViewModels.Count);
        }

        [Test]
        public async Task VerifyThatCancelCommandReturnsDialogResult()
        {
            var viewModel = new WorkbookRebuildViewModel(this.processedValueSets, ValueSetKind.All);
            await viewModel.CancelCommand.Execute();

            var negativeDialogResult = viewModel.DialogResult;
            Assert.IsFalse(negativeDialogResult.Result.Value);

            await viewModel.OkCommand.Execute();
            var positiviveDialogResult = (WorkbookRebuildDialogResult)viewModel.DialogResult;

            Assert.IsTrue(positiviveDialogResult.Result.Value);
            Assert.AreEqual(RebuildKind.Overwrite, positiviveDialogResult.RebuildKind);            
        }
    }
}
