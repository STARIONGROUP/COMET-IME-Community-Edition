namespace CDP4DiagramEditor.Helpers
{
    public static class DiagramRDLHelper
    {
        private static readonly string constraintTag = "constraint";
        private static readonly string constraintOptionalTag = $"{constraintTag}_optional";
        private static readonly string constraintRestrictedTag = $"{constraintTag}_restricted";
        private static readonly string constraintEnforcedTag = $"{constraintTag}_enforced";
    }
}
