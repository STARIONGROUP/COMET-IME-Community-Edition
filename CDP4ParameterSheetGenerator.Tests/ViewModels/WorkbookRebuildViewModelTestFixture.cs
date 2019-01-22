// -------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRebuildViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
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
            Assert.AreEqual(2, viewModel.ParameterOrOverrideWorkbookRebuildRowViewModels.Count);
            Assert.AreEqual(1, viewModel.ParameterSubscriptionWorkbookRebuildRowViewModels.Count);
        }

        [Test]
        public void VerifyThatCancelCommandReturnsDialogResult()
        {
            var viewModel = new WorkbookRebuildViewModel(this.processedValueSets, ValueSetKind.All);
            viewModel.CancelCommand.Execute(null);

            var negativeDialogResult = viewModel.DialogResult;
            Assert.IsFalse(negativeDialogResult.Result.Value);

            viewModel.OkCommand.Execute(null);
            var positiviveDialogResult = (WorkbookRebuildDialogResult)viewModel.DialogResult;

            Assert.IsTrue(positiviveDialogResult.Result.Value);
            Assert.AreEqual(RebuildKind.Overwrite, positiviveDialogResult.RebuildKind);            
        }
    }
}
