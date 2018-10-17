// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;

    public class SubSystem
    {
        /// <summary>
        /// The equipment of this sub-system
        /// </summary>
        private readonly List<ElementUsage> equipments;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystem"/> class
        /// </summary>
        /// <param name="definition">The sub-system definition</param>
        /// <param name="usage">The corresponding <see cref="ElementUsage"/> representing the sub-system</param>
        public SubSystem(SubSystemDefinition definition, ElementUsage usage)
        {
            this.equipments = new List<ElementUsage>();
            this.SubSystemDefinition = definition;
            this.SubSystemElementUsage = usage;
        }

        /// <summary>
        /// Gets the sub-system definition
        /// </summary>
        public SubSystemDefinition SubSystemDefinition { get; }

        /// <summary>
        /// Gets the <see cref="ElementUsage"/> representing the sub-system
        /// </summary>
        public ElementUsage SubSystemElementUsage { get; }

        /// <summary>
        /// Gets the equipments
        /// </summary>
        public IReadOnlyList<ElementUsage> Equipments => this.equipments;

        /// <summary>
        /// Gets the equipments of the sub-system
        /// </summary>
        /// <param name="usage">The <see cref="ElementUsage"/> representing the equipment</param>
        public void AddEquipment(ElementUsage usage)
        {
            this.equipments.Add(usage);
        }
        
    }
}
