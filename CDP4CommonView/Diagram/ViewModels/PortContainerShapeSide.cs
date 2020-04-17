
namespace CDP4CommonView.Diagram.ViewModels
{
    /// <summary>
    /// enumeration ShapeSide that denotes the possible sides of diagram element such as Element definitiom.
    /// Used to calculate Ports location on Port container
    /// </summary>
    public enum PortContainerShapeSide
    {
        /// <summary>
        /// Default value
        /// </summary>
        Undefined = -1,
        Bottom = 0,
        Left = 25,
        Top = 50,
        Right = 75
    }
}
