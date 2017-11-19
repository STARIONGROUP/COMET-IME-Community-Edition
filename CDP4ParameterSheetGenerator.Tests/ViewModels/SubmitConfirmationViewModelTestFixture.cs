// -------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using CDP4Composition.Navigation;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.ViewModels;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SubmitConfirmationViewModel"/> class
    /// </summary>
    [TestFixture]
    public class SubmitConfirmationViewModelTestFixture
    {
        private ParameterValueSet parameterValueSet;

        private ParameterType parameterType;
        private ElementDefinition elementDefinition;

        [SetUp]
        public void SetUp()
        {
            this.parameterType = new TextParameterType(Guid.NewGuid(), null, null) { ShortName = "TXT" };

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), null, null);
            var iteration = new Iteration(Guid.NewGuid(), null, null);
            engineeringModel.Iteration.Add(iteration);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            this.elementDefinition.ShortName = "BAT";
            iteration.Element.Add(this.elementDefinition);

            var parameter = new Parameter(Guid.NewGuid(), null, null) { ParameterType = this.parameterType };
            this.elementDefinition.Parameter.Add(parameter);
            this.parameterValueSet = new ParameterValueSet(Guid.NewGuid(), null, null);
            var manual = new ValueArray<string>(new List<string>() { "1" });
            var computed = new ValueArray<string>(new List<string>() { "2" });
            var reference = new ValueArray<string>(new List<string>() { "3" });
            var formula = new ValueArray<string>(new List<string>() { "-" });
            this.parameterValueSet.Manual = manual;
            this.parameterValueSet.Computed = computed;
            this.parameterValueSet.Reference = reference;
            this.parameterValueSet.Formula = formula;
            this.parameterValueSet.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameter.ValueSet.Add(this.parameterValueSet);
        }

        [Test]
        public void VerifyThatRowsAreCreated()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);
            var valueSetValue = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.COMPUTED, "a gazilion", "a gazilion", "a gazilion", "a gazilion");
            processedValueSet.UpdateClone(valueSetValue);

            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            processedValueSets.Add(processedValueSet.OriginalThing.Iid, processedValueSet);
            
            var viewmodel = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);

            Assert.AreEqual(1, viewmodel.ParameterOrOverrideWorkbookRebuildRowViewModels.Count);
        }

        [Test]
        public void VerifyThatAtLeastOneRowHasToBeSelectedBeforeOkCanExecute()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);
            var valueSetValue = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.COMPUTED, "a gazilion", "a gazilion", "a gazilion", "a gazilion");
            processedValueSet.UpdateClone(valueSetValue);

            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            processedValueSets.Add(processedValueSet.OriginalThing.Iid, processedValueSet);
            
            var viewmodel = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);

            var row = viewmodel.ParameterOrOverrideWorkbookRebuildRowViewModels.Single();
            Assert.IsTrue(row.IsSelected);
            Assert.IsTrue(viewmodel.OkCommand.CanExecute(null));

            row.IsSelected = false;

            Assert.IsFalse(viewmodel.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatDialogResultIsReturnedWhenOk()
        {
            var submitmessage = "this is the submitmessage";

            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);
            var valueSetValue = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.COMPUTED, "a gazilion", "a gazilion", "a gazilion", "a gazilion");
            processedValueSet.UpdateClone(valueSetValue);

            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            processedValueSets.Add(processedValueSet.OriginalThing.Iid, processedValueSet);

            var viewmodel = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);
            viewmodel.SubmitMessage = submitmessage;

            var row = viewmodel.ParameterOrOverrideWorkbookRebuildRowViewModels.Single();
            Assert.IsTrue(row.IsSelected);
            Assert.IsTrue(viewmodel.OkCommand.CanExecute(null));

            viewmodel.OkCommand.Execute(null);

            var result = (SubmitConfirmationDialogResult)viewmodel.DialogResult;
            Assert.IsTrue(result.Result.Value);
            
            Assert.AreEqual(submitmessage, result.SubmitMessage); 

            Assert.IsNotEmpty(result.Clones);        
        }

        [Test]
        public void VerifyThatCheckAllWorks()
        {
            var processedValueSet1 = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);
            var parameterValueSet2 = new ParameterValueSet(Guid.NewGuid(), null, null)
            {
                Manual = new ValueArray<string>(new List<string>() { "1" }),
                Computed = new ValueArray<string>(new List<string>() { "2" }),
                Reference = new ValueArray<string>(new List<string>() { "3" }),
                Formula = new ValueArray<string>(new List<string>() { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var parameter2 = new Parameter(Guid.NewGuid(), null, null) { ParameterType = this.parameterType };
            this.elementDefinition.Parameter.Add(parameter2);
            parameter2.ValueSet.Add(parameterValueSet2);

            var processedValueSet2 = new ProcessedValueSet(parameterValueSet2, ValidationResultKind.Valid);
            var valueSetValue1 = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.COMPUTED, "a gazilion", "a gazilion", "a gazilion", "a gazilion");
            processedValueSet1.UpdateClone(valueSetValue1);
            processedValueSet2.UpdateClone(valueSetValue1);

            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            processedValueSets.Add(processedValueSet1.OriginalThing.Iid, processedValueSet1);
            processedValueSets.Add(processedValueSet2.OriginalThing.Iid, processedValueSet2);


            var vm = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);

            var rows = vm.ParameterOrOverrideWorkbookRebuildRowViewModels;
            Assert.AreEqual(2, rows.Count);
            Assert.IsTrue(rows.All(r => r.IsSelected));
        }

        [Test]
        public void VerifyThatDialogResultIsReturnedWhenCancelled()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);
            var valueSetValue = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.COMPUTED, "a gazilion", "a gazilion", "a gazilion", "a gazilion");
            processedValueSet.UpdateClone(valueSetValue);

            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            processedValueSets.Add(processedValueSet.OriginalThing.Iid, processedValueSet);

            var viewmodel = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);

            viewmodel.CancelCommand.Execute(null);

            var result = (BaseDialogResult)viewmodel.DialogResult;
            Assert.IsFalse(result.Result.Value);
        }
    }
}
