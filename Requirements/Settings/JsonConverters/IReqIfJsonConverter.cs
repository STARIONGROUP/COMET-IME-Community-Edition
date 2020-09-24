namespace CDP4Requirements.Settings.JsonConverters
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using ReqIFSharp;

    /// <summary>
    /// Defines Properties for Json converters used in this Requirement plugin
    /// </summary>
    public interface IReqIfJsonConverter
    {
        /// <summary>
        /// The <see cref="ReqIF.CoreContent"/>
        /// </summary>
        ReqIFContent ReqIfCoreContent { get; }

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.Iteration"/>
        /// </summary>
        Iteration Iteration { get; }
    }
}
