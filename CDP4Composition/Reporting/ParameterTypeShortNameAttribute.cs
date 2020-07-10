namespace CDP4Composition.Reporting
{
    using System;

    public class ParameterTypeShortNameAttribute : Attribute
    {
        public string ShortName { get; }

        public ParameterTypeShortNameAttribute(string shortName)
        {
            this.ShortName = shortName;
        }
    }
}
