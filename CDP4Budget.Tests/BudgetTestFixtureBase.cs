// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BudgetTestFixtureBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using CDP4Budget.Services;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;        
    using CDP4Dal;
    using Config;
    using Moq;

    public abstract class BudgetTestFixtureBase
    {
        protected Uri uri;
        protected ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        protected Iteration iteration;
        protected ElementDefinition rootEd;
        protected ElementDefinition ssEd;
        protected ElementDefinition eqptEd;
        protected ElementDefinition consumableEd;
        protected Category ssCat;
        protected Category eqtCat;
        protected Category consCat;
        protected DomainOfExpertise domainOfExpertise;
        protected SimpleQuantityKind mass;
        protected SimpleQuantityKind massMargin;
        protected SimpleQuantityKind number;

        protected Option option_A;
        protected Option option_B;


        protected BudgetConfig MassBudgetConfig;

        protected Mock<ISession> session;

        /// <summary>
        /// Setup that include test data
        /// </summary>
        public virtual void Setup()
        {
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.InitializeRefData();

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SYS",
                Name = "System"
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.option_A = new Option(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "OPT_A",
                Name = "Option A"
            };

            this.option_B = new Option(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "OPT_B",
                Name = "Option B"
            };

            this.InitializeElementDef();

            this.iteration.Option.Add(this.option_A);
            this.iteration.Option.Add(this.option_B);
            this.iteration.DefaultOption = this.option_A;

            this.iteration.Element.Add(this.rootEd);
            this.iteration.Element.Add(this.ssEd);
            this.iteration.TopElement = this.rootEd;

            var subSysdef = new SubSystemDefinition(new List<Category> { this.ssCat }, new List<Category> { this.eqtCat });

            var extraConfig = new ExtraMassContributionConfiguration(new List<Category> { this.consCat }, this.mass, this.massMargin);
            var paramConfig = new MassBudgetParameterConfig(new BudgetParameterMarginPair(this.mass, this.massMargin), new List<ExtraMassContributionConfiguration> { extraConfig });

            this.MassBudgetConfig = new BudgetConfig(new List<ElementDefinition> { this.rootEd }, new List<SubSystemDefinition> { subSysdef }, paramConfig, this.number, null, null, null);
        }

        private void InitializeElementDef()
        {
            this.rootEd = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Sat",
                Name = "Satellite"
            };

            this.eqptEd = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "EQT",
                Name = "eqt"
            };

            this.ssEd = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SS",
                Name = "SS"
            };

            this.consumableEd = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Cons",
                Name = "Consumable"
            };

            var ssUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = this.ssEd,
                ShortName = "SS",
                Name = "SS"
            };
            ssUsage.Category.Add(this.ssCat);

            var eqtUsage1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = this.eqptEd,
                ShortName = "eqt1",
                Name = "eqt 1"
            };
            eqtUsage1.Category.Add(this.eqtCat);

            var eqtUsage2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = this.eqptEd,
                ShortName = "eqt2",
                Name = "eqt 2"
            };
            eqtUsage2.Category.Add(this.eqtCat);

            eqtUsage2.ExcludeOption.Add(this.option_B);

            var consUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = this.consumableEd,
                ShortName = "cons",
                Name = "consumable"
            };
            consUsage.Category.Add(this.consCat);

            #region Mass 
            var ssMassP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.mass,
                IsOptionDependent = true
            };

            this.SetValueSet(ssMassP, this.option_A, "2000");
            this.SetValueSet(ssMassP, this.option_B, "3000");

            var eqtMassP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.mass,
                IsOptionDependent = true
            };

            this.SetValueSet(eqtMassP, this.option_A, "500");
            this.SetValueSet(eqtMassP, this.option_B, "100");

            var consMassP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.mass
            };

            this.SetValueSet(consMassP, null, "200");

            this.ssEd.Parameter.Add(ssMassP);
            this.eqptEd.Parameter.Add(eqtMassP);
            this.consumableEd.Parameter.Add(consMassP);
            #endregion

            #region Margin
            // define margin parameters
            var ssMassMarginP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.massMargin,
            };
            this.SetValueSet(ssMassMarginP, null, "20");

            var eqtMassMarginP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.massMargin,
            };
            this.SetValueSet(eqtMassMarginP, null, "25");

            var consMassMarginP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.massMargin
            };

            this.SetValueSet(consMassMarginP, null, "100");

            this.ssEd.Parameter.Add(ssMassMarginP);
            this.eqptEd.Parameter.Add(eqtMassMarginP);
            this.consumableEd.Parameter.Add(consMassMarginP);
            #endregion

            #region number
            // define number parameter
            var eqtNumberP = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = this.number,
                ExpectsOverride = true
            };
            this.SetValueSet(eqtNumberP, null, "0");

            var subscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domainOfExpertise,
            };

            this.SetValueSet(subscription, eqtNumberP, new List<string> { "10" });

            eqtNumberP.ParameterSubscription.Add(subscription);

            this.eqptEd.Parameter.Add(eqtNumberP);
            #endregion

            #region override mass
            var eq1massOverride1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri) { Parameter = eqtMassP };
            this.SetValueSet(eq1massOverride1, eqtMassP, new List<string> { "1000", "2000" });

            eqtUsage1.ParameterOverride.Add(eq1massOverride1);
            #endregion

            this.rootEd.ContainedElement.Add(ssUsage);
            this.rootEd.ContainedElement.Add(eqtUsage1);
            this.rootEd.ContainedElement.Add(eqtUsage2);
            this.rootEd.ContainedElement.Add(consUsage);
        }

        private void InitializeRefData()
        {
            this.ssCat = new Category(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "ss",
                Name = "subsystem"
            };

            this.eqtCat = new Category(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "eqt",
                ShortName = "eqt"
            };

            this.consCat = new Category(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "consumable",
                ShortName = "cons"
            };

            this.mass = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "m",
                Name = "mass"
            };

            this.massMargin = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "margin",
                Name = "margin"
            };

            this.number = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "number",
                Name = "number"
            };
        }


        private void SetValueSet(Parameter param, Option option, string value)
        {
            var valueset = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { ActualOption = option };
            this.SetValueSet(valueset, value);
            param.ValueSet.Add(valueset);
        }

        private void SetValueSet(ParameterOverride paramOverride, Parameter param, IReadOnlyList<string> values)
        {
            var i = 0;
            foreach (var parameterValueSet in param.ValueSet)
            {
                var valueset = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri) { ParameterValueSet = parameterValueSet };
                this.SetValueSet(valueset, values[i]);
                paramOverride.ValueSet.Add(valueset);
                i++;
            }
        }

        private void SetValueSet(ParameterSubscription subscription, ParameterOrOverrideBase param, IReadOnlyList<string> values)
        {
            var i = 0;
            foreach (var parameterValueSet in param.ValueSets)
            {
                var valueset = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri) { SubscribedValueSet = (ParameterValueSetBase)parameterValueSet };
                this.SetValueSet(valueset, values[i]);
                subscription.ValueSet.Add(valueset);
                i++;
            }
        }

        private void SetValueSet(ParameterValueSetBase param, string value)
        {
            var stringlist = new List<string> {value};
            param.Manual = new CDP4Common.Types.ValueArray<string>(stringlist);
            param.Reference = new CDP4Common.Types.ValueArray<string>(stringlist);
            param.Computed = new CDP4Common.Types.ValueArray<string>(stringlist);
            param.Formula = new CDP4Common.Types.ValueArray<string>(stringlist);
            param.ValueSwitch = ParameterSwitchKind.MANUAL;
        }

        private void SetValueSet(ParameterSubscriptionValueSet param, string value)
        {
            var stringlist = new List<string> { value };
            param.Manual = new CDP4Common.Types.ValueArray<string>(stringlist);
            param.ValueSwitch = ParameterSwitchKind.MANUAL;
        }
    }
}
