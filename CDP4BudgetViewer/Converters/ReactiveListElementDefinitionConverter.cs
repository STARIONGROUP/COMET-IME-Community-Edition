// -------------------------------------------------------------------------------------------------
// <copyright file="ReactiveListElementDefinitionConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Converters
{
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Converters;
    using ReactiveUI;

    /// <summary>
    /// A converter that convert <see cref="ReactiveList{Object}"/> to <see cref="ReactiveList{ElementDefinition}"/>
    /// </summary>
    public class ReactiveListElementDefinitionConverter : GenericListToObjectListConverter<ElementDefinition>
    {
    }
}
