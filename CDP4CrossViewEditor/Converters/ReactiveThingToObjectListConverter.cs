// -------------------------------------------------------------------------------------------------
// <copyright file="ReactiveThingToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Converters
{
    using CDP4Common.CommonData;
    using CDP4Composition.Converters;
    using ReactiveUI;

    /// <summary>
    /// A converter that convert <see cref="ReactiveList{T}"/> to <see cref="ReactiveList{Thing}"/>
    /// </summary>
    public class ReactiveThingToObjectListConverter : GenericListToObjectListConverter<Thing>
    {
    }
}
